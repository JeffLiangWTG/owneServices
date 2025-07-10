using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ConsolRevenue
{
	public class ConsolRevenueValidationTest : BusinessObjectValidationTestCase
	{
		#region Sell Supply Type

		public void TestCheckSellSupplyType()
		{
			var consol = Factory.New<ForwardingConsol>() as IJobCostingPlugIn;
			Factory.Save();

			AssertCheckSellSupplyType(true);
			AssertCheckSellSupplyType(false);

			void AssertCheckSellSupplyType(bool enableSupplyTypeClassificationCodes)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableSupplyTypeClassificationCodes))
				{
					var master = new ConsolRevenueMaster(consol, Factory);
					var consolRevenue = new ConsolRevenue(master);

					var expectedError = "Enter a valid Sell Supply Type.";
					consolRevenue.SellSupplyType = "ERR";
					AssertEquals(enableSupplyTypeClassificationCodes, consolRevenue.SellSupplyTypeInfo.HasError(expectedError));

					consolRevenue.SellSupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationList[0].Code;
					AssertEquals(false, consolRevenue.SellSupplyTypeInfo.HasError(expectedError));

					var expectedWarning = "The Sell Supply Type is not specified. Please check if a supply type is needed before posting.";
					consolRevenue.SellSupplyType = string.Empty;
					AssertEquals(enableSupplyTypeClassificationCodes, consolRevenue.SellSupplyTypeInfo.HasWarning(expectedWarning));

					consolRevenue.SellSupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationList[0].Code;
					AssertEquals(false, consolRevenue.SellSupplyTypeInfo.HasWarning(expectedWarning));
				}
			}
		}

		#endregion

		public void TestCheckUnApportionedAmount()
		{
			IJobCostingPlugIn consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = ((ForwardingConsol)consol).Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S1";
			ForwardingShipment shipment2 = ((ForwardingConsol)consol).Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S2";

			var creator = new TestObjectCreator(Factory);
			var master = new ConsolRevenueMaster(consol, Factory);
			var rev = new ConsolRevenue(master);
			rev.ChargeCode = creator.CC1.PK;
			rev.SellAmount = 100m;
			AssertEquals(0m, rev.UnApportionedAmount);
			Assert(!rev.UnApportionedAmountInfo.HasError("Please ensure that this Sell Amount is fully apportioned."));

			rev.SplitCharges[0].JR_OSSellAmt = 0m;
			Assert(rev.UnApportionedAmountInfo.HasError("Please ensure that this Sell Amount is fully apportioned."));

			master.ReleaseMutexes();
		}

		public void TestValidateDescriptionWithEnableLocalChargeCodeDescriptionDefaultRegsitry()
		{
			var chargeCode = new TestObjectCreator(Factory).CreateChargeCode("ABC");
			chargeCode.AC_Desc = "My Test Charge Code";
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>() as IJobCostingPlugIn;
			var master = new ConsolRevenueMaster(consol, Factory);
			var consolRevenue = new ConsolRevenue(master);
			consolRevenue.ChargeCode = chargeCode.PK;
			consolRevenue.Description = chargeCode.AC_Desc;

			var expectedWarningMessage = "Charge description was changed from default. This description will appear on AR Invoice without translation.";

			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert(consolRevenue.Description.StartsWith(chargeCode.AC_Desc));
				AssertNoWarning(consolRevenue.DescriptionInfo, expectedWarningMessage);

				consolRevenue.Description = "My Test Charge Code and some appended text";
				Assert(consolRevenue.Description.StartsWith(chargeCode.AC_Desc));
				AssertNoWarning(consolRevenue.DescriptionInfo, expectedWarningMessage);

				consolRevenue.Description = "This is a very different charge code description";
				Assert(!consolRevenue.Description.StartsWith(chargeCode.AC_Desc));
				AssertHasWarning(consolRevenue.DescriptionInfo, expectedWarningMessage);
			}

			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consolRevenue.Description = "My Test Charge Code";
				Assert(consolRevenue.Description.StartsWith(chargeCode.AC_Desc));
				AssertNoWarning(consolRevenue.DescriptionInfo, expectedWarningMessage);

				consolRevenue.Description = "My Test Charge Code and some appended text";
				Assert(consolRevenue.Description.StartsWith(chargeCode.AC_Desc));
				AssertNoWarning(consolRevenue.DescriptionInfo, expectedWarningMessage);

				consolRevenue.Description = "This is a very different charge code description";
				Assert(!consolRevenue.Description.StartsWith(chargeCode.AC_Desc));
				AssertNoWarning(consolRevenue.DescriptionInfo, expectedWarningMessage);
			}
		}

		public void TestValidateMaster()
		{
			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			BusinessObjectFactory fact = new BusinessObjectFactory();
			IJobCostingPlugIn consol = fact.New<ForwardingConsol>();
			ConsolRevenueMaster master = new ConsolRevenueMaster(consol, fact);
			ConsolRevenue rev = new ConsolRevenue(master);
			rev.ApportionmentMethod = "";
			rev.Currency = ZString.Empty;
			rev.SellAmount = 0;
			rev.CostGovtChargeCode = "";
			rev.SellGovtChargeCode = "";
			rev.Validation.ValidateAll();
			AssertHasErrors(rev.ChargeCodeInfo);
			AssertHasErrors(rev.ApportionmentMethodInfo);
			AssertHasErrors(rev.CurrencyInfo);
			AssertHasErrors(rev.SellAmountInfo);
			AssertHasErrors(rev.CostGovtChargeCodeInfo);
			AssertHasErrors(rev.SellGovtChargeCodeInfo);
			rev.ChargeCode = Factory.Load<AccChargeCode>(new ZQuery())[0].PK;
			rev.ApportionmentMethod = Enterprise.ZArchitecture.Core.AllocationMethod.Shipment;
			rev.Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			rev.SellAmount = 1;
			rev.CostGovtChargeCode = "AAA";
			rev.SellGovtChargeCode = "BBB";
			AssertNoErrors(rev.ChargeCodeInfo);
			AssertNoErrors(rev.ApportionmentMethodInfo);
			AssertNoErrors(rev.CurrencyInfo);
			AssertNoErrors(rev.SellAmountInfo);
			AssertNoErrors(rev.CostGovtChargeCodeInfo);
			AssertNoErrors(rev.SellGovtChargeCodeInfo);
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Overhead;
			rev.ChargeCode = chargeCode.PK;
			AssertHasErrors(rev.ChargeCodeInfo);
		}
	}
}
