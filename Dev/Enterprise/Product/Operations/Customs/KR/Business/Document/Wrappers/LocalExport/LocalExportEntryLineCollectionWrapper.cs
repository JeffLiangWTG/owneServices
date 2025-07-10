using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExportEntryLineCollectionWrapper : NonPersistentBusinessObjectCollection<LocalExportEntryLineWrapper>
	{
		public LocalExportEntryLineCollectionWrapper(IEnumerable<ILocalExportEntryLine> localExportEntryLines, BusinessObjectFactory factory)
			: base(factory)
		{
			PopulateElements(factory, localExportEntryLines);
		}

		void PopulateElements(BusinessObjectFactory factory, IEnumerable<ILocalExportEntryLine> localExportEntryLines)
		{
			if (localExportEntryLines != null)
			{
				foreach (var item in localExportEntryLines)
				{
					Add(new LocalExportEntryLineWrapper(item, factory));
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotImplementedException();

		protected override bool AllowNewCore => false;
	}
}
