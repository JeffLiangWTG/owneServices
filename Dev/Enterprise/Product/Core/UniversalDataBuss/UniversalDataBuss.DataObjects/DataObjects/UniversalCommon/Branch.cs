using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.UniversalDataBuss.DataObjects
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), FlattenedIntoAttributes("Code")]
	public class Branch : ICodeNameDataObject
	{
		public static Branch New(IBranch branchBO)
		{
			return branchBO == null ? null : new Branch()
			{
				Code = branchBO.Code,
				Name = branchBO.Name,
			};
		}

		[MaxLength(3), Mandatory]
		public ZString? Code { get; set; }
		[MaxLength(50)]
		public ZString? Name { get; set; }
	}
}
