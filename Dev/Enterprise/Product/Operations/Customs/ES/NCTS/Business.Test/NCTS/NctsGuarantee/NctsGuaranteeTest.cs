using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(NctsGuarantee))]
	public class NctsGuaranteeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestApportionmentType()
		{
			CombineAssertions(() =>
			{
				AssertApportionmentType("0", GuaranteeApportionmentType.EqualShare);
				AssertApportionmentType("1", GuaranteeApportionmentType.EqualShare);
				AssertApportionmentType("2", GuaranteeApportionmentType.None);
				AssertApportionmentType("3", GuaranteeApportionmentType.None);
				AssertApportionmentType("4", GuaranteeApportionmentType.Voucher);
				AssertApportionmentType("5", GuaranteeApportionmentType.None);
				AssertApportionmentType("6", GuaranteeApportionmentType.EqualShare);
				AssertApportionmentType("7", GuaranteeApportionmentType.None);
				AssertApportionmentType("8", GuaranteeApportionmentType.EqualShare);
				AssertApportionmentType("9", GuaranteeApportionmentType.None);
				AssertApportionmentType("A", GuaranteeApportionmentType.None);
				AssertApportionmentType("B", GuaranteeApportionmentType.None);
				AssertApportionmentType("X", GuaranteeApportionmentType.None);
			});
		}

		public void TestPW_OverrideReadOnly()
		{
			var (nctsHeader, nctsGuarantee) = CreateHeaderAndGuarantee(Factory);
			try
			{
				Env.Security.NctsDepartureAllowBondAmountOverride.IsAllowed = true;
				AssertEquals("Security, PW_OverrideReadOnly", false, nctsGuarantee.PW_OverrideInfo.ReadOnly);
				Env.Security.NctsDepartureAllowBondAmountOverride.IsAllowed = false;
				AssertEquals("Without security, PW_OverrideReadOnly", true, nctsGuarantee.PW_OverrideInfo.ReadOnly);

				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
				AssertEquals("For arrival, the fields should not be readonly", false, nctsGuarantee.PW_OverrideInfo.ReadOnly);
			}
			finally
			{
				Env.Security.NctsDepartureAllowBondAmountOverride.ClearOverriddenSecurityValue();
			}
		}

		public void TestPW_BondNumberReadOnly_Phase5Arrival()
		{
			var (_, nctsGuarantee) = CreateHeaderAndGuarantee(Factory, NctsMovementType.Codes.Arrival);
			AssertEquals("PW_BondNumberReadOnly", false, nctsGuarantee.PW_BondNumberInfo.ReadOnly);
		}

		public void TestPW_BondAmountReadOnly_Phase5Arrival()
		{
			var (_, nctsGuarantee) = CreateHeaderAndGuarantee(Factory, NctsMovementType.Codes.Arrival);
			CombineAssertions(() =>
			{
				nctsGuarantee.PW_Override = true;
				AssertEquals("PW_BondAmountReadOnly when PW_Override = true", false, nctsGuarantee.PW_BondAmountInfo.ReadOnly);
				nctsGuarantee.PW_Override = false;
				AssertEquals("PW_BondAmountReadOnly when PW_Override = false", true, nctsGuarantee.PW_BondAmountInfo.ReadOnly);
			});
		}

		public void TestPW_RX_NKCurrencyReadOnly_Phase5Arrival()
		{
			var (_, nctsGuarantee) = CreateHeaderAndGuarantee(Factory, NctsMovementType.Codes.Arrival);
			AssertEquals("PW_RX_NKCurrency", true, nctsGuarantee.PW_RX_NKCurrencyInfo.ReadOnly);
		}

		public void TestPW_OverrideyReadOnly_Phase5Arrival()
		{
			var (_, nctsGuarantee) = CreateHeaderAndGuarantee(Factory, NctsMovementType.Codes.Arrival);
			AssertEquals("PW_Override", false, nctsGuarantee.PW_OverrideInfo.ReadOnly);
		}

		public void TestValidation_Phase4()
		{
			var (nctsHeader, nctsGuarantee) = CreateHeaderAndGuarantee(Factory);
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			
			AssertType<NctsGuaranteeValidation>(nctsGuarantee.Validation);
		}

		public void TestValidation_Phase5()
		{
			var (nctsHeader, nctsGuarantee) = CreateHeaderAndGuarantee(Factory);

			AssertType<NctsGuaranteePhase5Validation>(nctsGuarantee.Validation);
		}

		public void TestPW_BondNumber2ReadOnly()
		{
			var (nctsHeader, nctsGuarantee) = CreateHeaderAndGuarantee(Factory);

			CombineAssertions(() =>
			{
				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("When Phase4 PW_BondNumber2ReadOnly is false", false, nctsGuarantee.PW_BondNumber2Info.ReadOnly);

				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				AssertEquals("When Phase5 PW_BondNumber2ReadOnly is true", true, nctsGuarantee.PW_BondNumber2Info.ReadOnly);
			});
		}

		public void TestPW_BondNumber2DefaultWhenReadOnly()
		{
			var (nctsHeader, nctsGuarantee) = CreateHeaderAndGuarantee(Factory);

			CombineAssertions(() =>
			{
				nctsGuarantee.PW_BondNumber2 = "Test";
				AssertEquals("PW_BondNumber2 is set with a value before setting PW_BondType", "Test", nctsGuarantee.PW_BondNumber2);

				nctsGuarantee.PW_BondType = "8";
				AssertEquals("PW_BondNumber2 is set to empty after setting PW_BondType (PW_BondNumber2 is readonly)", ZString.Empty, nctsGuarantee.PW_BondNumber2);
			});
		}

		public void TestPW_SuretyCodeReadOnly()
		{
			var (nctsHeader, nctsGuarantee) = CreateHeaderAndGuarantee(Factory);

			CombineAssertions(() =>
			{
				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("When Phase4 PW_SuretyCodeReadOnly is false", false, nctsGuarantee.PW_SuretyCodeInfo.ReadOnly);

				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeNotRequiredForCertainPublicBodies;
				AssertEquals("When Phase5 and PW_BondType is 8, PW_SuretyCodeReadOnly is true", true, nctsGuarantee.PW_SuretyCodeInfo.ReadOnly);

				nctsGuarantee.PW_BondType = ZString.Empty;
				AssertEquals("When Phase5 and PW_BondType is empty, PW_SuretyCodeReadOnly is false", false, nctsGuarantee.PW_SuretyCodeInfo.ReadOnly);

				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaived;
				AssertEquals("When Phase5 and PW_BondType is 6, PW_SuretyCodeReadOnly is true", true, nctsGuarantee.PW_SuretyCodeInfo.ReadOnly);

				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.ComprehensiveGuarantee;
				AssertEquals("When Phase5 and PW_BondType is not 5, 6 or 8, PW_SuretyCodeReadOnly is false", false, nctsGuarantee.PW_SuretyCodeInfo.ReadOnly);

				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaivedForAmount0;
				AssertEquals("When Phase5 and PW_BondType is 5, PW_SuretyCodeReadOnly is true", true, nctsGuarantee.PW_SuretyCodeInfo.ReadOnly);
			});
		}

		public void TestPW_SuretyCodeDefaultWhenReadOnly()
		{
			var (nctsHeader, nctsGuarantee) = CreateHeaderAndGuarantee(Factory);

			CombineAssertions(() =>
			{
				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				nctsGuarantee.PW_SuretyCode = LiabilityApplicablePercentageCodeList.Codes.FUL;
				AssertEquals("PW_SuretyCode is set with a value before setting PW_BondType", "FUL", nctsGuarantee.PW_SuretyCode);

				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeNotRequiredForCertainPublicBodies;
				AssertEquals("PW_SuretyCode is set to ZER after setting PW_BondType with 8 (PW_SuretyCode is readonly)", LiabilityApplicablePercentageCodeList.Codes.ZER, nctsGuarantee.PW_SuretyCode);

				nctsGuarantee.PW_SuretyCode = LiabilityApplicablePercentageCodeList.Codes.HAL;
				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.ComprehensiveGuarantee;
				AssertEquals("PW_SuretyCode is not changed after setting PW_BondType with a value different from 5, 6 or 8 (PW_SuretyCode is not readonly)", "HAL", nctsGuarantee.PW_SuretyCode);

				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaived;
				AssertEquals("PW_SuretyCode is set to ZER after setting PW_BondType with 6 (PW_SuretyCode is readonly)", LiabilityApplicablePercentageCodeList.Codes.ZER, nctsGuarantee.PW_SuretyCode);

				nctsGuarantee.PW_SuretyCode = LiabilityApplicablePercentageCodeList.Codes.HAL;
				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaiver;
				AssertEquals("PW_SuretyCode is not changed after setting PW_BondType with a value different from 5, 6 or 8 (PW_SuretyCode is not readonly)", "HAL", nctsGuarantee.PW_SuretyCode);

				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaivedForAmount0;
				AssertEquals("PW_SuretyCode is set to ZER after setting PW_BondType with 5 (PW_SuretyCode is readonly)", LiabilityApplicablePercentageCodeList.Codes.ZER, nctsGuarantee.PW_SuretyCode);
			});
		}

		public void TestPW_BondAmountReadOnlyForArrival()
		{
			var (nctsHeader, nctsGuarantee) = CreateHeaderAndGuarantee(Factory, NctsMovementType.Codes.Arrival);

			CombineAssertions(() =>
			{
				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
				nctsGuarantee.PW_Override = true;
				AssertEquals("When Phase4 ", false, nctsGuarantee.PW_BondAmountInfo.ReadOnly);
				nctsGuarantee.PW_Override = false;
				AssertEquals("When Phase4 and PW_Override is not cheched", false, nctsGuarantee.PW_BondAmountInfo.ReadOnly);

				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				nctsGuarantee.PW_Override = true;
				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeNotRequiredForCertainPublicBodies;
				AssertEquals("When Phase5 and PW_BondType is 8 and PW_Override = true", false, nctsGuarantee.PW_BondAmountInfo.ReadOnly);
				nctsGuarantee.PW_Override = false;
				AssertEquals("When Phase5 and PW_BondType is 8 and PW_Override = false", true, nctsGuarantee.PW_BondAmountInfo.ReadOnly);

				nctsGuarantee.PW_BondType = ZString.Empty;
				nctsGuarantee.PW_Override = true;
				AssertEquals("When Phase5 and PW_BondType is empty and PW_Override = true", false, nctsGuarantee.PW_BondAmountInfo.ReadOnly);
				nctsGuarantee.PW_Override = false;
				AssertEquals("When Phase5 and PW_BondType is empty and PW_Override = false", true, nctsGuarantee.PW_BondAmountInfo.ReadOnly);

				nctsGuarantee.PW_Override = true;
				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaived;
				AssertEquals("When Phase5 and PW_BondType is 6 and PW_Override = true", false, nctsGuarantee.PW_BondAmountInfo.ReadOnly);
				nctsGuarantee.PW_Override = false;
				AssertEquals("When Phase5 and PW_BondType is 6 and PW_Override = false", true, nctsGuarantee.PW_BondAmountInfo.ReadOnly);

				nctsGuarantee.PW_Override = true;
				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.ComprehensiveGuarantee;
				AssertEquals("When Phase5 and PW_BondType is 1 and PW_Override = true", false, nctsGuarantee.PW_BondAmountInfo.ReadOnly);
				nctsGuarantee.PW_Override = false;
				AssertEquals("When Phase5 and PW_BondType is 1 and PW_Override = false", true, nctsGuarantee.PW_BondAmountInfo.ReadOnly);

				nctsGuarantee.PW_Override = true;
				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaivedForAmount0;
				AssertEquals("When Phase5 and PW_BondType is 5 and PW_Override = true", false, nctsGuarantee.PW_BondAmountInfo.ReadOnly);
				nctsGuarantee.PW_Override = false;
				AssertEquals("When Phase5 and PW_BondType is 5 and PW_Override = false", true, nctsGuarantee.PW_BondAmountInfo.ReadOnly);

				nctsGuarantee.PW_Override = true;
				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaiver;
				AssertEquals("When Phase5 and PW_BondType is 0 and PW_Override = true", false, nctsGuarantee.PW_BondAmountInfo.ReadOnly);
				nctsGuarantee.PW_Override = false;
				AssertEquals("When Phase5 and PW_BondType is 0 and PW_Override = false", true, nctsGuarantee.PW_BondAmountInfo.ReadOnly);
			});
		}

		public void TestPW_BondAmountReadOnlyForDeparture()
		{
			var (nctsHeader, nctsGuarantee) = CreateHeaderAndGuarantee(Factory, NctsMovementType.Codes.Departure);

			CombineAssertions(() =>
			{
				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
				nctsGuarantee.PW_Override = true;
				AssertEquals("When Phase4 ", false, nctsGuarantee.PW_BondAmountInfo.ReadOnly);
				nctsGuarantee.PW_Override = false;
				AssertEquals("When Phase4 and PW_Override is not cheched", false, nctsGuarantee.PW_BondAmountInfo.ReadOnly);

				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				nctsGuarantee.PW_Override = true;
				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeNotRequiredForCertainPublicBodies;
				AssertEquals("When Phase5 and PW_BondType is 8 and PW_Override = true", true, nctsGuarantee.PW_BondAmountInfo.ReadOnly);
				nctsGuarantee.PW_Override = false;
				AssertEquals("When Phase5 and PW_BondType is 8 and PW_Override = false", true, nctsGuarantee.PW_BondAmountInfo.ReadOnly);

				nctsGuarantee.PW_BondType = ZString.Empty;
				nctsGuarantee.PW_Override = true;
				AssertEquals("When Phase5 and PW_BondType is empty and PW_Override = true", false, nctsGuarantee.PW_BondAmountInfo.ReadOnly);
				nctsGuarantee.PW_Override = false;
				AssertEquals("When Phase5 and PW_BondType is empty and PW_Override = false", false, nctsGuarantee.PW_BondAmountInfo.ReadOnly);

				nctsGuarantee.PW_Override = true;
				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaived;
				AssertEquals("When Phase5 and PW_BondType is 6 and PW_Override = true", true, nctsGuarantee.PW_BondAmountInfo.ReadOnly);
				nctsGuarantee.PW_Override = false;
				AssertEquals("When Phase5 and PW_BondType is 6 and PW_Override = false", true, nctsGuarantee.PW_BondAmountInfo.ReadOnly);

				nctsGuarantee.PW_Override = true;
				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.ComprehensiveGuarantee;
				AssertEquals("When Phase5 and PW_BondType is 1 and PW_Override = true", false, nctsGuarantee.PW_BondAmountInfo.ReadOnly);
				nctsGuarantee.PW_Override = false;
				AssertEquals("When Phase5 and PW_BondType is 1and PW_Override = false", false, nctsGuarantee.PW_BondAmountInfo.ReadOnly);

				nctsGuarantee.PW_Override = true;
				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaivedForAmount0;
				AssertEquals("When Phase5 and PW_BondType is 5 and PW_Override = true", true, nctsGuarantee.PW_BondAmountInfo.ReadOnly);
				nctsGuarantee.PW_Override = false;
				AssertEquals("When Phase5 and PW_BondType is 5 and PW_Override = false", true, nctsGuarantee.PW_BondAmountInfo.ReadOnly);

				nctsGuarantee.PW_Override = true;
				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaiver;
				AssertEquals("When Phase5 and PW_BondType is 0 and PW_Override = true", false, nctsGuarantee.PW_BondAmountInfo.ReadOnly);
				nctsGuarantee.PW_Override = false;
				AssertEquals("When Phase5 and PW_BondType is 0 and PW_Override = false", false, nctsGuarantee.PW_BondAmountInfo.ReadOnly);
			});
		}

		public void TestPW_BondAmountDefaultWhenReadOnly()
		{
			var (nctsHeader, nctsGuarantee) = CreateHeaderAndGuarantee(Factory);

			CombineAssertions(() =>
			{
				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				nctsGuarantee.PW_BondAmount = 10.12m;
				AssertEquals("PW_BondAmount is set with a value before setting PW_BondType", 10.12m, nctsGuarantee.PW_BondAmount);

				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeNotRequiredForCertainPublicBodies;
				AssertEquals("PW_BondAmount is set to 0 after setting PW_BondType with 8 (PW_BondAmount is readonly)", ZDecimal.Zero, nctsGuarantee.PW_BondAmount);

				nctsGuarantee.PW_BondAmount = 20.11m;
				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.ComprehensiveGuarantee;
				AssertEquals("PW_BondAmount is not changed after setting PW_BondType with a value different from 5, 6 or 8 (PW_BondAmount is not readonly)", 20.11m, nctsGuarantee.PW_BondAmount);

				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaived;
				AssertEquals("PW_BondAmount is set to 0 after setting PW_BondType with 6 (PW_BondAmount is readonly)", ZDecimal.Zero, nctsGuarantee.PW_BondAmount);

				nctsGuarantee.PW_BondAmount = 22.11m;
				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaiver;
				AssertEquals("PW_BondAmount is not changed after setting PW_BondType with a value different from 5, 6 or 8 (PW_BondAmount is not readonly)", 22.11m, nctsGuarantee.PW_BondAmount);

				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaivedForAmount0;
				AssertEquals("PW_BondAmount is set to 0 after setting PW_BondType with 5 (PW_BondAmount is readonly)", ZDecimal.Zero, nctsGuarantee.PW_BondAmount);
			});
		}

		public void TestPW_BondNumberReadOnly()
		{
			var (nctsHeader, nctsGuarantee) = CreateHeaderAndGuarantee(Factory);

			CombineAssertions(() =>
			{
				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("When Phase4 PW_BondNumberReadOnly is false", false, nctsGuarantee.PW_BondNumberInfo.ReadOnly);

				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeNotRequiredForCertainPublicBodies;
				AssertEquals("When Phase5 and PW_BondType is 8, PW_BondNumberReadOnly is true", true, nctsGuarantee.PW_BondNumberInfo.ReadOnly);

				nctsGuarantee.PW_BondType = ZString.Empty;
				AssertEquals("When Phase5 and PW_BondType is empty, PW_BondNumberReadOnly is false", false, nctsGuarantee.PW_BondNumberInfo.ReadOnly);

				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaived;
				AssertEquals("When Phase5 and PW_BondType is 6, PW_BondNumberReadOnly is true", true, nctsGuarantee.PW_BondNumberInfo.ReadOnly);

				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.ComprehensiveGuarantee;
				AssertEquals("When Phase5 and PW_BondType is not B, 5, 6 or 8, PW_BondNumberReadOnly is false", false, nctsGuarantee.PW_BondNumberInfo.ReadOnly);

				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaivedForAmount0;
				AssertEquals("When Phase5 and PW_BondType is 5, PW_BondNumberReadOnly is true", true, nctsGuarantee.PW_BondNumberInfo.ReadOnly);

				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaiver;
				AssertEquals("When Phase5 and PW_BondType is not B, 5, 6 or 8, PW_BondNumberReadOnly is false", false, nctsGuarantee.PW_BondNumberInfo.ReadOnly);

				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeForGoodsDispatchedUnderTirProcedure;
				AssertEquals("When Phase5 and PW_BondType is B, PW_BondNumberReadOnly is true", true, nctsGuarantee.PW_BondNumberInfo.ReadOnly);
			});
		}

		public void TestPW_BondNumberDefaultWhenReadOnly()
		{
			var (nctsHeader, nctsGuarantee) = CreateHeaderAndGuarantee(Factory);

			CombineAssertions(() =>
			{
				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				nctsGuarantee.PW_BondNumber = "Test";
				AssertEquals("PW_BondNumber is set with a value before setting PW_BondType", "Test", nctsGuarantee.PW_BondNumber);

				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeNotRequiredForCertainPublicBodies;
				AssertEquals("PW_BondNumber is set to empty after setting PW_BondType with 8 (PW_BondNumber is readonly)", ZString.Empty, nctsGuarantee.PW_BondNumber);

				nctsGuarantee.PW_BondNumber = "Test2";
				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.ComprehensiveGuarantee;
				AssertEquals("PW_BondNumber is not changed after setting PW_BondType with a value different from B, 5, 6 or 8 (PW_BondNumber is not readonly)", "Test2", nctsGuarantee.PW_BondNumber);

				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaived;
				AssertEquals("PW_BondNumber is set to empty after setting PW_BondType with 6 (PW_BondNumber is readonly)", ZString.Empty, nctsGuarantee.PW_BondNumber);

				nctsGuarantee.PW_BondNumber = "Test3";
				nctsGuarantee.PW_BondType = ZString.Empty;
				AssertEquals("PW_BondNumber is not changed after setting PW_BondType empty (PW_BondNumber is not readonly)", "Test3", nctsGuarantee.PW_BondNumber);

				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaivedForAmount0;
				AssertEquals("PW_BondNumber is set to empty after setting PW_BondType with 5 (PW_BondNumber is readonly)", ZString.Empty, nctsGuarantee.PW_BondNumber);

				nctsGuarantee.PW_BondNumber = "Test4";
				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeWaiver;
				AssertEquals("PW_BondNumber is not changed after setting with a value different from B, 5, 6 or 8 (PW_BondNumber is not readonly)", "Test4", nctsGuarantee.PW_BondNumber);

				nctsGuarantee.PW_BondType = ESNCTS5GuaranteeTypeList.Codes.GuaranteeForGoodsDispatchedUnderTirProcedure;
				AssertEquals("PW_BondNumber is set to ZER after setting PW_BondType with B (PW_BondNumber is readonly)", ZString.Empty, nctsGuarantee.PW_BondNumber);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => CreateHeaderAndGuarantee(Factory).Guarantee;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateHeaderAndGuarantee(factory).Guarantee;

		(NctsHeader Header, NctsGuarantee Guarantee) CreateHeaderAndGuarantee(BusinessObjectFactory factory, string type = NctsMovementType.Codes.Departure)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(type);
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var nctsGuarantee = (NctsGuarantee)nctsHeader.GetEffectiveGuarantees().AddNew();

			return (nctsHeader, nctsGuarantee);
		}

		void AssertApportionmentType(ZString guaranteeType, GuaranteeApportionmentType apportionmentType)
		{
			var (_, nctsGuarantee) = CreateHeaderAndGuarantee(Factory);

			nctsGuarantee.PW_BondType = guaranteeType;
			AssertEquals(guaranteeType, apportionmentType, nctsGuarantee.ApportionmentType);
		}
	}
}
