using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.GDM;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.GDM.Testing
{
	[TestedType(typeof(GuidedDecisionMakingForm))]
	sealed class GuidedDecisionMakingFormTest : ZFormBasherTest
	{
		public void TestGDMBasicTabContainsRegionOrTerritoryOfDestinationDropEdit()
		{
			using (var form = (GuidedDecisionMakingForm)GetFormToBashCore())
			{
				AssertNotNull(form.Controls.Find("RegionOrTerritoryOfDestinationDropEdit", true).First());
			}
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var line1 = invoice.InvoiceLines.AddNew();
			line1.FillWithValidTestData();
			return new GuidedDecisionMakingForm(new GuidedDecisionMakingBasic(new GuidedDecisionMakingSingleInvoiceLineSource(line1), Factory), new GuidedDecisionMakingSingleInvoiceLineTarget(line1));
		}

		protected override bool AllowHasChangesOnFormOpen => true;
	}
}
