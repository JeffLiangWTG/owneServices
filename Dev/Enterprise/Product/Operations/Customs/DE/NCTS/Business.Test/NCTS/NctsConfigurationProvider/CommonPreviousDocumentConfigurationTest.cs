using System;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business;

[TestedType(typeof(CommonPreviousDocumentConfiguration))]
public sealed class CommonPreviousDocumentConfigurationTest : CommonPreviousDocumentConfigurationTestCase<CommonPreviousDocumentConfiguration>
{
	protected override Type ExpectedCommonPreviousDocumentDepartureValidationDeciderType => typeof(CommonPreviousDocumentDepartureValidationDecider);
	protected override Type ExpectedCommonPreviousDocumentArrivalValidationDeciderType => typeof(EU.NCTS.Business.CommonPreviousDocumentArrivalValidationDecider);
}
