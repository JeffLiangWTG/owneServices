using System;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security.ActiveDirectory
{
	public class ADConfigRegistryItem : StronglyTypedRegistryItem<ADConfig>
	{
		public ADConfigRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint,
			RegistryStorageFlags storage, RegistryOptions options, ADConfig defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ADConfigRegistryDataType(defaultValue), storage, options))
		{
			OnUpdateAction = (companyPK, branchPK, departmentPK, newvalue) => ActivatorDirectorSaveChanges();
		}

		void ActivatorDirectorSaveChanges()
		{
			var activationDirector = ((ADConfigRegistryDataType)DataType).ActivationDirector;
			if (activationDirector.HasChanges)
			{
				activationDirector.SaveChanges();
			}
		}

		protected override void DeleteValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			DataType.Validate(this, ADConfig.DefaultValue, Guid.Empty, Guid.Empty, Guid.Empty);
			base.DeleteValueCore(companyPK, branchPK, departmentPK);
		}

#if DEBUG
		public override string ToString()
		{
			return this.GetType().Name; // Because Rhino Mocks likes to call ToString in failing tests, which is not supported upstream.
		}
#endif
	}

	[RegistryEditor(ADConfigRegistryEditorInfo.FullyQualifiedEditorClassAndAssembly)]
	public class ADConfigRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ADConfig>
	{
		public ADConfigRegistryDataType(ADConfig defaultValue)
			: base(defaultValue)
		{
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new ADConfigRegistryEditorInfo();
		}

		protected override ADConfig CloneValue(ADConfig value)
		{
			return value.Clone();
		}

		protected override bool HasDefaultEditorInfoCore
		{
			get { return true; }
		}

		protected override void ValidateBeforeRegistryFormSaveCore(IRegistryItem registryItem, ADConfig proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateBeforeRegistryFormSaveCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			var currentValue = (ADConfig)registryItem.Value;
			try
			{
				if (currentValue != null && proposedValue != null && (currentValue.IsADIntegrationEnabled != proposedValue.IsADIntegrationEnabled || (currentValue.EntitiesToSyncCode != proposedValue.EntitiesToSyncCode)))
				{
					if (proposedValue.IsADIntegrationEnabled)
					{
						//AD already ON and changing sync entity 
						if (currentValue.IsADIntegrationEnabled)
						{
							// From All -> USR
							if (proposedValue.EntitiesToSyncCode == EntitiesToSyncList.Codes.Users)
							{
								//Disable Group Sync
								ActivationDirector.DisableIntegration(true);
							}
						}

						//AD is currently OFF or changing sync entity from USR to ALL
						if (!currentValue.IsADIntegrationEnabled || (currentValue.EntitiesToSyncCode != EntitiesToSyncList.Codes.UsersAndGroups))
						{
							ActiveDirectoryRegistry.Instance.ActiveConfigOverride = proposedValue;
							try
							{
								ActivationDirector.EnableIntegration(proposedValue.EntitiesToSync);
							}
							finally
							{
								ActiveDirectoryRegistry.Instance.ActiveConfigOverride = null;
							}
						}
					}
					else if (currentValue.IsADIntegrationEnabled)
					{
						ActivationDirector.DisableIntegration();
					}
				}
				else if (currentValue != null
					&& proposedValue != null
					&& proposedValue.IsADIntegrationEnabled
					&& (currentValue.SyncMode != proposedValue.SyncMode
					|| currentValue.SyncDirection != proposedValue.SyncDirection
					|| currentValue.SyncDirectionGroup != proposedValue.SyncDirectionGroup))
				{
					ActiveDirectoryRegistry.Instance.ActiveConfigOverride = proposedValue;
					try
					{
						var results = new ActiveDirectoryStabilityChecker().Check(true);
						if (results.Length > 0)
						{
							var message = new ZStringBuilder(Res.GetString("c76d089a-9341-4f59-8107-076f6d9be1ff", "Integration cannot be modified."));
							message.Append(Res.GetString("1394e9c1-5586-4fa2-8d1a-e0b3a7189491", "Please cancel your changes, correct the issues below and save your changes before retrying to modify the integration:"));
							foreach (var result in results)
							{
								message.Append("- " + result.Description);
							}
							throw new DirectoryServicesException(message.ToStringWithNewLineBetweenAppends());
						}
					}
					finally
					{
						ActiveDirectoryRegistry.Instance.ActiveConfigOverride = null;
					}
				}
			}
			catch (DirectoryServicesException ex)
			{
				throw new RegistryValidationException(ex.Message);
			}
			catch (SecurityAccessDeniedException ex)
			{
				throw new RegistryValidationException(ex.Message);
			}
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore
		{
			get { return true; }
		}

		public IADActivationDirector ActivationDirector
		{
			get { return activationManager ?? (activationManager = ObjectFactory.Get<IADActivationDirector>()); }
			set { activationManager = value; }
		}
		IADActivationDirector activationManager;
	}

	[RegistryEditor(ADConfigRegistryEditorInfo.FullyQualifiedEditorClassAndAssembly)]
	public class ADConfigRegistryEditorInfo : RegistryEditorInfo
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Fully qualified type name")]
		internal const string FullyQualifiedEditorClassAndAssembly = "Enterprise.Security.ActiveDirectory.GUI.Registry.ADRegistryControlItemEditor, Enterprise.Security.ActiveDirectory.GUI";

		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(ADConfigRegistryDataType); }
		}
	}
}
