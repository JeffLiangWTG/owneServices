using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class CPQACollectionUserControlTest : TestCaseWithFactory
	{
		public void TestGridId()
		{
			using (var control = new CPQACollectionUserControl())
			{
				var grid = (ZGrid)control.Controls.Find("zGrid1", true).First();
				AssertEquals("GridLayoutgESDuLG0NeG6iZ1dnTLcJQ==", grid.GridId);
			}
		}
	}
}
