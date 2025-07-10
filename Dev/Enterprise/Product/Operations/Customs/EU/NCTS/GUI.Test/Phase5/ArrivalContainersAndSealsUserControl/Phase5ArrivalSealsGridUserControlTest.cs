using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.Business.AutoCusSeal.Schema;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5ArrivalSealsGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(Business.CusSealCollection), userControl.BindingSource.DataSourceType);
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] { BK_SequenceNumber, BK_UnloadingState, BK_SealNumber },
				sealsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestSealsGrid()
		{
			CombineAssertions(() =>
			{
				AssertType<ZGrid>("Type", sealsGrid);
				AssertEquals("BindTo", ".", sealsGrid.BindTo);
			});
		}

		public void TestSealsTabControl()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Tabs", 1, userControl.SealsTabControl.TabPages.Count);
				AssertEquals("Seals", userControl.SealsTabPage.CaptionResourceString.Caption);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5ArrivalSealsGridUserControl();
			sealsGrid = userControl.SealsGrid;
		}
		Phase5ArrivalSealsGridUserControl userControl;
		ZGrid sealsGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
