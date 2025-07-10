using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.CreditStatus.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.CreditControlledDocumentsApproval
{
	[TestedType(typeof(CreditStatusBizObjForDocumentApproval))]
	class CreditStatusBizObjForDocumentApprovalTest : CreditStatusBusinessObjectTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CreditStatusBizObjForDocumentApproval(Factory, System.Array.Empty<ZGuid>());
		}
	}
}
