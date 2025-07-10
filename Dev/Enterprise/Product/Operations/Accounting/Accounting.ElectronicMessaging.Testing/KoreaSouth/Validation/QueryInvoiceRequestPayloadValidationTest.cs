using System.IO;
using System.Text;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	class QueryInvoiceRequestPayloadValidationTest : TestCaseWithFactory
	{
		public void TestValidateWithoutError()
		{
			var xml = @"<?xml version=""1.0"" encoding=""utf-8""?><Empty />";
			IPayloadValidation validator = new QueryInvoiceRequestPayloadValidation();
			var errorNotification = new Logger();
			var warnningNotification = new Logger();
			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				validator.Validate(ms, errorNotification, warnningNotification);

				ms.Position = 0;
				AssertEquals(xml, Encoding.UTF8.GetString(ms.ToArray()));
			}
			AssertEquals(false, errorNotification.HasErrors);
			AssertEquals(false, warnningNotification.HasErrors);
		}

		public void TestValidateWithError()
		{
			var xml = @"AAA";
			IPayloadValidation validator = new QueryInvoiceRequestPayloadValidation();
			var errorNotification = new Logger();
			var warnningNotification = new Logger();
			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				validator.Validate(ms, errorNotification, warnningNotification);

				ms.Position = 0;
				AssertEquals(xml, Encoding.UTF8.GetString(ms.ToArray()));
			}
			AssertEquals("Invalid payload content, it should be empty.", errorNotification.ToString());
			AssertEquals(false, warnningNotification.HasErrors);
		}
	}
}
