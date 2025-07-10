using CargoWise.Types;
using Enterprise.Customs.FR.Business;

namespace Enterprise.Customs.FR.GUI.CusTempStorage.Testing
{
	public class LADTTemporyStorageUserControlForPluginTest : CINTemporyStorageUserControlForPluginTest
	{
		protected override CINTemporyStorageUserControlForPlugin GetCINTemporyStorageUserControlForPlugin() => new LADTTemporyStorageUserControlForPlugin();

		protected override ZString AppCode => FRConstants.TemporaryStorage.AppCodeLAD;
	}
}
