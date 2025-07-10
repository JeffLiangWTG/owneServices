using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Module
{
	public class TempStorageRegisterModule : EU.TemporaryStorage.Module.TempStorageRegisterModule
	{
		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusTempStorageRegHeaderCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new TempStorageRegisterFilterBusinessObject();

		public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.FRTempStorageRegister;
	}
}
