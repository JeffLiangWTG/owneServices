using CargoWise.Types;

namespace Enterprise.Accounting.DataTransfer
{
	public class CASSHOTFileExportLineRow : CASSHOTFileLineRow
	{
		public CASSHOTFileExportLineRow()
			: this(NextSchemaFieldNumber)
		{
		}

		protected CASSHOTFileExportLineRow(int fieldCount)
			: base(fieldCount)
		{
		}

		#region Properties

		protected override ZString VATIndicatorCore
		{
			get { return GetField(Schema.VATIndicator); }
		}

		public ZDecimal WeightChargePP
		{
			get { return GetFieldAsZDecimal(Schema.WeightCharge); }
		}

		public ZDecimal ValuationChargePP
		{
			get { return GetFieldAsZDecimal(Schema.ValuationCharge); }
		}

		public ZDecimal ChargesDueCarrierPP
		{
			get { return GetFieldAsZDecimal(Schema.ChargesDueCarrier); }
		}

		public ZDecimal ChargesDueAgentCC
		{
			get { return GetFieldAsZDecimal(Schema.ChargesDueAgent); }
		}

		public ZDecimal Commission
		{
			get { return GetFieldAsZDecimal(Schema.Commission); }
		}

		public ZDecimal Discount
		{
			get { return GetFieldAsZDecimal(Schema.Discount); }
		}

		public ZDecimal VATDueAirline
		{
			get { return GetFieldAsZDecimal(Schema.VATDueAirline); }
		}

		public ZDecimal VATDueAgent
		{
			get { return GetFieldAsZDecimal(Schema.VATDueAgent); }
		}

		public ZString CCADCMNumber
		{
			get { return GetField(Schema.CCADCMNumber); }
		}

		#endregion

		protected new const int NextSchemaFieldNumber = CASSHOTFileLineRow.NextSchemaFieldNumber + 10;

		public new abstract class Schema : CASSHOTFileLineRow.Schema
		{
			public const int CCADCMNumber = CASSHOTFileLineRow.NextSchemaFieldNumber;
			public const int WeightCharge = CASSHOTFileLineRow.NextSchemaFieldNumber + 1;
			public const int ValuationCharge = CASSHOTFileLineRow.NextSchemaFieldNumber + 2;
			public const int ChargesDueCarrier = CASSHOTFileLineRow.NextSchemaFieldNumber + 3;
			public const int ChargesDueAgent = CASSHOTFileLineRow.NextSchemaFieldNumber + 4;
			public const int Commission = CASSHOTFileLineRow.NextSchemaFieldNumber + 5;
			public const int Discount = CASSHOTFileLineRow.NextSchemaFieldNumber + 6;
			public const int VATIndicator = CASSHOTFileLineRow.NextSchemaFieldNumber + 7;
			public const int VATDueAirline = CASSHOTFileLineRow.NextSchemaFieldNumber + 8;
			public const int VATDueAgent = CASSHOTFileLineRow.NextSchemaFieldNumber + 9;
		}
	}
}
