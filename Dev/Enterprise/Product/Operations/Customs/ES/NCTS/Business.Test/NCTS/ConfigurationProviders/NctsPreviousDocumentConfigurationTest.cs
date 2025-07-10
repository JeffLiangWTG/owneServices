using System;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

[TestedType(typeof(NctsPreviousDocumentConfiguration))]
sealed class NctsPreviousDocumentConfigurationTest : EU.NCTS.Business.Testing.NctsPreviousDocumentConfigurationTestCase<NctsPreviousDocumentConfiguration>
{
	protected override Type ExpectedNctsDeparturePreviousDocumentPhase4ValidationDecider => null;
	protected override Type ExpectedNctsDeparturePreviousDocumentPhase5ValidationDecider => typeof(NctsPreviousDocumentDeparturePhase5ValidationDecider);
	protected override Type ExpectedNctsArrivalPreviousDocumentPhase5ValidationDecider => typeof(EU.NCTS.Business.NctsPreviousDocumentArrivalPhase5ValidationDecider);
}
