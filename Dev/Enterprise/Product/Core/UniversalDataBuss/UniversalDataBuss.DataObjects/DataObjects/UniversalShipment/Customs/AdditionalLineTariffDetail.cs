using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Inner)]
	public partial class AdditionalLineTariffDetail : IDataObject, IAddInfoCollectionParent
	{
		public AdditionalLineTariffDetail()
		{
		}

		public AdditionalLineTariffDetail(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		public CodeDescriptionPair5Char Type { get; set; }

		[MaxLength(35)]
		public ZString? Tariff { get; set; }
		public ZDecimal? Value { get; set; }

		public ZDecimal? CustomsQuantity { get; set; }
		public CodeDescriptionPair CustomsQuantityUnit { get; set; }

		public List<AddInfo> AddInfoCollection { get; private set; }
	}
}
