using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusContainer))]
	sealed class CusContainerTest : Customs.Business.Testing.BaseCusContainerTest<CusContainer, JobDeclaration>
	{
		public void TestDeclaration()
		{
			var declaration = (JobDeclaration)GetJobDeclaration();
			var container = declaration.CusContainers.AddNew();
			AssertEquals(declaration, container.Declaration);
		}

		public void TestAdditionalSeals()
		{
			AssertType<CusSealCollection>(CusContainer.AdditionalSeals);
		}

		public void TestContainerTypeAndContainerSize()
		{
			var containerSet = ContainerHelperTest.CreateContainerTestData(Factory);
			var containerHelper = new ContainerHelper(Factory);
			CombineAssertions(() =>
			{
				CusContainer.CO_RC = containerSet.container1;
				AssertEquals("ContainerSize is 12", "12", CusContainer.ContainerSize);
				AssertEquals("ContainerType is GP", "GP", CusContainer.ContainerType);
				AssertEquals("Same as the value of GetNACCSContainerSize", containerHelper.GetNACCSContainerSize(CusContainer.Container), CusContainer.ContainerSize);
				AssertEquals("Same as the value of GetNACCSContainerType", containerHelper.GetNACCSContainerType(CusContainer.Container), CusContainer.ContainerType);

				CusContainer.CO_RC = containerSet.container4;
				AssertEquals("ContainerSize is 99", "99", CusContainer.ContainerSize);
				AssertEquals("ContainerType is SN", "SN", CusContainer.ContainerType);
			});
		}

		public void TestLookupsCachesInstance()
		{
			AssertSame(CusContainer.Lookups, CusContainer.Lookups);
		}

		CusContainer CusContainer => cusContainer ??= (CusContainer)GetNewBusinessObject();
		CusContainer cusContainer;
	}
}
