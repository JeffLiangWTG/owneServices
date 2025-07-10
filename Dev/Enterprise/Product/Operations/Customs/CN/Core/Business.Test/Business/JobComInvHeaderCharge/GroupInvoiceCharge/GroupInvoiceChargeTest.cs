using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(GroupInvoiceCharge))]
	class GroupInvoiceChargeTest : Customs.Business.Testing.BaseGroupInvoiceChargeTest
	{
		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseGroupInvoiceCharge)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestIsJ7_IsDutiable_ReadOnly()
		{
			var dec = Factory.New<JobDeclaration>();
			var groupHeader = dec.JobComInvoiceGroupHeaders[0];
			var groupCharge = groupHeader.Charges.AddNew();
			Assert(!groupCharge.J7_IsDutiableInfo.ReadOnly);
			Assert(!groupCharge.J7_IsGSTApplicableInfo.ReadOnly);
			groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			Assert(groupCharge.J7_IsDutiableInfo.ReadOnly);
			Assert(groupCharge.J7_IsGSTApplicableInfo.ReadOnly);
			groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			Assert(groupCharge.J7_IsDutiableInfo.ReadOnly);
			Assert(groupCharge.J7_IsGSTApplicableInfo.ReadOnly);
		}

		public void TestAllowNonWesternEuropeanCharacterForChargeDescription()
		{
			var charge = Factory.New<GroupInvoiceCharge>();
			Assert("AllowNonWesternEuropeanCharacterForChargeDescription should be true", charge.AllowNonWesternEuropeanCharacterForChargeDescription);
		}
	}
}
