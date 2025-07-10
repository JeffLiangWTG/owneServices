using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class PenaltyRefundRequestMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<PenaltyRefundRequestMessageSendingObject>
	{
		public PenaltyRefundRequestMessageSendingObjectCollection(JobDeclaration declaration) : base(declaration.Factory)
		{
			this.declaration = declaration;
			PopulateElements();
		}
		readonly JobDeclaration declaration;

		void PopulateElements()
		{
			//we need to check if there are previous attempts of 5UL sending which failed. We need to let users to send them first.
			//we can go through entry.EntryNums of 5UL which is rejected
			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				foreach (var statementLine in entry.PaidStatementLines)
				{
					var isOriginalMessageAllowed = (CusEntryNumber entryNumber) => CustomsMessageStatusTypeList.IsOriginalMessageAllowed(entryNumber.CE_EntryStatus);
					var customsDisbursementNumber = statementLine.IndividualCustomsDisbursementBillNo;
					var refundEntryNumber = entry.EntryNumbers.GetOrCreateCusEntryNumWithMatchingVersionNumber(ElectronicDocumentTypeList.Codes._5UL, customsDisbursementNumber, isOriginalMessageAllowed);
					Add(new PenaltyRefundRequestMessageSendingObject(entry, customsDisbursementNumber, refundEntryNumber));
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotImplementedException();
		}

		protected override bool AllowNewCore => false;
	}
}
