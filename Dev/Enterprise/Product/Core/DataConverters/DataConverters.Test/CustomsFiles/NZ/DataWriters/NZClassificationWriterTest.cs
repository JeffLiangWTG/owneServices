using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.DataConverters.Testing.DataWriters;
using NUnit.Framework;
using ClassificationWriter = Enterprise.DataConverters.CustomsFiles.ClassificationWriter;

namespace Enterprise.DataConverters.Testing.CustomsFiles.NZ.DataWriters
{
	[TestedType(typeof(CusClassification))]
	sealed internal class NZClassificationWriterTest : ClassificationWriterTestBase
	{
		protected override void AssertCountrySpecificData(ClassificationWriter baseClassification, Customs.Business.BaseCusClassification bizO)
		{
			var newRecord = (CusClassification)bizO;
			var classification = (DataConverters.CustomsFiles.NZ.ClassificationWriter)baseClassification;
			AssertEquals(classification.PartsOfTariffCode, newRecord.CC_PartsOfClassification);
			AssertEquals(classification.ConcessionCode, newRecord.CC_ConcessionCode);
			AssertEquals(Customs.Common.ClassificationType.Both, newRecord.CC_ClassificationType);

			AssertEquals(3, newRecord.PermitCodes.Count);
			AssertEquals(3, newRecord.ProhibitedCodes.Count);
			AssertEquals(3, newRecord.OtherInfos.Count);

			var permitCodeCodes = newRecord.PermitCodes.AggregatedCodes;
			var permitCodeDatas = newRecord.PermitCodes.AggregatedDatas;
			Assert(permitCodeCodes.Contains(classification.PermitCode1));
			Assert(permitCodeCodes.Contains(classification.PermitCode2));
			Assert(permitCodeCodes.Contains(classification.PermitCode3));
			Assert(permitCodeDatas.Contains(classification.PermitNumber1));
			Assert(permitCodeDatas.Contains(classification.PermitNumber2));
			Assert(permitCodeDatas.Contains(classification.PermitNumber3));

			var prohibitedCodes = newRecord.ProhibitedCodes.AggregatedCodes;
			Assert(prohibitedCodes.Contains(classification.ProhibitedCode1));
			Assert(prohibitedCodes.Contains(classification.ProhibitedCode2));
			Assert(prohibitedCodes.Contains(classification.ProhibitedCode3));

			var otherInfoCodes = newRecord.OtherInfos.AggregatedCodes;
			var otherInfoDatas = newRecord.OtherInfos.AggregatedDatas;
			Assert(otherInfoCodes.Contains(classification.OtherInfoCode1));
			Assert(otherInfoCodes.Contains(classification.OtherInfoCode2));
			Assert(otherInfoCodes.Contains(classification.OtherInfoCode3));
			Assert(otherInfoDatas.Contains(classification.OtherInfoNumber1));
			Assert(otherInfoDatas.Contains(classification.OtherInfoNumber2));
			Assert(otherInfoDatas.Contains(classification.OtherInfoNumber3));
		}

		protected override void FillInRecordWithUniqueAndCompleteDetails(DataWriter writer)
		{
			var classification = writer as DataConverters.CustomsFiles.NZ.ClassificationWriter;

			classification.LookupCode = "UNIQUECODE";
			classification.Description = "Hopefully a Unique Lookup Code";
			classification.TariffCode = "0000.00.00.00Z";
			classification.PartsOfTariffCode = "0000.00.00.00Z";
			classification.ClassificationType = Customs.Common.ClassificationType.Both;
			classification.ConcessionCode = "123456Z";

			classification.PermitCode1 = "AAA";
			classification.PermitNumber1 = "111";
			classification.PermitCode2 = "BBB";
			classification.PermitNumber2 = "222";
			classification.PermitCode3 = "CCC";
			classification.PermitNumber3 = "333";

			classification.ProhibitedCode1 = "DDD";
			classification.ProhibitedCode2 = "EEE";
			classification.ProhibitedCode3 = "FFF";

			classification.OtherInfoCode1 = "GGG";
			classification.OtherInfoNumber1 = "444";
			classification.OtherInfoCode2 = "HHH";
			classification.OtherInfoNumber2 = "555";
			classification.OtherInfoCode3 = "III";
			classification.OtherInfoNumber3 = "666";
		}

		protected override DataWriter GetNewDataWriter()
		{
			return new DataConverters.CustomsFiles.NZ.ClassificationWriter(Factory);
		}
	}
}
