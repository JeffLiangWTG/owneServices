using Enterprise.Core.Constants;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.EU;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.IT;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.IT
{
	[TestedType(typeof(LinkITNctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeader))]
	sealed class LinkITNctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeaderTest : LinkNctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeaderTest
	{
		protected override string TargetCountryCode => CountryCodes.Italy;

		protected override string ForeignCountryCode => CountryCodes.France;

		protected override DataTransformation GetNewTestTransformationInstance() => new LinkITNctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeader();
	}
}
