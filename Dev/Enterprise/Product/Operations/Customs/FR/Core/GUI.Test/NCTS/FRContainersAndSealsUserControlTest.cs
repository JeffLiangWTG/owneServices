using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	public class FRContainersAndSealsUserControlTest : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			using (var form = new ZForm())
			using (var control = new FRContainersAndSealsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var grid = control.FindSingle<ZGrid>("ContainersGrid");
				AssertNotNull("Type", grid.GetColumnStyle(FRNctsDepartureHeaderContainer.Schema.BC_RC));
				AssertNotNull("Mode", grid.GetColumnStyle(FRNctsDepartureHeaderContainer.Schema.BC_Mode));
			}
		}
	}
}
