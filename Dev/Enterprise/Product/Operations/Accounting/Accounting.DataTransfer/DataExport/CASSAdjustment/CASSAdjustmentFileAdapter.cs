using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;

namespace Enterprise.Accounting.DataTransfer
{
	public class CASSAdjustmentFileAdapter : ValueObjectDataAdapter<CASSBilling, CASSAdjustmentHeader>
	{
		public CASSAdjustmentFileAdapter()
			: base()
		{
		}

		#region Export

		protected override void ExportToValueObjectCore(CASSBilling cassBilling, CASSAdjustmentHeader cassAdjustmentHeader, Enterprise.DataTransfer.Integration.IValueObjectExportContext context)
		{
			if (cassBilling.IsExportBilling)
			{
				var msg = DoesCostHeaderHaveValidInfo(cassBilling);

				if (msg.Length == 0)
				{
					cassAdjustmentHeader.AgentNumber = GetIATAAgentCodeWithoutSpecialCharacters();
					cassAdjustmentHeader.BillingPeriodEnd = cassBilling.BillingPeriodEnd;

					foreach (CASSCostExportLine costline in cassBilling.CostHeader.ExportLines)
					{
						if (costline.HasAnyAmountChanged())
						{
							msg = DoesCostLineHaveValidInfo(costline);
							if (msg.Length == 0)
							{
								var adjustmentLine = cassAdjustmentHeader.Lines.AddNew();
								adjustmentLine.RecordType = ConvertCostRecordTypeToAdjustmentRecordType(costline.RecordType);
								adjustmentLine.AirlinePrefix = costline.AirlinePrefix;
								adjustmentLine.AgentCode = cassAdjustmentHeader.AgentNumber;
								adjustmentLine.AWBSerialNumber = costline.AWBSerialNumber;
								adjustmentLine.CCADCMNumber = costline.CCADCMNumber;
								adjustmentLine.WeightChargePP = costline.WeightChargePP.Round(2) * costline.CurrencyISOSubUnitRatio;
								adjustmentLine.ValuationChargePP = costline.ValuationChargePP.Round(2) * costline.CurrencyISOSubUnitRatio;
								adjustmentLine.ChargesDueCarrierPP = costline.ChargesDueCarrierPP.Round(2) * costline.CurrencyISOSubUnitRatio;
								adjustmentLine.ChargesDueAgentCC = costline.ChargesDueAgentCC.Round(2) * costline.CurrencyISOSubUnitRatio;
								adjustmentLine.Commission = costline.Commission.Round(2) * costline.CurrencyISOSubUnitRatio;
								adjustmentLine.Incentive = costline.Discount.Round(2) * costline.CurrencyISOSubUnitRatio;
								adjustmentLine.Weight = costline.Weight.Round(2) * costline.WeightSubUnitRatio;
								adjustmentLine.WeightUnit = costline.WeightUnit.Substring(0, 1);
								adjustmentLine.ReasonCode = ZInt.Parse(costline.AdjustmentReason);
								adjustmentLine.Comment = costline.AdjustmentReasonComment;
							}
							else
							{
								context.AddError(Res.GetString("c32f6f14-1911-41fd-a39c-48886d046127", "Could Not Export Line: {0}", msg.ToStringWithDelimiterBetweenAppends("|")));
							}
						}
					}
				}
				else
				{
					context.AddError(Res.GetString("267d2c87-8f0c-41f9-a94c-43c3d605869e", "Could Not Export Header: {0}", msg.ToStringWithDelimiterBetweenAppends("|")));
				}
			}
			else
			{
				throw new InvalidOperationException(Res.GetString("f7ac40df-084c-41c6-9950-c4106b0acc51", "CASS Adjustment file can be created only for CASS HOT file with Export records"));
			}
		}

		ZString ConvertCostRecordTypeToAdjustmentRecordType(ZString costRecordType)
		{
			ZString result = ZString.Empty;
			switch (costRecordType)
			{
				case CASSHOTFileFormat.RecortID.AWM:
					result = CASSAdjustmentFileFormat.RecortID.AW;
					break;
				case CASSHOTFileFormat.RecortID.CCO:
					result = CASSAdjustmentFileFormat.RecortID.CO;
					break;
				case CASSHOTFileFormat.RecortID.CCR:
					result = CASSAdjustmentFileFormat.RecortID.CR;
					break;
				case CASSHOTFileFormat.RecortID.DCO:
					result = CASSAdjustmentFileFormat.RecortID.DO;
					break;
				case CASSHOTFileFormat.RecortID.DCR:
					result = CASSAdjustmentFileFormat.RecortID.DR;
					break;
				case CASSHOTFileFormat.RecortID.ECR:
					result = CASSAdjustmentFileFormat.RecortID.ER;
					break;
				default:
					break;
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "CASS Adjustment File Field Name")]
		ZStringBuilder DoesCostHeaderHaveValidInfo(CASSBilling billing)
		{
			var agentCodeMsgBuilder = new ZStringBuilder();
			if (string.IsNullOrEmpty(Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode))
			{
				agentCodeMsgBuilder.Append(Res.GetString("899c9a41-54a8-4262-aec8-b5c0e7d8f219", "Agent Code is missing. You can set it at Registry > Freight > AWB > MAWB > Issuing Carrier Agent > IATA Code"));
			}

			var agentCodeWithoutSpecialCharacters = GetIATAAgentCodeWithoutSpecialCharacters();

			var match = new Regex(@"^[0-9]+$").Match(agentCodeWithoutSpecialCharacters);
			if (!match.Success)
			{
				agentCodeMsgBuilder.Append(Res.GetString("4fc57fba-d564-4cf1-a1e2-b53c4059e8a9", "Invalid Agent Code. Only numbers (0 - 9), '/', '-' and ' ' are allowed in Agent Code. You can modify the code at Registry > Freight > AWB > MAWB > Issuing Carrier Agent > IATA Code"));
			}

			if (agentCodeWithoutSpecialCharacters.Length > CASSAdjustmentFileFormat.CASSAdjustmentFileHeaderRecord.AgentNumber)
			{
				agentCodeMsgBuilder.Append(Res.GetString("deb7bbae-f596-4e5f-aeeb-3831cc80530d", "Agent Code is too long. Maximum possible length is 11 excluding special characters ('/', '-', ' '). You can modify the code at Registry > Freight > AWB > MAWB > Issuing Carrier Agent > IATA Code"));
			}

			var notificationBuilder = new ZStringBuilder();
			notificationBuilder.AppendIfNotEmpty(PrepareMessage("Agent Code", Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode, agentCodeMsgBuilder));

			if (billing.BillingPeriodEnd.IsEmpty)
			{
				notificationBuilder.AppendIfNotEmpty(Res.GetString("0ece17de-467b-42a5-bdac-aa4d33f8a838", "Invoice Period: Cannot be Empty"));
			}
			return notificationBuilder;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "CASS Adjustment File Field Name")]
		ZStringBuilder DoesCostLineHaveValidInfo(CASSCostExportLine costLine)
		{
			var notificationBuilder = new ZStringBuilder();

			if (ConvertCostRecordTypeToAdjustmentRecordType(costLine.RecordType).Equals(string.Empty))
			{
				notificationBuilder.Append(Res.GetString("67900709-0009-4bb5-bc6e-e88def31ecbb", "Record Type: Invalid Record Type {0}. Expected Record Types are: AWM, CCO, CCR, DCO, DCR, ECR"));
			}

			notificationBuilder.AppendIfNotEmpty(ValidateValue("Airline Number", costLine.AirlinePrefix, CASSAdjustmentFileFormat.CASSAdjustmentFileLineRecord.Airline, true));
			notificationBuilder.AppendIfNotEmpty(ValidateValueAsInt("Air Waybill Number", costLine.AWBSerialNumber, CASSAdjustmentFileFormat.CASSAdjustmentFileLineRecord.AWB));
			notificationBuilder.AppendIfNotEmpty(ValidateValue("CCA/DCM Number", costLine.CCADCMNumber, CASSAdjustmentFileFormat.CASSAdjustmentFileLineRecord.CCADCMNumber, false));
			notificationBuilder.AppendIfNotEmpty(ValidateValueAsDecimal("Prepaid Weight Charge", costLine.WeightChargePP.ToString("G", CultureInfo.InvariantCulture), CASSAdjustmentFileFormat.CASSAdjustmentFileLineRecord.WeightChargePP));
			notificationBuilder.AppendIfNotEmpty(ValidateValueAsDecimal("Prepaid Valuation Charge", costLine.ValuationChargePP.ToString("G", CultureInfo.InvariantCulture), CASSAdjustmentFileFormat.CASSAdjustmentFileLineRecord.ValuationChargePP));
			notificationBuilder.AppendIfNotEmpty(ValidateValueAsDecimal("Prepaid Charges due Airline", costLine.ChargesDueCarrierPP.ToString("G", CultureInfo.InvariantCulture), CASSAdjustmentFileFormat.CASSAdjustmentFileLineRecord.CarrierChargePP));
			notificationBuilder.AppendIfNotEmpty(ValidateValueAsDecimal("Collect Charges due Agent", costLine.ChargesDueAgentCC.ToString("G", CultureInfo.InvariantCulture), CASSAdjustmentFileFormat.CASSAdjustmentFileLineRecord.AgentChargeCC));
			notificationBuilder.AppendIfNotEmpty(ValidateValueAsDecimal("Commission", costLine.Commission.ToString("G", CultureInfo.InvariantCulture), CASSAdjustmentFileFormat.CASSAdjustmentFileLineRecord.Commission));
			notificationBuilder.AppendIfNotEmpty(ValidateValueAsDecimal("Incentive", Math.Abs(costLine.Discount).ToString("G", CultureInfo.InvariantCulture), CASSAdjustmentFileFormat.CASSAdjustmentFileLineRecord.Incentive));
			notificationBuilder.AppendIfNotEmpty(ValidateValueAsDecimal("Weight", Math.Abs(costLine.Weight).ToString("G", CultureInfo.InvariantCulture), CASSAdjustmentFileFormat.CASSAdjustmentFileLineRecord.Weight));
			notificationBuilder.AppendIfNotEmpty(ValidateValue("Weight Code", costLine.WeightUnit.Substring(0, 1), CASSAdjustmentFileFormat.CASSAdjustmentFileLineRecord.WeightUnit, true));
			notificationBuilder.AppendIfNotEmpty(ValidateValueAsInt("Reason Code", costLine.AdjustmentReason, CASSAdjustmentFileFormat.CASSAdjustmentFileLineRecord.Reason));
			notificationBuilder.AppendIfNotEmpty(ValidateValue("Comment", costLine.AdjustmentReasonComment, CASSAdjustmentFileFormat.CASSAdjustmentFileLineRecord.Comment, false));

			return notificationBuilder;
		}

		string ValidateValueAsInt(string fieldName, string proposedValue, int maxLength, bool matchMaxLength = false)
		{
			ZStringBuilder msg = new ZStringBuilder();

			msg.AppendIfNotEmpty(CheckLength(proposedValue, maxLength, matchMaxLength));

			if (!ZInt.CanParse(proposedValue))
			{
				msg.AppendIfNotEmpty(Res.GetString("198e71ee-52db-4c68-9db0-5d3ef51ae8bb", "Invalid Data Type: Expected Type: Integer"));
			}

			return PrepareMessage(fieldName, proposedValue, msg);
		}

		string ValidateValueAsDecimal(string fieldName, string proposedValue, int maxLength, bool matchMaxLength = false)
		{
			ZStringBuilder msg = new ZStringBuilder();

			msg.AppendIfNotEmpty(CheckLength(proposedValue, maxLength, matchMaxLength));

			if (!ZDecimal.CanParse(proposedValue))
			{
				msg.AppendIfNotEmpty(Res.GetString("5379ef55-f328-46c5-b863-21bba9140225", "Invalid Data Type: Expected Type: Decimal"));
			}

			return PrepareMessage(fieldName, proposedValue, msg);
		}

		string ValidateValue(string fieldName, string proposedValue, int maxLength, bool matchMaxLength)
		{
			ZStringBuilder msg = new ZStringBuilder();

			msg.AppendIfNotEmpty(CheckLength(proposedValue, maxLength, matchMaxLength));

			return PrepareMessage(fieldName, proposedValue, msg);
		}

		string CheckLength(string proposedValue, int maxLength, bool matchMaxLength)
		{
			ZStringBuilder msg = new ZStringBuilder();

			if (proposedValue.Length > maxLength)
			{
				msg.Append(Res.GetString("97e68d0f-356f-4d8e-a1d3-b61f51eddc9a", "Allowed Maximum Length: {0}, Provided Value Length: {1}", maxLength, proposedValue.Length));
			}

			if (matchMaxLength && proposedValue.Length != maxLength)
			{
				msg.Append(Res.GetString("d1efa802-d69f-41fd-a054-1e8f25c00593", "Length should be: {0}, Provided Value Length: {1}", maxLength, proposedValue.Length));
			}

			return msg.ToStringWithDelimiterBetweenAppends("|");
		}

		string PrepareMessage(string fieldName, string proposedValue, ZStringBuilder errorMessage)
		{
			return errorMessage.Length > 0 ? Res.GetString("7854a331-6346-4c40-8ee4-d914443457f4", "{0} [{1}] : {2}", fieldName, proposedValue, errorMessage.ToStringWithDelimiterBetweenAppends(" ; ")) : string.Empty;
		}

		string GetIATAAgentCodeWithoutSpecialCharacters() => Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode.Replace("/", "").Replace("-", "").Replace(" ", "").Trim();

		#endregion

		#region Import

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hardcoded Property Name")]
		protected override void ImportFromValueObjectCore(CASSBilling bizObj, CASSAdjustmentHeader value, Enterprise.DataTransfer.Integration.IValueObjectImportContext context)
		{
			throw GetNotSupportedException("ImportFromValueObjectCore");
		}

		#endregion

		#region Implementation

		public override System.Xml.Schema.XmlSchema CollectionSchema
		{
			get { throw GetNotSupportedException("CollectionSchema"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hardcoded Property Name")]
		public override string RootCollectionElementName
		{
			get { throw GetNotSupportedException("RootCollectionElementName"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hardcoded Property Name")]
		public override string RootElementName
		{
			get { throw GetNotSupportedException("RootElementName"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hardcoded Property Name")]
		public override System.Xml.Schema.XmlSchema Schema
		{
			get { throw GetNotSupportedException("Schema"); }
		}

		public NotSupportedException GetNotSupportedException(string name)
		{
			throw new NotSupportedException(Res.GetString("1c2f95b9-a0cf-4b38-a31b-eac3ce01ec7a", "{0} is not supported by CASS Adjustment File Adapter.", name));
		}

		#endregion
	}
}
