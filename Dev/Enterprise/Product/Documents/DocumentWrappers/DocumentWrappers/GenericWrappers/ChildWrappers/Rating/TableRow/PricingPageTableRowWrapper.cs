using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[WrapperTypeName("Pricing Page Table Row")]
	public class PricingPageTableRowWrapper : GenericWrapper
	{
		public PricingPageTableRowWrapper(RateEntry[] entries, BusinessObjectFactory factory)
			: base(null, factory)
		{
			this.rowEntries = entries;
		}

		public ZInt Index { get; set; }

		public PricingPageTableSubRowWrapperCollection SubRows
		{
			get { return subRows ?? (subRows = new PricingPageTableSubRowWrapperCollection(Factory)); }
		}
		PricingPageTableSubRowWrapperCollection subRows;

		public PricingPageLineWrapperCollection OtherCharges
		{
			get { return otherCharges ?? (otherCharges = new PricingPageLineWrapperCollection(Factory)); }
		}
		PricingPageLineWrapperCollection otherCharges;

		public RatingEntryWrapperCollection Entries
		{
			get
			{
				if (entries == null)
				{
					entries = new RatingEntryWrapperCollection(rowEntries, Factory);
				}

				return entries;
			}
		}
		RatingEntryWrapperCollection entries;

		#region Implementation

		readonly RateEntry[] rowEntries;

		#endregion
	}
}
