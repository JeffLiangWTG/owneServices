using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocCusContainer))]
	sealed class DocCusContainerTest : DocBaseCusContainerAbstractTest<CusContainer, DocCusContainer>
	{
		public void TestDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var container = Factory.New<CusContainer>();

			DocCusContainer cusContainerWrapper = DocCusContainer.New(container, declaration, Factory);
			cusContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertNotNull("Declaration wrapper should exist within the container wrapper", cusContainerWrapper.Declaration);
			AssertEquals("Document direction of declaration should match document direction of container", cusContainerWrapper.DocumentDirection, cusContainerWrapper.Declaration.DocumentDirection);
		}

		public void TestTotalAllocatedJobPackages()
		{
			var declaration = Factory.New<JobDeclaration>();
			CusContainer container1 = declaration.CusContainers.AddNew();
			CusContainer container2 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "111";
			container2.CO_ContainerNumber = "222";

			Package pack1 = declaration.Packages.AddNew();
			Package pack2 = declaration.Packages.AddNew();
			pack1.CW_PackQty = 10;
			pack1.CW_ContainerNoOrEquipmentNo = "111";
			pack2.CW_PackQty = 35;
			pack2.CW_ContainerNoOrEquipmentNo = "222";

			DocCusContainer container1Wrapper = DocCusContainer.New(container1, declaration, Factory);
			AssertEquals(10, container1Wrapper.TotalAllocatedJobPackages);
			DocCusContainer container2Wrapper = DocCusContainer.New(container2, declaration, Factory);
			AssertEquals(35, container2Wrapper.TotalAllocatedJobPackages);
		}

		#region Implementation

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.Australia; }
		}

		protected override DocCusContainer CreateContainerWrapper(CusContainer containerInternal, Enterprise.Customs.Business.BaseJobDeclaration declarationInternal)
		{
			return DocCusContainer.New(containerInternal, (JobDeclaration)declarationInternal, Factory);
		}

		#endregion
	}
}
