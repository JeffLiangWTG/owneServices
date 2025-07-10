namespace Enterprise.Customs.CA.Business
{
	using System;
	using System.Data;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Common.CA;
	using Enterprise.Edifact.D99B.Elements;
	using Enterprise.Edifact.D99B.Messages.CUSDEC;
	using Enterprise.Edifact.D99B.Segments;
	using Enterprise.Environment;
	using Enterprise.Messaging.Business;
	using Enterprise.ZArchitecture.Core;

	class B3Message : EDIMessageWithBatchNumber
	{
		public B3Message(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		internal static B3Message GetLastSentAcceptedB3Message(CusEntryHeader entryHeader)
		{
			if (entryHeader == null)
			{
				throw new ArgumentNullException(nameof(entryHeader));
			}
			B3Message result = null;
			var lastAccepted = (from EDIMessage message in entryHeader.Messages
								where message != null &&
										message.EM_MessageType == MessageTypeList.Codes.B3CUSDEC
									&& !message.IsTransmitMessage
									&& (message.EM_MessageSubType == B3EntryStatusList.Codes.Accepted || message.EM_MessageSubType == B3EntryStatusList.Codes.Confirmed)
								orderby message.EM_SystemCreateTimeUtc descending
								select (B3Message)message).FirstOrDefault();

			if (lastAccepted != null)
			{
				result = (from EDIMessage message in entryHeader.Messages
						  where message != null &&
							  message.EM_MessageType == MessageTypeList.Codes.B3CUSDEC
								&& message.IsTransmitMessage
								&& message.EM_Status == Status.Sent
								&& message.EM_SystemCreateTimeUtc < lastAccepted.EM_SystemCreateTimeUtc
						  orderby message.EM_SystemCreateTimeUtc descending
						  select (B3Message)message).FirstOrDefault();
			}
			return result;
		}

		#region Overrides

		public override ZString TransactionNumber
		{
			get
			{
				if (EdifactMessage is CUSDECMessage cusdec)
				{
					return (from SegmentGroup1 grp in cusdec.Group1
							from RFFSegment seg in grp.RFF
							where seg.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.TransactionReferenceNumber
							select seg.Reference.ReferenceIdentifier.PadLeft(9, '0')).FirstOrDefault();
				}
				return ZString.Empty;
			}
		}

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", "IMP", EDIInterchange.ApplicationCodes.CAIMP).GetNextFormatted(Factory);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIInterchange.ApplicationCodes.CAIMP;
			EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
		}

		protected override string GetEntryNumber()
		{
			var entryHeader = EM_LinkedObject as CusEntryHeader
				?? throw new ArgumentException("CusEntryHeader expected as Linked Object on an B3Message");
			return entryHeader.EntryNumber;
		}

		public override ZDateTime RNSProcessingDate
		{
			get
			{
				ZDateTime processingDate;
				var cusdec = EdifactMessage as Enterprise.Edifact.D99B.Messages.CUSRES.CUSRESMessage;
				return cusdec != null && cusdec.DTM.Count > 0 && ZDateTime.TryParseExact(cusdec.DTM[0].DateTimePeriod.DateTimePeriodValue, out processingDate, "yyyyMMdd")
					? processingDate : ZDateTime.Empty;
			}
		}

		#region Properties

		protected override CodeDescriptionPairList MessageSubTypeList
		{
			get { return EM_ReceiveTransmit == EDIInterchange.Direction.Receive ? new B3EntryStatusList() : base.MessageSubTypeList; }
		}

		protected override bool ShouldUseUnformattedMessageText
		{
			get { return false; }
		}

		public override bool ShouldShowInterpretation
		{
			get { return true; }
		}

		#endregion

		#endregion
	}
}
