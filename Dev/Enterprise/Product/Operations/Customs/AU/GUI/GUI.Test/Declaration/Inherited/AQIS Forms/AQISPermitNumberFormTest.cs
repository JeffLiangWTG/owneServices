using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(AQISPermitNumberForm))]
	sealed class AQISPermitNumberFormTest : ZFormBasherTest
	{
		public void TestGridId()
		{
			using (var form = (AQISPermitNumberForm)GetFormToBashCore())
			{
				AssertEquals("GridLayoutTLMvr7AiGn/fR12HLjl/WQ==", form.zGrid1.GridId);
			}
		}

		public void TestOKButton()
		{
			using (AQISPermitNumberForm form = (AQISPermitNumberForm)GetFormToBashCore())
			{
				AQISPermitId permitId1 = invoiceLine.AQISPermitIds.AddNew();
				permitId1.Code = "1";
				AQISPermitId permitId2 = invoiceLine.AQISPermitIds.AddNew();
				permitId2.Code = "2";
				form.OKButton_Click(null, null);
				AssertEquals("Add Info string", false, invoiceLine.AddInfo.ZA_AQISPermitIds_Hidden.IsEmpty);
				AssertEquals("Containes Code 1", true, invoiceLine.AddInfo.ZA_AQISPermitIds_Hidden.Contains("1"));
				AssertEquals("Containes Code 2", true, invoiceLine.AddInfo.ZA_AQISPermitIds_Hidden.Contains("2"));
			}
		}

		protected override Form GetFormToBashCore()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			return new AQISPermitNumberForm(invoiceLine);
		}

		JobComInvoiceLine invoiceLine;
	}
}
