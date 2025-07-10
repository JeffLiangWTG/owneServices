using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IScanForOutturnManager
	{
		ManualScanLineCollection ManualScanHistory { get; }
		OutturnLineCollection ManifestCollection { get; }
		int CountTotalNumberOfManualScansByBarcode(string barcode);
		ManualScanTarget CreateManualScanTarget(string barcode);
		string MergeManualScanResults();
		ManualScanTarget CreateEmptyManualScanTarget();
		bool IsInvalidConsignment(ManualScanTarget target);
		bool IsMatchedConsignmentCleared(ManualScanTarget target);
		bool IsPossibleSurplusPackage(OutturnLine outturn);
		bool IsPossibleSurplusConsignment(OutturnLine outturn);
		BusinessObject GetRelatedBusinessObject(ManualScanTarget target);
		ScanForOutturnManager.ManualScanStatuses GetStatus(BusinessObject relatedBusinessObject);
	}
}
