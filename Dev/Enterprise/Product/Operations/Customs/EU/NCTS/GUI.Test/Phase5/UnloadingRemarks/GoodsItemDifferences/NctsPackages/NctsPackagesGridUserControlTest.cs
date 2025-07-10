using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.Business.AutoCusInvPack.Schema;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class NctsPackagesGridUserControlTest : TestCaseWithFactory
	{
		public void TestGrid()
		{
			AssertType<ZGrid>(nctsPackagesGrid);
		}

		public void TestAvailableColumns()
		{
			var actualColumnNames = nctsPackagesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
			AssertSequencesEqual("Columns", new[] { B5_SequenceNumber, B5_TypeOfDifference, B5_UnitCount, B5_UnitType, B5_MarksAndNumbers }, actualColumnNames);
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new NctsPackagesGridUserControl();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			userControl.SetDataBinding(header, null);
			nctsPackagesGrid = userControl.NctsPackagesGrid;
		}
		NctsPackagesGridUserControl userControl;
		ZGrid nctsPackagesGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
