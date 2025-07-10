using System;
using System.Linq;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public sealed class TemporaryStorageBillValidationDecider : EU.Business.CusTempStorage.ITemporaryStorageBillValidationDecider
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

	public Func<AsycudaBill, bool> IsTypeOfBillDocumentMandatory => (x) => !((TemporaryStorageHeader)x.Header).IsMessageTypeManual && !CheckAllPrevDocHaveTRAInItems(x);

	public Func<AsycudaBill, bool> IsABL_BillNumberMandatory => (x) => !((TemporaryStorageHeader)x.Header).IsMessageTypeManual && !CheckAllPrevDocHaveTRAInItems(x);

	public Func<EU.Business.CusTempStorage.TemporaryStorageBill, bool> AllowDuplicateTypeAndNumber => (x) => false;

	public Func<AsycudaBill, bool> IsConsignorOrgPKMandatory => (x) => !((TemporaryStorageHeader)x.Header).IsMessageTypeManual;

	public Func<AsycudaBill, bool> IsShipperNameMandatory => (x) => !((TemporaryStorageHeader)x.Header).IsMessageTypeManual;

	public Func<AsycudaBill, bool> IsShipperCountryMandatory => (x) => !((TemporaryStorageHeader)x.Header).IsMessageTypeManual;

	public Func<AsycudaBill, bool> IsShipperPostcodeMandatory => (x) => !((TemporaryStorageHeader)x.Header).IsMessageTypeManual;

	public Func<AsycudaBill, bool> IsShipperRegNoTypeMandatory => (x) => !((TemporaryStorageHeader)x.Header).IsMessageTypeManual;

	public Func<AsycudaBill, bool> IsConsigneeOrgPKMandatory => (x) => !((TemporaryStorageHeader)x.Header).IsMessageTypeManual;

	public Func<AsycudaBill, bool> IsConsigneeNameMandatory => (x) => !((TemporaryStorageHeader)x.Header).IsMessageTypeManual;

	public Func<AsycudaBill, bool> IsConsigneeCountryMandatory => (x) => !((TemporaryStorageHeader)x.Header).IsMessageTypeManual;

	public Func<AsycudaBill, bool> IsConsigneePostcodeMandatory => (x) => !((TemporaryStorageHeader)x.Header).IsMessageTypeManual;

	public Func<AsycudaBill, bool> IsConsigneeRegNoTypeMandatory => (x) => !((TemporaryStorageHeader)x.Header).IsMessageTypeManual;

	bool CheckAllPrevDocHaveTRAInItems(AsycudaBill parent)
		=> parent.PackedItems.All(i =>
		{
			var item = (TemporaryStoragePackedItem)i;
			return item.AdditionalInfos.Cast<TemporaryStorageAdditionalInfo>().Any(x => x.CSI_SubType == AdditionalDocList.Codes.TransportDocuments);
		});
}