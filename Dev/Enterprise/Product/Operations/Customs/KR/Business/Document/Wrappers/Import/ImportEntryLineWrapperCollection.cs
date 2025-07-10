using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ImportEntryLineWrapperCollection : NonPersistentBusinessObjectCollection<ImportEntryLineWrapper>
	{
		public ImportEntryLineWrapperCollection(IEnumerable<IImportEntryLine> importEntryLines, ZDecimal uSDRate, ZString entryLinePackTitle, BusinessObjectFactory factory)
			: base(factory)
		{
			PopulateElements(importEntryLines, uSDRate, entryLinePackTitle);
		}

		void PopulateElements(IEnumerable<IImportEntryLine> importEntryLines, ZDecimal uSDRate, ZString entryLinePackTitle)
		{
			if (importEntryLines != null)
			{
				bool isFirstItem = true;
				foreach (var item in importEntryLines)
				{
					Add(new ImportEntryLineWrapper(item, uSDRate, entryLinePackTitle, isFirstItem));
					if (isFirstItem)
					{
						isFirstItem = false;
					}
				}
			}
		}

		public ZString FormattedTariffFor5BA => this.Cast<ImportEntryLineWrapper>().FirstOrDefault().EntryLine.HSCode;
		public ZString HSDescriptionFor5BA => this.Cast<ImportEntryLineWrapper>().FirstOrDefault().EntryLine.HSDescription;
		public ZString ModelNameFor5BA => this.Cast<ImportEntryLineWrapper>().FirstOrDefault().EntryLine.ModelName;

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotImplementedException();

		protected override bool AllowNewCore => false;
	}
}
