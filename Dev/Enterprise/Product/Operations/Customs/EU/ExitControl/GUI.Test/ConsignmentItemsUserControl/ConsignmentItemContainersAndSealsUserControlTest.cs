using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.ExitControlBase.Business.AutoCusExitContainer.Schema;
using CusExitConsignmentPivot = Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentPivot;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	class ConsignmentItemContainersAndSealsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ICusExitConsignmentPivotCollection<CusExitConsignmentPivot>), userControl.BindingSource.DataSourceType);
		}

		public void TestContainersAndSealsGroupBox()
		{
			var containersAndSealsGroupBox = userControl.ContainersAndSealsGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Dock", System.Windows.Forms.DockStyle.Fill, containersAndSealsGroupBox.Dock);
				AssertEquals("Caption", "Containers and Seals", containersAndSealsGroupBox.CaptionResourceString.Caption);
				AssertEquals("ContainersAndSealsGrid is within ContainersAndSealsGroupBox", true, containersAndSealsGroupBox.Controls.Contains(containersAndSealsGrid));
			});
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] { ContainerColumnName },
					containersAndSealsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestColumn_CXN_ContainerNumber()
		{
			AssertEquals(110, containersAndSealsGrid.GetColumnStyle(ContainerColumnName).Width);
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ConsignmentItemContainersAndSealsUserControl();
			containersAndSealsGrid = userControl.ContainersAndSealsGrid;
		}
		ConsignmentItemContainersAndSealsUserControl userControl;
		ZGrid containersAndSealsGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}

		const string Container = nameof(CusExitConsignmentPivot.Container);

		const string ContainerColumnName = Container + "+" + CXN_ContainerNumber;
	}
}
