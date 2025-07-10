using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;
using static Enterprise.Customs.GB.Business.GBUniversalReferenceConstants;

namespace Enterprise.Customs.GB.Chief.Declaration
{
	class ChiefPreferenceToA00Creator
	{
		public ChiefPreferenceToA00Creator(JobComInvoiceLine invoiceLine, IZType newPreference)
		{
			this.invoiceLine = invoiceLine;
			this.newPreference = newPreference.ToString();
		}
		readonly JobComInvoiceLine invoiceLine;
		readonly string newPreference;

		public void CreateOrUpdateA00()
		{
			switch (newPreference)
			{
				case string s when s.StartsWith("1"):
					_ = Preferences.PreferenceCode_100_NormalTariffDuty;
					HandlePreference100();
					break;
				case string s when s.StartsWith("2"):
					_ = Preferences.PreferenceCode_200_GSPRateWithoutConditionsOrLimitsIncludingCeilings;
					HandlePreference(EU.Business.TaxRateCustomsDutyListImport.Codes.DutyAPreferentialRateIsBeingClaimedUnderTheGSPScheme); //G
					break;
				case string s when s.StartsWith("3"):
					_ = Preferences.PreferenceCode_300_TariffPreferenceWithoutConditionsOrLimitsIncludingCeilings;
					HandlePreference(EU.Business.TaxRateCustomsDutyListImport.Codes.DutyAPreferentialRateIsBeingClaimedUnderAnotherECPreferenceAgreement); //A
					break;
				case Preferences.PreferenceCode_400_NonImpositionOfCustomsDutiesUnderTheProvisionsOfCustomsUnionAgreementsConcludedByTheCommunity:
					HandlePreference400();
					break;
				case Preferences.PreferenceCode_420:
					HandlePreference(EU.Business.TaxRateCustomsDutyListImport.Codes.DutyAPreferentialRateOfDutyIsBeingClaimedUnderAnotherPreferenceAgreementBetweenTheECAndTurkeyOnGoodsWhichAreCoveredByFormsEUR1OrEURMEDOrByAPreferentialOriginDeclarationOnAnInvoiceOrOtherCommercialDocument); //AT
					break;
			}
		}

		void HandlePreference(string dutyRate)
		{
			var a00 = FindA00();
			if (a00 == null)
			{
				a00 = invoiceLine.Taxes.AddNew();
				a00.JLT_Type = RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			}
			a00.JLT_MethodOfCalculation = dutyRate;
		}

		EU.Business.Declaration.JobComInvoiceLineTax FindA00()
		{
			return invoiceLine.Taxes.OfType<EU.Business.Declaration.JobComInvoiceLineTax>().FirstOrDefault(t => t.JLT_Type == RefCusRateCodes.CustomsDutyOnIndustrialProducts);
		}

		void HandlePreference100()
		{
			var rate = invoiceLine.UniversalDutyRate;
			if (rate != null)
			{
				if (rate.ZZ2_RateFormula == "0" || rate.ZZ2_RateFormula == "VFD*0")
				{
					var a00 = FindA00();
					if (a00 != null)
					{
						a00.Delete();
					}
				}
				else
				{
					// F
					HandlePreference(EU.Business.TaxRateCustomsDutyListImport.Codes.DutyTheGoodsAreLiableToDutyAtTheFullRateThisIncludesGoodsBeingEnteredForATariffQuotaReliefToWhichNoneOfTheCodesBelowApply);
				}
			}
		}

		void HandlePreference400()
		{
			var origin = invoiceLine.JI_CountryOfOrigin;
			if (origin == Core.Constants.CountryCodes.Turkey)
			{
				// UT
				HandlePreference(EU.Business.TaxRateCustomsDutyListImport.Codes.DutyNonImpositionOfCustomsDutiesUnderTheProvisionsOfTheCustomsUnionAgreementConcludedByTheCommunityAndTurkeyForMostProductsExcludingCoalAndSteelInChapters2597OfTheTariffWhichAreCoveredByFormsATR);
			}
			else if (origin == Core.Constants.CountryCodes.Andorra)
			{   // U
				HandlePreference(EU.Business.TaxRateCustomsDutyListImport.Codes.DutyNonImpositionOfCustomsDutiesUnderTheProvisionsOfCustomsUnionAgreementsConcludedByTheCommunityForAndorraForAllProductsInChapters2597OfTheTariffAndForSanMarinoForAllProductsExcludingCoalAndSteelInChapters197OfTheTariff);
			}
		}
	}
}
