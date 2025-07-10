using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.Accounting.Business.Testing
{
	public class PayablePaymentApprovalWithoutAuthorisationDataTest : PaymentApprovalBaseTest<APPaymentApprovalWithoutAuthorisation>
	{
		protected override IEDocsViaUniversalXmlSupport EDocsViaUniversalXmlSupport() => new PayablePaymentApprovalWithoutAuthorisationData().GetEDocsViaUniversalXmlSupport();
	}
}
