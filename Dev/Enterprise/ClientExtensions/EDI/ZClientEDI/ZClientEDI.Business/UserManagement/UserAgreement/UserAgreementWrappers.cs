using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using WTG.TrustedMessaging.MyAccount.Interfaces;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public interface IEnterpriseUserAgreementInfo : IUserAgreementInfo
	{
		public static new class Schema
		{
			public const string JobTitle = "job_title";

			public const string ClientAgreementOrgFullName = "client_agreement_org_full_name";

			public const string ClientMainAddressInSingleLine = "client_main_address_in_single_line";

			public const string ClientBusinessRegistration = "client_business_registration";
		}

		[JsonProperty(Schema.JobTitle)]
		public string JobTitle { get; set; }

		[JsonProperty(Schema.ClientAgreementOrgFullName)]
		public string ClientAgreementOrgFullName { get; set; }

		[JsonProperty(Schema.ClientMainAddressInSingleLine)]
		public string ClientMainAddressInSingleLine { get; set; }

		[JsonProperty(Schema.ClientBusinessRegistration)]
		public string ClientBusinessRegistration { get; set; }
	}

	public class EdiUserAgreementWrapper : NonPersistentBusinessObject
	{
		public EdiUserAgreement UserAgreement { get; set; }
		public ITrustedUserInfo UserInfo { get; set; }
		public LicenceDatabase Database { get; set; }
		public string AgreementTitle { get; set; }
		public string AgreementContent { get; set; }
		public string GetAgreementDocsURL { get; set; }

		public static OrgCusCode GetPrimaryCusCodeForCountry(OrgHeader orgHeader)
		{
			if (orgHeader != null && orgHeader.Country != null)
			{
				foreach (string numberType in OrgRegistrationNumberTypeList.GetApplicableNumberTypes(orgHeader.Country))
				{
					OrgCusCode cusCode = GetCusCode(numberType, orgHeader.Country.Code, orgHeader);
					if (cusCode != null)
					{
						return cusCode;
					}
				}
			}

			return null;
		}

		public static OrgCusCode GetCusCode(string numberType, ZString countryCode, OrgHeader orgHeader)
		{
			ZQuery query = new ZQuery(OrgCusCodeSchema.OK_CodeType, numberType);
			query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, countryCode);
			BusinessObject[] cusCodes = orgHeader.CustomsCodes.Find(query);
			return (cusCodes.Length > 0) ? (OrgCusCode)cusCodes[0] : null;
		}
	}

	public class DocEdiUserAgreement : DocumentWrapper
	{
		public static DocEdiUserAgreement New(EdiUserAgreementWrapper docUserAgreement, BusinessObjectFactory factory)
		{
			return new DocEdiUserAgreement(docUserAgreement, factory);
		}

		DocEdiUserAgreement(EdiUserAgreementWrapper docUserAgreement, BusinessObjectFactory factory) : base(docUserAgreement, factory)
		{
			Parent = docUserAgreement;
		}

		[DocumentField("Html Style Sheet")]
		public ZString HtmlStyleSheet => SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value;

		[DocumentField("Current Date")]
		public ZString CurrentDate => ZDateTime.Now.ToShortDateString();

		[DocumentField("Client Name")]
		public ZString ClientName
		{
			get
			{
				if (Parent.UserInfo is IEnterpriseUserAgreementInfo enterpriseInfo)
				{
					return enterpriseInfo.ClientAgreementOrgFullName;
				}
				else
				{
					return Parent.Database.WebAccessOrg?.OH_FullNameTruncated ?? ZString.Empty;
				}
			}
		}

		[DocumentField("ClientMainAddressInSingleLine")]
		public ZString ClientMainAddressInSingleLine
		{
			get
			{
				if (Parent.UserInfo is IEnterpriseUserAgreementInfo enterpriseInfo)
				{
					return enterpriseInfo.ClientMainAddressInSingleLine;
				}
				else
				{
					var address = Parent.Database.WebAccessOrg?.MainAddress;
					var clientAddress = ZString.Empty;
					if (address != null)
					{
						clientAddress = new ZStringBuilder().AppendIfNotEmpty(address.Address1).AppendIfNotEmpty(address.Address2)
	.AppendIfNotEmpty(address.OA_City).AppendIfNotEmpty(address.State).AppendIfNotEmpty(address.Postcode).AppendIfNotEmpty(address.CountryName).ToStringWithDelimiterBetweenAppends(", ");
					}
					return clientAddress;
				}
			}
		}

		[DocumentField("ClientBusinessRegistration")]
		public ZString ClientBusinessRegistration
		{
			get
			{
				if (Parent.UserInfo is IEnterpriseUserAgreementInfo enterpriseInfo)
				{
					return enterpriseInfo.ClientBusinessRegistration;
				}
				else
				{
					var cusCode = EdiUserAgreementWrapper.GetPrimaryCusCodeForCountry(Parent.Database.WebAccessOrg);
					var clientBusinessRegistration = cusCode != null
						? $"{cusCode.OK_CodeType} {cusCode.SecuredCustomsRegNo}"
						: "Not on file";
					return clientBusinessRegistration;
				}
			}
		}

		[DocumentField("GetAgreementDocsURL")]
		public ZString GetAgreementDocsURL => Parent.GetAgreementDocsURL ?? ZString.Empty;

		[DocumentField("User Full Name")]
		public ZString UserFullName => Parent.UserInfo.FullName;

		[DocumentField("User Job Title")]
		public ZString UserJobTitle => (Parent.UserInfo as IEnterpriseUserAgreementInfo)?.JobTitle ?? ZString.Empty;

		[DocumentField("User Email")]
		public ZString UserEmail => Parent.UserInfo.Email;

		[DocumentField("Agreement Title")]
		public ZString AgreementTitle => Parent.AgreementTitle;

		[DocumentField("Agreement Content")]
		public ZString AgreementContent => Parent.AgreementContent;

		[DocumentField("Agreement Version Number")]
		public ZString AgreementVersionNumber => Parent.UserAgreement.ERA_VersionNumber.ToString();

		readonly EdiUserAgreementWrapper Parent;
	}
}
