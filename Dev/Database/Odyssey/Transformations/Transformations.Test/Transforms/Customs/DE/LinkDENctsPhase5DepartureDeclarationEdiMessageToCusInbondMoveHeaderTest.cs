using Enterprise.Core.Constants;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.EU;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.DE;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.DE
{
	[TestedType(typeof(LinkDENctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeader))]
	sealed class LinkDENctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeaderTest : LinkNctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeaderTest
	{
		protected override string TargetCountryCode => CountryCodes.Germany;

		protected override string ForeignCountryCode => CountryCodes.France;

		protected override DataTransformation GetNewTestTransformationInstance() => new LinkDENctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeader();
	}
}
