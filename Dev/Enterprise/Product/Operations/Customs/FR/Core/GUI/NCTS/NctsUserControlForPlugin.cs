namespace Enterprise.Customs.FR.GUI.NCTS
{
	public partial class NctsUserControlForPlugin : EU.NCTS.GUI.NctsUserControlForPlugin
	{
		public NctsUserControlForPlugin(EU.NCTS.Business.NctsHeader nctsMovement) : base(nctsMovement)
		{
		}

		protected override EU.NCTS.GUI.DeclarationDetailsTabUserControl GetDeclarationDetailsTabUserControl() => new DeclarationDetailsTabUserControl();

		protected override EU.NCTS.GUI.NctsArrivalUserControl GetNctsArrivalUserControl() => new NctsArrivalUserControl();

		protected override EU.NCTS.GUI.NctsGoodsItemsUserControl GetNctsGoodsItemsUserControl() => new NctsGoodsItemsUserControl();

		protected override EU.NCTS.GUI.UnloadingRemarksUserControl GetUnloadingRemarksUserControl() => new FRUnloadingRemarksUserControl();
	}
}
