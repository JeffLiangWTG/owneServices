using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.IE
{
	sealed class LinkIENctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeader : LinkNctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeader
	{
		protected override string CountryCode => "IE";
	}
}
