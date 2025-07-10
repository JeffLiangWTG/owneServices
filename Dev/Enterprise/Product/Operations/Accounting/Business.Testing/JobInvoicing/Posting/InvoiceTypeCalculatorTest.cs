using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class InvoiceTypeCalculatorTest : TestCaseWithFactory
	{
		public void TestInvoiceTypeIsSetToEmptyWhenNoDebtor()
		{
			var debtor = GetDebtorForTest(InvoicePostingOptionsList.Codes.FinalInvoiceOnly, "EUR");

			var charge = NewChargeOnShipmentJob(null, debtor, LocalCurrency);
			AssertEquals("Invoice type should not have been set", ZString.Empty, charge.JR_InvoiceType);

			charge.JR_AC = FreightChargeCode.PK;
			charge.JR_OH_SellAccount = debtor.PK;
			AssertEquals("Invoice type should be set", FinalInvoice, charge.JR_InvoiceType);
			AssertEquals("Sell Invoice Currency should be set", "EUR", charge.JR_RX_NKSellInvoiceCurrency);

			charge.JR_OH_SellAccount = ZGuid.Empty;
			AssertEquals("Invoice type should be re-set back to empty", ZString.Empty, charge.JR_InvoiceType);
			AssertEquals("Sell Invoice Currency should be reset", ZString.Empty, charge.JR_RX_NKSellInvoiceCurrency);

			charge.JR_OH_SellAccount = debtor.PK;
			AssertEquals("Invoice type should be set again", FinalInvoice, charge.JR_InvoiceType);
			AssertEquals("Sell Invoice Currency should be set again", "EUR", charge.JR_RX_NKSellInvoiceCurrency);
		}

		public void TestInvoiceTypeDefaultingForCrossTrade()
		{
			var org = GetDebtorForTest(InvoicePostingOptionsList.Codes.FinalInvoiceOnly, "EUR");

			var group = org.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.CrossTrade;
			group.PG_TransportMode = "ALL";
			group.PG_GroupOrSubTotal = "DEF";
			group.PG_GroupOrSubtotalStyle = "DEF";
			group.PG_InvoiceLineDisplayOption = "DEF";
			group.PG_InvoicePostingStyle = "FOU";
			group.PG_RX_NKInvoicePostingCurrency = "EUR";

			org.Factory.Save();

			AssertInvoiceTypeSetCorrectly(org, "CUR", "GBLON", "USLAX", "AIR", "", "");
		}

		public void TestInvoiceTypeDefaultingForSelfBilling()
		{
			OrgHeader org = GetDebtorForTest(InvoicePostingOptionsList.Codes.FinalInvoiceOnly, "EUR");

			OrgInvoiceRollupOrGroup group = org.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.CrossTrade;
			group.PG_TransportMode = "ALL";
			group.PG_GroupOrSubTotal = "DEF";
			group.PG_GroupOrSubtotalStyle = "DEF";
			group.PG_InvoiceLineDisplayOption = "DEF";
			group.PG_InvoicePostingStyle = "FOU";
			group.PG_RX_NKInvoicePostingCurrency = "EUR";

			org.CompanyData.OB_ARCustomerSelfBillsRevenue = true;

			org.Factory.Save();

			AssertInvoiceTypeSetCorrectly(org, InvoiceTypesList.Codes.SelfBillingInvoice, "GBLON", "USLAX", "AIR", "", "");
			AssertInvoiceTypeSetCorrectly(org, InvoiceTypesList.Codes.SelfBillingInvoice, "AUSYD", "NZAKL", "SEA", "", "");
			AssertInvoiceTypeSetCorrectly(org, InvoiceTypesList.Codes.SelfBillingInvoice, "AUSYD", "NZAKL", "SEA", "", "");

			org.CompanyData.OB_ARCustomerSelfBillsRevenue = false;

			org.Factory.Save();

			AssertInvoiceTypeSetCorrectly(org, "CUR", "GBLON", "USLAX", "AIR", "", "");
		}

		public void TestInvoiceTypeDefaultingForDomestic()
		{
			OrgHeader org = GetDebtorForTest(InvoicePostingOptionsList.Codes.FinalInvoiceOnly, "EUR");

			OrgInvoiceRollupOrGroup group = org.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Domestic;
			group.PG_TransportMode = "ALL";
			group.PG_GroupOrSubTotal = "DEF";
			group.PG_GroupOrSubtotalStyle = "DEF";
			group.PG_InvoiceLineDisplayOption = "DEF";
			group.PG_InvoicePostingStyle = "FOU";
			group.PG_RX_NKInvoicePostingCurrency = "EUR";

			org.Factory.Save();

			AssertInvoiceTypeSetCorrectly(org, "CUR", "AUSYD", "AUBNE", "AIR", "", "");
		}

		public void TestInvoiceTypeDefaultingForLCLSeaJob()
		{
			var org = GetDebtorForTest(InvoicePostingOptionsList.Codes.FinalInvoiceOnly, "EUR");

			var lclGroup = org.CompanyData.InvoiceRollupOrGroups.AddNew();
			lclGroup.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			lclGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.CrossTrade;
			lclGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.LCL;
			lclGroup.PG_GroupOrSubTotal = "DEF";
			lclGroup.PG_GroupOrSubtotalStyle = "DEF";
			lclGroup.PG_InvoiceLineDisplayOption = "DEF";
			lclGroup.PG_InvoicePostingStyle = "FOU";
			lclGroup.PG_RX_NKInvoicePostingCurrency = "EUR";

			var allGroup = org.CompanyData.InvoiceRollupOrGroups.AddNew();
			allGroup.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			allGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.CrossTrade;
			allGroup.PG_TransportMode = "ALL";
			allGroup.PG_GroupOrSubTotal = "DEF";
			allGroup.PG_GroupOrSubtotalStyle = "DEF";
			allGroup.PG_InvoiceLineDisplayOption = "DEF";
			allGroup.PG_InvoicePostingStyle = "DFI";
			allGroup.PG_RX_NKInvoicePostingCurrency = "EUR";

			org.Factory.Save();

			AssertInvoiceTypeSetCorrectly(org, "CUR", "GBLON", "USLAX", "SEA", "LCL", "");
		}

		void AssertInvoiceTypeSetCorrectly(OrgHeader org, string expectedInvoiceType, string origin, string destination, string transportMode, string containerMode, string sellInvoiceCurrency)
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_TransportMode = transportMode;
			shipment.JS_PackingMode = containerMode;

			Job job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			charge.JR_OH_SellAccount = org.PK;
			charge.JR_RX_NKSellCurrency = "USD";

			AssertEquals("Invoice type should be " + expectedInvoiceType, expectedInvoiceType, charge.JR_InvoiceType);
			AssertEquals("Sell Invoice Currency", string.IsNullOrEmpty(sellInvoiceCurrency) ? string.Empty : sellInvoiceCurrency, charge.JR_RX_NKSellInvoiceCurrency);
		}

		public void TestInvoiceType_BillOfLading()
		{
			var debtor1 = GetDebtorForTest(InvoicePostingOptionsList.Codes.FinalInvoiceOnly, "EUR");
			fDebtorForTest = null; // who the hell comes up with this shit?

			var debtor2 = GetDebtorForTest(InvoicePostingOptionsList.Codes.ForeignCurrencyInvoiceAndFinal, "USD");

			var billOfLading = Factory.NewWithValidTestData<BillOfLading>();
			billOfLading.JS_RL_NKOrigin = "AUBNE";
			billOfLading.JS_RL_NKDestination = "NLAMS";

			var freightChargeCode = NewChargeCode(ChargeCodeGroupList.Codes.Freight, Core.Constants.ChargeType.Revenue);
			var nonFreightChargeCode = NewChargeCode(ChargeCodeGroupList.Codes.Insurance, Core.Constants.ChargeType.Revenue);

			var freightCharge = NewChargeOnJob(billOfLading, freightChargeCode, debtor1, ForeignCurrency);
			var nonFreightCharge = NewChargeOnJob(billOfLading, nonFreightChargeCode, debtor1, ForeignCurrency);

			var calculator = new InvoiceTypeCalculator(freightCharge);
			calculator.UpdateInvoiceType();
			AssertEquals("Invoice type remains blank as prepaid/collect not set on job", "", freightCharge.JR_InvoiceType);
			AssertEquals("Sell Invoice Currency should not be set", ZString.Empty, freightCharge.JR_RX_NKSellInvoiceCurrency);

			billOfLading.JS_INCO = Enterprise.Core.Constants.DomesticPaymentTerms.Prepaid;
			calculator.UpdateInvoiceType();
			AssertEquals("Updated to prepaid", AgencyInvoiceTypesList.Codes.LocalPrePaid, freightCharge.JR_InvoiceType);
			AssertEquals("Sell Invoice Currency should be set from Org InvoiceRollupOrGroup", "EUR", freightCharge.JR_RX_NKSellInvoiceCurrency);

			freightCharge.JR_OH_SellAccount = ZGuid.Empty;
			calculator.UpdateInvoiceType();
			AssertEquals("No changes as Debtor is Empty", AgencyInvoiceTypesList.Codes.LocalPrePaid, freightCharge.JR_InvoiceType);
			AssertEquals("Sell Invoice Currency should be changed as Debtor is set Empty", string.Empty, freightCharge.JR_RX_NKSellInvoiceCurrency);

			freightCharge.JR_OH_SellAccount = debtor2.PK;
			calculator.UpdateInvoiceType();
			AssertEquals("Switch to foreign", AgencyInvoiceTypesList.Codes.ForeignPrePaid, freightCharge.JR_InvoiceType);
			AssertEquals("Sell Invoice Currency should not be set", ZString.Empty, freightCharge.JR_RX_NKSellInvoiceCurrency);

			billOfLading.JS_INCO = Enterprise.Core.Constants.DomesticPaymentTerms.Collect;
			calculator.UpdateInvoiceType();
			AssertEquals("Stay Prepaid", AgencyInvoiceTypesList.Codes.ForeignPrePaid, freightCharge.JR_InvoiceType);
			AssertEquals("Sell Invoice Currency should not be set", ZString.Empty, freightCharge.JR_RX_NKSellInvoiceCurrency);

			freightCharge.JR_InvoiceType = "";
			calculator.UpdateInvoiceType();
			AssertEquals("Updated to collect", AgencyInvoiceTypesList.Codes.ForeignCollect, freightCharge.JR_InvoiceType);
			AssertEquals("Sell Invoice Currency should not be set", ZString.Empty, freightCharge.JR_RX_NKSellInvoiceCurrency);

			billOfLading.JS_INCO = ZString.Empty;

			calculator = new InvoiceTypeCalculator(nonFreightCharge);
			calculator.UpdateInvoiceType();
			AssertEquals("Invoice type set to blank (default -- FINAL is not valid)", "", nonFreightCharge.JR_InvoiceType);
			AssertEquals("Sell Invoice Currency should not be set", ZString.Empty, freightCharge.JR_RX_NKSellInvoiceCurrency);
		}

		public void TestInvoiceType_ChargeCodeTypeOverride()
		{
			var chargeCodeWithTypeOverride = NewChargeCode(ChargeCodeGroupList.Codes.Freight, Core.Constants.ChargeType.Revenue);
			chargeCodeWithTypeOverride.AC_Code = "CODE1";
			chargeCodeWithTypeOverride.AC_GC = GlbCompany.CurrentCompany.PK;

			var typeOverride = chargeCodeWithTypeOverride.ChargeTypeOverrides.AddNew();
			typeOverride.AN_JobType = "ALL";
			typeOverride.AN_JobDirection = "ALL";
			typeOverride.AN_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;

			var chargeCodeWithBlankTypeOverride = NewChargeCode(ChargeCodeGroupList.Codes.Freight, Core.Constants.ChargeType.Revenue);
			chargeCodeWithBlankTypeOverride.AC_Code = "CODE2";
			chargeCodeWithBlankTypeOverride.AC_GC = GlbCompany.CurrentCompany.PK;
			typeOverride = chargeCodeWithBlankTypeOverride.ChargeTypeOverrides.AddNew();
			typeOverride.AN_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			typeOverride.AN_JobDirection = "ALL";

			var chargeCodeWithNoTypeOverride = NewChargeCode(ChargeCodeGroupList.Codes.Freight, Core.Constants.ChargeType.Revenue);
			chargeCodeWithNoTypeOverride.AC_Code = "CODE3";
			chargeCodeWithNoTypeOverride.AC_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var debtor = GetDebtorForTest(InvoicePostingOptionsList.Codes.FinalInvoiceOnly);

			var charge = NewChargeOnShipmentJob(null, debtor, LocalCurrency);
			var calculator = new InvoiceTypeCalculator(charge);
			AssertEquals("Invoice type should not have been set", ZString.Empty, charge.JR_InvoiceType);

			AssertCorrectInvoiceForChargeCode(charge, calculator, chargeCodeWithTypeOverride, DisbursementInvoice, LocalCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, chargeCodeWithBlankTypeOverride, FinalInvoice, LocalCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, chargeCodeWithNoTypeOverride, FinalInvoice, LocalCurrency);

			debtor.CompanyData.InvoiceRollupOrGroups[0].PG_RX_NKInvoicePostingCurrency = USCurrency.RX_Code;
			debtor.Factory.Save();

			AssertCorrectInvoiceForChargeCode(charge, calculator, chargeCodeWithTypeOverride, DisbursementInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, chargeCodeWithBlankTypeOverride, FinalInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, chargeCodeWithNoTypeOverride, FinalInvoice, LocalCurrency, USCurrency);

			debtor.CompanyData.InvoiceRollupOrGroups[0].PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			debtor.Factory.Save();

			charge = NewChargeOnShipmentJob(null, debtor, LocalCurrency);
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKDestination = "USLAX";
			AssertEquals("Invoice type should not have been set", ZString.Empty, charge.JR_InvoiceType);
			AssertCorrectInvoiceForChargeCode(charge, calculator, chargeCodeWithNoTypeOverride, FinalInvoice, LocalCurrency); // from registry default

			debtor.CompanyData.InvoiceRollupOrGroups[0].PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			charge = NewChargeOnShipmentJob(null, debtor, LocalCurrency);
			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			AssertEquals("Invoice type should not have been set", ZString.Empty, charge.JR_InvoiceType);
			AssertCorrectInvoiceForChargeCode(charge, calculator, chargeCodeWithNoTypeOverride, FinalInvoice, LocalCurrency, USCurrency);
		}

		public void TestUpdateInvoiceTypeWhenPostingOptionNotSet()
		{
			var debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "DEB";
			debtor.OH_IsDebtor = true;
			var charge = NewChargeOnShipmentJob(null, debtor, LocalCurrency);
			InvoiceTypeCalculator calculator = new InvoiceTypeCalculator(charge);
			calculator.UpdateInvoiceType();
			AssertEquals("Invoice type should still not be set", ZString.Empty, charge.JR_InvoiceType);
			AssertEquals("Sell Invoice Currency should be not set", ZString.Empty, charge.JR_RX_NKSellInvoiceCurrency);
		}

		public void TestFinalInvoiceOnly()
		{
			var debtor = GetDebtorForTest(InvoicePostingOptionsList.Codes.FinalInvoiceOnly, ForeignCurrency.RX_Code);

			var charge = NewChargeOnShipmentJob(null, debtor, LocalCurrency);
			var calculator = new InvoiceTypeCalculator(charge);
			AssertEquals("Invoice type should not have been set", ZString.Empty, charge.JR_InvoiceType);

			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, FinalInvoice, LocalCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, FinalInvoice, LocalCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, FinalInvoice, LocalCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, FinalInvoice, LocalCurrency, ForeignCurrency);
		}

		public void TestDisbursementInvoiceOnly()
		{
			var debtor = GetDebtorForTest(InvoicePostingOptionsList.Codes.DisbursementInvoiceOnly, USCurrency.RX_Code);

			var charge = NewChargeOnShipmentJob(null, debtor, LocalCurrency);
			var calculator = new InvoiceTypeCalculator(charge);
			AssertEquals("Invoice type should not have been set", ZString.Empty, charge.JR_InvoiceType);

			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);
		}

		public void TestDisbursementFreightAsDisbursementAndFinal()
		{
			var debtor = GetDebtorForTest(InvoicePostingOptionsList.Codes.DisbursementFreightAsDisbursementAndFinal, USCurrency.RX_Code);

			var charge = NewChargeOnShipmentJob(null, debtor, LocalCurrency);
			InvoiceTypeCalculator calculator = new InvoiceTypeCalculator(charge);
			AssertEquals("Invoice type should not have been set", ZString.Empty, charge.JR_InvoiceType);

			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, FinalInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);

			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, DisbursementInvoice, ForeignCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, DisbursementInvoice, ForeignCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, FinalInvoice, ForeignCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, DisbursementInvoice, ForeignCurrency, USCurrency);
		}

		public void TestDisbursementFreightAsForeignDisbursementAndStandardAndFinal()
		{
			var debtor = GetDebtorForTest(InvoicePostingOptionsList.Codes.DisbursementFreightAsForeignDisbursementAndStandardAndFinal, USCurrency.RX_Code);

			var charge = NewChargeOnShipmentJob(null, debtor, LocalCurrency);
			var calculator = new InvoiceTypeCalculator(charge);
			AssertEquals("Invoice type should not have been set", ZString.Empty, charge.JR_InvoiceType);

			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, FinalInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);

			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, DisbursementInForeignCurrencyInvoice, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, DisbursementInForeignCurrencyInvoice, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, ForeignCurrencyInvoice, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, DisbursementInForeignCurrencyInvoice, ForeignCurrency);
		}

		public void TestDisbursementForeignOnly()
		{
			var debtor = GetDebtorForTest(InvoicePostingOptionsList.Codes.DisbursementForeignOnly, USCurrency.RX_Code);

			var charge = NewChargeOnShipmentJob(null, debtor, LocalCurrency);
			var calculator = new InvoiceTypeCalculator(charge);
			AssertEquals("Invoice type should not have been set", ZString.Empty, charge.JR_InvoiceType);

			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);

			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, DisbursementInForeignCurrencyInvoice, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, DisbursementInForeignCurrencyInvoice, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, DisbursementInForeignCurrencyInvoice, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, DisbursementInForeignCurrencyInvoice, ForeignCurrency);
		}

		public void TestDisbursementAndFinalInvoice()
		{
			var debtor = GetDebtorForTest(InvoicePostingOptionsList.Codes.DisbursementAndFinal, USCurrency.RX_Code);

			var charge = NewChargeOnShipmentJob(null, debtor, LocalCurrency);
			var calculator = new InvoiceTypeCalculator(charge);
			AssertEquals("Invoice type should not have been set", ZString.Empty, charge.JR_InvoiceType);

			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, FinalInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, FinalInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);
		}

		public void TestDisbursementForeignAndFinalInvoice()
		{
			var debtor = GetDebtorForTest(InvoicePostingOptionsList.Codes.DisbursementForeignAndFinal, USCurrency.RX_Code);

			var charge = NewChargeOnShipmentJob(null, debtor, LocalCurrency);
			var calculator = new InvoiceTypeCalculator(charge);
			AssertEquals("Invoice type should not have been set", ZString.Empty, charge.JR_InvoiceType);

			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, FinalInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, FinalInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);

			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, DisbursementInForeignCurrencyInvoice, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, FinalInvoice, ForeignCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, FinalInvoice, ForeignCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, DisbursementInForeignCurrencyInvoice, ForeignCurrency);
		}

		public void TestDisbursementStandardAndFinalInvoice()
		{
			var debtor = GetDebtorForTest(InvoicePostingOptionsList.Codes.DisbursementStandardAndFinal, USCurrency.RX_Code);

			var charge = NewChargeOnShipmentJob(null, debtor, LocalCurrency);
			var calculator = new InvoiceTypeCalculator(charge);
			AssertEquals("Invoice type should not have been set", ZString.Empty, charge.JR_InvoiceType);

			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, FinalInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, FinalInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);

			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, DisbursementInForeignCurrencyInvoice, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, ForeignCurrencyInvoice, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, ForeignCurrencyInvoice, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, DisbursementInForeignCurrencyInvoice, ForeignCurrency);
		}

		public void TestDisbursementFreightForeignAndFinal()
		{
			var debtor = GetDebtorForTest(InvoicePostingOptionsList.Codes.DisbursementFreightForeignAndFinal, USCurrency.RX_Code);

			var charge = NewChargeOnShipmentJob(null, debtor, LocalCurrency);
			var calculator = new InvoiceTypeCalculator(charge);
			AssertEquals("Invoice type should not have been set", ZString.Empty, charge.JR_InvoiceType);

			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, FreightInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, FinalInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);

			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, DisbursementInForeignCurrencyInvoice, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, FreightInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, FinalInvoice, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, DisbursementInForeignCurrencyInvoice, ForeignCurrency);
		}

		public void TestFreightAndFinal()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var debtor = GetDebtorForTest(InvoicePostingOptionsList.Codes.FreightAndFinal, USCurrency.RX_Code);

			var charge = NewChargeOnShipmentJob(null, debtor, AUCurrency);
			var calculator = new InvoiceTypeCalculator(charge);
			AssertEquals("Invoice type should not have been set", ZString.Empty, charge.JR_InvoiceType);

			//AUD set
			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, FinalInvoice, AUCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, FinalInvoice, AUCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, FinalInvoice, AUCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, FinalInvoice, AUCurrency, USCurrency);

			//USD set
			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, FinalInvoice, USCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, FreightInvoice, USCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, FinalInvoice, USCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, FreightInvoice, USCurrency, USCurrency);
		}

		public void TestDisbursementFreightAndFinal()
		{
			var debtor = GetDebtorForTest(InvoicePostingOptionsList.Codes.DisbursementFreightAndFinal, ForeignCurrency.RX_Code);

			var charge = NewChargeOnShipmentJob(null, debtor, AUCurrency);
			var calculator = new InvoiceTypeCalculator(charge);
			AssertEquals("Invoice type should not have been set", ZString.Empty, charge.JR_InvoiceType);

			//AUD set
			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, DisbursementInvoice, AUCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, FinalInvoice, AUCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, FinalInvoice, AUCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, DisbursementInvoice, AUCurrency, ForeignCurrency);

			//USD set
			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, DisbursementInvoice, USCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, FreightInvoice, USCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, FinalInvoice, USCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, DisbursementInvoice, USCurrency, ForeignCurrency);
		}

		public void TestDisbursementForeignCurrencyAndFinal()
		{
			var debtor = GetDebtorForTest(InvoicePostingOptionsList.Codes.DisbursementInvoiceForeignAndFinal, ForeignCurrency.RX_Code);

			var charge = NewChargeOnShipmentJob(null, debtor, LocalCurrency);
			var calculator = new InvoiceTypeCalculator(charge);
			AssertEquals("Invoice type should not have been set", ZString.Empty, charge.JR_InvoiceType);

			//Set as Local currency charges
			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, DisbursementInvoice, LocalCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, FinalInvoice, LocalCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, FinalInvoice, LocalCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, LoadingChargeCode, FinalInvoice, LocalCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginAndDisbursementChargeCode, DisbursementInvoice, LocalCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, DisbursementInvoice, LocalCurrency, ForeignCurrency);

			//Set as USD charges
			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, DisbursementInvoice, USCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, ForeignCurrencyInvoice, USCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, ForeignCurrencyInvoice, USCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, LoadingChargeCode, ForeignCurrencyInvoice, USCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginAndDisbursementChargeCode, DisbursementInvoice, USCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, DisbursementInvoice, USCurrency, ForeignCurrency);

			//Set as foreign charges 
			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, DisbursementInvoice, ForeignCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, ForeignCurrencyInvoice, ForeignCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, ForeignCurrencyInvoice, ForeignCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, LoadingChargeCode, ForeignCurrencyInvoice, ForeignCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginAndDisbursementChargeCode, DisbursementInvoice, ForeignCurrency, ForeignCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, DisbursementInvoice, ForeignCurrency, ForeignCurrency);
		}

		public void TestInvoicePerTaxCode()
		{
			var debtor = GetDebtorForTest(InvoicePostingOptionsList.Codes.InvoicePerTaxCode, USCurrency.RX_Code);

			var charge = NewChargeOnShipmentJob(null, debtor, LocalCurrency);
			var calculator = new InvoiceTypeCalculator(charge);
			AssertEquals("Invoice type should not have been set", ZString.Empty, charge.JR_InvoiceType);

			AssertCorrectInvoiceForChargeCode(charge, calculator, DisbursementChargeCode, InvoicePerTaxCode, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightChargeCode, InvoicePerTaxCode, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, OriginChargeCode, InvoicePerTaxCode, LocalCurrency, USCurrency);
			AssertCorrectInvoiceForChargeCode(charge, calculator, FreightAndDisbursementChargeCode, InvoicePerTaxCode, LocalCurrency, USCurrency);
		}

		public void TestGetInvoicePostingStyleJobTypeBRK()
		{
			var debtor = GetDebtorForTest(InvoicePostingOptionsList.Codes.DisbursementFreightAsDisbursementAndFinal);
			var freightChargeCode = FreightChargeCode;

			var group = debtor.CompanyData.InvoiceRollupOrGroups[0];
			group.PG_JobType = "BRK";
			group.PG_ServiceDirection = "IMX";
			group.PG_TransportMode = "AIR";
			group.PG_InvoicePostingStyle = "DFI";

			var group2 = debtor.CompanyData.InvoiceRollupOrGroups.AddNew();
			group2.PG_JobType = "BRK";
			group2.PG_ServiceDirection = "IMP";
			group2.PG_TransportMode = "AIR";
			group2.PG_InvoicePostingStyle = "DFO";
			OrgFactory.Save();

			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration1.JE_MessageType = "IMX";
			declaration1.JE_TransportMode = "AIR";
			var charge1 = NewChargeOnJob(declaration1, FreightChargeCode, debtor, LocalCurrency);
			InvoiceTypeCalculator calculator = new InvoiceTypeCalculator(charge1);
			AssertEquals("DFI", calculator.GetInvoicePostingStyle());

			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration2.JE_MessageType = "IMP";
			declaration2.JE_TransportMode = "AIR";
			var charge2 = NewChargeOnJob(declaration2, FreightChargeCode, debtor, LocalCurrency);
			calculator = new InvoiceTypeCalculator(charge2);
			AssertEquals("DFO", calculator.GetInvoicePostingStyle());
		}

		public void TestGetDirectionCodeInitialisesParent()
		{
			var debtor = GetDebtorForTest(InvoicePostingOptionsList.Codes.DisbursementFreightAsDisbursementAndFinal, USCurrency.RX_Code);
			var freightChargeCode = FreightChargeCode;

			var group = debtor.CompanyData.InvoiceRollupOrGroups[0];
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Sea;
			group.PG_InvoicePostingStyle = InvoicePostingOptionsList.Codes.DisbursementFreightAsDisbursementAndFinal;
			OrgFactory.Save();

			Shipment.JS_RL_NKOrigin = "USCHI";
			Shipment.JS_RL_NKDestination = "AUSYD";
			Shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			var charge = NewChargeOnShipmentJob(freightChargeCode, debtor, LocalCurrency);
			charge.Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var chargeReloaded = newFactory.Load<Charge>(charge.PK);
			AssertNull("PlugInData not initialized", chargeReloaded.InvoicingJob.PlugInData);

			var calculator = new InvoiceTypeCalculator(chargeReloaded);
			calculator.GetInvoicePostingStyle();
			AssertNotNull("PlugInData initialized", chargeReloaded.InvoicingJob.PlugInData);

			AssertCorrectInvoiceForChargeCode(chargeReloaded, calculator, freightChargeCode, DisbursementInvoice, LocalCurrency, USCurrency);
		}

		#region Implementation

		void AssertCorrectInvoiceForChargeCode(Charge charge, InvoiceTypeCalculator calculator, AccChargeCode chargeCodeToCheck, ZString invoiceType, RefCurrency sellCurrency, RefCurrency sellInvoiceCurrency = null)
		{
			charge.JR_InvoiceType = ZString.Empty;
			charge.JR_AC = chargeCodeToCheck.PK;
			charge.JR_RX_NKSellCurrency = sellCurrency.RX_Code;

			calculator.UpdateInvoiceType();
			AssertEquals("Charge should be on invoice: " + invoiceType, invoiceType, charge.JR_InvoiceType);

			if (!invoiceType.IsEmpty)
			{
				AssertInvoiceTypeForDeferredChargeCode(charge, calculator, invoiceType);
				AssertEquals("JR_RX_NKSellInvoiceCurrency", InvoiceTypeCalculationProvider.BillInLocalCurrency(invoiceType) && sellInvoiceCurrency != null ? sellInvoiceCurrency.RX_Code : ZString.Empty, charge.JR_RX_NKSellInvoiceCurrency);
			}
			else
			{
				AssertEquals("JR_RX_NKSellInvoiceCurrency", "", charge.JR_RX_NKSellInvoiceCurrency);
			}
		}

		void AssertInvoiceTypeForDeferredChargeCode(Charge charge, InvoiceTypeCalculator calculator, ZString invoiceType)
		{
			charge.SellAccount.CompanyData.InvoiceTypes.RemoveAndDeleteAll();
			charge.SellAccount.CompanyData.ClearInvoiceTypeCache_ForTestOnly();

			OrgInvoiceType orgInvoiceType = charge.SellAccount.CompanyData.InvoiceTypes.AddNew();
			orgInvoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			orgInvoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.INC;
			orgInvoiceType.PI_RS_NKServiceLevel = "STD";

			OrgInvTypeDeferredCharges deferredCharge = orgInvoiceType.DeferredCharges.AddNew();
			deferredCharge.PO_AC = charge.JR_AC;

			calculator = new InvoiceTypeCalculator(charge);
			calculator.UpdateInvoiceType();
			invoiceType = invoiceType.SubstringSafe(0, invoiceType.Length - 1) + "D";
			AssertEquals("For Deferred Charges Charge should be on invoice: " + invoiceType, invoiceType, charge.JR_InvoiceType);

			charge.SellAccount.CompanyData.InvoiceTypes.RemoveAndDeleteAll();
		}

		#region Debtor For Test With Final Invoice

		BusinessObjectFactory OrgFactory
		{
			get
			{
				if (fOrgFactory == null)
				{
					fOrgFactory = new BusinessObjectFactory();
				}

				return fOrgFactory;
			}
		}

		BusinessObjectFactory fOrgFactory;

		OrgHeader GetDebtorForTest(ZString invoicePostingStyle, string sellInvoiceCurrency = null)
		{
			if (fDebtorForTest == null)
			{
				fDebtorForTest = OrgFactory.NewWithValidTestData<OrgHeader>();
				fDebtorForTest.OH_IsDebtor = true;
			}
			fDebtorForTest.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			fDebtorForTest.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			var group = fDebtorForTest.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_InvoicePostingStyle = invoicePostingStyle;

			if (!string.IsNullOrEmpty(sellInvoiceCurrency))
			{
				group.PG_RX_NKInvoicePostingCurrency = sellInvoiceCurrency;
			}

			OrgFactory.Save();

			return fDebtorForTest;
		}
		OrgHeader fDebtorForTest;

		#endregion

		#region Charge Codes

		#region OriginChargeCode

		AccChargeCode OriginChargeCode
		{
			get
			{
				if (fOriginChargeCode == null)
				{
					fOriginChargeCode = NewChargeCode(ChargeCodeGroupList.Codes.Origin, ZString.Empty);
				}
				return fOriginChargeCode;
			}
		}

		AccChargeCode fOriginChargeCode;

		#endregion

		#region DisbursementChargeCode

		AccChargeCode DisbursementChargeCode
		{
			get
			{
				if (fDisbursementChargeCode == null)
				{
					fDisbursementChargeCode = NewChargeCode(ChargeCodeGroupList.Codes.ShippingDisbursements, Core.Constants.ChargeType.Disbursement);
				}
				return fDisbursementChargeCode;
			}
		}

		AccChargeCode fDisbursementChargeCode;

		#endregion

		#region FreightChargeCode

		AccChargeCode FreightChargeCode
		{
			get
			{
				if (fFreightChargeCode == null)
				{
					fFreightChargeCode = NewChargeCode(ChargeCodeGroupList.Codes.Freight, Core.Constants.ChargeType.Margin);
				}
				return fFreightChargeCode;
			}
		}

		AccChargeCode fFreightChargeCode;

		#endregion

		#region LoadingChargeCode

		AccChargeCode LoadingChargeCode
		{
			get
			{
				if (fLoadingChargeCode == null)
				{
					fLoadingChargeCode = NewChargeCode(ChargeCodeGroupList.Codes.Loading, ZString.Empty);
				}
				return fLoadingChargeCode;
			}
		}

		AccChargeCode fLoadingChargeCode;

		#endregion

		#region FreightAndDisbursementChargeCode

		AccChargeCode FreightAndDisbursementChargeCode
		{
			get
			{
				if (fFreightAndDisbursementChargeCode == null)
				{
					fFreightAndDisbursementChargeCode = NewChargeCode(ChargeCodeGroupList.Codes.Freight, Core.Constants.ChargeType.Disbursement);
				}
				return fFreightAndDisbursementChargeCode;
			}
		}

		AccChargeCode fFreightAndDisbursementChargeCode;

		#endregion

		#region OriginAndDisbursementChargeCode

		AccChargeCode OriginAndDisbursementChargeCode
		{
			get
			{
				if (fOriginAndDisbursementChargeCode == null)
				{
					fOriginAndDisbursementChargeCode = NewChargeCode(ChargeCodeGroupList.Codes.Origin, Core.Constants.ChargeType.Disbursement);
				}
				return fOriginAndDisbursementChargeCode;
			}
		}

		AccChargeCode fOriginAndDisbursementChargeCode;

		#endregion

		#endregion

		#region New Base Charge

		Charge NewChargeOnShipmentJob(AccChargeCode chargeCode, OrgHeader debtor, RefCurrency currency)
		{
			return NewChargeOnJob(Shipment, chargeCode, debtor, currency);
		}

		Charge NewChargeOnJob(IJobInvoicingPlugIn host, AccChargeCode chargeCode, OrgHeader debtor, RefCurrency currency)
		{
			Job testJob = Factory.NewJobForTesting<Job>();
			testJob.PlugInData = host;

			Charge charge = testJob.Charges.AddNew();
			charge.JR_OH_SellAccount = debtor.PK;
			if (chargeCode != null)
			{
				charge.JR_AC = chargeCode.PK;
			}
			charge.JR_RX_NKSellCurrency = currency.RX_Code;
			return charge;
		}

		ForwardingShipment Shipment
		{
			get { return shipment ?? (shipment = Factory.New<ForwardingShipment>()); }
		}
		ForwardingShipment shipment;

		#endregion

		#region New Charge Code

		AccChargeCode NewChargeCode(ZString chargeCodeGroup, ZString chargeCodeType)
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = chargeCodeGroup;
			chargeCode.AC_ChargeType = chargeCodeType;
			return chargeCode;
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			//AccountingConfigurationRegistry.Instance.DefaultChargesToDeferredTypeWithDeferredConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			LocalCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Env.CurrentCompany.LocalCurrency.Code);
			USCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			AUCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			ForeignCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "CAD");
		}

		RefCurrency AUCurrency;
		RefCurrency USCurrency;
		RefCurrency LocalCurrency;
		RefCurrency ForeignCurrency;

		readonly ZString FinalInvoice = InvoiceTypesList.Codes.FinalInvoice;
		readonly ZString DisbursementInvoice = InvoiceTypesList.Codes.DisbursementInvoice;
		readonly ZString FreightInvoice = InvoiceTypesList.Codes.FreightInvoice;
		readonly ZString InvoicePerTaxCode = InvoiceTypesList.Codes.InvoicePerTaxCode;
		readonly ZString ForeignCurrencyInvoice = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
		readonly ZString DisbursementInForeignCurrencyInvoice = InvoiceTypesList.Codes.DisbursementInForeignCurrency;

		#endregion
	}
}
