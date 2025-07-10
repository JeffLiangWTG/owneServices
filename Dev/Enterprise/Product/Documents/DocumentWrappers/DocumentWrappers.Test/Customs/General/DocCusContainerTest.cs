using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.General.Testing
{
	[TestedType(typeof(DocCusContainer))]
	sealed class DocCusContainerTest : DocBaseCusContainerAbstractTest<BaseCusContainer, DocCusContainer>
	{
		public void TestDeclaration()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseCusContainer container = Factory.New<BaseCusContainer>();

			DocCusContainer cusContainerWrapper = DocCusContainer.New(container, declaration, Factory);
			cusContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertNotNull("Declaration wrapper should exist within the container wrapper", cusContainerWrapper.Declaration);
			AssertEquals("Document direction of declaration should match document direction of container", cusContainerWrapper.DocumentDirection, cusContainerWrapper.Declaration.DocumentDirection);
		}

		#region Implementation

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.Fiji; }
		}

		protected override DocCusContainer CreateContainerWrapper(BaseCusContainer containerInternal, BaseJobDeclaration declarationInternal)
		{
			return DocCusContainer.New(containerInternal, declarationInternal, Factory);
		}

		#endregion
	}
}
