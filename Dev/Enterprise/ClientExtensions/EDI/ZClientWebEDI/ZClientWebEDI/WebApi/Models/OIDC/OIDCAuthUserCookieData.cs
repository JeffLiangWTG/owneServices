using System;
using System.Text;
using CargoWise.Common;
using Enterprise.Client.EDI;
using Enterprise.ZArchitecture.Core.Encryption;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Enterprise.ZClientWebCargoWiseEDI.OIDC
{
	public class OIDCAuthUserCookieData
	{
		public OIDCAuthUserCookieData(string verifier, string nonce, UserCredentialData userData) : this(userData.ReturnUrl, verifier, nonce, userData.EmailUsername, userData.OrganisationCode)
		{
			Argument.NotNull(userData, nameof(userData));
			this.State = userData.ReturnUrl;
			this.Nonce = nonce;
			this.Verifier = verifier;
		}

		public OIDCAuthUserCookieData(string state, string verifier, string nonce, string emailAddress, string organisationCode = "")
		{
			this.EmailAddress = emailAddress;
			this.OrganisationCode = organisationCode;
			this.State = state;
			this.Nonce = nonce;
			this.Verifier = verifier;
		}

		public OIDCAuthUserCookieData() { }

		public string EmailAddress { get; set; } = string.Empty;

		public string OrganisationCode { get; set; } = string.Empty;

		public string State { get; set; } = string.Empty;

		public string Verifier { get; set; } = string.Empty;

		public string Nonce { get; set; } = string.Empty;

		[JsonIgnore]
		public bool IsCookieDecodingSuccessful { get; private set; }

		[JsonIgnore]
		public string DeserializingMessage { get; private set; } = string.Empty;

		public override string ToString()
		{
			var iv = EDIDataRegistry.Instance.MyAccountOIDCUserDataHashIVGuid;
			if(iv == Guid.Empty)
			{
				return string.Empty;
			}

			var result = JsonConvert.SerializeObject(this);
			var encoder = new TwoWayEncoder(iv);
			var resultBytes = encoder.Encrypt(Encoding.UTF8.GetBytes(result));

			return Base64UrlEncoder.Encode(resultBytes);
		}

		public static OIDCAuthUserCookieData Deserialize(string base64UrlString)
		{
			var iv = EDIDataRegistry.Instance.MyAccountOIDCUserDataHashIVGuid;
			if (string.IsNullOrEmpty(base64UrlString) || iv == Guid.Empty)
			{
				var message = string.IsNullOrEmpty(base64UrlString) ? "OIDC cookie data is empty" : "OIDC cookie data hash IV is empty";
				return new OIDCAuthUserCookieData() { DeserializingMessage = message };
			}

			try
			{
				var cookieBytes = Base64UrlEncoder.DecodeBytes(base64UrlString);
				var encoder = new TwoWayEncoder(iv);
				var resultBytes = encoder.Decrypt(cookieBytes);

				var result = JsonConvert.DeserializeObject<OIDCAuthUserCookieData>(Encoding.UTF8.GetString(resultBytes));
				result.IsCookieDecodingSuccessful = true;

				return result;
			}
			catch (Exception ex)
			{
				if(ex is not JsonException && ex is not FormatException)
				{
					ErrorReporter.ReportOnce("An error occurred when deserializing OIDC cookie data", ex);
				}

				var result = new OIDCAuthUserCookieData();
				result.DeserializingMessage = ex.Message;
				return result;
			}
		}
	}
}
