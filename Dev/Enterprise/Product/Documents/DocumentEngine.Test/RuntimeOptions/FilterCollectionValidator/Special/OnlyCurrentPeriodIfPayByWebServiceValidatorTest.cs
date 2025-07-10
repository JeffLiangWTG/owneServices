using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class OnlyCurrentPeriodIfPayByWebServiceValidatorTest : TestCaseWithFactory
	{
		public void TestSingleAccountingPeriodFieldValidationWithoutWebService()
		{
			PeriodTestHelper.SetupPeriods();
			Validator.AddFilterField(SinglePeriodFilter);

			Assert("Originally should be not set", SinglePeriodFilter.IsEmpty);
			AssertEquals(true, Validator.IsValid(SinglePeriodFilter));
			AssertNoErrors(SinglePeriodFilter);

			SinglePeriodFilter.SinglePeriod = 99;
			Assert("Should be set", !SinglePeriodFilter.IsEmpty);
			AssertHasErrors("Invalid period", SinglePeriodFilter.SinglePeriodInfo);
			Assert("???", SinglePeriodFilter.IsValid);
			AssertEquals(true, Validator.IsValid(SinglePeriodFilter));

			SinglePeriodFilter.SinglePeriod = PeriodTestHelper.PreviousOpenPeriodInt;
			AssertEquals(true, Validator.IsValid(SinglePeriodFilter));
			AssertNoErrors(SinglePeriodFilter);

			SinglePeriodFilter.SinglePeriod = PeriodTestHelper.FuturePeriodInt;
			AssertEquals(true, Validator.IsValid(SinglePeriodFilter));
			AssertNoErrors(SinglePeriodFilter);
		}

		public void TestSingleAccountingPeriodFieldValidationWithEnabledWebService()
		{
			PeriodTestHelper.SetupPeriods();
			Validator.AddFilterField(SinglePeriodFilter);
			AssertEquals("Error Message", "With the Invoice Payment Web Service enabled it must be only the current period", Validator.GetErrorMessage(SinglePeriodFilter));

			var mockSupporter = new Mock<IAccounting>();
			mockSupporter.Setup(m => m.IsInvoicePaymentWebServiceEnabled(It.IsAny<Guid>())).Returns(true);
			using (ObjectFactory.Substitute<IAccounting>(mockSupporter.Object))
			{
				Assert("Originally should be not set", SinglePeriodFilter.IsEmpty);
				AssertEquals(true, Validator.IsValid(SinglePeriodFilter));
				AssertNoErrors(SinglePeriodFilter);

				SinglePeriodFilter.SinglePeriod = 99;
				Assert("Should be set", !SinglePeriodFilter.IsEmpty);
				AssertHasErrors("Invalid period", SinglePeriodFilter.SinglePeriodInfo);
				Assert("???", SinglePeriodFilter.IsValid);
				AssertEquals(true, Validator.IsValid(SinglePeriodFilter));

				SinglePeriodFilter.SinglePeriod = PeriodTestHelper.PreviousOpenPeriodInt;
				AssertEquals(false, Validator.IsValid(SinglePeriodFilter));
				AssertNoErrors(SinglePeriodFilter);

				SinglePeriodFilter.SinglePeriod = PeriodTestHelper.CurrentPeriodInt;
				AssertEquals(true, Validator.IsValid(SinglePeriodFilter));
				AssertNoErrors(SinglePeriodFilter);
			}
		}

		public void TestMultipleChoiceValidationWithoutWebService()
		{
			Validator.AddFilterField(MultipleChoiceFilter);
			MultipleChoiceFilter.DisplayName = "Ageing";

			Assert("Originally should be not set", MultipleChoiceFilter.IsEmpty);
			AssertEquals(true, Validator.IsValid(MultipleChoiceFilter));
			AssertNoErrors(MultipleChoiceFilter);
			AssertNoWarnings(MultipleChoiceFilter);

			MultipleChoiceFilter.ZValue = "XYZ";
			Assert("Should be set", !MultipleChoiceFilter.IsEmpty);
			AssertNoErrors(MultipleChoiceFilter);
			AssertHasWarnings("Invalid choice", MultipleChoiceFilter.ZValueInfo);
			AssertEquals(true, Validator.IsValid(MultipleChoiceFilter));

			MultipleChoiceFilter.ZValue = "ABC";
			AssertEquals(true, Validator.IsValid(MultipleChoiceFilter));
			AssertNoErrors(MultipleChoiceFilter);
			AssertNoWarnings(MultipleChoiceFilter);

			MultipleChoiceFilter.ZValue = "NON";
			AssertEquals(true, Validator.IsValid(MultipleChoiceFilter));
			AssertNoErrors(MultipleChoiceFilter);
			AssertNoWarnings(MultipleChoiceFilter);
		}

		public void TestMultipleChoiceValidationWithEnabledWebService()
		{
			Validator.AddFilterField(MultipleChoiceFilter);
			AssertEquals("Error Message", "With the Invoice Payment Web Service enabled it must be No Aging", Validator.GetErrorMessage(MultipleChoiceFilter));

			var mockSupporter = new Mock<IAccounting>();
			mockSupporter.Setup(m => m.IsInvoicePaymentWebServiceEnabled(It.IsAny<Guid>())).Returns(true);
			using (ObjectFactory.Substitute<IAccounting>(mockSupporter.Object))
			{
				Assert("Originally should be not set", MultipleChoiceFilter.IsEmpty);
				AssertEquals(true, Validator.IsValid(MultipleChoiceFilter));
				AssertNoErrors(MultipleChoiceFilter);
				AssertNoWarnings(MultipleChoiceFilter);

				MultipleChoiceFilter.ZValue = "XYZ";
				Assert("Should be set", !MultipleChoiceFilter.IsEmpty);
				AssertHasWarnings("Invalid choice", MultipleChoiceFilter.ZValueInfo);
				AssertEquals(false, Validator.IsValid(MultipleChoiceFilter));

				MultipleChoiceFilter.ZValue = "ABC";
				AssertEquals(false, Validator.IsValid(MultipleChoiceFilter));
				AssertNoErrors(MultipleChoiceFilter);
				AssertNoWarnings(MultipleChoiceFilter);

				MultipleChoiceFilter.ZValue = "NON";
				AssertEquals(true, Validator.IsValid(MultipleChoiceFilter));
				AssertNoErrors(MultipleChoiceFilter);
				AssertNoWarnings(MultipleChoiceFilter);
			}
		}

		public void TestDateRangeFieldValidationWithoutWebService()
		{
			Validator.AddFilterField(DateRangeFilter);

			Assert("Originally should be not set", DateRangeFilter.ValueHigh.IsEmpty);
			AssertEquals(true, Validator.IsValid(DateRangeFilter));
			AssertNoErrors(DateRangeFilter);

			DateRangeFilter.ValueHigh = ZDateTime.Invalid;
			AssertNoErrors("???", DateRangeFilter.ValueHighInfo);
			Assert("???", DateRangeFilter.IsValid);
			AssertEquals(true, Validator.IsValid(DateRangeFilter));

			DateRangeFilter.ValueHigh = ZDateTime.Today.AddDays(-1);
			AssertEquals(true, Validator.IsValid(DateRangeFilter));
			AssertNoErrors(DateRangeFilter);

			DateRangeFilter.ValueHigh = ZDateTime.Today;
			AssertEquals(true, Validator.IsValid(DateRangeFilter));
			AssertNoErrors(DateRangeFilter);
		}

		public void TestDateRangeFieldValidationWithEnabledWebService()
		{
			Validator.AddFilterField(DateRangeFilter);
			AssertEquals("Error Message", "With the Invoice Payment Web Service enabled Date To must be Today or a day in the future", Validator.GetErrorMessage(DateRangeFilter));

			var mockSupporter = new Mock<IAccounting>();
			mockSupporter.Setup(m => m.IsInvoicePaymentWebServiceEnabled(It.IsAny<Guid>())).Returns(true);
			using (ObjectFactory.Substitute<IAccounting>(mockSupporter.Object))
			{
				Assert("Originally should be not set", DateRangeFilter.ValueHigh.IsEmpty);
				AssertEquals(true, Validator.IsValid(DateRangeFilter));
				AssertNoErrors(DateRangeFilter);

				DateRangeFilter.ValueHigh = ZDateTime.Invalid;
				AssertNoErrors("???", DateRangeFilter.ValueHighInfo);
				Assert("???", DateRangeFilter.IsValid);
				AssertEquals(true, Validator.IsValid(DateRangeFilter));

				DateRangeFilter.ValueHigh = ZDateTime.Today.AddDays(-1);
				AssertEquals(false, Validator.IsValid(DateRangeFilter));
				AssertNoErrors(DateRangeFilter);

				DateRangeFilter.ValueHigh = ZDateTime.Today;
				AssertEquals(true, Validator.IsValid(DateRangeFilter));
				AssertNoErrors(DateRangeFilter);
			}
		}

		public void TestDateFieldValidationWithoutWebService()
		{
			Validator.AddFilterField(DateFilter);

			Assert("Originally should be not set", DateFilter.Value.IsEmpty);
			AssertEquals(true, Validator.IsValid(DateFilter));
			AssertNoErrors(DateFilter);

			DateFilter.Value = ZDateTime.Invalid;
			Assert("Should be set", !DateFilter.IsEmpty);
			AssertHasErrors("Invalid DateTime", DateFilter.ValueInfo);
			Assert("???", DateRangeFilter.IsValid);
			AssertEquals(true, Validator.IsValid(DateFilter));

			DateFilter.Value = ZDateTime.Today.AddDays(-1);
			AssertEquals(true, Validator.IsValid(DateFilter));
			AssertNoErrors(DateFilter);

			DateFilter.Value = ZDateTime.Today;
			AssertEquals(true, Validator.IsValid(DateFilter));
			AssertNoErrors(DateFilter);
		}

		public void TestDateFieldValidationWithEnabledWebService()
		{
			Validator.AddFilterField(DateFilter);
			AssertEquals("Error Message", "With the Invoice Payment Web Service enabled Date must be Today or a day in the future", Validator.GetErrorMessage(DateFilter));

			var mockSupporter = new Mock<IAccounting>();
			mockSupporter.Setup(m => m.IsInvoicePaymentWebServiceEnabled(It.IsAny<Guid>())).Returns(true);
			using (ObjectFactory.Substitute<IAccounting>(mockSupporter.Object))
			{
				Assert("Originally should be not set", DateFilter.Value.IsEmpty);
				AssertEquals(true, Validator.IsValid(DateFilter));
				AssertNoErrors(DateFilter);

				DateFilter.Value = ZDateTime.Invalid;
				Assert("Should be set", !DateFilter.IsEmpty);
				AssertHasErrors("Invalid DateTime", DateFilter.ValueInfo);
				Assert("???", DateFilter.IsValid);
				AssertEquals(true, Validator.IsValid(DateFilter));

				DateFilter.Value = ZDateTime.Today.AddDays(-1);
				AssertEquals(false, Validator.IsValid(DateFilter));
				AssertNoErrors(DateFilter);

				DateFilter.Value = ZDateTime.Today;
				AssertEquals(true, Validator.IsValid(DateFilter));
				AssertNoErrors(DateFilter);
			}
		}

		public void TestPaymentWebServiceDisabledByDefault()
		{
			AssertEquals("Should be disabled by default", false, Validator.IsPaymentWebServiceEnabled);
		}

		public void TestIsPaymentWebServiceEnabled()
		{
			var mockSupporter = new Mock<IAccounting>();
			mockSupporter.Setup(m => m.IsInvoicePaymentWebServiceEnabled(It.IsAny<Guid>())).Returns(true);
			using (ObjectFactory.Substitute<IAccounting>(mockSupporter.Object))
			{
				AssertEquals("Now it should be enabled", true, Validator.IsPaymentWebServiceEnabled);
			}
		}

		public void TestPeriodCalculator()
		{
			AssertNotNull(Validator.PeriodCalculator);
		}

		#region Implementation

		OnlyCurrentPeriodIfPayByWebServiceValidator Validator;

		#region Filters

		SingleAccountingPeriodField SinglePeriodFilter
		{
			get
			{
				if (singlePeriodFilter == null)
				{
					singlePeriodFilter = new SingleAccountingPeriodField(Factory);
					SinglePeriodFilter.FieldName = "f";
					SinglePeriodFilter.DisplayName = "Period";
				}
				return singlePeriodFilter;
			}
		}
		SingleAccountingPeriodField singlePeriodFilter;

		MultipleChoice MultipleChoiceFilter
		{
			get
			{
				if (multipleChoiceFilter == null)
				{
					multipleChoiceFilter = new MultipleChoice(Factory);
					multipleChoiceFilter.FieldName = "f";
					multipleChoiceFilter.List.AddPair("NON");
					multipleChoiceFilter.List.AddPair("ABC");
				}
				return multipleChoiceFilter;
			}
		}
		MultipleChoice multipleChoiceFilter;

		DateRangeField DateRangeFilter
		{
			get
			{
				if (dateRangeFilter == null)
				{
					dateRangeFilter = new DateRangeField(Factory);
					dateRangeFilter.FieldName = "f";
				}
				return dateRangeFilter;
			}
		}
		DateRangeField dateRangeFilter;

		DateField DateFilter
		{
			get
			{
				if (dateFilter == null)
				{
					dateFilter = new DateField(Factory);
					dateFilter.FieldName = "f";
				}
				return dateFilter;
			}
		}
		DateField dateFilter;

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);

			Validator = new OnlyCurrentPeriodIfPayByWebServiceValidator();
		}

		AccountingPeriodTestHelper PeriodTestHelper
		{
			get
			{
				if (periodTestHelper == null)
				{
					periodTestHelper = new AccountingPeriodTestHelper(Factory);
				}
				return periodTestHelper;
			}
		}
		AccountingPeriodTestHelper periodTestHelper;

		#endregion
	}
}
