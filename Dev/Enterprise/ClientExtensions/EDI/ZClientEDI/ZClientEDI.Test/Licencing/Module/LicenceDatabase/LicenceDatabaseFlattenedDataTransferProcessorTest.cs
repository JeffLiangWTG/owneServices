using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module.Testing
{
	public class LicenceDatabaseFlattenedDataTransferProcessorTest : TestCaseWithFactory
	{
		public void TestImport()
		{
			var regValue = EDIDataRegistry.Instance.SystemProductMappings.Value;
			var productCSP = regValue.AddNew();
			productCSP.Code = "CSP";
			productCSP.Description = (NoResString)"CSP";
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue);
			var flattenedCollection = new LicenceDatabaseFlattenedCollection(Factory);
			var collectionInfo = new LicenceDatabaseImportInfo(flattenedCollection);
			var org1 = Factory.New<EDIOrgHeader>();
			org1.OH_Code = "SOMEORG1";
			var org2 = Factory.New<EDIOrgHeader>();
			org2.OH_Code = "SOMEORG2";
			var orgWithLicenceAndNoDb1 = Factory.New<EDIOrgHeader>();
			orgWithLicenceAndNoDb1.OH_Code = "SOMEORG3";
			orgWithLicenceAndNoDb1.CreateAndLoadLicenceForOrg();
			orgWithLicenceAndNoDb1.LicEnterprise.LE_EnterpriseCode = "EN3";
			orgWithLicenceAndNoDb1.LicCompany.LC_CompanyCode = "CO3";
			var orgWithLicenceAndNoDb2 = Factory.New<EDIOrgHeader>();
			orgWithLicenceAndNoDb2.OH_Code = "SOMEORG4";
			orgWithLicenceAndNoDb2.CreateAndLoadLicenceForOrg();
			orgWithLicenceAndNoDb2.LicEnterprise.LE_EnterpriseCode = "EN4";
			orgWithLicenceAndNoDb2.LicCompany.LC_CompanyCode = "CO4";
			var licHeader1 = BillingTestHelper.CreateLicence(Factory, "EN5", "CO5", "DB5", false);
			var orgWithLicenceAndDb1 = licHeader1.Company.Header;
			var licHeader2 = BillingTestHelper.CreateLicence(Factory, "EN6", "CO6", "DB6", false);
			var orgWithLicenceAndDb2 = licHeader2.Company.Header;
			AccTaxRate rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", Enterprise.MasterFiles.Business.AccTaxRate.Types.Rated, 10);
			var salesTax = BillingTestHelper.CreateChargeCode(Factory, rate, "SALESTAX");
			Factory.Save();
			var rec1 = flattenedCollection.AddNew();
			rec1.EnterpriseCode = "EN1";
			rec1.OrgCode = org1.OH_Code;
			rec1.ServerCode = "DB1";
			rec1.Product = ProductTypes.Codes.CargoWiseOne;
			rec1.Edition = LicenceAdvStdOthList.Codes.SeatTransaction;
			rec1.ReleaseType = ReleaseRings.Codes.ALP;
			rec1.InvoiceBranch = Env.CurrentBranch.Code;
			rec1.InvoiceCurrency = "USD";
			rec1.InvoiceGst = "GST";
			rec1.InvoiceSalesTax = "SALESTAX";
			rec1.HostedLocation = "SYD";
			var rec2 = flattenedCollection.AddNew();
			rec2.EnterpriseCode = "EN1";
			rec2.OrgCode = org2.OH_Code;
			rec2.ServerCode = "DB2";
			rec2.Product = ProductTypes.Codes.Enterprise;
			rec2.HostedLocation = "NCW";
			var rec3 = flattenedCollection.AddNew();
			rec3.EnterpriseCode = "EN3";
			rec3.OrgCode = orgWithLicenceAndNoDb1.OH_Code;
			rec3.ServerCode = "DB3";
			rec3.Product = ProductTypes.Codes.Enterprise;
			rec3.HostedLocation = "SYD";
			var recWithEntCodeMismatchIsAccepted = flattenedCollection.AddNew();
			recWithEntCodeMismatchIsAccepted.EnterpriseCode = "OKQ";
			recWithEntCodeMismatchIsAccepted.OrgCode = orgWithLicenceAndNoDb2.OH_Code;
			recWithEntCodeMismatchIsAccepted.ServerCode = "DB4";
			recWithEntCodeMismatchIsAccepted.Product = ProductTypes.Codes.CargoWiseOne;
			recWithEntCodeMismatchIsAccepted.HostedLocation = "SYD";
			var recWithSameDbIsRejected = flattenedCollection.AddNew();
			recWithSameDbIsRejected.EnterpriseCode = licHeader1.Company.LicEnterprise.LE_EnterpriseCode;
			recWithSameDbIsRejected.OrgCode = orgWithLicenceAndDb1.OH_Code;
			recWithSameDbIsRejected.ServerCode = licHeader1.Database.LD_ServerCode;
			recWithSameDbIsRejected.Product = ProductTypes.Codes.CargoWiseOne;
			recWithSameDbIsRejected.HostedLocation = "SYD";
			var recWithAnotherDbIsAccepted = flattenedCollection.AddNew();
			recWithAnotherDbIsAccepted.EnterpriseCode = licHeader2.Company.LicEnterprise.LE_EnterpriseCode;
			recWithAnotherDbIsAccepted.OrgCode = orgWithLicenceAndDb2.OH_Code;
			recWithAnotherDbIsAccepted.ServerCode = "OTH";
			recWithAnotherDbIsAccepted.Product = ProductTypes.Codes.CargoWiseOne;
			recWithAnotherDbIsAccepted.HostedLocation = "SYD";
			var recWithTenantIdAndSystemId = flattenedCollection.AddNew();
			recWithTenantIdAndSystemId.EnterpriseCode = "EN1";
			recWithTenantIdAndSystemId.OrgCode = org2.OH_Code;
			recWithTenantIdAndSystemId.ServerCode = "CS1";
			recWithTenantIdAndSystemId.Product = "CSP";
			recWithTenantIdAndSystemId.TenantID = "Tenant#1";
			recWithTenantIdAndSystemId.SystemID = "Sys#2";
			recWithTenantIdAndSystemId.HostedLocation = "SYD";
			var dbCollection = new LicenceDatabaseNonDependentCollection(Factory);
			var processor = new LicenceDatabaseFlattenedDataTransferProcessor(dbCollection, collectionInfo);
			processor.Import();
			IEnumerable<LicenceDatabase> licDatabaseList = dbCollection.Cast<LicenceDatabase>();
			AssertEquals("LicenceDatabase count", 6, dbCollection.Count);
			var db1 = licDatabaseList.FirstOrDefault(x => x.LD_ServerCode == "DB1");
			var db2 = licDatabaseList.FirstOrDefault(x => x.LD_ServerCode == "DB2");
			var db3 = licDatabaseList.FirstOrDefault(x => x.LD_ServerCode == "DB3");
			var db4 = licDatabaseList.FirstOrDefault(x => x.LD_ServerCode == "DB4");
			var db6 = licDatabaseList.FirstOrDefault(x => x.LD_ServerCode == "OTH");
			var dbCSP = licDatabaseList.FirstOrDefault(x => x.LD_ServerCode == "CS1");
			db1.Validation.ValidateAll();
			AssertNoErrors(db1);
			AssertEquals(ProductTypes.Codes.CargoWiseOne, db1.LD_Product);
			AssertEquals("EN1", db1.LicEnterprise.LE_EnterpriseCode);
			AssertEquals(UpgradeMethods.Codes.Blocked, db1.LD_AvailableUpgradeMethod);
			AssertEquals("RG1", org1.LicCompany.LC_CompanyCode);
			AssertEquals(DatabaseTypes.Codes.Production, db1.LD_LicenceType);
			AssertEquals(ReleaseRings.Codes.ALP, db1.LD_ReleaseRing);
			AssertEquals(LicenceAdvStdOthList.Codes.SeatTransaction, org1.LicCompany.GetHeader(db1).LA_LicenceAdvStdOth);
			AssertEquals(1, org1.LicCompany.InvoiceDeliveries.Count);
			var delivery = org1.LicCompany.InvoiceDeliveries[0];
			AssertEquals(Env.CurrentBranchPK, delivery.L9_GB_InvoicingBranch.ToGuid());
			AssertEquals("USD", delivery.L9_RX_NKInvoiceCurrency);
			AssertEquals(rate.PK, delivery.L9_AT_TaxId);
			AssertEquals(salesTax.PK, delivery.L9_AC_SalesTaxChargeCode);
			AssertEquals(true, org1.CompanyData.OB_IsDebtor);
			AssertEquals("USD", org1.CompanyData.OB_RX_NKARDDefltCurrency);
			AssertNull(db1.TrustedSystem);
			AssertEquals("SYD", db1.LD_HostedLocation);
			AssertEquals(ProductTypes.Codes.Enterprise, db2.LD_Product);
			AssertEquals("EN1", db2.LicEnterprise.LE_EnterpriseCode);
			AssertEquals("RG2", org2.LicCompany.LC_CompanyCode);
			AssertNull(db2.TrustedSystem);
			AssertEquals("NCW", db2.LD_HostedLocation);
			AssertEquals(ProductTypes.Codes.Enterprise, db3.LD_Product);
			AssertEquals("EN3", db3.LicEnterprise.LE_EnterpriseCode);
			AssertEquals("CO3", orgWithLicenceAndNoDb1.LicCompany.LC_CompanyCode);
			AssertNull(db3.TrustedSystem);
			AssertEquals(ProductTypes.Codes.CargoWiseOne, db4.LD_Product);
			AssertEquals("EN4", db4.LicEnterprise.LE_EnterpriseCode);
			AssertEquals("CO4", orgWithLicenceAndNoDb2.LicCompany.LC_CompanyCode);
			AssertNull(db4.TrustedSystem);
			AssertEquals(ProductTypes.Codes.CargoWiseOne, db6.LD_Product);
			AssertEquals("EN6", db6.LicEnterprise.LE_EnterpriseCode);
			AssertEquals("CO6", orgWithLicenceAndDb2.LicCompany.LC_CompanyCode);
			AssertNull(db6.TrustedSystem);
			AssertEquals("CSP", dbCSP.LD_Product);
			AssertEquals("Tenant#1", dbCSP.LD_TenantID);
			AssertEquals("Sys#2", dbCSP.TrustedSystem.ETS_SystemID);
		}

		public void TestImportCompanyCodeUnique_EnterpriseAlreadyExists()
		{
			var flattenedCollection = new LicenceDatabaseFlattenedCollection(Factory);
			var collectionInfo = new LicenceDatabaseImportInfo(flattenedCollection);
			var org1 = Factory.New<EDIOrgHeader>();
			org1.OH_Code = "SOMEORG1";
			var org2 = Factory.New<EDIOrgHeader>();
			org2.OH_Code = "SOMEORG2";
			var org3 = Factory.New<EDIOrgHeader>();
			org3.OH_Code = "SOMEORG3";
			var org4 = Factory.New<EDIOrgHeader>();
			org4.OH_Code = "SOMEORG4";
			var orgWithLicence1 = Factory.New<EDIOrgHeader>();
			orgWithLicence1.OH_Code = "SOMEORG5";
			orgWithLicence1.CreateAndLoadLicenceForOrg();
			orgWithLicence1.LicEnterprise.LE_EnterpriseCode = "ENT";
			orgWithLicence1.LicCompany.LC_CompanyCode = "RG1";
			Factory.Save();
			var orgWithLicence2 = Factory.New<EDIOrgHeader>();
			orgWithLicence2.OH_Code = "SOMEORG6";
			orgWithLicence2.LicenceEnterpriseCode = "ENT";
			orgWithLicence2.LicCompany.LC_CompanyCode = "RG2";
			Factory.Save();
			var rec1 = flattenedCollection.AddNew();
			rec1.EnterpriseCode = "ENT";
			rec1.OrgCode = org1.OH_Code;
			rec1.ServerCode = "DB1";
			rec1.Product = ProductTypes.Codes.CargoWiseOne;
			rec1.HostedLocation = "SYD";
			var rec2 = flattenedCollection.AddNew();
			rec2.EnterpriseCode = "ENT";
			rec2.OrgCode = org2.OH_Code;
			rec2.ServerCode = "DB2";
			rec2.Product = ProductTypes.Codes.CargoWiseOne;
			rec2.HostedLocation = "SYD";
			var rec3 = flattenedCollection.AddNew();
			rec3.EnterpriseCode = "ENT";
			rec3.OrgCode = org3.OH_Code;
			rec3.ServerCode = "DB3";
			rec3.Product = ProductTypes.Codes.CargoWiseOne;
			rec3.HostedLocation = "SYD";
			var rec4 = flattenedCollection.AddNew();
			rec4.EnterpriseCode = "ENT";
			rec4.OrgCode = org4.OH_Code;
			rec4.ServerCode = "DB4";
			rec4.Product = ProductTypes.Codes.CargoWiseOne;
			rec4.HostedLocation = "SYD";
			var dbCollection = new LicenceDatabaseNonDependentCollection(Factory);
			var processor = new LicenceDatabaseFlattenedDataTransferProcessor(dbCollection, collectionInfo);
			processor.Import();
			IEnumerable<LicenceDatabase> licDatabaseList = dbCollection.Cast<LicenceDatabase>();
			AssertEquals("LicenceDatabase count", 4, dbCollection.Count);
			AssertEquals("LicenceDatabase count", 4, processor.HeadersCreated);
			var db1 = licDatabaseList.FirstOrDefault(x => x.LD_ServerCode == "DB1");
			var db2 = licDatabaseList.FirstOrDefault(x => x.LD_ServerCode == "DB2");
			var db3 = licDatabaseList.FirstOrDefault(x => x.LD_ServerCode == "DB3");
			var db4 = licDatabaseList.FirstOrDefault(x => x.LD_ServerCode == "DB4");
			AssertEquals(ProductTypes.Codes.CargoWiseOne, db1.LD_Product);
			AssertEquals(ProductTypes.Codes.CargoWiseOne, db2.LD_Product);
			AssertEquals(ProductTypes.Codes.CargoWiseOne, db3.LD_Product);
			AssertEquals(ProductTypes.Codes.CargoWiseOne, db4.LD_Product);
			AssertEquals("ENT", db1.LicEnterprise.LE_EnterpriseCode);
			AssertEquals("ENT", db2.LicEnterprise.LE_EnterpriseCode);
			AssertEquals("ENT", db3.LicEnterprise.LE_EnterpriseCode);
			AssertEquals("ENT", db4.LicEnterprise.LE_EnterpriseCode);
			AssertEquals(org1.LicCompany.LC_LE, orgWithLicence1.LicCompany.LC_LE);
			AssertEquals(org2.LicCompany.LC_LE, orgWithLicence1.LicCompany.LC_LE);
			AssertEquals(org3.LicCompany.LC_LE, orgWithLicence1.LicCompany.LC_LE);
			AssertEquals(org4.LicCompany.LC_LE, orgWithLicence1.LicCompany.LC_LE);
			AssertEquals("CO1", org1.LicCompany.LC_CompanyCode);
			AssertEquals("CO2", org2.LicCompany.LC_CompanyCode);
			AssertEquals("RG3", org3.LicCompany.LC_CompanyCode);
			AssertEquals("RG4", org4.LicCompany.LC_CompanyCode);
			AssertEquals(6, orgWithLicence1.LicEnterprise.Companies.Count);
		}

		public void TestImportCompanyCodeUnique_EnterpriseNotExists()
		{
			var flattenedCollection = new LicenceDatabaseFlattenedCollection(Factory);
			var collectionInfo = new LicenceDatabaseImportInfo(flattenedCollection);
			var org1 = Factory.New<EDIOrgHeader>();
			org1.OH_Code = "LOC1ORG1";
			var org2 = Factory.New<EDIOrgHeader>();
			org2.OH_Code = "LOC2ORG1";
			var org3 = Factory.New<EDIOrgHeader>();
			org3.OH_Code = "LOC3ORG1";
			var org4 = Factory.New<EDIOrgHeader>();
			org4.OH_Code = "LOC4ORG1";
			Factory.Save();
			var rec1 = flattenedCollection.AddNew();
			rec1.EnterpriseCode = "ENT";
			rec1.OrgCode = org1.OH_Code;
			rec1.ServerCode = "DB1";
			rec1.Product = ProductTypes.Codes.CargoWiseOne;
			rec1.HostedLocation = "SYD";
			var rec2 = flattenedCollection.AddNew();
			rec2.EnterpriseCode = "ENT";
			rec2.OrgCode = org2.OH_Code;
			rec2.ServerCode = "DB2";
			rec2.Product = ProductTypes.Codes.CargoWiseOne;
			rec2.HostedLocation = "SYD";
			var rec3 = flattenedCollection.AddNew();
			rec3.EnterpriseCode = "ENT";
			rec3.OrgCode = org3.OH_Code;
			rec3.ServerCode = "DB3";
			rec3.Product = ProductTypes.Codes.CargoWiseOne;
			rec3.HostedLocation = "SYD";
			var rec4 = flattenedCollection.AddNew();
			rec4.EnterpriseCode = "ENT";
			rec4.OrgCode = org4.OH_Code;
			rec4.ServerCode = "DB4";
			rec4.Product = ProductTypes.Codes.CargoWiseOne;
			rec4.HostedLocation = "SYD";
			var dbCollection = new LicenceDatabaseNonDependentCollection(Factory);
			var processor = new LicenceDatabaseFlattenedDataTransferProcessor(dbCollection, collectionInfo);
			processor.Import();
			IEnumerable<LicenceDatabase> licDatabaseList = dbCollection.Cast<LicenceDatabase>();
			AssertEquals("LicenceDatabase count", 4, dbCollection.Count);
			AssertEquals("LicenceDatabase count", 4, processor.HeadersCreated);
			var db1 = licDatabaseList.FirstOrDefault(x => x.LD_ServerCode == "DB1");
			var db2 = licDatabaseList.FirstOrDefault(x => x.LD_ServerCode == "DB2");
			var db3 = licDatabaseList.FirstOrDefault(x => x.LD_ServerCode == "DB3");
			var db4 = licDatabaseList.FirstOrDefault(x => x.LD_ServerCode == "DB4");
			AssertEquals(ProductTypes.Codes.CargoWiseOne, db1.LD_Product);
			AssertEquals(ProductTypes.Codes.CargoWiseOne, db2.LD_Product);
			AssertEquals(ProductTypes.Codes.CargoWiseOne, db3.LD_Product);
			AssertEquals(ProductTypes.Codes.CargoWiseOne, db4.LD_Product);
			AssertEquals("ENT", db1.LicEnterprise.LE_EnterpriseCode);
			AssertEquals("ENT", db2.LicEnterprise.LE_EnterpriseCode);
			AssertEquals("ENT", db3.LicEnterprise.LE_EnterpriseCode);
			AssertEquals("ENT", db4.LicEnterprise.LE_EnterpriseCode);
			AssertEquals("RG1", org1.LicCompany.LC_CompanyCode);
			AssertEquals("CO1", org2.LicCompany.LC_CompanyCode);
			AssertEquals("CO2", org3.LicCompany.LC_CompanyCode);
			AssertEquals("CO3", org4.LicCompany.LC_CompanyCode);
			AssertEquals(4, org1.LicEnterprise.Companies.Count);
		}

		public void TestImport_EntCode_EntID()
		{
			var flattenedCollection = new LicenceDatabaseFlattenedCollection(Factory);
			var collectionInfo = new LicenceDatabaseImportInfo(flattenedCollection);
			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CM1", "DB1");
			var org1 = lic1.Company.Header;
			var lic2 = BillingTestHelper.CreateLicence(Factory, "EN2", "CM2", "DB2");
			var org2 = lic2.Company.Header;
			var lic3 = BillingTestHelper.CreateLicence(Factory, "EN3", "CM3", "DB3");
			var org3 = lic3.Company.Header;
			var org4 = Factory.NewWithValidTestData<EDIOrgHeader>();
			Factory.Save();
			var rec1 = flattenedCollection.AddNew();
			rec1.EnterpriseCode = "EN1";
			rec1.EnterpriseID = "";
			rec1.OrgCode = org1.OH_Code;
			rec1.ServerCode = "D11";
			rec1.Product = ProductTypes.Codes.CargoWiseOne;
			rec1.HostedLocation = "SYD";
			var rec2 = flattenedCollection.AddNew();
			rec2.EnterpriseCode = "";
			rec2.EnterpriseID = lic2.Company.LicEnterprise.LE_EnterpriseID;
			rec2.OrgCode = org2.OH_Code;
			rec2.ServerCode = "D22";
			rec2.Product = ProductTypes.Codes.CargoWiseOne;
			rec2.HostedLocation = "SYD";
			var rec3 = flattenedCollection.AddNew();
			rec3.EnterpriseCode = "";
			rec3.EnterpriseID = "";
			rec3.OrgCode = org3.OH_Code;
			rec3.ServerCode = "D33";
			rec3.Product = ProductTypes.Codes.CargoWiseOne;
			rec3.HostedLocation = "SYD";
			var rec4 = flattenedCollection.AddNew();
			rec4.EnterpriseCode = "";
			rec4.EnterpriseID = "";
			rec4.OrgCode = org4.OH_Code;
			rec4.ServerCode = "D44";
			rec4.Product = ProductTypes.Codes.CargoWiseOne;
			rec4.HostedLocation = "SYD";
			var dbCollection = new LicenceDatabaseNonDependentCollection(Factory);
			var processor = new LicenceDatabaseFlattenedDataTransferProcessor(dbCollection, collectionInfo);
			processor.Import();
			IEnumerable<LicenceDatabase> licDatabaseList = dbCollection.Cast<LicenceDatabase>();
			AssertEquals("LicenceDatabase count", 4, dbCollection.Count);
			var db1 = licDatabaseList.Single(x => x.LD_ServerCode == "D11");
			var db2 = licDatabaseList.Single(x => x.LD_ServerCode == "D22");
			var db3 = licDatabaseList.Single(x => x.LD_ServerCode == "D33");
			var db4 = licDatabaseList.Single(x => x.LD_ServerCode == "D44");
			AssertEquals(ProductTypes.Codes.CargoWiseOne, db1.LD_Product);
			AssertEquals(ProductTypes.Codes.CargoWiseOne, db2.LD_Product);
			AssertEquals(ProductTypes.Codes.CargoWiseOne, db3.LD_Product);
			AssertEquals(ProductTypes.Codes.CargoWiseOne, db4.LD_Product);
			AssertEquals("EN1", db1.LicEnterprise.LE_EnterpriseCode);
			AssertEquals("EN2", db2.LicEnterprise.LE_EnterpriseCode);
			AssertEquals("EN3", db3.LicEnterprise.LE_EnterpriseCode);
			AssertEquals("", db4.LicEnterprise.LE_EnterpriseCode);
			AssertEquals(org1.LicEnterprise.LE_EnterpriseID, db1.LicEnterprise.LE_EnterpriseID);
			AssertEquals(org2.LicEnterprise.LE_EnterpriseID, db2.LicEnterprise.LE_EnterpriseID);
			AssertEquals(org3.LicEnterprise.LE_EnterpriseID, db3.LicEnterprise.LE_EnterpriseID);
			AssertEquals(org4.LicEnterprise.LE_EnterpriseID, db4.LicEnterprise.LE_EnterpriseID);
		}

		public void TestImport_UpdateSystemRefId()
		{
			var list = new SystemProductCollection();
			list.AddNew("CSP", (NoResString)"CargoSphere", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			var collection = new LicenceDatabaseFlattenedCollection(Factory);
			var collectionInfo = new LicenceDatabaseImportInfo(collection);
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var org1 = licence1.Company.Header;
			org1.OH_Code = "DDDABCSYD";
			var db1 = licence1.Database;
			db1.LD_Product = "CSP";
			db1.LD_TenantID = string.Empty;
			db1.LD_OH_WebAccessOrg = org1.PK;
			Factory.Save();
			var row1 = collection.AddNew();
			row1.EnterpriseCode = "DDD";
			row1.OrgCode = org1.OH_Code;
			row1.ServerCode = "SYD";
			row1.Product = "CSP";
			row1.TenantID = "CSP00010040";
			row1.HostedLocation = "SYD";
			var dbCollection = new LicenceDatabaseNonDependentCollection(Factory);
			var processor = new LicenceDatabaseFlattenedDataTransferProcessor(dbCollection, collectionInfo);
			processor.Import();
			AssertEquals("Existing database should have system ref id updated", "CSP00010040", db1.LD_TenantID);
		}

		EDIOrgHeader CreateLicencedOrg(string orgCode, string enterpriseAndCompanyCode)
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = orgCode;
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LC_CompanyCode = enterpriseAndCompanyCode;
			org.LicCompany.LicEnterprise.LE_EnterpriseCode = enterpriseAndCompanyCode;
			return org;
		}

		public void TestImport_GenerateEnterpriseCode()
		{
			var flattenedCollection = new LicenceDatabaseFlattenedCollection(Factory);
			var collectionInfo = new LicenceDatabaseImportInfo(flattenedCollection);
			CreateLicencedOrg("CCCORG", "CCC");
			for (int i = 0; i < 99; ++i)
			{
				CreateLicencedOrg("CCC" + i, (string)(new ZString("CC" + i).Right(3)));
			}

			var org1 = Factory.New<EDIOrgHeader>();
			org1.OH_Code = "CCC99";
			var org2 = Factory.New<EDIOrgHeader>();
			org2.OH_Code = "CCC100";
			var org3 = Factory.New<EDIOrgHeader>();
			org3.OH_Code = "CCC101";
			Factory.Save();
			var rec1 = flattenedCollection.AddNew();
			rec1.EnterpriseCode = "";
			rec1.OrgCode = org1.OH_Code;
			rec1.ServerCode = "DB1";
			rec1.Product = ProductTypes.Codes.CargoWiseOne;
			rec1.HostedLocation = "SYD";
			var rec2 = flattenedCollection.AddNew();
			rec2.EnterpriseCode = "";
			rec2.OrgCode = org2.OH_Code;
			rec2.ServerCode = "DB2";
			rec2.Product = ProductTypes.Codes.CargoWiseOne;
			rec2.HostedLocation = "SYD";
			var rec3 = flattenedCollection.AddNew();
			rec3.EnterpriseCode = "ENT";
			rec3.OrgCode = org3.OH_Code;
			rec3.ServerCode = "DB3";
			rec3.Product = ProductTypes.Codes.CargoWiseOne;
			rec3.HostedLocation = "SYD";
			var dbCollection = new LicenceDatabaseNonDependentCollection(Factory);
			var processor = new LicenceDatabaseFlattenedDataTransferProcessor(dbCollection, collectionInfo);
			processor.Import();
			IEnumerable<LicenceDatabase> licDatabaseList = dbCollection.Cast<LicenceDatabase>();
			AssertEquals("LicenceDatabase count", 3, dbCollection.Count);
			AssertEquals("LicenceDatabase count", 3, processor.HeadersCreated);
			var db1 = licDatabaseList.FirstOrDefault(x => x.LD_ServerCode == "DB1");
			var db2 = licDatabaseList.FirstOrDefault(x => x.LD_ServerCode == "DB3");
			var db3 = licDatabaseList.FirstOrDefault(x => x.LD_ServerCode == "DB2");
			AssertEquals("", db1.LicEnterprise.LE_EnterpriseCode);
			AssertEquals("ENT", db2.LicEnterprise.LE_EnterpriseCode);
			AssertEquals("", db3.LicEnterprise.LE_EnterpriseCode);
			AssertEquals(2, org1.LicEnterprise.Companies.Count);
			AssertEquals(null, processor.Log);
		}

		public void TestImport_DuplicateServerCodeDoesNotSetEnterpriseCode()
		{
			var flattenedCollection = new LicenceDatabaseFlattenedCollection(Factory);
			var collectionInfo = new LicenceDatabaseImportInfo(flattenedCollection);
			var org1 = Factory.New<EDIOrgHeader>();
			org1.OH_Code = "SOMEORG1";
			var orgWithLicence1 = Factory.New<EDIOrgHeader>();
			orgWithLicence1.OH_Code = "SOMEORG5";
			orgWithLicence1.CreateAndLoadLicenceForOrg();
			orgWithLicence1.LicEnterprise.LE_EnterpriseCode = "ENT";
			orgWithLicence1.LicCompany.LC_CompanyCode = "RG1";
			Factory.Save();
			var rec1 = flattenedCollection.AddNew();
			rec1.EnterpriseCode = "ENT";
			rec1.EnterpriseID = orgWithLicence1.LicEnterprise.LE_EnterpriseID;
			rec1.OrgCode = orgWithLicence1.OH_Code;
			rec1.ServerCode = "DB1";
			rec1.Product = ProductTypes.Codes.CargoWiseOne;
			rec1.HostedLocation = "SYD";
			var rec2 = flattenedCollection.AddNew();
			rec2.EnterpriseCode = "ENT";
			rec2.EnterpriseID = orgWithLicence1.LicEnterprise.LE_EnterpriseID;
			rec2.OrgCode = org1.OH_Code;
			rec2.ServerCode = "DB1";
			rec2.Product = ProductTypes.Codes.CargoWiseOne;
			rec2.HostedLocation = "SYD";
			var dbCollection = new LicenceDatabaseNonDependentCollection(Factory);
			var processor = new LicenceDatabaseFlattenedDataTransferProcessor(dbCollection, collectionInfo);
			processor.Import();
			IEnumerable<LicenceDatabase> licDatabaseList = dbCollection.Cast<LicenceDatabase>();
			AssertEquals("LicenceDatabase count", 1, dbCollection.Count);
			AssertEquals("LicenceDatabase count", 1, processor.HeadersCreated);
			var db1 = licDatabaseList.FirstOrDefault(x => x.LD_ServerCode == "DB1");
			org1.Reload();
			AssertEquals("Record [Org. Code: SOMEORG1, Server Code: DB1, Enterprise Code: ENT, Enterprise ID: E000001]: Attaching existing database in the enterprise with given server code\r\n", processor.Log);
			AssertNotNull(org1.LicCompany);
		}

		public void TestImport_TenantID_AutoGenerateEntCode()
		{
			var productCollection = new SystemProductCollection();
			var product = productCollection.AddNew();
			product.Code = "BOR";
			product.Description = "BorderWise";
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productCollection);
			var flattenedCollection = new LicenceDatabaseFlattenedCollection(Factory);
			var collectionInfo = new LicenceDatabaseImportInfo(flattenedCollection);
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = "TEST_ORG_001";
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			Factory.Save();
			AssertEquals(true, org1.LicenceEnterpriseCode.IsEmpty);
			AssertEquals(true, org1.LicenceEnterpriseID.IsEmpty);
			var rec1 = flattenedCollection.AddNew();
			rec1.EnterpriseCode = "";
			rec1.EnterpriseID = "";
			rec1.OrgCode = org1.OH_Code;
			rec1.ServerCode = "D11";
			rec1.Product = ProductTypes.Codes.BorderWise;
			rec1.TenantID = "BOR_REF#001";
			rec1.AutoGenerateEntCode = true;
			rec1.HostedLocation = "SYD";
			var rec2 = flattenedCollection.AddNew();
			rec2.EnterpriseCode = "EN1";
			rec2.OrgCode = org1.OH_Code;
			rec2.ServerCode = "D22";
			rec2.Product = ProductTypes.Codes.BorderWise;
			rec2.AutoGenerateEntCode = true;
			rec2.HostedLocation = "SYD";
			var rec3 = flattenedCollection.AddNew();
			rec3.EnterpriseID = "E001";
			rec3.OrgCode = org1.OH_Code;
			rec3.ServerCode = "D33";
			rec3.Product = ProductTypes.Codes.BorderWise;
			rec3.AutoGenerateEntCode = true;
			rec3.HostedLocation = "SYD";
			var dbCollection = new LicenceDatabaseNonDependentCollection(Factory);
			var processor = new LicenceDatabaseFlattenedDataTransferProcessor(dbCollection, collectionInfo);
			processor.Import();
			Factory.Save();
			var licDatabaseList = dbCollection.Cast<LicenceDatabase>();
			var db1 = licDatabaseList.Single(x => x.LD_ServerCode == "D11");
			AssertEquals(ProductTypes.Codes.BorderWise, db1.LD_Product);
			AssertEquals("BOR_REF#001", db1.LD_TenantID);
			AssertEquals(false, org1.LicenceEnterpriseCode.IsEmpty);
			AssertEquals(false, org1.LicenceEnterpriseID.IsEmpty);
			AssertEquals(org1.LicEnterprise.LE_EnterpriseCode, db1.LicEnterprise.LE_EnterpriseCode);
			AssertEquals(org1.LicEnterprise.LE_EnterpriseID, db1.LicEnterprise.LE_EnterpriseID);
			AssertEquals(@"Record [Org. Code: TEST_ORG_001, Server Code: D22, Enterprise Code: EN1, Enterprise ID: ] excluded: You cannot specify an Enterprise ID / Code when the flag 'Auto Generate Enterprise Code' is on.
Record [Org. Code: TEST_ORG_001, Server Code: D33, Enterprise Code: , Enterprise ID: E001] excluded: You cannot specify an Enterprise ID / Code when the flag 'Auto Generate Enterprise Code' is on.
", processor.Log);
		}

		public void TestValidateFields()
		{
			var productCollection = new SystemProductCollection();
			var product = productCollection.AddNew();
			product.Code = "BOR";
			product.Description = "BorderWise";
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productCollection);
			var flattenedCollection = new LicenceDatabaseFlattenedCollection(Factory);
			var collectionInfo = new LicenceDatabaseImportInfo(flattenedCollection);
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = "TEST_ORG_001";
			Factory.Save();
			AssertEquals(true, org1.LicenceEnterpriseCode.IsEmpty);
			AssertEquals(true, org1.LicenceEnterpriseID.IsEmpty);
			var rec1 = flattenedCollection.AddNew();
			rec1.EnterpriseCode = "";
			rec1.EnterpriseID = "";
			rec1.OrgCode = org1.OH_Code;
			rec1.ServerCode = "D11";
			rec1.Product = ProductTypes.Codes.BorderWise;
			rec1.TenantID = "BOR_REF#001";
			rec1.SystemID = "SYS#002";
			rec1.AutoGenerateEntCode = true;
			rec1.RegistrationStatus = "PRE";
			rec1.PreRegistrationExpiryDateUTC = new ZDateTime(2050, 1, 1);
			rec1.SystemType = "TST";
			rec1.AllowWebAutoLogin = true;
			rec1.HostedLocation = "SYD";
			var rec2 = flattenedCollection.AddNew();
			rec2.OrgCode = "!!!@@@@";
			rec2.ServerCode = "D22";
			rec2.Product = ProductTypes.Codes.BorderWise;
			rec2.AutoGenerateEntCode = true;
			var rec3 = flattenedCollection.AddNew();
			rec3.OrgCode = org1.OH_Code;
			rec3.ServerCode = "D33";
			rec3.Product = "";
			rec3.AutoGenerateEntCode = true;
			var rec4 = flattenedCollection.AddNew();
			rec4.OrgCode = org1.OH_Code;
			rec4.ServerCode = "D44";
			rec4.Product = "!!!";
			rec4.AutoGenerateEntCode = true;
			var rec5 = flattenedCollection.AddNew();
			rec5.EnterpriseCode = "";
			rec5.EnterpriseID = "";
			rec5.OrgCode = org1.OH_Code;
			rec5.ServerCode = "D55";
			rec5.Product = ProductTypes.Codes.CargoWiseOne;
			rec5.TenantID = "BOR_REF#001";
			rec5.AutoGenerateEntCode = true;
			var rec6 = flattenedCollection.AddNew();
			rec6.EnterpriseCode = "";
			rec6.EnterpriseID = "";
			rec6.OrgCode = org1.OH_Code;
			rec6.ServerCode = "D66";
			rec6.Product = ProductTypes.Codes.BorderWise;
			rec6.AutoGenerateEntCode = true;
			rec6.ReleaseType = "@@@";
			var rec7 = flattenedCollection.AddNew();
			rec7.EnterpriseCode = "";
			rec7.EnterpriseID = "";
			rec7.OrgCode = org1.OH_Code;
			rec7.ServerCode = "D77";
			rec7.Product = ProductTypes.Codes.BorderWise;
			rec7.AutoGenerateEntCode = true;
			rec7.Edition = "###";
			var rec8 = flattenedCollection.AddNew();
			rec8.EnterpriseCode = "";
			rec8.EnterpriseID = "";
			rec8.OrgCode = org1.OH_Code;
			rec8.ServerCode = "D88";
			rec8.Product = ProductTypes.Codes.BorderWise;
			rec8.TenantID = "BOR_REF#001";
			rec8.AutoGenerateEntCode = true;
			var rec9 = flattenedCollection.AddNew();
			rec9.EnterpriseCode = "";
			rec9.EnterpriseID = "";
			rec9.OrgCode = org1.OH_Code;
			rec9.ServerCode = "D99";
			rec9.Product = ProductTypes.Codes.BorderWise;
			rec9.TenantID = "BOR_REF#002";
			rec9.AutoGenerateEntCode = true;
			rec9.RegistrationStatus = "REG";
			var rec10 = flattenedCollection.AddNew();
			rec10.EnterpriseCode = "";
			rec10.EnterpriseID = "";
			rec10.OrgCode = org1.OH_Code;
			rec10.ServerCode = "E00";
			rec10.Product = ProductTypes.Codes.BorderWise;
			rec10.TenantID = "BOR_REF#003";
			rec10.AutoGenerateEntCode = true;
			rec10.RegistrationStatus = "NON";
			rec10.PreRegistrationExpiryDateUTC = ZDateTime.UtcToday.AddDays(10);
			var rec11 = flattenedCollection.AddNew();
			rec11.EnterpriseCode = "";
			rec11.EnterpriseID = "";
			rec11.OrgCode = org1.OH_Code;
			rec11.ServerCode = "E01";
			rec11.Product = ProductTypes.Codes.CargoWiseOne;
			rec11.AutoGenerateEntCode = true;
			rec11.SystemType = "@11";
			var rec12 = flattenedCollection.AddNew();
			rec12.EnterpriseCode = "";
			rec12.EnterpriseID = "";
			rec12.OrgCode = org1.OH_Code;
			rec12.ServerCode = "E02";
			rec12.Product = ProductTypes.Codes.BorderWise;
			rec12.AutoGenerateEntCode = true;
			rec12.SystemID = "SYS#002";
			var rec13 = flattenedCollection.AddNew();
			rec13.EnterpriseCode = "";
			rec13.EnterpriseID = "";
			rec13.OrgCode = org1.OH_Code;
			rec13.ServerCode = "E02";
			rec13.Product = ProductTypes.Codes.BorderWise;
			rec13.AutoGenerateEntCode = true;
			rec13.SystemID = "SYS#003";
			rec13.HostedLocation = "";
			var rec14 = flattenedCollection.AddNew();
			rec14.EnterpriseCode = "";
			rec14.EnterpriseID = "";
			rec14.OrgCode = org1.OH_Code;
			rec14.ServerCode = "E02";
			rec14.Product = ProductTypes.Codes.BorderWise;
			rec14.AutoGenerateEntCode = true;
			rec14.SystemID = "SYS#003";
			rec14.HostedLocation = "#$%";
			var dbCollection = new LicenceDatabaseNonDependentCollection(Factory);
			var processor = new LicenceDatabaseFlattenedDataTransferProcessor(dbCollection, collectionInfo);
			processor.Import();
			Factory.Save();
			var licDatabaseList = dbCollection.Cast<LicenceDatabase>();
			var db1 = licDatabaseList.Single(x => x.LD_ServerCode == "D11");
			AssertEquals(ProductTypes.Codes.BorderWise, db1.LD_Product);
			AssertEquals("BOR_REF#001", db1.LD_TenantID);
			AssertEquals("SYS#002", db1.TrustedSystem.ETS_SystemID);
			AssertEquals(false, org1.LicenceEnterpriseCode.IsEmpty);
			AssertEquals(false, org1.LicenceEnterpriseID.IsEmpty);
			AssertEquals(org1.LicEnterprise.LE_EnterpriseCode, db1.LicEnterprise.LE_EnterpriseCode);
			AssertEquals(org1.LicEnterprise.LE_EnterpriseID, db1.LicEnterprise.LE_EnterpriseID);
			AssertEquals("PRE", db1.LD_Status);
			AssertEquals(new ZDateTime(2050, 1, 1), db1.LD_PreRegistrationExpiryDateUTC);
			AssertEquals("TST", db1.LD_LicenceType);
			AssertEquals(true, db1.LD_AllowAutoLogin);
			AssertEquals("SYD", db1.LD_HostedLocation);
			AssertEquals(@"Record [Org. Code: !!!@@@@, Server Code: D22, Enterprise Code: , Enterprise ID: ] excluded: Org. Code not found
Record [Org. Code: TEST_ORG_001, Server Code: D33, Enterprise Code: , Enterprise ID: ] excluded: Please enter a Product
Record [Org. Code: TEST_ORG_001, Server Code: D44, Enterprise Code: , Enterprise ID: ] excluded: Please enter a valid Product
Record [Org. Code: TEST_ORG_001, Server Code: D55, Enterprise Code: , Enterprise ID: ] excluded: Tenant ID is not applicable for Product CW1
Record [Org. Code: TEST_ORG_001, Server Code: D66, Enterprise Code: , Enterprise ID: ] excluded: Invalid Release Ring: @@@
Record [Org. Code: TEST_ORG_001, Server Code: D77, Enterprise Code: , Enterprise ID: ] excluded: Invalid Edition: ###
Record [Org. Code: TEST_ORG_001, Server Code: D88, Enterprise Code: , Enterprise ID: ] excluded: A database with this Tenant ID already exists for this product.
Record [Org. Code: TEST_ORG_001, Server Code: D99, Enterprise Code: , Enterprise ID: ] excluded: Invalid Registration Status: 'REG', only 'PRE' (Pre-Registered) / 'NON' (Not Registered) allowed.
Record [Org. Code: TEST_ORG_001, Server Code: E00, Enterprise Code: , Enterprise ID: ] excluded: Pre-Registration Expiry Date is not applicable when Registration Status is not 'PRE' (Pre-Registered).
Record [Org. Code: TEST_ORG_001, Server Code: E01, Enterprise Code: , Enterprise ID: ] excluded: Please enter a valid System Type.
Record [Org. Code: TEST_ORG_001, Server Code: E02, Enterprise Code: , Enterprise ID: ] excluded: A database with this System ID (Trusted Messaging) already exists for this product.
Record [Org. Code: TEST_ORG_001, Server Code: E02, Enterprise Code: , Enterprise ID: ] excluded: Hosted Location is mandatory.
Record [Org. Code: TEST_ORG_001, Server Code: E02, Enterprise Code: , Enterprise ID: ] excluded: Invalid Hosted Location: #$%
", processor.Log);
		}

		public void TestConcurrencyException()
		{
			var flattenedCollection = new LicenceDatabaseFlattenedCollection(Factory);
			var collectionInfo = new LicenceDatabaseImportInfo(flattenedCollection);
			var rec1 = flattenedCollection.AddNew();
			rec1.EnterpriseCode = "EN1";
			rec1.OrgCode = "Code";
			rec1.ServerCode = "DB1";
			var dbCollection = new LicenceDatabaseNonDependentCollection(Factory);
			var processor = new ProcessorWithConcurrencyException(dbCollection, collectionInfo);
			processor.Import();
			AssertEquals("While you were working, another user has modified these records. Please try again.", processor.Log);
		}

		class ProcessorWithConcurrencyException : LicenceDatabaseFlattenedDataTransferProcessor
		{
			public ProcessorWithConcurrencyException(LicenceDatabaseNonDependentCollection licDatabaseCollection, IImportCollectionInfo collectionInfo) : base(licDatabaseCollection, collectionInfo)
			{
			}

			protected override LicenceDatabase CreateHeader(IBusinessObjectCollection headerCollection, LicenceDatabaseFlattened flat)
			{
				var org = Factory.New<OrgHeader>();
				throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new InvalidOperationException("Simulated Concurrency exception"), ((IBusinessObjectInternals)org).Row, ((CargoWise.Data.IDbConnected)Factory).Connection), Factory);
			}
		}
	}
}
