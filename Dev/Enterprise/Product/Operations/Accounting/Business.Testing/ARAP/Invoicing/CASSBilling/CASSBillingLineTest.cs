namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.ComponentModel;
	using CargoWise.Data;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using ConsolCosting;
	using Enterprise.Accounting.Business.JobInvoicing;
	using Enterprise.Accounting.Registry.Business;
	using Enterprise.Core;
	using Enterprise.Environment;
	using Enterprise.Freight.Business;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.Integration.Accounting;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Business.Testing;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	[TestedType(typeof(CASSBillingLine))]
	public class CASSBillingLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDelete()
		{
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
			SetupAssociatedBizos();
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();

			var dummyToPopulateInternalStructures = TestCASSBillingLine.ConsolID;

			AssertNotNull("AggregatedCostLine", TestCASSBillingLine.AggregatedCostLine);
			AssertNotEquals("SystemChargeDataByJobAndChargeCode is populated.", 0, TestCASSBillingLine.SystemChargeDataByJobAndChargeCode.Count);
			AssertNotEquals("SystemCostValueByChargeCodes is populated.", 0, TestCASSBillingLine.SystemCostValueByChargeCodes.Count);

			var costLine = TestCASSBillingLine.AggregatedCostLine;

			TestCASSBillingLine.Delete();

			AssertNull("AggregatedCostLine", TestCASSBillingLine.AggregatedCostLine);
			Assert("cost line is deleted", costLine.IsDeleted);
			AssertEquals("SystemChargeDataByJobAndChargeCode is empty.", 0, TestCASSBillingLine.SystemChargeDataByJobAndChargeCode.Count);
			AssertEquals("SystemCostValueByChargeCodes is empty.", 0, TestCASSBillingLine.SystemCostValueByChargeCodes.Count);
		}

		public void TestGetConsolRetrievesLatestConsol()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			TestObjectCreator.GLHeader1.AG_AccountType = Constants.AccountType.BalanceSheetAccount;
			TestObjectCreator.GLHeader1.AG_Description = "Test Account";
			AccountingConfigurationRegistry.Instance.CASSGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			ZQuery fRTFilter = new ZQuery(AccChargeCodeSchema.AC_Code, "FRT");
			fRTFilter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			var frt = Factory.LoadTop1<AccChargeCode>(fRTFilter);

			CASSBillingLine cassLine = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSBillingLine(cassLine);

			string origin = RefUNLOCO.LoadFromIATA(Factory, cassLine.LoadPortIATA).RL_Code;
			string destination = RefUNLOCO.LoadFromIATA(Factory, cassLine.DischargePortIATA).RL_Code;

			var consol = TestObjectCreator.CreateConsol(origin, destination, "C0001");
			consol.JK_MasterBillNum = cassLine.MAWBNumber;
			consol.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			consol.JK_AgentType = Constants.AgentType.CoLoad;

			var shipment1 = TestObjectCreator.CreateShipment("SHIP1", origin, destination, consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			TestObjectCreator.CreateCharge(job1, frt, "FRT", cassLine.CASSCostCurrency, 100M, null, cassLine.CASSCostCurrency, 100M, null);
			Factory.Save();

			var consol2 = TestObjectCreator.CreateConsol(origin, destination, "C0002");
			consol2.JK_MasterBillNum = cassLine.MAWBNumber; //both consol must have same master bill number
			consol2.JK_AgentType = Constants.AgentType.CoLoad;

			var shipment2 = TestObjectCreator.CreateShipment("SHIP2", origin, destination, consol2);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			TestObjectCreator.CreateCharge(job2, frt, "FRT", cassLine.CASSCostCurrency, 100M, null, cassLine.CASSCostCurrency, 100M, null);
			Factory.Save();

			cassLine.ForceRecalculateData();
			cassLine.InitializeConsolBranchSystemCostValues();
			var newFactory = new BusinessObjectFactory();
			var linkedConsol = newFactory.Load<ForwardingConsol>(cassLine.ConsolPK);
			AssertEquals("Line should be linked to the newer consol with same MAWB", "C0002", linkedConsol.JK_UniqueConsignRef);
		}

		public void TestInitialiseConsolBranchSystemCostValuesWhenNoCASSChargesDefined()
		{
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
			TestCASSBillingLine.ForceRecalculateData();
			try
			{
				string consolID = TestCASSBillingLine.ConsolID;
				Assert("No exception all good", true);
			}
			catch
			{
				Assert("Should not get an exception", false);
			}
		}

#region Credtior related Tests

		public void TestCreditorBasic()
		{
			AssertEquals("No Creditor when there is no MAWB", null, TestCASSBillingLine.Creditor?.PK);

			CreateOrUpdateAirlineOrg(null, "AAA");
			CreateMAWB("USLAX", "ZZZ");
			AddCostLineToTestCassBillingLine("ZZZ");
			Factory.Save();

			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals("No Creditor when no organisations with matching airline prefix", null, TestCASSBillingLine.Creditor?.PK);

			CreateOrUpdateAirlineOrg(TestObjectCreator.AALSHI, "ZZZ", false);
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals("No Creditor when no credit organisations, no agencies", null, TestCASSBillingLine.Creditor?.PK);

			TestObjectCreator.AALSHI.OH_IsCreditor = true;
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals("Org is creditor when it is a creditor, no agencies", TestObjectCreator.AALSHI.PK, TestCASSBillingLine.Creditor?.PK);
		}

		public void TestCreditorWithSingleOrgIncorrectUNLOCO()
		{
			//Existing
			CreateOrUpdateAirlineOrg(null, "AAA");
			CreateMAWB("USLAX", "ZZZ");
			AddCostLineToTestCassBillingLine("ZZZ");
			CreateOrUpdateAirlineOrg(TestObjectCreator.AALSHI, "ZZZ", false);

			var agencyOrg1 = CreateAgencyOrg("NZAKL");
			var agentPort1 = TestObjectCreator.AALSHI.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort1.O5_PortOrCountry = "ITMIL";
			agentPort1.O5_OA_AgentOfficeAddress = agencyOrg1.MainAddress.PK;

			var agencyOrg2 = CreateAgencyOrg("NZAKL");
			var agentPort2 = TestObjectCreator.AALSHI.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort2.O5_PortOrCountry = "ITMIL";
			agentPort2.O5_OA_AgentOfficeAddress = agencyOrg2.MainAddress.PK;
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();

			AssertEquals("No creditor when no orgs are creditors, no matching agent ports", null, TestCASSBillingLine.Creditor?.PK);

			agentPort2.O5_PortOrCountry = "USNA8";
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals("No creditor although there is an Agent whose Country matches with MAWB Country. But it cannot be picked as Port is mismatched", null, TestCASSBillingLine.Creditor?.PK);

			agentPort1.O5_PortOrCountry = "USLAX";
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals("Matching Agency as Creditor when it matches the MAWB Port, no orgs are creditors", agencyOrg1.PK, TestCASSBillingLine.Creditor?.PK);

			agentPort1.O5_PortOrCountry = "ITMIL";
			agentPort2.O5_PortOrCountry = "ITMIL";
			TestObjectCreator.AALSHI.OH_IsCreditor = true;
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals("Org is creditor when it is a creditor, no matching agencies", TestObjectCreator.AALSHI.PK, TestCASSBillingLine.Creditor?.PK);
		}

		public void TestCreditorWithMultipleOrgWhoseHomePortCountryAreNotSameAsCurrentLoginCompany()
		{
			var airlineOrg1 = CreateOrUpdateAirlineOrg(null, "AAA");
			CreateMAWB("USLAX", "ZZZ");
			AddCostLineToTestCassBillingLine("ZZZ");
			CreateOrUpdateAirlineOrg(TestObjectCreator.AALSHI, "ZZZ", false);

			var airlineOrg2 = CreateOrUpdateAirlineOrg(TestObjectCreator.AALSHI, "ZZZ", false);
			var agencyOrg2 = CreateAgencyOrg("NZAKL");
			var agentPort2 = TestObjectCreator.AALSHI.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort2.O5_PortOrCountry = "ITMIL";
			agentPort2.O5_OA_AgentOfficeAddress = agencyOrg2.MainAddress.PK;

			var agencyOrg4 = CreateAgencyOrg("NZAKL");
			var agentPort4 = TestObjectCreator.AALSHI.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort4.O5_PortOrCountry = "ITMIL";
			agentPort4.O5_OA_AgentOfficeAddress = agencyOrg4.MainAddress.PK;

			var airlineOrg3 = CreateOrUpdateAirlineOrg(TestObjectCreator.ABIGAS, "ZZZ", false, "NZAKL");
			var agencyOrg3 = CreateAgencyOrg("NZAKL");
			var agentPort3 = airlineOrg3.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort3.O5_OA_AgentOfficeAddress = agencyOrg3.MainAddress.PK;
			agentPort3.O5_PortOrCountry = "NZAKL";

			var agencyOrg5 = CreateAgencyOrg("NZAKL");
			var agentPort5 = airlineOrg3.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort5.O5_OA_AgentOfficeAddress = agencyOrg5.MainAddress.PK;
			agentPort5.O5_PortOrCountry = "NZAKL";

			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Iceland);

			Assert("Precondition: Current country must exist", !GlbCompany.CurrentCompany.GC_RN_NKCountryCode.IsEmpty);
			AssertNotEquals("Precondition: UNLOCO must not be matched", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, airlineOrg1.UNLOCO?.RL_RN_NKCountryCode);
			AssertNotEquals("Precondition: UNLOCO must not be matched", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, airlineOrg2.UNLOCO?.RL_RN_NKCountryCode);
			AssertNotEquals("Precondition: UNLOCO must not be matched", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, airlineOrg3.UNLOCO?.RL_RN_NKCountryCode);

			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals("No Creditor when no elegible agencies in orgs, no orgs UNLOCO matching current country", null, TestCASSBillingLine.Creditor?.PK);

			agentPort4.O5_PortOrCountry = "USNA8";
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals("No creditor although there is an Agent whose Country matches with MAWB Country. But it cannot be picked as Port is mismatched", null, TestCASSBillingLine.Creditor?.PK);

			agentPort2.O5_PortOrCountry = "USLAX";
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals("Agency as Creditor when it matches the MAWB Port, no orgs are creditors, no orgs UNLOCO matching current country", agencyOrg2.PK, TestCASSBillingLine.Creditor?.PK);

			airlineOrg2.OH_IsCreditor = false;
			agentPort2.O5_PortOrCountry = "USLAX";
			airlineOrg3.OH_IsCreditor = true;
			agentPort3.O5_PortOrCountry = "ITMIL";
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(@"agencyOrg2 is picked as a creditor
							WHEN
							airlineOrg2 is NOT a creditor,
							AND airlineOrg2 does not have the same home port country as the current login company
							AND airlineOrg2 has an agency org which has the same O5_PortOrCountry as MAWB's homeport or country", agencyOrg2.PK, TestCASSBillingLine.Creditor?.PK);

			agentPort2.O5_PortOrCountry = "ITMIL";
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(@"airlineOrg3 is a creditor
							WHEN
							airlineOrg3 is a creditor,
							ALTHOUGH airlineOrg3 does not have the same home port country as the current login company
							BUT airlineOrg3 does not have any agency org which has same O5_PortOrCountry as MAWB's homeport country", airlineOrg3.PK, TestCASSBillingLine.Creditor?.PK);

			agentPort5.O5_PortOrCountry = "US";
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(@"agencyOrg5 is a creditor
							WHEN
							airlineOrg3 is a creditor,
							ALTHOUGH airlineOrg3 does not have the same home port country as the current login company
							BUT airlineOrg3 has an agency org which has same O5_PortOrCountry as MAWB's homeport country", agencyOrg5.PK, TestCASSBillingLine.Creditor?.PK);

			agentPort3.O5_PortOrCountry = "USLAX";
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(@"agencyOrg3 is a creditor
							WHEN
							airlineOrg3 is a creditor,
							ALTHOUGH airlineOrg3 does not have the same home port country as the current login company
							BUT airlineOrg3 has an agency org which has same O5_PortOrCountry as MAWB's homeport", agencyOrg3.PK, TestCASSBillingLine.Creditor?.PK);
		}

		public void TestCreditorWithMultipleOrgWhoseHomePortCountryAreSameAsCurrentLoginCompany()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.UnitedStates);

			CreateOrUpdateAirlineOrg(null, "AAA");
			CreateMAWB("USLAX", "ZZZ");
			AddCostLineToTestCassBillingLine("ZZZ");

			var airlineOrg2 = CreateOrUpdateAirlineOrg(TestObjectCreator.AALSHI, "ZZZ", false, "USHNL");
			var agencyOrg2 = CreateAgencyOrg("ITMIL");
			var agentPort2 = TestObjectCreator.AALSHI.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort2.O5_OA_AgentOfficeAddress = agencyOrg2.MainAddress.PK;

			var agencyOrg4 = CreateAgencyOrg("ITMIL");
			var agentPort4 = TestObjectCreator.AALSHI.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort4.O5_OA_AgentOfficeAddress = agencyOrg4.MainAddress.PK;

			var airlineOrg3 = CreateOrUpdateAirlineOrg(TestObjectCreator.ABIGAS, "ZZZ", false);
			var agencyOrg3 = CreateAgencyOrg("USLAX");
			var agentPort3 = airlineOrg3.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort3.O5_OA_AgentOfficeAddress = agencyOrg3.MainAddress.PK;

			var agencyOrg5 = CreateAgencyOrg("USNA8");
			var agentPort5 = airlineOrg3.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort5.O5_OA_AgentOfficeAddress = agencyOrg5.MainAddress.PK;

			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(@"No org is picked as creditor WHEN
						all orgs that match unloco with current login company country are NOT creditors
						AND there is NO Agent as well", null, TestCASSBillingLine.Creditor?.PK);

			agentPort4.O5_PortOrCountry = "USNA8";
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(@"No org is picked as creditor WHEN
						None of the org is a creditor
						AND there is NO Agent as well whose O5_PortOrCountry matches with MAWB's home port or country.", null, TestCASSBillingLine.Creditor?.PK);

			agentPort4.O5_PortOrCountry = "US";
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(@"Agency 'agencyOrg4' is picked as creditor WHEN
						airlineOrg2 is NOT a creditor
						AND airlineOrg2's home port country is NOT same as the current login company country
						BUT there is an agency org that has same O5_PortOrCountry as the MAWB's home port country.", agencyOrg4.PK, TestCASSBillingLine.Creditor?.PK);

			agentPort2.O5_PortOrCountry = "USLAX";
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(@"Agency 'agencyOrg2' org is picked as the creditor
							WHEN
							airlineOrg2 is NOT a creditor
							AND airlineOrg2's home port country is NOT same as the current login company country
							BUT there is an agency org that has O5_PortOrCountry as the MAWB's home port.", agencyOrg2.PK, TestCASSBillingLine.Creditor?.PK);

			airlineOrg3.OH_RL_NKClosestPort = "USHNL";
			airlineOrg3.OH_IsCreditor = true;
			agentPort3.O5_PortOrCountry = "ITMIL";
			agentPort5.O5_PortOrCountry = "ITMIL";

			airlineOrg2.OH_RL_NKClosestPort = "ITMIL";
			airlineOrg2.OH_IsCreditor = true;
			agentPort2.O5_PortOrCountry = "ITMIL";
			agentPort4.O5_PortOrCountry = "ITMIL";

			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(@"airlineOrg3 is picked as CASS billing line creditor
							WHEN
							airlineOrg3 is a creditor
							AND airlineOrg3's home port country is SAME as as the current login company country
							BUT there is NO agency org that has same O5_PortOrCountry as the MAWB's home port or country", airlineOrg3.PK, TestCASSBillingLine.Creditor?.PK);

			agentPort5.O5_PortOrCountry = "US";
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(@"Agency 'agencyOrg5' is picked as creditor
							WHEN
							airlineOrg3 is Creditor
							AND airlineOrg3's home port country is SAME as as the current login company country
							AND there is an agent whose O5_PortOrCountry matches MAWB's home port Country
							BUT there is NO Agencies whose O5_PortOrCountry match MAWB Port.", agencyOrg5.PK, TestCASSBillingLine.Creditor?.PK);

			agentPort3.O5_PortOrCountry = "USLAX";
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(@"Agency 'agencyOrg3' is picked as creditor
							WHEN
							airlineOrg3 is a Creditor
							AND airlineOrg3's home port country is SAME as as the current login company country
							AND there is an agent whose O5_PortOrCountry matches MAWB's home port.", agencyOrg3.PK, TestCASSBillingLine.Creditor?.PK);
		}

		public void TestCreditorForMultipleMAWBStocksWithSameNumber()
		{
			var awbSerial = "897654";
			var airLinePrefix = "ZZZ";

			var objectCreator = new TestObjectCreator(Factory);
			var consol = objectCreator.CreateConsol();
			_ = objectCreator.CreateShipment("S0001", consol);
			consol.JK_MasterBillNum = airLinePrefix + awbSerial;

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.UnitedStates);

			var airlineOrg2 = CreateOrUpdateAirlineOrg(TestObjectCreator.AALSHI, "ZZZ", true, "US");
			var agencyOrg2 = CreateAgencyOrg("USCHI");
			var agentPort2 = TestObjectCreator.AALSHI.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort2.O5_OA_AgentOfficeAddress = agencyOrg2.MainAddress.PK;
			agentPort2.O5_PortOrCountry = "AU";

			var agencyOrg4 = CreateAgencyOrg("USLAX");
			var agentPort4 = TestObjectCreator.AALSHI.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort4.O5_OA_AgentOfficeAddress = agencyOrg4.MainAddress.PK;
			agentPort4.O5_PortOrCountry = "USLAX";
			Factory.Save();

			AddCostLineToTestCassBillingLine(airLinePrefix, awbSerialNumber: awbSerial);

			var sBranch = Factory.NewWithValidTestData<GlbBranch>();
			sBranch.GB_RL_NKHomePort = "AUSYD";
			Factory.Save();

			//Creating an old MAWB stock with the same MAWB number. Used direct SQL to bypass database level restriction imposed by a trigger.
			//The restriction is that a duplicate MAWB stock can be created only if the MAWB number was used at least 1 year ago.
			//AUSYD is the homeport of the old MAWB stock.
			TestConnection.ExecuteNonQuery(FormattableString.Invariant(@$"INSERT INTO dbo.JobMawb (JM_PK, JM_Airline3DigitPrefix, JM_MAWB, JM_GB, JM_GC_Company, JM_SystemCreateTimeUtc) VALUES (NEWID(), '{airLinePrefix}', '{awbSerial}', '{sBranch.PK}','{sBranch.Company.PK}', '{ZDate.Today.AddYears(-2).ToString("dd MMM yyyy")}');"));
			//Need to clear cache to make sure that Factory goes to DB to bring the directly inserted MAWB stock info.
			Factory.ClearQueryCache(); 

			TestCASSBillingLine.ForceRecalculateData();

			AssertEquals(@"agencyOrg2 is picked as CASS billing line creditor
							WHEN
							airlineOrg2 is a creditor
							AND airlineOrg2's home port country (US) is SAME as as the current login company country (US)
							BUT there is an agency org that has same O5_PortOrCountry (AU) as the MAWB's home port country (AU)", agencyOrg2.PK, TestCASSBillingLine.Creditor?.PK);

			//A new MAWB stock with homeport USLAX. The homeport of this new MAWB stock should be used for determining the Creditor
			_ = CreateMAWB("USLAX", airLinePrefix);
			Factory.Save();

			TestCASSBillingLine.ForceRecalculateData();

			AssertEquals(@"agencyOrg4 is picked as CASS billing line creditor
							WHEN
							airlineOrg2 is a creditor
							AND airlineOrg2's home port country (US) is SAME as as the current login company country (US)
							BUT there is an agency org that has same O5_PortOrCountry (USLAX) as the MAWB's home port (USLAX)", agencyOrg4.PK, TestCASSBillingLine.Creditor?.PK);
		}

		OrgHeader CreateOrUpdateAirlineOrg(OrgHeader org, ZString airlinePrefix, bool isCreditor = false, string closestPort = "")
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var airline = testObjectCreator.CreateAirLine(airlinePrefix);

			var airLineOrg = org ?? Factory.NewWithValidTestData<OrgHeader>();
			airLineOrg.MiscServ.OM_RM_Airline = airline.PK;
			airLineOrg.OH_IsCreditor = isCreditor;
			airLineOrg.OH_RL_NKClosestPort = closestPort;
			return airLineOrg;
		}

		JobMawb CreateMAWB(ZString homePort, ZString airlinePrefix)
		{
			JobMawb mawb = Factory.New<JobMawb>();
			mawb.JM_MAWB = TestCASSBillingLine.AWBNumber;
			mawb.JM_Airline3DigitPrefix = airlinePrefix;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = homePort;
			mawb.JM_GB = branch.PK;
			return mawb;
		}

		OrgHeader CreateAgencyOrg(ZString closestPort)
		{
			var agencyorg = Factory.New<OrgHeader>();
			agencyorg.OH_IsCreditor = true;
			agencyorg.OH_FullName = "Test Org 1 2 3";
			agencyorg.OH_RL_NKClosestPort = closestPort;
			agencyorg.MainAddress.OA_Address1 = "26 Myrtle Street";
			agencyorg.MainAddress.OA_City = "Prospect";
			agencyorg.OH_Code = "TestOrg123";
			return agencyorg;
		}

		void AddCostLineToTestCassBillingLine(string airLlnePrefix, string currency = "AUD", string awbSerialNumber = "")
		{
			var costLine1 = new CASSCostExportLine(Factory, CASSCostLineType.Default);
			costLine1.AirlinePrefix = airLlnePrefix;
			costLine1.AWBSerialNumber = awbSerialNumber;
			TestCASSBillingLine.AddCostLine(costLine1, currency);
		}

#endregion

		public void TestAirline()
		{
			RefAirline expectedAirLine = Factory.LoadTop1<RefAirline>(
				new ZQuery(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, SQLComparisonOperator.NotEqual, ZString.Empty));

			string airlinePrefix = expectedAirLine.RM_EagleAddedAirlinePrefixOrAccountingCode;
			TestObjectCreator.AALSHI.MiscServ.OM_RM_Airline = expectedAirLine.PK;

			var costLine = new CASSCostExportLine(Factory, CASSCostLineType.Default);
			costLine.AirlinePrefix = airlinePrefix;
			TestCASSBillingLine.AddCostLine(costLine, "AUD");
			AssertNull(TestCASSBillingLine.Airline);

			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals("Airline must be equal Airline associated with OrgHeader", expectedAirLine.PK, TestCASSBillingLine.Airline.PK);
			AssertEquals(expectedAirLine.RM_TwoCharacterCode, TestCASSBillingLine.Airline2LetterCode);

			costLine.AirlinePrefix = "XXX";
			TestCASSBillingLine.AddCostLine(costLine, "AUD");
			AssertNull(TestCASSBillingLine.Airline);
		}

		[TestDate(2013, 08, 14, 12, 11, 00)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestConsol()
		{
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
			SetupAssociatedBizos();
			Factory.Save();
			TestConnection.ExecuteNonQuery("UPDATE dbo.JobConsol SET JK_SystemLastEditUser = 'Q', JK_SystemLastEditTimeUtc = '20130814 12:18:00 PM' WHERE JK_PK = " + Consol.PK.ToSqlGuid());
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(Consol.PK, TestCASSBillingLine.ConsolPK);
			AssertEquals(Consol.JK_UniqueConsignRef, TestCASSBillingLine.ConsolID);
			AssertEquals(Env.CurrentUser.Initials, TestCASSBillingLine.ConsolCreateUser);
			AssertEquals(new ZDateTime(2013, 08, 14, 12, 11, 00), TestCASSBillingLine.ConsolCreateTime);
			AssertEquals("Q", TestCASSBillingLine.ConsolLastEditUser);
			AssertEquals(new ZDateTime(2013, 08, 14, 12, 18, 00), TestCASSBillingLine.ConsolLastEditTime);

			fTestCASSBillingLine = null;
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
			Job1.JH_GC = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentCompany.PK)).PK;
			Job2.JH_GC = Job1.JH_GC;
			AssertEquals(Consol.PK, TestCASSBillingLine.ConsolPK);

			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(ZGuid.Empty, TestCASSBillingLine.ConsolPK);
		}

		public void TestPreSaveValidation_ClaimStatusWarning()
		{
			AccQueryClaim claim = SetupAssociatedClaim(Factory, 400M);
			Factory.Save();

			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.OverBilled);
			SetupCASSClaimRequiredBillingLineAndBizObjsFor(TestCASSBillingLine, pWCAmount: 200M, adjustedPWCAmount: 600M);
			TestCASSBillingLine.ForceRecalculateData();

			TestCASSBillingLine.RunPreSaveValidation();
			AssertHasWarning(TestCASSBillingLine.StatusForBindingInfo, "Claim already exists for this MAWB and will be closed as accepted, unless overridden using the option below.");
		}

		public void TestPreSaveValidation_ClosedClaimWithSameMAWB()
		{
			SetupAssociatedClaim(Factory, 264.50M, "CCC");
			Factory.Save();
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine, false);
			SetupAssociatedBizos();

			TestCASSBillingLine.RunPreSaveValidation();
			var hasMAWBError = TestCASSBillingLine.MAWBNumberInfo.HasErrors();
			AssertEquals("Should NOT have creditor error", false, hasMAWBError);
		}

		public void TestPreSaveValidation_NotClosedClaimWithSameMAWB()
		{
			SetupAssociatedClaim(Factory, 264.50M);
			Factory.Save();

			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine, false);
			SetupAssociatedBizos();

			TestCASSBillingLine.RunPreSaveValidation();
			AssertHasError(TestCASSBillingLine.MAWBNumberInfo, "An open claim with same MAWB exists. Multiple open claims with same MAWB is not allowed.");
		}

		public void TestPreSaveValidation_ClaimCreditor_NotIdentified()
		{
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
			Factory.Save();
			SetupAssociatedBizos();
			(TestCASSBillingLine.AggregatedCostLine as CASSCostExportLine).WeightChargePP = 600M;

			TestCASSBillingLine.RunPreSaveValidation();
			AssertHasError(TestCASSBillingLine.CreditorCodeInfo, "A Creditor for this MAWB could not be identified. Please check that there is a Payables organization in your company that is also flagged as the Carrier for the Airline with code '172'.");
		}

		public void TestPreSaveValidation_ClaimCreditorContact_NotIdentified()
		{
			TestObjectCreator.SetupCreditorAirline(TestObjectCreator.CreateAirLine("172"), TestObjectCreator.AALSHI);
			Factory.Save();

			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine, false);
			SetupAssociatedBizos();
			(TestCASSBillingLine.AggregatedCostLine as CASSCostExportLine).WeightChargePP = 100M;

			TestCASSBillingLine.RunPreSaveValidation();
			AssertHasError(TestCASSBillingLine.CreditorCodeInfo, $"Cannot create claim for line(s) with MAWB: {TestCASSBillingLine.MAWBNumber} as there is no active contact defined for creditor: {TestCASSBillingLine.CreditorCode}");
		}

		public void TestValidateCreditorCode_WithoutDefaultContactInOrgainsation()
		{
			TestObjectCreator.SetupCreditorAirline(TestObjectCreator.CreateAirLine("172"), TestObjectCreator.AALSHI);
			Factory.Save();

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine, false);

			var hasCreditorError = TestCASSBillingLine.CreditorCodeInfo.HasErrors();
			AssertEquals("Should have creditor error", true, hasCreditorError);
			var criticalError = TestCASSBillingLine.CreditorCodeInfo.Notifications.Any(x => x.Message == $"Cannot create Claims for {TestCASSBillingLine.Creditor.OH_Code} as there is no contact defined");
		}

		public void TestValidateCASSClaimStatus_ClaimExist_Underbilled()
		{
			AccQueryClaim claim = SetupAssociatedClaim(Factory, 100M);
			Factory.Save();
			SetupCASSClaimRequiredBillingLineAndBizObjsFor(TestCASSBillingLine, pWCAmount: 100M, adjustedPWCAmount: 150M);
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(CASSBillingLine.StatusType.Underbilled, TestCASSBillingLine.Status);
			AssertEquals("Under-billed", TestCASSBillingLine.StatusForBinding);
			AssertHasWarning(TestCASSBillingLine.StatusForBindingInfo, "Claim already exists for this MAWB and will be linked to this new AP invoice.");
		}

		public void TestValidateCASSClaimStatus_NotInEnterprise()
		{
			AccQueryClaim claim = SetupAssociatedClaim(Factory, 400M);
			Factory.Save();

			var costLine2 = new CASSCostExportLine(Factory, CASSCostLineType.Default);
			costLine2.AWBSerialNumber = "12312312";
			TestCASSBillingLine.AddCostLine(costLine2, TestCASSBillingLine.CASSCostCurrencyCode);
			AssertEquals(CASSBillingLine.StatusType.NotInSystem, TestCASSBillingLine.Status);
			AssertEquals("Not In System", TestCASSBillingLine.StatusForBinding);
		}

		public void TestValidateCASSClaimStatus_ClaimAcceptedToBeClosed()
		{
			AccQueryClaim claim = SetupAssociatedClaim(Factory, 100M);
			Factory.Save();

			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.OverBilled);
			SetupCASSClaimRequiredBillingLineAndBizObjsFor(TestCASSBillingLine, pWCAmount: 200M, adjustedPWCAmount: 300M);
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(CASSBillingLine.StatusType.Underbilled, TestCASSBillingLine.Status);
			AssertEquals("Under-billed", TestCASSBillingLine.StatusForBinding);
			AssertHasWarning(TestCASSBillingLine.StatusForBindingInfo, "Claim already exists for this MAWB and will be closed as accepted, unless overridden using the option below.");
		}

		public void TestValidateCASSClaimStatus_ClaimExist_Overbilled()
		{
			AccQueryClaim claim = SetupAssociatedClaim(Factory, 100M);
			Factory.Save();
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.BothOverUnderBilled);
			SetupCASSClaimRequiredBillingLineAndBizObjsFor(TestCASSBillingLine, pWCAmount: 400M, adjustedPWCAmount: 180M);
			TestCASSBillingLine.ForceRecalculateData();

			AssertEquals(CASSBillingLine.StatusType.Overbilled, TestCASSBillingLine.Status);
			AssertEquals("Over-billed", TestCASSBillingLine.StatusForBinding);
			AssertHasWarning(TestCASSBillingLine.StatusForBindingInfo, "Claim already exists for this MAWB and will be linked to this new AP invoice.");
		}

		public void TestStatus()
		{
			SetupAssociatedBizos();
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine, setAdjustmentValues: false, setupAmount: false);
			Factory.Save();

			TestCASSBillingLine.ForceRecalculateData();

			(TestCASSBillingLine.AggregatedCostLine as CASSCostExportLine).WeightChargePP = 100.00M;
			AssertEquals(CASSBillingLine.StatusType.Underbilled, TestCASSBillingLine.Status);
			AssertEquals("Under-billed", TestCASSBillingLine.StatusForBinding);
			TestCASSBillingLine.ForceRecalculateData();
			(TestCASSBillingLine.AggregatedCostLine as CASSCostExportLine).WeightChargePP = 600.00M;
			AssertEquals(CASSBillingLine.StatusType.Overbilled, TestCASSBillingLine.Status);
			AssertEquals("Over-billed", TestCASSBillingLine.StatusForBinding);

			TestObjectCreator.CreateCharge(Job2, FRT, "FRT", TestCASSBillingLine.CASSCostCurrency, 480.18M, null, TestCASSBillingLine.CASSCostCurrency, 1M, null);
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			(TestCASSBillingLine.AggregatedCostLine as CASSCostExportLine).WeightChargePP = 680.18M;
			AssertEquals(CASSBillingLine.StatusType.Exact, TestCASSBillingLine.Status);
			AssertEquals("Exact", TestCASSBillingLine.StatusForBinding);

			TestObjectCreator.CreateCharge(Job2, FRT, "FRT", TestCASSBillingLine.CASSCostCurrency, 1000M, null, TestCASSBillingLine.CASSCostCurrency, 1M, null);
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			(TestCASSBillingLine.AggregatedCostLine as CASSCostExportLine).WeightChargePP = 180.00M;
			AssertEquals(CASSBillingLine.StatusType.Underbilled, TestCASSBillingLine.Status);
			AssertEquals("Under-billed", TestCASSBillingLine.StatusForBinding);
		}

		[TestDate(2018, 01, 01)]
		public void TestAddCostLine()
		{
			var today = ZDateTime.Today;
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine, setAdjustmentValues: false, setupAmount: false, isRejectedRecordType: true);
			(TestCASSBillingLine.AggregatedCostLine as CASSCostExportLine).WeightChargePP = 680.18M;
			(TestCASSBillingLine.AggregatedCostLine as CASSCostExportLine).VATDueAirline = 68.01M;

			AssertEquals(true, TestCASSBillingLine.IsRejectedClaimLine);
			AssertEquals(true, TestCASSBillingLine.IsGSTApplicable);
			AssertEquals("172", TestCASSBillingLine.AirlinePrefix);
			AssertEquals("67828073", TestCASSBillingLine.AWBNumber);
			AssertEquals("23470068510", TestCASSBillingLine.AgentCode);
			AssertEquals(today.AddMonths(-3), TestCASSBillingLine.IssueDate);
			AssertEquals(today.AddMonths(-2), TestCASSBillingLine.DateOfArrival);
			AssertEquals(today.AddMonths(-1), TestCASSBillingLine.DateOfDelivery);
			AssertEquals("LEJ", TestCASSBillingLine.LoadPortIATA);
			AssertEquals("MEX", TestCASSBillingLine.DischargePortIATA);
			AssertEquals(2150M, TestCASSBillingLine.CASSWeight);
			AssertEquals("KG", TestCASSBillingLine.CASSWeightUnit);
			AssertEquals("AUD", TestCASSBillingLine.CASSCostCurrencyCode);
			AssertEquals(680.18M, TestCASSBillingLine.CASSCostValue);
			AssertEquals(68.01M, TestCASSBillingLine.CASSCostTaxValue);
			AssertEquals(0M, TestCASSBillingLine.CASSCostAdjustedValue);
			AssertEquals(0M, TestCASSBillingLine.CASSCostTaxAdjustedValue);

			var adjutmentCostLine = new CASSCostExportLine(Factory, CASSCostLineType.Adjustment);
			adjutmentCostLine.RecordType = "DCO";
			adjutmentCostLine.CurrencyCode = "AUD";
			adjutmentCostLine.WeightChargePP = 1015.68M;
			adjutmentCostLine.VATDueAirline = 101.56M;

			TestCASSBillingLine.AddCostLine(adjutmentCostLine, "AUD");
			AssertEquals(680.18M, TestCASSBillingLine.CASSCostValue);
			AssertEquals(68.01M, TestCASSBillingLine.CASSCostTaxValue);
			AssertEquals(-1015.68M, TestCASSBillingLine.CASSCostAdjustedValue);
			AssertEquals(-101.56M, TestCASSBillingLine.CASSCostTaxAdjustedValue);
		}

		public void TestCASSCurrencyDecimals()
		{
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine, setupAmount: false);

			(TestCASSBillingLine.AggregatedCostLine as CASSCostExportLine).WeightChargePP = 680.18M;
			(TestCASSBillingLine.AggregatedCostLine as CASSCostExportLine).VATDueAirline = 68.01M;

			var costAdjustmentLine = new CASSCostExportLine(Factory, CASSCostLineType.Adjustment);
			costAdjustmentLine.CurrencyCode = "AUD";
			costAdjustmentLine.WeightChargePP = 1015.68M;
			costAdjustmentLine.VATDueAirline = 101.56M;

			TestCASSBillingLine.AddCostLine(costAdjustmentLine, "AUD");

			AssertEquals("AUD", TestCASSBillingLine.CASSCostCurrencyCode);
			AssertEquals(680.18M, TestCASSBillingLine.CASSCostValue);
			AssertEquals(-1015.68M, TestCASSBillingLine.CASSCostAdjustedValue);
			AssertEquals(68.01M, TestCASSBillingLine.CASSCostTaxValue);
			AssertEquals(-101.56M, TestCASSBillingLine.CASSCostTaxAdjustedValue);
		}

		public void TestBranch()
		{
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
			AssertNull(TestCASSBillingLine.Branch);

			SetupAssociatedBizos();
			GlbBranch testBranch = TestObjectCreator.CreateBranch("BRN", "Branch 1", GlbCompany.CurrentCompany);
			Job1.JH_GB = testBranch.PK;
			Factory.Save();

			TestCASSBillingLine.ForceRecalculateData();
			AssertNull("Branch should not be found from Job", TestCASSBillingLine.Branch);

			TestCASSBillingLine.ForceRecalculateData();
			Env.Registry.SetIssuingCarrierAgentIATACode(Env.CurrentBranch.PK, TestCASSBillingLine.AgentCode);
			AssertEquals("Branch should be found from Registry", Env.CurrentBranch.PK, TestCASSBillingLine.Branch.PK);

			var costLine = new CASSCostExportLine(Factory, CASSCostLineType.Adjustment);
			costLine.AgentCode = "XXX";
			TestCASSBillingLine.AddCostLine(costLine, TestCASSBillingLine.CASSCostCurrencyCode);
			AssertNull("Branch should not be found from Job (again)", TestCASSBillingLine.Branch);

			costLine.AgentCode = "23470068510";
			TestCASSBillingLine.AddCostLine(costLine, TestCASSBillingLine.CASSCostCurrencyCode);
			AssertEquals("Branch should be found from Registry again", Env.CurrentBranch.PK, TestCASSBillingLine.Branch.PK);

			costLine.AgentCode = "23470/06-8510";
			TestCASSBillingLine.AddCostLine(costLine, TestCASSBillingLine.CASSCostCurrencyCode);
			AssertEquals("Branch should be found from Registry again", Env.CurrentBranch.PK, TestCASSBillingLine.Branch.PK);

			Env.Registry.SetIssuingCarrierAgentIATACode(Env.CurrentBranch.PK, " 2 -  3/4-  7-0/0   /6-8 /510  ");
			costLine.AgentCode = "23-4 7006 / 8510";
			TestCASSBillingLine.AddCostLine(costLine, TestCASSBillingLine.CASSCostCurrencyCode);
			AssertEquals(Env.CurrentBranch.PK, TestCASSBillingLine.Branch.PK);

			Env.Registry.SetIssuingCarrierAgentIATACode(Env.CurrentBranch.PK, "23-4 7006 / 8510");
			costLine.AgentCode = " 2 -  3/4-  7-0/0   /6-8 /510  ";
			TestCASSBillingLine.AddCostLine(costLine, TestCASSBillingLine.CASSCostCurrencyCode);
			AssertEquals(Env.CurrentBranch.PK, TestCASSBillingLine.Branch.PK);
		}

		public void TestMAWBNumber()
		{
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
			AssertEquals("MAWB is Airline 3 letter code + AWB Number", TestCASSBillingLine.AirlinePrefix + TestCASSBillingLine.AWBNumber, TestCASSBillingLine.MAWBNumber);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestConsolGatewayBilling()
		{
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
			SetupAssociatedBizos();
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(Consol.PK, TestCASSBillingLine.ConsolPK);
			AssertEquals(Consol.JK_UniqueConsignRef, TestCASSBillingLine.ConsolID);

			fTestCASSBillingLine = null;
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
			Job1.JH_GC = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentCompany.PK)).PK;
			Job2.JH_GC = Job1.JH_GC;
			AssertEquals(Consol.PK, TestCASSBillingLine.ConsolPK);

			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(ZGuid.Empty, TestCASSBillingLine.ConsolPK);

			Job1.JH_GC = Env.CurrentCompany.PK;
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(Consol.PK, TestCASSBillingLine.ConsolPK);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestSystemCostPostedValue()
		{
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
			SetupAssociatedBizos();
			TestObjectCreator.SetupCreditorAirline(TestObjectCreator.CreateAirLine(TestCASSBillingLine.AirlinePrefix), TestObjectCreator.AALSHI);
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			APInvoice apInvoice = Factory.New<APInvoice>();
			apInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			apInvoice.AH_TransactionNum = "ADAD";
			apInvoice.SubmittedFromInvoicingForm = true;
			APInvoiceLine apLine = (APInvoiceLine)apInvoice.Lines.AddNew();
			apLine.GenericCharge = FRT.PK;
			apLine.AL_JH = Job1.PK;
			apLine.AL_OSExTaxAmount = 100M;
			Factory.Save();

			AssertEquals(100M, TestCASSBillingLine.SystemCostPostedValue);
			AssertEquals(100M, TestCASSBillingLine.SystemCostAccrualValue);

			fTestCASSBillingLine = null;
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
			Job1.JH_GC = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentCompany.PK)).PK;
			Job2.JH_GC = Job1.JH_GC;
			AssertEquals(100M, TestCASSBillingLine.SystemCostPostedValue);
			AssertEquals(100M, TestCASSBillingLine.SystemCostAccrualValue);

			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(0M, TestCASSBillingLine.SystemCostPostedValue);
			AssertEquals(0M, TestCASSBillingLine.SystemCostAccrualValue);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestSystemCostAccrualValue()
		{
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
			SetupAssociatedBizos();
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(200M, TestCASSBillingLine.SystemCostAccrualValue);

			fTestCASSBillingLine = null;
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
			Job1.JH_GC = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentCompany.PK)).PK;
			Job2.JH_GC = Job1.JH_GC;
			AssertEquals(200M, TestCASSBillingLine.SystemCostAccrualValue);

			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(0M, TestCASSBillingLine.SystemCostAccrualValue);
		}

		public void TestPefrormanceOfACRDataLoadingQueries()
		{
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
			SetupAssociatedBizos();
			Factory.Save();

			var chargeCodePKsFromRegistry = new CASSCostExportLine(Factory, CASSCostLineType.Default).GetCASSChargeCodePKsFromRegistry();
			var chargeCodes = new HashSet<AccChargeCode>();
			if (chargeCodePKsFromRegistry != null && chargeCodePKsFromRegistry.Length > 0)
			{
				foreach (ZGuid chargeCodePK in chargeCodePKsFromRegistry)
				{
					AccChargeCode chargeCode = Factory.Load<AccChargeCode>(chargeCodePK);
					if (chargeCode != null)
					{
						chargeCodes.Add(chargeCode);
					}
				}
			}

			TestCASSBillingLine.ForceRecalculateData();
			var bizos = TestCASSBillingLine.GetConsolBranchSystemCostValuesCollection(chargeCodes);
			AssertNotNull("GetConsolBranchSystemCostValuesCollection:Load", bizos);

			TestCASSBillingLine.ForceRecalculateData();
			TestCASSBillingLine.InitializeBranchAndDepartmentForExistingCharges(chargeCodes);
			AssertNotNull("InitializeBranchAndDepartmentForExistingCharges:Load", TestCASSBillingLine.SystemChargeDataByJobAndChargeCode);
		}

		public void TestApportionMethodInfoText()
		{
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
			SetupAssociatedBizos();
			Factory.Save();

			var mainChargeCode = TestObjectCreator.FRT;
			var organisation = TestObjectCreator.AALSHI;

			var cost1 = TestObjectCreator.CreateConsolCost(Consol, mainChargeCode, organisation, 250M, true, "MAN");
			cost1.E6_RX_NKCurrency = Constants.CurrencyCodes.Australia;

			var cost2 = TestObjectCreator.CreateConsolCost(Consol, mainChargeCode, null, 150M, true, "SHP");
			cost2.E6_RX_NKCurrency = Constants.CurrencyCodes.UnitedStates;
			cost2.E6_ExchangeRate = 1.0M;

			var cost3 = TestObjectCreator.CreateConsolCost(Consol, mainChargeCode, null, 50M, true, "SHP");
			cost3.E6_RX_NKCurrency = Constants.CurrencyCodes.EuropeanUnion;
			cost3.E6_ExchangeRate = 1.0M;

			Factory.Save();

			var chargeCodePKsFromRegistry = new CASSCostExportLine(Factory, CASSCostLineType.Default).GetCASSChargeCodePKsFromRegistry();
			var chargeCodes = new HashSet<AccChargeCode>();
			if (chargeCodePKsFromRegistry != null && chargeCodePKsFromRegistry.Length > 0)
			{
				foreach (var chargeCodePK in chargeCodePKsFromRegistry)
				{
					var chargeCode = Factory.Load<AccChargeCode>(chargeCodePK);
					if (chargeCode != null)
					{
						chargeCodes.Add(chargeCode);
					}
				}
			}

			var expectedAppMethodInfoText = $"{organisation.PK}|AUD|250.00|MAN|{mainChargeCode.AC_AT_GSTRate}|, |USD|150.00|SHP||, |EUR|50.00|SHP||".ToUpper();

			TestCASSBillingLine.ForceRecalculateData();
			var bizos = TestCASSBillingLine.GetConsolBranchSystemCostValuesCollection(chargeCodes);
			AssertNotNull("GetConsolBranchSystemCostValuesCollection:Load", bizos);

			var actualAppMethodInfoText = bizos[0][$"ApportionmentMethod_{mainChargeCode.PK.ToString().Replace('-', '_')}"].ToString();
			AssertEquals("Apportion Method Info Text", expectedAppMethodInfoText, actualAppMethodInfoText);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestSystemCostAccrualValue_WithPreparedCosts_NoAccruals()
		{
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
			SetupAssociatedBizos();
			Charge preparedNotPostedCharge = TestObjectCreator.CreateCharge(Job2, FRT, "FRT", TestObjectCreator.AUD, 400M, null, TestObjectCreator.AUD, 100M, null);
			preparedNotPostedCharge.JR_AL_APLine = ZGuid.Empty;
			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(600M, TestCASSBillingLine.SystemCostAccrualValue);

			fTestCASSBillingLine = null;
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
			Job1.JH_GC = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentCompany.PK)).PK;
			Job2.JH_GC = Job1.JH_GC;
			AssertEquals(600M, TestCASSBillingLine.SystemCostAccrualValue);

			Factory.Save();
			TestCASSBillingLine.ForceRecalculateData();
			AssertEquals(0M, TestCASSBillingLine.SystemCostAccrualValue);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestSystemCostAccrualValueGatewayBilling()
		{
			ZQuery gatewayDepartmentQuery = new ZQuery(GlbDepartmentSchema.GE_Code, SQLComparisonOperator.StartsWith, "G");
			GlbDepartment gatewayDepartment = Factory.LoadTop1<GlbDepartment>(gatewayDepartmentQuery);

			using (new TemporaryUserContext { DepartmentPK = gatewayDepartment.PK.ToGuid() }.Set())
			{
				TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
				SetupAssociatedBizos();

				Consol.JK_AgentType = Constants.AgentType.Agent;
				Consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

				var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
				port.O5_SeaAirCarrierOrForwarderType = "GTW";
				port.O5_PortOrCountry = Consol.JK_RL_NKLoadPort;
				port.O5_OA_AgentOfficeAddress = Consol.JK_OA_SendingForwarderAddress;
				port.O5_AgentDirection = AgentDirectionList.Codes.Both;
				port.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
				Consol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);

				var gatewayBillingJob = TestObjectCreator.CreateJob(Consol, false);
				var gatewayCost = gatewayBillingJob.Charges.AddNew();
				gatewayCost.JR_AC = FRT.PK;
				gatewayCost.JR_OH_SellAccount = Consol.SendingForwarderPK;
				gatewayCost.JR_RX_NKSellCurrency = "AUD";
				gatewayCost.JR_OSSellAmt = 50m;

				Factory.Save();
				TestCASSBillingLine.ForceRecalculateData();
				AssertEquals(true, Consol.IsGateway());

				AssertEquals(50M, TestCASSBillingLine.SystemCostAccrualValue);
				AssertEquals(true, TestCASSBillingLine.IsForGatewayBilling);
				AssertEquals(gatewayBillingJob.PK, TestCASSBillingLine.GatewayBillingJob.PK);

				fTestCASSBillingLine = null;
				TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
				var otherCompanyPK = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentCompany.PK)).PK;
				Job1.JH_GC = otherCompanyPK;
				Job2.JH_GC = otherCompanyPK;
				gatewayBillingJob.JH_GC = otherCompanyPK;
				AssertEquals(50M, TestCASSBillingLine.SystemCostAccrualValue);
				AssertEquals(true, TestCASSBillingLine.IsForGatewayBilling);
				AssertNull(TestCASSBillingLine.GatewayBillingJob);

				Factory.Save();
				AssertEquals(true, Consol.IsGateway());
				// No Gateway Job is assocoated with the Gateway Consol
				TestCASSBillingLine.ForceRecalculateData();
				AssertEquals(0M, TestCASSBillingLine.SystemCostAccrualValue);
				AssertEquals(true, TestCASSBillingLine.IsForGatewayBilling);
				AssertNull(TestCASSBillingLine.GatewayBillingJob);
			}
		}

		public void TestCheckPerformanceOnGetConsolBranchSystemCostValuesCollectionWithConsolIsNotGateway()
		{
			var gatewayDepartmentQuery = new ZQuery(GlbDepartmentSchema.GE_Code, SQLComparisonOperator.StartsWith, "G");
			var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(gatewayDepartmentQuery);

			using (new TemporaryUserContext { DepartmentPK = gatewayDepartment.PK.ToGuid() }.Set())
			{
				TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
				SetupAssociatedBizos();
				Consol.JK_AgentType = Constants.AgentType.CoLoad;

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var shipment2 = TestObjectCreator.CreateShipment("S0002");
				var consol = Factory.New<ForwardingConsol>();
				consol.Shipments.Add(shipment);
				consol.Shipments.Add(shipment2);
				Factory.Save();

				var apportionments = new ApportionmentListing(Factory, consol);
				var consolCost = apportionments.CostsCollection.TryAddNew();
				consolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				consolCost.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
				consolCost.E6_RX_NKCurrency = Constants.CurrencyCodes.Australia;
				consolCost.E6_ExchangeRate = 1m;
				consolCost.E6_OSCostAmount = 11m;
				//consolCost.E6_PPDCLT = Constants.DomesticPaymentTerms.Prepaid;
				consolCost.E6_AT_TaxRate = TestObjectCreator.GST1.PK;
				consolCost.E6_InvoiceNum = "I001";
				consolCost.E6_InvoiceDate = ZDateTime.Now;
				consolCost.E6_PaymentDate = ZDateTime.Now;
				Factory.Save();

				consol.JK_AgentType = Constants.AgentType.CoLoad;
				consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				consol.JK_MasterBillNum = Consol.JK_MasterBillNum;
				Factory.Save();

				Assert("Consol is not a Gateway Consol", !consol.IsGateway());

				// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
				using (Db.Connection.TrackExecutedCommands())
				{
					Assert(!TestCASSBillingLine.IsForGatewayBilling);
					Assert("JobConsol should not be loaded just to check the IsGatewayConsol property", !Db.Connection.ExecutedCommands.Any(command => command.Contains(ForwardingConsol.Schema.TableName) && !command.Contains("IsGatewayBilling")));
				}
			}
		}

		public void TestCheckPerformanceOnGetConsolBranchSystemCostValuesCollectionWithConsolIsGateway()
		{
			var gatewayDepartmentQuery = new ZQuery(GlbDepartmentSchema.GE_Code, SQLComparisonOperator.StartsWith, "G");
			var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(gatewayDepartmentQuery);

			using (new TemporaryUserContext { DepartmentPK = gatewayDepartment.PK.ToGuid() }.Set())
			{
				TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
				SetupAssociatedBizos();
				TestObjectCreator.SetupCreditorAirline(TestObjectCreator.CreateAirLine(TestCASSBillingLine.AirlinePrefix), TestObjectCreator.AALSHI);
				Factory.Save();

				TestCASSBillingLine.ForceRecalculateData();

				var gatewayAgent = GlbCompany.CurrentCompany.OrgProxy;

				Consol.JK_UniqueConsignRef = "C10011991";
				Consol.JK_AgentType = Constants.AgentType.Agent;
				Consol.JK_RL_NKLoadPort = "AUSYD";
				Consol.JK_RL_NKDischargePort = "CNSHA";
				Consol.JK_OA_SendingForwarderAddress = gatewayAgent.MainAddress.PK;
				Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

				var gatewayAgentPort = Consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
				gatewayAgentPort.O5_OA_AgentOfficeAddress = Consol.JK_OA_SendingForwarderAddress;
				gatewayAgentPort.O5_PortOrCountry = "AUSYD";
				gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
				gatewayAgentPort.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

				Consol.Shipments.AddNew();
				Factory.Save();

				using (var gatewayJob = TestObjectCreator.CreateJob(Consol))
				{
					gatewayJob.JH_OA_LocalChargesAddr = gatewayAgent.MainAddress.PK;
					gatewayJob.JH_GE = TestObjectCreator.GEADepartment.PK;

					var charge1 = gatewayJob.Charges.AddNew();
					charge1.JR_AC = FRT.PK;
					charge1.JR_GE = TestObjectCreator.GEADepartment.PK;
					charge1.JR_OH_SellAccount = gatewayAgent.PK;
					charge1.JR_LocalSellAmt = 125m;
					charge1.JR_JH_InternalJob = gatewayJob.PK;

					var apInvoice = Factory.New<APInvoice>();
					apInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
					apInvoice.AH_TransactionNum = "ADAD1";
					apInvoice.SubmittedFromInvoicingForm = true;
					var apLine = (APInvoiceLine)apInvoice.Lines.AddNew();
					apLine.GenericCharge = FRT.PK;
					apLine.AL_JH = gatewayJob.PK;
					apLine.AL_OSExTaxAmount = 66M;

					var charge2 = gatewayJob.Charges.AddNew();
					charge2.JR_AC = FRT.PK;
					charge2.JR_GE = TestObjectCreator.GEADepartment.PK;
					charge2.JR_OH_SellAccount = gatewayAgent.PK;
					charge2.JR_LocalSellAmt = 44;
					charge2.JR_JH_InternalJob = Job2.PK;

					var apInvoice2 = Factory.New<APInvoice>();
					apInvoice2.AH_OH = TestObjectCreator.AALSHI.PK;
					apInvoice2.AH_TransactionNum = "ADAD2";
					apInvoice2.SubmittedFromInvoicingForm = true;
					var apLine2 = (APInvoiceLine)apInvoice2.Lines.AddNew();
					apLine2.GenericCharge = FRT.PK;
					apLine2.AL_JH = gatewayJob.PK;
					apLine2.AL_OSExTaxAmount = 37M;
					Factory.Save();
				}
			}

			Assert("Consol is a Gateway Consol", Consol.IsGateway());

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (Db.Connection.TrackExecutedCommands())
			{
				Assert(TestCASSBillingLine.IsForGatewayBilling);
				Assert("JobConsol should not be loaded just to check the IsGatewayConsol property", !Db.Connection.ExecutedCommands.Any(command => command.Contains(ForwardingConsol.Schema.TableName) && !command.Contains("IsGatewayBilling")));
				AssertEquals(169m, TestCASSBillingLine.SystemCostAccrualValue);
				AssertEquals(103m, TestCASSBillingLine.SystemCostPostedValue);
			}
		}

		public void TestIsGatewayBillingOnGatewayConsolWithoutAccrualsNotCFS()
		{
			AssertIsGatewayBillingOnGatewayConsolWithoutAccruals(false);
		}

		public void TestIsGatewayBillingOnGatewayConsolWithoutAccrualsISCFS()
		{
			AssertIsGatewayBillingOnGatewayConsolWithoutAccruals(true);
		}

		void AssertIsGatewayBillingOnGatewayConsolWithoutAccruals(bool isCFS)
		{
			var gatewayDepartmentQuery = new ZQuery(GlbDepartmentSchema.GE_Code, SQLComparisonOperator.StartsWith, "G");
			var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(gatewayDepartmentQuery);

			using (new TemporaryUserContext { DepartmentPK = gatewayDepartment.PK.ToGuid() }.Set())
			{
				TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
				SetupAssociatedBizos();

				Consol.JK_AgentType = Constants.AgentType.Agent;
				Consol.JK_IsCFS = isCFS;
				Consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

				var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
				port.O5_SeaAirCarrierOrForwarderType = "GTW";
				port.O5_PortOrCountry = Consol.JK_RL_NKLoadPort;
				port.O5_OA_AgentOfficeAddress = Consol.JK_OA_SendingForwarderAddress;
				port.O5_AgentDirection = AgentDirectionList.Codes.Both;
				port.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
				Consol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);

				Factory.Save();
				TestCASSBillingLine.ForceRecalculateData();

				Assert("IsGateway", Consol.IsGateway());

				Assert("Line should have no accrual value", TestCASSBillingLine.SystemCostAccrualValue.IsEmpty);
				AssertNull("Should be no Gateway Job", TestCASSBillingLine.GatewayBillingJob);
				Assert("Still should be a Gateway Billing", TestCASSBillingLine.IsForGatewayBilling);

				var gatewayBillingJob = TestObjectCreator.CreateJob(Consol, false);
				Factory.Save();
				Assert("IsGateway", Consol.IsGateway());

				fTestCASSBillingLine = null;
				TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);

				Assert("Line has no accrual value", TestCASSBillingLine.SystemCostAccrualValue.IsEmpty);
				AssertNotNull("Should have a Gateway Job", TestCASSBillingLine.GatewayBillingJob);
				AssertEquals("Should be our Gateway Job", gatewayBillingJob.PK, TestCASSBillingLine.GatewayBillingJob.PK);
				Assert("Should be a Gateway Billing", TestCASSBillingLine.IsForGatewayBilling);
			}
		}

		public void TestIsGatewayBillingOnNonGatewayConsolWithAccruals()
		{
			var gatewayDepartmentQuery = new ZQuery(GlbDepartmentSchema.GE_Code, SQLComparisonOperator.StartsWith, "G");
			var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(gatewayDepartmentQuery);

			using (new TemporaryUserContext { DepartmentPK = gatewayDepartment.PK.ToGuid() }.Set())
			{
				TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
				SetupAssociatedBizos();

				Consol.JK_AgentType = Constants.AgentType.Agent;
				Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				Consol.JK_IsCFS = true;
				Consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

				Factory.Save();
				TestCASSBillingLine.ForceRecalculateData();
				AssertEquals("IsGateway", false, Consol.IsGateway());

				AssertEquals("Line accrual value", 200M, TestCASSBillingLine.SystemCostAccrualValue);
				AssertNull("Should be no Gateway Job", TestCASSBillingLine.GatewayBillingJob);
				AssertEquals("Should be non Gateway Billing", false, TestCASSBillingLine.IsForGatewayBilling);
			}
		}

		public void TestNetCASSCost()
		{
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
			SetupAssociatedBizos();

			var costLine = new CASSCostExportLine(Factory, CASSCostLineType.Default);
			TestCASSBillingLine.AddCostLine(costLine, "AUD");
			Factory.Save();

			AssertEquals(-335.5M, TestCASSBillingLine.NetCASSCost);
		}

		public void TestIsCASSAmendment()
		{
			var testCASSBillingLine = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSBillingLine(testCASSBillingLine, setAdjustmentValues: true);
			SetupAssociatedBizos();
			Factory.Save();

			AssertEquals(true, testCASSBillingLine.IsCASSAmendment);

			testCASSBillingLine = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSBillingLine(testCASSBillingLine, false);
			AssertEquals(false, testCASSBillingLine.IsCASSAmendment);
		}

		public void TestValidateCASSCostValues()
		{
			string expectedWarning = "No costs or adjustments on this line. This line is for information only and will not post to an invoice.";

			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine, setupAmount: false);
			AssertHasRowWarning(TestCASSBillingLine, expectedWarning);

			var costLine = new CASSCostExportLine(Factory, CASSCostLineType.Billing);
			costLine.CurrencyCode = "AUD";
			costLine.WeightChargePP = 500.00M;
			TestCASSBillingLine.AddCostLine(costLine, "AUD");
			AssertNoRowWarningContaining(TestCASSBillingLine, expectedWarning);

			costLine = new CASSCostExportLine(Factory, CASSCostLineType.Default);
			costLine.CurrencyCode = "AUD";
			costLine.WeightChargePP = 200.00M;
			TestCASSBillingLine.AddCostLine(costLine, "AUD");
			AssertNoRowWarningContaining(TestCASSBillingLine, expectedWarning);
		}

		public void TestCASSCostValuesForDisplay()
		{
			var cassBillingLine = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSBillingLine(cassBillingLine, setAdjustmentValues: false, setupAmount: false);
			(cassBillingLine.AggregatedCostLine as CASSCostExportLine).WeightChargePP = 680.18M;

			AssertEquals("Precondition:", 680.18M, cassBillingLine.CASSCostValue);
			AssertEquals("Precondition: IsRejectedClaimLinesExpected", false, cassBillingLine.IsRejectedClaimLine);

			AssertEquals("CASSCostValueInLocalCurrencyForDisplay", 680.18M, cassBillingLine.CASSCostValueInLocalCurrencyForDisplay);
			AssertEquals("CASSRejectedClaimValueInLocalCurrencyForDisplay", 0M, cassBillingLine.CASSRejectedClaimValueInLocalCurrencyForDisplay);

			cassBillingLine = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSBillingLine(cassBillingLine, setAdjustmentValues: false, isRejectedRecordType: true, setupAmount: false);
			(cassBillingLine.AggregatedCostLine as CASSCostExportLine).WeightChargePP = 680.18M;

			AssertEquals("CASSCostValueInLocalCurrencyForDisplay", 0M, cassBillingLine.CASSCostValueInLocalCurrencyForDisplay);
			AssertEquals("CASSRejectedClaimValueInLocalCurrencyForDisplay", 680.18M, cassBillingLine.CASSRejectedClaimValueInLocalCurrencyForDisplay);
		}

		public void TestGatewayBillingJobWithGcFilterAndIsNotCfs_GatewayAgent()
		{
			TestGatewayBillingJobWithGcFilterAndIsNotCfs(Constants.AgentType.Agent);
		}

		public void TestGatewayBillingJobWithGcFilterAndIsNotCfs_GatewayCoLoad()
		{
			TestGatewayBillingJobWithGcFilterAndIsNotCfs(Constants.AgentType.CoLoad);
		}

		void TestGatewayBillingJobWithGcFilterAndIsNotCfs(string agentType)
		{
			AssertGatewayBillingJobWithGcFilter(false, agentType);
		}

		public void TestGatewayBillingJobWithGcFilterAndIsCfs_GatewayAgent()
		{
			TestGatewayBillingJobWithGcFilterAndIsCfs(Constants.AgentType.Agent);
		}

		public void TestGatewayBillingJobWithGcFilterAndIsCfs_GatewayCoLoad()
		{
			TestGatewayBillingJobWithGcFilterAndIsCfs(Constants.AgentType.CoLoad);
		}

		void TestGatewayBillingJobWithGcFilterAndIsCfs(string agentType)
		{
			AssertGatewayBillingJobWithGcFilter(true, agentType);
		}

		public void AssertGatewayBillingJobWithGcFilter(bool isCfs, string agentType)
		{
			ZQuery gatewayDepartmentQuery = new ZQuery(GlbDepartmentSchema.GE_Code, SQLComparisonOperator.StartsWith, "G");
			GlbDepartment gatewayDepartment = Factory.LoadTop1<GlbDepartment>(gatewayDepartmentQuery);

			using (new TemporaryUserContext { DepartmentPK = gatewayDepartment.PK.ToGuid() }.Set())
			{
				TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
				SetupAssociatedBizos();

				Consol.JK_AgentType = agentType;
				Consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

				var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
				port.O5_SeaAirCarrierOrForwarderType = "GTW";
				port.O5_PortOrCountry = Consol.JK_RL_NKLoadPort;
				port.O5_OA_AgentOfficeAddress = Consol.JK_OA_SendingForwarderAddress;
				port.O5_AgentDirection = AgentDirectionList.Codes.Both;
				port.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
				Consol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);

				var gatewayBillingJob = TestObjectCreator.CreateJob(Consol, false);
				var gatewayCost = gatewayBillingJob.Charges.AddNew();
				gatewayCost.JR_AC = FRT.PK;
				gatewayCost.JR_OH_SellAccount = Consol.SendingForwarderPK;
				gatewayCost.JR_RX_NKSellCurrency = "AUD";
				gatewayCost.JR_OSSellAmt = 50m;

				Consol.JK_IsCFS = isCfs;

				Factory.Save();
				TestCASSBillingLine.ForceRecalculateData();

				// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
				using (Db.Connection.TrackExecutedCommands())
				{
					AssertEquals(50M, TestCASSBillingLine.SystemCostAccrualValue);
					AssertEquals(true, TestCASSBillingLine.IsForGatewayBilling);
					AssertEquals(gatewayBillingJob.PK, TestCASSBillingLine.GatewayBillingJob.PK);

					AssertEquals(gatewayBillingJob.JH_GC, Env.CurrentCompany.PK);
					gatewayBillingJob.JH_GC = TestObjectCreator.NonCurrentCompany.PK;
					AssertNull(TestCASSBillingLine.GatewayBillingJob);

					gatewayBillingJob.JH_GC = Env.CurrentCompany.PK;
					AssertNotNull(TestCASSBillingLine.GatewayBillingJob);

					AssertEquals(string.Format("JobConsol might be loaded just to check the IsGatewayConsol property depends on isCfs value: {0}", isCfs), isCfs, Db.Connection.ExecutedCommands.Any(command => command.Contains(ForwardingConsol.Schema.TableName) && !command.Contains("IsGatewayBilling")));
				}
			}
		}

		public void TestInitializeBranchAndDepartmentImportUnrecignizeCostForJRJ()
		{
			GlbBranch testBranch = TestObjectCreator.CreateBranch("BRN", "Branch 1", GlbCompany.CurrentCompany);

			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
			SetupAssociatedBizos();

			Job1.Charges.RemoveAndDeleteAll();
			Job2.Charges.RemoveAndDeleteAll();

			var journal1 = TestObjectCreator.CreateJobRevenueJournal(typeof(JobRevenueJournal), FRT, Job1, 100);
			var journal2 = TestObjectCreator.CreateJobRevenueJournal(typeof(JobRevenueJournal), FRT, Job2, 100);

			Factory.Save();

			Job1.Charges.Reload();
			Job2.Charges.Reload();
			AssertEquals("Should have 2 charges", 2, Job1.Charges.Count);
			AssertEquals("Should have 2 charges", 2, Job2.Charges.Count);

			var charge1 = Job1.Charges.FirstOrDefault() as Charge;
			charge1.JR_LocalCostAmt = 100m;
			Factory.Save();

			AssertNotEquals("Local amount of charge 1 is not 0.", 0, charge1.JR_LocalCostAmt);
			AssertEquals("local amount of Job 2 is all 0.", true, Job2.Charges.All(x => ((Charge)x).JR_LocalCostAmt == 0));
			TestCASSBillingLine.ForceRecalculateData();
			AssertNull(TestCASSBillingLine.Branch);
			AssertEquals("Should have 1 key value because only local amount is not 0 will to include.", 1, TestCASSBillingLine.SystemChargeDataByJobAndChargeCode.Count);
			AssertEquals(true, TestCASSBillingLine.SystemChargeDataByJobAndChargeCode.ContainsKey(new Tuple<ZGuid, ZGuid>(Job1.PK, FRT.PK)));
			AssertEquals(false, TestCASSBillingLine.SystemChargeDataByJobAndChargeCode.ContainsKey(new Tuple<ZGuid, ZGuid>(Job2.PK, FRT.PK)));
		}

		public void TestInitializeBranchAndDepartmentImportCostWithACRAndNotReverse()
		{
			SetupAssociatedBizos();
			var shipment3 = TestObjectCreator.CreateShipment("SHIP3", Shipment1.Origin.RL_Code, Shipment1.Destination.RL_Code, Consol);
			var job3 = TestObjectCreator.CreateJob(shipment3, false);

			var testBranch = TestObjectCreator.CreateBranch("BRN", "Branch 1", GlbCompany.CurrentCompany);
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);

			Job1.Charges.RemoveAndDeleteAll();
			Job2.Charges.RemoveAndDeleteAll();
			job3.Charges.RemoveAndDeleteAll();
			Factory.Save();

			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			var jobCharge1 = TestObjectCreator.CreateCharge(Job1, FRT, "FRT", TestCASSBillingLine.CASSCostCurrency, 100M, null, TestCASSBillingLine.CASSCostCurrency, 100M, null);
			jobCharge1.JR_GB = testBranch.PK;
			jobCharge1.JR_LocalCostAmt = 100m;
			jobCharge1.JR_LocalSellAmt = 0m;

			Factory.Save();

			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var jobCharge2 = TestObjectCreator.CreateCharge(Job2, FRT, "FRT", TestCASSBillingLine.CASSCostCurrency, 100M, null, TestCASSBillingLine.CASSCostCurrency, 100M, null);
			jobCharge2.JR_GB = testBranch.PK;
			jobCharge2.JR_LocalCostAmt = 100m;
			jobCharge2.JR_LocalSellAmt = 0m;

			var jobCharge3 = TestObjectCreator.CreateCharge(job3, FRT, "FRT", TestCASSBillingLine.CASSCostCurrency, 100M, null, TestCASSBillingLine.CASSCostCurrency, 100M, null);
			jobCharge3.JR_AC = FRT.PK;
			jobCharge3.JR_LocalCostAmt = 100m;
			jobCharge3.JR_LocalSellAmt = 0m;

			Factory.Save();

			jobCharge3.ReverseAccrual(ZDateTime.Today);
			jobCharge3.JR_LocalCostAmt = 0m;

			Factory.Save();

			AssertEquals("Should have 1 charges", 1, Job1.Charges.Count);
			AssertEquals("Should have 1 charges", 1, Job2.Charges.Count);
			AssertEquals("Should have 1 charges", 1, job3.Charges.Count);

			TestCASSBillingLine.ForceRecalculateData();
			AssertNull(TestCASSBillingLine.Branch);
			AssertEquals("Should have 2 key value", 2, TestCASSBillingLine.SystemChargeDataByJobAndChargeCode.Count);
			AssertEquals("Should contain job1 because has cost amount and is not recongnized.", true, TestCASSBillingLine.SystemChargeDataByJobAndChargeCode.ContainsKey(new Tuple<ZGuid, ZGuid>(Job1.PK, FRT.PK)));
			AssertEquals("Should contain job2 because it has ACR line and is not reversed.", true, TestCASSBillingLine.SystemChargeDataByJobAndChargeCode.ContainsKey(new Tuple<ZGuid, ZGuid>(Job2.PK, FRT.PK)));
			AssertEquals("Should not contain job3 because it has ACR line and is reversed.", false, TestCASSBillingLine.SystemChargeDataByJobAndChargeCode.ContainsKey(new Tuple<ZGuid, ZGuid>(job3.PK, FRT.PK)));
		}

		public void TestInvoiceNumberInput()
		{
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
			String expectedWarning = "Some CASS Costs have an AP Invoice Number entered. The system will calculate and group CASS Cost with AP Invoice Number against respective AP Invoices and CASS Cost without AP Invoice Number into separate AP Invoice with system generated AP Invoice Number.";

			TestCASSBillingLine.InvoiceNumber = "CASSUSD1";
			AssertEquals(true, TestCASSBillingLine.IsInvoiceNumberChanged);
			AssertHasWarning(TestCASSBillingLine.InvoiceNumberInfo, expectedWarning);

			TestCASSBillingLine.InvoiceNumber = "";
			AssertEquals(true, TestCASSBillingLine.IsInvoiceNumberChanged);
			AssertNoWarnings(TestCASSBillingLine.InvoiceNumberInfo);
		}

		public void TestAddCriticalError()
		{
			TestObjectCreator.SetupCASSBillingLine(TestCASSBillingLine);
			String expectedInvoiceNumberDuplicateInCollectionError = "This invoice number is already used on another invoice in this batch. Invoice numbers must be unique by creditor and currency.";
			String expectedInvoiceNumberDuplicateInDatabaseError = "This transaction number already exists for this creditor.";

			TestCASSBillingLine.AddCriticalError(CASSBillingLine.Schema.InvoiceNumber, CASSBillingLine.GetInvoiceNumberDuplicateInCollectionErrorMessage());
			TestCASSBillingLine.ValidateInvoiceNumber();
			AssertHasError(TestCASSBillingLine.InvoiceNumberInfo, expectedInvoiceNumberDuplicateInCollectionError);

			TestCASSBillingLine.ClearCriticalErrors();

			TestCASSBillingLine.AddCriticalError(CASSBillingLine.Schema.InvoiceNumber, CASSBillingLine.GetInvoiceNumberDuplicateInDatabaseErrorMessage());
			TestCASSBillingLine.ValidateInvoiceNumber();
			AssertHasError(TestCASSBillingLine.InvoiceNumberInfo, expectedInvoiceNumberDuplicateInDatabaseError);
		}

		public void TestGetConsolRetrievesLatestConsolOrderByCreateTime()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			TestObjectCreator.GLHeader1.AG_AccountType = Constants.AccountType.BalanceSheetAccount;
			TestObjectCreator.GLHeader1.AG_Description = "Test Account";
			AccountingConfigurationRegistry.Instance.CASSGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			ZQuery fRTFilter = new ZQuery(AccChargeCodeSchema.AC_Code, "FRT");
			fRTFilter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			var frt = Factory.LoadTop1<AccChargeCode>(fRTFilter);

			CASSBillingLine cassLine = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSBillingLine(cassLine);

			string origin = RefUNLOCO.LoadFromIATA(Factory, cassLine.LoadPortIATA).RL_Code;
			string destination = RefUNLOCO.LoadFromIATA(Factory, cassLine.DischargePortIATA).RL_Code;

			var consol = TestObjectCreator.CreateConsol(origin, destination, "C0001");
			consol.JK_MasterBillNum = cassLine.MAWBNumber;
			consol.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_SendingForwarderHandlingType = "GTA";
			var shipment1 = TestObjectCreator.CreateShipment("SHIP1", origin, destination, consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			TestObjectCreator.CreateCharge(job1, frt, "FRT", cassLine.CASSCostCurrency, 100M, null, cassLine.CASSCostCurrency, 100M, null);
			Factory.Save();

			var consol2 = TestObjectCreator.CreateConsol(origin, destination, "C0002");
			consol2.JK_MasterBillNum = cassLine.MAWBNumber; //both consol must have same master bill number
			consol2.JK_AgentType = Constants.AgentType.CoLoad;

			var shipment2 = TestObjectCreator.CreateShipment("SHIP2", origin, destination, consol2);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			TestObjectCreator.CreateCharge(job2, frt, "FRT", cassLine.CASSCostCurrency, 100M, null, cassLine.CASSCostCurrency, 100M, null);
			Factory.Save();

			cassLine.ForceRecalculateData();
			cassLine.InitializeConsolBranchSystemCostValues();
			var newFactory = new BusinessObjectFactory();
			var linkedConsol = newFactory.Load<ForwardingConsol>(cassLine.ConsolPK);
			AssertEquals("Line should be linked to the newer consol with same MAWB", "C0002", linkedConsol.JK_UniqueConsignRef);
		}

		public void TestGetConsolRetrievesLatestConsolOrderByCreateTime_BothGTW()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			TestObjectCreator.GLHeader1.AG_AccountType = Constants.AccountType.BalanceSheetAccount;
			TestObjectCreator.GLHeader1.AG_Description = "Test Account";
			AccountingConfigurationRegistry.Instance.CASSGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			ZQuery fRTFilter = new ZQuery(AccChargeCodeSchema.AC_Code, "FRT");
			fRTFilter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			var frt = Factory.LoadTop1<AccChargeCode>(fRTFilter);

			CASSBillingLine cassLine = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSBillingLine(cassLine);

			string origin = RefUNLOCO.LoadFromIATA(Factory, cassLine.LoadPortIATA).RL_Code;
			string destination = RefUNLOCO.LoadFromIATA(Factory, cassLine.DischargePortIATA).RL_Code;

			var consol = TestObjectCreator.CreateConsol(origin, destination, "C0001");
			consol.JK_MasterBillNum = cassLine.MAWBNumber;
			consol.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_SendingForwarderHandlingType = "GTA";

			var shipment1 = TestObjectCreator.CreateShipment("SHIP1", origin, destination, consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			TestObjectCreator.CreateCharge(job1, frt, "FRT", cassLine.CASSCostCurrency, 100M, null, cassLine.CASSCostCurrency, 100M, null);
			Factory.Save();

			var consol2 = TestObjectCreator.CreateConsol(origin, destination, "C0002");
			consol2.JK_MasterBillNum = cassLine.MAWBNumber; //both consol must have same master bill number
			consol2.JK_AgentType = Constants.AgentType.CoLoad;
			consol2.JK_SendingForwarderHandlingType = "GTA";

			var shipment2 = TestObjectCreator.CreateShipment("SHIP2", origin, destination, consol2);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			TestObjectCreator.CreateCharge(job2, frt, "FRT", cassLine.CASSCostCurrency, 100M, null, cassLine.CASSCostCurrency, 100M, null);
			Factory.Save();

			cassLine.ForceRecalculateData();
			cassLine.InitializeConsolBranchSystemCostValues();
			var newFactory = new BusinessObjectFactory();
			var linkedConsol = newFactory.Load<ForwardingConsol>(cassLine.ConsolPK);
			AssertEquals("Line should be linked to the newer consol with same MAWB", "C0002", linkedConsol.JK_UniqueConsignRef);
		}

		public void TestGetConsolRetrievesLatestConsolOrderByCreateTime_BothNotGTW()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			TestObjectCreator.GLHeader1.AG_AccountType = Constants.AccountType.BalanceSheetAccount;
			TestObjectCreator.GLHeader1.AG_Description = "Test Account";
			AccountingConfigurationRegistry.Instance.CASSGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			ZQuery fRTFilter = new ZQuery(AccChargeCodeSchema.AC_Code, "FRT");
			fRTFilter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			var frt = Factory.LoadTop1<AccChargeCode>(fRTFilter);

			CASSBillingLine cassLine = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSBillingLine(cassLine);

			string origin = RefUNLOCO.LoadFromIATA(Factory, cassLine.LoadPortIATA).RL_Code;
			string destination = RefUNLOCO.LoadFromIATA(Factory, cassLine.DischargePortIATA).RL_Code;

			var consol = TestObjectCreator.CreateConsol(origin, destination, "C0001");
			consol.JK_MasterBillNum = cassLine.MAWBNumber;
			consol.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			consol.JK_AgentType = Constants.AgentType.CoLoad;

			var shipment1 = TestObjectCreator.CreateShipment("SHIP1", origin, destination, consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			TestObjectCreator.CreateCharge(job1, frt, "FRT", cassLine.CASSCostCurrency, 100M, null, cassLine.CASSCostCurrency, 100M, null);
			Factory.Save();

			var consol2 = TestObjectCreator.CreateConsol(origin, destination, "C0002");
			consol2.JK_MasterBillNum = cassLine.MAWBNumber; //both consol must have same master bill number
			consol2.JK_AgentType = Constants.AgentType.CoLoad;

			var shipment2 = TestObjectCreator.CreateShipment("SHIP2", origin, destination, consol2);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			TestObjectCreator.CreateCharge(job2, frt, "FRT", cassLine.CASSCostCurrency, 100M, null, cassLine.CASSCostCurrency, 100M, null);
			Factory.Save();

			cassLine.ForceRecalculateData();
			cassLine.InitializeConsolBranchSystemCostValues();
			var newFactory = new BusinessObjectFactory();
			var linkedConsol = newFactory.Load<ForwardingConsol>(cassLine.ConsolPK);
			AssertEquals("Line should be linked to the newer consol with same MAWB", "C0002", linkedConsol.JK_UniqueConsignRef);
		}

#region Implementation

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;

		CASSBillingLine TestCASSBillingLine
		{
			get { return fTestCASSBillingLine ?? (fTestCASSBillingLine = new CASSBillingLine(Factory)); }
		}
		CASSBillingLine fTestCASSBillingLine;

		public static AccQueryClaim SetupAssociatedClaim(BusinessObjectFactory factory, decimal claimAmount, string claimStatus = QueryClaimStatusCodeList.Codes.QCStatus1Open)
		{
			var apInvoice = factory.NewWithValidTestData<AccTransactionHeader>();
			apInvoice.AH_Ledger = LedgerTypes.AccountsPayable;
			apInvoice.AH_AG = factory.NewWithValidTestData<AccGLHeader>().PK;
			var claim = (AccQueryClaim)factory.New<IAPAccQueryClaim>();
			claim.FillWithValidTestData();
			claim.AY_AH = apInvoice.PK;
			claim.AY_MasterBillNumber = "17267828073";
			claim.AY_QueryClaimAmount = claimAmount;
			claim.AY_QueryClaimStatus = claimStatus;
			return claim;
		}

		ForwardingConsol Consol;
		ForwardingShipment Shipment1;
		ForwardingShipment Shipment2;
		Job Job1;
		Job Job2;
		AccChargeCode FRT;

		void SetupCASSClaimRequiredBillingLineAndBizObjsFor(CASSBillingLine testCASSBillingLine,
			decimal acrAmountForShipment1 = 100M, decimal acrAmountForShipment2 = 100M,
			decimal pWCAmount = 0, decimal adjustedPWCAmount = 0, string currency = "AUD",
			bool setupConsol = true)
		{
			TestObjectCreator.SetupCreditorAirline(TestObjectCreator.CreateAirLine("172"), TestObjectCreator.AALSHI);
			TestObjectCreator.AddActiveContactIfRequires(TestObjectCreator.AALSHI);
			Factory.Save();

			if (pWCAmount > 0)
			{
				TestObjectCreator.SetupCASSCostComponent(testCASSBillingLine, isAdjustedAmount: false, pWCAmount: pWCAmount, pVCAmount: 0, pCCAmount: 0, cOAAmount: 0, cOMAmount: 0, dOIAmount: 0, currency: currency);
			}

			if (adjustedPWCAmount > 0)
			{
				TestObjectCreator.SetupCASSCostComponent(testCASSBillingLine, isAdjustedAmount: true, pWCAmount: adjustedPWCAmount, pVCAmount: 0, pCCAmount: 0, cOAAmount: 0, cOMAmount: 0, dOIAmount: 0, currency: currency);
			}

			if (setupConsol)
			{
				SetupAssociatedBizos(acrAmountForShipment1, acrAmountForShipment2);
			}
			Factory.Save();
		}

		void SetupAssociatedBizos(decimal acrAmountForShipment1 = 100M, decimal acrAmountForShipment2 = 100M)
		{
			CASSBillingLine cassLine = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSBillingLine(cassLine);

			string origin = RefUNLOCO.LoadFromIATA(Factory, cassLine.LoadPortIATA).RL_Code;
			string destination = RefUNLOCO.LoadFromIATA(Factory, cassLine.DischargePortIATA).RL_Code;

			Consol = TestObjectCreator.CreateConsol(origin, destination, "C0001");
			Consol.JK_MasterBillNum = cassLine.MAWBNumber;

			Shipment1 = TestObjectCreator.CreateShipment("SHIP1", origin, destination, Consol);
			Shipment2 = TestObjectCreator.CreateShipment("SHIP2", origin, destination, Consol);
			Job1 = TestObjectCreator.CreateJob(Shipment1, false);
			Job2 = TestObjectCreator.CreateJob(Shipment2, false);

			ZQuery fRTFilter = new ZQuery(AccChargeCodeSchema.AC_Code, "FRT");
			fRTFilter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			FRT = Factory.LoadTop1<AccChargeCode>(fRTFilter);
			TestObjectCreator.CreateCharge(Job1, FRT, "FRT", cassLine.CASSCostCurrency, 100M, null, cassLine.CASSCostCurrency, 100M, null);
			TestObjectCreator.CreateCharge(Job2, FRT, "FRT", cassLine.CASSCostCurrency, 100M, null, cassLine.CASSCostCurrency, 100M, null);

			TestObjectCreator.SetExchangeRate(Job1, cassLine.CASSCostCurrency, 2M);

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			TestObjectCreator.GLHeader1.AG_AccountType = Constants.AccountType.BalanceSheetAccount;
			TestObjectCreator.GLHeader1.AG_Description = "Test Account";
			AccountingConfigurationRegistry.Instance.CASSGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
		}

#endregion
	}
}
