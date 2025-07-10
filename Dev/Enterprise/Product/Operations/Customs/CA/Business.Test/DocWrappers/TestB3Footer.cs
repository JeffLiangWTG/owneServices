using Enterprise.Customs.CA.Messaging;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class TestB3Footer : B3Footer
	{
		public TestB3Footer(IB3Header header)
			: base(header)
		{
		}

		public bool IsTotalAmountsEmptyTest(ITotalAmounts totalAmounts)
		{
			return B3Footer.IsTotalAmountsEmpty(totalAmounts);
		}
	}
}
