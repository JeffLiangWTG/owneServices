using System.Collections.Generic;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using AccGenericCharge = Enterprise.Accounting.Business.GenericCharge.GenericCharge;

namespace Enterprise.Accounting.Business.PayableOrder.Testing
{
	using System;
	using Enterprise.ZArchitecture.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(AccPayableOrderLine))]
	public class AccPayableOrderLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZDecimalsHaveCorrectDecimalPlacesAccPayableOrderLine()
		{
			var line = Factory.New<AccPayableOrderLine>();
			var header = Factory.New<AccPayableOrderHeader>();
			line.APL_APH = header.PK;

			var unitList = new List<string>
			{
				nameof(line.APL_InnerPacks),
				nameof(line.APL_OuterPacks)
			};

			var chargedList = new List<string>
			{
				nameof(line.APL_Quantity),
				nameof(line.APL_QtyReceived),
				nameof(line.APL_QtyInvoiced),
				nameof(line.APL_QuantityRemaining)
			};

			var rateList = new List<string>
			{
				nameof(line.APL_ItemPrice),
				nameof(line.APL_InvoicedPrice)
			};

			var linePriceList = new List<string>
			{
				nameof(line.APL_LinePrice),
			};

			var tester = new DecimalPlacesAttributeTester(line, line.Company);
			tester.CheckConstant(unitList, nameof(line.UnitDecimals), Constants.DecimalPlaces.DefaultNumberOfDecimalsForIndividualUnits);
			tester.CheckConstant(chargedList, nameof(line.QuantityChargedDecimals), 5);
			tester.CheckConstant(rateList, nameof(line.QuantityRateDecimals), 4);
			header.APH_Calc_Currency.Currency = "AUD";
			tester.CheckConstant(linePriceList, nameof(line.OrderCurrencyDecimals), 2);
			header.APH_Calc_Currency.Currency = "VND";
			tester.CheckConstant(linePriceList, nameof(line.OrderCurrencyDecimals), 0);
		}

		public void TestSetDefaultValue()
		{
			var line = Factory.New<AccPayableOrderLine>();
			AssertEquals(Enterprise.Core.Constants.PkgUnit.Unit, line.APL_F3_NKPackType);
			AssertEquals(Constants.OrderStatus.Open, line.APL_Status);
			AssertEquals(GlbBranch.CurrentBranch.PK, line.APL_GB);
			AssertEquals(GlbCompany.CurrentCompany.PK, line.APL_GC);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, line.APL_GE);
		}

		public void TestDepartmentCollection()
		{
			var line = Factory.New<AccPayableOrderLine>();
			AssertNotNull("Department Collection should be instantiated", line.DepartmentCollection);
		}

		[ExpectNoExceptions]
		public void TestAPL_Partno_List()
		{
			OrgSupplierPartCollection parts = BO.APL_PartNo_List;
			parts.Load();
		}

		public void TestAPL_F3_NKPackType_List()
		{
			var line = Factory.New<AccPayableOrderLine>();
			AssertEquals(new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits(), line.APL_F3_NKPackType_List);
		}

		public void TestAPL_Status_List()
		{
			var line = Factory.New<AccPayableOrderLine>();
			AssertEquals(AccountingConfigurationRegistry.Instance.APOrderLineStatusCodesList.Value, line.APL_Status_List);
		}

		public void TestAPL_Quantity()
		{
			var line = Factory.New<AccPayableOrderLine>();
			line.APL_ItemPrice = 2m;
			line.APL_Quantity = 0;
			AssertEquals(0m, line.APL_LinePrice);
			line.APL_Quantity = 5;
			AssertEquals(10m, line.APL_LinePrice);
		}

		public void TestAPL_ItemPrice()
		{
			var line = Factory.New<AccPayableOrderLine>();
			line.APL_Quantity = 5;
			line.APL_ItemPrice = 0m;
			AssertEquals(0m, line.APL_LinePrice);
			line.APL_ItemPrice = 2.2345m;
			AssertEquals(11.17m, line.APL_LinePrice);
		}

		public void TestAPL_LinePriceValidation()
		{
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.APH_Calc_Currency.Currency = "AUD";
			var line = order.OrderLines.AddNew();
			line.APL_Quantity = 1;
			line.APL_ItemPrice = 2.0356m;
			order.APH_Calc_Currency.Currency = "VND";
			line.Validation.ValidateAll();
			var expectMessage = "The Item Price and number of Items entered on this line result in a Line Price of 2.04\r\n\r\nLine Price must be in whole numbers to the decimals allowed by the Purchase Order Currency.\r\n\r\nPlease either adjust the Item Price, or clear the Item Price and enter the Line Price and allow the Item Price to calculate from the Line Price and number of Items.";
			AssertHasError("Should have error when Line Price value different from its rounded value", line.APL_LinePriceInfo, expectMessage);

			line.ReadOnly = true;
			line.Validation.ValidateAll();
			AssertNoError("Should have no error when Order Line is read only", line.APL_LinePriceInfo, expectMessage);

			line.APL_ItemPrice = 2;
			line.ReadOnly = false;
			line.Validation.ValidateAll();
			AssertNoError("Should have no error when Line Price value equal to its rounded value", line.APL_LinePriceInfo, expectMessage);

			order.APH_Calc_Currency.Currency = "AUD";
			line.APL_Quantity = 1;
			line.APL_ItemPrice = 2.0356m;
			order.APH_Calc_Currency.Currency = "VND";
			line.Validation.ValidateAll();
			AssertHasError("Should have error when Line Price value different from its rounded value", line.APL_LinePriceInfo, expectMessage);

			line.ReadOnly = true;
			line.APL_ItemPrice = 2;
			line.Validation.ValidateAll();
			AssertNoError("Should have no error when Order Line is read only and Line Price value equal to its rounded value", line.APL_LinePriceInfo, expectMessage);
		}

		public void TestAPL_QuantityRemaining()
		{
			var line = Factory.New<AccPayableOrderLine>();
			line.APL_Quantity = 10;
			line.APL_QtyInvoiced = 2;
			line.APL_QtyReceived = 5;
			AccountingConfigurationRegistry.Instance.APOrderLineQtyRemainingManagement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(5m, line.APL_QuantityRemaining);
			AccountingConfigurationRegistry.Instance.APOrderLineQtyRemainingManagement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(8m, line.APL_QuantityRemaining);
		}

		public void TestAPL_QuantityInvoiced()
		{
			var line = Factory.New<AccPayableOrderLine>();
			line.APL_Quantity = 10;
			AssertEquals(0m, line.APL_QtyReceived);
			AccountingConfigurationRegistry.Instance.APOrderLineQtyRemainingManagement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			line.APL_QtyInvoiced = 2m;
			AssertEquals(2m, line.APL_QtyReceived);
			AccountingConfigurationRegistry.Instance.APOrderLineQtyRemainingManagement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			line.APL_QtyInvoiced = 5m;
			AssertEquals(2m, line.APL_QtyReceived);
		}

		public void TestAPL_LinePrice()
		{
			var line = Factory.New<AccPayableOrderLine>();
			line.APL_ItemPrice = 5;
			line.APL_Quantity = 3;
			line.APL_LinePrice = 11.17;
			AssertEquals(3.7233m, line.APL_ItemPrice);
		}
		
		public void TestAPL_InvoicedPrice()
		{
			var line = Factory.New<AccPayableOrderLine>();
			line.APL_ItemPrice = 5;
			line.APL_QtyInvoiced = 10;
			AssertEquals(50m, line.APL_InvoicedPrice);
		}

		public void TestGenericChargeCollection()
		{
			var line = Factory.New<AccPayableOrderLine>();
			AssertNotNull("Charge Collection should be instantiated", line.ChargeList);
		}

		public void TestGenericChargeForGLAccount()
		{
			var line = Factory.New<AccPayableOrderLine>();
			line.GenericCharge = TestGLCharge.PK;
			Assert(line.APL_AC.IsEmpty);
			Assert(line.APL_AG.IsValid);
		}

		public void TestGenericChargeForChargeCode()
		{
			var line = Factory.New<AccPayableOrderLine>();
			line.GenericCharge = TestStandardCharge.PK;
			Assert(line.APL_AG.IsEmpty);
			Assert(line.APL_AC.IsValid);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Shouldn't be fired on this business object", true);
		}

		protected AccPayableOrderLine BO;
		protected override void SetUp()
		{
			base.SetUp();
			BO = (AccPayableOrderLine)GetNewBusinessObject();
		}

		protected AccGenericCharge TestGLCharge
		{
			get
			{
				if (fTestGLCharge == null)
				{
					fTestGLCharge = Factory.New<AccGenericCharge>();
					fTestGLCharge.VC_Code = "TSTGLC";
					fTestGLCharge.VC_IsGLAccount = true;
				}

				return fTestGLCharge;
			}
		}

		protected AccGenericCharge TestStandardCharge
		{
			get
			{
				if (fTestStandardCharge == null)
				{
					fTestStandardCharge = Factory.New<AccGenericCharge>();
					fTestStandardCharge.VC_Code = "TSTCCC";
					fTestStandardCharge.VC_IsGLAccount = false;
				}

				return fTestStandardCharge;
			}
		}

		AccGenericCharge fTestGLCharge;
		AccGenericCharge fTestStandardCharge;
	}
}
