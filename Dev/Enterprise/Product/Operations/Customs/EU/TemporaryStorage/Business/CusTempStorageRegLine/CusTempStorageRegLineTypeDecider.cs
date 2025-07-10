using System;
using TS = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

namespace Enterprise.Customs.EU.TemporaryStorage.Business;

public class CusTempStorageRegLineTypeDecider : TS.CusTempStorageRegLineTypeDecider
{
	public override Type GetHeaderType() => typeof(CusTempStorageRegHeader);

	public override Type GetTypeForBinding() => typeof(CusTempStorageRegLine);

	public override Type GetTypeForNew() => typeof(CusTempStorageRegLine);
}
