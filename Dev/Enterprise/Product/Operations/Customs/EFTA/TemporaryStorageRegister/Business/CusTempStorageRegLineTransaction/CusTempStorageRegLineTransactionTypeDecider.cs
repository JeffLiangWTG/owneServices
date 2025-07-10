using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class CusTempStorageRegLineTransactionTypeDecider : TypeDecider
{
	public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
	{
		Type result = null;
		if (row != null)
		{
			var lineId = new ZGuid(row[CusTempStorageRegLineTransactionSchema.Constants.SRT_SRL]);
			var line = lineId.IsValid
				? factory.Load(GetRegLineType(), lineId)
				: null;

			if (line is CusTempStorageRegLine cusTempStorageRegLine)
			{
				result = cusTempStorageRegLine.GetStorageRegLineTransactionType();
			}
		}
		return result ?? GetTypeForNew();
	}

	public virtual Type GetRegLineType() => typeof(CusTempStorageRegLine);

	public override Type GetTypeForBinding() => typeof(CusTempStorageRegLineTransaction);

	public override Type GetTypeForNew() => typeof(CusTempStorageRegLineTransaction);
}
