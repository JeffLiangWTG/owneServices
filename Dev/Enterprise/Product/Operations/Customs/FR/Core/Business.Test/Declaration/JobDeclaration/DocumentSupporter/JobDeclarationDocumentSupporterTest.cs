using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclarationDocumentSupporter))]
	class JobDeclarationDocumentSupporterTest : EU.Business.Declaration.Testing.JobDeclarationDocumentSupporterTest
	{
		public void TestWrapperDataSources_ShouldHaveSameValuesAsNativeEntryHeader_WhenCurrentStatusIsBAE()
		{
			var declaration = GetDeclarationForTest();
			var entry1 = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First(x => x.CH_SequenceNumber == 3);
			var entry2 = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First(x => x.CH_SequenceNumber == 4);

			SendMessageForEntry(entry1, "VAA");
			SendMessageForEntry(entry2, "VAA");
			ProcessMessageToBAE(entry1);
			ProcessMessageToBAE(entry2);

			var docSupporter = new JobDeclarationDocumentSupporterForTesting(declaration);
			CombineAssertions("WrapperDataSource should have same values as current entry headers.", () =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "11111111", "22222222", "44444444" }, docSupporter.WrapperDataSources.First(x => x.PK == entry1.PK).InvoiceLines.Select(line => line.JI_Tariff));
				AssertContainsExactElementsInAnyOrder(new[] { "33333333" }, docSupporter.WrapperDataSources.First(x => x.PK == entry2.PK).InvoiceLines.Select(line => line.JI_Tariff));
			});
		}

		public void TestWrapperDataSources_ShouldHaveSameValueAsSnapshot_WhenCurrentStatusIsRectificationSentButNoResponse()
		{
			var declaration = GetDeclarationForTest();
			var entry1 = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First(x => x.CH_SequenceNumber == 3);
			var entry2 = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First(x => x.CH_SequenceNumber == 4);

			SendMessageForEntry(entry1, "VAA");
			SendMessageForEntry(entry2, "VAA");
			ProcessMessageToBAE(entry1);
			ProcessMessageToBAE(entry2);

			entry1.InvoiceLines.First(x => x.JI_Tariff == "11111111").JI_Tariff = "88888888";
			entry2.InvoiceLines.First().JI_Tariff = "99999999";
			Factory.Save();

			SendMessageForEntry(entry1, "REC");
			SendMessageForEntry(entry2, "REC");

			var docSupporter = new JobDeclarationDocumentSupporterForTesting(declaration);
			CombineAssertions("When REC message sent but no response received yet, WrapperDataSource should use the entry values at BAE.", () =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "11111111", "22222222", "44444444" }, docSupporter.WrapperDataSources.First(x => x.PK == entry1.PK).InvoiceLines.Select(line => line.JI_Tariff));
				AssertContainsExactElementsInAnyOrder(new[] { "33333333" }, docSupporter.WrapperDataSources.First(x => x.PK == entry2.PK).InvoiceLines.Select(line => line.JI_Tariff));
			});

			var anotherFactory = new BusinessObjectFactory();
			var reloadedEntry1 = anotherFactory.Load<CusEntryHeader>(entry1.PK);
			var reloadedEntry2 = anotherFactory.Load<CusEntryHeader>(entry2.PK);
			CombineAssertions("Though entry values at BAE are used for WrapperDataSource, the entries themselves should not be reverted.", () =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "88888888", "22222222", "44444444" }, reloadedEntry1.InvoiceLines.Select(line => line.JI_Tariff));
				AssertContainsExactElementsInAnyOrder(new[] { "99999999", }, reloadedEntry2.InvoiceLines.Select(line => line.JI_Tariff));
			});
		}

		public void TestWrapperDataSources_ShouldHaveSameValueAsSnapshot_WhenCurrentStatusIsRectificationRejected()
		{
			var declaration = GetDeclarationForTest();
			var entry1 = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First(x => x.CH_SequenceNumber == 3);
			var entry2 = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First(x => x.CH_SequenceNumber == 4);

			SendMessageForEntry(entry1, "VAA");
			SendMessageForEntry(entry2, "VAA");
			ProcessMessageToBAE(entry1);
			ProcessMessageToBAE(entry2);

			entry1.InvoiceLines.First(x => x.JI_Tariff == "11111111").JI_Tariff = "88888888";
			entry2.InvoiceLines.First().JI_Tariff = "99999999";
			Factory.Save();

			SendMessageForEntry(entry1, "REC");
			SendMessageForEntry(entry2, "REC");
			ProcessMessageToBAE(entry1, "DeltaCImportBAEResponseMessageWithRefusedRectification");
			ProcessMessageToBAE(entry2, "DeltaCImportBAEResponseMessageWithRefusedRectification");

			var docSupporter = new JobDeclarationDocumentSupporterForTesting(declaration);
			CombineAssertions("When REC message is rejected, WrapperDataSource should use the entry values at BAE.", () =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "11111111", "22222222", "44444444" }, docSupporter.WrapperDataSources.First(x => x.PK == entry1.PK).InvoiceLines.Select(line => line.JI_Tariff));
				AssertContainsExactElementsInAnyOrder(new[] { "33333333" }, docSupporter.WrapperDataSources.First(x => x.PK == entry2.PK).InvoiceLines.Select(line => line.JI_Tariff));
			});

			var anotherFactory = new BusinessObjectFactory();
			var reloadedEntry1 = anotherFactory.Load<CusEntryHeader>(entry1.PK);
			var reloadedEntry2 = anotherFactory.Load<CusEntryHeader>(entry2.PK);
			CombineAssertions("Though entry values at BAE are used for WrapperDataSource, the entries themselves should not be reverted.", () =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "88888888", "22222222", "44444444" }, reloadedEntry1.InvoiceLines.Select(line => line.JI_Tariff));
				AssertContainsExactElementsInAnyOrder(new[] { "99999999", }, reloadedEntry2.InvoiceLines.Select(line => line.JI_Tariff));
			});
		}

		public void TestWrapperDataSources_ShouldHaveSameValueAsSnapshot_WhenCurrentStatusIsRectificationAccepted()
		{
			var declaration = GetDeclarationForTest();
			var entry1 = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First(x => x.CH_SequenceNumber == 3);
			var entry2 = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First(x => x.CH_SequenceNumber == 4);

			SendMessageForEntry(entry1, "VAA");
			SendMessageForEntry(entry2, "VAA");
			ProcessMessageToBAE(entry1);
			ProcessMessageToBAE(entry2);

			entry1.InvoiceLines.First(x => x.JI_Tariff == "11111111").JI_Tariff = "88888888";
			entry2.InvoiceLines.First().JI_Tariff = "99999999";
			Factory.Save();

			SendMessageForEntry(entry1, "REC");
			SendMessageForEntry(entry2, "REC");
			ProcessMessageToBAE(entry1, "DeltaCImportBAEResponseMessageWithAcceptedRectification");
			ProcessMessageToBAE(entry2, "DeltaCImportBAEResponseMessageWithAcceptedRectification");

			var docSupporter = new JobDeclarationDocumentSupporterForTesting(declaration);
			CombineAssertions("When REC message is accepted, WrapperDataSource should use the entry values at current status.", () =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "88888888", "22222222", "44444444" }, docSupporter.WrapperDataSources.First(x => x.PK == entry1.PK).InvoiceLines.Select(line => line.JI_Tariff));
				AssertContainsExactElementsInAnyOrder(new[] { "99999999" }, docSupporter.WrapperDataSources.First(x => x.PK == entry2.PK).InvoiceLines.Select(line => line.JI_Tariff));
			});
			var anotherFactory = new BusinessObjectFactory();
			var reloadedEntry1 = anotherFactory.Load<CusEntryHeader>(entry1.PK);
			var reloadedEntry2 = anotherFactory.Load<CusEntryHeader>(entry2.PK);
			CombineAssertions("Entry itself should not be reverted.", () =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "88888888", "22222222", "44444444" }, reloadedEntry1.InvoiceLines.Select(line => line.JI_Tariff));
				AssertContainsExactElementsInAnyOrder(new[] { "99999999", }, reloadedEntry2.InvoiceLines.Select(line => line.JI_Tariff));
			});
		}

		public void TestWrapperDataSources_EntryHeadersAtDifferentStatusCanBeTreatedIndividually()
		{
			var declaration = GetDeclarationForTest();
			var entry1 = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First(x => x.CH_SequenceNumber == 3);
			var entry2 = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First(x => x.CH_SequenceNumber == 4);

			SendMessageForEntry(entry1, "VAA");
			SendMessageForEntry(entry2, "VAA");
			ProcessMessageToBAE(entry1);
			ProcessMessageToBAE(entry2);

			entry1.InvoiceLines.First(x => x.JI_Tariff == "11111111").JI_Tariff = "88888888";
			entry2.InvoiceLines.First().JI_Tariff = "99999999";
			Factory.Save();

			SendMessageForEntry(entry1, "REC");
			SendMessageForEntry(entry2, "REC");
			ProcessMessageToBAE(entry1, "DeltaCImportBAEResponseMessageWithRefusedRectification");
			ProcessMessageToBAE(entry2, "DeltaCImportBAEResponseMessageWithAcceptedRectification");

			var docSupporter = new JobDeclarationDocumentSupporterForTesting(declaration);
			CombineAssertions("When REC message is accepted and rejected separately, WrapperDataSource should use the entry values at different status.", () =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "11111111", "22222222", "44444444" }, docSupporter.WrapperDataSources.First(x => x.PK == entry1.PK).InvoiceLines.Select(line => line.JI_Tariff));
				AssertContainsExactElementsInAnyOrder(new[] { "99999999" }, docSupporter.WrapperDataSources.First(x => x.PK == entry2.PK).InvoiceLines.Select(line => line.JI_Tariff));
			});
			var anotherFactory = new BusinessObjectFactory();
			var reloadedEntry1 = anotherFactory.Load<CusEntryHeader>(entry1.PK);
			var reloadedEntry2 = anotherFactory.Load<CusEntryHeader>(entry2.PK);
			CombineAssertions("Entry itself should not be reverted.", () =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "88888888", "22222222", "44444444" }, reloadedEntry1.InvoiceLines.Select(line => line.JI_Tariff));
				AssertContainsExactElementsInAnyOrder(new[] { "99999999", }, reloadedEntry2.InvoiceLines.Select(line => line.JI_Tariff));
			});
		}

		JobDeclaration GetDeclarationForTest()
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

			var entry1 = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First(entry => entry.InvoiceLines.Any(line => line.EntryInstruction.CEI_Style == "A"));
			entry1.CH_SequenceNumber = 3;
			var entry2 = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First(entry => entry.InvoiceLines.Any(line => line.EntryInstruction.CEI_Style == "F"));
			entry2.CH_SequenceNumber = 4;

			return declaration;

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
			messageBAE.EM_MessageText = resourceRetriever.Value.GetString($"Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.{responseMessageFile ?? "DeltaCImport" + "BAEResponseMessage"}.xml").Replace("0000000001", entry.CorrelationID);
			processor.ProcessMessage(messageBAE);
			entry.Declaration.ResetMessageTypeChangeLogs();
			Factory.Save();
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		public void TestGetDocumentWrappersInternal_LiquidationDetailsContext()
		{
			var menuItemForTesting = Factory.New<IStmMenuItem>();
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var documentSupporter = new JobDeclarationDocumentSupporterForTesting(declaration);
			var wrappers = documentSupporter.GetDocumentWrappersInternalExposed(DataContext.LiquidationDetails, menuItemForTesting);
			AssertNotNull(wrappers);
			AssertEquals(2, wrappers.Length);
			AssertEquals("Enterprise.Customs.FR.DocumentWrappers.LiquidationDetails.LiquidationDetailsWrapper", wrappers[0].GetType().FullName);
			AssertEquals("Enterprise.Customs.FR.DocumentWrappers.LiquidationDetails.LiquidationDetailsWrapper", wrappers[1].GetType().FullName);
			AssertSame(wrappers[0].WrappedObject, entryHeader1);
			AssertSame(wrappers[1].WrappedObject, entryHeader2);
		}

		public void TestGetDocumentWrappersInternal_SADHDataContext()
		{
			var menuItemForTesting = Factory.New<IStmMenuItem>();
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var documentSupporter = new JobDeclarationDocumentSupporterForTesting(declaration);
			var wrappers = documentSupporter.GetDocumentWrappersInternalExposed(DataContext.FRSADH, menuItemForTesting);
			AssertNotNull(wrappers);
			AssertEquals(2, wrappers.Length);
			AssertSame(wrappers[0].WrappedObject, entryHeader1);
			AssertSame(wrappers[1].WrappedObject, entryHeader2);
		}

		public void TestGetSupportedDataContexts()
		{
			var declaration = Factory.New<JobDeclaration>();
			var documentSupporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var expectedSupportedDataContext = new DataContext[] { DataContext.SADH, DataContext.FRSADH, DataContext.LiquidationDetails };

			CombineAssertions("Expected Supported DataContexts", () =>
			{
				foreach (var dataContext in expectedSupportedDataContext)
				{
					Assert($"{dataContext} should be supported", documentSupporter.IsDataContextSupported(new DataContextValueForTesting(dataContext)));
				}
			});
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclarationForTest>();
			var inv = declaration.Invoices.AddNew();
			inv.FillWithValidTestData();
			var invLine = inv.InvoiceLines.AddNew();
			invLine.FillWithValidTestData();

			var landedCostHeader = (BusinessObject)Factory.New<LandedCosting.ILandedCostHeader>();
			landedCostHeader[LandedCostHeaderSchema.LT_ParentTableCode.Name] = declaration.TablePrefix;
			landedCostHeader[LandedCostHeaderSchema.LT_ParentID.Name] = declaration.PK;

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			return declaration;
		}

		protected override Dictionary<string, int> MaxDBHitCounts
		{
			get
			{
				var maxHits = base.MaxDBHitCounts;
				maxHits["CusEntryInstruction"] = 2; // One for FR.JobDeclaration.AddDefaultEntryInstruction, other for BaseJobComInvoiceLine.IsGoingIntoBondedWarehouseForMultipleWarehouseEntry
				maxHits["CusSupportingInfo"] = 3;
				maxHits["OrgCusCode"] = 11;

				return maxHits;
			}
		}
	}

	class JobDeclarationDocumentSupporterForTesting : JobDeclarationDocumentSupporter
	{
		public JobDeclarationDocumentSupporterForTesting(JobDeclaration declaration) : base(declaration)
		{
		}

		public DocumentWrapper[] GetDocumentWrappersInternalExposed(DataContext dataContext, IStmMenuItem commandBeingRun) => GetDocumentWrappersInternal(dataContext, commandBeingRun);
	}
}
