using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;

namespace Enterprise.Client.TGE.PMS.Testing
{
	public class TGEFlatFileFormatTest : TabDelimitedFlatFileFormatTest
	{
		public void TestFileExtensionType()
		{
			TGEFlatFileFormat flatFileFormat = new TGEFlatFileFormat();
			AssertEquals("File Extension Type Should be a Text File", FileExtensionType.Txt, flatFileFormat.FileExtensionForImport);
		}

		public override void TestConvertToRow()
		{
			Record = TestFlatFileFormat.ConvertToRow(LineRow1) as ConsolAndShipmentRecord;
			AssertNotNull("Data Type should be ConsolAndShipmentRecord", Record);
			AssertRetrieveString(Record);
			Record = (ConsolAndShipmentRecord)TestFlatFileFormat.ConvertToRow(LineRow2);
			AssertEquals("Number of Fields on the Record", Record.FieldCount, NumberOfFields);
			AssertExtraFieldsAreEmpty(Record, 8);
		}

		#region Implementation
		void AssertExtraFieldsAreEmpty(ConsolAndShipmentRecord record, int startField)
		{
			for (int i = startField; i < record.FieldCount; i++)
			{
				AssertEquals(string.Format("Field {0}", i.ToString()), "", record[i]);
			}
		}

		void AssertRetrieveString(ConsolAndShipmentRecord record)
		{
			ZString dataRow = LineRow1;
			ZString[] fieldsFromDataRow = dataRow.Split('\t');
			AssertEquals("No. of fields", fieldsFromDataRow.Length, record.FieldCount);
			for (int i = 0; i < NumberOfFields; i++)
			{
				AssertEquals(string.Format("Field {0}", i.ToString()), fieldsFromDataRow[i], record[i]);
			}
		}

		TGEFlatFileFormat TestFlatFileFormat;
		ZString LineRow1;
		ZString LineRow2;
		ConsolAndShipmentRecord Record;
		const int NumberOfFields = 74;
		protected override void SetUp()
		{
			base.SetUp();
			LineRow1 = "13\tConsolDate\tAgent2Agent\tLOOSE\tAU\tSYD\tNZ\tAKL\t20050325121200\t20050325000000\t20050325000000\tConsolAgentCode\tbkref1\tQF\t123\tAKL\t8112349301\tNZ\tAKL\t13qantas\tFrtCreditorCode\tDepotCode\tAUD\t63761\tAC\tWIG GALLERY\t73-77 SACKVILLE ST\t\tCOLLINGWOOD\t3066\t03 9419 8266\tABN1\tmisc\tCONSIGN-1\tCONSIGN ADDR1-1\tCONSIGN ADDR2-1\tCONSIGN ADDR3-1\tPCODE-1\tPHONE-1\tNZAKL\t63761\t12micon\tSTD\tDM\tQTE1\tY\tOR1\t7712349301\tNZ\tAKL\tJP\tTYO\t20050325\t1\tPK\tSTD\tFOB\tFRA\t1000\tAUD\tGoods1\t1.1\t0.001\t2\t0\tPrepaid\tAUD\t10EXLV\tFD69N\tOT\t20050323\tTCC\tCIL\t20050323";
			LineRow2 = "13\tConsolDate\tAgent2Agent\tLOOSE\tAU\tSYD\tNZ\tAKL";
			TestFlatFileFormat = new TGEFlatFileFormat();
		}
		#endregion
	}
}
