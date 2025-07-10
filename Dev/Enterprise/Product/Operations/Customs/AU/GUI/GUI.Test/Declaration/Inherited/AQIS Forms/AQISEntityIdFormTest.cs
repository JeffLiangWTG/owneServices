using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(AQISEntityIdForm))]
	sealed class AQISEntityIdFormTest : ZFormBasherTest
	{
		public void TestGridId()
		{
			using (var form = (AQISEntityIdForm)GetFormToBashCore())
			{
				AssertEquals("GridLayoutMZx1QjgxFru6vgbjS0WTuw==", form.zGrid1.GridId);
			}
		}

		public void TestOKButton()
		{
			using (AQISEntityIdForm form = (AQISEntityIdForm)GetFormToBashCore())
			{
				AQISEntityId entityId1 = invoiceLine.AQISEntityIds.AddNew();
				entityId1.Code = "1";
				AQISEntityId entityId2 = invoiceLine.AQISEntityIds.AddNew();
				entityId2.Code = "2";
				form.OKButton_Click(null, null);
				AssertEquals("Add Info string", false, invoiceLine.AddInfo.ZA_AQISEntityIds_Hidden.IsEmpty);
				AssertEquals("Containes Code 1", true, invoiceLine.AddInfo.ZA_AQISEntityIds_Hidden.Contains("1"));
				AssertEquals("Containes Code 2", true, invoiceLine.AddInfo.ZA_AQISEntityIds_Hidden.Contains("2"));
			}
		}

		protected override Form GetFormToBashCore()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			return new AQISEntityIdForm(invoiceLine);
		}

		JobComInvoiceLine invoiceLine;
	}
}
