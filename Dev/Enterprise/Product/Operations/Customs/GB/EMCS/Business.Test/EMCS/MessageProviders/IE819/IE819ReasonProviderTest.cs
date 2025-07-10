using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	sealed class IE819ReasonProviderTest : Customs.Business.Testing.DataProviderTestCase<IE819ReasonProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE819ReasonProvider(null));
		}

		public void TestReasonCode()
		{
			AssertEquals("1", dataProvider.ReasonCode);
		}

		public void TestComplementaryInformation()
		{
			AssertEquals("INFORMATION ON REASON", dataProvider.ComplementaryInformation.Text);
		}

		protected override IE819ReasonProvider GetProvider() => dataProvider;

		protected override IEnumerable<Expression<Func<IE819ReasonProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.ComplementaryInformation;
		}

		protected override void SetUp()
		{
			base.SetUp();
			alertOrRejectReason = new AlertOrRejectReason(Factory);
			alertOrRejectReason.Reason = "1";
			alertOrRejectReason.Information = "INFORMATION ON REASON";
			dataProvider = new IE819ReasonProvider(alertOrRejectReason);
		}
		AlertOrRejectReason alertOrRejectReason;
		IE819ReasonProvider dataProvider;
	}
}
