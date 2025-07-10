using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(NZCustomsUsage))]
	internal class NZCustomsUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			var usage = CreateUsage();
			AssertEquals(BillingConstants.BillingSystem.NZCustoms, usage.SystemCode);
		}

		public void TestUsageTypes()
		{
			var usage = CreateUsage();
			Assert(usage.IsJobUsage);

			usage.SubCode = "NZC";
			Assert(usage.IsMessageUsage);
		}

		#region Implementation

		NZCustomsUsage CreateUsage()
		{
			return GetNewBusinessObject() as NZCustomsUsage;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NZCustomsUsage("DEC", Factory, new UsingParty(), EdiDateTest.MonthToday);
		}

		protected override void SetUp()
		{
			base.SetUp();
			LicenceCompany.ClearStandardPricesCompanyCache();
		}

		protected override void TearDown()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();
			base.TearDown();
		}

		#endregion
	}
}
