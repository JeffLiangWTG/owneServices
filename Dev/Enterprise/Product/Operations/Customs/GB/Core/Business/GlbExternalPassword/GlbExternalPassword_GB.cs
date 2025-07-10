using System.ComponentModel;
using System.Data;
using System.Globalization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration.Customs.GB;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.GB.Business.GbConstants;

namespace Enterprise.Customs.GB.Business
{
	[SystemDefinedValues]
	public class GlbExternalPassword_GB : GlbExternalPassword, IGlbExternalPassword_GB
	{
		public GlbExternalPassword_GB(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : GlbExternalPassword.Schema
		{
			public const string IsTokenForCDS = "IsTokenForCDS";
			public const string IsTokenForEMCS = "IsTokenForEMCS";
			public const string IsTokenForGVMS = "IsTokenForGVMS";
			public const string IsTokenForNCTS = "IsTokenForNCTS";
			public const string IsTokenForSnSGB = "IsTokenForSnSGB";
			public const string IsTokenForAll = "IsTokenForAll";
		}

		ZString GetUrlRequestPart(string requestPart)
		{
			return new RefSysConfig.Loader(Factory).GetStringValue(requestPart);
		}

		bool IsValidAndExpired => base.GP_PasswordStatus == PasswordStatusList.Codes.Valid && GP_ExpiryDate < ZDateTime.UtcNow;

		public ZString GetUrl()
		{
			var result = string.Empty;
			IProductRegistrationKey registrationKey = ObjectFactory.Get<IProductRegistration>()?.Key;
			if (registrationKey != null)
			{
				var isLive = registrationKey.DatabaseType == DatabaseTypes.Codes.Production;
				var isCDSPilotMode = GBCustomsDataRegistry.Instance.CDSPilotMode.Value;
				var baseUrl = GetUrlRequestPart(isLive ? CredentialRefSysConfigCodes.BaseUrlLive : CredentialRefSysConfigCodes.BaseUrlTest);
				var baseUrlPath = GetUrlRequestPart(CredentialRefSysConfigCodes.BaseUrlPath);
				var clientId = GetUrlRequestPart(isLive ? CredentialRefSysConfigCodes.ClientIdLive : isCDSPilotMode ? CredentialRefSysConfigCodes.ClientIdDev : CredentialRefSysConfigCodes.ClientIdTest);
				var callbackUrl = GetUrlRequestPart(isLive || !isCDSPilotMode ? CredentialRefSysConfigCodes.CallbackUrlLive : CredentialRefSysConfigCodes.CallbackUrlTest);
				var licenceCode = registrationKey.EnterpriseCode + registrationKey.ServerCode;
				var credentialId = string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", licenceCode, EORI, Badge);

				result = string.Format(CultureInfo.InvariantCulture, "{0}{1}", baseUrl, baseUrlPath.Replace("[WTGClientID]", clientId).Replace("[CredentialID]", credentialId).Replace("[MoreScopes]", string.Empty).Replace("[CallbackUrl]", callbackUrl));
			}
			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypesList.Codes.CDS;
			GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			GP_GC = ZGuid.Empty;
		}

		[ReadOnly(true)]
		public ZString Status
		{
			get { return (IsValidAndExpired ? new ZString(PasswordStatusList.Codes.Invalid) : base.GP_PasswordStatus); }
			set { base.GP_PasswordStatus = value; }
		}

		[ReadOnly(true)]
		public ZString StatusMessage
		{
			get { return (IsValidAndExpired ? new ZString(StatusDescriptions.ExpiredAccessToken) : base.GP_StatusReason); }
			set { base.GP_StatusReason = value; }
		}

		[List(nameof(Lookups) + "." + nameof(GlbExternalPasswordLookups_GB.EORIs))]
		[MaxLength(14)]
		public ZString EORI
		{
			get
			{
				var safe = base.GP_UserID + ".";
				return safe.Split('.')[0];
			}
			set
			{
				var newValue = value.Replace(".", "") + "." + Badge;
				if (base.GP_UserID != newValue)
				{
					CheckMaximumLength(EORIInfo, value);
					base.GP_UserID = newValue;
					GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateEORI();
				}
				EORIInfo.RefreshBinding();
			}
		}

		public void Authorise()
		{
		}

		public ZPropertyInfo EORIInfo
		{
			get { return GetZPropertyInfo(nameof(EORI)); }
		}

		[List(nameof(Lookups) + "." + nameof(GlbExternalPasswordLookups_GB.BadgeCodes))]
		[MaxLength(3)]
		public ZString Badge
		{
			get
			{
				var safe = base.GP_UserID + ".";
				return safe.Split('.')[1];
			}
			set
			{
				var newValue = EORI + "." + value.Replace(".", "");
				if (base.GP_UserID != newValue)
				{
					CheckMaximumLength(BadgeInfo, value);
					base.GP_UserID = newValue;
					GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateBadge();
				}
				BadgeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BadgeInfo
		{
			get { return GetZPropertyInfo(nameof(Badge)); }
		}

		public ZBool IsTokenForAll
		{
			get => IsTokenForCDS && IsTokenForEMCS && IsTokenForGVMS && IsTokenForNCTS && IsTokenForSnSGB;
			set
			{
				var oldValue = IsTokenForAll;
				IsTokenForCDS = IsTokenForEMCS = IsTokenForGVMS = IsTokenForNCTS = IsTokenForSnSGB = value;
				IsTokenForAllInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo IsTokenForAllInfo => GetZPropertyInfo(Schema.IsTokenForAll);

		public ZBool IsTokenForCDS
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.IsTokenForCDS);
			set
			{
				var oldValue = IsTokenForCDS;
				this.SetSystemDefinedValue(Schema.IsTokenForCDS, value);
				IsTokenForCDSInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo IsTokenForCDSInfo => GetZPropertyInfo(Schema.IsTokenForCDS);

		public ZBool IsTokenForEMCS
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.IsTokenForEMCS);
			set
			{
				var oldValue = IsTokenForEMCS;
				this.SetSystemDefinedValue(Schema.IsTokenForEMCS, value);
				IsTokenForEMCSInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo IsTokenForEMCSInfo => GetZPropertyInfo(Schema.IsTokenForEMCS);

		public ZBool IsTokenForGVMS
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.IsTokenForGVMS);
			set
			{
				var oldValue = IsTokenForGVMS;
				this.SetSystemDefinedValue(Schema.IsTokenForGVMS, value);
				IsTokenForGVMSInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo IsTokenForGVMSInfo => GetZPropertyInfo(Schema.IsTokenForGVMS);

		public ZBool IsTokenForNCTS
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.IsTokenForNCTS);
			set
			{
				var oldValue = IsTokenForNCTS;
				this.SetSystemDefinedValue(Schema.IsTokenForNCTS, value);
				IsTokenForNCTSInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo IsTokenForNCTSInfo => GetZPropertyInfo(Schema.IsTokenForNCTS);

		public ZBool IsTokenForSnSGB
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.IsTokenForSnSGB);
			set
			{
				var oldValue = IsTokenForSnSGB;
				this.SetSystemDefinedValue(Schema.IsTokenForSnSGB, value);
				IsTokenForSnSGBInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo IsTokenForSnSGBInfo => GetZPropertyInfo(Schema.IsTokenForSnSGB);

		protected override GlbExternalPasswordValidation GetNewValidation()
		{
			return new GlbExternalPasswordValidation_GB(this);
		}

		public new GlbExternalPasswordValidation_GB Validation => (GlbExternalPasswordValidation_GB)base.Validation;

		public ZBool IsInvalid => Status == PasswordStatusList.Codes.Invalid;

		protected override GlbExternalPasswordLookups GetNewLookups()
		{
			return new GlbExternalPasswordLookups_GB(this);
		}
	}
}
