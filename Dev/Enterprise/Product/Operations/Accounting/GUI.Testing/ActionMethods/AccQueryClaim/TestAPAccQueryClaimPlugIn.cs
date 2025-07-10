using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(APAccQueryClaimPlugIn))]
	public class TestAPAccQueryClaimPlugIn : TestAccQueryClaimPlugIn
	{
		public void TestQueryClaimsCollection()
		{
			using (APAccQueryClaimPlugIn testPlugIn = new APAccQueryClaimPlugIn(Org))
			{
				AssertEquals("Collection loaded properly", Org, testPlugIn.QueryClaims.Master);
			}
		}

		public void TestPluginIsDockedToFill()
		{
			using (APAccQueryClaimPlugIn testPlugIn = new APAccQueryClaimPlugIn(Org))
			{
				AssertEquals("Control is Docked to Fill", DockStyle.Fill, testPlugIn.UserControl.Dock);
			}
		}
	}
}
