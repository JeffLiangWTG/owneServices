using System.Web.UI;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZLinkButtonTest : WebControlTest
	{
		#region Setup

		protected override Control GetNewControl()
		{
			return new ZLinkButton();
		}

		#endregion
	}
}
