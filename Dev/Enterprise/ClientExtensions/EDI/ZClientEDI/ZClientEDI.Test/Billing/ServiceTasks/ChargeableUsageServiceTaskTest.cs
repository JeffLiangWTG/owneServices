using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Billing.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.Billing.ServiceTasks.Testing
{
	[TestedType(typeof(ChargeableUsageServiceTask))]
	class ChargeableUsageServiceTaskTest : ServiceTaskTestCase<ChargeableUsageServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
		}

		public void TestUpdateOdpl()
		{
			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			OrgContact contact = client.Contacts.AddNew();
			contact.OC_ContactName = "Test Contact";
			contact.OC_Email = "test.contact@abc.org";
			var db1 = client.LicCompany.LicDatabases.AddNew();
			db1.LD_Product = ProductTypes.Codes.Enterprise;
			var licHeader1 = client.LicCompany.GetHeader(db1);
			var databaseStaff = Factory.New<ClientStaff>();
			databaseStaff.LS_LD = db1.PK;
			databaseStaff.LS_FullName = contact.OC_ContactName;
			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = client.LicCompany.LC_CompanyCode;
			clientCompany.LCC_LD = db1.PK;
			clientCompany.LCC_OH = client.PK;
			var usage = BillingTestHelper.CreateEdiLicenceUsage(clientCompany, databaseStaff, LicenceTypes.Codes.ODM, BillingConstants.CoreModuleCode, new ZDateTime(2010, 7, 13));
			var billingPeriod = new BillingPeriod(new ZDateTime(2010, 7, 1));
			Factory.Save();
			process.UpdateOdpl(billingPeriod);
			var factory2 = new BusinessObjectFactory();
			var results = factory2.Load<ClientChargeableUsage>(new ZQuery());
			AssertEquals("TST site live null included", 1, results.Length);
			db1.LD_LicenceType = DatabaseTypes.Codes.Production;
			Factory.Save();
			process.UpdateOdpl(billingPeriod);
			factory2 = new BusinessObjectFactory();
			results = factory2.Load<ClientChargeableUsage>(new ZQuery());
			AssertEquals("PRD site live null included", 1, results.Length);
			licHeader1.LA_AgreedLiveDate = new ZDateTime(2010, 7, 16);
			Factory.Save();
			process.UpdateOdpl(billingPeriod);
			factory2 = new BusinessObjectFactory();
			results = factory2.Load<ClientChargeableUsage>(new ZQuery());
			AssertEquals("site not live early in month", 0, results.Length);
			licHeader1.LA_AgreedLiveDate = new ZDateTime(2010, 7, 15);
			Factory.Save();
			process.UpdateOdpl(billingPeriod);
			factory2 = new BusinessObjectFactory();
			results = factory2.Load<ClientChargeableUsage>(new ZQuery());
			AssertEquals("site live, but usage was before", 0, results.Length);
			usage.LX2_LastUsageUtc = licHeader1.LA_AgreedLiveDate;
			usage.LX2_UsageCount = 2;
			Factory.Save();
			factory2 = new BusinessObjectFactory();
			process.UpdateOdpl(billingPeriod);
			results = factory2.Load<ClientChargeableUsage>(new ZQuery());
			AssertEquals("live usage", 1, results.Length);
		}

		[TestDate(2010, 1, 1)]
		public void TestHistoricalOdpl()
		{
			var task = new ChargeableUsageServiceTaskForTest();
			TestDateAttribute.Date = new DateTime(2010, 11, 2);
			task.RunTask();
			AssertEquals(1, task.CallsToUpdateOdpl);
			AssertEquals(1, task.CallsToUpdateCPT);
			AssertEquals(new DateTime(2010, 10, 1), task.odplFirst);
			AssertEquals(new DateTime(2010, 10, 1), task.odplLast);
			TestDateAttribute.Date = new DateTime(2010, 12, 26);
			task = new ChargeableUsageServiceTaskForTest();
			task.RunTask();
			AssertEquals(3, task.CallsToUpdateOdpl);
			AssertEquals(3, task.CallsToUpdateCPT);
			AssertEquals(new DateTime(2010, 10, 1), task.odplFirst);
			AssertEquals(new DateTime(2010, 12, 1), task.odplLast);
			TestDateAttribute.Date = new DateTime(2010, 12, 2);
			task = new ChargeableUsageServiceTaskForTest();
			task.RunTask();
			AssertEquals(2, task.CallsToUpdateOdpl);
			AssertEquals(2, task.CallsToUpdateCPT);
			AssertEquals(new DateTime(2010, 10, 1), task.odplFirst);
			AssertEquals(new DateTime(2010, 11, 1), task.odplLast);
			TestDateAttribute.Date = new DateTime(2011, 12, 30);
			task = new ChargeableUsageServiceTaskForTest();
			task.RunTask();
			AssertEquals(3, task.CallsToUpdateOdpl);
			AssertEquals(3, task.CallsToUpdateCPT);
			AssertEquals(new DateTime(2011, 10, 1), task.odplFirst);
			AssertEquals(new DateTime(2011, 12, 1), task.odplLast);
		}

		public void TestUpdateCPT()
		{
			var licHeader1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var contact = licHeader1.Company.Header.Contacts[0];
			var usage1 = CreateUsage(licHeader1, LicenceTypes.Codes.CPT, Env.Licence.ACIReportingPerTransaction.Name, contact, new ZDateTime(2010, 7, 13));
			var usage2 = CreateUsage(licHeader1, LicenceTypes.Codes.CPT, Env.Licence.AMSReporting.Name, contact, new ZDateTime(2010, 7, 13));
			usage2.LX2_UsageCount = 2;
			var usageBefore = CreateUsage(licHeader1, LicenceTypes.Codes.CPT, Env.Licence.AMSReporting.Name, contact, new ZDateTime(2010, 6, 13));
			var usageAfter = CreateUsage(licHeader1, LicenceTypes.Codes.CPT, Env.Licence.AMSReporting.Name, contact, new ZDateTime(2010, 8, 13));
			Factory.Save();
			var usages = Factory.Load<EdiLicenceUsage>(new ZQuery()
			{ OrderBy = EdiLicenceUsageSchema.Constants.LX2_FirstUsageUtc });
			AssertEquals(4, usages.Length);
			var billingPeriod = new BillingPeriod(new ZDateTime(2010, 7, 1));
			process.UpdateCPT(billingPeriod);
			var factory2 = new BusinessObjectFactory();
			var results = factory2.Load<ClientChargeableUsage>(new ZQuery());
			AssertEquals("module count", 2, results.Length);
			var aci = results.First(s => s.U1_SubCode == Env.Licence.ACIReportingPerTransaction.Name);
			var ams = results.First(s => s.U1_SubCode == Env.Licence.AMSReporting.Name);
			AssertEquals("aci units", 1m, aci.U1_UnitCount);
			AssertEquals("ams units", 2m, ams.U1_UnitCount);
		}

		EdiLicenceUsage CreateUsage(LicenceHeader licHeader, string mode, string moduleCode, OrgContact user, ZDateTime usageTime)
		{
			var usage = BillingTestHelper.CreateEdiLicenceUsage(licHeader.Modules.FindByCode(moduleCode), user, usageTime);
			usage.LX2_LicenceMode = mode;
			return usage;
		}

		[TestDate(2010, 10, 2)]
		public void TestRunTask()
		{
			var systemList = new BillingSystemList();
			HashSet<string> systemCodesNoNeedToCheck = new HashSet<string>();
			systemCodesNoNeedToCheck.Add(BillingConstants.BillingSystem.Fee);
			systemCodesNoNeedToCheck.Add(BillingConstants.BillingSystem.ODM);
			systemCodesNoNeedToCheck.Add(BillingConstants.BillingSystem.ABMCustoms);
			systemCodesNoNeedToCheck.Add(BillingConstants.BillingSystem.Service);
			systemCodesNoNeedToCheck.Add(BillingConstants.BillingSystem.OceanTracingLegacy);
			systemCodesNoNeedToCheck.Add(BillingConstants.BillingSystem.BorderWise);
			systemCodesNoNeedToCheck.Add(BillingConstants.BillingSystem.WiseCloudUser);
			systemCodesNoNeedToCheck.Add(BillingConstants.BillingSystem.OceanCarrierMessaging);
			systemCodesNoNeedToCheck.Add(BillingConstants.BillingSystem.PortMessaging);
			systemCodesNoNeedToCheck.Add(BillingConstants.BillingSystem.S8Cargo);
			systemCodesNoNeedToCheck.Add(BillingConstants.BillingSystem.GlobalContainerTracking);
			systemCodesNoNeedToCheck.Add(BillingConstants.BillingSystem.HostingRemoteDevices);
			systemCodesNoNeedToCheck.Add(BillingConstants.BillingSystem.eBACCA);
			systemCodesNoNeedToCheck.Add(BillingConstants.BillingSystem.ExDocs);
			foreach (CodeDescriptionPair pair in EDIDataRegistry.Instance.LicenceUsageBilledPerTransaction.Value)
			{
				systemCodesNoNeedToCheck.Add(pair.Code);
			}

			var processForTest = new ChargeableUsageServiceTaskForTest();
			processForTest.ServiceLogger = new TestServiceLogger();
			Mock.Get(processForTest.BillingDb).Setup(m => m.GetFirstStlPeriod(new DateTime(2010, 9, 1))).Returns(new DateTime(2010, 7, 1));
			processForTest.RunTask();
			CombineAssertions(() =>
			{
				AssertEquals("Should call to update ODPL once", 1, processForTest.CallsToUpdateOdpl);
				//AssertEquals("Should call to update eRouter once", 1, processForTest.CallsToUpdateErouter);
				AssertEquals("Should call to update CPT once", 1, processForTest.CallsToUpdateCPT);
				foreach (var billingSystem in systemList)
				{
					if (!systemCodesNoNeedToCheck.Contains(billingSystem.SystemCode))
					{
						var usageCount = 0;
						processForTest.CallsToUpdateSystemMap.TryGetValue(billingSystem.SystemCode, out usageCount);
						AssertEquals("Should call to update billing system " + billingSystem.SystemCode + " (" + billingSystem.SystemDescription + ") once", 1, usageCount);
					}
				}

				AssertEquals("UpdateStlPeriods.Count", 1, processForTest.UpdateStlPeriods.Count);
				AssertEquals("UpdateStlPeriods[0]", new DateTime(2010, 9, 1), processForTest.UpdateStlPeriods[0]);
				var stlUpdateSystemCalls = processForTest.updateSystemCalls.Where(x => x.SystemCode == "STL").ToArray();
				AssertEquals("stlUpdateSystemCalls.Length", 3, stlUpdateSystemCalls.Length);
				AssertEquals("stlUpdateSystemCalls[0].Period.PeriodStartDate", new DateTime(2010, 7, 1), stlUpdateSystemCalls[0].Period.PeriodStartDate);
				AssertEquals("stlUpdateSystemCalls[1].Period.PeriodStartDate", new DateTime(2010, 8, 1), stlUpdateSystemCalls[1].Period.PeriodStartDate);
				AssertEquals("stlUpdateSystemCalls[2].Period.PeriodStartDate", new DateTime(2010, 9, 1), stlUpdateSystemCalls[2].Period.PeriodStartDate);
				AssertEquals("CallsToUpdateContactCountry", 1, processForTest.CallsToUpdateContactCountry);
				AssertEquals("UpdateContactCountryPeriodDate", new DateTime(2010, 9, 1), processForTest.UpdateContactCountryPeriodDate);
			});
		}

		[TestDate(2018, 7, 28)]
		public void TestRunTask_BorderWiseReady()
		{
			var processForTest = new ChargeableUsageServiceTaskForTest();
			var periodToUpdate = new DateTime(TestDateAttribute.Date.Year, TestDateAttribute.Date.Month, 1);
			Mock.Get(processForTest.BillingDb).Setup(m => m.GetFirstStlPeriod(periodToUpdate)).Returns(periodToUpdate);
			processForTest.RunTask();
			var usageCount = 0;
			processForTest.CallsToUpdateSystemMap.TryGetValue(BillingConstants.BillingSystem.BorderWise, out usageCount);
			AssertEquals("Should call to update billing system BorderWise", 1, usageCount);
		}

		[TestDate(2018, 6, 1)]
		public void TestRunTask_BorderWiseNotReady()
		{
			var processForTest = new ChargeableUsageServiceTaskForTest();
			var periodToUpdate = new DateTime(TestDateAttribute.Date.Year, TestDateAttribute.Date.Month, 1);
			Mock.Get(processForTest.BillingDb).Setup(m => m.GetFirstStlPeriod(periodToUpdate)).Returns(periodToUpdate);
			processForTest.RunTask();
			var usageCount = 0;
			processForTest.CallsToUpdateSystemMap.TryGetValue(BillingConstants.BillingSystem.BorderWise, out usageCount);
			AssertEquals("Should call to update billing system BorderWise", 0, usageCount);
		}

		[TestDate(2021, 6, 2)]
		public void TestRunTask_OnlyOnEdiProd()
		{
			var ediProdLicenceCode = "EDIEDISYD";
			SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ediProdLicenceCode);
			var systemLicenceCode = EnvProxy.Instance.CurrentCompany.GetLicenceCode();
			AssertNotEquals(ediProdLicenceCode, systemLicenceCode);
			var processForTest = new ChargeableUsageServiceTaskForTest();
			Mock.Get(processForTest.BillingDb).Setup(m => m.GetFirstStlPeriod(new DateTime(2021, 6, 1))).Returns(new DateTime(2021, 6, 1));
			processForTest.RunTask();
			AssertEquals(0, processForTest.CallsToUpdateOdpl);
			var systemLicenceCodeWithDummyCompany = systemLicenceCode.Substring(0, 3) + "ZZZ" + systemLicenceCode.Substring(6, 3);
			AssertNotEquals(systemLicenceCode, systemLicenceCodeWithDummyCompany);
			SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemLicenceCodeWithDummyCompany);
			processForTest = new ChargeableUsageServiceTaskForTest();
			processForTest.RunTask();
			AssertEquals(3, processForTest.CallsToUpdateOdpl);
		}

		public void TestDeleteInternalUsage()
		{
			EDIOrgHeader internalOrg = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			EDIOrgHeader clientOrg = BillingTestHelper.CreateOrganisation(Factory, "OUR");
			Factory.Save();
			EDIDataRegistry.Instance.NonBilledEnterpriseCodesAsStringArray = new string[] { "AAA" };
			var usage1 = Factory.New<ClientChargeableUsage>();
			usage1.U1_Code = "FAX";
			usage1.U1_LC = internalOrg.LicCompany.PK;
			usage1.U1_UnitCount = 55;
			usage1.U1_PeriodStart = new ZDateTime(2010, 10, 1);
			var clientUsage1 = Factory.New<ClientChargeableUsage>();
			clientUsage1.U1_Code = "FAX";
			clientUsage1.U1_LC = clientOrg.LicCompany.PK;
			clientUsage1.U1_UnitCount = 22;
			clientUsage1.U1_PeriodStart = new ZDateTime(2010, 10, 1);
			Factory.Save();
			process.DeleteInternalUsage(new ZDateTime(2010, 10, 1));
			var factory2 = new BusinessObjectFactory();
			var usage = factory2.Load<ClientChargeableUsage>(new ZQuery());
			AssertEquals("usage deleted", 1, usage.Length);
			AssertEquals("client usage found", 22m, usage.First(s => s.U1_LC == clientOrg.LicCompany.PK).U1_UnitCount);
		}

		public void TestErrorHandling()
		{
			GlbGroup mailGroup = Factory.New<GlbGroup>();
			GlbStaff currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "teststaff@edi.com.au";
			mailGroup.GG_Code = "TXB";
			mailGroup.Staff.Add(currentStaff);
			Factory.Save();
			EDIDataRegistry.Instance.InternalNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mailGroup.PK.ToGuid());
			var billingPeriod = new BillingPeriod(new ZDateTime(2010, 10, 1));
			process.ChargeableUsageUpdate(billingPeriod, "bad", null, null, 0, "doesnotexist");
			AssertEquals("email count", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("ChargeableUsageServiceTaskForExecuteNonQueryTest Could not find stored procedure 'doesnotexist'.", email.Subject);
			AssertEquals("recipients", 1, email.Recipients.Count);
			AssertEquals("to", "teststaff@edi.com.au", email.Recipients[0].Email);
			AssertEquals("log count", 3, logger.Count);
			AssertEquals("log", "Information|Begin processing bad", logger[0]);
			Assert(logger[1], logger[1].StartsWith("Error|Error doesnotexist()"));
			Assert(logger[1], logger[1].Contains("Could not find stored procedure"));
			AssertEquals("log", "Information|End processing bad", logger[2]);
		}

		public void TestFreeModulesForHostedClients()
		{
			var licHosted = BillingTestHelper.CreateLicence(Factory, "AAA");
			var licNotHosted = BillingTestHelper.CreateLicence(Factory, "CCC");
			licHosted.Database.LD_HostedLocation = "SYD";
			licNotHosted.Database.LD_HostedLocation = Enterprise.Core.Constants.LicenceConstants.NotHostedWithCargoWise;
			OrgContact contact1 = licHosted.Company.Header.Contacts.AddNew();
			contact1.OC_ContactName = "Hosted Contact";
			contact1.OC_Email = "h@test.com";
			OrgContact contact2 = licNotHosted.Company.Header.Contacts.AddNew();
			contact2.OC_ContactName = "Not Hosted Contact";
			contact2.OC_Email = "nh@test.com";
			var usage1 = CreateUsage(licHosted, LicenceTypes.Codes.ODM, LegacyLicence.Codes.Core, contact1, new ZDateTime(2010, 7, 13));
			var usage3 = CreateUsage(licNotHosted, LicenceTypes.Codes.ODM, LegacyLicence.Codes.Core, contact2, new ZDateTime(2010, 7, 13));
			var usage4 = CreateUsage(licNotHosted, LicenceTypes.Codes.ODM, Env.Licence.RemoteDesktopServices.Name, contact2, new ZDateTime(2010, 7, 13));
			Factory.Save();
			var usages = Factory.Load<EdiLicenceUsage>(new ZQuery());
			AssertEquals(3, usages.Length);
			var billingPeriod = new BillingPeriod(new ZDateTime(2010, 7, 1));
			process.UpdateOdpl(billingPeriod);
			var factory2 = new BusinessObjectFactory();
			var results = factory2.Load<ClientChargeableUsage>(new ZQuery());
			AssertEquals("module count does not include free modules", 3, results.Length);
			AssertNotNull("expected module", results.First(s => s.U1_SubCode == LegacyLicence.Codes.Core && s.U1_LCC == licHosted.ClientCompany.PK));
			AssertNotNull("expected module", results.First(s => s.U1_SubCode == LegacyLicence.Codes.Core && s.U1_LCC == licNotHosted.ClientCompany.PK));
			AssertNotNull("expected module", results.First(s => s.U1_SubCode == Env.Licence.RemoteDesktopServices.Name && s.U1_LCC == licNotHosted.ClientCompany.PK));
		}

		public void TestIncludedModuleChargedToParent()
		{
			string parentCode = Env.Licence.RelationshipManager.Name;
			string childCode1 = Env.Licence.RelationshipOpportunityManager.Name;
			string childCode2 = Env.Licence.RelationshipCampaignManager.Name;
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			LicenceCompany company1 = lic1.Company;
			BillingTestHelper.CreateEdiLicenceUsage(lic1, childCode1, new ZDateTime(2010, 7, 13), "Albert");
			BillingTestHelper.CreateEdiLicenceUsage(lic1, childCode1, new ZDateTime(2010, 7, 13), "Bobby");
			BillingTestHelper.CreateEdiLicenceUsage(lic1, childCode2, new ZDateTime(2010, 7, 13), "Charlie");
			BillingTestHelper.CreateEdiLicenceUsage(lic1, parentCode, new ZDateTime(2010, 7, 13), "Charlie");
			BillingTestHelper.CreateEdiLicenceUsage(lic1, parentCode, new ZDateTime(2010, 7, 13), "Dave");
			BillingTestHelper.CreateEdiLicenceUsage(lic1, parentCode, new ZDateTime(2010, 7, 13), "Eddie");
			BillingTestHelper.CreateEdiLicenceUsage(lic1, childCode1, new ZDateTime(2010, 7, 13), "Eddie");
			var prices1 = BillingTestHelper.CreatePriceList(company1);
			var itemParent = prices1.Items.AddNew();
			itemParent.L7_Category = BillingConstants.BillingSystem.ODM;
			itemParent.L7_Code = parentCode;
			var item1 = prices1.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.ODM;
			item1.L7_Code = childCode1;
			item1.L7_ParentCategory = BillingConstants.BillingSystem.ODM;
			item1.L7_ParentCode = parentCode;
			var item2 = prices1.Items.AddNew();
			item2.L7_Category = BillingConstants.BillingSystem.ODM;
			item2.L7_Code = childCode2;
			item2.L7_ParentCategory = BillingConstants.BillingSystem.ODM;
			item2.L7_ParentCode = parentCode;
			Factory.Save();
			var billingPeriod = new BillingPeriod(new ZDateTime(2010, 7, 1));
			process.UpdateOdpl(billingPeriod);
			var factory2 = new BusinessObjectFactory();
			var results = factory2.Load<ClientChargeableUsage>(new ZQuery());
			AssertEquals("module count has parent and children", 3, results.Length);
			var parentUsage = results.First(x => x.U1_SubCode == parentCode);
			var child1Usage = results.First(x => x.U1_SubCode == childCode1);
			var child2Usage = results.First(x => x.U1_SubCode == childCode2);
			AssertEquals("parent UnitCount", 5m, parentUsage.U1_UnitCount);
			AssertEquals("child1 UnitCount", 3m, child1Usage.U1_UnitCount);
			AssertEquals("child2 UnitCount", 1m, child2Usage.U1_UnitCount);
		}

		[TestDate(2018, 2, 1)]
		public void TestUpdateBillingDbUsageFromConfiguredCodes()
		{
			var billingCodes = new BillingDbUsageCodesCollection();
			var usageCodes1 = billingCodes.AddNew();
			usageCodes1.Category = "ACC";
			usageCodes1.PriceItemCode = "IT1";
			usageCodes1.PriceHeaderCode = BillingConstants.PriceHeaderType.STL;
			var usageCodes2 = billingCodes.AddNew();
			usageCodes2.Category = "ACC";
			usageCodes2.PriceItemCode = "GTS";
			usageCodes2.PriceHeaderCode = BillingConstants.PriceHeaderType.STL;
			usageCodes2.KeyRefIndex1 = 3;
			EDIDataRegistry.Instance.BillingDbUsageCodesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, billingCodes);
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			Factory.Save();
			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ACC", "GTS", new ZDateTime(2019, 1, 1, 0, 0, 0), lic1, "", "", "Machine1", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ACC", "GTS", new ZDateTime(2019, 1, 15, 0, 0, 0), lic1, "", "", "Machine1", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ACC", "GTS", new ZDateTime(2019, 1, 20, 0, 0, 0), lic1, "", "", "Machine2", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ACC", "IT1", new ZDateTime(2019, 1, 1, 0, 0, 0), lic1, "", "", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ACC", "IT1", new ZDateTime(2019, 1, 15, 0, 0, 0), lic1, "", "", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ACC", "IT1", new ZDateTime(2019, 1, 20, 0, 0, 0), lic1, "", "", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "IT1", new ZDateTime(2019, 1, 20, 0, 0, 0), lic1, "", "", "", ""));
			// Other data
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ACC", "ZZZ", new ZDateTime(2019, 1, 1, 0, 0, 0), lic1, "", "", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ACC", "IT1", new ZDateTime(2018, 12, 30, 0, 0, 0), lic1, "", "", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ACC", "IT1", new ZDateTime(2019, 2, 1, 0, 0, 0), lic1, "", "", "", ""));
			EServicesBillingTestHelper.AddTransactions(infoList);
			var process = new ChargeableUsageServiceTaskForOtherSystemsTest(Array.Empty<string>());
			process.ServiceLogger = new TestServiceLogger();
			var billingPeriod = new BillingPeriod(new ZDateTime(2019, 1, 1));
			process.UpdateOtherBillingSystemsForTest(billingPeriod);
			var factory2 = new BusinessObjectFactory();
			var results = factory2.Load<ClientChargeableUsage>(new ZQuery());
			var actual = string.Join("\r\n", results.Select(x => x.U1_Code + "-" + x.U1_SubCode + "-" + x.U1_Reference1 + " " + x.U1_UnitCount).OrderBy(x => x));
			CombineAssertions(() =>
			{
				AssertEquals(@"ACC-GTS-Machine1 2.0000
ACC-GTS-Machine2 1.0000
ACC-IT1- 3.0000", actual);
				AssertEquals(@"Information|Begin processing Billing DB Usage Category=ACC PriceCode=IT1 KeyRefIndex=0
Information|End processing Billing DB Usage Category=ACC PriceCode=IT1 KeyRefIndex=0
Information|Begin processing Billing DB Usage Category=ACC PriceCode=GTS KeyRefIndex=3
Information|End processing Billing DB Usage Category=ACC PriceCode=GTS KeyRefIndex=3
", process.ServiceLogger.ToString());
			});
		}

		[TestDate(2022, 11, 1)]
		public void TestUsageBillingSettings()
		{
			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var settings = new UsageBillingSettings();
			var priceLists = settings.PriceLists;
			var priceList1 = priceLists.AddNew();
			priceList1.ProductCode = "ABC";
			priceList1.RawUsageCategory = "SAT";
			priceList1.PriceListCode = "DEF";
			priceList1.Description = "ABC - Price List #0";
			EDIDataRegistry.Instance.UsageBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			lic1.LA_LicenceAdvStdOth = "STL";
			lic1.Database.LD_Product = "ABC";
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			Factory.Save();

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SAT", "GTS", new ZDateTime(2022, 10, 1, 0, 0, 0), lic1, "", "", "Machine1", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SAT", "GTS", new ZDateTime(2022, 10, 15, 0, 0, 0), lic1, "", "", "Machine1", ""));

			EServicesBillingTestHelper.AddTransactions(infoList);
			var process = new ChargeableUsageServiceTaskForOtherSystemsTest(Array.Empty<string>());
			process.ServiceLogger = new TestServiceLogger();
			var billingPeriod = new BillingPeriod(new ZDateTime(2022, 10, 1));
			process.UpdateOtherBillingSystemsForTest(billingPeriod);
			var factory2 = new BusinessObjectFactory();
			var results = factory2.Load<ClientChargeableUsage>(new ZQuery());
			var actual = string.Join("\r\n", results.Select(x => x.U1_Code + "-" + x.U1_SubCode + "-" + x.U1_Reference1 + " " + x.U1_UnitCount).OrderBy(x => x));
			CombineAssertions(() =>
			{
				AssertEquals(@"SAT-GTS- 2.0000", actual);
				AssertEquals(@"Information|Begin processing Product=ABC,Category=SAT,ABC Product
Information|End processing Product=ABC,Category=SAT,ABC Product
", process.ServiceLogger.ToString());
			});
		}

		[TestDate(2018, 2, 1)]
		public void TestUpdateFlightStats()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			Factory.Save();
			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FMS", "FMS", new ZDateTime(2018, 1, 1, 0, 0, 0), lic1, "", "", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FMS", "FMS", new ZDateTime(2018, 1, 15, 0, 0, 0), lic1, "", "", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FMS", "FMS", new ZDateTime(2018, 1, 20, 0, 0, 0), lic1, "", "", "", ""));
			EServicesBillingTestHelper.AddTransactions(infoList);
			var process = new ChargeableUsageServiceTaskForOtherSystemsTest(new[] { BillingConstants.BillingSystem.FlightStats });
			process.ServiceLogger = new TestServiceLogger();
			var billingPeriod = new BillingPeriod(new ZDateTime(2018, 1, 1));
			process.RunTask();
			var factory2 = new BusinessObjectFactory();
			var results = factory2.Load<ClientChargeableUsage>(new ZQuery());
			AssertEquals(1, results.Length);
			var m1 = results.Single();
			AssertEquals(3m, m1.U1_UnitCount);
		}

		public void TestOnChargeableUsageUpdate()
		{
			var task = new ChargeableUsageServiceTaskForOnChargeableUsageUpdateTest();
			task.ServiceLogger = new TestServiceLogger();
			task.ChargeableUsageUpdate(new BillingPeriod(new ZDateTime(2023, 1, 1)), "MYS");
			AssertEquals(@"Information|Begin processing 
Information|MYS - OnChargeableUsageUpdate()
Information|End processing 
", task.ServiceLogger.ToString());
		}

		[TestDate(2022, 1, 27)]
		public void TestRunTask_DatabaseConsolidation()
		{
			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			mappings.AddNew("DEF", "DEF Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "A01", true);
			var db1 = lic1.Database;
			db1.LD_DatabaseNumber = 1901;
			db1.LD_HostedLocation = "SYD";
			db1.LD_Product = "ABC";
			db1.LD_OH_WebAccessOrg = org1.PK;

			var lic2 = BillingTestHelper.CreateAnotherDatabase(lic1, "A02");
			var db2 = lic2.Database;
			db2.LD_DatabaseNumber = 1902;
			db2.LD_HostedLocation = "SYD";
			db2.LD_Product = "ABC";
			db2.LD_OH_WebAccessOrg = org1.PK;
			Factory.Save();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var lic2_1 = BillingTestHelper.CreateLicence(Factory, "EN2", "CO2", "A01", true);
			var db2_1 = lic2_1.Database;
			db2_1.LD_DatabaseNumber = 1903;
			db2_1.LD_HostedLocation = "SYD";
			db2_1.LD_Product = "DEF";
			db2_1.LD_OH_WebAccessOrg = org2.PK;

			var lic2_2 = BillingTestHelper.CreateAnotherDatabase(lic2_1, "A02");
			var db2_2 = lic2_2.Database;
			db2_2.LD_DatabaseNumber = 1904;
			db2_2.LD_HostedLocation = "SYD";
			db2_2.LD_Product = "DEF";
			db2_2.LD_OH_WebAccessOrg = org2.PK;
			Factory.Save();

			var task = new ChargeableUsageServiceTaskForExecuteNonQueryTest(Array.Empty<string>());
			task.RunTask();
			AssertEquals(false, TestConnection.Exists("FROM dbo.EdiLicenceDatabaseConsolidationHistory"));

			var collection1 = new ConsolidatedBillingSettingCollection(null, null);
			collection1.Add(new ConsolidatedBillingSetting()
			{
				ProductCode = "ABC",
				Description = "123456789"
			});
			collection1.Add(new ConsolidatedBillingSetting()
			{
				ProductCode = "DEF",
				Description = "1234567890"
			});

			EDIDataRegistry.Instance.ConsolidatedBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection1);
			task = new ChargeableUsageServiceTaskForExecuteNonQueryTest(Array.Empty<string>());
			task.RunTask();
			AssertEquals(true, TestConnection.Exists("FROM dbo.EdiLicenceDatabaseConsolidationHistory"));

			var rowsAsString = string.Join("\r\n", Utilities.GetDataTableFromQuery(@"SELECT
EDH_Period, LD = D1.LD_DatabaseNumber, ConsolidatedDatabase = D2.LD_DatabaseNumber
FROM dbo.EdiLicenceDatabaseConsolidationHistory
JOIN dbo.LicenceDatabase D1 ON EDH_LD = D1.LD_PK
JOIN dbo.LicenceDatabase D2 ON EDH_LD_ConsolidatedDatabase = D2.LD_PK
ORDER BY 1,2,3;").Rows.OfType<DataRow>().Select(x => $"{x[0]}-{x[1]}-{x[2]}"));
			AssertEquals(@"202201-1901-1901
202201-1902-1901
202201-1903-1903
202201-1904-1903", rowsAsString);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		protected override void SetUpCore()
		{
			base.SetUpCore();
			process = new ChargeableUsageServiceTaskForExecuteNonQueryTest();
			logger = new TestServiceLogger();
			process.ServiceLogger = logger;
			EServicesBillingTestHelper.CreateTable();
			var systemLicenceCode = EnvProxy.Instance.CurrentCompany.GetLicenceCode();
			SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemLicenceCode);
		}

		protected override void TearDownCore()
		{
			EServicesBillingTestHelper.DropTable();
			base.TearDownCore();
		}

		ChargeableUsageServiceTask process;
		TestServiceLogger logger;
		class ChargeableUsageServiceTaskForTest : ChargeableUsageServiceTask
		{
			internal ChargeableUsageServiceTaskForTest()
			{
				ServiceLogger = new TestServiceLogger();
				BillingDb = new Mock<IBillingDatabase>(MockBehavior.Loose) { CallBase = true }.Object;
			}

			internal int CallsToUpdateOdpl;
			internal ZDateTime odplFirst;
			internal ZDateTime odplLast;

			public override void UpdateOdpl(BillingPeriod billingPeriod)
			{
				if (CallsToUpdateOdpl == 0)
				{
					odplFirst = odplLast = billingPeriod.PeriodStartDate;
				}
				else if (odplFirst > billingPeriod.PeriodStartDate)
				{
					odplFirst = billingPeriod.PeriodStartDate;
				}
				else if (odplLast < billingPeriod.PeriodStartDate)
				{
					odplLast = billingPeriod.PeriodStartDate;
				}
				else
				{
					Assert("duplicate periodStart", false);
				}

				++CallsToUpdateOdpl;
			}

			internal Dictionary<string, int> CallsToUpdateSystemMap = new Dictionary<string, int>();
			protected override void UpdateStl(ZDateTime currentPeriod)
			{
				UpdateStlPeriods.Add(currentPeriod.ToDateTime());
				base.UpdateStl(currentPeriod);
			}

			internal List<DateTime> UpdateStlPeriods = new List<DateTime>();
			internal override void ChargeableUsageUpdate(BillingPeriod billingPeriod, string systemDescription, string systemCode, string billingDbPriceItemCode = null, int keyRefIndex1 = 0, string storedProcedureName = BillingUsageSchema.ChargeableUsageUpdate)
			{
				if (CallsToUpdateSystemMap.ContainsKey(systemCode))
				{
					CallsToUpdateSystemMap[systemCode]++;
				}
				else
				{
					CallsToUpdateSystemMap.Add(systemCode, 1);
				}

				updateSystemCalls.Add(new UpdateSystemParams()
				{ Period = billingPeriod, SystemDescription = systemDescription, StoredProcedureName = storedProcedureName, SystemCode = systemCode });
			}

			internal List<UpdateSystemParams> updateSystemCalls = new List<UpdateSystemParams>();
			internal class UpdateSystemParams
			{
				public BillingPeriod Period { get; set; }

				public string SystemDescription { get; set; }

				public string StoredProcedureName { get; set; }

				public string SystemCode { get; set; }
			}

			internal int CallsToUpdateCPT;
			internal override void UpdateCPT(BillingPeriod billingPeriod)
			{
				++CallsToUpdateCPT;
			}

			protected override void UpdateClientMappingNames()
			{
			}

			internal int CallsToUpdateContactCountry;
			internal ZDateTime UpdateContactCountryPeriodDate;
			protected override void UpdateContactCountry(DateTime periodDate)
			{
				UpdateContactCountryPeriodDate = periodDate;
				++CallsToUpdateContactCountry;
			}
		}

		class ChargeableUsageServiceTaskForOtherSystemsTest : ChargeableUsageServiceTask
		{
			public ChargeableUsageServiceTaskForOtherSystemsTest(string[] otherBillingSystems)
			{
				BillingSystemCodesToUpdate = otherBillingSystems;
			}

			public void UpdateOtherBillingSystemsForTest(BillingPeriod billingPeriod)
			{
				base.UpdateOtherBillingSystems(billingPeriod);
			}

			protected override void UpdateClientMappingNames()
			{
			}

			internal override void UpdateCPT(BillingPeriod billingPeriod)
			{
			}

			public override void UpdateOdpl(BillingPeriod billingPeriod)
			{
			}

			protected override void UpdateStl(ZDateTime currentPeriod)
			{
			}

			protected override void UpdateLicenceModulesHistory()
			{
			}
		}

		class ChargeableUsageServiceTaskForOnChargeableUsageUpdateTest : ChargeableUsageServiceTask
		{
			protected override BillingSystemList AllBillingSystems
			{
				get
				{
					var systems = new BillingSystemList();
					systems.Clear();
					systems.Add(new MySystem());
					return systems;
				}
			}

			class MySystem : BillingSystem
			{
				public override string SystemCode => "MYS";

				public override SystemRawUsage LoadOdplRawUsage(BillingLoadRawUsageContext context)
				{
					throw new NotImplementedException();
				}

				public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
				{
					throw new NotImplementedException();
				}

				public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
				{
					throw new NotImplementedException();
				}

				public override StlRawUsage LoadStlRawUsage(BillingLoadRawUsageContext context)
				{
					throw new NotImplementedException();
				}

				protected override SystemBill CreateSystemBill()
				{
					throw new NotImplementedException();
				}

				protected override SystemUsage[] CreateSystemUsages(ClientChargeableUsage[] chargeableUsages)
				{
					throw new NotImplementedException();
				}

				public override void OnChargeableUsageUpdate(BillingPeriod billingPeriod, ILogger logger)
				{
					base.OnChargeableUsageUpdate(billingPeriod, logger);
					logger?.Information("MYS - OnChargeableUsageUpdate()");
				}
			}
		}

		class ChargeableUsageServiceTaskForExecuteNonQueryTest : ChargeableUsageServiceTask
		{
			public ChargeableUsageServiceTaskForExecuteNonQueryTest(string[] otherBillingSystems) : this()
			{
				BillingSystemCodesToUpdate = otherBillingSystems;
			}

			public ChargeableUsageServiceTaskForExecuteNonQueryTest()
			{
				ServiceLogger = new TestServiceLogger();
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "unit test")]
			protected override void ExecuteNonQuery(DbCommand cmd, string description, int timeoutMinutes)
			{
				if (description == nameof(UpdateLicenceDatabaseConsolidation))
				{
					base.ExecuteNonQuery(cmd, description, timeoutMinutes);
				}
				else
				{
					return;
				}
			}
		}
	}
}
