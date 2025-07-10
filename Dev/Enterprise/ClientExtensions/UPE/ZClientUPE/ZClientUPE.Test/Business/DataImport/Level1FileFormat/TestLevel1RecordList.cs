using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat.Testing
{
	public class TestLevel1RecordList : TestCase
	{
		[ExpectException(typeof(Level1FileReadException))]
		public void TestThereShouldOnlyBeOneRecordType100000_ReadException()
		{
			string record600000 = "11111111111111111111111111111111111111111111600000k";
			string record100000 = "11111111111111111111111111111111111111111111100000k";
			Level1RecordList records = new Level1RecordList();
			records.RecordLines.Add(record100000);
			records.RecordLines.Add(record600000);
			records.RecordLines.Add(record100000);
			int deano = records.Count;
		}

		public void TestCount()
		{
			Level1RecordList records = new Level1RecordList();
			AddRecord(records, '1');
			AddRecord(records, '2');
			AssertEquals(2, records.Count);
			AddRecord(records, '3');
			AssertEquals(3, records.Count);
			records.RecordLines.Add("            000000               W3188389763200000W3188389763                                                                                                                                                                                                                                                                                        B2015510377                  N       ");
			records.RecordLines.Add("            000000               W3188389763202000W3188389763                                                                                                                                   AU09639  S1AU9639T3.076B2005-03-16                              Y                                                                             AUDNNNNNBI                                  ");
			AssertEquals(3, records.Count);
		}

		public void TestEnumeration()
		{
			Level1RecordList records = new Level1RecordList();
			AddRecord(records, '1');
			AddRecord(records, '2');
			int iterationCount = 0;
			foreach (Level1Record record in records)
			{
				AssertEquals("b", record._200000.Value.SubstringSafe(50, 1));
				AssertEquals("d", record._202000.Value.Substring(50, 1));
				AssertEquals("e", record._300000.Value.Substring(50, 1));
				AssertEquals("f", record._400000.Value.Substring(50, 1));
				AssertEquals("g", record._401000.Value.Substring(50, 1));
				AssertEquals("h", record._500000Lines[0].Value.Substring(50, 1));
				AssertEquals("i", record._500000Lines[1].Value.Substring(50, 1));
				AssertEquals("j", record._600000Lines[0].Value.Substring(50, 1));
				AssertEquals("k", record._600000Lines[1].Value.Substring(50, 1));
				iterationCount++;
			}

			AssertEquals(2, iterationCount);
		}

		public void TestEnumeration_ReadException()
		{
			Level1RecordList records = new Level1RecordList();
			AddRecord(records, '1');
			records.RecordLines.Add("Junk kjhads khdf kjh kjh kjh kjlh kljh lkjh kjh kljh kljhasdkljfh kljdhf akljhdf kljashdf kljashdflkjashdf lkjasdh");
			try
			{
				foreach (Level1Record record in records)
				{
				}
			}
			catch (Level1FileReadException e)
			{
				AssertEquals(10, e.LineNumber);
				AssertEquals("Junk kjhads khdf kjh kjh kjh kjlh kljh lkjh kjh kljh kljhasdkljfh kljdhf akljhdf kljashdf kljashdflkjashdf lkjasdh", e.LineValue);
				AssertEquals("11111111111111111111111111111111111111111111600001k", e.PreviousLineValue);
				AssertNotNull(e.InnerException);
			}
		}

		void AddRecord(Level1RecordList records, char key)
		{
			records.RecordLines.Add("1".PadLeft(RecordLine.Constants.RecordKeyLength, key) + RecordLine.Constants.RecordTypes._100000 + "a");
			records.RecordLines.Add("2".PadLeft(RecordLine.Constants.RecordKeyLength, key) + RecordLine.Constants.RecordTypes._200000 + "b");
			records.RecordLines.Add("4".PadLeft(RecordLine.Constants.RecordKeyLength, key) + RecordLine.Constants.RecordTypes._202000 + "d");
			records.RecordLines.Add("5".PadLeft(RecordLine.Constants.RecordKeyLength, key) + RecordLine.Constants.RecordTypes._300000 + "e");
			records.RecordLines.Add("6".PadLeft(RecordLine.Constants.RecordKeyLength, key) + RecordLine.Constants.RecordTypes._400000 + "f");
			records.RecordLines.Add("7".PadLeft(RecordLine.Constants.RecordKeyLength, key) + RecordLine.Constants.RecordTypes._401000 + "g");
			records.RecordLines.Add("9".PadLeft(RecordLine.Constants.RecordKeyLength, key) + RecordLine.Constants.RecordTypes._500000 + "h");
			records.RecordLines.Add("9".PadLeft(RecordLine.Constants.RecordKeyLength, key) + "501000" + "i");
			records.RecordLines.Add("0".PadLeft(RecordLine.Constants.RecordKeyLength, key) + RecordLine.Constants.RecordTypes._600000 + "j");
			records.RecordLines.Add("1".PadLeft(RecordLine.Constants.RecordKeyLength, key) + "600001" + "k");
		}
	}
}
