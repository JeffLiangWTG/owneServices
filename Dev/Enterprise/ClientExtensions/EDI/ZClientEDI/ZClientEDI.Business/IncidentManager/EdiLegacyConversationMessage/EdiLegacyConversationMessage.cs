using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.EConversation.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EdiLegacyConversationMessage : AutoEdiLegacyConversationMessage
	{
		public EdiLegacyConversationMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("Message")]
		public override ZGuid ELC_JCM_Message
		{
			get { return base.ELC_JCM_Message; }
			set { base.ELC_JCM_Message = value; }
		}

		public JobConversationMessage Message => Factory.Load<JobConversationMessage>(ELC_JCM_Message);
	}
}

