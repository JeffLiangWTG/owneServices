using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(CommissionPaymentDocumentSupporter))]
	public class CommissionPaymentDocumentSupporterTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var commissionLine = Factory.NewWithValidTestData<AccCommissionLine>();
			Factory.Save();

			var commissionLineView = Factory.Load<ViewCommissionLine>(commissionLine.PK);
			return new CommissionPayment(Factory, new[] { commissionLineView });
		}
	}
}
