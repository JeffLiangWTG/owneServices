using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.ResourceStrings.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[WrapperTypeName("Pricing Page CFX")]
	[DefaultField("NameAndValue")]
	public class PricingPageCFXWrapper : GenericWrapper
	{
		public PricingPageCFXWrapper(BusinessObjectFactory factory, ZString name, ZDecimal value)
			: base(null, factory)
		{
			Name = name;
			Value = value;
		}

		[CodeStringFinderHint(typeof(PricingPageCFXWrapperCollection), ".ctor")]
		public ZString Name { get; set; }

		public ZDecimal Value { get; set; }

		public ZString NameAndValue
		{
			get { return string.Format("{0} {1:0.00}%", Name, Value); }
		}
	}
}
