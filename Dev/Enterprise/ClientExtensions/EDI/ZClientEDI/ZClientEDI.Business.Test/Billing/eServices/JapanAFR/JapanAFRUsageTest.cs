using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(JapanAFRUsage))]
	internal class JapanAFRUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			var usage = CreateUsage();
			AssertEquals(BillingConstants.BillingSystem.JapanAFR, usage.SystemCode);
		}

		#region Implementation

		JapanAFRUsage CreateUsage()
		{
			return GetNewBusinessObject() as JapanAFRUsage;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new JapanAFRUsage(Factory, new UsingParty(), EdiDateTest.MonthToday);
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
