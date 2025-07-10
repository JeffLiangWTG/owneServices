using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.UPE.Business.Testing
{
	class OrgCusCodeValidationTestCase : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected void AssertHasError(UPEOrgCusCode code, bool shouldHaveError, string expectedErrorText)
		{
			var messages = code.OK_CodeTypeInfo.GetErrors().Select(e => e.Message);
			var failureMessage = string.Format("Code [{0}] should{1} have error [{2}]", code.OK_CodeType, shouldHaveError ? "" : " NOT", expectedErrorText);
			if (shouldHaveError)
			{
				AssertCollectionContains(failureMessage, expectedErrorText, messages);
			}
			else
			{
				AssertCollectionNotContains(failureMessage, expectedErrorText, messages);
			}
		}
	}
}
