using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer;

namespace Enterprise.Accounting.Business.Testing
{
	public static class CASSTestHelper
	{
		public static void SetupExportCASSBillingLine(CASSBillingLine cassBillingLine, ZDecimal pWCAmount, ZDecimal pVCAmount, ZDecimal pCCAmount, ZDecimal cOAAmount, ZDecimal cOMAmount, ZDecimal dOIAmount, ZDecimal vatDueAirline, decimal vatDueAgent, ZDecimal adjustedVATDueAirline, ZDecimal adjustedVATDueAgent, bool setupAdjustmentLine = true)
		{
			SetupCASSCostComponent(cassBillingLine, false, pWCAmount, pVCAmount, pCCAmount, cOAAmount, cOMAmount, dOIAmount, vatDueAirline, vatDueAgent);
			if (setupAdjustmentLine)
			{
				SetupCASSCostComponent(cassBillingLine, true, pWCAmount * 0.05M, pVCAmount * 0.05M, pCCAmount * 0.05M, cOAAmount * 0.05M, cOMAmount * 0.05M, dOIAmount * 0.05M, adjustedVATDueAirline, adjustedVATDueAgent);
			}
		}

		public static CASSCostExportLine CreateExportCASSCostLine(string recordType, bool isAdjustedAmount, ZDecimal pWCAmount, ZDecimal pVCAmount, ZDecimal pCCAmount, ZDecimal cOAAmount, ZDecimal cOMAmount, ZDecimal dOIAmount, ZDecimal vatDueAirline, decimal vatDueAgent)
		{
			return SetupCASSCostComponent(null, isAdjustedAmount, pWCAmount, pVCAmount, pCCAmount, cOAAmount, cOMAmount, dOIAmount, vatDueAirline, vatDueAgent, recordType: recordType);
		}

		static CASSCostExportLine SetupCASSCostComponent(CASSBillingLine cassBillingLine, bool isAdjustedAmount, ZDecimal pWCAmount, ZDecimal pVCAmount, ZDecimal pCCAmount, ZDecimal cOAAmount, ZDecimal cOMAmount, ZDecimal dOIAmount,
													decimal vatDueAirline = 0M, decimal vatDueAgent = 0M,
													string vatIndicator = "Y", string airlinePrefix = "172", string awbSerialNumber = "67828073", string agentCode = "23470068510", string origin = "LEJ", string destination = "MEX", decimal weight = 2150M, string weightUnit = "KG", string currency = "AUD",
													bool setupAmount = true, bool isRejectedRecordType = false, int multiplier = 1, string recordType = "")
		{
			var myRecordType = string.IsNullOrEmpty(recordType) ? (isAdjustedAmount ? CASSHOTFileFormat.RecortID.DCO : CASSHOTFileFormat.RecortID.AWM) : recordType;

			CASSCostLineType lineType = CASSCostLineType.Default;
			if (myRecordType == CASSHOTFileFormat.RecortID.ECR)
			{
				lineType = CASSCostLineType.Rejected;
			}
			else if (myRecordType == CASSHOTFileFormat.RecortID.AWM)
			{
				lineType = CASSCostLineType.Billing;
			}
			else if (isAdjustedAmount)
			{
				lineType = CASSCostLineType.Adjustment;
			}

			var costLine = new CASSCostExportLine(new BusinessObjectFactory(), lineType);
			costLine.RecordType = myRecordType;
			costLine.VATIndicator = vatIndicator;
			costLine.AirlinePrefix = airlinePrefix;
			costLine.AWBSerialNumber = awbSerialNumber;
			costLine.AgentCode = agentCode;
			costLine.DateAWBExecution = new ZDateTime(2008, 06, 07);
			costLine.DateOfArrival = new ZDateTime(2008, 07, 07);
			costLine.DateOfDelivery = new ZDateTime(2008, 08, 07);
			costLine.Origin = origin;
			costLine.Destination = destination;
			costLine.Weight = weight;
			costLine.WeightUnit = weightUnit;
			costLine.CurrencyCode = currency;
			if (setupAmount)
			{
				costLine.WeightChargePP = pWCAmount * multiplier;
				costLine.ValuationChargePP = pVCAmount * multiplier;
				costLine.ChargesDueCarrierPP = pCCAmount * multiplier;
				costLine.ChargesDueAgentCC = cOAAmount * multiplier;
				costLine.Commission = cOMAmount * multiplier;
				costLine.Discount = dOIAmount * multiplier;
				costLine.VATDueAirline = vatDueAirline * multiplier;
				costLine.VATDueAgent = vatDueAgent * multiplier;
			}

			if (cassBillingLine != null)
			{
				cassBillingLine.AddCostLine(costLine, costLine.CurrencyCode);
			}

			return costLine;
		}
	}
}
