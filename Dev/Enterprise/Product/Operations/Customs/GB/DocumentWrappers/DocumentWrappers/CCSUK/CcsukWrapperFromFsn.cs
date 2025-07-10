using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.DocumentWrappers.Ccsuk
{
	/// <summary>
	/// Receipt of unsolicited FSN can grant 'clearance' after initial delay.  
	/// e.g. send IAR CUSDEC request, get FSN/CA and CUSRES/CA, then later get FSN/CW.
	/// This file produces docs from the latter.
	/// Because this is a DocWrapper and we are only given the EDIMessage, we have to do a bunch of loading & parsing again. 
	/// </summary>
	public class CcsukWrapperFromFsn : CcsukWrapper
	{
		public CcsukWrapperFromFsn(EDIMessage inboundEdiMessage, BusinessObjectFactory factoryToWrap)
			: base(inboundEdiMessage, inboundEdiMessage.EM_LinkedObject, factoryToWrap)
		{
			var cargoFactMessage = (CargoFactMessage)inboundEdiMessage.GetAutoEdifactMessageUsingNamedFactory(CcsukEdifactMessageFactory.Factory, inboundEdiMessage.CharacterSet);
			if (cargoFactMessage != null)
			{
				var cargoFactMessageProcessor = new CargoFactMessageProcessor(cargoFactMessage, inboundEdiMessage, null);
				cargoImpFsnParser = (CIMFSN)cargoFactMessageProcessor.GetCargoImpFromCargoFact();
				if (cargoImpFsnParser != null)
				{
					cargoImpFsnParser.ParseInboundFsn();
					var underbondQuery = new ZQuery(CusUnderbondSchema.C4_SendersMessageReference, "U" + cargoImpFsnParser.AgentReference);
					if (Hawb != null)
					{
						underbondQuery.AddToFilter(CusUnderbondSchema.C4_ParentID, Hawb.PK);
					}
					underbond = Factory.LoadTop1<CusUnderbond>(underbondQuery);
					tsr = underbond as TranshipmentRemoval;
				}
			}
		}

		protected override ZString NewShedCore
		{
			get
			{
				var shedProvider = underbond as INewShedProvider;
				return shedProvider != null ? shedProvider.NewShedId : ZString.Empty;
			}
		}

		protected override ZString AgentNameCore
		{
			get { return Utilities.GetAgentNameFromDatabase(iCcsukCusAwb); }
		}

		protected override ZString POSCore
		{
			get { return tsr != null ? tsr.PortOfShipment : ZString.Empty; }
		}

		protected override ZString TrnCore
		{
			get { return (tsr != null) ? tsr.TranshipmentEntryNumber : ZString.Empty; }
		}

		protected override bool IsLicenceIndicatorCore
		{
			get
			{
				var iLicenseRestrictionIndProvider = underbond as ILicenseRestrictionIndProvider;
				return iLicenseRestrictionIndProvider != null && (iLicenseRestrictionIndProvider.LicenseRestrictionInd == Enterprise.Customs.Business.YesNoList.Codes.Yes);
			}
		}

		protected override ZString AgentRefCore
		{
			get { return cargoImpFsnParser != null ? cargoImpFsnParser.AgentReference : ZString.Empty; }
		}

		protected override ZInt PiecesRelevantToThisRendering
		{
			get
			{
				var piecesToPrint = 0;
				var mostRecentPiecesReleased = NumberOfPiecesReleasedHelper.LastPiecesReleased(iCcsukCusAwb, EventCode);
				if (mostRecentPiecesReleased != 0 && mostRecentPiecesReleased != iCcsukCusAwb.NumberOfPiecesExpected)
				{
					// Get part release count from dbo.StmALog
					piecesToPrint = mostRecentPiecesReleased;
				}
				else
				{
					// Just use the (full) count from the FSN message
					piecesToPrint = ZInt.ParseSafe(cargoImpFsnParser.Pieces, 0);
				}
				return piecesToPrint;
			}
		}

		protected override ZBool NeedsT1Statement
		{
			get { return tsr != null; }
		}

		public ZString INWARDCARRIER
		{
			get { return mawb.CM_FlightNo.SubstringSafe(0, 2); }
		}

		public ZString SHEDNAME
		{
			get { return Utilities.GetShedNameFromDatabase(iCcsukCusAwb, SHED, AIRPORT); }
		}

		public ZString ONWARDCARRIER
		{
			get
			{
				var iOnwardCarrierProvider = underbond as IOnwardCarrierProvider;
				return iOnwardCarrierProvider == null ? ZString.Empty : Utilities.GetAirlineNameFromCode(iCcsukCusAwb, iOnwardCarrierProvider.OnwardCarrier);
			}
		}

		public JobDeclaration Declaration
		{
			get
			{
				JobDeclaration result = null;
				if (iCcsukCusAwb is SplitConsignment)
				{
					result = ((SplitConsignment)iCcsukCusAwb).OwnDeclaration;
				}
				else if (Hawb != null && Hawb.HasDeclaration)
				{
					result = Hawb.Declaration;  // For basics and houses
				}
				return result;
			}
		}

		public ZString NEWAOD
		{
			get { return underbond != null ? underbond.AirportOrCountryOfDestination_IATA : ZString.Empty; }
		}

		readonly TranshipmentRemoval tsr;
		readonly CusUnderbond underbond;
		readonly CIMFSN cargoImpFsnParser;
	}
}
