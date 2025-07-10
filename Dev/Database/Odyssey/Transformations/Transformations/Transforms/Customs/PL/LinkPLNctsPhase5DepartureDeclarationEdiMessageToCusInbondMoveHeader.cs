using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.PL
{
	sealed class LinkPLNctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeader : LinkNctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeader
	{
		protected override string CountryCode => "PL";
	}
}
