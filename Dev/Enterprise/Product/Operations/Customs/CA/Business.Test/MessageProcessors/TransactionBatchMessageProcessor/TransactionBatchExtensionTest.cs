using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class TransactionBatchExtensionTest : TestCaseWithFactory
	{
		public void TestGetOrgsFromBN()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.FillWithValidTestData();
			orgHeader1.OH_Code = "IMPORTER1";
			orgHeader1.OH_FullName = "IMPORTER1 NAME";
			orgHeader1.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123546789RM0001", "CA");

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.FillWithValidTestData();
			orgHeader2.OH_Code = "IMPORTER2";
			orgHeader2.OH_FullName = "IMPORTER2 NAME";
			orgHeader2.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123546789RM0002", "CA");

			var orgHeader3 = Factory.New<OrgHeader>();
			orgHeader3.FillWithValidTestData();
			orgHeader3.OH_Code = "IMPORTER3";
			orgHeader3.OH_FullName = "IMPORTER3 NAME";
			orgHeader3.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123546789RM0001", "CA");

			var orgHeader4 = Factory.New<OrgHeader>();
			orgHeader4.FillWithValidTestData();
			orgHeader4.OH_Code = "IMPORTER4";
			orgHeader4.OH_FullName = "IMPORTER4 NAME";
			orgHeader4.OH_IsActive = false;
			orgHeader4.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "987654321RM0003", "CA");

			Factory.Save();
			AssertArrayEqualsByElements("first look for an organization with a BN made up of the 9 digit BN9 in the header + RM + RM number", new[] { orgHeader1, orgHeader3 }, TransactionBatchExtension.GetOrgsFromBN("123546789", "0001", Factory, false));
			AssertArrayEqualsByElements("first look for an organization with a BN made up of the 9 digit BN9 in the header + RM + RM number", new[] { orgHeader2 }, TransactionBatchExtension.GetOrgsFromBN("123546789", "0002", Factory, false));
			AssertArrayEqualsByElements("If the BN is 15 characters look for an organization with that number", new[] { orgHeader2 }, TransactionBatchExtension.GetOrgsFromBN("123546789RM0002", "", Factory, false));
			AssertArrayEqualsByElements("If not found then do a “stars with” search for just the 9 characters in ascending order", new[] { orgHeader1, orgHeader3, orgHeader2 }, TransactionBatchExtension.GetOrgsFromBN("123546789RM0003", "", Factory, false));
			AssertArrayEqualsByElements("If not found then do a “stars with” search for just the 9 characters in ascending order", new[] { orgHeader1, orgHeader3, orgHeader2 }, TransactionBatchExtension.GetOrgsFromBN("123546789", "", Factory, false));
			AssertEquals("If org is not active, it could not be found", 0, TransactionBatchExtension.GetOrgsFromBN("987654321RM0003", "", Factory, false).Length);

			var loadOrgs = TransactionBatchExtension.GetOrgsFromBN("123546789", "0001", new BusinessObjectFactory(), false);
			AssertEquals(2, loadOrgs.Length);
			AssertEquals("Load from Cache", orgHeader1.PK, loadOrgs[0].PK);
			AssertEquals("Load from Cache", orgHeader3.PK, loadOrgs[1].PK);

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "DCA";
			company.GC_Name = "DCA TEST";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			company.GC_OH_OrgProxy = orgHeader1.PK;
			GlbCompany.CurrentCompany.GC_Code = "DCA";

			orgHeader1.OH_IsBroker = true;

			Factory.Save();

			AssertArrayEqualsByElements("first look for an organization with a BN made up of the 9 digit BN9 in the header + RM + RM number + orgProxy", new[] { orgHeader1 }, TransactionBatchExtension.GetOrgsFromBN("123546789", "0001", Factory, true));
			AssertArrayEqualsByElements("first look for an organization with a BN made up of the 9 digit BN9 in the header + RM + RM number + orgProxy", new[] { orgHeader1 }, TransactionBatchExtension.GetOrgsFromBN("123546789", "0002", Factory, true));
			AssertArrayEqualsByElements("If the BN is 15 characters look for an organization with that number  + orgProxy", new[] { orgHeader1 }, TransactionBatchExtension.GetOrgsFromBN("123546789RM0001", "", Factory, true));
			AssertArrayEqualsByElements("If the BN is 15 characters look for an organization with that number  + orgProxy", new[] { orgHeader1 }, TransactionBatchExtension.GetOrgsFromBN("123546789RM0002", "", Factory, true));
			AssertArrayEqualsByElements("If not found then do a “stars with” search for just the 9 characters in ascending order  + orgProxy", new[] { orgHeader1 }, TransactionBatchExtension.GetOrgsFromBN("123546789RM0003", "", Factory, true));
			AssertArrayEqualsByElements("If not found then do a “stars with” search for just the 9 characters in ascending order  + orgProxy", new[] { orgHeader1 }, TransactionBatchExtension.GetOrgsFromBN("123546789", "", Factory, true));

			var loadOrgs2 = TransactionBatchExtension.GetOrgsFromBN("123546789", "0001", new BusinessObjectFactory(), true);
			AssertEquals(1, loadOrgs2.Length);
			AssertEquals("Load from Cache", orgHeader1.PK, loadOrgs2[0].PK);
		}

		public void TestGetAccountSecurityCode()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.FillWithValidTestData();
			orgHeader.OH_Code = "IMPORTER1";
			orgHeader.OH_FullName = "IMPORTER1 NAME";

			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			AssertEquals("", TransactionBatchExtension.GetAccountSecurityCode(orgHeader, logger));
			AssertEquals("Warning - Cannot find the Account Security Code for importer or in Registry", logger.ToString());

			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "11111");
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			AssertEquals("11111", TransactionBatchExtension.GetAccountSecurityCode(orgHeader, logger));
			AssertEquals("Using the Account Security Code '11111' in Registry", logger.ToString());

			orgHeader.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.AccountSecurityCode, "22222", "CA");
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			AssertEquals("22222", TransactionBatchExtension.GetAccountSecurityCode(orgHeader, logger));
			AssertEquals("Using the Account Security Code '22222' for Organization (IMPORTER1)", logger.ToString());

			var importerAddInfo = OrgImpAddInfo.Get(orgHeader);
			importerAddInfo.ZO_IsImporterDirectPayment = true;
			importerAddInfo.ZO_AccountSecurityNumber = "33333";
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			AssertEquals("33333", TransactionBatchExtension.GetAccountSecurityCode(orgHeader, logger));
			AssertEquals("Using the Account Security Code '33333' for Organization (IMPORTER1)", logger.ToString());
		}
	}
}
