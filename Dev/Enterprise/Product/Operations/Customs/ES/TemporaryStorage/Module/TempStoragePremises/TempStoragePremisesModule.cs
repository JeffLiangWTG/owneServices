using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ES.TemporaryStorage.Module
{
	public class TempStoragePremisesModule : EU.TemporaryStorage.Module.TempStoragePremisesModule
	{
		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CustomsTemporaryStorage;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.EU.TempStoragePremises);
		}
	}
}
