namespace Enterprise.Customs.KR.Business
{
	public class Import5BFCancelCreator
	{
		public Import5BFCancel Create(CusEntryHeader entry, CancellationMessageSendingObject messageSendingObject)
		{
			var import5BFData = new Import5BFCancel();

			import5BFData.ImportDeclarationNumber = entry.EntryNumber;
			import5BFData.ApplicationReason = messageSendingObject.CancellationReason;

			return import5BFData;
		}
	}
}
