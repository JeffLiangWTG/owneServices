using System;
using System.Collections.Generic;

namespace Enterprise.Security
{
	class SecurityCertificateHashtable : ISecurityCertificateProvider
	{
		readonly Dictionary<SecurityCheckpoint, SecurityCertificate> certificates;
		readonly ISecurityOverrideProvider securityOverrideProvider;

		public SecurityCertificateHashtable(ISecurityOverrideProvider securityOverrideProvider)
		{
			if (securityOverrideProvider == null)
			{
				throw new ArgumentNullException(nameof(securityOverrideProvider));
			}

			this.certificates = new Dictionary<SecurityCheckpoint, SecurityCertificate>();
			this.securityOverrideProvider = securityOverrideProvider;
		}

		public SecurityCertificate this[SecurityCheckpoint checkpoint]
		{
			get
			{
				SecurityCertificate result = null;

				if (checkpoint.IsAllowed && !securityOverrideProvider.IsUserInitiatorAndNotAllowedToApprove)
				{
					if (securityOverrideProvider.ShouldPromptForGrantedConfirmation)
					{
						certificates.TryGetValue(checkpoint, out result);
						if (result == null)
						{
							result = securityOverrideProvider.PromptForAccessGrantConfirmation(checkpoint);
							if ((result != null) && result.IsAllowed)
							{
								certificates[checkpoint] = result;
							}
						}
					}
					else
					{
						result = SecurityCertificate.Granted;
					}
				}
				else
				{
					certificates.TryGetValue(checkpoint, out result);
					if (result == null)
					{
						result = securityOverrideProvider.PromptForTemporaryAccess(checkpoint);
						if ((result != null) && result.IsAllowed)
						{
							certificates[checkpoint] = result;
						}
					}
				}

				return result ?? SecurityCertificate.Denied;
			}
			set { certificates[checkpoint] = value; }
		}
	}
}
