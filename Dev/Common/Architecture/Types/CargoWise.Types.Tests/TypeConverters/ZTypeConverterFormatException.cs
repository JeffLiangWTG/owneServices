using System;

namespace CargoWise.Types.Tests
{
	using NUnit.Framework;

	class ZTypeConverterFormatExceptionTest : TestCase
	{
		public void TestRethrowForConvertFrom()
		{
			AssertEquals("Message (in ConvertFrom, value = 'Value')", GetConvertFromMessage(new FormatException("Message"), "Value"));
			AssertEquals("Message (in ConvertFrom, value = null)", GetConvertFromMessage(new FormatException("Message"), null));
			AssertEquals("Message (in ConvertFrom, value = DBNull.Value)", GetConvertFromMessage(new FormatException("Message"), DBNull.Value));
		}

		public void TestRethrowForConvertTo()
		{
			AssertEquals("Message (in ConvertTo, value = 'Value'; originalType = 'System.String'; destinationType = 'System.String')", GetConvertToMessage(new FormatException("Message"), "Value", typeof(string)));
		}

		static string GetConvertFromMessage(FormatException e, object value)
		{
			try
			{
				e.RethrowForConvertFrom(value);
				Fail("Expected an exception");
				return null;
			}
			catch (FormatException ex)
			{
				return ex.Message;
			}
		}

		static string GetConvertToMessage(FormatException e, object value, Type destinationType)
		{
			try
			{
				e.RethrowForConvertTo(value, destinationType);
				Fail("Expected an exception");
				return null;
			}
			catch (FormatException ex)
			{
				return ex.Message;
			}
		}
	}
}
