using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	sealed class InvoiceLineDetailsUserControlTest : TestCase
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(EMCSJobComInvoiceLine), control.BindingSource.DataSourceType);
		}

		public void TestCommentsLongTextControl()
		{
			var commentsControl = control.CommentsLongTextControl;
			AssertEquals("CharacterCasing", CharacterCasing.Normal, commentsControl.CharacterCasing);
			AssertType<LongTextControl>("Type", commentsControl);
			AssertEquals("BindingMember", nameof(EMCSJobComInvoiceLine.JI_WineDetailsComments), commentsControl.GetBindingMember());
		}

		public void TestWineCountryOriginCodeFindBox()
		{
			AssertType<ZCalcEdit>(control.LineNoCalcEdit);
		}

		public void TestGrowingZoneDropEdit()
		{
			AssertType<ZDropEdit>(control.GrowingZoneDropEdit);
		}

		public void TestWineCategoryDropEdit()
		{
			AssertType<ZDropEdit>(control.WineCategoryDropEdit);
		}

		public void TestOperationCodesGroupBox()
		{
			CombineAssertions(() =>
			{
				var operationCodesGroupBox = control.OperationCodesGroupBox;
				AssertType<ZGroupBox>("Type", operationCodesGroupBox);
				AssertEquals("Grid inside group box", true, operationCodesGroupBox.Controls.Contains(control.OperationCodesGrid));
			});
		}

		public void TestOperationCodesGrid()
		{
			CombineAssertions(() =>
			{
				var operationCodesGrid = control.OperationCodesGrid;
				AssertEquals("DockStyle", DockStyle.Fill, operationCodesGrid.Dock);
				AssertEquals("Type column size", 35, operationCodesGrid.GetColumnStyle(nameof(WineCodeData.CY_Code)).Width);
				AssertEquals("Description column size", 210, operationCodesGrid.GetColumnStyle(nameof(WineCodeData.CY_Description)).Width);
			});
		}

		public void TestLineNoCalcEdit()
		{
			AssertType<ZCalcEdit>(control.LineNoCalcEdit);
		}

		public void TestProductCodeFindBox()
		{
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>(control.ProductCodeFindBox);
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(100, 20, true), control.ProductCodeFindBox.CodeBox.MaximumSize);
			});
		}

		public void TestExciseProductCodeDropEdit()
		{
			AssertType<ZDropEdit>(control.ExciseProductCodeDropEdit);
		}

		public void TestTariffCodeFindBox()
		{
			CombineAssertions(() =>
			{
				var tariffCodeFindBox = control.TariffCodeFindBox;
				AssertType<TariffFindBox>("Type", tariffCodeFindBox);
				AssertEquals("MaximumSize", ControlDpiScalingHelper.NewScaledSize(60, 20, true), tariffCodeFindBox.CodeBox.MaximumSize);
				AssertNull("BindToTariffPropertyInfo", control.TariffCodeFindBox.BindToTariffPropertyInfo);
			});
		}

		public void TestIsMainPackCheckBox()
		{
			AssertType<ZCheckBox>(control.IsMainPackCheckBox);
		}

		public void TestCustomsQuantityCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.CustomsQuantityCalcDropEdit);
		}

		public void TestSizeOfProducerCalcEdit()
		{
			AssertType<ZCalcEdit>(control.SizeOfProducerCalcEdit);
		}

		public void TestDensityCalcEdit()
		{
			AssertType<ZCalcEdit>(control.DensityCalcEdit);
		}

		public void TestDegreePlatoCalcEdit()
		{
			AssertType<ZCalcEdit>(control.DegreePlatoCalcEdit);
		}

		public void TestNetWeightCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.NetWeightCalcDropEdit);
		}

		public void TestWeightCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.WeightCalcDropEdit);
		}

		public void TestBrandNameTextBox()
		{
			CombineAssertions(() =>
			{
				var brandNameControl = control.BrandNameTextBox;
				AssertEquals("CharacterCasing", CharacterCasing.Normal, brandNameControl.CharacterCasing);
				AssertType<ZTextBox>("Type", brandNameControl);
			});
		}

		public void TestFiscalMarkUserControl()
		{
			AssertType<InvoiceLineFiscalMarkUserControl>(control.FiscalMarkUserControl);
		}

		public void TestAlcoholicStrengthUserControl()
		{
			AssertType<InvoiceLineAlcoholicStrengthUserControl>(control.AlcoholicStrengthUserControl);
		}

		public void TestOriginLongTextControl()
		{
			CombineAssertions(() =>
			{
				var originControl = control.OriginLongTextControl;
				AssertEquals("CharacterCasing", CharacterCasing.Normal, originControl.CharacterCasing);
				AssertType<LongTextControl>("Type", originControl);
				AssertEquals("BindingMember", nameof(EMCSJobComInvoiceLine.ZG_Origin), originControl.GetBindingMember());
			});
		}

		public void TestWineDetailsSeparatorUserControl()
		{
			CombineAssertions(() =>
			{
				var wineDetailsSeparatorControl = control.WineDetailsSeparatorUserControl;
				AssertType<SeparatorUserControl>("Type", wineDetailsSeparatorControl);
				AssertEquals("Caption", "Wine Details", wineDetailsSeparatorControl.CaptionResourceString.Caption);
			});
		}

		public void TestDescriptionLongTextControl()
		{
			CombineAssertions(() =>
			{
				var descriptionControl = control.DescriptionLongTextControl;
				AssertEquals("CharacterCasing", CharacterCasing.Normal, descriptionControl.CharacterCasing);
				AssertType<LongTextControl>("Type", descriptionControl);
				AssertEquals("BindingMember", nameof(EMCSJobComInvoiceLine.JI_NDescription), descriptionControl.GetBindingMember());
			});
		}

		public void TestMaturationPeriodOrAgeOfProductsWordWrappingTextBox()
		{
			CombineAssertions(() =>
			{
				var maturationPeriodOrAgeOfProductsWordWrappingTextBox = control.MaturationPeriodOrAgeOfProductsWordWrappingTextBox;
				AssertEquals("CharacterCasing", CharacterCasing.Normal, maturationPeriodOrAgeOfProductsWordWrappingTextBox.CharacterCasing);
				AssertEquals("Does not go over the Details Panel minimum size", 94, maturationPeriodOrAgeOfProductsWordWrappingTextBox.Height);
				AssertType<WordWrappingTextBox>("Control", maturationPeriodOrAgeOfProductsWordWrappingTextBox);
			});
		}

		public void TestIndependentSmallProducersDeclarationTextBox()
		{
			CombineAssertions(() =>
			{
				var independentSmallProducersDeclarationControl = control.IndependentSmallProducersDeclarationWordWrappingTextBox;
				AssertEquals("CharacterCasing", CharacterCasing.Normal, independentSmallProducersDeclarationControl.CharacterCasing);
				AssertEquals("Does not go over the Details Panel minimum size", 94, independentSmallProducersDeclarationControl.Height);
				AssertType<WordWrappingTextBox>("Control", independentSmallProducersDeclarationControl);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new InvoiceLineDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		InvoiceLineDetailsUserControl control;
	}
}
