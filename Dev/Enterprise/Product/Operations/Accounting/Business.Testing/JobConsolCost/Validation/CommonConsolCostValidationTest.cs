using System;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public abstract class CommonConsolCostValidationTest : JobConsolCostValidationTest
	{
		public void TestCheckE6_OSCostAmountWhenNegative()
		{
			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			JobConsolCost cost = ObjectCreator.CreateConsolCost(ObjectCreator.CreateConsol("AUSYD", "USLAX", "C001"), ObjectCreator.CC1, ObjectCreator.Creditor1);
			cost.E6_InvoiceNum = "111";
			cost.E6_InvoiceDate = ZDateTime.Now;
			cost.E6_PaymentDate = ZDateTime.Now;
			cost.E6_OSCostAmount = -10;
			AssertNoErrorContaining(cost.E6_OSCostAmountInfo, "To enter a negative amount, you must enter the creditor, invoice number, invoice date and invoice due date.");
			cost.E6_InvoiceNum = "111";
			cost.E6_InvoiceDate = ZDateTime.Now;
			cost.E6_PaymentDate = ZDateTime.Empty;
			cost.E6_OSCostAmount = -15;
			AssertHasErrorContaining(cost.E6_OSCostAmountInfo, "To enter a negative amount, you must enter the creditor, invoice number, invoice date and invoice due date.");
			cost.E6_InvoiceNum = "111";
			cost.E6_InvoiceDate = ZDateTime.Empty;
			cost.E6_PaymentDate = ZDateTime.Now;
			cost.E6_OSCostAmount = -10;
			AssertHasErrorContaining(cost.E6_OSCostAmountInfo, "To enter a negative amount, you must enter the creditor, invoice number, invoice date and invoice due date.");
			cost.E6_InvoiceNum = "";
			cost.E6_InvoiceDate = ZDateTime.Now;
			cost.E6_PaymentDate = ZDateTime.Now;
			cost.E6_OSCostAmount = -15;
			AssertHasErrorContaining(cost.E6_OSCostAmountInfo, "To enter a negative amount, you must enter the creditor, invoice number, invoice date and invoice due date.");
			cost.E6_OH_Creditor = ZGuid.Empty;
			cost.E6_InvoiceNum = "111";
			cost.E6_InvoiceDate = ZDateTime.Now;
			cost.E6_PaymentDate = ZDateTime.Now;
			cost.E6_OSCostAmount = -10;
			AssertHasErrorContaining(cost.E6_OSCostAmountInfo, "To enter a negative amount, you must enter the creditor, invoice number, invoice date and invoice due date.");
			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			cost.E6_OSCostAmount = -15;
			AssertNoErrorContaining(cost.E6_OSCostAmountInfo, "To enter a negative amount, you must enter the creditor, invoice number, invoice date and invoice due date.");
		}
	}
}
