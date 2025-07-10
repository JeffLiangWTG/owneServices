using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.ElectronicMessaging.Common.DataValidation.Testing
{
	public class XsdValidationTest : TestCaseWithFactory
	{
		readonly ZString[] XsdResourceNames = new ZString[] { "Enterprise.Accounting.ElectronicMessaging.Common.DataValidation.TestData.XMLSchemaForTest.xsd" };
		readonly string XMLValid = @"<?xml version=""1.0"" encoding=""utf-8""?>
		<eInvoicing CountryCode=""str1234"" DocumentType=""str1234"">
		<StreetAddress>str1234</StreetAddress>
		</eInvoicing>";
		readonly string XMLInvalid = @"<?xml version=""1.0"" encoding=""utf-8""?>
		<eInvoicing DocumentType=""str1234"">
		<StreetAddress>str1234</StreetAddress>
		</eInvoicing>";

		public void TestValidation_XmlWithoutErrors()
		{
			var warningTestLogger = new NotificationBuffer();
			var errorTestLogger = new NotificationBuffer();
			Assert(!errorTestLogger.HasErrors);
			Assert(!warningTestLogger.HasWarnings);

			using (var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(XMLValid)))
			{
				var xsdValidation = new XsdValidation(XsdResourceNames);
				xsdValidation.ValidateXml(xmlStream, errorTestLogger, warningTestLogger);
			}

			Assert(!errorTestLogger.HasErrors);
			Assert(!warningTestLogger.HasWarnings);
		}

		public void TestValidationXsdWithErrors()
		{
			var errorTestLogger = new NotificationBuffer();
			var warningTestLogger = new NotificationBuffer();
			Assert(!errorTestLogger.HasErrors);
			Assert(!warningTestLogger.HasWarnings);

			using (var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(XMLInvalid)))
			{
				var xsdValidation = new XsdValidation(XsdResourceNames);
				xsdValidation.ValidateXml(xmlStream, errorTestLogger, warningTestLogger);
			}

			var errors = errorTestLogger.Events.Where(x => x.Type == NotificationType.Error);

			Assert(!warningTestLogger.HasWarnings);
			AssertEquals(1, errors.Count());
			Assert(errors.Contains("The required attribute 'CountryCode' is missing."));
		}

		public void TestXsdValidation_XsdResourceNames()
		{
			ZString[] xsdResourceNames = null;
			AssertExceptionThrown<ArgumentException>(() => new XsdValidation(xsdResourceNames));

			xsdResourceNames = Array.Empty<ZString>();
			AssertExceptionThrown<ArgumentException>(() => new XsdValidation(xsdResourceNames));

			AssertNoExceptionThrown("Should not throw any exception", () => new XsdValidation(XsdResourceNames));
		}
	}
}
