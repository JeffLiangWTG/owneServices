using System.Linq;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Core.Testing
{
	class IZTypeExtensionsTest : TestCase
	{
		public void TestGetValidMaxLengthValue()
		{
			var logger = new TestErrorLogger();
			AssertEquals("Value less than base schema.", "ABCDEFG", IZTypeExtensions.GetValidMaxLengthValue("ABCDEFG", DummyBizoSchema.Z0_Description, logger, 0));
			Assert(!logger.Logs.Any());

			AssertEquals("More than base schema.", "ABCDE", IZTypeExtensions.GetValidMaxLengthValue("ABCDEFG", DummyBizoSchema.Z0_Code, logger, 0));
			AssertContains("Warning - Attempted to insert 7 characters into Field [Z0_Code] which has a maximum length of 5 characters. Field was truncated.", logger.Logs);

			logger.ClearLogs();
			AssertEquals("More than base schema.", "ABC", IZTypeExtensions.GetValidMaxLengthValue("ABCDEFG", DummyBizoSchema.Z0_Code, logger, 3));
			AssertContains("Warning - Attempted to insert 7 characters into Field [Z0_Code] which has a maximum length of 3 characters. Field was truncated.", logger.Logs);

			logger.ClearLogs();
			AssertEquals("More than base schema.", "ABCDE", IZTypeExtensions.GetValidMaxLengthValue("ABCDEFG", DummyBizoSchema.Z0_Code, logger, 10));
			AssertContains("Warning - Attempted to insert 7 characters into Field [Z0_Code] which has a maximum length of 5 characters. Field was truncated.", logger.Logs);

			logger.ClearLogs();
			AssertEquals("More than base schema.", "A", IZTypeExtensions.GetValidMaxLengthValue("ABCDEFG", DummyBizoSchema.Z0_Code, logger, 1));
			AssertContains("Warning - Attempted to insert 7 characters into Field [Z0_Code] which has a maximum length of 1 characters. Field was truncated.", logger.Logs);
		}
	}
}
