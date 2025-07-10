using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	public class ProfitShareMasterFreightInvoiceNegator_InnerTest : TestCaseWithFactory
	{
		public void TestNegateInvoicesWhenAgentBeingIssuedCreditNote()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultReceivingForwarderAddress(creator.Agent);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			freightCost.E6_RX_NKCurrency = creator.USD.RX_Code;
			freightCost.E6_ExchangeRate = 0.7381m;

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			Job shipment1Job = Job.CreateWithMutex(Factory, shipment1);
			shipment1Job.PlugInData = shipment1;
			Job shipment2Job = Job.CreateWithMutex(Factory, shipment2);
			shipment2Job.PlugInData = shipment2;
			shipment1Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			shipment2Job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			shipment1Job.Dispose();
			shipment2Job.Dispose();

			Factory.Save();

			ZQuery taxFilter = new ZQuery(AccTaxRateSchema.AT_Type, AccTaxRate.Types.Rated);
			taxFilter.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, Env.CurrentCompany.Country.Code);
			taxFilter.AddToFilter(AccTaxRateSchema.AT_IsActive, true);
			var taxRate = Factory.Load<AccTaxRate>(taxFilter).First(x => x.GetRate_ForTestOnly() == 0);
			AssertNotNull("Pre-condition: Should be a FREEGSTTax Rate", taxRate);

			APInvoice profitShareAndMasterFreightInvoice = Factory.New<APInvoice>();
			profitShareAndMasterFreightInvoice.AH_OH = ((IJobCostingPlugIn)consol).ReceivingAgentAPInvoicingParty.PK;
			APInvoiceLine aPLine1 = (APInvoiceLine)profitShareAndMasterFreightInvoice.Lines.AddNew();
			aPLine1.GenericCharge = Env.Registry.FreightChargeCode;
			aPLine1.AL_JH = shipment1Job.PK;
			aPLine1.AL_OSExTaxAmount = 200m;
			aPLine1.AL_AT = taxRate.PK;

			APInvoiceLine aPLine2 = (APInvoiceLine)profitShareAndMasterFreightInvoice.Lines.AddNew();
			aPLine2.AL_JH = shipment2Job.PK;
			aPLine2.GenericCharge = AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value;
			aPLine2.AL_OSExTaxAmount = 250m;
			aPLine2.AL_AT = taxRate.PK;

			ARCreditNote agentInvoice = Factory.New<ARCreditNote>();
			agentInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			agentInvoice.AH_OH = ((IJobCostingPlugIn)consol).ReceivingAgentAPInvoicingParty.PK;
			ARCreditNoteLine aRLine1 = (ARCreditNoteLine)agentInvoice.Lines.AddNew();
			aRLine1.AL_JH = shipment1Job.PK;
			aRLine1.GenericCharge = creator.CC1.PK;
			aRLine1.AL_OSExTaxAmount = 150m;

			ChargePoster poster = new ChargePoster(Factory);
			poster.PostedInvoices.Add(agentInvoice);
			AgentChargePostingDetails postingDetails = new AgentChargePostingDetails(consol, null, creator.USD, 0.7381m, poster, apps.CostsCollection);
			postingDetails.AgentInvoices.Add(profitShareAndMasterFreightInvoice);

			var transactions = new TransactionCreatorHashtable();
			ProfitShareMasterFreightInvoiceNegator negator = new ProfitShareMasterFreightInvoiceNegator(Factory, postingDetails);
			negator.NegateInvoices(transactions);

			Assert("Agent Invoice should stay a credit note", agentInvoice.AH_TransactionType == ZArchitecture.Core.TransactionTypes.CreditNote);

			AssertEquals("Should be 3 lines on the AR Invoice", 3, agentInvoice.Lines.Count);
			AssertEquals("Should be 4 lines on the AP Invoice", 4, profitShareAndMasterFreightInvoice.Lines.Count);

			AssertEquals("AP Invoice should be for zero value", 0m, profitShareAndMasterFreightInvoice.AH_OSExTaxAmount);
			AssertEquals("Line 3 should be the inverse amount of line 1", -profitShareAndMasterFreightInvoice.Lines[0].AL_OSExTaxAmount, profitShareAndMasterFreightInvoice.Lines[2].AL_OSExTaxAmount);
			AssertEquals("Line 3 should be the same Tax Rate as line 1", profitShareAndMasterFreightInvoice.Lines[0].AL_AT, profitShareAndMasterFreightInvoice.Lines[2].AL_AT);
			AssertEquals("Line 4 should be the inverse amount of line 2", -profitShareAndMasterFreightInvoice.Lines[1].AL_OSExTaxAmount, profitShareAndMasterFreightInvoice.Lines[3].AL_OSExTaxAmount);
			AssertEquals("Line 4 should be the same Tax Rate as line 2", profitShareAndMasterFreightInvoice.Lines[1].AL_AT, profitShareAndMasterFreightInvoice.Lines[3].AL_AT);

			AssertEquals("Amount on AR Invoice line 2 should be the negative of AP INovice line 3", -profitShareAndMasterFreightInvoice.Lines[2].AL_OSExTaxAmount, agentInvoice.Lines[1].AL_OSExTaxAmount);
			AssertEquals("AR Invoice line 2 should have the same Tax Rate as AP Invoice line 1", profitShareAndMasterFreightInvoice.Lines[0].AL_AT, agentInvoice.Lines[1].AL_AT);
			AssertEquals("Amount on AR Invoice line 3 should be the negative of AP INovice line 4", -profitShareAndMasterFreightInvoice.Lines[3].AL_OSExTaxAmount, agentInvoice.Lines[2].AL_OSExTaxAmount);
			AssertEquals("AR Invoice line 3 should have the same Tax Rate as AP Invoice line 1", profitShareAndMasterFreightInvoice.Lines[0].AL_AT, agentInvoice.Lines[2].AL_AT);
		}

		public void TestNegateInvoices()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			creator.MRG100.AC_AG_AccrualAccount = creator.CreateGLHeader().PK;
			creator.MRG100.AC_AG_WIPAccount = creator.CreateGLHeader().PK;
			creator.MRG100.AC_AG_RevenueAccount = creator.CreateGLHeader().PK;
			creator.MRG100.AC_AG_CostAccount = creator.CreateGLHeader().PK;
			creator.DSBChargeCode.AC_AG_AccrualAccount = creator.CreateGLHeader().PK;
			creator.DSBChargeCode.AC_AG_WIPAccount = creator.CreateGLHeader().PK;
			creator.DSBChargeCode.AC_AG_RevenueAccount = creator.CreateGLHeader().PK;
			creator.DSBChargeCode.AC_AG_CostAccount = creator.CreateGLHeader().PK;
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultReceivingForwarderAddress(creator.Agent);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			freightCost.E6_RX_NKCurrency = creator.USD.RX_Code;
			freightCost.E6_ExchangeRate = 0.7381m;
			freightCost.E6_ApportionmentMethod = "SHP";

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			Job shipment1Job = Job.CreateWithMutex(Factory, shipment1);
			shipment1Job.PlugInData = shipment1;
			Job shipment2Job = Job.CreateWithMutex(Factory, shipment2);
			shipment2Job.PlugInData = shipment2;
			shipment1Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			shipment2Job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			shipment1Job.Dispose();
			shipment2Job.Dispose();

			Factory.Save();

			ZQuery taxFilter = new ZQuery(AccTaxRateSchema.AT_Type, AccTaxRate.Types.Rated);
			taxFilter.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, Env.CurrentCompany.Country.Code);
			taxFilter.AddToFilter(AccTaxRateSchema.AT_IsActive, true);
			var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.SetRateNumerator_ForTestOnly(0);
			var invMsg = Factory.NewWithValidTestData<AccInvMsg>();
			taxRate.AT_A9_DefaultVatClass = invMsg.PK;

			var taxRate2 = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate2.SetRateNumerator_ForTestOnly(0);
			var invMsg2 = Factory.NewWithValidTestData<AccInvMsg>();
			taxRate2.AT_A9_DefaultVatClass = invMsg2.PK;

			APInvoice profitShareAndMasterFreightInvoice = Factory.New<APInvoice>();
			profitShareAndMasterFreightInvoice.AH_OH = ((IJobCostingPlugIn)consol).ReceivingAgentAPInvoicingParty.PK;
			APInvoiceLine aPLine1 = (APInvoiceLine)profitShareAndMasterFreightInvoice.Lines.AddNew();
			aPLine1.GenericCharge = creator.MRG100.PK;
			aPLine1.AL_JH = shipment1Job.PK;
			aPLine1.AL_OSExTaxAmount = 200m;
			aPLine1.AL_AT = taxRate.PK;
			aPLine1.AL_TaxDate = ZDate.Today.AddDays(-7);

			APInvoiceLine aPLine2 = (APInvoiceLine)profitShareAndMasterFreightInvoice.Lines.AddNew();
			aPLine2.AL_JH = shipment2Job.PK;
			aPLine2.GenericCharge = creator.MRG100.PK;
			aPLine2.AL_OSExTaxAmount = 250m;
			aPLine2.AL_AT = taxRate2.PK;
			aPLine2.AL_TaxDate = ZDate.Today.AddDays(-4);

			ARInvoice agentInvoice = Factory.New<ARInvoice>();
			agentInvoice.AH_OH = ((IJobCostingPlugIn)consol).ReceivingAgentAPInvoicingParty.PK;
			agentInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef + "/BA";
			ARInvoiceLine aRLine1 = (ARInvoiceLine)agentInvoice.Lines.AddNew();
			aRLine1.AL_JH = shipment1Job.PK;
			aRLine1.GenericCharge = creator.MRG100.PK;
			aRLine1.AL_OSExTaxAmount = 150m;

			ChargePoster poster = new ChargePoster(Factory);
			poster.PostedInvoices.Add(agentInvoice);
			AgentChargePostingDetails postingDetails = new AgentChargePostingDetails(consol, null, creator.USD, 0.7381m, poster, apps.CostsCollection);
			postingDetails.AgentInvoices.Add(profitShareAndMasterFreightInvoice);

			var transactions = new TransactionCreatorHashtable();
			ProfitShareMasterFreightInvoiceNegator negator = new ProfitShareMasterFreightInvoiceNegator(Factory, postingDetails);
			negator.NegateInvoices(transactions);

			AssertEquals("Should be 3 lines on the AR Invoice", 3, agentInvoice.Lines.Count);
			AssertEquals("Should be 4 lines on the AP Invoice", 4, profitShareAndMasterFreightInvoice.Lines.Count);

			AssertEquals("AP Invoice should be for zero value", 0m, profitShareAndMasterFreightInvoice.AH_OSExTaxAmount);
			AssertEquals("Line 3 should be the inverse amount of line 1", -profitShareAndMasterFreightInvoice.Lines[0].AL_OSExTaxAmount, profitShareAndMasterFreightInvoice.Lines[2].AL_OSExTaxAmount);
			AssertEquals("Line 3 should be the same Tax Rate as line 1", profitShareAndMasterFreightInvoice.Lines[0].AL_AT, profitShareAndMasterFreightInvoice.Lines[2].AL_AT);
			AssertEquals("Line 3 should be the same Tax Date as line 1", profitShareAndMasterFreightInvoice.Lines[0].AL_TaxDate, profitShareAndMasterFreightInvoice.Lines[2].AL_TaxDate);
			AssertEquals("Line 3 should be the same Tax Msg as line 1", profitShareAndMasterFreightInvoice.Lines[0].AL_A9_VATClass, profitShareAndMasterFreightInvoice.Lines[2].AL_A9_VATClass);
			AssertEquals("Line 4 should be the inverse amount of line 2", -profitShareAndMasterFreightInvoice.Lines[1].AL_OSExTaxAmount, profitShareAndMasterFreightInvoice.Lines[3].AL_OSExTaxAmount);
			AssertEquals("Line 4 should be the same Tax Rate as line 2", profitShareAndMasterFreightInvoice.Lines[1].AL_AT, profitShareAndMasterFreightInvoice.Lines[3].AL_AT);
			AssertEquals("Line 4 should be the same Tax Date as line 2", profitShareAndMasterFreightInvoice.Lines[1].AL_TaxDate, profitShareAndMasterFreightInvoice.Lines[3].AL_TaxDate);
			AssertEquals("Line 4 should be the same Tax Msg as line 2", profitShareAndMasterFreightInvoice.Lines[1].AL_A9_VATClass, profitShareAndMasterFreightInvoice.Lines[3].AL_A9_VATClass);

			AssertEquals("Line 3 description", profitShareAndMasterFreightInvoice.Lines[0].ChargeCode.AC_Desc +
																					" - Job Number: " +
																					profitShareAndMasterFreightInvoice.Lines[0].Job.JH_JobNum,
																					profitShareAndMasterFreightInvoice.Lines[2].AL_Desc);

			AssertEquals("Line 4 description", profitShareAndMasterFreightInvoice.Lines[1].ChargeCode.AC_Desc +
																					" - Job Number: " +
																					profitShareAndMasterFreightInvoice.Lines[1].Job.JH_JobNum,
																					profitShareAndMasterFreightInvoice.Lines[3].AL_Desc);

			AssertEquals("Amount on AR Invoice line 2 should be the same as on AP INovice line 3", profitShareAndMasterFreightInvoice.Lines[2].AL_OSExTaxAmount, agentInvoice.Lines[1].AL_OSExTaxAmount);
			AssertEquals("AR Invoice line 2 should have the same Tax Rate as AP Invoice line 1", profitShareAndMasterFreightInvoice.Lines[0].AL_AT, agentInvoice.Lines[1].AL_AT);
			AssertEquals("AR Invoice line 2 should have the same Tax Date as AP Invoice line 1", profitShareAndMasterFreightInvoice.Lines[0].AL_TaxDate, agentInvoice.Lines[1].AL_TaxDate);
			AssertEquals("AR Invoice line 2 should have the same Tax Msg as AP Invoice line 1", profitShareAndMasterFreightInvoice.Lines[0].AL_A9_VATClass, agentInvoice.Lines[1].AL_A9_VATClass);
			AssertEquals("Amount on AR Invoice line 3 should be the same as on AP INovice line 4", profitShareAndMasterFreightInvoice.Lines[3].AL_OSExTaxAmount, agentInvoice.Lines[2].AL_OSExTaxAmount);
			AssertEquals("AR Invoice line 3 should have the same Tax Rate as AP Invoice line 2", profitShareAndMasterFreightInvoice.Lines[1].AL_AT, agentInvoice.Lines[2].AL_AT);
			AssertEquals("AR Invoice line 3 should have the same Tax Date as AP Invoice line 2", profitShareAndMasterFreightInvoice.Lines[1].AL_TaxDate, agentInvoice.Lines[2].AL_TaxDate);
			AssertEquals("AR Invoice line 3 should have the same Tax Msg as AP Invoice line 2", profitShareAndMasterFreightInvoice.Lines[1].AL_A9_VATClass, agentInvoice.Lines[2].AL_A9_VATClass);
		}

		public void TestNegateInvoicesHandlesForeignCurrencyConversionOnLineLevel()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultReceivingForwarderAddress(creator.Agent);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			freightCost.E6_RX_NKCurrency = creator.USD.RX_Code;
			freightCost.E6_ExchangeRate = 0.7381m;

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			Job shipment1Job = Job.CreateWithMutex(Factory, shipment1);
			shipment1Job.PlugInData = shipment1;
			Job shipment2Job = Job.CreateWithMutex(Factory, shipment2);
			shipment2Job.PlugInData = shipment2;
			shipment1Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			shipment2Job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			shipment1Job.Dispose();
			shipment2Job.Dispose();

			Factory.Save();

			ZQuery taxFilter = new ZQuery(AccTaxRateSchema.AT_Type, AccTaxRate.Types.Rated);
			taxFilter.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, Env.CurrentCompany.Country.Code);
			taxFilter.AddToFilter(AccTaxRateSchema.AT_IsActive, true);
			var taxRate = Factory.Load<AccTaxRate>(taxFilter).First(x => x.GetRate_ForTestOnly() == 0);
			AssertNotNull("Pre-condition: Should be a FREEGSTTax Rate", taxRate);

			APInvoice profitShareAndMasterFreightInvoice = Factory.New<APInvoice>();
			profitShareAndMasterFreightInvoice.AH_OH = ((IJobCostingPlugIn)consol).ReceivingAgentAPInvoicingParty.PK;
			profitShareAndMasterFreightInvoice.AH_RX_NKTransactionCurrency = creator.AUD.RX_Code;
			APInvoiceLine aPLine1 = (APInvoiceLine)profitShareAndMasterFreightInvoice.Lines.AddNew();
			aPLine1.GenericCharge = Env.Registry.FreightChargeCode;
			aPLine1.AL_JH = shipment1Job.PK;
			aPLine1.AL_RX_NKTransactionCurrency = creator.AUD.RX_Code;
			aPLine1.AL_OSExTaxAmount = 200m;
			aPLine1.AL_AT = taxRate.PK;

			APInvoiceLine aPLine2 = (APInvoiceLine)profitShareAndMasterFreightInvoice.Lines.AddNew();
			aPLine2.AL_JH = shipment2Job.PK;
			aPLine2.AL_RX_NKTransactionCurrency = creator.CNY.RX_Code;
			aPLine2.AL_ExchangeRate = 5.09m;
			aPLine2.GenericCharge = AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value;
			aPLine2.AL_OSExTaxAmount = 300m;
			aPLine2.AL_AT = taxRate.PK;

			ARCreditNote agentInvoice = Factory.New<ARCreditNote>();
			agentInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			agentInvoice.AH_OH = ((IJobCostingPlugIn)consol).ReceivingAgentAPInvoicingParty.PK;
			agentInvoice.AH_RX_NKTransactionCurrency = creator.USD.RX_Code;
			agentInvoice.AH_ExchangeRate = 0.7381m;
			ARCreditNoteLine aRLine1 = (ARCreditNoteLine)agentInvoice.Lines.AddNew();
			aRLine1.AL_JH = shipment1Job.PK;
			aRLine1.GenericCharge = creator.CC1.PK;
			aRLine1.AL_OSExTaxAmount = 150m;

			ChargePoster poster = new ChargePoster(Factory);
			poster.PostedInvoices.Add(agentInvoice);
			AgentChargePostingDetails postingDetails = new AgentChargePostingDetails(consol, null, creator.USD, 0.7381m, poster, apps.CostsCollection);
			postingDetails.AgentInvoices.Add(profitShareAndMasterFreightInvoice);

			var transactions = new TransactionCreatorHashtable();
			ProfitShareMasterFreightInvoiceNegator negator = new ProfitShareMasterFreightInvoiceNegator(Factory, postingDetails);
			negator.NegateInvoices(transactions);

			Assert("Agent Invoice should stay a credit note", agentInvoice.AH_TransactionType == ZArchitecture.Core.TransactionTypes.CreditNote);

			AssertEquals("Should be 3 lines on the AR Invoice", 3, agentInvoice.Lines.Count);
			AssertEquals("Should be 4 lines on the AP Invoice", 4, profitShareAndMasterFreightInvoice.Lines.Count);

			AssertEquals("USD", agentInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals("USD", agentInvoice.Lines[1].AL_RX_NKTransactionCurrency);
			AssertEquals("Expect local ExTaxAmount 200 AUD", 200m, agentInvoice.Lines[1].AL_LocalExTaxAmount);
			AssertEquals("Expect os ExTaxAmount 147.62 USD", 147.62m, agentInvoice.Lines[1].AL_OSExTaxAmount);
			AssertEquals("USD", agentInvoice.Lines[2].AL_RX_NKTransactionCurrency);
			AssertEquals("Expect local ExTaxAmount 58.94 AUD", 58.94m, agentInvoice.Lines[2].AL_LocalExTaxAmount);
			AssertEquals("Expect os ExTaxAmount 43.50 USD", 43.50m, agentInvoice.Lines[2].AL_OSExTaxAmount);
		}

		public void TestNegateInvoices_AL_AG()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var chargeCode = creator.CC1;
			AccChargeGLPostingOverride postingOverride = creator.CC1.GLPostingOverrides.AddNew();
			postingOverride.Y1_GE = GlbDepartment.CurrentDepartment.PK;
			postingOverride.Y1_AG_CST = creator.CreateGLHeader().PK;
			postingOverride.Y1_AG_ACR = creator.CreateGLHeader().PK;
			postingOverride.Y1_AG_REV = creator.CreateGLHeader().PK;
			postingOverride.Y1_AG_WIP = creator.CreateGLHeader().PK;

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultReceivingForwarderAddress(creator.Agent);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = chargeCode.PK;
			freightCost.E6_ApportionmentMethod = "SHP";

			ForwardingShipment shipment = consol.Shipments.AddNew();

			Job job = creator.CreateJob(shipment);
			job.PlugInData = shipment;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();

			APInvoice profitShareAndMasterFreightInvoice = Factory.New<APInvoice>();
			profitShareAndMasterFreightInvoice.AH_OH = ((IJobCostingPlugIn)consol).ReceivingAgentAPInvoicingParty.PK;
			APInvoiceLine aPLine1 = (APInvoiceLine)profitShareAndMasterFreightInvoice.Lines.AddNew();
			aPLine1.GenericCharge = chargeCode.PK;
			aPLine1.AL_JH = job.PK;
			aPLine1.AL_OSExTaxAmount = 200m;

			ARInvoice agentInvoice = Factory.New<ARInvoice>();
			agentInvoice.AH_OH = ((IJobCostingPlugIn)consol).ReceivingAgentAPInvoicingParty.PK;
			agentInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef + "/BA";
			ARInvoiceLine aRLine1 = (ARInvoiceLine)agentInvoice.Lines.AddNew();
			aRLine1.AL_JH = job.PK;
			aRLine1.GenericCharge = chargeCode.PK;
			aRLine1.AL_OSExTaxAmount = 200m;

			ChargePoster poster = new ChargePoster(Factory);
			AgentChargePostingDetails postingDetails = new AgentChargePostingDetails(consol, null, creator.AUD, 1m, poster, apps.CostsCollection);
			postingDetails.AgentInvoices.Add(profitShareAndMasterFreightInvoice);

			var transactions = new TransactionCreatorHashtable();
			ProfitShareMasterFreightInvoiceNegator negator = new ProfitShareMasterFreightInvoiceNegator(Factory, postingDetails);
			negator.NegateInvoices(transactions);

			AssertEquals("Should be 1 line on the AR Invoice", 1, agentInvoice.Lines.Count);
			AssertEquals("Should be 2 lines on the AP Invoice", 2, profitShareAndMasterFreightInvoice.Lines.Count);
			AssertEquals("AP Invoice should be for zero value", 0m, profitShareAndMasterFreightInvoice.AH_OSExTaxAmount);
			AssertEquals("Amount on AR Invoice line 1 should be the same as on AP INovice line 1", profitShareAndMasterFreightInvoice.Lines[0].AL_OSExTaxAmount, agentInvoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals("Line 2 should be the inverse amount of line 1", -profitShareAndMasterFreightInvoice.Lines[0].AL_OSExTaxAmount, profitShareAndMasterFreightInvoice.Lines[1].AL_OSExTaxAmount);
			AssertNotEquals("Pre-condition: Charge Code has GL Post overriding", chargeCode.AC_AG_RevenueAccount, chargeCode.GLPostingOverrides[0].Y1_AG_REV);
			AssertEquals("GL Account on AR Invoice should be overriden revenue account", chargeCode.GLPostingOverrides[0].Y1_AG_REV, agentInvoice.Lines[0].AL_AG);
			AssertEquals("GL Account on AP Invoice should be overriden revenue account", chargeCode.GLPostingOverrides[0].Y1_AG_REV, profitShareAndMasterFreightInvoice.Lines[1].AL_AG);
		}

		public void TestNegateInvoices_GovtChargeCode()
		{
			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var govtChargeCode = "GOVT1.1";
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var chargeCode = creator.CC1;
			chargeCode.AC_GovtChargeCode = govtChargeCode;
			AccChargeGLPostingOverride postingOverride = creator.CC1.GLPostingOverrides.AddNew();
			postingOverride.Y1_GE = GlbDepartment.CurrentDepartment.PK;
			postingOverride.Y1_AG_CST = creator.CreateGLHeader().PK;
			postingOverride.Y1_AG_ACR = creator.CreateGLHeader().PK;
			postingOverride.Y1_AG_REV = creator.CreateGLHeader().PK;
			postingOverride.Y1_AG_WIP = creator.CreateGLHeader().PK;

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultReceivingForwarderAddress(creator.Agent);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = chargeCode.PK;
			freightCost.E6_ApportionmentMethod = "SHP";

			ForwardingShipment shipment = consol.Shipments.AddNew();

			Job job = creator.CreateJob(shipment);
			job.PlugInData = shipment;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var overridedGovtChargeCode = new TestObjectCreator(Factory).SetupOrCreateGovtChargeCodeOverride(chargeCode, "ALL", "ALL", "ALL", "ALL", "GOVT1.2");
			Factory.Save();

			APInvoice profitShareAndMasterFreightInvoice = Factory.New<APInvoice>();
			profitShareAndMasterFreightInvoice.AH_OH = ((IJobCostingPlugIn)consol).ReceivingAgentAPInvoicingParty.PK;
			APInvoiceLine aPLine1 = (APInvoiceLine)profitShareAndMasterFreightInvoice.Lines.AddNew();
			aPLine1.GenericCharge = chargeCode.PK;
			AssertEquals("Precondition", false, aPLine1.AL_JH.IsValid);
			AssertEquals("Use charge default Govt Charge Code because charge is not job related", govtChargeCode, aPLine1.AL_GovtChargeCode);
			aPLine1.AL_JH = job.PK;
			aPLine1.AL_OSExTaxAmount = 200m;

			ARInvoice agentInvoice = Factory.New<ARInvoice>();
			agentInvoice.AH_OH = ((IJobCostingPlugIn)consol).ReceivingAgentAPInvoicingParty.PK;
			agentInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef + "/BA";
			ARInvoiceLine aRLine1 = (ARInvoiceLine)agentInvoice.Lines.AddNew();
			aRLine1.AL_JH = job.PK;
			aRLine1.GenericCharge = chargeCode.PK;
			AssertEquals("Precondition", true, aRLine1.AL_JH.IsValid);
			AssertEquals("Use charge Govt Charge Code oveerride because charge is job related", overridedGovtChargeCode.ACG_GovtChargeCode, aRLine1.AL_GovtChargeCode);
			aRLine1.AL_OSExTaxAmount = 200m;

			ChargePoster poster = new ChargePoster(Factory);
			AgentChargePostingDetails postingDetails = new AgentChargePostingDetails(consol, null, creator.AUD, 1m, poster, apps.CostsCollection);
			postingDetails.AgentInvoices.Add(profitShareAndMasterFreightInvoice);

			var transactions = new TransactionCreatorHashtable();
			ProfitShareMasterFreightInvoiceNegator negator = new ProfitShareMasterFreightInvoiceNegator(Factory, postingDetails);
			negator.NegateInvoices(transactions);

			AssertEquals("Should be 1 line on the AR Invoice", 1, agentInvoice.Lines.Count);
			AssertEquals("Should be 2 lines on the AP Invoice", 2, profitShareAndMasterFreightInvoice.Lines.Count);
			AssertEquals("AP Invoice should be for zero value", 0m, profitShareAndMasterFreightInvoice.AH_OSExTaxAmount);
			AssertEquals("Amount on AR Invoice line 1 should be the same as on AP INovice line 1", profitShareAndMasterFreightInvoice.Lines[0].AL_OSExTaxAmount, agentInvoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals("Line 2 should be the inverse amount of line 1", -profitShareAndMasterFreightInvoice.Lines[0].AL_OSExTaxAmount, profitShareAndMasterFreightInvoice.Lines[1].AL_OSExTaxAmount);
			AssertEquals("Govt charge code is set for the opposite line", overridedGovtChargeCode.ACG_GovtChargeCode, profitShareAndMasterFreightInvoice.Lines[1].AL_GovtChargeCode);
			AssertNotEquals("Pre-condition: Charge Code has GL Post overriding", chargeCode.AC_AG_RevenueAccount, chargeCode.GLPostingOverrides[0].Y1_AG_REV);
			AssertEquals("GL Account on AR Invoice should be overriden revenue account", chargeCode.GLPostingOverrides[0].Y1_AG_REV, agentInvoice.Lines[0].AL_AG);
			AssertEquals("GL Account on AP Invoice should be overriden revenue account", chargeCode.GLPostingOverrides[0].Y1_AG_REV, profitShareAndMasterFreightInvoice.Lines[1].AL_AG);
		}

		public void TestNegateInvoices_PlaceOfSupply()
		{
			using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var creator = new TestObjectCreator(Factory);
				var consol = Factory.New<ForwardingConsol>();
				consol.SetDefaultReceivingForwarderAddress(creator.Agent);

				var chargeCode = creator.CC1;
				var apps = new ApportionmentListing(Factory, consol);
				JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
				freightCost.E6_AC_ChargeCode = chargeCode.PK;
				freightCost.E6_ApportionmentMethod = "SHP";
				freightCost.E6_PlaceOfSupply = "NSW";

				var shipment = consol.Shipments.AddNew();

				var job = creator.CreateJob(shipment);
				job.PlugInData = shipment;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;

				Factory.Save();

				var profitShareAndMasterFreightInvoice = Factory.New<APInvoice>();
				profitShareAndMasterFreightInvoice.AH_OH = ((IJobCostingPlugIn)consol).ReceivingAgentAPInvoicingParty.PK;
				profitShareAndMasterFreightInvoice.AH_PlaceOfSupply = "NSW";
				var aPLine1 = (APInvoiceLine)profitShareAndMasterFreightInvoice.Lines.AddNew();
				aPLine1.GenericCharge = chargeCode.PK;
				aPLine1.AL_JH = job.PK;
				aPLine1.AL_OSExTaxAmount = 200m;
				aPLine1.AL_PlaceOfSupply = "NSW";

				var agentInvoice = Factory.New<ARInvoice>();
				agentInvoice.AH_OH = ((IJobCostingPlugIn)consol).ReceivingAgentAPInvoicingParty.PK;
				agentInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef + "/BA";
				var aRLine1 = (ARInvoiceLine)agentInvoice.Lines.AddNew();
				aRLine1.AL_JH = job.PK;
				aRLine1.GenericCharge = chargeCode.PK;
				aRLine1.AL_OSExTaxAmount = 200m;

				var poster = new ChargePoster(Factory);
				var postingDetails = new AgentChargePostingDetails(consol, null, creator.AUD, 1m, poster, apps.CostsCollection);
				postingDetails.AgentInvoices.Add(profitShareAndMasterFreightInvoice);

				var transactions = new TransactionCreatorHashtable();
				var negator = new ProfitShareMasterFreightInvoiceNegator(Factory, postingDetails);
				Assert(!poster.PostedInvoices.Any());
				negator.NegateInvoices(transactions);

				AssertEquals("Result of negating is an AR Credit Note because agentInvoice was not found as it is not added to the poster.PostedInvoices", 1, poster.PostedInvoices.Count);
				var resultOfNegating = poster.PostedInvoices[0];
				Assert("ARCreditNote", resultOfNegating is ARCreditNote);
				AssertEquals("Place of supply from profitShareAndMasterFreightInvoice header", "NSW", resultOfNegating.AH_PlaceOfSupply);
				AssertEquals("Should be 1 line on the AR Credit Note", 1, resultOfNegating.Lines.Count);
				AssertEquals("Amount on AR Credit Note line 1 should be the same as on AP INovice line 1", profitShareAndMasterFreightInvoice.Lines[0].AL_OSExTaxAmount, resultOfNegating.Lines[0].AL_OSExTaxAmount);
				AssertEquals("Place of Supply on AR Credit Note line 1 should be the same as on AP Inovice header", "NSW", resultOfNegating.Lines[0].AL_PlaceOfSupply);

				AssertEquals("Should be 1 line on the AR Invoice", 1, agentInvoice.Lines.Count);
				AssertEquals("Should be 2 lines on the AP Invoice", 2, profitShareAndMasterFreightInvoice.Lines.Count);
				AssertEquals("AP Invoice should be for zero value", 0m, profitShareAndMasterFreightInvoice.AH_OSExTaxAmount);
				AssertEquals("Amount on AR Invoice line 1 should be the same as on AP INovice line 1", profitShareAndMasterFreightInvoice.Lines[0].AL_OSExTaxAmount, agentInvoice.Lines[0].AL_OSExTaxAmount);
				AssertEquals("Line 2 should be the inverse amount of line 1", -profitShareAndMasterFreightInvoice.Lines[0].AL_OSExTaxAmount, profitShareAndMasterFreightInvoice.Lines[1].AL_OSExTaxAmount);
				AssertEquals("Line 2 should be the same Place of Supply as line 1", "NSW", profitShareAndMasterFreightInvoice.Lines[1].AL_PlaceOfSupply);
			}
		}

		public void TestNegateInvoices_TransactionCreatorHashtable()
		{
			var creator = new TestObjectCreator(Factory);
			var chargeCode = creator.CC1;
			var postingOverride = creator.CC1.GLPostingOverrides.AddNew();
			postingOverride.Y1_GE = GlbDepartment.CurrentDepartment.PK;
			postingOverride.Y1_AG_CST = creator.CreateGLHeader().PK;
			postingOverride.Y1_AG_ACR = creator.CreateGLHeader().PK;
			postingOverride.Y1_AG_REV = creator.CreateGLHeader().PK;
			postingOverride.Y1_AG_WIP = creator.CreateGLHeader().PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultReceivingForwarderAddress(creator.Agent);

			var apps = new ApportionmentListing(Factory, consol);
			var freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = chargeCode.PK;
			freightCost.E6_ApportionmentMethod = "SHP";

			var shipment = consol.Shipments.AddNew();

			var job = creator.CreateJob(shipment);
			job.PlugInData = shipment;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();

			var profitShareAndMasterFreightInvoice = Factory.New<APInvoice>();
			profitShareAndMasterFreightInvoice.AH_OH = ((IJobCostingPlugIn)consol).ReceivingAgentAPInvoicingParty.PK;
			var aPLine1 = (APInvoiceLine)profitShareAndMasterFreightInvoice.Lines.AddNew();
			aPLine1.GenericCharge = chargeCode.PK;
			aPLine1.AL_JH = job.PK;
			aPLine1.AL_OSExTaxAmount = 200m;

			var agentInvoice = Factory.New<ARInvoice>();
			agentInvoice.AH_OH = ((IJobCostingPlugIn)consol).ReceivingAgentAPInvoicingParty.PK;
			agentInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef + "/BA";
			var aRLine1 = (ARInvoiceLine)agentInvoice.Lines.AddNew();
			aRLine1.AL_JH = job.PK;
			aRLine1.GenericCharge = chargeCode.PK;
			aRLine1.AL_OSExTaxAmount = 200m;

			var poster = new ChargePoster(Factory);
			var postingDetails = new AgentChargePostingDetails(consol, null, creator.AUD, 1m, poster, apps.CostsCollection);
			postingDetails.AgentInvoices.Add(profitShareAndMasterFreightInvoice);

			var transactions = new TransactionCreatorHashtable();
			ProfitShareMasterFreightInvoiceNegator negator = new ProfitShareMasterFreightInvoiceNegator(Factory, postingDetails);
			AssertEquals("TransactionCreatorHashtable is empty before NegateInvoices", 0, transactions.Count);
			negator.NegateInvoices(transactions);

			AssertEquals("Should be 1 line on the AR Invoice", 1, agentInvoice.Lines.Count);
			AssertEquals("Should be 2 lines on the AP Invoice", 2, profitShareAndMasterFreightInvoice.Lines.Count);
			AssertEquals("Should have 1 negate invoice in TransactionCreatorHashtable", 1, transactions.Count);
		}

		public void TestNegateInvoices_NoAmountRecalculationInNegation()
		{
			var creator = new TestObjectCreator(Factory);
			var chargeCode = creator.CC1;

			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultReceivingForwarderAddress(creator.Agent);

			var apps = new ApportionmentListing(Factory, consol);
			var freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = chargeCode.PK;
			freightCost.E6_ApportionmentMethod = "SHP";

			var shipment = consol.Shipments.AddNew();
			var job = creator.CreateJob(shipment);
			job.PlugInData = shipment;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();

			var profitShareAndMasterFreightInvoice = Factory.New<APInvoice>();
			profitShareAndMasterFreightInvoice.AH_OH = ((IJobCostingPlugIn)consol).ReceivingAgentAPInvoicingParty.PK;
			var aPLine1 = (APInvoiceLine)profitShareAndMasterFreightInvoice.Lines.AddNew();
			aPLine1.GenericCharge = chargeCode.PK;
			aPLine1.AL_JH = job.PK;
			aPLine1.AL_RX_NKTransactionCurrency = CurrencyCodes.UnitedStates;
			aPLine1.AL_ExchangeRate = 1.1;
			aPLine1.AL_OSExTaxAmount = 90.91;
			AssertEquals(82.65m, aPLine1.AL_LocalExTaxAmount);

			// When calculating os amount from setting the above local amount, this will result
			// in a different os amount. When creating a negated invoice, if the amounts are
			// recalculated during this then the negated line will not have amounts equal to
			// the original line.
			var proofLine = new BusinessObjectFactory().New<APInvoiceLine>();
			proofLine.AL_RX_NKTransactionCurrency = CurrencyCodes.UnitedStates;
			proofLine.AL_ExchangeRate = aPLine1.AL_ExchangeRate;
			proofLine.AL_LocalExTaxAmount = aPLine1.AL_LocalExTaxAmount;
			AssertNotEquals(aPLine1.AL_OSExTaxAmount, proofLine.AL_OSExTaxAmount);

			var poster = new ChargePoster(Factory);
			var postingDetails = new AgentChargePostingDetails(consol, null, creator.AUD, 1m, poster, apps.CostsCollection);
			postingDetails.AgentInvoices.Add(profitShareAndMasterFreightInvoice);

			var transactions = new TransactionCreatorHashtable();
			ProfitShareMasterFreightInvoiceNegator negator = new ProfitShareMasterFreightInvoiceNegator(Factory, postingDetails);
			AssertEquals("TransactionCreatorHashtable is empty before NegateInvoices", 0, transactions.Count);
			negator.NegateInvoices(transactions);

			AssertEquals("Should be 2 lines on the AP Invoice", 2, profitShareAndMasterFreightInvoice.Lines.Count);
			AssertEquals("Not Allowing recalculation should result in equal but opposite amounts", profitShareAndMasterFreightInvoice.Lines[0].AL_OSExTaxAmount, -profitShareAndMasterFreightInvoice.Lines[1].AL_OSExTaxAmount);
			AssertEquals("Not Allowing recalculation should result in equal but opposite amounts", profitShareAndMasterFreightInvoice.Lines[0].AL_LocalExTaxAmount, -profitShareAndMasterFreightInvoice.Lines[1].AL_LocalExTaxAmount);
		}

		public void TestNegateInvoices_SupplyType()
		{
			var creator = new TestObjectCreator(Factory);
			var chargeCode = creator.CC1;

			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultReceivingForwarderAddress(creator.Agent);

			var apps = new ApportionmentListing(Factory, consol);
			var freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = chargeCode.PK;
			freightCost.E6_ApportionmentMethod = "SHP";

			var shipment = consol.Shipments.AddNew();
			var job = creator.CreateJob(shipment);
			job.PlugInData = shipment;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();

			var profitShareAndMasterFreightInvoice = Factory.New<APInvoice>();
			profitShareAndMasterFreightInvoice.AH_OH = ((IJobCostingPlugIn)consol).ReceivingAgentAPInvoicingParty.PK;
			var aPLine1 = (APInvoiceLine)profitShareAndMasterFreightInvoice.Lines.AddNew();
			aPLine1.GenericCharge = chargeCode.PK;
			aPLine1.AL_JH = job.PK;
			aPLine1.AL_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOA;

			var agentInvoice = Factory.New<ARCreditNote>();
			agentInvoice.AH_OH = ((IJobCostingPlugIn)consol).ReceivingAgentAPInvoicingParty.PK;

			var poster = new ChargePoster(Factory);
			var postingDetails = new AgentChargePostingDetails(consol, null, creator.AUD, 1m, poster, apps.CostsCollection);
			postingDetails.AgentInvoices.Add(profitShareAndMasterFreightInvoice);
			poster.PostedInvoices.Add(agentInvoice);

			var transactions = new TransactionCreatorHashtable();
			var negator = new ProfitShareMasterFreightInvoiceNegator(Factory, postingDetails);
			AssertEquals("Should be 1 lines on the AP Invoice", 1, profitShareAndMasterFreightInvoice.Lines.Count);
			AssertEquals("TransactionCreatorHashtable is empty before NegateInvoices", 0, transactions.Count);

			negator.NegateInvoices(transactions);

			AssertEquals("Should be 2 lines on the AP Invoice", 2, profitShareAndMasterFreightInvoice.Lines.Count);
			AssertEquals(AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOA, profitShareAndMasterFreightInvoice.Lines[0].AL_SupplyType);
			AssertEquals(AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOA, profitShareAndMasterFreightInvoice.Lines[1].AL_SupplyType);

			AssertEquals("TransactionCreatorHashtable contain 1 AR transaction after NegateInvoices", 1, transactions.ARTransactionsCount);
			var transaction = Factory.Load<ARCreditNote>(transactions.GetAllARTransactions()[0].PK);
			AssertEquals("Should be 1 lines on the AR Invoice", 1, transaction.Lines.Count);
			AssertEquals(AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOA, transaction.Lines[0].AL_SupplyType);
		}

		public void TestNegateInvoices_TaxBranch()
		{
			var creator = new TestObjectCreator(Factory);
			var chargeCode = creator.CC1;

			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultReceivingForwarderAddress(creator.Agent);

			var apps = new ApportionmentListing(Factory, consol);
			var freightCost = apps.CostsCollection.TryAddNew();
			freightCost.E6_AC_ChargeCode = chargeCode.PK;
			freightCost.E6_ApportionmentMethod = "SHP";

			var shipment = consol.Shipments.AddNew();
			var job = creator.CreateJob(shipment);
			job.PlugInData = shipment;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();

			var profitShareAndMasterFreightInvoice = Factory.New<APInvoice>();
			profitShareAndMasterFreightInvoice.AH_OH = ((IJobCostingPlugIn)consol).ReceivingAgentAPInvoicingParty.PK;
			var aPLine1 = (APInvoiceLine)profitShareAndMasterFreightInvoice.Lines.AddNew();
			profitShareAndMasterFreightInvoice.AH_GB_TaxBranch = creator.NonCurrentBranch.PK;
			aPLine1.GenericCharge = chargeCode.PK;
			aPLine1.AL_JH = job.PK;
			aPLine1.AL_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOA;

			var agentInvoice = Factory.New<ARCreditNote>();
			agentInvoice.AH_OH = ((IJobCostingPlugIn)consol).ReceivingAgentAPInvoicingParty.PK;
			agentInvoice.AH_GB_TaxBranch = creator.NonCurrentBranch.PK;
			agentInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;

			var poster = new ChargePoster(Factory);
			var postingDetails = new AgentChargePostingDetails(consol, null, creator.AUD, 1m, poster, apps.CostsCollection);
			postingDetails.AgentInvoices.Add(profitShareAndMasterFreightInvoice);
			poster.PostedInvoices.Add(agentInvoice);

			var transactions = new TransactionCreatorHashtable();
			var negator = new ProfitShareMasterFreightInvoiceNegator(Factory, postingDetails);
			AssertEquals("Should be 1 lines on the AP Invoice", 1, profitShareAndMasterFreightInvoice.Lines.Count);
			AssertEquals("TransactionCreatorHashtable is empty before NegateInvoices", 0, transactions.Count);

			negator.NegateInvoices(transactions);

			AssertEquals("Should be 2 lines on the AP Invoice", 2, profitShareAndMasterFreightInvoice.Lines.Count);
			AssertEquals(creator.NonCurrentBranch.PK, profitShareAndMasterFreightInvoice.Lines[0].AL_GB_TaxBranch);
			AssertEquals(creator.NonCurrentBranch.PK, profitShareAndMasterFreightInvoice.Lines[1].AL_GB_TaxBranch);

			AssertEquals("TransactionCreatorHashtable contain 1 AR transaction after NegateInvoices", 1, transactions.ARTransactionsCount);
			var transaction = Factory.Load<ARCreditNote>(transactions.GetAllARTransactions()[0].PK);
			AssertEquals("Should be 1 lines on the AR Invoice", 1, transaction.Lines.Count);
			AssertEquals(creator.NonCurrentBranch.PK, transaction.Lines[0].AL_GB_TaxBranch);
		}
	}
}
