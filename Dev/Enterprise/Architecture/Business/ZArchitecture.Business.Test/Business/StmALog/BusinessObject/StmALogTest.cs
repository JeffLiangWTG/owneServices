using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.ZArchitecture.Business.StmALog;
using Constants = Enterprise.Core.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class StmALogTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNotReportWhenLogInAnyBranch()
		{
			using (EnvProxy.Instance.TemporaryServiceTaskContext("XYZ", true))
			{
				Factory.New<DummyBizOWithAutoLogs>();
				Factory.Save();
			}
			AssertNull(ErrorReporter.LastExceptionReported);
		}

		public void TestSourceInfoItemsDoesntBlowUpWhenTheresNoMessageLinked()
		{
			var log = Factory.New<StmALog>();
			AssertMultilineASCIIEquals("Log Source Info Items", @"", string.Join("\r\n", log.SourceInfoItems.Select(item => ((KeyDataPair)item).Key + " - " + ((KeyDataPair)item).Data).ToArray()));
		}

		public void TestSL_ReferenceSetter_AnyValue_UpdateParameters()
		{
			var log = Factory.New<StmALog>();

			// With reference free text and parameters
			log.SetReferenceWithLock("Shche ne vmerla v Ukraini...|{0}=UAIEV|{1}=Maidan|", Params.Codes.Location, Params.Codes.Facility);
			AssertEquals("Reference free text", "Shche ne vmerla v Ukraini...", log.ReferenceFreeText);
			AssertEquals("Parameters count", 2, log.Parameters.Count);
			AssertEquals("Parameter 1", "UAIEV", log.Parameters[Params.Codes.Location]);
			AssertEquals("Parameter 2", "Maidan", log.Parameters[Params.Codes.Facility]);

			// Without parameters
			log.SetReferenceWithLock("Shche ne vmerla v Ukraini ni slava ni volya|");
			AssertEquals("Reference free text", "Shche ne vmerla v Ukraini ni slava ni volya|", log.ReferenceFreeText);
			AssertEquals("Parameters count", 0, log.Parameters.Count);

			// Without reference free text
			log.SetReferenceWithLock("|{0}=UAIEV|{1}=Maidan", Params.Codes.Location, Params.Codes.Facility);
			AssertEquals("Reference free text", ZString.Empty, log.ReferenceFreeText);
			AssertEquals("Parameters count", 2, log.Parameters.Count);
			AssertEquals("Parameter 1", "UAIEV", log.Parameters[Params.Codes.Location]);
			AssertEquals("Parameter 2", "Maidan", log.Parameters[Params.Codes.Facility]);

			// With parameter inside reference free text
			log.SetReferenceWithLock("Shche |notparam=notvalue|ne vmerla v Ukraini...|{0}=UAIEV|{1}=Maidan", Params.Codes.Location, Params.Codes.Facility);
			AssertEquals("Reference free text", "Shche |notparam=notvalue|ne vmerla v Ukraini...", log.ReferenceFreeText);
			AssertEquals("Parameters count", 2, log.Parameters.Count);
			AssertEquals("Parameter 1", "UAIEV", log.Parameters[Params.Codes.Location]);
			AssertEquals("Parameter 2", "Maidan", log.Parameters[Params.Codes.Facility]);

			// With free text which is valid parameter
			log.SetReferenceWithLock("LOC=UAIEV");
			AssertEquals("Reference free text", "LOC=UAIEV", log.ReferenceFreeText);
			AssertEquals("Parameters count", 0, log.Parameters.Count);

			// With system symbol '|' inside reference free text
			var guid = Guid.NewGuid();
			log.SetReferenceWithLock("{0}|AAA|BBB|{0}|{1}=UAIEV|{2}=Maidan", guid, Params.Codes.Location, Params.Codes.Facility);
			AssertEquals("Reference free text", string.Format("{0}|AAA|BBB|{0}", guid), log.ReferenceFreeText);
			AssertEquals("Parameters count", 2, log.Parameters.Count);
			AssertEquals("Parameter 1", "UAIEV", log.Parameters[Params.Codes.Location]);
			AssertEquals("Parameter 2", "Maidan", log.Parameters[Params.Codes.Facility]);
		}

		public void TestCreateSLWithInvalidParametersThrowsException()
		{
			var log = Factory.New<StmALog>();
			log.SetReferenceWithLock("AAA|BBB|{0}=UAIEV|{1}=Maidan", "FOO", "BAR");
			var errors = ErrorReporter.ExceptionsThrown;
			AssertContains("The following parameter codes are not valid: FOO,BAR", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestCopyPersistentValues()
		{
			var shipment = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();

			var log = shipment.GetLogs().AddNew(Events.CustomisableEvent00);

			Factory.Save();

			var clonedLog = Factory.New<StmALog>();
			using (clonedLog.LockForUpdatingKeyFieldsForTesting())
			{
				clonedLog.CopyPersistentValuesFrom(log);
			}

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNotEquals(log.PK, clonedLog.PK);
				AssertNotEquals(log.SL_PostedTimeUtc, clonedLog.SL_PostedTimeUtc);
				AssertEquals(log.SL_Parent, clonedLog.SL_Parent);
				AssertEquals(log.SL_SE_NKEvent, clonedLog.SL_SE_NKEvent);
			});
		}

		public void TestLogInAndOutReturnCompanyAsSource()
		{
			var relatedCompany = (IGlbCompany)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbCompany>());
			((EnterpriseBusinessObject)relatedCompany)[GlbCompanySchema.GC_Name] = "SangoCompany";
			var relatedBranch = (IGlbBranch)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbBranch>());
			relatedBranch.GB_GC = relatedCompany.PK;

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_GB_NKBranch = relatedBranch.GB_Code;
				log.SL_SE_NKEvent = AutoEvents.LoginCode;
			}
			AssertEquals("Source should contain company name", relatedCompany.GC_Name, log.SL_TableFriendlyName);
			AssertEquals("SangoCompany", log.SL_TableFriendlyName);
		}

		public void TestSL_TableFriendlyNameReturnBase()
		{
			var relatedCompany = (IGlbCompany)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbCompany>());
			((EnterpriseBusinessObject)relatedCompany)[GlbCompanySchema.GC_Name] = "SangoCompany";
			var relatedBranch = (IGlbBranch)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbBranch>());
			relatedBranch.GB_GC = relatedCompany.PK;

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_GB_NKBranch = relatedBranch.GB_Code;
				log.SL_Table = "SangoTable";
			}
			AssertEquals("This SangoTable", log.SL_TableFriendlyName);
		}

		public void TestParameters_SomeParametersAreAdded_UpdateSL_Reference()
		{
			var log = Factory.New<StmALog>()
				.SetReferenceWithLock("Shche ne vmerla v Ukraini ni slava ni volya")
				.Set(Params.Codes.Location.AsKeyFor("UAIEV"))
				.Set(Params.Codes.Facility.AsKeyFor("Maidan"));

			AssertEquals("Final event reference",
				string.Format("Shche ne vmerla v Ukraini ni slava ni volya|{0}=Maidan|{1}=UAIEV", Params.Codes.Facility, Params.Codes.Location),
				log.SL_Reference);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{ log.Parameters.Clear(); }
			AssertEquals("Final event reference",
				string.Format("Shche ne vmerla v Ukraini ni slava ni volya", Params.Codes.Location, Params.Codes.Facility),
				log.SL_Reference);
		}

		public void TestSL_Reference_StripsNonWesternEuropeanCharacters()
		{
			var log = Factory.New<StmALog>()
				.SetReferenceWithLock("Aşkın");
			AssertNoErrors(log.SL_ReferenceInfo);
			AssertEquals("Non Western-European characters shoud be removed", "Akn", log.SL_Reference);
		}

		public void TestParameters_StripsNonWesternEuropeanCharacters()
		{
			var log = Factory.New<StmALog>()
				.SetReferenceWithLock("XXX")
				.Set(Params.Codes.Facility.AsKeyFor("Aşkın"));

			AssertNoErrors(log.SL_ReferenceInfo);
			AssertEquals("Non Western-European characters shoud be removed", string.Format("XXX|{0}=Akn", Params.Codes.Facility), log.SL_Reference);
		}

		public void TestReferenceFreeTextSetter_AnyValue_UpdateSL_Reference()
		{
			var log = Factory.New<StmALog>();

			log.SetReferenceWithLock("Shche ne vmerla v Ukraini...|{0}=UAIEV", Params.Codes.Location);
			log.SetReferenceFreeTextWithLock("Shche ne vmerla v Ukraini ni slava ni volya");
			AssertEquals("Event reference", string.Format("Shche ne vmerla v Ukraini ni slava ni volya|{0}=UAIEV", Params.Codes.Location), log.SL_Reference);

			log.SetReferenceFreeTextWithLock(ZString.Empty);
			AssertEquals("Final event reference", string.Format("|{0}=UAIEV", Params.Codes.Location), log.SL_Reference);
		}

		#region DisplayEventReference

		static class EventDataContextSchema
		{
			public const string Event = "EVENT";
			public const string Ref = "REF";
		}

		public void TestDisplayEventReferenceGetter_MacroIsSpecified_ReturnEvaluatedString()
		{
			Events.Arrival.WithReferenceFormat("<{0}> at <FAC>, <If(IsUnloco(LOC),CityCountry(LOC),LOC)>", EventDataContextSchema.Event);

			var log = Factory.New<StmALog>().SetEventCodeWithLock(Events.Arrival).SetReferenceWithLock("Shche ne vmerla v Ukraini...|LOC=UAIEV|FAC=MAIDAN");
			AssertEquals("DisplayEventReference", ZString.Format("{0} at MAIDAN, Kiev, UA", Events.Arrival.Description), log.DisplayEventReference);
		}

		public void TestDisplayEventReferenceGetter_PartialEvent()
		{
			Events.Received.WithReferenceFormat("<List(If(PTL != \"\",\"Partial\",\"\"),EVENT,If(\"<FAC>\" != \"\",\"at <FAC>\",\"\"),If(IsUnloco(LOC),CityCountry(LOC),LOC),VFL,FDT,REF,If(PTL+TTL != \"\",\"<If(PTL != \"\",PTL,\"total\")> of <TTL> pieces\", \"\")).Select({{\"<@data>\".Trim()}}).Where({{@data != \"\"}}).Join(\" \")>");

			var log = Factory.New<StmALog>().SetEventCodeWithLock(Events.Received).SetReferenceWithLock("|PTL=2|TTL=5");
			AssertEquals("DisplayEventReference", "Partial Received 2 of 5 pieces", log.DisplayEventReference);
		}

		public void TestDisplayEventReferenceGetter_MacroIsOverridden_ReturnEvaluatedString()
		{
			Events.Arrival
				.WithReferenceFormat("<FAC>", EventDataContextSchema.Event)
				.WithOverriddenReferenceFormat("<{0}> at <FAC>, <If(IsUnloco(LOC),CityCountry(LOC),LOC)>", EventDataContextSchema.Event);

			var log = Factory.New<StmALog>().SetEventCodeWithLock(Events.Arrival).SetReferenceWithLock("Shche ne vmerla v Ukraini...|LOC=UAIEV|FAC=MAIDAN");
			AssertEquals("DisplayEventReference", ZString.Format("{0} at MAIDAN, Kiev, UA", Events.Arrival.Description), log.DisplayEventReference);
		}

		public void TestDisplayEventReferenceGetter_MacroIsNotSpecified_ReturnReferenceAsItIs()
		{
			Events.Arrival.WithReferenceFormat(ZString.Empty);

			var log = Factory.New<StmALog>().SetEventCodeWithLock(Events.Arrival).SetReferenceWithLock("Shche ne vmerla v Ukraini...|FAC=MAIDAN|LOC=UAIEV");
			AssertEquals("DisplayEventReference", "Shche ne vmerla v Ukraini...|FAC=MAIDAN|LOC=UAIEV", log.DisplayEventReference);
		}

		public void TestDisplayEventReferenceGetter_MacroHasSyntaxError_ReturnReferenceAsItIs()
		{
			Events.Arrival.WithReferenceFormat("<EVENT> at <FAC>, <If(IsUnloco(LOC)>,<CityCountry2(<LOC>)>,<LOC>)>");

			var log = Factory.New<StmALog>().SetEventCodeWithLock(Events.Arrival).SetReferenceWithLock("Shche ne vmerla v Ukraini...|FAC=MAIDAN|LOC=UAIEV");
			AssertEquals("DisplayEventReference", "Shche ne vmerla v Ukraini...|FAC=MAIDAN|LOC=UAIEV", log.DisplayEventReference);
		}

		public void TestDisplayEventReference_ParameterHasBeenChanged_FireValueChangedEvent()
		{
			Events.Arrival.WithReferenceFormat("<FAC>");
			var valueChangedEventFired = false;

			var log = Factory.New<StmALog>().SetEventCodeWithLock(Events.Arrival);
			log.Set("FAC".AsKeyFor("MAIDAN"));
			log.DisplayEventReferenceInfo.ValueChanged += (s, e) => valueChangedEventFired = true;
			var displayRef = log.DisplayEventReference;

			// Parameter has been updated
			log.Set("FAC".AsKeyFor("Independence Square"));
			AssertEquals("Value changed event has been fired", true, valueChangedEventFired);
			AssertEquals("DisplayEventReference", "Independence Square", log.DisplayEventReference);

			// Parameter has been added
			Events.Departure.WithReferenceFormat("<LOC>, <FAC>");
			log.SetEventCodeWithLock(Events.Departure);
			valueChangedEventFired = false;
			log.Set("LOC".AsKeyFor("Kyiv"));
			AssertEquals("Value changed event has been fired", true, valueChangedEventFired);
			AssertEquals("DisplayEventReference", "Kyiv, Independence Square", log.DisplayEventReference);
		}

		public void TestDisplayEventReference_EventHasBeenChanged_FireValueChangedEvent()
		{
			var valueChangedEventFired = false;

			var log = Factory.New<StmALog>().SetEventCodeWithLock(Events.Arrival);
			log.DisplayEventReferenceInfo.ValueChanged += (s, e) => valueChangedEventFired = true;
			var displayRef = log.DisplayEventReference;

			// Event exists in macro
			Events.Departure.WithReferenceFormat("[<{0}>]", EventDataContextSchema.Event);
			valueChangedEventFired = false;
			log.SetEventCodeWithLock(Events.Departure);
			AssertEquals("Value changed event has been fired", true, valueChangedEventFired);
			AssertEquals("DisplayEventReference", string.Format("[{0}]", Events.Departure.Description), log.DisplayEventReference);

			// Event doesn't exist in macro
			Events.Arrival.WithReferenceFormat("McLaren");
			valueChangedEventFired = false;
			log.SetEventCodeWithLock(Events.Arrival);
			AssertEquals("Value changed event has been fired", true, valueChangedEventFired);
			AssertEquals("DisplayEventReference", "McLaren", log.DisplayEventReference);
		}

		public void TestDisplayEventReference_ReferenceFreeTextHasBeenChanged_FireValueChangedEvent()
		{
			Events.Arrival.WithReferenceFormat("<{0}>", EventDataContextSchema.Ref);
			Events.Departure.WithReferenceFormat("McLaren");

			var valueChangedEventFired = false;
			var log = Factory.New<StmALog>().SetEventCodeWithLock(Events.Arrival);
			log.DisplayEventReferenceInfo.ValueChanged += (s, e) => valueChangedEventFired = true;
			var displayRef = log.DisplayEventReference;

			// Reference changed via SL_Reference
			log.SetReferenceWithLock("Shche ne vmerla v Ukraini ni slava ni volya|LOC=UAIEV");
			AssertEquals("Value changed event has been fired", true, valueChangedEventFired);
			AssertEquals("DisplayEventReference", "Shche ne vmerla v Ukraini ni slava ni volya", log.DisplayEventReference);

			// Reference changed via ReferenceFreeText
			valueChangedEventFired = false;
			log.SetReferenceFreeTextWithLock("Shche ne vmerla v Ukraini...");
			AssertEquals("Value changed event has been fired", true, valueChangedEventFired);
			AssertEquals("DisplayEventReference", "Shche ne vmerla v Ukraini...", log.DisplayEventReference);
		}

		public void TestDisplayEventReference_ChangeOfIdentifier()
		{
			var cidLog = Factory.New<StmALog>().SetEventCodeWithLock(Events.ChangeOfIdentifier).SetReferenceWithLock("REE12|NEW=C456|OLD=C123|RES=NAME CHANGE|TYP=CONTAINER ID");
			AssertEquals("DisplayEventReference", "CONTAINER ID Change of Identifier from C123 to C456 because NAME CHANGE REE12", cidLog.DisplayEventReference);
		}

		public void TestDisplayEventReference_WithImplementingIStmALogOperationProvider()
		{
			var log = Factory.New<StmALog>().SetEventCodeWithLock(Events.EditedARecord);
			var dummyWithStmALogOperationProvider = Factory.New<DummyWithStmALogOperationProvider>();

			log.SetReferenceFreeTextWithLock("Testing, Testing, One, Two, Three");
			AssertEquals("DisplayEventReference", "Testing, Testing, One, Two, Three", log.DisplayEventReference);

			log.SetReferenceFreeTextWithLock("Testing, Testing, One, Two, Three"); // Reset displayEventReference value

			using (log.LockForUpdatingKeyFields(false))
			{
				log.Master = dummyWithStmALogOperationProvider;
			}

			AssertEquals("This test is fabulous!!!", log.DisplayEventReference);
		}

		public void TestDisplayEventReference_StatusUpdated()
		{
			var log = Factory.New<StmALog>()
				.SetEventCodeWithLock(AutoEvents.StatusUpdated)
				.SetReferenceWithLock("|DEP=PortBase|TYP=1|EQN=MOOP1234560|CRF=219307502");

			AssertEquals("Status Updated by PortBase: Arrival at exit message accepted by Customs, MOOP1234560, 219307502", log.DisplayEventReference);

			log = Factory.New<StmALog>()
				.SetEventCodeWithLock(AutoEvents.StatusUpdated)
				.SetReferenceWithLock("|TYP=2|EQN=MOOP1234560|CRF=219307502");

			AssertEquals("No port base department, no special type translation", "Status Updated: 2, MOOP1234560, 219307502", log.DisplayEventReference);

			log = Factory.New<StmALog>()
				.SetEventCodeWithLock(AutoEvents.StatusUpdated)
				.SetReferenceWithLock("|DEP=PortBase|TYP=2|OLD=BKD|NEW=CNF|EQN=MEEP1234560|CRF=MRN|FAC=CNR|LOC=AUSYD");

			AssertEquals("Status Updated by PortBase: Control notification for export shipment received from Customs from Booked to Confirmed, MEEP1234560, MRN at Consignor Sydney, NSW, AU", log.DisplayEventReference);

			log = Factory.New<StmALog>()
				.SetEventCodeWithLock(AutoEvents.StatusUpdated)
				.SetReferenceWithLock("FREETEXT|DEP=PORTBASE|MST=Bobby|TYP=0|OLD=BKD|NEW=CNF|EQN=MEEP1234560|CRF=MRN|FAC=CNR|LOC=AUSYD|RES=REASONS");

			AssertEquals("Status Updated by PORTBASE from Bobby: Export shipment not (completely) arrived from Booked to Confirmed, MEEP1234560, MRN at Consignor Sydney, NSW, AU because REASONS, FREETEXT", log.DisplayEventReference);
		}

		public void TestDisplayEventReference_StatusUpdatedForHVLVReplacementDictionary()
		{
			var log = Factory.New<StmALog>()
				.SetEventCodeWithLock(AutoEvents.StatusUpdated)
				.SetReferenceWithLock("|OLD=OPN|NEW=CLS");

			AssertEquals("Status Updated from Open to Closed", log.DisplayEventReference);

			log = Factory.New<StmALog>()
				.SetEventCodeWithLock(AutoEvents.StatusUpdated)
				.SetReferenceWithLock("|OLD=PEN|NEW=FAL");

			AssertEquals("Status Updated from Pending to Failed", log.DisplayEventReference);

			log = Factory.New<StmALog>()
				.SetEventCodeWithLock(AutoEvents.StatusUpdated)
				.SetReferenceWithLock("|OLD=LDG|NEW=CON");

			AssertEquals("Status Updated from Load List Lodged to Consolidated", log.DisplayEventReference);
		}

		public void TestDisplayEventReference_Returned()
		{
			var log = Factory.New<StmALog>()
				.SetEventCodeWithLock(AutoEvents.Returned)
				.SetReferenceWithLock("|TYP=Package P1|FAC=TW|LOC=DOCK-1|EQN=Vehicle REG123|RES=Missed Flight|RFN=DTU1");

			AssertEquals("Package P1 Returned to Transit Warehouse, DOCK-1, Vehicle REG123 with reason Missed Flight, Ref No: DTU1", log.DisplayEventReference);

			log = Factory.New<StmALog>()
				.SetEventCodeWithLock(AutoEvents.Returned)
				.SetReferenceWithLock("|TYP=Package P1|FAC=CFS|LOC=DOCK-1|EQN=Vehicle REG123|RES=Missed Flight|RFN=DTU1");

			AssertEquals("Package P1 Returned to Depot, DOCK-1, Vehicle REG123 with reason Missed Flight, Ref No: DTU1", log.DisplayEventReference);

			log = Factory.New<StmALog>()
				.SetEventCodeWithLock(AutoEvents.Returned)
				.SetReferenceWithLock("|TYP=Package P1|FAC=ABC|LOC=DOCK-1|EQN=Vehicle REG123|RES=Missed Flight|RFN=DTU1");

			AssertEquals("Package P1 Returned to ABC, DOCK-1, Vehicle REG123 with reason Missed Flight, Ref No: DTU1", log.DisplayEventReference);
		}

		public void TestDisplayEventReference_Held()
		{
			var log = Factory.New<StmALog>()
				.SetEventCodeWithLock(AutoEvents.Held)
				.SetReferenceWithLock("|LOC=US|MST=ACAS Shipment Report|RES=Do Not Hold Load");

			AssertEquals("ACAS Shipment Report Held, Reason: Do Not Hold Load US", log.DisplayEventReference);

			log = Factory.New<StmALog>()
				.SetEventCodeWithLock(AutoEvents.Held)
				.SetReferenceWithLock("|DEP=Customs|LOC=US|MST=ACAS Shipment Report|RES=Do Not Hold Load");

			AssertEquals("ACAS Shipment Report Held by Customs, Reason: Do Not Hold Load US", log.DisplayEventReference);

			log = Factory.New<StmALog>()
				.SetEventCodeWithLock(AutoEvents.Held)
				.SetReferenceWithLock("REF IS FREE TEXT!|MST=Review Task");

			AssertEquals("Review Task Held, REF IS FREE TEXT!", log.DisplayEventReference);
		}

		public void TestDisplayEventReference_ClearanceCompleted()
		{
			var log = Factory.New<StmALog>()
				.SetEventCodeWithLock(AutoEvents.ClearanceCompleted)
				.SetReferenceWithLock("|LOC=US|MST=ACAS Shipment Report|RES=Do Not Hold Load");

			AssertEquals("ACAS Shipment Report Clearance Completed, Reason: Do Not Hold Load US", log.DisplayEventReference);

			log = Factory.New<StmALog>()
				.SetEventCodeWithLock(AutoEvents.ClearanceCompleted)
				.SetReferenceWithLock("|DEP=Customs|LOC=US|MST=ACAS Shipment Report|RES=Do Not Hold Load");

			AssertEquals("ACAS Shipment Report Clearance Completed by Customs, Reason: Do Not Hold Load US", log.DisplayEventReference);

			log = Factory.New<StmALog>()
				.SetEventCodeWithLock(AutoEvents.ClearanceCompleted)
				.SetReferenceWithLock("REF IS FREE TEXT!|MST=Review Task");

			AssertEquals("Review Task Clearance Completed, REF IS FREE TEXT!", log.DisplayEventReference);
		}

		public void TestEvaluateAfterAddingNewParameter()
		{
			Events.Arrival.WithReferenceFormat("<LOC>");

			var log = Factory.New<StmALog>().SetEventCodeWithLock(Events.Arrival);

			AssertEquals("DisplayEventReference", "", log.DisplayEventReference);

			log.SetReferenceWithLock("|LOC=AUSYD");

			AssertEquals("DisplayEventReference", "AUSYD", log.DisplayEventReference);
		}

		public void TestDisplayEventReference_DisplayEventDescription()
		{
			var log = Factory.New<StmALog>().SetEventCodeWithLock(Events.Auxiliary).SetReferenceWithLock("|EVT=DEP|FAC=CTO|FRM=Carrier|LOC=AUSYD|MOD=Sea");
			var content = log.DisplayEventReference;
			AssertEquals("DisplayEventReference", ZString.Format("Auxiliary Sea Departure from Carrier Terminal Sydney, NSW, AU "), content);
		}

		#endregion

		#region GetParametersFromReference

		public void TestGetParametersFromReference_ReferenceContainsDuplicatedParamaters_ThrowArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => StmALog.GetParametersFromReference("|L=AUYSD|L=UAIEV", throwOnDuplicates: StmALog.ParseReferenceError.Exception));
			AssertExceptionThrown<ArgumentException>(() => StmALog.GetParametersFromReference("|LO=AUYSD|LO=UAIEV", throwOnDuplicates: StmALog.ParseReferenceError.Exception));
			AssertExceptionThrown<ArgumentException>(() => StmALog.GetParametersFromReference("|LOC=AUYSD|LOC=UAIEV", throwOnDuplicates: StmALog.ParseReferenceError.Exception));
		}

		public void TestGetParametersFromReference_ReferenceContainsDuplicatedParamaters_NoException()
		{
			AssertNoExceptionThrown(() => StmALog.GetParametersFromReference("|CMP=AUYSD|CMP=UAIEV", throwOnDuplicates: StmALog.ParseReferenceError.None));
			AssertNoExceptionThrown(() => StmALog.GetParametersFromReference("|WHS=AUYSD|WHS=UAIEV", throwOnDuplicates: StmALog.ParseReferenceError.None));
			AssertNoExceptionThrown(() => StmALog.GetParametersFromReference("|LOC=AUYSD|LOC=UAIEV", throwOnDuplicates: StmALog.ParseReferenceError.None));
		}

		public void TestGetParametersFromText_DoNotCrash()
		{
			using (new DisposableAction(() => ErrorReporter.Instance.Clear()))
			{
				StmALog.GetParametersFromText("|CMP=BUNG|CMP=BING");
				AssertEquals("No Error should have been reporter", 0, ErrorReporter.TotalErrorCount);

				StmALog.GetParametersFromText("|TO=BUNG|TO=BING");
				AssertEquals("No Error should have been reporter", 0, ErrorReporter.TotalErrorCount);

				StmALog.GetParametersFromText("|WHS=BUNG|WHS=BING");
				AssertEquals("No Error should have been reporter", 0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestDodgyParametersTreatedAsFreeText()
		{
			ObservableDictionary<string, string> parameters = null;

			AssertNoExceptionThrown(() => parameters = StmALog.GetParametersFromText("NOTVALID=THING|NOTVALID=IGNORE|CMP=BUNG", ParseReferenceError.Exception));
			AssertEquals("BUNG", parameters["CMP"]);

			AssertNoExceptionThrown(() => parameters = StmALog.GetParametersFromText("NOTVALID=THING|NOTVALID=IGNORE", ParseReferenceError.Exception));
			AssertEquals(0, parameters.Count);

			try
			{
				StmALog.GetParametersFromText("NOTVALID=THING|NOTVALID=IGNORE|CMP=BUNG|CMP=BING", ParseReferenceError.Exception);
			}
			catch (ArgumentException ex)
			{
				AssertContains("CMP=BING", ex.Message);
			}
		}

		public void TestDuplicateParametersOnStmALogErrorReporting()
		{
			var bizo = (BusinessObject)Factory.New<IDummyWithWorkflow>();
			var log = bizo.GetLogs().AddNew(Events.ServiceInvoicePosted);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = "som=dfogdk=gdf;dg;=d|f====ecrap||EVT=(\"WRC\")|EVT=(\"WAW\")";
			}

			AssertContains("For StmALog:", ErrorReporter.LastMessageReported);
			ErrorReporter.Instance.Clear();
		}

		public void TestGetParametersFromReference_ReferenceIsValid_ReturnParametersd()
		{
			var parameters = StmALog.GetParametersFromReference("|CMP=11111|WHS=22222");
			AssertEquals("Parameters count", 2, parameters.Count);
			AssertEquals("CMP", "11111", parameters["CMP"]);
			AssertEquals("WHS", "22222", parameters["WHS"]);

			parameters = StmALog.GetParametersFromReference("CMP=11111|WHS=22222");
			AssertEquals("Parameters count", 1, parameters.Count);
			AssertEquals("WHS", "22222", parameters["WHS"]);

			parameters = StmALog.GetParametersFromReference("CMP=11111");
			AssertEquals("Parameters count", 0, parameters.Count);

			parameters = StmALog.GetParametersFromReference("");
			AssertEquals("Parameters count", 0, parameters.Count);

			parameters = StmALog.GetParametersFromReference(null);
			AssertEquals("Parameters count", 0, parameters.Count);
		}

		public void TestGetParametersFromReference_ValidLengths()
		{
			var parameters = StmALog.GetParametersFromReference("|CMP=33333|TO=22222");
			AssertEquals("Parameters count", 2, parameters.Count);
			AssertEquals("TO", "22222", parameters["TO"]);
			AssertEquals("ABC", "33333", parameters["CMP"]);

			parameters = StmALog.GetParametersFromReference("|ABCD=44444|CMP=33333|TO=22222");
			AssertEquals("Parameters count", 2, parameters.Count);
			AssertEquals("TO", "22222", parameters["TO"]);
			AssertEquals("ABC", "33333", parameters["CMP"]);

			var abcd = parameters.ContainsKey("ABCD") ? parameters["ABCD"] : "";
			AssertEquals("ABCD", "", abcd);

			parameters = StmALog.GetParametersFromReference("|A=11111|AB=22222|ABC=33333|ABCD=44444");
			AssertEquals("Parameters count", 0, parameters.Count);

			parameters = StmALog.GetParametersFromReference("A=11111");
			AssertEquals("Parameters count", 0, parameters.Count);

			parameters = StmALog.GetParametersFromReference("");
			AssertEquals("Parameters count", 0, parameters.Count);

			parameters = StmALog.GetParametersFromReference(null);
			AssertEquals("Parameters count", 0, parameters.Count);
		}

		public void TestGetParametersFromSL_Reference_ValidLengths()
		{
			var stmALog = Factory.NewWithValidTestData<StmALog>();
			using (stmALog.LockForUpdatingKeyFields())
			{
				stmALog.SL_Reference = "A=00000|A=11111|AB=22222|ABC=33333";
			}

			Factory.Save();

			AssertEquals("Parameters count", 3, stmALog.Parameters.Count);
			AssertEquals("A=00000|A=11111|AB=22222|ABC=33333", stmALog.DisplayEventReference);
			AssertEquals("A", "11111", stmALog.Parameters["A"]);
			AssertEquals("AB", "22222", stmALog.Parameters["AB"]);
			AssertEquals("ABC", "33333", stmALog.Parameters["ABC"]);

			ErrorReporter.Instance.Clear();

			using (stmALog.LockForUpdatingKeyFields())
			{
				stmALog.SL_Reference = "A=00000|ABCD=44444|ABC=33333|AB=22222|A=11111";
			}

			Factory.Save();

			AssertEquals("Parameters count", 3, stmALog.Parameters.Count);
			AssertEquals("A=00000|ABCD=44444|ABC=33333|AB=22222|A=11111", stmALog.DisplayEventReference);
			AssertEquals("A", "11111", stmALog.Parameters["A"]);
			AssertEquals("AB", "22222", stmALog.Parameters["AB"]);
			AssertEquals("ABC", "33333", stmALog.Parameters["ABC"]);

			var abcd = stmALog.Parameters.ContainsKey("ABCD") ? stmALog.Parameters["ABCD"] : "";
			AssertEquals("ABCD", "", abcd);

			ErrorReporter.Instance.Clear();

			using (stmALog.LockForUpdatingKeyFields())
			{
				stmALog.SL_Reference = "A=00000|A=11111|AB=22222|ABC=33333|ABCD=44444";
			}

			Factory.Save();

			AssertEquals("Parameters count", 0, stmALog.Parameters.Count);
			AssertEquals("A=00000|A=11111|AB=22222|ABC=33333|ABCD=44444", stmALog.DisplayEventReference);

			ErrorReporter.Instance.Clear();
		}

		public void TestGetParametersFromText_ThatOnceTheDamageIsDoneWeJustAcceptItIsBrokenAndMoveOn()
		{
			var stmALog = Factory.NewWithValidTestData<StmALog>();
			using (stmALog.LockForUpdatingKeyFields())
			{
				stmALog.SL_Reference = "|NEW=BOGO|NEW=FROGO";
			}
			ErrorReporter.Instance.Clear();

			Factory.Save();

			AssertEquals("|NEW=BOGO|NEW=FROGO", stmALog.DisplayEventReference);
		}

		#endregion

		public void TestGetFreeTextFromReference()
		{
			string[] badInput = { "1234|FAC=Terminal|LOC=SYDNEY|RES=Pickup| ",
												"1234|FAC=Terminal|LOC=SYDNEY|RES=Pickup|123|",
												"1234|FAC=Terminal|LOC=SYDNEY|RES=Pickup|123" };

			AssertEquals("1234|FAC=Terminal|LOC=SYDNEY|RES=Pickup| ", StmALog.GetFreeTextFromReference(badInput[0]));
			AssertEquals("1234|FAC=Terminal|LOC=SYDNEY|RES=Pickup|123|", StmALog.GetFreeTextFromReference(badInput[1]));
			AssertEquals("1234|FAC=Terminal|LOC=SYDNEY|RES=Pickup|123", StmALog.GetFreeTextFromReference(badInput[2]));

			string[] goodInput = { "1234|FAC=Terminal|LOC=SYDNEY|RES=Pickup|",
												"1234|FAC=Terminal|LOC=SYDNEY|badInput|RES=Pickup|",
												"1234|FAC=Terminal|LOC=SYDNEY|RES=Pickup" };

			AssertEquals("1234", StmALog.GetFreeTextFromReference(goodInput[0]));
			AssertEquals("1234|FAC=Terminal|LOC=SYDNEY|badInput", StmALog.GetFreeTextFromReference(goodInput[1]));
			AssertEquals("1234", StmALog.GetFreeTextFromReference(goodInput[2]));
		}

		public void TestCreateRecreateFitsToMaxLength()
		{
			BusinessObjectWithLoggedDates.EstimatedDate = new ZDateTimeOffset(2000, 1, 1);
			var dict = new Dictionary<string, string>();
			dict["CMP"] = "Eloquent sophistication";
			dict["DEP"] = "Smurfing";
			dict["NAM"] = "Rejecting the Status Quo";
			dict["WHS"] = "The Status is not QUO";

			var freeRef = new string('a', 1024);

			var log = BusinessObjectWithLoggedDates.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.Arrival, EstimateActual.Actual, ZDateTimeOffset.UtcNow, freeRef, dict.AsEnumerable().ToArray());
			BusinessObjectWithLoggedDates.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.Arrival, EstimateActual.Actual, ZDateTimeOffset.UtcNow, freeRef, dict.AsEnumerable().ToArray());
			AssertEquals(new string('a', 928) + "|CMP=Eloquent sophistication|DEP=Smurfing|NAM=Rejecting the Status Quo|WHS=The Status is not QUO", log.SL_Reference);
		}

		public void TestCreateRecreateFitsToMaxLength_ReferenceExceeds_BehaveTheSame()
		{
			BusinessObjectWithLoggedDates.EstimatedDate = new ZDateTimeOffset(2000, 1, 1);
			var dict = new Dictionary<string, string>();
			dict["CMP"] = "Eloquent sophistication";
			dict["DEP"] = "Smurfing";
			dict["NAM"] = "Rejecting the Status Quo";
			dict["WHS"] = "The Status is not QUO";

			var freeRef = new string('a', 1024);

			var log = BusinessObjectWithLoggedDates.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.Arrival, EstimateActual.Actual, ZDateTimeOffset.UtcNow, freeRef, dict.AsEnumerable().ToArray());
			BusinessObjectWithLoggedDates.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.Arrival, EstimateActual.Actual, ZDateTimeOffset.UtcNow, freeRef, dict.AsEnumerable().ToArray());
			AssertEquals(new string('a', 928) + "|CMP=Eloquent sophistication|DEP=Smurfing|NAM=Rejecting the Status Quo|WHS=The Status is not QUO", log.SL_Reference);
		}

		public void TestCreateRecreateFitsToMaxLength_DoNotRaiseParametersEvent_Snail()
		{
			BusinessObjectWithLoggedDates.EstimatedDate = new ZDateTimeOffset(2000, 1, 1);
			var dict = new Dictionary<string, string>();
			dict["CMP"] = "Eloquent sophistication";
			dict["DEP"] = "Smurfing";
			dict["NAM"] = "Rejecting the Status Quo";
			dict["WHS"] = "The Status is not QUO";

			var freeRef = new string('a', 128);

			var log = BusinessObjectWithLoggedDates.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.Arrival, EstimateActual.Actual, ZDateTimeOffset.UtcNow, freeRef, dict.AsEnumerable().ToArray());
			var events = 0;
			((ObservableDictionary<string, string>)log.Parameters).CollectionChanged += (s, e) => events++;
			for (int i = 0; i < 1000; i++)
			{
				BusinessObjectWithLoggedDates.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.Arrival, EstimateActual.Actual, ZDateTimeOffset.UtcNow, freeRef, dict.AsEnumerable().ToArray());
			}
			AssertEquals("These should be the same.", 0, events);
		}

		public void TestGenerateEventReferenceToFitInReferenceMaxLength()
		{
			var referenceFreeText = new string('z', 1000);
			var parameters = new[]
			{
				new KeyValuePair<string, string>("BBB", new string('1', 25)),
				new KeyValuePair<string, string>("AAA", new string('2', 25)),
				new KeyValuePair<string, string>("CCC", new string('3', 25)),
			};

			var actual = StmALog.GenerateEventReferenceToFitInReferenceMaxLength(referenceFreeText, parameters);
			var expected =
				new string('z', 934)
				+ "|AAA=" + new string('2', 25)
				+ "|BBB=" + new string('1', 25)
				+ "|CCC=" + new string('3', 25);

			AssertMultilineASCIIEquals("should have trimmed referenceFreeText", expected, actual);

			parameters = new[]
			{
				new KeyValuePair<string, string>("BBB", new string('1', 800)),
				new KeyValuePair<string, string>("AAA", new string('2', 800)),
				new KeyValuePair<string, string>("CCC", new string('3', 800)),
			};

			actual = StmALog.GenerateEventReferenceToFitInReferenceMaxLength(referenceFreeText, parameters);
			expected =
				"|AAA=" + new string('2', 214)
				+ "|BBB=" + new string('1', 800);

			AssertMultilineASCIIEquals("should have trimmed 'AAA' parameter, and totally ommited the 'CCC' parameter and freeText part", expected, actual);
		}

		public void TestLogCorrectlyObtainsSourceInfoItems()
		{
			var log = Factory.New<StmALog>();
			var interchange = Factory.New<IEDIInterchange>();
			interchange.EI_From = "The Death Star";
			interchange.EI_To = "Yavin 4";
			var message = Factory.New<IXmlEDIMessage>();
			message.EM_MessageNum = "99999999999";
			message.EM_MessageSubType = "XUS";
			message.EM_MessageType = "XDC";
			((BusinessObject)message)[EDIMessageSchema.EM_EI] = interchange.PK;
			message.Content = XElement.Parse(@"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>StarDestroyer</Type>
          <Key>S00001043</Key>
        </DataSource>
      </DataSourceCollection>

      <DataTargetCollection>
        <DataTarget>
          <Type>StarDestroyer</Type>
          <Key>S00001043</Key>
        </DataTarget>
      </DataTargetCollection>

      <ActionPurpose>
        <Code>DST</Code>
        <Description>Destruction</Description>
      </ActionPurpose>
      <Company>
        <Code>GAL</Code>
        <Name>Galactic Empire</Name>
      </Company>
      <EnterpriseID>GAL</EnterpriseID>
      <EventType>
        <Code>IMP</Code>
        <Description>Imperial</Description>
      </EventType>
      <EventUser>
        <Code>DRV</Code>
        <Name>Darth Vader</Name>
      </EventUser>
      <ServerID>GAL</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2011-04-21T12:06:00</TriggerDate>
      <TriggerDescription>Surrender or Die</TriggerDescription>
      <TriggerReference>Darth Vader</TriggerReference>
      <TriggerType>Trigger</TriggerType>
    </DataContext>

    <WayBillNumber>IAMTHEMASTER</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>
  </Shipment>
</UniversalShipment>".Trim());

			var pivot = Factory.New<IGenPivot>();
			pivot.XX_Relation1ID = log.PK;
			pivot.XX_Relation1TableCode = "SL";
			pivot.XX_Relation2ID = message.PK;
			pivot.XX_Relation2TableCode = "EM";
			pivot.XX_RelationType = Constants.GenPivotTypes.XmlEdiMessage;

			AssertMultilineASCIIEquals("Log Source Info Items", @"
Data Source Action Purpose - DST - Destruction
Data Source Company - GAL - Galactic Empire
Data Source Enterprise ID - GAL
Data Source Server ID - GAL
Data Source Trigger Count - 1
Data Source Trigger Date - 21-Apr-11 12:06:00 +10:00
Data Source Trigger Description - Surrender or Die
Data Source Trigger Event - IMP - Imperial
Data Source Trigger Event User - DRV - Darth Vader
Data Source Trigger Reference - Darth Vader
Data Source Trigger Type - Trigger
Sender ID - The Death Star
Recipient ID - Yavin 4
			".Trim(), string.Join("\r\n", log.SourceInfoItems.Select(item => ((KeyDataPair)item).Key + " - " + ((KeyDataPair)item).Data).ToArray()));
		}

		public void TestKeysGetTurnedIntoHumanReadableNames()
		{
			var log = Factory.New<StmALog>();
			var message = Factory.New<IXmlEDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.Content = XElement.Parse(@"
			<UniversalEvent>
				<Event>
					<EventType>OCR</EventType>
					<EventTime>10-JUL-2010 18:00</EventTime>
					<ContextCollection>
						<Context>
							<Type>MAWBNumber</Type>
							<Value>020-12345675</Value>
						</Context>
						<Context>
							<Type>MAWBOriginIATAAirportCode</Type>
							<Value>JFK</Value>
						</Context>
						<Context>
							<Type>MAWBDestinationIATAAirportCode</Type>
							<Value>BKK</Value>
						</Context>
						<Context>
							<Type>MAWBNumberOfPieces</Type>
							<Value>20</Value>
						</Context>
						<Context>
							<Type>NativeEventCode</Type>
							<Value>RCS</Value>
						</Context>
						<Context>
							<Type>NumberOfPieces</Type>
							<Value>20</Value>
						</Context>
            <Context>
              <Type Description=""AMS Number"">AMS</Type>
              <Value>134FREGT</Value>
            </Context>
            <Context>
              <Type Description="""">COC</Type>
              <Value>267AIRGT</Value>
            </Context>
					</ContextCollection>
				</Event>
			</UniversalEvent>");

			var pivot = Factory.New<IGenPivot>();
			pivot.XX_Relation1ID = log.PK;
			pivot.XX_Relation1TableCode = "SL";
			pivot.XX_Relation2ID = message.PK;
			pivot.XX_Relation2TableCode = "EM";
			pivot.XX_RelationType = Constants.GenPivotTypes.XmlEdiMessage;

			var sourceInfoItems = log.SourceInfoItems;

			AssertMultilineASCIIEquals("", @"
MAWB Number - 020-12345675
MAWB Origin IATA Airport Code - JFK
MAWB Destination IATA Airport Code - BKK
MAWB Number Of Pieces - 20
Native Event Code - RCS
Number Of Pieces - 20
AMS Number - 134FREGT
COC - 267AIRGT
".Trim(), string.Join("\r\n", sourceInfoItems.OfType<KeyDataPair>().Select(o => o.Key + " - " + o.Data).ToArray()));
		}

		public void TestRelatedMessageNotFound()
		{
			var log = Factory.New<StmALog>();
			AssertNull("Should not find any message link to log", log.RelatedEDIMessage);
		}

		public void TestRelatedMessage()
		{
			var log = Factory.New<StmALog>();
			var firstMessage = Factory.New<IXmlEDIMessage>();
			firstMessage.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			firstMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			firstMessage.Content = XElement.Parse(@"
			<UniversalEvent>
				<Event>
					<EventType>OCR</EventType>
					<EventTime>10-JUL-2010 18:00</EventTime>
					<ContextCollection>
						<Context>
							<Type>MAWBNumber</Type>
							<Value>020-12345675</Value>
						</Context>
					</ContextCollection>
				</Event>
			</UniversalEvent>");

			var firstPivot = Factory.New<IGenPivot>();
			firstPivot.XX_Relation1ID = log.PK;
			firstPivot.XX_Relation1TableCode = "SL";
			firstPivot.XX_Relation2ID = firstMessage.PK;
			firstPivot.XX_Relation2TableCode = "EM";
			firstPivot.XX_RelationType = Constants.GenPivotTypes.XmlEdiMessage;

			var secondLog = Factory.New<StmALog>();

			var secondMessageWrongType = Factory.New<IXmlEDIMessage>();
			secondMessageWrongType.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			secondMessageWrongType.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			secondMessageWrongType.Content = XElement.Parse(@"
			<UniversalEvent>
				<Event>
					<EventType>OCR</EventType>
					<EventTime>10-JUL-2010 18:00</EventTime>
					<ContextCollection>
						<Context>
							<Type>MAWBNumber</Type>
							<Value>081-11111111</Value>
						</Context>
					</ContextCollection>
				</Event>
			</UniversalEvent>");

			var secondPivotWrongType = Factory.New<IGenPivot>();
			secondPivotWrongType.XX_Relation1ID = secondLog.PK;
			secondPivotWrongType.XX_Relation1TableCode = "SL";
			secondPivotWrongType.XX_Relation2ID = secondMessageWrongType.PK;
			secondPivotWrongType.XX_Relation2TableCode = "EM";
			secondPivotWrongType.XX_RelationType = Constants.GenPivotTypes.InvalidCodeForTesting;

			var secondMessage = Factory.New<IXmlEDIMessage>();
			secondMessage.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			secondMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			secondMessage.Content = XElement.Parse(@"
			<UniversalEvent>
				<Event>
					<EventType>OCR</EventType>
					<EventTime>10-JUL-2010 18:00</EventTime>
					<ContextCollection>
						<Context>
							<Type>MAWBNumber</Type>
							<Value>020-76543215</Value>
						</Context>
					</ContextCollection>
				</Event>
			</UniversalEvent>");

			var secondPivot = Factory.New<IGenPivot>();
			secondPivot.XX_Relation1ID = secondLog.PK;
			secondPivot.XX_Relation1TableCode = "SL";
			secondPivot.XX_Relation2ID = secondMessage.PK;
			secondPivot.XX_Relation2TableCode = "EM";
			secondPivot.XX_RelationType = Constants.GenPivotTypes.XmlEdiMessage;

			AssertEquals("First message should be linked to log", "020-12345675", log.RelatedEDIMessage.ContextList.MAWBNumber);
			AssertEquals("Second message should be linked to log", "020-76543215", secondLog.RelatedEDIMessage.ContextList.MAWBNumber);
		}

		public void TestDeleteWithdraws()
		{
			var logParent = (DummyEnterpriseBusinessObject)Factory.New<IDummyWithWorkflow>();
			var log = logParent.Logs.AddNew(Events.Authorised, new ZDateTimeOffset(2010, 9, 9));

			var milestone = ((IDummyWithWorkflow)logParent).AddNewMilestone();
			((IBaseTrigger)milestone).TriggerEventCode = Events.AuthorisedCode;
			milestone.P9_ParentID = logParent.PK;
			milestone.P9_Type = "MIL";

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = new ZDateTime(2008, 1, 1);
			}
			AssertEquals("WorkFlow should fire when SL_EventTime is changed - milestone.P9_ActualDate", new ZDateTime(2008, 1, 1), milestone.P9_ActualDate.ToZDateTime());
			log.Delete();
			AssertEquals("WorkFlow should fire when log is deleted - milestone.P9_ActualDate", ZDateTimeOffset.Empty, milestone.P9_ActualDate);
		}

		/// <summary>
		///		Yeah, naming is confusing. SL_FireWorkflow means that the event should fire workflow later, in services tasks (TasksAndMilestonesLoader).
		///		Its purpose is to notify the CW1 that the event has been generated outside CW1 and it is still has to be processed.
		/// </summary>
		public void TestLockForUpdatingKeyFields_SL_FireWorkflowIsSet_DoNotFireWorkflow()
		{
			var logParent = (DummyEnterpriseBusinessObject)Factory.New<IDummyWithWorkflow>();
			var milestone = ((IDummyWithWorkflow)logParent).AddNewMilestone();
			milestone.P9_Type = Constants.Workflow.MilestoneType;
			milestone.P9_ParentID = logParent.PK;
			((IBaseTrigger)milestone).TriggerEventCode = Events.Departure.Code;

			var log = Factory.New<StmALog>();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.Departure.Code;
				log.SL_Parent = logParent.PK;
				log.SL_Table = "DummyBizo";

				log.SL_FireWorkflow = true;
				log.SL_EventTime = new ZDateTime(2016, 1, 1);
			}

			AssertEquals("Actual date on the related milestone", ZDateTimeOffset.Empty, milestone.P9_ActualDate);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_FireWorkflow = false;
				log.SL_EventTime = new ZDateTime(2016, 2, 2);
			}

			AssertEquals("Actual date on the related milestone", new ZDateTimeOffset(new ZDateTime(2016, 2, 2)), milestone.P9_ActualDate);
		}

		public void TestOnlyUpdatingKeyFieldsWillFireWorkflow()
		{
			var logParent = (DummyEnterpriseBusinessObject)Factory.New<IDummyWithWorkflow>();
			var log = logParent.Logs.AddNew(Events.ServiceInvoicePosted, new ZDateTimeOffset(2015, 1, 1));
			Factory.Save();

			var trigger = ((IDummyWithWorkflow)logParent).AddNewTrigger();
			// We need to temporarily block this new trigger from firing
			((IBaseTrigger)trigger).TriggerCondition = "MCR";
			((IBaseTrigger)trigger).TriggerConditionValue = "false";

			((IBaseTrigger)trigger).TriggerEventCode = Events.ServiceInvoicePostedCode;

			// Now that we have set the event we can allow the trigger to fire (on event changes)
			((IBaseTrigger)trigger).TriggerConditionValue = "true";

			trigger.P9_ParentID = logParent.PK;
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

			AssertEquals("Milestone is not updated", ZDateTimeOffset.Empty, trigger.P9_ActualDate);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
			}
			AssertEquals("WorkFlow should not fire when no fields changed during lock", ZDateTimeOffset.Empty, trigger.P9_ActualDate);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = "Something DIFFERENT!";
			}
			AssertEquals("WorkFlow should fire when SL_Reference is updated", log.SL_EventTime, trigger.P9_ActualDate.ToZDateTime());

			((IBaseTrigger)trigger).TriggerEventCode = Events.CashbookItemTickedOffCode;
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.CashbookItemTickedOffCode;
			}
			AssertEquals("WorkFlow should fire when SL_SE_NKEvent is changed", log.SL_EventTime, trigger.P9_ActualDate.ToZDateTime());

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = new ZDateTime(2015, 4, 1);
			}
			AssertEquals("WorkFlow should fire when SL_EventTime is changed", log.SL_EventTime, trigger.P9_ActualDate.ToZDateTime());

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_IsEstimate = true;
			}
			AssertEquals("WorkFlow should fire when SL_IsEstimate is changed", log.SL_EventTime, trigger.P9_ScheduledDate.ToZDateTime());

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.Cancel();
			}
			AssertEquals("WorkFlow should fire when SL_IsCancelled is changed", ZDateTimeOffset.Empty, trigger.P9_ScheduledDate);
		}

		#region TestSL_Reference

		public void TestSL_ReferenceShouldBeChangedInKeyFieldsLocker()
		{
			StmALog log = Factory.New<StmALog>();

			AssertExceptionThrown(typeof(InvalidOperationException),
				"Must wrap expressions setting key fields with 'using (log.LockForUpdatingKeyFieldsForTesting())' so that Event Management knows when you're finished updating and can fire Milestones, Triggers and Date Updates.",
				() => log.SL_Reference = "abc");

			AssertNotEquals("abc", log.SL_Reference);

			using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
			{
				log.SL_Reference = "abc";
			}
			AssertEquals("abc", log.SL_Reference);
		}

		public void TestSL_ReferenceForBinding()
		{
			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFields(false))
			{
				log.SL_Reference = "11";
			}
			AssertEquals("11", log.SL_ReferenceForBinding);

			string guid = Guid.NewGuid().ToString();
			using (log.LockForUpdatingKeyFields(false))
			{
				log.SL_Reference = guid;
			}
			AssertEquals("", log.SL_ReferenceForBinding);
			AssertEquals(guid, log.SL_Reference);

			using (log.LockForUpdatingKeyFields(false))
			{
				log.SL_ReferenceForBinding = "z";
			}
			AssertEquals("z", log.SL_ReferenceForBinding);
			AssertEquals("z", log.SL_Reference);
		}

		public void TestSL_ReferenceForBinding_WithImplementingIStmALogOperationProvider()
		{
			var log = Factory.New<StmALog>();
			var dummyWithStmALogOperationProvider = Factory.New<DummyWithStmALogOperationProvider>();

			using (log.LockForUpdatingKeyFields(false))
			{
				log.SL_Reference = "Testing, Testing, One, Two, Three";
			}

			AssertEquals("Testing, Testing, One, Two, Three", log.SL_Reference);
			AssertEquals("Testing, Testing, One, Two, Three", log.SL_ReferenceForBinding);

			using (log.LockForUpdatingKeyFields(false))
			{
				log.SL_Reference = "Testing, Testing, One, Two, Three";
				log.Master = dummyWithStmALogOperationProvider;
			}

			AssertEquals("Testing, Testing, One, Two, Three", log.SL_Reference);
			AssertEquals("This test is fabulous!!!", log.SL_ReferenceForBinding);
		}

		class DummyWithStmALogOperationProvider : DummyEnterpriseBusinessObject, IStmALogOperationProvider
		{
			public DummyWithStmALogOperationProvider(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public ZString GetHandledStringProperty(ZString property)
			{
				return "This test is fabulous!!!";
			}
		}

		#endregion

		public void TestCheckNotEqualParentAndMaster()
		{
			AssertCheckNotEqualParentAndMaster(true);
			AssertCheckNotEqualParentAndMaster(false);
		}

		void AssertCheckNotEqualParentAndMaster(bool masterFirst)
		{
			var log = Factory.New<StmALog>();
			var obj1 = Factory.New<DummyBizOWithAutoLogs>() as IStmALogParent;
			var obj2 = Factory.New<DummyEnterpriseBusinessObject>();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				if (masterFirst)
				{
					log.Master = obj1;
				}
				else
				{
					log.SL_Parent = obj2.PK;
				}
				Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

				if (masterFirst)
				{
					log.SL_Parent = obj2.PK;
				}
				else
				{
					log.Master = obj1;
				}
			}

			var tableName = masterFirst ? "DummyBizo" : "";
			var masterType = "Enterprise.ZArchitecture.Business.Testing.DummyBizOWithAutoLogs";
			var masterPK = (obj1 as DummyBizOWithAutoLogs).PK;
			var expected = string.Format(@"Trying to set different SL_Parent and Master.
Values are:
PK:          {0}
SL_Parent:   {1}
SL_Table:    {2}
MasterType:  {3}
MasterPK:    {4}
MasterTable: {5}", log.PK, obj2.PK, tableName, masterType, masterPK, "DummyBizo");

			AssertMultilineASCIIEquals("Should report correct error when master and sl_parent are not the same", expected, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("StmALog currently does not support delete", true);
		}

		public void TestReloadStmALog()
		{
			var obj = Factory.New<DummyEnterpriseBusinessObject>();
			var log = obj.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log.SL_SE_NKEvent = Events.EditedARecord.Code;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
			}
			Factory.Save();
			AssertNoExceptionThrown(log.Reload);
		}

		public void TestLoadByQueryStmaLog()
		{
			ZQuery query = new ZQuery(StmALogSchema.PK, ZGuid.NewZGuid());
			AssertExceptionThrown<InvalidOperationException>(() => Factory.Load<StmALog>(query));
		}

		public void TestLoadByPKWithQueryStmaLog()
		{
			AssertExceptionThrown<InvalidOperationException>(() => Factory.Load<StmALog>(ZGuid.NewZGuid()));
		}

		public void TestMasterIsReturnedFromSettingSL_ParentAndSL_TableOnly()
		{
			var bizO = Factory.New<DummyEnterpriseBusinessObject>();
			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = bizO.PK;
				log.SL_Table = bizO.TableName;
			}
			AssertEquals(bizO, log.Master);
		}

		public void TestCanBeCancelledByUser()
		{
			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.Master = Factory.New<DummyEnterpriseBusinessObject>();
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log.SL_SE_NKEvent = Events.AddedARecordToTheSystem.Code;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				Assert(!log.CanBeCancelledByUser);

#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log.SL_SE_NKEvent = Events.EditedARecord.Code;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				Assert(!log.CanBeCancelledByUser);

#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log.SL_SE_NKEvent = Events.DeletedARecordInTheSystem.Code;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				Assert(!log.CanBeCancelledByUser);

				log.SL_SE_NKEvent = Events.ReadRelatedNotes.Code;
				Assert(!log.CanBeCancelledByUser);

				log.SL_SE_NKEvent = Events.RelatedNotesNotRead.Code;
				Assert(!log.CanBeCancelledByUser);

				log.SL_SE_NKEvent = Events.DocumentSent.Code;
				Assert(!log.CanBeCancelledByUser);

				log.SL_SE_NKEvent = Events.DocumentDelivered.Code;
				Assert(!log.CanBeCancelledByUser);

				log.SL_SE_NKEvent = Events.DocumentNotDelivered.Code;
				Assert(!log.CanBeCancelledByUser);

				log.SL_SE_NKEvent = Events.Arrival.Code;
				Assert(log.CanBeCancelledByUser);
			}
		}

		public void TestReload_UsingIndex()
		{
			DummyEnterpriseBusinessObject dummy = Factory.New<DummyEnterpriseBusinessObject>();
			StmALog log = dummy.GetLogs().AddNew(Events.Authorised);
			Factory.Save();

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();
			StmALog reloadedLog = (StmALog)((IBusinessObjectReload)log).Reload(loadingFactory);
			AssertEquals("Loaded in the right factory", loadingFactory, reloadedLog.Factory);
			AssertEquals("Loaded the correct item", log.PK, reloadedLog.PK);
		}

		public void TestReload_WhenIndexCannotBeUsed()
		{
			DummyEnterpriseBusinessObject dummy = Factory.New<DummyEnterpriseBusinessObject>();
			StmALog log = dummy.GetLogs().AddNew(Events.Authorised);
			Factory.Save();
			Db.Connection.ExecuteNonQuery("update dbo.StmALog set SL_SE_NKEvent='XXX' where SL_PK='" + log.PK + "'");

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();
			StmALog reloadedLog = (StmALog)((IBusinessObjectReload)log).Reload(loadingFactory);
			AssertEquals("No error expected", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			AssertEquals("Loaded in the right factory", loadingFactory, reloadedLog.Factory);
			AssertEquals("Loaded the correct item", log.PK, reloadedLog.PK);
		}

		#region CheckReferenceEquals
		public void TestCheckReferenceEquals_EmptyReference()
		{
			var log = Factory.New<StmALog>();
			Assert(log.CheckReferenceEquals(""));
			Assert(log.CheckReferenceEquals(null));
		}

		public void TestCheckReferenceEquals_DifferentFreeTextReference()
		{
			var log = Factory.New<StmALog>();
			Assert(!log.CheckReferenceEquals("invalid reference"));
			log.SetReferenceWithLock("Shche ne vmerla v Ukraini ni slava ni volya|");
			Assert(!log.CheckReferenceEquals("invalid reference"));
		}

		public void TestCheckReferenceEquals_DifferentParameters()
		{
			var log = Factory.New<StmALog>();
			log.SetReferenceWithLock("|{0}=UAIEV|{1}=Maidan", Params.Codes.Location, Params.Codes.Facility);
			Assert(!log.CheckReferenceEquals(""));
			Assert(!log.CheckReferenceEquals($"|{Params.Codes.Location}=UAIEV"));
		}

		public void TestCheckReferenceEquals_Succeeds()
		{
			var log = Factory.New<StmALog>();
			var expectedSL_Reference = $"Shche |notparam=notvalue|ne vmerla v Ukraini...|{Params.Codes.Location}=UAIEV|{Params.Codes.Facility}=Maidan";
			log.SetReferenceWithLock(expectedSL_Reference);
			Assert(log.CheckReferenceEquals(expectedSL_Reference));
		}

		public void TestCheckReferenceEquals_DifferentParametersOrderSucceeds()
		{
			var log = Factory.New<StmALog>();
			var reference1 = $"reftext|{Params.Codes.Location}=val1|{Params.Codes.Facility}=val2";
			var reference2 = $"reftext|{Params.Codes.Facility}=val2|{Params.Codes.Location}=val1";
			log.SetReferenceWithLock(reference1);
			Assert(log.CheckReferenceEquals(reference2));
		}

		#endregion

		#region Business Object Overrides

		public void TestSettingDefaultUser()
		{
			AssertEquals("A new StmALog should have default user set (SL_GS_NKUser)", StaticCurrentFetcher.Instance.CurrentUser.GS_Code, Log.SL_GS_NKUser);
		}

		public void TestDelete()
		{
			StmALog logNotInDB = Factory.NewWithValidTestData<StmALog>();
			logNotInDB.Delete();
			AssertEquals("Log is deleted?", true, logNotInDB.IsDeleted);

			StmALog logInDB = Factory.NewWithValidTestData<StmALog>();
			Factory.Save();

			try
			{
				logInDB.Delete();
				Fail("Should Throw Exception");
			}
			catch (InvalidOperationException ex)
			{
				Assert("Exception caught (below) is not the one expected.\r\n" + ex.Message, ex.Message.StartsWith("Disallowed attempt to delete an StmALog object already in database"));
			}
		}

		#endregion

		#region Overridden Properties

		public void TestInvalidEventIsSavedAndReportSilentError()
		{
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

			Log.SL_SE_NKEvent = Events.AddedARecordToTheSystem.Code;
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

			Log.SL_SE_NKEvent = "~ZZ"; // purposefully assigning an invalid event code for testing
			AssertEquals("Setting Log.SL_SE_NKEvent to an invalid StmEvent code should report an error", 1, ExceptionReporterTestListener.Instance.Count);

			Factory.Save();
			ExceptionReporterTestListener.Instance.Clear();
			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			AssertNotNull("Should have saved invalid Event code", otherFactory.LoadTop1(typeof(StmALog), new ZQuery(StmALogSchema.SL_SE_NKEvent, "~ZZ")));
		}

		public void TestSL_Table()
		{
			StmALog log = Factory.New<StmALog>();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Table = "StandardTableName";
			}
			AssertEquals("StandardTableName", log.SL_Table);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Table = "SomeReferenceDatabase.dbo.ERFTableName";
			}
			AssertEquals("ERFTableName", log.SL_Table);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Table = "SomeOtherReferenceDatabase..TRFTableName";
			}
			AssertEquals("TRFTableName", log.SL_Table);
		}

		#endregion

		#region New Properties

		public void TestSL_EventDescription()
		{
			AssertEquals("Log.SL_EventDescription should be empty", ZString.Empty, Log.SL_EventDescription);
			using (Log.LockForUpdatingKeyFieldsForTesting())
			{
				Log.SL_SE_NKEvent = Events.AddedARecordToTheSystem.Code;
			}
			AssertEquals("Log.SL_EventDescription should be set", Event.SE_Desc, Log.SL_EventDescription);
		}

		public void TestSL_EventDescription_ForLegacyDeprecatedEventType()
		{
			using (Log.LockForUpdatingKeyFieldsForTesting())
			{
				Log.SL_SE_NKEvent = "CAP"; // deprecated event code... no constant defined
			}
			AssertEquals("Deprecated event code still produces a description", "Cartage Advise Printed", Log.SL_EventDescription);
			ErrorReporter.Clear();
		}

		public void TestSL_TableFriendlyName()
		{
			DummyBizOWithRelatedNotes dummyWithRelatedNotes = Factory.New<DummyBizOWithRelatedNotes>();

			// SL_TableFriendlyName is only valid when the base bizO's notes are in an StmALogCollectionView (as Log.IsRelated is dependent on it's parent view collection)
			StmALogCollectionView logsView = new StmALogCollectionView(dummyWithRelatedNotes);

			StmALog log = dummyWithRelatedNotes.Logs.AddNew(Events.Arrival);
			AssertEquals("Not a related Log.", "This " + dummyWithRelatedNotes.TableName, log.SL_TableFriendlyName);
			log.SuffixForTableFriendlyName = "ZZZ";
			AssertEquals("Not a related Log.", "This " + dummyWithRelatedNotes.TableName + " ZZZ", log.SL_TableFriendlyName);

			StmALog relatedLog = dummyWithRelatedNotes.RelatedDummy.Logs.AddNew();
			AssertEquals("A related Log.", dummyWithRelatedNotes.TableName, relatedLog.SL_TableFriendlyName);
		}

		public void TestSL_UserNameAndInitials()
		{
			Log.SL_GS_NKUser = "";
			AssertEquals("Log.SL_UserNameAndInitials should be empty", ZString.Empty, Log.SL_UserNameAndInitials);

			IGlbStaff user = Factory.New<IGlbStaff>();

			user.GS_FullName = "Fred Flintstone";
			user.GS_Code = "FF";

			Log.SL_GS_NKUser = user.GS_Code;
			AssertEquals("Log.SL_UserNameAndInitials should be set", "Fred Flintstone (FF)", Log.SL_UserNameAndInitials);
		}

		#endregion

		#region Related Business Objects

		public void TestUser()
		{
			AssertEquals("User default value", StaticCurrentFetcher.Instance.CurrentUser.PK, Log.User.PK);

			IGlbStaff staff = Factory.New<IGlbStaff>();
			staff.GS_Code = "NEW";

			Log.SL_GS_NKUser = staff.GS_Code;
			AssertNotNull("After setting user code, user not null", Log.User);
			AssertEquals("After setting user code, user not null", Log.SL_GS_NKUser, Log.User.GS_Code);
		}

		#endregion

		#region Updating Milestones and BusinessObject date properties

		public void TestSL_EventTime_UpdatesMasterBizOEventDateProperty_ForEstimate()
		{
			BusinessObjectWithLoggedDates.EstimatedDate = new ZDateTimeOffset(2000, 1, 1);
			BusinessObjectWithLoggedDates.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.Arrival, EstimateActual.Estimate, BusinessObjectWithLoggedDates.EstimatedDate);
			TestSL_EventTime_UpdatesBizODateProperty(BusinessObjectWithLoggedDates.EstimatedDateInfo, BusinessObjectWithLoggedDates.Logs, EstimateActual.Estimate);
		}

		public void TestSL_EventTime_UpdatesMasterBizOEventDateProperty_ForActual()
		{
			BusinessObjectWithLoggedDates.ActualDate = new ZDateTimeOffset(2000, 1, 1);
			BusinessObjectWithLoggedDates.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, BusinessObjectWithLoggedDates.ActualDate);
			TestSL_EventTime_UpdatesBizODateProperty(BusinessObjectWithLoggedDates.ActualDateInfo, BusinessObjectWithLoggedDates.Logs, EstimateActual.Actual);
		}

		public void TestSL_EventTime_UpdatesMilestoneP9_ScheduledDate()
		{
			IProcessTask milestone = CreateMilestone(Events.Arrival.Code);
			milestone.P9_ScheduledDate = new ZDateTimeOffset(new ZDateTime(2000, 1, 1));

			// manually creating the initial StmALog here as setting P9_ScheduledDate won't auto-create it
			var log = DummyWithWorkflow.GetLogs().AddNew(Events.Arrival);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_IsEstimate = true;
				log.SL_EventTime = milestone.P9_ScheduledDate.ToLocalZDateTime();
			}
			TestSL_EventTime_UpdatesBizODateProperty(milestone.P9_ScheduledDateInfo, DummyWithWorkflow.GetLogs(), EstimateActual.Estimate);
		}

		public void TestSL_EventTime_UpdatesMilestoneP9_ActualDate()
		{
			IProcessTask milestone = CreateMilestone(Events.Arrival.Code);
			ObjectFactory.Get<IActualDateWorkAround>().SetActualDateForTest(milestone, new ZDateTime(2000, 1, 1));
			TestSL_EventTime_UpdatesBizODateProperty(milestone.P9_ActualDateInfo, DummyWithWorkflow.GetLogs(), EstimateActual.Actual, isMilestone: true);
		}

		public void TestSL_EventTime_UpdatesADDMilestoneP9_ActualDate()
		{
			IProcessTask milestone = CreateMilestone(Events.CustomisableEvent00.Code);
			ObjectFactory.Get<IActualDateWorkAround>().SetActualDateForTest(milestone, new ZDateTime(2000, 1, 1));
			TestSL_EventTime_UpdatesBizODateProperty(milestone.P9_ActualDateInfo, DummyWithWorkflow.GetLogs(), EstimateActual.Actual, Events.CustomisableEvent00, isMilestone: true);
		}

		public void TestSL_EventTime_UpdatesEDTMilestoneP9_ActualDate()
		{
			var milestone = CreateMilestone(Events.EditedARecord.Code);
			ObjectFactory.Get<IActualDateWorkAround>().SetActualDateForTest(milestone, new ZDateTime(2000, 1, 1));
			TestSL_EventTime_UpdatesBizODateProperty(milestone.P9_ActualDateInfo, DummyWithWorkflow.GetLogs(), EstimateActual.Actual, Events.EditedARecord, isMilestone: true);
		}

		public void TestSL_EventTime_UpdatesWorkflowTriggerP9_ActualDate()
		{
			var milestone = CreateWorkflowTrigger(Events.Arrival.Code);
			var log = DummyWithWorkflow.GetLogs().AddNew(Events.Arrival);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = new ZDateTime(2000, 1, 1);
			}
			AssertEquals("P9_ActualDate updated from SL_EventTime", new ZDateTimeOffset(new ZDateTime(2000, 1, 1)), milestone.P9_ActualDate);
		}

		#region ADD - EDT triggers

		[TestDateIncremental(0, 0, 1)]
		public void TestWhenBusinessObjectIsAdded_StmJobQueueIsCreatedForADDTrigger()
		{
			//Use a seperate factory for ProcessTasks otherwise saving Shipment loads them from cache
			//and this alters the workflow as they are saved before the Shipment instead of after the Shipment
			//which is what happens in normal workflow when user creates a BO using a template.
			BusinessObjectFactory processFactory = new BusinessObjectFactory();

			BusinessObject processTaskTemplate = (BusinessObject)processFactory.New<IProcessTaskTemplate>();
			processTaskTemplate.FillWithValidTestData();
			processTaskTemplate[ProcessTaskTemplateSchema.P0_ProcessType] = "SHP";
			BusinessObject glbCompany = (BusinessObject)Factory.New<IGlbCompany>();

			processTaskTemplate[ProcessTaskTemplateSchema.P0_GC] = EnvProxy.Instance.CurrentCompany.PK;// ((BusinessObject)GlbCompany["CurrentCompany"])[GlbCompanySchema.PK];
			processTaskTemplate[ProcessTaskTemplateSchema.P0_IsActive] = true;
			processTaskTemplate[ProcessTaskTemplateSchema.P0_IsSystem] = false;

			BusinessObject trigger = ((BusinessObjectCollection)processTaskTemplate["WorkflowItems"]).AddNew();
			trigger[ProcessTasksSchema.P9_Type] = "TRG";

			BusinessObject notification = ((IBusinessObjectCollection)trigger["ProcessTaskNotifications"]).AddNew();
			trigger[ProcessTasksSchema.P9_Description] = "ARTS TEST DESC";
			trigger["IsWorkflowTrigger"] = true;
			((IBaseTrigger)trigger).TriggerEventCode = Events.AddedARecordToTheSystem.Code;
			((IBaseTrigger)trigger).TriggerEventCode = "ADD";

			notification[ProcessTaskNotificationSchema.PQ_TriggerType] = "SHP";

			processFactory.Save();
			processFactory.ClearQueryCache();

			BusinessObject shipment = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			shipment[JobShipmentSchema.JS_HouseBill] = "HB"; // to set HasChanges=true

			Factory.Save();

			ZQuery query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			query.AddToFilter(StmALogSchema.SL_Table, ProcessTasksSchema.Constants.TableName);
			StmALog log = Factory.LoadTop1<StmALog>(query);
			AssertNotNull("Workflow trigger log should be created for ADD when object is saved", log);
		}

		[TestDateIncremental(0, 0, 1)]
		public void TestWhenBusinessObjectIsEdited_StmJobQueueIsCreatedForEDTTrigger()
		{
			//Use a seperate factory for ProcessTasks otherwise saving Shipment loads them from cache
			//and this alters the workflow as they are saved before the Shipment instead of after the Shipment
			//which is what happens in normal workflow when user creates a BO using a template.
			BusinessObjectFactory processFactory = new BusinessObjectFactory();

			BusinessObject processTaskTemplate = (BusinessObject)processFactory.New<IProcessTaskTemplate>();
			processTaskTemplate.FillWithValidTestData();
			processTaskTemplate[ProcessTaskTemplateSchema.P0_ProcessType] = "SHP";
			BusinessObject glbCompany = (BusinessObject)Factory.New<IGlbCompany>();

			processTaskTemplate[ProcessTaskTemplateSchema.P0_GC] = EnvProxy.Instance.CurrentCompany.PK;// ((BusinessObject)GlbCompany["CurrentCompany"])[GlbCompanySchema.PK];
			processTaskTemplate[ProcessTaskTemplateSchema.P0_IsActive] = true;
			processTaskTemplate[ProcessTaskTemplateSchema.P0_IsSystem] = false;

			BusinessObject trigger = ((BusinessObjectCollection)processTaskTemplate["WorkflowItems"]).AddNew();
			trigger[ProcessTasksSchema.P9_Type] = "TRG";

			BusinessObject notifiacation = ((IBusinessObjectCollection)trigger["ProcessTaskNotifications"]).AddNew();
			trigger[ProcessTasksSchema.P9_Description] = "ARTS TEST DESC";
			trigger["IsWorkflowTrigger"] = true;
			((IBaseTrigger)trigger).TriggerEventCode = Events.AddedARecordToTheSystem.Code;
			((IBaseTrigger)trigger).TriggerEventCode = "EDT";

			notifiacation[ProcessTaskNotificationSchema.PQ_TriggerType] = "SHP";

			processFactory.Save();
			processFactory.ClearQueryCache();

			var logQuery = new ZQuery();
			logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			logQuery.AddToFilter(StmALogSchema.SL_Table, ProcessTasksSchema.Constants.TableName);

			var logs = Factory.Load<StmALog>(logQuery);
			var logCountBefore = logs.Length;

			var shipment = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			shipment[JobShipmentSchema.JS_HouseBill] = "HB"; // to set HasChanges=true

			Factory.Save();

			logs = Factory.Load<StmALog>(logQuery);
			var logCountFirstSave = logs.Length;
			AssertEquals("An Workflow Trigger Log should not be created for EDT when object is first saved", logCountBefore, logCountFirstSave);

			shipment[JobShipmentSchema.JS_HouseBill] = "HB123";
			Factory.Save();

			logs = Factory.Load<StmALog>(logQuery);
			var logCountAfterEdit = logs.Length;
			AssertEquals("Object edited and saved => a Workflow Trigger Log should be created for EDT", logCountBefore + 1, logCountAfterEdit);

			shipment[JobShipmentSchema.JS_HouseBill] = "HB123456";
			Factory.Save();

			logs = Factory.Load<StmALog>(logQuery);
			logCountAfterEdit = logs.Length;
			AssertEquals("Object edited and saved => another Workflow Trigger Log should be created for EDT", logCountBefore + 2, logCountAfterEdit);
		}

		#endregion

		void TestSL_EventTime_UpdatesBizODateProperty(ZPropertyInfo dateProperty, Logs logs, EstimateActual estimateActual, bool isMilestone = false)
		{
			TestSL_EventTime_UpdatesBizODateProperty(dateProperty, logs, estimateActual, Events.Arrival, isMilestone);
		}

		void TestSL_EventTime_UpdatesBizODateProperty(ZPropertyInfo dateProperty, Logs logs, EstimateActual estimateActual, Event eventType, bool isMilestone = false)
		{
			var orginalDate = dateProperty.Value;

			bool isEstimate = estimateActual == EstimateActual.Estimate;
			var existingLog = logs.MostRecentLogByEventTime(eventType);
			AssertNotNull("An StmALog of type EventType: " + eventType.Code + "  should be logged", existingLog);

			AssertEquals("SL_EventTime", AsZDateTime(dateProperty), existingLog.SL_EventTime);
			AssertEquals("SL_IsEstimate", isEstimate, existingLog.SL_IsEstimate);

			using (existingLog.LockForUpdatingKeyFieldsForTesting())
			{
				existingLog.SL_EventTime = new ZDateTime(2000, 2, 2);
			}

			AssertEquals("Business object updated when SL_EventTime changes on existing log", new ZDateTime(2000, 2, 2), AsZDateTime(dateProperty));
			existingLog.Cancel();

			var newLog = logs.AddNew(eventType);
			if (isEstimate)
			{
				using (newLog.LockForUpdatingKeyFieldsForTesting())
				{
					newLog.SL_IsEstimate = true;
				}
			}

			AssertEquals("Business object updated when SL_EventTime set on new log (after cancelling existing log)", newLog.SL_EventTime.ToSmallDateTimeFloor(), AsZDateTime(dateProperty).ToSmallDateTimeFloor());
		}

		ZDateTime AsZDateTime(ZPropertyInfo info)
		{
			var val = info.Value;
			if (val is ZDateTimeOffset offset)
			{
				return offset.ToZDateTime();
			}
			else
			{
				return (ZDateTime)val;
			}
		}

		public void TestEventCancelClearsBizODateProperty_ForEstimate()
		{
			TestEventCancelClearsBizODateProperty(BusinessObjectWithLoggedDates.EstimatedDateInfo, true);
		}

		public void TestEventCancelClearsBizODateProperty_ForActual()
		{
			TestEventCancelClearsBizODateProperty(BusinessObjectWithLoggedDates.ActualDateInfo, false);
		}

		void TestEventCancelClearsBizODateProperty(ZPropertyInfo dateProperty, bool isEstimate)
		{
			StmALog log1 = BusinessObjectWithLoggedDates.Logs.AddNew(Events.Arrival);
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_IsEstimate = isEstimate;
			}

			StmALog log2 = BusinessObjectWithLoggedDates.Logs.AddNew(Events.Arrival);
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_IsEstimate = isEstimate;
			}

			dateProperty.Value = ZDateTimeOffset.Now;
			Factory.Save();
			log1.Cancel();
			AssertEquals("Property value not cleared until all events are cancelled", false, dateProperty.Value.IsEmpty);

			((IStmALogInternals)log2).CancelWithoutNotifications();
			AssertEquals("Property value not cleared - changes on log were ignored", false, dateProperty.Value.IsEmpty);

			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.IsCancelled = false;
			}
			AssertEquals("Property value not cleared - just restoring log", false, dateProperty.Value.IsEmpty);

			log2.Cancel();
			AssertEquals("Property value cleared when all events cancelled", true, dateProperty.Value.IsEmpty);
		}

		public void TestEventCancelClearsMilestoneAndTriggerDates()
		{
			IProcessTask trigger = CreateWorkflowTrigger(Events.Arrival.Code);
			IProcessTask milestone = CreateMilestone(Events.Arrival.Code);
			StmALog log = DummyWithWorkflow.GetLogs().AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.Arrival.Code;
				log.SL_EventTime = new ZDateTime(2009, 1, 1);
			}
			AssertEquals("Precondition", new ZDateTimeOffset(new ZDateTime(2009, 1, 1)), milestone.P9_ActualDate);
			AssertEquals("Precondition", new ZDateTimeOffset(new ZDateTime(2009, 1, 1)), trigger.P9_ActualDate);

			((IStmALogInternals)log).CancelWithoutNotifications();
			AssertEquals("No changes", new ZDateTimeOffset(new ZDateTime(2009, 1, 1)), milestone.P9_ActualDate);
			AssertEquals("No changes", new ZDateTimeOffset(new ZDateTime(2009, 1, 1)), trigger.P9_ActualDate);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.IsCancelled = false;
			}
			AssertEquals("No changes", new ZDateTimeOffset(new ZDateTime(2009, 1, 1)), milestone.P9_ActualDate);
			AssertEquals("No changes", new ZDateTimeOffset(new ZDateTime(2009, 1, 1)), trigger.P9_ActualDate);

			log.Cancel();
			AssertEquals(ZDateTimeOffset.Empty, milestone.P9_ActualDate);
			AssertEquals(ZDateTimeOffset.Empty, trigger.P9_ActualDate);
		}

		public void TestEventCode_UpdatesMilestoneAndTriggerDates()
		{
			IProcessTask trigger = CreateWorkflowTrigger(Events.Arrival.Code);
			IProcessTask milestone = CreateMilestone(Events.Arrival.Code);
			StmALog log = DummyWithWorkflow.GetLogs().AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = new ZDateTime(2000, 1, 1);
			}
			AssertEquals("Precondition", ZDateTimeOffset.Empty, milestone.P9_ActualDate);
			AssertEquals("Precondition", ZDateTimeOffset.Empty, trigger.P9_ActualDate);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.Arrival.Code;
			}
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2000, 1, 1)), milestone.P9_ActualDate);
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2000, 1, 1)), trigger.P9_ActualDate);
		}

		public void TestEventCodeAndReference_UpdatesMilestoneAndTriggerDates()
		{
			IProcessTask trigger = CreateWorkflowTrigger(Events.Arrival.Code);
			((IBaseTrigger)trigger).TriggerCondition = "REF";
			((IBaseTrigger)trigger).TriggerConditionValue = "Arriving in LA";
			IProcessTask milestone = CreateWorkflowTrigger(Events.Arrival.Code);
			((IBaseTrigger)milestone).TriggerCondition = "REF";
			((IBaseTrigger)milestone).TriggerConditionValue = "Arriving in Sydney";

			var logParent = DummyWithWorkflow.GetLogs();

			logParent.AddNew(Events.Arrival, new ZDateTimeOffset(2000, 1, 1));
			AssertEquals("Precondition", ZDateTimeOffset.Empty, trigger.P9_ActualDate);
			AssertEquals("Precondition", ZDateTimeOffset.Empty, milestone.P9_ActualDate);

			logParent.AddNew(Events.Arrival, "Some reference", new ZDateTimeOffset(2000, 1, 1));
			AssertEquals(ZDateTimeOffset.Empty, trigger.P9_ActualDate);
			AssertEquals(ZDateTimeOffset.Empty, milestone.P9_ActualDate);

			logParent.AddNew(Events.Arrival, "Arriving in LA", new ZDateTimeOffset(2000, 1, 1));
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2000, 1, 1)), trigger.P9_ActualDate);
			AssertEquals(ZDateTimeOffset.Empty, milestone.P9_ActualDate);

			logParent.AddNew(Events.Arrival, "Arriving in Sydney", new ZDateTimeOffset(2000, 1, 2));
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2000, 1, 1)), trigger.P9_ActualDate);
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2000, 1, 2)), milestone.P9_ActualDate);
		}

		public void TestGetProcessTaskHandler()
		{
			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Table = DummyWithWorkflow.TableName;
				log.SL_Parent = ZGuid.NewZGuid();
			}

			AssertNull(log.Master);
			AssertNotNull(log.GetProcessTaskHandler());
			AssertEquals(typeof(ProcessTaskHandler), log.GetProcessTaskHandler().GetType());

			log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Table = DummyWithWorkflow.TableName;
				log.SL_Parent = DummyWithWorkflow.PK;
			}

			AssertEquals(DummyWithWorkflow, log.Master);
			AssertNotNull(log.GetProcessTaskHandler());
			AssertEquals(typeof(ProcessTaskHandler), log.GetProcessTaskHandler().GetType());

			var nonWorkflowDummy = Factory.New<DummyEnterpriseBusinessObject>();
			log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Table = nonWorkflowDummy.TableName;
				log.SL_Parent = nonWorkflowDummy.PK;
			}

			AssertEquals(nonWorkflowDummy, log.Master);
			AssertNotNull(log.GetProcessTaskHandler());
			AssertEquals(typeof(NullProcessTaskHandler), log.GetProcessTaskHandler().GetType());
		}

		IProcessTask CreateMilestone(ZString eventType)
		{
			return CreateMilestoneOrWorkflowTrigger(eventType, "MIL");
		}

		IProcessTask CreateWorkflowTrigger(ZString eventType)
		{
			return CreateMilestoneOrWorkflowTrigger(eventType, "TRG");
		}

		IProcessTask CreateMilestoneOrWorkflowTrigger(ZString eventType, ZString type)
		{
			BusinessObjectCollection workflowItems = (BusinessObjectCollection)DummyWithWorkflow["WorkflowItems"];
			IProcessTask milestone = (IProcessTask)workflowItems.AddNew();
			((IBaseTrigger)milestone).TriggerEventCode = eventType;
			milestone.P9_Type = type;
			return milestone;
		}

		BusinessObject DummyWithWorkflow
		{
			get
			{
				if (dummyWithWorkflow == null)
				{
					dummyWithWorkflow = (BusinessObject)Factory.New<IDummyWithWorkflow>();
				}
				return dummyWithWorkflow;
			}
		}
		BusinessObject dummyWithWorkflow;

		#endregion

		#region SL_PostedTimeUtc

		[ExpectExceptionMessage(typeof(NotSupportedException), "Setting SL_PostedTimeUtc is not supported")]
		public void TestSettingSL_PostedTimeIsNotSupported()
		{
			Log.SL_PostedTimeUtc = ZDateTime.Now;
		}

		[TestDate(2019, 4, 15, 13, 53, 59)]
		public void TestSL_PostedTime_IsSetBySave()
		{
			Assert("SL_PostedTimeUtc.IsValid", !Log.SL_PostedTimeUtc.IsValid);

			Factory.Save();

			Assert("The value of Log.SL_PostedTimeUtc should be available after save", Log.SL_PostedTimeUtc.IsValid);
			AssertEquals("Log.SL_PostedTimeUtc should be set to a test time", new ZDateTime(2019, 4, 15, 13, 53, 59), Log.SL_PostedTimeUtc);
		}

		public void TestSL_PostedTime_NotSetOnSavingIfLogIsInDatabase()
		{
			Assert(!Log.SL_PostedTimeUtc.IsValid);

			Factory.Save();
			Assert("Log.SL_PostedTimeUtc.IsValid", Log.SL_PostedTimeUtc.IsValid);

			ZDateTime postedTime = Log.SL_PostedTimeUtc;
			Log.Cancel();
			Factory.Save();
			AssertEquals("Log.SL_PostedTimeUtc should not have changed", postedTime, Log.SL_PostedTimeUtc);
		}

		public void TestSL_PostedTime_NoChangesToLogAfterFactorySave()
		{
			Factory.Save();
			AssertEquals("Log should not have changes after it has saved", false, Log.HasChanges);

			StmALog[] matches = (StmALog[])Factory.Load(typeof(StmALog), new ZQuery(StmALogSchema.SL_PostedTimeUtc, ZDateTime.Now));
			AssertEquals("Log should still have no changes after it has saved and a Factory.Load has occurred", false, Log.HasChanges);
			AssertEquals("Log should still have no changes after it has saved and a Factory.Load has occurred", DataRowState.Unchanged, ((INeedRow)Log).Row.RowState);
		}

		public void TestSL_PostedTime_NumCommandsExecuted2()
		{
			// potentially +3 DB hit to check the Registry
			var useParameters = ObjectFactory.Get<IEntityFrameworkSettings>().ParameterizeInsertAndUpdateStatements;
			var bindingEnabled = ObjectFactory.Get<ISystemDataRegistry>().ServiceTaskBusinessObjectBindingEnabled;
			var rowsToPostSqlStatements = ObjectFactory.Get<IEntityFrameworkSettings>().RowsToPostPerSqlStatement;

			int commandCountBefore = Db.Connection.ExecutedCommandCount;

			Factory.Save();
			// additional DB hits due to Factory.OnSaved extension and registry read
			// Normally, we expect there to be 1 command executed, but it's possible we lazy load the extension.Value, causing an additional execution. 
			AssertLessThanOrEqualTo("2 or less commands expected", Db.Connection.ExecutedCommandCount - commandCountBefore, 2);
			commandCountBefore = Db.Connection.ExecutedCommandCount;

			ZDateTime postedTime = Log.SL_PostedTimeUtc;
			AssertEquals("Getting Log.SL_PostedTimeUtc shouldn't query database", commandCountBefore, Db.Connection.ExecutedCommandCount);
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestPostedLocalBranchTime()
		{
			IEnvironment env = EnvProxy.Instance;
			Factory.Save();

			IBusiness ediHQBranch = (IBusiness)Factory.LoadTop1<IGlbBranch>(new ZQuery(Schema.GlbBranchSchema.GB_BranchName, "EDIHQ"));
			IBusiness singaporeBranch = (IBusiness)Factory.LoadTop1<IGlbBranch>(new ZQuery(Schema.GlbBranchSchema.GB_BranchName, "Singapore Branch"));
			IBusiness brisbaneBranch = (IBusiness)Factory.LoadTop1<IGlbBranch>(new ZQuery(Schema.GlbBranchSchema.GB_BranchName, "BN - AUBNE"));

			ZDateTime time = Log.PostedLocalBranchTime;
			ZDateTime utcTime = Log.SL_PostedTimeUtc.ToDateTime();
			AssertEquals(utcTime.AddHours(10), time);
		}

		[TestDate(2050, 1, 2)]
		public void TestSL_PostedTime_WhenTestDateAttributeApplied()
		{
			Factory.Save();
			AssertEquals(2050, Log.SL_PostedTimeUtc.Year);
			AssertEquals(1, Log.SL_PostedTimeUtc.Month);
			AssertEquals(2, Log.SL_PostedTimeUtc.Day);
		}

		public void TestFilteringBy_SL_PostedTime_AfterSave()
		{
			ZDateTime beforeSave = (ZDateTime)EnvProxy.Instance.Time.CurrentUtcDateTime.AddSeconds(-5);
			Factory.Save();
			ZDateTime afterSave = (ZDateTime)EnvProxy.Instance.Time.CurrentUtcDateTime.AddSeconds(5);

			ZQuery filter = new ZQuery();
			filter.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThan, beforeSave);
			filter.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.LessThan, afterSave);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.NotEqual, AutoEvents.Login.Code);

			StmALog[] matches = (StmALog[])Factory.Load(typeof(StmALog), filter);
			AssertEquals("Should be able to factory load the log even though it is in the local DataTable with an out-of-date SL_PostedTimeUtc", 1, matches.Length);
			AssertEquals("Should be able to factory load the log even though it is in the local DataTable with an out-of-date SL_PostedTimeUtc", Log.PK, matches[0].PK);
		}

		public void TestSL_PostedTime_PreSaveValidationDoesNotValidate()
		{
			Assert(!Log.SL_PostedTimeUtc.IsValid);
			Assert(!Log.SL_PostedTimeUtcInfo.HasErrors());

			Log.SL_SE_NKEvent = Events.AddedARecordToTheSystem.Code;
			Log.RunPreSaveValidation();
			Assert(!Log.SL_PostedTimeUtcInfo.HasErrors());
		}

		[TestDate(2019, 4, 15, 13, 53, 59)]
		public void TestSL_PostedTime_SetOnAutoLog()
		{
			var dummy = new BusinessObjectFactory { RefreshEnabled = false }.New<DummyAutoLogged>();
			dummy.Factory.Save();

			var log = dummy.Logs.AddedLog;

			AssertNotNull(log);
			Assert("Log should be saved to db", log.IsInDatabase);
			Assert("There should be no local changes - i.e. SL_PostedTimeUtc should be saved to db", !log.HasChanges);
			AssertEquals("Log.SL_PostedTimeUtc should be set to a test time", new ZDateTime(2019, 4, 15, 13, 53, 59), log.SL_PostedTimeUtc);
		}

		#endregion

		public override void TestBizObjectFields()
		{
			var bizO = (StmALog)GetNewBusinessObject();
			using (bizO.LockForUpdatingKeyFieldsForTesting())
			{
				TestBizObjectFieldsCore(bizO);
			}
		}

		public override void TestSettingValueCallsRefreshBinding()
		{
			var bo = (StmALog)GetNewBusinessObjectForSettingValueCallsRefreshBindingTest();
			using (bo.LockForUpdatingKeyFieldsForTesting())
			{
				TestSettingValueCallsRefreshBindingCore(bo);
			}
		}

		#region Test duplicate PKs

		public void TestDuplicatePkAfterSaveHalfFail()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			Factory.Save();

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = dummy.PK;
				log.SL_Table = dummy.TableName;
				log.SL_SE_NKEvent = Events.ArrivalCode;
			}
			Assert("Just created log should not be in database.", !log.IsInDatabase);

			EventHandler failureCauser = (_, x_) =>
			{
				var error = SqlExceptionBuilder.CreateSqlError(-2, byte.MaxValue, byte.MinValue, Db.Connection.ServerName, "Timeout expired", "", 0);
				var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
				var sqlException = SqlExceptionBuilder.CreateSqlException(errors);

				throw new ZDataException(sqlException, ((INeedRow)dummy).Row, Db.Connection);
			};

			Factory.CheckSaveButNotUpdatedInSameConnectionForTest = true;
			((IBusinessObjectFactoryInternals)Factory).RowFactory.CommittingTransaction += failureCauser;
			AssertExceptionThrown<ZSaveErrorAfterCommitInDbException>(Factory.Save);
			Assert("Log object should not be marked as saved.", !log.IsInDatabase);

			var query = new ZQuery(StmALogSchema.SL_Parent, dummy.PK);
			query.AddToFilter(StmALogSchema.SL_Table, dummy.TableName);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ArrivalCode);
			AssertEquals("Log should have been actually saved to db.", 1, new BusinessObjectFactory { RefreshEnabled = false }.GetDatabaseCount(typeof(StmALog), query));

			((IBusinessObjectFactoryInternals)Factory).RowFactory.CommittingTransaction -= failureCauser;
			AssertNoExceptionThrown(Factory.Save);
			Assert("Now log should be marked as saved.", log.IsInDatabase);

			AssertEquals("Record will be duplicated if factory is saved after exception.", 2, new BusinessObjectFactory { RefreshEnabled = false }.GetDatabaseCount(typeof(StmALog), query));
		}

		#endregion

		#region Test Classes

		class BusinessObjectWithEventLoggedDates : DummyEnterpriseBusinessObject
		{
			public BusinessObjectWithEventLoggedDates(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region EstimatedDate

			[EventDateProperty(AutoEvents.ArrivalCode, EstimateActual.Estimate)]
			public ZDateTimeOffset EstimatedDate { get; set; }
			public ZPropertyInfo EstimatedDateInfo
			{
				get { return GetZPropertyInfo(nameof(EstimatedDate)); }
			}

			#endregion

			#region ActualDate

			[EventDateProperty(AutoEvents.ArrivalCode, EstimateActual.Actual)]
			public ZDateTimeOffset ActualDate { get; set; }
			public ZPropertyInfo ActualDateInfo
			{
				get { return GetZPropertyInfo(nameof(ActualDate)); }
			}

			#endregion
		}

		class TestStmALog : StmALog
		{
			public TestStmALog(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZDateTime OnSaving_CompleteTime;

			public override void OnSaving()
			{
				base.OnSaving();
				OnSaving_CompleteTime = DateTime.UtcNow; // latency between ZDateTime and DateTime.Now will affect the outcome of the test. This test may be unreliable if run on a remote database.
				Thread.Sleep(50);
			}
		}

		#endregion

		#region Implementation

		public override void TestSettingDateTimeFieldsWithInvalidDateDoesntCauseTheInvalidDateToGetDefaultedToOtherFields()
		{
			Assert("This should not be run on StmALog as it tries to set fields it can't on this type of object.", true);
		}

		protected override bool CanPersistedObjectBeDeleted
		{
			get { return false; }
		}

		BusinessObjectWithEventLoggedDates BusinessObjectWithLoggedDates
		{
			get
			{
				if (businessObjectWithLoggedDates == null)
				{
					businessObjectWithLoggedDates = Factory.New<BusinessObjectWithEventLoggedDates>();
				}
				return businessObjectWithLoggedDates;
			}
		}
		BusinessObjectWithEventLoggedDates businessObjectWithLoggedDates;

		protected override void SetUp()
		{
			base.SetUp();
			Event = Factory.LoadTop1<StmEvent>(new ZQuery(StmEventSchema.SE_Code, "ADD"));
			Log = Factory.NewWithValidTestData<TestStmALog>();
			Lock = Log.LockForUpdatingKeyFieldsForTesting();
			Extensions.Factory = Factory;
		}

		protected override void TearDown()
		{
			base.TearDown();
			Lock.Dispose();
		}

		protected override BusinessObject GetBusinessObjectInNewFactory(BusinessObject bizObj, BusinessObjectFactory newFactory)
		{
			return newFactory.LoadTop1(bizObj.GetType(), new ZQuery(StmALogSchema.SL_Parent, ((BaseStmALog)bizObj).SL_Parent));
		}

		StmEvent Event;
		TestStmALog Log;
		IDisposable Lock;
		#endregion
	}
}
