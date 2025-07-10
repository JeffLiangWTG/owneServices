using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(CustomsCusContainersWithTrackingAndAdditionalSealUserControl))]
	public sealed class CustomsCusContainersWithTrackingAndAdditionalSealUserControlTest : TestCaseWithFactory
	{
		public void TestColumnDefaultVisibility()
		{
			using (var control = new CustomsCusContainersWithTrackingAndAdditionalSealUserControl())
			{
				var grid = control.FindSingle<ZModuleButtonGrid>("CusContainersBoundGrid");
				var secondSealColumn = grid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().FirstOrDefault(style => style.ColumnName == "CO_SecondSeal");
				Assert("Second Seal No. should be visible by default", secondSealColumn.IsVisible);

				var containerTypeColumnStyleInfo = grid.InnerGrid.GetColumnStyle("ContainerType");
				var containerSizeColumnStyleInfo = grid.InnerGrid.GetColumnStyle("ContainerSize");
				var containerColumnStyleInfo = grid.InnerGrid.GetColumnStyle("CO_RC");

				Assert(containerColumnStyleInfo.IsVisible);
				Assert(containerTypeColumnStyleInfo.IsVisible);
				Assert(containerSizeColumnStyleInfo.IsVisible);
				AssertEquals(containerColumnStyleInfo.GroupName, containerTypeColumnStyleInfo.GroupName);
				AssertEquals(containerColumnStyleInfo.GroupName, containerSizeColumnStyleInfo.GroupName);
			}
		}
	}
}
