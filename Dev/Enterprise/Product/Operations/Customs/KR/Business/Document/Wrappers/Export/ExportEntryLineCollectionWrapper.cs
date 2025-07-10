using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ExportEntryLineCollectionWrapper : NonPersistentBusinessObjectCollection<ExportEntryLineWrapper>
	{
		public ExportEntryLineCollectionWrapper(IEnumerable<IExportEntryLine> exportEntryLines, ZDecimal uSDRate, BusinessObjectFactory factory)
			: base(factory)
		{
			PopulateElements(factory, exportEntryLines, uSDRate);
		}

		void PopulateElements(BusinessObjectFactory factory, IEnumerable<IExportEntryLine> exportEntryLines, ZDecimal uSDRate)
		{
			if (exportEntryLines != null)
			{
				bool isFirstItem = true;
				foreach (var item in exportEntryLines)
				{
					Add(new ExportEntryLineWrapper(item, uSDRate, isFirstItem, factory));
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
