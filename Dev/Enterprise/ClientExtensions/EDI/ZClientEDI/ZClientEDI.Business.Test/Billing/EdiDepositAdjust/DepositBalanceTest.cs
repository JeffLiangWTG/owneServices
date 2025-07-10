using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(DepositBalance))]
	internal class DepositBalanceTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "DDDSYD";
			org.CreateAndLoadLicenceForOrg();
			return new DepositBalance(org.LicCompany, "");
		}

		#endregion

		public void TestIsValidDefaultValue()
		{
			var dep = GetNewBusinessObject() as DepositBalance;
			AssertEquals(true, dep.IsValid);
		}

		public void TestAdjustmentsHasChanges()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "DDDSYD";
			org.CreateAndLoadLicenceForOrg();

			var deposit1 = org.LicCompany.DepositBalances.OfType<DepositBalance>().First();
			var adjustment1 = deposit1.Adjustments.AddNew();
			adjustment1.DEA_Amount = 12.34;
			adjustment1.DEA_RX_NKCurrency = "AUD";

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var org2 = newFactory.Load<EDIOrgHeader>(org.PK);
			var deposit2 = org2.LicCompany.DepositBalances.OfType<DepositBalance>().First(x => x.ChargeCode == deposit1.ChargeCode);
			var adjustment2 = deposit2.Adjustments[0];
			AssertEquals(adjustment1.PK, adjustment2.PK);
			AssertEquals(12.34m, adjustment2.DEA_Amount);
			AssertEquals(false, org2.LicCompany.DepositBalances.HasChanges);
			AssertEquals(false, org2.LicCompany.HasChanges);
			AssertEquals(false, org2.HasChanges);

			var newAdjustment = deposit2.Adjustments.AddNew();
			AssertEquals(true, org2.LicCompany.DepositBalances.HasChanges);
			AssertEquals(true, org2.LicCompany.HasChanges);
			AssertEquals(true, org2.HasChanges);
		}
	}
}
