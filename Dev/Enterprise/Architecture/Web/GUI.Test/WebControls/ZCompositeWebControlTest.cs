using System.Web.UI;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZCompositeWebControlTest : WebControlTest
	{
		protected override Control GetNewControl()
		{
			return new ZCompositeWebControl();
		}
	}
}
