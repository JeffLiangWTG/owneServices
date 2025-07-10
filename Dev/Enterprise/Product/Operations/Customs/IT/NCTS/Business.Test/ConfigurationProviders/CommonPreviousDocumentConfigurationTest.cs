using System;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class CommonPreviousDocumentConfigurationTest : EU.NCTS.Business.Testing.CommonPreviousDocumentConfigurationTestCase<CommonPreviousDocumentConfiguration>
{
	protected override Type ExpectedCommonPreviousDocumentDepartureValidationDeciderType => typeof(CommonPreviousDocumentDepartureValidationDecider);

	protected override Type ExpectedCommonPreviousDocumentArrivalValidationDeciderType => typeof(CommonPreviousDocumentArrivalValidationDecider);
}
