using Enterprise.Core.Constants;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.EU;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.GB;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.GB
{
	[TestedType(typeof(LinkGBNctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeader))]
	sealed class LinkGBNctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeaderTest : LinkNctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeaderTest
	{
		protected override string TargetCountryCode => CountryCodes.UnitedKingdom;

		protected override string ForeignCountryCode => CountryCodes.France;

		protected override DataTransformation GetNewTestTransformationInstance() => new LinkGBNctsPhase5DepartureDeclarationEdiMessageToCusInbondMoveHeader();
	}
}
