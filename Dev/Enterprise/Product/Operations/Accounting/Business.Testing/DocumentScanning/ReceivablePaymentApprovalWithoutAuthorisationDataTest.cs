using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.Accounting.Business.Testing
{
	public class ReceivablePaymentApprovalWithoutAuthorisationDataTest : PaymentApprovalBaseTest<ARPaymentApprovalWithoutAuthorisation>
	{
		protected override IEDocsViaUniversalXmlSupport EDocsViaUniversalXmlSupport() => new ReceivablePaymentApprovalWithoutAuthorisationData().GetEDocsViaUniversalXmlSupport();
	}
}
