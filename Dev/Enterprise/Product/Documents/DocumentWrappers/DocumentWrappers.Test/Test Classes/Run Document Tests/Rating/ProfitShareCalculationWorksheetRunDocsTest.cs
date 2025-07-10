using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class ProfitShareCalculationWorksheetRunDocsTest : BaseRunDocumentsTest
	{
		public void TestTemplate()
		{
			var sendParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			sendParty.PS_PartyType = "SEN";
			sendParty.PS_PartyProfitSharePercent = 10;
			var rcvParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyProfitSharePercent = 90;

			Factory.Save();

			RunDocumentWithAllSections = ZBool.True;
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Profit Share Calculation Worksheet");
			AssertRunDocument
			(
				(IDocumentSupportable)GetBusinessObject,
				@"{C}-[<Image(CompanyLogo,1,47)>]

{C}-[Consol <Consol.ConsolNumber> - Profit Share Calculation]

{F}-[2]   {G}-[<RecipientNameAndAddress>]   {AH}-[PAGE:]   {AP}-[<Current Page> of <TotalPages>]
{AH}-[DATE:]   {AP}-[<Now>]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]
{C}-[<Consol.SendingForwarder.Name>]   {AA}-[<Consol.ReceivingForwarder.Name>]
{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{C}-[<OrgToCredit.Name>]   {AA}-[PS <Consol.ConsolNumber>]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[<Consol.MasterBillNum>]   {S}-[<Consol.TotalWeight> <Consol.WeightUnit>]   {AA}-[<Consol.TotalVolume> <Consol.VolumeUnit>]   {AI}-[<Consol.ETD>]   {AQ}-[<Consol.ETA>]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[<Consol.FirstLoadPort.PortName>]   {AA}-[<Consol.LastDischargePort.PortName>]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]



{C}-[<ShipmentChargesDetails.ChargeCode.Desc>]   {S}-[<ShipmentChargesDetails.AgentDeclaredSellAmount>]   {AA}-[<ShipmentChargesDetails.AgentDeclaredCostAmount>]   {AI}-[<ShipmentChargesDetails.AgentDeclaredProfit>]   {AQ}-[<ShipmentChargesDetails.ProfitToShare>]

{C}-[SHIPMENT <ShipmentChargesDetails.ProfitShareShipmentDetail.JobNumber>  -  <ShipmentChargesDetails.ProfitShareShipmentDetail.RelevantAgentPercent> % of <ShipmentChargesDetails.ProfitShareShipmentDetail.ProfitShareAgreement.AgreementType>]

{C}-[Basis Charge]   {AI}-[Minimum]   {AQ}-[Profit Share]
{C}-[Flat Fee]   {AI}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.PartyMinimum>]   {AQ}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.CalculatedBasisRateTotal>]

{C}-[Basis Charge]   {S}-[Minimum]   {AA}-[Quantity]   {AI}-[Rate]   {AQ}-[Profit Share]
{C}-[Per Chargeable Unit]   {S}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.PartyMinimum>]   {AA}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.ActualChargeable>]   {AI}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.PartyRate>]   {AQ}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.CalculatedBasisRateTotal>]

{C}-[Basis Charge]   {S}-[Minimum]   {AA}-[Containers]   {AI}-[Rate]   {AQ}-[Profit Share]
{C}-[Per Container]   {S}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.PartyMinimum>]   {AA}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.ContainerCount>]   {AI}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.PartyRate>]   {AQ}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.CalculatedBasisRateTotal>]

{C}-[Basis Charge]   {AQ}-[Minimum]
{C}-[Gross Revenue]   {AQ}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.PartyMinimum>]

{C}-[Basis Charge]   {AQ}-[Minimum]
{C}-[N/A]   {AQ}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.PartyMinimum>]


{C}-[Shipment Total]   {R}-[<Total ShipmentChargesDetails.AgentDeclaredSellAmount>]   {X}-[<CurrentCompany.Currency>]   {Z}-[<Total ShipmentChargesDetails.AgentDeclaredCostAmount>]   {AF}-[<CurrentCompany.Currency>]   {AH}-[<Total ShipmentChargesDetails.AgentDeclaredProfit>]   {AN}-[<CurrentCompany.Currency>]   {AP}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.ProfitShareInLocalCurrency>]   {AV}-[<CurrentCompany.Currency>]









{C}-[TOTAL PROFIT SHARE]   {AE}-[0]   {AU}-[<CurrentCompany.Currency>]
{C}-[<Image(CompanyLogo,1,47)>]

{C}-[Consol <Consol.ConsolNumber> - Profit Share Calculation]

{F}-[2]   {G}-[<RecipientNameAndAddress>]   {AH}-[PAGE:]   {AP}-[<Current Page> of <TotalPages>]
{AH}-[DATE:]   {AP}-[<Now>]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]
{C}-[<Consol.SendingForwarder.Name>]   {AA}-[<Consol.ReceivingForwarder.Name>]
{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{C}-[<OrgToCredit.Name>]   {AA}-[PS <Consol.ConsolNumber>]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[<Consol.MasterBillNum>]   {S}-[<Consol.TotalWeight> <Consol.WeightUnit>]   {AA}-[<Consol.TotalVolume> <Consol.VolumeUnit>]   {AI}-[<Consol.ETD>]   {AQ}-[<Consol.ETA>]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[<Consol.FirstLoadPort.PortName>]   {AA}-[<Consol.LastDischargePort.PortName>]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]



{C}-[<ShipmentChargesDetails.ChargeCode.Desc>]   {S}-[<ShipmentChargesDetails.AgentDeclaredSellAmount>]   {AA}-[<ShipmentChargesDetails.AgentDeclaredCostAmount>]   {AI}-[<ShipmentChargesDetails.AgentDeclaredProfit>]   {AQ}-[<ShipmentChargesDetails.ProfitToShare>]

{C}-[SHIPMENT <ShipmentChargesDetails.ProfitShareShipmentDetail.JobNumber>  -  <ShipmentChargesDetails.ProfitShareShipmentDetail.RelevantAgentPercent> % of <ShipmentChargesDetails.ProfitShareShipmentDetail.ProfitShareAgreement.AgreementType>]

{C}-[Basis Charge]   {AI}-[Minimum]   {AQ}-[Profit Share]
{C}-[Flat Fee]   {AI}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.PartyMinimum>]   {AQ}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.CalculatedBasisRateTotal>]

{C}-[Basis Charge]   {S}-[Minimum]   {AA}-[Quantity]   {AI}-[Rate]   {AQ}-[Profit Share]
{C}-[Per Chargeable Unit]   {S}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.PartyMinimum>]   {AA}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.ActualChargeable>]   {AI}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.PartyRate>]   {AQ}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.CalculatedBasisRateTotal>]

{C}-[Basis Charge]   {S}-[Minimum]   {AA}-[Containers]   {AI}-[Rate]   {AQ}-[Profit Share]
{C}-[Per Container]   {S}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.PartyMinimum>]   {AA}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.ContainerCount>]   {AI}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.PartyRate>]   {AQ}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.CalculatedBasisRateTotal>]

{C}-[Basis Charge]   {AQ}-[Minimum]
{C}-[Gross Revenue]   {AQ}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.PartyMinimum>]

{C}-[Basis Charge]   {AQ}-[Minimum]
{C}-[N/A]   {AQ}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.PartyMinimum>]


{C}-[Shipment Total]   {R}-[<Total ShipmentChargesDetails.AgentDeclaredSellAmount>]   {X}-[<CurrentCompany.Currency>]   {Z}-[<Total ShipmentChargesDetails.AgentDeclaredCostAmount>]   {AF}-[<CurrentCompany.Currency>]   {AH}-[<Total ShipmentChargesDetails.AgentDeclaredProfit>]   {AN}-[<CurrentCompany.Currency>]   {AP}-[<ShipmentChargesDetails.ProfitShareShipmentDetail.ProfitShareInLocalCurrency>]   {AV}-[<CurrentCompany.Currency>]









{C}-[TOTAL PROFIT SHARE]   {AE}-[0]   {AU}-[<CurrentCompany.Currency>]
"
			);
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_Percent()
		{
			var sendParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			sendParty.PS_PartyType = "SEN";
			sendParty.PS_PartyProfitSharePercent = 10;
			var rcvParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyProfitSharePercent = 90;

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Local Charge > AgentDeclaredCostAmt", 1000m, LocalCharge.JR_AgentDeclaredCostAmt);
				AssertEquals("Local Charge > AgentDeclaredSellAmt", 2000m, LocalCharge.JR_AgentDeclaredSellAmt);
			});

			RunDocumentWithAllSections = ZBool.False;
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Profit Share Calculation Worksheet");
			{
				AssertRunDocument
				(
					ForwardingConsol,
					@"{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  10 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[100]

{C}-[Shipment Total]   {R}-[2000]   {X}-[ERN]   {Z}-[1000]   {AF}-[ERN]   {AH}-[1000]   {AN}-[ERN]   {AP}-[100]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[100]   {AU}-[ERN]
{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  90 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[900]

{C}-[Shipment Total]   {R}-[2000]   {X}-[ERN]   {Z}-[1000]   {AF}-[ERN]   {AH}-[1000]   {AN}-[ERN]   {AP}-[900]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[900]   {AU}-[ERN]
",
					message: "PS_PartyProfitSharePercent x (AgentDeclaredSellAmt - AgentDeclaredCostAmt)"
				);
			}
		}

		#region Rate

		[TestDate(2020, 1, 1)]
		public void TestDocument_Rate_ChargeableUnit()
		{
			var sendParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			sendParty.PS_PartyType = "SEN";
			sendParty.PS_PartyRate = 5m;
			sendParty.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.ChargeableUnit;

			var rcvParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyRate = 6m;
			rcvParty.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.ChargeableUnit;

			Factory.Save();

			AssertEquals("Precondition: Shipment ActualChargeable", 100m, ForwardingShipment.JS_ActualChargeable);

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Profit Share Calculation Worksheet");
			{
				AssertRunDocument
				(
					ForwardingConsol,
					@"{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  0 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[0]
{C}-[Basis Charge]   {S}-[Minimum]   {AA}-[Quantity]   {AI}-[Rate]   {AQ}-[Profit Share]
{C}-[Per Chargeable Unit]   {S}-[0]   {AA}-[100]   {AI}-[5]   {AQ}-[500]

{C}-[Shipment Total]   {R}-[2000]   {X}-[ERN]   {Z}-[1000]   {AF}-[ERN]   {AH}-[1000]   {AN}-[ERN]   {AP}-[500]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[500]   {AU}-[ERN]
{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  0 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[0]
{C}-[Basis Charge]   {S}-[Minimum]   {AA}-[Quantity]   {AI}-[Rate]   {AQ}-[Profit Share]
{C}-[Per Chargeable Unit]   {S}-[0]   {AA}-[100]   {AI}-[6]   {AQ}-[600]

{C}-[Shipment Total]   {R}-[2000]   {X}-[ERN]   {Z}-[1000]   {AF}-[ERN]   {AH}-[1000]   {AN}-[ERN]   {AP}-[600]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[600]   {AU}-[ERN]
",
					message: "PS_PartyRate x JS_ActualChargeable"
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_Rate_FlatFee()
		{
			var sendParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			sendParty.PS_PartyType = "SEN";
			sendParty.PS_PartyRate = 5m;
			sendParty.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.FlatFee;

			var rcvParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyRate = 6m;
			rcvParty.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.FlatFee;

			Factory.Save();

			AssertEquals("Precondition: Shipment ActualChargeable", 100m, ForwardingShipment.JS_ActualChargeable);

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Profit Share Calculation Worksheet");
			{
				AssertRunDocument
				(
					ForwardingConsol,
					@"{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  0 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[0]
{C}-[Basis Charge]   {AI}-[Minimum]   {AQ}-[Profit Share]
{C}-[Flat Fee]   {AI}-[0]   {AQ}-[5]

{C}-[Shipment Total]   {R}-[2000]   {X}-[ERN]   {Z}-[1000]   {AF}-[ERN]   {AH}-[1000]   {AN}-[ERN]   {AP}-[5]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[5]   {AU}-[ERN]
{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  0 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[0]
{C}-[Basis Charge]   {AI}-[Minimum]   {AQ}-[Profit Share]
{C}-[Flat Fee]   {AI}-[0]   {AQ}-[6]

{C}-[Shipment Total]   {R}-[2000]   {X}-[ERN]   {Z}-[1000]   {AF}-[ERN]   {AH}-[1000]   {AN}-[ERN]   {AP}-[6]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[6]   {AU}-[ERN]
",
					message: "PS_PartyRate"
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_Rate_GrossRevenue()
		{
			var sendParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			sendParty.PS_PartyType = "SEN";
			sendParty.PS_PartyProfitSharePercent = 10m;
			sendParty.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.GrossRevenue;

			var rcvParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyProfitSharePercent = 90;
			rcvParty.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.GrossRevenue;

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Shipment ActualChargeable", 100m, ForwardingShipment.JS_ActualChargeable);
				AssertEquals("Charge JR_AgentDeclaredCostAmt", 1000m, LocalCharge.JR_AgentDeclaredCostAmt);
				AssertEquals("Charge JR_AgentDeclaredSellAmtLocal (Profit)", 2000m, LocalCharge.JR_AgentDeclaredSellAmtLocal);
			});

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Profit Share Calculation Worksheet");
			{
				AssertRunDocument
				(
					ForwardingConsol,
					@"{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  10 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[200]

{C}-[Shipment Total]   {R}-[2000]   {X}-[ERN]   {Z}-[1000]   {AF}-[ERN]   {AH}-[1000]   {AN}-[ERN]   {AP}-[200]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[200]   {AU}-[ERN]
{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  90 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[1800]

{C}-[Shipment Total]   {R}-[2000]   {X}-[ERN]   {Z}-[1000]   {AF}-[ERN]   {AH}-[1000]   {AN}-[ERN]   {AP}-[1800]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[1800]   {AU}-[ERN]
",
					message: "PS_PartyProfitSharePercent x JR_AgentDeclaredSellAmtLocal"
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_Rate_GrossRevenueWithMinimum()
		{
			var sendParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			sendParty.PS_PartyType = "SEN";
			sendParty.PS_PartyProfitSharePercent = 10m;
			sendParty.PS_PartyMinimum = 50m;
			sendParty.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.GrossRevenue;

			var rcvParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyProfitSharePercent = 90;
			rcvParty.PS_PartyMinimum = 100m;
			rcvParty.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.GrossRevenue;

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Shipment ActualChargeable", 100m, ForwardingShipment.JS_ActualChargeable);
				AssertEquals("Charge JR_AgentDeclaredCostAmt", 1000m, LocalCharge.JR_AgentDeclaredCostAmt);
				AssertEquals("Charge JR_AgentDeclaredSellAmtLocal (Profit)", 2000m, LocalCharge.JR_AgentDeclaredSellAmtLocal);
			});

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Profit Share Calculation Worksheet");
			{
				AssertRunDocument
				(
					ForwardingConsol,
					@"{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  10 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[200]
{C}-[Basis Charge]   {AQ}-[Minimum]
{C}-[Gross Revenue]   {AQ}-[50]

{C}-[Shipment Total]   {R}-[2000]   {X}-[ERN]   {Z}-[1000]   {AF}-[ERN]   {AH}-[1000]   {AN}-[ERN]   {AP}-[200]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[200]   {AU}-[ERN]
{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  90 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[1800]
{C}-[Basis Charge]   {AQ}-[Minimum]
{C}-[Gross Revenue]   {AQ}-[100]

{C}-[Shipment Total]   {R}-[2000]   {X}-[ERN]   {Z}-[1000]   {AF}-[ERN]   {AH}-[1000]   {AN}-[ERN]   {AP}-[1800]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[1800]   {AU}-[ERN]
",
					message: "PS_PartyProfitSharePercent x JR_AgentDeclaredSellAmtLocal"
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_Rate_PerContainer()
		{
			var container1 = ForwardingConsol.Containers.AddNew();
			var packLine1 = ForwardingShipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_ActualWeight = 10m;
			packLine1.JL_ActualVolume = 100m;
			packLine1.JL_JS = ForwardingShipment.PK;
			container1.PackLines.Add(packLine1);

			var container2 = ForwardingConsol.Containers.AddNew();
			var packLine2 = ForwardingShipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 1;
			packLine2.JL_ActualWeight = 10m;
			packLine2.JL_ActualVolume = 100m;
			packLine2.JL_JS = ForwardingShipment.PK;
			container2.PackLines.Add(packLine2);

			AssertEquals("Precondition: Shipment Containers", 2, ForwardingShipment.Containers.Count());

			var sendParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			sendParty.PS_PartyType = "SEN";
			sendParty.PS_PartyRate = 5m;
			sendParty.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.PerContainer;

			var rcvParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyRate = 6m;
			rcvParty.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.PerContainer;

			Factory.Save();

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Profit Share Calculation Worksheet");
			{
				AssertRunDocument
				(
					ForwardingConsol,
					@"{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  0 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[0]
{C}-[Basis Charge]   {S}-[Minimum]   {AA}-[Containers]   {AI}-[Rate]   {AQ}-[Profit Share]
{C}-[Per Container]   {S}-[0]   {AA}-[2]   {AI}-[5]   {AQ}-[10]

{C}-[Shipment Total]   {R}-[2000]   {X}-[ERN]   {Z}-[1000]   {AF}-[ERN]   {AH}-[1000]   {AN}-[ERN]   {AP}-[10]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[10]   {AU}-[ERN]
{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  0 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[0]
{C}-[Basis Charge]   {S}-[Minimum]   {AA}-[Containers]   {AI}-[Rate]   {AQ}-[Profit Share]
{C}-[Per Container]   {S}-[0]   {AA}-[2]   {AI}-[6]   {AQ}-[12]

{C}-[Shipment Total]   {R}-[2000]   {X}-[ERN]   {Z}-[1000]   {AF}-[ERN]   {AH}-[1000]   {AN}-[ERN]   {AP}-[12]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[12]   {AU}-[ERN]
",
					message: "Total Containers x PS_PartyRate"
				);
			}
		}

		#endregion

		[TestDate(2020, 1, 1)]
		public void TestDocument_Minimum()
		{
			var sendParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			sendParty.PS_PartyType = "SEN";
			sendParty.PS_PartyMinimum = 10000m;

			var rcvParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyMinimum = 20000m;

			Factory.Save();

			AssertEquals("Precondition: Shipment ActualChargeable", 100m, ForwardingShipment.JS_ActualChargeable);

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Profit Share Calculation Worksheet");
			{
				AssertRunDocument
				(
					ForwardingConsol,
					@"{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  0 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[0]
{C}-[Basis Charge]   {AQ}-[Minimum]
{C}-[N/A]   {AQ}-[10000]

{C}-[Shipment Total]   {R}-[2000]   {X}-[ERN]   {Z}-[1000]   {AF}-[ERN]   {AH}-[1000]   {AN}-[ERN]   {AP}-[10000]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[10000]   {AU}-[ERN]
{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  0 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[0]
{C}-[Basis Charge]   {AQ}-[Minimum]
{C}-[N/A]   {AQ}-[20000]

{C}-[Shipment Total]   {R}-[2000]   {X}-[ERN]   {Z}-[1000]   {AF}-[ERN]   {AH}-[1000]   {AN}-[ERN]   {AP}-[20000]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[20000]   {AU}-[ERN]
",
					message: "PS_PartyMinimum"
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_PercentAndChargeable()
		{
			var sendParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			sendParty.PS_PartyType = "SEN";
			sendParty.PS_PartyProfitSharePercent = 10m;
			sendParty.PS_PartyRate = 5m;
			sendParty.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.ChargeableUnit;
			sendParty.PS_PartyMinimum = 10m;

			var rcvParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyProfitSharePercent = 90m;
			rcvParty.PS_PartyRate = 6m;
			rcvParty.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.ChargeableUnit;
			rcvParty.PS_PartyMinimum = 100m;

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Local Charge > AgentDeclaredCostAmt", 1000m, LocalCharge.JR_AgentDeclaredCostAmt);
				AssertEquals("Local Charge > AgentDeclaredSellAmt", 2000m, LocalCharge.JR_AgentDeclaredSellAmt);
				AssertEquals("Shipment ActualChargeable", 100m, ForwardingShipment.JS_ActualChargeable);
			});

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Profit Share Calculation Worksheet");
			{
				AssertRunDocument
				(
					ForwardingConsol,
					@"{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  10 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[100]
{C}-[Basis Charge]   {S}-[Minimum]   {AA}-[Quantity]   {AI}-[Rate]   {AQ}-[Profit Share]
{C}-[Per Chargeable Unit]   {S}-[10]   {AA}-[100]   {AI}-[5]   {AQ}-[500]

{C}-[Shipment Total]   {R}-[2000]   {X}-[ERN]   {Z}-[1000]   {AF}-[ERN]   {AH}-[1000]   {AN}-[ERN]   {AP}-[600]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[600]   {AU}-[ERN]
{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  90 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[900]
{C}-[Basis Charge]   {S}-[Minimum]   {AA}-[Quantity]   {AI}-[Rate]   {AQ}-[Profit Share]
{C}-[Per Chargeable Unit]   {S}-[100]   {AA}-[100]   {AI}-[6]   {AQ}-[600]

{C}-[Shipment Total]   {R}-[2000]   {X}-[ERN]   {Z}-[1000]   {AF}-[ERN]   {AH}-[1000]   {AN}-[ERN]   {AP}-[1500]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[1500]   {AU}-[ERN]
",
					message: "[PS_PartyProfitSharePercent x  (AgentDeclaredSellAmt - AgentDeclaredCostAmt) ] + [PS_PartyRate x JS_ActualChargeable]"
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestGivenChargesAllAreDeletedInJob_WhenGenerateDocument_ShouldGenerateCorrectly()
		{
			var sendParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			sendParty.PS_PartyType = "SEN";
			sendParty.PS_PartyMinimum = 10000m;

			var rcvParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyMinimum = 20000m;

			Factory.Save();

			AssertEquals("Precondition: Shipment ActualChargeable", 100m, ForwardingShipment.JS_ActualChargeable);

			var job = ForwardingShipment.Job as Job;
			job.Charges.Remove(LocalCharge);

			RunDocumentWithAllSections = ZBool.False;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Profit Share Calculation Worksheet");
			{
				AssertRunDocument
				(
					ForwardingConsol,
					@"{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]





{C}-[TOTAL PROFIT SHARE]   {AE}-[0]   {AU}-[ERN]
{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]





{C}-[TOTAL PROFIT SHARE]   {AE}-[0]   {AU}-[ERN]
",
					message: "Given all charges are deleted In Job, When generating Profit Share Calculation Worksheet, Should print correctly."
				);
			}
		}

		#region Currencies

		[TestDate(2020, 1, 1)]
		public void TestDocument_Currencies_RegistryDisabled()
		{
			AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);

			var foreignCharge = CreateProfitShareChargeLocal
			(
				(Job)ForwardingShipment.Job,
				TestObjectCreator.CC1.PK.ToGuid(),
				agentDeclaredCostAmtLocal: 3000m,
				agentDeclaredSellAmtLocal: 4000m,
				costCurrency: TestObjectCreator.GBP.RX_Code,
				sellCurrency: TestObjectCreator.GBP.RX_Code
			);

			var sendParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			sendParty.PS_PartyType = "SEN";
			sendParty.PS_PartyProfitSharePercent = 10;
			var rcvParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyProfitSharePercent = 90;

			Factory.Save();

			CombineAssertions("Precondition: charge", () =>
			{
				AssertEquals("Local Charge: Local Cost", 1000m, LocalCharge.JR_AgentDeclaredCostAmtLocal);
				AssertEquals("Local Charge: Local Sell (Profit)", 2000m, LocalCharge.JR_AgentDeclaredSellAmtLocal);
				AssertEquals("Local Charge: Cost", 1000m, LocalCharge.JR_AgentDeclaredCostAmt);
				AssertEquals("Local Charge: Sell (Profit)", 2000m, LocalCharge.JR_AgentDeclaredSellAmt);

				AssertEquals("Foreign Charge: Local Cost", 3000m, foreignCharge.JR_AgentDeclaredCostAmtLocal);
				AssertEquals("Foreign Charge: Local Sell", 4000m, foreignCharge.JR_AgentDeclaredSellAmtLocal);
				AssertEquals("Foreign Charge: Cost", 6000m, foreignCharge.JR_AgentDeclaredCostAmt);
				AssertEquals("Foreign Charge: Sell", 8000m, foreignCharge.JR_AgentDeclaredSellAmt);
				AssertEquals("Foreign Charge: Cost Exchange Rate", 2m, foreignCharge.JR_OSCostExRate);
				AssertEquals("Foreign Charge: Sell Exchange Rate", 2m, foreignCharge.JR_OSSellExRate);
			});

			RunDocumentWithAllSections = ZBool.False;
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Profit Share Calculation Worksheet");
			{
				AssertRunDocument
				(
					ForwardingConsol,
					@"{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  10 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[100]
{C}-[Charge Code 1]   {S}-[4000]   {AA}-[3000]   {AI}-[1000]   {AQ}-[100]

{C}-[Shipment Total]   {R}-[6000]   {X}-[ERN]   {Z}-[4000]   {AF}-[ERN]   {AH}-[2000]   {AN}-[ERN]   {AP}-[200]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[200]   {AU}-[ERN]
{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  90 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[900]
{C}-[Charge Code 1]   {S}-[4000]   {AA}-[3000]   {AI}-[1000]   {AQ}-[900]

{C}-[Shipment Total]   {R}-[6000]   {X}-[ERN]   {Z}-[4000]   {AF}-[ERN]   {AH}-[2000]   {AN}-[ERN]   {AP}-[1800]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[1800]   {AU}-[ERN]
",
					message: "Should only consider local currency"
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_Currencies_RegistryEnabled()
		{
			AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			var foreignCharge = CreateProfitShareChargeLocal
			(
				(Job)ForwardingShipment.Job,
				TestObjectCreator.CC1.PK.ToGuid(),
				agentDeclaredCostAmtLocal: 3000m,
				agentDeclaredSellAmtLocal: 4000m,
				costCurrency: TestObjectCreator.GBP.RX_Code,
				sellCurrency: TestObjectCreator.GBP.RX_Code
			);

			var sendParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			sendParty.PS_PartyType = "SEN";
			sendParty.PS_PartyProfitSharePercent = 10;
			var rcvParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyProfitSharePercent = 90;

			Factory.Save();

			CombineAssertions("Precondition: charge", () =>
			{
				AssertEquals("Local Charge: Local Cost", 1000m, LocalCharge.JR_AgentDeclaredCostAmtLocal);
				AssertEquals("Local Charge: Local Sell (Profit)", 2000m, LocalCharge.JR_AgentDeclaredSellAmtLocal);
				AssertEquals("Local Charge: Cost", 1000m, LocalCharge.JR_AgentDeclaredCostAmt);
				AssertEquals("Local Charge: Sell (Profit)", 2000m, LocalCharge.JR_AgentDeclaredSellAmt);

				AssertEquals("Foreign Charge: Local Cost", 3000m, foreignCharge.JR_AgentDeclaredCostAmtLocal);
				AssertEquals("Foreign Charge: Local Sell", 4000m, foreignCharge.JR_AgentDeclaredSellAmtLocal);
				AssertEquals("Foreign Charge: Cost", 6000m, foreignCharge.JR_AgentDeclaredCostAmt);
				AssertEquals("Foreign Charge: Sell", 8000m, foreignCharge.JR_AgentDeclaredSellAmt);
				AssertEquals("Foreign Charge: Cost Exchange Rate", 2m, foreignCharge.JR_OSCostExRate);
				AssertEquals("Foreign Charge: Sell Exchange Rate", 2m, foreignCharge.JR_OSSellExRate);
			});

			RunDocumentWithAllSections = ZBool.False;
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Profit Share Calculation Worksheet");
			{
				AssertRunDocument
				(
					ForwardingConsol,
					@"{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  10 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[100]
{C}-[Charge Code 1]   {S}-[4000]   {AA}-[3000]   {AI}-[1000]   {AQ}-[100]

{C}-[Shipment Total]   {R}-[6000]   {X}-[ERN]   {Z}-[4000]   {AF}-[ERN]   {AH}-[2000]   {AN}-[ERN]   {AP}-[200]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[200]   {AU}-[ERN]
{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  90 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[900]
{C}-[Charge Code 1]   {S}-[4000]   {AA}-[3000]   {AI}-[1000]   {AQ}-[900]

{C}-[Shipment Total]   {R}-[6000]   {X}-[ERN]   {Z}-[4000]   {AF}-[ERN]   {AH}-[2000]   {AN}-[ERN]   {AP}-[1800]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[1800]   {AU}-[ERN]
",
					message: "Each currencies should have its own section with total"
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_Currencies_DifferentCostAndSellCurrencies_RegistryDisabled()
		{
			AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);

			var foreignCharge = CreateProfitShareChargeLocal
			(
				(Job)ForwardingShipment.Job,
				TestObjectCreator.CC1.PK.ToGuid(),
				agentDeclaredCostAmtLocal: 3000m,
				agentDeclaredSellAmtLocal: 4000m,
				costCurrency: TestObjectCreator.GBP.RX_Code,
				sellCurrency: TestObjectCreator.IDR.RX_Code
			);

			var sendParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			sendParty.PS_PartyType = "SEN";
			sendParty.PS_PartyProfitSharePercent = 10;
			var rcvParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyProfitSharePercent = 90;

			Factory.Save();

			CombineAssertions("Precondition: charge", () =>
			{
				AssertEquals("Local Charge: Local Cost", 1000m, LocalCharge.JR_AgentDeclaredCostAmtLocal);
				AssertEquals("Local Charge: Local Sell (Profit)", 2000m, LocalCharge.JR_AgentDeclaredSellAmtLocal);
				AssertEquals("Local Charge: Cost", 1000m, LocalCharge.JR_AgentDeclaredCostAmt);
				AssertEquals("Local Charge: Sell (Profit)", 2000m, LocalCharge.JR_AgentDeclaredSellAmt);

				AssertEquals("Foreign Charge: Local Cost", 3000m, foreignCharge.JR_AgentDeclaredCostAmtLocal);
				AssertEquals("Foreign Charge: Local Sell", 4000m, foreignCharge.JR_AgentDeclaredSellAmtLocal);
				AssertEquals("Foreign Charge: Cost", 6000m, foreignCharge.JR_AgentDeclaredCostAmt);
				AssertEquals("Foreign Charge: Sell", 8000m, foreignCharge.JR_AgentDeclaredSellAmt);
				AssertEquals("Foreign Charge: Cost Exchange Rate", 2m, foreignCharge.JR_OSCostExRate);
				AssertEquals("Foreign Charge: Sell Exchange Rate", 2m, foreignCharge.JR_OSSellExRate);
			});

			RunDocumentWithAllSections = ZBool.False;
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Profit Share Calculation Worksheet");
			{
				AssertRunDocument
				(
					ForwardingConsol,
					@"{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  10 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[100]
{C}-[Charge Code 1]   {S}-[4000]   {AA}-[3000]   {AI}-[1000]   {AQ}-[100]

{C}-[Shipment Total]   {R}-[6000]   {X}-[ERN]   {Z}-[4000]   {AF}-[ERN]   {AH}-[2000]   {AN}-[ERN]   {AP}-[200]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[200]   {AU}-[ERN]
{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  90 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[900]
{C}-[Charge Code 1]   {S}-[4000]   {AA}-[3000]   {AI}-[1000]   {AQ}-[900]

{C}-[Shipment Total]   {R}-[6000]   {X}-[ERN]   {Z}-[4000]   {AF}-[ERN]   {AH}-[2000]   {AN}-[ERN]   {AP}-[1800]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[1800]   {AU}-[ERN]
",
					message: "Should only consider local currency"
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestDocument_Currencies_DifferentCostAndSellCurrencies_RegistryEnabled()
		{
			AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			var foreignCharge = CreateProfitShareChargeLocal
			(
				(Job)ForwardingShipment.Job,
				TestObjectCreator.CC1.PK.ToGuid(),
				agentDeclaredCostAmtLocal: 3000m,
				agentDeclaredSellAmtLocal: 4000m,
				costCurrency: TestObjectCreator.GBP.RX_Code,
				sellCurrency: TestObjectCreator.IDR.RX_Code
			);

			var sendParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			sendParty.PS_PartyType = "SEN";
			sendParty.PS_PartyProfitSharePercent = 10;
			var rcvParty = ProfitShareShipment.ProfitShareAgreement.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyProfitSharePercent = 90;

			Factory.Save();

			CombineAssertions("Precondition: charge", () =>
			{
				AssertEquals("Local Charge: Local Cost", 1000m, LocalCharge.JR_AgentDeclaredCostAmtLocal);
				AssertEquals("Local Charge: Local Sell (Profit)", 2000m, LocalCharge.JR_AgentDeclaredSellAmtLocal);
				AssertEquals("Local Charge: Cost", 1000m, LocalCharge.JR_AgentDeclaredCostAmt);
				AssertEquals("Local Charge: Sell (Profit)", 2000m, LocalCharge.JR_AgentDeclaredSellAmt);

				AssertEquals("Foreign Charge: Local Cost", 3000m, foreignCharge.JR_AgentDeclaredCostAmtLocal);
				AssertEquals("Foreign Charge: Local Sell", 4000m, foreignCharge.JR_AgentDeclaredSellAmtLocal);
				AssertEquals("Foreign Charge: Cost", 6000m, foreignCharge.JR_AgentDeclaredCostAmt);
				AssertEquals("Foreign Charge: Sell", 8000m, foreignCharge.JR_AgentDeclaredSellAmt);
				AssertEquals("Foreign Charge: Cost Exchange Rate", 2m, foreignCharge.JR_OSCostExRate);
				AssertEquals("Foreign Charge: Sell Exchange Rate", 2m, foreignCharge.JR_OSSellExRate);
			});

			RunDocumentWithAllSections = ZBool.False;
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Profit Share Calculation Worksheet");
			{
				AssertRunDocument
				(
					ForwardingConsol,
					@"{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  10 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[100]
{C}-[Charge Code 1]   {S}-[4000]   {AA}-[3000]   {AI}-[1000]   {AQ}-[100]

{C}-[Shipment Total]   {R}-[6000]   {X}-[ERN]   {Z}-[4000]   {AF}-[ERN]   {AH}-[2000]   {AN}-[ERN]   {AP}-[200]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[200]   {AU}-[ERN]
{C}-[Consol C00001001 - Profit Share Calculation]

{F}-[2]   {G}-[#1|>ATTENTION: THE ACCOUNTS PAYABLE MANAGER]   {AH}-[PAGE:]   {AP}-[1 of 1]
{AH}-[DATE:]   {AP}-[43831]






{C}-[SENDING AGENT]   {AA}-[RECEIVING AGENT]

{C}-[ORGANISATION TO CREDIT]   {AA}-[INVOICE NUMBER]
{AA}-[PS C00001001]
{C}-[MASTER BILL NUMBER]   {S}-[WEIGHT]   {AA}-[VOLUME]   {AI}-[ ETD]   {AQ}-[ ETA]
{C}-[081]   {S}-[0 KG]   {AA}-[0.6 M3]   {AI}-[43841]   {AQ}-[43844]
{C}-[PORT OF LOADING]   {AA}-[PORT OF DISCHARGE]
{C}-[Sydney]   {AA}-[Los Angeles]



{C}-[PROFIT SHARED CHARGES]   {S}-[REVENUE]   {AA}-[COST]   {AI}-[PROFIT]   {AQ}-[PROFIT SHARE]


{C}-[SHIPMENT S00001000  -  90 % of Prepaid and Collect Freight Charges]
{C}-[International Freight]   {S}-[2000]   {AA}-[1000]   {AI}-[1000]   {AQ}-[900]
{C}-[Charge Code 1]   {S}-[4000]   {AA}-[3000]   {AI}-[1000]   {AQ}-[900]

{C}-[Shipment Total]   {R}-[6000]   {X}-[ERN]   {Z}-[4000]   {AF}-[ERN]   {AH}-[2000]   {AN}-[ERN]   {AP}-[1800]   {AV}-[ERN]








{C}-[TOTAL PROFIT SHARE]   {AE}-[1800]   {AU}-[ERN]
",
					message: "Each currency should have its own section with total"
				);
			}
		}

		#endregion

		#region Implementation

		public override BusinessObject GetBusinessObject => ForwardingConsol;

		public override BusinessContext BusinessContext => BusinessContext.Consol;

		protected override void SetUp()
		{
			base.SetUp();

			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			ForwardingConsol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C00001001");
			ForwardingConsol.SetDefaultSendingForwarderAddress(forwarder);

			var deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();

			ForwardingShipment = TestObjectCreator.CreateShipment("S00001000", "AUSYD", "USLAX", consol: ForwardingConsol, transportMode: Core.Constants.TransportModes.Air);
			ForwardingShipment.JS_OH_DeliveryAgent = deliveryAgent.PK;

			var profitShare = new ProfitShareDetail(deliveryAgent, Factory, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent);
			ProfitShareShipment = new ProfitShareShipmentDetail(ForwardingShipment, deliveryAgent, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, Factory);
			profitShare.ProfitShareShipmentDetails.Add(ProfitShareShipment);

			using (var job = Job.CreateWithMutex_ForTestOnly(Factory, ForwardingShipment))
			{
				job.JH_JobNum = "111";
				LocalCharge = CreateProfitShareCharge(job, Env.Registry.FreightChargeCode, agentDeclaredCostAmt: 1000m, agentDeclaredSellAmt: 2000m);
				ProfitShareShipment.SetJobForTesting(job);

				job.AddCurrency(TestObjectCreator.GBP, 2m, Integration.Accounting.ExchangeRateValidLedgerEnum.AP);
				job.AddCurrency(TestObjectCreator.GBP, 2m, Integration.Accounting.ExchangeRateValidLedgerEnum.AR);

				job.AddCurrency(TestObjectCreator.IDR, 2m, Integration.Accounting.ExchangeRateValidLedgerEnum.AP);
				job.AddCurrency(TestObjectCreator.IDR, 2m, Integration.Accounting.ExchangeRateValidLedgerEnum.AR);
			}

			var creator = new TestObjectCreator(Factory);
			var agentRelationship = creator.CreateAgentRelationship(forwarder, deliveryAgent);

			ProfitShareShipment.ProfitShareAgreement = creator.CreateProfitShare(agentRelationship, "AUSYD", "USLAX", "AIR");

			Factory.Save();
		}

		ForwardingConsol ForwardingConsol;

		ForwardingShipment ForwardingShipment;

		ProfitShareShipmentDetail ProfitShareShipment;

		Charge LocalCharge;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		static Charge CreateProfitShareCharge(Job job, Guid chargePK, decimal agentDeclaredCostAmt, decimal agentDeclaredSellAmt)
		{
			var charge = job.Charges.AddNew();
			charge.JR_AC = chargePK;

			charge.JR_AgentDeclaredCostAmt = agentDeclaredCostAmt;
			charge.JR_AgentDeclaredSellAmt = agentDeclaredSellAmt;

			charge.JR_IsIncludedInProfitShare = true;

			return charge;
		}

		static Charge CreateProfitShareChargeLocal(Job job, Guid chargePK, decimal agentDeclaredCostAmtLocal, decimal agentDeclaredSellAmtLocal, string costCurrency, string sellCurrency)
		{
			var charge = job.Charges.AddNew();
			charge.JR_AC = chargePK;

			charge.JR_RX_NKCostCurrency = costCurrency;
			charge.JR_RX_NKSellCurrency = sellCurrency;

			charge.JR_AgentDeclaredCostAmtLocal = agentDeclaredCostAmtLocal;
			charge.JR_AgentDeclaredSellAmtLocal = agentDeclaredSellAmtLocal;

			charge.JR_IsIncludedInProfitShare = true;

			return charge;
		}

		#endregion
	}
}
