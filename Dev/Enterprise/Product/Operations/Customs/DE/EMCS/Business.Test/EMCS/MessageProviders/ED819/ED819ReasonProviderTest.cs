using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	class ED819ReasonProviderTest : Customs.Business.Testing.DataProviderTestCase<ED819ReasonProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED819ReasonProvider(null));
		}

		public void TestReasonCode()
		{
			AssertEquals("1", dataProvider.ReasonCode);
		}

		public void TestComplementaryInformation()
		{
			AssertEquals("INFORMATION ON REASON", dataProvider.ComplementaryInformation.Text);
		}

		protected override void SetUp()
		{
			base.SetUp();
			alertOrRejectReason = new AlertOrRejectReason(Factory);
			alertOrRejectReason.Reason = "1";
			alertOrRejectReason.Information = "INFORMATION ON REASON";
			dataProvider = new ED819ReasonProvider(alertOrRejectReason);
		}
		AlertOrRejectReason alertOrRejectReason;
		ED819ReasonProvider dataProvider;

		protected override ED819ReasonProvider GetProvider() => dataProvider;

		protected override IEnumerable<Expression<Func<ED819ReasonProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.ComplementaryInformation;
		}
	}
}
