using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class StandAloneAirCargoCARSTBusinessObjectLoaderOrCreator : CARSTBusinessObjectLoaderOrCreator
	{
		public StandAloneAirCargoCARSTBusinessObjectLoaderOrCreator()
		{
		}

		protected internal override bool IsInterestedInCARST(CMRCARSTMessage message)
		{
			return message.IsAir;
		}

		protected bool ForwarderMAWBExists(CMRCARSTMessage message)
		{
			return !message.MAWB.IsEmpty && new CusMAWBBase.Loader(message.Factory).FindFirstMatchingForwardedMAWB(message.MAWB) != null;
		}

		protected internal override BusinessObject[] LoadOrCreateRecordForMessageCore(CMRCARSTMessage message)
		{
			var packages = IsGhostCARST(message.SendersReference) ? 0 : (short)message.NumberOfPackages;    // For Ghost CARST message do not update Manifested Packages
			return new AirCargoRecordLoaderAndCreator(message.Factory).LoadOrCreateRecords(
				message.FlightNumber,
				message.ArrivalDate,
				message.MAWB,
				message.HAWB,
				(short)packages,
				message.PortOfDischarge,
				message.PremiseID,
				message.CARSTSendersReference,
				message.TranshipmentNumber,
				message);
		}

		protected override bool EnableAuditLogCargoStatusResponse { get { return false; } }

		static bool IsGhostCARST(string msgRef)
		{
			var result = false;
			if (!msgRef.Contains("/") && msgRef.IndexOf(" ", StringComparison.Ordinal) > -1)    // Ghost CARST has reference format of "xxxx xxxx xxxx"
			{
				result = true;
			}

			return result;
		}
	}
}
