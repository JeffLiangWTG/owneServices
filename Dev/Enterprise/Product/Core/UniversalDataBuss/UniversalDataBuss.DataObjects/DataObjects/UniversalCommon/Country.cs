using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), FlattenedIntoAttributes("Code")]
	public class Country : ICodeNameDataObject
	{
		public static Country New(IRefCountry countryBO)
		{
			return countryBO == null ? null : new Country()
			{
				Code = countryBO.RN_Code,
				Name = countryBO.RN_Desc,
			};
		}

		public static Country NewOrEmpty(IRefCountry countryBO)
		{
			return countryBO == null ? new Country() { Code = ZString.Empty } : new Country()
			{
				Code = countryBO.RN_Code,
				Name = countryBO.RN_Desc,
			};
		}

		[MaxLength(2), Mandatory, CodeMap(Constants.OrgPatternMatchOverrideRelationships.Country)]
		public ZString? Code { get; set; }
		[MaxLength(35)]
		public ZString? Name { get; set; }
	}
}
