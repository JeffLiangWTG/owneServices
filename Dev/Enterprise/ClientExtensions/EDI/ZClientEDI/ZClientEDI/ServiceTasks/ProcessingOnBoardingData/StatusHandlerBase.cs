using System;
using CargoWise.IO;
using Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business;

namespace Enterprise.Client.EDI.ServiceTasks.ProcessingOnBoardingData
{
	public abstract class StatusHandlerBase
	{
		protected StatusHandlerBase(EdiTokenAuthOnBoardingData ediTokenAuthOnBoardingData)
		{
			this.ediTokenAuthOnBoardingData = ediTokenAuthOnBoardingData;
		}

		#region EdiTokenAuthOnBoardingData

		readonly EdiTokenAuthOnBoardingData ediTokenAuthOnBoardingData;

		protected EdiTokenAuthOnBoardingData EdiTokenAuthOnBoardingData
		{
			get { return ediTokenAuthOnBoardingData; }
		}

		protected string CompanyCode => EdiTokenAuthOnBoardingData.LicenceEnterpriseCode;

		#endregion

		public abstract void Handle();

		protected virtual string CreatePRAndMerge(string envFolder, bool isRollback = false)
		{
			using (var localTempDirectory = new TempDirectory())
			{
				var customPolicyFileBase = GetCustomPolicyFileHandler();
				return customPolicyFileBase.CreatePRAndMerge(CompanyCode,
					ediTokenAuthOnBoardingData.TOD_SystemUniqueIdentifier, ediTokenAuthOnBoardingData.TOD_ValidTokenIssuerPrefix, GetGitHubAppInfo(envFolder), isRollback);
			}
		}

		CustomPolicyFileBase GetCustomPolicyFileHandler()
		{
			switch (ediTokenAuthOnBoardingData.TOD_OIDCServer)
			{
				case Enterprise.Registry.Business.OIDCServerTypesList.Codes.Azure:
					return new AzureCustomPolicyFile();

				case Enterprise.Registry.Business.OIDCServerTypesList.Codes.Okta:
					return new OktaCustomPolicyFile();

				case Enterprise.Registry.Business.OIDCServerTypesList.Codes.OneLogin:
					return new OneLoginCustomPolicyFile();

				default:
					throw new InvalidOperationException($"It does not support server type {ediTokenAuthOnBoardingData.TOD_OIDCServer}");
			}
		}

		GitHubAppInfo GetGitHubAppInfo(string envFolder)
		{
			return new GitHubAppInfo
			{
				AppId = EDIDataRegistry.Instance.B2CConfigurationRepositoryGitHubAppID.Value,
				AppName = EDIDataRegistry.Instance.B2CConfigurationRepositoryGitHubAppName.Value,
				PrivateKey = System.Text.Encoding.UTF8.GetString(EDIDataRegistry.Instance.B2CConfigurationRepositoryGitHubAppPrivateKey.Value),
				RepositoryName = "AzureB2C.Config",
				RepositoryOwner = EDIDataRegistry.Instance.B2CConfigurationRepositoryOwnerName.Value,
				FilePath = $"CustomPolicyFiles/{envFolder}/TRUSTFRAMEWORKEXTENSIONS.xml",
			};
		}
	}
}
