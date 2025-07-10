using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ApplicationIdentifier))]
	sealed class ApplicationIdentifierTest : RegistryBusinessObjectTemplateTestCase<ApplicationIdentifier>
	{
		public void TestApplicationID()
		{
			BizObj.ApplicationID = "1234";
			AssertEquals("1234", BizObj.ApplicationID);
		}

		public void TestApplicationIDCode()
		{
			BizObj.ApplicationID = "12";
			AssertEquals("12", BizObj.ApplicationIDCode);

			BizObj.ApplicationID = "12n";
			AssertEquals("12", BizObj.ApplicationIDCode);

			BizObj.ApplicationID = "12sn";
			AssertEquals("12", BizObj.ApplicationIDCode);
		}

		public void TestApplicationIDHasDecimalPointIndicator()
		{
			BizObj.ApplicationID = "12";
			AssertEquals(false, BizObj.ApplicationIDHasDecimalPointIndicator);

			BizObj.ApplicationID = "123n";
			AssertEquals(true, BizObj.ApplicationIDHasDecimalPointIndicator);
		}

		public void TestApplicationIDHasSequence()
		{
			BizObj.ApplicationID = "12";
			AssertEquals(false, BizObj.ApplicationIDHasSequence);

			BizObj.ApplicationID = "123s";
			AssertEquals(true, BizObj.ApplicationIDHasSequence);
		}

		public void TestIsFixedLengthField()
		{
			BizObj.MinFieldLength = 10;
			BizObj.MaxFieldLength = 20;
			AssertEquals(false, BizObj.IsFixedLengthField);

			BizObj.MinFieldLength = 20;
			AssertEquals(true, BizObj.IsFixedLengthField);
		}

		public void TestFullTitle()
		{
			BizObj.EnglishFullTitle = "Serial Shipping Container Code";
			AssertEquals("Serial Shipping Container Code", BizObj.FullTitle);
		}

		public void TestDataTitle()
		{
			BizObj.EnglishDataTitle = "SSCC";
			AssertEquals("SSCC", BizObj.DataTitle);
		}

		public void TestDataType()
		{
			BizObj.DataType = "AN";
			AssertEquals("AN", BizObj.DataType);
		}

		public void TestMinFieldLength()
		{
			BizObj.MinFieldLength = 56;
			AssertEquals(56, BizObj.MinFieldLength);
		}

		public void TestMaxFieldLength()
		{
			BizObj.MaxFieldLength = 189;
			AssertEquals(189, BizObj.MaxFieldLength);
		}

		public void TestLengthOfFieldPlusAnyIndicator()
		{
			BizObj.ApplicationID = "12n";
			BizObj.MinFieldLength = 9;
			AssertEquals(10, BizObj.LengthOfFieldPlusAnyIndicator);

			BizObj.ApplicationID = "12s";
			BizObj.MinFieldLength = 5;
			AssertEquals(6, BizObj.LengthOfFieldPlusAnyIndicator);

			BizObj.ApplicationID = "12";
			BizObj.MinFieldLength = 7;
			AssertEquals(7, BizObj.LengthOfFieldPlusAnyIndicator);
		}

		public void TestValidateApplicationID()
		{
			AssertNoErrors(BizObj.ApplicationIDInfo);

			BizObj.ApplicationID = "";
			AssertHasError(BizObj.ApplicationIDInfo, "Please enter a value.");

			BizObj.ApplicationID = "10";
			AssertNoError(BizObj.ApplicationIDInfo, "Please enter a value.");
		}

		public void TestValidateFullTitle()
		{
			AssertNoErrors(BizObj.FullTitleInfo);

			BizObj.EnglishFullTitle = "";
			AssertHasError(BizObj.FullTitleInfo, "Please enter a value.");

			BizObj.EnglishFullTitle = "Serial Shipping Container Code";
			AssertNoError(BizObj.FullTitleInfo, "Please enter a value.");
		}

		public void TestValidateDataType()
		{
			AssertNoErrors(BizObj.DataTypeInfo);

			BizObj.DataType = "";
			AssertHasError(BizObj.DataTypeInfo, "Please enter a value.");

			BizObj.DataType = "BB";
			AssertHasError(BizObj.DataTypeInfo, "Enter a valid selection.");

			BizObj.DataType = "D";
			AssertNoErrors(BizObj.DataTypeInfo);
		}

		public void TestValidateMinFieldLength()
		{
			AssertNoErrors(BizObj.MinFieldLengthInfo);

			BizObj.MinFieldLength = -1;
			AssertHasError(BizObj.MinFieldLengthInfo, "Please enter a 'Min Field Length' greater than or equal to 0.");

			BizObj.MinFieldLength = 0;
			AssertNoErrors(BizObj.MinFieldLengthInfo);
		}

		public void TestValidateMaxFieldLength()
		{
			AssertNoErrors(BizObj.MaxFieldLengthInfo);

			BizObj.MaxFieldLength = -1;
			AssertHasError(BizObj.MaxFieldLengthInfo, "Please enter a 'Max Field Length' greater than or equal to 0.");

			BizObj.MaxFieldLength = 0;
			AssertNoErrors(BizObj.MaxFieldLengthInfo);
		}

		#region Implementation

		protected override ApplicationIdentifier GetBusinessObjectToClone()
		{
			return new ApplicationIdentifier();
		}

		protected override ApplicationIdentifier GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
