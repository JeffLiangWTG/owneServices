using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(FeeChargeLevel))]
	sealed class FeeChargeLevelObjTest : RegistryBusinessObjectTemplateTestCase<FeeChargeLevel>
	{
		public void TestFeeChargeLevelValidation()
		{
			BizObj.Code = ZString.Empty;
			AssertHasError(BizObj.CodeInfo, "Please enter a value.");

			BizObj.Code = "ECD";
			AssertNoErrors(BizObj.CodeInfo);
		}

		public void TestFeeChargeLevelDescription()
		{
			BizObj.Description = (NoResString)ZString.Empty;
			AssertHasError(BizObj.DescriptionInfo, "Please enter a value.");

			BizObj.Description = (NoResString)"Test Description";
			AssertNoErrors(BizObj.DescriptionInfo);
		}

		public void TestAmount1Type()
		{
			BizObj.Amount1Type = ZString.Empty;
			AssertHasError(BizObj.Amount1TypeInfo, "Please enter a value.");

			BizObj.Amount1Type = "EEE";
			AssertHasError(BizObj.Amount1TypeInfo, "Enter a valid selection.");

			BizObj.Amount1Type = OrgConstants.ServiceLevelAmountTypes.Code.Excess;
			AssertNoErrors(BizObj.Amount1TypeInfo);

			BizObj.Amount1 = 300;
			BizObj.Amount1Currency = "AUD";
			BizObj.Amount1Type = OrgConstants.ServiceLevelAmountTypes.Code.None;
			AssertEquals((ZDecimal)0, BizObj.Amount1);
			AssertEquals(ZString.Empty, BizObj.Amount1Currency);
			Assert(BizObj.Amount1Info.ReadOnly);
			Assert(BizObj.Amount1CurrencyInfo.ReadOnly);
		}

		public void TestAmount1Currency()
		{
			BizObj.Amount1Currency = ZString.Empty;
			AssertHasError(BizObj.Amount1CurrencyInfo, "Please enter a value.");

			BizObj.Amount1Currency = "DDD";
			AssertHasError(BizObj.Amount1CurrencyInfo, "Enter a valid selection.");

			BizObj.Amount1Currency = "AUD";
			AssertNoErrors(BizObj.Amount1CurrencyInfo);
		}

		public void TestAmount2Type()
		{
			BizObj.Amount2Type = ZString.Empty;
			AssertHasError(BizObj.Amount2TypeInfo, "Please enter a value.");

			BizObj.Amount2Type = "EEE";
			AssertHasError(BizObj.Amount2TypeInfo, "Enter a valid selection.");

			BizObj.Amount2Type = OrgConstants.ServiceLevelAmountTypes.Code.Maximum;
			AssertNoErrors(BizObj.Amount2TypeInfo);

			BizObj.Amount2 = 300;
			BizObj.Amount2Currency = "AUD";
			BizObj.Amount2Type = OrgConstants.ServiceLevelAmountTypes.Code.None;

			AssertEquals((ZDecimal)0, BizObj.Amount2);
			AssertEquals(ZString.Empty, BizObj.Amount2Currency);
			Assert(BizObj.Amount2Info.ReadOnly);
			Assert(BizObj.Amount2CurrencyInfo.ReadOnly);
		}

		public void TestAmount2Currency()
		{
			BizObj.Amount2Currency = ZString.Empty;
			AssertHasError(BizObj.Amount2CurrencyInfo, "Please enter a value.");

			BizObj.Amount2Currency = "DDD";
			AssertHasError(BizObj.Amount2CurrencyInfo, "Enter a valid selection.");

			BizObj.Amount2Currency = "AUD";
			AssertNoErrors(BizObj.Amount2CurrencyInfo);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override FeeChargeLevel GetBusinessObjectToClone()
		{
			return (FeeChargeLevel)GetNewBusinessObject();
		}

		protected override FeeChargeLevel GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new FeeChargeLevel();
		}
	}
}
