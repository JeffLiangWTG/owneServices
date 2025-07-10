using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(GroupInvoiceCharge))]
	public class GroupInvoiceChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestJ7_ChargeType_ApplyApplicableInsuranceRate() => CombineAssertions(() =>
		{
			TestDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var insurance = Factory.NewWithValidTestData<CusCalculationRule>();
			insurance.CCR_StartDate = new ZDateTime(2024, 05, 01).ToOffset();
			insurance.CCR_EndDate = new ZDateTime(2024, 05, 31).ToOffset();
			insurance.CCR_Formula = "IF(VFD >= 100, 1000, IF(VFD >= 10, 100, 10))";
			insurance.CCR_BasedOn = IncoTerms.FreeOnBoard;
			insurance.CCR_RX_NKCurrency = "AUD";
			insurance.CCR_TransportMode = ZString.Empty;
			Factory.Save();

			var invoice = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2024, 5, 7);
			invoice.JZ_RX_NKInvoice_Currency = "AUD";
			invoice.JZ_InvoiceAmount = 2100;
			invoice.JZ_IncoTerm = IncoTerms.CostAndFreight;
			var oftCharge = invoice.Charges[CustomsChargeTypeList.Codes.OverseasFreight];
			oftCharge.J7_Amount = 100m;
			AssertEquals("JZ_Calc_OFTInInvoiceCurrency", 100m, invoice.JZ_Calc_OFTInInvoiceCurrency);
			AssertEquals("OFT Charge currency", "AUD", oftCharge.J7_RX_NKCurrency);
			AssertEquals("EffectiveFOBAmount", 2000m, invoice.EffectiveFOBAmount);

			var onsCharge = GroupHeader.Charges[CustomsChargeTypeList.Codes.OverseasInsurance];
			AssertEquals("ValueForDuty", 2000m, ((IUniversalRateCalcData)GroupHeader).ValueForDuty);
			AssertEquals("Insurance amount", 1000m, onsCharge.J7_Amount);
			AssertEquals("J7_IsCalculated", true, onsCharge.J7_IsCalculated);
		});

		public void TestJ7_ChargeType_ApplyApplicableInsuranceRate_RateNotFound() => CombineAssertions(() =>
		{
			var invoice1 = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_ValuationDateOverride = new ZDateTime(2024, 5, 7);
			invoice1.JZ_RX_NKInvoice_Currency = "AUD";
			invoice1.JZ_InvoiceAmount = 2000;
			invoice1.JZ_IncoTerm = IncoTerms.CostAndFreight;
			AssertEquals("EffectiveFOBAmount", 2000m, invoice1.EffectiveFOBAmount);

			var onsCharge = invoice1.GroupHeader.Charges[CustomsChargeTypeList.Codes.OverseasInsurance];
			AssertEquals("Insurance amount", 0m, onsCharge.J7_Amount);
			AssertEquals("J7_IsCalculated", false, onsCharge.J7_IsCalculated);
		});

		public void TestJ7_ChargeType_ReadOnly() => CombineAssertions(() =>
		{
			var groupInvoiceCharge = Factory.New<GroupInvoiceCharge>();
			AssertEquals("Editable by default", false, groupInvoiceCharge.J7_ChargeTypeInfo.ReadOnly);
			groupInvoiceCharge.J7_IsCalculated = true;
			AssertEquals("Read-only when J7_IsCalculated = True", true, groupInvoiceCharge.J7_ChargeTypeInfo.ReadOnly);
			groupInvoiceCharge.J7_IsCalculated = false;
			AssertEquals("Editable when J7_IsCalculated = False", false, groupInvoiceCharge.J7_ChargeTypeInfo.ReadOnly);
		});

		public void TestJ7_IsCalculated()
		{
			var groupInvoiceCharge = Factory.New<GroupInvoiceCharge>();
			AssertEquals("J7_IsCalculated = False by default", false, groupInvoiceCharge.J7_IsCalculated);
			AssertEquals("Caption", "Calculated", DataBoundResourceStrings.GetDataForProperty(groupInvoiceCharge.J7_IsCalculatedInfo).Caption);
			AssertEquals("Read-only by default", true, groupInvoiceCharge.J7_IsCalculatedInfo.ReadOnly);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_ValuationDateOverride = new ZDateTime(2024, 5, 7);
			invoice1.JZ_RX_NKInvoice_Currency = "AUD";
			invoice1.JZ_InvoiceAmount = 2000;
			invoice1.JZ_IncoTerm = IncoTerms.FreeOnBoard;

			var oftCharge = groupHeader.Charges[CustomsChargeTypeList.Codes.OverseasFreight];
			var onsCharge = groupHeader.Charges[CustomsChargeTypeList.Codes.OverseasInsurance];
			AssertEquals("Editable when J7_ChargeType = ONS, with jobdeclaration is import", false, onsCharge.J7_IsCalculatedInfo.ReadOnly);
			AssertEquals("Read-only when J7_ChargeType <> ONS, with jobdeclaration is import", true, oftCharge.J7_IsCalculatedInfo.ReadOnly);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			oftCharge = groupHeader.Charges[CustomsChargeTypeList.Codes.OverseasFreight];
			onsCharge = groupHeader.Charges[CustomsChargeTypeList.Codes.OverseasInsurance];
			AssertEquals("Read-only when J7_ChargeType = ONS, with jobdeclaration is export", true, onsCharge.J7_IsCalculatedInfo.ReadOnly);
			AssertEquals("Read-only when J7_ChargeType <> ONS, with jobdeclaration is export", true, oftCharge.J7_IsCalculatedInfo.ReadOnly);
		}

		public void TestJ7_Amount_ReadOnly()
		{
			var groupInvoiceCharge = Factory.New<GroupInvoiceCharge>();
			AssertEquals("Editable by default", false, groupInvoiceCharge.J7_AmountInfo.ReadOnly);
			groupInvoiceCharge.J7_IsCalculated = true;
			AssertEquals("Read-only when J7_IsCalculated = True", true, groupInvoiceCharge.J7_AmountInfo.ReadOnly);
			groupInvoiceCharge.J7_IsCalculated = false;
			AssertEquals("Editable when J7_IsCalculated = False", false, groupInvoiceCharge.J7_AmountInfo.ReadOnly);
		}

		public void TestJ7_Percentage_ReadOnly()
		{
			var groupInvoiceCharge = Factory.New<GroupInvoiceCharge>();
			AssertEquals("Editable by default", false, groupInvoiceCharge.J7_PercentageInfo.ReadOnly);
			groupInvoiceCharge.J7_IsCalculated = true;
			AssertEquals("Read-only when J7_IsCalculated = True", true, groupInvoiceCharge.J7_PercentageInfo.ReadOnly);
			groupInvoiceCharge.J7_IsCalculated = false;
			AssertEquals("Editable when J7_IsCalculated = False", false, groupInvoiceCharge.J7_PercentageInfo.ReadOnly);
		}

		public void TestClearJ7_Percentage_WhenApplyApplicableInsuranceRate()
		{
			TestDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var insurance = Factory.NewWithValidTestData<CusCalculationRule>();
			insurance.CCR_StartDate = new ZDateTime(2024, 05, 01).ToOffset();
			insurance.CCR_EndDate = new ZDateTime(2024, 05, 31).ToOffset();
			insurance.CCR_Formula = "IF(VFD >= 100, 1000, IF(VFD >= 5, 0.06*VFD, 5))";
			insurance.CCR_BasedOn = IncoTerms.FreeOnBoard;
			insurance.CCR_RX_NKCurrency = "AUD";
			insurance.CCR_TransportMode = ZString.Empty;
			Factory.Save();

			var invoice = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2024, 5, 7);
			invoice.JZ_RX_NKInvoice_Currency = "AUD";
			invoice.JZ_InvoiceAmount = 2100;
			invoice.JZ_IncoTerm = IncoTerms.CostAndFreight;
			var oftCharge = invoice.Charges[CustomsChargeTypeList.Codes.OverseasFreight];
			oftCharge.J7_Amount = 100m;
			AssertEquals("JZ_Calc_OFTInInvoiceCurrency", 100m, invoice.JZ_Calc_OFTInInvoiceCurrency);
			AssertEquals("OFT Charge currency", "AUD", oftCharge.J7_RX_NKCurrency);
			AssertEquals("EffectiveFOBAmount", 2000m, invoice.EffectiveFOBAmount);

			var onsCharge = GroupHeader.Charges[CustomsChargeTypeList.Codes.OverseasInsurance];
			onsCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			onsCharge.J7_Percentage = 0.18;
			onsCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			AssertEquals("ValueForDuty", 2000m, ((IUniversalRateCalcData)GroupHeader).ValueForDuty);
			AssertEquals("Insurance amount", 1000m, onsCharge.J7_Amount);
			AssertEquals("J7_IsCalculated", true, onsCharge.J7_IsCalculated);
			AssertEquals("J7_Percentage", 0m, onsCharge.J7_Percentage);
		}

		public void TestJ7_RX_NKCurrency_ReadOnly()
		{
			var groupInvoiceCharge = Factory.New<GroupInvoiceCharge>();
			AssertEquals("Editable by default", false, groupInvoiceCharge.J7_RX_NKCurrencyInfo.ReadOnly);
			groupInvoiceCharge.J7_IsCalculated = true;
			AssertEquals("Read-only when J7_IsCalculated = True", true, groupInvoiceCharge.J7_RX_NKCurrencyInfo.ReadOnly);
			groupInvoiceCharge.J7_IsCalculated = false;
			AssertEquals("Editable when J7_IsCalculated = False", false, groupInvoiceCharge.J7_RX_NKCurrencyInfo.ReadOnly);
		}

		public void TestApportionChargeIfCurrencyIsThere()
		{
			TestDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			TestDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;

			JobComInvoiceHeader invoice = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceAmount = 1000;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.PackedAtFactory;
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 1000m;

			AssertEquals("There are FIFT charge defaulted", true, invoice.GroupHeader.Charges[CustomsChargeTypeList.Codes.ForeignInlandFreight] != null);

			BaseJobComInvHeaderCharge fIFT = invoice.GroupHeader.Charges[CustomsChargeTypeList.Codes.ForeignInlandFreight];
			fIFT.J7_RX_NKCurrency = JobDeclaration.LocalCurrencyConstantCode;
			AssertEquals("JI_ForeignInlandFreight Amount", 0m, line.JI_ForeignInlandFreight.Amount);
			AssertEquals("JI_ForeignInlandFreight Currency", JobDeclaration.LocalCurrencyConstantCode, line.JI_ForeignInlandFreight.Currency.Code);
		}

		#region Implementation

		JobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = Factory.New<JobDeclaration>();
				}
				return fTestDec;
			}
		}
		JobDeclaration fTestDec;

		JobComInvoiceGroupHeader GroupHeader
		{
			get
			{
				if (fGroupHeader == null)
				{
					fGroupHeader = TestDec.JobComInvoiceGroupHeaders[0];
				}
				return fGroupHeader;
			}
		}

		JobComInvoiceGroupHeader fGroupHeader;

		protected override BusinessObject GetNewBusinessObject()
		{
			return GroupHeader.Charges.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GroupHeader.Charges.AddNew();
		}

		#endregion
	}
}
