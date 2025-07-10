using System;
using TS = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

namespace Enterprise.Customs.EU.TemporaryStorage.Business;

public class CusTempStorageRegHeaderTypeDecider : TS.CusTempStorageRegHeaderTypeDecider
{
	public override Type GetTypeForBinding() => typeof(CusTempStorageRegHeader);

	public override Type GetTypeForNew() => typeof(CusTempStorageRegHeader);
}
