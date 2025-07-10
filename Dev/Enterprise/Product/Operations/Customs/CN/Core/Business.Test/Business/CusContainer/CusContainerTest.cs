using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CusContainer))]
	class CusContainerTest : Customs.Business.Testing.BaseCusContainerTest<CusContainer, JobDeclaration>
	{
		public void TestContainerCodeList()
		{
			var list = Factory.GetCachedValue<CNContainerCodeList>();
			Assert("Container code list should be untranslatable", list is UntranslatableCodeDescriptionPairList);
		}

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
			var container = (CusContainer)GetNewBusinessObject();
			var lookup1 = container.Lookups;
			var lookup2 = container.Lookups;
			AssertEquals(lookup2, lookup1);
		}

		public void TestContainerCode()
		{
			var containerRef = Factory.New<RefContainer>();
			containerRef.RC_Code = "20PP";
			var containerMap = Factory.New<RefContainerCodeMap>();
			containerMap.RCM_RC_Container = containerRef.PK;
			containerMap.RCM_RN_NKCountry = "CN";
			containerMap.RCM_Code = "31";
			var declaration = (JobDeclaration)GetJobDeclaration();
			var container = declaration.CusContainers.AddNew();
			container.CO_RC = containerRef.PK;
			AssertEquals("31", container.ContainerCode);
			AssertEquals("其他标准箱（S）", container.ContainerCodeDescription);
		}

		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bo)
		{
			ICustomLabelsProvider result = new Customs.Business.BaseCusContainer.CustomLabelsProvider(((CusContainer)bo).Declaration);
			return result;
		}
	}
}
