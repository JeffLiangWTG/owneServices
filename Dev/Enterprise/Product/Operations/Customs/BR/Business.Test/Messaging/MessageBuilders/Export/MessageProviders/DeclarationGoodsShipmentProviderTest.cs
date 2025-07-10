using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Export.Testing
{
	class DeclarationGoodsShipmentProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationGoodsShipmentExporter()
		{
			OrgHeader oSupplier = Factory.New<OrgHeader>();
			oSupplier.PrimaryRegistrationNumber.Number = "58.500.398/0001-05";
			oSupplier.OH_FullName = "ORGANIZATION BR";
			oSupplier.MainAddress.OA_RN_NKCountryCode = "BR";
			oSupplier.MainAddress.OA_State = "SP";
			oSupplier.MainAddress.Address1 = "PAULISTA AVENUE";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = oSupplier.PK;

			var cusEntryHeader = Factory.New<CusEntryHeader>();
			cusEntryHeader.CH_JE = declaration.PK;

			var cusEntryLine = Factory.New<CusEntryLine>();
			cusEntryLine.CL_CH = cusEntryHeader.PK;

			var goodsShipment = new DeclarationGoodsShipmentProvider(cusEntryLine);

			AssertEquals("Declarant ID should be", "58500398000105", goodsShipment.Exporter.ID);
		}

		public void TestDeclarationGoodsShipmentImporter()
		{
			OrgHeader oImporter = Factory.New<OrgHeader>();
			oImporter.OH_FullName = "ORGANIZATION ABROAD";
			oImporter.MainAddress.OA_RN_NKCountryCode = "US";
			oImporter.MainAddress.Address1 = "FIFTH AVENUE";

			var declaration = Factory.New<JobDeclaration>();

			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.JZ_JE = declaration.PK;
			invoiceHeader.JZ_OH_Buyer = oImporter.PK;

			var cusEntryHeader = Factory.New<CusEntryHeader>();
			cusEntryHeader.CH_JE = declaration.PK;

			var cusEntryLine = Factory.New<CusEntryLine>();
			cusEntryLine.CL_CH = cusEntryHeader.PK;
			var invoiceLine = cusEntryLine.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoiceHeader.PK;

			var goodsShipment = new DeclarationGoodsShipmentProvider(cusEntryLine);

			AssertEquals("CountryCode should be", "US", goodsShipment.Importer.CountryCode);
			AssertEquals("ID should be", ZString.Empty, goodsShipment.Importer.ID);
			AssertEquals("Name should be", "ORGANIZATION ABROAD", goodsShipment.Importer.Name);
		}

		public void TestGovernmentAgencyGoodsItemSequenceNumeric()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;

			var cusEntryHeader = Factory.New<CusEntryHeader>();
			cusEntryHeader.CH_JE = declaration.PK;

			var cusEntryLine = Factory.New<CusEntryLine>();
			cusEntryLine.CL_CH = cusEntryHeader.PK;
			cusEntryLine.CL_LineNumber = 1;

			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "111111";

			var invLine = invoice.InvoiceLines.AddNew();
			invLine.JI_CEI = instruction.PK;
			invLine.JI_CL = cusEntryLine.PK;

			var goodsShipment = new DeclarationGoodsShipmentProvider(cusEntryLine);

			AssertEquals("Sequence Number should be", 1, goodsShipment.GovernmentAgencyGoodsItem.SequenceNumeric);
		}

		public void TestNfeInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;

			var cusEntryHeader = Factory.New<CusEntryHeader>();
			cusEntryHeader.CH_JE = declaration.PK;

			var cusEntryLine = Factory.New<CusEntryLine>();
			cusEntryLine.CL_CH = cusEntryHeader.PK;

			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "111111";

			var invLine = invoice.InvoiceLines.AddNew();
			invLine.JI_CEI = instruction.PK;
			invLine.JI_CL = cusEntryLine.PK;
			invLine.JI_LineNo = 1;
			invLine.JI_NFeNumber = "00000000000000000000000000000000000000000000";
			invLine.JI_NFeItemNumber = "1";

			var goodsShipment = new DeclarationGoodsShipmentProvider(cusEntryLine);

			AssertEquals("NFEItemNumber should be", (ZShort)1, goodsShipment.Invoice.NFEItemNumber);
			AssertEquals("NFEKey should be", "00000000000000000000000000000000000000000000", goodsShipment.Invoice.NFEKey);
			AssertEquals("NFEItemSequence should be", ZInt.Zero, goodsShipment.Invoice.NFEItemSequence);
			AssertEquals("CustomsQuantityRelated should be", ZDecimal.Zero, goodsShipment.Invoice.CustomsQuantityRelated);
		}

		public void TestIncoterm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;

			var cusEntryHeader = Factory.New<CusEntryHeader>();
			cusEntryHeader.CH_JE = declaration.PK;

			var cusEntryLine = Factory.New<CusEntryLine>();
			cusEntryLine.CL_CH = cusEntryHeader.PK;

			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "111111";
			invoice.JZ_IncoTerm = BRIncoTermList.Codes.CFR;

			var invLine = invoice.InvoiceLines.AddNew();
			invLine.JI_CEI = instruction.PK;
			invLine.JI_CL = cusEntryLine.PK;
			invLine.JI_LineNo = 1;

			var goodsShipment = new DeclarationGoodsShipmentProvider(cusEntryLine);

			AssertEquals("IncotermCode should be", BRIncoTermList.Codes.CFR, goodsShipment.IncotermCode);
		}
	}
}
