using System.Windows.Forms;
using Enterprise.UserPortal;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	[TestedType(typeof(UserPortalDisclaimerForm))]
	sealed class UserPortalDisclaimerFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new UserPortalDisclaimerForm(new UserPortalDisclaimerBizO());
		}
	}
}
