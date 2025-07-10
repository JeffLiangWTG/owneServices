using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.Business.Messaging
{
	public abstract class CusDecFunctionChooser
	{
		protected CusDecFunctionChooser(CusEntryHeader entry, Customs.Business.CusdecMessageFunction declarationMessageFunctionFromUserClick)
		{
			this.entryHeader = entry;
			this.declarationMessageFunctionFromUserClick = declarationMessageFunctionFromUserClick;
		}

		public CusDecMessageTypeFunction Function
		{
			get
			{
				var msgTypeNewOrDeleted = declarationMessageFunctionFromUserClick is Customs.Business.CusdecMessageFunction.Deleted ? CusDecMessageTypeFunction.Delete : CusDecMessageTypeFunction.Original;

				if (!entryHeader.CH_CustomsMessageRemarks.IsEmpty && !IsEntryHeaderNumberOrStatusEmpty && msgTypeNewOrDeleted != CusDecMessageTypeFunction.Delete)
				{   // Entry number(or status) exists and amendment remarks exists, and we're not deleting, send an amendment
					msgTypeNewOrDeleted = CusDecMessageTypeFunction.Replacement;
				}
				else if (IsSendingSecondMessageUnderFallback())
				{
					msgTypeNewOrDeleted = CusDecMessageTypeFunction.Replacement;
				}
				return msgTypeNewOrDeleted;
			}
		}

		public ZString MessageTypeForEdiMessage
		{
			get
			{
				if (Function == CusDecMessageTypeFunction.Delete)
				{
					return GetMessageTypeForDeleteEdiMessage();
				}
				else
				{
					var gbDec = entryHeader.Declaration as Declaration.JobDeclaration;
					return gbDec?.JE_DeclarationType ?? ZString.Empty;
				}
			}
		}

		protected abstract string GetMessageTypeForDeleteEdiMessage();

		protected abstract bool IsSendingSecondMessageUnderFallback();

		protected virtual bool IsEntryHeaderNumberOrStatusEmpty
		{
			get { return entryHeader.EntryNumber.IsEmpty; }
		}

		protected CusEntryHeader entryHeader;
		protected Customs.Business.CusdecMessageFunction declarationMessageFunctionFromUserClick;
	}
}
