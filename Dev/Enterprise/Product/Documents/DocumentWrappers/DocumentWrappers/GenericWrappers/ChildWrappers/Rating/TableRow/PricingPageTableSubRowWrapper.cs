using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.ResourceStrings.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[WrapperTypeName("Pricing Page Table Sub Row")]
	public class PricingPageTableSubRowWrapper : GenericWrapper
	{
		public PricingPageTableSubRowWrapper(BusinessObjectFactory factory)
			: base(null, factory) { }

		public ZInt Index { get; set; }
		public ZInt CaptionGroup { get; set; }
		public CurrencyWrapper Currency { get; set; }
		public CodeAndDescriptionWrapper Charge { get; set; }
		public ZBool UseOnlyActualWeightMeasure { get; set; }
		public RatingConversionFactorWrapper ConversionFactor { get; set; }

		[CodeStringFinderHint(typeof(CompactTableStrategy), "CreateTableSubRow")]
		public ZString Split { get; set; }

		public PricingPageColumnWrapperCollection Columns
		{
			get { return columns ?? (columns = new PricingPageColumnWrapperCollection(Factory)); }
		}
		PricingPageColumnWrapperCollection columns;
	}
}
