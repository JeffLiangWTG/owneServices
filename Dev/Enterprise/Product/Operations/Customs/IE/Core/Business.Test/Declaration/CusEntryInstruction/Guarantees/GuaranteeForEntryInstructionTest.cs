using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(GuaranteeForEntryInstruction))]
	sealed class GuaranteeForEntryInstructionTest : EU.Business.Declaration.Testing.GuaranteeForEntryInstructionAbstractTest<GuaranteeForEntryInstruction>
	{
		public override void TestLookups()
		{
			AssertType<GuaranteeForEntryInstructionLookups>("LooksUps for GuaranteeForEntryInstruction should be IE.GuaranteeForEntryInstructionLookups", guarantee.Lookups);
		}

		protected override GuaranteeForEntryInstruction GetNewGuaranteeForEntryInstruction() => Factory.New<CusEntryInstruction>().Guarantees.AddNew();

		public void TestPW_CPH_Guarantee_Caption()
		{
			AssertEquals("Guarantee", DataBoundResourceStrings.GetDataForProperty(guarantee.PW_CPH_GuaranteeInfo).Caption);
		}

		public void TestPW_BondType_Caption()
		{
			AssertEquals("Type", DataBoundResourceStrings.GetDataForProperty(guarantee.PW_BondTypeInfo).Caption);
		}

		public void TestPW_BondType_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Type Read-Only when no guarantee selected", false, guarantee.PW_BondTypeInfo.ReadOnly);
				guarantee.PW_CPH_Guarantee = Factory.NewWithValidTestData<CusGuaranteeHeader>().PK;
				AssertEquals("Type Read-Only when guarantee is selected", true, guarantee.PW_BondTypeInfo.ReadOnly);
			});
		}

		public void TestPW_BondNumber_Caption()
		{
			AssertEquals("Guarantee Number", DataBoundResourceStrings.GetDataForProperty(guarantee.PW_BondNumberInfo).Caption);
		}

		public void TestPW_BondNumber_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Guaranree Number Read-Only when no guarantee selected", false, guarantee.PW_BondNumberInfo.ReadOnly);
				guarantee.PW_CPH_Guarantee = Factory.NewWithValidTestData<CusGuaranteeHeader>().PK;
				AssertEquals("Guarantee Number Read-Only when guarantee is selected", true, guarantee.PW_BondNumberInfo.ReadOnly);
			});
		}

		public void TestPW_GuaranteeDescription_Caption()
		{
			AssertEquals("Reference", DataBoundResourceStrings.GetDataForProperty(guarantee.PW_GuaranteeDescriptionInfo).Caption);
		}

		public void TestPW_Password_Caption()
		{
			AssertEquals("Access Code", DataBoundResourceStrings.GetDataForProperty(guarantee.PW_PasswordInfo).Caption);
		}

		public void TestPW_Password_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Access Code Read-Only when no guarantee selected", false, guarantee.PW_PasswordInfo.ReadOnly);
				guarantee.PW_CPH_Guarantee = Factory.NewWithValidTestData<CusGuaranteeHeader>().PK;
				AssertEquals("Access Code Read-Only when guarantee is selected", true, guarantee.PW_PasswordInfo.ReadOnly);
			});
		}

		public void TestPW_BondAmount_Caption()
		{
			AssertEquals("Amount", DataBoundResourceStrings.GetDataForProperty(guarantee.PW_BondAmountInfo).Caption);
		}

		public void TestPW_RX_NKCurrency_Caption()
		{
			AssertEquals("Currency", DataBoundResourceStrings.GetDataForProperty(guarantee.PW_RX_NKCurrencyInfo).Caption);
		}

		public void TestPW_RX_NKCurrency_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Currency Read-Only when no guarantee selected", false, guarantee.PW_RX_NKCurrencyInfo.ReadOnly);
				guarantee.PW_CPH_Guarantee = Factory.NewWithValidTestData<CusGuaranteeHeader>().PK;
				AssertEquals("Currency Read-Only when guarantee is selected", true, guarantee.PW_RX_NKCurrencyInfo.ReadOnly);
			});
		}

		public void TestPW_BondFiledPort_Caption()
		{
			AssertEquals("Customs Office", DataBoundResourceStrings.GetDataForProperty(guarantee.PW_BondFiledPortInfo).Caption);
		}

		public void TestPW_RN_NKCountryOfIssue_Caption()
		{
			AssertEquals("CC Qualifier", DataBoundResourceStrings.GetDataForProperty(guarantee.PW_RN_NKCountryOfIssueInfo).Caption);
		}

		public void TestDefaultingOfPW_BondTypeAndNumber()
		{
			var cusGuaranteeHeader = base.Factory.NewWithValidTestData<CusGuaranteeHeader>();
			cusGuaranteeHeader.CPH_Type = "GUA";
			cusGuaranteeHeader.CPH_OH_PermitHolder = base.Factory.New<OrgHeader>().PK;
			cusGuaranteeHeader.CPH_Number = "G1";
			cusGuaranteeHeader.CPH_UnitOfMeasure = "USD";
			cusGuaranteeHeader.MainAccessCode = "PWD";
			guarantee.PW_CPH_Guarantee = cusGuaranteeHeader.PK;

			CombineAssertions(() =>
			{
				AssertEquals("When selecting a guarantee header, the bond type must be defaulted", "G", guarantee.PW_BondType);
				AssertEquals("When selecting a guarantee header, the bond type must be made read only", true, guarantee.PW_BondTypeInfo.ReadOnly);

				AssertEquals("When selecting a guarantee header, the bond number must be defaulted", "G1", guarantee.PW_BondNumber);
				AssertEquals("When selecting a guarantee header, the bond number must be made read only", true, guarantee.PW_BondNumberInfo.ReadOnly);

				AssertEquals("When selecting a guarantee header, the password must be defaulted", "PWD", guarantee.PW_Password);
				AssertEquals("When selecting a guarantee header, the password must be made read only", true, guarantee.PW_PasswordInfo.ReadOnly);

				AssertEquals("When selecting a guarantee header, the currency must be defaulted", "USD", guarantee.PW_RX_NKCurrency);
				AssertEquals("When selecting a guarantee header, the currency must be made read only", true, guarantee.PW_RX_NKCurrencyInfo.ReadOnly);
			});
		}

		public void TestPW_GuaranteeDescription_MaxLength()
		{
			AssertEquals(35, guarantee.PW_GuaranteeDescriptionInfo.MaxLength);
		}

		protected override Type GetGuaranteeForEntryInstructionValidationType()
		{
			return typeof(GuaranteeForEntryInstructionValidation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			guarantee = GetNewGuaranteeForEntryInstruction();
		}

		GuaranteeForEntryInstruction guarantee;
	}
}
