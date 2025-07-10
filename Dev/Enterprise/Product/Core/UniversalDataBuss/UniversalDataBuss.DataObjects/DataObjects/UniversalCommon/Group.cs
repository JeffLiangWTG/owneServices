using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), FlattenedIntoAttributes("Code")]
	public class Group : ICodeNameDataObject
	{
		public static Group New(IGlbGroup groupBusinessObject)
		{
			return groupBusinessObject == null ? null : new Group
			{
				Code = groupBusinessObject.GG_Code,
				Name = groupBusinessObject.GG_Desc,
			};
		}

		[MaxLength(15), Mandatory]
		public ZString? Code { get; set; }
		[MaxLength(64)]
		public ZString? Name { get; set; }
	}
}
