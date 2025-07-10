using System;
using CargoWise.EntityFramework.Testing;
using Json.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class JsonSchemaLoaderTest : TestCaseWithFactory
	{
		public void TestLoadJsonSchema_IfSchemaDoesNotExist()
		{
			AssertNull(JsonSchemaLoader.Load("NonExistentSchema"));
		}

		public void TestLoadJsonSchema_IfSchemaExists()
		{
			var schema = JsonSchemaLoader.Load("Enterprise.Accounting.ElectronicMessaging.TaxCore.EInvoice.TaxCoreEInvoiceSchema.json");
			AssertNotNull(schema);
			AssertEquals("JSON Schema for Fiji E-Invoice JSON file format. The original draft3 version of the schema has been upgraded to current 2020-12 version", schema.Description);
		}

		public void TestLoadJsonSchema2_Throws_IfSchemaDoesNotExist()
		{
			AssertExceptionThrown<ArgumentException>(
				() => JsonSchemaLoader.Load2("NonExistentSchema")
			);
		}

		public void TestLoadJsonSchema2_ReturnsSchema()
		{
			var schema = JsonSchemaLoader.Load2("Enterprise.Accounting.ElectronicMessaging.TaxCore.EInvoice.TaxCoreEInvoiceSchema.json");
			AssertNotNull(schema);
			AssertEquals("JSON Schema for Fiji E-Invoice JSON file format. The original draft3 version of the schema has been upgraded to current 2020-12 version", schema.GetDescription());
		}
	}
}
