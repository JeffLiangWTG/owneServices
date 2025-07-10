using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

public partial class Phase5NctsUserControlForPlugin : EU.NCTS.GUI.Phase5NctsUserControlForPlugin
{
	[Obsolete("Just for the Designer")]
	public Phase5NctsUserControlForPlugin() : base()
	{
	}

	public Phase5NctsUserControlForPlugin(EU.NCTS.Business.NctsHeader nctsMovement) : base(nctsMovement)
	{
	}

	protected override ZUserControl GetMessagesUserControl() => new MessagesTabUserControl();
}
