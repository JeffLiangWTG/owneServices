using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CUSPCSSplitCusTempStorageLineCollection : EU.Business.CusTempStorage.CusTempStorageLineCollectionTo<CUSPCSSplitCusTempStorageLine, CUSPCSConsolidatedCusTempStorageLine>
	{
		public CUSPCSSplitCusTempStorageLineCollection(CUSPCSConsolidatedCusTempStorageLine consolidatedLine)
			: base(consolidatedLine)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var toLine = child as CUSPCSSplitCusTempStorageLine;
			if (FromLine != null && toLine != null)
			{
				using (toLine.SuspendSettingDefaultCustodianDetails())
				{
					toLine.TSL_OA_Custodian = FromLine.TSL_OA_Custodian;
					toLine.TSL_CustodianIdentifier = FromLine.TSL_CustodianIdentifier;
					toLine.TSL_CustodianIdentifierBranchNo = FromLine.TSL_CustodianIdentifierBranchNo;
				}
			}
		}
	}
}
