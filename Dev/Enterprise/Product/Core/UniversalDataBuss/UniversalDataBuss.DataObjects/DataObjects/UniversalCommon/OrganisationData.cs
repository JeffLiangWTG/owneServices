using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), FlattenedIntoAttributes("Code")]
	public class OrganisationData : ICodeNameDataObject
	{
		public static OrganisationData New(IOrganisationData organisationData)
		{
			return organisationData == null ? null : new OrganisationData()
			{
				Code = organisationData.Code,
				Name = organisationData.FullName
			};
		}

		[MaxLength(12), Mandatory]
		public ZString? Code { get; set; }
		[MaxLength(100)]
		public ZString? Name { get; set; }
	}
}
