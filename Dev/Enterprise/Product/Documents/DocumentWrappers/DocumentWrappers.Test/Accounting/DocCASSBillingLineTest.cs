using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocCASSBillingLine))]
	sealed class DocCASSBillingLineTest : DocumentWrapperTestCase
	{
		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocCASSBillingLine.New(cassBillingLine, Factory)
			};
		}

		CASSBillingLine cassBillingLine;
		protected override void SetUp()
		{
			cassBillingLine = new CASSBillingLine(Factory);
			cassBillingLine.AddCostLine(new CASSCostExportLine(Factory, CASSCostLineType.Default), "AUD");
			base.SetUp();
		}

		ForwardingConsol Consol;
		ForwardingShipment Shipment1;
		ForwardingShipment Shipment2;
		Job Job1;
		Job Job2;
		Job GatewayBillingJob;
		AccChargeCode FRT;

		void SetupAssociatedBizos(CASSBillingLine cassBillingLine, bool isGateway)
		{
			TestObjectCreator.SetupCASSBillingLine(cassBillingLine, currencyCode: "ERN");
			cassBillingLine.ForceRecalculateData();
			Consol = isGateway
				? TestObjectCreator.CreateGatewayConsol(cassBillingLine.LoadPort, cassBillingLine.DischargePort, "C0001", receivingGatewayCompany: GlbCompany.CurrentCompany, agentStatus: AgentStatusList.Codes.GatewayAgentWithTariff)
				: TestObjectCreator.CreateConsol(cassBillingLine.LoadPort, cassBillingLine.DischargePort, "C0001");

			Consol.JK_MasterBillNum = cassBillingLine.MAWBNumber;

			Shipment1 = TestObjectCreator.CreateShipment("SHIP1", cassBillingLine.LoadPort, cassBillingLine.DischargePort, Consol);
			Shipment2 = TestObjectCreator.CreateShipment("SHIP2", cassBillingLine.LoadPort, cassBillingLine.DischargePort, Consol);
			Job1 = TestObjectCreator.CreateJob(Shipment1);
			Job2 = TestObjectCreator.CreateJob(Shipment2);
			if (isGateway)
			{
				GatewayBillingJob = TestObjectCreator.CreateJob(Consol, false);
			}

			ZQuery fRTFilter = new ZQuery(AccChargeCodeSchema.AC_Code, "FRT");
			fRTFilter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			FRT = Factory.LoadTop1<AccChargeCode>(fRTFilter);
			TestObjectCreator.CreateCharge(Job1, FRT, "FRT", cassBillingLine.CASSCostCurrency, 100M, null, cassBillingLine.CASSCostCurrency, 100M, null);
			TestObjectCreator.CreateCharge(Job2, FRT, "FRT", cassBillingLine.CASSCostCurrency, 100M, null, cassBillingLine.CASSCostCurrency, 100M, null);
			if (isGateway)
			{
				TestObjectCreator.CreateCharge(GatewayBillingJob, FRT, "FRT", cassBillingLine.CASSCostCurrency, 50M, null, cassBillingLine.CASSCostCurrency, 50M, null);
			}

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			TestObjectCreator.GLHeader1.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			TestObjectCreator.GLHeader1.AG_Description = "Test Account";
			AccountingConfigurationRegistry.Instance.CASSGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			Factory.Save();
		}

		#endregion

		public void TestWeightDifferenceMargin()
		{
			DocCASSBillingLine wrapper = DocCASSBillingLine.New(cassBillingLine, Factory);
			AssertEquals("WeightDifferenceMargin should return 100%", "100%", wrapper.WeightDifferenceMargin);
		}

		public void TestCostDifferenceMargin()
		{
			DocCASSBillingLine wrapper = DocCASSBillingLine.New(cassBillingLine, Factory);
			AssertEquals("CostDifferenceMargin should return 100%", "100%", wrapper.CostDifferenceMargin);
		}

		public void TestNetCASSCost()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				SetupAssociatedBizos(cassBillingLine, false);
				Factory.Save();

				DocCASSBillingLine wrapper = DocCASSBillingLine.New(cassBillingLine, Factory);
				AssertEquals(-335.5M, wrapper.NetCASSCost);
			}
		}

		public void TestNetCASSCostForGatewyConsol()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				SetupAssociatedBizos(cassBillingLine, true);
				Factory.Save();

				DocCASSBillingLine wrapper = DocCASSBillingLine.New(cassBillingLine, Factory);
				AssertEquals(-335.5M, wrapper.NetCASSCost);
			}
		}

		public void TestCASSCostAdjustedValue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				SetupAssociatedBizos(cassBillingLine, false);
				Factory.Save();

				DocCASSBillingLine wrapper = DocCASSBillingLine.New(cassBillingLine, Factory);
				AssertEquals(-1015.68M, wrapper.CASSCostAdjustedValue);
			}
		}

		public void TestCASSCostAdjustedValueForGatewyConsol()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				SetupAssociatedBizos(cassBillingLine, true);
				Factory.Save();

				DocCASSBillingLine wrapper = DocCASSBillingLine.New(cassBillingLine, Factory);
				AssertEquals(-1015.68M, wrapper.CASSCostAdjustedValue);
			}
		}

		public void TestCASSCostAndRejectedClaimValues()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				TestObjectCreator.SetupCASSBillingLine(cassBillingLine, currencyCode: "ERN");
				cassBillingLine.ForceRecalculateData();

				DocCASSBillingLine wrapper = DocCASSBillingLine.New(cassBillingLine, Factory);
				AssertEquals("Precondition: IsRejectedClaimLinesExpected", false, cassBillingLine.IsRejectedClaimLine);

				AssertEquals("CASSCostValue", 680.18M, wrapper.CASSCostValue);
				AssertEquals("CASSRejectedClaimValue", 0M, wrapper.CASSRejectedClaimValue);

				cassBillingLine = new CASSBillingLine(Factory);
				TestObjectCreator.SetupCASSBillingLine(cassBillingLine, currencyCode: "ERN", setAdjustmentValues: false, isRejectedRecordType: true);
				cassBillingLine.ForceRecalculateData();

				wrapper = DocCASSBillingLine.New(cassBillingLine, Factory);
				AssertEquals("CASSCostValue", 0M, wrapper.CASSCostValue);
				AssertEquals("CASSRejectedClaimValue", 680.18M, wrapper.CASSRejectedClaimValue);
			}
		}

		public void TestSystemCostPostedValue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				SetupAssociatedBizos(cassBillingLine, false);
				TestObjectCreator.SetupCreditorAirline(TestObjectCreator.CreateAirLine(cassBillingLine.AirlinePrefix), TestObjectCreator.AALSHI);
				Factory.Save();
				cassBillingLine.ForceRecalculateData();
				APInvoice apInvoice = Factory.New<APInvoice>();
				apInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
				apInvoice.AH_TransactionNum = "ADAD";
				apInvoice.SubmittedFromInvoicingForm = true;
				APInvoiceLine apLine = (APInvoiceLine)apInvoice.Lines.AddNew();
				apLine.GenericCharge = FRT.PK;
				apLine.AL_JH = Job1.PK;
				apLine.AL_OSExTaxAmount = 100M;
				Factory.Save();
				DocCASSBillingLine wrapper = DocCASSBillingLine.New(cassBillingLine, Factory);
				AssertEquals(100M, wrapper.SystemCostPostedValue);
			}
		}

		public void TestSystemCostPostedValueForGatewyConsol()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				SetupAssociatedBizos(cassBillingLine, true);
				TestObjectCreator.SetupCreditorAirline(TestObjectCreator.CreateAirLine(cassBillingLine.AirlinePrefix), TestObjectCreator.AALSHI);
				Factory.Save();
				cassBillingLine.ForceRecalculateData();
				APInvoice apInvoice = Factory.New<APInvoice>();
				apInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
				apInvoice.AH_TransactionNum = "ADAD";
				apInvoice.SubmittedFromInvoicingForm = true;
				APInvoiceLine apLine = (APInvoiceLine)apInvoice.Lines.AddNew();
				apLine.GenericCharge = FRT.PK;
				apLine.AL_JH = GatewayBillingJob.PK;
				apLine.AL_OSExTaxAmount = 100M;
				Factory.Save();
				DocCASSBillingLine wrapper = DocCASSBillingLine.New(cassBillingLine, Factory);
				AssertEquals(100M, wrapper.SystemCostPostedValue);
			}
		}

		public void TestAdjustment()
		{
			DocCASSBillingLine wrapper = DocCASSBillingLine.New(cassBillingLine, Factory);
			AssertEquals("Adjustment should return Empty string", "", wrapper.Adjustment);

			var costLine = new CASSCostExportLine(Factory, CASSCostLineType.Adjustment);
			costLine.WeightChargePP = 500M;
			cassBillingLine.AddCostLine(costLine, "AUD");

			Assert(cassBillingLine.IsCASSAmendment);
			AssertEquals("Adjustment should return 'Amended' string", "Amended", wrapper.Adjustment);
		}

		public void TestDateOfArrival()
		{
			var costLine = new CASSCostExportLine(Factory, CASSCostLineType.Adjustment);
			costLine.DateOfArrival = ZDateTime.BrettsBirthday;
			cassBillingLine.AddCostLine(costLine, "AUD");

			DocCASSBillingLine wrapper = DocCASSBillingLine.New(cassBillingLine, Factory);
			AssertEquals(ZDateTime.BrettsBirthday, wrapper.DateOfArrival);

			costLine = new CASSCostExportLine(Factory, CASSCostLineType.Adjustment);
			costLine.DateOfArrival = ZDateTime.Empty;
			cassBillingLine.AddCostLine(costLine, "AUD");

			AssertEquals(ZDateTime.Empty, wrapper.DateOfArrival);
		}

		public void TestDateOfDelivery()
		{
			var costLine = new CASSCostExportLine(Factory, CASSCostLineType.Adjustment);
			costLine.DateOfDelivery = ZDateTime.BrettsBirthday;
			cassBillingLine.AddCostLine(costLine, "AUD");

			DocCASSBillingLine wrapper = DocCASSBillingLine.New(cassBillingLine, Factory);
			AssertEquals(ZDateTime.BrettsBirthday, wrapper.DateOfDelivery);

			costLine = new CASSCostExportLine(Factory, CASSCostLineType.Adjustment);
			costLine.DateOfDelivery = ZDateTime.Empty;
			cassBillingLine.AddCostLine(costLine, "AUD");

			AssertEquals(ZDateTime.Empty, wrapper.DateOfDelivery);
		}
	}
}
