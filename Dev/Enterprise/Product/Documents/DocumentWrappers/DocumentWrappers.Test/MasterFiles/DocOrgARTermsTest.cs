using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocOrgARTerms))]
	public class DocOrgARTermsTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocOrgARTerms.New(orgARTermsData, Factory)
			};
		}

		OrgARTerms orgARTermsData;
		protected override void SetUp()
		{
			orgARTermsData = Factory.New<OrgARTerms>();
			base.SetUp();
		}

		public void TestAgreedPaymentMethod()
		{
			orgARTermsData.PY_AgreedPaymentMethod = "CRD";
			AssertEquals("AgreedPaymentMethod", "CRD", ((DocOrgARTerms)Wrappers[0]).AgreedPaymentMethod);
		}

		public void TestDirection()
		{
			orgARTermsData.PY_Direction = "ICO";
			AssertEquals("Direction", "ICO", ((DocOrgARTerms)Wrappers[0]).Direction);
		}

		public void TestInvoiceClass()
		{
			orgARTermsData.PY_InvoiceClass = "DSB";
			AssertEquals("InvoiceClass", "DSB", ((DocOrgARTerms)Wrappers[0]).InvoiceClass);
		}

		public void TestInvoiceTerm()
		{
			orgARTermsData.PY_InvoiceTerm = "INV";
			AssertEquals("InvoiceTerm", "INV", ((DocOrgARTerms)Wrappers[0]).InvoiceTerm);
		}

		public void TestJobType()
		{
			orgARTermsData.PY_JobType = "AST";
			AssertEquals("JobType", "AST", ((DocOrgARTerms)Wrappers[0]).JobType);
		}

		public void TestTransportMode()
		{
			orgARTermsData.PY_TransportMode = "SEA";
			AssertEquals("TransportMode", "SEA", ((DocOrgARTerms)Wrappers[0]).TransportMode);
		}

		public void TestInvoiceDays()
		{
			ZByte obj = new ZByte(1);
			orgARTermsData.PY_InvoiceDays = obj;
			AssertEquals("InvoiceDays", obj, ((DocOrgARTerms)Wrappers[0]).InvoiceDays);
		}

		public void TestDepartment()
		{
			orgARTermsData.PY_GE_Department = Env.CurrentDepartmentPK;
			AssertEquals("Department", Env.CurrentDepartmentPK, ((DocOrgARTerms)Wrappers[0]).Department.DepartmentPK);
		}

		public void TestBranch()
		{
			orgARTermsData.PY_GB_Branch = Env.CurrentBranchPK;
			AssertEquals("Branch", Env.CurrentBranchPK, ((DocOrgARTerms)Wrappers[0]).Branch.Branch);
		}
	}
}
