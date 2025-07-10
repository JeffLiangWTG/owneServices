using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(HTSClassificationCollection))]
	sealed class HTSClassificationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			var collection = new HTSClassificationCollection(Factory);
			var classification = collection.AddNew();
			AssertNotNull("classification", classification);
			AssertEquals("CC_ClassificationType", CusClassification.ClassificationType.IMP, classification.CC_ClassificationType);
		}

		public void TestLoadWithRelationshipfilter()
		{
			var classInThisCountry = Factory.New<CusClassification>();
			classInThisCountry.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classInThisCountry.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var classInOtherCountry = Factory.New<CusClassification>();
			classInOtherCountry.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classInOtherCountry.CC_RN_NKCountryCode = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).Code;

			var exportClassInThisCountry = Factory.New<CusClassification>();
			exportClassInThisCountry.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			exportClassInThisCountry.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var collection = new HTSClassificationCollection(Factory);

			var filter = new ZQuery();
			filter.FetchOnlyFromLocalCache = true;
			collection.Load(filter);
			AssertEquals("Should load records made in this country, IMP type", 1, collection.Count);
			AssertEquals("Should load records made in this country, IMP type", CusClassification.ClassificationType.IMP, collection[0].CC_ClassificationType);
			AssertEquals("Should load records made in this country, IMP type", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, collection[0].CC_RN_NKCountryCode);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new HTSClassificationCollection(Factory);
		}
	}
}
