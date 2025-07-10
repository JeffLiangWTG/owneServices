using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CertificateOfOrigin))]
	sealed class CertificateOfOriginTest : CusSupportingInfoTest<CertificateOfOrigin>
	{
		protected override BusinessObject GetNewBusinessObject() => certificateOfOrigin;

		protected override IEnumerable<CertificateOfOrigin> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var invoiceLine = factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			var certificateOfOriginCollection = new CertificateOfOriginCollection(invoiceLine);
			yield return certificateOfOriginCollection.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var businessObj = (CertificateOfOrigin)base.GetBusinessObjectForFetchForLoad();
			businessObj.CSI_Type = CusSupportingInfoTypeList.Codes.CertificateOfOrigin;
			return businessObj;
		}

		public void TestValidation()
		{
			var supportingInfo = Factory.New<CertificateOfOrigin>();
			AssertEquals(typeof(CertificateOfOriginValidation), supportingInfo.Validation.GetType());
		}

		protected override void SetUp()
		{
			invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			var certificateOfOriginCollection = new CertificateOfOriginCollection(invoiceLine);
			certificateOfOrigin = certificateOfOriginCollection.AddNew();
		}
		CertificateOfOrigin certificateOfOrigin;
		JobComInvoiceLine invoiceLine;
	}
}
