using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), FlattenedIntoAttributes("Code")]
	public class State : ICodeNameDataObject
	{
		public static State New(IRefCountryStates countryStateBO)
		{
			return countryStateBO == null ? null : new State()
			{
				Code = countryStateBO.RW_Code,
				Name = countryStateBO.RW_Description,
				Region = countryStateBO.RW_RegionName,
			};
		}
		public static State NewOrEmpty(IRefCountryStates countryStateBO)
		{
			return countryStateBO == null ? new State() { Code = ZString.Empty } : new State()
			{
				Code = countryStateBO.RW_Code,
				Name = countryStateBO.RW_Description,
				Region = countryStateBO.RW_RegionName,
			};
		}

		[MaxLength(3), Mandatory]
		public ZString? Code { get; set; }
		[MaxLength(35)]
		public ZString? Name { get; set; }
		[MaxLength(35)]
		public ZString? Region { get; set; }
	}
}
