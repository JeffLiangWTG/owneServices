using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ExportClassificationCollection))]
	sealed class ExportClassificationCollectionTest : BusinessObjectCollectionTestCase
	{
		[ExpectNoExceptions()]
		public void TestConstruction()
		{
			new ExportClassificationCollection(Factory);
		}

		public void TestLoadWithRelationshipfilter()
		{
			Classification importClassInThisCountry = Factory.New<Classification>();
			importClassInThisCountry.CC_ClassificationType = Classification.ClassificationType.IMP;
			importClassInThisCountry.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			Classification exportClassInOtherCountry = Factory.New<Classification>();
			exportClassInOtherCountry.CC_ClassificationType = Classification.ClassificationType.EXP;
			exportClassInOtherCountry.CC_RN_NKCountryCode = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).Code;

			Classification exportClassInThisCountry = Factory.New<Classification>();
			exportClassInThisCountry.CC_ClassificationType = Classification.ClassificationType.EXP;
			exportClassInThisCountry.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var exportClasses = new ExportClassificationCollection(Factory);
			ZQuery filter = new ZQuery();
			filter.FetchOnlyFromLocalCache = true;
			exportClasses.Load(filter);
			AssertEquals("Should load records made in this country, EXP type", 1, exportClasses.Count);
			AssertEquals("Should load records made in this country, EXP type", Classification.ClassificationType.EXP, exportClasses[0].CC_ClassificationType);
			AssertEquals("Should load records made in this country, EXP type", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, exportClasses[0].CC_RN_NKCountryCode);
		}

		public void TestIndexer()
		{
			var collection = new ExportClassificationCollection(Factory);
			BusinessObject bizO = collection.AddNew();
			AssertEquals(bizO, collection[0]);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ExportClassificationCollection(Factory);
		}
	}
}
