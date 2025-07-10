using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers
{
	class CuscarParserAndProcessor
	{
		public CuscarParserAndProcessor(CUSCARMessage cuscar, EDIMessage inboundEdiMessageForAuditing, ILogger iLogger)
		{
			source = cuscar;
			this.inboundEdiMessageForAuditing = inboundEdiMessageForAuditing;
			serviceLogger = iLogger;
			flagableCuscar = new CuscarInboundParser(source).ParseCuscarForListOfUpdatedFields();
		}

		internal bool DoProcessing()
		{
			inboundEdiMessageForAuditing.EM_MessageType = CcsukTransmissionMessageFunction.CUSCAR.Code;
			switch (source.BGM[0].DocumentMessageName.DocumentName)
			{
				case CcsukTransmissionMessageFunction.CUSCAR.FRX.Subcode:
					return new CuscarFrxConsignmentDeleter(inboundEdiMessageForAuditing, serviceLogger, flagableCuscar).HandleDeleteOfConsignment();
				case CcsukTransmissionMessageFunction.CUSCAR.FRC.Subcode:
					return new CuscarFrcConsignmentUpdater(inboundEdiMessageForAuditing, serviceLogger, flagableCuscar).HandleUpdateOfConsignment();
				case CcsukTransmissionMessageFunction.CUSCAR.FRI.Subcode:
					return (new CuscarFriConsignmentInserter(inboundEdiMessageForAuditing, serviceLogger, flagableCuscar).HandleInsertionOfConsignment() != null);
				case CcsukTransmissionMessageFunction.CUSCAR.FCS.Subcode:
					return new CuscarFcsConsignmentSplitter(inboundEdiMessageForAuditing, serviceLogger, flagableCuscar).SplitConsignment();
			}
			return false; // unknown cuscar
		}

		internal static ICcsukCusAwb GetExistingAwbFromInboundCuscarReferenceNumbers(CuscarWithFlagsToShowWhatsSet flagableCuscar, BusinessObjectFactory factory)
		{
			ZString masterAwbNumber = flagableCuscar.AirlinePrefix + flagableCuscar.AirWaybillSerialNumber;
			var airportAndShed = flagableCuscar.AirportOfArrival + flagableCuscar.CargoTerminalOperator;
			if (!masterAwbNumber.IsEmpty)
			{
				if (flagableCuscar.HouseAirWaybillNumber.IsEmpty || flagableCuscar.HouseAirWaybillNumber == "M")  // Empty: basic.  M: consol. Neither empty nor M: proper house. 
				{
					// master or basic
					var query = new ZQuery(CusMAWBSchema.CM_MAWB, masterAwbNumber);
					query.AddToFilter(CusMAWBSchema.CM_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
					var allMawbOrBasics = factory.Load<CusMAWB>(query);
					var bestMatchMawbOrBasic = (from CusMAWB m in allMawbOrBasics where m.CargoTerminalOperatorAirport + m.CargoTerminalOperator == airportAndShed select m).FirstOrDefault();
					return (bestMatchMawbOrBasic != null && bestMatchMawbOrBasic.IsBasic && !flagableCuscar.SplitReference.IsEmpty && bestMatchMawbOrBasic.HasSplits)
							? bestMatchMawbOrBasic.Splits[flagableCuscar.SplitReference]
							: bestMatchMawbOrBasic;
				}
				else
				{
					// house
					var house = GetHawbThatMatchesOnHawbAndMawbNumber(flagableCuscar.HouseAirWaybillNumber, masterAwbNumber, factory, airportAndShed);
					return (!flagableCuscar.SplitReference.IsEmpty && house != null && house.HasSplits)
								? house.Splits[flagableCuscar.SplitReference]
								: house;
				}
			}
			return null;
		}

		static ICcsukCusAwb GetHawbThatMatchesOnHawbAndMawbNumber(ZString hawbNo, ZString mawbNo, BusinessObjectFactory factory, string airportAndShed)
		{
			var hawbQuery = new ZQuery(); // DB only!  Gah!
			hawbQuery.AddToFilter(CusHAWBSchema.CS_HAWB, SQLComparisonOperator.Equal, hawbNo);
			hawbQuery.AddToFilter(CusHAWBSchema.CS_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
			hawbQuery.AddToFilter(CusHAWBSchema.CS_IsActive, true);
			hawbQuery.AddToFilter(CusHAWBSchema.CS_WarehouseLocation, airportAndShed);
			var mawbQuery = new ZQuery(CusMAWBSchema.CM_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
			mawbQuery.AddToFilter(CusMAWBSchema.CM_MAWB, mawbNo);
			var allHawbs = factory.Load<CusHAWB>(hawbQuery);
			var allMawbs = factory.Load<CusMAWB>(mawbQuery);
			var mawbPks = new List<ZGuid>();
			foreach (var mawb in allMawbs)
			{
				mawbPks.Add(mawb.PK);
			}
			return (from CusHAWB h in allHawbs where mawbPks.Contains(h.CS_CM) select h).FirstOrDefault();
		}

		readonly CUSCARMessage source;
		readonly EDIMessage inboundEdiMessageForAuditing;
		readonly ILogger serviceLogger;
		readonly CuscarWithFlagsToShowWhatsSet flagableCuscar;
	}
}
