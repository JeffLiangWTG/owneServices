using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.BR.DataTransfer.Universal.Testing
{
	public class BRDataObjectWriterHelperTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestJobComInvoiceLineCustomsReferences()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var helper = new BRDataObjectWriterHelper(declaration.Factory);
			var result = helper.GetAdditionalCustomsReferenceDataFor(invoiceLine, null);
			Assert(!(result?.Any(reference => reference.Type.GetCodeAsUpperCase() == Constants.JobComInvLineRefsType.Codes.LPCO) ?? false));
			var lpco = invoiceLine.LPCOJobComInvLineRefsCollection.AddNew();
			lpco.JG_ReferenceNumber = "LPCO123";
			invoiceLine.LPCOJobComInvLineRefsCollection.AddNew();
			result = helper.GetAdditionalCustomsReferenceDataFor(invoiceLine, null);
			AssertEquals(1, result.Count());
			AssertReference(result, Constants.JobComInvLineRefsType.Codes.LPCO, Constants.JobComInvLineRefsType.Descriptions.LPCO, "LPCO123");
		}

		void AssertReference(IEnumerable<CustomsReference> result, ZString type, ZString description, ZString reference)
		{
			Assert(result.Any(x => x.Type.Code.Equals(type) && x.Type.Description.Equals(description) && x.Reference.Equals(reference)));
		}
	}
}
