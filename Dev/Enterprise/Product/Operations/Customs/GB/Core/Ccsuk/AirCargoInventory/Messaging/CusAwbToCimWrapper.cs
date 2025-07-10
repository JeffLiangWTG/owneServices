using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CusAwbToCimWrapper : CusAwbToInventoryMessageGenerator
	{
		public CusAwbToCimWrapper(CcsukTransmissionMessageFunction how, ICcsukCusAwb iAwb, ErrorCollector ec)
			: base(iAwb)
		{
			howAsCim = how as CcsukTransmissionMessageFunction.CIM;
			if (howAsCim == null)
			{
				throw new ArgumentException("'how' must be type CcsukTransmissionMessageFunction.CIM.  Type=" + how.GetType().FullName);
			}
			errorCollector = ec;
		}

		public CusAwbToCimWrapper(BusinessObjectFactory factory, CcsukTransmissionMessageFunction how)
			: this(factory)
		{
			howAsCim = how as CcsukTransmissionMessageFunction.CIM;
		}

		public CusAwbToCimWrapper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override string MakeMessageText()
		{
			var baseCimWrapper = this.GetCargoImpBaseMessage();
			return WrapAndInterpret(baseCimWrapper);
		}

		protected virtual CargoImpBase GetCargoImpBaseMessage()
		{
			CargoImpBase baseCimWrapper = null;
			var hawb = Awb as CusHAWB;
			var splitHouse = Awb as SplitHouse;
			ZString houseBillNumber = hawb != null ? hawb.CS_HAWB : splitHouse != null ? splitHouse.HAWB.CS_HAWB : ZString.Empty;

			var howAsCimFrn = howAsCim as CcsukTransmissionMessageFunction.CIM.FRN;
			var howAsCimFcs = howAsCim as CcsukTransmissionMessageFunction.CIM.FCS;
			var howAsCimFrd = howAsCim as CcsukTransmissionMessageFunction.CIM.FRD;
			var howAsCimFsr = howAsCim as CcsukTransmissionMessageFunction.CIM.FSR;
			var howAsCimDrp = howAsCim as CcsukTransmissionMessageFunction.CIM.DRP;
			howAsCimFsa = howAsCim as CcsukTransmissionMessageFunction.CIM.FSA;

			if (howAsCimFrn != null)
			{
				baseCimWrapper = new CIMFRN(Awb.CargoTerminalOperatorAirport, Awb.CargoTerminalOperator, Awb.MasterBill, houseBillNumber, Awb.SplitReference, howAsCimFrn.NewAgent, errorCollector);
			}
			else if (howAsCimFcs != null)
			{
				baseCimWrapper = new CIMFCS(Awb.CargoTerminalOperatorAirport, Awb.CargoTerminalOperator, Awb.MasterBill, houseBillNumber, howAsCimFcs.Splits, errorCollector);
			}
			else if (howAsCimFrd != null)
			{
				SendSplitConsignmentRequestOrPackagesArrivedSplitAllocation(ref baseCimWrapper, ref houseBillNumber, howAsCimFrd);
			}
			else if (howAsCimFsa != null)
			{
				baseCimWrapper = new CIMFSA_OSI(Awb, howAsCimFsa.OtherShipmentInformationLine, errorCollector);
			}
			else if (howAsCimFsr != null)
			{
				baseCimWrapper = new CIMFSR(Awb.MasterBill, errorCollector);
			}
			else if (howAsCimDrp != null)
			{
				baseCimWrapper = new CIMDRP(howAsCimDrp.Wrapper, errorCollector);
			}

			if (baseCimWrapper == null)
			{
				throw new NotImplementedException("Cannot make that type of CIM message.  Type=" + howAsCim.GetType().FullName);
			}
			return baseCimWrapper;
		}

		void SendSplitConsignmentRequestOrPackagesArrivedSplitAllocation(ref CargoImpBase baseCimWrapper, ref ZString houseBillNumber, CcsukTransmissionMessageFunction.CIM.FRD howAsCimFrd)
		{
			var cred = CredentialsSetting.GetCredentialsForBadge(Awb.AgentBadge, Awb.Branch.Company.PK);
			var badge = cred != null ? cred.Company : Awb.AgentBadge;
			baseCimWrapper = new CIMFRD(Awb.CargoTerminalOperatorAirport, Awb.CargoTerminalOperator, Awb.MasterBill, houseBillNumber, howAsCimFrd.SplitsAndFlightData, errorCollector, badge);
		}

		string WrapAndInterpret(CargoImpBase baseCimWrapper)
		{
			var cimGenerator = new CargoFACTWrapper(baseCimWrapper);
			var cargoImpLines = new ZStringBuilder(baseCimWrapper.AllCargoImpLinesIncludingType).ToStringWithDelimiterBetweenAppends(" <br> \r\n");
			cargoImpForInterpretation = string.Format("{0} {1} <br> \r\n {2}", MessagePrettierCss.CSS, baseCimWrapper.MessageInterpretation, cargoImpLines);
			var fsa = howAsCim as CcsukTransmissionMessageFunction.CIM.FSA;
			var result = "";
			if (fsa != null && !fsa.CommonAccessReference.IsEmpty)
			{
				result = cimGenerator.Wrap(fsa.CommonAccessReference);
			}
			else
			{
				result = cimGenerator.Wrap();
			}
			return result;
		}

		public override ZString RecipientPima
		{
			get
			{
				var airportAndShed = Awb.CargoTerminalOperatorAirport + Awb.CargoTerminalOperator;
				return GetRecipientPimaForAirportAndShed(airportAndShed);
			}
		}

		internal ZString GetRecipientPimaForAirportAndShed(string airportAndShed)
		{
			if (howAsCim != null)
			{
				var standardEtsfPima = string.Format("CUK{0}98{1}", "AIR", airportAndShed);

				if (howAsCimFsa != null)
				{
					return howAsCimFsa.IncomingMessage.Interchange.EI_From;  // bounce back to sender
				}

				// Renom and FRD/genral split requests go to shed, FCS to the CommDB. For FRD and FRN, there are some exceptions to the standard pima for these airline sheds....
				else if (howAsCim.MessageSubType == CcsukTransmissionMessageFunction.CIM.FRD.SubCode ||
						howAsCim.MessageSubType == CcsukTransmissionMessageFunction.CIM.FRN.Subcode)
				{
					return GBCustomsDataRegistry.Instance.CcsukNonStandardPimas.Value.ConvertAirportAndShedForMessageType(airportAndShed, howAsCim.MessageSubType, standardEtsfPima);
				}
				else if (howAsCim.MessageSubType == CcsukTransmissionMessageFunction.CIM.DRP.Subcode)
				{
					return standardEtsfPima;
				}
				else if (howAsCim.MessageSubType != CcsukTransmissionMessageFunction.CUSCAR.FCS.Subcode)
				{
					// Renom and FRD/genral split requests go to shed, FCS to the CommDB.
					return standardEtsfPima;
				}
			}
			return base.RecipientPima; // CommDB
		}

		public override ZString MessageInterpretation
		{
			get { return cargoImpForInterpretation; }
		}

		protected override ZString SenderPimaCore
		{
			get
			{
				var result = Awb.Profile;
				if (result.IsEmpty && howAsCimFsa != null && howAsCimFsa.IncomingMessage != null && howAsCimFsa.IncomingMessage.Interchange != null)
				{
					// If the Awb is a phantom Awb without a pima (!) just reply back using the sender that received the incoming request
					result = howAsCimFsa.IncomingMessage.Interchange.EI_To.Replace("/", "");
				}
				return result;
			}
		}

		CcsukTransmissionMessageFunction.CIM.FSA howAsCimFsa;
		protected ZString cargoImpForInterpretation;
		protected CcsukTransmissionMessageFunction.CIM howAsCim;
		readonly ErrorCollector errorCollector;
	}
}
