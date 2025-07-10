using CargoWise.Types;
using Enterprise.Customs.FR.Business;

namespace Enterprise.Customs.FR.GUI.CusTempStorage.Testing
{
	public class ISTTemporyStorageUserControlForPluginTest : CINTemporyStorageUserControlForPluginTest
	{
		protected override CINTemporyStorageUserControlForPlugin GetCINTemporyStorageUserControlForPlugin() => new ISTTemporyStorageUserControlForPlugin();

		protected override ZString AppCode => FRConstants.TemporaryStorage.AppCodeIST;
	}
}
