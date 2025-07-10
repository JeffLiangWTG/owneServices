using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat
{
	public class RecordLineTest : TestCase
	{
		public void TestValue()
		{
			AssertEquals(RecordLineValue, RecordLine.Value);
		}

		public void TestRecordKey()
		{
			AssertEquals(RecordLineValue.Substring(0, RecordLine.Constants.RecordKeyLength), RecordLine.RecordKey);
		}

		public void TestOriginCountry()
		{
			AssertEquals("US", RecordLine.OriginCountry);
		}

		public void TestOriginPort()
		{
			AssertEquals("2795", RecordLine.OriginPort);
		}

		public void TestDestinationCountry()
		{
			AssertEquals("AU", RecordLine.DestinationCountry);
		}

		public void TestDestinationPort()
		{
			AssertEquals("9639", RecordLine.DestinationPort);
		}

		public void TestDutyType()
		{
			AssertEquals(DutyTypeCodeDescriptionPairList.Codes.GCC, RecordLine.DutyType);
			RecordLine = new RecordLine("US2795AU963900062440610000001   L09576XNZGX7100000SomeOtherInformation");
			AssertEquals(DutyTypeCodeDescriptionPairList.Codes.LowValue, RecordLine.DutyType);
			RecordLine = new RecordLine("US2795AU963900062440610000001   N09576XNZGX7100000SomeOtherInformation");
			AssertEquals(DutyTypeCodeDescriptionPairList.Codes.NonDutiable, RecordLine.DutyType);
			RecordLine = new RecordLine("US2795AU963900062440610000001   D09576XNZGX7100000SomeOtherInformation");
			AssertEquals(DutyTypeCodeDescriptionPairList.Codes.Dutiable, RecordLine.DutyType);
		}

		public void TestUPSStringToZDateTime()
		{
			AssertEquals(ZDateTime.Empty, RecordLine.ToZDateTime(""));
			AssertEquals(ZDateTime.Empty, RecordLine.ToZDateTime("    "));
			AssertEquals(new ZDateTime(2005, 6, 23), RecordLine.ToZDateTime("23JUN2005"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			RecordLine = new RecordLine(RecordLineValue);
		}

		RecordLine RecordLine;
		const string RecordLineValue = "US2795AU963900062440610000001   C09576XNZGX7100000SomeOtherInformation";
	}
}
