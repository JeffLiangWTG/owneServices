using System;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.Wow
{
	public class MIHeaderCsvRecord : CsvRecord
	{
		internal MIHeaderCsvRecord(string line) : base(line, -1)
		{
		}

		public override string DisplayIdentifier
		{
			get { return "Header"; }
		}

		public override bool SupportsUpdateBusinessData()
		{
			return false;
		}

		protected override void OnUpdateBusinessData(BusinessObjectFactoryProvider factoryProvider, INotifications notify)
		{
			throw new NotSupportedException();
		}

		public void UpdateSequenceNoForEdiTrack()
		{
			UpdateSequenceNoForEdiTrack(ZString.Empty);
		}

		public void UpdateSequenceNoForEdiTrack(ZString numberFountainID)
		{
			string nextID = WowNumberFountains.Instance.GetWowEdiTrackMessageID(numberFountainID).GetNextFormatted(Db.Connection);
			string paddedNextID = nextID.PadLeft(4, '0');
			FieldValues[3] = paddedNextID;
		}
	}
}
