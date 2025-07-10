using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(ExportClassificationCollection))]
	sealed class ExportClassificationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			var collection = new ExportClassificationCollection(Factory);
			var classification = collection.AddNew();
			AssertNotNull("classification", classification);
			AssertEquals("CC_ClassificationType", CusClassification.ClassificationType.EXP, classification.CC_ClassificationType);
		}

		public void TestLoadWithRelationshipfilter()
		{
			var classInThisCountry = Factory.New<CusClassification>();
			classInThisCountry.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			classInThisCountry.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var classInOtherCountry = Factory.New<CusClassification>();
			classInOtherCountry.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			classInOtherCountry.CC_RN_NKCountryCode = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).Code;

			var htsClassInThisCountry = Factory.New<CusClassification>();
			htsClassInThisCountry.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			htsClassInThisCountry.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var collection = new ExportClassificationCollection(Factory);

			var filter = new ZQuery();
			filter.FetchOnlyFromLocalCache = true;
			collection.Load(filter);
			AssertEquals("Should load records made in this country, EXP type", 1, collection.Count);
			AssertEquals("Should load records made in this country, EXP type", CusClassification.ClassificationType.EXP, collection[0].CC_ClassificationType);
			AssertEquals("Should load records made in this country, EXP type", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, collection[0].CC_RN_NKCountryCode);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ExportClassificationCollection(Factory);
		}
	}
}
