using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Accounting.DataTransfer
{
	public class CASSAdjustmentFileConverter : FlatFileConverter
	{
		public CASSAdjustmentFileConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
		}

		public override void ImportFlatFile(IValueObject valueObject, IFlatFileFormat flatFileFormat, TextReader reader)
		{
			throw new NotSupportedException("This converter doesn't support file import");  // Hardcoded Error Description
		}

		protected override FlatFileDataRowCollection MapExport(IValueObject valueObject)
		{
			var fileLines = new FlatFileDataRowCollection();
			var header = valueObject as CASSAdjustmentHeader;
			if (header != null)
			{
				var headerRow = new CASSAdjustmentFileHeaderRow();
				headerRow.Agent = header.AgentNumber;
				headerRow.InvoicePeriod = header.InvoicePeriod;
				fileLines.Add(headerRow);

				foreach (CASSAdjustmentLine adjustmentLine in header.Lines)
				{
					var lineRow = new CASSAdjustmentFileLineRow();
					lineRow.RecordType = adjustmentLine.RecordType;
					lineRow.Airline = adjustmentLine.AirlinePrefix;
					lineRow.Agent = adjustmentLine.AgentCode;
					lineRow.AWBNumber = adjustmentLine.AWBSerialNumber;
					lineRow.CCADCMNumber = adjustmentLine.CCADCMNumber;
					lineRow.WeightChargePP = adjustmentLine.WeightChargePP;
					lineRow.ValuationChargePP = adjustmentLine.ValuationChargePP;
					lineRow.CarrierChargePP = adjustmentLine.ChargesDueCarrierPP;
					lineRow.AgentChargeCC = adjustmentLine.ChargesDueAgentCC;
					lineRow.Commission = adjustmentLine.Commission;
					lineRow.Incentive = adjustmentLine.Incentive;
					lineRow.Weight = adjustmentLine.Weight;
					lineRow.WeightUnit = adjustmentLine.WeightUnit;
					lineRow.Reason = adjustmentLine.ReasonCode;
					lineRow.Comment = adjustmentLine.Comment;
					fileLines.Add(lineRow);
				}

				var trailerRecord = new CASSAdjustmentFileTrailerRow();
				trailerRecord.NumberOfRecords = header.Lines.Count;
				fileLines.Add(trailerRecord);
			}

			return fileLines;
		}

		protected override void CheckArguments(IValueObject valueObject, IFlatFileFormat flatFileFormat)
		{
			base.CheckArguments(valueObject, flatFileFormat);
			if (!(valueObject is CASSAdjustmentHeader))
			{
				throw new ArgumentException("Not supported type of ValueObject", nameof(valueObject)); // Hardcoded Error Description
			}
			if (!(flatFileFormat is CASSAdjustmentFileFormat))
			{
				throw new ArgumentException("Not supported format type", nameof(flatFileFormat)); // Hardcoded Error Description
			}
		}
	}
}
