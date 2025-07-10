using System;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	public class ManufacturerProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNull(ManufacturerProvider.New(null));

			var declaration = Factory.New<JobDeclaration>();
			AssertType<ManufacturerProvider>(ManufacturerProvider.New(declaration.Invoices.AddNew().InvoiceLines.AddNew()));
		}

		public void TestProperties()
		{
			var orgHeaderManufacturer = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderManufacturer.OH_RL_NKClosestPort = "BR";
			orgHeaderManufacturer.PrimaryRegistrationNumber.Number = "75.400.331/0001-15";
			orgHeaderManufacturer.OH_Code = "TEST";

			var orgHeaderConsignee = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderConsignee.OH_RL_NKClosestPort = "US";
			orgHeaderConsignee.PrimaryRegistrationNumber.Number = "86.400.331/0001-15";
			orgHeaderConsignee.OH_Code = "TEST2";

			var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress1.OA_OH = orgHeaderManufacturer.PK;

			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog.CGC_OH_Owner = orgHeaderConsignee.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CGC_Catalog = goodsCatalog.PK;

			var dataProvider = ManufacturerProvider.New(invoiceLine);
			CombineAssertions(() =>
			{
				Assert("Code (ManufacturerOrgCode) should be empty", dataProvider.Code.IsEmpty());
				Assert("Version (JI_ManufacturerAuthorityVersion) should be Empty", dataProvider.Version.IsEmpty());
				Assert("CountryCode (JI_CountryOfOrigin) should be Empty", dataProvider.CountryCode.IsEmpty());
			});

			invoiceLine.JI_ManufacturerIndicator = "1";
			invoiceLine.JI_OA_ManufacturerAddress = orgAddress1.PK;
			invoiceLine.JI_CountryOfOrigin = "BR";
			invoiceLine.JI_ManufacturerAuthorityVersion = "1";
			invoiceLine.JI_ManufacturerAuthorityIdentifier = "123";

			dataProvider = ManufacturerProvider.New(invoiceLine);
			CombineAssertions(() =>
			{
				AssertEquals("Code (ManufacturerOrgCode)", "123", dataProvider.Code);
				AssertEquals("Version (JI_ManufacturerAuthorityVersion)", "1", dataProvider.Version);
				AssertEquals("CountryCode (JI_CountryOfOrigin)", "BR", dataProvider.CountryCode);
				AssertEquals("RootCnpj (ManufacturerAddress.Header.GetRootCNPJFromCNPJ())", "75400331", dataProvider.RootCnpj);
			});

			invoiceLine.JI_ManufacturerIndicator = "3";
			dataProvider = ManufacturerProvider.New(invoiceLine);
			CombineAssertions(() =>
			{
				AssertEquals("Code (ManufacturerOrgCode)", string.Empty, dataProvider.Code);
				AssertEquals("Version (JI_ManufacturerAuthorityVersion)", string.Empty, dataProvider.Version);
				AssertEquals("CountryCode (JI_CountryOfOrigin)", "BR", dataProvider.CountryCode);
				AssertEquals("RootCnpj", string.Empty, dataProvider.RootCnpj);
			});

			invoiceLine.JI_ManufacturerIndicator = "1";
			invoiceLine.JI_CountryOfOrigin = "US";
			dataProvider = ManufacturerProvider.New(invoiceLine);
			CombineAssertions(() =>
			{
				AssertEquals("Code (ManufacturerOrgCode)", string.Empty, dataProvider.Code);
				AssertEquals("Version (JI_ManufacturerAuthorityVersion)", string.Empty, dataProvider.Version);
				AssertEquals("CountryCode (JI_CountryOfOrigin)", "US", dataProvider.CountryCode);
				AssertEquals("RootCnpj", "86400331", dataProvider.RootCnpj);
			});

			invoiceLine.JI_OA_ManufacturerAddress = Guid.Empty;
			invoiceLine.JI_CGC_Catalog = Guid.Empty;
			dataProvider = ManufacturerProvider.New(invoiceLine);
			CombineAssertions(() =>
			{
				AssertEquals("Code (ManufacturerOrgCode)", string.Empty, dataProvider.Code);
				AssertEquals("Version (JI_ManufacturerAuthorityVersion)", string.Empty, dataProvider.Version);
				AssertEquals("CountryCode (JI_CountryOfOrigin)", "US", dataProvider.CountryCode);
				AssertEquals("RootCnpj", string.Empty, dataProvider.RootCnpj);
			});

			invoiceLine.JI_CountryOfOrigin = "BR";
			dataProvider = ManufacturerProvider.New(invoiceLine);
			CombineAssertions(() =>
			{
				AssertEquals("Code (ManufacturerOrgCode)", string.Empty, dataProvider.Code);
				AssertEquals("Version (JI_ManufacturerAuthorityVersion)", string.Empty, dataProvider.Version);
				AssertEquals("CountryCode (JI_CountryOfOrigin)", "BR", dataProvider.CountryCode);
				AssertEquals("RootCnpj", string.Empty, dataProvider.RootCnpj);
			});
		}
	}
}
