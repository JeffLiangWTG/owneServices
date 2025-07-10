using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;

namespace Enterprise.Customs.GB.GUI.Testing
{
	sealed class CustomsBrokerageUserControlTest : TestCaseWithFactory
	{
		public void TestGetDeclarationUserControl()
		{
			using (var control = new CustomsBrokerageUserControlForTest())
			using (var innerControl = control.GetDeclarationUserControlExposed())
			{
				AssertType<JobDeclarationUserControl>(innerControl);
			}
		}

		public void TestGetContainerUserControl()
		{
			using (var control = new CustomsBrokerageUserControlForTest())
			using (var innerControl = control.GetContainerUserControlExposed())
			{
				AssertType<CusContainerUserControl>(innerControl);
			}
		}

		public void TestContainerUserControlGridColumnStyle()
		{
			using (var control = new CustomsBrokerageUserControlForTest())
			using (var innerControl = control.GetContainerUserControlExposed())
			{
				var containerControl = (CusContainerUserControl)innerControl;
				var containerGrid = containerControl.CusContainersBoundGrid;
				var statusColumn = containerGrid.ColumnStyles.Cast<ZGridColumnInfo>().SingleOrDefault(column => column.Caption == "Status");
				AssertNotNull("Status column", statusColumn);
				AssertType<CusContainerUserControl.ContainerStatusDropEditColumnStyleInfo>(statusColumn);
				AssertEquals(Customs.Business.AutoCusContainer.Schema.CO_MessageStatus, statusColumn.ColumnName);
				AssertEquals(typeof(CusContainerUserControl.ContainerStatusDropEditColumnStyle), statusColumn.ColumnStyleType);
			}
		}

		class CustomsBrokerageUserControlForTest : CustomsBrokerageUserControl
		{
			public Customs.GUI.BaseCustomsEntryUserControl GetDeclarationUserControlExposed() => base.GetDeclarationUserControl();

			public Customs.GUI.BaseCustomsCusContainersUserControl GetContainerUserControlExposed() => base.GetContainerUserControl();
		}
	}
}
