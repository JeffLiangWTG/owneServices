using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderCharges))]
	class CusEntryHeaderChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestC1_ChargeAmount_ReadOnly()
		{
			var charge = Factory.New<CusEntryHeaderCharges>();
			charge.C1_RateOverrideReasonCode = ZString.Empty;
			Assert("ChargeAmount must be read only when C1_RateOverrideReasonCode is empty", charge.C1_ChargeAmountInfo.ReadOnly);
			charge.C1_RateOverrideReasonCode = "1";
			Assert("ChargeAmount should not be read only when C1_RateOverrideReasonCode is not empty", !charge.C1_ChargeAmountInfo.ReadOnly);
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>() { CusEntryHeaderCharges.Schema.C1_Source };
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew().Charges.AddNew();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var charge = entry.Charges.AddNew();
			charge.C1_ChargeAmount = 10m;
			return charge;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew().Charges.AddNew();
	}
}
