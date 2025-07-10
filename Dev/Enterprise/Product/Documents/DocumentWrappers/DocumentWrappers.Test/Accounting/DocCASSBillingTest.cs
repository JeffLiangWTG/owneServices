using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocCASSBilling))]
	sealed class DocCASSBillingTest : DocumentWrapperTestCase
	{
		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocCASSBilling.New(CASSBilling, Factory)
			};
		}

		CASSBilling CASSBilling;
		protected override void SetUp()
		{
			CASSBilling = new CASSBilling(Factory);
			base.SetUp();
		}

		void SetupAssociatedBizos(bool isGateway)
		{
			CASSBillingLine cassLine = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSBillingLine(cassLine, currencyCode: "ERN");

			Consol = isGateway
				? TestObjectCreator.CreateGatewayConsol(cassLine.LoadPort, cassLine.DischargePort, "C0001", receivingGatewayCompany: GlbCompany.CurrentCompany, agentStatus: AgentStatusList.Codes.GatewayAgentWithTariff)
				: TestObjectCreator.CreateConsol(cassLine.LoadPort, cassLine.DischargePort, "C0001");

			Consol.JK_MasterBillNum = cassLine.MAWBNumber;

			Shipment1 = TestObjectCreator.CreateShipment("SHIP1", cassLine.LoadPort, cassLine.DischargePort, Consol);
			Shipment2 = TestObjectCreator.CreateShipment("SHIP2", cassLine.LoadPort, cassLine.DischargePort, Consol);
			Job1 = TestObjectCreator.CreateJob(Shipment1);
			Job2 = TestObjectCreator.CreateJob(Shipment2);
			var departmentPK = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA")).PK;
			if (isGateway)
			{
				GatewayBillingJob = TestObjectCreator.CreateJob(Consol, false);
				GatewayBillingJob.JH_GE = departmentPK;
			}
			Job1.JH_GE = departmentPK;
			Job2.JH_GE = departmentPK;

			ZQuery fRTFilter = new ZQuery(AccChargeCodeSchema.AC_Code, "FRT");
			fRTFilter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			AccChargeCode fRT = Factory.LoadTop1<AccChargeCode>(fRTFilter);
			if (fRT == null)
			{
				AccChargeCode fRTExisting = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
				fRT = Factory.New<AccChargeCode>();
				fRT.CopyPersistentValuesFrom(fRTExisting);
				fRT.AC_GC = GlbCompany.CurrentCompany.PK;
			}

			TestObjectCreator.CreateCharge(Job1, fRT, "FRT", cassLine.CASSCostCurrency, 100M, null, cassLine.CASSCostCurrency, 100M, null);
			TestObjectCreator.CreateCharge(Job2, fRT, "FRT", cassLine.CASSCostCurrency, 100M, null, cassLine.CASSCostCurrency, 100M, null);

			if (isGateway)
			{
				TestObjectCreator.CreateCharge(GatewayBillingJob, fRT, "FRT", cassLine.CASSCostCurrency, 50M, null, cassLine.CASSCostCurrency, 50M, null);
			}

			TestObjectCreator.SetExchangeRate(Job1, cassLine.CASSCostCurrency, 2M);

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			TestObjectCreator.GLHeader1.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			TestObjectCreator.GLHeader1.AG_Description = "Test Account";
			AccountingConfigurationRegistry.Instance.CASSGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			TestObjectCreator.SetupCreditorAirline(TestObjectCreator.CreateAirLine(cassLine.AirlinePrefix), TestObjectCreator.AALSHI);
		}

		ForwardingConsol Consol;
		ForwardingShipment Shipment1;
		ForwardingShipment Shipment2;
		Job Job1;
		Job Job2;
		Job GatewayBillingJob;

		#endregion

		public void TestHOTFileName()
		{
			CASSBilling.CostHeader.HOTFileName = Path.Combine(TempForTest.TempPath, "MyFile.hot");
			DocCASSBilling wrapper = DocCASSBilling.New(CASSBilling, Factory);
			AssertEquals("HOTFileName should return file name without path", "MyFile.hot", wrapper.HOTFileName);
		}

		public void TestBillingPeriodStart()
		{
			CASSBilling.CostHeader.DatePeriodStart = ZDateTime.BrettsBirthday;
			DocCASSBilling wrapper = DocCASSBilling.New(CASSBilling, Factory);
			AssertEquals("BillingPeriodStart should return the same value", ZDateTime.BrettsBirthday, wrapper.BillingPeriodStart);
		}

		public void TestBillingPeriodEnd()
		{
			CASSBilling.CostHeader.DatePeriodEnd = ZDateTime.BrettsBirthday;
			DocCASSBilling wrapper = DocCASSBilling.New(CASSBilling, Factory);
			AssertEquals("BillingPeriodEnd should return the same value", ZDateTime.BrettsBirthday, wrapper.BillingPeriodEnd);
		}

		public void TestBillingDate()
		{
			CASSBilling.CostHeader.DateOfBilling = ZDateTime.BrettsBirthday;
			DocCASSBilling wrapper = DocCASSBilling.New(CASSBilling, Factory);
			AssertEquals("BillingDate should return the same value", ZDateTime.BrettsBirthday, wrapper.BillingDate);
		}

		public void TestLines()
		{
			CASSBillingLine line1 = new CASSBillingLine(Factory);
			CASSBillingLine line2 = new CASSBillingLine(Factory);
			CASSBilling.Lines.Add(line1);
			CASSBilling.Lines.Add(line2);
			DocCASSBilling wrapper = DocCASSBilling.New(CASSBilling, Factory);
			AssertEquals("Lines count should be 2", 2, wrapper.Lines.Count);
		}

		public void TestHiddenLines()
		{
			CASSBillingLine line1 = new CASSBillingLine(Factory);
			CASSBillingLine line2 = new CASSBillingLine(Factory);
			CASSBilling.HiddenLines.Add(line1);
			CASSBilling.HiddenLines.Add(line2);
			DocCASSBilling wrapper = DocCASSBilling.New(CASSBilling, Factory);
			AssertEquals("HiddenLines count should be 2", 2, wrapper.HiddenLines.Count);
		}

		public void TestTotalCASSCostValue()
		{
			CASSBillingLine line1 = new CASSBillingLine(Factory);
			var costLine = new CASSCostExportLine(Factory, CASSCostLineType.Default);
			costLine.WeightChargePP = 100.00m;
			line1.AddCostLine(costLine, "AUD");

			CASSBillingLine line2 = new CASSBillingLine(Factory);
			costLine = new CASSCostExportLine(Factory, CASSCostLineType.Default);
			costLine.WeightChargePP = 200.00m;
			line2.AddCostLine(costLine, "AUD");

			CASSBilling.Lines.Add(line1);
			CASSBilling.Lines.Add(line2);
			DocCASSBilling wrapper = DocCASSBilling.New(CASSBilling, Factory);
			AssertEquals("TotalCASSCostValue should be 300", 300m, wrapper.TotalCASSCostValue);
		}

		public void TestTotalsForGatewayConsol()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

				SetupAssociatedBizos(true);
				Factory.Save();

				var cassCostHeader = CASSBilling.CostHeader;
				cassCostHeader.HOTFileName = "TestCASS.hot";
				cassCostHeader.DatePeriodStart = new ZDateTime(2008, 08, 01);
				cassCostHeader.DatePeriodEnd = new ZDateTime(2008, 08, 31);
				cassCostHeader.DateOfBilling = new ZDateTime(2008, 09, 10);
				cassCostHeader.InitializeAsExportCASS();

				for (int i = 0; i < 2; i++)
				{
					var cassBillingLine = CASSBilling.Lines.AddNew();
					foreach (bool isAdjustment in new bool[] { false, true })
					{
						if (!(i == 0 && isAdjustment))
						{
							var costLine = new CASSCostExportLine(Factory, isAdjustment ? CASSCostLineType.Adjustment : CASSCostLineType.Billing);
							costLine.VATIndicator = "Y";
							costLine.AirlinePrefix = "172";
							costLine.AWBSerialNumber = "67828073";
							costLine.AgentCode = "23470/068-510";
							costLine.DateAWBExecution = new ZDateTime(2008, 06, 07);
							costLine.DateOfArrival = new ZDateTime(2008, 07, 07);
							costLine.DateOfDelivery = new ZDateTime(2008, 08, 07);
							costLine.Origin = "LEJ";
							costLine.Destination = "MEX";
							costLine.Weight = 2150M;
							costLine.WeightUnit = "KG";
							costLine.CurrencyCode = "ERN";
							costLine.WeightChargePP = isAdjustment ? 1015.68M : (i == 1 ? 900.00m : 680.18M);
							costLine.VATDueAirline = isAdjustment ? 101.56M : 68.01M;
							cassBillingLine.AddCostLine(costLine, "ERN");
						}
					}
				}

				AssertEquals("Precondition:", 2, CASSBilling.Lines.Count);

				AccountingConfigurationRegistry.Instance.CASSCostImportAllowedDiscrepancy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 400);
				AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
				CASSBilling.ForceRecalculateData();

				AssertEquals(1, CASSBilling.Lines.Count);

				DocCASSBilling wrapper = DocCASSBilling.New(CASSBilling, Factory);

				AssertEquals(0M, wrapper.TotalCASSCostAdjustedValue);
				AssertEquals(680.18M, wrapper.TotalCASSCostValue);
				AssertEquals(0M, wrapper.TotalCASSRejectedClaimValue);
				AssertEquals(-630.18M, wrapper.TotalCostDifferenceValue);
				AssertEquals(680.18M, wrapper.TotalNetCASSCostValue);
				AssertEquals(50M, wrapper.TotalSystemCostAccrualValue);

				AssertEquals(-1015.68M, wrapper.TotalHiddenCASSCostAdjustedValue);
				AssertEquals(900M, wrapper.TotalHiddenCASSCostValue);
				AssertEquals(0M, wrapper.TotalHiddenCASSRejectedClaimValue);
				AssertEquals(165.68M, wrapper.TotalHiddenCostDifferenceValue);
				AssertEquals(-115.68M, wrapper.TotalHiddenNetCASSCostValue);
				AssertEquals(50M, wrapper.TotalHiddenSystemCostAccrualValue);

				AssertEquals(-1015.68M, wrapper.TotalAllCASSCostAdjustedValue);
				AssertEquals(1580.18M, wrapper.TotalAllCASSCostValue);
				AssertEquals(0M, wrapper.TotalAllCASSRejectedClaimValue);
				AssertEquals(-464.5M, wrapper.TotalAllCostDifferenceValue);
				AssertEquals(564.5M, wrapper.TotalAllNetCASSCostValue);
				AssertEquals(100M, wrapper.TotalAllSystemCostAccrualValue);

				var costLine1 = new CASSCostExportLine(Factory, CASSCostLineType.Rejected);
				costLine1.VATIndicator = "Y";
				costLine1.AirlinePrefix = "172";
				costLine1.AWBSerialNumber = "67828073";
				costLine1.AgentCode = "23470/068-510";
				costLine1.DateAWBExecution = new ZDateTime(2008, 06, 07);
				costLine1.DateOfArrival = new ZDateTime(2008, 07, 07);
				costLine1.DateOfDelivery = new ZDateTime(2008, 08, 07);
				costLine1.Origin = "LEJ";
				costLine1.Destination = "MEX";
				costLine1.Weight = 2150M;
				costLine1.WeightUnit = "KG";
				costLine1.CurrencyCode = "ERN";
				costLine1.WeightChargePP = 0M;
				costLine1.VATDueAirline = 0M;
				CASSBilling.Lines[0].AddCostLine(costLine1, "ERN");

				CASSBilling.ForceRecalculateData();

				AssertEquals(0M, wrapper.TotalCASSCostAdjustedValue);
				AssertEquals(0M, wrapper.TotalCASSCostValue);
				AssertEquals(680.18M, wrapper.TotalCASSRejectedClaimValue);
				AssertEquals(-630.18M, wrapper.TotalCostDifferenceValue);
				AssertEquals(680.18M, wrapper.TotalNetCASSCostValue);
				AssertEquals(50M, wrapper.TotalSystemCostAccrualValue);

				AssertEquals(-1015.68M, wrapper.TotalHiddenCASSCostAdjustedValue);
				AssertEquals(900M, wrapper.TotalHiddenCASSCostValue);
				AssertEquals(0M, wrapper.TotalHiddenCASSRejectedClaimValue);
				AssertEquals(165.68M, wrapper.TotalHiddenCostDifferenceValue);
				AssertEquals(-115.68M, wrapper.TotalHiddenNetCASSCostValue);
				AssertEquals(50M, wrapper.TotalHiddenSystemCostAccrualValue);

				AssertEquals(-1015.68M, wrapper.TotalAllCASSCostAdjustedValue);
				AssertEquals(900M, wrapper.TotalAllCASSCostValue);
				AssertEquals(680.18M, wrapper.TotalAllCASSRejectedClaimValue);
				AssertEquals(-464.5M, wrapper.TotalAllCostDifferenceValue);
				AssertEquals(564.5M, wrapper.TotalAllNetCASSCostValue);
				AssertEquals(100M, wrapper.TotalAllSystemCostAccrualValue);

				costLine1 = new CASSCostExportLine(Factory, CASSCostLineType.Rejected);
				costLine1.VATIndicator = "Y";
				costLine1.AirlinePrefix = "172";
				costLine1.AWBSerialNumber = "67828073";
				costLine1.AgentCode = "23470/068-510";
				costLine1.DateAWBExecution = new ZDateTime(2008, 06, 07);
				costLine1.DateOfArrival = new ZDateTime(2008, 07, 07);
				costLine1.DateOfDelivery = new ZDateTime(2008, 08, 07);
				costLine1.Origin = "LEJ";
				costLine1.Destination = "MEX";
				costLine1.Weight = 2150M;
				costLine1.WeightUnit = "KG";
				costLine1.CurrencyCode = "ERN";
				costLine1.WeightChargePP = 0M;
				costLine1.VATDueAirline = 0M;
				CASSBilling.HiddenLines[0].AddCostLine(costLine1, CASSBilling.Lines[0].CASSCostCurrencyCode);
				CASSBilling.ForceRecalculateData();

				AssertEquals(0M, wrapper.TotalCASSCostAdjustedValue);
				AssertEquals(0M, wrapper.TotalCASSCostValue);
				AssertEquals(680.18M, wrapper.TotalCASSRejectedClaimValue);
				AssertEquals(-630.18M, wrapper.TotalCostDifferenceValue);
				AssertEquals(680.18M, wrapper.TotalNetCASSCostValue);
				AssertEquals(50M, wrapper.TotalSystemCostAccrualValue);

				AssertEquals(-1015.68M, wrapper.TotalHiddenCASSCostAdjustedValue);
				AssertEquals(0M, wrapper.TotalHiddenCASSCostValue);
				AssertEquals(900M, wrapper.TotalHiddenCASSRejectedClaimValue);
				AssertEquals(165.68M, wrapper.TotalHiddenCostDifferenceValue);
				AssertEquals(-115.68M, wrapper.TotalHiddenNetCASSCostValue);
				AssertEquals(50M, wrapper.TotalHiddenSystemCostAccrualValue);

				AssertEquals(-1015.68M, wrapper.TotalAllCASSCostAdjustedValue);
				AssertEquals(0M, wrapper.TotalAllCASSCostValue);
				AssertEquals(1580.18M, wrapper.TotalAllCASSRejectedClaimValue);
				AssertEquals(-464.5M, wrapper.TotalAllCostDifferenceValue);
				AssertEquals(564.5M, wrapper.TotalAllNetCASSCostValue);
				AssertEquals(100M, wrapper.TotalAllSystemCostAccrualValue);
			}
		}

		public void TestTotals()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

				SetupAssociatedBizos(false);
				Factory.Save();

				var cassCostHeader = CASSBilling.CostHeader;
				cassCostHeader.HOTFileName = "TestCASS.hot";
				cassCostHeader.DatePeriodStart = new ZDateTime(2008, 08, 01);
				cassCostHeader.DatePeriodEnd = new ZDateTime(2008, 08, 31);
				cassCostHeader.DateOfBilling = new ZDateTime(2008, 09, 10);
				cassCostHeader.InitializeAsExportCASS();

				for (int i = 0; i < 2; i++)
				{
					var cassBillingLine = CASSBilling.Lines.AddNew();
					foreach (bool isAdjustment in new bool[] { false, true })
					{
						if (!(i == 0 && isAdjustment))
						{
							var costLine = new CASSCostExportLine(Factory, isAdjustment ? CASSCostLineType.Adjustment : CASSCostLineType.Billing);
							costLine.VATIndicator = "Y";
							costLine.AirlinePrefix = "172";
							costLine.AWBSerialNumber = "67828073";
							costLine.AgentCode = "23470/068-510";
							costLine.DateAWBExecution = new ZDateTime(2008, 06, 07);
							costLine.DateOfArrival = new ZDateTime(2008, 07, 07);
							costLine.DateOfDelivery = new ZDateTime(2008, 08, 07);
							costLine.Origin = "LEJ";
							costLine.Destination = "MEX";
							costLine.Weight = 2150M;
							costLine.WeightUnit = "KG";
							costLine.CurrencyCode = "ERN";
							costLine.WeightChargePP = isAdjustment ? 1015.68M : (i == 1 ? 900.00m : 680.18M);
							costLine.VATDueAirline = isAdjustment ? 101.56M : 68.01M;
							cassBillingLine.AddCostLine(costLine, "ERN");
						}
					}
				}

				AssertEquals("Precondition:", 2, CASSBilling.Lines.Count);

				AccountingConfigurationRegistry.Instance.CASSCostImportAllowedDiscrepancy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 400);
				AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
				CASSBilling.ForceRecalculateData();

				AssertEquals(1, CASSBilling.Lines.Count);

				DocCASSBilling wrapper = DocCASSBilling.New(CASSBilling, Factory);

				AssertEquals(0M, wrapper.TotalCASSCostAdjustedValue);
				AssertEquals(680.18M, wrapper.TotalCASSCostValue);
				AssertEquals(0M, wrapper.TotalCASSRejectedClaimValue);
				AssertEquals(-480.18M, wrapper.TotalCostDifferenceValue);
				AssertEquals(680.18M, wrapper.TotalNetCASSCostValue);
				AssertEquals(200M, wrapper.TotalSystemCostAccrualValue);

				AssertEquals(-1015.68M, wrapper.TotalHiddenCASSCostAdjustedValue);
				AssertEquals(900M, wrapper.TotalHiddenCASSCostValue);
				AssertEquals(0M, wrapper.TotalHiddenCASSRejectedClaimValue);
				AssertEquals(315.68M, wrapper.TotalHiddenCostDifferenceValue);
				AssertEquals(-115.68M, wrapper.TotalHiddenNetCASSCostValue);
				AssertEquals(200M, wrapper.TotalHiddenSystemCostAccrualValue);

				AssertEquals(-1015.68M, wrapper.TotalAllCASSCostAdjustedValue);
				AssertEquals(1580.18M, wrapper.TotalAllCASSCostValue);
				AssertEquals(0M, wrapper.TotalAllCASSRejectedClaimValue);
				AssertEquals(-164.5M, wrapper.TotalAllCostDifferenceValue);
				AssertEquals(564.5M, wrapper.TotalAllNetCASSCostValue);
				AssertEquals(400M, wrapper.TotalAllSystemCostAccrualValue);

				var costLine1 = new CASSCostExportLine(Factory, CASSCostLineType.Rejected);
				costLine1.VATIndicator = "Y";
				costLine1.AirlinePrefix = "172";
				costLine1.AWBSerialNumber = "67828073";
				costLine1.AgentCode = "23470/068-510";
				costLine1.DateAWBExecution = new ZDateTime(2008, 06, 07);
				costLine1.DateOfArrival = new ZDateTime(2008, 07, 07);
				costLine1.DateOfDelivery = new ZDateTime(2008, 08, 07);
				costLine1.Origin = "LEJ";
				costLine1.Destination = "MEX";
				costLine1.Weight = 2150M;
				costLine1.WeightUnit = "KG";
				costLine1.CurrencyCode = "ERN";
				costLine1.WeightChargePP = 0M;
				costLine1.VATDueAirline = 0M;
				CASSBilling.Lines[0].AddCostLine(costLine1, "ERN");

				CASSBilling.ForceRecalculateData();

				AssertEquals(0M, wrapper.TotalCASSCostAdjustedValue);
				AssertEquals(0M, wrapper.TotalCASSCostValue);
				AssertEquals(680.18M, wrapper.TotalCASSRejectedClaimValue);
				AssertEquals(-480.18M, wrapper.TotalCostDifferenceValue);
				AssertEquals(680.18M, wrapper.TotalNetCASSCostValue);
				AssertEquals(200M, wrapper.TotalSystemCostAccrualValue);

				AssertEquals(-1015.68M, wrapper.TotalHiddenCASSCostAdjustedValue);
				AssertEquals(900M, wrapper.TotalHiddenCASSCostValue);
				AssertEquals(0M, wrapper.TotalHiddenCASSRejectedClaimValue);
				AssertEquals(315.68M, wrapper.TotalHiddenCostDifferenceValue);
				AssertEquals(-115.68M, wrapper.TotalHiddenNetCASSCostValue);
				AssertEquals(200M, wrapper.TotalHiddenSystemCostAccrualValue);

				AssertEquals(-1015.68M, wrapper.TotalAllCASSCostAdjustedValue);
				AssertEquals(900M, wrapper.TotalAllCASSCostValue);
				AssertEquals(680.18M, wrapper.TotalAllCASSRejectedClaimValue);
				AssertEquals(-164.5M, wrapper.TotalAllCostDifferenceValue);
				AssertEquals(564.5M, wrapper.TotalAllNetCASSCostValue);
				AssertEquals(400M, wrapper.TotalAllSystemCostAccrualValue);

				costLine1 = new CASSCostExportLine(Factory, CASSCostLineType.Rejected);
				costLine1.VATIndicator = "Y";
				costLine1.AirlinePrefix = "172";
				costLine1.AWBSerialNumber = "67828073";
				costLine1.AgentCode = "23470/068-510";
				costLine1.DateAWBExecution = new ZDateTime(2008, 06, 07);
				costLine1.DateOfArrival = new ZDateTime(2008, 07, 07);
				costLine1.DateOfDelivery = new ZDateTime(2008, 08, 07);
				costLine1.Origin = "LEJ";
				costLine1.Destination = "MEX";
				costLine1.Weight = 2150M;
				costLine1.WeightUnit = "KG";
				costLine1.CurrencyCode = "ERN";
				costLine1.WeightChargePP = 0M;
				costLine1.VATDueAirline = 0M;
				CASSBilling.HiddenLines[0].AddCostLine(costLine1, CASSBilling.Lines[0].CASSCostCurrencyCode);
				CASSBilling.ForceRecalculateData();

				AssertEquals(0M, wrapper.TotalCASSCostAdjustedValue);
				AssertEquals(0M, wrapper.TotalCASSCostValue);
				AssertEquals(680.18M, wrapper.TotalCASSRejectedClaimValue);
				AssertEquals(-480.18M, wrapper.TotalCostDifferenceValue);
				AssertEquals(680.18M, wrapper.TotalNetCASSCostValue);
				AssertEquals(200M, wrapper.TotalSystemCostAccrualValue);

				AssertEquals(-1015.68M, wrapper.TotalHiddenCASSCostAdjustedValue);
				AssertEquals(0M, wrapper.TotalHiddenCASSCostValue);
				AssertEquals(900M, wrapper.TotalHiddenCASSRejectedClaimValue);
				AssertEquals(315.68M, wrapper.TotalHiddenCostDifferenceValue);
				AssertEquals(-115.68M, wrapper.TotalHiddenNetCASSCostValue);
				AssertEquals(200M, wrapper.TotalHiddenSystemCostAccrualValue);

				AssertEquals(-1015.68M, wrapper.TotalAllCASSCostAdjustedValue);
				AssertEquals(0M, wrapper.TotalAllCASSCostValue);
				AssertEquals(1580.18M, wrapper.TotalAllCASSRejectedClaimValue);
				AssertEquals(-164.5M, wrapper.TotalAllCostDifferenceValue);
				AssertEquals(564.5M, wrapper.TotalAllNetCASSCostValue);
				AssertEquals(400M, wrapper.TotalAllSystemCostAccrualValue);
			}
		}

		public void TestTotalCostDifferenceMargin()
		{
			DocCASSBilling wrapper = DocCASSBilling.New(CASSBilling, Factory);
			AssertEquals("TotalCostDifferenceMargin should return 100%", "100%", wrapper.TotalCostDifferenceMargin);
		}

		public void TestTotalHiddenCostDifferenceMargin()
		{
			DocCASSBilling wrapper = DocCASSBilling.New(CASSBilling, Factory);
			AssertEquals("TotalHiddenCostDifferenceMargin should return 100%", "100%", wrapper.TotalHiddenCostDifferenceMargin);
		}

		public void TestTotalAllCostDifferenceMargin()
		{
			DocCASSBilling wrapper = DocCASSBilling.New(CASSBilling, Factory);
			AssertEquals("TotalAllCostDifferenceMargin should return 100%", "100%", wrapper.TotalAllCostDifferenceMargin);
		}

		public void TestIsRejectedClaimLinesExpected()
		{
			DocCASSBilling wrapper = DocCASSBilling.New(CASSBilling, Factory);
			AssertEquals("IsRejectedClaimLinesExpected", false, wrapper.IsRejectedClaimLinesExpected);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("IsRejectedClaimLinesExpected", true, wrapper.IsRejectedClaimLinesExpected);
		}

		public void TestIsImportBilling()
		{
			DocCASSBilling wrapper = DocCASSBilling.New(CASSBilling, Factory);
			CASSBilling.CostHeader.InitializeAsExportCASS();
			AssertEquals("IsRejectedClaimLinesExpected", false, wrapper.IsImportBilling);

			CASSBilling.CostHeader.InitializeAsImportCASS();
			AssertEquals("IsRejectedClaimLinesExpected", true, wrapper.IsImportBilling);
		}

		public void TestIsExportBilling()
		{
			DocCASSBilling wrapper = DocCASSBilling.New(CASSBilling, Factory);
			CASSBilling.CostHeader.InitializeAsImportCASS();
			AssertEquals("IsRejectedClaimLinesExpected", false, wrapper.IsExportBilling);

			CASSBilling.CostHeader.InitializeAsExportCASS();
			AssertEquals("IsRejectedClaimLinesExpected", true, wrapper.IsExportBilling);
		}
	}
}
