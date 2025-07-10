using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocJobCharge))]
	internal class DocJobChargeTest : DocumentWrapperTestCase
	{
		#region Local Amount & Exchange Rate

		public void TestShowLocalAmountAndExRateOnInvoice()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			OrgHeader client = orgFactory.NewWithValidTestData<OrgHeader>();
			orgFactory.Save();

			AssertEquals(ZBool.False, ChargeWrapper.ShowLocalAmountAndExRateOnInvoice);

			client.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup group = client.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			orgFactory.Save();

			Charge.JR_JH = Factory.NewJobForTesting<JobHeader>().PK;
			Charge.Job.LocalChargesPK = client.PK;
			AssertEquals(false, ChargeWrapper.ShowLocalAmountAndExRateOnInvoice);

			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.AllExRate;
			orgFactory.Save();
			AssertEquals(true, ChargeWrapper.ShowLocalAmountAndExRateOnInvoice);
		}

		#endregion

		#region Properties

		#region Profit To Share

		#region Profit To Share with Registry Enable

		public void TestProfitToShare_RegistryEnable_Min()
			=> TestProfitToShare(isRegistryEnable: true, chargeable: 100m, totalContainers: 2, localCost: 10m, localSell: 30m, psPercent: 10, psRate: 0, psRateBasis: string.Empty, psMinimum: 50m, expectedProfitToShare: 2m, expectedTotalProfitToShare: 50m);

		public void TestProfitToShare_RegistryEnable_Percent()
			=> TestProfitToShare(isRegistryEnable: true, chargeable: 100m, totalContainers: 2, localCost: 10m, localSell: 30m, psPercent: 10, psRate: 0, psRateBasis: string.Empty, psMinimum: 0m, expectedProfitToShare: 2m, expectedTotalProfitToShare: 2m);

		public void TestProfitToShare_RegistryEnable_PercentAndChargeable()
			=> TestProfitToShare(isRegistryEnable: true, chargeable: 100m, totalContainers: 2, localCost: 10m, localSell: 30m, psPercent: 10, psRate: 5, psRateBasis: OrgProfitSharePartyLookups.FeeBasisCodes.ChargeableUnit, psMinimum: 0m, expectedProfitToShare: 2m, expectedTotalProfitToShare: 502m);

		public void TestProfitToShare_RegistryEnable_Rate_ChargeableUnit()
			=> TestProfitToShare(isRegistryEnable: true, chargeable: 100m, totalContainers: 2, localCost: 10m, localSell: 30m, psPercent: 0, psRate: 5, psRateBasis: OrgProfitSharePartyLookups.FeeBasisCodes.ChargeableUnit, psMinimum: 0m, expectedProfitToShare: 0m, expectedTotalProfitToShare: 500m);

		public void TestProfitToShare_RegistryEnable_Rate_FlatFee()
			=> TestProfitToShare(isRegistryEnable: true, chargeable: 100m, totalContainers: 2, localCost: 10m, localSell: 30m, psPercent: 0, psRate: 5, psRateBasis: OrgProfitSharePartyLookups.FeeBasisCodes.FlatFee, psMinimum: 0m, expectedProfitToShare: 0m, expectedTotalProfitToShare: 5m);

		public void TestProfitToShare_RegistryEnable_Rate_GrossRevenue()
			=> TestProfitToShare(isRegistryEnable: true, chargeable: 100m, totalContainers: 2, localCost: 10m, localSell: 30m, psPercent: 10, psRate: 0, psRateBasis: OrgProfitSharePartyLookups.FeeBasisCodes.GrossRevenue, psMinimum: 0m, expectedProfitToShare: 3m, expectedTotalProfitToShare: 3m);

		public void TestProfitToShare_RegistryEnable_Rate_PerContainer()
			=> TestProfitToShare(isRegistryEnable: true, chargeable: 100m, totalContainers: 2, localCost: 10m, localSell: 30m, psPercent: 0, psRate: 5, psRateBasis: OrgProfitSharePartyLookups.FeeBasisCodes.PerContainer, psMinimum: 0m, expectedProfitToShare: 0m, expectedTotalProfitToShare: 10m);

		#endregion

		#region Profit To Share with Registry Disable

		public void TestProfitToShare_RegistryDisable_Min()
			=> TestProfitToShare(isRegistryEnable: false, chargeable: 100m, totalContainers: 2, localCost: 10m, localSell: 30m, psPercent: 10, psRate: 0, psRateBasis: string.Empty, psMinimum: 50m, expectedProfitToShare: 2m, expectedTotalProfitToShare: 50m);

		public void TestProfitToShare_RegistryDisable_Percent()
			=> TestProfitToShare(isRegistryEnable: false, chargeable: 100m, totalContainers: 2, localCost: 10m, localSell: 30m, psPercent: 10, psRate: 0, psRateBasis: string.Empty, psMinimum: 0m, expectedProfitToShare: 2m, expectedTotalProfitToShare: 2m);

		public void TestProfitToShare_RegistryDisable_PercentAndChargeable()
			=> TestProfitToShare(isRegistryEnable: false, chargeable: 100m, totalContainers: 2, localCost: 10m, localSell: 30m, psPercent: 10, psRate: 5, psRateBasis: OrgProfitSharePartyLookups.FeeBasisCodes.ChargeableUnit, psMinimum: 0m, expectedProfitToShare: 2m, expectedTotalProfitToShare: 502m);

		public void TestProfitToShare_RegistryDisable_Rate_ChargeableUnit()
			=> TestProfitToShare(isRegistryEnable: false, chargeable: 100m, totalContainers: 2, localCost: 10m, localSell: 30m, psPercent: 0, psRate: 5, psRateBasis: OrgProfitSharePartyLookups.FeeBasisCodes.ChargeableUnit, psMinimum: 0m, expectedProfitToShare: 0m, expectedTotalProfitToShare: 500m);

		public void TestProfitToShare_RegistryDisable_Rate_FlatFee()
			=> TestProfitToShare(isRegistryEnable: false, chargeable: 100m, totalContainers: 2, localCost: 10m, localSell: 30m, psPercent: 0, psRate: 5, psRateBasis: OrgProfitSharePartyLookups.FeeBasisCodes.FlatFee, psMinimum: 0m, expectedProfitToShare: 0m, expectedTotalProfitToShare: 5m);

		public void TestProfitToShare_RegistryDisable_Rate_GrossRevenue()
			=> TestProfitToShare(isRegistryEnable: false, chargeable: 100m, totalContainers: 2, localCost: 10m, localSell: 30m, psPercent: 10, psRate: 0, psRateBasis: OrgProfitSharePartyLookups.FeeBasisCodes.GrossRevenue, psMinimum: 0m, expectedProfitToShare: 3m, expectedTotalProfitToShare: 3m);

		public void TestProfitToShare_RegistryDisable_Rate_PerContainer()
			=> TestProfitToShare(isRegistryEnable: false, chargeable: 100m, totalContainers: 2, localCost: 10m, localSell: 30m, psPercent: 0, psRate: 5, psRateBasis: OrgProfitSharePartyLookups.FeeBasisCodes.PerContainer, psMinimum: 0m, expectedProfitToShare: 0m, expectedTotalProfitToShare: 10m);

		#endregion

		void TestProfitToShare(bool isRegistryEnable, decimal chargeable, int totalContainers, decimal localCost, decimal localSell, decimal psPercent, decimal psRate, string psRateBasis, decimal psMinimum, decimal expectedProfitToShare, decimal expectedTotalProfitToShare = 0m)
		{
			AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, isRegistryEnable);

			var deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();

			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			var forwardingConsol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C00001001");
			forwardingConsol.SetDefaultSendingForwarderAddress(forwarder);

			var forwardingShipment = TestObjectCreator.CreateShipment("S00001000", "AUSYD", "USLAX", consol: forwardingConsol, transportMode: Core.Constants.TransportModes.Air);

			using (var job = Job.CreateWithMutex_ForTestOnly(Factory, forwardingShipment))
			{
				job.Charges.Add(Charge);
				forwardingShipment.JS_OH_DeliveryAgent = deliveryAgent.PK;
				var container1 = forwardingConsol.Containers.AddNew();
				var packLine1 = forwardingShipment.OuterPackLines.AddNew();
				packLine1.JL_PackageCount = 1;
				packLine1.JL_ActualWeight = 10m;
				packLine1.JL_ActualVolume = 100m;
				packLine1.JL_JS = forwardingShipment.PK;
				container1.PackLines.Add(packLine1);

				var container2 = forwardingConsol.Containers.AddNew();
				var packLine2 = forwardingShipment.OuterPackLines.AddNew();
				packLine2.JL_PackageCount = 1;
				packLine2.JL_ActualWeight = 10m;
				packLine2.JL_ActualVolume = 100m;
				packLine2.JL_JS = forwardingShipment.PK;
				container2.PackLines.Add(packLine2);

				var profitShareDetail = new ProfitShareDetail(deliveryAgent, Factory, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent);
				var profitShareShipmentDetail = new ProfitShareShipmentDetail(forwardingShipment, deliveryAgent, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, Factory);
				profitShareDetail.ProfitShareShipmentDetails.Add(profitShareShipmentDetail);

				var creator = new TestObjectCreator(Factory);
				var agentRelationship = creator.CreateAgentRelationship(forwarder, deliveryAgent);
				profitShareShipmentDetail.ProfitShareAgreement = creator.CreateProfitShare(agentRelationship, "AUSYD", "USLAX", "AIR");

				var sendParty = profitShareShipmentDetail.ProfitShareAgreement.PartyDetails.AddNew();
				var rcvParty = profitShareShipmentDetail.ProfitShareAgreement.PartyDetails.AddNew();
				rcvParty.PS_PartyType = "RCV";
				rcvParty.PS_PartyProfitSharePercent = psPercent;
				rcvParty.PS_PartyRate = psRate;
				rcvParty.PS_PartyRateBasis = psRateBasis;
				rcvParty.PS_PartyMinimum = psMinimum;

				var docProfitShareShipmentDetail = DocProfitShareShipmentDetail.New(profitShareShipmentDetail, Factory);
				ChargeWrapper.ProfitShareShipmentDetail = docProfitShareShipmentDetail;

				Charge.JR_JH = job.PK;
				Charge.JR_AC = Env.Registry.FreightChargeCode;
				Charge.JR_RX_NKCostCurrency = "IDR";
				Charge.JR_OSCostExRate = 2;
				Charge.JR_AgentDeclaredCostAmtLocal = localCost;

				Charge.JR_RX_NKSellCurrency = "NZD";
				Charge.JR_OSSellExRate = 2;
				Charge.JR_AgentDeclaredSellAmtLocal = localSell;
				Charge.JR_IsIncludedInProfitShare = true;

				CombineAssertions("Precondition", () =>
				{
					AssertEquals("ActualChargeable", chargeable, forwardingShipment.JS_ActualChargeable); // set on TestObjectCreator > CreateShipment > SetShipmentDefaultsForTest
					AssertEquals("Containers", totalContainers, forwardingShipment.Containers.Count());
					AssertEquals("JR_AgentDeclaredCostAmt", 2 * localCost, Charge.JR_AgentDeclaredCostAmt);
					AssertEquals("JR_AgentDeclaredCostAmt", 2 * localSell, Charge.JR_AgentDeclaredSellAmt);
				});

				AssertEquals("ProfitToShare for single charge", expectedProfitToShare, ChargeWrapper.ProfitToShare);
				AssertEquals("Total Profit Share for shipment", expectedTotalProfitToShare, ChargeWrapper.ProfitShareShipmentDetail.ProfitShareInLocalCurrency);
			}
		}

		#endregion

		public void TestFormattedLocalAmmount()
		{
			Charge.JR_GB = GlbBranch.CurrentBranch.PK;

			Charge.JR_LocalSellAmt = 100;
			Charge.Branch.Country.RN_RX_NKLocalCurrency = ZString.Empty;
			AssertEquals("100.00", ChargeWrapper.FormattedLocalAmmount);

			RefCurrency currency1 = Factory.New<RefCurrency>();
			currency1.RX_Code = "ZZZ";
			currency1.RX_Symbol = "&";

			Charge.Branch.Country.RN_RX_NKLocalCurrency = currency1.RX_Code;

			AssertEquals("&100.00 ZZZ", ChargeWrapper.FormattedLocalAmmount);
		}

		public void TestFormattedLocalAmmountIncTax()
		{
			RefCurrency currency1 = Factory.New<RefCurrency>();
			currency1.RX_Code = "ZZZ";
			currency1.RX_Symbol = "&";

			Charge.JR_GB = GlbBranch.CurrentBranch.PK;
			Charge.Branch.Country.RN_RX_NKLocalCurrency = currency1.RX_Code;
			TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(10);
			Charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			Charge.JR_RX_NKSellCurrency = currency1.RX_Code;
			Charge.JR_OSSellExRate = 1m;
			Charge.JR_LocalSellAmt = 100;
			AssertEquals("&110.00 ZZZ", ChargeWrapper.FormattedLocalAmmountIncTax);
		}

		public void TestFormattedTaxAmmount()
		{
			RefCurrency currency1 = Factory.New<RefCurrency>();
			currency1.RX_Code = "ZZZ";
			currency1.RX_Symbol = "&";
			TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(10);
			Charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			Charge.Branch.Country.RN_RX_NKLocalCurrency = currency1.RX_Code;
			Charge.JR_GB = GlbBranch.CurrentBranch.PK;
			Charge.JR_RX_NKSellCurrency = currency1.RX_Code;
			Charge.JR_OSSellExRate = 1m;
			Charge.JR_LocalSellAmt = 100;
			AssertEquals("&10.00 ZZZ", ChargeWrapper.FormattedTaxAmount);
		}

		public void TestFormattedOSSellAmmount()
		{
			Charge.JR_GB = GlbBranch.CurrentBranch.PK;

			Charge.JR_OSSellAmt = 100;
			Charge.JR_RX_NKSellCurrency = ZString.Empty;
			AssertEquals("100.00", ChargeWrapper.FormattedOSSellAmmount);

			RefCurrency currency1 = Factory.New<RefCurrency>();
			currency1.RX_Code = "ZZZ";
			currency1.RX_Symbol = "&";

			Charge.JR_RX_NKSellCurrency = currency1.RX_Code;
			Charge.JR_OSSellAmt = 100;

			AssertEquals("&100.00 ZZZ", ChargeWrapper.FormattedOSSellAmmount);
		}

		public void TestIsCollectCharge()
		{
			Assert("No charge code - result should be false", !ChargeWrapper.IsCollectCharge(""));

			var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery());
			Charge.JR_AC = chargeCode.PK;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.NotGrouped;
			Assert("Empty IncoTerm should not cause an exception - result is false", !ChargeWrapper.IsCollectCharge(""));

			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			Assert("Result is true", ChargeWrapper.IsCollectCharge("FOB"));
		}

		public void TestChargesAmount()
		{
			AssertEquals("Zero SellAmt should return zero Charges amount", ZDecimal.Zero, ChargeWrapper.OSSellAmt);

			var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery());
			Charge.JR_AC = chargeCode.PK;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.NotGrouped;

			ZDecimal oSSellAmt = new ZDecimal(11.9);
			Charge.JR_OSSellAmt = oSSellAmt;
			ChargeWrapper = DocJobCharge.New(Charge, Factory);

			AssertEquals("Charges amount", oSSellAmt, ChargeWrapper.OSSellAmt);
		}

		public void TestCollectChargesAmount()
		{
			AssertEquals("Empty IncoTerm should return zero Collect Charges amount", ZDecimal.Zero, ChargeWrapper.CollectChargesAmount(""));
			AssertEquals("Zero SellAmt should return zero Collect Charges amount", ZDecimal.Zero, ChargeWrapper.CollectChargesAmount("FOB"));

			var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery());
			Charge.JR_AC = chargeCode.PK;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;

			ZDecimal oSSellAmt = new ZDecimal(11.9);
			Charge.JR_OSSellAmt = oSSellAmt;
			ChargeWrapper = DocJobCharge.New(Charge, Factory);
			AssertEquals("Collect Charges amount", oSSellAmt, ChargeWrapper.CollectChargesAmount("FOB"));
		}

		public void TestChargesDescription()
		{
			var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery());
			Charge.JR_AC = chargeCode.PK;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.NotGrouped;

			chargeCode.AC_Desc = "blah - charge\ncode";
			Charge.JR_Desc = ZString.Empty;
			Charge.JR_AC = chargeCode.PK;
			ChargeWrapper = DocJobCharge.New(Charge, Factory);
			AssertEquals("Charges description = Charge code description", "blah - charge code", ChargeWrapper.GetChargeDescription(DocJobCharge.ChargeDescriptionLanguageModes.LocalLanguage));

			Charge.JR_Desc = "Zayden Charge";
			AssertEquals("Charges description = Job charge description", "Zayden Charge", ChargeWrapper.GetChargeDescription(DocJobCharge.ChargeDescriptionLanguageModes.LocalLanguage));

			Charge.JR_Desc = ZString.Empty;
			AssertEquals("Charges description = Charge code description", "blah - charge code", ChargeWrapper.GetChargeDescription(DocJobCharge.ChargeDescriptionLanguageModes.LocalLanguage));
		}

		public void TestGetChargesInfo()
		{
			var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery());
			Charge.JR_AC = chargeCode.PK;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.OriginBrokerageOnly;
			chargeCode.AC_Desc = ChargeCodeGroupList.Descriptions.OriginBrokerageOnly;
			Charge.JR_Desc = ZString.Empty;
			Charge.JR_AC = chargeCode.PK;
			Charge.JR_OSSellAmt = new ZDecimal(11.9);
			ChargeWrapper = DocJobCharge.New(Charge, Factory);

			AssertEquals("Charges unformatted but description has a default width of 25", "Origin Customs Brokerage  11.90 AUD\n", ChargeWrapper.GetChargesInfo(DocJobCharge.ChargeDescriptionLanguageModes.LocalLanguage));

			ChargeWrapper.ChargeInfoEnbableColumnarFormat = true;
			ChargeWrapper.ChargeInfoDescriptionWidth = 30;
			ChargeWrapper.ChargeInfoGapWidth = 2;
			ChargeWrapper.ChargeInfoAmountWidth = 10;
			AssertEquals("Charges unformatted but description has a default width of 25", "Origin Customs Brokerage (Stan   11.90 AUD\n", ChargeWrapper.GetChargesInfo(DocJobCharge.ChargeDescriptionLanguageModes.LocalLanguage));
		}

		public void TestChargeInfoProperties()
		{
			AssertEquals("Default value for ChargeInfoAmountWidth", 15, ChargeWrapper.ChargeInfoAmountWidth);
			AssertEquals("Default value for ChargeInfoDescriptionWidth", 25, ChargeWrapper.ChargeInfoDescriptionWidth);
			AssertEquals("Default value for ChargeInfoEnbableColumnarFormat", false, ChargeWrapper.ChargeInfoEnbableColumnarFormat);
			AssertEquals("Default value for ChargeInfoGapWidth", 1, ChargeWrapper.ChargeInfoGapWidth);

			ChargeWrapper.ChargeInfoAmountWidth = 16;
			ChargeWrapper.ChargeInfoDescriptionWidth = 30;
			ChargeWrapper.ChargeInfoEnbableColumnarFormat = true;
			ChargeWrapper.ChargeInfoGapWidth = 2;

			AssertEquals("Set value for ChargeInfoAmountWidth", 16, ChargeWrapper.ChargeInfoAmountWidth);
			AssertEquals("Set value for ChargeInfoDescriptionWidth", 30, ChargeWrapper.ChargeInfoDescriptionWidth);
			AssertEquals("Set value for ChargeInfoEnbableColumnarFormat", true, ChargeWrapper.ChargeInfoEnbableColumnarFormat);
			AssertEquals("Set value for ChargeInfoGapWidth", 2, ChargeWrapper.ChargeInfoGapWidth);
		}

		public void TestCollectChargesDescription()
		{
			AssertEquals("Empty IncoTerm should return Empty Collect Charges description", ZString.Empty, ChargeWrapper.CollectChargesDescription("", DocJobCharge.ChargeDescriptionLanguageModes.LocalLanguage));

			var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery());
			Charge.JR_AC = chargeCode.PK;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;

			chargeCode.AC_Desc = "blah - charge\ncode";
			Charge.JR_Desc = ZString.Empty;
			Charge.JR_AC = chargeCode.PK;
			ChargeWrapper = DocJobCharge.New(Charge, Factory);
			AssertEquals("Collect Charges description = Charge code description", "blah - charge code", ChargeWrapper.CollectChargesDescription("FOB", DocJobCharge.ChargeDescriptionLanguageModes.LocalLanguage));
		}

		public void TestChequeNo()
		{
			ZString chequeNo = new ZString("Cheque Number");
			Charge.JR_ChequeNo = chequeNo;
			AssertEquals("Cheque Number", chequeNo, ChargeWrapper.ChequeNo);
		}

		public virtual void TestDescription()
		{
			ZString description = new ZString("Cheque Number");
			Charge.JR_Desc = description;
			AssertEquals("Description", description, ChargeWrapper.Description);

			description = new ZString("Cheque\r\nNumber");
			Charge.JR_Desc = description;
			AssertEquals("Description", description.Replace("\r", ""), ChargeWrapper.Description);
		}

		public void TestPaymentType()
		{
			ZString paymentType = new ZString("PPP");
			Charge.JR_PaymentType = paymentType;
			AssertEquals("PaymentType", paymentType, ChargeWrapper.PaymentType);
		}

		public void TestCostRated()
		{
			Charge.JR_CostRated = ZBool.False;
			Assert("Is not cost rated", !ChargeWrapper.CostRated);

			Charge.JR_CostRated = ZBool.True;
			Assert("Is cost rated", ChargeWrapper.CostRated);
		}

		public void TestCostSplitGroup()
		{
			ZGuid costSplitGroup = ZGuid.NewZGuid();
			Charge.JR_E6 = costSplitGroup;
			AssertEquals("CostSplitGroup", costSplitGroup, ChargeWrapper.CostSplitGroup);
		}

		public void TestDeclaredOSCostAmt()
		{
			ZDecimal declaredOSCostAmt = new ZDecimal(2);
			Charge.JR_DeclaredOSCostAmt = declaredOSCostAmt;
			AssertEquals("DeclaredOSCostAmt", declaredOSCostAmt, ChargeWrapper.DeclaredOSCostAmt);
		}

		public void TestLineCFX()
		{
			ZDecimal lineCFX = new ZDecimal(3);
			Charge.JR_LineCFX = lineCFX;
			AssertEquals("LineCFX", lineCFX, ChargeWrapper.LineCFX);
		}

		public void TestLocalCostAmount()
		{
			ZDecimal localCostAmount = new ZDecimal(4);
			Charge.JR_LocalCostAmt = localCostAmount;
			AssertEquals("LocalCostAmount", localCostAmount, ChargeWrapper.LocalCostAmount);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestLocalSellAmount()
		{
			var localSellAmount = new ZDecimal(5);
			Charge.JR_LocalSellAmt = localSellAmount;
			AssertEquals("LocalSellAmount", localSellAmount, ChargeWrapper.LocalSellAmount);

			var receivableCharge = Charge as IReceivablesPostingCharge;
			AssertNotNull("IReceivablesPostingCharge", receivableCharge);

			var osSellExRate = new ZDecimal(1.3);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.ABIGAS, 5m, TestObjectCreator.AALSHI, 7m);
			Charge.JR_JH = job.PK;
			Charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			Charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			Charge.JR_OSSellAmt = 1000m;
			Charge.JR_OSSellExRate = osSellExRate;
			var usdRate = ((IExchangeRateProvider)job).GetExchangeRate(TestObjectCreator.USD.RX_Code, Charge.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR);
			usdRate.SetBuyRate_ForTestOnly(osSellExRate);
			AssertEquals("OSSellAmount", 1000m, receivableCharge.OSSellAmount);
			AssertEquals("JR_LocalSellAmt", 809.72m, Charge.JR_LocalSellAmt); // = 769.23m / 0.95m - cfx 5m
			AssertEquals("LocalSellAmount", 809.72m, ChargeWrapper.LocalSellAmount);

			Charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
			var eurRate = ((IExchangeRateProvider)job).GetExchangeRate(TestObjectCreator.EUR.RX_Code, Charge.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR);
			eurRate.SetBuyRate_ForTestOnly(1.4m);
			eurRate.EnsureWillNotBeAutoDeleted();
			Assert("BillInInvoice", Charge.BillInInvoiceCurrency);

			AssertEquals("OSSellAmount", 1133.61m, receivableCharge.OSSellAmount);
			AssertEquals("LocalSellAmount with SellInvoiceCurrency", 809.72m, ChargeWrapper.LocalSellAmount);

			TestObjectCreator.SetCurrentCompanyReciprocal(true);

			Charge.JR_OSSellAmt = 2000m;
			AssertEquals("Ex Rate updated on setting OS Amount", 1.365m, Charge.JR_OSSellExRate);
			AssertEquals("OSSellAmount", 1950m, receivableCharge.OSSellAmount);
			AssertEquals("JR_LocalSellAmt", 2730m, Charge.JR_LocalSellAmt);
			AssertEquals("LocalSellAmount with SellInvoiceCurrency", 2730m, ChargeWrapper.LocalSellAmount);

			TestObjectCreator.SetCurrentCompanyReciprocal(false);

			Charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			Charge.JR_OSSellAmt = 1000m;
			Charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
			Assert("BillInInvoiceCurrencyWithLocalSellCurrency", Charge.BillInInvoiceCurrencyWithLocalSellCurrency);

			AssertEquals("OSSellAmount", 1470m, receivableCharge.OSSellAmount);
			AssertEquals("JR_LocalSellAmt", 1000m, Charge.JR_LocalSellAmt);
			AssertEquals("LocalSellAmount with CFX uplift", 1050m, ChargeWrapper.LocalSellAmount);

			TestObjectCreator.SetCurrentCompanyReciprocal(true);
			Charge.JR_OSSellAmt = 2000m;

			AssertEquals("OSSellAmount", 1500m, receivableCharge.OSSellAmount);
			AssertEquals("JR_LocalSellAmt", 2000m, Charge.JR_LocalSellAmt);
			AssertEquals("LocalSellAmount with CFX uplift", 2100m, ChargeWrapper.LocalSellAmount);

			TestObjectCreator.SetCurrentCompanyReciprocal(false);
		}

		public void TestLocalSellAmountForIceland()
		{
			Charge.JR_OSSellAmt = 100;
			Charge.JR_OSSellExRate = 2;
			var localSellAmountForIceland = Env.CurrentCompany.ExchangeRate.ForeignToLocal(ChargeWrapper.OSSellAmtOrEstimatedRevenue, Charge.JR_OSSellExRate);

			AssertEquals("LocalSellAmountForIceland", localSellAmountForIceland, ChargeWrapper.LocalSellAmountForIceland);
		}

		public void TestLocalSellAmountIncTaxForIceland()
		{
			Charge.JR_OSSellAmt = 100;
			Charge.JR_OSSellExRate = 2;
			AssertEquals("LocalSellAmountForIceland", ChargeWrapper.LocalSellAmountForIceland + ChargeWrapper.TaxAmount, ChargeWrapper.LocalSellAmountIncTaxForIceland);
		}

		public void TestSumLocalSellAndTaxAmount()
		{
			Charge charge = Factory.New<Charge>();
			DocJobCharge jobChargeWrapper = DocJobCharge.New(charge, Factory);
			AssertEquals(0M, jobChargeWrapper.TaxAmount);
			TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(10);
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			charge.JR_OSSellAmt = 10M;
			charge.JR_OSSellExRate = 0.001M;

			AssertEquals(1m, charge.JR_OSSellGSTAmt_Calc);
			AssertEquals(1000m, charge.JR_Calc_LocalSellTaxAmt);
			AssertEquals(1000m, jobChargeWrapper.TaxAmount);
			AssertEquals(11000m, jobChargeWrapper.SumLocalSellAndTaxAmount);
		}

		public void TestLocalSellTaxAmount()
		{
			AssertEquals("LocalSellTaxAmount", Charge.JR_Calc_LocalSellTaxAmt, ChargeWrapper.LocalSellTaxAmount);
		}

		public void TestOSCostAmt()
		{
			ZDecimal oSCostAmt = new ZDecimal(7);
			Charge.JR_OSCostAmt = oSCostAmt;
			AssertEquals("OSCostAmt", oSCostAmt, ChargeWrapper.OSCostAmt);
		}

		public void TestOSCostExRate()
		{
			ZDecimal oSCostExRate = new ZDecimal(8);
			Charge.JR_OSCostExRate = oSCostExRate;
			AssertEquals("OSCostExRate", oSCostExRate, ChargeWrapper.OSCostExRate);
		}

		public void TestOSSellAmt()
		{
			ZDecimal oSSellAmt = new ZDecimal(9);
			Charge.JR_OSSellAmt = oSSellAmt;
			AssertEquals("OSSellAmt", oSSellAmt, ChargeWrapper.OSSellAmt);
		}

		public void TestOSSellAmtOrEstimatedRevenue()
		{
			ZDecimal oSSellAmt = new ZDecimal(9);
			Charge.JR_OSSellAmt = oSSellAmt;
			Charge.JR_EstimatedRevenue = oSSellAmt + 10;

			AssertEquals("OSSellAmtOrEstimatedRevenue", Charge.JR_OSSellAmt, ChargeWrapper.OSSellAmtOrEstimatedRevenue);

			Charge.JR_OSSellAmt = 0;

			AssertEquals("OSSellAmtOrEstimatedRevenue", Charge.JR_EstimatedRevenue, ChargeWrapper.OSSellAmtOrEstimatedRevenue);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestOSSellExRate_NonReciprocal()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			AssertEquals("GC_IsReciprocal", false, GlbCompany.CurrentCompany.GC_IsReciprocal);

			var osSellExRate = new ZDecimal(1.3);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.ABIGAS, 5m, TestObjectCreator.AALSHI, 7m);
			Charge.JR_JH = job.PK;
			Charge.JR_OH_SellAccount = job.LocalChargesPK;
			var receivableCharge = Charge as IReceivablesPostingCharge;
			AssertNotNull("IReceivablesPostingCharge", receivableCharge);

			Charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			Charge.JR_OSSellAmt = 1000m;
			var usdRate = ((IExchangeRateProvider)job).GetExchangeRate(TestObjectCreator.USD.RX_Code, Charge.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR);
			usdRate.SetBuyRate_ForTestOnly(osSellExRate);
			usdRate.EnsureWillNotBeAutoDeleted();

			AssertEquals("JR_LocalSellAmt", 809.72m, Charge.JR_LocalSellAmt); //769.23m + cfx (5%)
			AssertEquals("OSSellAmount", 1000m, receivableCharge.OSSellAmount);
			AssertEquals("OSSellExRate", 1.235m, ChargeWrapper.OSSellExRate); // osSellExRate amended by cfx (5%)

			Charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
			Assert("BillInInvoiceCurrency", Charge.BillInInvoiceCurrency);
			var eurRate = ((IExchangeRateProvider)job).GetExchangeRate(TestObjectCreator.EUR.RX_Code, Charge.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR);
			Assert("JF_BuyRate is not set yet", eurRate.Rate.IsEmpty);
			AssertEquals("OSSellAmount", 0m, receivableCharge.OSSellAmount);
			AssertEquals("OSSellExRate with SellInvoiceCurrency but no JF_BuyRate", 0m, ChargeWrapper.OSSellExRate);

			eurRate.SetBuyRate_ForTestOnly(1.4m);
			eurRate.EnsureWillNotBeAutoDeleted();
			AssertEquals("OSSellAmount", 1133.61m, receivableCharge.OSSellAmount); //1076.92m + cfx (5%)
			AssertEquals("OSSellExRate with SellInvoiceCurrency and JF_BuyRate", 0.882138m, ChargeWrapper.OSSellExRate); //0.9286m * 0.95 (cfx 5%)

			Charge.JR_OSSellAmt = 0m;
			AssertEquals("JR_OSSellExRate was updated", 1.235m, Charge.JR_OSSellExRate);
			AssertEquals("IReceivablesPostingCharge.OSSellAmount", 0m, receivableCharge.OSSellAmount);
			AssertEquals("OSSellExRate with SellInvoiceCurrency and zero OSSellAmount", 0m, ChargeWrapper.OSSellExRate);

			Charge.JR_OSSellAmt = 1000m;
			AssertEquals("JR_OSSellExRate", 1.235m, Charge.JR_OSSellExRate);
			AssertEquals("JR_LocalSellAmt", 809.72m, Charge.JR_LocalSellAmt);
			AssertEquals("IReceivablesPostingCharge.OSSellAmount", 1133.61m, receivableCharge.OSSellAmount);
			AssertEquals("OSSellExRate with SellInvoiceCurrency and zero OSSellAmount", 0.882138m, ChargeWrapper.OSSellExRate);

			Charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("JR_OSSellExRate was updated", 1m, Charge.JR_OSSellExRate);
			Assert("BillInInvoiceCurrency", !Charge.BillInInvoiceCurrency);

			Charge.JR_OSSellAmt = 0m;
			AssertEquals("JR_OSSellExRate", 1m, Charge.JR_OSSellExRate);
			AssertEquals("IReceivablesPostingCharge.OSSellAmount", 0m, receivableCharge.OSSellAmount);
			AssertEquals("OSSellExRate with SellInvoiceCurrency and zero OSSellAmount", Charge.JR_OSSellExRate, ChargeWrapper.OSSellExRate);

			Charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
			Assert("BillInInvoiceCurrency", Charge.BillInInvoiceCurrency);

			Charge.JR_OSSellAmt = 1000m;
			AssertEquals("JR_LocalSellAmt", 1000m, Charge.JR_LocalSellAmt);
			AssertEquals("OSSellAmount", 1470m, receivableCharge.OSSellAmount);
			AssertEquals("OSSellExRate with SellInvoiceCurrency and JF_BuyRate", 0.680272m, ChargeWrapper.OSSellExRate);

			Charge.JR_OSSellAmt = 0m;
			AssertEquals("JR_OSSellExRate", 1m, Charge.JR_OSSellExRate);
			AssertEquals("IReceivablesPostingCharge.OSSellAmount", 0m, receivableCharge.OSSellAmount);
			AssertEquals("OSSellExRate with SellInvoiceCurrency and zero OSSellAmount", 0m, ChargeWrapper.OSSellExRate);

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		[DisableZeroExchangeRateOverriding]
		public void TestOSSellExRate_Reciprocal()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			var oldValue = TestObjectCreator.SetCurrentCompanyReciprocal(true);

			var osSellExRate = new ZDecimal(0.76923m);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.ABIGAS, 5m, TestObjectCreator.AALSHI, 7m);
			Charge.JR_JH = job.PK;
			Charge.JR_OH_SellAccount = job.LocalChargesPK;
			var receivableCharge = Charge as IReceivablesPostingCharge;
			AssertNotNull("IReceivablesPostingCharge", receivableCharge);

			Charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			Charge.JR_OSSellAmt = 1000m;
			var usdRate = ((IExchangeRateProvider)job).GetExchangeRate(TestObjectCreator.USD.RX_Code, Charge.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR);
			usdRate.SetBuyRate_ForTestOnly(osSellExRate);
			usdRate.EnsureWillNotBeAutoDeleted();

			AssertEquals("JR_LocalSellAmt", 807.69m, Charge.JR_LocalSellAmt); //769.23m + cfx (5%)
			AssertEquals("OSSellAmount", 1000m, receivableCharge.OSSellAmount);
			AssertEquals("OSSellExRate", 0.807692m, ChargeWrapper.OSSellExRate); //osSellExRate * 1.05 (cfx = 5%)

			Charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
			Assert("BillInInvoiceCurrency", Charge.BillInInvoiceCurrency);

			var eurRate = ((IExchangeRateProvider)job).GetExchangeRate(TestObjectCreator.EUR.RX_Code, Charge.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR);
			Assert("JF_BuyRate is not set yet", eurRate.Rate.IsEmpty);
			AssertEquals("OSSellAmount", 0m, receivableCharge.OSSellAmount);
			AssertEquals("OSSellExRate with SellInvoiceCurrency but no JF_BuyRate", 0m, ChargeWrapper.OSSellExRate);

			eurRate.SetBuyRate_ForTestOnly(0.714285m);
			eurRate.EnsureWillNotBeAutoDeleted();
			AssertEquals("OSSellAmount", 1130.77m, receivableCharge.OSSellAmount); //1076.92m + cfx (5%)
			AssertEquals("OSSellExRate with SellInvoiceCurrency and JF_BuyRate", 1.13077m, ChargeWrapper.OSSellExRate);

			Charge.JR_OSSellAmt = 0m;
			AssertEquals("JR_OSSellExRate", 0.807692m, Charge.JR_OSSellExRate);
			AssertEquals("IReceivablesPostingCharge.OSSellAmount", 0m, receivableCharge.OSSellAmount);
			AssertEquals("OSSellExRate with SellInvoiceCurrency and zero OSSellAmount", 0m, ChargeWrapper.OSSellExRate);

			Charge.JR_OSSellAmt = 2000m;
			AssertEquals("JR_OSSellExRate", 0.807692m, Charge.JR_OSSellExRate);
			AssertEquals("JR_LocalSellAmt", 1615.38m, Charge.JR_LocalSellAmt);
			AssertEquals("IReceivablesPostingCharge.OSSellAmount", 2261.53m, receivableCharge.OSSellAmount);
			AssertEquals("OSSellExRate with SellInvoiceCurrency and JF_BuyRate", 1.130765m, ChargeWrapper.OSSellExRate);

			Charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("JR_OSSellExRate was updated", 1m, Charge.JR_OSSellExRate);
			Assert("BillInInvoiceCurrency", !Charge.BillInInvoiceCurrency);

			Charge.JR_OSSellAmt = 0m;
			AssertEquals("JR_OSSellExRate", 1m, Charge.JR_OSSellExRate);
			AssertEquals("IReceivablesPostingCharge.OSSellAmount", 0m, receivableCharge.OSSellAmount);
			AssertEquals("OSSellExRate with SellInvoiceCurrency and zero OSSellAmount", Charge.JR_OSSellExRate, ChargeWrapper.OSSellExRate);

			Charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
			Assert("BillInInvoiceCurrency", Charge.BillInInvoiceCurrency);

			Charge.JR_OSSellAmt = 2000m;
			AssertEquals("JR_LocalSellAmt", 2000m, Charge.JR_LocalSellAmt);
			AssertEquals("OSSellAmount", 2940m, receivableCharge.OSSellAmount);
			AssertEquals("OSSellExRate with SellInvoiceCurrency and JF_BuyRate", 1.47m, ChargeWrapper.OSSellExRate);

			Charge.JR_OSSellAmt = 0m;
			AssertEquals("JR_OSSellExRate", 1m, Charge.JR_OSSellExRate);
			AssertEquals("IReceivablesPostingCharge.OSSellAmount", 0m, receivableCharge.OSSellAmount);
			AssertEquals("OSSellExRate with SellInvoiceCurrency and zero OSSellAmount", 0m, ChargeWrapper.OSSellExRate);

			TestObjectCreator.SetCurrentCompanyReciprocal(oldValue);

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		public void TestTaxAmount()
		{
			var charge = Factory.New<Charge>();
			var chargeWrapper = DocJobCharge.New(charge, Factory);

			AssertEquals(0M, chargeWrapper.TaxAmount);

			TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(10);
			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			charge.JR_OSSellExRate = 0.001M; //job is null
			charge.JR_LocalSellAmt = 100000m;

			AssertEquals(Env.CurrentCompany.ExchangeRate.ForeignToLocal(10M, 0.001M), chargeWrapper.TaxAmount);

			var osSellExRate = new ZDecimal(1.3);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.ABIGAS, 5m, TestObjectCreator.AALSHI, 7m); //otherwise we will have to amend all assertions by CFX
			charge.JR_JH = job.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			charge.JR_OSSellAmt = 1000m;
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(osSellExRate);
			AssertEquals("JR_LocalSellAmt", 809.72m, charge.JR_LocalSellAmt); //769.23m + cfx
			AssertEquals("JR_Calc_LocalSellTaxAmt", 80.97m, charge.JR_Calc_LocalSellTaxAmt);
			AssertEquals("TaxAmount", 80.97m, chargeWrapper.TaxAmount);

			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
			var eurRate = ((IExchangeRateProvider)job).GetExchangeRate(TestObjectCreator.EUR.RX_Code, charge.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR);
			eurRate.SetBuyRate_ForTestOnly(1.4m);
			Assert("BillInInvoiceCurrency", charge.BillInInvoiceCurrency);

			AssertEquals("JR_LocalSellAmt", 809.72m, charge.JR_LocalSellAmt);
			AssertEquals("JR_Calc_LocalSellTaxAmt", 80.97m, charge.JR_Calc_LocalSellTaxAmt);
			AssertEquals("TaxAmount with SellInvoiceCurrency", 80.97m, chargeWrapper.TaxAmount);

			TestObjectCreator.SetCurrentCompanyReciprocal(true);

			charge.JR_OSSellAmt = 2000m;
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(osSellExRate);
			AssertEquals("JR_LocalSellAmt", 2730m, charge.JR_LocalSellAmt); //2600m + cfx
			AssertEquals("JR_Calc_LocalSellTaxAmt", 273m, charge.JR_Calc_LocalSellTaxAmt);
			AssertEquals("TaxAmount, beware of rounding error!", 273m, chargeWrapper.TaxAmount);

			TestObjectCreator.SetCurrentCompanyReciprocal(false);

			charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			charge.JR_OSSellAmt = 1000m;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
			Assert("BillInInvoiceCurrencyWithLocalSellCurrency", charge.BillInInvoiceCurrencyWithLocalSellCurrency);

			AssertEquals("JR_LocalSellAmt", 1000m, charge.JR_LocalSellAmt);
			AssertEquals("JR_Calc_LocalSellTaxAmt", 100m, charge.JR_Calc_LocalSellTaxAmt);
			AssertEquals("TaxAmount with CFX uplift", 105m, chargeWrapper.TaxAmount);

			TestObjectCreator.SetCurrentCompanyReciprocal(true);
			charge.JR_OSSellAmt = 2000m;

			AssertEquals("JR_LocalSellAmt", 2000m, charge.JR_LocalSellAmt);
			AssertEquals("JR_Calc_LocalSellTaxAmt", 200m, charge.JR_Calc_LocalSellTaxAmt);
			AssertEquals("TaxAmount with CFX uplift", 210m, chargeWrapper.TaxAmount);

			TestObjectCreator.SetCurrentCompanyReciprocal(false);
		}

		public void TestTaxAmount_With_UseLocalExTaxAmountWhileCalculatingLocalTaxAmount()
		{
			TestObjectCreator.SetCurrentCompanyCountryCode(Core.Constants.CountryCodes.Chile);
			TestObjectCreator.SetCurrentCompanyReciprocal(true);

			var taxRate = TestObjectCreator.CreateTaxRate("IVA", "Chile IVA", 19);

			var charge = Factory.New<Charge>();
			DocJobCharge jobChargeWrapper = DocJobCharge.New(charge, Factory);
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge.JR_OSSellAmt = 10495.13M;
			charge.JR_OSSellExRate = 664.9865M;

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			charge.JR_AT_SellGSTRate = taxRate.PK;

			AssertEquals("Precondition: Local ex tax amount", 6979120M, charge.JR_LocalSellAmt);
			AssertEquals("Local tax is calculated applying 19% tax rate to local ex tax amount", 1326033M, jobChargeWrapper.TaxAmount);

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			charge.JR_AT_SellGSTRate = Guid.Empty;

			charge.JR_AT_SellGSTRate = taxRate.PK;

			AssertEquals("Precondition: OS tax amount", 1994.07M, charge.JR_OSSellGSTAmt_Calc);
			AssertEquals("Local tax is calculated by converting os tax amount to local tax amount using exchange rate", 1326030M, jobChargeWrapper.TaxAmount);
		}

		public void TestTaxAmountHasProperRoundingForCurrentCompanyCurrency()
		{
			AssertEquals("Precondition - CurrentCompany Currency SubUnitRatio", 100, GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio);

			var charge = Factory.New<Charge>();
			DocJobCharge jobChargeWrapper = DocJobCharge.New(charge, Factory);

			TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(10);
			charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			charge.JR_LocalSellAmt = 555.55m;

			AssertEquals("Should have proper rounding based on the GlbCompany.CurrentCompany.LocalCurrency", 55.56M, jobChargeWrapper.TaxAmount);
		}

		public void TestTaxAmountForTaiwanCompanyWithHeaderLevelTaxCalculation()
		{
			var taiwanCompany = TestObjectCreator.CreateCompanyAndBranch("TWTPE");
			taiwanCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Taiwan;
			taiwanCompany.GC_IsReciprocal = true;
			Factory.Save();

			using (TestObjectCreator.SwitchEnvToCompany(taiwanCompany))
			using (AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetTemporaryValue(taiwanCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Current company country should be Taiwan", Core.Constants.CountryCodes.Taiwan, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				AssertEquals("Current company currency should be Taiwanese", Core.Constants.CurrencyCodes.Taiwan, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

				TestObjectCreator.AALSHI.CompanyData.OB_IsDebtor = true;

				var shipment = TestObjectCreator.CreateShipment("S1", "AUSYD", "TWTPE", transportMode: Core.Constants.TransportCodes.Air);
				var job = (Job)new JobHeader.Loader(shipment).TryCreate();
				job.JH_OA_LocalChargesAddr = TestObjectCreator.AALSHI.MainAddress.PK;
				job.JH_GE = TestObjectCreator.FIADepartment.PK;
				job.AddCurrency(TestObjectCreator.USD, 33.122m, TestObjectCreator.AALSHI.PK, ExchangeRateValidLedgerEnum.AR);
				Factory.Save();

				TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(5);

				JobCharge AddJobCharge(AccChargeCode chargeCode, ZString sellCurrency, ZDecimal sellAmount)
				{
					var jobCharge = (JobCharge)job.Charges.AddNew();
					jobCharge.JR_GE = TestObjectCreator.FIADepartment.PK;
					jobCharge.JR_AC = chargeCode.PK;
					jobCharge.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
					jobCharge.JR_RX_NKSellCurrency = sellCurrency;
					jobCharge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
					jobCharge.JR_OSSellAmt = sellAmount;
					jobCharge.JR_InvoiceType = InvoiceTypesList.Codes.InvoicePerTaxCode;
					return jobCharge;
				}

				var charge1 = AddJobCharge(TestObjectCreator.CC1, Core.Constants.CurrencyCodes.Taiwan, 850m);
				var charge2 = AddJobCharge(TestObjectCreator.CC2, Core.Constants.CurrencyCodes.Taiwan, 2835m);
				var charge3 = AddJobCharge(TestObjectCreator.CC3, Core.Constants.CurrencyCodes.UnitedStates, 619.18m);
				var charge4 = AddJobCharge(TestObjectCreator.CC4, Core.Constants.CurrencyCodes.UnitedStates, 25m);

				AssertEquals("Should be same as job exchange rate", charge3.JR_OSSellExRate, 33.122m);
				AssertEquals("Should be same as job exchange rate", charge4.JR_OSSellExRate, 33.122m);

				AssertEquals("Local currency same as os", 850m, charge1.JR_LocalSellAmt);
				AssertEquals("Local currency same as os", 2835m, charge2.JR_LocalSellAmt);
				AssertEquals("Local currency same as os", 20508m, charge3.JR_LocalSellAmt);
				AssertEquals("Local currency same as os", 828m, charge4.JR_LocalSellAmt);

				AssertEquals("Should round tax rate amount to 0 decimals", 43m, charge1.JR_Calc_LocalSellTaxAmt);
				AssertEquals("Should round tax rate amount to 0 decimals", 142m, charge2.JR_Calc_LocalSellTaxAmt);
				AssertEquals("Should round tax rate amount to 0 decimals", 1025m, charge3.JR_Calc_LocalSellTaxAmt);
				AssertEquals("Should round tax rate amount to 0 decimals", 41m, charge4.JR_Calc_LocalSellTaxAmt);

				job.Validation.ValidateAll();
				AssertNoErrors(job);
				Factory.Save();

				var docShipmentJobHeader = DocShipment.New(shipment, Factory).JobHeader;
				AssertEquals("There should be 4 charges", 4, docShipmentJobHeader.JobChargesForDebtorOrLocalClient.Count);
				var docJobCharge1 = docShipmentJobHeader.JobChargesForDebtorOrLocalClient.FindByPKOfWrappedObject(charge1.PK);
				var docJobCharge2 = docShipmentJobHeader.JobChargesForDebtorOrLocalClient.FindByPKOfWrappedObject(charge2.PK);
				var docJobCharge3 = docShipmentJobHeader.JobChargesForDebtorOrLocalClient.FindByPKOfWrappedObject(charge3.PK);
				var docJobCharge4 = docShipmentJobHeader.JobChargesForDebtorOrLocalClient.FindByPKOfWrappedObject(charge4.PK);

				AssertEquals(43m, docJobCharge1.TaxAmount);
				AssertEquals(142m, docJobCharge2.TaxAmount);
				AssertEquals(1025m, docJobCharge3.TaxAmount);
				AssertEquals(41m, docJobCharge4.TaxAmount);
			}
		}

		public void TestTaxAmountWhenARLineIsPosted()
		{
			var charge = Factory.New<JobCharge>();
			DocJobCharge jobChargeWrapper = DocJobCharge.New(charge, Factory);

			AssertEquals(0M, jobChargeWrapper.TaxAmount);

			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellExRate = 0.001M;

			AccTransactionLines line = Factory.New<AccTransactionLines>();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			charge.JR_AL_ARLine = line.PK;
			charge.ARLine.AL_GSTVAT = 50M;

			AssertEquals(charge.ARLine.AL_GSTVAT, jobChargeWrapper.TaxAmount);
		}

		public void TestBranch()
		{
			Charge.JR_GB = ZGuid.Empty;
			AssertNull("Branch should be null", ChargeWrapper.Branch);

			Charge.JR_GB = GlbBranch.CurrentBranch.PK;
			AssertNotNull("Branch should not be null", ChargeWrapper.Branch);
			AssertEquals("Should be of type DocBranch", typeof(DocBranch), ChargeWrapper.Branch.GetType());
		}

		public void TestDepartment()
		{
			Charge.JR_GE = ZGuid.Empty;
			AssertNull("Department should be null", ChargeWrapper.Department);

			Charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			AssertNotNull("Department should not be null", ChargeWrapper.Department);
			AssertEquals("Should be of type DocDepartment", typeof(DocDepartment), ChargeWrapper.Department.GetType());
		}

		public void TestJob()
		{
			AssertNull("Job should be null", ChargeWrapper.JobHeader);

			var header = Factory.NewJobForTesting<JobHeader>();
			Charge.JR_JH = header.PK;
			AssertNotNull("Job should not be null", ChargeWrapper.JobHeader);
			AssertEquals("Should be of type DocJobHeader", typeof(DocJobHeader), ChargeWrapper.JobHeader.GetType());
		}

		public void TestCostAccount()
		{
			AssertNull("Cost account should be null", ChargeWrapper.CostAccount);

			var header = Factory.New<OrgHeader>();
			Charge.JR_OH_CostAccount = header.PK;
			AssertNotNull("Cost account should not be null", ChargeWrapper.CostAccount);
			AssertEquals("Should be of type DocOrganisation", typeof(DocOrganisation), ChargeWrapper.CostAccount.GetType());
		}

		public void TestSellAccount()
		{
			AssertNull("Sell account should be null", ChargeWrapper.CostAccount);

			var header = Factory.New<OrgHeader>();
			Charge.JR_OH_SellAccount = header.PK;
			AssertNotNull("Sell account should not be null", ChargeWrapper.SellAccount);
			AssertEquals("Should be of type DocOrganisation", typeof(DocOrganisation), ChargeWrapper.SellAccount.GetType());
		}

		public void TestPaymentDate()
		{
			ZDateTime paymentDate = new ZDateTime(2004, 01, 01);
			Charge.JR_PaymentDate = paymentDate;
			AssertEquals("Payment date", paymentDate, ChargeWrapper.PaymentDate);
		}

		public void TestOSCostCurrency()
		{
			Charge.JR_RX_NKCostCurrency = ZString.Empty;
			AssertNull("OSCostCurrency account should be null", ChargeWrapper.OSCostCurrency);

			var currency = Factory.New<RefCurrency>();
			currency.RX_Code = Core.Constants.CurrencyCodes.UnitedStates;
			Charge.JR_RX_NKCostCurrency = currency.RX_Code;
			AssertNotNull("OSCostCurrency should not be null", ChargeWrapper.OSCostCurrency);
			AssertEquals("Should be of type DocCurrency", typeof(DocCurrency), ChargeWrapper.OSCostCurrency.GetType());
		}

		public void TestOSSellCurrency()
		{
			Charge.JR_RX_NKSellCurrency = ZString.Empty;
			AssertNull("OSSellCurrency account should be null", ChargeWrapper.OSSellCurrency);

			var currency = Factory.New<RefCurrency>();
			currency.RX_Code = Core.Constants.CurrencyCodes.UnitedStates;
			Charge.JR_RX_NKSellCurrency = currency.RX_Code;
			AssertNotNull("OSSellCurrency should not be null", ChargeWrapper.OSSellCurrency);
			AssertEquals("Should be of type DocCurrency", typeof(DocCurrency), ChargeWrapper.OSSellCurrency.GetType());
		}

		public void TestSellRated()
		{
			Charge.JR_SellRated = ZBool.False;
			Assert("Is not sell rated", !ChargeWrapper.SellRated);

			Charge.JR_SellRated = ZBool.True;
			Assert("Is sell rated", ChargeWrapper.SellRated);
		}

		public void TestToString()
		{
			ZString description = new ZString("Description");
			Charge.JR_Desc = description;
			AssertEquals("ToString()", description, ChargeWrapper.ToString());
		}

		public void TestBankAccount()
		{
			AssertNull("BankAccount account should be null", ChargeWrapper.BankAccount);

			var bankAccount = Factory.New<AccBankAccount>();
			Charge.JR_AB = bankAccount.PK;
			AssertNotNull("BankAccount should not be null", ChargeWrapper.BankAccount);
			AssertEquals("Should be of type DocBankAccount", typeof(DocBankAccount), ChargeWrapper.BankAccount.GetType());
		}

		public void TestChargeCode()
		{
			AssertNull("ChargeCode account should be null", ChargeWrapper.ChargeCode);

			var chargeCode = Factory.New<AccChargeCode>();
			Charge.JR_AC = chargeCode.PK;
			AssertNotNull("ChargeCode should not be null", ChargeWrapper.ChargeCode);
			AssertEquals("Should be of type DocChargeCode", typeof(DocChargeCode), ChargeWrapper.ChargeCode.GetType());
		}

		public void TestExchangeRateAndAmount()
		{
			var audCurr = TestObjectCreator.AUD;
			var usdCurr = TestObjectCreator.USD;
			var eurCurr = TestObjectCreator.EUR;

			var orgFactory = new BusinessObjectFactory();
			var client = orgFactory.NewWithValidTestData<OrgHeader>();
			client.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			var group = client.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			orgFactory.Save();

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Charge.JR_JH = job.PK;
			var receivableCharge = Charge as IReceivablesPostingCharge;
			AssertNotNull("IReceivablesPostingCharge", receivableCharge);

			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = audCurr.RX_Code;
			Charge.JR_RX_NKSellCurrency = audCurr.RX_Code;
			Charge.JR_OSSellAmt = 1000M;
			AssertEquals("OSSellAmount", 1000m, receivableCharge.OSSellAmount);
			AssertEquals("OSSellExRate", 1m, ChargeWrapper.OSSellExRate);
			AssertEquals("", ChargeWrapper.ExchangeRateAndAmount);

			job.LocalChargesPK = client.PK;
			AssertEquals("", ChargeWrapper.ExchangeRateAndAmount);

			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.AllExRate;
			orgFactory.Save();

			AssertEquals("", ChargeWrapper.ExchangeRateAndAmount);

			Charge.JR_RX_NKSellCurrency = usdCurr.RX_Code;
			Charge.JR_OSSellAmt = 1000M;
			var usdRate = ((IExchangeRateProvider)job).GetExchangeRate(TestObjectCreator.USD.RX_Code, Charge.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR);
			usdRate.SetBuyRate_ForTestOnly(0.78m);
			usdRate.EnsureWillNotBeAutoDeleted();
			AssertEquals(1282.05m, Charge.JR_LocalSellAmt);
			AssertEquals("OSSellAmount", 1000m, receivableCharge.OSSellAmount);
			AssertEquals("OSSellExRate", 0.78m, ChargeWrapper.OSSellExRate);
			AssertEquals("USD 1000.00 @ 0.780000", ChargeWrapper.ExchangeRateAndAmount);

			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
			orgFactory.Save();

			AssertEquals("", ChargeWrapper.ExchangeRateAndAmount);

			Charge.JR_RX_NKSellInvoiceCurrency = eurCurr.RX_Code;
			var eurRate = ((IExchangeRateProvider)job).GetExchangeRate(TestObjectCreator.EUR.RX_Code, Charge.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR);
			eurRate.SetBuyRate_ForTestOnly(1.4m);
			AssertEquals("OSSellAmount", 1794.87m, receivableCharge.OSSellAmount);
			AssertEquals("OSSellExRate", 0.557143m, ChargeWrapper.OSSellExRate);

			AssertEquals("", ChargeWrapper.ExchangeRateAndAmount);

			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.AllExRate;
			orgFactory.Save();

			AssertEquals("USD 1000.00 @ 0.557143", ChargeWrapper.ExchangeRateAndAmount);

			Charge.JR_RX_NKSellInvoiceCurrency = usdCurr.RX_Code;

			AssertEquals("JR_LocalSellAmt", 1282.05m, Charge.JR_LocalSellAmt);
			AssertEquals("OSSellAmount", 1000m, receivableCharge.OSSellAmount);
			AssertEquals("USD 1000.00 @ 1.000000", ChargeWrapper.ExchangeRateAndAmount);

			Charge.JR_RX_NKSellCurrency = audCurr.RX_Code;
			Charge.JR_OSSellAmt = 1000M;
			Charge.JR_RX_NKSellInvoiceCurrency = usdCurr.RX_Code;
			AssertEquals("JR_LocalSellAmt", 1000m, Charge.JR_LocalSellAmt);
			AssertEquals("OSSellAmount", 780m, receivableCharge.OSSellAmount);
			AssertEquals("AUD 1000.00 @ 1.282051", ChargeWrapper.ExchangeRateAndAmount);
		}

		public void TestIsRevenuePosted()
		{
			var charge = Factory.New<JobCharge>();
			DocJobCharge chargeWrapper = DocJobCharge.New(charge, Factory);

			AssertEquals("Charge is not posted", false, chargeWrapper.IsRevenuePosted);

			AccTransactionLines line = Factory.New<AccTransactionLines>();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			charge.JR_AL_ARLine = line.PK;

			AssertEquals("IsRevenuePosted", true, chargeWrapper.IsRevenuePosted);
		}

		public void TestIsProfitShareCharge()
		{
			Assert("No charge code - IsProfitShareCharge should be false", !ChargeWrapper.IsProfitShare);

			Charge.JR_AC = AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value;
			Assert("IsProfitShareCharge should be true", ChargeWrapper.IsProfitShare);
		}

		#endregion

		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocJobCharge.New(Charge, Factory) };
		}

		protected JobCharge Charge;
		protected DocJobCharge ChargeWrapper;

		protected override void SetUp()
		{
			Charge = Factory.New<JobCharge>();
			ChargeWrapper = DocJobCharge.New(Charge, Factory);
			AssertNotNull("PreCondition: Valid DocJobCharge", ChargeWrapper);

			base.SetUp();
		}

		#endregion
	}
}
