using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class OrganizationContact : IDataObject
	{
		[MaxLength(50), Mandatory]
		public ZString? FullName { get; set; }
		[MaxLength(20)]
		public ZString? Phone { get; set; }
		[MaxLength(60)]
		public ZString? Email { get; set; }
	}
}

