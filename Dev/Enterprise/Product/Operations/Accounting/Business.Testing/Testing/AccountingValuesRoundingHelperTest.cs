using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing
{
	class AccountingValuesRoundingHelperTest : TestCaseWithFactory
	{
		public void TestPropertyHasChanges()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol("KRSEL", "AUSYD", "C0001");

			Assert("business object is not changing currency", AccountingValuesRoundingHelper.PropertyHasChanges(consol, true));
			Assert("business object is not changing currency", !AccountingValuesRoundingHelper.PropertyHasChanges(consol, false));

			consol.SetContext(BusinessContext.ChangingCurrencyToDifferentDecimalPlaces);
			Assert("business object is changing currency", AccountingValuesRoundingHelper.PropertyHasChanges(consol, true));
			Assert("business object is changing currency", AccountingValuesRoundingHelper.PropertyHasChanges(consol, false));
		}

		public void TestReportErrorIfPropertiesNotRounded()
		{
			var dummy = new MyDummyBizo();
			AssertEquals("Precondition", 0, dummy.y.DecimalPlaces);
			AssertEquals("Precondition", 1, dummy.z.DecimalPlaces);
			var currency = "VND";
			AssertEquals(0, RefCurrency.LoadFromCurrencyCode(Factory, currency).Decimals);

			AssertNoExceptionThrown("all decimals are rounded correctly (y)", () => AccountingValuesRoundingHelper.ReportErrorIfPropertiesNotRounded(dummy, new[] { "y", "x" }, "QQQ", currency, null));
			AccountingValuesRoundingHelper.ReportErrorIfPropertiesNotRounded(dummy, new[] { "y", "z" }, "QQQ", currency, null);
			AssertErrorMessageForRounding("QQQ");
		}

		public void TestReportErrorWhenSuspenderSuspendedOrNot()
		{
			var dummy = new MyDummyBizo();
			AssertEquals("Precondition", 0, dummy.y.DecimalPlaces);
			AssertEquals("Precondition", 1, dummy.z.DecimalPlaces);
			var currency = "VND";
			AssertEquals(0, RefCurrency.LoadFromCurrencyCode(Factory, currency).Decimals);

			var suspender = new FunctionalitySuspender();

			using (AccountingValuesRoundingHelper.GetActionForChangeInDecimalPlaces(dummy, new[] { "y", "z" }, "QQQ", currency, suspender))
			{
				Assert(!suspender.IsSuspended);
			}
			AssertErrorMessageForRounding("QQQ");

			using (suspender.GetSuspender())
			using (AccountingValuesRoundingHelper.GetActionForChangeInDecimalPlaces(dummy, new[] { "y", "z" }, "QQQ", currency, suspender))
			{
				Assert(suspender.IsSuspended);
			}
			AssertEquals("No error message reported.", string.Empty, ErrorReporter.LastMessageReported);
		}

		void AssertErrorMessageForRounding(string oldCurrency)
		{
			AssertMultilineASCIIEquals("z not rounded right",
@"In Enterprise.Accounting.Business.Testing.MyDummyBizo -
Properties requiring to be rounded on currency change have more decimal places than allowed, this may be due to setter not propagating values due to has changes check.
Incorrectly rounded values are:
z: 3.5
Currency " + oldCurrency + @" was changed to VND
Only 0 decimals are allowed."
, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestGetActionForChangeInDecimalPlaces()
		{
			var bizo = new MyDummyBizo();
			var testObjectCreator = new TestObjectCreator(Factory);
			var invalidCurrency = "QQQ";
			var validCurrency = "AUD";
			var validCurrency1 = "USD";
			var validCurrency2 = "VND";

			AssertNull(RefCurrency.LoadFromCurrencyCode(Factory, invalidCurrency));
			AssertEquals(2, RefCurrency.LoadFromCurrencyCode(Factory, validCurrency).Decimals);
			AssertEquals(2, RefCurrency.LoadFromCurrencyCode(Factory, validCurrency1).Decimals);
			AssertEquals(0, RefCurrency.LoadFromCurrencyCode(Factory, validCurrency2).Decimals);

			checkActionGenerated("change to invalid currency", AccountingValuesRoundingHelper.GetActionForChangeInDecimalPlaces(bizo, new[] { "z" }, invalidCurrency, invalidCurrency, null), false);
			checkActionGenerated("change to currency with same dec pts", AccountingValuesRoundingHelper.GetActionForChangeInDecimalPlaces(bizo, new[] { "z" }, validCurrency, validCurrency1, null), false);
			checkActionGenerated("change to valid currency", AccountingValuesRoundingHelper.GetActionForChangeInDecimalPlaces(bizo, new[] { "z" }, invalidCurrency, validCurrency2, null), true);
			AssertErrorMessageForRounding("QQQ");

			checkActionGenerated("change to invalid currency", AccountingValuesRoundingHelper.GetActionForChangeInDecimalPlaces(bizo, new[] { "z" }, validCurrency1, invalidCurrency, null), false);
			checkActionGenerated("change to currency with diff dec pts", AccountingValuesRoundingHelper.GetActionForChangeInDecimalPlaces(bizo, new[] { "z" }, validCurrency1, validCurrency2, null), true);
			AssertErrorMessageForRounding("USD");

			void checkActionGenerated(string message, DisposableAction action, bool expectContext)
			{
				using (action)
				{
					AssertEquals(message, expectContext, bizo.HasContext(BusinessContext.ChangingCurrencyToDifferentDecimalPlaces));
				}

				Assert(message, !bizo.HasContext(BusinessContext.ChangingCurrencyToDifferentDecimalPlaces));
			}
		}
	}

	class MyDummyBizo : NonPersistentBusinessObject
	{
		public MyDummyBizo()
			: base(new BusinessObjectFactory())
		{
			a = 4.5m;
			x = "Venkata";
			y = 2m;
			z = new ZDecimal(3.5);
		}

		public ZDecimal a { get; set; }
		public string x { get; set; }
		public ZDecimal y { get; set; }
		public ZDecimal z { get; set; }
	}
}
