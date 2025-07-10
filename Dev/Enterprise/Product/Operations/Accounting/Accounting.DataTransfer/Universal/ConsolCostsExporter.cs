using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	class ConsolCostsExporter
	{
		public static ConsolCosts Generate(ApportionmentListing apportionmentListing, IDataObjectWriterStrategy writerStrategy)
		{
			ConsolCosts consolCosts = null;
			apportionmentListing.Factory.SetContext(DataTransferContext.UniversalExport);
			try
			{
				if (apportionmentListing != null && apportionmentListing.CostsCollection.Count > 0)
				{
					consolCosts = new ConsolCosts(writerStrategy);
					consolCosts.SetConsolCostLineCollection(() =>
					{
						var consolCostLineCollection = new List<ConsolCostLine>();

						var placeOfSUpplyHelper = new UniversalPlaceOfSupplyHelper(apportionmentListing.CostsCollection[0].Company);

						foreach (JobConsolCost jobConsolCost in apportionmentListing.CostsCollection)
						{
							var consolCostLine = GenerateConsolCostLine(jobConsolCost, placeOfSUpplyHelper, writerStrategy);
							consolCostLineCollection.Add(consolCostLine);
						}

						return consolCostLineCollection;
					});
				}
			}
			finally
			{
				apportionmentListing.Factory.RemoveContext(DataTransferContext.UniversalExport);
			}

			return consolCosts;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		static ConsolCostLine GenerateConsolCostLine(JobConsolCost jobConsolCost, UniversalPlaceOfSupplyHelper placeOfSupplyHelper, IDataObjectWriterStrategy writerStrategy)
		{
			var consolCostLine = new ConsolCostLine(writerStrategy);

			if (!jobConsolCost.E6_InvoiceNum.IsEmpty)
			{
				consolCostLine.CostAPInvoiceNumber = jobConsolCost.E6_InvoiceNum;
			}

			if (!jobConsolCost.E6_InvoiceDate.IsEmpty)
			{
				consolCostLine.CostInvoiceDate = jobConsolCost.E6_InvoiceDate;
			}

			if (!jobConsolCost.E6_PaymentDate.IsEmpty)
			{
				consolCostLine.CostDueDate = jobConsolCost.E6_PaymentDate;
			}

			if (!jobConsolCost.E6_CostReference.IsEmpty)
			{
				consolCostLine.SupplierReference = jobConsolCost.E6_CostReference;
			}

			if (!jobConsolCost.E6_RatingBehaviour.IsEmpty)
			{
				var jobConsolCostLookups = new JobConsolCostLookups(jobConsolCost);
				consolCostLine.RatingBehaviour = new RatingBehaviour()
				{
					Code = jobConsolCost.E6_RatingBehaviour,
					Description = jobConsolCostLookups.RatingBehaviourList.GetDescriptionFromCode(jobConsolCost.E6_RatingBehaviour)
				};
			}

			if (jobConsolCost.ChargeCode != null)
			{
				consolCostLine.ChargeCode = new ChargeCode();
				consolCostLine.ChargeCode.Code = jobConsolCost.ChargeCode.AC_Code;
				consolCostLine.ChargeCode.Description = jobConsolCost.ChargeCode.AC_Desc;

				ZString code = jobConsolCost.ChargeCode.AC_ChargeGroup;
				if (!code.IsEmpty)
				{
					ChargeCodeGroupList chargeGroupList = new ChargeCodeGroupList();
					ZString description = chargeGroupList.GetDescriptionFromCode(code);

					consolCostLine.ChargeCodeGroup = new UniversalCodeDescriptionPair() { Code = jobConsolCost.ChargeCode.AC_ChargeGroup, Description = description };
				}
			}

			if (jobConsolCost.Currency != null)
			{
				Currency consolCostLineOSCostCurrency = new Currency();
				consolCostLineOSCostCurrency.Code = jobConsolCost.Currency.RX_Code;
				consolCostLineOSCostCurrency.Description = jobConsolCost.Currency.RX_DescMultilingual.GetUnresolvedString();
				consolCostLine.CostOSCurrency = consolCostLineOSCostCurrency;
			}

			consolCostLine.CostIsPosted = jobConsolCost.IsPosted;
			consolCostLine.CostLocalAmount = jobConsolCost.E6_LocalCostAmount;
			consolCostLine.CostOSAmount = jobConsolCost.E6_OSCostAmount;
			consolCostLine.CostOSGSTVATAmount = jobConsolCost.E6_OSGSTAmount_Calc;
			if (jobConsolCost.TaxRate != null)
			{
				consolCostLine.CostGSTVATID = new TaxID();
				consolCostLine.CostGSTVATID.TaxCode = jobConsolCost.TaxRate.AT_Code;
				consolCostLine.CostGSTVATID.Description = jobConsolCost.TaxRate.AT_Description;
			}

			consolCostLine.CostExchangeRate = jobConsolCost.E6_ExchangeRate;

			consolCostLine.PrepaidCollectFilter = jobConsolCost.E6_PPDCLT;
			consolCostLine.ApportionmentMethod = jobConsolCost.E6_ApportionmentMethod;
			consolCostLine.ApportionToSubShipments = jobConsolCost.E6_ApportionToRelatedShipments;
			consolCostLine.IncludeOnCollectInvoice = jobConsolCost.E6_IsForCollectInvoice;

			if (jobConsolCost.Creditor != null)
			{
				consolCostLine.Creditor = new OrganizationReference();
				consolCostLine.Creditor.Key = jobConsolCost.Creditor.OH_Code;
				consolCostLine.Creditor.Type = nameof(DataContextType.Organization);
				if (!jobConsolCost.Creditor.CompanyData.OB_APExternalCreditorCode.IsEmpty)
				{
					consolCostLine.ExternalCreditorCode = jobConsolCost.Creditor.CompanyData.OB_APExternalCreditorCode;
				}
			}

			if (jobConsolCost.ConsolCostOwner != null )
			{
				consolCostLine.CostOwner = new Staff();
				consolCostLine.CostOwner.Code = jobConsolCost.ConsolCostOwner.GS_Code;
				consolCostLine.CostOwner.Name = jobConsolCost.ConsolCostOwner.GS_FullName;
			}

			if (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value)
			{
				if (!jobConsolCost.E6_CostGovtChargeCode.IsEmpty)
				{
					consolCostLine.GovernmentReportingCostChargeCode = jobConsolCost.E6_CostGovtChargeCode;
				}

				if (!jobConsolCost.E6_SellGovtChargeCode.IsEmpty)
				{
					consolCostLine.GovernmentReportingSellChargeCode = jobConsolCost.E6_SellGovtChargeCode;
				}
			}

			if (jobConsolCost is IPaymentBasisViewCharge updateableCost)
			{
				consolCostLine.SetCostRatingBasisCollection(() => Helpers.PopulateUniversalPaymentBases(updateableCost.CostPaymentBasesView));
			}

			var placeOfSupply = placeOfSupplyHelper.GetPlaceOfSupply(jobConsolCost.E6_PlaceOfSupply, jobConsolCost.E6_PlaceOfSupplyType);
			if (placeOfSupply != null)
			{
				consolCostLine.PlaceOfSupply = placeOfSupply;
			}

			if (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
			{
				CargoWise.Integration.ICodeDescription supplyType = (Enterprise.Registry.Business.CodeDescriptionBool)AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.Value.FindByCode(jobConsolCost.E6_SupplyType);
				consolCostLine.SupplyType = new UniversalCodeDescriptionPair()
				{
					Code = supplyType?.Code ?? jobConsolCost.E6_SupplyType,
					Description = supplyType?.Description ?? ZString.Empty
				};
			}

			var costTaxBranch = jobConsolCost.CostTaxBranch;

			if (costTaxBranch != null)
			{
				consolCostLine.CostTaxBranch = new Branch()
				{
					Code = costTaxBranch.GB_Code,
					Name = costTaxBranch.GB_BranchName
				};
			}

			return consolCostLine;
		}
	}
}
