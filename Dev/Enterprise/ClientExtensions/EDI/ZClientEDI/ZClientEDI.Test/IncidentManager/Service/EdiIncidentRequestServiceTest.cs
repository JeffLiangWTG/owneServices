using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.Registry.ProductAreaModuleMapping;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using static Enterprise.Core.Constants.CustomerService;
using IHttpClientFactory = WTG.Foundation.Http.IHttpClientFactory;

namespace Enterprise.Client.EDI.IncidentManager.Service
{
	internal class EdiIncidentRequestServiceTest : TestCaseWithFactory
	{
		[TestDate(2021, 6, 11)]
		public void TestGetProductList_DirectMasterOrg()
		{
			var products = new SystemProductCollection();
			var productExternal1 = products.AddNew();
			productExternal1.Code = "AAA"; // licenced
			productExternal1.Description = (NoResString)"AAA";
			var productExternal2 = products.AddNew();
			productExternal2.Code = "BBB"; // licenced but disabled
			productExternal2.Enabled = false;
			productExternal2.Description = (NoResString)"BBB";
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, products);
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			db1.LD_OH_WebAccessOrg = ExternalLicCompany.Header.PK;
			db1.LD_Product = "AAA";
			db1.LD_ServerCode = "S1";
			var db2 = ExternalLicCompany.LicDatabases.AddNew();
			db2.LD_Product = "TLX";
			db2.LD_ServerCode = "S1";
			Factory.Save();
			var expectedExternalProducts = new Tuple<string, string>[] { Tuple.Create("AAA", "AAA"), };
			var service = new EdiIncidentRequestService();
			var externalProducts = service.GetProductList(SharedConstants.Languages.English, ExternalContact.PK.ToGuid()).ToArray();
			AssertArrayEqualsByElements("externalProducts", expectedExternalProducts, externalProducts);
		}

		[TestDate(2021, 4, 30)]
		public void TestGetProductList_External()
		{
			var products = new SystemProductCollection();
			var productExternal1 = products.AddNew();
			productExternal1.Code = "TLX"; // licenced
			productExternal1.Description = (NoResString)"Translogix";
			var productExternal2 = products.AddNew();
			productExternal2.Code = "CF"; // licenced but disabled
			productExternal2.Enabled = false;
			productExternal2.Description = (NoResString)"Core Freight";
			var productExternal3 = products.AddNew();
			productExternal3.Code = "ZZZ"; // not licenced
			productExternal3.Description = (NoResString)"ZZZ";
			var productExternal4 = products.AddNew();
			productExternal4.Code = "YYY"; // inactive
			productExternal4.Description = (NoResString)"YYY";
			var productExternal5 = products.AddNew();
			productExternal5.Code = "XXX"; // added later
			productExternal5.Description = (NoResString)"XXX";
			var productExternal6 = products.AddNew();
			productExternal6.Code = "TEL"; // added later
			productExternal6.Description = (NoResString)"Telematics";
			var productExternal7 = products.AddNew();
			productExternal7.Code = "AAA"; // internal
			productExternal7.Description = (NoResString)"AAA";
			productExternal7.IsInternal = true;
			productExternal7.ModuleMappings.AddNew("ZZ1", "ZZZ Module 1", "", true);
			productExternal7.ModuleMappings.AddNew("ZZ2", "ZZZ Module 2", "", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, products);
			var db1 = ExternalLicCompany.LicDatabases.AddNew();
			var db2 = ExternalLicCompany.LicDatabases.AddNew();
			var db3 = ExternalLicCompany.LicDatabases.AddNew();
			var db4 = Factory.NewWithValidTestData<LicenceDatabase>();
			var db5 = ExternalLicCompany.LicDatabases.AddNew();
			var db7 = ExternalLicCompany.LicDatabases.AddNew();
			db1.LD_Product = "TLX";
			db1.LD_ServerCode = "S1";
			db2.LD_Product = "CF";
			db2.LD_ServerCode = "S2";
			db3.LD_Product = "CW1"; // CW1 should enable ENT
			db3.LD_ServerCode = "S3";
			db4.LD_Product = "ZZZ";
			db4.LD_ServerCode = "S4";
			db5.LD_Product = "YYY";
			db5.LD_ServerCode = "S5";
			db5.LD_IsActive = false;
			db7.LD_Product = "AAA";
			db7.LD_ServerCode = "S7";
			Factory.Save();
			var expectedExternalProducts = new Tuple<string, string>[] { Tuple.Create("ENT", "CargoWise"), Tuple.Create("TLX", "Translogix"), Tuple.Create("TEL", "Telematics"), };
			var service = new EdiIncidentRequestService();
			var externalProducts = service.GetProductList(SharedConstants.Languages.English, ExternalContact.PK.ToGuid()).ToArray();
			AssertArrayEqualsByElements("externalProducts", expectedExternalProducts, externalProducts);
			var db6 = ExternalLicCompany.LicDatabases.AddNew();
			db6.LD_Product = "XXX";
			db6.LD_ServerCode = "S6";
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(14);
			externalProducts = service.GetProductList(SharedConstants.Languages.English, ExternalContact.PK.ToGuid()).ToArray();
			AssertArrayEqualsByElements("externalProducts", expectedExternalProducts, externalProducts);
			expectedExternalProducts = new Tuple<string, string>[] { Tuple.Create("ENT", "CargoWise"), Tuple.Create("TLX", "Translogix"), Tuple.Create("XXX", "XXX"), Tuple.Create("TEL", "Telematics"), };
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			externalProducts = service.GetProductList(SharedConstants.Languages.English, ExternalContact.PK.ToGuid()).ToArray();
			AssertArrayEqualsByElements("externalProducts", expectedExternalProducts, externalProducts);
			var collection = new WebSecurityMappingCollection();
			collection.AddNew(EDIWebSecurityRightsList.WiseBusinessPartner.Code, "AAA", "ZZ1");
			collection.AddNew(EDIWebSecurityRightsList.WiseBusinessPartner.Code, "AAA", "ZZ2");
			EDIDataRegistry.Instance.WebSecurityProductModuleMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			foreach (OrgSecurity sec in ExternalContact.Header.SecurityRights)
			{
				if (sec.OX_SecurityItemName == EDIWebSecurityRightsList.WiseBusinessPartner.Code)
				{
					sec.OX_Granted = true;
				}
				else
				{
					sec.OX_Granted = false;
				}
			}

			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(20);
			expectedExternalProducts = new Tuple<string, string>[] { Tuple.Create("ENT", "CargoWise"), Tuple.Create("TLX", "Translogix"), Tuple.Create("XXX", "XXX"), Tuple.Create("TEL", "Telematics"), Tuple.Create("AAA", "AAA"), };
			externalProducts = service.GetProductList(SharedConstants.Languages.English, ExternalContact.PK.ToGuid()).ToArray();
			AssertArrayEqualsByElements("externalProducts", expectedExternalProducts, externalProducts);
		}

		[TestDate(2018, 5, 1)]
		public void TestGetProductListCacheExpiry_External()
		{
			var db1 = ExternalLicCompany.LicDatabases.AddNew();
			var db2 = ExternalLicCompany.LicDatabases.AddNew();
			var db3 = ExternalLicCompany.LicDatabases.AddNew();
			db1.LD_Product = "TLX";
			db1.LD_ServerCode = "S1";
			db2.LD_Product = "CF";
			db2.LD_ServerCode = "S2";
			db3.LD_Product = "CW1"; // CW1 should enable ENT
			db3.LD_ServerCode = "S3";
			Factory.Save();
			var products = new SystemProductCollection();
			var productExternal1 = products.AddNew();
			productExternal1.Code = "TLX";
			productExternal1.Description = (NoResString)"Translogix";
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, products);
			var service = new EdiIncidentRequestService();
			var productExternal2 = products.AddNew();
			productExternal2.Code = "CF";
			productExternal2.Description = (NoResString)"Core Freight";
			var externalProducts1 = service.GetProductList(SharedConstants.Languages.English, ExternalContact.PK.ToGuid()).ToArray();
			var expectedExternalProducts1 = new Tuple<string, string>[] { Tuple.Create("ENT", "CargoWise"), Tuple.Create("TLX", "Translogix"), };
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, products);
			var expectedExternalProducts2 = new Tuple<string, string>[] { Tuple.Create("ENT", "CargoWise"), Tuple.Create("TLX", "Translogix"), Tuple.Create("CF", "Core Freight"), };
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(14).AddSeconds(59);
			var externalProducts2 = service.GetProductList(SharedConstants.Languages.English, ExternalContact.PK.ToGuid()).ToArray();
			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			var externalProducts3 = service.GetProductList(SharedConstants.Languages.English, ExternalContact.PK.ToGuid()).ToArray();
			AssertArrayEqualsByElements("externalProducts1", expectedExternalProducts1, externalProducts1);
			AssertArrayEqualsByElements("externalProducts2 use cached value", expectedExternalProducts1, externalProducts2);
			AssertArrayEqualsByElements("externalProducts3 use new value", expectedExternalProducts2, externalProducts3);
		}

		public void TestGetProductList_Internal()
		{
			var products = new SystemProductCollection();
			var productInternal1 = products.AddNew();
			productInternal1.Code = "HUB";
			productInternal1.Description = (NoResString)"eHub";
			productInternal1.IsInternal = true;
			productInternal1.Enabled = false;
			var productInternal2 = products.AddNew();
			productInternal2.Code = "TTN";
			productInternal2.Description = (NoResString)"TiTAN";
			productInternal2.IsInternal = true;
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, products);
			var service = new EdiIncidentRequestService();
			var internalProducts = service.GetProductList(SharedConstants.Languages.English, InternalContact.PK.ToGuid()).ToArray();
			// test caching by deleting the contact
			InternalContact.Delete();
			Factory.Save();
			var internalProductsCall2 = service.GetProductList(SharedConstants.Languages.English, InternalContact.PK.ToGuid()).ToArray();
			var expectedInternalProducts = new Tuple<string, string>[] { Tuple.Create("ENT", "CargoWise"), Tuple.Create("TTN", "TiTAN"), };
			AssertArrayEqualsByElements("internalProducts", expectedInternalProducts, internalProducts);
			AssertArrayEqualsByElements("internalProductsCall2", expectedInternalProducts, internalProductsCall2);
		}

		public void TestGetProductList_ServiceProvider()
		{
			var products = new SystemProductCollection();
			var productInternal = products.AddNew();
			productInternal.Code = "HUB";
			productInternal.Description = (NoResString)"eHub";
			productInternal.IsInternal = true;
			productInternal.Enabled = false;
			var productExternal = products.AddNew();
			productExternal.Code = "ABU";
			productExternal.Description = (NoResString)"Product - ABU";
			productExternal.IsInternal = false;
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, products);
			var licenceCompany = BillingTestHelper.CreateLicencedOrganization(Factory, "EN9", "CO9");
			var contact = licenceCompany.Header.Contacts[0];
			Factory.Save();
			var service = new EdiIncidentRequestService();
			var dynMethod = service.GetType().GetMethod("GetListOfOrgLicencedProducts", BindingFlags.NonPublic | BindingFlags.Instance);
			var licencedProducts = dynMethod.Invoke(service, new object[] { contact.PK.ToGuid() });
			var dynField = licencedProducts.GetType().GetField("List", BindingFlags.Public | BindingFlags.Instance);
			var list = dynField.GetValue(licencedProducts) as ReadOnlyCollection<Tuple<string, string>>;
			AssertEquals("the service provider doesn't have any licenses/products", 0, list.Count);
			var result = service.GetProductList(SharedConstants.Languages.English, contact.PK.ToGuid()).ToArray();
			var expectedProducts = new Tuple<string, string>[] { Tuple.Create("ENT", "CargoWise"), Tuple.Create("ABU", "Product - ABU"), };
			AssertArrayEqualsByElements("full product list", expectedProducts, result);
		}

		public void TestGetProductList_ServiceProvider_WTALicence()
		{
			var products = new SystemProductCollection();
			var productInternal = products.AddNew();
			productInternal.Code = "HUB";
			productInternal.Description = (NoResString)"eHub";
			productInternal.IsInternal = true;
			productInternal.Enabled = false;
			var productExternal = products.AddNew();
			productExternal.Code = "ABU";
			productExternal.Description = (NoResString)"Product - ABU";
			productExternal.IsInternal = false;
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, products);
			var licenceCompany = BillingTestHelper.CreateLicencedOrganization(Factory, "EN9", "CO9");
			var contact = licenceCompany.Header.Contacts[0];
			var db = licenceCompany.LicDatabases.AddNew();
			db.LD_Product = ProductTypes.Codes.WiseTechAcademy;
			db.LD_ServerCode = "S1";
			Factory.Save();
			var service = new EdiIncidentRequestService();
			var dynMethod = service.GetType().GetMethod("GetListOfOrgLicencedProducts", BindingFlags.NonPublic | BindingFlags.Instance);
			var licencedProducts = dynMethod.Invoke(service, new object[] { contact.PK.ToGuid() });
			var dynField = licencedProducts.GetType().GetField("List", BindingFlags.Public | BindingFlags.Instance);
			var list = dynField.GetValue(licencedProducts) as ReadOnlyCollection<Tuple<string, string>>;
			AssertEquals("Should have one license/product", 1, list.Count);
			AssertEquals("Should have one license/product", "WTA", list[0].Item1);
			var result = service.GetProductList(SharedConstants.Languages.English, contact.PK.ToGuid()).ToArray();
			var expectedProducts = new Tuple<string, string>[] { Tuple.Create("ENT", "CargoWise"), Tuple.Create("ABU", "Product - ABU"), };
			AssertArrayEqualsByElements("full product list", expectedProducts, result);
		}

		public void TestGetProductList_CWNOrg()
		{
			var cwnLicence = BillingTestHelper.CreateLicence(Factory, "CON");
			cwnLicence.Database.LD_Product = "CWN";
			var cwnContact = cwnLicence.Company.Header.Contacts[0];
			Factory.Save();
			var service = new EdiIncidentRequestService();
			var productList = service.GetProductList(SharedConstants.Languages.English, cwnContact.PK.ToGuid());
			AssertCollectionContains("Should contain ENT - CargoWise", productList, x => x.Item1 == ProductTypes.Codes.Enterprise && x.Item2 == ProductTypes.Descriptions.CargoWise);
		}

		[TestDate(2018, 5, 1)]
		public void TestGetProductListCacheExpiry_Internal()
		{
			var products = new SystemProductCollection();
			var productInternal1 = products.AddNew();
			productInternal1.Code = "TTN";
			productInternal1.Description = (NoResString)"TiTAN";
			productInternal1.IsInternal = true;
			var productExternal1 = products.AddNew();
			productExternal1.Code = "TLX";
			productExternal1.Description = (NoResString)"Translogix";
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, products);
			var service = new EdiIncidentRequestService();
			var productExternal2 = products.AddNew();
			productExternal2.Code = "CF";
			productExternal2.Description = (NoResString)"Core Freight";
			var internalProducts1 = service.GetProductList(SharedConstants.Languages.English, InternalContact.PK.ToGuid()).ToArray();
			var expectedInternalProducts1 = new Tuple<string, string>[] { Tuple.Create("ENT", "CargoWise"), Tuple.Create("TTN", "TiTAN"), Tuple.Create("TLX", "Translogix"), };
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, products);
			var expectedInternalProducts2 = new Tuple<string, string>[] { Tuple.Create("ENT", "CargoWise"), Tuple.Create("TTN", "TiTAN"), Tuple.Create("TLX", "Translogix"), Tuple.Create("CF", "Core Freight"), };
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(14).AddSeconds(59);
			var internalProducts2 = service.GetProductList(SharedConstants.Languages.English, InternalContact.PK.ToGuid()).ToArray();
			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			var internalProducts3 = service.GetProductList(SharedConstants.Languages.English, InternalContact.PK.ToGuid()).ToArray();
			AssertArrayEqualsByElements("internalProducts1", expectedInternalProducts1, internalProducts1);
			AssertArrayEqualsByElements("internalProducts2 use cached value", expectedInternalProducts1, internalProducts2);
			AssertArrayEqualsByElements("internalProducts3 use new value", expectedInternalProducts2, internalProducts3);
		}

		[TestDate(2021, 4, 30)]
		public void TestGetFullProductList()
		{
			var products = new SystemProductCollection();
			var productExternal1 = products.AddNew();
			productExternal1.Code = "TLX"; // licenced
			productExternal1.Description = (NoResString)"Translogix";
			var productExternal2 = products.AddNew();
			productExternal2.Code = "CF"; // licenced but disabled
			productExternal2.Enabled = false;
			productExternal2.Description = (NoResString)"Core Freight";
			var productExternal3 = products.AddNew();
			productExternal3.Code = "ZZZ"; // not licenced
			productExternal3.Description = (NoResString)"ZZZ";
			var productExternal4 = products.AddNew();
			productExternal4.Code = "YYY"; // inactive (should be shown as it is not database dependent in full list)
			productExternal4.Description = (NoResString)"YYY";
			var productExternal5 = products.AddNew();
			productExternal5.Code = "XXX"; // added later (should be shown as it is not database dependent in full list)
			productExternal5.Description = (NoResString)"XXX";
			var productExternal6 = products.AddNew();
			productExternal6.Code = "TEL"; // added later (should be shown as it is not database dependent in full list)
			productExternal6.Description = (NoResString)"Telematics";
			var productExternal7 = products.AddNew();
			productExternal7.Code = "AAA"; // internal (should be shown as internal products are shown in full list)
			productExternal7.Description = (NoResString)"AAA";
			productExternal7.IsInternal = true;
			productExternal7.ModuleMappings.AddNew("ZZ1", "ZZZ Module 1", "", true);
			productExternal7.ModuleMappings.AddNew("ZZ2", "ZZZ Module 2", "", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, products);
			var db1 = ExternalLicCompany.LicDatabases.AddNew();
			var db2 = ExternalLicCompany.LicDatabases.AddNew();
			var db3 = ExternalLicCompany.LicDatabases.AddNew();
			var db4 = Factory.NewWithValidTestData<LicenceDatabase>();
			var db5 = ExternalLicCompany.LicDatabases.AddNew();
			var db7 = ExternalLicCompany.LicDatabases.AddNew();
			db1.LD_Product = "TLX";
			db1.LD_ServerCode = "S1";
			db2.LD_Product = "CF";
			db2.LD_ServerCode = "S2";
			db3.LD_Product = "CW1"; // CW1 should enable ENT
			db3.LD_ServerCode = "S3";
			db4.LD_Product = "ZZZ";
			db4.LD_ServerCode = "S4";
			db5.LD_Product = "YYY";
			db5.LD_ServerCode = "S5";
			db5.LD_IsActive = false;
			db7.LD_Product = "AAA";
			db7.LD_ServerCode = "S7";
			Factory.Save();
			//ENT, TEL, TLX, AAA
			var expectedExternalProducts = new Tuple<string, string>[] { Tuple.Create("ENT", "CargoWise"), Tuple.Create("TLX", "Translogix"), Tuple.Create("ZZZ", "ZZZ"), Tuple.Create("YYY", "YYY"), Tuple.Create("XXX", "XXX"), Tuple.Create("TEL", "Telematics"), Tuple.Create("AAA", "AAA"), };
			var service = new EdiIncidentRequestService();
			var externalProducts = service.GetFullProductList().ToArray();
			AssertArrayEqualsByElements("externalProducts", expectedExternalProducts, externalProducts);
			var db6 = ExternalLicCompany.LicDatabases.AddNew();
			db6.LD_Product = "XXX";
			db6.LD_ServerCode = "S6";
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(14);
			externalProducts = service.GetFullProductList().ToArray();
			AssertArrayEqualsByElements("externalProducts", expectedExternalProducts, externalProducts);
			//ENT, TEL, TLX, XXX, AAA
			expectedExternalProducts = new Tuple<string, string>[] { Tuple.Create("ENT", "CargoWise"), Tuple.Create("TLX", "Translogix"), Tuple.Create("ZZZ", "ZZZ"), Tuple.Create("YYY", "YYY"), Tuple.Create("XXX", "XXX"), Tuple.Create("TEL", "Telematics"), Tuple.Create("AAA", "AAA"), };
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			externalProducts = service.GetFullProductList().ToArray();
			AssertArrayEqualsByElements("externalProducts", expectedExternalProducts, externalProducts);
		}

		public void TestGetFullModuleList()
		{
			var products = RegistryDefaultsHelper.GetEnterpriseDefaults();
			var product1 = products.AddNew();
			product1.Code = "ZZZ";
			product1.Description = (NoResString)"ZZZ Product";
			product1.IsInternal = true;
			product1.ModuleMappings.AddNew("ZZ1", "ZZZ Module 1", "", true);
			product1.ModuleMappings.AddNew("ZZ2", "ZZZ Module 2", "", true);
			product1.ModuleMappings.AddNew("ZZ3", "ZZZ Module 3 Internal", "", true, true);
			var enterpriseProduct = products.GetProductByCode(ProductTypes.Codes.Enterprise);
			enterpriseProduct.ModuleMappings.AddNew("E01", "Ent 1", ZString.Empty, true);
			enterpriseProduct.ModuleMappings.AddNew("E02", "Ent 2 Internal", ZString.Empty, true, true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, products);
			var cr8Products = new SystemProductCollection();
			{
				var ent = cr8Products.AddNew(ProductTypes.Codes.Enterprise, ProductTypes.Descriptions.EnterpriseCW1, true);
				ent.ModuleMappings.AddNew("NTZ", "NTZ's module", ZString.Empty, true);
				ent.ModuleMappings.AddNew("E82", "Ent CR8 Mod 2", ZString.Empty, true);
				EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr8Products);
			}

			var cr9Products = new SystemProductCollection();
			{
				var ent = cr9Products.AddNew(ProductTypes.Codes.Enterprise, ProductTypes.Descriptions.EnterpriseCW1, true);
				ent.ModuleMappings.AddNew("NTZ", "NTZ's module", ZString.Empty, true);
				ent.ModuleMappings.AddNew("E92", "Ent CR9 Mod 2", ZString.Empty, true);
				EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr9Products);
			}

			var service = new EdiIncidentRequestService();
			var results = service.GetFullModuleListByProduct();
			AssertEquals("Internal product", 1, results.Count(x => x.Item1.Equals("ZZZ") && string.IsNullOrEmpty(x.Item2) && x.Item3.Equals("ZZ1") && x.Item4.Equals("ZZZ Module 1")));
			AssertEquals("Internal product", 1, results.Count(x => x.Item1.Equals("ZZZ") && string.IsNullOrEmpty(x.Item2) && x.Item3.Equals("ZZ2") && x.Item4.Equals("ZZZ Module 2")));
			AssertEquals("Internal module", 1, results.Count(x => x.Item1.Equals("ZZZ") && string.IsNullOrEmpty(x.Item2) && x.Item3.Equals("ZZ3")  && x.Item4.Equals("ZZZ Module 3 Internal")));
			AssertEquals("Enterprise product", 1, results.Count(x => x.Item1.Equals(ProductTypes.Codes.Enterprise) && string.IsNullOrEmpty(x.Item2) && x.Item3.Equals("E01")  && x.Item4.Equals("Ent 1")));
			AssertEquals("Enterprise product internal module", 1, results.Count(x => x.Item1.Equals(ProductTypes.Codes.Enterprise) && string.IsNullOrEmpty(x.Item2) && x.Item3.Equals("E02") && x.Item4.Equals("Ent 2 Internal")));
			AssertEquals("Enterprise product CR8/9", 1, results.Count(x => x.Item1.Equals(ProductTypes.Codes.Enterprise) && x.Item2.Equals(CriticalityCodes.CR8_ComplianceRequirement) && x.Item3.Equals("NTZ") && x.Item4.Equals("NTZ's module")));
			AssertEquals("Enterprise product CR8", 1, results.Count(x => x.Item1.Equals(ProductTypes.Codes.Enterprise) && x.Item2.Equals(CriticalityCodes.CR8_ComplianceRequirement) && x.Item3.Equals("E82") && x.Item4.Equals("Ent CR8 Mod 2")));
			AssertEquals("Enterprise product CR9", 1, results.Count(x => x.Item1.Equals(ProductTypes.Codes.Enterprise) && x.Item2.Equals(CriticalityCodes.CR9_CustomerServiceRequest) && x.Item3.Equals("E92") && x.Item4.Equals("Ent CR9 Mod 2")));
		}

		public void TestGetModuleList_AllArgsAreNull()
		{
			var products = RegistryDefaultsHelper.GetEnterpriseDefaults();
			var product1 = products.AddNew();
			product1.Code = "ZZZ";
			product1.Description = (NoResString)"ZZZ Product";
			product1.IsInternal = true;
			product1.ModuleMappings.AddNew("ZZ1", "ZZZ Module 1", "", true);
			product1.ModuleMappings.AddNew("ZZ2", "ZZZ Module 2", "", true);
			product1.ModuleMappings.AddNew("ZZ3", "ZZZ Module 3 Internal", "", true, true);
			var enterpriseProduct = products.GetProductByCode(ProductTypes.Codes.Enterprise);
			enterpriseProduct.ModuleMappings.AddNew("E01", "Ent 1", ZString.Empty, true);
			enterpriseProduct.ModuleMappings.AddNew("E02", "Ent 2 Internal", ZString.Empty, true, true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, products);
			var cr8Products = new SystemProductCollection();
			{
				var ent = cr8Products.AddNew(Licencing.Business.ProductTypes.Codes.Enterprise, Licencing.Business.ProductTypes.Descriptions.EnterpriseCW1, true);
				ent.ModuleMappings.AddNew("NTZ", "NTZ's module", ZString.Empty, true);
				ent.ModuleMappings.AddNew("E82", "Ent CR8 Mod 2", ZString.Empty, true);
				EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr8Products);
			}

			var cr9Products = new SystemProductCollection();
			{
				var ent = cr9Products.AddNew(Licencing.Business.ProductTypes.Codes.Enterprise, Licencing.Business.ProductTypes.Descriptions.EnterpriseCW1, true);
				ent.ModuleMappings.AddNew("NTZ", "NTZ's module", ZString.Empty, true);
				ent.ModuleMappings.AddNew("E92", "Ent CR9 Mod 2", ZString.Empty, true);
				EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr9Products);
			}

			var service = new EdiIncidentRequestService();
			var results = service.GetModuleList(null, null, SharedConstants.Languages.English, string.Empty, Guid.Empty);
			AssertEquals("Results should be unique", 1, results.Count(x => x.Item1.Equals("NTZ")));
		}

		public void TestGetModuleList_External_WebSecurity_NullContact()
		{
			var products = RegistryDefaultsHelper.GetEnterpriseDefaults();
			var product1 = products.AddNew();
			product1.Code = "ZZZ";
			product1.Description = (NoResString)"ZZZ Product";
			product1.IsInternal = true;
			product1.ModuleMappings.AddNew("ZZ1", "ZZZ Module 1", "", true);
			product1.ModuleMappings.AddNew("ZZ2", "ZZZ Module 2", "", true);
			product1.ModuleMappings.AddNew("ZZ3", "ZZZ Module 3 Internal", "", true, true);
			product1.ModuleMappings.AddNew("ZZ4", "ZZZ Module 4 Internal", "", true, true);
			product1.ModuleMappings.AddNew("ZZ5", "ZZZ Module 5 Internal", "", true, true);
			var enterpriseProduct = products.GetProductByCode(ProductTypes.Codes.Enterprise);
			enterpriseProduct.ModuleMappings.AddNew("E01", "Ent 1", ZString.Empty, true);
			enterpriseProduct.ModuleMappings.AddNew("E02", "Ent 2 Internal", ZString.Empty, true, true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, products);
			foreach (OrgSecurity sec in ExternalContact.Header.SecurityRights)
			{
				if (sec.OX_SecurityItemName == EDIWebSecurityRightsList.WiseBusinessPartner.Code)
				{
					sec.OX_Granted = true;
				}
				else
				{
					sec.OX_Granted = false;
				}
			}

			Factory.Save();
			var collection = new WebSecurityMappingCollection();
			collection.AddNew(EDIWebSecurityRightsList.WiseBusinessPartner.Code, "ZZZ", "ZZ4");
			collection.AddNew(EDIWebSecurityRightsList.WiseBusinessPartner.Code, "ZZZ", "ZZ5");
			EDIDataRegistry.Instance.WebSecurityProductModuleMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			var service = new EdiIncidentRequestService();
			var modules = service.GetModuleList("ZZZ", "CR4", SharedConstants.Languages.English, "", Guid.NewGuid()).ToArray();
			AssertEquals("no modules", 0, modules.Length);
		}

		public void TestGetModuleList_External_WebSecurity()
		{
			var products = RegistryDefaultsHelper.GetEnterpriseDefaults();
			var product1 = products.AddNew();
			product1.Code = "ZZZ";
			product1.Description = (NoResString)"ZZZ Product";
			product1.IsInternal = true;
			product1.ModuleMappings.AddNew("ZZ1", "ZZZ Module 1", "", true);
			product1.ModuleMappings.AddNew("ZZ2", "ZZZ Module 2", "", true);
			product1.ModuleMappings.AddNew("ZZ3", "ZZZ Module 3 Internal", "", true, true);
			product1.ModuleMappings.AddNew("ZZ4", "ZZZ Module 4 Internal", "", true, true);
			product1.ModuleMappings.AddNew("ZZ5", "ZZZ Module 5 Internal", "", true, true);
			var enterpriseProduct = products.GetProductByCode(ProductTypes.Codes.Enterprise);
			enterpriseProduct.ModuleMappings.AddNew("E01", "Ent 1", ZString.Empty, true);
			enterpriseProduct.ModuleMappings.AddNew("E02", "Ent 2 Internal", ZString.Empty, true, true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, products);
			foreach (OrgSecurity sec in ExternalContact.Header.SecurityRights)
			{
				if (sec.OX_SecurityItemName == EDIWebSecurityRightsList.WiseBusinessPartner.Code)
				{
					sec.OX_Granted = true;
				}
				else
				{
					sec.OX_Granted = false;
				}
			}

			Factory.Save();
			var collection = new WebSecurityMappingCollection();
			collection.AddNew(EDIWebSecurityRightsList.WiseBusinessPartner.Code, "ZZZ", "ZZ4");
			collection.AddNew(EDIWebSecurityRightsList.WiseBusinessPartner.Code, "ZZZ", "ZZ5");
			EDIDataRegistry.Instance.WebSecurityProductModuleMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			var service = new EdiIncidentRequestService();
			var zzzInternalModules = service.GetModuleList("ZZZ", "CR4", SharedConstants.Languages.English, "", InternalContact.PK.ToGuid()).ToArray();
			var zzzExternalModules = service.GetModuleList("ZZZ", "CR4", SharedConstants.Languages.English, "", ExternalContact.PK.ToGuid()).ToArray();
			var expectedZZZExternalModules = new[] { Tuple.Create("ZZ4", "ZZZ Module 4 Internal"), Tuple.Create("ZZ5", "ZZZ Module 5 Internal"), };
			var expectedZZZInternalModules = new[] { Tuple.Create("ZZ1", "ZZZ Module 1"), Tuple.Create("ZZ2", "ZZZ Module 2"), Tuple.Create("ZZ3", "ZZZ Module 3 Internal"), Tuple.Create("ZZ4", "ZZZ Module 4 Internal"), Tuple.Create("ZZ5", "ZZZ Module 5 Internal"), };
			AssertArrayEqualsByElements("zzzInternalModules", expectedZZZInternalModules, zzzInternalModules);
			AssertArrayEqualsByElements("zzzExternalModules", expectedZZZExternalModules, zzzExternalModules);
		}

		public void TestGetModuleList_External_WebSecurity_CR8OrCR9()
		{
			var products = RegistryDefaultsHelper.GetEnterpriseDefaults();
			var product1 = products.AddNew();
			product1.Code = "ZZZ";
			product1.Description = (NoResString)"ZZZ Product";
			product1.IsInternal = true;
			product1.ModuleMappings.AddNew("ZZ1", "ZZZ Module 1", "", true);
			product1.ModuleMappings.AddNew("ZZ2", "ZZZ Module 2", "", true);
			product1.ModuleMappings.AddNew("ZZ3", "ZZZ Module 3 Internal", "", true, true);
			product1.ModuleMappings.AddNew("ZZ4", "ZZZ Module 4 Internal", "", true, true);
			product1.ModuleMappings.AddNew("ZZ5", "ZZZ Module 5 Internal", "", true, true);
			var enterpriseProduct = products.GetProductByCode(ProductTypes.Codes.Enterprise);
			enterpriseProduct.ModuleMappings.AddNew("E01", "Ent 1", ZString.Empty, true);
			enterpriseProduct.ModuleMappings.AddNew("E02", "Ent 2 Internal", ZString.Empty, true, true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, products);
			foreach (OrgSecurity sec in ExternalContact.Header.SecurityRights)
			{
				if (sec.OX_SecurityItemName == EDIWebSecurityRightsList.WiseBusinessPartner.Code)
				{
					sec.OX_Granted = true;
				}
				else
				{
					sec.OX_Granted = false;
				}
			}

			Factory.Save();

			var collection = new WebSecurityMappingCollection();
			collection.AddNew(EDIWebSecurityRightsList.WiseBusinessPartner.Code, "ZZZ", "ZZ4");
			collection.AddNew(EDIWebSecurityRightsList.WiseBusinessPartner.Code, "ZZZ", "ZZ5");
			EDIDataRegistry.Instance.WebSecurityProductModuleMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			var service = new EdiIncidentRequestService();
			var zzzInternalModules = service.GetModuleList("ZZZ", "CR4", SharedConstants.Languages.English, "", InternalContact.PK.ToGuid()).ToArray();
			var zzzExternalModules = service.GetModuleList("ZZZ", "CR4", SharedConstants.Languages.English, "", ExternalContact.PK.ToGuid()).ToArray();
			var expectedZZZExternalModules = new[] { Tuple.Create("ZZ4", "ZZZ Module 4 Internal"), Tuple.Create("ZZ5", "ZZZ Module 5 Internal"), };
			var expectedZZZInternalModules = new[] { Tuple.Create("ZZ1", "ZZZ Module 1"), Tuple.Create("ZZ2", "ZZZ Module 2"), Tuple.Create("ZZ3", "ZZZ Module 3 Internal"), Tuple.Create("ZZ4", "ZZZ Module 4 Internal"), Tuple.Create("ZZ5", "ZZZ Module 5 Internal"), };
			AssertArrayEqualsByElements("zzzInternalModules", expectedZZZInternalModules, zzzInternalModules);
			AssertArrayEqualsByElements("zzzExternalModules", expectedZZZExternalModules, zzzExternalModules);

			var cr8Products = new SystemProductCollection();
			{
				var ent = cr8Products.AddNew("ZZZ", "ZZZ Desc", true);
				ent.ModuleMappings.AddNew("Z81", "ZZZ CR8 Mod 1", ZString.Empty, true);
				ent.ModuleMappings.AddNew("Z82", "ZZZ CR8 Mod 2", ZString.Empty, true);
				ent.ModuleMappings.AddNew("Z83", "ZZZ CR8 Mod 3 Internal", ZString.Empty, true, true);
				EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr8Products);
			}

			var cr9Products = new SystemProductCollection();
			{
				var ent = cr9Products.AddNew("ZZZ", "ZZZ Desc", true);
				ent.ModuleMappings.AddNew("Z91", "ZZZ CR9 module 1", ZString.Empty, true);
				ent.ModuleMappings.AddNew("Z92", "ZZZ CR9 module 2", ZString.Empty, true);
				ent.ModuleMappings.AddNew("Z93", "ZZZ CR9 module 3 Internal", ZString.Empty, true, true);
				EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr9Products);
			}

			var entCR8InternalModules = service.GetModuleList("ZZZ", "CR8", SharedConstants.Languages.English, "", InternalContact.PK.ToGuid()).ToArray();
			var entCR8ExternalModules = service.GetModuleList("ZZZ", "CR8", SharedConstants.Languages.English, "", ExternalContact.PK.ToGuid()).ToArray();
			var expectedEntCR8InternalModules = new[] { Tuple.Create("Z81", "ZZZ CR8 Mod 1"), Tuple.Create("Z82", "ZZZ CR8 Mod 2"), Tuple.Create("Z83", "ZZZ CR8 Mod 3 Internal") };
			Assert("expectedEntCR8ExternalModules should be empty", entCR8ExternalModules.IsNullOrEmpty());
			AssertArrayEqualsByElements("expectedEntCR8InternalModules", expectedEntCR8InternalModules, entCR8InternalModules);

			var entCR9InternalModules = service.GetModuleList("ZZZ", "CR9", SharedConstants.Languages.English, "", InternalContact.PK.ToGuid()).ToArray();
			var entCR9ExternalModules = service.GetModuleList("ZZZ", "CR9", SharedConstants.Languages.English, "", ExternalContact.PK.ToGuid()).ToArray();
			var expectedEntCR9InternalModules = new[] { Tuple.Create("Z91", "ZZZ CR9 module 1"), Tuple.Create("Z92", "ZZZ CR9 module 2"), Tuple.Create("Z93", "ZZZ CR9 module 3 Internal") };
			Assert("expectedEntCR9ExternalModules should be empty", entCR9ExternalModules.IsNullOrEmpty());
			AssertArrayEqualsByElements("expectedEntCR9InternalModules", expectedEntCR9InternalModules, entCR9InternalModules);
		}

		public void TestGetModuleListInternalFlag()
		{
			var products = RegistryDefaultsHelper.GetEnterpriseDefaults();
			var product1 = products.AddNew();
			product1.Code = "ZZZ";
			product1.Description = (NoResString)"ZZZ Product";
			product1.IsInternal = true;
			product1.ModuleMappings.AddNew("ZZ1", "ZZZ Module 1", "", true);
			product1.ModuleMappings.AddNew("ZZ2", "ZZZ Module 2", "", true);
			product1.ModuleMappings.AddNew("ZZ3", "ZZZ Module 3 Internal", "", true, true);
			var enterpriseProduct = products.GetProductByCode(ProductTypes.Codes.Enterprise);
			enterpriseProduct.ModuleMappings.AddNew("E01", "Ent 1", ZString.Empty, true);
			enterpriseProduct.ModuleMappings.AddNew("E02", "Ent 2 Internal", ZString.Empty, true, true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, products);
			var cr8Products = new SystemProductCollection();
			{
				var ent = cr8Products.AddNew(Licencing.Business.ProductTypes.Codes.Enterprise, Licencing.Business.ProductTypes.Descriptions.EnterpriseCW1, true);
				ent.ModuleMappings.AddNew("E81", "Ent CR8 Mod 1", ZString.Empty, true);
				ent.ModuleMappings.AddNew("E82", "Ent CR8 Mod 2", ZString.Empty, true);
				ent.ModuleMappings.AddNew("E83", "Ent CR8 Mod 3 Internal", ZString.Empty, true, true);
				var zzz = cr8Products.AddNew("ZZZ", "ZZZ Product", true);
				zzz.ModuleMappings.AddNew("Z81", "ZZZ CR8 Mod 1", ZString.Empty, true);
				zzz.ModuleMappings.AddNew("Z82", "ZZZ CR8 Mod 2", ZString.Empty, true);
				zzz.ModuleMappings.AddNew("Z83", "ZZZ CR8 Mod 3 Internal", ZString.Empty, true, true);
				EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr8Products);
			}

			var cr9Products = new SystemProductCollection();
			{
				var ent = cr9Products.AddNew(Licencing.Business.ProductTypes.Codes.Enterprise, Licencing.Business.ProductTypes.Descriptions.EnterpriseCW1, true);
				ent.ModuleMappings.AddNew("E91", "Ent CR9 Mod 1", ZString.Empty, true);
				ent.ModuleMappings.AddNew("E92", "Ent CR9 Mod 2", ZString.Empty, true);
				ent.ModuleMappings.AddNew("E93", "Ent CR9 Mod 3 Internal", ZString.Empty, true, true);
				var zzz = cr9Products.AddNew("ZZZ", "ZZZ Product", true);
				zzz.ModuleMappings.AddNew("Z91", "ZZZ CR9 Mod 1", ZString.Empty, true);
				zzz.ModuleMappings.AddNew("Z92", "ZZZ CR9 Mod 2", ZString.Empty, true);
				zzz.ModuleMappings.AddNew("Z93", "ZZZ CR9 Mod 3 Internal", ZString.Empty, true, true);
				EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr9Products);
			}

			var service = new EdiIncidentRequestService();
			var zzzInternalModulesCR1to7 = service.GetModuleList("ZZZ", "CR4", SharedConstants.Languages.English, "", InternalContact.PK.ToGuid()).ToArray();
			var zzzInternalModulesCR8 = service.GetModuleList("ZZZ", "CR8", SharedConstants.Languages.English, "", InternalContact.PK.ToGuid()).ToArray();
			var zzzInternalModulesCR9 = service.GetModuleList("ZZZ", "CR9", SharedConstants.Languages.English, "", InternalContact.PK.ToGuid()).ToArray();
			var zzzExternalModulesCR1to7 = service.GetModuleList("ZZZ", "CR4", SharedConstants.Languages.English, "", ExternalContact.PK.ToGuid()).ToArray();
			var zzzExternalModulesCR8 = service.GetModuleList("ZZZ", "CR8", SharedConstants.Languages.English, "", ExternalContact.PK.ToGuid()).ToArray();
			var zzzExternalModulesCR9 = service.GetModuleList("ZZZ", "CR9", SharedConstants.Languages.English, "", ExternalContact.PK.ToGuid()).ToArray();
			var entInternalModulesCR1to7 = service.GetModuleList("ENT", "CR4", SharedConstants.Languages.English, "", InternalContact.PK.ToGuid()).ToArray();
			var entInternalModulesCR8 = service.GetModuleList("ENT", "CR8", SharedConstants.Languages.English, "", InternalContact.PK.ToGuid()).ToArray();
			var entInternalModulesCR9 = service.GetModuleList("ENT", "CR9", SharedConstants.Languages.English, "", InternalContact.PK.ToGuid()).ToArray();
			var entExternalModulesCR1to7 = service.GetModuleList("ENT", "CR4", SharedConstants.Languages.English, "", ExternalContact.PK.ToGuid()).ToArray();
			var entExternalModulesCR8 = service.GetModuleList("ENT", "CR8", SharedConstants.Languages.English, "", ExternalContact.PK.ToGuid()).ToArray();
			var entExternalModulesCR9 = service.GetModuleList("ENT", "CR9", SharedConstants.Languages.English, "", ExternalContact.PK.ToGuid()).ToArray();
			var internalFullModules = service.GetModuleList(string.Empty, string.Empty, SharedConstants.Languages.English, "", InternalContact.PK.ToGuid()).ToArray();
			var externalFullModules = service.GetModuleList(string.Empty, string.Empty, SharedConstants.Languages.English, "", ExternalContact.PK.ToGuid()).ToArray();
			var externalContactStatusEmptyModules = service.GetModuleList(string.Empty, string.Empty, SharedConstants.Languages.English, "", ExternalContact.PK.ToGuid()).ToArray();
			var externalContactStatusNewModules = service.GetModuleList(string.Empty, string.Empty, SharedConstants.Languages.English, "NEW", ExternalContact.PK.ToGuid()).ToArray();
			var externalContactStatusCSVModules = service.GetModuleList(string.Empty, string.Empty, SharedConstants.Languages.English, "CSV", ExternalContact.PK.ToGuid()).ToArray();
			var externalContactStatusAPRModules = service.GetModuleList(string.Empty, string.Empty, SharedConstants.Languages.English, "APR", ExternalContact.PK.ToGuid()).ToArray();
			var expectedZZZExternalModulesCR1to7 = new[] { Tuple.Create("ZZ1", "ZZZ Module 1"), Tuple.Create("ZZ2", "ZZZ Module 2"), };
			var expectedZZZInternalModulesCR1to7 = expectedZZZExternalModulesCR1to7.Append(Tuple.Create("ZZ3", "ZZZ Module 3 Internal")).ToArray();
			var expectedZZZExternalModulesCR8 = new[] { Tuple.Create("Z81", "ZZZ CR8 Mod 1"), Tuple.Create("Z82", "ZZZ CR8 Mod 2"), };
			var expectedZZZInternalModulesCR8 = expectedZZZExternalModulesCR8.Append(Tuple.Create("Z83", "ZZZ CR8 Mod 3 Internal")).ToArray();
			var expectedZZZExternalModulesCR9 = new[] { Tuple.Create("Z91", "ZZZ CR9 Mod 1"), Tuple.Create("Z92", "ZZZ CR9 Mod 2"), };
			var expectedZZZInternalModulesCR9 = expectedZZZExternalModulesCR9.Append(Tuple.Create("Z93", "ZZZ CR9 Mod 3 Internal")).ToArray();
			var builder = new SupportIncidentModuleListBuilder();
			var expectedEntInternalModulesCR1to7 = builder.Build(CustomerService.Business.ModuleListType.MenuSection, "ENT", "").Cast<ICodeDescription>().Select(x => Tuple.Create(x.Code, x.Description)).ToArray();
			var expectedEntInternalModulesCR8 = builder.Build(CustomerService.Business.ModuleListType.Cr8, "ENT", "").Cast<ICodeDescription>().Select(x => Tuple.Create(x.Code, x.Description)).ToArray();
			var expectedEntInternalModulesCR9 = builder.Build(CustomerService.Business.ModuleListType.Cr9, "ENT", "").Cast<ICodeDescription>().Select(x => Tuple.Create(x.Code, x.Description)).ToArray();
			var expectedEntExternalModulesCR1to7 = GetExpectedExternalCR1to7Modules();
			var expectedEntExternalModulesCR8 = builder.Build(CustomerService.Business.ModuleListType.Cr8, "ENT", "", true, false).Cast<ICodeDescription>().Select(x => Tuple.Create(x.Code, x.Description)).ToArray();
			var expectedEntExternalModulesCR9 = builder.Build(CustomerService.Business.ModuleListType.Cr9, "ENT", "", true, false).Cast<ICodeDescription>().Select(x => Tuple.Create(x.Code, x.Description)).ToArray();
			var expectedInternalFullModules = new List<Tuple<string, string>>();
			var expectedEntInternalModules = new List<Tuple<string, string>>();
			expectedEntInternalModules.AddRange(expectedEntInternalModulesCR1to7);
			expectedEntInternalModules.AddRange(expectedEntInternalModulesCR8);
			expectedEntInternalModules.AddRange(expectedEntInternalModulesCR9);
			foreach (var tuple in expectedEntInternalModules)
			{
				expectedInternalFullModules.Add(Tuple.Create(tuple.Item1, "[" + ProductTypes.Descriptions.EnterpriseCW1 + "] " + tuple.Item2));
			}

			var expectedZZZInternalModules = new List<Tuple<string, string>>();
			expectedZZZInternalModules.AddRange(expectedZZZInternalModulesCR1to7);
			expectedZZZInternalModules.AddRange(expectedZZZInternalModulesCR8);
			expectedZZZInternalModules.AddRange(expectedZZZInternalModulesCR9);
			foreach (var tuple in expectedZZZInternalModules)
			{
				expectedInternalFullModules.Add(Tuple.Create(tuple.Item1, "[ZZZ Product] " + tuple.Item2));
			}

			var expectedExternalFullModules = new List<Tuple<string, string>>();
			var expectedEntExternalModules = new List<Tuple<string, string>>();
			expectedEntExternalModules.AddRange(expectedEntExternalModulesCR1to7);
			expectedEntExternalModules.AddRange(expectedEntExternalModulesCR8);
			expectedEntExternalModules.AddRange(expectedEntExternalModulesCR9);
			foreach (var tuple in expectedEntExternalModules)
			{
				expectedExternalFullModules.Add(Tuple.Create(tuple.Item1, "[" + ProductTypes.Descriptions.EnterpriseCW1 + "] " + tuple.Item2));
			}

			AssertArrayEqualsByElements("zzzInternalModulesCR1to7", expectedZZZInternalModulesCR1to7, zzzInternalModulesCR1to7);
			AssertArrayEqualsByElements("zzzInternalModulesCR8", expectedZZZInternalModulesCR8, zzzInternalModulesCR8);
			AssertArrayEqualsByElements("zzzInternalModulesCR9", expectedZZZInternalModulesCR9, zzzInternalModulesCR9);
			AssertArrayEqualsByElements("zzzExternalModulesCR1to7", expectedZZZExternalModulesCR1to7, zzzExternalModulesCR1to7);
			AssertArrayEqualsByElements("zzzExternalModulesCR8", expectedZZZExternalModulesCR8, zzzExternalModulesCR8);
			AssertArrayEqualsByElements("zzzExternalModulesCR9", expectedZZZExternalModulesCR9, zzzExternalModulesCR9);
			AssertArrayEqualsByElements("entInternalModulesCR1to7", expectedEntInternalModulesCR1to7, entInternalModulesCR1to7);
			AssertArrayEqualsByElements("entInternalModulesCR8", expectedEntInternalModulesCR8, entInternalModulesCR8);
			AssertArrayEqualsByElements("entInternalModulesCR9", expectedEntInternalModulesCR9, entInternalModulesCR9);
			AssertEquals("Internal list should contain module", true, entInternalModulesCR1to7.Contains(Tuple.Create("E01", "Ent 1")));
			AssertEquals("Internal list should contain internal module", true, entInternalModulesCR1to7.Contains(Tuple.Create("E02", "Ent 2 Internal")));
			AssertEquals("Internal list should contain internal module", true, entInternalModulesCR8.Contains(Tuple.Create("E83", "Ent CR8 Mod 3 Internal")));
			AssertEquals("Internal list should contain internal module", true, entInternalModulesCR9.Contains(Tuple.Create("E93", "Ent CR9 Mod 3 Internal")));
			AssertArrayEqualsByElements("entExternalModulesCR1to7", expectedEntExternalModulesCR1to7, entExternalModulesCR1to7);
			AssertArrayEqualsByElements("entExternalModulesCR8", expectedEntExternalModulesCR8, entExternalModulesCR8);
			AssertArrayEqualsByElements("entExternalModulesCR9", expectedEntExternalModulesCR9, entExternalModulesCR9);
			AssertEquals("External list should contain module", true, entExternalModulesCR1to7.Contains(Tuple.Create("E01", "Ent 1")));
			AssertEquals("External list should not contain internal module", false, entExternalModulesCR1to7.Contains(Tuple.Create("E02", "Ent 2 Internal")));
			AssertEquals("External list should not contain internal module", false, entExternalModulesCR8.Contains(Tuple.Create("E83", "Ent CR8 Mod 3 Internal")));
			AssertEquals("External list should not contain internal module", false, entExternalModulesCR9.Contains(Tuple.Create("E93", "Ent CR9 Mod 3 Internal")));
			AssertArrayEqualsByElements("InternalFullModules", expectedInternalFullModules.OrderBy(x => x.Item2).ToArray(), internalFullModules);
			AssertArrayEqualsByElements("ExternalFullModules", expectedExternalFullModules.OrderBy(x => x.Item2).ToArray(), externalFullModules);
			//if status not in (NEW, APR) then show full list
			AssertArrayEqualsByElements(externalContactStatusEmptyModules, externalFullModules);
			AssertArrayEqualsByElements(externalContactStatusNewModules, externalFullModules);
			AssertArrayEqualsByElements(externalContactStatusAPRModules, externalFullModules);
			AssertArrayEqualsByElements(externalContactStatusCSVModules, internalFullModules);
		}

		public void TestGetModuleListEnabledFlag()
		{
			var products = RegistryDefaultsHelper.GetEnterpriseDefaults();
			var product1 = products.AddNew();
			product1.Code = "ZZZ";
			product1.Description = (NoResString)"ZZZ Product";
			product1.IsInternal = true;
			product1.ModuleMappings.AddNew("ZZ1", "ZZZ Module 1 Enabled", "", true);
			product1.ModuleMappings.AddNew("ZZ2", "ZZZ Module 2 Enabled", "", true);
			product1.ModuleMappings.AddNew("ZZ3", "ZZZ Module 3 Enabled", "", true);
			var enterpriseProduct = products.GetProductByCode(ProductTypes.Codes.Enterprise);
			enterpriseProduct.ModuleMappings.AddNew("E01", "Ent 1 Disabled", ZString.Empty, true, false, false);
			enterpriseProduct.ModuleMappings.AddNew("E02", "Ent 2 Enabled", ZString.Empty, true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, products);
			var cr8Products = new SystemProductCollection();
			{
				var ent = cr8Products.AddNew(Licencing.Business.ProductTypes.Codes.Enterprise, Licencing.Business.ProductTypes.Descriptions.EnterpriseCW1, true);
				ent.ModuleMappings.AddNew("E81", "Ent CR8 Mod 1 Disabled", ZString.Empty, true, false, false);
				ent.ModuleMappings.AddNew("E82", "Ent CR8 Mod 2 Enabled", ZString.Empty, true);
				ent.ModuleMappings.AddNew("E83", "Ent CR8 Mod 3 Disabled", ZString.Empty, true, false, false);
				var zzz = cr8Products.AddNew("ZZZ", "ZZZ Product", true);
				zzz.ModuleMappings.AddNew("Z81", "ZZZ CR8 Mod 1 Enabled", ZString.Empty, true);
				zzz.ModuleMappings.AddNew("Z82", "ZZZ CR8 Mod 2 Disabled", ZString.Empty, true, false, false);
				zzz.ModuleMappings.AddNew("Z83", "ZZZ CR8 Mod 3 Enabled", ZString.Empty, true);
				EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr8Products);
			}

			var cr9Products = new SystemProductCollection();
			{
				var ent = cr9Products.AddNew(Licencing.Business.ProductTypes.Codes.Enterprise, Licencing.Business.ProductTypes.Descriptions.EnterpriseCW1, true);
				ent.ModuleMappings.AddNew("E91", "Ent CR9 Mod 1 Enabled", ZString.Empty, true);
				ent.ModuleMappings.AddNew("E92", "Ent CR9 Mod 2 Enabled", ZString.Empty, true);
				ent.ModuleMappings.AddNew("E93", "Ent CR9 Mod 3 Enabled", ZString.Empty, true);
				var zzz = cr9Products.AddNew("ZZZ", "ZZZ Product", true);
				zzz.ModuleMappings.AddNew("Z91", "ZZZ CR9 Mod 1 Disabled", ZString.Empty, true, false, false);
				zzz.ModuleMappings.AddNew("Z92", "ZZZ CR9 Mod 2 Disabled", ZString.Empty, true, false, false);
				zzz.ModuleMappings.AddNew("Z93", "ZZZ CR9 Mod 3 Disabled", ZString.Empty, true, true, false);
				zzz.ModuleMappings.AddNew("Z94", "ZZZ CR9 Mod 4 Enabled", ZString.Empty, true);
				EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr9Products);
			}

			var service = new EdiIncidentRequestService();
			var zzzEnabledModulesCR1to7 = service.GetModuleList("ZZZ", "CR4", SharedConstants.Languages.English, "", InternalContact.PK.ToGuid()).ToArray();
			var zzzEnabledModulesCR8 = service.GetModuleList("ZZZ", "CR8", SharedConstants.Languages.English, "", InternalContact.PK.ToGuid()).ToArray();
			var zzzEnabledModulesCR9 = service.GetModuleList("ZZZ", "CR9", SharedConstants.Languages.English, "", InternalContact.PK.ToGuid()).ToArray();
			var entEnabledModulesCR1to7 = service.GetModuleList("ENT", "CR4", SharedConstants.Languages.English, "", InternalContact.PK.ToGuid()).ToArray();
			var entEnabledModulesCR8 = service.GetModuleList("ENT", "CR8", SharedConstants.Languages.English, "", InternalContact.PK.ToGuid()).ToArray();
			var entEnabledModulesCR9 = service.GetModuleList("ENT", "CR9", SharedConstants.Languages.English, "", InternalContact.PK.ToGuid()).ToArray();
			var enabledFullModules = service.GetModuleList(string.Empty, string.Empty, SharedConstants.Languages.English, "", InternalContact.PK.ToGuid()).ToArray();
			var expectedZZZEnabledModulesCR1to7 = new[] { Tuple.Create("ZZ1", "ZZZ Module 1 Enabled"), Tuple.Create("ZZ2", "ZZZ Module 2 Enabled"), Tuple.Create("ZZ3", "ZZZ Module 3 Enabled"), };
			var expectedZZZEnabledModulesCR8 = new[] { Tuple.Create("Z81", "ZZZ CR8 Mod 1 Enabled"), Tuple.Create("Z83", "ZZZ CR8 Mod 3 Enabled"), };
			var expectedZZZEnabledModulesCR9 = new[] { Tuple.Create("Z94", "ZZZ CR9 Mod 4 Enabled"), };
			var builder = new SupportIncidentModuleListBuilder();
			var expectedEntEnabledModulesCR1to7 = builder.Build(CustomerService.Business.ModuleListType.MenuSection, "ENT", "", false, true).Cast<ICodeDescription>().Select(x => Tuple.Create(x.Code, x.Description)).ToArray();
			var expectedEntEnabledModulesCR8 = builder.Build(CustomerService.Business.ModuleListType.Cr8, "ENT", "", false, true).Cast<ICodeDescription>().Select(x => Tuple.Create(x.Code, x.Description)).ToArray();
			var expectedEntEnabledModulesCR9 = builder.Build(CustomerService.Business.ModuleListType.Cr9, "ENT", "", false, true).Cast<ICodeDescription>().Select(x => Tuple.Create(x.Code, x.Description)).ToArray();
			var expectedEnabledFullModules = new List<Tuple<string, string>>();
			var expectedEntEnabledModules = new List<Tuple<string, string>>();
			expectedEntEnabledModules.AddRange(expectedEntEnabledModulesCR1to7);
			expectedEntEnabledModules.AddRange(expectedEntEnabledModulesCR8);
			expectedEntEnabledModules.AddRange(expectedEntEnabledModulesCR9);
			foreach (var tuple in expectedEntEnabledModules)
			{
				expectedEnabledFullModules.Add(Tuple.Create(tuple.Item1, "[" + ProductTypes.Descriptions.EnterpriseCW1 + "] " + tuple.Item2));
			}

			var expectedZZZEnabledModules = new List<Tuple<string, string>>();
			expectedZZZEnabledModules.AddRange(expectedZZZEnabledModulesCR1to7);
			expectedZZZEnabledModules.AddRange(expectedZZZEnabledModulesCR8);
			expectedZZZEnabledModules.AddRange(expectedZZZEnabledModulesCR9);
			foreach (var tuple in expectedZZZEnabledModules)
			{
				expectedEnabledFullModules.Add(Tuple.Create(tuple.Item1, "[ZZZ Product] " + tuple.Item2));
			}

			AssertArrayEqualsByElements("zzzEnabledModulesCR1to7", expectedZZZEnabledModulesCR1to7, zzzEnabledModulesCR1to7);
			AssertArrayEqualsByElements("zzzEnabledModulesCR8", expectedZZZEnabledModulesCR8, zzzEnabledModulesCR8);
			AssertArrayEqualsByElements("zzzEnabledModulesCR9", expectedZZZEnabledModulesCR9, zzzEnabledModulesCR9);
			AssertEquals("Enabled list should not contain any disabled module", true, !zzzEnabledModulesCR9.Contains(Tuple.Create("Z91", "ZZZ CR9 Mod 1 Disabled")));
			AssertEquals("Enabled list should not contain any disabled module", true, !zzzEnabledModulesCR9.Contains(Tuple.Create("Z92", "ZZZ CR9 Mod 2 Disabled")));
			AssertEquals("Enabled list should not contain any disabled module", true, !zzzEnabledModulesCR9.Contains(Tuple.Create("Z93", "ZZZ CR9 Mod 3 Disabled")));
			AssertArrayEqualsByElements("entEnabledModulesCR1to7", expectedEntEnabledModulesCR1to7, entEnabledModulesCR1to7);
			AssertArrayEqualsByElements("entEnabledModulesCR8", expectedEntEnabledModulesCR8, entEnabledModulesCR8);
			AssertArrayEqualsByElements("entEnabledModulesCR9", expectedEntEnabledModulesCR9, entEnabledModulesCR9);
			AssertEquals("Enabled list should not contain any disabled module", true, !entEnabledModulesCR1to7.Contains(Tuple.Create("E01", "Ent 1 Disabled")));
			AssertEquals("Enabled list should not contain any disabled module", true, !entEnabledModulesCR8.Contains(Tuple.Create("E83", "Ent CR8 Mod 3 Disabled")));
			AssertArrayEqualsByElements("entEnabledModulesCR1to7", expectedEntEnabledModulesCR1to7, entEnabledModulesCR1to7);
			AssertArrayEqualsByElements("entEnabledModulesCR8", expectedEntEnabledModulesCR8, entEnabledModulesCR8);
			AssertArrayEqualsByElements("entEnabledModulesCR9", expectedEntEnabledModulesCR9, entEnabledModulesCR9);
			AssertEquals("Enabled list should not contain disabled module", true, !entEnabledModulesCR1to7.Contains(Tuple.Create("E01", "Ent 1 Disabled")));
			AssertEquals("Enabled list should not contain disabled module", false, entEnabledModulesCR8.Contains(Tuple.Create("E83", "Ent CR8 Mod 3 Disabled")));
			AssertArrayEqualsByElements("EnabledFullModules", expectedEnabledFullModules.OrderBy(x => x.Item2).ToArray(), enabledFullModules);
		}

		[TestDate(2018, 5, 22)]
		public void TestGetModuleListCacheExpiry()
		{
			var products = RegistryDefaultsHelper.GetEnterpriseDefaults();
			var product1 = products.AddNew();
			product1.Code = "ZZZ";
			product1.Description = (NoResString)"ZZZ Product";
			product1.IsInternal = true;
			product1.ModuleMappings.AddNew("ZZ1", "ZZZ Module 1", "", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, products);
			var cr8Products = new SystemProductCollection();
			var zzz8 = cr8Products.AddNew("ZZZ", "ZZZ Product", true);
			zzz8.ModuleMappings.AddNew("Z81", "ZZZ CR8 Mod 1", ZString.Empty, true);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr8Products);
			var cr9Products = new SystemProductCollection();
			var zzz9 = cr9Products.AddNew("ZZZ", "ZZZ Product", true);
			zzz9.ModuleMappings.AddNew("Z91", "ZZZ CR9 Mod 1", ZString.Empty, true);
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr9Products);
			var expectedZZZInternalModulesCR1to7 = new Tuple<string, string>[] { Tuple.Create("ZZ1", "ZZZ Module 1"), };
			var expectedZZZInternalModulesCR8 = new Tuple<string, string>[] { Tuple.Create("Z81", "ZZZ CR8 Mod 1"), };
			var expectedZZZInternalModulesCR9 = new Tuple<string, string>[] { Tuple.Create("Z91", "ZZZ CR9 Mod 1"), };
			AssertModules("ZZZ", expectedZZZInternalModulesCR1to7, expectedZZZInternalModulesCR8, expectedZZZInternalModulesCR9);
			product1.ModuleMappings.AddNew("ZZ2", "ZZZ Module 2", "", true);
			zzz8.ModuleMappings.AddNew("Z82", "ZZZ CR8 Mod 2", ZString.Empty, true);
			zzz9.ModuleMappings.AddNew("Z92", "ZZZ CR9 Mod 2", ZString.Empty, true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, products);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr8Products);
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr9Products);
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(14).AddSeconds(59);
			AssertModules("ZZZ", expectedZZZInternalModulesCR1to7, expectedZZZInternalModulesCR8, expectedZZZInternalModulesCR9);
			var newExpectedZZZInternalModulesCR1to7 = new Tuple<string, string>[] { Tuple.Create("ZZ1", "ZZZ Module 1"), Tuple.Create("ZZ2", "ZZZ Module 2"), };
			var newExpectedZZZInternalModulesCR8 = new Tuple<string, string>[] { Tuple.Create("Z81", "ZZZ CR8 Mod 1"), Tuple.Create("Z82", "ZZZ CR8 Mod 2"), };
			var newExpectedZZZInternalModulesCR9 = new Tuple<string, string>[] { Tuple.Create("Z91", "ZZZ CR9 Mod 1"), Tuple.Create("Z92", "ZZZ CR9 Mod 2"), };
			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			AssertModules("ZZZ", newExpectedZZZInternalModulesCR1to7, newExpectedZZZInternalModulesCR8, newExpectedZZZInternalModulesCR9);
		}

		void AssertModules(string product, Tuple<string, string>[] expected1to7, Tuple<string, string>[] expected8, Tuple<string, string>[] expected9)
		{
			var service = new EdiIncidentRequestService();
			AssertArrayEqualsByElements(product + " 1to7 internal", expected1to7, service.GetModuleList(product, "CR1", SharedConstants.Languages.English, "", InternalContact.PK.ToGuid()).ToArray());
			AssertArrayEqualsByElements(product + " 8 internal", expected8, service.GetModuleList(product, "CR8", SharedConstants.Languages.English, "", InternalContact.PK.ToGuid()).ToArray());
			AssertArrayEqualsByElements(product + " 9 internal", expected9, service.GetModuleList(product, "CR9", SharedConstants.Languages.English, "", InternalContact.PK.ToGuid()).ToArray());
			AssertArrayEqualsByElements(product + " 1to7 external", expected1to7, service.GetModuleList(product, "CR1", SharedConstants.Languages.English, "", ExternalContact.PK.ToGuid()).ToArray());
			AssertArrayEqualsByElements(product + " 8 external", expected8, service.GetModuleList(product, "CR8", SharedConstants.Languages.English, "", ExternalContact.PK.ToGuid()).ToArray());
			AssertArrayEqualsByElements(product + " 9 external", expected9, service.GetModuleList(product, "CR9", SharedConstants.Languages.English, "", ExternalContact.PK.ToGuid()).ToArray());
		}

		public void TestGetModulesLanguage()
		{
			var service = new EdiIncidentRequestService();
			var modules = service.GetModuleList("ENT", "CR4", Core.Constants.Languages.German, "", ExternalContact.PK.ToGuid()).ToArray();
			using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.German))
			{
				AssertArrayEqualsByElements("modules", GetExpectedExternalCR1to7Modules(), modules);
			}
		}

		Tuple<string, string>[] GetExpectedExternalCR1to7Modules()
		{
			var list = EDIDataRegistry.Instance.SystemProductMappings.Value.GetModuleList(ProductTypes.Codes.Enterprise, string.Empty, true, true);
			list.SortByDescription();
			return list.Cast<ICodeDescription>().Select(x => Tuple.Create(x.Code, x.Description)).ToArray();
		}

		SystemProductCollection SetupServiceTypeTestData()
		{
			var areas = new CodeDescriptionPairList();
			areas.AddPair("XRM", "XRM");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			// setup for ServiceTypeProductAreaModuleMappingLookups.Modules
			var systemProductcollection = new SystemProductCollection();
			var productA = systemProductcollection.AddNew("AAA", "AAA", true);
			productA.ModuleMappings.AddNew("AA1", "AA1 Description", "XRM", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemProductcollection);

			#region Service Type Test Data
			{
				var collectionServiceType = new SystemProductCollection();
				var parent1 = collectionServiceType.AddNew();
				parent1.Code = "AAA";
				parent1.Description = "Service Type Mapping";
				var child1 = parent1.ServiceTypeModuleMappings.AddNew();
				child1.ProductArea = "XRM";
				child1.ModuleCode = "AA1";
				child1.ModuleDescription = "des1";
				child1.ServiceTypeMappings.AddNew("TEA");
				EDIDataRegistry.Instance.ServiceTypeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionServiceType);

				// Config for IncidentDetailsHelper.FindProductArea
				var collectionIncident = new SystemProductCollection();
				var parentIncident = collectionIncident.AddNew("AAA", "AAA desc", false);
				parentIncident.ModuleMappings.AddNew("AA1", "des1", "XRM", false);
				EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionIncident);
			}
			#endregion

			#region CR8 Test Data
			{
				var collectionCr8 = new SystemProductCollection();
				var parentCr8 = collectionCr8.AddNew();
				parentCr8.Code = "AAA";
				parentCr8.Description = "CR8 Desc";
				var child1 = parentCr8.ServiceTypeModuleMappings.AddNew();
				child1.ProductArea = "XRM";
				child1.ModuleCode = "AA1";
				child1.ModuleDescription = "des1";
				child1.ServiceTypeMappings.AddNew("CON");
				EDIDataRegistry.Instance.ServiceTypeCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionCr8);

				// Config for IncidentDetailsHelper.FindProductArea
				var collectionCr8Incident = new SystemProductCollection();
				var parentCr8Incident = collectionCr8Incident.AddNew("AAA", "AAA desc", false);
				parentCr8Incident.ModuleMappings.AddNew("AA1", "des1", "XRM", false);
				EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionCr8Incident);
			}
			#endregion

			var collectionCr9 = new SystemProductCollection();
			#region CR9 Test Data
			{
				var parentCr9 = collectionCr9.AddNew();
				parentCr9.Code = "AAA";
				parentCr9.Description = "CR9 Desc";
				var child1 = parentCr9.ServiceTypeModuleMappings.AddNew();
				child1.ProductArea = "XRM";
				child1.ModuleCode = "AA1";
				child1.ModuleDescription = "des1";
				child1.ServiceTypeMappings.AddNew("DEP");
				EDIDataRegistry.Instance.ServiceTypeCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionCr9);

				// Config for IncidentDetailsHelper.FindProductArea
				var collectionCr9Incident = new SystemProductCollection();
				var parentCr9Incident = collectionCr9Incident.AddNew("AAA", "AAA desc", false);
				parentCr9Incident.ModuleMappings.AddNew("AA1", "des1", "XRM", false);
				EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionCr9Incident);
			}
			#endregion
			return collectionCr9;
		}

		public void TestGetServiceTypeListCacheExpiry_Criticality()
		{
			SetupServiceTypeTestData();

			var service = new EdiIncidentRequestService();

			var serviceTypes = service.GetServiceTypeList("AAA", "CR4", "AA1", "").ToArray();
			AssertEquals("TEA", serviceTypes[0].Item1);

			var serviceTypesCR8 = service.GetServiceTypeList("AAA", "CR8", "AA1", "").ToArray();
			AssertEquals("CON", serviceTypesCR8[0].Item1);

			var serviceTypesCR9 = service.GetServiceTypeList("AAA", "CR9", "AA1", "").ToArray();
			AssertEquals("DEP", serviceTypesCR9[0].Item1);
		}

		[TestDate(2022, 8, 1)]
		public void TestGetServiceTypeListCacheExpiry()
		{
			var collectionCr9 = SetupServiceTypeTestData();
			var service = new EdiIncidentRequestService();
			var serviceTypes1 = service.GetServiceTypeList("AAA", "CR9", "AA1", "").ToArray();

			var expectedserviceTypes1 = new Tuple<string, string>[] { Tuple.Create("DEP", "Deployment") };
			collectionCr9[0].ServiceTypeModuleMappings[0].ServiceTypeMappings.AddNew("TEA");
			EDIDataRegistry.Instance.ServiceTypeCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionCr9);

			var expectedserviceTypes2 = new Tuple<string, string>[] { Tuple.Create("DEP", "Deployment"), Tuple.Create("TEA", "Tear down") };
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(14).AddSeconds(59);

			var serviceTypes2 = service.GetServiceTypeList("AAA", "CR9", "AA1", "").ToArray();
			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);

			var serviceTypes3 = service.GetServiceTypeList("AAA", "CR9", "AA1", "").ToArray();

			AssertArrayEqualsByElements("serviceTypes1", expectedserviceTypes1, serviceTypes1);
			AssertArrayEqualsByElements("serviceTypes2 use cached value", expectedserviceTypes1, serviceTypes2);
			AssertArrayEqualsByElements("serviceTypes3 use new value", expectedserviceTypes2, serviceTypes3);
		}

		public void TestGetServiceType_NullOrEmptyParameters_ThrowNoExeptions()
		{
			SetupServiceTypeTestData();
			var service = new EdiIncidentRequestService();
			AssertEquals(true, service.GetServiceTypeList("AAA", "CR9", "AA1", "").Any());
			AssertEquals(false, service.GetServiceTypeList(null, "CR9", "AA1", "").Any());
			AssertEquals(false, service.GetServiceTypeList("AAA", null, "AA1", "").Any());
			AssertEquals(false, service.GetServiceTypeList("AAA", "CR9", null, "").Any());
		}

		public void TestGetDocumentUrls()
		{
			var tokenUrl = "https://api.test.com/endpoint/api/tokenCargowise";
			var documentUrl = "https://api.test.com/endpoint/api/resources/getSignedUrls";
			EDIDataRegistry.Instance.WiseTechAcademyTokenEndpointUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tokenUrl);
			EDIDataRegistry.Instance.WiseTechAcademyDocumentUrlsEndpointUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, documentUrl);
			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

			var tokenResponseMessage = new HttpResponseMessage()
			{
				Content = new StringContent("thisIsTheJwtToken")
			};

			var dummyUrlContent = new Dictionary<string, string>
			{
				{ "E039", "https://testsite.com/documentid=E039" },
				{ "12341", "https://testsite.com/documentid=12341" }
			};
			var jsonContent = JsonConvert.SerializeObject(dummyUrlContent);

			var documentUrlsResponseMessage = new HttpResponseMessage()
			{
				Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
			};

			Expression<Func<HttpRequestMessage, bool>> tokenRequestMatchUri = x => x.RequestUri == new Uri(tokenUrl);
			Expression<Func<HttpRequestMessage, bool>> documentUrlsRequestMatchUri = x => x.RequestUri == new Uri(documentUrl);
			Expression<Func<CancellationToken, bool>> anyToken = x => true;

			httpMessageHandlerMock
				.Protected() // required as SendAsync is protected
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is(tokenRequestMatchUri), ItExpr.Is(anyToken))
				.ReturnsAsync(tokenResponseMessage);
			httpMessageHandlerMock
				.Protected() // required as SendAsync is protected
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is(documentUrlsRequestMatchUri), ItExpr.Is(anyToken))
				.ReturnsAsync(documentUrlsResponseMessage);

			var httpClientFactory = new Mock<IHttpClientFactory>();
			httpClientFactory.Setup(x => x.CreateNew(It.IsAny<HttpMessageHandler>(), It.IsAny<TimeSpan>()))
				.Returns(new HttpClient(httpMessageHandlerMock.Object));

			var service = new EdiIncidentRequestService();

			using (ObjectFactory.Substitute(httpClientFactory.Object))
			{
				AssertNoExceptionThrown(() =>
				{
					var result = service.GetDocumentUrls(ExternalContact.PK.ToGuid(), new List<string> { "E039", "12341" });

					AssertEquals("Should return url", "https://testsite.com/documentid=E039", result["E039"]);
					AssertEquals("Should return url", "https://testsite.com/documentid=12341", result["12341"]);
				});
			}
		}

		public void TestGetDocumentUrls_InvalidContact()
		{
			var service = new EdiIncidentRequestService();

			AssertNoExceptionThrown(() =>
			{
				var result = service.GetDocumentUrls(Guid.NewGuid(), new List<string> { "E039", "12341" });
				AssertEquals("Should not find any urls if invalid contact", false, result.Any());
			});
		}

		public void TestGetDocumentUrls_NullOrEmptyDocumentIds()
		{
			var service = new EdiIncidentRequestService();

			AssertNoExceptionThrown(() =>
			{
				var result = service.GetDocumentUrls(ExternalContact.PK.ToGuid(), new List<string> { });
				AssertEquals("Should not find any urls if empty document ids", false, result.Any());
			});

			AssertNoExceptionThrown(() =>
			{
				var result = service.GetDocumentUrls(ExternalContact.PK.ToGuid(), null);
				AssertEquals("Should not find any urls if null document ids", false, result.Any());
			});
		}

		public void TestGetDocumentUrls_InvalidDocumentIds()
		{
			var tokenUrl = "https://api.test.com/endpoint/api/tokenCargowise";
			var documentUrl = "https://api.test.com/endpoint/api/resources/getSignedUrls";
			EDIDataRegistry.Instance.WiseTechAcademyTokenEndpointUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tokenUrl);
			EDIDataRegistry.Instance.WiseTechAcademyDocumentUrlsEndpointUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, documentUrl);
			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

			var tokenResponseMessage = new HttpResponseMessage()
			{
				Content = new StringContent("thisIsTheJwtToken")
			};

			var documentUrlBadRequestResponseMessage = new HttpResponseMessage()
			{
				StatusCode = System.Net.HttpStatusCode.BadRequest,
			};

			Expression<Func<HttpRequestMessage, bool>> tokenRequestMatchUri = x => x.RequestUri == new Uri(tokenUrl);
			Expression<Func<HttpRequestMessage, bool>> documentUrlsRequestMatchUri = x => x.RequestUri == new Uri(documentUrl);
			Expression<Func<CancellationToken, bool>> anyToken = x => true;

			httpMessageHandlerMock
				.Protected() // required as SendAsync is protected
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is(tokenRequestMatchUri), ItExpr.Is(anyToken))
				.ReturnsAsync(tokenResponseMessage);
			httpMessageHandlerMock
				.Protected() // required as SendAsync is protected
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is(documentUrlsRequestMatchUri), ItExpr.Is(anyToken))
				.ReturnsAsync(documentUrlBadRequestResponseMessage);

			var httpClientFactory = new Mock<IHttpClientFactory>();
			httpClientFactory.Setup(x => x.CreateNew(It.IsAny<HttpMessageHandler>(), It.IsAny<TimeSpan>()))
				.Returns(new HttpClient(httpMessageHandlerMock.Object));

			var service = new EdiIncidentRequestService();

			using (ObjectFactory.Substitute(httpClientFactory.Object))
			{
				AssertNoExceptionThrown(() =>
				{
					var result = service.GetDocumentUrls(ExternalContact.PK.ToGuid(), new List<string> { "Invalid" });
					AssertEquals("Should not find any urls if document id invalid", false, result.Any());
				});
			}
		}

		LicenceCompany InternalLicCompany;
		LicenceCompany ExternalLicCompany;
		OrgContact InternalContact;
		OrgContact ExternalContact;
		protected override void SetUp()
		{
			base.SetUp();
			InternalLicCompany = BillingTestHelper.CreateLicencedOrganization(Factory, "EN1", "CO1");
			InternalLicCompany.LicEnterprise.LE_IsInternal = true;
			InternalContact = InternalLicCompany.Header.Contacts[0];
			ExternalLicCompany = BillingTestHelper.CreateLicencedOrganization(Factory, "ENX", "COX");
			ExternalContact = ExternalLicCompany.Header.Contacts[0];
			EdiIncidentRequestService.ClearCacheForTest();
			Factory.Save();
		}

		protected override void TearDown()
		{
			EdiIncidentRequestService.ClearCacheForTest();
			base.TearDown();
		}
	}
}
