using CargoWise.Types;

namespace Enterprise.Accounting.DataTransfer
{
	public class CASSHOTFileImportLineRow : CASSHOTFileLineRow
	{
		public CASSHOTFileImportLineRow()
			: this(NextSchemaFieldNumber)
		{
		}

		protected CASSHOTFileImportLineRow(int fieldCount)
			: base(fieldCount)
		{
		}

		#region Properties

		protected override ZString VATIndicatorCore
		{
			get { return ZString.Empty; }
		}

		public ZDecimal WeightCharges
		{
			get { return GetFieldAsZDecimal(Schema.WeightCharges); }
		}

		public ZDecimal ChargesDueAgentCC
		{
			get { return GetFieldAsZDecimal(Schema.ChargesDueAgent); }
		}

		public ZDecimal ChargesDueCarrierCC
		{
			get { return GetFieldAsZDecimal(Schema.ChargesDueCarrier); }
		}

		public ZString FeeCharged
		{
			get { return GetField(Schema.FeeCharged); }
		}

		public ZDecimal FeeAmount
		{
			get { return GetFieldAsZDecimal(Schema.FeeAmount); }
		}

		public ZDecimal HandlingCharges
		{
			get { return GetFieldAsZDecimal(Schema.HandlingCharges); }
		}

		public ZDecimal StorageCharges
		{
			get { return GetFieldAsZDecimal(Schema.StorageCharges); }
		}

		public ZDecimal OtherCharge1Amount
		{
			get { return GetFieldAsZDecimal(Schema.OtherCharge1Amount); }
		}

		public ZDecimal OtherCharge2Amount
		{
			get { return GetFieldAsZDecimal(Schema.OtherCharge2Amount); }
		}

		public ZDecimal MiscellaneousChargesAmount
		{
			get { return GetFieldAsZDecimal(Schema.MiscellaneousChargesAmount); }
		}

		#endregion

		protected new const int NextSchemaFieldNumber = CASSHOTFileLineRow.NextSchemaFieldNumber + 10;

		public new abstract class Schema : CASSHOTFileLineRow.Schema
		{
			public const int WeightCharges = CASSHOTFileLineRow.NextSchemaFieldNumber;
			public const int ChargesDueAgent = CASSHOTFileLineRow.NextSchemaFieldNumber + 1;
			public const int ChargesDueCarrier = CASSHOTFileLineRow.NextSchemaFieldNumber + 2;
			public const int FeeAmount = CASSHOTFileLineRow.NextSchemaFieldNumber + 3;
			public const int FeeCharged = CASSHOTFileLineRow.NextSchemaFieldNumber + 4;
			public const int HandlingCharges = CASSHOTFileLineRow.NextSchemaFieldNumber + 5;
			public const int StorageCharges = CASSHOTFileLineRow.NextSchemaFieldNumber + 6;
			public const int OtherCharge1Amount = CASSHOTFileLineRow.NextSchemaFieldNumber + 7;
			public const int OtherCharge2Amount = CASSHOTFileLineRow.NextSchemaFieldNumber + 8;
			public const int MiscellaneousChargesAmount = CASSHOTFileLineRow.NextSchemaFieldNumber + 9;
		}
	}
}
