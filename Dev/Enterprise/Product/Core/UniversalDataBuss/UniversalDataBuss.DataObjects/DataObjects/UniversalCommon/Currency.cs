using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), FlattenedIntoAttributes("Code")]
	public class Currency : ICodeDescriptionDataObject
	{
		public static Currency New(IRefCurrency currencyBO)
		{
			return currencyBO == null ? null : new Currency()
			{
				Code = currencyBO.RX_Code,
				Description = currencyBO.RX_Desc,
			};
		}

		[MaxLength(3), Mandatory, CodeMap(Constants.OrgPatternMatchOverrideRelationships.Currency)]
		public ZString? Code { get; set; }
		[MaxLength(35)]
		public ZString? Description { get; set; }
	}
}

