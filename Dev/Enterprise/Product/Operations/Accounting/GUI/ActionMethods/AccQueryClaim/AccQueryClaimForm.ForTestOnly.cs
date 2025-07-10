#if DEBUG

namespace Enterprise.Accounting.GUI
{
	public partial class AccQueryClaimForm
	{
		public ZArchitecture.GUI.ZTemplateTabControl TabControl_ForTestOnly
		{
			get { return TabControl; }
			set { TabControl = value; }
		}

		public Business.AccQueryClaims.AccQueryClaimBase Claim_ForTestOnly => Claim;
	}
}

#endif
