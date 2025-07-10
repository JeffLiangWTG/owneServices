using System;
using CargoWise.Application;
using CargoWise.Definitions.Authentication;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.Security
{
	public class PasswordInstructionUrlStrategy : IPasswordInstructionUrlStrategy
	{
		public string GenerateUrl(IPasswordInstructionEmailSource source, PasswordInstructionType instructionType, PasswordResetInfo passwordResetInfo)
		{
			var token = GeneratePasswordInstructionToken(source, instructionType, passwordResetInfo);
			return source.GeneratePasswordInstructionUrl(token, instructionType);
		}

		public static string GeneratePasswordInstructionToken(IPasswordInstructionEmailSource source, PasswordInstructionType instructionType, PasswordResetInfo passwordResetInfo, TimeSpan? time = null)
		{
			ITokenizedAccessControl accessControl = ObjectFactory.Get<ITokenizedAccessControl>();
			string token;

			if (instructionType == PasswordInstructionType.Reset)
			{
				var scope = passwordResetInfo != null ? GetPasswordResetInfoJson(passwordResetInfo) : source.Email.ToString();
				token = accessControl.CreateLimitedToken(source.ShouldSendMasterPassword ? AccessTokenTypes.ResetMasterPassword : AccessTokenTypes.ResetPassword, new AccessTokenInfo(scope, Guid.Empty, "INV"), time ?? TimeSpan.FromHours(24), 1);
			}
			else
			{
				if (source is OrgContact)
				{
					string scope;
					string accessTokenType;

					if (source.ShouldSendMasterPassword)
					{
						accessTokenType = AccessTokenTypes.SetMasterPassword;
						scope = string.Empty;
					}
					else
					{
						accessTokenType = AccessTokenTypes.SetPassword;
						scope = passwordResetInfo != null ? GetPasswordResetInfoJson(passwordResetInfo) : string.Empty;
					}

					token = accessControl.CreateLimitedToken(accessTokenType, new AccessTokenInfo(scope, source.PK.ToGuid(), OrgContactSchema.Constants.Prefix), time ?? TimeSpan.FromHours(24), 1);
				}
				else
				{
					throw new ArgumentOutOfRangeException(source.ToString());
				}
			}

			return token;
		}

		static string GetPasswordResetInfoJson(PasswordResetInfo passwordResetInfo)
		{
			return JsonConvert.SerializeObject(passwordResetInfo);
		}
	}
}
