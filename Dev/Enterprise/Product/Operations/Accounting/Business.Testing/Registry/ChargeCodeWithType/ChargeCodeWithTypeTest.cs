using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ChargeCodeWithType))]
	public class ChargeCodeWithTypeTest : RegistryBusinessObjectTemplateTestCase<ChargeCodeWithType>
	{
		public void TestChargeCodeListSetsCompanyPK()
		{
			GlbCompany newCompany = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();
			ChargeCodeWithType chargeCodeWithType1 = (ChargeCodeWithType)GetNewBusinessObject();
			chargeCodeWithType1.CurrentFallbackLevel = null;
			IBusinessObjectCollection list = chargeCodeWithType1.ChargeCodeList;
			AssertEquals("company pk should be empty", ZGuid.Empty, ((AccChargeCodeCollectionForRegistry)list).CompanyPK);

			ChargeCodeWithType chargeCodeWithType2 = (ChargeCodeWithType)GetNewBusinessObject();
			chargeCodeWithType2.CurrentFallbackLevel = new FallbackLevel(newCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			list = chargeCodeWithType2.ChargeCodeList;
			AssertEquals("company pk should be set to the list", newCompany.PK, ((AccChargeCodeCollectionForRegistry)list).CompanyPK);
		}

		public void TestChargeDescription()
		{
			BusinessObject charge = Factory.NewWithValidTestData<AccChargeCode>();
			charge[AccChargeCodeSchema.AC_Desc] = "ChargeDescription";
			Factory.Save();

			ChargeCodeWithType chargeCodeWithType = (ChargeCodeWithType)GetNewBusinessObject();
			chargeCodeWithType.ChargeCode = charge.PK;

			AssertEquals("ChargeCode", charge.PK, chargeCodeWithType.ChargeCode);
			AssertEquals("ChargeDescription", "ChargeDescription", chargeCodeWithType.ChargeCodeDescription);
		}

		public void TestUseDefaultProfitShareChargeCode()
		{
			BusinessObject charge = Factory.NewWithValidTestData<AccChargeCode>();
			charge[AccChargeCodeSchema.AC_Desc] = "ChargeDescription";
			Factory.Save();

			ChargeCodeWithType chargeCodeWithType = (ChargeCodeWithType)GetNewBusinessObject();
			chargeCodeWithType.ChargeCode = charge.PK;

			AssertEquals("UseDefaultProfitShareChargeCode", false, chargeCodeWithType.UseDefaultProfitShareChargeCode);
			AssertEquals("ChargeCode", charge.PK, chargeCodeWithType.ChargeCode);

			chargeCodeWithType.UseDefaultProfitShareChargeCode = true;
			AssertEquals("UseDefaultProfitShareChargeCode", true, chargeCodeWithType.UseDefaultProfitShareChargeCode);
			AssertEquals("ChargeCode set to default", ZGuid.Empty, chargeCodeWithType.ChargeCode);
		}

		public void TestRequiredValueForProfitShareChargeCode()
		{
			string requiredError = "Please enter a value.";
			var chargeCodeWithType = (ChargeCodeWithType)GetNewBusinessObject();

			chargeCodeWithType.UseDefaultProfitShareChargeCode = ZBool.True;
			Assert("Precondition: Use Default Profit Share Charge Code", chargeCodeWithType.UseDefaultProfitShareChargeCode);
			AssertNoError("Charge Code value is not required when using default profit share charge code", chargeCodeWithType.ChargeCodeInfo, requiredError);

			chargeCodeWithType.UseDefaultProfitShareChargeCode = ZBool.False;
			Assert("Precondition: Don't use Default Profit Share Charge Code", !chargeCodeWithType.UseDefaultProfitShareChargeCode);
			AssertHasError("Charge Code value is required when not using default profit share charge code", chargeCodeWithType.ChargeCodeInfo, requiredError);
		}

		#region Implementation

		protected override ChargeCodeWithType GetBusinessObjectToClone()
		{
			return new ChargeCodeWithType();
		}

		protected override ChargeCodeWithType GetBusinessObjectToSerialise()
		{
			BusinessObject chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			BizObj.ChargeCode = chargeCode.PK;

			return BizObj;
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected new ChargeCodeWithType BizObj
		{
			get { return base.BizObj; }
		}

		#endregion
	}
}
