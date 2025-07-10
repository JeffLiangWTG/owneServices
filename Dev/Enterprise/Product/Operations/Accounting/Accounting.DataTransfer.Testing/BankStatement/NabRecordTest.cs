using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.BankStatement.Testing
{
	public class NabRecordTest : TestCase
	{
		[ExpectException(typeof(FormatException))]
		public void TestConstructor_WrongFieldsCount()
		{
			NabRecord record = new NabRecord("", new CsvFlatFileFormat(false));
		}

		public void TestConstructor_SupportedType()
		{
			CsvFlatFileFormat format = new CsvFlatFileFormat(false);

			NabRecord record = new NabRecord("NATAAU3M,AUD,480687306,6/11/2006,475,CHEQUE,123463, ,-10336.7", format);
			AssertEquals("Currency", "AUD", record.Currency);
			AssertEquals("Date", new ZDateTime(2006, 11, 6), record.Date);
			AssertEquals("Type", ZArchitecture.Core.ReceiptTypes.Cheque, record.Type);
			AssertEquals("Reference", "123463", record.Reference);
			AssertEquals("Description", "CHEQUE", record.Description);
			AssertEquals("Amount", 10336.7m, record.Amount);

			record = new NabRecord("\"NATAAU3M\",\"AUD\",\"480687306\",\"6/11/2006\",\"475\",\"CHEQUE\",\"123463\",\" \",-10336.7", format);
			AssertEquals("Currency", "AUD", record.Currency);
			AssertEquals("Date", new ZDateTime(2006, 11, 6), record.Date);
			AssertEquals("Type", ZArchitecture.Core.ReceiptTypes.Cheque, record.Type);
			AssertEquals("Reference", "123463", record.Reference);
			AssertEquals("Description", "CHEQUE", record.Description);
			AssertEquals("Amount", 10336.7m, record.Amount);
		}

		public void TestConstructor_NotSupportedType()
		{
			CsvFlatFileFormat format = new CsvFlatFileFormat(false);

			NabRecord record = new NabRecord("NATAAU3M,AUD,480687306,6/11/2006,100,NOT SUPPORTED TYPE, ,BLAH,2541.3", format);
			AssertEquals("Currency", "AUD", record.Currency);
			AssertEquals("Date", new ZDateTime(2006, 11, 6), record.Date);
			AssertEquals("Type", "", record.Type);
			AssertEquals("Reference", "REF", record.Reference);
			AssertEquals("Description", "BLAH", record.Description);
			AssertEquals("Amount", -2541.3m, record.Amount);

			record = new NabRecord("\"NATAAU3M\",\"AUD\",\"480687306\",\"6/11/2006\",\"100\",\"NOT SUPPORTED TYPE\",\" \",\"BLAH\",2541.3", format);
			AssertEquals("Currency", "AUD", record.Currency);
			AssertEquals("Date", new ZDateTime(2006, 11, 6), record.Date);
			AssertEquals("Type", "", record.Type);
			AssertEquals("Reference", "REF", record.Reference);
			AssertEquals("Description", "BLAH", record.Description);
			AssertEquals("Amount", -2541.3m, record.Amount);
		}
	}
}
