using System.Windows.Forms;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(CalculateFreightForm))]
	class CalculateFreightFormTest : ZFormBasherTest
	{
		public void TestIsFreightIncludedInLinesNotVisible()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var isFreightIncludedInLinesCheckBox = form.FindSingleOrDefault<ZCheckBox>("zCheckBoxIsFreightIncludedInLines");
				AssertEquals("Form doesn't have zCheckBoxIsFreightIncludedInLines", null, isFreightIncludedInLinesCheckBox);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var bizobj = EU.Business.Declaration.CalculateFreightBizObj.New(invoice.Charges, declaration);
			bizobj.HasChanges = false;
			return new CalculateFreightForm(bizobj);
		}
	}
}
