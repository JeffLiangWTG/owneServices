using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using CustomsStatus = Enterprise.Customs.EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus;
using RefCusCodeListType = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code;
using RefDataGrouping = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	static class PNTSMessageTestHelper
	{
		public static void SetUpStatusAndDescriptionMaps(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var (minTime, maxTime) = (ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateCusMapType(RefCusMapTypeList.Codes.PNTSS, "INW", "Customs PNTS Status to CW1 PNTS Status", true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.PNTSS, CustomsStatus.TSDInvalidated, "03", minTime, maxTime, RefDataGrouping.EuropeanUnionEUN);
			helper.CreateCusMap(RefCusMapTypeList.Codes.PNTSS, CustomsStatus.PresentationNotificationLinked, "04", minTime, maxTime, RefDataGrouping.EuropeanUnionEUN);
			helper.CreateCusMap(RefCusMapTypeList.Codes.PNTSS, CustomsStatus.PresentationNotificationNotLinked, "05", minTime, maxTime, RefDataGrouping.EuropeanUnionEUN);
			helper.CreateCusMap(RefCusMapTypeList.Codes.PNTSS, CustomsStatus.ProofOfUnionStatusPresented, "13", minTime, maxTime, RefDataGrouping.EuropeanUnionEUN);
			helper.CreateCusMap(RefCusMapTypeList.Codes.PNTSS, CustomsStatus.IrregularityUnderInvestigation, "06", minTime, maxTime, RefDataGrouping.EuropeanUnionEUN);
			helper.CreateCusMap(RefCusMapTypeList.Codes.PNTSS, CustomsStatus.MeasuresRequired, "09", minTime, maxTime, RefDataGrouping.EuropeanUnionEUN);
			helper.CreateCusMap(RefCusMapTypeList.Codes.PNTSS, CustomsStatus.TemporaryStorageEnded, "12", minTime, maxTime, RefDataGrouping.EuropeanUnionEUN);
			helper.CreateCusMap(RefCusMapTypeList.Codes.PNTSS, CustomsStatus.TemporaryStorageActivated, "02", minTime, maxTime, RefDataGrouping.EuropeanUnionEUN);
			helper.CreateCusMap(RefCusMapTypeList.Codes.PNTSS, CustomsStatus.TemporaryStoragePreLodged, "01", minTime, maxTime, RefDataGrouping.EuropeanUnionEUN);
			helper.CreateCusMap(RefCusMapTypeList.Codes.PNTSS, CustomsStatus.UnderControl, "08", minTime, maxTime, RefDataGrouping.EuropeanUnionEUN);

			helper.CreateCusCodeType(RefCusCodeListType.TemporaryStorageCustomsStatus, "TSTA");
			helper.CreateCusCodeList(RefDataGrouping.EuropeanUnionEUN, RefCusCodeListType.TemporaryStorageCustomsStatus, "INV", "TSD Invalidated", minTime, maxTime);
			helper.CreateCusCodeList(RefDataGrouping.EuropeanUnionEUN, RefCusCodeListType.TemporaryStorageCustomsStatus, "PLN", "Presentation Notification Linked", minTime, maxTime);
			helper.CreateCusCodeList(RefDataGrouping.EuropeanUnionEUN, RefCusCodeListType.TemporaryStorageCustomsStatus, "PNL", "Presentation Notification Not Linked", minTime, maxTime);
			helper.CreateCusCodeList(RefDataGrouping.EuropeanUnionEUN, RefCusCodeListType.TemporaryStorageCustomsStatus, "TCT", "Intended Control", minTime, maxTime);
			helper.CreateCusCodeList(RefDataGrouping.EuropeanUnionEUN, RefCusCodeListType.TemporaryStorageCustomsStatus, "TEP", "Proof of Union Status Presented", minTime, maxTime);
			helper.CreateCusCodeList(RefDataGrouping.EuropeanUnionEUN, RefCusCodeListType.TemporaryStorageCustomsStatus, "TII", "Irregularity Under Investigation", minTime, maxTime);
			helper.CreateCusCodeList(RefDataGrouping.EuropeanUnionEUN, RefCusCodeListType.TemporaryStorageCustomsStatus, "TMR", "Measures Required", minTime, maxTime);
			helper.CreateCusCodeList(RefDataGrouping.EuropeanUnionEUN, RefCusCodeListType.TemporaryStorageCustomsStatus, "TND", "Temporary Storage Ended", minTime, maxTime);
			helper.CreateCusCodeList(RefDataGrouping.EuropeanUnionEUN, RefCusCodeListType.TemporaryStorageCustomsStatus, "TSA", "Temporary Storage Activated", minTime, maxTime);
			helper.CreateCusCodeList(RefDataGrouping.EuropeanUnionEUN, RefCusCodeListType.TemporaryStorageCustomsStatus, "TSP", "Temporary Storage Pre-Lodged", minTime, maxTime);
			helper.CreateCusCodeList(RefDataGrouping.EuropeanUnionEUN, RefCusCodeListType.TemporaryStorageCustomsStatus, "TUC", "Under Control", minTime, maxTime);

			helper.CreateCusCodeList(RefDataGrouping.EuropeanUnionEUN, RefCusCodeListType.TemporaryStorageStatusReason, "03", "Amendment irregularity", minTime, maxTime);
			helper.CreateCusCodeList(RefDataGrouping.EuropeanUnionEUN, RefCusCodeListType.TemporaryStorageStatusReason, "04", "Location of goods type D", minTime, maxTime);
			helper.CreateCusCodeList(RefDataGrouping.EuropeanUnionEUN, RefCusCodeListType.TemporaryStorageStatusReason, "05", "Non satisfactory control result", minTime, maxTime);
			helper.CreateCusCodeList(RefDataGrouping.EuropeanUnionEUN, RefCusCodeListType.TemporaryStorageStatusReason, "06", "Irregularity refused", minTime, maxTime);
			helper.CreateCusCodeList(RefDataGrouping.EuropeanUnionEUN, RefCusCodeListType.TemporaryStorageStatusReason, "07", "Decision to end temporary storage by customs", minTime, maxTime);
			helper.CreateCusCodeList(RefDataGrouping.EuropeanUnionEUN, RefCusCodeListType.TemporaryStorageStatusReason, "08", "Write-off completed at timer expiration", minTime, maxTime);
			helper.CreateCusCodeList(RefDataGrouping.EuropeanUnionEUN, RefCusCodeListType.TemporaryStorageStatusReason, "09", "Write-off not completed at timer expiration", minTime, maxTime);
			helper.CreateCusCodeList(RefDataGrouping.EuropeanUnionEUN, RefCusCodeListType.TemporaryStorageStatusReason, "10", "Irregularity at timer expiration", minTime, maxTime);

			factory.Save();
		}
	}
}
