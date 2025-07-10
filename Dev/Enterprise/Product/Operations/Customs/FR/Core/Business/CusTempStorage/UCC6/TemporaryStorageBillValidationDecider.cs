using System;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public sealed class TemporaryStorageBillValidationDecider : ITemporaryStorageBillValidationDecider
	{
		public bool IsGrossWeightCheckSupported => true;

		public bool IsTypeOfBillDocumentCheckSupported => true;

		public bool IsConsignorOrgPKCheckSupported => true;

		public bool IsConsigneeOrgPKCheckSupported => true;

		public bool IsShipperNameCheckSupported => true;

		public bool IsShipperCountryCheckSupported => true;

		public bool IsShipperPostcodeCheckSupported => true;

		public bool IsShipperRegNoTypeCheckSupported => true;

		public bool IsConsigneeNameCheckSupported => true;

		public bool IsConsigneeCountryCheckSupported => true;

		public bool IsConsigneePostcodeCheckSupported => true;

		public bool IsConsigneeRegNoTypeCheckSupported => true;

		public Func<AsycudaBill, bool> IsTypeOfBillDocumentMandatory => (x) => true;

		public Func<AsycudaBill, bool> IsABL_BillNumberMandatory => (x) => true;

		public Func<EU.Business.CusTempStorage.TemporaryStorageBill, bool> AllowDuplicateTypeAndNumber => (x) => x?.Header?.IsENSReuse ?? false;

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
}
