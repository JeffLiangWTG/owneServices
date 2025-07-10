using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusBondDetailValidationTest : TestCaseWithFactory
	{
		public void TestValidateUS_BondType()
		{
			bondData.PW_BondType = "~";
			AssertHasError(bondData.PW_BondTypeInfo, "Enter a valid selection.");
			bondData.PW_BondType = bondData.Lookups.BondTypeList[0].Code;
			AssertNoError(bondData.PW_BondTypeInfo, "Enter a valid selection.");
		}

		public void TestCheckPW_BondExpiryDate()
		{
			bondData.PW_BondEffectiveDate = ZDateTime.Today;
			bondData.PW_BondExpiryDate = ZDateTime.Today.AddDays(-2);
			AssertHasError(bondData.PW_BondExpiryDateInfo, CusBondDetailValidation.ExpiryDateLessThenEffectiveDate);
			bondData.PW_BondExpiryDate = ZDateTime.Today.AddDays(4);
			AssertNoError(bondData.PW_BondExpiryDateInfo, CusBondDetailValidation.ExpiryDateLessThenEffectiveDate);
		}

		public void TestCheckPW_BondEffectiveDate()
		{
			AssertNoErrors(bondData.PW_BondEffectiveDateInfo);
			bondData.Validation.ValidatePW_BondEffectiveDate();
			AssertHasErrors(bondData.PW_BondEffectiveDateInfo);
			bondData.PW_BondExpiryDate = ZDateTime.Today;
			bondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(5);
			AssertHasError(bondData.PW_BondExpiryDateInfo, CusBondDetailValidation.ExpiryDateLessThenEffectiveDate);
			bondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-5);
			AssertNoError(bondData.PW_BondExpiryDateInfo, CusBondDetailValidation.ExpiryDateLessThenEffectiveDate);
			bondData.PW_BondType = BondTypeList.Codes.SingleTransactionBond;
			bondData.PW_BondEffectiveDate = ZDateTime.Empty;
			AssertNoErrors(bondData.PW_BondEffectiveDateInfo);
			bondData.PW_BondType = BondTypeList.Codes.OnPortal;
			AssertNoErrors(bondData.PW_BondEffectiveDateInfo);
		}

		public void TestCheckPW_BondAmount()
		{
			bondData.PW_BondAmount = -23.1m;
			AssertHasErrors(bondData.PW_BondAmountInfo);
			bondData.PW_BondAmount = 23.1m;
			AssertNoErrors(bondData.PW_BondAmountInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			organisation = Factory.New<OrgHeader>();
			bondData = Factory.New<CusBondDetail>();
			bondData.Parent = organisation;
		}
		OrgHeader organisation;
		CusBondDetail bondData;
	}
}
