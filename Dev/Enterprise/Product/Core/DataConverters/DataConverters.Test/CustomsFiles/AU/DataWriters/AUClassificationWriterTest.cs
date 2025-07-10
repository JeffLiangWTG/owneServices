using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataConverters.Testing.DataWriters;
using NUnit.Framework;
using ClassificationWriter = Enterprise.DataConverters.CustomsFiles.AU.ClassificationWriter;

namespace Enterprise.DataConverters.Testing.CustomsFiles.AU.DataWriters
{
	[TestedType(typeof(Classification))]
	sealed internal class AUClassificationWriterTest : ClassificationWriterTestBase
	{
		protected override DataWriter GetNewDataWriter()
		{
			return new ClassificationWriter(Factory);
		}

		public void TestLine()
		{
			var expectedLine = new ZString("UNIQUECODE,IMP,Hopefully a Unique Lookup Code,0000.00.00.00,111,MD1,0001");

			var writer = (ClassificationWriter)GetNewDataWriter();
			FillInRecordWithUniqueAndCompleteDetails(writer);
			AssertEquals(expectedLine, writer.CSVOutputLine);
		}

		protected override void FillInRecordWithUniqueAndCompleteDetails(DataWriter writer)
		{
			var classification = writer as ClassificationWriter;

			classification.LookupCode = "UNIQUECODE";
			classification.Description = "Hopefully a Unique Lookup Code";
			classification.TariffCode = "0000.00.00.00";
			classification.Treatment = "111";
			classification.ClassificationType = "IMP";
			classification.InstrumentType = "MD1";
			classification.InstrumentCode = "0001";
			classification.Preference = "A";
			classification.Origin = "AU";
		}

		protected override void AssertCountrySpecificData(DataConverters.CustomsFiles.ClassificationWriter classification, Customs.Business.BaseCusClassification bizO)
		{
			var newRecord = (Classification)bizO;
			AssertEquals("0000.00.00 00", newRecord.CC_TariffNum);
			AssertEquals("IMP", newRecord.CC_ClassificationType);
			Assert(newRecord.CC_AddInfo.Contains("InstrumentType_Hidden=MD1"));
			Assert(newRecord.CC_AddInfo.Contains("TreatmentCode_Hidden=111"));
			Assert(newRecord.CC_AddInfo.Contains("PRF=A"));
			Assert(newRecord.CC_AddInfo.Contains("InstrumentCode_Hidden=0001"));
			Assert(newRecord.CC_AddInfo.Contains("ORG=AU"));
		}

		public void TestGetAddInfoString()
		{
			var writer = (ClassificationWriter)GetNewDataWriter();
			AssertEquals(ZString.Empty, writer.GetAddInfoString());
			writer.InstrumentCode = "0001";
			AssertEquals("InstrumentCode_Hidden=0001", writer.GetAddInfoString());
			writer.InstrumentType = "MD1";
			AssertEquals("InstrumentCode_Hidden=0001*InstrumentType_Hidden=MD1", writer.GetAddInfoString());
			writer.Treatment = "111";
			AssertEquals("InstrumentCode_Hidden=0001*InstrumentType_Hidden=MD1*TreatmentCode_Hidden=111", writer.GetAddInfoString());
			writer.Preference = "A";
			AssertEquals("InstrumentCode_Hidden=0001*InstrumentType_Hidden=MD1*TreatmentCode_Hidden=111*PRF=A", writer.GetAddInfoString());
			writer.Origin = "AU";
			AssertEquals(ExpectedAddInfo, writer.GetAddInfoString());
		}

		const string ExpectedAddInfo = "InstrumentCode_Hidden=0001*InstrumentType_Hidden=MD1*TreatmentCode_Hidden=111*PRF=A*ORG=AU";
	}
}
