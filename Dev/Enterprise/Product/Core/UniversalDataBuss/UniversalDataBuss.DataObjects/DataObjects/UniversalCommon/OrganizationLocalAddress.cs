using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class OrganizationLocalAddress : IDataObject
	{
		[MaxLength(200)]
		public ZString? CompanyName { get; set; }

		[MaxLength(50)]
		public ZString? Address1 { get; set; }

		[MaxLength(50)]
		public ZString? Address2 { get; set; }

		[MaxLength(50)]
		public ZString? City { get; set; }

		[MaxLength(25)]
		public ZString? State { get; set; }

		[MaxLength(20)]
		public ZString? Postcode { get; set; }

		public Language Language { get; set; }
	}
}