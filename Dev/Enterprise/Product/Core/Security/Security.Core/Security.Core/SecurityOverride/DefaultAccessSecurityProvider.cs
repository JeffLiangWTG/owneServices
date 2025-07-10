namespace Enterprise.Security
{
	public class DefaultAccessSecurityProvider : ISecurityOverrideProvider
	{
		#region ISecurityOverrideProvider Members

		ISecurityCertificateProvider ISecurityOverrideProvider.SecurityCertificates
		{
			get
			{
				if (fSecurityCertificates == null)
				{
					fSecurityCertificates = new DefaultAccessSecurityCertificateProvider(this);
				}

				return fSecurityCertificates;
			}
		}

		ISecurityCertificateProvider fSecurityCertificates;

		public SecurityCore UserSecurityOverride { get { return null; } }

		SecurityCertificate ISecurityOverrideProvider.PromptForTemporaryAccess(SecurityCheckpoint checkPoint)
		{
			return checkPoint.IsAllowed ? SecurityCertificate.Granted : SecurityCertificate.Denied;
		}

		SecurityCertificate ISecurityOverrideProvider.PromptForAccessGrantConfirmation(SecurityCheckpoint checkPoint)
		{
			return SecurityCertificate.Granted;
		}

		bool ISecurityOverrideProvider.ShouldPromptForGrantedConfirmation
		{
			get { return false; }
		}

		bool ISecurityOverrideProvider.IsUserInitiatorAndNotAllowedToApprove
		{
			get { return false; }
		}

		#endregion

		#region Security Certificate Provider

		class DefaultAccessSecurityCertificateProvider : ISecurityCertificateProvider
		{
			public DefaultAccessSecurityCertificateProvider(ISecurityOverrideProvider provider)
			{
				this.Provider = provider;
			}

			readonly ISecurityOverrideProvider Provider;

			#region ISecurityCertificateProvider Members

			SecurityCertificate ISecurityCertificateProvider.this[SecurityCheckpoint checkPoint]
			{
				get { return Provider.PromptForTemporaryAccess(checkPoint); }
			}

			#endregion
		}

		#endregion
	}
}
