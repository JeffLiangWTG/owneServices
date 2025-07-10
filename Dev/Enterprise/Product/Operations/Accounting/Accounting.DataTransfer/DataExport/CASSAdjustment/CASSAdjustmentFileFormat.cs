using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Accounting.DataTransfer
{
	public class CASSAdjustmentFileFormat : FixedWidthFlatFileFormat
	{
		public override ZString ConvertToLine(FlatFileDataRow row)
		{
			ZStringBuilder lineBuilder = new ZStringBuilder();

			if (row.GetType() == typeof(CASSAdjustmentFileHeaderRow))
			{
				var headerRow = row as CASSAdjustmentFileHeaderRow;
				lineBuilder.Append(headerRow.RecordType);
				lineBuilder.Append(headerRow.Agent.ToString());
				lineBuilder.Append(headerRow.InvoicePeriod.ToString());
				lineBuilder.Append(GenerateWhitespace(CASSAdjustmentFileHeaderRecord.Filler, ' '));
			}
			else if (row.GetType() == typeof(CASSAdjustmentFileLineRow))
			{
				var linerow = row as CASSAdjustmentFileLineRow;
				lineBuilder.Append(linerow.RecordType);
				FillFixedLengthField(linerow.Airline, CASSAdjustmentFileLineRecord.Airline, lineBuilder, false, ' ');
				FillFixedLengthField(linerow.Agent.Length > 11 ? linerow.Agent.SubstringSafe(0, 11) : linerow.Agent, CASSAdjustmentFileLineRecord.Agent, lineBuilder, true, '0');
				lineBuilder.Append(linerow.AWBNumber);
				FillFixedLengthField(linerow.CCADCMNumber, CASSAdjustmentFileLineRecord.CCADCMNumber, lineBuilder, false, ' ');
				FillFixedLengthField(linerow.WeightChargePP.ToString(0), CASSAdjustmentFileLineRecord.WeightChargePP, lineBuilder, true, '0');
				FillFixedLengthField(linerow.ValuationChargePP.ToString(0), CASSAdjustmentFileLineRecord.ValuationChargePP, lineBuilder, true, '0');
				FillFixedLengthField(linerow.CarrierChargePP.ToString(0), CASSAdjustmentFileLineRecord.CarrierChargePP, lineBuilder, true, '0');
				FillFixedLengthField(linerow.AgentChargePP.ToString(0), CASSAdjustmentFileLineRecord.AgentChargePP, lineBuilder, true, '0');
				FillFixedLengthField(linerow.WeightChargeCC.ToString(0), CASSAdjustmentFileLineRecord.WeightChargeCC, lineBuilder, true, '0');
				FillFixedLengthField(linerow.ValuationChargeCC.ToString(0), CASSAdjustmentFileLineRecord.ValuationChargeCC, lineBuilder, true, '0');
				FillFixedLengthField(linerow.CarrierChargeCC.ToString(0), CASSAdjustmentFileLineRecord.CarrierChargeCC, lineBuilder, true, '0');
				FillFixedLengthField(linerow.AgentChargeCC.ToString(0), CASSAdjustmentFileLineRecord.AgentChargeCC, lineBuilder, true, '0');
				FillFixedLengthField(linerow.Commission.ToString(0), CASSAdjustmentFileLineRecord.Commission, lineBuilder, true, '0');
				FillFixedLengthField(linerow.Incentive.ToString(0), CASSAdjustmentFileLineRecord.Incentive, lineBuilder, true, '0');
				lineBuilder.Append(linerow.IncentiveSign);
				FillFixedLengthField(linerow.Weight.ToString(0), CASSAdjustmentFileLineRecord.Weight, lineBuilder, true, '0');
				lineBuilder.Append(linerow.WeightUnit);
				FillFixedLengthField(linerow.Reason.ToString(), CASSAdjustmentFileLineRecord.Reason, lineBuilder, true, '0');
				FillFixedLengthField(linerow.Contract.ToString(), CASSAdjustmentFileLineRecord.Contract, lineBuilder, false, ' ');
				FillFixedLengthField(linerow.Rate.ToString(0), CASSAdjustmentFileLineRecord.Rate, lineBuilder, true, '0');
				FillFixedLengthField(linerow.AirlineBranch.ToString(), CASSAdjustmentFileLineRecord.AirlineBranch, lineBuilder, false, ' ');
				FillFixedLengthField(linerow.Comment.ToString(), CASSAdjustmentFileLineRecord.Comment, lineBuilder, false, ' ');
			}
			else if (row.GetType() == typeof(CASSAdjustmentFileTrailerRow))
			{
				var trailerRow = row as CASSAdjustmentFileTrailerRow;
				lineBuilder.Append(trailerRow.RecordType);
				FillFixedLengthField(trailerRow.NumberOfRecords.ToString(), CASSAdjustmentFileTrailerRecord.NumberOfRecords, lineBuilder, true, '0');
				lineBuilder.Append(GenerateWhitespace(CASSAdjustmentFileTrailerRecord.Filler, ' '));
			}

			return lineBuilder.ToString();
		}

		public override FileExtensionType FileExtensionForExport
		{
			get { return FileExtensionType.Txt; }
		}

		public override FlatFileDataRow ConvertToRow(ZString rawDataRow)
		{
			throw GetNotSupportedException();
		}

		public override FileExtensionType FileExtensionForImport
		{
			get { throw GetNotSupportedException(); }
		}

		public NotSupportedException GetNotSupportedException()
		{
			throw new NotSupportedException(Res.GetString("0a1953b3-cc5e-43ce-aa4d-b711b3fe39f6", "Import is not supported"));
		}

		#region Constants

		public static class CASSAdjustmentFileHeaderRecord
		{
			public const int RecordID = 2;
			public const int AgentNumber = 11;
			public const int InvoicePeriod = 6;
			public const int Filler = 231;
		}

		public static class CASSAdjustmentFileLineRecord
		{
			public const int RecordID = 2;
			public const int Airline = 3;
			public const int Agent = 11;
			public const int AWB = 8;
			public const int CCADCMNumber = 6;
			public const int WeightChargePP = 12;
			public const int ValuationChargePP = 12;
			public const int CarrierChargePP = 12;
			public const int AgentChargePP = 12;
			public const int WeightChargeCC = 12;
			public const int ValuationChargeCC = 12;
			public const int CarrierChargeCC = 12;
			public const int AgentChargeCC = 12;
			public const int Commission = 12;
			public const int Incentive = 12;
			public const int IncentiveSign = 1;
			public const int Weight = 7;
			public const int WeightUnit = 1;
			public const int Reason = 2;
			public const int Contract = 20;
			public const int Rate = 4;
			public const int AirlineBranch = 1;
			public const int Comment = 215;
		}

		public static class CASSAdjustmentFileTrailerRecord
		{
			public const int RecordID = 2;
			public const int NumberOfRecords = 7;
			public const int Filler = 241;
		}

		public static class RecortID
		{
			public const string Header = "AA";
			public const string AW = "AW";
			public const string CO = "CO";
			public const string CR = "CR";
			public const string DO = "DO";
			public const string DR = "DR";
			public const string ER = "ER";
			public const string Trailer = "TT";
		}

		#endregion
	}
}