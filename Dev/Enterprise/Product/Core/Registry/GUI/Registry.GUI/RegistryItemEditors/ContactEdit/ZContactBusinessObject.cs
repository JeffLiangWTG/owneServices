using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.GUI
{
	public class ZContactBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string SelectedContactPK = "SelectedContactPK";
			public const string SelectedOrganisationPK = "SelectedOrganisationPK";
		}

		#endregion

		public ZContactBusinessObject() : base(new BusinessObjectFactory())
		{
			if (!GlbCompany.CurrentCompany.GC_OH_OrgProxy.IsEmpty)
			{
				SelectedOrganisationPK = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				HasChanges = false;
			}
		}

		public OrgHeaderCollection SelectedOrganisationList
		{
			get
			{
				if (fSelectedOrganisationList == null)
				{
					fSelectedOrganisationList = new OrgHeaderCollection(Factory);
				}
				return fSelectedOrganisationList;
			}
		}
		OrgHeaderCollection fSelectedOrganisationList;

		public OrgHeader SelectedOrganisation
		{
			get
			{
				return (OrgHeader)Factory.Load(typeof(OrgHeader), SelectedOrganisationPK);
			}
		}

		#region SelectedOrganisationPK

		[RelatedBusinessObject("SelectedOrganisation")]
		public ZGuid SelectedOrganisationPK
		{
			get { return fSelectedOrganisationPK; }
			set
			{
				if (SelectedOrganisationPK != value)
				{
					fSelectedOrganisationPK = value;
					HasChanges = true;
				}

				Contacts.Load();

				if (!IsValidationSuspended)
				{
					ValidateSelectedOrganisationPK();
				}
				SelectedOrganisationPKInfo.RefreshBinding();
			}
		}

		public virtual void ValidateSelectedOrganisationPK()
		{
			SelectedOrganisationPKInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(SelectedOrganisationPKInfo);
		}

		public ZPropertyInfo SelectedOrganisationPKInfo
		{
			get { return GetZPropertyInfo(Schema.SelectedOrganisationPK); }
		}

		ZGuid fSelectedOrganisationPK;

		#endregion

#if DEBUG
		public
#endif
		class FilteredOrgContactCollection : MasterFiles.Business.OrgContactCollection
		{
			public FilteredOrgContactCollection(BusinessObjectFactory factory) : base(factory)
			{
			}

			public void SetRelationshipFilter(ZQuery filter)
			{
				fRelationshipFilter = filter;
			}

			protected override ZQuery CreateRelationshipFilter()
			{
				return fRelationshipFilter;
			}
			ZQuery fRelationshipFilter;
		}

		FilteredOrgContactCollection fContacts;
		public MasterFiles.Business.OrgContactCollection Contacts
		{
			get
			{
				if (fContacts == null)
				{
					fContacts = new FilteredOrgContactCollection(Factory);
					fContacts.SetRelationshipFilter(new ZQuery(OrgContactSchema.OC_OH, SelectedOrganisationPK));
					fContacts.Load();
				}
				else
				{
					fContacts.SetRelationshipFilter(new ZQuery(OrgContactSchema.OC_OH, SelectedOrganisationPK));
				}

				return fContacts;
			}
		}

		#region SelectedContactPK

		public ZGuid SelectedContactPK
		{
			get { return fSelectedContactPK; }
			set
			{
				if (SelectedContactPK != value)
				{
					fSelectedContactPK = value;
					HasChanges = true;

					if (fSelectedContactPK.IsValid)
					{
						OrgContact contact = (OrgContact)Factory.Load(typeof(OrgContact), fSelectedContactPK);
						if (contact != null)
						{
							if (contact.OC_OH != SelectedOrganisationPK)
							{
								SelectedOrganisationPK = contact.OC_OH;
							}
						}
					}
				}
				if (!IsValidationSuspended)
				{
					ValidateSelectedContactPK();
				}
				SelectedContactPKInfo.RefreshBinding();
			}
		}

		public virtual void ValidateSelectedContactPK()
		{
			SelectedContactPKInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(SelectedContactPKInfo);
		}

		public ZPropertyInfo SelectedContactPKInfo
		{
			get { return GetZPropertyInfo(Schema.SelectedContactPK); }
		}

		ZGuid fSelectedContactPK;

		#endregion
	}
}
