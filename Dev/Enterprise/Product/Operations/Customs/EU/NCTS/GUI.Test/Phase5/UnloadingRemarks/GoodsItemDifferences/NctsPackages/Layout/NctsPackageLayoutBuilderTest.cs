using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(NctsPackageLayoutBuilder<NctsPackage>))]
	sealed class NctsPackageLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<NctsPackageLayoutBuilder<NctsPackage>, NctsPackage, NctsPackageControlBag>
	{
		public void TestUnloadingDifferencesVisibility()
		{
			CombineAssertions(() =>
			{
				AssertEquals("DifUnitCountCalcEdit Not Visible, if Unloaded State = NEW", false, Layout.IsVisible(NctsPackageControlBag.Instance.DifUnitCountCalcEdit, package));
				AssertEquals("DifUnitTypeDropEdit Not Visible, if Unloaded State = NEW", false, Layout.IsVisible(NctsPackageControlBag.Instance.DifUnitTypeDropEdit, package));
				AssertEquals("DifMarksAndNumbersTextBox Not Visible, if Unloaded State = NEW", false, Layout.IsVisible(NctsPackageControlBag.Instance.DifMarksAndNumbersTextBox, package));
				AssertEquals("UnloadedValueLabel Not Visible, if Unloaded State = NEW", false, Layout.IsVisible(NctsPackageControlBag.Instance.UnloadedValueLabel, package));
				AssertEquals("DifPackageIDTextBox Not Visible, if Unloaded State = NEW", false, Layout.IsVisible(NctsPackageControlBag.Instance.DifPackageIDTextBox, package));
				AssertEquals("DifBrandTextBox Not Visible, if Unloaded State = NEW", false, Layout.IsVisible(NctsPackageControlBag.Instance.DifBrandTextBox, package));
				AssertEquals("DifModelTextBox Not Visible, if Unloaded State = NEW", false, Layout.IsVisible(NctsPackageControlBag.Instance.DifModelTextBox, package));

				package.B5_TypeOfDifference = "DIF";
				AssertEquals("DifUnitCountCalcEdit Visible, if Unloaded State = DIF", true, Layout.IsVisible(NctsPackageControlBag.Instance.DifUnitCountCalcEdit, package));
				AssertEquals("DifUnitTypeDropEdit Visible, if Unloaded State = DIF", true, Layout.IsVisible(NctsPackageControlBag.Instance.DifUnitTypeDropEdit, package));
				AssertEquals("DifMarksAndNumbersTextBox Visible, if Unloaded State = DIF", true, Layout.IsVisible(NctsPackageControlBag.Instance.DifMarksAndNumbersTextBox, package));
				AssertEquals("UnloadedValueLabel Visible, if Unloaded State = DIF", true, Layout.IsVisible(NctsPackageControlBag.Instance.UnloadedValueLabel, package));
				AssertEquals("DifPackageIDTextBox Visible, if Unloaded State = DIF", true, Layout.IsVisible(NctsPackageControlBag.Instance.DifPackageIDTextBox, package));
				AssertEquals("DifBrandTextBox Visible, if Unloaded State = DIF", true, Layout.IsVisible(NctsPackageControlBag.Instance.DifBrandTextBox, package));
				AssertEquals("DifModelTextBox Visible, if Unloaded State = DIF", true, Layout.IsVisible(NctsPackageControlBag.Instance.DifModelTextBox, package));

				package.B5_TypeOfDifference = "DEC";
				AssertEquals("DifUnitCountCalcEdit Not Visible, if Unloaded State = DEC", false, Layout.IsVisible(NctsPackageControlBag.Instance.DifUnitCountCalcEdit, package));
				AssertEquals("DifUnitTypeDropEdit Not Visible, if Unloaded State = DEC", false, Layout.IsVisible(NctsPackageControlBag.Instance.DifUnitTypeDropEdit, package));
				AssertEquals("DifMarksAndNumbersTextBox Not Visible, if Unloaded State = DEC", false, Layout.IsVisible(NctsPackageControlBag.Instance.DifMarksAndNumbersTextBox, package));
				AssertEquals("UnloadedValueLabel Not Visible, if Unloaded State = DEC", false, Layout.IsVisible(NctsPackageControlBag.Instance.UnloadedValueLabel, package));
				AssertEquals("DifPackageIDTextBox Not Visible, if Unloaded State = DEC", false, Layout.IsVisible(NctsPackageControlBag.Instance.DifPackageIDTextBox, package));
				AssertEquals("DifBrandTextBox Not Visible, if Unloaded State = DEC", false, Layout.IsVisible(NctsPackageControlBag.Instance.DifBrandTextBox, package));
				AssertEquals("DifModelTextBox Not Visible, if Unloaded State = DEC", false, Layout.IsVisible(NctsPackageControlBag.Instance.DifModelTextBox, package));
			});
		}

		public void TestStatusLabelCaption()
		{
			var testPackage = goodsItem.Packages.AddNew();
			var control = NctsPackageControlBag.Instance.DeclaredValueLabel;

			CombineAssertions(() =>
			{
				Layout.TryGetCaption(control, testPackage, out var resourceStringData);
				AssertEquals("B5_TypeOfDifference = 'NEW'", "New Value", resourceStringData.Caption);

				testPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
				Layout.TryGetCaption(control, testPackage, out resourceStringData);
				AssertEquals("B5_TypeOfDifference = 'DEC'", "Declared Value", resourceStringData.Caption);

				testPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
				Layout.TryGetCaption(control, testPackage, out resourceStringData);
				AssertEquals("B5_TypeOfDifference = 'DIF'", "Declared Value", resourceStringData.Caption);

				testPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
				Layout.TryGetCaption(control, testPackage, out resourceStringData);
				AssertEquals("B5_TypeOfDifference = 'MIS'", "Declared Value", resourceStringData.Caption);
			});
		}

		protected override int ExpectedMaxColumns => 2;

		protected override NctsPackageLayoutBuilder<NctsPackage> GetColumnLayoutBuilderForTesting() => new NctsPackageLayoutBuilder<NctsPackage>();

		protected override void SetUp()
		{
			base.SetUp();

			var nctsHeader = Factory.New<NctsHeaderForTest>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			goodsItem = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();

			package = Factory.New<NctsPackage>();
			package.B5_ParentTableCode = goodsItem.TablePrefix;
			package.B5_ParentID = goodsItem.PK;
		}
		NctsPackage package;
		NctsArrivalCargoDesc goodsItem;

		public PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new NctsPackageLayoutWithGrid()).Layout);
		PanelLayout layout;
	}
}
