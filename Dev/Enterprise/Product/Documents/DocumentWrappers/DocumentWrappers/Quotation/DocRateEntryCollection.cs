using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocRateEntryCollection : DocumentWrapperCollection<DocRateEntry>
	{
		public DocRateEntryCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public DocRateEntryCollection(DocTableQuotation tableQuotation, IEnumerable<RateEntry> entryCollection, BusinessObjectFactory factory)
			: base(factory)
		{
			Parent = tableQuotation;

			foreach (RateEntry entry in DocRateEntry.UniqueFreightEntriesIgnoringMode(entryCollection))
			{
				Add(DocRateEntry.New(entry, factory));
			}

			Sort(new Quotation.EntryComparer());
		}

		public DocTableQuotation Parent { get; protected set; }
	}
}
