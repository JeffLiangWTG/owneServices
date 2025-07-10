
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class PWSDetailRecord : PWSRecordBase
	{
		#region Constants

		static class Constants
		{
			public abstract class PackageNumber
			{
				public const int Length = 35;
				public const int Position = 3;
			}

			public abstract class DataRecordSequenceNumber
			{
				public const int Length = 4;
				public const int Position = 38;
			}

			public abstract class LastRecordIdentifier
			{
				public const int Length = 1;
				public const int Position = 42;
			}

			public abstract class DataArea
			{
				public const int Length = 120;
				public const int Position = 43;
			}
		}

		#endregion

		public PWSDetailRecord(ZString recordData, INotifications notifications)
			: base(recordData, notifications)
		{
		}

		public ZString PackageNumber
		{
			get { return RecordData.SubstringSafe(Constants.PackageNumber.Position, Constants.PackageNumber.Length).Trim(); }
		}

		public ZInt DataRecordSequenceNumber
		{
			get { return ToZInt(RecordData.SubstringSafe(Constants.DataRecordSequenceNumber.Position, Constants.DataRecordSequenceNumber.Length)); }
		}

		public bool IsLastRecord
		{
			get { return RecordData.SubstringSafe(Constants.LastRecordIdentifier.Position, Constants.LastRecordIdentifier.Length).Equals("D"); }
		}

		public ZString Data
		{
			get { return RecordData.SubstringSafe(Constants.DataArea.Position, Constants.DataArea.Length).PadRight(120, ' '); }
		}

		public bool HasCorrectRecordCode
		{
			get { return RecordData.Left(3).Equals("020"); }
		}

		public bool CanConstructTotalDataArea(int expectedSequenceNumber, ZString expectedPackageNumber)
		{
			return !HasErrors &&
				expectedSequenceNumber == DataRecordSequenceNumber &&
				(expectedPackageNumber == PackageNumber || (expectedPackageNumber.IsEmpty && DataRecordSequenceNumber == 1));
		}

		protected override bool ValidateRecordData()
		{
			bool result = true;
			if (RecordData.Length < 43)
			{
				WarningNotification warning = new WarningNotification("Detail Record less than 43 characters");
				Notifications.Notify(warning);
				result = false;
			}
			return result;
		}
	}
}
