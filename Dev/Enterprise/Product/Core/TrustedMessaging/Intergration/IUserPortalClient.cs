using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.TrustedMessaging.Intergration
{
	[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
	public interface IUserPortalClient
	{
		Task<TrustedResponse<UserAgreementResponseData>> GetUserAgreementAsync(string userAgreementType);

		Task<TrustedResponse<EnterpriseAgreementResponseData>> GetEnterpriseAgreementUrlAsync(string userAgreementType);

		Task<TrustedResponse<bool>> AcknowledgeAgreementAsync(string userAgreementType, bool shouldSendCopy);

		Task<TrustedResponse<bool>> SignAgreementAsync(IUserAgreementSignerDetails userAgreementSignerDetails);

		Task<TrustedResponse<GetAcceptancesResponse>> GetAcceptancesAsync(string userAgreementType);

		Task<TrustedResponse<AutoLoginResponse>> ERequestPortalAutoLoginAsync(string landingPageId, string incidentNumber, string module, string subModule, string referenceId, string licenceCode);

		Task<TrustedResponse<AutoLoginResponse>> MyAccountAutoLoginAsync(Uri returnUrl);

		Uri GetMyAccountAutoLoginUrl(Uri returnUrl);

		Task<TrustedResponse<OAuthLoginResponse>> OAuthAutoLoginAsync(Uri returnUrl);
	}

	public interface IUserAgreementSignerDetails
	{
		string Type { get; }
		string Name { get; }
		string Email { get; }
		string IPAddress { get; }
		DateTime AgreementDateUtc { get; }
		AgreementVersion Version { get; }

		public class AgreementVersion
		{
			public AgreementVersion(string major, string minor, string variant)
			{
				Major = major ?? throw new ArgumentNullException(nameof(major));
				Minor = minor ?? throw new ArgumentNullException(nameof(minor));
				Variant = variant ?? throw new ArgumentNullException(nameof(variant));
			}
			public string Major { get; }
			public string Minor { get; }
			public string Variant { get; }
		}
	}
}

#region Test

#if DEBUG

namespace Enterprise.TrustedMessaging.Intergration.Testing
{
	public class UserPortalClientForTest : IUserPortalClient
	{
		public Task<TrustedResponse<bool>> AcknowledgeAgreementAsync(string userAgreementType, bool shouldSendCopy)
			=> Task.FromResult(new TrustedResponse<bool>() { Success = true, Response = true });

		public Task<TrustedResponse<AutoLoginResponse>> ERequestPortalAutoLoginAsync(string landingPageId, string incidentNumber, string module, string subModule, string referenceId, string licenceCode)
			=> Task.FromResult(new TrustedResponse<AutoLoginResponse>() { Success = true, Response = new AutoLoginResponse(new Uri("http://www.cw1.com/erequst.aspx?token=123")) });

		public Task<TrustedResponse<GetAcceptancesResponse>> GetAcceptancesAsync(string userAgreementType) 
			=> Task.FromResult(new TrustedResponse<GetAcceptancesResponse>() { Success = true, Response = new GetAcceptancesResponse() });

		public Uri GetMyAccountAutoLoginUrl(Uri returnUrl)
			=> new Uri($"http://www.cw1.com/autologin.aspx?token=123&return={returnUrl}");

		public Task<TrustedResponse<UserAgreementResponseData>> GetUserAgreementAsync(string userAgreementType)
			=> Task.FromResult(new TrustedResponse<UserAgreementResponseData>() { Success = true, Response = new UserAgreementResponseData() });

		public Task<TrustedResponse<EnterpriseAgreementResponseData>> GetEnterpriseAgreementUrlAsync(string userAgreementType)
			=> Task.FromResult(new TrustedResponse<EnterpriseAgreementResponseData>() { Success = true, Response = new EnterpriseAgreementResponseData() });

		public Task<TrustedResponse<AutoLoginResponse>> MyAccountAutoLoginAsync(Uri returnUrl)
			=> Task.FromResult(new TrustedResponse<AutoLoginResponse>() { Success = true, Response = new AutoLoginResponse(GetMyAccountAutoLoginUrl(returnUrl)) });

		public Task<TrustedResponse<OAuthLoginResponse>> OAuthAutoLoginAsync(Uri returnUrl)
			=> Task.FromResult(new TrustedResponse<OAuthLoginResponse>() { Success = true, Response = new OAuthLoginResponse(new Uri(returnUrl + "?token=123"), "123") });

		public Task<TrustedResponse<bool>> SignAgreementAsync(IUserAgreementSignerDetails userAgreementSignerDetails)
			=> Task.FromResult(new TrustedResponse<bool>() { Success = true, Response = true });
	}
}

#endif

#endregion Test
