using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AQISProducerCodeValidationTest : AQISSingleValueValidationTest
	{
		public override void TestCodeAgainstLookupList()
		{
			Assert("No list to test", true);
		}

		public void TestDuplicateAQISProducerCodes()
		{
			TestCaseHelper.ClearTable(CMRAqisProducer.Schema.TableName);
			CMRAqisProducer cMRProducer1 = CMRAqisProducer.New(Factory);
			cMRProducer1.QR_AQISProducerCode = "1";
			cMRProducer1.QR_AQISProducerName = "Name1";

			CMRAqisProducer cMRProducer2 = CMRAqisProducer.New(Factory);
			cMRProducer2.QR_AQISProducerCode = "2";
			cMRProducer2.QR_AQISProducerName = "Name2";

			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			AQISProducerCode producer1 = declaration.AQISProducerCodes.AddNew();
			producer1.Code = "1";
			AssertNoMessageErrors("No Message Error", producer1.CodeInfo);

			AQISProducerCode producer2 = declaration.AQISProducerCodes.AddNew();
			producer2.Code = "1";
			AssertHasMessageErrors("Has Message Error", producer2.CodeInfo);

			producer2.Code = "2";
			AssertNoMessageErrors("No Message Error", producer2.CodeInfo);
		}

		public override void TestNumberOfCodesEntered()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AQISProducerCode producerCode1 = invoiceLine.AQISProducerCodes.AddNew();
			producerCode1.Code = "1";
			AQISProducerCode producerCode2 = invoiceLine.AQISProducerCodes.AddNew();
			producerCode2.Code = "2";
			AQISProducerCode producerCode3 = invoiceLine.AQISProducerCodes.AddNew();
			producerCode3.Code = "3";
			AQISProducerCode producerCode4 = invoiceLine.AQISProducerCodes.AddNew();
			producerCode4.Code = "4";
			AQISProducerCode producerCode5 = invoiceLine.AQISProducerCodes.AddNew();
			producerCode5.Code = "5";
			AQISProducerCode producerCode6 = invoiceLine.AQISProducerCodes.AddNew();
			producerCode6.Code = "6";
			AQISProducerCode producerCode7 = invoiceLine.AQISProducerCodes.AddNew();
			producerCode7.Code = "7";
			AQISProducerCode producerCode8 = invoiceLine.AQISProducerCodes.AddNew();
			producerCode8.Code = "8";
			AQISProducerCode producerCode9 = invoiceLine.AQISProducerCodes.AddNew();
			producerCode9.Code = "9";
			AQISProducerCode producerCode10 = invoiceLine.AQISProducerCodes.AddNew();
			producerCode10.Code = "10";
			producerCode10.Validation.ValidateAll();
			AssertNoErrors("Producer Code 1", producerCode1.CodeInfo);
			AssertNoErrors("Producer Code 2", producerCode2.CodeInfo);
			AssertNoErrors("Producer Code 3", producerCode3.CodeInfo);
			AssertNoErrors("Producer Code 4", producerCode4.CodeInfo);
			AssertNoErrors("Producer Code 5", producerCode5.CodeInfo);
			AssertNoErrors("Producer Code 6", producerCode6.CodeInfo);
			AssertNoErrors("Producer Code 7", producerCode7.CodeInfo);
			AssertNoErrors("Producer Code 8", producerCode8.CodeInfo);
			AssertNoErrors("Producer Code 9", producerCode9.CodeInfo);
			AssertNoErrors("Producer Code 10", producerCode10.CodeInfo);

			AQISProducerCode producerCode11 = invoiceLine.AQISProducerCodes.AddNew();
			producerCode11.Code = "11";
			AssertNoErrors("Producer Code 1", producerCode1.CodeInfo);
			AssertNoErrors("Producer Code 2", producerCode2.CodeInfo);
			AssertNoErrors("Producer Code 3", producerCode3.CodeInfo);
			AssertNoErrors("Producer Code 4", producerCode4.CodeInfo);
			AssertNoErrors("Producer Code 5", producerCode5.CodeInfo);
			AssertNoErrors("Producer Code 6", producerCode6.CodeInfo);
			AssertNoErrors("Producer Code 7", producerCode7.CodeInfo);
			AssertNoErrors("Producer Code 8", producerCode8.CodeInfo);
			AssertNoErrors("Producer Code 9", producerCode9.CodeInfo);
			AssertNoErrors("Producer Code 10", producerCode10.CodeInfo);
			AssertHasErrors("Producer Code 11", producerCode11.CodeInfo);

			invoiceLine.AQISProducerCodes.RemoveAndDelete(producerCode2);
			producerCode11.Validation.ValidateCode();
			AssertNoErrors("Producer Code 1", producerCode1.CodeInfo);
			AssertNoErrors("Producer Code 3", producerCode3.CodeInfo);
			AssertNoErrors("Producer Code 4", producerCode4.CodeInfo);
			AssertNoErrors("Producer Code 5", producerCode5.CodeInfo);
			AssertNoErrors("Producer Code 6", producerCode6.CodeInfo);
			AssertNoErrors("Producer Code 7", producerCode7.CodeInfo);
			AssertNoErrors("Producer Code 8", producerCode8.CodeInfo);
			AssertNoErrors("Producer Code 9", producerCode9.CodeInfo);
			AssertNoErrors("Producer Code 10", producerCode10.CodeInfo);
			AssertNoErrors("Producer Code 11", producerCode11.CodeInfo);
		}

		AQISProducerCode aqisProducerCode;
		public override AQISSingleValueBusinessObject BizObjToTest => aqisProducerCode ?? (aqisProducerCode = new AQISProducerCode(Factory));

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(CMRAqisProducer.Schema.TableName);
		}
	}
}
