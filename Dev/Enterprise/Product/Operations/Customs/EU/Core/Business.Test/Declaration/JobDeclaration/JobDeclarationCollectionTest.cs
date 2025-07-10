using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclarationCollection))]
	public class JobDeclarationCollectionTest : Customs.Business.Testing.BaseJobDeclarationBizoCollectionTest
	{
		public void TestFetchStrategy()
		{
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			AssertType<JobDeclarationCollectionFetchStrategy>(collection.FetchStrategy);
		}

		public void TestTypedIndexer()
		{
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var declaration = collection.AddNew();
			AssertEquals(declaration, collection[0]);
		}

		public void TestExcludeEMCSJobs()
		{
			var emcs = (Customs.Business.BaseJobDeclaration)Factory.New<Integration.Customs.EUEMCS.IJobDeclaration>();
			emcs.FillWithValidTestData();
			Factory.Save();
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load();
			Assert(collection.Cast<JobDeclaration>().All(x => x.JE_ApplicationCode != Enterprise.Customs.Business.BaseJobDeclarationTypeDecider.EMCSApplicationCode));
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
	}
}
