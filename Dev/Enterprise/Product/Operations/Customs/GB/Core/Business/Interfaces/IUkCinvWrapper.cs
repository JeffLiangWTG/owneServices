using CargoWise.Types;

namespace Enterprise.Customs.GB.Business
{
	public interface IUkCinvWrapper
	{
		ZString MasterUniqueConsignmentReference { get; }
		ZString LocationOfGoods { get; }
		ZString ShedCode { get; }
		ZString DeclarationUniqueConsignmentReference { get; }
		ZString DeclarationUniqueConsignmentReferencePartSuffix { get; }
		ZDateTime DateAndTimeTheGoodsWillBeAvailableForInspectionAtLCPPremises { get; }
		ZDateTime DateAndTimeTheGoodsWillBeLeavingLCPPremises { get; }
		ZString TransportModeAtTheBorderBox25 { get; }
		ZString TransportNationalityAtTheBorderBox21 { get; }
		ZString TransportIdentityAtTheBorderBox21 { get; }
		ZString MasterOpt { get; }
		ZString MovementReference { get; }

		ZString CDSDeclarationUniqueConsignmentReference { get; }
		ZString CDSDeclarationUniqueConsignmentReferencePartSuffix { get; }
	}

	public interface IUkCinvWrapperConsol : IUkCinvWrapper
	{
		ZString OriginAirport { get; }
		ZString DestinationAirport { get; }
		ZString DestinationCountry { get; }
		ZBool PartMovement { get; }
		ZBool UseAntiSmugglingTrptId { get; }
		ZString CommunityTransitStatus { get; }
	}
}
