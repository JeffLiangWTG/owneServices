using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class SystemMinimumFeesTest : TestCaseWithFactory
	{
		public void TestGetSystemMinimumFeesByDatabase()
		{
			var db1 = ZGuid.NewZGuid();
			var db2 = ZGuid.NewZGuid();
			var db3 = ZGuid.NewZGuid();

			var period1 = new ZDateTime(2017, 4, 1);
			var period2 = new ZDateTime(2017, 5, 1);
			var period3 = new ZDateTime(2017, 6, 1);

			var fee1 = new SystemMinimumFee(db1, period1, 1m, "AUD", "MF");
			var fee2 = new SystemMinimumFee(db2, period2, 2m, "USD", "MF");
			var fee3 = new SystemMinimumFee(db3, period3, 3m, "GBP", "MF");

			var fee4 = new SystemMinimumFee(db1, period1, 4m, "USD", "MF");
			var fee5 = new SystemMinimumFee(db2, period2, 5m, "AUD", "MF");
			var fee6 = new SystemMinimumFee(db3, period3, 6m, "GBP", "MF");

			var bill1 = new DummyBill(Factory);
			bill1.Fees = new[] { fee1, fee2 };
			var bill2 = new DummyBill(Factory);
			bill2.Fees = new[] { fee3, fee4 };
			var bill3 = new DummyBill(Factory);
			bill3.Fees = new[] { fee5, fee6 };

			ISystemMinimumFees fees = new SystemMinimumFees(new[] { bill1, bill2, bill3 });

			AssertContainsExactElementsInAnyOrder(fees.GetSystemMinimumFeesByDatabase(db1, period1), new[] { fee1, fee4 });
			AssertContainsExactElementsInAnyOrder(fees.GetSystemMinimumFeesByDatabase(db2, period2), new[] { fee2, fee5 });
			AssertContainsExactElementsInAnyOrder(fees.GetSystemMinimumFeesByDatabase(db3, period3), new[] { fee3, fee6 });

			Assert(!fees.GetSystemMinimumFeesByDatabase(db1, period2).Any());
			Assert(!fees.GetSystemMinimumFeesByDatabase(db2, period3).Any());
			Assert(!fees.GetSystemMinimumFeesByDatabase(db3, period1).Any());
		}

		class DummyBill : SystemBill, ISystemMinimumFeeContributionBill
		{
			public DummyBill(BusinessObjectFactory factory) : base(factory)
			{
			}

			public IEnumerable<SystemMinimumFee> CalculateMinimumFeeContribution()
			{
				return Fees;
			}

			public IEnumerable<SystemMinimumFee> Fees = Enumerable.Empty<SystemMinimumFee>();
		}
	}
}