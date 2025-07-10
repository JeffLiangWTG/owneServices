using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public abstract class GuaranteeForEntryInstructionAbstractTest<T> : CommonGuaranteeTest
		where T : GuaranteeForEntryInstruction
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Guarantee Reference", GetNewGuaranteeForEntryInstruction().HumanReadableName);
		}

		public void TestSyncroniseCurrency()
		{
			var guarantee = GetNewGuaranteeForEntryInstruction();

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Type = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			guaranteeHeader.CPH_OH_PermitHolder = Factory.New<OrgHeader>().PK;
			guaranteeHeader.CPH_Number = "G1";
			guaranteeHeader.CPH_UnitOfMeasure = "USD";

			AssertEquals("[PRE-CONDITION] PW_RX_NKCurrency", "EUR", guarantee.PW_RX_NKCurrency);
			guarantee.PW_BondNumber = "G1";
			AssertEquals("Selecting Authorisation, PW_RX_NKCurrency", "USD", guarantee.PW_RX_NKCurrency);

			guarantee.PW_BondNumber = "";
			guarantee.PW_RX_NKCurrency = "GBP";
			guarantee.PW_BondNumber = "G1";
			AssertEquals("Selecting Authorisation, PW_RX_NKCurrency", "USD", guarantee.PW_RX_NKCurrency);

			guarantee.PW_RX_NKCurrency = "AUD";
			guarantee.PW_BondNumber = "XXX";
			AssertEquals("Selecting an invalid Authorisation, PW_RX_NKCurrency", "AUD", guarantee.PW_RX_NKCurrency);
		}

		public void TestSyncroniseSuretyCode()
		{
			var guarantee = GetNewGuaranteeForEntryInstruction();

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Type = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			guaranteeHeader.CPH_OH_PermitHolder = Factory.New<OrgHeader>().PK;
			guaranteeHeader.CPH_Number = "GRN1234123456890";

			var lapRule = guaranteeHeader.CusGuaranteeRules.AddNew();
			lapRule.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.LAP;
			lapRule.CPR_ValueFrom = LiabilityApplicablePercentageCodeList.Codes.HAL;

			guarantee.PW_BondNumber = "GRN1234123456890";
			AssertEquals("Selecting Authorisation, PW_SuretyCode", LiabilityApplicablePercentageCodeList.Codes.HAL, guarantee.PW_SuretyCode);

			guarantee.PW_BondNumber = "";
			guarantee.PW_SuretyCode = LiabilityApplicablePercentageCodeList.Codes.FUL;
			guarantee.PW_BondNumber = "GRN1234123456890";
			AssertEquals("Selecting Authorisation, PW_SuretyCode", LiabilityApplicablePercentageCodeList.Codes.HAL, guarantee.PW_SuretyCode);

			guarantee.PW_SuretyCode = LiabilityApplicablePercentageCodeList.Codes.ZER;
			guarantee.PW_BondNumber = "XXX";
			AssertEquals("Selecting an invalid Authorisation, PW_SuretyCode", LiabilityApplicablePercentageCodeList.Codes.ZER, guarantee.PW_SuretyCode);
		}

		public void TestEntryInstruction()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var guarantee = entryInstruction.Guarantees.AddNew();

			AssertNotNull(guarantee.EntryInstruction);

			AssertEquals("Parent Entry Instruction", entryInstruction.PK, guarantee.EntryInstruction.PK);
		}

		public void TestGuaranteeForEntryInstructionValidationType()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var guarantee = entryInstruction.Guarantees.AddNew();

			AssertType("GuaranteeForEntryInstruction Validation", GetGuaranteeForEntryInstructionValidationType(), guarantee.Validation);
		}

		public void TestGuaranteeForInstructionHolderIdentificationCaptions()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var guarantee = entryInstruction.Guarantees.AddNew();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(guarantee.PW_HolderIdentificationInfo);

			AssertNotNull("PW_HolderIdentificationInfo->ResourceData", resourceStringData);

			CombineAssertions(() =>
			{
				AssertEquals("Short Caption", "Holder ID", resourceStringData.ShortCaption);
				AssertEquals("Medium Caption", "Holder Identification", resourceStringData.MediumCaption);
				AssertEquals("Normal Caption", "Holder Identification", resourceStringData.Caption);
				AssertEquals("Long Caption", "Guarantee Holder Identification", resourceStringData.FullDescription);
			});
		}

		protected virtual Type GetGuaranteeForEntryInstructionValidationType() => typeof(GuaranteeForEntryInstructionValidation);

		protected override BusinessObject GetNewBusinessObject() => GetNewGuaranteeForEntryInstruction();

		protected abstract T GetNewGuaranteeForEntryInstruction();
	}

	[TestedType(typeof(GuaranteeForEntryInstruction))]
	sealed class GuaranteeForEntryInstructionBaseOnlyTest : GuaranteeForEntryInstructionAbstractTest<GuaranteeForEntryInstruction>
	{
		public void TestLabelAndMaxLength()
		{
			var guarantee = GetNewGuaranteeForEntryInstruction();

			CombineAssertions("Captions", () =>
			{
				AssertLabelAndMaxLenght(nameof(guarantee.PW_BondType), guarantee.PW_BondTypeInfo, "Type", 1);
				AssertLabelAndMaxLenght(nameof(guarantee.PW_BondNumber), guarantee.PW_BondNumberInfo, "Reference", 24);
				AssertLabelAndMaxLenght(nameof(guarantee.PW_BondNumber2), guarantee.PW_BondNumber2Info, "Reference 2", 35);
				AssertLabelAndMaxLenght(nameof(guarantee.PW_Password), guarantee.PW_PasswordInfo, "Access Code (PIN)", 4);
				AssertLabelAndMaxLenght(nameof(guarantee.PW_RX_NKCurrency), guarantee.PW_RX_NKCurrencyInfo, "Currency", 3);
				AssertLabelAndMaxLenght(nameof(guarantee.PW_BondAmount), guarantee.PW_BondAmountInfo, "Duty Amount");
				AssertLabelAndMaxLenght(nameof(guarantee.PW_BondFiledPort), guarantee.PW_BondFiledPortInfo, "Customs Office", 8);
				AssertLabelAndMaxLenght(nameof(guarantee.PW_SuretyCode), guarantee.PW_SuretyCodeInfo, "Reduction Fraction", 3);
				AssertLabelAndMaxLenght(nameof(guarantee.PW_HolderIdentification), guarantee.PW_HolderIdentificationInfo, "Holder Identification", 35);
			});

			void AssertLabelAndMaxLenght(string propertyName, ZPropertyInfo propertyInfo, string expectedLabel, int? expectedMaxLenght = null)
			{
				var resourceStringData = propertyInfo.GetAttribute<ResourceStringDataAttribute>();
				AssertNotNull($"ResourceStringData is mandatory for {propertyName}", resourceStringData);
				AssertEquals($"{propertyName} label", expectedLabel, resourceStringData.Caption);
				if (expectedMaxLenght.HasValue)
				{
					AssertEquals($"{propertyName} MaxLength", expectedMaxLenght, propertyInfo.MaxLength);
				}
			}
		}

		public new void TestLookups()
		{
			var guarantee = GetNewGuaranteeForEntryInstruction();
			AssertType<GuaranteeForEntryInstructionLookups>(nameof(guarantee.Lookups), guarantee.Lookups);
		}

		public void TestGuaranteeBondAmountDecimalPlaces()
		{
			var guarantee = GetNewGuaranteeForEntryInstruction();
			AssertEquals("GuaranteeBondAmountDecimalPlaces", 2, guarantee.GuaranteeBondAmountDecimalPlaces);
		}

		protected override GuaranteeForEntryInstruction GetNewGuaranteeForEntryInstruction() => Factory.New<CusEntryInstruction>().Guarantees.AddNew();
	}
}
