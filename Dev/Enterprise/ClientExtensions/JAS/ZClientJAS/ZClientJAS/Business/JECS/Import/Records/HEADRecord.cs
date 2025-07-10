
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class HEADRecord : JXCRecord
	{
		public HEADRecord(ZString lineType, ZString lineContent)
			: base(lineType, lineContent)
		{
		}

		public ZString SendingOfficeCode
		{
			get { return Fields.GetFieldValue(JXCConstants.HEADFieldPositions.SendingOfficeCode); }
		}

		public ZString SendingNettingCode
		{
			get { return Fields.GetFieldValue(JXCConstants.HEADFieldPositions.SendingNettingCode); }
		}

		public ZString DestinationOfficeCode
		{
			get { return Fields.GetFieldValue(JXCConstants.HEADFieldPositions.DestOfficeCode); }
		}

		public ZString DestinationNettingCode
		{
			get { return Fields.GetFieldValue(JXCConstants.HEADFieldPositions.DestNettingCode); }
		}

		public ZString FreightDestination
		{
			get { return Fields.GetFieldValue(JXCConstants.HEADFieldPositions.FreightDest); }
		}

		public void UpdateJXCHeaderBusinessObject(IJXCImportHeader jXCHeader, INotifications notificationSubscriber)
		{
			using (new DataImportFlagChanger(jXCHeader))
			{
				jXCHeader.SetDestinationForwarder(DestinationOfficeCode, DestinationNettingCode);
				jXCHeader.SetSendingForwarder(SendingOfficeCode, SendingNettingCode);
				jXCHeader.SetFreightDestination(FreightDestination);
			}
		}
	}
}
