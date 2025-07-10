using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ImportIPRInventorySelectionHeader))]
	sealed class ImportIPRInventorySelectionHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFillInvoiceLineWithInventoryDetails_GroupedByInvoiceNumberAndDate()
		{
			var attribute1 = new Mock<IWhsBondedWarehouseAttribute>();
			attribute1.Setup(x => x.WB_AddInfo).Returns("IncotermCode=FOB*LinePriceCurrency=USD*IncotermPlace=Frankfurt*TransNature=21*InvoiceNumber=12345*InvoiceDate=01-Oct-24");
			var attribute2 = new Mock<IWhsBondedWarehouseAttribute>();
			attribute2.Setup(x => x.WB_AddInfo).Returns("IncotermCode=DAP*LinePriceCurrency=EUR*IncotermPlace=Hamburg*TransNature=20*InvoiceNumber=12345*InvoiceDate=01-Oct-24");
			var attribute3 = new Mock<IWhsBondedWarehouseAttribute>();
			attribute3.Setup(x => x.WB_AddInfo).Returns("IncotermCode=DAP*LinePriceCurrency=EUR*IncotermPlace=Hamburg*TransNature=20*InvoiceNumber=12346*InvoiceDate=01-Oct-24");
			var attribute4 = new Mock<IWhsBondedWarehouseAttribute>();
			attribute4.Setup(x => x.WB_AddInfo).Returns("IncotermCode=DAP*LinePriceCurrency=EUR*IncotermPlace=Hamburg*TransNature=20*InvoiceNumber=12345*InvoiceDate=02-Oct-24");

			var inventorySelectionHeader = new ImportInventorySelectionHeaderForTest(declaration, createDeclarationBizObj);
			AssertEquals("Prerequisite - AVABR type", ImportDeclarationTypeList.Codes.AVABR, createDeclarationBizObj.DeclarationType);
			AssertEquals("Prerequisite - No invoice headers", 0, declaration.Invoices.Count);

			var invHeader1 = inventorySelectionHeader.CallGetFirstOrCreateNewInvoiceHeader(attribute1.Object);
			var invHeader2 = inventorySelectionHeader.CallGetFirstOrCreateNewInvoiceHeader(attribute2.Object);
			inventorySelectionHeader.CallGetFirstOrCreateNewInvoiceHeader(attribute3.Object);
			inventorySelectionHeader.CallGetFirstOrCreateNewInvoiceHeader(attribute4.Object);

			CombineAssertions(() =>
			{
				AssertEquals("3 invoice headers", 3, declaration.Invoices.Count);
				AssertSame("invHeader1 and invHeader2 the same object", invHeader1, invHeader2);

				AssertEquals("12345", invHeader1.JZ_InvoiceNumber);
				AssertEquals(new ZDateTime(2024, 10, 1), invHeader1.JZ_InvoiceDate);
				AssertEquals("EUR", invHeader1.JZ_RX_NKInvoice_Currency);
				AssertEquals("DAP", invHeader1.JZ_IncoTerm);
				AssertEquals("Hamburg", invHeader1.JZ_IncoTermPlace);
				AssertEquals("20", invHeader1.JZ_ValuationCode);
			});
		}

		public void TestFillInvoiceLineWithInventoryDetails_PricesFilled_AVABR()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var attribute = new Mock<IWhsBondedWarehouseAttribute>();
			attribute.Setup(x => x.WB_ValueForDuty).Returns(1000m);
			attribute.Setup(x => x.WB_AddInfo).Returns("LinePriceCurrency=EUR*LinePrice=500*LineNetPrice=400");

			var whsReceiveLine = Factory.New<WhsReceiveLine>();
			var ratio = 0.3m;
			var inventorySelectionHeader = new ImportInventorySelectionHeaderForTest(declaration, createDeclarationBizObj);
			AssertEquals("Prerequisite - AVABR type", ImportDeclarationTypeList.Codes.AVABR, createDeclarationBizObj.DeclarationType);
			inventorySelectionHeader.CallFillInvoiceLineWithInventoryDetails(invoiceLine, whsReceiveLine, attribute.Object, ratio);
			CombineAssertions(() =>
			{
				AssertEquals("JI_LinePrice", 300m, invoiceLine.JI_LinePrice);
				AssertEquals("JI_NetPrice", 300m, invoiceLine.JI_NetPrice);
			});
		}

		public void TestFillInvoiceLineWithInventoryDetails_PricesFilled_non_AVABR()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var attribute = new Mock<IWhsBondedWarehouseAttribute>();
			attribute.Setup(x => x.WB_ValueForDuty).Returns(1000m);
			attribute.Setup(x => x.WB_AddInfo).Returns("LinePriceCurrency=EUR*LinePrice=500*LineNetPrice=400");

			var whsReceiveLine = Factory.New<WhsReceiveLine>();
			var ratio = 0.3m;
			createDeclarationBizObj.DeclarationType = ImportDeclarationTypeList.Codes.EZA;
			var inventorySelectionHeader = new ImportInventorySelectionHeaderForTest(declaration, createDeclarationBizObj);
			AssertNotEquals("Prerequisite - non AVABR type", ImportDeclarationTypeList.Codes.AVABR, createDeclarationBizObj.DeclarationType);
			inventorySelectionHeader.CallFillInvoiceLineWithInventoryDetails(invoiceLine, whsReceiveLine, attribute.Object, ratio);
			CombineAssertions(() =>
			{
				AssertEquals("JI_LinePrice", 150m, invoiceLine.JI_LinePrice);
				AssertEquals("JI_NetPrice", 120m, invoiceLine.JI_NetPrice);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new WhsDataTestHelper(Factory);
			declaration = CreateImportJobDeclaration();
			createDeclarationBizObj = new CreateDeclarationIPR();
		}
		JobDeclaration declaration;
		CreateDeclarationIPR createDeclarationBizObj;
		WhsDataTestHelper helper;

		protected override BusinessObject GetNewBusinessObject() => new ImportIPRInventorySelectionHeader(declaration, new CreateDeclarationIPR());

		JobDeclaration CreateImportJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = helper.Importer.PK;
			declaration.WarehouseDocAddress.E2_OA_Address = helper.Warehouse.MainAddress.PK;

			return declaration;
		}

		class ImportInventorySelectionHeaderForTest : ImportIPRInventorySelectionHeader
		{
			public ImportInventorySelectionHeaderForTest(JobDeclaration declaration, CreateDeclarationBizObj createDeclarationBizObj) : base(declaration, createDeclarationBizObj)
			{
			}

			public BaseJobComInvoiceHeader CallGetFirstOrCreateNewInvoiceHeader(IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute) => GetFirstOrCreateNewInvoiceHeader(whsBondedWarehouseAttribute);
			public void CallFillInvoiceLineWithInventoryDetails(EU.Business.Declaration.JobComInvoiceLine invoiceLine, IWhsDocketLine whsReceiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal ratio)
				=> FillInvoiceLineWithInventoryDetails(invoiceLine, whsReceiveLine, whsBondedWarehouseAttribute, ratio);
		}
	}
}
