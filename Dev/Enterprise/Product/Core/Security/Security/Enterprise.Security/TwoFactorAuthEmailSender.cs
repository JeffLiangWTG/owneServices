using System;
using System.Diagnostics;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security
{
	public class TwoFactorAuthEmailSender : ITwoFactorAuthenticationEngine
	{
		public string SendTwoFactorAuthenticationCode(IUser user)
		{
			var email = new EmailDef(Guid.Empty); // Guid.Empty so the 2FA email is always sent from system account instead of user's account
			email.AddRecipientForSystemCommunication(user.EmailAddress, true);
			var code = GenerateTwoFactorAuthenticationCode();
			email.Subject = Res.GetString("242BE533-A76C-444D-820F-348025FFED54", "Your {0} Login Code", Enterprise.Core.Constants.ProductName);

			var bodyBuilder = new ZStringBuilder();
			bodyBuilder.Append(code);
			bodyBuilder.AppendLine();
			bodyBuilder.AppendLine();
			bodyBuilder.Append(Res.GetString("A2340774-D016-47CF-8894-CEF596C781BB", "Please note that this code is only valid for the current login attempt"));
			email.Body = bodyBuilder.ToString();
			if (Registry.Business.SystemDataRegistry.Instance.MakeTwoFactorAuthenticationEmailHTML.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				email.ContentType = EmailContentTypes.HTML;
				email.Body += Res.GetString("A6D4D385-199C-4E55-AAF1-1C5B67A600FF", "<!--Call Stack: {0}\r\nProcess Id: {1}\r\nThread Id: {2}-->",
					new StackTrace(),
					Process.GetCurrentProcess().Id,
					System.Threading.Thread.CurrentThread.ManagedThreadId);
			}

			Env.OutgoingMailManager.CreateAndSave(email);
			return code;
		}

		internal string GenerateTwoFactorAuthenticationCode() => randomGenerator.Next(1, 1000000).ToString("D6", CultureInfo.InvariantCulture);

		readonly Random randomGenerator = new Random();
	}
}
