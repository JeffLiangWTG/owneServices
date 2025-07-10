using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Newtonsoft.Json.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class JsonHelperTest : TestCaseWithFactory
	{
		public void TestValidateJSON_NoValidationError()
		{
			var notifications = new Logger();
			JsonHelper.ValidateJSON(Schema, SampleValidJson, notifications);
			AssertEquals("No Error", string.Empty, notifications.ToString());
		}

		public void TestValidateJSON_WithJsonSchemaNetOverload_NoValidationError()
		{
			var notifications = new Logger();
			var isValid = JsonHelper.ValidateJSON(SchemaFromChina, SampleValidJsonForChina, notifications);
			CombineAssertions(() =>
			{
				AssertEquals("JSON should be valid according to schema", true, isValid);
				AssertEquals("No Error", string.Empty, notifications.ToString());
			});
		}

		public void TestValidateJSON_HasValidationError()
		{
			var notifications = new Logger();
			JsonHelper.ValidateJSON(Schema, SampleInvalidJson, notifications);
			AssertEquals("Has Error", "Required properties are missing from object: IT, TT, PaymentType, InvoiceNumber. Path '', line 1, position 1.", notifications.ToString());
		}

		public void TestValidateJSON_WithJsonSchemaNetOverload_HasValidationErrorByParser()
		{
			var notifications = new Logger();
			var isValid = JsonHelper.ValidateJSON(SchemaFromChina, SampleNotParsableAsJson, notifications);
			CombineAssertions(() =>
			{
				AssertEquals("JSON should be invalid according to parsing", false, isValid);
				AssertEquals("Should have error from parser", "'T' is an invalid start of a value. LineNumber: 0 | BytePositionInLine: 0.", notifications.ToString());
			});
		}

		public void TestValidateJSON_WithJsonSchemaNetOverload_HasValidationErrorBySchema()
		{
			var notifications = new Logger();
			var isValid = JsonHelper.ValidateJSON(SchemaFromChina, SampleInvalidJsonForChina, notifications);
			CombineAssertions(() =>
			{
				AssertEquals("JSON should be invalid according to schema", false, isValid);
				var expectedErrors = @"
/: Required properties [""clientNo"",""data""] are not present
/reqType: Value should be at most 3 characters
".Trim();
				AssertEquals("Should have errors from schema validation", expectedErrors, notifications.ToString());
			});
		}

		string SampleValidJson => @"{""DateAndTimeOfIssue"":""2019-07-16T15:26:00Z"",""BD"":""12345678798"",""IT"":0,""TT"":0,""PaymentType"":0,""InvoiceNumber"":""00001000"",""OmitQRCodeGen"":""0"",""OmitTextualRepresentation"":""0"",""Hash"":""tSBkM8Wf9\/KSEU+FiHBKUg=="",""Items"":[{""Name"":""Charge Code 1"",""Quantity"":1,""Labels"":[""A""],""TotalAmount"":100.0000}]}";

		string SampleValidJsonForChina => @"{""reqType"":""02"",""taxNo"":""12345678901234567890"",""clientNo"":""0001"",""autoexec"":""0"",""data"":{""serialNumber"":""abe93f09-1f10-41c7-bef7-2e8178799be6|EDIEDIDAT"",""extend"":""PDF"",""invType"":""11"",""version"":"""",""drawer"":"""",""payee"":"""",""reviewer"":"""",""seller"":{""identifier"":"""",""name"":"""",""address"":"""",""telephoneNo"":"""",""bank"":"""",""bankAcc"":""""},""order"":{""orderNo"":""INV0001"",""invoiceList"":"""",""invoiceSplit"":""0"",""invoiceSfdy"":""0"",""orderDate"":""2024-11-18 08:18:03"",""chargeTaxWay"":""0"",""totalAmount"":""540.0"",""taxMark"":""0"",""totalDiscount"":"""",""remark"":"""",""extractedCode"":"""",""buyer"":{""customerType"":""1"",""identifier"":""123456789012345"",""name"":""Test company name\/\"""",""address"":""state1-CN city1 address1 address2"",""telephoneNo"":""133099822"",""bank"":""Bank name1"",""bankAcc"":""Bank account1"",""email"":""12345678@qq.com"",""memberId"":"""",""isSend"":""1"",""recipient"":"""",""reciAddress"":"""",""zip"":""""},""orderDetails"":[{""venderOwnCode"":""ZZCC1"",""productCode"":"""",""productName"":""费用"",""rowType"":""0"",""spec"":"""",""unit"":"""",""quantity"":""1"",""unitPrice"":""100"",""amount"":""100"",""deductAmount"":"""",""taxRate"":""0"",""taxAmount"":""0"",""mxTotalAmount"":""100"",""taxRateMark"":""1"",""policyMark"":""1"",""policyName"":""免税""},{""venderOwnCode"":""ZZCC1"",""productCode"":"""",""productName"":""费用"",""rowType"":""2"",""spec"":"""",""unit"":"""",""quantity"":""1"",""unitPrice"":""50"",""amount"":""50"",""deductAmount"":"""",""taxRate"":""0"",""taxAmount"":""0"",""mxTotalAmount"":""50"",""taxRateMark"":""1"",""policyMark"":""1"",""policyName"":""免税""},{""venderOwnCode"":""ZZCC1"",""productCode"":"""",""productName"":""费用"",""rowType"":""1"",""spec"":"""",""unit"":"""",""quantity"":""1"",""unitPrice"":""-10"",""amount"":""-10"",""deductAmount"":"""",""taxRate"":""0"",""taxAmount"":""0"",""mxTotalAmount"":""-10"",""taxRateMark"":""1"",""policyMark"":""1"",""policyName"":""免税""},{""venderOwnCode"":""ZZCC1"",""productCode"":"""",""productName"":""费用"",""rowType"":""2"",""spec"":"""",""unit"":"""",""quantity"":""1"",""unitPrice"":""500"",""amount"":""500"",""deductAmount"":"""",""taxRate"":""0"",""taxAmount"":""0"",""mxTotalAmount"":""500"",""taxRateMark"":""1"",""policyMark"":""1"",""policyName"":""免税""},{""venderOwnCode"":""ZZCC1"",""productCode"":"""",""productName"":""费用"",""rowType"":""1"",""spec"":"""",""unit"":"""",""quantity"":""1"",""unitPrice"":""-100"",""amount"":""-100"",""deductAmount"":"""",""taxRate"":""0"",""taxAmount"":""0"",""mxTotalAmount"":""-100"",""taxRateMark"":""1"",""policyMark"":""1"",""policyName"":""免税""}]}}}";

		string SampleInvalidJson => @"{""Name"":""Jhon Smith"", ""Age"":20}";

		string SampleInvalidJsonForChina => @"{""reqType"":""02999"",""taxNo"":""12345678901234567890"",""autoexec"":""0""}";

		string SampleNotParsableAsJson => @"This is not JSON";

		JSchema Schema => JsonSchemaLoader.Load("Enterprise.Accounting.ElectronicMessaging.TaxCore.EInvoice.TaxCoreEInvoiceSchema.json");

		Json.Schema.JsonSchema SchemaFromChina
			=> Json.Schema.JsonSchema.FromText(
				typeof(JsonHelper).Assembly.GetManifestResourceStream("Enterprise.Accounting.ElectronicMessaging.China.EInvoice.SendInvoice.ChinaEInvoiceSchema.json")
					.ConvertToUTF8StringAndCloseStream()
					.Replace("http://json-schema.org/draft-04/schema#", "http://json-schema.org/draft-07/schema#"),
				new System.Text.Json.JsonSerializerOptions() { ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip }
			);
	}
}
