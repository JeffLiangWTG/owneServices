using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(DocCusContainer))]
	sealed class DocCusContainerTest : DocBaseCusContainerAbstractTest<CusContainer, DocCusContainer>
	{
		#region Implementation

		protected override string TestingCountry => Core.Constants.CountryCodes.Mexico;

		protected override DocCusContainer CreateContainerWrapper(CusContainer containerInternal, Customs.Business.BaseJobDeclaration declarationInternal)
		{
			return DocCusContainer.New(containerInternal, (JobDeclaration)declarationInternal, Factory);
		}

		#endregion
	}
}
