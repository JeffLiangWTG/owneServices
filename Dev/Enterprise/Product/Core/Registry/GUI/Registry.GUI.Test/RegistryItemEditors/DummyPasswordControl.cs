using System.Reflection;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class DummyPasswordControl : PasswordControl
	{
		protected override bool IsValidPassword(DeveloperLoginForm loginForm)
		{
			((ZTextBox)typeof(DeveloperLoginForm).GetField("PasswordTextBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(loginForm)).Text = LoginPassword;
			return base.IsValidPassword(loginForm);
		}

		public string LoginPassword;
	}
}
