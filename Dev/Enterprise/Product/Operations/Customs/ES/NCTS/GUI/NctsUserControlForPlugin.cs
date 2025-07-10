namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class NctsUserControlForPlugin : EU.NCTS.GUI.NctsUserControlForPlugin
	{
		public NctsUserControlForPlugin(EU.NCTS.Business.NctsHeader nctsMovement) : base(nctsMovement)
		{
		}

		protected override EU.NCTS.GUI.NctsArrivalUserControl GetNctsArrivalUserControl() => new NctsArrivalUserControl();

		protected override EU.NCTS.GUI.UnloadingRemarksUserControl GetUnloadingRemarksUserControl() => new UnloadingRemarksUserControl();

		protected override EU.NCTS.GUI.SecurityTabUserControl GetSecurityUserControl() => new SecurityTabUserControl();

		protected override EU.NCTS.GUI.DeclarationDetailsTabUserControl GetDeclarationDetailsTabUserControl() => new DeclarationDetailsTabUserControl();

		protected override EU.NCTS.GUI.NctsGoodsItemsUserControl GetNctsGoodsItemsUserControl() => new NctsGoodsItemsUserControl();

		protected override bool ShouldCreateUnloadingRemarksUserControl => nctsMovement.IsArrivalTabVisible;

		protected override string[] ControlsAllowedToRemainEditableAfterSending
		{
			get
			{
				var userControl = (NctsArrivalUserControl)NctsArrivalUserControl;
				return new string[] { nameof(userControl.CertificateDropEdit), nameof(userControl.BrokerFindBox) };
			}
		}
	}
}
