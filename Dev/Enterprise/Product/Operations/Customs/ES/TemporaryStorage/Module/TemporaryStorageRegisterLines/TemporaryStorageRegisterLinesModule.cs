using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ES.TemporaryStorage.Module
{
	public class TemporaryStorageRegisterLinesModule : EU.TemporaryStorage.Module.TempStorageRegisterLinesModule
	{
		public const string AppCode = "ADT";

		protected override ZString ApplicationCodeFilter => AppCode;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new TemporaryStorageRegisterLinesFilterBusinessObject();
	}
}
