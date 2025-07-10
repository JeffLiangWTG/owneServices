using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(SCWDECLineProvider))]
	sealed class SCWDECLineProviderTest : ImportDecLineProviderAbstractTest<SCWDECLineProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new SCWDECLineProvider(null));
		}

		public void TestArticleNumber()
		{
			invoiceLine.JI_PartNo = "PartNo1";
			AssertEquals("PartNo1", Provider.ArticleNumber);
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

		public void TestCustomsValue()
		{
			declaration.ZG_IsHighValueOvrd = false;

			CombineAssertions(() =>
			{
				AssertNull("Null", Provider.CustomsValue);

				var customsValue = Provider.CustomsValue;
				AssertEquals("Cached", customsValue, Provider.CustomsValue);
			});
		}

		public void TestCustomsValue_ConcessionInE01OrE02()
		{
			declaration.ZG_IsHighValueOvrd = true;
			invoiceLine.JI_Procedure = "7005E01";

			CombineAssertions(() =>
			{
				AssertNull("Null", Provider.CustomsValue);

				var customsValue = Provider.CustomsValue;
				AssertEquals("Cached", customsValue, Provider.CustomsValue);
			});
		}

		public void TestCustomsValue_ConcessionNotInE01OrE02()
		{
			declaration.ZG_IsHighValueOvrd = true;
			invoiceLine.JI_Procedure = "7005F01";

			CombineAssertions(() =>
			{
				var customsValue = Provider.CustomsValue;
				AssertNotNull("Not Null", customsValue);

				AssertEquals("Cached", customsValue, Provider.CustomsValue);
			});
		}

		public void TestRequestedPreferentialTreatment()
		{
			invoiceLine.JI_PrimaryPreference = "Pr1";
			AssertEquals("Pr1", Provider.RequestedPreferentialTreatment);
		}

		protected override SCWDECLineProvider GetProvider() => new SCWDECLineProvider(entryLine);

		new ISCWDECLine Provider => base.Provider;
	}
}
