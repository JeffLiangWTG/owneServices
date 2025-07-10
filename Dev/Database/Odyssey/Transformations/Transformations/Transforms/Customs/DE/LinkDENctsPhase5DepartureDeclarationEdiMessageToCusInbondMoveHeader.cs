using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.DE;

sealed class LinkDENctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeader : LinkNctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeader
{
	protected override string CountryCode => "DE";
}
