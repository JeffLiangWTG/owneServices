using System;
using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(DateTimeRegistryDataType))]
	sealed class DateTimeRegistryDataTypeTest : RegistryDataTypeTestCase<DateTimeRegistryDataType>
	{
		protected override DateTimeRegistryDataType GetNewDataType()
		{
			return new DateTimeRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(new DateTime(2005, 5, 2, 23, 24, 32, 532), Encoding.Unicode.GetBytes("02/05/2005 23:24:32:532")),
				new ValidSampleAndBinaryValueInDB(DateTime.MinValue, Encoding.Unicode.GetBytes(DateTime.MinValue.ToString())),
				new ValidSampleAndBinaryValueInDB(new DateTime(2005, 5, 2, 23, 24, 25, 0), Encoding.Unicode.GetBytes(new DateTime(2005, 5, 2, 23, 24, 25, 0).ToString())),
				new ValidSampleAndBinaryValueInDB(new DateTime(2004, 10, 21, 0, 0, 0, 0), Encoding.Unicode.GetBytes(new DateTime(2004, 10, 21, 0, 0, 0, 0).ToString()))
			};
		}

		public void TestDeserialisationFormatFallback()
		{
			DateTimeRegistryDataType testRegistryType = GetNewDataType();
			AssertEquals("yyyy-MM-dd HH:mm:ss.fff",
				new DateTime(2007, 6, 21, 17, 5, 14, 200),
				testRegistryType.Deserialise(Encoding.Unicode.GetBytes("2007-06-21 17:05:14.200")));
			AssertEquals("yyyy-MM-dd HH:mm:ss.fff",
				new DateTime(2008, 7, 1, 1, 9, 12, 3),
				testRegistryType.Deserialise(Encoding.Unicode.GetBytes("2008-07-01 01:09:12.003")));
			AssertEquals("dd/mm/yyyy HH:mm:ss:fff",
				new DateTime(2005, 5, 2, 23, 24, 32, 532),
				testRegistryType.Deserialise(Encoding.Unicode.GetBytes("02/05/2005 23:24:32:532")));
			AssertEquals("blank string",
				DateTime.MinValue,
				testRegistryType.Deserialise(Encoding.Unicode.GetBytes("")));
			AssertEquals("dd-MM-yyyy HH:mm:ss.fff", new DateTime(2008, 2, 21, 16, 42, 36, 924), testRegistryType.Deserialise(Encoding.Unicode.GetBytes("21-02-2008 16:42:36:924")));
			AssertEquals("dd.MM.yyyy HH:mm:ss.fff", new DateTime(1984, 7, 10, 3, 34, 48, 0), testRegistryType.Deserialise(Encoding.Unicode.GetBytes("10.07.1984 03:34:48:000")));
		}

		public void TestInvalidDateTimeThrowsException()
		{
			DateTimeRegistryDataType testRegistryType = GetNewDataType();

			AssertExceptionThrown<RegistryValidationException>(() => testRegistryType.Validate(null, DateTime.MinValue, Guid.Empty, Guid.Empty, Guid.Empty));

			AssertNoExceptionThrown(() => testRegistryType.Validate(null, new DateTime(1999, 04, 14), Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestDeserialisationWithInvalidFormatThrowsException()
		{
			AssertDeserialiseThrowsException("02,05,2005 23:24:32:532");
			AssertDeserialiseThrowsException("0205.2005 23:24:32:532");
			AssertDeserialiseThrowsException("02,05,2005 23:2432");
			AssertDeserialiseThrowsException("2005");
		}

		void AssertDeserialiseThrowsException(string dateTimeString)
		{
			try
			{
				GetNewDataType().Deserialise(Encoding.Unicode.GetBytes(dateTimeString));
				Fail("Should throw exception");
			}
			catch (RegistryParsingException ex)
			{
				AssertEquals("Message", string.Format("Unable to parse DateTime registry value [{0}]", dateTimeString), ex.Message);
				AssertEquals("InnerException Type", true, ex.InnerException is FormatException);
				AssertEquals("InnerException Message", "String was not recognized as a valid DateTime.", ex.InnerException.Message);
			}
		}
	}
}
