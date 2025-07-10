using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Business
{
	public class EntryHeaderContainerCollection : NonPersistentBusinessObjectCollection<EntryHeaderContainer>
	{
		public EntryHeaderContainerCollection(CusEntryHeader cusEntryHeader) : base(cusEntryHeader.Factory)
		{
			Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
			this.cusEntryHeader = cusEntryHeader;
		}
		readonly CusEntryHeader cusEntryHeader;

		public override void Load()
		{
			RemoveAndDeleteAll();

			if (cusEntryHeader.Declaration.ContainersRequired)
			{
				foreach (CusContainer container in cusEntryHeader.Containers)
				{
					Add(new EntryHeaderContainer(cusEntryHeader, container));
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotImplementedException();
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
