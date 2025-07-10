using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(SCWRECLineProvider))]
	sealed class SCWRECLineProviderTest : ImportDecLineProviderAbstractTest<SCWRECLineProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new SCWRECLineProvider(null));
		}

		public void TestInwardMovementAmount()
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;

			invoiceLine.JI_BondedWhsQuantity = 10.5;
			invoiceLine.JI_BondedWhsUnitQty = "KGMA";
			invoiceLine2.JI_BondedWhsQuantity = 5.1;
			invoiceLine2.JI_BondedWhsUnitQty = "KGMA";
			var amount = Provider.InwardMovementAmount;

			CombineAssertions(() =>
			{
				AssertEquals("Quantity", 15.6m, amount.Quantity);
				AssertEquals("MeasurementUnit", "KGM", amount.MeasurementUnit);
				AssertEquals("Qualifier", "A", amount.Qualifier);
			});
		}

		public void TestRequestedPreferentialTreatment()
		{
			CombineAssertions(() =>
			{
				AssertNull("Empty", Provider.RequestedPreferentialTreatment);
				invoiceLine.JI_PrimaryPreference = "300";
				AssertEquals("Not empty", "300", Provider.RequestedPreferentialTreatment);
			});
		}

		public void TestAssessmentCustomsValue()
		{
			declaration.ZG_IsHighValueOvrd = false;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 502.58;
			AssertEquals(502.58m, Provider.AssessmentCustomsValue);
		}

		public void TestAssessmentCustomsValue_DeclarationDV1()
		{
			declaration.ZG_IsHighValueOvrd = true;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 502.58;
			AssertEquals(502.58m, Provider.AssessmentCustomsValue);
		}

		protected override SCWRECLineProvider GetProvider() => new SCWRECLineProvider(entryLine);

		new ISCWRECLine Provider => base.Provider;
	}
}
