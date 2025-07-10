using System;
using System.Web;
using System.Web.UI;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZNumericTextBoxTest : ZTextBoxBaseTest
	{
		#region setup

		protected override Control GetNewControl()
		{
			return new ZNumericTextBox();
		}

		ZNumericTextBox TestNumericTextBox
		{
			get { return (ZNumericTextBox)Control; }
		}

		protected override ZPropertyInfo BindToProperty
		{
			get { return TestBizO.Z0_AnotherDecimalInfo; }
		}

		protected override bool BindToProperty_ReadOnly
		{
			get => TestBizO.Z0_AnotherDecimal_ReadOnly;
			set => TestBizO.Z0_AnotherDecimal_ReadOnly = value;
		}

		protected override IZType TestValue
		{
			get { return new ZDecimal(1.23m); }
		}

		protected override IZType TestValue2
		{
			get { return new ZDecimal(2.35m); }
		}

		protected override IZType TestValueTooLong => null;

		#endregion

		public void TestDefaultDecimals()
		{
			AssertEquals(0, TestNumericTextBox.Decimals);

			TestNumericTextBox.BindTo = BindToProperty.Name;
			TestNumericTextBox.Bind(TestBizO);
			AssertEquals("ZDecimal = 2 decimal places", 2, TestNumericTextBox.Decimals);

			TestNumericTextBox.BindTo = TestBizO.Z0_NumberInfo.Name;
			TestNumericTextBox.Bind(TestBizO);
			AssertEquals("ZInt = 0 decimal places", 0, TestNumericTextBox.Decimals);

			TestNumericTextBox.BindTo = TestBizO.Z0_ShortInfo.Name;
			TestNumericTextBox.Bind(TestBizO);
			AssertEquals("ZShort = 0 decimal places", 0, TestNumericTextBox.Decimals);

			TestNumericTextBox.BindTo = TestBizO.Z0_ByteInfo.Name;
			TestNumericTextBox.Bind(TestBizO);
			AssertEquals("ZByte = 0 decimal places", 0, TestNumericTextBox.Decimals);
		}

		public void TestValidationMessage()
		{
			var numericTextBox = new ZNumericTextBoxForTest();

			numericTextBox.BindTo = BindToProperty.Name;
			numericTextBox.Bind(TestBizO);

			AssertEquals("Please enter numeric characters only", numericTextBox.ValidationMessageForTest);

			numericTextBox.BindTo = BindToProperty.Name;
			numericTextBox.ValidateBindToDecimals = false;
			numericTextBox.Bind(TestBizO);

			AssertEquals("Please enter numeric characters only", numericTextBox.ValidationMessageForTest);

			numericTextBox.ValidateBindToDecimals = true;
			numericTextBox.Decimals = 5;
			numericTextBox.Bind(TestBizO);

			AssertEquals("Please enter numeric characters only (allowed number of decimal places is 5)", numericTextBox.ValidationMessageForTest);
		}

		[HttpContextEnabledTest]
		public void TestTextIsAccordingToCulture()
		{
			var numericTextBox = new ZNumericTextBoxForTest();
			numericTextBox.BindTo = BindToProperty.Name;
			numericTextBox.Bind(TestBizO);

			numericTextBox.SelectedValueForTest = TestValue;
			AssertEquals(TestValue, numericTextBox.SelectedValueForTest);
			AssertEquals("1.23", numericTextBox.Text);

			numericTextBox.SelectedValueForTest = new ZDecimal("1548795211254789654.236");
			AssertEquals("1548795211254789654.23", numericTextBox.Text);

			numericTextBox.SelectedValueForTest = new ZDecimal("200");
			AssertEquals("200.00", numericTextBox.Text);

			numericTextBox.SelectedValueForTest = new ZDecimal(".1");
			AssertEquals("0.10", numericTextBox.Text);

			numericTextBox.SelectedValueForTest = new ZDecimal("2.");
			AssertEquals("2.00", numericTextBox.Text);

			var dummyApplication = (DummyHttpApplication)HttpContext.Current.ApplicationInstance;
			var dummyWorkerRequest = dummyApplication.WorkerRequest;
			dummyWorkerRequest.SetUserLanguagesSeparatedByComma("ru-RU");

			numericTextBox.SelectedValueForTest = TestValue;
			AssertEquals(TestValue, numericTextBox.SelectedValueForTest);
			AssertEquals("1,23", numericTextBox.Text);

			numericTextBox.SelectedValueForTest = new ZDecimal("1548795211254789654.2365");
			AssertEquals("1548795211254789654,23", numericTextBox.Text);

			numericTextBox.SelectedValueForTest = new ZDecimal("200");
			AssertEquals("200,00", numericTextBox.Text);

			numericTextBox.SelectedValueForTest = new ZDecimal(".1");
			AssertEquals("0,10", numericTextBox.Text);

			numericTextBox.SelectedValueForTest = new ZDecimal("2.");
			AssertEquals("2,00", numericTextBox.Text);

			numericTextBox.Decimals = 0;
			numericTextBox.SelectedValueForTest = new ZDecimal("12");
			AssertEquals("12", numericTextBox.Text);

			dummyWorkerRequest.ClearUserLanguages();
		}

		[HttpContextEnabledTest]
		public void TestIncompleteEntryShort()
		{
			var numericTextBox = new ZNumericTextBoxForTest();
			numericTextBox.BindTo = TestBizO.Z0_ShortInfo.Name;
			numericTextBox.Bind(TestBizO);
			numericTextBox.Text = "2.";
			var number = numericTextBox.SelectedValueForTest;
			AssertEquals(new ZShort(2), number);
		}

		[HttpContextEnabledTest]
		public void TestIncompleteEntryInteger()
		{
			var numericTextBox = new ZNumericTextBoxForTest();
			numericTextBox.BindTo = TestBizO.Z0_NumberInfo.Name;
			numericTextBox.Bind(TestBizO);
			numericTextBox.Text = "2.";
			var number = numericTextBox.SelectedValueForTest;
			AssertEquals(2, number);
		}

		[HttpContextEnabledTest]
		public void TestValidationPattern()
		{
			var numericTextBox = new ZNumericTextBoxForTest();

			numericTextBox.BindTo = BindToProperty.Name;
			numericTextBox.Bind(TestBizO);

			AssertEquals("^\\\\d+\\\\.?\\\\d*$", numericTextBox.ValidationPatternForTest);

			numericTextBox.BindTo = BindToProperty.Name;
			numericTextBox.ValidateBindToDecimals = false;
			numericTextBox.Bind(TestBizO);

			AssertEquals("^\\\\d+\\\\.?\\\\d*$", numericTextBox.ValidationPatternForTest);

			numericTextBox.ValidateBindToDecimals = true;
			numericTextBox.Decimals = 7;
			numericTextBox.Bind(TestBizO);

			AssertEquals("^\\\\d+\\\\.?\\\\d{0,7}$", numericTextBox.ValidationPatternForTest);

			var dummyApplication = (DummyHttpApplication)HttpContext.Current.ApplicationInstance;
			var dummyWorkerRequest = dummyApplication.WorkerRequest;
			dummyWorkerRequest.SetUserLanguagesSeparatedByComma("ru-RU");

			AssertEquals("^\\\\d+\\\\,?\\\\d{0,7}$", numericTextBox.ValidationPatternForTest);

			dummyWorkerRequest.ClearUserLanguages();
		}

		class ZNumericTextBoxForTest : ZNumericTextBox
		{
			public string ValidationMessageForTest
			{
				get
				{
					return base.ValidationMessage;
				}
			}

			public string ValidationPatternForTest
			{
				get
				{
					return base.ValidationPattern;
				}
			}

			public IZType SelectedValueForTest
			{
				set { base.SelectedValue = value; }
				get { return base.SelectedValue; }
			}
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestNegativeDecimalsThrowsException()
		{
			TestNumericTextBox.Decimals = -1;
		}
	}
}
