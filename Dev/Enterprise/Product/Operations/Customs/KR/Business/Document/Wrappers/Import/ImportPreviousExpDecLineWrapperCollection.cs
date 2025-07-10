using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ImportPreviousExpDecLineWrapperCollection : NonPersistentBusinessObjectCollection<ImportPreviousExpDecLineWrapper>
	{
		public ImportPreviousExpDecLineWrapperCollection(IEnumerable<IImportEntryLine> importEntryLines, BusinessObjectFactory factory)
			: base(factory)
		{
			PopulateElements(importEntryLines);
		}
		void PopulateElements(IEnumerable<IImportEntryLine> importEntryLines)
		{
			var orderedEntryLines = importEntryLines.OrderBy(x => x.EntryLineNo);
			foreach (var entryLine in orderedEntryLines)
			{
				var orderedPrevExpDecLines = entryLine.PreviousExpDecLines.OrderBy(x => x.DeclarationNumber).ThenBy(x => x.EntryLineNo);
				foreach (var prevExpDecLine in orderedPrevExpDecLines)
				{
					Add(new ImportPreviousExpDecLineWrapper(entryLine.EntryLineNo, prevExpDecLine));
				}
			}
		}
		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new System.NotImplementedException();

		protected override bool AllowNewCore => false;
	}
}
