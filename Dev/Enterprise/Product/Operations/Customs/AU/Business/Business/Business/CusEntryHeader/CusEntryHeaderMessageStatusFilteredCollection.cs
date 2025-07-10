using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusEntryHeaderMessageStatusFilteredCollection : Customs.Business.CusEntryHeaderMessageStatusSubsetCollection
	{
		public CusEntryHeaderMessageStatusFilteredCollection(Customs.Business.ActiveCusEntryHeaderCollection allEntryHeaders)
			: base(allEntryHeaders)
		{
		}

		public CusEntryHeaderMessageStatusFilteredCollection(CusEntryHeader[] entries, BusinessObjectFactory factory)
			: base(entries, factory)
		{
		}

		public new CusEntryHeader this[int index]
		{
			get { return (CusEntryHeader)base[index]; }
		}

		public new CusEntryHeader AddNew()
		{
			return (CusEntryHeader)base.AddNew();
		}

		public bool AreAllCPDecQuestionsAnswered()
		{
			foreach (CusEntryHeader entryHeader in this)
			{
				if (!entryHeader.Questions.AreAllCPDecQuestionsAnswered || !entryHeader.MergedLines.AreAllCPQuestionsAnswered)
				{
					return false;
				}
			}
			return true;
		}

		protected override bool CanSendAmendmentForThisEntry(Customs.Business.CusEntryHeader entryHeader)
		{
			CusEntryHeader aUEntryHeader = entryHeader as CusEntryHeader;
			return aUEntryHeader.IsStatusPostLodge && !aUEntryHeader.IsWithdrawn;
		}

		protected override bool CanSendOriginalForThisEntry(Customs.Business.CusEntryHeader entryHeader)
		{
			return !CanSendAmendmentForThisEntry(entryHeader) && !(entryHeader as CusEntryHeader).IsWithdrawn;
		}

		protected override bool CanSendWithdrawForThisEntry(Customs.Business.CusEntryHeader entryHeader)
		{
			return CanSendAmendmentForThisEntry(entryHeader) && !(entryHeader as CusEntryHeader).IsWithdrawn;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
