using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Scheduler.Business
{
	public class StmScheduleTaskRecipientLookups : AutoStmScheduleTaskRecipientLookups
	{
		public StmScheduleTaskRecipientLookups(AutoStmScheduleTaskRecipient parent)
			: base(parent)
		{
		}

		protected new StmScheduleTaskRecipient Parent
		{
			get { return (StmScheduleTaskRecipient)base.Parent; }
		}

		public CodeDescriptionPairList AttachmentTypes
		{
			get { return GetAttachmentTypesCore(); }
		}

		protected virtual CodeDescriptionPairList GetAttachmentTypesCore()
		{
			return OrgCodeLists.AttachmentType_List;
		}

		public CodeDescriptionPairList BlankReportActivities
		{
			get { return new EmptyReportContingencyList(); }
		}

		public OrganisationsFindBoxCollection Organisations
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		public IBusinessObjectCollection DeliveryRecipients
		{
			get
			{
				switch (Parent.S6_DeliveryToType)
				{
					case ScheduledReportDeliveryRecipientConstants.RecipientType.Group:
						return Groups;
					case ScheduledReportDeliveryRecipientConstants.RecipientType.Staff:
						return Recipients;
					default:
						return Contacts;
				}
			}
		}

		#region Notify Modes

		public CodeDescriptionPairList NotifyModes
		{
			get { return notifyModes ?? (notifyModes = GetNewNotifyModesList()); }
		}
		CodeDescriptionPairList notifyModes;

		protected virtual CodeDescriptionPairList GetNewNotifyModesList()
		{
			return new CodeDescriptionPairList(OLookUpEditType.NotifyMode);
		}

		#endregion

		#region Contact Names

		public CodeDescriptionPairList ContactNames
		{
			get
			{
				if (contactNames == null || (organisationPK != Parent.S6_OH))
				{
					organisationPK = Parent.S6_OH;
					contactNames = new CodeDescriptionPairList();
					foreach (OrgContact contact in Parent.Contacts)
					{
						if (contact.OC_IsActive)
						{
							contactNames.AddPair(contact.OC_ContactName);
						}
					}
				}
				return contactNames;
			}
		}
		ZGuid organisationPK;
		CodeDescriptionPairList contactNames;

		#endregion

		#region Delivery Recipient Types

		public CodeDescriptionPairList DeliveryRecipientTypes => deliveryRecipientTypes ?? (deliveryRecipientTypes = GetNewDeliveryRecipientTypesList());
		CodeDescriptionPairList deliveryRecipientTypes;

		protected virtual CodeDescriptionPairList GetNewDeliveryRecipientTypesList()
		{
			var deliveryRecipientTypes = new CodeDescriptionPairList();
			deliveryRecipientTypes.AddPair(ScheduledReportDeliveryRecipientConstants.RecipientType.Contact, ResString.GetMultilingualString("5aa5c86e-6efe-4602-b2d1-2992d6742bd7", "Contact"));
			deliveryRecipientTypes.AddPair(ScheduledReportDeliveryRecipientConstants.RecipientType.Group, ResString.GetMultilingualString("2f3c2f2d-6971-4407-9ecc-b906d413de9f", "Group"));
			deliveryRecipientTypes.AddPair(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, ResString.GetMultilingualString("d3cce607-a735-4aec-80fa-6b25c82c4886", "Staff"));
			return deliveryRecipientTypes;
		}

		#endregion
	}
}
