using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Enterprise.ZClientWebCargoWiseEDI.OIDC
{
	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Not (yet) needed.")]
	public struct UserCredentialData
	{
		public UserCredentialData(string emailUsername, string organisationCode, string returnUrl = "")
		{
			EmailUsername = emailUsername;
			OrganisationCode = organisationCode;
			ReturnUrl = returnUrl;
		}

		public string EmailUsername { get; set; }

		public string OrganisationCode { get; set; }

		[DataMember(Name = "return_url")]
		public string ReturnUrl { get; set; }
	}
}
