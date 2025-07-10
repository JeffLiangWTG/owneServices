using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ImportClassificationCollection))]
	sealed class ImportClassificationCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ImportClassificationCollection(Factory);
		}

		public void TestLoadWithRelationshipfilter()
		{
			Classification importClassInThisCountry = Factory.New<Classification>();
			importClassInThisCountry.CC_ClassificationType = Classification.ClassificationType.IMP;
			importClassInThisCountry.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			Classification importClassInOtherCountry = Factory.New<Classification>();
			importClassInOtherCountry.CC_ClassificationType = Classification.ClassificationType.IMP;
			importClassInOtherCountry.CC_RN_NKCountryCode = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).Code;

			Classification exportClassInThisCountry = Factory.New<Classification>();
			exportClassInThisCountry.CC_ClassificationType = Classification.ClassificationType.EXP;
			exportClassInThisCountry.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var importClasses = new ImportClassificationCollection(Factory);

			ZQuery filter = new ZQuery();
			filter.FetchOnlyFromLocalCache = true;
			importClasses.Load(filter);
			AssertEquals("Should load records made in this country, IMP type", 1, importClasses.Count);
			AssertEquals("Should load records made in this country, IMP type", Classification.ClassificationType.IMP, importClasses[0].CC_ClassificationType);
			AssertEquals("Should load records made in this country, IMP type", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, importClasses[0].CC_RN_NKCountryCode);
		}

		[ExpectNoExceptions()]
		public void TestConstruction()
		{
			new ImportClassificationCollection(Factory);
		}

		public void TestIndexer()
		{
			var collection = new ImportClassificationCollection(Factory);
			BusinessObject bizO = collection.AddNew();
			AssertEquals(bizO, collection[0]);
		}
	}
}
