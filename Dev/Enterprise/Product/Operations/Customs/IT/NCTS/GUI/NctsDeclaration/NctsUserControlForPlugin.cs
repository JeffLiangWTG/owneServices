using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.GUI;

public partial class NctsUserControlForPlugin : EU.NCTS.GUI.NctsUserControlForPlugin
{
	public NctsUserControlForPlugin(EU.NCTS.Business.NctsHeader nctsMovement) : base(nctsMovement)
	{
	}

	protected override EU.NCTS.GUI.MessagesTabUserControl GetMessagesUserControl() => new MessagesTabUserControl();

	protected override EU.NCTS.GUI.DeclarationDetailsTabUserControl GetDeclarationDetailsTabUserControl() => new DeclarationDetailsTabUserControl();

	protected override EU.NCTS.GUI.NctsGoodsItemsUserControl GetNctsGoodsItemsUserControl() => new NctsGoodsItemsUserControl();

	protected override EU.NCTS.GUI.SecurityTabUserControl GetSecurityUserControl() => new SecurityTabUserControl();

	protected override EU.NCTS.GUI.MiscOptionsUserControl GetMiscOptionsUserControl() => new NctsMiscOptionsUserControl();

	protected override EU.NCTS.GUI.DeclarationStatusTabUserControl GetDeclarationStatusUserControl() => new DeclarationStatusTabUserControl();

	protected override ZBool SupportsMiscOptionsTabPage => true;

	protected override ZBool SupportsStatusTabPage => true;
}
