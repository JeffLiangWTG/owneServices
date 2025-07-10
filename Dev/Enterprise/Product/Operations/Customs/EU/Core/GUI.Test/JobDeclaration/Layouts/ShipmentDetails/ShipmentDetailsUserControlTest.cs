using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class ShipmentDetailsUserControlTest : TestCase
	{
		public void TestDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestCaptionRenderingEnabled()
		{
			AssertEquals(true, control.CaptionRenderingEnabled);
		}

		public void TestGoodsLocationDropEdit()
		{
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", control.GoodsLocationDropEdit);
				AssertEquals("BindingMember", "JE_LocationOfGoods", control.GoodsLocationDropEdit.GetBindingMember());
			});
		}

		public void TestAgentsReferenceTextBox()
		{
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", control.AgentsReferenceTextBox);
				AssertEquals("BindingMember", "JE_AgentsReference", control.AgentsReferenceTextBox.GetBindingMember());
			});
		}

		public void TestUCRTextBox()
		{
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", control.UCRTextBox);
				AssertEquals("BindingMember", "JE_UCR", control.UCRTextBox.GetBindingMember());
			});
		}

		public void TestShipmentDetailsQuantitiesUserControl()
		{
			CombineAssertions(() =>
			{
				AssertType<ShipmentDetailsQuantitiesUserControl>("Type", control.ShipmentDetailsQuantitiesUserControl);
				AssertEquals("BindingMember", ".", control.ShipmentDetailsQuantitiesUserControl.GetBindingMember());
			});
		}

		public void TestShipmentDetailsCountUserControl()
		{
			CombineAssertions(() =>
			{
				AssertType<ShipmentDetailsCountUserControl>("Type", control.ShipmentDetailsCountUserControl);
				AssertEquals("BindingMember", ".", control.ShipmentDetailsCountUserControl.GetBindingMember());
			});
		}

		public void TestShipmentDetailsIncoTermsPlaceUserControl()
		{
			CombineAssertions(() =>
			{
				AssertType<ShipmentDetailsIncoTermsPlaceUserControl>("Type", control.ShipmentDetailsIncoTermsPlaceUserControl);
				AssertEquals("BindingMember", ".", control.ShipmentDetailsIncoTermsPlaceUserControl.GetBindingMember());
			});
		}

		public void TestAgreedPlaceCodeFindBox()
		{
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", control.AgreedPlaceCodeFindBox);
				AssertEquals("BindingMember", "ZG_AgreedPlaceCode", control.AgreedPlaceCodeFindBox.GetBindingMember());
			});
		}

		public void TestShipmentIncoTermPlaceTextBox()
		{
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", control.ShipmentIncoTermPlaceTextBox);
				AssertEquals("BindingMember", "JE_ShipmentIncoTermPlace", control.ShipmentIncoTermPlaceTextBox.GetBindingMember());
			});
		}

		public void TestRegionOfDestinationDropEdit()
		{
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", control.RegionOfDestinationDropEdit);
				AssertEquals("BindingMember", nameof(JobDeclaration.ZG_RegionOfDestination), control.RegionOfDestinationDropEdit.GetBindingMember());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ShipmentDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ShipmentDetailsUserControl control;
	}
}
