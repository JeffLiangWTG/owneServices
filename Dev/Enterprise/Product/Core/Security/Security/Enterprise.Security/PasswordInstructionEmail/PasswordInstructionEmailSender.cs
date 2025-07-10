using System;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Security
{
	public static class PasswordInstructionEmailSender
	{
		public static PasswordResetInfo GetPasswordResetInfoForSendPasswordResetEmail(IPasswordInstructionEmailSource emailSource)
		{
			PasswordResetInfo passwordResetInfo = null;

			if (emailSource != null && emailSource.CompanyPKForEmailTemplate != Guid.Empty)
			{
				passwordResetInfo = new PasswordResetInfo()
				{
					ContactEmail = emailSource.Email,
					EmailTemplateCompanyPk = emailSource.CompanyPKForEmailTemplate.ToString()
				};
			}

			return passwordResetInfo;
		}

		public static bool SendPasswordResetEmail(IPasswordInstructionEmailSource emailSource, PasswordInstructionUrlType urlType = PasswordInstructionUrlType.Default)
		{
			var passwordResetInfo = GetPasswordResetInfoForSendPasswordResetEmail(emailSource);
			return SendPasswordInstructionEmail(emailSource, PasswordInstructionType.Reset, passwordResetInfo, urlType);
		}

		public static bool SendPasswordInstructionEmail(
			IPasswordInstructionEmailSource emailSource,
			PasswordInstructionType instructionType = PasswordInstructionType.Reset,
			PasswordResetInfo passwordResetInfo = null,
			PasswordInstructionUrlType urlType = PasswordInstructionUrlType.Default,
			bool useCurrentUserInfo = false)
		{
			if (emailSource != null && emailSource.PasswordInstructionEmail != null)
			{
				var previousValueUseCurrentEmail = emailSource.PasswordInstructionEmail.UseCurrentUsersEmailAddress;
				var previousValueUseCurrentName = emailSource.PasswordInstructionEmail.UseCurrentUsersNameAndTitle;
				try
				{
					var passwordInstructionUrl = GeneratePasswordInstructionUrl(emailSource, instructionType, passwordResetInfo, urlType);
					emailSource.PasswordInstructionEmail.UseCurrentUsersEmailAddress = useCurrentUserInfo;
					emailSource.PasswordInstructionEmail.UseCurrentUsersNameAndTitle = useCurrentUserInfo;

					Guid.TryParse(passwordResetInfo?.EmailTemplateCompanyPk, out var emailTemplateCompanyPk);
					emailSource.PasswordInstructionEmail.SendEmailWithPasswordInstructionUrl(passwordInstructionUrl, instructionType, emailTemplateCompanyPk);
				}
				catch (Exception e) when (!e.IsCriticalException() && !(e is WebSiteUrlNotSetException))
				{
					return false;
				}
				finally
				{
					emailSource.PasswordInstructionEmail.UseCurrentUsersEmailAddress = previousValueUseCurrentEmail;
					emailSource.PasswordInstructionEmail.UseCurrentUsersNameAndTitle = previousValueUseCurrentName;
				}
				return true;
			}

			return false;
		}

		static string GeneratePasswordInstructionUrl(IPasswordInstructionEmailSource source, PasswordInstructionType instructionType, PasswordResetInfo passwordResetInfo, PasswordInstructionUrlType urlType)
		{
			var urlStrategy = PasswordInstructionUrlStrategyFactory.GetStrategy(urlType);
			return urlStrategy.GenerateUrl(source, instructionType, passwordResetInfo);
		}
	}
}
