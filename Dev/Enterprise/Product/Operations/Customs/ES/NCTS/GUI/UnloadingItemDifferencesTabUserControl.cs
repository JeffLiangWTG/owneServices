namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class UnloadingItemDifferencesTabUserControl : EU.NCTS.GUI.UnloadingItemDifferencesTabUserControl
	{
		public UnloadingItemDifferencesTabUserControl()
		{
			InitializeComponent();
		}

		protected override EU.NCTS.GUI.UnloadedItemNewUserControl GetUnloadedItemNewUserControl()
		{
			return new UnloadedItemNewUserControl();
		}

		protected override EU.NCTS.GUI.UnloadedItemDetailsUserControl GetUnloadedItemDetailsUserControl()
		{
			return new UnloadedItemDetailsUserControl();
		}
	}
}
