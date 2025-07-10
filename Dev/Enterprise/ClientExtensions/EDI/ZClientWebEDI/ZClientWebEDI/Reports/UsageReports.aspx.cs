namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class UsageReports : BasePage
	{
		protected override void OnLoad(System.EventArgs e)
		{
			UserSession = UserSessionHelper.GenerateSessionString(SiteUser);
			Product = Request.QueryString["product"] ?? "";
			base.OnLoad(e);
			AdjustControlsForLiteViewMode();
		}

		void AdjustControlsForLiteViewMode()
		{
			if (IsInLiteViewMode)
			{
				Breadcrumb.Visible = false;
			}
		}

		protected string UserSession { get; private set; }
		protected string Product { get; private set; }
	}
}
