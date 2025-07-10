using System;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

[TestedType(typeof(CommonPreviousDocumentConfiguration))]
public sealed class CommonPreviousDocumentConfigurationTest : CommonPreviousDocumentConfigurationTestCase<CommonPreviousDocumentConfiguration>
{
	protected override Type ExpectedCommonPreviousDocumentDepartureValidationDeciderType => typeof(CommonPreviousDocumentDepartureValidationDecider);
	protected override Type ExpectedCommonPreviousDocumentArrivalValidationDeciderType => typeof(CommonPreviousDocumentArrivalValidationDecider);
}
