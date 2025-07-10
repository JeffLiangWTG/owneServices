using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(LPCOJobComInvLineRefs))]
	public class LPCOJobComInvLineRefsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var jobComInvLineRefs = Factory.New<LPCOJobComInvLineRefs>();
			AssertEquals(JobComInvLineRefsType.Codes.Lpco, jobComInvLineRefs.JG_ReferenceType);
		}

		public void TestJG_ReferenceNumberMaxLength()
		{
			AssertEquals(11, LineRefs.JG_ReferenceNumberInfo.MaxLength);
		}

		public void TestDeleteIfReferenceNumberIsEmpty()
		{
			LPCOJobComInvLineRefs lineRefsNo1 = InvoiceLine.LPCOJobComInvLineRefsCollection.AddNew();
			LPCOJobComInvLineRefs lineRefsNo2 = InvoiceLine.LPCOJobComInvLineRefsCollection.AddNew();
			lineRefsNo2.JG_ReferenceNumber = "E020000001";
			Factory.Save();
			AssertEquals("LPCO with empty reference is Deleted", true, lineRefsNo1.IsDeleted);
			AssertEquals("LPCO with reference stays", false, lineRefsNo2.IsDeleted);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return LineRefs;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var lineRefs = InvoiceLine.LPCOJobComInvLineRefsCollection.AddNew();
			lineRefs.JG_ReferenceNumber = "REF";
			return lineRefs;
		}

		public void TestValidation()
		{
			AssertEquals(typeof(LPCOJobComInvLineRefsValidation), LineRefs.Validation.GetType());
		}

		public void TestInvoiceLine()
		{
			AssertSame(InvoiceLine, LineRefs.InvoiceLine);
		}

		LPCOJobComInvLineRefs LineRefs
		{
			get
			{
				if (lineRefs == null)
				{
					lineRefs = InvoiceLine.LPCOJobComInvLineRefsCollection.AddNew();
				}

				return lineRefs;
			}
		}

		LPCOJobComInvLineRefs lineRefs;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
				}

				return invoiceLine;
			}
		}

		JobComInvoiceLine invoiceLine;
	}
}
