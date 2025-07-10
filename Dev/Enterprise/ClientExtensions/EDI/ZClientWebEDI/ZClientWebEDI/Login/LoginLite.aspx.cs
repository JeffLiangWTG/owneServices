namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class LoginLite : Login
	{
		public LoginLite()
			: base()
		{
		}

		protected override MyAccountLoginHelper GetNewLoginHelper()
		{
			return new MyAccountLoginLiteHelper(this, false);
		}
	}
}
