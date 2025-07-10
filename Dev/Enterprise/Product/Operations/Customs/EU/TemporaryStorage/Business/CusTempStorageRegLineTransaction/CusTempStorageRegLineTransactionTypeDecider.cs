using System;
using TS = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

namespace Enterprise.Customs.EU.TemporaryStorage.Business;

public class CusTempStorageRegLineTransactionTypeDecider : TS.CusTempStorageRegLineTransactionTypeDecider
{
	public override Type GetRegLineType() => typeof(CusTempStorageRegLine);

	public override Type GetTypeForBinding() => typeof(CusTempStorageRegLineTransaction);

	public override Type GetTypeForNew() => typeof(CusTempStorageRegLineTransaction);
}
