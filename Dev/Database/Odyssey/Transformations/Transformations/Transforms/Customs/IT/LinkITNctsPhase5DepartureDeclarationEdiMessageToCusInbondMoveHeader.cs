using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.IT
{
	sealed class LinkITNctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeader : LinkNctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeader
	{
		protected override string CountryCode => "IT";
	}
}

