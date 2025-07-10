using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	class NctsGoodsItemContainersUserControlTest : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			using (var form = new ZForm())
			using (var control = new NctsGoodsItemContainersUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var grid = control.FindSingle<ZGrid>("SelectedContainersGrid");
				AssertNotNull("Type", grid.GetColumnStyle(NonPersistentDepartureContainerPivot.Schema.ContainerType));
				AssertNotNull("Mode", grid.GetColumnStyle(NonPersistentDepartureContainerPivot.Schema.ContainerMode));
			}
		}
	}
}
