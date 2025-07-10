using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CusContainer))]
	class CusContainerTest : Customs.Business.Testing.BaseCusContainerTest<CusContainer, JobDeclaration>
	{
		public void TestTypeDecider()
		{
			Assert("Update BaseCusContainerTypeDecider to include a decider for this class", Factory.New(typeof(Customs.Business.BaseCusContainer)).GetType() == typeof(CusContainer));
		}

		public void TestDeclaration()
		{
			var declaration = (JobDeclaration)GetJobDeclaration();
			var container = declaration.CusContainers.AddNew();
			AssertEquals(declaration, container.Declaration);
		}

		public void TestLookupsCachesInstance()
		{
			CusContainer container = (CusContainer)GetNewBusinessObject();
			CusContainerLookups lookup1 = container.Lookups;
			CusContainerLookups lookup2 = container.Lookups;
			AssertEquals(lookup2, lookup1);
		}

		#region Implementation

		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bo)
		{
			ICustomLabelsProvider result = new Customs.Business.BaseCusContainer.CustomLabelsProvider(((CusContainer)bo).Declaration);
			return result;
		}

		#endregion
	}
}
