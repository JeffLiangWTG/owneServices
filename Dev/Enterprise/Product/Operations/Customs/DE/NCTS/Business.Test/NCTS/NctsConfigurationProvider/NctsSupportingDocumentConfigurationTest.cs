using System;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsSupportingDocumentConfiguration))]
	sealed class NctsSupportingDocumentConfigurationTest : EU.NCTS.Business.Testing.NctsSupportingDocumentConfigurationTestCase<NctsSupportingDocumentConfiguration>
	{
		protected override Type ExpectedNctsDepartureSupportingDocumentPhase4ValidationDecider => null;
		protected override Type ExpectedNctsDepartureSupportingDocumentPhase5ValidationDecider => typeof(NctsSupportingDocumentDeparturePhase5ValidationDecider);
		protected override Type ExpectedNctsArrivalSupportingDocumentPhase5ValidationDecider => null;
	}
}
