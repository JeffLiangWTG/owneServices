using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	sealed class SupplyChainActorTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>), userControl.BindingSource.DataSourceType);
		}

		public void TestControls()
		{
			CombineAssertions(() =>
			{
				var splitContainer = userControl.SupplyChainActorSplitContainer;
				AssertEquals("HouseConsignmentSupportingDocumentsSplitContainer.Orientation", Orientation.Horizontal, splitContainer.Orientation);
				AssertEquals("SupplyChainActorDynamicLayoutPanel in Panel2", true, splitContainer.Panel2.Contains(userControl.SupplyChainActorDynamicLayoutPanel));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new SupplyChainActorTabUserControl();
		}
		SupplyChainActorTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
