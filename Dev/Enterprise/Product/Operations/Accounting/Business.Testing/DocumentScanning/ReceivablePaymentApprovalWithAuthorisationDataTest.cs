using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.Accounting.Business.Testing
{
	public class ReceivablePaymentApprovalWithAuthorisationDataTest : PaymentApprovalBaseTest<ARPaymentApprovalWithAuthorisation>
	{
		protected override IEDocsViaUniversalXmlSupport EDocsViaUniversalXmlSupport() => new ReceivablePaymentApprovalWithAuthorisationData().GetEDocsViaUniversalXmlSupport();
	}
}
