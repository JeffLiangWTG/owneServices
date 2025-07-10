using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusPartShipAirCargoReportHeader : IAirCargoReportHeader
	{
		public CusPartShipAirCargoReportHeader(CusPartShip partShip)
		{
			this.partShip = partShip;
			hAWBReport = new CusHAWBAirCargoReportHeader(partShip.HouseBill);
		}

		#region ICargoReportHeader Members

		public EDIMessageCollection Messages
		{
			get { return hAWBReport.Messages; }
		}

		public ZString MethodOfPayment
		{
			get { return hAWBReport.MethodOfPayment; }
		}

		public ZString Origin
		{
			get { return hAWBReport.Origin; }
		}

		public ZString Destination
		{
			get { return hAWBReport.Destination; }
		}

		public ZString Loading
		{
			get { return partShip.CG_RL_NKLoadPort; }
		}

		public ZString Discharge
		{
			get { return partShip.CG_RL_NKDischargePort; }
		}

		public ZString FirstArrivalPort
		{
			get { return hAWBReport.FirstArrivalPort; }
		}

		public ZString[] Routings
		{
			get { return hAWBReport.Routings; }
		}

		public ZString ConsigneeGeneralAddress
		{
			get { return hAWBReport.ConsigneeGeneralAddress; }
		}

		public ZString ConsigneeName
		{
			get { return hAWBReport.ConsigneeName; }
		}

		public ZString ConsigneeStreet
		{
			get { return hAWBReport.ConsigneeStreet; }
		}

		public ZString ConsigneeStreet2
		{
			get { return hAWBReport.ConsigneeStreet2; }
		}

		public ZString ConsigneeCity
		{
			get { return hAWBReport.ConsigneeCity; }
		}

		public ZString ConsigneeState
		{
			get { return hAWBReport.ConsigneeState; }
		}

		public ZString ConsigneePostCode
		{
			get { return hAWBReport.ConsigneePostCode; }
		}

		public ZString ConsigneeCountry
		{
			get { return hAWBReport.ConsigneeCountry; }
		}

		public ZString ConsignorGeneralAddress
		{
			get { return hAWBReport.ConsignorGeneralAddress; }
		}

		public ZString ConsignorName
		{
			get { return hAWBReport.ConsignorName; }
		}

		public ZString ConsignorStreet
		{
			get { return hAWBReport.ConsignorStreet; }
		}

		public ZString ConsignorStreet2
		{
			get { return hAWBReport.ConsignorStreet2; }
		}

		public ZString ConsignorCity
		{
			get { return hAWBReport.ConsignorCity; }
		}

		public ZString ConsignorState
		{
			get { return hAWBReport.ConsignorState; }
		}

		public ZString ConsignorPostCode
		{
			get { return hAWBReport.ConsignorPostCode; }
		}

		public ZString ConsignorCountry
		{
			get { return hAWBReport.ConsignorCountry; }
		}

		public ZString ConsigneeIdentifier => hAWBReport.ConsigneeIdentifier;

		public ZString ConsigneeABN => hAWBReport.ConsigneeABN;

		public ZString ConsigneeCAC => hAWBReport.ConsigneeCAC;

		public ZString ConsigneeTIN => hAWBReport.ConsigneeTIN;

		public ZString ConsignorIdentifier => hAWBReport.ConsignorIdentifier;

		public ZString ConsignorVendor => hAWBReport.ConsignorVendor;

		public ZString ConsignorTIN => hAWBReport.ConsignorTIN;

		public ZString MasterHouseBill
		{
			get { return hAWBReport.MasterHouseBill; }
		}

		public ZString HAWBNum
		{
			get { return hAWBReport.HAWBNum; }
		}

		public ZString MAWB
		{
			get { return hAWBReport.MAWB; }
		}

		ZString IAirCargoReportHeader.MatchConsignmentReference
		{
			get { return ZString.Empty; }
		}

		public ZString ResponsiblePartyID
		{
			get { return hAWBReport.ResponsiblePartyID; }
		}

		public ZString FlightNo
		{
			get { return partShip.CG_FlightNo; }
		}

		public ZDateTime ArivalDate
		{
			get { return partShip.CG_ArrivalDate; }
		}

		public bool IsMasterHouse
		{
			get { return hAWBReport.IsMasterHouse; }
		}

		public bool IsDocuments
		{
			get { return hAWBReport.IsDocuments; }
		}

		public bool IsPersonalEffects
		{
			get { return hAWBReport.IsPersonalEffects; }
		}

		public bool IsSelfAssessedClearance
		{
			get { return hAWBReport.IsSelfAssessedClearance; }
		}

		public int PackageCount
		{
			get { return hAWBReport.PackageCount; }
		}

		public ZString GoodsDescription
		{
			get { return hAWBReport.GoodsDescription; }
		}

		public ZDecimal Weight
		{
			get { return hAWBReport.Weight; }
		}

		public ZString WeightUQ
		{
			get { return hAWBReport.WeightUQ; }
		}

		public ZDecimal GoodsValue
		{
			get { return hAWBReport.GoodsValue; }
		}

		public ZString GoodsValueCurrency
		{
			get { return hAWBReport.GoodsValueCurrency; }
		}

		bool IAirCargoReportHeader.IsHVLVSpecialReporter
		{
			get { return hAWBReport.IsHVLVSpecialReporter; }
		}

		bool IAirCargoReportHeader.IsRemailSpecialReporter
		{
			get { return hAWBReport.IsRemailSpecialReporter; }
		}

		public bool CanDelaySending
		{
			get { return hAWBReport.CanDelaySending; }
		}

		public bool IsBureau
		{
			get { return partShip.MAWB.CM_IsBureau; }
		}

		ZString ICargoReportHeader.NotifyPartyName => ZString.Empty;

		ZString ICargoReportHeader.NotifyPartyStreet => ZString.Empty;

		ZString ICargoReportHeader.NotifyPartyStreet2 => ZString.Empty;

		ZString ICargoReportHeader.NotifyPartyCity => ZString.Empty;

		ZString ICargoReportHeader.NotifyPartyPostCode => ZString.Empty;

		ZString ICargoReportHeader.NotifyPartyCountry => ZString.Empty;

		ZString ICargoReportHeader.NotifyPartyGeneralAddress => ZString.Empty;

		#endregion

		#region Implementation

		readonly CusHAWBAirCargoReportHeader hAWBReport;
		protected CusPartShip partShip;

		#endregion
	}
}
