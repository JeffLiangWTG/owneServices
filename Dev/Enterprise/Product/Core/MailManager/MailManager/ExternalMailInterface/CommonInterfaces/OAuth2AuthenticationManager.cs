using System;
using System.Globalization;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MailManager.ExternalMailInterface
{
	public static class OAuth2AuthenticationManager
	{
		#region OAuth2 Type

		public static bool OutgoingEnabled => !Env.Registry.UseOAuth2ForOutgoing.IsNullOrEmpty();

		public static bool Ms365OutgoingEnabled => OutgoingEnabled && Env.Registry.UseOAuth2ForOutgoing.Equals(OAuth2TypeList.Codes.Ms365);

		#endregion

		#region Load Configuration

		public static IOAuth2Configuration LoadOutgoingConfiguration()
		{
			return Env.Registry.UseOAuth2ForOutgoing switch
			{
				OAuth2TypeList.Codes.Ms365 => CreateMs365OutgoingConfiguration(),
				OAuth2TypeList.Codes.GMail => new GmailOAuth2Configuration(Env.Registry.GmailDelegatedMailForOutgoing, Env.Registry.GmailServiceAccountKeyForOutgoing),
				_ => throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Invalid OAuth2 type."))
			};
		}

		#region Microsoft 365 Configuration

		static IOAuth2Configuration CreateMs365OutgoingConfiguration()
		{
			var token = Env.Registry.Ms365OAuth2TokenForOutgoing;

			return new Ms365OAuth2Configuration(
				tenantId: Env.Registry.Ms365OAuth2TenantId,
				applicationId: Env.Registry.Ms365ApplicationIdForOutgoing,
				permissionType: Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_OutLook,
				cachedToken: token?.Token,
				tokenSaveAction: b => SaveRegisterToken(token, b),
				identifier: token?.Identifier);

			void SaveRegisterToken(Ms365OAuth2Token ms365OAuth2Token, byte[] bytes)
			{
				if (bytes != null)
				{
					ms365OAuth2Token.Token = bytes;
				}
			}
		}

		#endregion

		#endregion
	}
}
