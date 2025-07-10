using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Accounting.DataTransfer
{
	public partial class CASSHOTFileFormat : FixedWidthFlatFileFormat
	{
		public override FlatFileDataRow ConvertToRow(ZString rawDataRow)
		{
			CASSHOTFileDataRow result = null;

			string recordType = GetValue((int)AWMTransactionRecord.Position.RecordType, (int)AWMTransactionRecord.Length.RecordType, rawDataRow);

			if (recordType == RecortID.AWM)
			{
				result = ReadAWMValues(rawDataRow);
			}
			else if (recordType == RecortID.DCO ||
				recordType == RecortID.DCR ||
				recordType == RecortID.CCO ||
				recordType == RecortID.CCR ||
				(recordType == RecortID.ECR && CASSBilling.IsRejectedClaimLinesExpected))
			{
				result = ReadAWMCorrectionValues(rawDataRow);
			}
			else if (recordType == RecortID.IBI ||
				recordType == RecortID.IBO ||
				recordType == RecortID.IBR)
			{
				result = ReadIBIValues(rawDataRow);
			}
			else if (recordType == RecortID.Header)
			{
				result = ReadHeaderValues(rawDataRow);
			}
			else if (recordType == RecortID.Header2 || recordType == RecortID.Header3)
			{
				result = ReadHeader2Values(rawDataRow);
			}

			if (result != null)
			{
				result.SetField(CASSHOTFileDataRow.Schema.RecordType, recordType);
			}

			return result;
		}

		CASSHOTFileDataRow ReadHeaderValues(ZString rawDataRow)
		{
			var result = new CASSHOTFileHeaderRow();
			result.SetField(CASSHOTFileHeaderRow.Schema.HeaderDatePeriodStart, GetValue((int)HeaderRecord.Position.DatePeriodStart, (int)HeaderRecord.Length.DatePeriodStart, rawDataRow));
			result.SetField(CASSHOTFileHeaderRow.Schema.HeaderDatePeriodEnd, GetValue((int)HeaderRecord.Position.DatePeriodEnd, (int)HeaderRecord.Length.DatePeriodEnd, rawDataRow));
			result.SetField(CASSHOTFileHeaderRow.Schema.HeaderDateOfBilling, GetValue((int)HeaderRecord.Position.DateOfBilling, (int)HeaderRecord.Length.DateOfBilling, rawDataRow));

			return result;
		}

		CASSHOTFileDataRow ReadHeader2Values(ZString rawDataRow)
		{
			var result = new CASSHOTFileHeaderRow();
			result.SetField(CASSHOTFileHeaderRow.Schema.HeaderDatePeriodStart, GetValue((int)Header2Record.Position.DatePeriodStart, (int)Header2Record.Length.DatePeriodStart, rawDataRow));
			result.SetField(CASSHOTFileHeaderRow.Schema.HeaderDatePeriodEnd, GetValue((int)Header2Record.Position.DatePeriodEnd, (int)Header2Record.Length.DatePeriodEnd, rawDataRow));
			result.SetField(CASSHOTFileHeaderRow.Schema.HeaderDateOfBilling, GetValue((int)Header2Record.Position.DateOfBilling, (int)Header2Record.Length.DateOfBilling, rawDataRow));
			result.SetField(CASSHOTFileHeaderRow.Schema.BillingCurrency, GetValue((int)Header2Record.Position.BillingCurrency, (int)Header2Record.Length.BillingCurrency, rawDataRow));

			return result;
		}

		CASSHOTFileDataRow ReadAWMValues(ZString rawDataRow)
		{
			var result = new CASSHOTFileExportLineRow();
			result.SetField(CASSHOTFileLineRow.Schema.AirlinePrefix, GetValue((int)AWMTransactionRecord.Position.AirlinePrefix, (int)AWMTransactionRecord.Length.AirlinePrefix, rawDataRow));
			result.SetField(CASSHOTFileLineRow.Schema.AWBSerialNumber, GetValue((int)AWMTransactionRecord.Position.AWBSerialNumber, (int)AWMTransactionRecord.Length.AWBSerialNumber, rawDataRow));
			result.SetField(CASSHOTFileLineRow.Schema.AgentCode, GetValue((int)AWMTransactionRecord.Position.AgentCode, (int)AWMTransactionRecord.Length.AgentCode, rawDataRow));
			result.SetField(CASSHOTFileLineRow.Schema.DateAWBExecution, GetValue((int)AWMTransactionRecord.Position.DateAWBExecution, (int)AWMTransactionRecord.Length.DateAWBExecution, rawDataRow));

			result.SetField(CASSHOTFileLineRow.Schema.Origin, GetValue((int)AWMTransactionRecord.Position.Origin, (int)AWMTransactionRecord.Length.Origin, rawDataRow));
			result.SetField(CASSHOTFileLineRow.Schema.Destination, GetValue((int)AWMTransactionRecord.Position.Destination, (int)AWMTransactionRecord.Length.Destination, rawDataRow));

			result.SetField(CASSHOTFileLineRow.Schema.Weight, GetValue((int)AWMTransactionRecord.Position.Weight, (int)AWMTransactionRecord.Length.Weight, rawDataRow));
			result.SetField(CASSHOTFileLineRow.Schema.WeightIndicator, GetValue((int)AWMTransactionRecord.Position.WeightIndicator, (int)AWMTransactionRecord.Length.WeightIndicator, rawDataRow));

			result.SetField(CASSHOTFileLineRow.Schema.CurrencyCode, GetValue((int)AWMTransactionRecord.Position.CurrencyCode, (int)AWMTransactionRecord.Length.CurrencyCode, rawDataRow));

			result.SetField(CASSHOTFileExportLineRow.Schema.WeightCharge, GetValue((int)AWMTransactionRecord.Position.WeightCharge, (int)AWMTransactionRecord.Length.WeightCharge, rawDataRow));
			result.SetField(CASSHOTFileExportLineRow.Schema.ValuationCharge, GetValue((int)AWMTransactionRecord.Position.ValuationCharge, (int)AWMTransactionRecord.Length.ValuationCharge, rawDataRow));
			result.SetField(CASSHOTFileExportLineRow.Schema.ChargesDueCarrier, GetValue((int)AWMTransactionRecord.Position.ChargesDueCarrier, (int)AWMTransactionRecord.Length.ChargesDueCarrier, rawDataRow));
			result.SetField(CASSHOTFileExportLineRow.Schema.ChargesDueAgent, GetValue((int)AWMTransactionRecord.Position.ChargesDueAgent, (int)AWMTransactionRecord.Length.ChargesDueAgent, rawDataRow));
			result.SetField(CASSHOTFileExportLineRow.Schema.Commission, GetValue((int)AWMTransactionRecord.Position.Commission, (int)AWMTransactionRecord.Length.Commission, rawDataRow));
			result.SetField(CASSHOTFileExportLineRow.Schema.Discount, GetValue((int)AWMTransactionRecord.Position.DiscountIndicator, (int)AWMTransactionRecord.Length.DiscountIndicator, rawDataRow) +
																		GetValue((int)AWMTransactionRecord.Position.Discount, (int)AWMTransactionRecord.Length.Discount, rawDataRow));

			result.SetField(CASSHOTFileExportLineRow.Schema.VATIndicator, GetValue((int)AWMTransactionRecord.Position.VATIndicator, (int)AWMTransactionRecord.Length.VATIndicator, rawDataRow));
			result.SetField(CASSHOTFileExportLineRow.Schema.VATDueAirline, GetValue((int)AWMTransactionRecord.Position.TaxDueAirline, (int)AWMTransactionRecord.Length.TaxDueAirline, rawDataRow));
			result.SetField(CASSHOTFileExportLineRow.Schema.VATDueAgent, GetValue((int)AWMTransactionRecord.Position.TaxDueAgent, (int)AWMTransactionRecord.Length.TaxDueAgent, rawDataRow));

			return result;
		}

		CASSHOTFileDataRow ReadAWMCorrectionValues(ZString rawDataRow)
		{
			var result = new CASSHOTFileExportLineRow();
			result.SetField(CASSHOTFileLineRow.Schema.AirlinePrefix, GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.AirlinePrefix, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.AirlinePrefix, rawDataRow));
			result.SetField(CASSHOTFileLineRow.Schema.AWBSerialNumber, GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.AWBSerialNumber, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.AWBSerialNumber, rawDataRow));
			result.SetField(CASSHOTFileLineRow.Schema.AgentCode, GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.AgentCode, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.AgentCode, rawDataRow));
			result.SetField(CASSHOTFileExportLineRow.Schema.CCADCMNumber, GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.CCADCMNumber, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.CCADCMNumber, rawDataRow));
			result.SetField(CASSHOTFileLineRow.Schema.DateAWBExecution, GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.DateAWBExecution, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.DateAWBExecution, rawDataRow));

			result.SetField(CASSHOTFileLineRow.Schema.Origin, GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.Origin, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.Origin, rawDataRow));
			result.SetField(CASSHOTFileLineRow.Schema.Destination, GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.Destination, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.Destination, rawDataRow));

			result.SetField(CASSHOTFileLineRow.Schema.Weight, GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.Weight, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.Weight, rawDataRow));
			result.SetField(CASSHOTFileLineRow.Schema.WeightIndicator, GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.WeightIndicator, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.WeightIndicator, rawDataRow));

			result.SetField(CASSHOTFileLineRow.Schema.CurrencyCode, GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.CurrencyCode, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.CurrencyCode, rawDataRow));

			string indicator = GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.WeightChargeIndicator, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.WeightChargeIndicator, rawDataRow);
			string charge = GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.WeightCharge, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.WeightCharge, rawDataRow);
			result.SetField(CASSHOTFileExportLineRow.Schema.WeightCharge, indicator == PP_CCIndicator.PP ? charge : "0");

			indicator = GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.ValuationChargeIndicator, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.ValuationChargeIndicator, rawDataRow);
			charge = GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.ValuationCharge, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.ValuationCharge, rawDataRow);
			result.SetField(CASSHOTFileExportLineRow.Schema.ValuationCharge, indicator == PP_CCIndicator.PP ? charge : "0");

			indicator = GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.ChargesDueCarrierIndicator, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.ChargesDueCarrierIndicator, rawDataRow);
			charge = GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.ChargesDueCarrier, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.ChargesDueCarrier, rawDataRow);
			result.SetField(CASSHOTFileExportLineRow.Schema.ChargesDueCarrier, indicator == PP_CCIndicator.PP ? charge : "0");

			indicator = GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.ChargesDueAgentIndicator, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.ChargesDueAgentIndicator, rawDataRow);
			charge = GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.ChargesDueAgent, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.ChargesDueAgent, rawDataRow);
			result.SetField(CASSHOTFileExportLineRow.Schema.ChargesDueAgent, indicator == PP_CCIndicator.CC ? charge : "0");

			result.SetField(CASSHOTFileExportLineRow.Schema.Commission, GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.Commission, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.Commission, rawDataRow));
			result.SetField(CASSHOTFileExportLineRow.Schema.Discount, GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.DiscountIndicator, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.DiscountIndicator, rawDataRow) +
																		GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.Discount, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.Discount, rawDataRow));

			result.SetField(CASSHOTFileExportLineRow.Schema.VATIndicator, GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.VATIndicator, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.VATIndicator, rawDataRow));
			result.SetField(CASSHOTFileExportLineRow.Schema.VATDueAirline, GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.VATOnAWBCharges, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.VATOnAWBCharges, rawDataRow));
			result.SetField(CASSHOTFileExportLineRow.Schema.VATDueAgent, GetValue((int)DCR_DCR_CCO_CCRTransactionRecord.Position.VATOnCommission, (int)DCR_DCR_CCO_CCRTransactionRecord.Length.VATOnCommission, rawDataRow));

			return result;
		}

		CASSHOTFileDataRow ReadIBIValues(ZString rawDataRow)
		{
			var result = new CASSHOTFileImportLineRow();
			result.SetField(CASSHOTFileLineRow.Schema.AirlinePrefix, GetValue((int)IBI_IBO_IBRTransactionRecord.Position.AirlinePrefix, (int)IBI_IBO_IBRTransactionRecord.Length.AirlinePrefix, rawDataRow));
			result.SetField(CASSHOTFileLineRow.Schema.AWBSerialNumber, GetValue((int)IBI_IBO_IBRTransactionRecord.Position.AWBSerialNumber, (int)IBI_IBO_IBRTransactionRecord.Length.AWBSerialNumber, rawDataRow));
			result.SetField(CASSHOTFileLineRow.Schema.AgentCode, GetValue((int)IBI_IBO_IBRTransactionRecord.Position.RecipientCode, (int)IBI_IBO_IBRTransactionRecord.Length.RecipientCode, rawDataRow));
			result.SetField(CASSHOTFileLineRow.Schema.DateOfArrival, GetValue((int)IBI_IBO_IBRTransactionRecord.Position.DateOfArrival, (int)IBI_IBO_IBRTransactionRecord.Length.DateOfArrival, rawDataRow));
			result.SetField(CASSHOTFileLineRow.Schema.DateOfDelivery, GetValue((int)IBI_IBO_IBRTransactionRecord.Position.DateOfDelivery, (int)IBI_IBO_IBRTransactionRecord.Length.DateOfDelivery, rawDataRow));

			result.SetField(CASSHOTFileLineRow.Schema.Origin, GetValue((int)IBI_IBO_IBRTransactionRecord.Position.Origin, (int)IBI_IBO_IBRTransactionRecord.Length.Origin, rawDataRow));
			result.SetField(CASSHOTFileLineRow.Schema.Destination, GetValue((int)IBI_IBO_IBRTransactionRecord.Position.Destination, (int)IBI_IBO_IBRTransactionRecord.Length.Destination, rawDataRow));

			result.SetField(CASSHOTFileLineRow.Schema.Weight, GetValue((int)IBI_IBO_IBRTransactionRecord.Position.Weight, (int)IBI_IBO_IBRTransactionRecord.Length.Weight, rawDataRow));
			result.SetField(CASSHOTFileLineRow.Schema.WeightIndicator, GetValue((int)IBI_IBO_IBRTransactionRecord.Position.WeightIndicator, (int)IBI_IBO_IBRTransactionRecord.Length.WeightIndicator, rawDataRow));

			result.SetField(CASSHOTFileLineRow.Schema.CurrencyCode, GetValue((int)IBI_IBO_IBRTransactionRecord.Position.CurrencyCode, (int)IBI_IBO_IBRTransactionRecord.Length.CurrencyCode, rawDataRow));

			result.SetField(CASSHOTFileImportLineRow.Schema.WeightCharges, GetValue((int)IBI_IBO_IBRTransactionRecord.Position.WeightCharges, (int)IBI_IBO_IBRTransactionRecord.Length.WeightCharges, rawDataRow));
			result.SetField(CASSHOTFileImportLineRow.Schema.ChargesDueAgent, GetValue((int)IBI_IBO_IBRTransactionRecord.Position.ChargesDueAgent, (int)IBI_IBO_IBRTransactionRecord.Length.ChargesDueAgent, rawDataRow));
			result.SetField(CASSHOTFileImportLineRow.Schema.ChargesDueCarrier, GetValue((int)IBI_IBO_IBRTransactionRecord.Position.ChargesDueCarrier, (int)IBI_IBO_IBRTransactionRecord.Length.ChargesDueCarrier, rawDataRow));
			result.SetField(CASSHOTFileImportLineRow.Schema.FeeAmount, GetValue((int)IBI_IBO_IBRTransactionRecord.Position.FeeAmount, (int)IBI_IBO_IBRTransactionRecord.Length.FeeAmount, rawDataRow));
			result.SetField(CASSHOTFileImportLineRow.Schema.FeeCharged, GetValue((int)IBI_IBO_IBRTransactionRecord.Position.FeeCharged, (int)IBI_IBO_IBRTransactionRecord.Length.FeeCharged, rawDataRow));
			result.SetField(CASSHOTFileImportLineRow.Schema.HandlingCharges, GetValue((int)IBI_IBO_IBRTransactionRecord.Position.HandlingCharges, (int)IBI_IBO_IBRTransactionRecord.Length.HandlingCharges, rawDataRow));
			result.SetField(CASSHOTFileImportLineRow.Schema.StorageCharges, GetValue((int)IBI_IBO_IBRTransactionRecord.Position.StorageCharges, (int)IBI_IBO_IBRTransactionRecord.Length.StorageCharges, rawDataRow));
			result.SetField(CASSHOTFileImportLineRow.Schema.OtherCharge1Amount, GetValue((int)IBI_IBO_IBRTransactionRecord.Position.OtherCharge1Amount, (int)IBI_IBO_IBRTransactionRecord.Length.OtherCharge1Amount, rawDataRow));
			result.SetField(CASSHOTFileImportLineRow.Schema.OtherCharge2Amount, GetValue((int)IBI_IBO_IBRTransactionRecord.Position.OtherCharge2Amount, (int)IBI_IBO_IBRTransactionRecord.Length.OtherCharge2Amount, rawDataRow));
			result.SetField(CASSHOTFileImportLineRow.Schema.MiscellaneousChargesAmount, GetValue((int)IBI_IBO_IBRTransactionRecord.Position.MiscellaneousChargesAmount, (int)IBI_IBO_IBRTransactionRecord.Length.MiscellaneousChargesAmount, rawDataRow));

			return result;
		}

		public override FileExtensionType FileExtensionForImport
		{
			get { return FileExtensionType.ClientSpecific; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded file extension")]
		protected override string GetClientSpecificFileExtension()
		{
			return "hot";
		}

		public override FileExtensionType FileExtensionForExport
		{
			get { return FileExtensionType.ClientSpecific; }
		}

		public override ZString ConvertToLine(FlatFileDataRow row)
		{
			throw new NotSupportedException("Export to CASS HOT file format is not supported");
		}

		#region Constants

#if DEBUG
		internal
#endif
		class AWMTransactionRecord
		{
			public enum Position
			{
				RecordType = 0,
				AirlinePrefix = 5,
				AWBSerialNumber = 8,
				AgentCode = 21,
				DateAWBExecution = 39,
				Origin = 18,
				Destination = 36,
				Weight = 45,
				WeightIndicator = 52,
				CurrencyCode = 53,
				WeightCharge = 56,
				ValuationCharge = 68,
				ChargesDueCarrier = 80,
				ChargesDueAgent = 140,
				Commission = 156,
				Discount = 210,
				DiscountIndicator = 249,
				VATIndicator = 4,
				TaxDueAirline = 222,
				TaxDueAgent = 230
			}

			public enum Length
			{
				RecordType = 3,
				AirlinePrefix = 3,
				AWBSerialNumber = 8,
				AgentCode = 11,
				DateAWBExecution = 6,
				Origin = 3,
				Destination = 3,
				Weight = 7,
				WeightIndicator = 1,
				CurrencyCode = 3,
				WeightCharge = 12,
				ValuationCharge = 12,
				ChargesDueCarrier = 12,
				ChargesDueAgent = 12,
				Commission = 12,
				Discount = 12,
				DiscountIndicator = 1,
				VATIndicator = 1,
				TaxDueAirline = 8,
				TaxDueAgent = 8
			}
		}

		sealed
#if DEBUG
		internal
#endif
		class DCR_DCR_CCO_CCRTransactionRecord
		{
			public enum Position
			{
				RecordType = 0,
				AirlinePrefix = 5,
				AWBSerialNumber = 8,
				AgentCode = 21,
				CCADCMNumber = 32,
				DateAWBExecution = 52,
				Origin = 18,
				Destination = 180,
				Weight = 173,
				WeightIndicator = 172,
				CurrencyCode = 38,
				WeightChargeIndicator = 58,
				WeightCharge = 59,
				ValuationChargeIndicator = 71,
				ValuationCharge = 72,
				ChargesDueCarrierIndicator = 110,
				ChargesDueCarrier = 111,
				ChargesDueAgentIndicator = 97,
				ChargesDueAgent = 98,
				Commission = 135,
				Discount = 159,
				DiscountIndicator = 171,
				VATIndicator = 4,
				VATOnAWBCharges = 123,
				VATOnCommission = 147
			}

			public enum Length
			{
				RecordType = 3,
				AirlinePrefix = 3,
				AWBSerialNumber = 8,
				AgentCode = 11,
				CCADCMNumber = 6,
				DateAWBExecution = 6,
				Origin = 3,
				Destination = 3,
				Weight = 7,
				WeightIndicator = 1,
				CurrencyCode = 3,
				WeightChargeIndicator = 1,
				WeightCharge = 12,
				ValuationChargeIndicator = 1,
				ValuationCharge = 12,
				ChargesDueCarrierIndicator = 1,
				ChargesDueCarrier = 12,
				ChargesDueAgentIndicator = 1,
				ChargesDueAgent = 12,
				Commission = 12,
				Discount = 12,
				DiscountIndicator = 1,
				VATIndicator = 1,
				VATOnAWBCharges = 12,
				VATOnCommission = 12
			}
		}

		sealed
#if DEBUG
		internal
#endif
		class IBI_IBO_IBRTransactionRecord
		{
			public enum Position
			{
				RecordType = 0,
				AirlinePrefix = 37,
				AWBSerialNumber = 40,
				RecipientCode = 22,
				DateOfArrival = 68,
				DateOfDelivery = 74,
				Origin = 56,
				Destination = 59,
				Weight = 80,
				WeightIndicator = 87,
				CurrencyCode = 88,
				WeightCharges = 101,
				ChargesDueAgent = 113,
				ChargesDueCarrier = 125,
				FeeAmount = 149,
				FeeCharged = 161,
				HandlingCharges = 162,
				StorageCharges = 174,
				OtherCharge1Amount = 200,
				OtherCharge2Amount = 214,
				MiscellaneousChargesAmount = 226
			}

			public enum Length
			{
				RecordType = 3,
				AirlinePrefix = 3,
				AWBSerialNumber = 8,
				RecipientCode = 11,
				DateOfArrival = 6,
				DateOfDelivery = 6,
				Origin = 3,
				Destination = 3,
				Weight = 7,
				WeightIndicator = 1,
				CurrencyCode = 3,
				WeightCharges = 12,
				ChargesDueAgent = 12,
				ChargesDueCarrier = 12,
				FeeAmount = 12,
				FeeCharged = 1,
				HandlingCharges = 12,
				StorageCharges = 12,
				OtherCharge1Amount = 12,
				OtherCharge2Amount = 12,
				MiscellaneousChargesAmount = 12
			}
		}

		sealed
#if DEBUG
		internal
#endif
		class HeaderRecord
		{
			public enum Position
			{
				RecordType = 0,
				DatePeriodStart = 8,
				DatePeriodEnd = 14,
				DateOfBilling = 20
			}

			public enum Length
			{
				RecordType = 3,
				DatePeriodStart = 6,
				DatePeriodEnd = 6,
				DateOfBilling = 6
			}
		}

		sealed
#if DEBUG
		internal
#endif
		class Header2Record
		{
			public enum Position
			{
				RecordType = 0,
				DatePeriodStart = 16,
				DatePeriodEnd = 22,
				DateOfBilling = 28,
				BillingCurrency = 36
			}

			public enum Length
			{
				RecordType = 3,
				DatePeriodStart = 6,
				DatePeriodEnd = 6,
				DateOfBilling = 6,
				BillingCurrency = 3
			}
		}

		sealed class PP_CCIndicator
		{
			public const string PP = "P";
			public const string CC = "C";
		}

		public static class RecortID
		{
			public const string Header = "AAA";
			public const string Header2 = "AA2";
			public const string Header3 = "AA3";
			public const string AWM = "AWM";
			public const string DCO = "DCO";
			public const string DCR = "DCR";
			public const string CCO = "CCO";
			public const string CCR = "CCR";
			public const string ECR = "ECR";
			public const string IBI = "IBI";
			public const string IBO = "IBO";
			public const string IBR = "IBR";
		}

		#endregion
	}
}
