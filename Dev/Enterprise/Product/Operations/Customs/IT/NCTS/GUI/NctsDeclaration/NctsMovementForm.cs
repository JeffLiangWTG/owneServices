using System;
using CargoWise.Types;
using Enterprise.Customs.IT.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.GUI;

public partial class NctsMovementForm : EU.NCTS.GUI.NctsMovementForm
{
	public NctsMovementForm(NctsHeader nctsMovement) : base(nctsMovement)
	{
		InitializeComponent();
	}

	protected override Type GetGoodsItemUserControlType() => typeof(NctsGoodsItemsUserControl);

	protected override Type GetDeclarationDetailsUserControlType() => typeof(DeclarationDetailsTabUserControl);

	protected override Type GetMiscOptionsUserControlType() => typeof(NctsMiscOptionsUserControl);

	protected override Type GetDeclarationStatusUserControlType() => typeof(DeclarationStatusTabUserControl);

	protected override ZBool SupportsMiscOptionsTabPage => true;

	protected override ZBool SupportsStatusTabPage => true;

	protected override Type GetMessagesUserControlType() => typeof(MessagesTabUserControl);

	protected override Type GetSecurityUserControlType() => typeof(SecurityTabUserControl);
}
