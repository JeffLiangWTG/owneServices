using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CertificateOfOrigin))]
	public class CertificateOfOriginTest : Customs.Business.Testing.CusSupportingInfoTest<CertificateOfOrigin>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().CertificateOfOriginCollection.AddNew();
		}

		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<CertificateOfOrigin>();
			AssertEquals(CusSupportingInfoTypeList.Codes.CertificateOfOrigin, supporting.CSI_Type);
			AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, supporting.CSI_ParentTableCode);
		}

		protected override IEnumerable<CertificateOfOrigin> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var certificateOfOriginCusSupporting = factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().CertificateOfOriginCollection.AddNew();
			certificateOfOriginCusSupporting.CSI_SubType = CertificateTypeList.Codes.CCROM;
			certificateOfOriginCusSupporting.CSI_ReferenceNumber = "1";
			certificateOfOriginCusSupporting.CSI_Quantity = 81;
			certificateOfOriginCusSupporting.CSI_ReferenceNumber2 = "2";
			certificateOfOriginCusSupporting.CSI_ItemNumber = 42;
			certificateOfOriginCusSupporting.CSI_Quantity2 = 12;
			yield return certificateOfOriginCusSupporting;
		}

		public void TestCertificateOfOriginCusSupportingSettingValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "56049000";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			var certificateOfOrigin = invoiceLine.CertificateOfOriginCollection.AddNew();
			certificateOfOrigin.CSI_SubType = CertificateTypeList.Codes.CCROM;
			certificateOfOrigin.CSI_ReferenceNumber = "1234567890123456789012345";
			certificateOfOrigin.CSI_Quantity = 123456789012345.67890m;

			AssertEquals(CertificateTypeList.Codes.CCROM, certificateOfOrigin.CSI_SubType);
			AssertEquals("1234567890123456789012345", certificateOfOrigin.CSI_ReferenceNumber);
			AssertEquals(123456789012345.67890m, certificateOfOrigin.CSI_Quantity);
			AssertEquals("56049000", certificateOfOrigin.TariffCode);
			AssertEquals("KG", certificateOfOrigin.UQ);
		}
	}
}
