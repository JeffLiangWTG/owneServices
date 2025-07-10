using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportChargesProviderTest : TestCase
	{
		public void TestGetChargeConfiguration()
		{
			var chargeConfiguration = ImportChargesProvider.GetChargeConfiguration();
			CombineAssertions(() =>
			{
				Assert("IsIncludedInInvoice must be False", !chargeConfiguration.IsIncludedInInvoice);
				Assert("IsIncludedInInvoiceAmountFixed must be True", chargeConfiguration.IsIncludedInInvoiceAmountFixed);
				Assert("IsMandatory must be False", !chargeConfiguration.IsMandatory);
				Assert("IsRecommended must be False", !chargeConfiguration.IsRecommended);
			});
		}

		public void TestCustomsChargeCodeFreightInNationalTerritory()
		{
			var freightInNationalTerritory = ImportChargesProvider.FreightInNationalTerritory;
			CombineAssertions(() =>
			{
				Assert("IsDutiable must be False", !freightInNationalTerritory.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge must be True", freightInNationalTerritory.IsDutiableDeemedForThisCharge);
				Assert("IsVATible must be False", !freightInNationalTerritory.IsVATible);
				Assert("IsVATibleDeemedForThisCharge must be True", freightInNationalTerritory.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable must be False", !freightInNationalTerritory.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed must be False", !freightInNationalTerritory.IsStatisticalValueApplicableDeemed);
				AssertEquals("IsIncludedInITOTIfDeemed must be False", false, freightInNationalTerritory.IsIncludedInITOTIfDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge must be True", freightInNationalTerritory.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("DistributeBy must be ChargeDistributeByList.Codes.NetWeight", ChargeDistributeByList.Codes.NetWeight, freightInNationalTerritory.DistributeBy);
				Assert("IsIncoTermNeutral must be False", !freightInNationalTerritory.IsIncoTermNeutral);
			});
		}

		public void TestCustomsChargeCodeOverseasInsurance()
		{
			var overseasInsurance = ImportChargesProvider.OverseasInsurance;
			CombineAssertions(() =>
			{
				Assert("IsDutiable must be False", !overseasInsurance.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge must be True", overseasInsurance.IsDutiableDeemedForThisCharge);
				Assert("IsVATible must be True", overseasInsurance.IsVATible);
				Assert("IsVATibleDeemedForThisCharge must be True", overseasInsurance.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable must be False", !overseasInsurance.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed must be False", !overseasInsurance.IsStatisticalValueApplicableDeemed);
				AssertEquals("IsIncludedInITOTIfDeemed must be False", false, overseasInsurance.IsIncludedInITOTIfDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge must be false", !overseasInsurance.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("DistributeBy must be ChargeDistributeByList.Codes.FOB", ChargeDistributeByList.Codes.FOB, overseasInsurance.DistributeBy);
				Assert("IsIncoTermNeutral must be False", !overseasInsurance.IsIncoTermNeutral);
			});
		}

		public void TestDeductionsAndAdditionsCustomsChargesProperties()
		{
			CombineAssertions(() =>
			{
				foreach (var chargeCode in ImportChargesProvider.Codes)
				{
					if (ImportChargesProvider.IsAdditionsOrDeductions(chargeCode.Code))
					{
						Assert($"Charge {chargeCode.Code} IsDutiable must be True", chargeCode.IsDutiable);
						Assert($"Charge {chargeCode.Code} IsDutiableDeemedForThisCharge must be True", chargeCode.IsDutiableDeemedForThisCharge);
						Assert($"Charge {chargeCode.Code} IsVATible must be True", chargeCode.IsVATible);
						Assert($"Charge {chargeCode.Code} IsVATibleDeemedForThisCharge must be True", chargeCode.IsVATibleDeemedForThisCharge);
						AssertEquals($"Charge {chargeCode.Code} IsIncludedInITOTIfDeemed must be False", false, chargeCode.IsIncludedInITOTIfDeemed);
						Assert($"Charge {chargeCode.Code} IsIncludedInITOTDeemedForThisCharge must be True", chargeCode.IsIncludedInITOTDeemedForThisCharge);
					}
				}
			});
		}

		public void TestIsDeductions()
		{
			foreach (var chargeCode in ImportChargesProvider.Codes)
			{
				if (ImportChargesProvider.IsDeductions(chargeCode.Code))
				{
					Assert($"Description of charge {chargeCode.Code} should end with (Deductions)", chargeCode.Description.ToString().EndsWith("(Deductions)"));
					AssertEquals($"DistributeBy of charge {chargeCode.Code} should", ChargeDistributeByList.Codes.FOB, chargeCode.DistributeBy);
				}
				else if (chargeCode.Description.ToString().EndsWith("(Deductions)"))
				{
					Assert($"IsDeductions() for {chargeCode.Code} should be true", ImportChargesProvider.IsDeductions(chargeCode.Code));
				}
			}
		}

		public void TestIsAdditions()
		{
			foreach (var chargeCode in ImportChargesProvider.Codes)
			{
				if (ImportChargesProvider.IsAdditions(chargeCode.Code))
				{
					Assert($"Description of charge {chargeCode.Code} should end with (Additions)", chargeCode.Description.ToString().EndsWith("(Additions)"));
					AssertEquals($"DistributeBy of charge {chargeCode.Code} should", ChargeDistributeByList.Codes.FOB, chargeCode.DistributeBy);
				}
				else if (chargeCode.Description.ToString().EndsWith("(Additions)"))
				{
					Assert($"IsAdditions() for {chargeCode.Code} should be true", ImportChargesProvider.IsAdditions(chargeCode.Code));
				}
			}
		}

		public void TestFreightComponentsProperties()
		{
			var freightComponents = ImportChargesProvider.FreightComponents;
			CombineAssertions(() =>
			{
				Assert("IsDutiable must be False", !freightComponents.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge must be True", freightComponents.IsDutiableDeemedForThisCharge);
				Assert("IsVATible must be False", !freightComponents.IsVATible);
				Assert("IsVATibleDeemedForThisCharge must be True", freightComponents.IsVATibleDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed must be False", false, freightComponents.IsIncludedInITOTIfDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge must be True", freightComponents.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("DistributeBy must be ChargeDistributeByList.Codes.NetWeight", ChargeDistributeByList.Codes.NetWeight, freightComponents.DistributeBy);
				Assert("IsPercentageApplicable must be False", !freightComponents.IsPercentageApplicable);
			});
		}

		public void TestOtherExpensesICMSProperties()
		{
			var otherExpensesICMS = ImportChargesProvider.OtherExpensesICMS;
			CombineAssertions(() =>
			{
				Assert("IsDutiable must be False", !otherExpensesICMS.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge must be True", otherExpensesICMS.IsDutiableDeemedForThisCharge);
				Assert("IsVATible must be False", !otherExpensesICMS.IsVATible);
				Assert("IsVATibleDeemedForThisCharge must be True", otherExpensesICMS.IsVATibleDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed must be False", false, otherExpensesICMS.IsIncludedInITOTIfDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge must be True", otherExpensesICMS.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("DistributeBy must be Value", Common.ChargeDistributeByList.Codes.Value, otherExpensesICMS.DistributeBy);
				Assert("IsPercentageApplicable must be False", !otherExpensesICMS.IsPercentageApplicable);
			});
		}
	}
}
