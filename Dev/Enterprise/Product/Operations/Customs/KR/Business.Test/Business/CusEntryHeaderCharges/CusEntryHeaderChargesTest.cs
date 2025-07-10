using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderCharges))]
	public class CusEntryHeaderChargesTest : EnterpriseBusinessObjectTestCase
	{
		public void TestShouldResetDataOnMerging()
		{
			var charge = (CusEntryHeaderCharges)GetNewBusinessObject();
			AssertEquals("Should reset to zero on merging", true, charge.ShouldResetDataOnMerging);

			charge.C1_RateOverrideReasonCode = "1";
			AssertEquals("Should reset to zero on merging", false, charge.ShouldResetDataOnMerging);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			return entry.Charges.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var charge = entry.Charges.AddNew();
			charge.C1_ChargeAmount = 10m;
			return charge;
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>() { CusEntryHeaderCharges.Schema.C1_Source };
		}
	}
}
