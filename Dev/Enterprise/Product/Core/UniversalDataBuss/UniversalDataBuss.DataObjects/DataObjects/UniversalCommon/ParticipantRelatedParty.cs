using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class ParticipantRelatedParty : IDataObject
	{
		[Mandatory]
		public OrganizationAddress Party { get; set; }

		[MaxLength(30)]
		public ZString? Relation { get; set; }

		public UNLOCO Location { get; set; }

		public ZBool? IsActive { get; set; }
		public ZBool? IsSubscribed { get; set; }

		[Mandatory]
		public RelatedPartyType? RelatedPartyType { get; set; }
	}

	public enum RelatedPartyType
	{
		Contact,
		Organization,
		Email,
	}
}
