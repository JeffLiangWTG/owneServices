using System.Linq;
using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class LoggingMethodsTypeTest : TestCase
	{
		public void TestValues()
		{
			// Arrange
			var expectedResult = new (string code, string description)[]
			{
				("FSL", "File System Logging"),
				("ELK", "Elastic Search Logging"),
				("KAF", "Kafka Logging"),
				("SYS", "Syslog Logging"),
				("CFL", "Combined File System Logging"),
			};

			// Act
			var result = new LoggingMethods()
				.Cast<ICodeDescription>()
				.Select(b => (b.Code, b.Description));

			// Assert
			AssertContainsExactElementsInAnyOrder(expectedResult, result);
		}

		public void TestDefaultValue()
		{
			// Arrange
			var loggingMethods = new LoggingMethods();

			// Act
			var result = loggingMethods.DefaultCode;

			// Assert
			AssertEquals("FSL", result);
		}
	}
}
