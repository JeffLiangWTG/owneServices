using System;
using System.DirectoryServices;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security.ActiveDirectory.Synchronisation;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security.ActiveDirectory
{
	public abstract class ADEntity<EnterpriseT, ADEntityT> : NonPersistentBusinessObject, IADEntity, IObsoleteValidation
		where EnterpriseT : class, IBusiness, IADLinkedEntity
		where ADEntityT : class, IDirectoryEntry
	{
		protected ADEntity(EnterpriseT enterpriseEntity)
			: base(enterpriseEntity != null ? enterpriseEntity.Factory : null)
		{
			Argument.NotNull(enterpriseEntity, nameof(enterpriseEntity));
			this.enterpriseEntity = enterpriseEntity;
		}

		#region Services

		protected IDirectorySearcher GetDirectorySearcher(bool requireWritePrivileges)
		{
			return DirectorySearcherFactory.GetDirectorySearcher(GetDomainCredentials(), requireWritePrivileges);
		}

		#endregion

		#region Entities

		public ADEntityT GetDirectoryEntry(bool requireDomainWritePrivilege = false)
		{
			if (directoryEntry == null || !retrievedWithWritePrivilege && requireDomainWritePrivilege)
			{
				if (directoryEntry != null && directoryEntry.HasChanges)
				{
					throw new InvalidOperationException("Domain privileges required but operations have been performed under a non-privileged context.");
				}
				directoryEntry = FindDirectoryEntry(GetDirectorySearcher(requireDomainWritePrivilege), DomainCredentials);
				retrievedWithWritePrivilege = requireDomainWritePrivilege;
			}
			return directoryEntry;
		}

		protected void SetDirectoryEntry(ADEntityT directoryEntry)
		{
			this.directoryEntry = directoryEntry;
			if (directoryEntry != null)
			{
				SetGuid(directoryEntry.Guid);
			}
		}

		ADEntityT directoryEntry;
		bool retrievedWithWritePrivilege;

		ADEntityT FindDirectoryEntry(IDirectorySearcher directorySearcher, IDomainCredentials domainCredentials)
		{
			ADEntityT result = null;

			if (EnterpriseEntity != null)
			{
				var guidLinkValue = (ZGuid)GuidPropertyInfo.Value;
				if (guidLinkValue.IsValid)
				{
					result = FindDirectoryEntryCore(directorySearcher, guidLinkValue.ToGuid(), OUPathForEntity(domainCredentials));
				}
				if (result == null && !string.IsNullOrEmpty(EnterpriseIdentity))
				{
					result = FindDirectoryEntryCore(directorySearcher, EnterpriseIdentity, OUPathForEntity(domainCredentials));
				}
			}
			return result;
		}

		protected abstract ADEntityT FindDirectoryEntryCore(IDirectorySearcher searcher, Guid guid, string rootOU);
		protected abstract ADEntityT FindDirectoryEntryCore(IDirectorySearcher searcher, string enterpriseIdentity, string rootOU);

		protected ADEntityT RequireDirectoryEntry(bool requireDomainWritePrivilege = false)
		{
			var directoryEntry = GetDirectoryEntry(requireDomainWritePrivilege);
			if (directoryEntry != null && (!requireDomainWritePrivilege || directoryEntry.CanUpdate))
			{
				return directoryEntry;
			}
			else
			{
				throw new DirectoryServicesException("Could not locate or write to directory entry for entity " + EnterpriseIdentity);
			}
		}

		public EnterpriseT EnterpriseEntity
		{
			get { return enterpriseEntity; }
		}
		readonly EnterpriseT enterpriseEntity;

		IADLinkedEntity IADEntity.EnterpriseEntity
		{
			get { return EnterpriseEntity; }
		}

		#endregion

		#region Properties

		[ResourceStringData("ADEntity.IsActive", Caption = "Active", FullDescription = "Indicates whether this record is active in Active Directory")]
		public ZBool IsActive
		{
			get { return RequireDirectoryEntry().IsActive; }
			set { RequireDirectoryEntry(true).IsActive = value; }
		}

		#endregion

		#region Synchronise

		void Synchronise(SyncMode? preferredSyncMode = null)
		{
			IsSynchronisedSuccessfully = false;
			var isInitialSync = IsInitialSync;
			try
			{
				var directoryEntry = GetDirectoryEntry(RequiresDomainWritePrivilegeToSynchronise);
				var guidLinkValue = (ZGuid)GuidPropertyInfo.Value;

				if (directoryEntry == null)
				{
					if (guidLinkValue.IsValid)
					{
						if (EnterpriseEntity.IsActive)
						{
							// The linked guid is not found in the domain or given OU
							// Note: If the bizo was created recently, the AD object may only be created on one AD Domain Controller Site but has not been replicated to the other Sites yet.
							// This is likely to happen when the Process Controller and CW1 are hosted on different Sites (eg. SYD, AU2).
							// The AD object could be created by either Process Controller or CW1 on one Site but not available to the other before the Sites are replicated.

							var message = Res.GetString("92E0A2DA-6D99-44BA-888E-7D8777BA207F",
									@"Cannot synchronize '{0}'. The linked Active Directory object is not accessible at this time. It is either pending creation, deleted or is located outside the Organizational Unit {1} of domain {2} set in the registry item: {3}.
If '{0}' was created or activated recently please try again later.",
									EnterpriseIdentity,
									OUPathForEntity(DomainCredentials),
									DomainCredentialDomainName,
									((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.DomainCredentialsCollection).LocationMultilingual
								);

							// we throw exception here so the sync stop and user won't be disabled
							throw new DirectoryServicesException(message);
						}
						else
						{
							return;
						}
					}
					else
					{
						if (NeedsToModifyAD)
						{
							if (isInitialSync)
							{
								//When enabling Integration with EnterpriseIsMaster or two-way sync, we set it to ZGuid.Invalid so a new AD user will be created during the next sync by the AD Synchronization service task AFTER AD Integration has been enabled.
								//We should *NOT* create a new AD user while the Integration is being enabled as there is no way to undo the changes if the current user decides to cancel the Integration enabling process.
								GuidPropertyInfo.Value = CreateDirectoryEntryFlagValue;
							}
							else if (guidLinkValue == CreateDirectoryEntryFlagValue)
							{
								// Reload from DB to ensure the bizo is not already linked by another instance 
								((BusinessObject)(IBusiness)EnterpriseEntity).ReloadSafe();
								guidLinkValue = (ZGuid)GuidPropertyInfo.Value;

								if (guidLinkValue == CreateDirectoryEntryFlagValue)
								{
									if (EntityType == DirectoryObjectType.User && DefaultPasswordFailsToMeetCurrentDomainPolicy)
									{
										// We dont want to attempt creating another AD user if the previous creation already throw PasswordDoesntMatchPolicyException
										return;
									}

									DirectoryExceptionHandler.ExecuteWithExceptionHandling(() => directoryEntry = CreateNewDirectoryEntry(), new Type[] { typeof(UsernameTooLongException), typeof(DirectoryServicesException) }, EnterpriseIdentity);
									if (directoryEntry != null)
									{
										preferredSyncMode = SyncMode.EnterpriseIsMaster;
										SetDirectoryEntry(directoryEntry);
									}
								}
								else
								{
									// The AD object was created on another DC Site but not accessible yet during this sync, we quit and do nothing, otherwise the record could be deactivated
									IsSynchronisedSuccessfully = true;
									return;
								}
							}
						}
					}
				}

				if (directoryEntry != null && (guidLinkValue.IsEmpty || guidLinkValue != directoryEntry.Guid))
				{
					SetGuid(directoryEntry.Guid);
				}

				SetEnterpriseEntityDomainNameIfRequired();

				var strategy = GetSyncStrategy(preferredSyncMode, isInitialSync);
				var result = strategy.Synchronise();

				Synchronised?.Invoke(this, result);
				IsSynchronisedSuccessfully = true;
			}
			catch (COMException ex) when (ex.ErrorCode == -2147016646 || ex.ErrorCode == -2147023570)
			{
				// -2147016646 LDAP_SERVER_DOWN: The server is not operational. http://www.selfadsi.org/errorcodes.htm
				// -2147023570 LDAP_INVALID_CREDENTIALS: The user name or password is incorrect. http://www.selfadsi.org/errorcodes.htm
				throw new DirectoryServicesException(ex.Message, ex);
			}
			catch (PasswordDoesNotMatchPolicyException)
			{
				// We swallow this exception here to allow sync of other entities to continue 
				// Since DomainCredentials.DefaultPasswordFailsToMeetDomainPolicy has been flagged, it will be reported in one error later
			}
		}

		public virtual SyncDirection SyncDirection => ActiveDirectoryRegistry.Instance.SyncDirection;

		public virtual SyncMode SyncMode => ActiveDirectoryRegistry.Instance.SyncMode;

		protected bool NeedsToModifyAD => SyncDirection == SyncDirection.TwoWay || SyncMode == SyncMode.EnterpriseIsMaster;

		protected virtual ZGuid CreateDirectoryEntryFlagValue => ZGuid.Invalid;

		protected virtual bool IsInitialSync => GuidPropertyInfo.Value.IsEmpty;

		protected virtual ISyncStrategy GetSyncStrategy(SyncMode? preferredSyncMode, bool isInitialSync)
		{
			return SyncStrategyFactory.GetSyncStrategy(this, preferredSyncMode, isInitialSync);
		}

		protected virtual bool DefaultPasswordFailsToMeetCurrentDomainPolicy => ((DomainCredentials)DomainCredentials)?.DefaultPasswordFailsToMeetDomainPolicy ?? false;

		protected bool IsSynchronisedSuccessfully { get; private set; }

		void SetEnterpriseEntityDomainNameIfRequired()
		{
			var domainNameFromRegistry = DomainCredentials?.DomainName;

			if (string.IsNullOrEmpty(EnterpriseEntity.DomainName) || (!EnterpriseEntity.DomainName.EqualsIgnoringCase(domainNameFromRegistry)))
			{
				EnterpriseEntity.DomainName = domainNameFromRegistry;
			}
		}

		bool IsAMatchedADObjectOutsideTheOU()
		{
			var isFoundInRootOU = FindDirectoryEntryCore(GetDirectorySearcher(false), EnterpriseIdentity, string.Empty) != null;
			var isNotFoundInOU = FindDirectoryEntryCore(GetDirectorySearcher(false), EnterpriseIdentity, OUPathForEntity(DomainCredentials)) == null;
			return isFoundInRootOU && isNotFoundInOU;
		}

		bool RequiresDomainWritePrivilegeToSynchronise
		{
			get
			{
				var isInitialSync = GuidPropertyInfo.Value.IsEmpty;
				var syncMode = SyncMode;
				var syncDirection = SyncDirection;
				var requiresRightsForInitialSync = isInitialSync && (syncDirection == SyncDirection.TwoWay || syncMode == SyncMode.EnterpriseIsMaster);
				var requiresRightsForOngoingSync = !isInitialSync && (syncDirection == SyncDirection.TwoWay && !IsADEntityModifiedAfterEnterprise || syncDirection == SyncDirection.OneWay && syncMode == SyncMode.EnterpriseIsMaster);

				return requiresRightsForInitialSync || requiresRightsForOngoingSync;
			}
		}

		protected virtual ADEntityT CreateNewDirectoryEntryCore(IOrganisationalUnit organisationalUnitEntity, IDirectorySearcher directorySearcherWithWritePrivileges)
		{
			return (ADEntityT)organisationalUnitEntity.CreateNewChild(EnterpriseIdentity, EntityType, directorySearcherWithWritePrivileges);
		}

		protected virtual ADEntityT CreateNewDirectoryEntry()
		{
			var ouPath = OUPathForEntity(DomainCredentials);

			var directorySearcher = GetDirectorySearcher(requireWritePrivileges: true);

			if (IsAMatchedADObjectOutsideTheOU())
			{
				//already a matched AD object somewhere, cannot create
				throw new DirectoryServicesException(string.Format(CultureInfo.InvariantCulture,
					@"Cannot create {0} in domain {1}, the same object already exists and may be located outside the Organizational Unit {2} set in the registry item: {3}",
					EnterpriseIdentity,
					DomainCredentialDomainName,
					OUPathForEntity(DomainCredentials),
					((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.DomainCredentialsCollection).LocationMultilingual
				));
			}

			var organisationalUnitEntity = directorySearcher.FindOrganisationalUnit(ouPath);
			try
			{
				var newChild = CreateNewDirectoryEntryCore(organisationalUnitEntity, directorySearcher);

				// IMPORTANT: Do not remove this commit! It is required before changing other properties, otherwise the Win2kName/sAMAccountName will be missing
				newChild.CommitChanges();

				return newChild;
			}
			catch (DirectoryServicesCOMException ex) when (ex.ErrorCode == ErrorCodeObjectAlreadyExists)
			{
				throw new DirectoryServicesCOMException(ex.Message + System.Environment.NewLine + GetObjectAlreadyExistsErrorMessage());
			}
		}

		const int ErrorCodeObjectAlreadyExists = unchecked((int)0x80071392);    // -2147019886
		const int ErrorCode_NO_SUCH_ATTRIBUTE = unchecked((int)0x8007200A);     // -2147016694
		const int ErrorCode_NO_SUCH_OBJECT = unchecked((int)0x80072030);        // -2147016656
		const int ErrorCode_UNWILLING_TO_PERFORM = unchecked((int)0x80072035);  // -2147016651

		string GetObjectAlreadyExistsErrorMessage()
		{
			var message = Res.GetString("8EEA676E-5D32-4A02-A4A5-DD5CD1F8100F", @"Please check for Active Directory objects with any of these values:
{1}={0}", EnterpriseIdentity, "CN");

			message = message + GetObjectAlreadyExistsErrorMessageCore();
			return message;
		}

		protected virtual string GetObjectAlreadyExistsErrorMessageCore() => string.Empty;

		protected abstract string OUPathForEntity(IDomainCredentials domainCredentials);
		protected abstract DirectoryObjectType EntityType { get; }

		public event EntitySynchronisedEventHandler Synchronised;

		protected virtual void CommitChangesCore()
		{
			var entry = GetDirectoryEntry();
			if (entry != null && entry.HasChanges)
			{
				try
				{
					RequireDirectoryEntry(true).CommitChanges();
				}
				catch (DirectoryServicesException ex)
				{
					if (ex.InnerException is DirectoryServicesCOMException innerEx)
					{
						if (innerEx.ErrorCode == ErrorCode_NO_SUCH_ATTRIBUTE)
						{
							throw new DirectoryServicesException(Res.GetString("C149F31E-9BE6-40DC-AB22-F5B6550BE87B",
								@"One of the attributes in the {0} Registry setting does not exist in the domain. Please review the mappings in the {1} setting in the Registry. If there are any custom attributes used in the mappings, make sure that the custom attributes are defined in all domains.",
								((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.AttributeMapping).CaptionMultilingual,
								((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.AttributeMapping).LocationMultilingual
								));
						}
						if (innerEx.ErrorCode == ErrorCode_NO_SUCH_OBJECT || innerEx.ErrorCode == ErrorCode_UNWILLING_TO_PERFORM)
						{
							throw new DirectoryServicesException(Res.GetString("5A3DB432-4E7B-4B34-BE8F-2BABCF1CAFF0",
								@"One of the attributes in the {0} Registry setting is not writable. Please review the mappings in the {1} setting in the Registry.",
								((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.AttributeMapping).CaptionMultilingual,
								((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.AttributeMapping).LocationMultilingual
							));
						}
					}
					throw;
				}
			}
		}

#if DEBUG
		// For existing UT only, otherwise should call IADEntity.CommitChanges()
		public void CommitChanges()
		{
			CommitChangesCore();
		}
#endif

		#endregion

		#region NonPersistentBusinessObject

		protected override void AddToFactoryCache()
		{
			// Don't cache ADEntities
		}

		#endregion

		public Guid Guid
		{
			get
			{
				if (EnterpriseEntity != null && GuidPropertyInfo.Value.IsValid)
				{
					return ((ZGuid)GuidPropertyInfo.Value).ToGuid();
				}
				else if (GetDirectoryEntry() != null)
				{
					return GetDirectoryEntry().Guid;
				}
				else
				{
					return default;
				}
			}
		}

		protected virtual void SetGuid(Guid value) => GuidPropertyInfo.Value = new ZGuid(value);

		public void DisconnectFromAD()
		{
			EnterpriseEntity.DisconnectFromAD();
		}

		public bool IsIdentityInConflict()
		{
			var searcher = GetDirectorySearcher(false);
			if (EnterpriseEntity.IsADLinked)
			{
				var directoryEntryByIdentity = FindDirectoryEntryCore(searcher, EnterpriseIdentity, string.Empty);
				return directoryEntryByIdentity != null && directoryEntryByIdentity.Guid != (ZGuid)GuidPropertyInfo.Value;
			}
			return false;
		}

		public abstract ZPropertyInfo GuidPropertyInfo { get; }
		public abstract string EnterpriseIdentity { get; }

		public bool IsADEntityModifiedAfterEnterprise
			=> GetDirectoryEntry() != null && (!EnterpriseEntity.SystemLastEditTimeUtc.IsValid || GetDirectoryEntry().LastModified > EnterpriseEntity.SystemLastEditTimeUtc);

		#region IADEntity Members

		IDirectoryEntry IADEntity.GetDirectoryEntry(bool requireDomainWritePrivilege)
		{
			return GetDirectoryEntry(requireDomainWritePrivilege);
		}

		void IADEntity.SetDirectoryEntry(IDirectoryEntry directoryEntry)
		{
			SetDirectoryEntry((ADEntityT)directoryEntry);
		}

		public ZString DomainCredentialDomainName => DomainCredentials?.DomainName ?? "";

		public IDomainCredentials DomainCredentials => _domainCredentials ?? (_domainCredentials = GetDomainCredentials());
		IDomainCredentials _domainCredentials;

		protected virtual IDomainCredentials GetDomainCredentials()
		{
			if (ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value.Count <= 1)
			{
				// If DomainCredentialsCollection registry is not set (Count == 0), DefaultDomainCredentials will be null, so the return value is null, and this covers the backward compatibility (which will use current user's domain and credentials)
				// If DomainCredentialsCollection registry has only 1 domain (Count == 1), return DefaultDomainCredentials regardless
				return ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value.DefaultDomainCredentials;
			}
			else if (EnterpriseEntity.DomainName.IsEmpty)
			{
				return GetDomainContainingMatchingEntry();
			}
			else
			{
				// Domain Credentials of the Domain set in the Staff record
				var result = ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value.Cast<DomainCredentials>().FirstOrDefault(d => d.DomainName.EqualsIgnoringCase(EnterpriseEntity.DomainName))
					?? throw new DirectoryServicesException(Res.GetString("7b792622-8d44-4948-aa9c-9d779364cf3f", "The domain '{0}' of '{1}' is not specified in the registry item: '{2}'",
							EnterpriseEntity.DomainName,
							EnterpriseIdentity,
							((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.DomainCredentialsCollection).LocationMultilingual));

				return result;
			}
		}

		IDomainCredentials GetDomainContainingMatchingEntry()
		{
			//Try default domain credentials first, so if there is more than one match, default domain always win
			var defaultDomain = ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value.DefaultDomainCredentials;
			if (IsInDomain(defaultDomain))
			{
				return defaultDomain;
			}

			foreach (IDomainCredentials domain in ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value.Where<DomainCredentials>(d => d != defaultDomain))
			{
				if (IsInDomain(domain))
				{
					return domain;
				}
			}
			return defaultDomain;

			bool IsInDomain(IDomainCredentials domainCredentials)
			{
				// We get searcher via DirectorySearcherProvider so it can be mocked and tested for multiple domains
				var domainSearcher = ObjectFactory.Get<IDirectorySearcherProvider>().GetDirectorySearcher(domainCredentials, false);
				return FindDirectoryEntry(domainSearcher, domainCredentials) != null;
			}
		}

		public bool HasExistingDirectoryEntry()
		{
			return GetDirectoryEntry() != null;
		}

		bool IADEntity.Synchronise(SyncMode? preferredSyncMode)
		{
			var result = false;
			DirectoryExceptionHandler.ExecuteWithExceptionHandling(
				() =>
				{
					Synchronise(preferredSyncMode);
					result = true; // if Synchronise not throw, ie. sync successful
				}, EnterpriseIdentity);
			return result;
		}

		bool IADEntity.CommitChanges()
		{
			var result = false;
			DirectoryExceptionHandler.ExecuteWithExceptionHandling(
			() =>
			{
				CommitChangesCore();
				result = true;
			}, EnterpriseIdentity);
			return result;
		}

		void IADEntity.DisconnectFromAD()
		{
			DirectoryExceptionHandler.ExecuteWithExceptionHandling(DisconnectFromAD, EnterpriseIdentity);
		}

		#endregion
	}
}
