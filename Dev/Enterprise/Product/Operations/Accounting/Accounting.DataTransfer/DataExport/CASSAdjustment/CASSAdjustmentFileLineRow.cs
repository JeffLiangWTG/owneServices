using CargoWise.Types;

namespace Enterprise.Accounting.DataTransfer
{
	public class CASSAdjustmentFileLineRow : CASSAdjustmentFileDataRow
	{
		public CASSAdjustmentFileLineRow()
			: this(NextSchemaFieldNumber)
		{
		}

		protected CASSAdjustmentFileLineRow(int fieldCount)
			: base(fieldCount)
		{
			DataRow[Schema.AgentChargePP] = ZInt.Zero.ToString();
			DataRow[Schema.WeightChargeCC] = ZInt.Zero.ToString();
			DataRow[Schema.ValuationChargeCC] = ZInt.Zero.ToString();
			DataRow[Schema.CarrierChargeCC] = ZInt.Zero.ToString();
			DataRow[Schema.Contract] = ZString.Empty;
			DataRow[Schema.Rate] = ZInt.Zero.ToString();
		}

		public ZString Airline
		{
			get { return DataRow[Schema.Airline]; }
			set { DataRow[Schema.Airline] = value; }
		}

		public ZString Agent
		{
			get { return DataRow[Schema.Agent]; }
			set { DataRow[Schema.Agent] = value; }
		}

		public ZString AWBNumber
		{
			get { return DataRow[Schema.AWBNumber]; }
			set { DataRow[Schema.AWBNumber] = value; }
		}

		public ZString CCADCMNumber
		{
			get { return DataRow[Schema.CCADCMNumber]; }
			set { DataRow[Schema.CCADCMNumber] = value; }
		}

		public ZDecimal WeightChargePP
		{
			get { return ZDecimal.ParseSafe(DataRow[Schema.WeightChargePP], ZDecimal.Zero); }
			set { DataRow[Schema.WeightChargePP] = value.ToString(); }
		}

		public ZDecimal ValuationChargePP
		{
			get { return ZDecimal.ParseSafe(DataRow[Schema.ValuationChargePP], ZDecimal.Zero); }
			set { DataRow[Schema.ValuationChargePP] = value.ToString(); }
		}

		public ZDecimal CarrierChargePP
		{
			get { return ZDecimal.ParseSafe(DataRow[Schema.CarrierChargePP], ZDecimal.Zero); }
			set { DataRow[Schema.CarrierChargePP] = value.ToString(); }
		}

		public ZDecimal AgentChargePP
		{
			get { return ZDecimal.Parse(DataRow[Schema.AgentChargePP]); }
		}

		public ZDecimal WeightChargeCC
		{
			get { return ZDecimal.Parse(DataRow[Schema.WeightChargeCC]); }
		}

		public ZDecimal ValuationChargeCC
		{
			get { return ZDecimal.Parse(DataRow[Schema.ValuationChargeCC]); }
		}

		public ZDecimal CarrierChargeCC
		{
			get { return ZDecimal.Parse(DataRow[Schema.CarrierChargeCC]); }
		}

		public ZDecimal AgentChargeCC
		{
			get { return ZDecimal.ParseSafe(DataRow[Schema.AgentChargeCC], ZDecimal.Zero); }
			set { DataRow[Schema.AgentChargeCC] = value.ToString(); }
		}

		public ZDecimal Commission
		{
			get { return ZDecimal.ParseSafe(DataRow[Schema.Commission], ZDecimal.Zero); }
			set { DataRow[Schema.Commission] = value.ToString(); }
		}

		public ZDecimal Incentive
		{
			get { return ZDecimal.ParseSafe(DataRow[Schema.Incentive], ZDecimal.Zero); }
			set
			{
				DataRow[Schema.Incentive] = value.ToString();
				DataRow[Schema.IncentiveSign] = Incentive < 0 ? "-" : "+"; // Incentive Sign Positive or Negative.
			}
		}

		public ZString IncentiveSign
		{
			get { return DataRow[Schema.IncentiveSign]; }
		}

		public ZDecimal Weight
		{
			get { return ZDecimal.ParseSafe(DataRow[Schema.Weight], ZDecimal.Zero); }
			set { DataRow[Schema.Weight] = value.ToString(); }
		}

		public ZString WeightUnit
		{
			get { return DataRow[Schema.WeightUnit]; }
			set { DataRow[Schema.WeightUnit] = value; }
		}

		public ZInt Reason
		{
			get { return ZInt.Parse(DataRow[Schema.Reason]); }
			set { DataRow[Schema.Reason] = value.ToString(); }
		}

		public ZString Contract
		{
			get { return DataRow[Schema.Contract]; }
		}

		public ZDecimal Rate
		{
			get { return ZDecimal.ParseSafe(DataRow[Schema.Rate], ZDecimal.Zero); }
		}

		public ZString AirlineBranch
		{
			get { return DataRow[Schema.AirlineBranch]; }
			set { DataRow[Schema.AirlineBranch] = value; }
		}

		public ZString Comment
		{
			get { return DataRow[Schema.Comment]; }
			set { DataRow[Schema.Comment] = value; }
		}

		protected new const int NextSchemaFieldNumber = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 22;

		public new abstract class Schema : CASSAdjustmentFileDataRow.Schema
		{
			public const int Airline = CASSAdjustmentFileDataRow.NextSchemaFieldNumber;
			public const int Agent = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 1;
			public const int AWBNumber = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 2;
			public const int CCADCMNumber = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 3;
			public const int WeightChargePP = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 4;
			public const int ValuationChargePP = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 5;
			public const int CarrierChargePP = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 6;
			public const int AgentChargePP = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 7;
			public const int WeightChargeCC = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 8;
			public const int ValuationChargeCC = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 9;
			public const int CarrierChargeCC = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 10;
			public const int AgentChargeCC = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 11;
			public const int Commission = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 12;
			public const int Incentive = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 13;
			public const int IncentiveSign = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 14;
			public const int Weight = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 15;
			public const int WeightUnit = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 16;
			public const int Reason = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 17;
			public const int Contract = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 18;
			public const int Rate = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 19;
			public const int AirlineBranch = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 20;
			public const int Comment = CASSAdjustmentFileDataRow.NextSchemaFieldNumber + 21;
		}
	}
}
