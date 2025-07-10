using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.ImportLicense.Testing
{
	public class ConsentingProcessProviderTest : TestCaseWithFactory
	{
		public void TestConsentingProcessProvider()
		{
			var oDeclaration = Factory.New<JobDeclaration>();
			oDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var oInvoice = oDeclaration.Invoices.AddNew();
			oInvoice.JZ_IncoTerm = "CIF";

			var line = oInvoice.InvoiceLines.AddNew();
			line.JI_LineNo = 1;

			var consProcess = line.ConsentingProcessCollection.AddNew();
			consProcess.CSI_ReferenceNumber = "11-58";
			consProcess.CSI_CustomsOffice = "ANVISA";

			var provider = ConsentingProcessProvider.New(consProcess);

			AssertEquals("ReferenceNumber should be", "11-58", provider.ReferenceNumber);
			AssertEquals("ReferenceType should be", "ANVISA", provider.ReferenceType);
		}
	}
}
