using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(DocDeclaration))]
	sealed class DocJobDeclarationTest : DocBaseJobDeclarationAbstractTest<JobDeclaration, DocDeclaration>
	{
		public void TestDeclarationImporter()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "XXX";
			importer.OH_IsConsignee = true;
			importer.PrimaryRegistrationNumber.Number = "123456";
			importer.OH_FullName = "Importer full name test";
			importer.MainAddress.Address1 = "Main Address 123";
			importer.MainAddress.OA_PostCode = "6789";
			importer.MainAddress.State = "importer State";
			importer.MainAddress.City = "importer City";
			importer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Mexico;

			Declaration.JE_OH_Importer = importer.PK;

			CombineAssertions(() =>
			{
				AssertEquals("PrimaryRegistrationNumber", importer.PrimaryRegistrationNumber.Number, DeclarationWrapper.ImporterRFC);
				AssertEquals("OH_FullName", importer.OH_FullName, DeclarationWrapper.ImporterName);
				AssertEquals("MainAddress.Address1", importer.MainAddress.Address1, DeclarationWrapper.ImporterAddress);
				AssertEquals("MainAddress.OA_PostCode", importer.MainAddress.OA_PostCode, DeclarationWrapper.ImporterPostCode);
				AssertEquals("MainAddress.State", importer.MainAddress.State, DeclarationWrapper.ImporterState);
				AssertEquals("MainAddress.City", importer.MainAddress.City, DeclarationWrapper.ImporterCity);
				AssertEquals("MainAddress.Country", "Mexico", DeclarationWrapper.ImporterCountry);
			});
		}

		public void TestDeclarationSupplier()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "YYY";
			supplier.OH_IsConsignee = true;
			supplier.PrimaryRegistrationNumber.Number = "987654";
			supplier.OH_FullName = "Supplier full name test";
			supplier.MainAddress.Address1 = "Main Address 457";
			supplier.MainAddress.OA_PostCode = "1234";
			supplier.MainAddress.State = "Supplier State";
			supplier.MainAddress.City = "Supplier City";
			supplier.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Mexico;

			Declaration.JE_OH_Importer = supplier.PK;

			CombineAssertions(() =>
			{
				AssertEquals("PrimaryRegistrationNumber", supplier.PrimaryRegistrationNumber.Number, DeclarationWrapper.ImporterRFC);
				AssertEquals("OH_FullName", supplier.OH_FullName, DeclarationWrapper.ImporterName);
				AssertEquals("MainAddress.Address1", supplier.MainAddress.Address1, DeclarationWrapper.ImporterAddress);
				AssertEquals("MainAddress.OA_PostCode", supplier.MainAddress.OA_PostCode, DeclarationWrapper.ImporterPostCode);
				AssertEquals("MainAddress.State", supplier.MainAddress.State, DeclarationWrapper.ImporterState);
				AssertEquals("MainAddress.City", supplier.MainAddress.City, DeclarationWrapper.ImporterCity);
				AssertEquals("MainAddress.Country", "Mexico", DeclarationWrapper.ImporterCountry);
			});
		}

		public void TestInvoiceDeclaration()
		{
			var invoice = Declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "112233";
			invoice.JZ_InvoiceDate = ZDateTime.Now;

			var invoice2 = Declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "334455";
			invoice2.JZ_InvoiceDate = ZDateTime.Now.AddDays(-2);

			CombineAssertions(() =>
			{
				AssertEquals("JZ_InvoiceNumber", invoice.JZ_InvoiceNumber, DeclarationWrapper.InvoiceHeaders[0].InvoiceNumber);
				AssertEquals("JZ_InvoiceDate", invoice.JZ_InvoiceDate.ToShortDateString(), DeclarationWrapper.InvoiceHeaders[0].InvoiceDate.ToShortDateString());

				AssertEquals("JZ_InvoiceNumber", invoice2.JZ_InvoiceNumber, DeclarationWrapper.InvoiceHeaders[1].InvoiceNumber);
				AssertEquals("JZ_InvoiceDate", invoice2.JZ_InvoiceDate.ToShortDateString(), DeclarationWrapper.InvoiceHeaders[1].InvoiceDate.ToShortDateString());
			});
		}

		#region Implementation

		protected override string TestingCountry => Core.Constants.CountryCodes.Mexico;

		protected override JobDeclaration GetNewJobDeclaration()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			return (JobDeclaration)declaration;
		}

		protected override DocDeclaration CreateDeclarationWrapper(JobDeclaration declaration)
		{
			var result = DocDeclaration.New(declaration, Factory);
			((IBODocDataProvider)result).SetDocWrapperContext(new Dictionary<string, object>());
			result.SetReportNameForTesting("Report Name");
			return result;
		}

		#endregion
	}
}
