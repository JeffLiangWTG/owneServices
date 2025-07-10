using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.ReportingBook.AlternateGLAccountWithAttribute.Testing
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class AlternateGLAccountWithAttributeCSVFlatFileConverterTest : TestCaseWithFactory
	{
		public void TestImportCorrectRowInFile()
		{
			using (var reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\ReportingBook\AlternateGLAccountWithAttribute\Testing\ValidData.csv"))
			{
				Converter.ImportFlatFile(Value, new CsvFlatFileFormat(false), reader);
			}

			Assert(!Buffer.HasErrors);

			var alternateGLAccount = Value.AlternateGLAccount;
			AssertEquals(1, alternateGLAccount.Count);

			AssertEquals("111", alternateGLAccount[0].ChartCode.ToString());
			AssertEquals("P&L", alternateGLAccount[0].AccountType.ToString());
			AssertEquals("3410.13.21", alternateGLAccount[0].ParentAccount);
			AssertEquals("320110", alternateGLAccount[0].AccountNum.ToString());
			AssertEquals("测试account", alternateGLAccount[0].AccountName.ToString());
			AssertEquals("DR", alternateGLAccount[0].DebitCredit.ToString());
			AssertEquals("AS", alternateGLAccount[0].ReportSection.ToString());

			AssertEquals("111", alternateGLAccount[0].PercentNum.ToString());
			AssertEquals("222", alternateGLAccount[0].ConsolidationNum.ToString());
			AssertEquals("333", alternateGLAccount[0].AlternateNum.ToString());
			AssertEquals("444", alternateGLAccount[0].TotalReference.ToString());
			AssertEquals(0, alternateGLAccount[0].TotalLevel);
			AssertEquals(0, alternateGLAccount[0].PrintSequence);
			AssertEquals("ORG", alternateGLAccount[0].ORGAttrValue);
			AssertEquals("OCG", alternateGLAccount[0].OCGAttrValue);
			AssertEquals("LFO", alternateGLAccount[0].LFOAttrValue);
			AssertEquals("LFE", alternateGLAccount[0].LFEAttrValue);
			AssertEquals("TIC", alternateGLAccount[0].TICAttrValue);
			AssertEquals("SPR", alternateGLAccount[0].SPRAttrValue);
		}

		public void TestImport_InvalidRecordType()
		{
			Assert(!Buffer.HasErrors);
			using (var reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\ReportingBook\AlternateGLAccountWithAttribute\Testing\InvalidRecordTypeData.csv"))
			{
				Converter.ImportFlatFile(Value, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals(true, Buffer.HasErrors);
			AssertContains("A valid Record Type must be 'AGACCOUNT'", Buffer.AsString);
			AssertEquals(0, Value.AlternateGLAccount.Count);
		}

		public void TestImport_InvalidTotalLevel()
		{
			Assert(!Buffer.HasErrors);

			using (var reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\ReportingBook\AlternateGLAccountWithAttribute\Testing\InvalidTotalLevelData.csv"))
			{
				Converter.ImportFlatFile(Value, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals(true, Buffer.HasErrors);
			AssertContains("Invalid Total Level.", Buffer.AsString);
			AssertEquals(1, Value.AlternateGLAccount.Count);
		}

		public void TestImport_InvalidPrintSequence()
		{
			Assert(!Buffer.HasErrors);

			using (var reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\ReportingBook\AlternateGLAccountWithAttribute\Testing\InvalidPrintSequenceData.csv"))
			{
				Converter.ImportFlatFile(Value, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals(true, Buffer.HasErrors);
			AssertContains("Invalid Print Sequence.", Buffer.AsString);
			AssertEquals(1, Value.AlternateGLAccount.Count);
		}

		public void TestImport_EmptyFile()
		{
			Assert(!Buffer.HasErrors);

			using (var reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\ReportingBook\AlternateGLAccountWithAttribute\Testing\EmptyFile.csv"))
			{
				Converter.ImportFlatFile(Value, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals(true, Buffer.HasErrors);
			AssertContains("The file does not contain any record.", Buffer.AsString);
			AssertEquals(0, Value.AlternateGLAccount.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Buffer = new NotificationBuffer();
			Converter = new AlternateGLAccountWithAttributeCSVFlatFileConverter(Buffer, Factory);
			Value = new Xsd.AlternateGLAccounts();
		}
		NotificationBuffer Buffer;
		AlternateGLAccountWithAttributeCSVFlatFileConverter Converter;
		Xsd.AlternateGLAccounts Value;
	}
}
