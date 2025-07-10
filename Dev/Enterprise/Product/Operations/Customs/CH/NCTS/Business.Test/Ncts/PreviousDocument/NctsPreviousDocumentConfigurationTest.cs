using System;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsPreviousDocumentConfiguration))]
sealed class NctsPreviousDocumentConfigurationTest : EU.NCTS.Business.Testing.NctsPreviousDocumentConfigurationTestCase<NctsPreviousDocumentConfiguration>
{
	protected override Type ExpectedNctsDeparturePreviousDocumentPhase4ValidationDecider => null;

	protected override Type ExpectedNctsDeparturePreviousDocumentPhase5ValidationDecider => typeof(NctsPreviousDocumentDeparturePhase5ValidationDecider);

	protected override Type ExpectedNctsArrivalPreviousDocumentPhase5ValidationDecider => typeof(NctsPreviousDocumentArrivalPhase5ValidationDecider);
}
