using Enterprise.Core.Constants;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.EU;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.BE;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.BE
{
	[TestedType(typeof(LinkBENctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeader))]
	sealed class LinkBENctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeaderTest : LinkNctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeaderTest
	{
		protected override string TargetCountryCode => CountryCodes.Belgium;

		protected override string ForeignCountryCode => CountryCodes.France;

		protected override DataTransformation GetNewTestTransformationInstance() => new LinkBENctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeader();
	}
}
