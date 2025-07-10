using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusContainerInternalTest : TestCaseWithFactory
	{
		public void TestGetNewValidationForEdifice()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			CusContainer container = declaration.CusContainers.AddNew();
			AssertEquals("EdificeCusContainerValidation", typeof(EdificeCusContainerValidation), container.GetNewValidationInternal().GetType());
		}

		public void TestGetNewValidationForCMR()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CusContainer container = declaration.CusContainers.AddNew();
			AssertEquals("IMDCusContainerValidation", typeof(IMDCusContainerValidation), container.GetNewValidationInternal().GetType());
		}
	}
}
