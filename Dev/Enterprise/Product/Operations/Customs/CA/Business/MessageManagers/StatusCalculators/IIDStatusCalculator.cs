namespace Enterprise.Customs.CA.Business.MessageManagers
{
	using CargoWise.Types;
	using Enterprise.Customs.Common.MessageBuilders;

	public class IIDStatusCalculator : EDIFACTMessageStatusCalculator
	{
		public override ZString CalculatedJobStatus(IEDIFACTMessageAttachee linkedObject)
		{
			return ZString.Empty;
		}

		public override ZString MessageTypeDescription
		{
			get { return MessageTypeList.Descriptions.IntegratedImportDeclaration; }
		}

		public override bool IsLodged(ZString currentMessageStatus)
		{
			return !currentMessageStatus.IsEmpty && currentMessageStatus != MessageStatusList.Codes.AwaitingOriginal && currentMessageStatus != MessageStatusList.Codes.ErrorOriginal;
		}

		public override ZString GetMessageAwaitingStatus(ZString messageSubType)
		{
			switch (messageSubType)
			{
				case IIDMessageSubTypeList.Codes.Original:
					return MessageStatusList.Codes.AwaitingOriginal;
				case IIDMessageSubTypeList.Codes.Change:
					return MessageStatusList.Codes.AwaitingChange;
				case IIDMessageSubTypeList.Codes.Amendment:
					return MessageStatusList.Codes.AwaitingReplace;
				case IIDMessageSubTypeList.Codes.Cancellation:
					return MessageStatusList.Codes.AwaitingDelete;
				default:
					return ZString.Empty;
			}
		}
	}
}
