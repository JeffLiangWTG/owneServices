using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.ElectronicMessaging.Common.DataValidation.Testing
{
	public class JsonValidationTest : TestCaseWithFactory
	{
		const string JsonResourceName = "Enterprise.Accounting.ElectronicMessaging.Common.DataValidation.TestData.JsonSchemaForTest.json";

		readonly string ValidJson = @"{
		""countryCode"":""CN"",
		""reqType"":""02""
		}";
		readonly string InvalidJson = @"{
		""countryCode"":""XXCN""
		}";

		public void TestValidate()
		{
			var warningTestLogger = new NotificationBuffer();
			var errorTestLogger = new NotificationBuffer();
			var validator = new JsonValidation(JsonResourceName);

			using (var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(ValidJson)))
			{
				validator.Validate(jsonStream, errorTestLogger, warningTestLogger);
			}

			Assert(!errorTestLogger.HasErrors);
			Assert(!warningTestLogger.HasWarnings);

			using (var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(InvalidJson)))
			{
				validator.Validate(jsonStream, errorTestLogger, warningTestLogger);
			}

			var errors = errorTestLogger.Events.Where(x => x.Type == NotificationType.Error);

			Assert(!warningTestLogger.HasWarnings);
			AssertEquals(2, errors.Count());
			Assert(errors.Contains("String 'XXCN' exceeds maximum length of 2. Path 'countryCode', line 2, position 22."));
			Assert(errors.Contains("Required properties are missing from object: reqType. Path '', line 1, position 1."));
		}

		public void TestValidateWithJsonSchemaNet()
		{
			var warningTestLogger = new NotificationBuffer();
			var errorTestLogger = new NotificationBuffer();
			var validator = new JsonValidation2(JsonResourceName);

			using (var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(ValidJson)))
			{
				validator.Validate(jsonStream, errorTestLogger, warningTestLogger);
			}

			Assert(!errorTestLogger.HasErrors);
			Assert(!warningTestLogger.HasWarnings);

			using (var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(InvalidJson)))
			{
				validator.Validate(jsonStream, errorTestLogger, warningTestLogger);
			}

			Assert(!warningTestLogger.HasWarnings);
			var expectedErrors = @"
/: Required properties [""reqType""] are not present
/countryCode: Value should be at most 2 characters
".Trim();
			AssertEquals(expectedErrors, errorTestLogger.AsString.Trim());
		}

		public void TestJsonValidation_InvalidJsonResourceName()
		{
			var jsonResourceName = "";

			AssertExceptionThrown<ArgumentException>(() => new JsonValidation(null));
			AssertExceptionThrown<ArgumentException>(() => new JsonValidation(jsonResourceName));

			var warningTestLogger = new NotificationBuffer();
			var errorTestLogger = new NotificationBuffer();

			using (var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(ValidJson)))
			{
				var jsonValidation = new JsonValidation("XX");
				AssertExceptionThrown<ArgumentNullException>(() => jsonValidation.Validate(xmlStream, errorTestLogger, warningTestLogger));
			}
		}

		public void TestJsonValidation_InvalidJsonResourceName_WithJsonSchemaNet()
		{
			var jsonResourceName = "";

			AssertExceptionThrown<ArgumentException>(() => new JsonValidation2(null));
			AssertExceptionThrown<ArgumentException>(() => new JsonValidation2(jsonResourceName));

			var warningTestLogger = new NotificationBuffer();
			var errorTestLogger = new NotificationBuffer();

			using (var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(ValidJson)))
			{
				var jsonValidation = new JsonValidation2("XX");
				AssertExceptionThrown<ArgumentNullException>(() => jsonValidation.Validate(xmlStream, errorTestLogger, warningTestLogger));
			}
		}
	}
}
