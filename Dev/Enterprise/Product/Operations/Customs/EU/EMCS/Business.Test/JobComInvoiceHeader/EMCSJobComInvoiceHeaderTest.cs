using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSJobComInvoiceHeader))]
	class EMCSJobComInvoiceHeaderTest : BaseJobComInvoiceHeaderAbstractTest<EMCSJobComInvoiceHeader, EMCSJobComInvoiceLine>
	{
		protected override Type ExpectedMetadataType => typeof(Metadata.Business.BaseJobComInvoiceHeader);

		public void TestSupplier_OwnerDocumentaryAddressOrganisation()
		{
			var supplier1 = OrgHeader.New(Factory);
			supplier1.OH_Code = "SUP1";
			supplier1.OH_FullName = "SUPPLIER NAME 1";

			var supplier2 = OrgHeader.New(Factory);
			supplier2.OH_Code = "SUP2";
			supplier2.OH_FullName = "SUPPLIER NAME 2";

			var supplier3 = OrgHeader.New(Factory);
			supplier3.OH_Code = "SUP3";
			supplier3.OH_FullName = "SUPPLIER NAME 3";

			var jobDec = Factory.New<EMCSJobDeclaration>();
			jobDec.OwnerDocumentaryAddress.OrganisationPK = supplier1.PK;
			var invHead = jobDec.Invoices.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("return Dec.Owner", supplier1, invHead.Supplier);

				invHead.JZ_OH_Supplier = ZGuid.Empty;
				jobDec.JE_OH_Supplier = supplier2.PK;
				jobDec.OwnerDocumentaryAddress.E2_AddressOverride = ZBool.True;
				AssertEquals("Dec.Owner is overridden, return Dec.Supplier", supplier2, invHead.Supplier);

				jobDec.OwnerDocumentaryAddress.E2_AddressOverride = ZBool.False;
				jobDec.OwnerDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				jobDec.JE_OH_Supplier = supplier2.PK;
				AssertEquals("Dec.Owner is empty, return Dec.Supplier", supplier2, invHead.Supplier);

				invHead.JZ_OH_Supplier = supplier3.PK;
				AssertEquals("return InvHeader.Supplier", supplier3, invHead.Supplier);
			});
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(ZDateTime.Empty, Factory.New<EMCSJobComInvoiceHeader>().JZ_InvoiceDate);
		}

		public void TestLookups()
		{
			var jobDec = Factory.New<EMCSJobDeclaration>();
			var invoiceHeader = jobDec.Invoices.AddNew();

			var lookups = invoiceHeader.Lookups;
			AssertType<EMCSJobComInvoiceHeaderLookups>(lookups);
		}

		public void TestGetNewValidation()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			AssertType<EMCSJobComInvoiceHeaderValidation>(invoiceHeader.Validation);
		}

		public void TestJZ_InvoiceNumber_Case()
		{
			var jobDec = Factory.New<EMCSJobDeclaration>();
			var invoiceHeader = jobDec.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "ABC def";
			AssertEquals("It's not forced to uppercase", "ABC def", invoiceHeader.JZ_InvoiceNumber);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var jobDec = Factory.New<EMCSJobDeclaration>();
			return jobDec.Invoices.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}
	}
}
