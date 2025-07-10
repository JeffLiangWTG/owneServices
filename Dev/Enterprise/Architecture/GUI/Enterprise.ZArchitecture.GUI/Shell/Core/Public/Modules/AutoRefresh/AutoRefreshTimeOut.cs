namespace Enterprise.ZArchitecture.Modules.AutoRefresh
{
	public class AutoRefreshTimeOut
	{
		const int DefaultRefreshTimeInMinutes = 5;

		public AutoRefreshTimeOut(bool isEnabled, byte refreshTimeInMinutes)
		{
			IsEnabled = isEnabled;
			RefreshTimeInMinutes = refreshTimeInMinutes;
		}

		public AutoRefreshTimeOut()
			: this(false, DefaultRefreshTimeInMinutes)
		{
		}

		public bool IsEnabled
		{
			get { return fIsEnabled; }
			set { fIsEnabled = value; }
		}

		public byte RefreshTimeInMinutes
		{
			get { return fRefreshTimeInMinutes; }
			set { fRefreshTimeInMinutes = value; }
		}

		bool fIsEnabled;
		byte fRefreshTimeInMinutes;
	}
}
