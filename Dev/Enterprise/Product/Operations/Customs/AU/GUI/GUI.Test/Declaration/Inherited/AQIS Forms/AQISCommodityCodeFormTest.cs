using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(AQISCommodityCodeForm))]
	sealed class AQISCommodityCodeFormTest : ZFormBasherTest
	{
		public void TestGridId()
		{
			using (var form = (AQISCommodityCodeForm)GetFormToBashCore())
			{
				AssertEquals("GridLayoutcg+K4aS7mI14HUJQMhx++A==", form.zGrid1.GridId);
			}
		}

		public void TestOKButton()
		{
			using (AQISCommodityCodeForm form = (AQISCommodityCodeForm)GetFormToBashCore())
			{
				AQISCommodityCode commodityCode1 = invoiceLine.AQISCommodityCodes.AddNew();
				commodityCode1.Code = "1";
				AQISCommodityCode commodityCode2 = invoiceLine.AQISCommodityCodes.AddNew();
				commodityCode2.Code = "2";
				form.OKButton_Click(null, null);
				AssertEquals("Add Info string", false, invoiceLine.AddInfo.ZA_AQISCommCodes_Hidden.IsEmpty);
				AssertEquals("Containes Code 1", true, invoiceLine.AddInfo.ZA_AQISCommCodes_Hidden.Contains("1"));
				AssertEquals("Containes Code 2", true, invoiceLine.AddInfo.ZA_AQISCommCodes_Hidden.Contains("2"));
			}
		}

		protected override Form GetFormToBashCore()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			return new AQISCommodityCodeForm(invoiceLine);
		}

		JobComInvoiceLine invoiceLine;
	}
}
