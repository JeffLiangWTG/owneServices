using System;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class TermsAgreementWithFallback : BaseTermsAgreementForTest
	{
		public bool IsFallbackSucceed { get; set; } = true;

		protected override (string Title, string Contents, int VersionNo) GetLocalFallbackTerm()
		{
			if (IsFallbackSucceed)
			{
				return ("Test Title", "Test Content", 1);
			}

			throw new Exception("Local fallback setting exception");
		}
	}
}
