using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[WrapperTypeName("Compound Line")]
	public class PricingPageCompoundLineWrapper : GenericWrapper
	{
		public PricingPageCompoundLineWrapper(BusinessObjectFactory factory)
			: base(null, factory) { }

		public ZString Label { get; set; }
		public ZString SubLabel { get; set; }
		public ZInt LabelOrdinal { get; set; }
		public ZInt SubLabelOrdinal { get; set; }
		public PricingPageWrapper Page { get; set; }
		public PricingPageLineWrapper RateLine { get; set; }
		public PricingPageTableRowWrapper Row { get; set; }
		public PricingPageTableSubRowWrapper SubRow { get; set; }
	}
}
