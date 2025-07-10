using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.EConversation.Business;
using Enterprise.EConversation.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public class IncidentConversationPlugin : EConversationPlugin
	{
		public IncidentConversationPlugin(ModuleIdentifier parentModule, IBusiness host)
			: base(parentModule, host)
		{ }

		protected override Control GetNewUserControl()
		{
			return new IncidentConversationTabControl(ParentModule, SecurityItems);
		}

		protected override void HookFormEventsCore()
		{
			// Prevent base class adding another Add Internal Log menu
		}

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			var result = ContinueWithSave.Yes;
			using (DefaultPreSaveMessageNotificationSuspender.GetSuspender())
			{
				result = base.ShowPreSaveDialogsCore();
			}

			return result;
		}
	}

	public class IncidentConversationTabControl : EConversationFullControl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		public IncidentConversationTabControl(ModuleIdentifier parentModuleId, EConversationSecurityCheckpoints securityCheckpoints)
			: base(parentModuleId, securityCheckpoints, new IncidentConversationViewController(true))
		{
		}
	}

	public class IncidentConversationPluginController : EConversationPluginController
	{
		protected override ZPlugIn GetPlugIn(IBusiness host)
		{
			IConversationProvider conversationProvider = host as SupportIncident;
			return conversationProvider != null ? new IncidentConversationPlugin(conversationProvider.ParentModule, host) : null;
		}
	}
}
