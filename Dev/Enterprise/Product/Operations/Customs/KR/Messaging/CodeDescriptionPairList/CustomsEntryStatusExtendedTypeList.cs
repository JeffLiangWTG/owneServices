namespace Enterprise.Customs.KR.Messaging
{
	public class CustomsEntryStatusExtendedTypeList : CustomsEntryStatusTypeList
	{
		public CustomsEntryStatusExtendedTypeList()
		{
			AddPair(CustomsMessageStatusTypeList.Codes.OriginalRejected, CustomsMessageStatusTypeList.Descriptions.OriginalRejected);
			AddPair(CustomsMessageStatusTypeList.Codes.OriginalAccepted, CustomsMessageStatusTypeList.Descriptions.OriginalAccepted);
		}
	}
}
