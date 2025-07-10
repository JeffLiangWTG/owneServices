using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(CusContainer))]
	class CusContainerTest : Customs.Business.Testing.BaseCusContainerTest<CusContainer, JobDeclaration>
	{
		/***
		 * !!! Test to be removed in production code !!!
		 * Please remove the whole method TestTypeDecider from the production code once the following test case passes.
		 * It is only meant as an initial completeness check immdiately after the country project is set up.
		 ***/
		public void TestTypeDecider()
		{
			AssertType<CusContainer>("Update BaseCusContainerTypeDecider to include a decider for this class", Factory.New(typeof(Customs.Business.BaseCusContainer)));
		}

		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bo) => new Customs.Business.BaseCusContainer.CustomLabelsProvider(((CusContainer)bo).Declaration);
	}
}
