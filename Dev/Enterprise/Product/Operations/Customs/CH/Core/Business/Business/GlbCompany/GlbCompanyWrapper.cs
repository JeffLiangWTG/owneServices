using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.CH;
using CHPasswordStatusList = Enterprise.Customs.CH.Business.UniversalReferenceConstants.PasswordStatusList;

namespace Enterprise.Customs.CH.Business;

public class GlbCompanyWrapper : MasterFiles.Business.GlbCompanyWrapper, ICHGlbCompanyWrapper
{
	protected GlbCompanyWrapper(GlbCompany company)
		: base(company)
	{
		company.CredentialDataCreator = () => new GlbCompanyCredentialData(MessagingConstants.xTCustomConfiguration.Names.RegistrationNumber, MessagingConstants.xTCustomConfiguration.InterchangeTypes, new[] { company.GC_CustomsRegistrationNoInfo }, CreateCredential);
	}

	protected object CreateCredential(GlbCompany company)
	{
		return new Group()
		{
			Type = Constants.GroupTypes.CompanyType,
			Reference = company.GC_Code,
			Status = !company.IsDeleted && !company.GC_CustomsRegistrationNo.IsEmpty
				? MessagingConstants.xTCustomConfiguration.Status.Update
				: MessagingConstants.xTCustomConfiguration.Status.Delete,
			Items = new object[] { CredentialSender.CreateCredential(string.Empty, company.GC_CustomsRegistrationNo, string.Empty) }
		};
	}

	#region GlbExternalPassword

	[ChildEditable]
	public GlbCompanyCredential GlbExternalPassword
	{
		get
		{
			if (glbExternalPassword == null)
			{
				glbExternalPassword = GetGlbExternalPasswordOrCreateNew<GlbCompanyCredential>(PasswordTypesList.Codes.CHC);
				RegisterEditableChildObject(glbExternalPassword);
			}

			return glbExternalPassword;
		}
	}
	GlbCompanyCredential glbExternalPassword;

	public GlbCompanyTokenCredentials TokenCredentials
	{
		get
		{
			if (tokenCredentials == null || tokenCredentials.IsDeleted)
			{
				tokenCredentials = GetGlbExternalPassword<GlbCompanyTokenCredentials>(PasswordTypesList.Codes.CHT);
				RegisterEditableChildObject(tokenCredentials);
			}

			return tokenCredentials;
		}
	}
	GlbCompanyTokenCredentials tokenCredentials;

	#endregion

	[ResourceStringData("CH.Business.GlbCompanyWrapper|TokenCredentialsEnabled", Caption = "Enable Token Credentials")]
	public ZBool TokenCredentialsEnabled
	{
		get => !TokenCredentials?.IsSuspended ?? false;
		set
		{
			if (value)
			{
				if (TokenCredentials == null)
				{
					GetGlbExternalPasswordOrCreateNew<GlbCompanyTokenCredentials>(PasswordTypesList.Codes.CHT);
				}
				else
				{
					if (TokenCredentials.IsSuspended)
					{
						TokenCredentials.GP_PasswordStatus = ZString.Empty;
					}
				}
			}
			else
			{
				if (TokenCredentials != null)
				{
					TokenCredentials.GP_PasswordStatus = CHPasswordStatusList.Suspended;
				}
			}
			if (!IsValidationSuspended)
			{
				TokenCredentials?.Validation.ValidateAll();
			}
			TokenCredentialsEnabledInfo.RefreshBinding();
		}
	}

	public ZPropertyInfo TokenCredentialsEnabledInfo => GetZPropertyInfo(nameof(TokenCredentialsEnabled));

	public override bool IsValidWrapper => true;

	IGlbExternalPassword ICHGlbCompanyWrapper.GlbExternalPassword => GlbExternalPassword;

	public static bool CurrentCompanyTokenCredentialsEnabled
	{
		get
		{
			var wrapper = GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany);
			wrapper.TokenCredentials?.ReloadSafe();
			return wrapper.TokenCredentialsEnabled;
		}
	}
}
