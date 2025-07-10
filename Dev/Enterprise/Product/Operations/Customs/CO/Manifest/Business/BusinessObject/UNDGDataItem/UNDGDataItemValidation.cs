using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CO.Manifest.Business
{
	public class UNDGDataItemValidation : MasterFiles.Business.UNDGDataItemValidation
	{
		public UNDGDataItemValidation(AutoUNDGDataItem parent)
		: base(parent)
		{ }

		protected override bool UNDGSubstanceIsRequired => false;

		protected override void CheckDI_DG_NKSubs()
		{
			if (!Parent.DI_DG_NKSubs.IsNumbersOnlyOrEmpty)
			{
				Parent.DI_DG_NKSubsInfo.AddWarning(ResString.GetMultilingualString("B256E4C4-E925-4087-B012-F3A31BA612D6", "The value should be numeric"));
			}
		}
	}
}
