using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.TemporaryStorage.Business
{
	public class CusTempStorageRegPremisesLookups : EU.TemporaryStorage.Business.CusTempStorageRegPremisesLookups
	{
		public CusTempStorageRegPremisesLookups(AutoCusTempStorageRegPremises parent) : base(parent)
		{
		}

		protected override string GetAuthorizationType(string srpType)
		{
			return (srpType switch
			{
				LAM => LAME,
				_ => AuthorizationTypeList.Codes.TST
			});
		}

		const string LAM = "LAM";
		const string LAME = "LAME";
	}
}
