using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NctsHeaderMessageSendingObject : EU.NCTS.Business.NctsHeaderMessageSendingObject
	{
		public NctsHeaderMessageSendingObject(EU.NCTS.Business.NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public new NctsHeader NctsHeader => (NctsHeader)base.NctsHeader;

		public new NctsHeaderMessageSendingObjectValidation Validation => (NctsHeaderMessageSendingObjectValidation)base.Validation;

		protected override EU.NCTS.Business.NctsHeaderMessageSendingObjectValidation GetNewValidation() => new NctsHeaderMessageSendingObjectValidation(this);

		protected override bool EnquiryText_ReadOnly => !is141Message;

		protected override bool TC11DeliveryDate_ReadOnly => !is141Message;

		protected override bool DestinationCustomsOfficeCode_ReadOnly => !is141Message;

		protected override bool Consignee_ReadOnly => !is141Message;

		bool is141Message => MessageType == Messaging.NCTSOutgoingMessageTypeList.Codes.InformationAboutNonArrivedMovement;

		public override ZString MessageType
		{
			get => base.MessageType;
			set
			{
				var oldValue = MessageType;
				base.MessageType = value;
				if (oldValue != MessageType)
				{
					RefreshGuaranteeProcessorAndUnlockMutexIfNeeded();
				}
			}
		}

		public override ZBool ShouldSend
		{
			get => base.ShouldSend;
			set
			{
				var oldValue = ShouldSend;
				base.ShouldSend = value;
				if (oldValue != ShouldSend)
				{
					RefreshGuaranteeProcessorAndUnlockMutexIfNeeded();
				}
			}
		}

		internal void UnlockMutexGuaranteeProcessorIfNeeded()
		{
			if (guaranteeProcessor != null)
			{
				guaranteeProcessor.UnlockPermitMutexes();
				guaranteeProcessor = null;
			}
		}

		void RefreshGuaranteeProcessorAndUnlockMutexIfNeeded()
		{
			UnlockMutexGuaranteeProcessorIfNeeded();
			if (UseGuaranteeProcessor)
			{
				CreateGuaranteeProcessor();
				guaranteeProcessor.AddPermitRecordsAndLockMutexIfNeeded();
				// TODO: use guaranteeProcessor.PermitErrors(ZString.Empty); to extra errors messages
			}
		}

		internal NctsCusPermitCusDecProcessor GuaranteeProcessor
		{
			get
			{
				if (guaranteeProcessor == null && UseGuaranteeProcessor)
				{
					CreateGuaranteeProcessor();
				}
				return guaranteeProcessor;
			}
		}
		NctsCusPermitCusDecProcessor guaranteeProcessor;

		void CreateGuaranteeProcessor()
		{
			guaranteeProcessor = new NctsCusPermitCusDecProcessor(NctsHeader, MessageType);
		}

		bool UseGuaranteeProcessor
		{
			get
			{
				switch (MessageType)
				{
					case Messaging.NCTSOutgoingMessageTypeList.Codes.DeclarationData:
					case Messaging.NCTSOutgoingMessageTypeList.Codes.DeclarationAmendment:
					case Messaging.NCTSOutgoingMessageTypeList.Codes.DeclarationInvalidationRequest:
						return ShouldSend;
					default:
						return false;
				}
			}
		}
	}
}
