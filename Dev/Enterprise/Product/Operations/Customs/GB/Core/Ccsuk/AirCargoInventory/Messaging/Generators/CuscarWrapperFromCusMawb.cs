using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class CuscarWrapperFromCusMawb : ICuscar
	{
		public CuscarWrapperFromCusMawb(CusMAWB mawbToWrap)
		{
			mawb = mawbToWrap;
		}

		public ZString AirportOfOrigin
		{
			get { return PortConverter.UnlocoToIata(mawb.AirportOfOrigin, mawb.Factory); }
		}

		public ZString AirportOfArrival
		{
			get { return mawb.AirportOfArrival; }
		}

		public ZString AirportOfDestination
		{
			get { return mawb.AirportOfDestination; }
		}

		public ZString CargoTerminalOperator
		{
			get { return mawb.CargoTerminalOperator; }
		}

		public ZString AirlinePrefix
		{
			get { return mawb.CM_MAWB.Left(3); }
		}

		public ZString AirWaybillSerialNumber
		{
			get { return mawb.CM_MAWB.Right(8); }
		}

		public ZString HouseAirWaybillNumber
		{
			get { return !mawb.IsBasic ? new ZString("M") : ZString.Empty; }
		}

		public ZBool IsPrearrival
		{
			get { return mawb.IsPrearrival; }
		}

		public ZDateTime DateTimeOfRecordCreation
		{
			get { throw new NotImplementedException("Date created"); }
		}

		public ZString ShipmentDescriptionCode
		{
			get { return mawb.ShipmentDescriptionCode; }
		}

		public ZShort NumberOfPiecesExpected
		{
			get { return NumberOfPiecesExpectedCore; }
		}

		protected virtual ZShort NumberOfPiecesExpectedCore
		{
			get { return mawb.NumberOfPiecesExpected; }
		}

		public ZString WeightCode
		{
			get { return mawb.WeightCode == Core.Constants.Weight.Kilograms ? new ZString("KGM") : mawb.WeightCode; }
		}

		public ZDecimal Weight
		{
			get { return WeightCore; }
		}

		protected virtual ZDecimal WeightCore
		{
			get { return mawb.Weight; }
		}

		public ZString DescriptionOfGoods
		{
			get { return mawb.DescriptionOfGoods; }
		}

		public ZString CarrierCode
		{
			get { return mawb.CM_FlightNo.Left(2); }  // check this...
		}

		public ZString FlightNumber
		{
			get { return mawb.CM_FlightNo.IsEmpty ? ZString.Empty : mawb.CM_FlightNo.Replace(CarrierCode, ""); }
		}

		public ZDateTime DateOfFlightArrival
		{
			get { return mawb.CM_ArrivalDate; }
		}

		public ZShort NumberOfPiecesReceived
		{
			get { return NumberOfPiecesReceivedCore; }
		}

		protected virtual ZShort NumberOfPiecesReceivedCore
		{
			get { return mawb.NumberOfPiecesReceived; }
		}

		public ZBool Status2Indicator
		{
			get { return mawb.Status2Granted; }
		}

		public ZString AgentBrokerConsolidatorCode
		{
			get { return AgentBrokerConsolidatorCodeCore; }
		}

		protected virtual ZString AgentBrokerConsolidatorCodeCore
		{
			get { return mawb.AgentBadge; }
		}

		public ZString HarmonisedCommodityCode
		{
			get { return ZString.Empty; }
		}

		public ZString SplitReference
		{
			get { return SplitReferenceCore; }
		}

		/// <summary>
		///  For split basics
		/// </summary>
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
							mawb.CommunityHandlingCodes
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
			get { return mawb.Status1Date; }
		}

		protected CusMAWB mawb;
	}
}
