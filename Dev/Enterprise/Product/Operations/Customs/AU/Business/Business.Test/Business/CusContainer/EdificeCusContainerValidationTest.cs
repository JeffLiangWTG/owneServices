using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EdificeCusContainerValidationTest : CusContainerValidationTest
	{
		public void TestValidateCO_FCL_LCL_AIR()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRXU1234568";
			container.Validation.ValidateCO_FCL_LCL_AIR();
			AssertEquals("Has a message error", true, container.CO_FCL_LCL_AIRInfo.HasMessageErrors());
		}
	}
}
