using System;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class ExporterProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNull(ExporterProvider.New(null));

			var declaration = Factory.New<JobDeclaration>();
			AssertType<ExporterProvider>(ExporterProvider.New(declaration.Invoices.AddNew()));
		}

		public void TestProperties()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_RL_NKClosestPort = "US";
			supplier.PrimaryRegistrationNumber.Number = "75.400.331/0001-15";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_RL_NKClosestPort = "BR";
			importer.PrimaryRegistrationNumber.Number = "86.400.331/0001-15";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			var invoice = declaration.Invoices.AddNew();

			var dataProvider = ExporterProvider.New(invoice);
			CombineAssertions(() =>
			{
				Assert("Code (JZ_SupplierAuthorityIdentifier) should be empty", dataProvider.Code.IsEmpty());
				Assert("Version (JZ_SupplierAuthorityVersion) should be Empty", dataProvider.Version.IsEmpty());
				Assert("CountryCode (Supplier.CountryCode) should be Empty", dataProvider.CountryCode.IsEmpty());
			});

			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_SupplierAuthorityIdentifier = "XXX";
			invoice.JZ_SupplierAuthorityVersion = "1";

			dataProvider = ExporterProvider.New(invoice);
			CombineAssertions(() =>
			{
				AssertEquals("Code (JZ_SupplierAuthorityIdentifier)", "XXX", dataProvider.Code);
				AssertEquals("Version (JZ_SupplierAuthorityVersion)", "1", dataProvider.Version);
				AssertEquals("CountryCode (Supplier.CountryCode)", "US", dataProvider.CountryCode);
				AssertEquals("RootCnpj (Supplier.GetRootCNPJFromCNPJ())", "86400331", dataProvider.RootCnpj);

				supplier.OH_RL_NKClosestPort = "BR";
				dataProvider = ExporterProvider.New(invoice);
				AssertEquals("RootCnpj (Consignee.GetRootCNPJFromCNPJ())", "75400331", dataProvider.RootCnpj);

				invoice.JZ_OH_Supplier = Guid.Empty;
				declaration.JE_OH_Importer = Guid.Empty;
				dataProvider = ExporterProvider.New(invoice);
				AssertEquals("RootCnpj (Consignee.GetRootCNPJFromCNPJ())", string.Empty, dataProvider.RootCnpj);

				supplier.OH_RL_NKClosestPort = "US";
				dataProvider = ExporterProvider.New(invoice);
				AssertEquals("RootCnpj (Consignee.GetRootCNPJFromCNPJ())", string.Empty, dataProvider.RootCnpj);
			});
		}
	}
}
