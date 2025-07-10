using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class CusTempStorageRegLineTypeDecider : TypeDecider
{
	public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
	{
		Type result = null;
		if (row != null)
		{
			var headerId = new ZGuid(row[CusTempStorageRegLine.Schema.SRL_SRH]);
			var header = headerId.IsValid
				? factory.Load(GetHeaderType(), headerId)
				: null;

			if (header is CusTempStorageRegHeader cusTempStorageRegHeader)
			{
				result = cusTempStorageRegHeader.GetStorageRegLineType();
			}
		}

		return result ?? GetTypeForNew();
	}

	public virtual Type GetHeaderType() => typeof(CusTempStorageRegHeader);

	public override Type GetTypeForBinding() => typeof(CusTempStorageRegLine);

	public override Type GetTypeForNew() => typeof(CusTempStorageRegLine);
}
