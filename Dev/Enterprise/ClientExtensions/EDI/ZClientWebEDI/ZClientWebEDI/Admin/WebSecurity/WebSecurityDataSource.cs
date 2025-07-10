using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class WebSecurityDataSource : WebSecurityContactFilterStripBusinessObject
	{
		public WebSecurityDataSource(BusinessObjectFactory factory, ZGuid orgPK) : base(factory)
		{
			Master = Factory.Load<OrgHeader>(orgPK);
			Master.SecurityRights.Sort(OrgSecurity.Schema.SecurityItemNameForDisplay);
			SecurityRights = new SecurityView(Master.SecurityRights);
			SetSecurityProfileDefault();
			CreateOrUpdateBulkUpdateProfile();
		}

		public static class Schema
		{
			public const string SecurityProfile = "SecurityProfile";
			public const string BulkUpdateProfile = "BulkUpdateProfile";
		}

		protected override OrgHeader MasterOrg => Master;
		const string CustomProfileCode = "";

		readonly OrgHeader Master;

		public override bool HasChanges { get => MasterOrg.HasChanges; set => MasterOrg.HasChanges = value; }

		void SetSecurityProfileDefault()
		{
			SecurityProfileList = new CodeDescriptionPairList();
			SecurityProfileList.AddPair(CustomProfileCode, "Custom");

			foreach (OrgSecurityProfile profile in OrganisationRegistry.Instance.WebSecurityDefaultValues.Value)
			{
				if (profile.Published)
				{
					SecurityProfileList.AddPair(profile.Name, profile.Name);
				}
			}

			UpdateSecurityProfile();
		}

		void UpdateSecurityProfile()
		{
			securityProfile = CustomProfileCode;

			var currentSetting = SecurityRights.OfType<OrgSecurity>().Select(x => Tuple.Create(x.SecurityKey, x.OX_GrantedForWeb)).Distinct()
				.OrderBy(x => x.Item1);

			foreach (OrgSecurityProfile profile in OrganisationRegistry.Instance.WebSecurityDefaultValues.Value)
			{
				if (profile.Published)
				{
					var settings = profile.OrgSecuritySettings.OfType<OrgSecurityProfileSetting>().Select(x => Tuple.Create(x.SecurityKey, x.Granted)).Distinct()
						.Where(x => currentSetting.Any(y => y.Item1 == x.Item1))
						.OrderBy(x => x.Item1);

					if (currentSetting.SequenceEqual(settings))
					{
						securityProfile = profile.Name;
					}
				}
			}

			if (securityProfile == CustomProfileCode)
			{
				CustomSettings = new Dictionary<ZString, ZBool>(SecurityRights.OfType<OrgSecurity>().ToDictionary(x => x.SecurityKey, x => x.OX_GrantedForWeb));
			}
		}

		IReadOnlyDictionary<ZString, ZBool> CustomSettings = new Dictionary<ZString, ZBool>(0);

		#region Default Security

		public SecurityView SecurityRights { get; private set; }

		public class SecurityView : OrgSecurityCollectionView
		{
			public SecurityView(OrgSecurityCollection collectionToFilter) : base(collectionToFilter)
			{
			}

			protected override bool IsThisPartOfTheCollection(BusinessObject element)
			{
				return base.IsThisPartOfTheCollection(element) && (element as OrgSecurity).OX_IsCustomerManaged;
			}
		}

		#endregion Default Security

		#region Contact Security

		public OrgContact SelectedOrgContact { get; private set; }

		public ContactSecurityView ContactSecurityRights => SelectedOrgContact != null ? ContactSecurityViewMap.GetOrAdd(SelectedOrgContact.PK,
			() =>
			{
				SelectedOrgContact.SecurityRightsForBindingOnly.Sort(OrgSecurityContacts.Schema.SecurityItemName);
				var view = new ContactSecurityView(SelectedOrgContact.SecurityRightsForBindingOnly);
				return view;
			}) : null;

		readonly Dictionary<ZGuid, ContactSecurityView> ContactSecurityViewMap = new Dictionary<ZGuid, ContactSecurityView>();

		public void SelectContact(OrgContact contact) => SelectedOrgContact = contact;

		public class ContactSecurityView : OrgSecurityContactsCollectionView
		{
			public ContactSecurityView(OrgSecurityContactsCollection collectionToFilter) : base(collectionToFilter)
			{
			}

			public new OrgSecurityContactsCollection CollectionToFilter => (OrgSecurityContactsCollection)base.CollectionToFilter;

			protected override bool IsThisPartOfTheCollection(BusinessObject element)
			{
				return (element as OrgSecurityContacts)?.Security?.OX_IsCustomerManaged ?? ZBool.False;
			}
		}

		#endregion Contact Security

		#region Profiles

		public ZString SecurityProfile
		{
			get { return securityProfile; }
			set
			{
				SetNonPersistentPropertyValue(SecurityProfileInfo, ref securityProfile, value);

				if (securityProfile == CustomProfileCode && CustomSettings.Any())
				{
					foreach (OrgSecurity security in SecurityRights)
					{
						if (CustomSettings.TryGetValue(security.SecurityKey, out var granted))
						{
							security.OX_GrantedForWeb = granted;
						}
					}
					return;
				}

				var profile = OrganisationRegistry.Instance.WebSecurityDefaultValues.Value.OfType<OrgSecurityProfile>().FirstOrDefault(x => x.Name == value && x.Published);
				if (profile != null)
				{
					var settings = profile.OrgSecuritySettings.OfType<OrgSecurityProfileSetting>().GroupBy(x => x.SecurityKey).ToDictionary(x => x.Key, y => y.First());

					foreach (OrgSecurity security in SecurityRights)
					{
						if (settings.TryGetValue(security.SecurityKey, out var setting))
						{
							security.OX_GrantedForWeb = setting.Granted;
						}
					}
				}
			}
		}

		ZString securityProfile;

		public ZPropertyInfo SecurityProfileInfo => GetZPropertyInfo(Schema.SecurityProfile);

		public CodeDescriptionPairList SecurityProfileList { get; private set; }

		#endregion Profiles

		#region Bulk Update

		BulkUpdateMode allContacts;
		BulkUpdateMode selectedContacts;

		public bool IsUpdateAllContacts { get { return allContacts?.Selected ?? false; } }
		public bool IsUpdateSelectedContacts { get { return selectedContacts?.Selected ?? false; } }

		public BulkUpdateModeCollection BulkUpdateModeCollection
		{
			get
			{
				if (bulkUpdateModes == null)
				{
					bulkUpdateModes = new BulkUpdateModeCollection();
					selectedContacts = bulkUpdateModes.Add(Res.GetString("9A29D398-C641-48BE-82A0-C7C75BD82B7E", "Apply to selected"));
					selectedContacts.Selected = true;
					allContacts = bulkUpdateModes.Add(Res.GetString("38DE652C-5998-40E9-BC96-E8F39CB649BA", "Apply to all (could be slow)"));
				}
				return bulkUpdateModes;
			}
		}
		BulkUpdateModeCollection bulkUpdateModes;

		public SecurityProfile BulkUpdateProfile { get; private set; }

		public void BulkUpdateSelected(OrgContactCollection contacts, IEnumerable<ZGuid> contactPks)
		{
			if (IsUpdateAllContacts)
			{
				return;
			}

			var settings = BulkUpdateProfile.Items.OfType<SecurityItem>().Where(x => !x.Skip).GroupBy(x => x.SecurityKey).ToDictionary(x => x.Key, y => y.First().Granted);
			var orgSecurity = Master.SecurityRightsView.Cast<OrgSecurity>().ToDictionary(x => x.SecurityKey, y => y);
			var sameAsOrg = GetSecuritiesSameAsOrg(settings);

			foreach (var pk in contactPks)
			{
				var contact = (OrgContact)contacts.FindByPK(pk);

				ApplySecurityToContact(settings, orgSecurity, sameAsOrg, contact);
			}

			HasBulkUpdated = true;
		}

		public void BulkUpdateAll(ZQuery query)
		{
			if (IsUpdateSelectedContacts)
			{
				return;
			}

			var reader = new FilteredBusinessObjectReader(new BusinessObjectFactoryProvider(), query, typeof(OrgContact));
			reader.BatchSize = 100;
			reader.SaveBeforeLoadNextEnabled = true;

			var settings = BulkUpdateProfile.Items.OfType<SecurityItem>().Where(x => !x.Skip).GroupBy(x => x.SecurityKey).ToDictionary(x => x.Key, y => y.First().Granted);
			var orgSecurity = Master.SecurityRightsView.Cast<OrgSecurity>().ToDictionary(x => x.SecurityKey, y => y);

			var sameAsOrg = GetSecuritiesSameAsOrg(settings);

			foreach (OrgContact contact in reader)
			{
				ApplySecurityToContact(settings, orgSecurity, sameAsOrg, contact);
			}

			HasBulkUpdated = true;
		}

		List<string> GetSecuritiesSameAsOrg(Dictionary<ZString, ZBool> settings)
		{
			var sameAsOrg = new List<string>();
			foreach (var security in Master.SecurityRightsView.OfType<OrgSecurity>())
			{
				if (settings.TryGetValue(security.SecurityKey, out var granted))
				{
					if (granted == security.OX_Granted)
					{
						sameAsOrg.Add(security.SecurityKey);
					}
				}
			}

			return sameAsOrg;
		}

		void ApplySecurityToContact(Dictionary<ZString, ZBool> settings, Dictionary<ZString, OrgSecurity> orgSecurities, List<string> sameAsOrg,OrgContact contact)
		{
			// if exists on contact and is different
			//		if same as org, delete
			//		if diff to org, update
			// if not exists on contact and same as org, skip
			// if not exists on contact and different to org, create

			var processedKeys = new List<string>(sameAsOrg);
			var toDelete = new List<OrgSecurityContacts>();
			foreach (var security in contact.SecurityRightsNoDummies.OfType<OrgSecurityContacts>())
			{
				if (settings.TryGetValue(security.Security.SecurityKey, out var granted))
				{
					if (orgSecurities.TryGetValue(security.Security.SecurityKey, out var orgSecurity) && orgSecurity.OX_Granted == granted)
					{
						toDelete.Add(security);
					}
					else
					{
						security.OZ_Granted = granted;
					}

					processedKeys.Add(security.Security.SecurityKey);
				}
			}

			for (int i = 0; i < toDelete.Count; i++)
			{
				toDelete[i].Delete();
			}

			foreach (var setting in settings.Where(s => !processedKeys.Contains(s.Key)))
			{
				OrgSecurityContacts newSecurity = contact.Factory.New<OrgSecurityContacts>();
				using (newSecurity.SuspendSettingHasChanges())
				{
					using (newSecurity.GetValidationSuspender())
					{
						newSecurity.OZ_OC = contact.PK;
						newSecurity.OZ_OX = orgSecurities[setting.Key].PK;
						newSecurity.OZ_Granted = setting.Value;
					}
				}
			}
		}

		bool HasBulkUpdated;

		public void InitBulkUpdate(int allContactsCount, int selectedContactsCount)
		{
			if (allContacts != null)
			{
				allContacts.RecordsAffected = allContactsCount;
			}

			if (selectedContacts != null)
			{
				selectedContacts.RecordsAffected = selectedContactsCount;
			}

			if (!HasBulkUpdated)
			{
				CreateOrUpdateBulkUpdateProfile();
			}
			BulkUpdateProfile.CreateSnapshot();
			BulkUpdateProfile.CustomSettings = CustomSettings;
		}

		public void CancelBulkUpdate() => BulkUpdateProfile.RestoreSnapshot();

		void CreateOrUpdateBulkUpdateProfile()
		{
			if (BulkUpdateProfile == null)
			{
				BulkUpdateProfile = new SecurityProfile(Factory);
			}

			BulkUpdateProfile.Items.RemoveAndDeleteAll();
			BulkUpdateProfile.ProfileName = CustomProfileCode;
			BulkUpdateProfile.Items.AddRange(SecurityRights.OfType<OrgSecurity>()
				.Select(x => new SecurityItem(Factory) { SecurityKey = x.SecurityKey, SecurityItemName = x.SecurityItemNameForDisplay, Granted = x.OX_GrantedForWeb }).OrderBy(x => x.SecurityItemName));
		}

		#endregion Bulk Update

		public void RefreshAfterSave()
		{
			var query = new ZQuery(OrgContactSchema.OC_OH, Master.PK);
			query.FetchOnlyFromLocalCache = true;
			foreach (var contact in Master.Factory.Load<OrgContact>(query))
			{
				contact.RefreshSecurityRights();
			}

			ContactSecurityViewMap.Clear();
			UpdateSecurityProfile();
		}
	}

	#region Helper Classes

	public class SecurityItem : NonPersistentBusinessObject
	{
		public SecurityItem(BusinessObjectFactory factory) : base(factory)
		{
			Skip = true;
		}

		public abstract class Schema
		{
			public const string SecurityKey = "SecurityKey";
			public const string SecurityItemName = "SecurityItemName";
			public const string Granted = "Granted";
			public const string Skip = "Skip";
		}

		[BusinessObjectTestExclude]
		public ZString SecurityKey { get; set; }

		public ZPropertyInfo SecurityKeyInfo => GetZPropertyInfo(Schema.SecurityKey);

		[BusinessObjectTestExclude]
		public ZString SecurityItemName { get; set; }

		public ZPropertyInfo SecurityItemNameInfo => GetZPropertyInfo(Schema.SecurityItemName);

		[BusinessObjectTestExclude]
		public ZBool Granted { get; set; }

		public ZPropertyInfo GrantedInfo => GetZPropertyInfo(Schema.Granted);

		[BusinessObjectTestExclude]
		public ZBool Skip { get; set; }

		public ZPropertyInfo SkipInfo => GetZPropertyInfo(Schema.Skip);
	}

	public class SecurityItemCollection : NonPersistentBusinessObjectCollection<SecurityItem>
	{
		public SecurityItemCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new SecurityItem(Factory);
	}

	public class SecurityProfile : NonPersistentBusinessObject
	{
		public SecurityProfile(BusinessObjectFactory factory) : base(factory)
		{
			Items = new SecurityItemCollection(factory);
		}

		public ZString ProfileName
		{
			get { return profileName; }
			set
			{
				SetNonPersistentPropertyValue(ProfileNameInfo, ref profileName, value);

				if (profileName == CustomProfileCode && CustomSettings.Any())
				{
					foreach (SecurityItem item in Items)
					{
						if (CustomSettings.TryGetValue(item.SecurityKey, out var granted))
						{
							item.Granted = granted;
						}
					}
					return;
				}

				if (Items.Any())
				{
					var profile = OrganisationRegistry.Instance.WebSecurityDefaultValues.Value.OfType<OrgSecurityProfile>().FirstOrDefault(x => x.Name == value && x.Published);
					if (profile != null)
					{
						var settings = profile.OrgSecuritySettings.OfType<OrgSecurityProfileSetting>().GroupBy(x => x.SecurityKey).ToDictionary(x => x.Key, y => y.First());

						foreach (SecurityItem item in Items)
						{
							if (settings.TryGetValue(item.SecurityKey, out var setting))
							{
								item.Granted = setting.Granted;
								item.Skip = false;
							}
						}
					}
				}
			}
		}

		ZString profileName;

		public ZPropertyInfo ProfileNameInfo => GetZPropertyInfo(nameof(ProfileName));

		public SecurityItemCollection Items { get; private set; }

		public IReadOnlyDictionary<ZString, ZBool> CustomSettings { get; set; } = new Dictionary<ZString, ZBool>(0);

		const string CustomProfileCode = "";

		#region Snapshot

		public void CreateSnapshot()
		{
			Snapshot.Clear();
			Snapshot.Add(() => profileName = CustomProfileCode);
			Snapshot.AddRange(Items.Cast<SecurityItem>().Select(x => CreateAction(x, x.Granted, x.Skip)));
		}

		public void RestoreSnapshot()
		{
			Snapshot.ForEach(x => x());
			Snapshot.Clear();
		}

		readonly List<Action> Snapshot = new List<Action>();

		static Action CreateAction(SecurityItem item, ZBool granted, ZBool skip) => () => { item.Granted = granted; item.Skip = skip; };

		#endregion Snapshot
	}

	#endregion Helper Classes
}
