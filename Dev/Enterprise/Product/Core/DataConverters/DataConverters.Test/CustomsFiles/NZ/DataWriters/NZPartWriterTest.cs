using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.DataConverters.CustomsFiles.NZ;
using Enterprise.DataConverters.Testing.DataWriters;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using GlbCompany = Enterprise.MasterFiles.Business.GlbCompany;

namespace Enterprise.DataConverters.Testing.CustomsFiles.NZ.DataWriters
{
	[TestedType(typeof(OrgSupplierPart))]
	sealed internal class NZPartWriterTest : PartWriterTestBase
	{
		protected override void AssertCountrySpecificData(DataConverters.CustomsFiles.PartWriter basePartWriter, MasterFiles.Business.OrgSupplierPart bizO)
		{
			var testNZPartWriter = (PartWriter)basePartWriter;
			var testPart = (OrgSupplierPart)bizO;
			var filter = new ZQuery(CusClassificationSchema.CC_LookupCode, testNZPartWriter.LookupCode);
			filter.AddToFilter(CusClassificationSchema.CC_ClassificationType, Customs.Common.ClassificationType.Both);
			filter.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var classification = Factory.LoadTop1<CusClassification>(filter);

			testNZPartWriter.UnitQuantityFactor = 1;

			var testUnit = testPart.PartUnits.AddNew();
			testUnit.OF_QuantityInParent = 1;
			testUnit.OF_PackType = testPart.OP_StockKeepingUnit;

			AssertEquals("QuantityInParent", testNZPartWriter.UnitQuantityFactor, testUnit.OF_QuantityInParent);
			AssertEquals("Package", testNZPartWriter.DefaultStockUnit, testUnit.OF_PackType);
		}

		protected override void AdditionalCountrySpecificSetup(DataConverters.CustomsFiles.PartWriter baseWriter)
		{
			var writer = (PartWriter)baseWriter;
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = writer.LookupCode;
			classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			classification.CC_ClassificationType = Customs.Common.ClassificationType.Both;
		}

		protected override DataWriter GetNewDataWriter()
		{
			return new PartWriter(Factory);
		}
	}
}
