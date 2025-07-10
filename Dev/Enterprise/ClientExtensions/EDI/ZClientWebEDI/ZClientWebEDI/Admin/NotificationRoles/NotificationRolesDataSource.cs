using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class NotificationRolesDataSource : NotificationRolesContactFilterStripBusinessObject
	{
		public NotificationRolesDataSource(BusinessObjectFactory factory, ZGuid orgPK) : base(factory)
		{
			Master = Factory.Load<OrgHeader>(orgPK);
		}

		protected override OrgHeader MasterOrg => Master;
		readonly OrgHeader Master;

		public override bool HasChanges
		{
			get => MasterOrg.HasChanges;
			set => MasterOrg.HasChanges = value;
		}

		#region Bulk update

		public BulkUpdateModeCollection BulkUpdateModeCollection
		{
			get
			{
				if (bulkUpdateModes == null)
				{
					bulkUpdateModes = new BulkUpdateModeCollection();
					bulkUpdateModes.Add(new BulkUpdateMode(Res.GetString("9A29D398-C641-48BE-82A0-C7C75BD82B7E", "Apply to selected"), 0, BulkUpdateModeCodes.SelectedContacts) { Selected = true });
					bulkUpdateModes.Add(new BulkUpdateMode(Res.GetString("38DE652C-5998-40E9-BC96-E8F39CB649BA", "Apply to all (could be slow)"), 0, BulkUpdateModeCodes.AllContacts));
				}
				return bulkUpdateModes;
			}
		}

		BulkUpdateModeCollection bulkUpdateModes;

		public void InitBulkUpdate(int allContactsCount, int selectedContactsCount)
		{
			BulkUpdateModeCollection[BulkUpdateModeCodes.AllContacts].RecordsAffected = allContactsCount;
			BulkUpdateModeCollection[BulkUpdateModeCodes.SelectedContacts].RecordsAffected = selectedContactsCount;
			CreateSnapshot();
		}

		public void BulkUpdateSelected(OrgContactCollection contacts, IEnumerable<ZGuid> contactPks)
		{
			var delegates = AdminGroupItems.OfType<AdminGroupItem>().Where(x => !x.Skip).Select(x => CreateSetAdminGroupDelegate(x.GroupCode, x.Granted)).ToList();
			contactPks.Select(pk => (EDIOrgContact)contacts.FindByPK(pk)).ToList().ForEach(c => delegates.ForEach(d => d(c)));
		}

		public void BulkUpdateAll(ZQuery query)
		{
			var reader = new FilteredBusinessObjectReader(new SingleBusinessObjectFactoryProvider(Factory), query, typeof(OrgContact));
			reader.BatchSize = 100;
			reader.SaveBeforeLoadNextEnabled = true;
			var delegates = AdminGroupItems.OfType<AdminGroupItem>().Where(x => !x.Skip).Select(x => CreateSetAdminGroupDelegate(x.GroupCode, x.Granted)).ToList();

			foreach (EDIOrgContact contact in reader)
			{
				delegates.ForEach(d => d(contact));
			}
		}

		delegate void SetAdminGroupDelegate(EDIOrgContact orgContact);

		static SetAdminGroupDelegate CreateSetAdminGroupDelegate(ZString groupCode, ZBool value)
		{
			if (groupCode == ContactType.CustomerService.Code)
			{
				return (c) => c.IsCustomerServiceContact = value;
			}
			else if (groupCode == ContactType.Receivables.Code)
			{
				return (c) => c.IsAccountsReceivableContact = value;
			}
			else if (groupCode == EDIOrgDocumentGroupTypes.Codes.BorderWiseAdministrator)
			{
				return (c) => c.IsBorderWiseAdministrator = value;
			}
			else if (groupCode == EDIOrgDocumentGroupTypes.Codes.ERequestPendingApprovals)
			{
				return (c) => c.IsERequestApprover = value;
			}
			else if (groupCode == EDIOrgDocumentGroupTypes.Codes.InformationServicesTechnicalAdministrator)
			{
				return (c) => c.IsInformationServicesTechnicalAdministrator = value;
			}
			else if (groupCode == EDIOrgDocumentGroupTypes.Codes.CertificationProgramContact)
			{
				return (c) => c.IsCertificationProgramContact = value;
			}
			else
			{
				throw new ArgumentOutOfRangeException(groupCode);
			}
		}

		public void CancelBulkUpdate() => RestoreSnapshot();

		public AdminGroupItemCollection AdminGroupItems
		{
			get
			{
				if (adminGroupItems == null)
				{
					adminGroupItems = new AdminGroupItemCollection(Factory);
					foreach (ICodeDescription notificationGroup in new NotificationRolesContactsModuleColumnProvider().NotificationGroupList)
					{
						var item = AdminGroupItems.AddNew();
						item.GroupCode = notificationGroup.Code;
						item.GroupDescription = notificationGroup.Description;
					}
				}

				return adminGroupItems;
			}
		}

		AdminGroupItemCollection adminGroupItems;

		#region Snapshot

		void CreateSnapshot()
		{
			Snapshot.Clear();
			Snapshot.AddRange(BulkUpdateModeCollection.Select(x => x).Cast<BulkUpdateMode>().Select(x => CreateRollbackAction(x, x.Selected)));
			Snapshot.AddRange(AdminGroupItems.OfType<AdminGroupItem>().Select(x => CreateRollbackAction(x, x.Granted, x.Skip)));
		}

		void RestoreSnapshot()
		{
			Snapshot.ForEach(x => x());
			Snapshot.Clear();
		}

		readonly List<Action> Snapshot = new List<Action>();

		static Action CreateRollbackAction(BulkUpdateMode mode, bool selected) => () => { mode.Selected = selected; };

		static Action CreateRollbackAction(AdminGroupItem item, ZBool granted, ZBool skip) => () => { item.Granted = granted; item.Skip = skip; };

		#endregion Snapshot

		#endregion Bulk update
	}

	#region Helper Classes

	public class AdminGroupItem : NonPersistentBusinessObject
	{
		public AdminGroupItem(BusinessObjectFactory factory) : base(factory)
		{
			Skip = true;
		}

		[BusinessObjectTestExclude]
		public ZString GroupCode { get; set; }

		public ZPropertyInfo GroupCodeInfo => GetZPropertyInfo(nameof(GroupCode));

		[BusinessObjectTestExclude]
		public ZString GroupDescription { get; set; }

		public ZPropertyInfo GroupDescriptionInfo => GetZPropertyInfo(nameof(GroupDescription));

		[BusinessObjectTestExclude]
		public ZBool Granted { get; set; }

		public ZPropertyInfo GrantedInfo => GetZPropertyInfo(nameof(Granted));

		[BusinessObjectTestExclude]
		public ZBool Skip { get; set; }

		public ZPropertyInfo SkipInfo => GetZPropertyInfo(nameof(Skip));
	}

	public class AdminGroupItemCollection : NonPersistentBusinessObjectCollection<AdminGroupItem>
	{
		public AdminGroupItemCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new AdminGroupItem(Factory);
	}

	#endregion Helper Classes
}
