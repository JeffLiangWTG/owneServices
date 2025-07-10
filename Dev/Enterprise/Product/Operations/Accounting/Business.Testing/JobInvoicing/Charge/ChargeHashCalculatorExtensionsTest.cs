using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class ChargeHashCalculatorExtensionsTest : TestCaseWithFactory
	{
		public void TestCalculateCostPartHash()
		{
			AssertEquals("Expected hash", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));

			Charge.JR_OH_CostAccount = Creator.Creditor1.PK;
			AssertNotEquals("Hash should not match", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));

			Charge.JR_OH_CostAccount = ZGuid.Empty;
			AssertEquals("Hash should match again", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));

			Charge.JR_RX_NKCostCurrency = Creator.USD.Code;
			AssertNotEquals("Hash should not match", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));

			Charge.JR_RX_NKCostCurrency = Creator.AUD.Code;
			AssertEquals("Hash should match again", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));

			Charge.JR_OSCostAmt = 101m;
			AssertNotEquals("Hash should not match", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));

			Charge.JR_OSCostAmt = 100.00m;
			AssertEquals("Hash should match again", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));

			Charge.JR_LocalCostAmt = 101m;
			AssertNotEquals("Hash should not match", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));

			Charge.JR_LocalCostAmt = 100.00m;
			AssertEquals("Hash should match again", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));

			Charge.JR_AT_CostGSTRate = Creator.CAP.PK;
			AssertNotEquals("Hash should not match", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));

			Charge.JR_AT_CostGSTRate = ZGuid.Empty;
			AssertEquals("Hash should match again", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));

			Charge.JR_A9_CostVATClass = Creator.TaxMsg1.PK;
			AssertNotEquals("Hash should not match", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));

			Charge.JR_A9_CostVATClass = ZGuid.Empty;
			AssertEquals("Hash should match again", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));

			Charge.JR_OSSellAmt = 111m;
			AssertEquals("Should have no effect on Cost hash", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));
		}

		public void TestCalculateCostPartHashWithUnsupportedVersion()
		{
			AssertExceptionThrown("Invalid hash version 255 for Job Charge Cost part.", typeof(ArgumentException), () => Charge.CalculateCostPartHash(255));
		}

		public void TestCalculateSellPartHash()
		{
			AssertEquals("Expected hash", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_OH_SellAccount = Creator.LocalClient2.PK;
			AssertNotEquals("Hash should not match", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_OH_SellAccount = ZGuid.Empty;
			AssertEquals("Hash should match again", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_RX_NKSellCurrency = Creator.USD.Code;
			AssertNotEquals("Hash should not match", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_RX_NKSellCurrency = Creator.AUD.Code;
			AssertEquals("Hash should match again", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_OSSellAmt = 111m;
			AssertNotEquals("Hash should not match", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_OSSellAmt = 110.00m;
			AssertEquals("Hash should match again", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_LocalSellAmt = 111m;
			AssertNotEquals("Hash should not match", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_LocalSellAmt = 110.00m;
			AssertEquals("Hash should match again", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_AT_SellGSTRate = Creator.CAP.PK;
			AssertNotEquals("Hash should not match", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_AT_SellGSTRate = ZGuid.Empty;
			AssertEquals("Hash should match again", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_A9_SellVATClass = Creator.TaxMsg1.PK;
			AssertNotEquals("Hash should not match", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_A9_SellVATClass = ZGuid.Empty;
			AssertEquals("Hash should match again", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_RX_NKSellInvoiceCurrency = Creator.USD.Code;
			AssertNotEquals("Hash should not match", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_RX_NKSellInvoiceCurrency = ZString.Empty;
			AssertEquals("Hash should match again", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_OSCostAmt = 101;
			AssertEquals("Should have no effect on Sell hash", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));
		}

		public void TestCalculateSellPartHashWithUnsupportedVersion()
		{
			AssertExceptionThrown("Invalid hash version 255 for Job Charge Sell part.", typeof(ArgumentException), () => Charge.CalculateSellPartHash(255));
		}

		public void TestCommonProperties()
		{
			AssertEquals("Expected Cost hash", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));
			AssertEquals("Expected hash", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			var jobPK = Charge.JR_JH;
			Charge.JR_JH = ZGuid.NewZGuid();
			AssertNotEquals("Cost hash should not match", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));
			AssertNotEquals("Sell hash should not match", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_JH = jobPK;
			AssertEquals("Cost hash should match again", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));
			AssertEquals("Sell hash should match again", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_GB = Creator.NonCurrentBranch.PK;
			AssertNotEquals("Cost hash should not match", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));
			AssertNotEquals("Sell hash should not match", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals("Cost hash should match again", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));
			AssertEquals("Sell hash should match again", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_GE = Creator.NonCurrentDepartment.PK;
			AssertNotEquals("Cost hash should not match", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));
			AssertNotEquals("Sell hash should not match", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			AssertEquals("Cost hash should match again", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));
			AssertEquals("Sell hash should match again", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_AC = ZGuid.Empty;
			AssertNotEquals("Cost hash should not match", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));
			AssertNotEquals("Sell hash should not match", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_AC = Creator.FRT.PK;
			Charge.JR_OSCostAmt = 100m;
			Charge.JR_OSSellAmt = 110m;
			AssertEquals("Cost hash should match again", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));
			AssertEquals("Sell hash should match again", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));

			Charge.JR_Desc = "New Description";
			AssertEquals("Cost hash should match", ExpectedCostHash, Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion));
			AssertEquals("Sell hash should match", ExpectedSellHash, Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion));
		}

		public void TestCalculatePartHashesWithDifferentDecimalPlaces()
		{
			Charge.JR_OSCostAmt = 101m;
			Charge.JR_OSSellAmt = 102m;

			// Now JR_OSCostAmt as hash source is 101.
			var hashCost1 = Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion);
			// Now JR_OSSellAmt as hash source is 102.
			var hashSell1 = Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion);

			Factory.Save();
			Factory.Save();

			// Now JR_OSCostAmt as hash source is 101.0000.
			var hashCost2 = Charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion);
			// Now JR_OSSellAmt as hash source is 101.0000.
			var hashSell2 = Charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion);

			// The hash result with the same charge will be different.
			AssertEquals("Cost hash should match with different decimal places", hashCost1, hashCost2);
			AssertEquals("Sell hash should match with different decimal places", hashSell1, hashSell2);
		}

		#region Implementation

		Charge Charge;
		TestObjectCreator Creator;
		byte[] ExpectedCostHash, ExpectedSellHash;

		protected override void SetUp()
		{
			base.SetUp();
			Creator = new TestObjectCreator(Factory);

			ExpectedCostHash = new byte[] { 192, 83, 13, 43, 96, 223, 244, 22, 70, 93, 56, 139, 43, 163, 152, 7, 29, 44, 151, 95, 56, 91, 147, 7, 108, 187, 206, 191, 135, 151, 26, 12 };
			ExpectedSellHash = new byte[] { 56, 11, 14, 63, 23, 9, 214, 226, 101, 248, 104, 84, 17, 55, 174, 15, 25, 19, 180, 192, 97, 17, 31, 191, 24, 165, 144, 25, 62, 246, 48, 108 };

			var job = Factory.NewJobWithPrimaryKeyForTesting<Job>(Guid.Parse("10355f23-20ba-4848-bae9-06b8ee755ee8"));
			job.FillWithValidTestData();
			Charge = Creator.CreateCharge(job, Creator.FRT, 100m, 110m);
		}

		#endregion
	}
}
