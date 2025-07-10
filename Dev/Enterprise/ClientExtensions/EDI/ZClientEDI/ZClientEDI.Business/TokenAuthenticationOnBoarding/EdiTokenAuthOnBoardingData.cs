using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business
{
	public class EdiTokenAuthOnBoardingData : AutoEdiTokenAuthOnBoardingData
	{
		public const string AzureDefaultConfigurationIdentifier = "Azure";

		ZString? fVerificationResultDetails;

		public ZString VerificationResultDetails
		{
			get => fVerificationResultDetails ?? ZString.Empty;
			set
			{
				fVerificationResultDetails = value;
				VerificationResultDetailsInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateVerificationResultDetails();
				}
			}
		}

		public ZPropertyInfo VerificationResultDetailsInfo => GetZPropertyInfo(nameof(VerificationResultDetails));

		public ZString VerificationResult
		{
			get
			{
				switch (fVerificationResultDetails?.IsEmpty)
				{
					case null: return EdiTokenAuthOnBoardingDataLookups.VerificationResult.NotVerified;
					case true: return EdiTokenAuthOnBoardingDataLookups.VerificationResult.Success;
					default: return EdiTokenAuthOnBoardingDataLookups.VerificationResult.Failed;
				}
			}
		}

		public ZPropertyInfo VerificationResultInfo =>  GetWrappedZPropertyInfo(nameof(VerificationResult), x => VerificationResultDetailsInfo);

		[List("AzureB2CEnvironmentCodeDescriptionList")]
		public ZString Environment
		{
			get => environment;
			set
			{
				SetNonPersistentPropertyValue(EnvironmentInfo, ref environment, value);
			}
		}

		public ZPropertyInfo EnvironmentInfo => GetZPropertyInfo(nameof(Environment));

		ZString environment;

		public CodeDescriptionPairList AzureB2CEnvironmentCodeDescriptionList
		{
			get
			{
				if (azureB2CEnvironmentCodeDescriptionList == null)
				{
					azureB2CEnvironmentCodeDescriptionList = new CodeDescriptionPairList();
					EDIDataRegistry.Instance.AzureOpenIDConnectConfiguration.Value
						.OfType<AzureOpenIDConnectConfiguration>().
						ForEach(configure => azureB2CEnvironmentCodeDescriptionList.AddPair(configure.Code.ToString(), new AzureB2CEnvironmentCodeDescriptionList().GetMultilingualDescriptionFromCode(configure.Code.ToString())));
				}
				return azureB2CEnvironmentCodeDescriptionList;
			}
		}

		CodeDescriptionPairList azureB2CEnvironmentCodeDescriptionList;

		public AzureOpenIDConnectConfiguration ApplicationManagement => EDIDataRegistry.Instance.AzureOpenIDConnectConfiguration.Value.OfType<AzureOpenIDConnectConfiguration>().FirstOrDefault(x => x.Code == Environment);

		public EdiTokenAuthOnBoardingData(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override void OnSaving()
		{
			if (TOD_StatusInfo.HasChanges)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, $"Changed {TOD_StatusInfo.HumanReadableName} from {TOD_StatusInfo.OriginalValue} to {TOD_Status}.");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
			base.OnSaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded)
			{	// We need to delete unsaved added logs, otherwise they will be saved at next transaction
				Logs.LogsNotInDB.Where(x => x.SL_Parent == PK).DeleteAll();
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			var row = ((IBusinessObjectInternals)this).Row;
			row[EdiTokenAuthOnBoardingDataSchema.Constants.TOD_ConfigurationIdentifier] = AzureDefaultConfigurationIdentifier;
		}

		public bool TryGetOidcConfig(out OIDCConfig oidcConfig, out string domainHint)
		{
			Validation.ValidateTOD_ClaimMappingIdentifier();
			Validation.ValidateTOD_ClaimMappingName();
			Validation.ValidateTOD_ConfigurationIdentifier();
			Validation.ValidateTOD_OIDCServer();
			Validation.ValidateTOD_SystemUniqueIdentifier();
			Validation.ValidateTOD_IDT();

			if (Notifications.HasErrors())
			{
				oidcConfig = null;
				domainHint = null;
				return false;
			}

			oidcConfig = GetOidcConfig();
			domainHint = TOD_ConfigurationIdentifier;
			return true;
		}

		OIDCConfig GetOidcConfig()
		{
			var oidcConfig = new OIDCConfig
			{
				ClaimsMappings =
				{
					new OIDCClaimsMapping
					{
						ClaimName = TOD_ClaimMappingName,
						Identifier = TOD_ClaimMappingIdentifier,
					},
				},
				IsOIDCEnabled = true,
				IsVerified = false,
				OIDCServerTypeCode = OIDCServerTypesList.Codes.Azure,
				Scopes =
				{
					new OIDCScope
					{
						ScopeName = string.Empty
					},
				},
			};

			if (IsProdEnvironment)
			{
				oidcConfig.AuthorityURL = Tenant.IDT_AuthorityUrl;
				oidcConfig.ClientIdentifier = Tenant.IDT_OidcClientId;
				oidcConfig.Scopes[0].ScopeName = Tenant.IDT_OidcClientId;
			}
			else
			{
				oidcConfig.AuthorityURL = ApplicationManagement.AuthorityUrl;
				oidcConfig.ClientIdentifier = ApplicationManagement.ClientID;
				oidcConfig.Scopes[0].ScopeName = ApplicationManagement.ClientID;
			}

			return oidcConfig;
		}

		[RelatedBusinessObject(nameof(Incident))]
		[List(nameof(Lookups) + "." + nameof(EdiTokenAuthOnBoardingDataLookups.RelatedIncidents))]
		public override ZGuid TOD_IM
		{
			get
			{
				return	base.TOD_IM;
			}
			set
			{
				base.TOD_IM = value;
				base.TOD_LE = Incident?.EnterprisePK ?? ZGuid.Empty;
			}
		}

		[RelatedBusinessObject(nameof(LicenceEnterprise))]
		[List("Lookups.LicenceEnterpriseList")]
		public override ZGuid TOD_LE => base.TOD_LE;

		[RelatedBusinessObject(nameof(Tenant))]
		[List("Lookups.Tenants")]
		public override ZGuid TOD_IDT
		{
			get
			{
				return base.TOD_IDT;
			}
			set
			{
				base.TOD_IDT = value;
			}
		}

		public override ZString TOD_ClaimMappingName
		{
			get => base.TOD_ClaimMappingName;
			set
			{
				fVerificationResultDetails = null;
				base.TOD_ClaimMappingName = value;
			}
		}

		[List(nameof(Lookups) + "." + nameof(EdiTokenAuthOnBoardingDataLookups.ClaimMappingIdentifiersList))]
		public override ZString TOD_ClaimMappingIdentifier
		{
			get => base.TOD_ClaimMappingIdentifier;
			set
			{
				fVerificationResultDetails = null;
				base.TOD_ClaimMappingIdentifier = value;
			}
		}

		public override ZString TOD_ConfigurationIdentifier
		{
			get => base.TOD_ConfigurationIdentifier;
			set
			{
				fVerificationResultDetails = null;
				base.TOD_ConfigurationIdentifier = value;
			}
		}

		[List(nameof(Lookups) + "." + nameof(EdiTokenAuthOnBoardingDataLookups.OIDCServerTypesList))]
		public override ZString TOD_OIDCServer
		{
			get => base.TOD_OIDCServer;
			set
			{
				fVerificationResultDetails = null;
				base.TOD_OIDCServer = value;
			}
		}

		[List(nameof(Lookups) + "." + nameof(EdiTokenAuthOnBoardingDataLookups.OnBoardingStatusList))]
		public override ZString TOD_Status => base.TOD_Status;

		public override ZString TOD_SystemUniqueIdentifier
		{
			get => base.TOD_SystemUniqueIdentifier;
			set
			{
				fVerificationResultDetails = null;
				base.TOD_SystemUniqueIdentifier = value;
			}
		}

		// Dev note
		// This ensures that the controls in the Create/Edit form are not enabled.
		public bool TOD_ConfigurationIdentifier_ReadOnly => RelevantFieldReadOnly;
		public bool TOD_OIDCServer_ReadOnly => RelevantFieldReadOnly;
		public bool TOD_SystemUniqueIdentifier_ReadOnly => RelevantFieldReadOnly;
		public bool TOD_Status_ReadOnly => true;
		public bool TOD_IDT_ReadOnly => RelevantFieldReadOnly;
		public bool TOD_LE_ReadOnly => true;

		bool RelevantFieldReadOnly => TOD_Status != OnBoardingStatuses.Codes.New && TOD_Status != OnBoardingStatuses.Codes.Error;

		bool IsProdEnvironment => Environment == Registry.Business.AzureB2CEnvironmentCodeDescriptionList.Codes.PRD;

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore => Res.GetString("0DE208D4-3E9F-4082-8B4F-1E322F7164FF", "Token Authentication Onboarding Data");

		// Dev note
		// Keep the trailing space, as we do not have a human readable key.
		// This will allow to have multiple shortcuts with the same description but different ID in the Recent Item list
		protected override ZString HumanReadableShortcutNameCore => $"{HumanReadableName} - {Incident?.Number} ";

		public LicenceEnterprise LicenceEnterprise => Factory.Load<LicenceEnterprise>(TOD_LE);

		public SupportIncident Incident => Factory.Load<SupportIncident>(TOD_IM);

		public EdiIdentityTenant Tenant => Factory.Load<EdiIdentityTenant>(TOD_IDT);

		public ZString LicenceEnterpriseCode => LicenceEnterprise?.LE_EnterpriseCode ?? ZString.Empty;
	}
}
