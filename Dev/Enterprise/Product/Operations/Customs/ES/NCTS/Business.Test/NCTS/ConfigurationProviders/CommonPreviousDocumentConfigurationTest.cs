using System;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

public class CommonPreviousDocumentConfigurationTest : EU.NCTS.Business.Testing.CommonPreviousDocumentConfigurationTestCase<CommonPreviousDocumentConfiguration>
{
	protected override Type ExpectedCommonPreviousDocumentDepartureValidationDeciderType => typeof(CommonPreviousDocumentDepartureValidationDecider);
	protected override Type ExpectedCommonPreviousDocumentArrivalValidationDeciderType => typeof(EU.NCTS.Business.CommonPreviousDocumentArrivalValidationDecider);
}
