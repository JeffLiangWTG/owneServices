using System.Collections.Generic;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class PWSTotalDataAreaRecord : PWSRecordBase
	{
		#region Constants

		static class Constants
		{
			public abstract class WayBillShortNumber
			{
				public const int Length = 11;
				public const int Position = 0;
			}

			public abstract class WayBillNumber
			{
				public const int Length = 35;
				public const int Position = 11;
			}

			public abstract class InvoiceNumber
			{
				public const int Length = 12;
				public const int Position = 46;
			}

			public abstract class BillToAccount
			{
				public const int Length = 10;
				public const int Position = 83;
			}

			public abstract class BillableWeight
			{
				public const int Length = 8;
				public const int Position = 533;
			}

			public const int MaxChargesCount = 22;
			public const int ChargesStartingPosition = 1852;
			public const int ChargeDataLength = 88;
		}

		#endregion

		public static PWSTotalDataAreaRecord New(PWSDetailRecord[] detailRecords, INotifications notifications)
		{
			PWSTotalDataAreaRecord result = null;

			if (detailRecords != null && detailRecords.Length > 0)
			{
				StringBuilder builder = new StringBuilder();
				foreach (PWSDetailRecord record in detailRecords)
				{
					builder.Append(record.Data);
				}
				result = new PWSTotalDataAreaRecord(detailRecords.Length, builder.ToString(), notifications);
			}

			return result;
		}

		protected PWSTotalDataAreaRecord(int numberOfDetailRecords, ZString recordData, INotifications notifications)
			: base(recordData, notifications)
		{
			this.NumberOfDetailRecords = numberOfDetailRecords;
		}

		public ZString WayBillShortNumber
		{
			get { return RecordData.SubstringSafe(Constants.WayBillShortNumber.Position, Constants.WayBillShortNumber.Length).Trim(); }
		}

		public ZString WayBillNumber
		{
			get { return RecordData.SubstringSafe(Constants.WayBillNumber.Position, Constants.WayBillNumber.Length).Trim(); }
		}

		public ZString InvoiceNumber
		{
			get { return RecordData.SubstringSafe(Constants.InvoiceNumber.Position, Constants.InvoiceNumber.Length).Trim(); }
		}

		public ZString BillToAccount
		{
			get
			{
				ZString result = RecordData.SubstringSafe(Constants.BillToAccount.Position, Constants.BillToAccount.Length).Trim();
				return result.StartsWith("0000") ? result.SubstringSafe(4) : result;
			}
		}

		public ZDecimal DimensionalWeight
		{
			get
			{
				ZDecimal result = 0;

				ZString billableWeightString = RecordData.SubstringSafe(Constants.BillableWeight.Position, Constants.BillableWeight.Length).Trim();
				ZDecimal.TryParse(billableWeightString, out result);

				return result;
			}
		}

		public IReadOnlyList<PWSChargeDetails> Charges
		{
			get
			{
				if (charges == null)
				{
					var list = new List<PWSChargeDetails>();
					for (var i = 0; i < Constants.MaxChargesCount; i++)
					{
						var startIndex = (i * Constants.ChargeDataLength) + Constants.ChargesStartingPosition;
						var chargeData = RecordData.SubstringSafe(startIndex, Constants.ChargeDataLength);
						if (!chargeData.IsEmpty)
						{
							list.Add(new PWSChargeDetails(chargeData));
						}
					}
					charges = list;
				}
				return charges;
			}
		}

		protected override bool ValidateRecordData()
		{
			return true;
		}

		public readonly int NumberOfDetailRecords;
		List<PWSChargeDetails> charges;
	}
}
