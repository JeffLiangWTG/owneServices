using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Export.Testing
{
	class DeclarationComplementaryLogisticInvoiceProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationComplementaryLogisticInvoiceProvider()
		{
			var oTestSupplier = Factory.New<OrgHeader>();
			oTestSupplier.PrimaryRegistrationNumber.Number = "58500398000105";
			var oInvoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			oInvoiceLine.InvoiceHeader.JZ_OA_SupplierAddress = oTestSupplier.MainAddress.PK;
			var cLogisticInvoice = oInvoiceLine.ComplementaryLogisticInvoiceCollection.AddNew();
			cLogisticInvoice.CSI_ReferenceNumber = "35170658500398000105550010001023631156448633";
			cLogisticInvoice.CSI_LineNo = 1;

			var dComplementaryLogisticInvoice = new DeclarationComplementaryLogisticInvoiceProvider(cLogisticInvoice);

			AssertEquals("CustomsValueAmount should be", ZDecimal.Zero, dComplementaryLogisticInvoice.CustomsQuantityRelated);
			AssertEquals("NFEItemNumber should be", (ZShort)1, dComplementaryLogisticInvoice.NFEItemNumber);
			AssertEquals("NFEItemSequence should be", ZInt.Zero, dComplementaryLogisticInvoice.NFEItemSequence);
			AssertEquals("NFEKey should be", "35170658500398000105550010001023631156448633", dComplementaryLogisticInvoice.NFEKey);
			AssertEquals("Type should be", CusSupportingInfoTypeList.Codes.ComplementaryLogisticInvoice, dComplementaryLogisticInvoice.Type);
		}
	}
}
