using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class Import5ULEntryLineWrapperCollection : NonPersistentBusinessObjectCollection<Import5ULEntryLineWrapper>
	{
		public Import5ULEntryLineWrapperCollection(Import5ULHeader header, BusinessObjectFactory factory)
			: base(factory)
		{
			PopulateElements(header);
		}

		void PopulateElements(Import5ULHeader header)
		{
			if (header.EntryLines != null)
			{
				bool isFirstItem = true;
				foreach (var item in header.EntryLines)
				{
					Add(new Import5ULEntryLineWrapper(item, isFirstItem));
					if (isFirstItem)
					{
						isFirstItem = false;
					}
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotImplementedException();

		protected override bool AllowNewCore => false;
	}
}
