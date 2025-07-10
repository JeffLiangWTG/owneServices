using System;
using CargoWise.Types;

namespace Enterprise.Client.TNT.Testing
{
	public class TBagRecordTest : IQDownBaseRecordTest
	{
		public override void TestFieldProperty()
		{
			TBagRecord record = new TBagRecord(DataString);
			AssertEquals("Record Type", "02", record.RecordType);
			AssertEquals("MBag Number", "B1927029  ", record.MBagNo);
			record.MBagNo = "MBag Number";
			AssertEquals("MBag Number", "MBag Number", record.MBagNo);
			AssertEquals("TBag Number", "TBAGNO    ", record.TBagNo);
			record.TBagNo = "TBag Number";
			AssertEquals("TBag Number", "TBag Number", record.TBagNo);
			AssertEquals("TBag Origin", "SIN", record.TBagOrigin);
			record.TBagOrigin = "TBag Origin";
			AssertEquals("TBag Origin", "TBag Origin", record.TBagOrigin);
			AssertEquals("TBag Destination", "SYD", record.TBagDestination);
			record.TBagDestination = "TBag Destination";
			AssertEquals("TBag Destination", "TBag Destination", record.TBagDestination);
			AssertEquals("Spaces", ZString.Replicate(' ', 461), record.spaces);
			AssertEquals("Record Delimiter", ".", record.RecordDelimiter);
		}

#region TestHumanReadable
		public override void TestHumanReadable()
		{
			TBagRecord record = new TBagRecord(DataString);
			AssertEquals("HumanReadable", GenerateExpectedHumanReadable(record.RecordType, record.MBagNo, record.TBagNo), record.HumanReadable);
			record.MBagNo = "MBAGNOTEST";
			record.TBagNo = "TBAGNOTEST";
			AssertEquals("HumanReadable", GenerateExpectedHumanReadable(record.RecordType, "MBAGNOTEST", "TBAGNOTEST"), record.HumanReadable);
		}

		ZString GenerateExpectedHumanReadable(ZString recordType, ZString mBagNo, ZString tBagNo)
		{
			return ZString.Format("Record {0} (MBagNo={1}, TBagNo={2})", recordType, mBagNo, tBagNo);
		}

#endregion
		protected override IQDownBaseRecord GetRecord(ZString rawData)
		{
			return new TBagRecord(rawData);
		}

		protected override ZString DataString
		{
			get
			{
				return "02B1927029  TBAGNO    SINSYD                                                                                                                                                                                                                                                                                                                                                                                                                                                                             .";
			}
		}

		protected override Type ExpectedRecordType
		{
			get
			{
				return typeof(TBagRecord);
			}
		}

		protected override int ExpectedFieldCount
		{
			get
			{
				return 7;
			}
		}
	}
}
