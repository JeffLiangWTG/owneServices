using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), FlattenedIntoAttributes("Code")]
	public class Staff : ICodeNameDataObject
	{
		public static Staff New(IUser staffBO)
		{
			return staffBO == null ? null : new Staff()
			{
				Code = staffBO.Initials,
				Name = staffBO.FullName,
			};
		}

		[MaxLength(3), Mandatory]
		public ZString? Code { get; set; }
		[MaxLength(256)]
		public ZString? Name { get; set; }
	}
}
