using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class ValuationDetailsUserControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestBindingMembers()
		{
			AssertEquals("ProductCodeCodeFindBox Binding", "FilteredInvoiceLines.JI_PartNo", control.FindSingle<ZCodeFindBox>("ProductCodeCodeFindBox").BindTo);
			AssertEquals("TariffFindBox Binding", "FilteredInvoiceLines.JI_FormattedTariff", control.FindSingle<TariffFindBox>("TariffFindBox").BindTo);
			AssertNotNull("DescriptionLongTextControl Binding", control.FindSingle<Customs.GUI.LongTextControl>("DescriptionLongTextControl"));
			AssertEquals("ModelTradeNameTextBox Binding", "FilteredInvoiceLines.JI_Model", control.FindSingle<ZTextBox>("ModelTradeNameTextBox").BindTo);
			AssertEquals("BrandNameTextBox Binding", "FilteredInvoiceLines.JI_BrandName", control.FindSingle<ZTextBox>("BrandNameTextBox").BindTo);
			AssertNotNull("IngredientLongTextControl Binding", control.FindSingle<Customs.GUI.LongTextControl>("IngredientLongTextControl"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ValuationDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ValuationDetailsUserControl control;
	}
}
