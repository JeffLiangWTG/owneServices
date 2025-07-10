using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusEntryLineFee))]
	public class CusEntryLineFeeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestShouldResetDataOnMerging()
		{
			var fees = (CusEntryLineFee)GetNewBusinessObject();
			AssertEquals("Should reset to zero on merging", true, fees.ShouldResetDataOnMerging);

			fees.CF_RateOverrideReasonCode = "1";
			AssertEquals("Should reset to zero on merging", false, fees.ShouldResetDataOnMerging);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryline = entry.MergedLines.AddNew();
			return entryline.Fees.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryline = entry.MergedLines.AddNew();
			var fee = entryline.Fees.AddNew();
			fee.CF_ChargeAmount = 10m;
			return fee;
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>() { CusEntryLineFee.Schema.CF_Source };
		}
	}
}
