using System;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public interface ITemporaryStorageBillValidationDecider
	{
		bool IsGrossWeightCheckSupported { get; }

		bool IsTypeOfBillDocumentCheckSupported { get; }

		bool IsConsignorOrgPKCheckSupported { get; }

		bool IsConsigneeOrgPKCheckSupported { get; }

		bool IsShipperNameCheckSupported { get; }

		bool IsShipperCountryCheckSupported { get; }

		bool IsShipperPostcodeCheckSupported { get; }

		bool IsShipperRegNoTypeCheckSupported { get; }

		bool IsConsigneeNameCheckSupported { get; }

		bool IsConsigneeCountryCheckSupported { get; }

		bool IsConsigneePostcodeCheckSupported { get; }

		bool IsConsigneeRegNoTypeCheckSupported { get; }

		Func<AsycudaBill, bool> IsTypeOfBillDocumentMandatory { get; }

		Func<AsycudaBill, bool> IsABL_BillNumberMandatory { get; }

		Func<TemporaryStorageBill, bool> AllowDuplicateTypeAndNumber { get; }

		Func<AsycudaBill, bool> IsConsignorOrgPKMandatory { get; }

		Func<AsycudaBill, bool> IsShipperNameMandatory { get; }

		Func<AsycudaBill, bool> IsShipperCountryMandatory { get; }

		Func<AsycudaBill, bool> IsShipperPostcodeMandatory { get; }

		Func<AsycudaBill, bool> IsShipperRegNoTypeMandatory { get; }

		Func<AsycudaBill, bool> IsConsigneeOrgPKMandatory { get; }

		Func<AsycudaBill, bool> IsConsigneeNameMandatory { get; }

		Func<AsycudaBill, bool> IsConsigneeCountryMandatory { get; }

		Func<AsycudaBill, bool> IsConsigneePostcodeMandatory { get; }
		
		Func<AsycudaBill, bool> IsConsigneeRegNoTypeMandatory { get; }
	}
}
