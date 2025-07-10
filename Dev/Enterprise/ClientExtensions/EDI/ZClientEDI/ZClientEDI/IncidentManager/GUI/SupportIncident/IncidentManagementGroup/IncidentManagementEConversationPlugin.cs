using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.EConversation.Business;
using Enterprise.EConversation.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	internal class IncidentManagementEConversationPlugin : EConversationPlugin
	{
		public IncidentManagementEConversationPlugin(ModuleIdentifier parentModule, IBusiness host) : base(parentModule, host)
		{
		}

		protected override Control GetNewUserControl()
		{
			return new IncidentManagementEConversationTabControl(ParentModule, SecurityItems);
		}

		protected override void HookFormEventsCore()
		{
			// Prevent base class adding another Add Internal Log menu
		}
	}

	internal class IncidentManagementEConversationTabControl : EConversationFullControl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		public IncidentManagementEConversationTabControl(ModuleIdentifier parentModuleId, EConversationSecurityCheckpoints securityCheckpoints)
			: base(parentModuleId, securityCheckpoints, new IncidentManagementEConversationViewController())
		{
		}
	}

	public class IncidentManagementEConversationPluginController : EConversationPluginController
	{
		protected override ZPlugIn GetPlugIn(IBusiness host)
		{
			IConversationProvider conversationProvider = host as IncidentManagementGroup;
			return conversationProvider != null ? new IncidentManagementEConversationPlugin(conversationProvider.ParentModule, host) : null;
		}
	}
}
