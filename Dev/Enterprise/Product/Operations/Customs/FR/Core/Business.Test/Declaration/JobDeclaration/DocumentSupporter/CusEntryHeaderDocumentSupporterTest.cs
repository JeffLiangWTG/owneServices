using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryHeaderDocumentSupporter))]
	sealed class CusEntryHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetSnapshotRevertedEntryIfPossible_ShouldBeAbleToConsiderPersistedSnapshotOnly()
		{
			using (EU.Registry.EUCustomsDataRegistry.Instance.SADGenerationOnClearanceEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (declaration, entryToRevert) = GetDeclarationForTest();
				SendMessageForEntry(entryToRevert, "VAA");
				AssertNoExceptionThrown("While firstly processing to BAE, and registry SADGenerationOnClearanceEnabled is turned on, a snapshot is captured but not persisted yet, in this case, we should not try to revert.", () => ProcessMessageToBAE(entryToRevert));
			}
		}

		public void TestWrapperDataSource_EntryLoadedFromAnotherFactoryShouldNotBeAbleToSave()
		{
			var (declaration, entryToRevert) = GetDeclarationForTest();
			SendMessageForEntry(entryToRevert, "VAA");
			ProcessMessageToBAE(entryToRevert);

			var docSupporter = (CusEntryHeaderDocumentSupporter)entryToRevert.DocumentSupporter;
			docSupporter.WrapperDataSource.Factory.Save();
			AssertStartsWith("Error should be reported if the entry reverted in memory only is attempting to save.", "Error: ReadOnlyBusinessObjectFactory named [] should not be saved. Call stack trace:", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestWrapperDataSource_ShouldHaveSameValuesAsNativeEntryHeader_WhenCurrentStatusIsBAE()
		{
			var (declaration, entryToRevert) = GetDeclarationForTest();
			SendMessageForEntry(entryToRevert, "VAA");
			ProcessMessageToBAE(entryToRevert);

			var docSupporter = (CusEntryHeaderDocumentSupporter)entryToRevert.DocumentSupporter;
			AssertContainsExactElementsInAnyOrder("WrapperDataSource should have same values as current entry header.", new[] { "11111111", "22222222", "44444444" }, docSupporter.WrapperDataSource.InvoiceLines.Select(line => line.JI_Tariff));
		}

		public void TestWrapperDataSource_ShouldHaveSameValueAsSnapshot_WhenCurrentStatusIsRectificationSentButNoResponse()
		{
			var (declaration, entryToRevert) = GetDeclarationForTest();
			SendMessageForEntry(entryToRevert, "VAA");
			ProcessMessageToBAE(entryToRevert);

			entryToRevert.InvoiceLines.First(x => x.JI_Tariff == "11111111").JI_Tariff = "88888888";
			Factory.Save();

			SendMessageForEntry(entryToRevert, "REC");

			var docSupporter = (CusEntryHeaderDocumentSupporter)entryToRevert.DocumentSupporter;
			AssertContainsExactElementsInAnyOrder("When REC message sent but no response received yet, WrapperDataSource should use the entry values at BAE.",
				new[] { "11111111", "22222222", "44444444" }, docSupporter.WrapperDataSource.InvoiceLines.Select(line => line.JI_Tariff));

			var anotherFactory = new BusinessObjectFactory();
			var reloadedEntry = anotherFactory.Load<CusEntryHeader>(entryToRevert.PK);
			AssertContainsExactElementsInAnyOrder("Though entry values at BAE is used for WrapperDataSource, the entry itself should not be reverted.",
				new[] { "88888888", "22222222", "44444444" }, reloadedEntry.InvoiceLines.Select(line => line.JI_Tariff));
		}

		public void TestWrapperDataSource_ShouldHaveSameValueAsSnapshot_WhenCurrentStatusIsRectificationRejected()
		{
			var (declaration, entryToRevert) = GetDeclarationForTest();
			SendMessageForEntry(entryToRevert, "VAA");
			ProcessMessageToBAE(entryToRevert);

			entryToRevert.InvoiceLines.First(x => x.JI_Tariff == "11111111").JI_Tariff = "88888888";
			Factory.Save();

			SendMessageForEntry(entryToRevert, "REC");
			ProcessMessageToBAE(entryToRevert, "DeltaCImportBAEResponseMessageWithRefusedRectification");

			var docSupporter = (CusEntryHeaderDocumentSupporter)entryToRevert.DocumentSupporter;
			AssertContainsExactElementsInAnyOrder("When REC message is rejected, WrapperDataSource should use the entry values at BAE.",
				new[] { "11111111", "22222222", "44444444" }, docSupporter.WrapperDataSource.InvoiceLines.Select(line => line.JI_Tariff));

			var anotherFactory = new BusinessObjectFactory();
			var reloadedEntry = anotherFactory.Load<CusEntryHeader>(entryToRevert.PK);
			AssertContainsExactElementsInAnyOrder("Though entry values at BAE is used for WrapperDataSource, the entry itself should not be reverted.",
				new[] { "88888888", "22222222", "44444444" }, reloadedEntry.InvoiceLines.Select(line => line.JI_Tariff));
		}

		public void TestWrapperDataSource_ShouldHaveSameValueAsSnapshot_WhenCurrentStatusIsRectificationAccepted()
		{
			var (declaration, entryToRevert) = GetDeclarationForTest();
			SendMessageForEntry(entryToRevert, "VAA");
			ProcessMessageToBAE(entryToRevert);

			entryToRevert.InvoiceLines.First(x => x.JI_Tariff == "11111111").JI_Tariff = "88888888";
			Factory.Save();

			SendMessageForEntry(entryToRevert, "REC");
			ProcessMessageToBAE(entryToRevert, "DeltaCImportBAEResponseMessageWithAcceptedRectification");

			var docSupporter = (CusEntryHeaderDocumentSupporter)entryToRevert.DocumentSupporter;
			AssertContainsExactElementsInAnyOrder("When REC message is accepted, WrapperDataSource should use the entry values at current status.",
				new[] { "88888888", "22222222", "44444444" }, docSupporter.WrapperDataSource.InvoiceLines.Select(line => line.JI_Tariff));

			var anotherFactory = new BusinessObjectFactory();
			var reloadedEntry = anotherFactory.Load<CusEntryHeader>(entryToRevert.PK);
			AssertContainsExactElementsInAnyOrder("Entry itself should not be reverted.",
				new[] { "88888888", "22222222", "44444444" }, reloadedEntry.InvoiceLines.Select(line => line.JI_Tariff));
		}

		(JobDeclaration, CusEntryHeader) GetDeclarationForTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_HouseBill = "BILL1234";
			var packingGroup = declaration.PrimaryHouseBill.PackingGroups[0];
			var package1 = packingGroup.Packages.AddNew();
			package1.CW_PackType = "1A";
			package1.CW_PackQty = 10;
			package1.CW_MarksAndNos = "Coke";

			var package2 = packingGroup.Packages.AddNew();
			package2.CW_PackType = "1B";
			package2.CW_PackQty = 15;
			package2.CW_MarksAndNos = "Sprite";

			var cei1 = declaration.CustomsEntryInstructions.AddNew();
			cei1.CEI_Style = "A";
			var cei2 = declaration.CustomsEntryInstructions.AddNew();
			cei2.CEI_Style = "F";

			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "INV1";
			invoiceHeader1.JZ_InvoiceAmount = 0;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "INV2";
			invoiceHeader1.JZ_InvoiceAmount = 0;

			var invoiceLine1 = AddInvoiceLine(invoiceHeader1, cei1, "11111111", "InvoiceLine1 Desc.", "111", 123m, 134m, "PRO1", "MK1");
			AddPackagePivot(invoiceLine1, package1, 2);

			var invoiceLine2 = AddInvoiceLine(invoiceHeader1, cei1, "22222222", "InvoiceLine2 Desc.", "222", 234m, 253m, "PRO2", "MK2");
			AddPackagePivot(invoiceLine2, package2, 10);

			var invoiceLine3 = AddInvoiceLine(invoiceHeader2, cei2, "33333333", "InvoiceLine3 Desc.", "333", 313m, 384m, "PRO3", "MK3");
			AddPackagePivot(invoiceLine3, package1, 8);

			var invoiceLine4 = AddInvoiceLine(invoiceHeader2, cei1, "44444444", "InvoiceLine4 Desc.", "444", 482m, 428m, "PRO4", "MK4");
			AddPackagePivot(invoiceLine4, package2, 5);

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			Factory.Save();

			var entryToRevert = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First(entry => entry.InvoiceLines.Any(line => line.EntryInstruction.CEI_Style == "A"));
			entryToRevert.CH_SequenceNumber = 3;

			return (declaration, entryToRevert);

			JobComInvoiceLine AddInvoiceLine(JobComInvoiceHeader invoice, CusEntryInstruction cei, string tariff, string desc, string primaryPreference, decimal secondQuantity, decimal thirdQuantity, string procedure, string matchingKey)
			{
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = cei.PK;
				invoiceLine.JI_Tariff = tariff;
				invoiceLine.JI_Description = desc;
				invoiceLine.JI_PrimaryPreference = primaryPreference;
				invoiceLine.JI_CustomsSecondQuantity = secondQuantity;
				invoiceLine.JI_CustomsThirdQuantity = thirdQuantity;
				invoiceLine.JI_Procedure = procedure;
				invoiceLine.JI_MatchingKey = matchingKey;
				return invoiceLine;
			}

			void AddPackagePivot(JobComInvoiceLine invoiceLine, BasePackage package, int number)
			{
				var pivot = invoiceLine.PackagesPivot.AddNew();
				pivot.CHC_CW = package.PK;
				pivot.CHC_NumberOfPacks = number;
			}
		}

		void SendMessageForEntry(CusEntryHeader entry, string messageType)
		{
			var outgoingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			outgoingInterchange.EI_InterchangeNum = "123";
			var outgoingMessage = Factory.NewWithValidTestData<FREDIMessage>();
			outgoingMessage.MessageNumberStrategy = new FRMessageNumberStrategy(Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
			entry.Messages.Add(outgoingMessage);
			outgoingMessage.EM_EI = outgoingInterchange.PK;
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			outgoingMessage.EM_MessageSubType = messageType;
			Factory.Save();
		}

		void ProcessMessageToBAE(CusEntryHeader entry, string responseMessageFile = null)
		{
			var logger = new LoggingInformation();
			var processor = new ImportDeltaCResponseMessageProcessor(logger);

			var messageBAE = Factory.New<DeltaCImportFREDIMessage>();
			messageBAE.EM_ApplicationCode = "FRC";
			messageBAE.EM_MessageType = MessageTypeList.Codes.IMC;
			messageBAE.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			messageBAE.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageBAE.EM_Status = EDIMessage.Status.Queued;
			messageBAE.EM_MessageText = resourceRetriever.Value.GetString($"Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.{responseMessageFile ?? "DeltaCImport" + "BAEResponseMessage"}.xml");
			processor.ProcessMessage(messageBAE);
			entry.Declaration.ResetMessageTypeChangeLogs();
			Factory.Save();
		}

		public void TestCusEntryHeaderGetsTheRightDocumentSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertType<CusEntryHeaderDocumentSupporter>("EntryHeader.DocumentSupporter should be of type CusEntryHeaderDocumentSupporter.", entryHeader.DocumentSupporter);

			var docSupporter = (CusEntryHeaderDocumentSupporter)entryHeader.DocumentSupporter;
			AssertEquals(true, docSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.SADH)));
			AssertEquals(1, docSupporter.GetDocumentWrappers(Core.Constants.DataContext.SADH, null).Length);
			AssertEquals(true, docSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.FRSADH)));
			AssertEquals(1, docSupporter.GetDocumentWrappers(Core.Constants.DataContext.FRSADH, null).Length);
			AssertEquals(true, docSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.LiquidationDetails)));
			AssertEquals(1, docSupporter.GetDocumentWrappers(Core.Constants.DataContext.LiquidationDetails, null).Length);
			AssertEquals(true, docSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.T2L)));
			AssertEquals(1, docSupporter.GetDocumentWrappers(Core.Constants.DataContext.T2L, null).Length);
			AssertEquals(true, docSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.T2LF)));
			AssertEquals(1, docSupporter.GetDocumentWrappers(Core.Constants.DataContext.T2LF, null).Length);
		}

		public void TestCusEntryHeaderGetsTheRightIDDDocumentSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			AssertType<CusEntryHeaderDocumentSupporter>("EntryHeader.DocumentSupporter should be of type CusEntryHeaderDocumentSupporter.", entryHeader.DocumentSupporter);

			var docSupporter = (CusEntryHeaderDocumentSupporter)entryHeader.DocumentSupporter;

			AssertIDDDocumentSupporter(DeltaIEResponseMessageSubTypeList.Codes.PrelodgedDeclarationAcceptance, new ZDateTime(2024, 03, 11, DateTimeKind.Utc), "Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE426ResponseMessage.json");
			AssertIDDDocumentSupporter(DeltaIEResponseMessageSubTypeList.Codes.DeclarationAcceptance, new ZDateTime(2024, 03, 11, DateTimeKind.Utc), "Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE428ResponseMessage.json");
			AssertIDDDocumentSupporter(DeltaIEResponseMessageSubTypeList.Codes.ReleaseNotification, new ZDateTime(2024, 03, 11, DateTimeKind.Utc), "Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE429ResponseMessage.json");

			void AssertIDDDocumentSupporter(ZString messageSubType, ZDateTime createdDate, ZString messageSource)
			{
				var message = AddMessage(messageSubType, createdDate, messageSource);
				entryHeader.Messages.Add(message);

				AssertEquals(true, docSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.IDD)));
				AssertEquals(1, docSupporter.GetDocumentWrappers(Core.Constants.DataContext.IDD, null).Length);
				entryHeader.Messages.Remove(message);
			}

			DeltaIEFREDIMessage AddMessage(ZString messageSubType, ZDateTime createdDateTime, ZString messageSource)
			{
				var msg = Factory.New<DeltaIEFREDIMessage>();
				msg.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
				msg.EM_SystemCreateTimeUtc = createdDateTime;
				msg.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
				msg.EM_MessageSubType = messageSubType;
				msg.EM_MessageText = resourceRetriever.Value.GetString(messageSource);
				return msg;
			}
		}

		public void TestGetFilterValue_MSGBKRCTYMOD()
		{
			var entry = GetDocumentSupportableBusinessObject();
			var docSupporter = (CusEntryHeaderDocumentSupporter)entry.DocumentSupporter;
			AssertEquals("MSGBKRCTYMOD filter should return Direction+CountryOfJurisdiction+TransportMode, therefore 'IMPFRSEA' for entry", "IMPFRSEA", docSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYMOD));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Martinique))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclarationForTest>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				var entryOfMQ = declaration.ActiveEntryHeaders.AddNew();
				var docSupporterMQ = (CusEntryHeaderDocumentSupporter)entryOfMQ.DocumentSupporter;
				AssertEquals("MSGBKRCTYMOD filter should return Direction+CountryOfJurisdiction+TransportMode, therefore 'EXPFRAIR' for entryOfMQ", "EXPFRAIR", docSupporterMQ.GetFilterValue(DocumentFilters.MSGBKRCTYMOD));
			}
		}

		public void TestGetFilterValue_T2LDocumentSupport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.FillWithValidTestData();
			invoiceLine1.JI_Tariff = "1111";
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.FillWithValidTestData();
			invoiceLine2.JI_Tariff = "2222";
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Spain;

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entry = declaration.CustomsEntryHeaders[0];
			Assert("Prerequisite: There're not any qualified elements in entry.T2LApplicableEntryLines.", !entry.T2LApplicableEntryLines.Any());
			var docSupporter = (CusEntryHeaderDocumentSupporter)entry.DocumentSupporter;
			AssertEquals("T2LDocumentSupport should be N as there are no qualified entry lines for T2L generation.", "N", docSupporter.GetFilterValue(DocumentFilters.T2LDocumentSupport));

			var invoiceLine3 = declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine3.FillWithValidTestData();
			invoiceLine3.JI_Tariff = "3333";
			invoiceLine3.JI_CountryOfOrigin = Core.Constants.CountryCodes.Germany;
			invoiceLine3.SupportingDocuments.AddNew().CSI_Code = UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LDocument;
			merger.DoMerge();
			Assert("Prerequisite: There're qualified elements in entry.T2LApplicableEntryLines.", entry.T2LApplicableEntryLines.Any());
			AssertEquals("T2LDocumentSupport should be Y as there is a qualified entry line for T2L generation.", "Y", docSupporter.GetFilterValue(DocumentFilters.T2LDocumentSupport));
		}

		public void TestGetFilterValue_T2LFDocumentSupport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.FillWithValidTestData();
			invoiceLine1.JI_Tariff = "1111";
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.FillWithValidTestData();
			invoiceLine2.JI_Tariff = "2222";
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Spain;

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entry = declaration.CustomsEntryHeaders[0];
			Assert("Prerequisite: There're not any qualified elements in entry.T2LFApplicableEntryLines.", !entry.T2LFApplicableEntryLines.Any());
			var docSupporter = (CusEntryHeaderDocumentSupporter)entry.DocumentSupporter;
			AssertEquals("T2LDocumentSupport should be N as there are no qualified entry lines for T2L generation.", "N", docSupporter.GetFilterValue(DocumentFilters.T2LFDocumentSupport));

			var invoiceLine3 = declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine3.FillWithValidTestData();
			invoiceLine3.JI_Tariff = "3333";
			invoiceLine3.JI_CountryOfOrigin = Core.Constants.CountryCodes.Germany;
			invoiceLine3.SupportingDocuments.AddNew().CSI_Code = UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LFDocument;
			merger.DoMerge();
			Assert("Prerequisite: There're qualified elements in entry.T2LFApplicableEntryLines.", entry.T2LFApplicableEntryLines.Any());
			AssertEquals("T2LDocumentSupport should be Y as there is a qualified entry line for T2LF generation.", "Y", docSupporter.GetFilterValue(DocumentFilters.T2LFDocumentSupport));
		}

		public void TestGetFilterValue_IDD()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var supporter = new CusEntryHeaderDocumentSupporterForTest(entryHeader);

			AssertEquals("IDD is available in FR", EconomicGroupList.Codes.EuropeanUnion, supporter.GetFilterValue(DocumentFilters.CTYEGIDD));
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclarationForTest>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			return entry;
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		class CusEntryHeaderDocumentSupporterForTest : CusEntryHeaderDocumentSupporter
		{
			public CusEntryHeaderDocumentSupporterForTest(CusEntryHeader entryHeader)
				: base(entryHeader)
			{
			}
		}
	}
}
