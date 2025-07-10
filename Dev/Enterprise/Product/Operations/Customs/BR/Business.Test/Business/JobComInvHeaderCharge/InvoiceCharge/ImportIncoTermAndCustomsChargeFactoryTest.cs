using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportIncoTermAndCustomsChargeFactoryTest : IncoTermAndCustomsChargeFactoryTest
	{
		public override void TestGetAllCharges()
		{
			CombineAssertions(() =>
			{
				AssertEquals("There should be 37 charges", 37, incoTermAndChargeFactory.GetAllCharges().Length);

				AssertNameLater(BRIncoTermList.Codes.DAP, "ONS");
				AssertNameLater(BRIncoTermList.Codes.DAP, "OFC");
				AssertNameLater(BRIncoTermList.Codes.DAP, "OFP");

				AssertNameLater(BRIncoTermList.Codes.DAT, "ONS");
				AssertNameLater(BRIncoTermList.Codes.DAT, "OFC");
				AssertNameLater(BRIncoTermList.Codes.DAT, "OFP");

				AssertNameLater(BRIncoTermList.Codes.EXW, "ONS");
				AssertNameLater(BRIncoTermList.Codes.EXW, "OFC");
				AssertNameLater(BRIncoTermList.Codes.EXW, "OFP");

				AssertNameLater(BRIncoTermList.Codes.DDP, "ONS");
				AssertNameLater(BRIncoTermList.Codes.DDP, "OFC");
				AssertNameLater(BRIncoTermList.Codes.DDP, "OFP");

				AssertNameLater(BRIncoTermList.Codes.CIP, "ONS");
				AssertNameLater(BRIncoTermList.Codes.CIP, "OFC");
				AssertNameLater(BRIncoTermList.Codes.CIP, "OFP");

				AssertNameLater(BRIncoTermList.Codes.CPT, "ONS");
				AssertNameLater(BRIncoTermList.Codes.CPT, "OFC");
				AssertNameLater(BRIncoTermList.Codes.CPT, "OFP");

				AssertNameLater(BRIncoTermList.Codes.CIF, "ONS");
				AssertNameLater(BRIncoTermList.Codes.CIF, "OFC");
				AssertNameLater(BRIncoTermList.Codes.CIF, "OFP");

				AssertNameLater(BRIncoTermList.Codes.CFR, "ONS");
				AssertNameLater(BRIncoTermList.Codes.CFR, "OFC");
				AssertNameLater(BRIncoTermList.Codes.CFR, "OFP");

				AssertNameLater(BRIncoTermList.Codes.FAS, "ONS");
				AssertNameLater(BRIncoTermList.Codes.FAS, "OFC");
				AssertNameLater(BRIncoTermList.Codes.FAS, "OFP");

				AssertNameLater(BRIncoTermList.Codes.FOB, "ONS");
				AssertNameLater(BRIncoTermList.Codes.FOB, "OFC");
				AssertNameLater(BRIncoTermList.Codes.FOB, "OFP");

				AssertNameLater(BRIncoTermList.Codes.FCA, "ONS");
				AssertNameLater(BRIncoTermList.Codes.FCA, "OFC");
				AssertNameLater(BRIncoTermList.Codes.FCA, "OFP");

				AssertNameLater(BRIncoTermList.Codes.CPLUSI, "ONS");
				AssertNameLater(BRIncoTermList.Codes.CPLUSI, "OFC");
				AssertNameLater(BRIncoTermList.Codes.CPLUSI, "OFP");

				AssertNameLater(BRIncoTermList.Codes.CPLUSF, "ONS");
				AssertNameLater(BRIncoTermList.Codes.CPLUSF, "OFC");
				AssertNameLater(BRIncoTermList.Codes.CPLUSF, "OFP");

				AssertNameLater(BRIncoTermList.Codes.OCV, "ONS");
				AssertNameLater(BRIncoTermList.Codes.OCV, "OFC");
				AssertNameLater(BRIncoTermList.Codes.OCV, "OFP");

				AssertNameLater(BRIncoTermList.Codes.DPU, "ONS");
				AssertNameLater(BRIncoTermList.Codes.DPU, "OFC");
				AssertNameLater(BRIncoTermList.Codes.DPU, "OFP");
			});
		}

		public override void TestIsThisChargeDiscount()
		{
			foreach (var chargeCode in ImportChargesProvider.Codes)
			{
				AssertEquals($"{chargeCode}", ImportChargesProvider.IsDeductions(chargeCode.Code), incoTermAndChargeFactory.IsThisChargeDiscount(chargeCode.Code));
			}
		}

		public void TestGetChargeTypeList()
		{
			var commonChargesCodesList = ImportCommonChargesProvider.CommonChargesList.Select(c => c.Code).ToList();

			var chargeCodes = new List<string>() { "OFC", "OFP", "ONS" };
			chargeCodes.AddRange(ImportChargesProvider.Codes.Cast<ICustomsChargeCode>().Select(x => x.Code));

			var expectedChargeTypeListInOrderGroupInvoice = chargeCodes.Where(x => !ImportChargesProvider.IsDeductions(x) && !ImportChargesProvider.IsAdditions(x)).OrderBy(x => x)
				.Union(chargeCodes.Where(x => ImportChargesProvider.IsAdditions(x)).OrderBy(x => x))
				.Union(chargeCodes.Where(x => ImportChargesProvider.IsDeductions(x)).OrderBy(x => x));

			var expectedChargeTypeListInOrderInvoiceAndInvoiceLine = expectedChargeTypeListInOrderGroupInvoice.ToList();
			expectedChargeTypeListInOrderInvoiceAndInvoiceLine.AddRange(commonChargesCodesList);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInExactOrder("ChargeTypeList should be when ChargeParentTypes.GroupInvoice", expectedChargeTypeListInOrderGroupInvoice, incoTermAndChargeFactory.GetChargeTypeList(ChargeParentTypes.GroupInvoice).GetAllCodes());
				AssertContainsExactElementsInAnyOrder("ChargeTypeList should be when ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine", expectedChargeTypeListInOrderInvoiceAndInvoiceLine, incoTermAndChargeFactory.GetChargeTypeList(ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine).GetAllCodes());
			});
		}

		public override void TestGetCharge()
		{
			CombineAssertions(() =>
			{
				AssertCharge(ImportCustomsChargeTypeList.Codes.OtherAdditionsCustomsValue, ImportChargesProvider.OtherAdditionsCustomsValue);
				AssertCharge(ImportCustomsChargeTypeList.Codes.ConstructionInstallationAssembly, ImportChargesProvider.ConstructionInstallationAssembly);
				AssertCharge(ImportCustomsChargeTypeList.Codes.CommissionsBrokerage, ImportChargesProvider.CommissionsBrokerage);
				AssertCharge(ImportCustomsChargeTypeList.Codes.OtherDeductionsCustomsValue, ImportChargesProvider.OtherDeductionsCustomsValue);
				AssertCharge(ImportCustomsChargeTypeList.Codes.EngineeringProjects, ImportChargesProvider.EngineeringProjects);
				AssertCharge(ImportCustomsChargeTypeList.Codes.FinancingInterest, ImportChargesProvider.FinancingInterest);
				AssertCharge(ImportCustomsChargeTypeList.Codes.InternalFreightExportingCountry, ImportChargesProvider.InternalFreightExportingCountry);
				AssertCharge(ImportCustomsChargeTypeList.Codes.InternalFreightImportingCountry, ImportChargesProvider.InternalFreightImportingCountry);
				AssertCharge(ImportCustomsChargeTypeList.Codes.InternationalLoadingUnloadingHandling, ImportChargesProvider.InternationalLoadingUnloadingHandling);
				AssertCharge(ImportCustomsChargeTypeList.Codes.ValueInstallment, ImportChargesProvider.ValueInstallment);
				AssertCharge(ImportCustomsChargeTypeList.Codes.InternalInsuranceExportingCountry, ImportChargesProvider.InternalInsuranceExportingCountry);
				AssertCharge(ImportCustomsChargeTypeList.Codes.InternalInsuranceImportingCountry, ImportChargesProvider.InternalInsuranceImportingCountry);
				AssertCharge(ImportCustomsChargeTypeList.Codes.LoadingUnloadingHandlingEntranceImportingCountry, ImportChargesProvider.LoadingUnloadingHandlingEntranceImportingCountry);
				AssertCharge(ImportCustomsChargeTypeList.Codes.LoadingUnloadingHandlingExportingCountry, ImportChargesProvider.LoadingUnloadingHandlingExportingCountry);
				AssertCharge(ImportCustomsChargeTypeList.Codes.LoadingUnloadingHandlingImportingCountry, ImportChargesProvider.LoadingUnloadingHandlingImportingCountry);
				AssertCharge(ImportCustomsChargeTypeList.Codes.MaterialsComponents, ImportChargesProvider.MaterialsComponents);
				AssertCharge(ImportCustomsChargeTypeList.Codes.MaterialsConsumedProduction, ImportChargesProvider.MaterialsConsumedProduction);
				AssertCharge(ImportCustomsChargeTypeList.Codes.PackingCosts, ImportChargesProvider.PackingCosts);
				AssertCharge(ImportCustomsChargeTypeList.Codes.PackagingReceptacles, ImportChargesProvider.PackagingReceptacles);
				AssertCharge(ImportCustomsChargeTypeList.Codes.RightsOtherTaxes, ImportChargesProvider.RightsOtherTaxes);
				AssertCharge(ImportCustomsChargeTypeList.Codes.RoyaltiesLicenseRights, ImportChargesProvider.RoyaltiesLicenseRights);
				AssertCharge(ImportCustomsChargeTypeList.Codes.ToolsMatricesMolds, ImportChargesProvider.ToolsMatricesMolds);
				AssertCharge(ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory, ImportChargesProvider.FreightInNationalTerritory);
				AssertCharge(ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory, ImportChargesProvider.FreightComponents);
			});
		}

		public void TestDistributeBy()
		{
			var commonCharges = ImportCommonChargesProvider.CommonChargesList;
			var commonChargesCodesList = commonCharges.Select(c => c.Code).ToList();
			var charges = incoTermAndChargeFactory.GetAllCharges().ToList();
			charges.AddRange(commonCharges);

			CombineAssertions(() =>
			{
				foreach (var chargeProvider in charges)
				{
					if (ImportChargesProvider.IsDeductions(chargeProvider.Code) || ImportChargesProvider.IsAdditions(chargeProvider.Code) || chargeProvider.Code == ImportChargesProvider.OverseasInsurance.Code)
					{
						AssertEquals($"Charge Code {chargeProvider.Code} should be", ChargeDistributeByList.Codes.FOB, chargeProvider.DistributeBy);
					}
					else if (chargeProvider.Code == ImportChargesProvider.OtherExpensesICMS.Code)
					{
						AssertEquals($"Charge Code {chargeProvider.Code} should be", Common.ChargeDistributeByList.Codes.Value, chargeProvider.DistributeBy);
					}
					else if (commonChargesCodesList.Contains(chargeProvider.Code))
					{
						AssertNull($"Charge Code {chargeProvider.Code} should be NULL", chargeProvider.DistributeBy);
					}
					else
					{
						AssertEquals($"Charge Code {chargeProvider.Code} should be", ChargeDistributeByList.Codes.NetWeight, chargeProvider.DistributeBy);
					}
				}
			});
		}

		void AssertNameLater(string incoTerm, string expectedValue)
		{
			var configurations = (SortedDictionary<ZString, SortedDictionary<ZString, ChargeConfiguration>>)incoTermAndChargeFactory.GetIncotermChargeRelationshipConfigurations();
			AssertEquals($@"Configuration for Incoterm {incoTerm} should contain {expectedValue}", true, configurations[incoTerm].ContainsKey(expectedValue));
		}

		void AssertCharge(string chargeType, CustomsChargeCode expectedChargeCode)
		{
			var actualChargeCode = incoTermAndChargeFactory.GetCharge(chargeType);
			AssertEquals(expectedChargeCode.GetType(), actualChargeCode.GetType());
		}

		protected override string GetCountryContext() => Core.Constants.CountryCodes.Brazil + BRJobMessageTypeList.Codes.Import;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\BR\Business.Test\Business\JobComInvHeaderCharge\InvoiceCharge\TestFile\ImportIncoTermAndCustomsChargeConfiguration.csv";
	}
}
