using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.Business.AutoCusReference.Schema;
using static Enterprise.Customs.Business.CommonCusReference.Schema;
using CusSupplyChainActorReference = Enterprise.Customs.EU.NCTS.Business.CusSupplyChainActorReference;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class HouseConsignmentSupplyChainActorsGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>), userControl.BindingSource.DataSourceType);
		}

		public void TestRemoveAction()
		{
			AssertEquals(RemoveAction.RemoveAndDelete, selectedSupplyChainActorsGrid.RemoveAction);
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] { CFR_Code, CFR_Reference, OwnerOrgPK }, selectedSupplyChainActorsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestColumnWidth()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CFR_Code", 80, selectedSupplyChainActorsGrid.GetColumnStyle(CFR_Code).Width);
				AssertEquals("CFR_Reference", 152, selectedSupplyChainActorsGrid.GetColumnStyle(CFR_Reference).Width);
				AssertEquals("OwnerOrgPK", 87, selectedSupplyChainActorsGrid.GetColumnStyle(OwnerOrgPK).Width);
			});
		}

		public void TestCFR_Code_CharacterCasing()
		{
			AssertEquals(System.Windows.Forms.CharacterCasing.Upper, selectedSupplyChainActorsGrid.GetColumnStyle(CFR_Code).CharacterCasing);
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new HouseConsignmentSupplyChainActorsGridUserControl();
			selectedSupplyChainActorsGrid = userControl.SelectedSupplyChainActorsGrid;
		}
		HouseConsignmentSupplyChainActorsGridUserControl userControl;
		ZGrid selectedSupplyChainActorsGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
