using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(CommissionApprovalRequestDocumentSupporter))]
	public class CommissionApprovalRequestDocumentSupporterTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<AccCommissionApprovalRequest>();
		}
	}
}
