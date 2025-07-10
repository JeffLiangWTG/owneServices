using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Export.Testing
{
	class DeclarationEletronicLogisticInvoiceProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationEletronicLogisticInvoiceProvider()
		{
			var invHeader = Factory.New<JobComInvoiceHeader>();
			invHeader.JZ_InvoiceNumber = "11111";

			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_LineNo = 1;
			var electronicLogisticInvoice = invLine.ElectronicLogisticInvoiceCollection.AddNew();
			electronicLogisticInvoice.CSI_ReferenceNumber = "11111111111111111111111111111111111111111111";
			electronicLogisticInvoice.CSI_LineNo = 1;
			electronicLogisticInvoice.CSI_Quantity = 10m;

			var dEletronicLogisticInvoice = new DeclarationEletronicLogisticInvoiceProvider(electronicLogisticInvoice);

			AssertEquals("CustomsValueAmount should be", 10m, dEletronicLogisticInvoice.CustomsQuantityRelated);
			AssertEquals("NFEItemNumber should be", (ZShort)1, dEletronicLogisticInvoice.NFEItemNumber);
			AssertEquals("NFEItemSequence should be", ZInt.Zero, dEletronicLogisticInvoice.NFEItemSequence);
			AssertEquals("NFEKey should be", "11111111111111111111111111111111111111111111", dEletronicLogisticInvoice.NFEKey);
			AssertEquals("Type should be", CusSupportingInfoTypeList.Codes.ElectronicLogisticInvoice, dEletronicLogisticInvoice.Type);
		}
	}
}
