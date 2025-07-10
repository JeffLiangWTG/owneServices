using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class MXMessageSendingNotificationHelper : MessageSendingNotificationHelper
	{
		public MXMessageSendingNotificationHelper(AsycudaManifestHeader header) : base(header)
		{
		}

		public override ZString GetNotifications()
		{
			var result = base.GetNotifications();

			if (result.IsEmpty && !header.AMA_GB.IsEmpty)
			{
				var company = header.Factory?.Load<GlbBranch>(header.AMA_GB)?.Company;
				if (company != null)
				{
					var credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company)?.GetGlbExternalPassword<GlbCompanyCredential>(PasswordTypesList.Codes.MXB);
					if (credential == null || credential.GP_UserID.IsEmpty)
					{
						result = EmptyCredentials;
					}
				}
			}

			return result;
		}

		static MultilingualString EmptyCredentials => ResString.GetMultilingualString("7D4EB24D-6EE3-477F-8A63-AA8AA7D3E707",
			"Company credentials have not been entered. You can enter the credentials from Maintain -> User Admin -> Companies. Picking the company, in the Brokerage Tab.");

		static MultilingualString CaatWrongLength => ResString.GetMultilingualString("5108A41C-BB90-4530-B09D-4103057BFBA4",
			"The CAAT of the transmitter must be an alphanumeric value between 2 and 4 characters.You can check the CAAT from Maintain -> User Admin -> Companies. Picking the company, Customs Registration Number field.");

		protected override ZString GetExtraMessageSendingNotificationCore()
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.Mexico)
			{
				return (ZString)ValidationsConstants.MustBeLoggedInUnderMXToSendMXMessages;
			}
			else if (GlbCompany.CurrentCompany.GC_CustomsRegistrationNo.Length < 2 || GlbCompany.CurrentCompany.GC_CustomsRegistrationNo.Length > 4)
			{
				return CaatWrongLength;
			}
			else
			{
				return base.GetExtraMessageSendingNotificationCore();
			}
		}
	}
}
