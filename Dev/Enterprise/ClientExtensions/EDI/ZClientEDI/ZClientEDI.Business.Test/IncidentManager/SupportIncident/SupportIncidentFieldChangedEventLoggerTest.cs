using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(SupportIncidentFieldChangedEventLogger))]
	public class SupportIncidentFieldChangedEventLoggerTest : TestCaseWithFactory
	{
		public void TestFieldChangeEventParser()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var logger = new SupportIncidentFieldChangedEventLoggerForTest(incident);
			var parser = logger.Parser;

			var str1 = parser.Match("-to");
			Assert(!str1.Success);

			var str2 = parser.Match("!@#$%^&*()_+=- - abc to ABC");
			Assert(str2.Success);

			var str3 = parser.Match("abc - 123 to abcdefg");
			Assert(str3.Success);

			var str4 = parser.Match(" -  to ");
			Assert(str4.Success);
			AssertNullOrEmpty(str4.Groups["FieldDescription"].Value);
			AssertNullOrEmpty(str4.Groups["From"].Value);
			AssertNullOrEmpty(str4.Groups["To"].Value);

			var str5 = parser.Match("123 xyz ABC_ - abc to ABC");
			Assert(str5.Success);
			AssertEquals("123 xyz ABC_", str5.Groups["FieldDescription"].Value);
			AssertEquals("abc", str5.Groups["From"].Value);
			AssertEquals("ABC", str5.Groups["To"].Value);

			var edgeCaseString = @"0cPd?R^Z=tE7W(QbDu+6X*Hw8jg&iM1yFr2BhUoKfzGJ9q`Nln3vs,[]LkCxVAIm5Y$OaS`4%TpT!-e{:Wy}#|@_r)';(""9.<-==}^bhXu cN/\";
			var str6 = parser.Match($"{edgeCaseString} - {edgeCaseString} to {edgeCaseString}");
			Assert(str6.Success);
			AssertEquals(edgeCaseString, str6.Groups["FieldDescription"].Value);
			AssertEquals(edgeCaseString, str6.Groups["From"].Value);
			AssertEquals(edgeCaseString, str6.Groups["To"].Value);

			var str7 = parser.Match("- - - to -");
			Assert(str7.Success);
			AssertEquals("-", str7.Groups["FieldDescription"].Value);
			AssertEquals("-", str7.Groups["From"].Value);
			AssertEquals("-", str7.Groups["To"].Value);
		}

		public void TestTryParseFieldChangedEvent()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var logger = new SupportIncidentFieldChangedEventLoggerForTest(incident);

			var cid = new KeyValuePair<string, string>("CID", "CDCC8E94-3052-4FDA-B695-CC876C74B890");
			var log1 = incident.Logs.AddNew(Events.StatusChange, "123 xyz ABC_ - abc to ABC", cid);
			Assert(logger.TryParseFieldChangedEvent(log1, out var log1Object));
			AssertEquals("123 xyz ABC_", log1Object.FieldDescription);
			AssertEquals("abc", log1Object.From);
			AssertEquals("ABC", log1Object.To);

			var edgeCaseString = @"0cPd?R^Z=tE7W(QbDu+6X*Hw8jg&iM1yFr2BhUoKfzGJ9q`Nln3vs,[]LkCxVAIm5Y$OaS`4%TpT!-e{:Wy}#@_r)';(""9.<-}^bhXu cN/\";
			var log2 = incident.Logs.AddNew(Events.StatusChange, $"{edgeCaseString} - {edgeCaseString} to {edgeCaseString}", cid);
			Assert(logger.TryParseFieldChangedEvent(log2, out var log2Object));
			AssertEquals(edgeCaseString, log2Object.FieldDescription);
			AssertEquals(edgeCaseString, log2Object.From);
			AssertEquals(edgeCaseString, log2Object.To);

			var log3 = incident.Logs.AddNew(Events.StatusChange, $"{edgeCaseString} - {edgeCaseString} to", cid);
			Assert(logger.TryParseFieldChangedEvent(log3, out var log3Object));
			AssertEquals(edgeCaseString, log3Object.FieldDescription);
			AssertEquals(edgeCaseString, log3Object.From);
			AssertEquals(string.Empty, log3Object.To);
		}

		public void TestLogFieldChange()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Other, "");
			Factory.Save();
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Other, incident.IM_ClosureResolution);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
			Factory.Save();
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, incident.IM_ClosureResolution);

			var logger = new SupportIncidentFieldChangedEventLoggerForTest(incident);
			var log = logger.MostRecentLogStatusChange("Disposition");
			AssertNotNull(log);
			Assert(logger.TryParseFieldChangedEvent(log, out var logObject));
			AssertEquals("Disposition", logObject.FieldDescription);
			AssertEquals(string.Empty, logObject.From);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, logObject.To);
			AssertEquals("Disposition -  to CLS", log.SL_Reference);
		}

		public void TestLogFieldChange_ResolvedToClosed()
		{
			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var any = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = registryValue.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			var resolvedCode = "ZZZ";
			var resolvedDecription = "ZZZ Description";
			registryValue.AddSystemChildren(registryValue.Add(resolvedCode, (NoResString)resolvedDecription, supportParent, ZBool.True));

			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var incident = Factory.NewWithValidTestData<SupportIncident>();
				incident.CloseIncident("ZZZ", "");
				Factory.Save();
				AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Resolved, incident.IM_ResolutionCode);
				AssertEquals("ZZZ", incident.IM_ClosureResolution);

				incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
				Factory.Save();
				AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
				AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, incident.IM_ClosureResolution);

				var logger = new SupportIncidentFieldChangedEventLoggerForTest(incident);
				var log = logger.MostRecentLogStatusChange("Disposition");
				AssertNotNull(log);
				Assert(logger.TryParseFieldChangedEvent(log, out var logObject));
				AssertEquals("Disposition", logObject.FieldDescription);
				AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Resolved, logObject.From);
				AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, logObject.To);
				AssertEquals("Disposition - SLV to CLS", log.SL_Reference);
			}
		}

		public void TestTryGetPreviousFieldValue_ParentIsNotConsistent()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var logger = new SupportIncidentFieldChangedEventLoggerForTest(incident);

			var result = logger.TryGetPreviousFieldValue<ZString>(incident.IM_ResolutionCodeInfo, out _);
			AssertEquals(LoadingPreviousFieldValueResult.ParentCouldNotBeLoadedFromDatabase, result);

			Factory.Save();
			result = logger.TryGetPreviousFieldValue<ZString>(incident.IM_ResolutionCodeInfo, out var fieldValue);
			AssertEquals(LoadingPreviousFieldValueResult.Success, result);
			AssertNullOrEmpty(fieldValue);

			incident.Delete();
			result = logger.TryGetPreviousFieldValue<ZString>(incident.IM_ResolutionCodeInfo, out _);
			AssertEquals(LoadingPreviousFieldValueResult.ParentHasBeenDeleted, result);

			Factory.Save();
			result = logger.TryGetPreviousFieldValue<ZString>(incident.IM_ResolutionCodeInfo, out _);
			AssertEquals(LoadingPreviousFieldValueResult.ParentCouldNotBeLoadedFromDatabase, result);
		}

		public void TestGetPreviousFieldValue_FieldDoseNotHasAttribute()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var logger = new SupportIncidentFieldChangedEventLoggerForTest(incident);
			Factory.Save();

			Assert(incident.IM_IncidentNumberInfo.GetAttribute<LoggingValueChangesAttribute>() == null);
			var result = logger.TryGetPreviousFieldValue<ZString>(incident.IM_IncidentNumberInfo, out _);
			AssertEquals(LoadingPreviousFieldValueResult.PropertyDoesNotHaveDescription, result);

			Factory.Save();
			Assert(incident.IM_ResolutionCodeInfo.GetAttribute<LoggingValueChangesAttribute>() != null);
			result = logger.TryGetPreviousFieldValue<ZString>(incident.IM_ResolutionCodeInfo, out var fieldValue);
			AssertEquals(LoadingPreviousFieldValueResult.Success, result);
			AssertNullOrEmpty(fieldValue);
		}

		public void TestGetPreviousFieldValue_PropertyHasChanges()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_ResolutionCode = "ADD";
			var logger = new SupportIncidentFieldChangedEventLoggerForTest(incident);
			Factory.Save();

			var result = logger.TryGetPreviousFieldValue<ZString>(incident.IM_ResolutionCodeInfo, out var fieldValue);
			AssertEquals(LoadingPreviousFieldValueResult.Success, result);
			AssertNullOrEmpty(fieldValue);

			incident.IM_ResolutionCode = "WRK";
			result = logger.TryGetPreviousFieldValue(incident.IM_ResolutionCodeInfo, out fieldValue);
			AssertEquals(LoadingPreviousFieldValueResult.Success, result);
			AssertEquals("ADD", fieldValue);
			Factory.Save();

			result = logger.TryGetPreviousFieldValue(incident.IM_ResolutionCodeInfo, out fieldValue);
			AssertEquals(LoadingPreviousFieldValueResult.Success, result);
			AssertEquals("ADD", fieldValue);
		}

		public void TestGetPreviousFieldValue_Concurrency()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_ResolutionCode = "ADD";
			var logger = new SupportIncidentFieldChangedEventLoggerForTest(incident);
			Factory.Save();

			var result = logger.TryGetPreviousFieldValue<ZString>(incident.IM_ResolutionCodeInfo, out var fieldValue);
			AssertEquals(LoadingPreviousFieldValueResult.Success, result);
			AssertNullOrEmpty(fieldValue);

			var incidentFromAnotherFactory = (new BusinessObjectFactory() { RefreshEnabled = false }).LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));
			incidentFromAnotherFactory.IM_ResolutionCode = "WRK";
			incidentFromAnotherFactory.Factory.Save();

			AssertContains("ADD to WRK", logger.MostRecentLogStatusChange(SupportIncident.ChangedFieldDescription.IM_ResolutionCode).SL_Reference);
			result = logger.TryGetPreviousFieldValue(incident.IM_ResolutionCodeInfo, out fieldValue);
			AssertEquals(LoadingPreviousFieldValueResult.ParentDataOutDated, result);
			AssertEquals("ADD", fieldValue);
		}

		class SupportIncidentFieldChangedEventLoggerForTest : SupportIncidentFieldChangedEventLogger
		{
			public SupportIncidentFieldChangedEventLoggerForTest(SupportIncident incident) : base(incident)
			{
			}

			public Regex Parser => base.FieldChangeEventParser;
		}
	}
}
