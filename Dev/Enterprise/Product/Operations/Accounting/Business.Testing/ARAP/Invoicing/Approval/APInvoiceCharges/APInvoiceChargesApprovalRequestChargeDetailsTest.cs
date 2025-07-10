using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoiceChargesApprovalRequestChargeDetails))]
	public class APInvoiceChargesApprovalRequestChargeDetailsTest : ApprovalRequestChargeDetailsTest<APInvoiceChargesApprovalRequestChargeDetails>
	{
		#region Decimal Places

		public void TestZDecimalsHaveCorrectDecimalPlaces()
		{
			var charge = (APInvoiceChargesApprovalRequestChargeDetails)GetNewBusinessObject();

			var localList = new List<string>
			{
				nameof(charge.LocalCostAmount),
			};

			var osList = new List<string>
			{
				nameof(charge.OSCostAmount)
			};

			var tester = new DecimalPlacesAttributeTester(charge);
			tester.CheckLocalCurrency(localList, nameof(charge.LocalDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(charge.OSDecimals), nameof(charge.CostCurrency), charge);
		}

		public void TestDecimalPlacesAttributeApplyToAllZDecimalProperties()
		{
			var properties = typeof(APInvoiceChargesApprovalRequestChargeDetails).GetProperties().Where(x => x.PropertyType == typeof(ZDecimal)).ToList();
			Assert(properties.All(x => Attribute.IsDefined(x, typeof(DecimalPlacesAttribute))));
		}

		#endregion

		public override void TestOpertorEqual()
		{
			base.TestOpertorEqual();

			var a = (APInvoiceChargesApprovalRequestChargeDetails)GetNewBusinessObject();
			var b = (APInvoiceChargesApprovalRequestChargeDetails)GetNewBusinessObject();

			a.CostCurrency = "1";
			AssertEquals(false, a == b);
			b.CostCurrency = "1";
			AssertEquals(true, a == b);

			a.OSCostAmount = 1M;
			AssertEquals(false, a == b);
			b.OSCostAmount = 1M;
			AssertEquals(true, a == b);

			a.LocalCostAmount = 1M;
			AssertEquals(false, a == b);
			b.LocalCostAmount = 1M;
			AssertEquals(true, a == b);
		}

		public override void TestCopyFrom()
		{
			base.TestCopyFrom();

			var charge = (APInvoiceChargesApprovalRequestChargeDetails)GetNewBusinessObject();

			charge.CostCurrency = "currency";
			charge.OSCostAmount = 10M;
			charge.LocalCostAmount = 20M;

			var newCharge = (APInvoiceChargesApprovalRequestChargeDetails)GetNewBusinessObject();

			newCharge.CopyFrom(charge);
			AssertEquals("CostCurrency", "currency", newCharge.CostCurrency);
			AssertEquals("OSCostAmount", 10M, newCharge.OSCostAmount);
			AssertEquals("LocalCostAmount", 20M, newCharge.LocalCostAmount);
		}

		protected override void SetInstanceSpecificBizOPropertiesForXMLTest(APInvoiceChargesApprovalRequestChargeDetails charge)
		{
			charge.CostCurrency = "currency";
			charge.OSCostAmount = 10M;
			charge.LocalCostAmount = 20M;
		}

		protected override string GetInstanceSpecificExpectedXML()
		{
			return "<CostCurrency>currency</CostCurrency><OSCostAmount>10</OSCostAmount><LocalCostAmount>20</LocalCostAmount>";
		}

		protected override void AssertInstanceSpecificBizOPropertiesForXMLTest(APInvoiceChargesApprovalRequestChargeDetails charge)
		{
			AssertEquals("CostCurrency", "currency", charge.CostCurrency);
			AssertEquals("OSCostAmount", 10M, charge.OSCostAmount);
			AssertEquals("LocalCostAmount", 20M, charge.LocalCostAmount);
		}
	}
}
