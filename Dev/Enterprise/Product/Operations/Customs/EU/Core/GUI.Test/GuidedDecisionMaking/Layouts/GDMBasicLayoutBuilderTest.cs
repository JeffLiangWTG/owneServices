using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(GDMBasicLayoutBuilder<GuidedDecisionMakingBasic>))]
	sealed class GDMBasicLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<GDMBasicLayoutBuilder<GuidedDecisionMakingBasic>, GuidedDecisionMakingBasic, GDMBasicControlBag>
	{
		protected override GDMBasicLayoutBuilder<GuidedDecisionMakingBasic> GetColumnLayoutBuilderForTesting() => new GDMBasicLayoutBuilder<GuidedDecisionMakingBasic>();

		protected override int ExpectedMaxColumns => 1;

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		public void TestPreferenceDropEdit_VisibleWhenImport()
		{
			gdmBasic.IsImport = true;
			var layout = ((IPanelLayoutProvider)new GDMBasicLayout()).Layout;
			AssertEquals(true, layout.IsVisible(GDMBasicControlBag.Instance.PreferenceDropEdit, gdmBasic));
		}

		public void TestPreferenceDropEdit_InvisibleWhenNotImport()
		{
			gdmBasic.IsImport = false;
			var layout = ((IPanelLayoutProvider)new GDMBasicLayout()).Layout;
			AssertEquals(false, layout.IsVisible(GDMBasicControlBag.Instance.PreferenceDropEdit, gdmBasic));
		}

		public void TestQuotaOrderNumberDropEdit_VisibleWhenImport()
		{
			gdmBasic.IsImport = true;
			var layout = ((IPanelLayoutProvider)new GDMBasicLayout()).Layout;
			AssertEquals(true, layout.IsVisible(GDMBasicControlBag.Instance.QuotaOrderNumberDropEdit, gdmBasic));
		}

		public void TestQuotaOrderNumberDropEdit_InvisibleWhenNotImport()
		{
			gdmBasic.IsImport = false;
			var layout = ((IPanelLayoutProvider)new GDMBasicLayout()).Layout;
			AssertEquals(false, layout.IsVisible(GDMBasicControlBag.Instance.QuotaOrderNumberDropEdit, gdmBasic));
		}

		public void TestCountryOfOriginDropEdit_InvisibleWhenNotImport()
		{
			gdmBasic.IsImport = false;
			var layout = ((IPanelLayoutProvider)new GDMBasicLayout()).Layout;
			AssertEquals(false, layout.IsVisible(GDMBasicControlBag.Instance.CountryOfOriginDropEdit, gdmBasic));
		}

		public void TestCountryOfOriginDropEdit_VisibleWhenImport()
		{
			gdmBasic.IsImport = true;
			var layout = ((IPanelLayoutProvider)new GDMBasicLayout()).Layout;
			AssertEquals(true, layout.IsVisible(GDMBasicControlBag.Instance.CountryOfOriginDropEdit, gdmBasic));
		}

		public void TestCountryOfDestinationDropEdit_InvisibleWhenNotExport()
		{
			gdmBasic.IsExport = false;
			var layout = ((IPanelLayoutProvider)new GDMBasicLayout()).Layout;
			AssertEquals(false, layout.IsVisible(GDMBasicControlBag.Instance.CountryOfDestinationDropEdit, gdmBasic));
		}

		public void TestCountryOfDestinationDropEdit_VisibleWhenExport()
		{
			gdmBasic.IsExport = true;
			var layout = ((IPanelLayoutProvider)new GDMBasicLayout()).Layout;
			AssertEquals(true, layout.IsVisible(GDMBasicControlBag.Instance.CountryOfDestinationDropEdit, gdmBasic));
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			gdmBasic = invoiceLine.GetGuidedDecisionMakingBasic();
		}

		GuidedDecisionMakingBasic gdmBasic;
	}
}
