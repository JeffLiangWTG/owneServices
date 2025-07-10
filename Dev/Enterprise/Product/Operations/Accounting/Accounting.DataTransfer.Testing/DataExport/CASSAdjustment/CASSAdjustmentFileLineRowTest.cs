using Enterprise.DataTransfer.Business.Testing;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	public class CASSAdjustmentFileLineRowTest : FlatFileDataRowTest
	{
		public void TestPublicFields()
		{
			var line = new CASSAdjustmentFileLineRow();
			line.Agent = "12345678910";
			line.AgentChargeCC = 205.03M;
			line.Airline = "081";
			line.AWBNumber = "00345678";
			line.CarrierChargePP = 206.25M;
			line.CCADCMNumber = "123456";
			line.Comment = "No Test";
			line.Commission = 256.30M;
			line.Incentive = 52.15M;
			line.Reason = 10;
			line.RecordType = "DO";
			line.ValuationChargePP = 652.25M;
			line.Weight = 15M;
			line.WeightChargePP = 147.2M;
			line.WeightUnit = "K";

			AssertEquals("Agent", "12345678910", line.GetField(CASSAdjustmentFileLineRow.Schema.Agent));
			AssertEquals("AgentChargeCC", "205.03", line.GetField(CASSAdjustmentFileLineRow.Schema.AgentChargeCC));
			AssertEquals("Airline", "081", line.GetField(CASSAdjustmentFileLineRow.Schema.Airline));
			AssertEquals("AWBNumber", "00345678", line.GetField(CASSAdjustmentFileLineRow.Schema.AWBNumber));
			AssertEquals("CarrierChargePP", "206.25", line.GetField(CASSAdjustmentFileLineRow.Schema.CarrierChargePP));
			AssertEquals("CCADCMNumber", "123456", line.GetField(CASSAdjustmentFileLineRow.Schema.CCADCMNumber));
			AssertEquals("Comment", "No Test", line.GetField(CASSAdjustmentFileLineRow.Schema.Comment));
			AssertEquals("Commission", "256.30", line.GetField(CASSAdjustmentFileLineRow.Schema.Commission));
			AssertEquals("Incentive", "52.15", line.GetField(CASSAdjustmentFileLineRow.Schema.Incentive));
			AssertEquals("Reason", "10", line.GetField(CASSAdjustmentFileLineRow.Schema.Reason));
			AssertEquals("RecordType", "DO", line.GetField(CASSAdjustmentFileLineRow.Schema.RecordType));
			AssertEquals("ValuationChargePP", "652.25", line.GetField(CASSAdjustmentFileLineRow.Schema.ValuationChargePP));
			AssertEquals("Weight", "15", line.GetField(CASSAdjustmentFileLineRow.Schema.Weight));
			AssertEquals("WeightUnit", "K", line.GetField(CASSAdjustmentFileLineRow.Schema.WeightUnit));
			AssertEquals("AgentChargePP", "0", line.GetField(CASSAdjustmentFileLineRow.Schema.AgentChargePP));
			AssertEquals("WeightChargeCC", "0", line.GetField(CASSAdjustmentFileLineRow.Schema.WeightChargeCC));
			AssertEquals("ValuationChargeCC", "0", line.GetField(CASSAdjustmentFileLineRow.Schema.ValuationChargeCC));
			AssertEquals("CarrierChargeCC", "0", line.GetField(CASSAdjustmentFileLineRow.Schema.CarrierChargeCC));
			AssertEquals("Contract", "", line.GetField(CASSAdjustmentFileLineRow.Schema.Contract));
			AssertEquals("Rate", "0", line.GetField(CASSAdjustmentFileLineRow.Schema.Rate));
		}
	}
}
