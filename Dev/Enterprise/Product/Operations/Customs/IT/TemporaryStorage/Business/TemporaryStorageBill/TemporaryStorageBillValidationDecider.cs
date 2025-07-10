using System;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

sealed class TemporaryStorageBillValidationDecider : ITemporaryStorageBillValidationDecider
{
	public bool IsGrossWeightCheckSupported => false;

	public bool IsTypeOfBillDocumentCheckSupported => false;

	public bool IsConsignorOrgPKCheckSupported => false;

	public bool IsConsigneeOrgPKCheckSupported => false;

	public bool IsShipperNameCheckSupported => false;

	public bool IsShipperCountryCheckSupported => false;

	public bool IsShipperPostcodeCheckSupported => false;

	public bool IsShipperRegNoTypeCheckSupported => false;

	public bool IsConsigneeNameCheckSupported => false;

	public bool IsConsigneeCountryCheckSupported => false;

	public bool IsConsigneePostcodeCheckSupported => false;

	public bool IsConsigneeRegNoTypeCheckSupported => false;

	public Func<AsycudaBill, bool> IsTypeOfBillDocumentMandatory => (x) => true;

	public Func<AsycudaBill, bool> IsABL_BillNumberMandatory => (x) => true;

	public Func<EU.Business.CusTempStorage.TemporaryStorageBill, bool> AllowDuplicateTypeAndNumber => (x) => false;

	public Func<AsycudaBill, bool> IsConsignorOrgPKMandatory => (x) => true;

	public Func<AsycudaBill, bool> IsShipperNameMandatory => (x) => true;

	public Func<AsycudaBill, bool> IsShipperCountryMandatory => (x) => true;

	public Func<AsycudaBill, bool> IsShipperPostcodeMandatory => (x) => true;

	public Func<AsycudaBill, bool> IsShipperRegNoTypeMandatory => (x) => true;

	public Func<AsycudaBill, bool> IsConsigneeOrgPKMandatory => (x) => true;

	public Func<AsycudaBill, bool> IsConsigneeNameMandatory => (x) => true;

	public Func<AsycudaBill, bool> IsConsigneeCountryMandatory => (x) => true;

	public Func<AsycudaBill, bool> IsConsigneePostcodeMandatory => (x) => true;

	public Func<AsycudaBill, bool> IsConsigneeRegNoTypeMandatory => (x) => true;
}

