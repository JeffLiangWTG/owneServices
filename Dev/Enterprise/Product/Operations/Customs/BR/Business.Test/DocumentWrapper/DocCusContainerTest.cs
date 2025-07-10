using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
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

		#region Implementation

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.Brazil; }
		}

		protected override DocCusContainer CreateContainerWrapper(CusContainer containerInternal, Customs.Business.BaseJobDeclaration declarationInternal)
		{
			return DocCusContainer.New(containerInternal, (JobDeclaration)declarationInternal, Factory);
		}

		#endregion
	}
}
