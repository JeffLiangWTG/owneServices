using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), FlattenedIntoAttributes("Code")]
	public class UNLOCO : ICodeDataObject
	{
		public static UNLOCO New(IRefUNLOCO locationBO)
		{
			return locationBO == null ? null : new UNLOCO()
			{
				Code = locationBO.RL_Code,
				Name = locationBO.RL_PortName,
			};
		}

		[MaxLength(5), Mandatory, CodeMap(Constants.OrgPatternMatchOverrideRelationships.Port)]
		public ZString? Code { get; set; }
		[MaxLength(35)]
		public ZString? Name { get; set; }
	}
}

