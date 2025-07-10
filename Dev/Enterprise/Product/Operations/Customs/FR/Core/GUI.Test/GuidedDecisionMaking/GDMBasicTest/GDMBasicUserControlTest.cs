using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.GDM.Testing
{
	sealed class GDMBasicUserControlTest : TestCase
	{
		public void TestControls()
		{
			using (var control = new GDMBasicUserControl())
			{
				AssertNotNull("RegionOrTerritoryOfDestinationDropEdit", control.FindSingle<ZDropEdit>("RegionOrTerritoryOfDestinationDropEdit"));
			}
		}
	}
}
