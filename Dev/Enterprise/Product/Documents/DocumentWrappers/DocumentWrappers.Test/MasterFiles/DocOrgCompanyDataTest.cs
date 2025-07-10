using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocOrgCompanyData))]
	public class DocOrgCompanyDataTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocOrgCompanyData.New(orgCompanyData, Factory)
			};
		}

		OrgCompanyData orgCompanyData;
		protected override void SetUp()
		{
			orgCompanyData = Factory.New<OrgCompanyData>();
			base.SetUp();
		}

		public void TestGetPaymentMethodFromARTerms()
		{
			orgCompanyData.ARTerms.DeleteAll();
			var term1 = orgCompanyData.ARTerms.AddNew();
			term1.PY_InvoiceClass = "ALL";
			term1.PY_AgreedPaymentMethod = "CHK";
			var term2 = orgCompanyData.ARTerms.AddNew();
			term2.PY_InvoiceClass = "DSB";
			term2.PY_AgreedPaymentMethod = "CRQ";

			AssertEquals("Business Check", ((DocOrgCompanyData)Wrappers[0]).GetPaymentMethodFromARTerms("ALL"));
			AssertEquals("Collection Request", ((DocOrgCompanyData)Wrappers[0]).GetPaymentMethodFromARTerms("DSB"));
		}

		public void TestOB_RX_NKARDDefltCurrency()
		{
			AssertEquals("OB_RX_NKARDDefltCurrency", orgCompanyData.OB_RX_NKARDDefltCurrency, ((DocOrgCompanyData)Wrappers[0]).OB_RX_NKARDDefltCurrency);
		}

		public void TestOB_AROnCreditHold()
		{
			orgCompanyData.OB_AROnCreditHold = true;
			Assert("OB_AROnCreditHold", ((DocOrgCompanyData)Wrappers[0]).OB_AROnCreditHold);
		}

		public void TestOB_ARUseSettlementGroupCreditLimit()
		{
			orgCompanyData.OB_ARUseSettlementGroupCreditLimit = true;
			Assert("OB_ARUseSettlementGroupCreditLimit", ((DocOrgCompanyData)Wrappers[0]).OB_ARUseSettlementGroupCreditLimit);
		}

		public void TestOB_GC()
		{
			AssertEquals("OB_GC", orgCompanyData.OB_GC, ((DocOrgCompanyData)Wrappers[0]).OB_GC);
		}

		public void TestARTerms()
		{
			AssertEquals("ARTerms.Count", orgCompanyData.ARTerms.Count, ((DocOrgCompanyData)Wrappers[0]).ARTerms.Count);
			AssertEquals("ARTerms.InvoiceClass", orgCompanyData.ARTerms[0].PY_InvoiceClass, ((DocOrgCompanyData)Wrappers[0]).ARTerms[0].InvoiceClass);
			AssertEquals("ARTerms.AgreedPaymentMethod", orgCompanyData.ARTerms[0].PY_AgreedPaymentMethod, ((DocOrgCompanyData)Wrappers[0]).ARTerms[0].AgreedPaymentMethod);
		}
	}
}
