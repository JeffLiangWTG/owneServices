using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.BE
{
	sealed class LinkBENctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeader : LinkNctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeader
	{
		protected override string CountryCode => "BE";
	}
}
