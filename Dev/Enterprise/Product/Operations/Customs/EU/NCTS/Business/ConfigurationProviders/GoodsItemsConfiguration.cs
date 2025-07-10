using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class GoodsItemsConfiguration
	{
		public virtual ZBool ImportMethodOfPaymentVisible(BusinessObject businessObject) => false;

		public virtual ZBool AdditionalInfosSupport(BusinessObject businessObject) => true;

		public virtual ZBool SupportingDocumentsSupport(BusinessObject businessObject) => true;

		public virtual ZBool PreviousDocumentsSupport(NctsHeader header) => true;

		public virtual ZBool TaxSupport(BusinessObject businessObject) => true;

		public ZBool DeleteConfirmationSupport(NctsHeader header) => DeleteConfirmationSupportCore(header);
		protected virtual ZBool DeleteConfirmationSupportCore(NctsHeader header) => false;

		public INctsCargoDescValidationDecider GetValidationDecider(NctsCommonCargoDesc goodsItem) => GetValidationDeciderCore(goodsItem);

		protected virtual INctsCargoDescValidationDecider GetValidationDeciderCore(NctsCommonCargoDesc goodsItem)
		{
			INctsCargoDescValidationDecider result = null;
			if (goodsItem != null)
			{
				if (goodsItem.IsPhase5Departure)
				{
					result = GetDeparturePhase5ValidationDecider(goodsItem);
				}
				else if (goodsItem.IsPhase5Arrival)
				{
					result = GetArrivalPhase5ValidationDecider();
				}
			}

			return result;
		}

		protected virtual INctsDepartureCargoDescPhase5ValidationDecider GetDeparturePhase5ValidationDecider(NctsCommonCargoDesc goodsItem) => new NctsDepartureCargoDescPhase5ValidationDecider();

		protected virtual INctsArrivalCargoDescPhase5ValidationDecider GetArrivalPhase5ValidationDecider() => new NctsArrivalCargoDescPhase5ValidationDecider();

		public INctsAdditionalInfoValidationDecider GetAdditionalInfoValidationDecider(NctsHeader header) => GetAdditionalInfoValidationDeciderCore(header);

		protected virtual INctsAdditionalInfoValidationDecider GetAdditionalInfoValidationDeciderCore(NctsHeader header)
		{
			INctsAdditionalInfoValidationDecider result = null;
			if (header is { IsPhase5: true })
			{
				result = GetAdditionalInfoPhase5ValidationDecider();
			}

			return result;
		}

		public ICollection<RefCusCodeListAttributeFilter> GetCusCodeListAttributeFilters()
		{
			if (IsCL016CodeListFilterActive)
			{
				return [new RefCusCodeListAttributeFilter(CL016CusCode, SQLComparisonOperator.Equal, YesNoList.Codes.Yes)];
			}
			return Array.Empty<RefCusCodeListAttributeFilter>();
		}

		const string CL016CusCode = "CL016";
		protected virtual bool IsCL016CodeListFilterActive => false;

		protected virtual INctsAdditionalInfoPhase5ValidationDecider GetAdditionalInfoPhase5ValidationDecider() => new NctsAdditionalInfoPhase5ValidationDecider();

		public ZBool IsLiabilityCalculationForArrivalSupported() => IsLiabilityCalculationForArrivalSupportedCore();
		protected virtual ZBool IsLiabilityCalculationForArrivalSupportedCore() => false;

		public NctsPreviousDocumentConfiguration NctsPreviousDocumentConfiguration => nctsPreviousDocumentConfiguration ??= GetNewNctsPreviousDocumentConfiguration();
		NctsPreviousDocumentConfiguration nctsPreviousDocumentConfiguration;

		protected virtual NctsPreviousDocumentConfiguration GetNewNctsPreviousDocumentConfiguration() => new();

		public NctsSupportingDocumentConfiguration NctsSupportingDocumentConfiguration => nctsSupportingDocumentConfiguration ??= GetNewNctsSupportingDocumentConfiguration();
		NctsSupportingDocumentConfiguration nctsSupportingDocumentConfiguration;

		protected virtual NctsSupportingDocumentConfiguration GetNewNctsSupportingDocumentConfiguration() => new();
	}
}
