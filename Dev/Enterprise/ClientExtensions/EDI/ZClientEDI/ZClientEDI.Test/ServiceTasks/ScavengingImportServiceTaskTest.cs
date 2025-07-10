using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.eHub.Common.Extensions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;
using Enterprise.Client.EDI.ScavengingImportServiceTask.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.ServiceTasks
{
	[TestedType(typeof(ScavengingImportServiceTask))]
	[UseSnapshotProtection(skipTransaction: true)]
	public class ScavengingImportServiceTaskTest : ServiceTaskTestCase<ScavengingImportServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
			const string clientKey = "EJZORDORD";
			AddClientStatisticsFromFile("OrgACOART1", clientKey, DateTime.Now);
			var initialRow = SelectTopRow("ClientStatisticsXML");
			AssertEquals("Precondition: no row in table ClientStatisticsXMLArchive", 0, GetTableRowCount("ClientStatisticsXMLArchive"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(task.GetType().Name, canRunInAnyBranch: true))
			{
				AssertNoExceptionThrown(() => task.RunTask());
			}
			AssertNotContains(@"
Object reference not set to an instance of an object", task.ServiceLogger.ToString());
		}

		static IDisposable ClearUserContext()
		{
			var userContext = EnvProxy.Instance.CurrentUserContext;
			EnvProxy.Instance.ClearUserContext();
			(EnvProxy.Instance as IEnvironmentForTest)?.ResetSecurityForTest();
			return new DisposableAction(() =>
			{
				EnvProxy.Instance.SetUserContext(userContext);
			});
		}

		public void TestExecute()
		{
			const string clientKey = "EJZORDORD";
			AddClientStatisticsFromFile("OrgACOART1", clientKey, DateTime.Now);
			AssertEquals("Precondition: 1 row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			var initialRow = SelectTopRow("ClientStatisticsXML");
			AssertEquals("Precondition: no row in table ClientStatisticsXMLArchive", 0, GetTableRowCount("ClientStatisticsXMLArchive"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AssertEquals("row in ClientStatisticsXML should be deleted", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("row should be archived", 1, GetTableRowCount("ClientStatisticsXMLArchive"));
			var archivedRow = SelectTopRow("ClientStatisticsXMLArchive");
			AssertArrayEqualsByElements(initialRow, archivedRow);
			AssertEquals("1 import history record should be created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("1 new org should be created", orgCount + 1, GetTableRowCount(OrgHeader.Schema.TableName));
			AssertEquals("2 consols should be created", 2, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			var orgHeaders = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "ACOARTLUD"));
			AssertEquals("There should be 1 org with ACOARTLUD code", 1, orgHeaders.Length);
			var org = orgHeaders[0];
			AssertEquals("PK from xml should be used", new ZGuid("a86ba5e6-6b95-4ee6-bfd4-2847681d1822"), org.PK);
			var query = new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, org.PK);
			var overrides = org.Factory.Load<OrgPatternMatchOverride>(query);
			Assert("Client eHub ID is stored in EDI Code Mapping", overrides.Length == 1 && overrides[0].OO_ForeignCode == clientKey && overrides[0].OO_Relationship == EDIConstants.OrgPatternMatchOverrideRelationships.EHubClientID);
			var histories = Factory.Load<ClientOrgImportHistory>(new ZQuery(ClientOrgImportHistorySchema.O2_TargetOrgPK, org.PK));
			AssertEquals("There should be 1 import history", 1, histories.Length);
			CombineAssertions("History parameters", () =>
			{
				AssertEquals("Client Key", clientKey, histories[0].O2_ClientKey);
				AssertEquals("Org Code", "ACOARTLUD", histories[0].O2_OrgCode);
				AssertEquals("Action Type ", "CreateNew", histories[0].O2_ActionType);
				AssertEquals("Match Count", 0, histories[0].O2_MatchCount);
			});
			var consols = Factory.Load<ClientOrgConsol>(new ZQuery(ClientOrgConsolSchema.O7_ClientKey, clientKey));
			AssertEquals("There should be 2 consols", 2, consols.Length);
			var receivingConsols = consols.Where(c => c.O7_AgentType == "Receiving");
			AssertEquals("1 receiving consol should be created", 1, receivingConsols.Count());
			var receivingConsol = receivingConsols.First();
			CombineAssertions(() =>
			{
				AssertEquals("Consol Transport Mode", "SEA", receivingConsol.O7_TransMode);
				AssertEquals("Consol Loading Port", "AUSYD", receivingConsol.O7_LoadPort);
				AssertEquals("Consol Discharge Port", "USCHI", receivingConsol.O7_DischargePort);
				AssertEquals("Consol Total Weight", (ZDecimal)21275000.000, receivingConsol.O7_TotalWeight);
				AssertEquals("Consol Total Volume", (ZDecimal)0.090, receivingConsol.O7_TotalVolume);
				AssertEquals("Consol Shipment Count", (Int16)2, receivingConsol.O7_ShipCount);
				AssertEquals("Consol Create Time", new ZDateTime(2013, 10, 5, 22, 0, 0), receivingConsol.O7_CreateUTC);
				AssertEquals("Consol Export Time", new ZDateTime(2014, 2, 21, 9, 58, 0), receivingConsol.O7_ExportUTC);
				AssertEquals("Consol OH PK", org.PK, receivingConsol.O7_OH);
			});
			var sendingConsols = consols.Where(c => c.O7_AgentType == "Sending");
			AssertEquals("1 sending consol should be created", 1, sendingConsols.Count());
			var sendingConsol = sendingConsols.First();
			CombineAssertions(() =>
			{
				AssertEquals("Consol Transport Mode", "Sending", sendingConsol.O7_AgentType);
				AssertEquals("Consol Transport Mode", "AIR", sendingConsol.O7_TransMode);
				AssertEquals("Consol Loading Port", "USLAX", sendingConsol.O7_LoadPort);
				AssertEquals("Consol Discharge Port", "AUSYD", sendingConsol.O7_DischargePort);
				AssertEquals("Consol Total Weight", (ZDecimal)50.421, sendingConsol.O7_TotalWeight);
				AssertEquals("Consol Total Volume", (ZDecimal)9.200, sendingConsol.O7_TotalVolume);
				AssertEquals("Consol Shipment Count", (Int16)3, sendingConsol.O7_ShipCount);
				AssertEquals("Consol Create Time", new ZDateTime(2013, 10, 6, 23, 0, 0), sendingConsol.O7_CreateUTC);
				AssertEquals("Consol Export Time", new ZDateTime(2014, 2, 21, 9, 58, 1), sendingConsol.O7_ExportUTC);
				AssertEquals("Consol OH PK", org.PK, sendingConsol.O7_OH);
			});
		}

		public void TestFailure()
		{
			const string clientKey = "ACEPROSYD";
			AddClientStatisticsFromFile("OrgINVALID", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			Assert("Incorrect log message", task.ServiceLogger.ToString().StartsWith(@"Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...
"));
			Assert("Incorrect log message", task.ServiceLogger.ToString().EndsWith(@"Information|0 organizations have been imported to database.
"));
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 0, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestInconsistentParentPK()
		{
			const string clientKey = "ASECARSYD";
			var ofcAddressCapability = Factory.LoadTop1<OrgAddressCapability>(new ZQuery(OrgAddressCapabilitySchema.PZ_AddressType, "OFC"));
			var text = GetTestFileText("OrgAIRINTSYD");
			var xml = text.Replace("606abdf0-cc84-4fbb-bb65-6eac3bcf8be9", ofcAddressCapability.PZ_OA.ToString());
			AddClientStatisticsFromXML(xml, clientKey, DateTime.Now);
			var task1 = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task1.RunTask();
			var org1 = Factory.Load<OrgHeader>(new ZGuid("2796344e-86fc-4397-9142-1ac359d17245"));
			CombineAssertions(() =>
			{
				AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...".Trim(), task1.ServiceLogger.ToString());
				AssertContains("Warning|Message", task1.ServiceLogger.ToString());
				AssertContains("Database parent does not match entity parent. All PKs will be stripped and item will be reprocessed.", task1.ServiceLogger.ToString());
				AssertContains("Information|1 organization has been imported to database.", task1.ServiceLogger.ToString());
				AssertNotNull("Organization is in database", org1);
				AssertEquals("Conflict note was added", 1, org1.Notes.FindByDescription("Scavenging Import Conflict").Length);
				AssertEquals("One has 1 address", 1, org1.Addresses.Count);
				AssertEquals("Address phone number", "61 2 9317 5577", org1.Addresses[0].OA_Phone);
			});
			AddClientStatisticsFromXML(xml.Replace("<Phone>61 2 9317 5577</Phone>", "<Phone>61 2 1234 5678</Phone>"), clientKey, DateTime.Now);
			var task2 = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task2.RunTask();
			var org2 = new BusinessObjectFactory().Load<OrgHeader>(new ZGuid("2796344e-86fc-4397-9142-1ac359d17245"));
			CombineAssertions(() =>
			{
				AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...".Trim(), task2.ServiceLogger.ToString());
				AssertContains("Warning|Message", task2.ServiceLogger.ToString());
				AssertContains("Database parent does not match entity parent. All PKs will be stripped and item will be reprocessed.", task2.ServiceLogger.ToString());
				AssertContains("Information|1 organization has been imported to database.", task2.ServiceLogger.ToString());
				AssertEquals("Conflict note was added", 2, org2.Notes.FindByDescription("Scavenging Import Conflict").Length);
				AssertEquals("One has 1 address", 1, org2.Addresses.Count);
				AssertEquals("Address phone number updated", "61 2 1234 5678", org2.Addresses[0].OA_Phone);
			});
		}

		[SnailTest]
		[TestDate(2021, 9, 1)]
		public void TestSystemWithErrorMarkedAsProcessedShouldNotBeProcessedAgain()
		{
			AddClientStatisticsFromFile("OrgACOART2_Error", "EJZORD111", DateTime.Now);
			AddClientStatisticsFromFile("OrgACOART2_Error", "EJZTTT111", DateTime.Now);
			AddClientStatisticsFromFile("OrgACOART2_Error", "EJZORD111", DateTime.Now);
			AddClientStatisticsFromFile("OrgACOART2", "EJZORD111", DateTime.Now);
			AddClientStatisticsFromFile("OrgACOART2_Error", "EJZTTT222", DateTime.Now);
			var task = new Mock<ScavengingImportServiceTask>() { CallBase = true };
			task.Object.ServiceLogger = new TestServiceLogger();
			task.Object.RunTask();
			AssertContains(@"Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)".TrimStart(), task.Object.ServiceLogger.ToString());
			AssertEquals("Precondition: All 5 rows in table ClientStatisticsXMLArchive", 5, GetTableRowCount("ClientStatisticsXMLArchive"));
			AssertEquals("The exception should be recorded 4 times", 4, System.Text.RegularExpressions.Regex.Matches(task.Object.ServiceLogger.ToString(), "System.Exception: Record: Organization failed to Import:").Count);
		}

		public void TestPKandCodesConflict()
		{
			AddClientStatisticsFromFile("OrgNMTOCEAKL1", "NMTOCEBNE", DateTime.Now);
			var task1 = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task1.RunTask();
			AssertSuccessLog(task1);
			var factory1 = new BusinessObjectFactory();
			var org1 = factory1.Load<OrgHeader>(new ZGuid("699c9a5c-b5f1-4d3c-b7b4-07ff47103139"));
			AssertEquals(1, org1.Addresses.Count);
			AssertEquals(new ZGuid("bab11107-aced-49c3-b1d9-0145531f2250"), org1.Addresses[0].PK);
			AddClientStatisticsFromFile("OrgNMTOCEAKL2", "NMTOCEBNE", DateTime.Now);
			var task2 = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task2.RunTask();
			AssertSuccessLog(task2);
			var factory2 = new BusinessObjectFactory();
			var org2 = factory2.Load<OrgHeader>(new ZGuid("ca87660a-33db-4da0-a1f7-776dcdd201d9"));
			AssertEquals(1, org2.Addresses.Count);
			var address21 = org2.Addresses[0];
			AssertEquals(new ZGuid("16fed5d1-1803-49e3-8291-021a258c9e1a"), address21.PK);
			AssertEquals("02 55555555", address21.OA_Phone);
			AddClientStatisticsFromFile("OrgNMTOCEAKL3", "NMTOCEBNE", DateTime.Now);
			var task3 = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task3.RunTask();
			var log = task3.ServiceLogger.ToString();
			AssertContains("Warning|Message", log);
			AssertContains("Database parent does not match entity parent. All PKs will be stripped and item will be reprocessed.", log);
			AssertContains("Information|1 organization has been imported to database.", log);
			var factory3 = new BusinessObjectFactory();
			var org3 = factory3.Load<OrgHeader>(new ZGuid("699c9a5c-b5f1-4d3c-b7b4-07ff47103139"));
			AssertEquals(3, org3.Addresses.Count);
			var address3 = org3.Addresses.FindByPK(new ZGuid("bab11107-aced-49c3-b1d9-0145531f2250")) as OrgAddress;
			AssertNotNull(address3);
			AssertEquals("Phone was udated", "02 12345678", address3.OA_Phone);
			var address22 = factory3.Load<OrgAddress>(new ZGuid("16fed5d1-1803-49e3-8291-021a258c9e1a"));
			AssertEquals("Phone was not udated", "02 55555555", address22.OA_Phone);
		}

		public void TestProblemFile_TOLGLOSHA()
		{
			const string clientKey = "TOLGLOSHA";
			AddClientStatisticsFromFile("TOLGLOSHA", clientKey, DateTime.Now);
			AssertEquals("Precondition: 1 row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			var initialRow = SelectTopRow("ClientStatisticsXML");
			AssertEquals("Precondition: no row in table ClientStatisticsXMLArchive", 0, GetTableRowCount("ClientStatisticsXMLArchive"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var log = task.ServiceLogger.ToString();
			AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...".Trim(), log);
			AssertContains("Information|1 organization has been imported to database.", log);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_COMPES_CL1()
		{
			const string clientKey = "ANDLOGMIA";
			var algorithm = new OrgCodeAlgorithm { AlgorithmType = OrgCodeAlgorithmType.Default, RegenerateOrgCodeOnChanges = true };
			algorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.FirstName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.SecondName].Order = 2;
			algorithm.Elements[OrgCodeElementDescription.SecondName].Length = 3;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Order = 3;
			algorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Length = 6;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);
			var org0 = Factory.NewWithValidTestData<OrgHeader>();
			org0.OH_Code = "COM000001_CL";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "COM999999_CL";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "CO1000000_CL";
			Factory.Save();
			AddClientStatisticsFromFile("OrgCOMPES_CL1", clientKey, DateTime.Now);
			AssertEquals("Precondition: 1 row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			var initialRow = SelectTopRow("ClientStatisticsXML");
			AssertEquals("Precondition: no row in table ClientStatisticsXMLArchive", 0, GetTableRowCount("ClientStatisticsXMLArchive"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var log = task.ServiceLogger.ToString();
			AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...".Trim(), log);
			AssertContains("Information|1 organization has been imported to database.", log);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
			Assert("Org code generated correctly", Factory.Exists(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_Code, "COM000000_CL")));
		}

		public void TestProblemFile_ACAINTSYD3()
		{
			const string clientKey = "ACAINTSYD";
			var findCMMainImportCmdty = new ZQuery(RefCommodityCodeSchema.RH_Code, "XXXX");
			var findIMDefaultServiceLevel = new ZQuery(RefServiceLevelSchema.RS_Code, "XXX");
			var commodityCode = Factory.LoadTop1<RefCommodityCode>(findCMMainImportCmdty);
			var serviceLevelCode = Factory.LoadTop1<RefServiceLevel>(findIMDefaultServiceLevel);
			AddClientStatisticsFromFile("OrgACAINTSYD3", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			AssertNull(commodityCode);
			AssertNull(serviceLevelCode);
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var orgnisation = new BusinessObjectFactory().LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, Guid.Parse("8d155ca6-5d20-4735-900e-ebf5edc3c300")));
			AssertEquals("XXXX", orgnisation.MiscServ.CMMainImportCmdty.RH_Code);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
			AddClientStatisticsFromFile("Org219237", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: a added row in table ClientOrgImportHistory", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			task.RunTask();
			var log = task.ServiceLogger.ToString();
			AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...".Trim(), log);
			AssertContains("Warning|Message", log);
			AssertContains("Could not insert/update the Organization Details (OrgMiscServ) as it had an invalid reference to a CMMainImportCmdty (CMMainImportCmdty).", log);
			AssertContains("Information|1 organization has been imported to database.", log);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 2, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
			var orgnisation2 = new BusinessObjectFactory().LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, Guid.Parse("f408c9db-61b9-4c8d-ac60-70c5a456f3fa")));
			AssertEquals("XXXX", orgnisation2.MiscServ.CMMainImportCmdty.RH_Code);
			AssertEquals("XXX", orgnisation2.MiscServ.IMDefaultServiceLevel.RS_Code);
			AssertEquals(orgnisation.MiscServ.CMMainImportCmdty.PK, orgnisation2.MiscServ.CMMainImportCmdty.PK);
		}

		public void TestProblemFile_ENLPACBNE()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "ENLOG PACIFIC HOLDINGS (RGM AUSTRALIA) (FW44E)";
			org.OH_RL_NKClosestPort = "AUBNE";
			org.OH_Code = "ENLPAC000052";
			var address1 = org.Addresses.AddNew();
			address1.OA_Code = "DLV: LOMANDRA DRIVE";
			address1.OA_Address1 = "LOMANDRA DRIVE";
			var address2 = org.Addresses.AddNew();
			address2.OA_Code = "94-95 LOMANDRA DRIVE";
			address2.OA_Address1 = "94-95 LOMANDRA DRIVE";
			Factory.Save();
			const string clientKey = "KJVINTSYD";
			var text = GetTestFileText("OrgENLPACBNE");
			var xml = text.Replace("1c718073-f471-45d1-acef-0ae4d38e7a96", org.PK.ToString()).Replace("60199185-409a-4eea-a1e7-6d19de3cfb2c", address1.PK.ToString()).Replace("fb369341-cb36-4cbd-8d76-90f3df69ff61", address2.PK.ToString()).Replace("3fa9e3fa-c4a0-4c83-a5b2-81abdcd4ee74", org.MiscServ.PK.ToString());
			AddClientStatisticsFromXML(xml, clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var log = task.ServiceLogger.ToString();
			AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...".Trim(), log);
			AssertContains("Information|1 organization has been imported to database.", log);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			var assertAddress1 = new BusinessObjectFactory().Load<OrgAddress>(address1.PK);
			AssertNotNull(assertAddress1);
			AssertEquals("Address was updated", "94-95 LOMANDRA DRIVE", assertAddress1.OA_Code);
			AssertEquals("Address was updated", "94-95 LOMANDRA DRIVE (THE OLD WEATHER STATION)", assertAddress1.OA_Address1);
			var assertAddress2 = new BusinessObjectFactory().Load<OrgAddress>(address2.PK);
			AssertNotNull(assertAddress2);
			AssertEquals("Address was updated", "HEADOFFICE", assertAddress2.OA_Code);
			AssertEquals("Address was updated", "131 MOORINGE AVE", assertAddress2.OA_Address1);
		}

		public void TestProblemFile_VINAFREIGHT()
		{
			const string clientKey = "HABSPESGN";
			AddClientStatisticsFromFile("OrgVINAFREIGHT", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_ACAINTSYD()
		{
			const string clientKey = "ACAINTSYD";
			AddClientStatisticsFromFile("OrgACAINTSYD", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
			AddClientStatisticsFromFile("OrgACAINTSYD2", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: a added row in table ClientOrgImportHistory", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			task.RunTask();
			var log = task.ServiceLogger.ToString();
			AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...".Trim(), log);
			AssertContains("Information|1 organization has been imported to database.", log);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 2, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 0, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_CONCONMEL()
		{
			const string clientKey = "CONCONMEL";
			AddClientStatisticsFromFile("OrgCONCONMEL", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
			AddClientStatisticsFromFile("OrgCONCONMEL2", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: a added row in table ClientOrgImportHistory", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			task.RunTask();
			var log = task.ServiceLogger.ToString();
			AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...".Trim(), log);
			AssertContains("Information|1 organization has been imported to database.", log);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 2, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 0, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		//WI00083282
		public void TestProblemFile_WCBWORMEL()
		{
			const string clientKey = "WCBWORMEL";
			AddClientStatisticsFromFile("OrgWCBWORMEL", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var log = task.ServiceLogger.ToString();
			AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...".Trim(), log);
			AssertContains("Warning|Message", log);
			AssertContains("Invalid code will be replaced, and the item will be reprocessed", log);
			AssertContains("Information|1 organization has been imported to database.", log);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
			var orgMiscServ = new BusinessObjectFactory().LoadTop1<OrgMiscServ>(new ZQuery(OrgMiscServSchema.OM_RH_NKCMMainImportCmdty, "XXXX"));
			AssertNotNull(orgMiscServ);
		}

		//WI00083285
		public void TestProblemFile_FRESER()
		{
			const string clientKey = "FRESER";
			AddClientStatisticsFromFile("OrgFRESER", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var log = task.ServiceLogger.ToString();
			AssertSuccessLog(task);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		//WI00083286
		public void TestProblemFile_GUAADVHUA()
		{
			const string clientKey = "GUAADVHUA";
			AddClientStatisticsFromFile("OrgGUAADVHUA", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var log = task.ServiceLogger.ToString();
			AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...".Trim(), log);
			AssertContains("Warning|Message", log);
			AssertContains("Invalid code will be replaced, and the item will be reprocessed", log);
			AssertContains("Information|0 organizations have been imported to database.", log);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 0, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
			var orgMiscServ = new BusinessObjectFactory().LoadTop1<OrgMiscServ>(new ZQuery(OrgMiscServSchema.OM_RX_NKEXDefCurrency, "XXX"));
			AssertNull(orgMiscServ);
		}

		//WI00083289
		public void TestProblemFile_AUSTB()
		{
			const string clientKey = "UNTLAXLAX";
			AddClientStatisticsFromFile("OrgAUSTB", clientKey, DateTime.Now);
			AssertEquals("Precondition: 1 row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			var initialRow = SelectTopRow("ClientStatisticsXML");
			AssertEquals("Precondition: no row in table ClientStatisticsXMLArchive", 0, GetTableRowCount("ClientStatisticsXMLArchive"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			int addrCount = GetTableRowCount(OrgAddress.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var log = task.ServiceLogger.ToString();
			AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...".Trim(), log);
			AssertContains("Warning|Message", log);
			AssertContains("Invalid code will be replaced, and the item will be reprocessed", log);
			AssertContains("Information|1 organization has been imported to database.", log);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
			AssertEquals("Address created", 4, GetTableRowCount(OrgAddress.Schema.TableName) - addrCount);
		}

		public void TestProblemFile_OrgSYDFRESYD()
		{
			const string clientKey = "CYCGLOSYD";
			AddClientStatisticsFromFile("OrgSYDFRESYD", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			var orgPK = new ZGuid("d69ba2c8-65bc-4148-91fb-191770ea5b32");
			var org1 = Factory.Load<OrgHeader>(orgPK);
			AssertNotNull(org1);
			var padAddress = Factory.Load<OrgAddress>(new ZGuid("49225032-e46b-4d7a-a6a5-e91441e48fc8"));
			AssertEquals("Precondition:address belongs to org1", orgPK, padAddress.OA_OH);
			var filter = new ZQuery(OrgAddressCapabilitySchema.PZ_OA, padAddress.PK);
			filter.AddToFilter(OrgAddressCapabilitySchema.PZ_IsMainAddress, false);
			filter.AddToFilter(OrgAddressCapabilitySchema.PZ_AddressType, OrgAddressType.PickupAndDelivery.Code);
			var addrCapability = Factory.Load<OrgAddressCapability>(filter);
			AssertEquals(1, addrCapability.Length);
			var text = GetTestFileText("OrgSYDFRESYD");
			var xml = text.Replace("41eda975-eef3-4ec8-b22b-9c3825e9a400", ZGuid.NewZGuid().ToString());
			AddClientStatisticsFromXML(xml, clientKey, DateTime.Now);
			var task2 = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task2.RunTask();
			AssertSuccessLog(task2);
		}

		public void TestProblemFile_OrgSAFPANJNB1()
		{
			const string clientKey = "SAFPANJNB1";
			AddClientStatisticsFromFile("OrgSAFPANJNB", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			var orgPK = new ZGuid("79764d4f-acf0-4295-ac15-8abf0c89feef");
			var org1 = Factory.Load<OrgHeader>(orgPK);
			AssertNotNull(org1);
			AssertNotNull("MiscServ record should not be null", org1.MiscServ);
			var text = GetTestFileText("OrgSAFPANJNB");
			var xml = text.Replace(org1.MiscServ.PK.ToString(), ZGuid.NewZGuid().ToString());
			AddClientStatisticsFromXML(xml, clientKey, DateTime.Now);
			var task2 = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task2.RunTask();
			AssertSuccessLog(task2);
		}

		public void TestProblemFile_DIVLABHYD()
		{
			const string clientKey = "DIVLABHYD";
			AddClientStatisticsFromFile("OrgDIVLABHYD", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
			AddClientStatisticsFromFile("OrgDIVLABHYD2", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: a added row in table ClientOrgImportHistory", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			task.RunTask();
			var log = task.ServiceLogger.ToString();
			AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...".Trim(), log);
			AssertContains("Warning|Message", log);
			AssertContains("The value of Organization + Address Code must be unique on Address. The duplicate value(s) are: (6007a2ae-831c-476f-9ba7-7bec741e64d6, 100% EOU CHIPPADA VILLAGE).", log);
			AssertContains("Information|1 organization has been imported to database.", log);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 2, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 0, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_CATHAYDEFRA()
		{
			const string clientKey = "CATHAYDEFRA";
			AddClientStatisticsFromFile("OrgCATHAYDEFRA", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
			AddClientStatisticsFromFile("OrgCATHAYDEFRA2", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: a added row in table ClientOrgImportHistory", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			task.RunTask();
			var log = task.ServiceLogger.ToString();
			AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...".Trim(), log);
			AssertContains("Information|1 organization has been imported to database.", log);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 2, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 0, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_SCHENK_AU()
		{
			const string clientKey = "ACEPROSYD";
			AddClientStatisticsFromFile("OrgSCHENK_AU", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_DHLINTAKL()
		{
			const string clientKey = "DHLINTAKL";
			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, Guid.Parse("0879927d-5b88-4124-aa4b-142cfab5ee65")));
			AssertNull(orgHeader);
			AddClientStatisticsFromFile("OrgFULLON2", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var log = task.ServiceLogger.ToString();
			AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...".Trim(), log);
			AssertContains(@"
Could not insert/update the Organization (OrgHeader) as it had an invalid reference to a ClosestPort (ClosestPort). There is no ClosestPort with the following values: [Code:?]LIV].
 Invalid code will be replaced, and the item will be reprocessed.".Trim(), log);
			AssertContains(@"
Could not insert/update the Address (OrgAddress) as it had an invalid reference to a RelatedPortCode (RelatedPortCode). There is no RelatedPortCode with the following values: [Code:?]LIV].
 Invalid code will be replaced, and the item will be reprocessed.".Trim(), log);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
			orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, Guid.Parse("0879927d-5b88-4124-aa4b-142cfab5ee65")));
			AssertNotNull(orgHeader);
			AssertEquals("XXXXX", orgHeader.ClosestPort.Code);
		}

		public void TestProblemFile_AIRLOGGOT()
		{
			const string clientKey = "SSENSEHKG";
			AddClientStatisticsFromFile("OrgAIRLOGGOT", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_FIRGLOAKL()
		{
			const string clientKey = "GWETAY";
			var xxxxx = Factory.New<RefUNLOCO>();
			xxxxx.RL_Code = "XXXXX";
			Factory.Save();
			AddClientStatisticsFromFile("OrgFIRGLOAKL", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var log = task.ServiceLogger.ToString();
			AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...".Trim(), log);
			AssertContains("Warning|Message", log);
			AssertContains("Address PK and Code mismatch. All addresses PKs will be stripped and item will be reprocessed.", log);
			AssertContains("Information|1 organization has been imported to database.", log);
			var addresses = Factory.Load<OrgAddress>(new ZQuery(OrgAddressSchema.OA_OH, new Guid("4905289c-29f4-469c-8640-d154fd01e641")));
			AssertEquals(2, addresses.Length);
			AssertNotNull(addresses.FirstOrDefault(a => string.Equals("3 HOYLAKE COURTCORNUBIA", a.OA_Code, StringComparison.OrdinalIgnoreCase)));
			AssertNotNull(addresses.FirstOrDefault(a => string.Equals("3 Hoylake Ct", a.OA_Code, StringComparison.OrdinalIgnoreCase)));
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_JHBACHMNN()
		{
			const string clientKey = "JHBACHMNN";
			AddClientStatisticsFromFile("OrgJHBACHMNN", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_DEMORG()
		{
			const string clientKey = "YUSNLGSTC";
			AddClientStatisticsFromFile("OrgDEMORG", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertEquals(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...
Information|Skipping message in old format which is no longer supported.
Information|0 organizations have been imported to database.
".TrimStart(), task.ServiceLogger.ToString());
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 0, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_CAFCASAKL()
		{
			var factory = new BusinessObjectFactory();
			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "CAFORD CASTORS LTD";
			org.OH_RL_NKClosestPort = "NZAKL";
			org.OH_Code = "CAFCASAKL";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Kim";
			contact1.OC_Title = "Software Developer";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Julie";
			contact2.OC_Title = "Receptionist";
			var britishEnglishUser = factory.NewWithValidTestData<GlbStaff>();
			britishEnglishUser.GS_FullName = "Bean";
			britishEnglishUser.GS_WorkingLanguage = Core.SharedConstants.Languages.EnglishBritish;
			factory.Save();
			const string clientKey = "TRALOGAKL1";
			var text = ScavengingImportServiceTaskTest.GetTestFileText("OrgCAFCASAKL");
			var xml = text.Replace("e0e19891-b3b1-4663-a33e-5616dc0baf17", org.PK.ToString()).Replace("650b6738-eba4-46ef-9392-67af4ad97cf4", org.MiscServ.PK.ToString());
			ScavengingImportServiceTaskTest.AddClientStatisticsFromXML(xml, clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, ScavengingImportServiceTaskTest.GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, ScavengingImportServiceTaskTest.GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, ScavengingImportServiceTaskTest.GetTableRowCount(ClientOrgConsol.Schema.TableName));
			var log = string.Empty;
			using (Env.SetTemporaryUserContext(britishEnglishUser.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
				task.RunTask();
				log = task.ServiceLogger.ToString();
			}

			AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...".Trim(), log);
			AssertContains("Warning|Message", log);
			AssertContains("Contact PK and Code mismatch. All Contact PKs will be stripped and item will be reprocessed.", log);
			AssertContains("Information|1 organization has been imported to database.", log);
			var newFactory = new BusinessObjectFactory();
			var reloadedContacts = newFactory.Load<OrgContact>(new ZQuery(OrgContactSchema.OC_OH, org.PK));
			AssertEquals("reloadedContacts.Length", 3, reloadedContacts.Length);
			var reloadedContact1 = reloadedContacts.First(c => c.PK == contact1.PK);
			AssertEquals("Kim", reloadedContact1.OC_ContactName);
			AssertEquals("Kim's title should be updated", "Business Analyst", reloadedContact1.OC_Title);
			var reloadedContact2 = reloadedContacts.First(c => c.PK == contact2.PK);
			AssertEquals("Julie", reloadedContact2.OC_ContactName);
			AssertEquals("Julie's title should not be touched", "Receptionist", reloadedContact2.OC_Title);
			var reloadedContact3 = reloadedContacts.First(c => !new[] { contact1.PK, contact2.PK }.Contains(c.PK));
			AssertEquals("Bob Phillips should be added", "Bob Phillips", reloadedContact3.OC_ContactName);
			AssertEquals("ClientStatisticsXML records", 0, ScavengingImportServiceTaskTest.GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, ScavengingImportServiceTaskTest.GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
		}

		public void TestProblemFile_CARTRASYD()
		{
			AddClientStatisticsFromFile("OrgCARTRASYD1", "COMCUSSYD", DateTime.Now);
			AddClientStatisticsFromFile("OrgCARTRASYD2", "AAACUSSYD", DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 2, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertEquals(@"
Information|Retrieving messages...
Information|2 messages retrieved.
Information|Importing messages...
Information|2 organizations have been imported to database.
".TrimStart(), task.ServiceLogger.ToString());
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 2, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_GENCARZZZ()
		{
			AddClientStatisticsFromFile("OrgGENCARZZZ1", "GATCARAKL", DateTime.Now);
			AddClientStatisticsFromFile("OrgGENCARZZZ2", "AAACARAKL", DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 2, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertEquals(@"
Information|Retrieving messages...
Information|2 messages retrieved.
Information|Importing messages...
Information|2 organizations have been imported to database.
".TrimStart(), task.ServiceLogger.ToString());
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 2, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_AGSWORTRA()
		{
			const string clientKey = "FIRPORLOG";
			AddClientStatisticsFromFile("OrgAGSWORTRA", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_OTSASTDEOSF()
		{
			const string clientKey = "OTSASTDEOSF";
			AddClientStatisticsFromFile("OrgOTSASTDEOSF", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_CPT()
		{
			const string clientKey = "OTSASTDEOSF";
			AddClientStatisticsFromFile("OrgCPT", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_HORDISYKM()
		{
			const string clientKey = "ALAEXPHWD";
			AddClientStatisticsFromFile("OrgHORDISYKM", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_KALINETRY()
		{
			const string clientKey = "UNILOGSOF1";
			InsertBGLCurrencyIfDoesNotExist();
			AddClientStatisticsFromFile("OrgKALINETRY", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
			void InsertBGLCurrencyIfDoesNotExist()
			{
				TestConnection.ExecuteNonQuery(@"
				IF NOT EXISTS (SELECT 1 FROM dbo.RefCurrency WHERE RX_PK = '665002b7-edbb-44cb-9315-69011e293ffe')
				BEGIN
					INSERT INTO [dbo].[RefCurrency]
					   ([RX_PK]
					   ,[RX_Code]
					   ,[RX_IsActive]
					   ,[RX_IsSystem]
					   ,[RX_Symbol]
					   ,[RX_Desc]
					   ,[RX_UnitName]
					   ,[RX_SubUnitName]
					   ,[RX_SubUnitRatio]
					   ,[RX_ISOSubUnitRatio])
						VALUES
					   ('665002b7-edbb-44cb-9315-69011e293ffe'
					   , 'BGL'
					   , 1
					   , 0
					   , '$'
					   , 'BGL Description'
					   , 'Dollar'
					   , 'Cents'
					   , 100
					   , 100)
				 END");
			}
		}

		public void TestProblemFile_ITT()
		{
			const string clientKey = "BURGERRTM";
			var sql = "Update dbo.RefUNLOCO Set RL_IATA = '', RL_Code = 'GR86' WHERE RL_Code = 'NLITT'"; //simulating GR86 existing, as it no longer does
			Db.Connection.ExecuteNonQuery(sql);
			AddClientStatisticsFromFile("OrgITT", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertEquals(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...
Error|Org code generated was empty. Please adjust the Org Code generation setting in the registry at 'Organisation > Codes > Organisation Code - Default Set'.
Information|0 organizations have been imported to database.
".TrimStart(), task.ServiceLogger.ToString());
			AssertEquals("ClientStatisticsXML records", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 0, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_SATREMLEH()
		{
			const string clientKey = "BURGERRTM";
			AddClientStatisticsFromFile("OrgSATREMLEH", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_MERFOOTRA()
		{
			const string clientKey = "SEALINDUB";
			AddClientStatisticsFromFile("OrgMERFOOTRA", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_FURPLAAKL()
		{
			const string clientKey = "GOFREIMEL";
			AddClientStatisticsFromFile("OrgFURPLAAKL", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			var cusCode = Factory.Load<OrgCusCode>(new ZGuid("40985b1b-43e8-4d02-982d-cc4028f63909"));
			AssertEquals("CCD", cusCode.OK_CodeType);
			AssertEquals("NZ", cusCode.CodeCountry.Code);
			AddClientStatisticsFromFile("OrgFURPLAAKL2", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: one row in table ClientOrgImportHistory", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			var task2 = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task2.RunTask();
			var log = task2.ServiceLogger.ToString();
			AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...".Trim(), log);
			AssertContains("Warning|Message", log);
			AssertContains("CusCode PK and Code mismatch. All CusCode PKs will be stripped and item will be reprocessed.", log);
			AssertContains("Information|1 organization has been imported to database.", log);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 2, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_GROIMPMEL()
		{
			const string clientKey = "VENFREMEL";
			AddClientStatisticsFromFile("OrgGROIMPMEL1", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AddClientStatisticsFromFile("OrgGROIMPMEL2", clientKey, DateTime.Now);
			task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 2, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_NORYESCLD()
		{
			const string clientKey = "AIRGRO_HK";
			AddClientStatisticsFromFile("OrgNORYESCLD", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AddClientStatisticsFromFile("OrgNORYESCLD2", clientKey, DateTime.Now);
			var task2 = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task2.RunTask();
			AssertSuccessLog(task2);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 2, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_HETTICAKL()
		{
			const string clientKey = "GATCARAKL";
			AddClientStatisticsFromFile("OrgHETTICAKL", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var log = task.ServiceLogger.ToString();
			AssertContains("Warning|Message", log);
			AssertContains(" There is no IMDefaultServiceLevel with the following values: [PK:b03accf1-b4c5-4232-a46d-29d6e47a4875] OR [Code:AKL]", log);
			AssertContains("Information|1 organization has been imported to database.", log);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_HALINTSYD()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "MAIINTSYD";
			Factory.Save();
			const string clientKey = "MAIINTMEL";
			AddClientStatisticsFromFile("OrgHALINTSYD", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_MACSOUSEA()
		{
			const string clientKey = "MALALEMEM";
			AddClientStatisticsFromFile("OrgMACSOUSEA", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_SASPREISD()
		{
			var britishEnglishUser = Factory.NewWithValidTestData<GlbStaff>();
			britishEnglishUser.GS_FullName = "Mr Bean";
			britishEnglishUser.GS_WorkingLanguage = Core.SharedConstants.Languages.EnglishBritish;
			Factory.Save();
			const string clientKey = "GLOLOGCPT";
			AddClientStatisticsFromFile("OrgSASPREISDPre", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			using (Env.SetTemporaryUserContext(britishEnglishUser.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
				task.RunTask();
				AssertSuccessLog(task);
				AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
				AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
				AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
				// The duplicated orgCus occur.
				AddClientStatisticsFromFile("OrgSASPREISD", clientKey, DateTime.Now);
				AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
				AssertEquals("Precondition: a added row in table ClientOrgImportHistory", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
				AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
				orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
				task.RunTask();
				var log = task.ServiceLogger.ToString();
				AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...".Trim(), log);
				AssertContains("Warning|Message", log);
				AssertContains("CusCode PK and Code mismatch. All CusCode PKs will be stripped and item will be reprocessed.", log);
				AssertContains("Information|1 organization has been imported to database.", log);
				AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
				AssertEquals("Import History records created", 2, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
				AssertEquals("Orgs created", 0, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
			}
		}

		public void TestProblemFile_HINBURZZZ()
		{
			const string clientKey = "HINBURZZZ";
			AddClientStatisticsFromFile("OrgHINBURZZZ", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_SGMEDPHSINGA()
		{
			const string clientKey = "GWAEHO";
			AddClientStatisticsFromFile("OrgSGMEDPHSINGA", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestProblemFile_DKDGSDIASSEN()
		{
			const string clientKey = "GWAEHO";
			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, Guid.Parse("7eeae3b7-a4ad-4139-9723-9fd50438d8c9")));
			AssertNull(orgHeader);
			AddClientStatisticsFromFile("OrgDKDGSDIASSEN", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var log = task.ServiceLogger.ToString();
			AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...".Trim(), log);
			AssertContains(@"
Could not insert/update the Organization (OrgHeader) as it had an invalid reference to a ClosestPort (ClosestPort). There is no ClosestPort with the following values: [Code:GPMIA].
 Invalid code will be replaced, and the item will be reprocessed.".Trim(), log);
			AssertContains(@"
Could not insert/update the Address (OrgAddress) as it had an invalid reference to a RelatedPortCode (RelatedPortCode). There is no RelatedPortCode with the following values: [Code:GPMIA].
 Invalid code will be replaced, and the item will be reprocessed.".Trim(), log);
			AssertContains(@"
Could not insert/update the Organization Details (OrgMiscServ) as it had an invalid reference to a IMDefaultServiceLevel (IMDefaultServiceLevel). There is no IMDefaultServiceLevel with the following values: [Code:AC2].
 Invalid code will be replaced, and the item will be reprocessed.".Trim(), log);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
			orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, Guid.Parse("7eeae3b7-a4ad-4139-9723-9fd50438d8c9")));
			AssertNotNull(orgHeader);
			AssertEquals("XXXXX", orgHeader.ClosestPort.Code);
		}

		public void TestProblemFile_NATPOWMNL()
		{
			const string clientKey = "ROHLIGSYD";
			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, Guid.Parse("ccfc19df-54f5-408d-8d84-c67d84a06ef2")));
			AssertNull(orgHeader);
			AddClientStatisticsFromFile("OrgNATPOWMNL", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var log = task.ServiceLogger.ToString();
			AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...
Information|1 organization has been imported to database.".Trim(), log);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
			orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, Guid.Parse("ccfc19df-54f5-408d-8d84-c67d84a06ef2")));
			AssertNotNull(orgHeader);
			AssertEquals("XXXXX", orgHeader.ClosestPort.Code);
		}

		public void TestProblemFile_4BELEV()
		{
			const string clientKey = "ROHLIGSYD";
			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, Guid.Parse("9AEC249D-6FA2-49BC-ADB1-626EB1ABDCEF")));
			AssertNull(orgHeader);
			AddClientStatisticsFromFile("Org4BELEV", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var log = task.ServiceLogger.ToString();
			AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...
Information|1 organization has been imported to database.".Trim(), log);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
			orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, Guid.Parse("9AEC249D-6FA2-49BC-ADB1-626EB1ABDCEF")));
			AssertNotNull(orgHeader);
			AssertEquals("XXXXX", orgHeader.ClosestPort.Code);
		}

		public void TestProblemFile_ARJIMPBNE()
		{
			const string clientKey = "ROHLIGSYD";
			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, Guid.Parse("6A1C5FB0-43F5-4EB1-ADE2-E2AABAC21052")));
			AssertNull(orgHeader);
			AddClientStatisticsFromFile("OrgARJIMPBNE", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var log = task.ServiceLogger.ToString();
			AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...
Information|1 organization has been imported to database.".Trim(), log);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
			orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, Guid.Parse("6A1C5FB0-43F5-4EB1-ADE2-E2AABAC21052")));
			AssertNotNull(orgHeader);
			AssertEquals("XXXXX", orgHeader.ClosestPort.Code);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:Do Not Cast Factory Method", Justification = "Baseline")]
		public void TestProblemFile_ITVITSROMA()
		{
			var orgheader = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var orgaddress = (OrgAddress)Factory.New(typeof(OrgAddress), new Guid("11533744-b1a9-412a-8c88-3aab68c35eec"));
			orgaddress.OA_OH = orgheader.PK;
			orgaddress.OA_Address1 = "address1";
			Factory.Save();
			const string clientKey = "GWAEHO";
			AddClientStatisticsFromFile("OrgITVITSROMA", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var log = task.ServiceLogger.ToString();
			AssertContains(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...".Trim(), log);
			AssertContains("Warning|Message", log);
			AssertContains("Could not insert/update the Organization Details (OrgMiscServ) as it had an invalid reference to a IMDefaultServiceLevel (IMDefaultServiceLevel). There is no IMDefaultServiceLevel with the following values", log);
			AssertContains("Could not insert/update the Organization Details (OrgMiscServ) as it had an invalid reference to a EXDefaultServiceLevel (EXDefaultServiceLevel). There is no EXDefaultServiceLevel with the following values", log);
			AssertContains("Invalid code will be replaced, and the item will be reprocessed.", log);
			AssertContains(@"System.Exception: Record: Organization failed to Import:
Database parent does not match entity parent.", log);
			AssertContains(@"Information|1 organization has been imported to database.", log);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		[SnailTest]
		[TestDate(2021, 9, 1)]
		public void TestSystemOrg()
		{
			const string clientKey = "HABSPESGN";
			AddClientStatisticsFromFile("OrgUNMATCHED", clientKey, DateTime.Now);
			AssertEquals("Precondition: row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertEquals(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...
Information|Skipping system organization.
Information|0 organizations have been imported to database.
".TrimStart(), task.ServiceLogger.ToString());
			AssertEquals("row in ClientStatisticsXML should be deleted", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("import history record should not be created", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("no org should be created", 0, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestMerge()
		{
			const string clientKey1 = "FIRST_ID";
			const string clientKey2 = "SECON_ID";
			AddClientStatisticsFromFile("OrgACOART1", clientKey1, DateTime.Now.AddMinutes(-3));
			AddClientStatisticsFromFile("OrgACOARTLUD1", clientKey2, DateTime.Now.AddMinutes(-2));
			AddClientStatisticsFromFile("OrgACOARTLUD2", clientKey2, DateTime.Now.AddMinutes(-1));
			AssertEquals("Precondition: 3 rows in table ClientStatisticsXML", 3, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Precondition: no rows in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no rows in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertEquals(@"
Information|Retrieving messages...
Information|3 messages retrieved.
Information|Importing messages...
Information|3 organizations have been imported to database.
".TrimStart(), task.ServiceLogger.ToString());
			AssertEquals("rows in ClientStatisticsXML are deleted", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("3 import history records should be created", 3, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs should be merged so only one org created", orgCount + 1, GetTableRowCount(OrgHeader.Schema.TableName));
			AssertEquals("3 consols should be created", 3, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			var org = Factory.Load<OrgHeader>(new ZGuid("a86ba5e6-6b95-4ee6-bfd4-2847681d1822"));
			AssertNotNull("PK from XML was used", org);
			var ehubClientIDMappings = org.Factory.Load<OrgPatternMatchOverride>(new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, org.PK)).Where(_ => _.OO_Relationship == EDIConstants.OrgPatternMatchOverrideRelationships.EHubClientID);
			AssertNotNull("First eHub Client ID is stored", ehubClientIDMappings.FirstOrDefault(_ => _.OO_ForeignCode == clientKey1));
			AssertNotNull("Second eHub Client ID is stored", ehubClientIDMappings.FirstOrDefault(_ => _.OO_ForeignCode == clientKey2));
			var histories = Factory.Load<ClientOrgImportHistory>(new ZQuery(ClientOrgImportHistorySchema.O2_TargetOrgPK, org.PK));
			AssertEquals("There should be 3 import histories", 3, histories.Length);
			var history1 = histories.First(h => h.O2_ClientKey == clientKey1);
			CombineAssertions("First history parameters", () =>
			{
				AssertEquals("Org Code", "ACOARTLUD", history1.O2_OrgCode);
				AssertEquals("Action Type ", "CreateNew", history1.O2_ActionType);
				AssertEquals("Match Count", 0, history1.O2_MatchCount);
			});
			var history2 = histories.First(h => h.O2_ClientKey == clientKey2);
			CombineAssertions("Second history parameters", () =>
			{
				AssertEquals("Org Code", "ACOARTLUD", history2.O2_OrgCode);
				AssertEquals("Action Type ", "Merge (Native)", history2.O2_ActionType);
				AssertEquals("Match Count", 1, history2.O2_MatchCount);
			});
			var history3 = histories.Last(h => h.O2_ClientKey == clientKey2);
			CombineAssertions("Third history parameters", () =>
			{
				AssertEquals("Org Code", "ACOARTLUD", history3.O2_OrgCode);
				AssertEquals("Action Type ", "Merge (Native)", history3.O2_ActionType);
				AssertEquals("Match Count", 1, history3.O2_MatchCount);
			});
		}

		public void TestNoMergeForNonEqualPKs()
		{
			AddClientStatisticsFromFile("OrgACOART1", "FIRST_ID", DateTime.Now.AddMinutes(-2));
			AddClientStatisticsFromFile("OrgACOART2", "SECOND_ID", DateTime.Now.AddMinutes(-1));
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertEquals(@"
Information|Retrieving messages...
Information|2 messages retrieved.
Information|Importing messages...
Information|2 organizations have been imported to database.
".TrimStart(), task.ServiceLogger.ToString());
			AssertNotNull("First org was imported", Factory.Load<OrgHeader>(new ZGuid("a86ba5e6-6b95-4ee6-bfd4-2847681d1822")));
			AssertNotNull("Second org was imported", Factory.Load<OrgHeader>(new ZGuid("fb1f8dd0-f59a-4c77-bfc1-c4b5f0d9a352")));
		}

		public void TestExecute_PW_PasswordExceedMaxLength()
		{
			const string clientKey = "UNTLAXLAX";
			AddClientStatisticsFromFile("OrgSPILIGLAX", clientKey, DateTime.Now);
			AssertEquals("Precondition: 1 row in table ClientStatisticsXML", 1, GetTableRowCount("ClientStatisticsXML"));
			var initialRow = SelectTopRow("ClientStatisticsXML");
			AssertEquals("Precondition: no row in table ClientStatisticsXMLArchive", 0, GetTableRowCount("ClientStatisticsXMLArchive"));
			AssertEquals("Precondition: no row in table ClientOrgImportHistory", 0, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Precondition: no row in table ClientOrgConsol", 0, GetTableRowCount(ClientOrgConsol.Schema.TableName));
			int orgCount = GetTableRowCount(OrgHeader.Schema.TableName);
			var task = new ScavengingImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSuccessLog(task);
			AssertEquals("ClientStatisticsXML records", 0, GetTableRowCount("ClientStatisticsXML"));
			AssertEquals("Import History records created", 1, GetTableRowCount(ClientOrgImportHistory.Schema.TableName));
			AssertEquals("Orgs created", 1, GetTableRowCount(OrgHeader.Schema.TableName) - orgCount);
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1minute", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		static void AssertSuccessLog(ScavengingImportServiceTask task)
		{
			AssertEquals(@"
Information|Retrieving messages...
Information|1 messages retrieved.
Information|Importing messages...
Information|1 organization has been imported to database.
".TrimStart(), task.ServiceLogger.ToString());
		}

		internal static int GetTableRowCount(string tableName)
		{
			using (var command = Db.Connection.Command(string.Format("select COUNT(*) from {0}", tableName)))
			{
				var rowCount = command.ExecuteScalar();
				return Convert.ToInt32(rowCount);
			}
		}

		static object[] SelectTopRow(string tableName)
		{
			using (var command = Db.Connection.Command(string.Format("select TOP 1 * from {0}", tableName)))
			using (var reader = command.ExecuteReader())
			{
				if (reader.Read())
				{
					var row = new object[reader.FieldCount];
					reader.GetValues(row);
					return row;
				}

				return null;
			}
		}

		#region Implementation
		internal static void AddClientStatisticsFromXML(string xml, string clientKey, DateTime insertUTC)
		{
			AddClientStatisticsFromStream(new MemoryStream(Encoding.UTF8.GetBytes(xml)), clientKey, insertUTC);
		}

		internal static void AddClientStatisticsFromFile(string fileName, string clientKey, DateTime insertUTC)
		{
			using (var ms = new MemoryStream(GetTestFileBytes(fileName)))
			{
				AddClientStatisticsFromStream(ms, clientKey, insertUTC);
			}
		}

		static byte[] GetTestFileBytes(string fileName)
		{
			var resourceRetriever = new EmbeddedResourceRetriever(typeof(ScavengingImportServiceTaskTest).Assembly);
			return resourceRetriever.GetBytes("ZClientEDI.Test." + fileName + ".xml");
		}

		static string GetTestFileText(string fileName)
		{
			var resourceRetriever = new EmbeddedResourceRetriever(typeof(ScavengingImportServiceTaskTest).Assembly);
			return resourceRetriever.GetString("ZClientEDI.Test." + fileName + ".xml");
		}

		internal static void AddClientStatisticsFromStream(Stream stream, string clientKey, DateTime insertUTC)
		{
			string sql = @"
INSERT INTO dbo.ClientStatisticsXML ([IM_PK], [IM_ClientID], [IM_MessageTrackingID], [IM_MessageType], [IM_ApplicationCode], [IM_InsertUTC], [IM_Content])
VALUES (newid(), '" + clientKey + "', newid(), 'http://www.cargowise.com/Schemas/Native', 'SCV', @insertUTC, '" + stream.CompressAndEncode().ReadToEnd() + "')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("insertUTC", System.Data.SqlDbType.DateTime, insertUTC);
				command.ExecuteNonQuery();
			}
		}

		#endregion
	}
}
