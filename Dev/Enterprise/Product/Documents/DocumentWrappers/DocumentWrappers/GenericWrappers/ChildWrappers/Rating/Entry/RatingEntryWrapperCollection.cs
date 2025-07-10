using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class RatingEntryWrapperCollection : GenericWrapperCollection<RatingEntryWrapper>
	{
		public RatingEntryWrapperCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public RatingEntryWrapperCollection(IEnumerable<RateEntry> entries, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (RateEntry entry in entries)
			{
				Add(new RatingEntryWrapper(entry, factory));
			}
		}
	}
}
