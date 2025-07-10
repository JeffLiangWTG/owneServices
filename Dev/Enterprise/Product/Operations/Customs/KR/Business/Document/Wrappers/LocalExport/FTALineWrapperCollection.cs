using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class FTALineWrapperCollection : NonPersistentBusinessObjectCollection<FTALineWrapper>
	{
		public FTALineWrapperCollection(IEnumerable<IImportFTALine> entryLines, BusinessObjectFactory factory)
		{
			PopulateElements(entryLines, factory);
		}

		void PopulateElements(IEnumerable<IImportFTALine> entryLines, BusinessObjectFactory factory)
		{
			FTALineWrapper wrapper = null;

			if (entryLines != null)
			{
				var orderedEntryLines = entryLines.OrderBy(x => x.EntryLineNo);
				foreach (var item in orderedEntryLines)
				{
					if (wrapper == null)
					{
						wrapper = new FTALineWrapper(factory);
					}

					if (item.SequenceNo % 3 == 1)
					{
						wrapper.FirstLine = item;
					}
					else if (item.SequenceNo % 3 == 2)
					{
						wrapper.SecondLine = item;
					}
					else if (item.SequenceNo % 3 == 0)
					{
						wrapper.ThirdLine = item;
					}

					if (wrapper != null && wrapper.ThirdLine != null)
					{
						Add(wrapper);
						wrapper = null;
					}
				}

				if (wrapper != null)
				{
					if (wrapper.SecondLine == null)
					{
						wrapper.SecondLine = new ImportFTALine();
					}

					if (wrapper.ThirdLine == null)
					{
						wrapper.ThirdLine = new ImportFTALine();
					}

					Add(wrapper);
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotImplementedException();

		protected override bool AllowNewCore => false;
	}
}
