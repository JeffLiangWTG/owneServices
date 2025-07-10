using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MY.Business.Testing
{
	[TestedType(typeof(CusContainer))]
	class CusContainerTest : Customs.Business.Testing.BaseCusContainerTest<CusContainer, JobDeclaration>
	{
		public void TestDeclaration()
		{
			var declaration = (JobDeclaration)GetJobDeclaration();
			var container = declaration.CusContainers.AddNew();
			AssertEquals(declaration, container.Declaration);
		}

		public void TestLookupsCachesInstance()
		{
			var container = (CusContainer)GetNewBusinessObject();
			AssertSame(container.Lookups, container.Lookups);
		}

		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bo) => new Customs.Business.BaseCusContainer.CustomLabelsProvider(((CusContainer)bo).Declaration);
	}
}
