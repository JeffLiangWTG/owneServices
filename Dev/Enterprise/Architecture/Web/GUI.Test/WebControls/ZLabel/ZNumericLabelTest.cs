using System.Globalization;
using System.Threading;
using System.Web;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZNumericLabelTest : ZLabelBaseTestBase
	{
		public virtual void TestBindToDecimalProperty()
		{
			ZLabelTestO.BindTo = DummyBusinessObject.Schema.Z0_Decimal;
			ZLabelTestO.BindToDecimals = DummyBusinessObject.Schema.Z0_Short;
			TestBizO.Z0_Short = 4;
			ZLabelTestO.Bind(TestBizO);
			AssertEquals(TestBizO.Z0_Decimal.ToString(ZLabelTestO.Decimals), ZLabelTestO.Text);
		}

		public virtual void TestBindToNumericProperty()
		{
			ZLabelTestO.BindTo = DummyBusinessObject.Schema.Z0_Number;
			ZLabelTestO.Bind(TestBizO);
			AssertEquals(TestBizO.Z0_Number.ToString(), ZLabelTestO.Text);
		}

		public virtual void TestBindToNonNumericProperty()
		{
			ZLabelTestO.BindTo = DummyBusinessObject.Schema.Z0_Description;
			ZLabelTestO.Bind(TestBizO);
			AssertEquals("", ZLabelTestO.Text);
		}

		public void TestDecimals()
		{
			AssertEquals("Default Value", 2, ZLabelTestO.Decimals);
			ZLabelTestO.Decimals = 3;
			AssertEquals("Assigned Value", 3, ZLabelTestO.Decimals);

			ZLabelTestO.BindTo = DummyBusinessObject.Schema.Z0_Decimal;
			ZLabelTestO.Bind(TestBizO);
			AssertEquals("ToString Format", TestBizO.Z0_Decimal.ToString(ZLabelTestO.Decimals), ZLabelTestO.Text);
		}

		public void TestShowGroupSeparatorsProperty()
		{
			AssertEquals("Default Value", false, ZLabelTestO.ShowGroupSeparators);

			ZLabelTestO.ShowGroupSeparators = true;
			AssertEquals("Assigned Value", true, ZLabelTestO.ShowGroupSeparators);

			ZLabelTestO.ShowGroupSeparators = false;
		}

		[HttpContextEnabledTest]
		public void TestFormatNumber()
		{
			AssertEquals("Precondition: show group separators", false, ZLabelTestO.ShowGroupSeparators);
			AssertEquals("Precondition: decimals", 2, ZLabelTestO.Decimals);

			double originalDouble = 12345.391;
			ZString expectedDouble = "12345.39";
			ZString result = ZLabelTestO.FormatNumberInternal(originalDouble, ZLabelTestO.Decimals);

			AssertEquals("Format double without separators(AU)", expectedDouble, result);

			int originalInt = 1234567;
			ZString expectedInt = "1234567";
			result = ZLabelTestO.FormatNumberInternal(originalInt, 0);

			AssertEquals("Format int without separators(AU)", expectedInt, result);

			ZLabelTestO.ShowGroupSeparators = true;

			expectedDouble = "12,345.39";
			result = ZLabelTestO.FormatNumberInternal(originalDouble, ZLabelTestO.Decimals);
			AssertEquals("Format double with separators(AU)", expectedDouble, result);

			expectedInt = "1,234,567";
			result = ZLabelTestO.FormatNumberInternal(originalInt, 0);
			AssertEquals("Format int with separators(AU)", expectedInt, result);

			ZLabelTestO.ShowGroupSeparators = false;

			DummyHttpApplication dummyApplication = (DummyHttpApplication)HttpContext.Current.ApplicationInstance;
			DummyWorkerRequest dummyWorkerRequest = dummyApplication.WorkerRequest;
			dummyWorkerRequest.SetUserLanguagesSeparatedByComma("ru-RU");

			expectedDouble = "12345,39";
			result = ZLabelTestO.FormatNumberInternal(originalDouble, ZLabelTestO.Decimals);

			AssertEquals("Format double without separators(RU)", expectedDouble, result);

			expectedInt = "1234567";
			result = ZLabelTestO.FormatNumberInternal(originalInt, 0);

			AssertEquals("Format int without separators(RU)", expectedInt, result);

			ZLabelTestO.ShowGroupSeparators = true;

			string separator = CultureInfo.CreateSpecificCulture("ru-RU").NumberFormat.NumberGroupSeparator;

			expectedDouble = string.Format("12{0}345,39", separator);
			result = ZLabelTestO.FormatNumberInternal(originalDouble, ZLabelTestO.Decimals);
			AssertEquals("Format double with separators(RU)", expectedDouble, result);

			expectedInt = string.Format("1{0}234{0}567", separator);
			result = ZLabelTestO.FormatNumberInternal(originalInt, 0);
			AssertEquals("Format int with separators(RU)", expectedInt, result);

			dummyWorkerRequest.ClearUserLanguages();
			ZLabelTestO.ShowGroupSeparators = false;
		}

		public void TestShowGroupSeparators()
		{
			AssertEquals("Precondition", false, ZLabelTestO.ShowGroupSeparators);

			ZDecimal originalValueDecimal = TestBizO.Z0_Decimal;
			ZDecimal testDecimal = 123456789.12;
			TestBizO.Z0_Decimal = testDecimal;

			ZLabelTestO.BindTo = TestBizO.Z0_DecimalInfo.Name;
			ZLabelTestO.Bind(TestBizO);

			AssertContainsGroupSeperator(false, ZLabelTestO.Text);

			ZLabelTestO.ShowGroupSeparators = true;
			ZLabelTestO.BindTo = TestBizO.Z0_DecimalInfo.Name;
			ZLabelTestO.Bind(TestBizO);

			AssertContainsGroupSeperator(true, ZLabelTestO.Text);

			ZInt originalValueInt = TestBizO.Z0_Number;
			ZInt testInt = 123456789;
			TestBizO.Z0_Number = testInt;
			ZLabelTestO.ShowGroupSeparators = false;

			ZLabelTestO.BindTo = TestBizO.Z0_NumberInfo.Name;
			ZLabelTestO.Bind(TestBizO);

			AssertContainsGroupSeperator(false, ZLabelTestO.Text);

			ZLabelTestO.ShowGroupSeparators = true;
			ZLabelTestO.BindTo = TestBizO.Z0_NumberInfo.Name;
			ZLabelTestO.Bind(TestBizO);

			AssertContainsGroupSeperator(true, ZLabelTestO.Text);

			ZLabelTestO.ShowGroupSeparators = false;
			TestBizO.Z0_Decimal = originalValueDecimal;
			TestBizO.Z0_Number = originalValueInt;
		}

		protected void AssertContainsGroupSeperator(bool expectedResult, ZString labelText)
		{
			Assert("Label text should not be empty", !labelText.IsEmpty);

			ZString message = expectedResult ?
				"Label text should contain number group seperator" :
				"Label text should not contain number group seperator";

			AssertEquals(message, expectedResult, labelText.Contains(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator));
		}

		#region Implementation

		protected override ZLabelBase GetNewLabel()
		{
			return new ZNumericLabel();
		}

		protected new ZNumericLabel ZLabelTestO
		{
			get { return base.ZLabelTestO as ZNumericLabel; }
			set { base.ZLabelTestO = value; }
		}

		#endregion
	}
}
