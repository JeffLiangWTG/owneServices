using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.GUI.Test
{
	public static class AcceptabilityBandTestConfigsHelper
	{
		#region Test Configs

		public static AcceptabilityBandTestConfig CreateAcceptabilityBandTestConfig(BusinessObjectFactory factory, string workflowType = "ORG", bool shouldUseExistingSystem = false)
		{
			return AcceptabilityBandTestConfig.Create(factory, workflowType, shouldUseExistingSystem);
		}

		#endregion
	}
}
