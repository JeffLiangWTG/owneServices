using System;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.Definitions.Authentication;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using GlowRegistry = Enterprise.Registry.Business.GlowRegistry;

namespace Enterprise.Security
{
	public class GlowPasswordInstructionUrlStrategy : IPasswordInstructionUrlStrategy
	{
		public string GenerateUrl(IPasswordInstructionEmailSource source, PasswordInstructionType instructionType, PasswordResetInfo passwordResetInfo)
		{
			if (passwordResetInfo?.NavigateUrl != null)
			{
				return passwordResetInfo.NavigateUrl.ToString();
			}

			var token = GeneratePasswordInstructionToken(source);
			var portalsUri = GlowRegistry.Instance.GlowExternalUserPortalsUri.Value;
			if (string.IsNullOrEmpty(portalsUri))
			{
				portalsUri = GlowRegistry.Instance.GlowPortalsUri.Value;
			}

			var shouldUseNeoPortal =
				WebDataRegistry.Instance.DefaultCargoWiseWebPortal.Value == DefaultCargoWiseWebPortalCodeList.Codes.Default &&
				ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.NeoFeature) != null;

			return PasswordResetHelper.GetContactPasswordResetUrl(portalsUri, token, shouldUseNeoPortal ? "NEO" : "GHC");
		}

		static string GeneratePasswordInstructionToken(IPasswordInstructionEmailSource source)
		{
			var scope = JsonConvert.SerializeObject(new PasswordResetScopeData(source.OrgCode, source.Email));

			ITokenizedAccessControl accessControl = ObjectFactory.Get<ITokenizedAccessControl>();
			var token = accessControl.CreateLimitedToken(AccessTokenTypes.GlowPasswordReset, new AccessTokenInfo(scope, source.PK.ToGuid(), OrgContactSchema.Constants.Prefix), TimeSpan.FromHours(24), 1);
			return token;
		}
	}
}
