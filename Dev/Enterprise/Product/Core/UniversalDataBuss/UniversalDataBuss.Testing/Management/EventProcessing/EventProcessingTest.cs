using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.UniversalDataBuss.ServiceTasks;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.UniversalDataBuss.Management.EventProcessing.Testing
{
	class EventProcessingTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestEventsWontLinkToDeclarationsOrConsolsUsingShortMBOLs()
		{
			eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var consol = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			consol[JobConsolSchema.JK_UniqueConsignRef] = "C00001111";
			consol[JobConsolSchema.JK_TransportMode] = "SEA";
			consol[JobConsolSchema.JK_MasterBillNum] = "TBA";

			var declaration = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			declaration[JobDeclarationSchema.JE_DeclarationReference] = "B00001111";
			declaration[JobDeclarationSchema.JE_TransportMode] = "SEA";
			declaration[JobDeclarationSchema.JE_MasterBill] = "TBA";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(UniversalEventForMBOLTBA);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Message Log Note", @"
Warning - Disregarded MBOL 'TBA' as it is less than 5 characters.
Warning - No Module found a Business Entity to link this Universal Event to.
Message Discarded.
".Trim(), message.GetLogNoteText());
			});
		}

		#region const string UniversalEventForMBOLTBA

		const string UniversalEventForMBOLTBA = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
	<EventTime>2016-05-12T12:03:04</EventTime>
	<EventType>ATH</EventType>
	<DataContext>
		<DataProvider>CargoWise One</DataProvider>
	</DataContext>

	<ContextCollection>
		<Context>
			<Type>MBOLNumber</Type>
			<Value>TBA</Value>
		</Context>
	</ContextCollection>
</Event>
</UniversalEvent>";

		#endregion

		public void TestEventsWontLinkTo200DeclarationsWithTheSameMAWB()
		{
			eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			for (int counter = 1; counter <= 200; counter++)
			{
				var declaration = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
				declaration[JobDeclarationSchema.JE_DeclarationReference] = "B" + counter.ToString().PadLeft(8, '0');
				declaration[JobDeclarationSchema.JE_TransportMode] = "SEA";
				declaration[JobDeclarationSchema.JE_MasterBill] = "CRACKERJACK";
			}
			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(UniversalEventForMBOLCrackerJack);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Matching failed, more than 100 (200) matches were found in the [CustomsDeclaration] Data Context for this event using references from the Context Collection. This occurs when references specified are generic rather than unique.
Warning - No Module found a Business Entity to link this Universal Event to.
".Trim(), serviceTaskLog.ToString());
			});
		}

		#region const string UniversalEventForMBOLCrackerJack

		const string UniversalEventForMBOLCrackerJack = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
	<EventTime>2016-05-12T12:03:04</EventTime>
	<EventType>ATH</EventType>
	<DataContext>
		<DataProvider>CargoWise One</DataProvider>
	</DataContext>

	<ContextCollection>
		<Context>
			<Type>MBOLNumber</Type>
			<Value>CRACKERJACK</Value>
		</Context>
	</ContextCollection>
</Event>
</UniversalEvent>";

		#endregion

		#region Internal Universal XML Sending

		public void TestProcessEventDataObjectWithNoDataTarget()
		{
			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var universalEvent = new UniversalEvent();
			universalEvent.EventType = "ATH";
			universalEvent.ContextCollection = new List<Context>();
			var message = GetQueuedUniversalEventMessage(string.Empty);

			var sessionTracker = manager.Process(message, universalEvent);
			var attemptedImports = sessionTracker.ImportResults;
			AssertMultilineASCIIEquals("attemptedImports"
				, "False|Warning - No Module found a Business Entity to link this Universal Event to.|NULL"
				, attemptedImports.FormatAndOrderImportAttempts());
		}

		public void TestProcessEventDataObjectWithSuccessfulImport()
		{
			var universalEvent = new UniversalEvent();
			universalEvent.EventType = "ATH";
			universalEvent.EventReference = "TestProcessEventDataObjectWithSuccessfulImport";
			universalEvent.DataContext = DataContextFactory.New();
			universalEvent.DataContext.AddDataTarget(DataContextType.ForwardingShipment, null);
			universalEvent.DataContext.AddDataTarget(DataContextType.ForwardingConsol, null);
			universalEvent.DataContext.CodesMappedToTarget = true;
			universalEvent.ContextCollection = new List<Context>() { new Context { Type = "HBOLNumber", Value = "House" }, new Context { Type = "MBOLNumber", Value = "MASTERB1" } };

			universalEvent.AttachedDocumentCollection = new List<AttachedDocument>();
			var attachedDocument = new AttachedDocument();
			var type = new DocumentType();
			type.Code = "CAD";
			type.Description = "Cartage Advice";
			attachedDocument.Type = type;
			attachedDocument.FileName = "TestCartageAdviceFile";
			attachedDocument.IsPublished = true;
			attachedDocument.VisibleBranchCode = GlbBranch.CurrentBranch.GB_Code;
			attachedDocument.VisibleCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			attachedDocument.VisibleDepartmentCode = GlbDepartment.CurrentDepartment.GE_Code;
			attachedDocument.ImageData = (SubStreamableStream)new MemoryStream(new byte[] { 1, 2, 3 });

			var attachedDocument1 = new AttachedDocument();
			var type1 = new DocumentType();
			type1.Code = "CAD";
			type1.Description = "Cartage Advice";
			attachedDocument1.Type = type;
			attachedDocument1.FileName = "TestCartageAdviceFile1";
			attachedDocument1.IsPublished = true;
			attachedDocument1.VisibleBranchCode = GlbBranch.CurrentBranch.GB_Code;
			attachedDocument1.VisibleCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			attachedDocument1.VisibleDepartmentCode = GlbDepartment.CurrentDepartment.GE_Code;
			attachedDocument1.ImageData = (SubStreamableStream)new MemoryStream(new byte[] { 4, 5, 6 });

			universalEvent.AttachedDocumentCollection.Add(attachedDocument);
			universalEvent.AttachedDocumentCollection.Add(attachedDocument1);

			var shipment = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_HouseBill] = "House";

			var consol = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			consol[JobConsolSchema.JK_MasterBillNum] = "MASTERB1";

			var message = GetQueuedUniversalEventMessage(Factory, string.Empty);

			Factory.SaveForTesting();

			var messageFactory = new BusinessObjectFactory();
			var reloadedMessage = messageFactory.Load<IEDIMessage>(message.PK);

			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			using (messageFactory.AddDisposableService())
			{
				var sessionTracker = manager.Process(reloadedMessage, universalEvent);
				var attemptedImports = sessionTracker.ImportResults;
				AssertMultilineASCIIEquals("attemptedImports"
					, @"True|Successfully Added eDoc: TestCartageAdviceFile.
Adding eDoc with a document type of CAD and name of TestCartageAdviceFile.
Successfully Added eDoc: TestCartageAdviceFile1.
Adding eDoc with a document type of CAD and name of TestCartageAdviceFile1.
Linked Event to Shipment S00001000 (House Bill='HOUSE').|ForwardingConsol"
					, attemptedImports.FormatAndOrderImportAttempts());

				messageFactory.Save();
			}
			AssertEquals(1, shipment.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "ATH")).Length);
			AssertEquals(2, shipment.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "DDI")).Length);

			AssertEquals(2, ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs.Count);
			var eDoc = ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs[0];
			AssertEquals("CAD", eDoc.DocType);
			AssertEquals("Cartage Advice", eDoc.Description);
			AssertEquals("TestCartageAdviceFile", eDoc.FileName);
			AssertEquals(true, eDoc.IsPublished);
			AssertArrayEqualsByElements(new byte[] { 1, 2, 3 }, eDoc.GetImageDataReader().ConvertToByteArrayAndCloseStream());
			AssertEquals(GlbCompany.CurrentCompany.GC_Code, eDoc.VisibleCompanyCode);
			AssertEquals(GlbBranch.CurrentBranch.GB_Code, eDoc.VisibleBranchCode);
			AssertEquals(GlbDepartment.CurrentDepartment.GE_Code, eDoc.VisibleDepartmentCode);
		}

		public void TestProcessEventDataObjectWithSuccessfulImport_OnUniversalEventAdded()
		{
			var universalEvent = new UniversalEvent();
			universalEvent.EventType = "DCF";
			universalEvent.EventTime = ZDateTimeOffset.Now;
			universalEvent.DataContext = DataContextFactory.New();
			universalEvent.DataContext.AddDataTarget(DataContextType.ForwardingShipment, null);
			universalEvent.DataContext.AddDataTarget(DataContextType.ForwardingConsol, null);
			universalEvent.DataContext.CodesMappedToTarget = true;
			universalEvent.ContextCollection = new List<Context>() {
				new Context { Type = nameof(UniversalEvent.ContextTypes.HBOLNumber), Value = "House" },
				new Context { Type = nameof(UniversalEvent.ContextTypes.MBOLNumber), Value = "MASTERB1" },
				new Context { Type = nameof(UniversalEvent.ContextTypes.ReceivedFromName), Value = "Bob" } };

			var shipment = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_HouseBill] = "House";

			((BusinessObjectCollection)shipment["OuterPackLines"]).AddNew();
			((BusinessObject)shipment["DocsAndCartage"])[JobDocsAndCartageSchema.JP_DeliveryCartageCompleted] = ZDateTime.Now;

			var consol = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			consol[JobConsolSchema.JK_MasterBillNum] = "MASTERB1";
			AssertEquals(1, shipment.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "DCF")).Length);

			Factory.SaveForTesting();

			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var message = GetQueuedUniversalEventMessage(string.Empty);
			var sessionTracker = manager.Process(message, universalEvent);
			var attemptedImports = sessionTracker.ImportResults;
			AssertMultilineASCIIEquals("attemptedImports"
				,
@"True|'Goods Signed By' has been updated to value 'Bob' on the confirmation of Shipment S00001000 (House Bill='HOUSE').
Linked Event to Shipment S00001000 (House Bill='HOUSE').|ForwardingConsol"
				, attemptedImports.FormatAndOrderImportAttempts());

			AssertEquals(2, shipment.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "DCF")).Length);
			AssertEquals("Bob", shipment["DeliveryGoodsSignedForBy"]);
		}

		public void TestAttachedDocumentNotLinkedToParentThatHasNoeDocs()
		{
			var consol = Factory.BOFactory.New<Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = "08112347893";

			var leg0 = consol.Transports_Get(0);
			leg0.JW_RL_NKDiscPort = "AUSYD";
			leg0.JW_RL_NKLoadPort = "CNNKG";

			Factory.SaveForTesting();

			#region consolUniversalEventWithFieldUpdates

			const string consolUniversalEventWithAttachedDocument = @"<UniversalEvent>
	<Event>
	<EventType>DEP</EventType>
	<EventTime>2012-04-12T14:11:59</EventTime>
	<ContextCollection>
		<Context>
		<Type>MAWBNumber</Type>
		<Value>081-12347893</Value>
		</Context>
		<Context>
		<Type>FlightNumber</Type>
		<Value>6852S</Value>
		</Context>
	</ContextCollection>
	<AttachedDocumentCollection>
		<AttachedDocument>
		<FileName>TestCartageAdviceFile</FileName>
		<Type>
			<Code>CAD</Code>
			<Description>""Cartage Advice""</Description>
		</Type>
		<ImageData>AQID</ImageData>
				<IsPublished>true</IsPublished>
				<VisibleBranchCode></VisibleBranchCode>
				<VisibleCompanyCode></VisibleCompanyCode>
				<VisibleDepartmentCode></VisibleDepartmentCode>
		</AttachedDocument>
	</AttachedDocumentCollection>
	</Event>
</UniversalEvent>";

			#endregion

			var message = GetQueuedUniversalEventMessage(consolUniversalEventWithAttachedDocument);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				Factory.SaveForTesting();
				AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Message Log Note", @"
Warning - Could not link Attached Documents to Transport Leg (Consol='C00001000', Flight=''). Does not have eDocs.
Linked Event to Transport Leg (Consol='C00001000', Flight='').
".Trim(), message.GetLogNoteText());
			});
		}

		[ExpectNoExceptions]
		public void TestProcessInvalidAttachedDocument()
		{
			var setupFactory = new BusinessObjectFactory();
			var shipment = setupFactory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001953";
			setupFactory.Save();

			var message = GetQueuedUniversalEventMessage(Factory, AddStorageDocHavingInvalidImageDataWithUniversalEvent);
			Factory.SaveForTesting();

			var messageFactory = new BusinessObjectFactory();
			var messageToProcess = messageFactory.Load<IEDIMessage>(message.PK);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			manager.Process(messageToProcess);

			AssertEquals(EDIMessageStatusList.Codes.Warning, messageToProcess.EM_Status);
			AssertContains("Message Log Note", "<Event>.<AttachedDocumentCollection>.<AttachedDocument>.<ImageData> - Data should not be empty".Trim(), messageToProcess.GetLogNoteText());
		}

		#region const string AddEmptyStorageDocWithUniversalEvent

		const string AddStorageDocHavingInvalidImageDataWithUniversalEvent = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
			<Event>
				<DataContext>
					<DataTargetCollection>
						<DataTarget>
							<Type>ForwardingShipment</Type>
							<Key>S00001953</Key>
						</DataTarget>
					</DataTargetCollection>
					<CodesMappedToTarget>true</CodesMappedToTarget>
				</DataContext>

				<EventTime>2014-08-18T10:36:33.557</EventTime>
				<EventType>DDI</EventType>
				<IsEstimate>false</IsEstimate>

				<ContextCollection>
					<Context>
						<Type>OrderNumber</Type>
						<Value>3380343</Value>
					</Context>
				</ContextCollection>
				<AttachedDocumentCollection>
				<AttachedDocument>
					<FileName />
					<ImageData/>
					<Type>
						<Code/>
						<Description/>
					</Type>
					<IsPublished/>
					<SaveDateUTC/>
					<SavedBy>
						<Code/>
						<Name/>
					</SavedBy>
					<VisibleBranchCode/>
					<VisibleCompanyCode/>
					<VisibleDepartmentCode/>
				</AttachedDocument>
				</AttachedDocumentCollection>
			</Event>
		</UniversalEvent>";

		#endregion

		[ExpectNoExceptions]
		public void TestProcessAttachedDocumentWithWhiteSpaceInBase64_case1()
		{
			AssertProcessAttachedDocumentWithWhiteSpaceInBase64(" SGVsb  G8sIF  dvcmx   kIQ==");
		}

		[ExpectNoExceptions]
		public void TestProcessAttachedDocumentWithWhiteSpaceInBase64_case2()
		{
			AssertProcessAttachedDocumentWithWhiteSpaceInBase64(" SGVsb  G8sIF  dvcmx   kIQ== ");
		}

		public void TestProcessAttachedDocumentWithWhiteSpaceInBase64_case4()
		{
			AssertProcessAttachedDocumentWithWhiteSpaceInBase64(@"
		JVBERi0xLjQKJdP0zOEKMSAwIG9iago8PAovQ3JlYXRpb25EYXRlKEQ6MjAyMTA0MjYxMTE3NDMrMDAnMDAnKQovQ3JlYXRvcihQREZzaGFycCAxLjUwLjUxNDctZ2RpIFwod3d3LnBkZnNoYXJwLmNvbVwpKQovUHJvZHVjZXIoUERGc2hhcnAgMS41MC41MTQ3LWdkaSBcKHd3dy5wZGZzaGFycC5jb21cKSkKPj4KZW5kb2JqCjIgMCBvYmoKPDwKL1R5cGUvQ2F0YWxvZwovUGFnZXMgMyAwIFIKPj4KZW5kb2JqCjMgMCBvYmoKPDwKL1R5cGUvUGFnZXMKL0NvdW50IDEKL0tpZHNbNCAwIFJdCj4 + CmVuZG9iago0IDAgb2JqCjw8Ci9UeXBlL1BhZ2UKL01lZGlhQm94WzAgMCA1OTUgODQyXQovUGFyZW50IDMgMCBSCi9Db250ZW50cyA1IDAgUgovUmVzb3VyY2VzCjw8Ci9Qcm9jU2V0IFsvUERGL1RleHQvSW1hZ2VCL0ltYWdlQy9JbWFnZUldCi9FeHRHU3RhdGUKPDwKL0dTMCA2IDAgUgo + PgovWE9iamVjdAo8PAovSTAgNyAwIFIKPj4KPj4KL0dyb3VwCjw8Ci9DUy9EZXZpY2VSR0IKL1MvVHJhbnNwYXJlbmN5Cj4 + Cj4 + CmVuZG9iago1IDAgb2JqCjw8Ci9MZW5ndGggODQKL0ZpbHRlci9GbGF0ZURlY29kZQo + PgpzdHJlYW0KeNpVijEOgCAQBPt7xb4A7pADrzcxlsYnWFAZg / 4 / EejMNDuTrVSJXVQTjgbu / Oy8aEQ8hfx6MMpLFRIsjSpzSC5oW9llnWL7w2 + M5cZOnQ + zwxOqCmVuZHN0cmVhbQplbmRvYmoKNiAwIG9iago8PAovVHlwZS9FeHRHU3RhdGUKL2NhIDEKPj4KZW5kb2JqCjcgMCBvYmoKPDwKL1R5cGUvWE9iamVjdAovU3VidHlwZS9JbWFnZQovTGVuZ3RoIDE1NTExCi9GaWx0ZXIvRmxhdGVEZWNvZGUKL1dpZHRoIDE3MjgKL0hlaWdodCAyNDM1Ci9CaXRzUGVyQ29tcG9uZW50IDEKL0NvbG9yU3BhY2UvRGV2aWNlR3JheQovSW50ZXJwb2xhdGUgdHJ1ZQo + PgpzdHJlYW0KeNrt3V9sJEd + H / AatS6t4GSVgAtgAxa2NzgEyEusde4ACziatY4C + FEIkLwl0QF + yOPJuMCn4OitWYxhxrCTyaMfDG + MPCQvgZD4IQZiH3vUjpnk7JsAQnJBzjGbGSeLRLZZ1EhmUyx25fer6hkOl392ZsgZdvd + e6VdLskd9md + Vb / 6VU33lMAhRKeWD7Wa4 + EC3 / sKmsYdH / fxFOBodkbEUZ8DCb02RxdPAQ4cOJCucaCVt / B4GaHFMT0erPOH3cfzjQMHjrPjJTwFONDq4cKBA3NFHDhWO4PKF / 8nUQPGhmUePUJ7wlGLo / ucr3fut9MNlxCvr / hcHiJecK1hfH51jfXO7cTr1dU98V20w9t1LfEAa1iMaOv1aK29zu72 + te8w2rayufxoXiRjhdLi6Pms8HaHM2 / lOT1dReGtcpczY1fulJXO8dsHKs7lp1rvIAX472O1vJCHC9THfNX / vS / 2r / xyc / rP3XO2Z3PS / cvvvVzVn87 + 7dbdTnLS14D + hJPgL + 26dyeW / ww65qozpNxev9EHbnbOcydRqnzF7++6VZyrN0VpX9Ju9Ufi7k6C7WuV86Ky5ce / r1 / d + LWeMzheuUaxsWFrdc7r / P3xTtH7i6PG7bDzmzJ6t51tTnMYhOrZ9Le / Z9wNT2KxZJxcL32LVf3o5in2O2Z8juuWUdx6RKL0Seu2ceZK3JtOs5cuqUuBxdc6F9woR2if8EFF1xwwQUXXHDBBRdccMEFF + bLcMEF140Oi3jBBRdccCHPI16IF + YpiBdcyIe1dSnECy7kDcQL7RB5HvGqjQvxQrwQL7jQDuGCC3kD8UK8XoR6Hi644Fqdy7XUhXEZLrjgggsuuOCCCy644IILLrjgggsuuOCCqw2uB4gXXHDBBRde / 4ILLrjgggsuuOCCCy644IILLrjgggsuuOCCCy644IILLrjgggsuuOCCCy644IILLrjgggsuuOCCCy644IILLrjgggsuuOCCCy644IILLrjgggsuuOCCCy644IILLrgWOkq40A7hQjuECy7kDbjgggsuuODCuAwXXHAhH8LVdtdROMmLXxg20HUw / ag6q + LiufVrnzd6fBbnPhPPuKIrXLK + rmF1AvSbueKcheg0xlVUf6hwarZzwTUbL + G / p4au09m / ZGzQlSt + vit2qa5ZvLJJePQzfan6hNWxm7axq1zS5Wo511k + TG4I2XFuf9ZVanehb / Quxsuos89dyHUyfPlSVzlnvBZ2nW6ps7D4ZzA / 51IXXTPtcNJnfPiuiFcpXZGce4wPnKN2W8hCuFT6p + nG49fpsyOkef9al73E5S66zPXt0FJTpMcoueFKYQWdsDJpVAhNQ0Dvhv2rlJPzOTtL64otZXU2ezbnXMV8rkvi1fcPFFWuUqhC5YWy6lXxltBGJCbtFOIRDQG9G7bDMp669Fm6pngV + ly8Ds + 1hDnjFVynKrSwyJkodkLZByL0ImmFLNTAksvvlmeENEIUwsbpzeMVT0hnZ2k0xesZ1 / lx6gpXcebisKQRNS6j85gAqrjfcbmILf33gDe4YpcRcaEiS0GjcBmV2tjI1Ekr85lOcAPXM / Eyzybx + VxGmff5O9O3KB9yG + lSX1HGpS / 5wDykfMAuwvIJ0XfKXEUTl41sMnCRSYyWVhnlVhEv90x + ji91GWpA1FME / SDNWaYri / vcc7qvKg4LxyunlkWVhTSvx4XoutTEJolsZIKLm1uxIbkdRmVsk57r0XOjJbUXdeM8H0 / qn9l2eFW8nlD7euyOjIemD6hHKCsSigYnfxEZrvnox7Ir4nilG3SSUc6 / FbGh0NBvAxsXlWvgBrOubZdVLntj10w7VFe3w4c0WyLML0ZZd0PoLPWluHhFpZTBuLOziygF13xlp6vo9Hv8sKmLzSNZcH8ppHflLrfSVq7M5cUDxfkwpl6VROTS7HI3d7lL8jz9tevMmxSRDW24xpNC5S7tuJejrnhDqODiuKQeZd6gJ6dnZcq9ntJ3Si4i0VPDLYvSnhzw2MBlSOVKQv / q0V8eaDtxxW6fXfGtuEK8qFIqNhRXqpaGRWpGKo0yiojOue90KSz0GzWsLvfWyhV3KQAyLelc6UGoCeUUE86GueLT5 + ZN46uxSSkzN + DsILnp0ScqV9xzprjv2LUdXINCF5qT / W24UmneoxMr3qMBRuhCJGlM / d27hE75Rwjf / VNNrg1RJGFcpmzMTzMlsQGPNtSEcu4dFHej4n1uzPTU06etYheNvsVmcG3Nut4XbhovOtPQDud1XZ8POQfzuCl4gOHBsRtT + 0rjno0obJy7xBb1EOofXd11HSuDy5 +/ o1PeHvBo06NTKryLshmfPrsiNzBUanKD414kXX / qin07NNQ8pvGauPJbcLnY5zHBLh5gknyD + kdqKSn1CpVq6jbOPS5lXiZWdSkZRhMXBSc0LDfwxZVxJ4ZGcx7VFWl95LjHWBUAynK8UiGm8eJPb1FGPRevgl1 / Td / cZaOOibmXx7nOqL9QCqN2FVO9YZRvaVzPxfv002mAIVc5cWmjht6V + nmKCSF03OsphD5ysooXdcvg2ibXpgquUm7PusrkC5yKOV7zziuvzfM2oTzGvZwTcUnnScms57bZxT + kcuXUTXjgpPhOXfxHnKowQczYpTle5OKwaL8esH / RNYmXd9k3EuszpWScq / L8Lcy / Ys5jCZ9U4hMx9Rfu75XrkeSOQJ / 23b9HHUmeuaiFcUVH5WwVL98OizOXj9f7wVVUrr / +s1tVvKykRFWNy75 / SY5XsYDr2rxBTxF1FW5E3LEpC9KDH3AaYNdW4ra9i / p95aJUYcPEik5IxIbnF / rKeOU + Xo81zbGI4JvtJM9b7l + UJKt4lUkVr / g26g3uTabKz / YezVPTUG / 0KF6546e4x65temKF9O3QTeJF588FeVVH9fIZ1 / ZgxN9Bk7uMkqSjBudCnt9m1yReiU8nsXeVVb3hbqmOYpcLLs0toUf9hf5SUqGd7dP80i9f + ngJ4ePFQ9Y0XlSgl1Th8veEeJkqXr1Qt3A7DA24ag7xrEv5YS2a9q8efTf3r2Leev5a1 + R5pobG8aLOQmeYUmOgeBWbQsRVvMQ0XmeuroxdHvF / Pl6cIoMrC67Y58PK5euN1Gd8Tjj0bA5cWqgeT0v63L + 46qIziam76 + w2XDwUcbxeEzyW0VNGYxq1w31nJ / GauDKfDytXoTv0JZqr2CpeRlfjVzwIpTPXG5T + fEcKrvys3shV6lLL8 + WcvkKulOZf9Aid25kvT + Oly3uCZ33UvnPvyn075MLA9Y3xz / FsnnfcsPwowPUGuYr3wrhsqnqjcvm6N / V1L / +0lEcMfsyuzqla5hogDeNyuhHzkyXoh9JAeSvt0ExdTnAxHvV8vIqtJNSQnA8dpcbs3Pi16UeBwQlXHNzl39OT + jANLm5ZjuYpPNFShZ9 / 6TTkQ17HNveoc + V + wsYunhrQM9zl9ajHt9MOff + yr / KkJY + oxtU8fqUzLhNcvdnxq9jipO + yjF1cH5rQT43aDvW8fswti4a4XD / mXsVTyqQzcXWozKYckT8Sr4bxq + BpA8H19euHc9ZRob / 7fHiPFWXE9ep21Q6TMEfb5njp3rl4KbMpS8oQ2SDES + XeZZN8Mv9SNInx6wD5j0Ucr5SHhSjkQ27g1OY5aYg32FUmpahWeq9N8wv0r + p55vHLVw7UL3o + H9KZ89Iz5Q2qo6w8F6 + CXZZ6EsWV63m / Ekhf8fMXk / AMQfO8U3f95JPzfO6HhUm8EjpDXxzmesnrAZLn5g3Oh8E15p5CdcdsHcXx4rM5q6MKLhtydlESy / 38y8rH / vnx3aXgNErNgOY9Og / rUQQw0TbNikrlVPXK5UfLvFowp4v7e9W / uLrIZlyaE3CoD / OSzmxS91quJ4mkIhPnG5H5ql8HED5LUySou / Bozc2AXUZKXg3V1i8umxu / nDNvfTjNY1xR0 + Q34zqB6w1D86 + Yz4XbYUldfhIvSyHycRGi79dteB3AJ1J2dTkN8HNZ8DrvJ9Va + e0dE9eD58RLV66E5imVK / Z1VJF0KY / xBJDjJVI9Gb + 8i9ejup0eucI6G68faqurlxv2Zn9GshLXc / J8wXmM + xdPkbm79XigiYoos1SUd4Wfs9MErSt0VR9Wru4rKo17pdB + XfTSl7L8aawqXs9ZZ6P0G / IY1wKFzN + ITSy6URFn5WTdhtuhS4Wb1PM0KoWXgTg04 + e9vOjT + rpdNF + WHa5e / LpNtc5J0yeKCfWYt8LcivKhcSa++Ar5PEdI6ytxXZs3eN3Gz1Me8jobTakSamaRd6Xvaxtd98r / XC5ZqLvoX5THQt37dS4AeF2U + pw894LWjVwufqjvIl4uTQp + EbasVvUueaHu4rHIKBQu1FizixLy8VVfGl99 + ocLnEe3cxeuaw5ztWu0yMNEtXPlt3IiO3Vz6dTV8Lixq3gtaqXLina6nJD1di1bx3RVO131PNrvUogXXHDBBRfyIVxoh4gXXLjPF + 0QLrjgggsuuOCCCy644IILLrjggmu9B1xwwbUGl0K84IILrhfahfoQLvQvuNAO0b8QL7jQDuGCCy641u2SrWJh / IILLrjgggsuuFBvIF5wYZ6CeMEF14vYv + BC / 0K84ILrDlwn7XP594b + uH0u / 27ko / a5 / FaRuy3tX / 2WupKW5o0Wup665 + 1h0EzXbktd2 + zS7XPxtubGtdN12ELXO2spN9bv4p + TwdUo17CFridrKQ / X79pbSxm1fhcNyaVqoau / ljJq / a6dtZRRd9K / CtdC1zfWUkbdyXy5ra6Ra2fe2G1pvPotdcl2um55y7nauNZRbtyFy + h2unLXVNd1PShbx2z59l2 + prXXPNjYRnV3ndXlw2kZEba7ve7B / rmspyuNL3aU7dDCJi5z3YOt5x1nFnd1p9 + ZTuMVPeO6pn + VwtXSZc + 2Ljxz8c7VesZ1XSaP6 + kqzlxdN + OyM9veG3fnx8IuE6dqJkrT / lWobL0j1C27cm3UbLaYunTDXW66Ra2cbYdNj9fgzBVfjBfvaf7TLt1rnit7Jl7UKN / 28dJZqu39LWVEHfYSXMJF1d + hT + gxTTnSiDf47Qn6IBPKig2VCyXiJrrcu1wzpLEVWw8UZX0jVJddqZCF2FLpw6SJ8Rr4sTlxIirEhtgkVy6SrpDsIur76rFNmpg3eDtYI2QpBDU6sRGnOi1kd0PmP5yqiPe9jkrZWNe92MrURTa2iVFZKbsuKlSqM5sUKnayWfVG6b8lp99zigyBtq20qlC7Lu66HrncgP8qXdwsV9j7mdI4jbtZQYI + h8eqyG33 + BMDt0 + f3fQb0zfJlXZC3SudF1glbWL5D / ckonjpxy6vXM3qX6kI85SIcv2 + 0exSvGoWcx2VcR1lyLUhXa9h8RJ + WtUVVDHl5Eq2ufLgeE1dFD7K / NEc8RrWxyWCKxe654xxlStNpF9k4vqQXMUGueaI15P1uO7P76JiY7Dz0Hz118ilqLtV8bKhHdqN + eKV1CleYeKVy64QRogkJlfqXRnn + RCvOduhqp / LVi7J8RLetU3x0iFvbFIk9p8 / FtbQVc7Eiwv40A7NpH / N5bKufi4ne2HJqU8MWeVD3w5zzodynvlyWFI1q1vTXthFo1UchSWnmMsmq87yYW79uNy / zvU5j + 3a6EIXytBYfveurqiWMuIqXtS / jHdtV / kwvaY + 3HefUhwN / UP6X / EvdmV1csls2g7pead6Y1IfDrisil2cT4defvWEPk + ifKIKv1ThXb3a1FGZd1X1hndtc4fLqnre8vzLeH / mHpOIXan / Vdk4XjrXRW3ilUdhvlySy9e9MbuM6pWVS / e4HdLkMveuXhWrWZepYpZyD1PGfbcO85Q4zCtLSWfKTzfFizIAzyvZlaRJxNr0veRZ18CrJqbcna7 + tb1F5pUhaorjkVlpvEulCUE4b8S8vkGfyIUyM67w / 747nj4O36GyWyNX1RrjnKbKURmlepvHZZEQxLsy4adjRuhJ / 8rcXrjP5tzBi6b92rmo0KBTj + kfOl9vvCYNF / iZi7KusrrUtjPHo7xTO1dBZb2l7 + 52nK8PaXbZ8Sl9OEf5tLZyfnFXSYEqRcIvxx447Zen9hb7kUfruMJ3iddh47DYpm / wU20NXTc + 5Dqu8L0b1 + qv8L0DV9Jel2mpa7 + NLtXK++i9K2uhq2ypi8fk7Ta69DquyIbrNl1JO11ruGHvTly2la52vu8Bu9Zww976XYVex / Wk3bvoX5 + 10UXxGrW0HY5a2g6ftjFeNHjJdrps0k6XUa10Jaluo4uvwmyjy4lOS11xHV3Vt46X / 5FpUmPXDVY213LV5bKugav3AdfkCBfTPG6dK3QP0U6X7cB1J0d3OZdpqSsftNOV5q1zeVG23854DVvkKuOZ8avurgXyfBG105WLdrqq + 6Sa4erC1Q33cbBrx1 + p1zZX6W + xbKerRe1QtNR1q + 1wWMe88TeTm8arfOIX6vWgBq70LB8uuW5zQr / 4zo / C33XDt + KvzCWWdC0wTzlyIwov359S8P0p / h6k8GuFrpWNy3vVXTeTe4nM9L6byd1f / IJRLe6TWqg + HMzcS5TO3tXm42VW7VogXtWaxpyu83d / zd775e9A3KlPPrSz86 / n9q / smXgZ / w9PZyenNZwvz + MKstEld0qt4a2kl3XdMJGN4LphPoxvuR1efzxdW7wWdNW8PLyD64j80W + pK6lfO7yNY / UXLt + Na / UXLqd30g5Xf + Hy3bhWf + Hy + vPGwTrKwzuI1946ysM7yhujlrqyFvYvPnZbGq9 + C11P3Drua7sbV9JCl1rLRnR34rJrdMk1ugrdznit4b62tbu4bx221NXG + 6TKnbXse7t + l1rLRnRrb4ec42UbXdqt4 / 6vu3AVqqXx0u10reMGlbtwHdbctb2kq1dHlxFfmHzYW9IV19M13XBtuXipUtbUldwoXmu5zXcJ11 + dllzLueQ6bvNdwqXt5FuXKvNKsZZbrBZ3VbsELhsvJ6J6uvTUNfNPjukLar72lcpauvKzeD1xfkvHxPWzrnLdV9eRD1bZDnu88aYV7heFM3lcylJmvMGUaLorct2I8r3 + BaFMGjlyfUj / POo230U5LeVdvxLzUoeGWZnyHkxpo13cv8oOuR7I7qY0MuV48Vu0J6ahrrCrsnFlbKXRj0vZLaVRxrtcb011xLxHOr8rbJxswn4VulfKjovJ5dth2DirmS7zUmiHhvfHSmIXPyaXNpwPH7sBX9faTFe4vjenupcM9yTv6tDj3RroowG5dFNd4XpsculClYnfDWabXI94 / HID02CXCi5HLoqX6 / MOZkZL512uqa5wHwfvFWDUd9gV8056Ph / Wz9Vd2FVSrXFfvDaN16QdVuNAU125e8xz5nt + d6xt3w4b3r8mrvTMdda / sqLp7ZB3zNZ + k86YXUW5fLx475 / 6xIvvxPCuSbyCa + 5x + XNnNW + 0wrt61sGVVvHiO2f4tZ7 + Odfz6w2jD / Vkh6zgWuF9UguPy5zPuRKkJB9cIc / v2wuukfvfFN39s3uJpnd9sYs + Ilcd4hXqKHJJcnF9yLvquUJLy / HiVjX73Rf2karu / ArxKqp41cFlQn1IXYvPKXbSx6vQsfXxkia56Mpn72vT1S5tQeVstZNTjeYpVM / 32JVxO4wMu8oovRCvZ + 9pK3RRXbtRrjrPp8usA / AmWY / tJF7pQ26Hz67bnKnGl7ww9Oe1XI8qo1ylD5Iqb3SFdz18 + Vw7vP4N9w5rF6 / c + dhwdqzyfB5xO3T5GwuMy + M6ulLeJIuyiO9f1GPCAm65SL3xUT1deeTKC8vsi7h2a + cKh74sXy7gGtbUdZlqEdfKp2rdW3Tpub959RfC3mK8zPyulW8rfYvxkgvcSsoX6zUkXvO / vpqs4 / 6v27veZt63UeblnTXe / 3VjV3fO11c5Vqu / kujW2uHcbcuuo4zC / V + 3emQtdJXrKA9vsX / N3Q / XUR7ewX2jvMF0v4XxOlxH2Ttx3W + pa43xGq / lvrapK1qXa7SW + 9rWH6 / RWvZruxPXGu5rW78rW8vOFet37a7lfr07cY3W6Ipa6lpnvHZb6uq30TVsgivJlnEldcwbb08niMrxK7CLu9ZQHi7xeuV0nbBMXLmEq7 + W + xDT5e + TIlcVL7WYax33IS7jqqbWVlXxWqxdDdfrihZ2 + f5VARdyDWrpmt4nZZPqDmu7YDvs1dI1vY + oXM61u5b7K5dxRWf5cOJaYGF6t5B1dOXnXHSKv8AXHi5wL1G2lvsilolXz + XKmdgmRSmHpZB5ZBdYfszSurqykkaFtGMTsyUzIqVRsUAZlq7l / sql4mXpu1Nhk5zGr + LlKI3M5vzTtzyqp0u7nhEdJ8SjJCWXUT3zptHz524j6xqvyKhBGZuNpEd1lHGZVfkiY62qmavsTV2OWmBxL4nIlQdX3fZvW8BVVNfBlhExbGLvJXxlr + Hxa7CeTV9X4wrXwVIdJY0z7HpV2sqV1dc1z / W93DPM30nlCbmo6P0R5fNGvV3RHN + qpvW8d72uOF7Su77eZFcynVemPPkS7CoE1VGpEM138XU1YuqyImmJK + FdBgp2JTxfFlHDXd3g0pQAhY / XS / 4ODhNZlTnX + Hj5mL3Hrte8q + y0xyWNX7SJ / bpNupTL + ntv0lq4VNUOc5 / nS19H0d / mdp24YnLPzdruu5nb5fyF5laV95IexeuUP762jjLndiay07uJ6uLKp65B5fLzFI7d4GLdO6juuEnP3SUVXLZWrkl9yC8Rh7qXXVwDq / zifpbn7ibSZ / E63w5HNXCFPc1DvFIr7WZiOF6ajPnFjfLS6u6vc3e1hZ3a9Gmt8mFYD + V4kYvmX + R6xHXvwH7RJNHl8crPdv5yn9c0z0 / ny3S + wsX5o6TYpPrwhzr2snWb1N9b + UkDxq / p + oYzW0J3ReldvIaz0DpbnV3igU47JaUOmfEmnZGr1XssLd0OC / G + Ml8sE0r19AgEXcMF1utw8b1ftm4RuqGrIQdccMEFF1xwwQUXXHDBBRdccMEFF1xwwQUXXHDBBRdccMEFF1xwwQUXXHd75IgXXHDBBRfyPOIFF1xwwQUXXHDBBRfqXrjQDuFC / 4IL / QsuuJDn2 + jqtNTVQzuECy7kDcQL7XDpI25nvKxopysXupXtMC1UK13xOra4WX87LBMn2 + iyeh1bLd2By92da5X96zTsdDu17bVmXFZDd3Lm4g + KYdiyuKmu44mr4JxoptUHbwFkD1z5pGn9q9rM0UxcxxNX6ZMj7w5T0Mgmm + ba5 / frnXG5ictqjhdvX3HoylXtf7dS10y8 + sHFQSz8hjlF4t81MdptmisPisrF9YaauHy8FG / B9LHabVreqFy5rlwfsIvf19JQyijd4RPOi1912811UayUcglv68bvQ3xoE / 9OxdQwbcfJprnSWZeecQ0K7XLF7VCVX26gS3vXh + w6faQIofwmxW5AofqQXX5fxKRprt8NKZFcJNii323l4i1XDhKj / ZBdrshlVufSvtX9HseLXFaVOux4m / kd6U0oRVa1R + fqXL + i / WZMv6jdu64oqNFxs9um4EWjysXJcVVvl7u6dvgryv0H + uM1ShquOKS5Crv65PoSN0ZlQhKxjat7X1M + Xq / xx8WYRmMmSIrPD2dHzu3kwVWs2nXr9cY95X6J / 9BuxxXZ1KVLlRFI74fOZhoXr4nLlTvO7LpD70qI9 + Y2udR + mIYdNs61qdwXKd9tcv8yfQJwk0tyZ78hR5QGR9YPyVnTXCWVhD9KeWHTnWqelIy5yZWq4 + w7MqPYZaUfklfsur8S1ze9i1ubpCTBro2OK96heJHLaE7xu02Ll1VJ + Zv0xxav2hgRXPZ + 5My9pEefy / INcn00bJ5LUlXLrtJv9ZZxirDiZ9z4O8nb9HEm2DV6p3EuLUsqKIrgeiXZ5dHMdkbuk82X36bopYLHaZM00GUpnxc2rAlULt5Xa4PGysylHc6PK3s9YmWuwsmCXQX3r5Hr73LqozHZ / TG7dl33x4tqta1xrn1yHfpCaeSGw5DSd93va8Gz5JdGPl5R01zG9bleOjTBtVtt / z10v + 96XNX3R34NR / x4w1yHbptd44JXEb3Lh6bvftUdSPs6fc6vVuUrWmc2q3P1euQaGXZlU5d0v8XVr1DBdWiuXRcd3twV3bori4PryLtkcCXuAbu + mrhs7MJU5bKjWlTt1S9eI5dRvcSLGUecLXZl + XZw / bhfq5GOW + m58nB2qSOvsStP6Kx3D2n6Ra6hLL1BUdFYBld + MCkPswa5aADm7QSHh + 5NPv2 + 9IZSUWHPu9j33e4omrzUF06 / lLfoylfo0uTqH / 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t01aUdWnErD1O/cXnYUpeDCy644IILLrjgasSRwgUX+hfihXghb8CF/oV4wQUXXHCt1xXBhXaIeCFecNX7/i/EC/kQLrjgggsuuOCCCy644IILLrjgggsuuOCCCy644IILLrjgggsuuOCCCy644IILLrjggguudrjuI15wwQUXXHDBBRdccMEFF1xwwQUXXHDBBRdccMG1xiNHvOCCCy7kDbRDtEPECy644IILLoxfcMEFF1xwwQUXXHDBBRdccMEFF1xwwQUXXKt0deCCCy644IILLrjgWuQwcKEdwgUXXMjzcKEdwgUXXMiHcMEFF1xwwQUXXHC1yyUQL8QLLrjgggsuuOCCCy644IILLrjgggsuuOCCCy644IILLrjgggsuuOCCCy644IILLrjggguuerjuI15wwQUXXHDBVYOjQLzgggsuuOCCCy644IILLrjgwrwS82W44EL/QrwQL8QLLrjgesHzhkY7RLzgggsujMtoh4gXXGiHiBdccMEFF1xwwQUXXHDBBRdccL1ALgsXXHDBBRdccMEF1w3rQwUX6nm4VpY3FFxwwQUXXHDBBRdccMEFF1xwwQUXXHDBBRdccMEFF1xwwQUXXHDBBRdccMEFF1xwwQUXXHDBBRdccMEFF1xwwQUXXHDBteRRwgUXXHDBNeO6j3jBBRdccMEFF1xwYb6Mdoh2iHitzZXABRdccMEFF1xwwQUXXHDBBRdccDXzQLzgQjtEvBAvuBrdvyTyBtohXHAhH8IFF/oXXHDBBRdccMF1wSXhggv9Cy60Q7jgggsuuOCCCy644IILLrjgggsuuOCCCy644IILLrjgggsuuOCCCy644IILLrjgggsuuOCCCy644IILLrjgggsuuOCCCy644IILLrjgggsuuOCCCy644IILLrjggmuFrvuIF1xwwQUXXHDBBRdcL4grhgvtEPGCCy70L7jgggsuuOCCCy644IILLrjgggsuuOCCCy644IILrhfYFSFecMGF/oV4wYX+hXghXnDBBRdcyPOIF1xwwQUXXHDBBRdccMEFF1xwwQUXXHDBBRdccMEFF1xwwQUXXHDBBRdccMEFF1xwwQUXXHDBBRdccMEFF1xwwTWHq4N4IV5wwQUXXHDBBRdccMEFF1xwwQUXXGt0Yd0GLrjggqu9rvuIF1xwwQUXXHDBBRdccMEFF1xwwQUXXHDdiksgXogX4oV4wYXxC+0QLrRDuNC/4IILLrjgggsuuOCCCy644IILLrjgggsuuOCCCy644IILLrjgggsuuOCCCy644IILLrjgggsuuOCCCy641u/SiBfiBRfaIVxwwQUX8jxcaIdwoR3ChXYIF1xwwQUXXHDBBRdcL6RLIV6IF+IFF1xwwYU8DxdcqDfgQjuECy644EKeR7zgQv+CC+0Q8UK84IKrzq77iBdccMEFF1xwwQUXXHDBBRdccMEFF1xwwQUXXHDBBRdcbXNpxAsuuOCCCy644IILLrjgggsuuOCCCy644IILLrjgggsuuGrvShAvxAvxggvtEC644IILLrjgggsuuDD/QrzgggsuuOCCCy644IKr9i6JeCFeiBfihXihHcKFdggX2iHaIdohXHDBBRdccMEFF1xwwQUXXHDBBRdccMEFF1xwwQUXXHDBBRdccMEFF1xwwQUXXHAt57qPeMEFF1wvusu+0jLXn4kLR6NdZTodr549VDMD9J54ztE0VyrmO5rTDo1Y5GiC62fF4ketXR1zXyx5xPUU7XXFzY66xcuK2znq4zpNxS0edXD9+6tFLzXTVTzv9KKmuYolz/fl2rpKsY5jra5MrO1Yk+uPxZqP1bvEnRzxXXSkhw112edUQc1ziVTU44ib3Y1W6DoWdTxukg9PRX2PpeJlRe2PBV3/rL6Sl5ZziWYdz3eloonHNa4/EQ0+LnN9norGH3EL2tzVrn/4ULTteFngmP/o4inA8WI31P8PvLDpMAplbmRzdHJlYW0KZW5kb2JqCnhyZWYKMCA4CjAwMDAwMDAwMDAgNjU1MzUgZiAKMDAwMDAwMDAxNSAwMDAwMCBuIAowMDAwMDAwMTg0IDAwMDAwIG4gCjAwMDAwMDAyMzIgMDAwMDAgbiAKMDAwMDAwMDI4NyAwMDAwMCBuIAowMDAwMDAwNTI3IDAwMDAwIG4gCjAwMDAwMDA2ODEgMDAwMDAgbiAKMDAwMDAwMDcyNCAwMDAwMCBuIAp0cmFpbGVyCjw8Ci9JRFs8OUExOTNFNzRGQjhGMzM0ODkwNDIyNjNFN0NDNDdFQjM+PDlBMTkzRTc0RkI4RjMzNDg5MDQyMjYzRTdDQzQ3RUIzPl0KL0luZm8gMSAwIFIKL1Jvb3QgMiAwIFIKL1NpemUgOAo+PgpzdGFydHhyZWYKMTY0MjMKJSVFT0YKAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=
		");
		}

		public void AssertProcessAttachedDocumentWithWhiteSpaceInBase64(string base64String)
		{
			var setupFactory = new BusinessObjectFactory();
			var shipment = setupFactory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001286";
			shipment[JobShipmentSchema.JS_HouseBill] = "HOUSE";

			var message = EDIMessageTestFactory.New(setupFactory);

			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_Status = XmlEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText =
			#region Message Text
			$@"<UniversalEvent>
  <Event>
    <DataContext>
		<Company>
        <Code>EDI</Code>
        <Name>Eagle Datamation International</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00053321</Key>
        </DataSource>
      </DataSourceCollection>
      <DataProvider>CargoWise One</DataProvider>
      <EventType>
        <Code>DDI</Code>
        <Description>Document Imported</Description>
      </EventType>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2020-05-12T11:49:50.243</TriggerDate>
      <TriggerDescription>Send CIV to Receiving Agent</TriggerDescription>
      <TriggerReference>*CIV*</TriggerReference>
      <TriggerType>Trigger</TriggerType>
    </DataContext>
    <EventTime>2020-05-12T11:49:50.243</EventTime>
    <EventType>DDI</EventType>
    <CreatedTime>2020-05-12T04:25:56.32</CreatedTime>
    <EventReference>CIV</EventReference>
    <IsEstimate>false</IsEstimate>
    <AttachedDocumentCollection>
      <AttachedDocument>
        <FileName>hello.txt</FileName>
         <ImageData>{base64String}</ImageData>
         <Type>
          <Code>CIV</Code>
          <Description>Commercial Invoice</Description>
        </Type>
        <IsPublished>true</IsPublished>
        <SaveDateUTC>2020-05-12T03:49:00</SaveDateUTC>
      </AttachedDocument>
    </AttachedDocumentCollection>
    <ContextCollection>
	  <Context>
        <Type>HBOLNumber</Type>
        <Value>HOUSE</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";
			#endregion // Message Text

			setupFactory.Save();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			AssertEquals("eDoc is attached to shipment", 1, ((IDocManagerSupport)shipment).DocManagerInfo.Files.Count);

			var newFactory = new BusinessObjectFactory();

			message = newFactory.Load<EDIMessage>(message.PK);
			AssertEquals(EDIMessage.Status.ProcessedOK, message.EM_Status);
			AssertContains("Message Log Note", "Successfully Added eDoc:".Trim(), message.GetLogNoteText());
			AssertEquals("Should not have invalid Base64 log", false, message.GetLogNoteText().Contains("Invalid Base64 encoded binary data"));
		}

		[ExpectNoExceptions]
		public void TestProcessAttachedDocumentWithInvalidBase64()
		{
			var setupFactory = new BusinessObjectFactory();
			var shipment = setupFactory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001286";
			shipment[JobShipmentSchema.JS_HouseBill] = "HOUSE";

			var message = EDIMessageTestFactory.New(setupFactory);

			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_Status = XmlEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText =
			#region Message Text
			$@"<UniversalEvent>
  <Event>
    <DataContext>
		<Company>
        <Code>EDI</Code>
        <Name>Eagle Datamation International</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00053321</Key>
        </DataSource>
      </DataSourceCollection>
      <DataProvider>CargoWise One</DataProvider>
      <EventType>
        <Code>DDI</Code>
        <Description>Document Imported</Description>
      </EventType>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2020-05-12T11:49:50.243</TriggerDate>
      <TriggerDescription>Send CIV to Receiving Agent</TriggerDescription>
      <TriggerReference>*CIV*</TriggerReference>
      <TriggerType>Trigger</TriggerType>
    </DataContext>
    <EventTime>2020-05-12T11:49:50.243</EventTime>
    <EventType>DDI</EventType>
    <CreatedTime>2020-05-12T04:25:56.32</CreatedTime>
    <EventReference>CIV</EventReference>
    <IsEstimate>false</IsEstimate>
    <AttachedDocumentCollection>
      <AttachedDocument>
        <FileName>hello.txt</FileName>
        <ImageData>XXX</ImageData>
		<Type>
          <Code>CIV</Code>
          <Description>Commercial Invoice</Description>
        </Type>
        <IsPublished>true</IsPublished>
        <SaveDateUTC>2020-05-12T03:49:00</SaveDateUTC>
      </AttachedDocument>
    </AttachedDocumentCollection>
    <ContextCollection>
	  <Context>
        <Type>HBOLNumber</Type>
        <Value>HOUSE</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";
			#endregion // Message Text

			setupFactory.Save();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var newFactory = new BusinessObjectFactory();

			message = newFactory.Load<EDIMessage>(message.PK);
			AssertEquals(EDIMessage.Status.Warning, message.EM_Status);
			AssertContains("Message Log Note", "<Event>.<AttachedDocumentCollection>.<AttachedDocument>.<ImageData> - Invalid Base64 encoded binary data".Trim(), message.GetLogNoteText());
		}

		#endregion

		public void TestInboundUniversalEventWithVerboseLoggingOn()
		{
			eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var consol = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			consol[JobConsolSchema.JK_UniqueConsignRef] = "C00001286";
			consol[JobConsolSchema.JK_TransportMode] = "SEA";
			consol[JobConsolSchema.JK_MasterBillNum] = "FILLET-O-FISH";
			consol[JobConsolSchema.JK_BookingReference] = "";
			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(ConsolUniversalEventWithFieldUpdates);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Consol C00001286 (Master Bill='FILLET-O-FISH').
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Found 1 match using Context values.
Linked Event to Consol C00001286 (Master Bill='FILLET-O-FISH').
Field [JobConsol.JK_BookingReference] has been updated to value [SIC-EM-REX] on Consol C00001286 (Master Bill='FILLET-O-FISH').
Warning - Field [JobConsol.JK_NonExistantField] could not be found to update with value [Should give a Warning] on Consol C00001286 (Master Bill='FILLET-O-FISH').
".Trim(), message.GetLogNoteText());

				var logs = consol.GetLogs();
				var athLogs = logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "ATH"));
				AssertEquals("athLogs.Length", 1, athLogs.Length);
				var athLog = athLogs[0];
				AssertEquals("athLog.SL_EventTime", new ZDateTime(2011, 2, 15, 12, 3, 4), athLog.SL_EventTime);

				AssertEquals("consol.JK_BookingReference", "SIC-EM-REX", consol[JobConsolSchema.JK_BookingReference]);
			});
		}

		public void TestInboundUniversalEvent()
		{
			var consol = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			consol[JobConsolSchema.JK_UniqueConsignRef] = "C00001286";
			consol[JobConsolSchema.JK_TransportMode] = "SEA";
			consol[JobConsolSchema.JK_MasterBillNum] = "FILLET-O-FISH";
			consol[JobConsolSchema.JK_BookingReference] = "";
			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(ConsolUniversalEventWithFieldUpdates);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Consol C00001286 (Master Bill='FILLET-O-FISH').
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Consol C00001286 (Master Bill='FILLET-O-FISH').
Field [JobConsol.JK_BookingReference] has been updated to value [SIC-EM-REX] on Consol C00001286 (Master Bill='FILLET-O-FISH').
Warning - Field [JobConsol.JK_NonExistantField] could not be found to update with value [Should give a Warning] on Consol C00001286 (Master Bill='FILLET-O-FISH').
".Trim(), message.GetLogNoteText());

				var logs = consol.GetLogs();
				var athLogs = logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "ATH"));
				AssertEquals("athLogs.Length", 1, athLogs.Length);
				var athLog = athLogs[0];
				AssertEquals("athLog.SL_EventTime", new ZDateTime(2011, 2, 15, 12, 3, 4), athLog.SL_EventTime);

				AssertEquals("consol.JK_BookingReference", "SIC-EM-REX", consol[JobConsolSchema.JK_BookingReference]);
			});
		}

		#region const string ConsolUniversalEventWithFieldUpdates

		const string ConsolUniversalEventWithFieldUpdates = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
	<EventTime>2011-02-15T12:03:04</EventTime>
	<EventType>ATH</EventType>
	<DataContext>
		<DataProvider>CargoWise One</DataProvider>
	</DataContext>

	<ContextCollection>
		<Context>
		<Type>MBOLNumber</Type>
		<Value>FILLET-O-FISH</Value>
		</Context>
	</ContextCollection>

	<AdditionalFieldsToUpdateCollection>
		<AdditionalFieldsToUpdate>
		<Type>JobConsol.JK_BookingReference</Type>
		<Value>SIC-EM-REX</Value>
		</AdditionalFieldsToUpdate>
		<AdditionalFieldsToUpdate>
		<Type>JobShipment.JS_SomethingElse</Type>
		<Value>Should Not Give Warning</Value>
		</AdditionalFieldsToUpdate>
		<AdditionalFieldsToUpdate>
		<Type>JobConsol.JK_NonExistantField</Type>
		<Value>Should give a Warning</Value>
		</AdditionalFieldsToUpdate>
	</AdditionalFieldsToUpdateCollection>
</Event>
</UniversalEvent>";

		#endregion

		public void TestInboundUniversalEvent_InvalidContextValue()
		{
			var consol = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			consol[JobConsolSchema.JK_UniqueConsignRef] = "C00001286";
			consol[JobConsolSchema.JK_TransportMode] = "SEA";
			consol[JobConsolSchema.JK_MasterBillNum] = "CRACKERJACK";
			consol[JobConsolSchema.JK_BookingReference] = "";
			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(ConsolUniversalEventWithInvalidContextValue);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Consol C00001286 (Master Bill='CRACKERJACK').
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Warning - Context Collection Value from Key [FlightDate] - Invalid value [blah]. Value must be a valid date between 2-Jan-1900 and 6-Jun-2079.
Warning - Context Collection Value from Key [MAWBNumberOfPieces] - Invalid value [1T14]. Value must be a valid Integer.
Linked Event to Consol C00001286 (Master Bill='CRACKERJACK').
Field [JobConsol.JK_BookingReference] has been updated to value [GREAT_SUCCESS] on Consol C00001286 (Master Bill='CRACKERJACK').
Warning - Field [JobConsol.JK_ConsolCutOffDateLocal] could not be updated to value [I LIKE BIG DATES] on Consol C00001286 (Master Bill='CRACKERJACK') - Invalid value [I LIKE BIG DATES]. Value must be a valid date between 2-Jan-1900 and 6-Jun-2079.
Warning - Field [JobConsol.JK_TotalShipmentChargableCheck] could not be updated to value [EXPLOSION!4] on Consol C00001286 (Master Bill='CRACKERJACK') - Invalid value [EXPLOSION!4]. Value must be a valid Decimal.
Warning - Field [JobConsol.JK_NonExistantField] could not be found to update with value [Should give a Warning] on Consol C00001286 (Master Bill='CRACKERJACK').
".Trim(), message.GetLogNoteText());

				var logs = consol.GetLogs();
				var athLogs = logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "ATH"));
				AssertEquals("athLogs.Length", 1, athLogs.Length);
				var athLog = athLogs[0];
				AssertEquals("athLog.SL_EventTime", new ZDateTime(2011, 2, 15, 12, 3, 4), athLog.SL_EventTime);

				AssertEquals("consol.JK_BookingReference", "GREAT_SUCCESS", consol[JobConsolSchema.JK_BookingReference]);
				Assert("consol.JK_ConsolCutOffDate", !((ZDateTime)consol[JobConsolSchema.JK_ConsolCutOffDate]).IsValid);
				AssertEquals("consol.JK_TotalShipmentChargableCheck", 0m, (ZDecimal)consol[JobConsolSchema.JK_TotalShipmentChargableCheck]);
			});
		}

		#region Help TestInboundUniversalEvent_InvalidContextValue

		const string ConsolUniversalEventWithInvalidContextValue = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
	<EventTime>2011-02-15T12:03:04</EventTime>
	<EventType>ATH</EventType>
	<DataContext>
		<DataProvider>CargoWise One</DataProvider>
	</DataContext>

	<ContextCollection>
		<Context>
		<Type>MBOLNumber</Type>
		<Value>CRACKERJACK</Value>
		</Context>
		<Context>
		<Type>FlightDate</Type>
		<Value>blah</Value>
		</Context>
		<Context>
		<Type>MAWBNumberOfPieces</Type>
		<Value>1T14</Value>
		</Context>
		<Context>
		<Type>InvalidType</Type>
		<Value>aaa</Value>
		</Context>
	</ContextCollection>

	<AdditionalFieldsToUpdateCollection>
		<AdditionalFieldsToUpdate>
		<Type>JobConsol.JK_BookingReference</Type>
		<Value>GREAT_SUCCESS</Value>
		</AdditionalFieldsToUpdate>
		<AdditionalFieldsToUpdate>
		<Type>JobConsol.JK_ConsolCutOffDateLocal</Type>
		<Value>I LIKE BIG DATES</Value>
		</AdditionalFieldsToUpdate>
		<AdditionalFieldsToUpdate>
		<Type>JobConsol.JK_TotalShipmentChargableCheck</Type>
		<Value>EXPLOSION!4</Value>
		</AdditionalFieldsToUpdate>
		<AdditionalFieldsToUpdate>
		<Type>JobConsol.JK_NonExistantField</Type>
		<Value>Should give a Warning</Value>
		</AdditionalFieldsToUpdate>
	</AdditionalFieldsToUpdateCollection>
</Event>
</UniversalEvent>";

		#endregion

		public void TestUniversalEventIsAddedOnce()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001286";
			shipment[JobShipmentSchema.JS_TransportMode] = "SEA";
			shipment[JobShipmentSchema.JS_HouseBill] = "HB23010";
			shipment[JobShipmentSchema.JS_BookingReference] = "";

			var declaration = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			declaration[JobDeclarationSchema.JE_DeclarationReference] = "S00001286";
			declaration[JobDeclarationSchema.JE_TransportMode] = "SEA";
			declaration[JobDeclarationSchema.JE_HouseBill] = "HB23010";
			declaration[JobDeclarationSchema.JE_OwnerRef] = "";
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;
			Factory.SaveForTesting();

			const string shipmentUniversalEventWithFieldUpdates = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>2011-02-15T12:03:04</EventTime>
		<EventReference>Dummy Reference</EventReference>
		<DataContext>
			<DataProvider>CargoWise One</DataProvider>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>HB23010</Value>
			</Context>
		</ContextCollection>

		<AdditionalFieldsToUpdateCollection>
			<AdditionalFieldsToUpdate>
				<Type>JobShipment.JS_BookingReference</Type>
				<Value>BOOK123</Value>
			</AdditionalFieldsToUpdate>
			<AdditionalFieldsToUpdate>
				<Type>JobDeclaration.JE_OwnerRef</Type>
				<Value>OWN2343</Value>
			</AdditionalFieldsToUpdate>
		</AdditionalFieldsToUpdateCollection>
	</Event>
</UniversalEvent>
";
			var message = GetQueuedUniversalEventMessage(shipmentUniversalEventWithFieldUpdates);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Shipment S00001286 (House Bill='HB23010').
".Trim(), serviceTaskLog.ToString());

				AssertContainsExactLinesInAnyOrder("Message Log Note", @"
Linked Event to Shipment S00001286 (House Bill='HB23010').
Field [JobDeclaration.JE_OwnerRef] has been updated to value [OWN2343] on Declaration S00001286.
Field [JobShipment.JS_BookingReference] has been updated to value [BOOK123] on Shipment S00001286 (House Bill='HB23010').
".Trim(), message.GetLogNoteText());

				var ccdLogs = shipment.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "CCD"));
				AssertEquals("ccdLogs.Length", 1, ccdLogs.Length);
				var ccdLog = ccdLogs[0];
				AssertEquals("ccdLog.SL_EventTime", new ZDateTime(2011, 2, 15, 12, 3, 4), ccdLog.SL_EventTime);
				AssertEquals("ccdLog.SL_Reference", "Dummy Reference", ccdLog.SL_Reference);

				ccdLogs = declaration.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "CCD"));
				AssertEquals("CCD event should be added to shipment when the declaration is linked to a shipment", 0, ccdLogs.Length);

				AssertEquals("shipment.JS_BookingReference", "BOOK123", shipment[JobShipmentSchema.JS_BookingReference]);
				AssertEquals("declaration.JE_OwnerRef", "OWN2343", declaration[JobDeclarationSchema.JE_OwnerRef]);
			});
		}

		public void TestUniversalEventAdded_WithFailureOnFindingFieldToUpdate()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001286";
			shipment[JobShipmentSchema.JS_TransportMode] = "SEA";
			shipment[JobShipmentSchema.JS_HouseBill] = "HB23010";
			shipment[JobShipmentSchema.JS_BookingReference] = "";

			var declaration = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			declaration[JobDeclarationSchema.JE_DeclarationReference] = "S00001286";
			declaration[JobDeclarationSchema.JE_TransportMode] = "SEA";
			declaration[JobDeclarationSchema.JE_HouseBill] = "HB23010";
			declaration[JobDeclarationSchema.JE_OwnerRef] = "";
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;
			Factory.SaveForTesting();

			const string shipmentUniversalEventWithFieldUpdates = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>2011-02-15T12:03:04</EventTime>
		<EventReference>Dummy Reference</EventReference>
		<DataContext>
			<DataProvider>CargoWise One</DataProvider>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>HB23010</Value>
			</Context>
		</ContextCollection>

		<AdditionalFieldsToUpdateCollection>
			<AdditionalFieldsToUpdate>
				<Type>JobShipment.JS_BookingReference</Type>
				<Value>BOOK123</Value>
			</AdditionalFieldsToUpdate>
			<AdditionalFieldsToUpdate>
				<Type>JobShipment.ArrivalConsol.JK_AgentsReference</Type>
				<Value>AREF456</Value>
			</AdditionalFieldsToUpdate>
			<AdditionalFieldsToUpdate>
				<Type>JobDeclaration.JE_OwnerRef.ShouldFail</Type>
				<Value>OWN2343</Value>
			</AdditionalFieldsToUpdate>
		</AdditionalFieldsToUpdateCollection>
	</Event>
</UniversalEvent>
";
			var message = GetQueuedUniversalEventMessage(shipmentUniversalEventWithFieldUpdates);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Shipment S00001286 (House Bill='HB23010').
".Trim(), serviceTaskLog.ToString());

				AssertContainsExactLinesInAnyOrder("Message Log Note", @"
Linked Event to Shipment S00001286 (House Bill='HB23010').
Warning - Entity in property [JobDeclaration.JE_OwnerRef] does not have any child fields to update on Declaration S00001286.
Field [JobShipment.JS_BookingReference] has been updated to value [BOOK123] on Shipment S00001286 (House Bill='HB23010').
Warning - No entity present to update in property [JobShipment.ArrivalConsol] on Shipment S00001286 (House Bill='HB23010').
".Trim(), message.GetLogNoteText());

				var ccdLogs = shipment.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "CCD"));
				AssertEquals("ccdLogs.Length", 1, ccdLogs.Length);
				var ccdLog = ccdLogs[0];
				AssertEquals("ccdLog.SL_EventTime", new ZDateTime(2011, 2, 15, 12, 3, 4), ccdLog.SL_EventTime);
				AssertEquals("ccdLog.SL_Reference", "Dummy Reference", ccdLog.SL_Reference);

				ccdLogs = declaration.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "CCD"));
				AssertEquals("CCD event should be added to shipment when the declaration is linked to a shipment", 0, ccdLogs.Length);

				AssertEquals("shipment.JS_BookingReference", "BOOK123", shipment[JobShipmentSchema.JS_BookingReference]);
				AssertEquals("declaration.JE_OwnerRef", string.Empty, declaration[JobDeclarationSchema.JE_OwnerRef]);
			});
		}

		public void TestUniversalEventIsAddedOnceOnSingleTransportLegForConsolByUsingDefaultMatching()
		{
			var consol = Factory.BOFactory.New<Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = "08112347893";

			var leg0 = consol.Transports_Get(0);
			var leg1 = consol.Transports_AddNew();
			var leg2 = consol.Transports_AddNew();
			leg0.JW_RL_NKLoadPort = "AUSYD";
			leg0.JW_RL_NKDiscPort = "CNNKG";
			leg0.JW_VoyageFlight = "6852S";
			leg0.JW_ETD = new ZDateTime(2012, 03, 22);
			leg1.JW_RL_NKLoadPort = "CNNKG";
			leg1.JW_RL_NKDiscPort = "CNFOC";
			leg2.JW_RL_NKLoadPort = "CNFOC";
			leg2.JW_RL_NKDiscPort = "CNSHP";

			Factory.SaveForTesting();

			#region consolUniversalEventWithFieldUpdates

			const string consolUniversalEventWithFieldUpdates = @"<UniversalEvent>
	<Event>
	<EventType>DEP</EventType>
	<EventTime>2012-04-12T14:11:59</EventTime>
	<ContextCollection>
		<Context>
		<Type>MAWBNumber</Type>
		<Value>081-12347893</Value>
		</Context>
		<Context>
		<Type>MAWBNumberOfPieces</Type>
		<Value>1</Value>
		</Context>
		<Context>
		<Type>SourceEventCode</Type>
		<Value>DEP</Value>
		</Context>
		<Context>
		<Type>NumberOfPieces</Type>
		<Value>1</Value>
		</Context>
		<Context>
		<Type>WeightOfGoods</Type>
		<Value>109KG</Value>
		</Context>
		<Context>
		<Type>IATACarrierCode</Type>
		<Value>LH</Value>
		</Context>
		<Context>
		<Type>FlightNumber</Type>
		<Value>6852S</Value>
		</Context>
		<Context>
		<Type>FlightDate</Type>
		<Value>2012-03-22</Value>
		</Context>
		<Context>
		<Type>OriginIATAAirportCode</Type>
		<Value>NKG</Value>
		</Context>
		<Context>
		<Type>DestinationIATAAirportCode</Type>
		<Value>SHP</Value>
		</Context>
		<Context>
		<Type>LegOriginUNLOCO</Type>
		<Value>AUSYD</Value>
		</Context>
		<Context>
		<Type>LegDestinationUNLOCO</Type>
		<Value>CNSHP</Value>
		</Context>
		<Context>
		<Type>TimeOfDeparture</Type>
		<Value>A 22-Mar-2012 22:00</Value>
		</Context>
	</ContextCollection>
	<AdditionalFieldsToUpdateCollection>
		<AdditionalFieldsToUpdate>
		<Type>JobConsolTransport.JW_ATD</Type>
		<Value>2012-04-12T14:11:59.8403643</Value>
		</AdditionalFieldsToUpdate>
	</AdditionalFieldsToUpdateCollection>
	</Event>
</UniversalEvent>";

			#endregion

			var message = GetQueuedUniversalEventMessage(consolUniversalEventWithFieldUpdates);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var consolBusinessObject = consol as BusinessObject;
			var leg0BusinessObject = leg0 as BusinessObject;
			var leg1BusinessObject = leg1 as BusinessObject;
			var leg2BusinessObject = leg2 as BusinessObject;

			var departureEventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DepartureCode);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Queued, message.EM_Status);

				var depLogsforconsol = consolBusinessObject.GetLogs().Find(departureEventFilter);
				var depLogsforleg1 = leg0BusinessObject.GetLogs().Find(departureEventFilter);
				var depLogsforleg2 = leg1BusinessObject.GetLogs().Find(departureEventFilter);
				var depLogsforleg3 = leg2BusinessObject.GetLogs().Find(departureEventFilter);
				AssertEquals("PRE: Leg1_DEP_log", 1, depLogsforleg1.Length);
				AssertEquals("PRE: Leg2_DEP_log", 0, depLogsforleg2.Length);
				AssertEquals("PRE: Leg3_DEP_log", 0, depLogsforleg3.Length);
				AssertEquals("PRE: Consol_DEP_log", 1, depLogsforconsol.Length);
				AssertNotEquals("PRE: leg1.JW_ATD", new ZDateTime(2012, 4, 12, 14, 11, 59).ToString(), leg0BusinessObject[JobConsolTransportSchema.JW_ATD].ToString());
				AssertNotEquals("PRE: leg2.JW_ATD", new ZDateTime(2012, 4, 12, 14, 11, 59).ToString(), leg1BusinessObject[JobConsolTransportSchema.JW_ATD].ToString());
				AssertNotEquals("PRE: leg3.JW_ATD", new ZDateTime(2012, 4, 12, 14, 11, 59).ToString(), leg2BusinessObject[JobConsolTransportSchema.JW_ATD].ToString());
			});
			manager.Process(message);

			CombineAssertions(delegate
			{
				Factory.SaveForTesting();
				AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);

				var depLogsforleg0 = leg0BusinessObject.GetLogs().Find(departureEventFilter);
				var depLogsforleg1 = leg1BusinessObject.GetLogs().Find(departureEventFilter);
				var depLogsforleg2 = leg2BusinessObject.GetLogs().Find(departureEventFilter);
				var depLogsforconsol = consolBusinessObject.GetLogs().Find(departureEventFilter);

				AssertEquals("Leg0_DEP_log", 2, depLogsforleg0.Length);
				AssertEquals("Leg1_DEP_log", 0, depLogsforleg1.Length);
				AssertEquals("Leg2_DEP_log", 0, depLogsforleg2.Length);
				AssertEquals("Consol_DEP_log", 2, depLogsforconsol.Length);

				var depLog = depLogsforleg0[1];
				AssertEquals("depLog.SL_EventTime", new ZDateTime(2012, 4, 12, 14, 11, 59), depLog.SL_EventTime);
				AssertEquals("DEP Event should not be Estimate", false, depLog.SL_IsEstimate);
				AssertEquals("DEP Event should not be Cancelled", false, depLog.IsCancelled);
				AssertNotNull("DEP Event has no edimessage to link to", depLog.RelatedEDIMessage);
				if (depLog.RelatedEDIMessage != null)
				{
					AssertEquals("DEP Event should be linked to edimessage that triggered the evenet", message, depLog.RelatedEDIMessage.Message);
				}

				AssertEquals("leg0.JW_ATD", new ZDateTime(2012, 4, 12, 14, 11, 59).ToString(), leg0BusinessObject[JobConsolTransportSchema.JW_ATD].ToString());
				AssertNotEquals("leg1.JW_ATD", new ZDateTime(2012, 4, 12, 14, 11, 59).ToString(), leg1BusinessObject[JobConsolTransportSchema.JW_ATD].ToString());
				AssertNotEquals("leg2.JW_ATD", new ZDateTime(2012, 4, 12, 14, 11, 59).ToString(), leg2BusinessObject[JobConsolTransportSchema.JW_ATD].ToString());
			});
		}

		[TestDate(2011, 8, 1)]
		public void TestUniversalEventUseMessageCompanyIfNeeded()
		{
			var usCompany = (BusinessObject)Factory.BOFactory.New<MasterFiles.Integration.IGlbCompany>();
			usCompany[GlbCompanySchema.GC_Code] = "ZUS";
			usCompany[GlbCompanySchema.GC_OH_OrgProxy] = Env.CurrentCompany.OrganisationPK;
			usCompany[GlbCompanySchema.GC_RN_NKCountryCode] = "US";
			var usBranch = (BusinessObject)Factory.BOFactory.New<MasterFiles.Integration.IGlbBranch>();
			usBranch[GlbBranchSchema.GB_GC] = usCompany.PK;
			usBranch[GlbBranchSchema.GB_Code] = "ZBS";
			usBranch[GlbBranchSchema.GB_OH_OrgProxy] = Env.CurrentBranch.OrganisationPK;

			var localDec = Factory.BOFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			localDec.JE_GB = Env.CurrentBranch.PK;
			localDec.JE_DeclarationReference = "B00000123";
			localDec.JE_MasterBill = "MASTERB1";
			localDec.JE_HouseBill = "HB1";
			var localDecBO = (BusinessObject)localDec;

			var usDec = Factory.BOFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			usDec.JE_GB = usBranch.PK;
			usDec.JE_DeclarationReference = "B00000123";
			usDec.JE_MasterBill = "MASTERB1";
			usDec.JE_HouseBill = "HB1";
			var usDecBO = (BusinessObject)usDec;

			Factory.SaveForTesting();

			const string universalEvent1 = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CustomsDeclaration</Type>
					<Key>B00000123</Key>
				</DataTarget>
			</DataTargetCollection>
			<CodesMappedToTarget>true</CodesMappedToTarget>
			<DataProvider>Dummy</DataProvider>
		</DataContext>
		<EventType>CCD</EventType>
		<EventTime>2011-02-15T12:03:04</EventTime>
		<EventReference>Dummy Reference</EventReference>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>MASTERB1</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>HB1</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var usMessage = GetQueuedUniversalEventMessage(universalEvent1);
			usMessage.EM_GB = usBranch.PK;

			var serviceTaskLog = new ServiceTaskLogForTesting();
			new UniversalMessageProcessingManager(serviceTaskLog).Process(usMessage);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, usMessage.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Declaration B00000123.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Declaration B00000123.
".Trim(), usMessage.GetLogNoteText());

				var ccdLogs = usDecBO.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "CCD"));
				AssertEquals("ccdLogs.Length", 1, ccdLogs.Length);
				var ccdLog = ccdLogs[0];
				AssertEquals("ccdLog.SL_EventTime", new ZDateTime(2011, 2, 15, 12, 3, 4), ccdLog.SL_EventTime);
				AssertEquals("ccdLog.SL_Reference", "Dummy Reference", ccdLog.SL_Reference);

				ccdLogs = localDecBO.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "CCD"));
				AssertEquals("CCD event should not be added to local declaration as the branch is different", 0, ccdLogs.Length);
			});

			const string universalEvent2 = @"
<UniversalEvent>
	<Event>
		<EventType>ATH</EventType>
		<EventTime>2011-02-16T12:03:04</EventTime>
		<EventReference>Dummy Reference 2</EventReference>
		<DataContext>
			<DataProvider>Dummy</DataProvider>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>MASTERB1</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>HB1</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var localMessage1 = GetQueuedUniversalEventMessage(universalEvent2);
			localMessage1.EM_GB = Env.CurrentBranch.PK;
			ClearDataImportLogs(usDecBO);
			ClearDataImportLogs((BusinessObject)usMessage);
			ClearLogPivot(usMessage.PK);
			serviceTaskLog.ClearLogs();
			Factory.SaveForTesting();
			new UniversalMessageProcessingManager(serviceTaskLog).Process(localMessage1);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, localMessage1.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Declaration B00000123.
Linked Event to Declaration B00000123.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Declaration B00000123.
Linked Event to Declaration B00000123.
".Trim(), localMessage1.GetLogNoteText());

				var athLogs = localDecBO.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "ATH"));
				AssertEquals("athLogs.Length", 1, athLogs.Length);
				var athLog = athLogs[0];
				AssertEquals("athLog.SL_EventTime", new ZDateTime(2011, 2, 16, 12, 3, 4), athLog.SL_EventTime);
				AssertEquals("athLog.SL_Reference", "Dummy Reference 2", athLog.SL_Reference);

				athLogs = usDecBO.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "ATH"));
				AssertEquals("athLogs.Length", 1, athLogs.Length);
				athLog = athLogs[0];
				AssertEquals("athLog.SL_EventTime", new ZDateTime(2011, 2, 16, 12, 3, 4), athLog.SL_EventTime);
				AssertEquals("athLog.SL_Reference", "Dummy Reference 2", athLog.SL_Reference);
			});
		}

		public void TestUniversalEventFromJobDeclarationHaveCarrierCode()
		{
			var usCompany = (BusinessObject)Factory.BOFactory.New<MasterFiles.Integration.IGlbCompany>();
			usCompany[GlbCompanySchema.GC_Code] = "ZUS";
			usCompany[GlbCompanySchema.GC_OH_OrgProxy] = Env.CurrentCompany.OrganisationPK;
			usCompany[GlbCompanySchema.GC_RN_NKCountryCode] = "AU";
			var usBranch = (BusinessObject)Factory.BOFactory.New<MasterFiles.Integration.IGlbBranch>();
			usBranch[GlbBranchSchema.GB_GC] = usCompany.PK;
			usBranch[GlbBranchSchema.GB_Code] = "ZBS";
			usBranch[GlbBranchSchema.GB_OH_OrgProxy] = Env.CurrentBranch.OrganisationPK;

			var localDec = Factory.BOFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			localDec.JE_GB = Env.CurrentBranch.PK;
			localDec.JE_DeclarationReference = "B00000123";
			localDec.JE_MasterBill = "MASTERB1";
			localDec.JE_HouseBill = "HB1";
			var localDecBO = (BusinessObject)localDec;

			var usDec = Factory.BOFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			usDec.JE_GB = usBranch.PK;
			usDec.JE_DeclarationReference = "B00000123";
			usDec.JE_MasterBill = "MASTERB1";
			usDec.JE_HouseBill = "HB1";
			var usDecBO = (BusinessObject)usDec;

			Factory.SaveForTesting();

			const string universalEvent1 = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CustomsDeclaration</Type>
					<Key>B00000123</Key>
				</DataTarget>
			</DataTargetCollection>
			<CodesMappedToTarget>true</CodesMappedToTarget>
			<DataProvider>Dummy</DataProvider>
		</DataContext>
		<EventType>CCD</EventType>
		<EventTime>2011-02-15T12:03:04</EventTime>
		<EventReference>Dummy Reference</EventReference>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>MASTERB1</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>HB1</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var usMessage = GetQueuedUniversalEventMessage(universalEvent1);
			usMessage.EM_GB = usBranch.PK;

			var serviceTaskLog = new ServiceTaskLogForTesting();
			new UniversalMessageProcessingManager(serviceTaskLog).Process(usMessage);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, usMessage.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Declaration B00000123.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Declaration B00000123.
".Trim(), usMessage.GetLogNoteText());

				var ccdLogs = usDecBO.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "CCD"));
				AssertEquals("ccdLogs.Length", 1, ccdLogs.Length);
				var ccdLog = ccdLogs[0];
				AssertEquals("ccdLog.SL_EventTime", new ZDateTime(2011, 2, 15, 12, 3, 4), ccdLog.SL_EventTime);
				AssertEquals("ccdLog.SL_Reference", "Dummy Reference", ccdLog.SL_Reference);

				ccdLogs = localDecBO.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "CCD"));
				AssertEquals("CCD event should not be added to local declaration as the branch is different", 0, ccdLogs.Length);
			});

			const string universalEvent2 = @"
<UniversalEvent>
	<Event>
		<EventType>ATH</EventType>
		<EventTime>2011-02-16T12:03:04</EventTime>
		<EventReference>Dummy Reference 2</EventReference>
		<DataContext>
			<DataProvider>Dummy</DataProvider>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>MASTERB1</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>HB1</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var localMessage1 = GetQueuedUniversalEventMessage(universalEvent2);
			localMessage1.EM_GB = Env.CurrentBranch.PK;
			ClearDataImportLogs(usDecBO);
			ClearDataImportLogs((BusinessObject)usMessage);
			ClearLogPivot(usMessage.PK);
			serviceTaskLog.ClearLogs();
			Factory.SaveForTesting();
			new UniversalMessageProcessingManager(serviceTaskLog).Process(localMessage1);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, localMessage1.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Declaration B00000123.
Linked Event to Declaration B00000123.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Declaration B00000123.
Linked Event to Declaration B00000123.
".Trim(), localMessage1.GetLogNoteText());

				var athLogs = localDecBO.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "ATH"));
				AssertEquals("athLogs.Length", 1, athLogs.Length);
				var athLog = athLogs[0];
				AssertEquals("athLog.SL_EventTime", new ZDateTime(2011, 2, 16, 12, 3, 4), athLog.SL_EventTime);
				AssertEquals("athLog.SL_Reference", "Dummy Reference 2", athLog.SL_Reference);

				athLogs = usDecBO.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "ATH"));
				AssertEquals("athLogs.Length", 1, athLogs.Length);
				athLog = athLogs[0];
				AssertEquals("athLog.SL_EventTime", new ZDateTime(2011, 2, 16, 12, 3, 4), athLog.SL_EventTime);
				AssertEquals("athLog.SL_Reference", "Dummy Reference 2", athLog.SL_Reference);
			});
		}

		public void TestUniversalEventWillNotImportSameSenderReceiverDataTargetSource()
		{
			eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			const string stringMessage = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataSourceCollection>
				<DataSource>
					<Type>CustomsDeclaration</Type>
					<Key>B00000123</Key>
				</DataSource>
			</DataSourceCollection>
			<DataTargetCollection>
				<DataTarget>
					<Type>CustomsDeclaration</Type>
					<Key>B00000123</Key>
				</DataTarget>
			</DataTargetCollection>
			<CodesMappedToTarget>true</CodesMappedToTarget>
			<DataProvider>Dummy</DataProvider>
		</DataContext>
		<EventType>CCD</EventType>
		<EventTime>2011-02-15T12:03:04</EventTime>
		<EventReference>Dummy Reference</EventReference>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>MASTERB1</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>HB1</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var message = GetQueuedUniversalEventMessage(stringMessage);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
				AssertMultilineASCIIEquals("Message Log Note", @"
Import into CustomsDeclaration was skipped as it was the original source of this data.
Warning - No Module found a Business Entity to link this Universal Event to.
Message Discarded.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUniversalEventMatchByContextIsDiscardedWhenItsReceivedBackIntoTheSameSystem()
		{
			var declaration = Factory.BOFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_GB = Env.CurrentBranch.PK;
			declaration.JE_DeclarationReference = "B00000123";
			declaration.JE_MasterBill = "MASTERB1";
			declaration.JE_HouseBill = "HB1";
			var declarationBO = (BusinessObject)declaration;

			Factory.SaveForTesting();

			const string eventXML = @"
<UniversalEvent>
	<Event>
		<EventType>ATH</EventType>
		<EventTime>2013-04-16T12:03:04</EventTime>
		<EventReference>DRACULA</EventReference>
		<DataContext>
			<DataSourceCollection>
				<DataSource>
					<Type>CustomsDeclaration</Type>
				</DataSource>
			</DataSourceCollection>
			<CodesMappedToTarget>true</CodesMappedToTarget>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>MASTERB1</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>HB1</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var message = GetQueuedUniversalEventMessage(eventXML);
			message.EM_GB = Env.CurrentBranch.PK;

			Factory.SaveForTesting();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			new UniversalMessageProcessingManager(serviceTaskLog).Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Warning - No Module found a Business Entity to link this Universal Event to.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Warning - No Module found a Business Entity to link this Universal Event to.
Message Discarded.
".Trim(), message.GetLogNoteText());

				var athLogs = declarationBO.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "ATH"));
				AssertEquals("athLogs.Length", 0, athLogs.Length);
			});
		}

		public void TestDeclarationExport_AddCarrierCodeIntoUniversalEvent()
		{
			var declaration = Factory.BOFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_GB = Env.CurrentBranch.PK;
			declaration.JE_DeclarationReference = "B00000123";
			declaration.JE_MasterBill = "MASTERB1";
			declaration.JE_HouseBill = "HB1";
			var declarationBO = (BusinessObject)declaration;

			Factory.SaveForTesting();

			const string eventXML = @"
<UniversalEvent>
	<Event>
		<EventType>ATH</EventType>
		<EventTime>2013-04-16T12:03:04</EventTime>
		<EventReference>DRACULA</EventReference>
		<DataContext>
			<DataSourceCollection>
				<DataSource>
					<Type>CustomsDeclaration</Type>
				</DataSource>
			</DataSourceCollection>
			<CodesMappedToTarget>true</CodesMappedToTarget>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>MASTERB1</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>HB1</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var message = GetQueuedUniversalEventMessage(eventXML);
			message.EM_GB = Env.CurrentBranch.PK;

			Factory.SaveForTesting();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			new UniversalMessageProcessingManager(serviceTaskLog).Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Warning - No Module found a Business Entity to link this Universal Event to.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Warning - No Module found a Business Entity to link this Universal Event to.
Message Discarded.
".Trim(), message.GetLogNoteText());

				var athLogs = declarationBO.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "ATH"));
				AssertEquals("athLogs.Length", 0, athLogs.Length);
			});
		}

		[TestDate(2014, 8, 1)]
		public void TestUniversalEvent_WithEventParameters_1()
		{
			var declaration = Factory.BOFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_GB = Env.CurrentBranch.PK;
			declaration.JE_MasterBill = "MASTERB1";
			declaration.JE_HouseBill = "HB1";

			const string universalEvent = @"
<UniversalEvent>
	<Event>
		<EventType>ATH</EventType>
		<EventTime>2014-03-06T12:03:04</EventTime>
		<EventReference>txt|AAA=aaa</EventReference>
		<EventParameters>
			<Location>UAIEV</Location>
			<Facility>CTO</Facility>
			<Department>CTO</Department>
			<Reason>AAA</Reason>
			<Service>BBB</Service>
			<MessageType>X</MessageType>
			<MessageSubType>Z</MessageSubType>
			<Old>1</Old>
			<New>2</New>
		</EventParameters>
		<DataContext>
			<DataProvider>Dummy</DataProvider>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>MASTERB1</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>HB1</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var localMessage = GetQueuedUniversalEventMessage(universalEvent);
			localMessage.EM_GB = Env.CurrentBranch.PK;

			Factory.SaveForTesting();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			new UniversalMessageProcessingManager(serviceTaskLog).Process(localMessage);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, localMessage.EM_Status);

				var declarationBO = (BusinessObject)declaration;

				var athLogs = declarationBO.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "ATH"));
				AssertEquals("Events found", 1, athLogs.Length);

				var athLog = athLogs[0];
				AssertEquals("EventTime", new ZDateTimeOffset(2014, 3, 6, 12, 3, 4), athLog.SL_EventTime.ToOffset());
				AssertEquals("RefereneceFreeText", "txt", athLog.ReferenceFreeText);
				AssertEquals("Unsupported parameter", "aaa", athLog.Parameters["AAA"]);
				AssertEquals("Location", "UAIEV", athLog.Parameters[Constants.EventReferenceParameters.Codes.Location]);
				AssertEquals("Facility", "CTO", athLog.Parameters[Constants.EventReferenceParameters.Codes.Facility]);
				AssertEquals("Department", "CTO", athLog.Parameters[Constants.EventReferenceParameters.Codes.Department]);
				AssertEquals("Reason", "AAA", athLog.Parameters[Constants.EventReferenceParameters.Codes.Reason]);
				AssertEquals("Service", "BBB", athLog.Parameters[Constants.EventReferenceParameters.Codes.Service]);
				AssertEquals("MessageType", "X", athLog.Parameters[Constants.EventReferenceParameters.Codes.MessageType]);
				AssertEquals("MessageSubType", "Z", athLog.Parameters[Constants.EventReferenceParameters.Codes.MessageSubType]);
				AssertEquals("Old", "1", athLog.Parameters[Constants.EventReferenceParameters.Codes.Old]);
				AssertEquals("New", "2", athLog.Parameters[Constants.EventReferenceParameters.Codes.New]);
			});
			AssertContains("The following parameter codes are not valid: AAA.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear(); //Reason: Testing of unspported parameters is a part of this unit test
		}

		[TestDate(2014, 8, 1)]
		public void TestUniversalEvent_WithEventParameters_2()
		{
			var declaration = Factory.BOFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_GB = Env.CurrentBranch.PK;
			declaration.JE_MasterBill = "MASTERB1";
			declaration.JE_HouseBill = "HB1";

			const string universalEvent = @"
<UniversalEvent>
	<Event>
		<EventType>ATH</EventType>
		<EventTime>2014-03-06T12:03:04</EventTime>
		<EventReference>txt|AAA=aaa</EventReference>
		<EventParameters>
			<Type>CCC</Type>
			<Name>Peter</Name>
			<Partial>5</Partial>
			<Total>12</Total>
			<VoyageFlightNumber>QF1</VoyageFlightNumber>
			<FlightDate>2014-12-21</FlightDate>
			<ExternalDocumentType>OTH</ExternalDocumentType>
			<ReceiptNumber>29F799DF-FE5C-4150-9A57-40995FC83590</ReceiptNumber>
			<ReferenceNumber>80312346</ReferenceNumber>
			<RequestNumber>11547</RequestNumber>
		</EventParameters>
		<DataContext>
			<DataProvider>Dummy</DataProvider>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>MASTERB1</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>HB1</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var localMessage = GetQueuedUniversalEventMessage(universalEvent);
			localMessage.EM_GB = Env.CurrentBranch.PK;

			Factory.SaveForTesting();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			new UniversalMessageProcessingManager(serviceTaskLog).Process(localMessage);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, localMessage.EM_Status);

				var declarationBO = (BusinessObject)declaration;

				var athLogs = declarationBO.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "ATH"));
				AssertEquals("Events found", 1, athLogs.Length);

				var athLog = athLogs[0];
				AssertEquals("EventTime", new ZDateTime(2014, 3, 6, 12, 3, 4), athLog.SL_EventTime);
				AssertEquals("RefereneceFreeText", "txt", athLog.ReferenceFreeText);
				AssertEquals("Unsupported parameter", "aaa", athLog.Parameters["AAA"]);
				AssertEquals("Type", "CCC", athLog.Parameters[Constants.EventReferenceParameters.Codes.Type]);
				AssertEquals("Name", "Peter", athLog.Parameters[Constants.EventReferenceParameters.Codes.Name]);
				AssertEquals("Partial", "5", athLog.Parameters[Constants.EventReferenceParameters.Codes.Partial]);
				AssertEquals("Total", "12", athLog.Parameters[Constants.EventReferenceParameters.Codes.Total]);
				AssertEquals("VoyageFlightNumber", "QF1", athLog.Parameters[Constants.EventReferenceParameters.Codes.VoyageFlightNumber]);
				AssertEquals("FlightDate", "2014-12-21", athLog.Parameters[Constants.EventReferenceParameters.Codes.FlightDate]);
				AssertEquals("ExternalDocumentType", "OTH", athLog.Parameters[Constants.EventReferenceParameters.Codes.ExternalDocumentType]);
				AssertEquals("ReceiptNumber", "29F799DF-FE5C-4150-9A57-40995FC83590", athLog.Parameters[Constants.EventReferenceParameters.Codes.ReceiptNumber]);
				AssertEquals("ReferenceNumber", "80312346", athLog.Parameters[Constants.EventReferenceParameters.Codes.ReferenceNumber]);
				AssertEquals("RequestNumber", "11547", athLog.Parameters[Constants.EventReferenceParameters.Codes.RequestNumber]);
			});
			AssertContains("The following parameter codes are not valid: AAA.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear(); //Reason: Testing of unspported parameters is a part of this unit test
		}

		public void TestUniversalEvent_WithEventParameters_LocationHasBeenTransformed()
		{
			var declaration = Factory.BOFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_GB = Env.CurrentBranch.PK;
			declaration.JE_MasterBill = "MB1";
			declaration.JE_HouseBill = "HB1";

			const string universalEvent = @"
<UniversalEvent>
	<Event>
		<EventType>ATH</EventType>
		<EventTime>2014-03-06T12:03:04</EventTime>
		<EventReference>McLaren|CMP=aaa</EventReference>
		<EventParameters>
			<Location> JFK </Location>
			<Facility>Maidan</Facility>
			<Department>Maidan</Department>
			<Reason>Ovoshch</Reason>
			<Service>Lustration</Service>
			<MessageType>Oi Oi Oi!</MessageType>
			<Old>CONT123</Old>
			<New>CONT456</New>
			<Type>ContainerID</Type>
		</EventParameters>
		<DataContext>
			<DataProvider>Dummy</DataProvider>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>MB1</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>HB1</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var localMessage = GetQueuedUniversalEventMessage(universalEvent);
			localMessage.EM_GB = Env.CurrentBranch.PK;

			Factory.SaveForTesting();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			new UniversalMessageProcessingManager(serviceTaskLog).Process(localMessage);

			var declarationBO = (BusinessObject)declaration;

			var athLogs = declarationBO.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "ATH"));
			var athLog = athLogs[0];

			AssertEquals("Location", "USJFK", athLog.Parameters[Constants.EventReferenceParameters.Codes.Location]);
		}

		public void TestUniversalEvent_WithEventParameters_QuantityHasBeenTransformed()
		{
			var declaration = Factory.BOFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_GB = Env.CurrentBranch.PK;
			declaration.JE_MasterBill = "MB1";
			declaration.JE_HouseBill = "HB1";

			const string universalEvent = @"
<UniversalEvent>
	<Event>
		<EventType>RCV</EventType>
		<EventTime>2014-03-06T12:03:04</EventTime>
		<EventReference>McLaren|CMP=aaa</EventReference>
		<EventParameters>
			<Quantity>2</Quantity>
			<Facility>Maidan</Facility>
			<Department>Maidan</Department>
			<Reason>Ovoshch</Reason>
			<Service>Lustration</Service>
			<MessageType>Oi Oi Oi!</MessageType>
			<Old>CONT123</Old>
			<New>CONT456</New>
			<Type>ContainerID</Type>
		</EventParameters>
		<DataContext>
			<DataProvider>Dummy</DataProvider>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>MB1</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>HB1</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var localMessage = GetQueuedUniversalEventMessage(universalEvent);
			localMessage.EM_GB = Env.CurrentBranch.PK;

			Factory.SaveForTesting();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			new UniversalMessageProcessingManager(serviceTaskLog).Process(localMessage);

			var declarationBO = (BusinessObject)declaration;

			var athLogs = declarationBO.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "RCV"));
			var athLog = athLogs[0];

			AssertEquals("Quantity", "2", athLog.Parameters[Constants.EventReferenceParameters.Codes.Quantity]);
		}

		public void TestUniversalEvent_WithEventParameters_StatusHasBeenTransformed()
		{
			var declaration = Factory.BOFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_GB = Env.CurrentBranch.PK;
			declaration.JE_MasterBill = "MB1";
			declaration.JE_HouseBill = "HB1";

			const string universalEvent = @"
<UniversalEvent>
	<Event>
		<EventType>RCV</EventType>
		<EventTime>2014-03-06T12:03:04</EventTime>
		<EventReference>McLaren|CMP=aaa</EventReference>
		<EventParameters>
			<Status>VAD</Status>
			<Facility>Maidan</Facility>
			<Department>Maidan</Department>
			<Reason>Ovoshch</Reason>
			<Service>Lustration</Service>
			<MessageType>Oi Oi Oi!</MessageType>
			<Old>CONT123</Old>
			<New>CONT456</New>
			<Type>ContainerID</Type>
		</EventParameters>
		<DataContext>
			<DataProvider>Dummy</DataProvider>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>MB1</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>HB1</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var localMessage = GetQueuedUniversalEventMessage(universalEvent);
			localMessage.EM_GB = Env.CurrentBranch.PK;

			Factory.SaveForTesting();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			new UniversalMessageProcessingManager(serviceTaskLog).Process(localMessage);

			var declarationBO = (BusinessObject)declaration;

			var athLogs = declarationBO.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "RCV"));
			var athLog = athLogs[0];

			AssertEquals("Status", "VAD", athLog.Parameters[Constants.EventReferenceParameters.Codes.Status]);
		}

		public void TestUniversalEvent_WithEventParameters_ScoreHasBeenTransformed()
		{
			var declaration = Factory.BOFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_GB = Env.CurrentBranch.PK;
			declaration.JE_MasterBill = "MB1";
			declaration.JE_HouseBill = "HB1";

			const string universalEvent = @"
<UniversalEvent>
	<Event>
		<EventType>RCV</EventType>
		<EventTime>2014-03-06T12:03:04</EventTime>
		<EventReference>McLaren|CMP=aaa</EventReference>
		<EventParameters>
			<Score>68%</Score>
			<Facility>Maidan</Facility>
			<Department>Maidan</Department>
			<Reason>Ovoshch</Reason>
			<Service>Lustration</Service>
			<MessageType>Oi Oi Oi!</MessageType>
			<Old>CONT123</Old>
			<New>CONT456</New>
			<Type>ContainerID</Type>
		</EventParameters>
		<DataContext>
			<DataProvider>Dummy</DataProvider>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>MB1</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>HB1</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var localMessage = GetQueuedUniversalEventMessage(universalEvent);
			localMessage.EM_GB = Env.CurrentBranch.PK;

			Factory.SaveForTesting();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			new UniversalMessageProcessingManager(serviceTaskLog).Process(localMessage);

			var declarationBO = (BusinessObject)declaration;

			var athLogs = declarationBO.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "RCV"));
			var athLog = athLogs[0];

			AssertEquals("Score", "68%", athLog.Parameters[Constants.EventReferenceParameters.Codes.Score]);
		}

		public void TestImportUniversalEventCreateEDocAndUpdateLogReference()
		{
			var setupFactory = new BusinessObjectFactory();
			var shipment = setupFactory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001953";
			AssertEquals(0, ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs.Count);
			setupFactory.Save();

			string xml = string.Format(AddStorageDocWithUniversalEvent, GlbBranch.CurrentBranch.GB_Code, GlbCompany.CurrentCompany.GC_Code, GlbDepartment.CurrentDepartment.GE_Code);
			var message = GetQueuedUniversalEventMessage(Factory, xml);
			Factory.SaveForTesting();

			var messageFactory = new BusinessObjectFactory();
			var messageToProcess = messageFactory.Load<IEDIMessage>(message.PK);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			using (messageFactory.AddDisposableService())
			{
				manager.Process(messageToProcess);
				messageFactory.Save();
			}

			AssertEquals(1, shipment.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "DDI")).Length);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, messageToProcess.EM_Status);
			AssertMultilineASCIIEquals("Service Task Log", @"Linked Event to Shipment S00001953.".Trim(), serviceTaskLog.ToString());
		}

		[TestDate(2020, 1, 1)]
		public void TestUniversalEventTimeImportDefaultsToDateTimeNow()
		{
			var setupFactory = new BusinessObjectFactory();
			var shipment = setupFactory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S0001000";
			setupFactory.Save();

			string xml = $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
			<Event>
				<DataContext>
					<DataTargetCollection>
						<DataTarget>
							<Type>ForwardingShipment</Type>
							<Key>S0001000</Key>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventTime></EventTime>
				<EventType>Z00</EventType>
				<IsEstimate>false</IsEstimate>
			</Event>
		</UniversalEvent>";

			var message = GetQueuedUniversalEventMessage(Factory, xml);
			Factory.SaveForTesting();

			var messageFactory = new BusinessObjectFactory();
			var messageToProcess = messageFactory.Load<IEDIMessage>(message.PK);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			using (messageFactory.AddDisposableService())
			{
				manager.Process(messageToProcess);
				messageFactory.Save();
			}

			var logs = shipment.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "Z00"));

			AssertEquals(1, logs.Length);

			var log = logs[0];
			AssertEquals(ZDateTime.Now, log.SL_EventTime);

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, messageToProcess.EM_Status);
			AssertMultilineASCIIEquals("Service Task Log", @"Linked Event to Shipment S0001000.".Trim(), serviceTaskLog.ToString());
		}

		public void TestImportUniversalEventCreateEDocOnShipmentDoNotSaveRequiredDocument_WI00068037()
		{
			var setupFactory = new BusinessObjectFactory();
			var shipment = setupFactory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001953";
			AssertEquals(0, ((IDocManagerSupport)shipment).DocManagerInfo.AllEDocs.Count);
			setupFactory.Save();

			string xml = string.Format(AddStorageDocWithUniversalEvent, GlbBranch.CurrentBranch.GB_Code, GlbCompany.CurrentCompany.GC_Code, GlbDepartment.CurrentDepartment.GE_Code);
			var message = GetQueuedUniversalEventMessage(Factory, xml);
			Factory.SaveForTesting();

			var validationFactory = new BusinessObjectFactory();
			var query = new ZQuery(JobRequiredDocumentSchema.EQ_DocPeriod, "SHP");
			query.AddToFilter(JobRequiredDocumentSchema.EQ_DocType, "HCC");
			AssertEquals("PREREQUISITE: Should be no JobRequiredDocument with EQ_DocType {HCC}", 0, validationFactory.Load<JobRequiredDocument>(query).Length);

			var messageFactory = new BusinessObjectFactory();
			var messageToProcess = messageFactory.Load<IEDIMessage>(message.PK);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (messageFactory.AddDisposableService())
			{
				manager.Process(messageToProcess);
				messageFactory.Save();
			}

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, messageToProcess.EM_Status);
			AssertMultilineASCIIEquals("Service Task Log", @"Linked Event to Shipment S00001953.".Trim(), serviceTaskLog.ToString());
			AssertEquals("1 HCC JobRequiredDocument should be created", 1, validationFactory.Load<JobRequiredDocument>(query).Length);
			var reloadedShipment = (IDocManagerSupport)new BusinessObjectFactory().Load<Forwarding.IForwardingShipment>(shipment.PK);
			AssertEquals(1, reloadedShipment.DocManagerInfo.AllEDocs.Count);
			AssertEquals(GlbBranch.CurrentBranch.GB_Code, reloadedShipment.DocManagerInfo.AllEDocs[0].VisibleBranchCode);
			AssertEquals(GlbCompany.CurrentCompany.GC_Code, reloadedShipment.DocManagerInfo.AllEDocs[0].VisibleCompanyCode);
			AssertEquals(GlbDepartment.CurrentDepartment.GE_Code, reloadedShipment.DocManagerInfo.AllEDocs[0].VisibleDepartmentCode);
		}

		#region const string AddStorageDocWithUniversalEvent

		const string AddStorageDocWithUniversalEvent = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
			<Event>
				<DataContext>
					<DataTargetCollection>
						<DataTarget>
							<Type>ForwardingShipment</Type>
							<Key>S00001953</Key>
						</DataTarget>
					</DataTargetCollection>
					<CodesMappedToTarget>true</CodesMappedToTarget>
				</DataContext>

				<EventTime>2014-08-18T10:36:33.557</EventTime>
				<EventType>DDI</EventType>
				<IsEstimate>false</IsEstimate>

				<ContextCollection>
					<Context>
						<Type>OrderNumber</Type>
						<Value>3380343</Value>
					</Context>
				</ContextCollection>
				<AttachedDocumentCollection>
					<AttachedDocument>
						<FileName>docTest.txt</FileName>
						<ImageData>5465737420446F63756D656E742055706C6F61642066696C652E</ImageData>
						<Type>
							<Code>HCC</Code>
						</Type>
						<IsPublished>false</IsPublished>
						<VisibleBranchCode>{0}</VisibleBranchCode>
			<VisibleCompanyCode>{1}</VisibleCompanyCode>
			<VisibleDepartmentCode>{2}</VisibleDepartmentCode>
			</AttachedDocument>
				</AttachedDocumentCollection>
			</Event>
		</UniversalEvent>";

		#endregion

		public void TestMessageIsDiscardedIfMultipleMatchesFoundForIDataContextCoordinator()
		{
			eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var quotedBooking = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			quotedBooking[JobShipmentSchema.JS_HouseBill] = "TBA12345";
			quotedBooking[JobShipmentSchema.JS_IsBooking] = true;
			quotedBooking[JobShipmentSchema.JS_IsForwardRegistered] = false;

			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_HouseBill] = "TBA12345";
			shipment[JobShipmentSchema.JS_IsForwardRegistered] = true;

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "C1CO";
			shippingLine.RSL_IsNVO = true;

			Factory.SaveForTesting();

			using (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty,
					   Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = true }))
			{
				var message = GetQueuedUniversalEventMessage(UniversalEventForMultipleMatchesDiscarded);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

					AssertMultilineASCIIEquals("Message Log Note", @"
Warning - Multiple records found. Message import failed.
Warning - No Module found a Business Entity to link this Universal Event to.
Message Discarded.
".Trim(), message.GetLogNoteText());
				});
			}
		}

		#region const string UniversalEventForMultipleMatchesDiscarded

		const string UniversalEventForMultipleMatchesDiscarded = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
	<EventTime>2016-05-12T12:03:04</EventTime>
	<EventType>SBR</EventType>
	<EventParameters>
		<Type>Shipment Visibility</Type>
	</EventParameters>
	<DataContext>
		<DataProvider>CargoWise One</DataProvider>
	</DataContext>

	<ContextCollection>
		<Context>
			<Type>MBOLNumber</Type>
			<Value>TBA12345</Value>
		</Context>
		<Context>
			<Type>CarrierC1CCode</Type>
			<Value>C1CO</Value>
		</Context>
		<Context>
			<Type>Reference</Type>
			<Value>b97aaa91-819f-ee11-b399-005056a592a9</Value>
		</Context>
	</ContextCollection>
</Event>
</UniversalEvent>";

		#endregion

		public void TestMessageIsDiscardedIfNoMatchFoundForIDataContextCoordinator()
		{
			eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "C1CO";
			shippingLine.RSL_IsNVO = false;
			shippingLine.RSL_IsShippingLine = true;

			Factory.SaveForTesting();

			using (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty,
					   Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = true }))
			{
				var message = GetQueuedUniversalEventMessage(UniversalEventForNoMatch);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var sessionTracker = manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);
					AssertContains("Cannot link Bill of Lading because: [* Master Bill Number is Invalid. *]".Trim(), sessionTracker.ToString());
					AssertContains("Cannot link Bill of Lading because: [* Master Bill Number is Invalid. *]".Trim(), sessionTracker.ToString());
				});
			}
		}

		#region const string UniversalEventForOneMatch

		const string UniversalEventForNoMatch = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
	<EventTime>2016-05-12T12:03:04</EventTime>
	<EventType>SBR</EventType>
	<EventParameters>
		<Type>Shipment Visibility</Type>
	</EventParameters>
	<DataContext>
		<DataProvider>CargoWise One</DataProvider>
	</DataContext>

	<ContextCollection>
		<Context>
			<Type>MBOLNumber</Type>
			<Value>TBA12345</Value>
		</Context>
		<Context>
			<Type>CarrierC1CCode</Type>
			<Value>C1CO</Value>
		</Context>
		<Context>
			<Type>Reference</Type>
			<Value>b97aaa91-819f-ee11-b399-005056a592a9</Value>
		</Context>
	</ContextCollection>
</Event>
</UniversalEvent>";

		#endregion

		public void TestMessageIsNotDiscardedIfOneMatchFoundForIDataContextCoordinator()
		{
			eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var quotedBooking = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			quotedBooking[JobShipmentSchema.JS_HouseBill] = "TBA12345";
			quotedBooking[JobShipmentSchema.JS_IsBooking] = true;
			quotedBooking[JobShipmentSchema.JS_IsForwardRegistered] = false;
			quotedBooking[JobShipmentSchema.JS_UniqueConsignRef] = "S10101010";

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "C1CO";
			shippingLine.RSL_IsNVO = true;

			Factory.SaveForTesting();

			using (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty,
					   Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = true }))
			{
				var message = GetQueuedUniversalEventMessage(UniversalEventForOneMatch);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
					AssertMultilineASCIIEquals("Service Task Log", @"Linked Event to Quick Booking - Booking (S10101010).".Trim(), serviceTaskLog.ToString());
				});
			}
		}

		#region const string UniversalEventForOneMatch

		const string UniversalEventForOneMatch = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
	<EventTime>2016-05-12T12:03:04</EventTime>
	<EventType>SBR</EventType>
	<EventParameters>
		<Type>Shipment Visibility</Type>
	</EventParameters>
	<DataContext>
		<DataProvider>CargoWise One</DataProvider>
	</DataContext>

	<ContextCollection>
		<Context>
			<Type>MBOLNumber</Type>
			<Value>TBA12345</Value>
		</Context>
		<Context>
			<Type>CarrierC1CCode</Type>
			<Value>C1CO</Value>
		</Context>
		<Context>
			<Type>Reference</Type>
			<Value>b97aaa91-819f-ee11-b399-005056a592a9</Value>
		</Context>
	</ContextCollection>
</Event>
</UniversalEvent>";

		#endregion

		public void TestMessageIsNotDiscardedIfMultipleMatchesFoundForIDataContextCoordinatorIdentifierReturnsEmpty()
		{
			eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var quotedBooking = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			quotedBooking[JobShipmentSchema.JS_HouseBill] = "TBA12345";
			quotedBooking[JobShipmentSchema.JS_IsBooking] = true;
			quotedBooking[JobShipmentSchema.JS_IsForwardRegistered] = false;
			quotedBooking[JobShipmentSchema.JS_TransportMode] = "AIR";
			quotedBooking[JobShipmentSchema.JS_BookingReference] = "REFERME123";
			quotedBooking[JobShipmentSchema.JS_UniqueConsignRef] = "S10101010";

			var consol = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			consol[JobConsolSchema.JK_BookingReference] = "02012345675";
			consol[JobConsolSchema.JK_TransportMode] = "SEA";

			Factory.SaveForTesting();

			using (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty,
					   Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = true }))
			{
				var message = GetQueuedUniversalEventMessage(UniversalEventForMultipleMatches);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
					AssertContains("Booking Service Task Log", "Linked Event to Quick Booking - Booking (S10101010).", serviceTaskLog.ToString());
					AssertContains("Consol Service Task Log", "Linked Event to Consol C00001000.", serviceTaskLog.ToString());
				});
			}
		}

		#region const string UniversalEventForMultipleMatches

		const string UniversalEventForMultipleMatches = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
	<EventTime>2016-05-12T12:03:04</EventTime>
	<EventType>CCD</EventType>
	<DataContext>
		<DataProvider>CargoWise One</DataProvider>
	</DataContext>

	<ContextCollection>
		<Context>
		  <Type>ShippersReference</Type>
		  <Value>REFERME123</Value>
		</Context>
		<Context>
		  <Type>CarriersBookingReference</Type>
		  <Value>02012345675</Value>
		</Context>
	</ContextCollection>
</Event>
</UniversalEvent>";

		#endregion

		void ClearLogPivot(ZGuid pk)
		{
			foreach (var pivot in Factory.Load<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation2ID, pk)))
			{
				pivot.Delete();
			}
		}

		void ClearDataImportLogs(BusinessObject bizObj)
		{
			foreach (var log in bizObj.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description))
			{
				log.Delete();
			}
		}

		#region Universal Container Event Transformer

		public void TestInboundUniversalContainerEventTransformer_Delivery()
		{
			var anotherFactory = new BusinessObjectFactory();

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "DLV");
			query.AddToFilter(StmALogSchema.SL_EventTime, new ZDateTime(2016, 2, 16, 9, 0, 0));
			var existedLogs = anotherFactory.Load<StmALog>(query);
			AssertEquals("existedLogs.Length should be 0", 0, existedLogs.Length);

			var consol = anotherFactory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			consol[JobConsolSchema.JK_UniqueConsignRef] = "C00001286";
			consol[JobConsolSchema.JK_TransportMode] = "SEA";
			consol[JobConsolSchema.JK_MasterBillNum] = "FILLET-O-FISH";
			consol[JobConsolSchema.JK_BookingReference] = "";

			var container = anotherFactory.New(ObjectFactory.GetType<Forwarding.IForwardingContainer>());
			container[JobContainerSchema.JC_ContainerMode] = "FCL";
			container[JobContainerSchema.JC_ContainerNum] = "CONT0020110";
			container[JobContainerSchema.JC_ContainerJobID] = "D00001016";
			container[JobContainerSchema.JC_RC] = new RefContainer.Loader(new BusinessObjectFactory()).LoadFromCode("20GP").PK;
			container[JobContainerSchema.JC_JK] = consol.PK;

			anotherFactory.Save();

			var message = GetQueuedUniversalEventMessage(ContainerUniversalEventWithTransformerDelivery);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			Factory.SaveForTesting();

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"Linked Event to Container 'CONT0020110'.".Trim(), serviceTaskLog.ToString());

				var query2 = new ZQuery();
				query2.AddToFilter(StmALogSchema.SL_SE_NKEvent, "DLV");
				query2.AddToFilter(StmALogSchema.SL_EventTime, new ZDateTime(2016, 2, 16, 9, 0, 0));
				query2.AddToFilter(StmALogSchema.SL_Table, "JobContainer");
				var transformedLogs = Factory.Load<StmALog>(query2);
				AssertEquals("transformedLogs.Length should be 1", 1, transformedLogs.Length);
				AssertEquals("SL_IsEstimate", false, transformedLogs[0].SL_IsEstimate);
				AssertEquals("SL_Reference", "|FAC=CNE|TYP=FUL", transformedLogs[0].SL_Reference);
				AssertEquals("Parameters count", 2, transformedLogs[0].Parameters.Count);
				AssertEquals("Parameter FAC", "CNE", transformedLogs[0].Parameters["FAC"]);
				AssertEquals("Parameter TYP", "FUL", transformedLogs[0].Parameters["TYP"]);
			});
		}

		#region const string ContainerUniversalEventWithTransformerDelivery

		const string ContainerUniversalEventWithTransformerDelivery = @"<?xml version=""1.0"" encoding=""utf-8""?>
	<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingConsol</Type>
					<Key>C00001286</Key>
				</DataTarget>
			</DataTargetCollection>

			<DataProvider>EDIDATEDI</DataProvider>
			<EnterpriseID>EDI</EnterpriseID>
		</DataContext>

	<EventTime>2016-02-16T09:00:00.000</EventTime>
		<EventType>DLV</EventType>
		<IsEstimate>false</IsEstimate>
	<EventReference>|FAC=CNE|TYP=FUL</EventReference>

		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>FILLET-O-FISH</Value>
			</Context>
			<Context>
				<Type>MBOLOriginUNLOCO</Type>
				<Value>HKHKG</Value>
			</Context>
			<Context>
				<Type>MBOLDestinationUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
		<Context>
				<Type>ContainerNumber</Type>
				<Value>CONT0020110</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		#endregion

		public void TestInboundUniversalContainerEventTransformer_Pickup()
		{
			var anotherFactory = new BusinessObjectFactory();

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "GOU");
			query.AddToFilter(StmALogSchema.SL_EventTime, new ZDateTime(2016, 2, 16, 10, 0, 0));
			var existedLogs = anotherFactory.Load<StmALog>(query);
			AssertEquals("No GOU event log", 0, existedLogs.Length);

			query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "PUP");
			query.AddToFilter(StmALogSchema.SL_EventTime, new ZDateTime(2016, 2, 16, 10, 0, 0));
			existedLogs = anotherFactory.Load<StmALog>(query);
			AssertEquals("No PUP event log", 0, existedLogs.Length);

			var consol = anotherFactory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			consol[JobConsolSchema.JK_UniqueConsignRef] = "C00001286";
			consol[JobConsolSchema.JK_TransportMode] = "SEA";
			consol[JobConsolSchema.JK_MasterBillNum] = "FILLET-O-FISH";
			consol[JobConsolSchema.JK_BookingReference] = "";

			var container = anotherFactory.New(ObjectFactory.GetType<Forwarding.IForwardingContainer>());
			container[JobContainerSchema.JC_ContainerMode] = "FCL";
			container[JobContainerSchema.JC_ContainerNum] = "CONT0020110";
			container[JobContainerSchema.JC_ContainerJobID] = "D00001016";
			container[JobContainerSchema.JC_RC] = new RefContainer.Loader(new BusinessObjectFactory()).LoadFromCode("20GP").PK;
			container[JobContainerSchema.JC_JK] = consol.PK;

			anotherFactory.Save();

			var message = GetQueuedUniversalEventMessage(ContainerUniversalEventWithTransformerPickup);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			Factory.SaveForTesting();

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"Linked Event to Container 'CONT0020110'.".Trim(), serviceTaskLog.ToString());

				var query2 = new ZQuery();
				query2.AddToFilter(StmALogSchema.SL_SE_NKEvent, "GOU");
				query2.AddToFilter(StmALogSchema.SL_EventTime, new ZDateTime(2016, 2, 16, 10, 0, 0));
				query2.AddToFilter(StmALogSchema.SL_Table, "JobContainer");
				var transformedLogs = Factory.Load<StmALog>(query2);
				AssertEquals("transformedLogs.Length should be 1", 1, transformedLogs.Length);
				AssertEquals("SL_IsEstimate", false, transformedLogs[0].SL_IsEstimate);
				AssertEquals("SL_Reference", "|FAC=CTO|TYP=FUL", transformedLogs[0].SL_Reference);
				AssertEquals("Parameters count", 2, transformedLogs[0].Parameters.Count);
				AssertEquals("Parameter FAC", "CTO", transformedLogs[0].Parameters["FAC"]);
				AssertEquals("Parameter TYP", "FUL", transformedLogs[0].Parameters["TYP"]);

				existedLogs = Factory.Load<StmALog>(query);
				AssertEquals("No PUP event log after transformed", 0, existedLogs.Length);
			});
		}

		#region const string ContainerUniversalEventWithTransformerPickup

		const string ContainerUniversalEventWithTransformerPickup = @"<?xml version=""1.0"" encoding=""utf-8""?>
	<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingConsol</Type>
					<Key>C00001286</Key>
				</DataTarget>
			</DataTargetCollection>

			<DataProvider>EDIDATEDI</DataProvider>
			<EnterpriseID>EDI</EnterpriseID>
		</DataContext>

	<EventTime>2016-02-16T10:00:00.000</EventTime>
		<EventType>PUP</EventType>
		<IsEstimate>false</IsEstimate>
	<EventReference>|FAC=CTO|TYP=FUL</EventReference>

	<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>FILLET-O-FISH</Value>
			</Context>
			<Context>
				<Type>MBOLOriginUNLOCO</Type>
				<Value>HKHKG</Value>
			</Context>
			<Context>
				<Type>MBOLDestinationUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
		<Context>
				<Type>ContainerNumber</Type>
				<Value>CONT0020110</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		#endregion

		#endregion

	}
}
