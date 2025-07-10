using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Modules;
using Enterprise.CustomerService.Business;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EdiIncidentRequest : IncidentRequest, IProcessHandlingInfoProvider, IGlobalSearchBusinessObjectProvider, IConversationWithDetails
	{
		public EdiIncidentRequest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => RelatedSupportIncident?.HumanReadableName ?? base.HumanReadableNameCore;
		protected override ZString HumanReadableShortcutNameCore => RelatedSupportIncident?.HumanReadableShortcutName ?? base.HumanReadableShortcutNameCore;

		protected override DocManagerInfo CreateDocManagerInfo()
		{
			return new IncidentDocManagerInfo(this);
		}

		public void RunConversationUpdateActionBeforeSaving()
		{
			return;
		}

		public SupportIncident RelatedSupportIncident
		{
			get
			{
				if (relatedSupportIncident == null)
				{
					var query = new ZQuery(IncidentMainSchema.IM_INC_Request, PK);
					if (!IsInDatabase)
					{
						query.FetchOnlyFromLocalCache = true;
					}
					relatedSupportIncident = Factory.LoadTop1<SupportIncident>(query);
				}

				return relatedSupportIncident;
			}
		}

		SupportIncident relatedSupportIncident;

		#region IProcessHandlingInfoProvider

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo => new EdiIncidentRequestProcessHandlingInfo(this);

		#endregion

		#region IGlobalSearchBusinessObjectProvider

		BusinessObject IGlobalSearchBusinessObjectProvider.BusinessObjectForController => RelatedSupportIncident;

		#endregion

		#region IConversationWithDetails

		public JobConversation eConversation => RelatedSupportIncident.EConversation.Conversation;

		public ModuleIdentifier ParentModule => ClientModuleRegistration.SupportIncident;

		public ControllerID ParentController => ClientControllerRegistration.SupportIncident;

		IEnumerable<EConversation.Business.RelatedParty> IConversationProvider.AdditionalParticipants
		{
			get { return Enumerable.Empty<EConversation.Business.RelatedParty>(); }
		}

		public bool SendEmailNotificationsOnSave => false;

		public string EmailSubjectContentOverride => default;

		public string FromAddressOverride => SupportIncidentLookups.SupportEmailAddress;

		public NotificationEmailTemplate NotificationEmailTemplateOverride => EDIDataRegistry.Instance.SubscriberUpdateEConversationEmailTemplate.Value;

		public ZString Summary => INC_Summary;

		public ZString DetailedDescription => INC_Details;

		#endregion
	}
}
