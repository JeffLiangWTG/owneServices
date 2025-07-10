using CargoWise.Types;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public interface IJobReferenceMatcher
	{
		public (ZGuid jobPK, ZString parentTableCode) FindBestMatching(string referenceNumber, string[] searchFields = null);
		public void ResetQueryEngine();
	}
}
