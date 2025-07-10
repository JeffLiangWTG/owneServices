using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Business.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Universal.Constants.ProfileQuestion;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class DuimpLineProviderTest : TestCaseWithFactory
	{
		public void TestItemNumberAndProductCode()
		{
			var goodsCatalog1 = Factory.New<CusGoodsCatalog>();
			goodsCatalog1.CGC_AuthorityIdentifier = "TST";
			goodsCatalog1.CGC_AuthorityVersion = "5";

			var goodsCatalog2 = Factory.New<CusGoodsCatalog>();
			goodsCatalog2.CGC_AuthorityIdentifier = "10";
			goodsCatalog2.CGC_AuthorityVersion = "8";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();

			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LineNo = 1;
			invoiceLine1.JI_CGC_Catalog = goodsCatalog1.PK;

			var entryLine1 = entryHeader1.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.InvoiceLines.Add(invoiceLine1);
			var dataProvider = DuimpLineProvider.New(entryLine1);
			CombineAssertions(() =>
			{
				AssertEquals("ItemNumber", 1, dataProvider.ItemNumber);
				AssertEquals("ProductCode", 0, dataProvider.ProductCode);
				AssertEquals("ProductVersion", "5", dataProvider.ProductVersion);
			});

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			entryLine1.InvoiceLines.Add(invoiceLine2);
			dataProvider = DuimpLineProvider.New(entryLine1);
			AssertEquals("ItemNumber", 1, dataProvider.ItemNumber);

			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_LineNo = 3;
			invoiceLine3.JI_CGC_Catalog = goodsCatalog2.PK;

			var entryLine2 = entryHeader1.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.InvoiceLines.Add(invoiceLine3);

			dataProvider = DuimpLineProvider.New(entryLine2);
			CombineAssertions(() =>
			{
				AssertEquals("ItemNumber", 2, dataProvider.ItemNumber);
				AssertEquals("ProductCode", 10, dataProvider.ProductCode);
				AssertEquals("ProductVersion", "8", dataProvider.ProductVersion);
			});
		}

		public void TestProductRootCNPJ()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			goodsCatalog.CGC_AuthorityIdentifier = "TST";
			goodsCatalog.CGC_AuthorityVersion = "5";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;

			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.InvoiceLines.Add(invoiceLine);

			var dataProvider = DuimpLineProvider.New(entryLine1);
			Assert("ProductVersion", dataProvider.ProductRootCNPJ.IsEmpty());

			invoiceLine.JI_CGC_Catalog = goodsCatalog.PK;
			dataProvider = DuimpLineProvider.New(entryLine1);
			Assert("ProductVersion", dataProvider.ProductRootCNPJ.IsEmpty());

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "XXX";
			consignee.OH_FullName = "TEST COMPANY";
			consignee.PrimaryRegistrationNumber.Number = "75.400.331/0001-15";

			goodsCatalog.CGC_OH_Owner = consignee.PK;
			dataProvider = DuimpLineProvider.New(entryLine1);
			AssertEquals("ProductVersion", "75400331", dataProvider.ProductRootCNPJ);
		}

		public void TestManufacturerIndicatorCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RelatedIndicator = RelatedIndicatorList.Codes.NoBuyerSellerRelation;

			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LineNo = 1;
			invoiceLine1.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._1;

			var entryLine = entryHeader1.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine1);
			var dataProvider = DuimpLineProvider.New(entryLine);
			AssertEquals("EXPORTADOR_IGUAL_FABRICANTE", dataProvider.ManufacturerIndicatorCode);

			invoiceLine1.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;
			dataProvider = DuimpLineProvider.New(entryLine);
			AssertEquals("EXPORTADOR_DIFERENTE_FABRICANTE", dataProvider.ManufacturerIndicatorCode);

			invoiceLine1.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._3;
			dataProvider = DuimpLineProvider.New(entryLine);
			AssertEquals("EXPORTADOR_DIFERENTE_FABRICANTE", dataProvider.ManufacturerIndicatorCode);
		}

		public void TestBuyerSellerIndicatorCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RelatedIndicator = RelatedIndicatorList.Codes.NoBuyerSellerRelation;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;

			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);
			var dataProvider = DuimpLineProvider.New(entryLine);
			AssertEquals("NAO_HA_VINCULACAO", dataProvider.BuyerSellerIndicatorCode);

			invoice.JZ_RelatedIndicator = RelatedIndicatorList.Codes.BuyerSellerRelationNoInfluence;
			dataProvider = DuimpLineProvider.New(entryLine);
			AssertEquals("VINCULACAO_SEM_INFLUENCIA_PRECO", dataProvider.BuyerSellerIndicatorCode);

			invoice.JZ_RelatedIndicator = RelatedIndicatorList.Codes.BuyerSellerRelationWithInfluence;
			dataProvider = DuimpLineProvider.New(entryLine);
			AssertEquals("VINCULACAO_COM_INFLUENCIA_PRECO", dataProvider.BuyerSellerIndicatorCode);
		}

		public void TestAcquirerIndicatorAndRegistrationNumber()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.PrimaryRegistrationNumber.Number = "00.000.000/0000-01";

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.PrimaryRegistrationNumber.Number = "00.000.000/0000-02";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.OperationType = TypeOfOperationImportList.Codes.OnItsOwn;
			declaration.JE_OH_Consignee = orgHeader1.PK;

			var invoice = declaration.Invoices.AddNew();
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LineNo = 1;

			var entryLine = entryHeader1.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine1);
			var dataProvider = DuimpLineProvider.New(entryLine);
			CombineAssertions(() =>
			{
				AssertEquals("AcquirerIndicator", "IMPORTACAO_DIRETA", dataProvider.AcquirerIndicator);
				AssertEquals("AcquirerRegistrationNumber", "00000000000001", dataProvider.AcquirerRegistrationNumber);
			});

			declaration.OperationType = TypeOfOperationImportList.Codes.AccountAndOrder;
			declaration.JE_OH_Consignee = orgHeader2.PK;
			dataProvider = DuimpLineProvider.New(entryLine);
			CombineAssertions(() =>
			{
				AssertEquals("AcquirerIndicator", "IMPORTACAO_POR_CONTA_E_ORDEM", dataProvider.AcquirerIndicator);
				AssertEquals("AcquirerRegistrationNumber", "00000000000002", dataProvider.AcquirerRegistrationNumber);
			});

			declaration.OperationType = ZString.Empty;
			declaration.JE_OH_Consignee = ZGuid.Empty;
			dataProvider = DuimpLineProvider.New(entryLine);
			CombineAssertions(() =>
			{
				Assert("AcquirerIndicator should be Empty", dataProvider.AcquirerIndicator.IsEmpty());
				AssertNull("AcquirerRegistrationNumber should be null", dataProvider.AcquirerRegistrationNumber);
			});
		}

		public void TestMercosulCertificates()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RelatedIndicator = RelatedIndicatorList.Codes.NoBuyerSellerRelation;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LineNo = 1;

			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine1);
			var dataProvider = DuimpLineProvider.New(entryLine);
			AssertEquals(0, dataProvider.MercosulCertificates.Count());

			invoiceLine1.MercosulForeignDeclarations.AddNew();
			dataProvider = DuimpLineProvider.New(entryLine);
			AssertEquals(1, dataProvider.MercosulCertificates.Count());

			invoiceLine1.MercosulForeignDeclarations.AddNew();
			dataProvider = DuimpLineProvider.New(entryLine);
			AssertEquals(2, dataProvider.MercosulCertificates.Count());

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			entryLine.InvoiceLines.Add(invoiceLine2);
			dataProvider = DuimpLineProvider.New(entryLine);
			AssertEquals(2, dataProvider.MercosulCertificates.Count());

			invoiceLine2.MercosulForeignDeclarations.AddNew();
			dataProvider = DuimpLineProvider.New(entryLine);
			AssertEquals(3, dataProvider.MercosulCertificates.Count());
		}

		public void TestLinkedDocuments()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();

			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			invoiceLine1.PreviousDocuments.AddNew();
			invoiceLine1.PreviousDocuments.AddNew();
			invoiceLine2.PreviousDocuments.AddNew();
			invoiceLine2.PreviousDocuments.AddNew();

			var entryLine = entryHeader1.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine1);
			entryLine.InvoiceLines.Add(invoiceLine2);

			var dataProvider = DuimpLineProvider.New(entryLine);
			AssertEquals("LinkedDocuments count should be", 4, dataProvider.LinkedDocuments.Count());
		}

		public void TestLpcos()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();

			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			invoiceLine1.Permits.AddNew();
			invoiceLine1.Permits.AddNew();
			invoiceLine2.Permits.AddNew();
			invoiceLine2.Permits.AddNew();

			var entryLine = entryHeader1.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine1);
			entryLine.InvoiceLines.Add(invoiceLine2);

			var dataProvider = DuimpLineProvider.New(entryLine);
			AssertEquals("LinkedDocuments count should be", 4, dataProvider.Lpcos.Count());
		}

		public void TestMerchandise()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RelatedIndicator = RelatedIndicatorList.Codes.NoBuyerSellerRelation;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);
			var dataProvider = DuimpLineProvider.New(entryLine);
			AssertNotNull("Merchandise should not be null", dataProvider.Merchandise);
		}

		public void TestSellingCondition()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RelatedIndicator = RelatedIndicatorList.Codes.NoBuyerSellerRelation;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);
			var dataProvider = DuimpLineProvider.New(entryLine);
			AssertNotNull("SellingCondition should not be null", dataProvider.SellingCondition);
		}

		public void TestExchangeHedge()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);

			var dataProvider = DuimpLineProvider.New(entryLine);
			AssertNotNull("ExchangeHedge should not be null", dataProvider.ExchangeHedge);
		}

		public void TestAttributes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);

			var question1 = Factory.New<RefCusProfileQuestion>();
			question1.XQ2_AnswerDataType = AnswerDataTypes.String;
			question1.XQ2_StartDate = ZDateTime.Today.AddDays(-10);

			var question2 = Factory.New<RefCusProfileQuestion>();
			question2.XQ2_AnswerDataType = AnswerDataTypes.String;
			question2.XQ2_StartDate = ZDateTime.Today.AddDays(-10);

			var question3 = Factory.New<RefCusProfileQuestion>();
			question3.XQ2_AnswerDataType = AnswerDataTypes.String;
			question3.XQ2_StartDate = ZDateTime.Today.AddDays(10);

			var att1 = invoiceLine.Attributes.AddNew();
			att1.CY_Data = "123";
			att1.TariffProfileQuestion = TariffProfileQuestion.New(question1);

			var dataProvider = DuimpLineProvider.New(entryLine);
			AssertNotNull("Attributes should not be null", dataProvider.Attributes);

			var att2 = invoiceLine.Attributes.AddNew();
			att2.CY_Data = "456";
			att2.TariffProfileQuestion = TariffProfileQuestion.New(question2);

			dataProvider = DuimpLineProvider.New(entryLine);
			AssertEquals("Attributes count should be", 2, dataProvider.Attributes.Count());

			var att3 = invoiceLine.Attributes.AddNew();
			att3.CY_Data = "789";
			att3.TariffProfileQuestion = TariffProfileQuestion.New(question3);

			dataProvider = DuimpLineProvider.New(entryLine);
			AssertEquals("Attributes count should be", 2, dataProvider.Attributes.Count());
		}

		public void TestExporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);

			var dataProvider = DuimpLineProvider.New(entryLine);
			AssertNotNull("Exporter should not be null", dataProvider.Exporter);
		}

		public void TestManufacturer()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);

			var dataProvider = DuimpLineProvider.New(entryLine);

			AssertNotNull("Manufacturer should not be null", dataProvider.Manufacturer);
		}

		public void TestLegalBasisAttributes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);

			var dataProvider = DuimpLineProvider.New(entryLine);
			AssertNull("LegalBasisAttributes should not be null", dataProvider.LegalBasisAttributes);
		}

		public void TestTaxes()
		{
			ReferenceTestDataHelper.CreateTTRefCusProfileAndQuestions(Factory);
			ReferenceTestDataHelper.CreateDuimpLegalBaseCodes(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "12345678";
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "12345678";
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine1);
			entryLine.InvoiceLines.Add(invoiceLine2);

			invoiceLine1.DuimpLegalBase = "P01";
			invoiceLine1.AddDuimpTaxRegimes();
			invoiceLine2.DuimpLegalBase = "P02";
			invoiceLine2.AddDuimpTaxRegimes();

			var dataProvider = DuimpLineProvider.New(entryLine);
			AssertEquals("Taxes count should be", 3, dataProvider.Taxes.Count());
			AssertContainsExactElementsInAnyOrder([1, 1, 2], dataProvider.Taxes.Select(x => x.LegalBasisCode));
		}
	}
}
