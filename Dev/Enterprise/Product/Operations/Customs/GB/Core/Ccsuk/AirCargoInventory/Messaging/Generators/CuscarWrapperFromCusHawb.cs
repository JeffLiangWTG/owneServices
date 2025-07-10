using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class CuscarWrapperFromCusHawb : ICuscar
	{
		public CuscarWrapperFromCusHawb(CusHAWB hawbToWrap)
		{
			hawb = hawbToWrap;
			if (hawbToWrap.CS_CM == ZGuid.Empty)
			{
				throw new NotSupportedException("Can only send Hawbs that are attached to a Mawb; this one was not attached");
			}
			parentMawb = new CuscarWrapperFromCusMawb(hawbToWrap.MAWB);
		}

		public ZString AirportOfOrigin
		{
			get { return PortConverter.UnlocoToIata(hawb.AirportOfOrigin, hawb.Factory); }
		}

		public ZString AirportOfArrival
		{
			get { return hawb.AirportOfArrival; }
		}

		public ZString AirportOfDestination
		{
			get { return hawb.AirportOfDestination; }
		}

		public ZString CargoTerminalOperator
		{
			get { return hawb.CargoTerminalOperator; }
		}

		public ZString AirlinePrefix
		{
			get { return parentMawb.AirlinePrefix; }
		}

		public ZString AirWaybillSerialNumber
		{
			get { return parentMawb.AirWaybillSerialNumber; }
		}

		public ZString HouseAirWaybillNumber
		{
			get { return hawb.CS_HAWB; }
		}

		public ZBool IsPrearrival
		{
			get { return DateOfFlightArrival.IsEmpty && NumberOfPiecesReceived == 0; }
		}

		public ZDateTime DateTimeOfRecordCreation
		{
			get { throw new NotImplementedException("Date created"); }
		}

		public ZString ShipmentDescriptionCode
		{
			get { return hawb.ShipmentDescriptionCode; }
		}

		public ZShort NumberOfPiecesExpected
		{
			get { return NumberOfPiecesExpectedCore; }
		}

		protected virtual ZShort NumberOfPiecesExpectedCore
		{
			get { return hawb.CS_PiecesManifested; }
		}

		public ZShort NumberOfPiecesReceived
		{
			get { return NumberOfPiecesReceivedCore; }
		}

		protected virtual ZShort NumberOfPiecesReceivedCore
		{
			get { return hawb.CS_PiecesLanded; }
		}

		public ZString WeightCode
		{
			get { return hawb.CS_WeightUQ == Core.Constants.Weight.Kilograms ? new ZString("KGM") : hawb.CS_WeightUQ; }
		}

		public ZDecimal Weight
		{
			get { return WeightCore; }
		}

		protected virtual ZDecimal WeightCore
		{
			get { return hawb.CS_Weight; }
		}

		public ZString DescriptionOfGoods
		{
			get { return hawb.CS_GoodsDescription; }
		}

		public ZString CarrierCode
		{
			get { return parentMawb.CarrierCode; }
		}

		public ZString FlightNumber
		{
			get { return parentMawb.FlightNumber; }
		}

		public ZDateTime DateOfFlightArrival
		{
			get { return parentMawb.DateOfFlightArrival; }
		}

		public ZBool Status2Indicator
		{
			get { return hawb.Status2Granted; }
		}

		public ZString AgentBrokerConsolidatorCode
		{
			get { return AgentBrokerConsolidatorCodeCore; }
		}

		protected virtual ZString AgentBrokerConsolidatorCodeCore
		{
			get { return hawb.AgentBadge; }
		}

		public ZString HarmonisedCommodityCode
		{
			get { return ZString.Empty; }
		}

		public ZString SplitReference
		{
			get { return SplitReferenceCore; }
		}

		protected virtual ZString SplitReferenceCore
		{
			get { return ""; }
		}

		public ZString LineOrSplitNumber
		{
			get { return "0"; }
		}

		public List<ZString> CommunityHandlingCodes
		{
			get { return CommunityHandlingCodesCore; }
		}

		protected virtual List<ZString> CommunityHandlingCodesCore
		{
			get
			{
				return (from CusAddInfo<CommunityHandlingCode> chc in
							hawb.CommunityHandlingCodes
						where chc.Data.C4_SplitReferenceToWhichThisPertains.IsEmpty
						select chc.Data.C4_CommunityHandlingCode
						).ToList();
			}
		}

		public ZDateTime Status1Date
		{
			get { return Status1DateCore; }
		}

		protected virtual ZDateTime Status1DateCore
		{
			get { return hawb.Status1Date; }
		}

		protected CusHAWB hawb;
		protected ICuscar parentMawb;
	}
}
