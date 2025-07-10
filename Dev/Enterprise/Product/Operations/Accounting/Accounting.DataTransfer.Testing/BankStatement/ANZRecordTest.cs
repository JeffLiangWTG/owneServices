using System;
using Enterprise.DataTransfer.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.BankStatement.Testing
{
	public class ANZRecordTest : TestCase
	{
		[ExpectException(typeof(FormatException))]
		public void TestConstructor_WrongFieldsCount()
		{
			ANZRecord record = new ANZRecord("3,111111,\"01-0102-0787326-00\",1263.56,000000000000,062,\"DC\",\"PH0800863863\",\"REFER: 07060\"", new CsvFlatFileFormat(true));
		}

		public void TestConstructor_SupportedType()
		{
			CsvFlatFileFormat format = new CsvFlatFileFormat(true);

			ANZRecord record = new ANZRecord("3,111111,01-0102-0787326-00,1263.56,000000000000,062,DC,PH0800863863,REFER: 07060,10098431179,Australia Post,01/06/07", format);
			AssertEquals("Type", ZArchitecture.Core.ReceiptTypes.DirectCredit, record.Type);
			AssertEquals("Reference", "10098431179", record.Reference);
			AssertEquals("Amount", -1263.56m, record.Amount);

			record = new ANZRecord("3,111111,\"01-0102-0787326-00\",1263.56,000000000000,062,\"DC\",\"PH0800863863\",\"REFER: 07060\",\"10098431179\",\"Australia Post\",\"01/06/07\"", format);
			AssertEquals("Type", ZArchitecture.Core.ReceiptTypes.DirectCredit, record.Type);
			AssertEquals("Reference", "10098431179", record.Reference);
			AssertEquals("Amount", -1263.56m, record.Amount);

			record = new ANZRecord("3,111111,\"01-0102-0787326-00\",-1263.56,000000000000,062,\"DC\",\"PH0800863863\",\"REFER: 07060\",\"10098431179\",\"Australia Post\",\"01/06/07\"", format);
			AssertEquals("Type", ZArchitecture.Core.ReceiptTypes.DirectDebit, record.Type);
			AssertEquals("Reference", "10098431179", record.Reference);
			AssertEquals("Amount", 1263.56m, record.Amount);

			record = new ANZRecord("3,111111,\"01-0102-0787326-00\",1263.56,000000000000,062,\"AP\",\"PH0800863863\",\"REFER: 07060\",\"10098431179\",\"Australia Post\",\"01/06/07\"", format);
			AssertEquals("Type", ZArchitecture.Core.ReceiptTypes.EFT, record.Type);
			AssertEquals("Reference", "10098431179", record.Reference);
			AssertEquals("Amount", -1263.56m, record.Amount);

			record = new ANZRecord("3,111111,\"01-0102-0787326-00\",-1263.56,000000000000,062,\"AP\",\"PH0800863863\",\"REFER: 07060\",\"10098431179\",\"Australia Post\",\"01/06/07\"", format);
			AssertEquals("Type", ZArchitecture.Core.ReceiptTypes.EFT, record.Type);
			AssertEquals("Reference", "10098431179", record.Reference);
			AssertEquals("Amount", 1263.56m, record.Amount);

			record = new ANZRecord("3,111111,\"01-0102-0787326-00\",1263.56,000000000000,062,\"\",\"PH0800863863\",\"REFER: 07060\",\"10098431179\",\"Australia Post\",\"01/06/07\"", format);
			AssertEquals("Type", ZArchitecture.Core.TransactionTypes.ReceiptBatch, record.Type);
			AssertEquals("Reference", "10098431179", record.Reference);
			AssertEquals("Amount", -1263.56m, record.Amount);

			record = new ANZRecord("3,111111,\"01-0102-0787326-00\",-1263.56,000000000000,062,\"  \",\"PH0800863863\",\"REFER: 07060\",\"10098431179\",\"Australia Post\",\"01/06/07\"", format);
			AssertEquals("Type", ZArchitecture.Core.ReceiptTypes.Cheque, record.Type);
			AssertEquals("Reference", "10098431179", record.Reference);
			AssertEquals("Amount", 1263.56m, record.Amount);
		}

		public void TestConstructor_NotSupportedType()
		{
			CsvFlatFileFormat format = new CsvFlatFileFormat(true);

			ANZRecord record = new ANZRecord("5,111111,01-0102-0787326-00,1263.56,000000000000,062,DC,PH0800863863,REFER: 07060,10098431179,Australia Post,01/06/07", format);
			Assert("It shoud be not supported line", record.IsNotSupported);
			AssertEquals("Type", "", record.Type);
			AssertEquals("Reference", "", record.Reference);
			AssertEquals("Amount", 0m, record.Amount);

			record = new ANZRecord("3,111111,\"01-0102-0787326-00\",1263.56,000000000000,062,\"ABC\",\"PH0800863863\",\"REFER: 07060\",\"10098431179\",\"Australia Post\",\"01/06/07\"", format);
			AssertEquals("Type", ZArchitecture.Core.TransactionTypes.ReceiptBatch, record.Type);
			AssertEquals("Reference", "10098431179", record.Reference);
			AssertEquals("Amount", -1263.56m, record.Amount);

			record = new ANZRecord(",,,,,,,,,", format);
			Assert("It shoud be not supported line", record.IsNotSupported);

			record = new ANZRecord("3,,,,,,,,,", format);
			AssertEquals("Type", ZArchitecture.Core.ReceiptTypes.Cheque, record.Type);
			AssertEquals("Reference", "", record.Reference);
			AssertEquals("Amount", 0m, record.Amount);
		}
	}
}
