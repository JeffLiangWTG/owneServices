namespace Enterprise.Customs.KR.Business
{
	public class Import5BDCreator
	{
		public Import5BD Create(CusEntryHeader entry, EarlyReleaseMiscMessageSendingObject messageSendingObject)
		{
			var import5BDData = new Import5BD();
			import5BDData.ImportDeclarationNumber = entry.EntryNumber;
			import5BDData.RequestReason = messageSendingObject.AmendmentReason;
			import5BDData.SecurityType = messageSendingObject.SecurityType;
			if (messageSendingObject.SecurityStartDate.IsValid)
			{
				import5BDData.SecurityStartDate = messageSendingObject.SecurityStartDate.ToDateTime();
			}
			if (messageSendingObject.SecurityEndDate.IsValid)
			{
				import5BDData.SecurityEndDate = messageSendingObject.SecurityEndDate.ToDateTime();
			}
			import5BDData.SecurityAmount = messageSendingObject.SecurityAmount;
			import5BDData.OtherSecurityType = messageSendingObject.OtherSecurityType;
			import5BDData.ReasonForEarlyRemoval = messageSendingObject.ReasonForEarlyRemoval;
			return import5BDData;
		}
	}
}
