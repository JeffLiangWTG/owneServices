#if DEBUG

using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.GUI
{
	public partial class LoginFormWithTwoCredentialSupportBranchDepartmentLevel
	{
		public ZTextBox LoginTextBox_ForTestOnly
		{
			get { return LoginTextBox; }
			set { LoginTextBox = value; }
		}

		public ZTextBox PasswordTextBox_ForTestOnly
		{
			get { return PasswordTextBox; }
			set { PasswordTextBox = value; }
		}

		public ZArchitecture.GUI.ZButton OKButton_ForTestOnly
		{
			get { return OKButton; }
			set { OKButton = value; }
		}
	}
}

#endif
