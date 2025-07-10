namespace Enterprise.Customs.GB.CDS
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Will be used for CDS soon.")]
	public class CDSCusDecFunctionChooser : Business.Messaging.CusDecFunctionChooser
	{
		public CDSCusDecFunctionChooser(EU.Business.Declaration.CusEntryHeader entry, Customs.Business.CusdecMessageFunction declarationMessageFunctionFromUserClick)
			: base(entry, declarationMessageFunctionFromUserClick)
		{
		}

		protected override bool IsEntryHeaderNumberOrStatusEmpty
		{
			get { return entryHeader.CH_EntryStatus.IsEmpty; }
		}

		protected override bool IsSendingSecondMessageUnderFallback()
		{
			return false;
		}

		protected override string GetMessageTypeForDeleteEdiMessage()
		{
			return "";
		}
	}
}
