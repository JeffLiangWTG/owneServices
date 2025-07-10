using System;
using System.Threading.Tasks;
using CargoWise.Types;
using Enterprise.TrustedMessaging.Intergration;
using Enterprise.ZArchitecture.Business;
using WTG.TrustedMessaging;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.TrustedMessaging.Business
{
	public class UserPortalClient : IUserPortalClient
	{
		public UserPortalClient()
		{
			ApiService = new RemoteApiService(new UserPortalClientConfiguration());
		}

		public async Task<TrustedResponse<UserAgreementResponseData>> GetUserAgreementAsync(string userAgreementType)
			=> await ApiService.GetUserAgreementAsync(CreateUserAgreementInfo(userAgreementType)).AwaitSmart();

		public async Task<TrustedResponse<EnterpriseAgreementResponseData>> GetEnterpriseAgreementUrlAsync(string userAgreementType)
			=> await ApiService.GetEnterpriseAgreementUrlAsync(CreateEnterpriseAgreementInfo(userAgreementType)).AwaitSmart();

		public async Task<TrustedResponse<bool>> AcknowledgeAgreementAsync(string userAgreementType, bool shouldSendAgreementCopy)
			=> await ApiService.AcknowledgeAgreementAsync(CreateUserAgreementInfo(userAgreementType, shouldSendAgreementCopy)).AwaitSmart();

		#region ERequest

		public async Task<TrustedResponse<AutoLoginResponse>> ERequestPortalAutoLoginAsync(string landingPageId, string incidentNumber, string module, string subModule, string referenceId, string licenceCode)
			=> await ApiService.ERequestPortalAutoLoginAsync(CreateERequestInfo(landingPageId, incidentNumber, module, subModule, referenceId, licenceCode)).AwaitSmart();

		public async Task<TrustedResponse<bool>> SignAgreementAsync(IUserAgreementSignerDetails details)
		{
			var timestamp = ZDateTime.UtcNow.ToDateTime();
			var info = new UserAgreementInfo();
			info.UserAgreementType = details.Type;
			info.MajorVersion = details.Version.Major;
			info.MinorVersion = details.Version.Minor;
			info.Variant = details.Version.Variant;
			info.Product = "CW1";
			info.SystemId = ApiService.DatabaseNumber;
			info.FullName = details.Name;
			info.Email = details.Email;
			info.IPAddress = details.IPAddress;
			info.AgreementDate = details.AgreementDateUtc;
			info.InfoExpires = timestamp.AddDays(1);
			info.InfoTimestamp = timestamp;
			info.ShouldSendAgreementCopy = true;

			return await ApiService.AcknowledgeAgreementAsync(info).AwaitSmart();
		}

		protected ERequestInfo CreateERequestInfo(string landingPageId, string incidentNumber, string module, string subModule, string referenceId, string licenceCode)
		{
			var info = CreateUserInfo<ERequestInfo>();
			info.LandingPageId = landingPageId;
			info.IncidentNumber = incidentNumber;
			info.Module = module;
			info.SubModule = subModule;
			info.ReferenceId = referenceId;
			info.LicenceCode = licenceCode;
			return info;
		}

		#endregion ERequest

		#region AutoLogin

		public async Task<TrustedResponse<AutoLoginResponse>> MyAccountAutoLoginAsync(Uri returnUrl)
			=> await ApiService.MyAccountAutoLoginAsync(CreateAutoLoginInfo(returnUrl)).AwaitSmart();

		public Uri GetMyAccountAutoLoginUrl(Uri returnUrl)
		{
			//avoid the UI synchronization context deadlock.
			var result = Task.Run(() => MyAccountAutoLoginAsync(returnUrl)).GetAwaiter().GetResult();
			return result.Success ? result.Response.AutoLoginUrl : null;
		}

		protected TrustedAutoLoginInfo CreateAutoLoginInfo(Uri returnUrl)
		{
			var info = CreateUserInfo<TrustedAutoLoginInfo>();
			info.ReturnUrl = returnUrl;
			return info;
		}

		#endregion

		#region OAuth

		public async Task<TrustedResponse<OAuthLoginResponse>> OAuthAutoLoginAsync(Uri returnUrl)
			=> await ApiService.OAuthAutoLoginAsync(CreateOauthLoginInfo(returnUrl)).AwaitSmart();

		protected AuthenticationTokenInfo CreateOauthLoginInfo(Uri returnUrl)
		{
			var info = CreateUserInfo<AuthenticationTokenInfo>();
			info.RedirectUriString = returnUrl.ToString();
			return info;
		}

		#endregion

		protected UserAgreementInfo CreateUserAgreementInfo(string userAgreementType, bool shouldSendAgreementCopy = false)
		{
			var info = CreateUserInfo<UserAgreementInfo>();
			info.UserAgreementType = userAgreementType;
			info.ShouldSendAgreementCopy = shouldSendAgreementCopy;
			return info;
		}

		T CreateUserInfo<T>() where T : TrustedUserInfo, new()
		{
			return new T()
			{
				Product = "CW1",
				SystemId = ApiService.DatabaseNumber,
				UserId = StaticCurrentFetcher.Instance.CurrentUser.GS_Code,
				FullName = StaticCurrentFetcher.Instance.CurrentUser.GS_FullName,
				Email = StaticCurrentFetcher.Instance.CurrentUser.GS_EmailAddress,
				UserCountry = StaticCurrentFetcher.Instance.CurrentBranch?.Company?.Country?.RN_Code.ToString() ?? "",
				InfoExpires = ZDateTime.UtcNow.AddDays(1).ToDateTime(),
			};
		}

		public async Task<TrustedResponse<GetAcceptancesResponse>> GetAcceptancesAsync(string userAgreementType)
			=> await ApiService.GetAcceptancesAsync(new GetAcceptancesInfo() {
				Product = "CW1",
				SystemId = ApiService.DatabaseNumber,
				UserAgreementType = userAgreementType,
				InfoExpires = ZDateTime.UtcNow.AddDays(1).ToDateTime(),
			});

		protected EnterpriseAgreementInfo CreateEnterpriseAgreementInfo(string userAgreementType, bool shouldSendAgreementCopy = false)
		{
			var info = new EnterpriseAgreementInfo
			{
				Product = "CW1",
				SystemId = ApiService.DatabaseNumber,
				UserAgreementType = userAgreementType,
				ShouldSendAgreementCopy = shouldSendAgreementCopy,
				InfoExpires = ZDateTime.UtcNow.AddDays(1).ToDateTime(),
			};

			return info;
		}

		readonly RemoteApiService ApiService;
	}
}
