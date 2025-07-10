using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.Accounting.Business.Testing
{
	public class PayablePaymentApprovalWithAuthorisationDataTest : PaymentApprovalBaseTest<APPaymentApprovalWithAuthorisation>
	{
		protected override IEDocsViaUniversalXmlSupport EDocsViaUniversalXmlSupport() => new PayablePaymentApprovalWithAuthorisationData().GetEDocsViaUniversalXmlSupport();
	}
}
