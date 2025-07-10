using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(ARAccQueryClaimPlugIn))]
	public class TestARAccQueryClaimPlugIn : TestAccQueryClaimPlugIn
	{
		public void TestQueryClaimsCollection()
		{
			using (ARAccQueryClaimPlugIn testPlugIn = new ARAccQueryClaimPlugIn(Org))
			{
				AssertEquals("Collection loaded properly", Org, testPlugIn.QueryClaims.Master);
			}
		}

		public void TestPluginIsDockedToFill()
		{
			using (ARAccQueryClaimPlugIn testPlugIn = new ARAccQueryClaimPlugIn(Org))
			{
				AssertEquals("Control is Docked to Fill", DockStyle.Fill, testPlugIn.UserControl.Dock);
			}
		}
	}
}
