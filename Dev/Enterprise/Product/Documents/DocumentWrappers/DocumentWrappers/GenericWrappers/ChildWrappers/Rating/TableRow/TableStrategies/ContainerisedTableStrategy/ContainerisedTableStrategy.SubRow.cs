using CargoWise.Types;
using Enterprise.Rating.Business.DocumentPrinting.DocAmount;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	partial class ContainerisedTableStrategy
	{
		sealed class SubRow
		{
			public SubRow(ZString currency, DocAmount[] values, ConversionFactor conversionFactor)
			{
				Currency = currency;
				Values = values;
				ConversionFactor = conversionFactor;
			}

			public ZString Currency { get; }
			public DocAmount[] Values { get; }
			public ConversionFactor ConversionFactor { get; }
		}
	}
}
