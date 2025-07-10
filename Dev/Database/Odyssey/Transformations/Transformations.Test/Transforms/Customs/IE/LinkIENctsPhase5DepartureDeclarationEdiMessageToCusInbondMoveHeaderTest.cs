using Enterprise.Core.Constants;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.EU;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.IE;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.IE
{
	[TestedType(typeof(LinkIENctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeader))]
	sealed class LinkIENctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeaderTest : LinkNctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeaderTest
	{
		protected override string TargetCountryCode => CountryCodes.Ireland;

		protected override string ForeignCountryCode => CountryCodes.France;

		protected override DataTransformation GetNewTestTransformationInstance() => new LinkIENctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeader();
	}
}
