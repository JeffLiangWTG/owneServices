using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business
{
	public class DepositRefundApplicationMessageSendingActionLookups : CusEntryHeaderMessageSendingActionLookups
	{
		public DepositRefundApplicationMessageSendingActionLookups(DepositRefundApplicationMessageSendingAction parent) : base(parent) { }

		public CodeDescriptionPairList ExportMovementReferenceNumberList
		{
			get
			{
				return Factory.GetCachedValue($"IE.Business.DepositRefundApplicationMessageSendingActionLookups.ExportMovementReferenceNumberList|{Parent.EntryHeader.EntryInstruction.PK}", () =>
				{
					var result = new CodeDescriptionPairList();
					foreach (var previousDocument in Parent.EntryHeader.EntryInstruction.PreviousDocuments)
					{
						if (previousDocument.CSI_Code == Constants.PreviousDocumentTypeCodes.MRN)
						{
							result.AddPair(previousDocument.CSI_ReferenceNumber, string.Empty);
						}
					}
					return result;
				});
			}
		}

		public override CodeDescriptionPairList SendingActionTypeList => new CodeDescriptionPairList();

		protected new DepositRefundApplicationMessageSendingAction Parent => (DepositRefundApplicationMessageSendingAction)base.Parent;
	}
}
