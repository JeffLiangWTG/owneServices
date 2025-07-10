using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers.Quotation
{
	class EntryComparer : IComparer<RateEntry>, IComparer<DocRateEntry>, IComparer<ILocation>, IComparer<Directions>, System.Collections.IComparer
	{
		public int Compare(RateEntry entry1, RateEntry entry2)
		{
			int result = Compare(((IImportExport)entry1).JobDirection, ((IImportExport)entry2).JobDirection);
			if (result == 0)
			{
				if (entry1.IsExport())
				{
					result = Compare(entry1.Destination(), entry2.Destination());
					if (result == 0)
					{
						result = Compare(entry1.Origin(), entry2.Origin());
					}
				}
				else
				{
					result = Compare(entry1.Origin(), entry2.Origin());
					if (result == 0)
					{
						result = Compare(entry1.Destination(), entry2.Destination());
					}
				}
			}

			return result;
		}

		public int Compare(DocRateEntry docEntry1, DocRateEntry docEntry2)
		{
			return Compare(docEntry1.Entry, docEntry2.Entry);
		}

		public int Compare(ILocation location1, ILocation location2)
		{
			int result = 0;
			if (location1 == null && location2 == null)
			{
				result = 0;
			}
			else if (location1 == null)
			{
				result = -1;
			}
			else if (location2 == null)
			{
				result = 1;
			}
			else
			{
				if (!(location1 is RefCountry && location2 is RefCountry))
				{
					result = Compare(location1.Country, location2.Country);
				}
				if (result == 0)
				{
					result = location1.Description.CompareTo(location2.Description);
				}
			}

			return result;
		}

		public int Compare(Directions state1, Directions state2)
		{
			return ((int)state1).CompareTo(((int)state2));
		}

		#region IComparer Members

		int System.Collections.IComparer.Compare(object x, object y)
		{
			if (x is RateEntry && y is RateEntry)
			{
				return Compare((RateEntry)x, (RateEntry)y);
			}
			if (x is DocRateEntry && y is DocRateEntry)
			{
				return Compare((DocRateEntry)x, (DocRateEntry)y);
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		#endregion
	}
}
