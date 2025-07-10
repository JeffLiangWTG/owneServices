using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Outer)]
	public class LocationOfGoods : IDataObject
	{
		public OrganizationAddress OrgAddress { get; set; }
		public LocationOfGoodsType? Type { get; set; }
		public CodeDescriptionPair2Char SubType { get; set; }
		public CodeDescriptionPair1Char Qualifier { get; set; }
		[MaxLength(10)]
		public ZString? AdditionalIdentifier { get; set; }
		[MaxLength(10)]
		public ZString? CustomsOffice { get; set; }
		[MaxLength(35)]
		public ZString? AuthorizationNumber { get; set; }
		public Contact Contact { get; set; }
		public CodeDescriptionPair1Char LocationType { get; set; }
	}
}
