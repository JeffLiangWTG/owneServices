using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;

namespace Enterprise.Customs.CA.Business
{
	class DefaultSetterForInvoiceHeader : Customs.Business.DefaultSetterForInvoiceHeader
	{
		public DefaultSetterForInvoiceHeader(BaseJobComInvoiceHeader child, BaseJobDeclaration declaration)
			: base(child, declaration)
		{
		}

		JobDeclaration Declaration
		{
			get { return (JobDeclaration)declaration; }
		}

		JobComInvoiceHeader NewElement
		{
			get { return (JobComInvoiceHeader)newElement; }
		}

		protected override void DefaultForNewElementCore()
		{
			if (Declaration != null)
			{
				if (!Declaration.IsSimplifiedLVSMode)
				{
					base.DefaultForNewElementCore();

					if (Declaration.SupplierImporterLink != null && Declaration.SupplierImporterLink.OL_ValuationBasis.IsValid)
					{
						NewElement.CA_ValueForDutyCode = Declaration.SupplierImporterLink.OL_ValuationBasis.Left(2);
					}
					if (Declaration.IsImport)
					{
						NewElement.SupplierDocumentaryAddress.E2_AddressOverride = Declaration.SupplierDocumentaryAddress.E2_AddressOverride;
						NewElement.SupplierDocumentaryAddress.CopyPersistentValuesFrom(Declaration.SupplierDocumentaryAddress);

						NewElement.SupplierPickupDeliveryAddress.E2_AddressOverride = Declaration.SupplierPickupAddress.E2_AddressOverride;
						NewElement.SupplierPickupDeliveryAddress.CopyPersistentValuesFrom(Declaration.SupplierPickupAddress);

						var importer = Declaration.Importer;
						if (importer != null)
						{
							NewElement.FinalConsigneeAddress.OrganisationPK = importer.PK;
						}
					}
				}
				else
				{
					NewElement.CA_TreatmentCode = GetRegistryDefaultTariffTreatment();
				}
			}
		}

		protected override void SetDefaultsForFirstInvoiceCore()
		{
			base.SetDefaultsForFirstInvoiceCore();
			if (Declaration.IsImport)
			{
				var importLeg = Declaration.MostInterestingLegProvider.GetInboundLeg();
				if (importLeg != null)
				{
					NewElement.CA_RL_NKLastPort = importLeg.Load;
					NewElement.JZ_ValuationDateOverride = importLeg.DepartureDate;
				}
				NewElement.CA_TreatmentCode = GetRegistryDefaultTariffTreatment();
			}
		}

		protected override void SetDefaultsForAdditionalInvoiceCore(BaseJobComInvoiceHeader previousInvoice)
		{
			base.SetDefaultsForAdditionalInvoiceCore(previousInvoice);
			if (Declaration.IsImport)
			{
				var previousInvoiceCA = (JobComInvoiceHeader)previousInvoice;
				NewElement.JZ_RN_NKDefaultOrigin = previousInvoiceCA.JZ_RN_NKDefaultOrigin;
				NewElement.JZ_RW_NKOriginState = previousInvoiceCA.JZ_RW_NKOriginState;
				NewElement.CA_RN_NKExport = previousInvoiceCA.CA_RN_NKExport;
				NewElement.CA_USStateOfExport = previousInvoiceCA.CA_USStateOfExport;
				NewElement.CA_RN_NKTranshipment = previousInvoiceCA.CA_RN_NKTranshipment;
				NewElement.BuyerDocumentaryAddress.E2_OA_Address = previousInvoiceCA.BuyerDocumentaryAddress.E2_OA_Address;
				NewElement.ExporterDocumentaryAddress.E2_OA_Address = previousInvoiceCA.ExporterDocumentaryAddress.E2_OA_Address;
				NewElement.JZ_OA_ManufacturerAddress = previousInvoiceCA.JZ_OA_ManufacturerAddress;
				NewElement.CA_DepartmentRuling = previousInvoiceCA.CA_DepartmentRuling;
				NewElement.CA_RL_NKLastPort = previousInvoiceCA.CA_RL_NKLastPort;
				NewElement.CA_ConditionsOfSale = previousInvoiceCA.CA_ConditionsOfSale;
				NewElement.CA_TermsOfPayment = previousInvoiceCA.CA_TermsOfPayment;
				NewElement.CA_ServicesInd = previousInvoiceCA.CA_ServicesInd;
				NewElement.CA_RoyaltyInd = previousInvoiceCA.CA_RoyaltyInd;
				NewElement.CA_OtherReference = previousInvoiceCA.CA_OtherReference;
				NewElement.CA_TradeZone = previousInvoiceCA.CA_TradeZone;
				NewElement.CA_USPortOfExit = previousInvoiceCA.CA_USPortOfExit;
				NewElement.CA_TreatmentCode = !previousInvoiceCA.CA_TreatmentCode.IsEmpty ? previousInvoiceCA.CA_TreatmentCode : GetRegistryDefaultTariffTreatment();
				NewElement.CA_TimeLimit = previousInvoiceCA.CA_TimeLimit;
				NewElement.CA_TimeLimitCode = previousInvoiceCA.CA_TimeLimitCode;
				NewElement.CA_ValueForDutyCode = previousInvoiceCA.CA_ValueForDutyCode;
				NewElement.JZ_ValuationDateOverride = previousInvoiceCA.JZ_ValuationDateOverride;
			}
		}

		ZString GetRegistryDefaultTariffTreatment()
		{
			return Declaration != null ? new ZString(CACustomsDataRegistry.Instance.DefaultTariffTreatment.GetFallBackValueAtAllLevels(Declaration.CompanyPK.ToGuid(), Declaration.JE_GB.IsValid ? Declaration.JE_GB.ToGuid() : Environment.Env.CurrentBranchPK, Guid.Empty)) : ZString.Empty;
		}
	}
}
