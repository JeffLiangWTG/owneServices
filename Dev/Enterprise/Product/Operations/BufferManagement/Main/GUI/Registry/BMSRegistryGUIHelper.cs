using CargoWise.Application;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.GUI
{
	static class BMSRegistryGUIHelper
	{
		public static bool IsPlanningManagementEnabled => Registry.IsPlanningManagementEnabled;

		static IBMSRegistry Registry => ObjectFactory.Get<IBMSRegistry>();
	}
}
