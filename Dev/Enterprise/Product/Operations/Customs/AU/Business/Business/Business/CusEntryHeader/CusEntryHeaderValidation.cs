namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusEntryHeaderValidation : Customs.Business.CusEntryHeaderValidation
	{
		public CusEntryHeaderValidation(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
			this.entryHeader = entryHeader;
		}

		readonly CusEntryHeader entryHeader;

		protected override string MessageToAddForNonAmendableChanges
		{
			get { return string.Format(HasNonAmendableNatureChanges, entryHeader.ZA_DetailsNotToBeAmended, entryHeader.Nature); }
		}

		public const string HasNonAmendableNatureChanges = "The changes you have just made lead to the entry's nature change from {0} to {1} and it will be rejected by Customs as the nature is not amendable. You should withdraw the current entry first WITHOUT changes you have just made and then relodge a new entry with the change.";
	}
}
