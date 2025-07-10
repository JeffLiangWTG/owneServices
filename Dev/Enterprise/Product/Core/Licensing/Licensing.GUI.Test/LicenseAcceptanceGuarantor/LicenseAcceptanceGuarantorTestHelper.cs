using System;
using System.Threading.Tasks;
using CargoWise.Application;
using Enterprise.TrustedMessaging.Intergration;
using Moq;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.Licensing.GUI.Test
{
	public static class LicenseAcceptanceGuarantorTestHelper
	{
		public static IDisposable WithAccepted()
		{
			var client = new Mock<IUserPortalClient>();
			var responseData = new EnterpriseAgreementResponseData();
			responseData.Required = false;
			responseData.Url = string.Empty;
			var trustedResponse = Task.FromResult(new TrustedResponse<EnterpriseAgreementResponseData>() { Success = true, Response = responseData });
			client.Setup(client => client.GetEnterpriseAgreementUrlAsync(It.IsAny<string>())).Returns(trustedResponse);

			return ObjectFactory.Substitute(client.Object);
		}

		public static IDisposable WithNotAccepted()
		{
			var client = new Mock<IUserPortalClient>();
			var responseData = new EnterpriseAgreementResponseData();
			responseData.Required = true;
			responseData.Url = "https://myaccount.com/UserAgreement.aspx";
			var trustedResponse = Task.FromResult(new TrustedResponse<EnterpriseAgreementResponseData>() { Success = true, Response = responseData });
			client.Setup(client => client.GetEnterpriseAgreementUrlAsync(LicenseAgreementTypeList.Codes.CargoWiseNext)).Returns(trustedResponse);

			return ObjectFactory.Substitute(client.Object);
		}

		public static IDisposable WithNoResponse()
		{
			var client = new Mock<IUserPortalClient>();
			client.Setup(client => client.GetEnterpriseAgreementUrlAsync(LicenseAgreementTypeList.Codes.CargoWiseNext)).Returns(Task.FromResult<TrustedResponse<EnterpriseAgreementResponseData>>(null));

			return ObjectFactory.Substitute(client.Object);
		}

		public static IDisposable WithNonSuccessResponse()
		{
			var client = new Mock<IUserPortalClient>();
			var trustedResponse = Task.FromResult(new TrustedResponse<EnterpriseAgreementResponseData>() { Success = false, Response = null });
			client.Setup(client => client.GetEnterpriseAgreementUrlAsync(LicenseAgreementTypeList.Codes.CargoWiseNext)).Returns(trustedResponse);

			return ObjectFactory.Substitute(client.Object);
		}
	}
}
