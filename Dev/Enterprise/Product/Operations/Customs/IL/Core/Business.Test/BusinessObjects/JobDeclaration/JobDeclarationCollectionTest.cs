using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(JobDeclarationCollection))]
	class JobDeclarationCollectionTest : Customs.Business.Testing.BaseJobDeclarationBizoCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
	}
}

