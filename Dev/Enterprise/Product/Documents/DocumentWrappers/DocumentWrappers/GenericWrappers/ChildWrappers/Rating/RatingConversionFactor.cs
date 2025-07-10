using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("Metric")]
	[WrapperTypeName("Rating Conversion Factor")]
	public class RatingConversionFactorWrapper : GenericWrapper
	{
		public RatingConversionFactorWrapper(ConversionFactor conversionFactor, BusinessObjectFactory factory)
			: base(null, factory)
		{
			Factor = conversionFactor;
		}

		ConversionFactor Factor { get; set; }

		public ZString Metric
		{
			get { return Factor.IsValid ? Factor.ToShortString() : string.Empty; }
		}
	}
}
