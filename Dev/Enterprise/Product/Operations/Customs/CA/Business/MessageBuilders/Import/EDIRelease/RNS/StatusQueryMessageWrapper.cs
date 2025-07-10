namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.CA.Messaging;
	using Enterprise.Customs.Common;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Messaging.Business;

	public class StatusQueryMessageWrapper : IRNSRequest
	{
		public StatusQueryMessageWrapper(CusEntryHeader entryHeader)
		{
			CanSendDeclarationChecker.EntryNotNullAndAttachedToDeclaration(entryHeader);
			this.entryHeader = entryHeader;
			declaration = entryHeader.Declaration;
		}

		#region IRNSRequest Members

		#region ICAEDIFACTMessageAttachee Members

		BusinessObjectFactory IEDIMessageCollectionProvider.Factory
		{
			get { return declaration.Factory; }
		}

		void IEDIFACTMessageAttachee.AddMessage(EDIMessage message)
		{
			entryHeader.Messages.Add(message);
		}

		EDIMessageCollection IEDIMessageCollectionProvider.Messages
		{
			get { return entryHeader.Messages; }
		}

		ZString IEDIFACTMessageAttachee.MessageStatus
		{
			get { return entryHeader.CH_Status; }
			set { entryHeader.CH_Status = value; }
		}

		ZString IEDIFACTMessageAttachee.JobStatus
		{
			get { return entryHeader.CH_EntryStatus; }
			set { entryHeader.CH_EntryStatus = value; }
		}

		bool IEDIFACTMessageAttachee.HasChanges
		{
			get { return declaration.HasChanges; }
		}

		ZString IEDIFACTMessageAttachee.JobIdentification
		{
			get { return declaration.JE_DeclarationReference; }
		}

		BusinessObject IEDIFACTMessageAttachee.TopLevelBusinessObject
		{
			get { return declaration; }
		}

		bool ICAEDIFACTMessageAttachee.IsCancelled
		{
			get { return declaration.IsCancelled; }
		}

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage
		{
			get { return declaration.RefreshValidationBeforeSendMessage; }
		}

		#endregion

		ZDateTime IRNSRequestData.DateOfArrival
		{
			get { return ZDateTime.Now; }
		}

		ZString IRNSRequestData.CargoControlNumber
		{
			get
			{
				var cusNumber = declaration.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(CanadaAdditionalReferenceNumberTypes.Codes.CCN);
				return cusNumber != null ? cusNumber.CE_EntryNum.Replace(" ", "") : ZString.Empty;
			}
		}

		ZString IRNSRequestData.HouseBillNumber
		{
			get { return ZString.Empty; }
		}

		ZString IRNSRequestData.TransactionNumber
		{
			get { return declaration.TransactionNumber; }
		}

		ZString IRNSRequestData.OfficeCode
		{
			get { return ZString.Empty; }
		}

		ZString IRNSRequestData.SubLocationCode
		{
			get { return ZString.Empty; }
		}

		void IRNSRequest.RefreshMessagesForDisplay()
		{
		}

		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;

		#endregion
	}
}
