using System.Collections;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class CusTempStorageRegHeaderTypeDecider : TypeDecider
{
	public override Type GetTypeForNew() => typeof(CusTempStorageRegHeader);

	public override Type GetTypeForBinding() => typeof(CusTempStorageRegHeader);

	public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
	{
		var appCode = row == null ? ZString.Empty : new ZString(row[CusTempStorageRegHeader.Schema.SRH_AppCode]);
		Type bizOType = GetType(appCode);
		return bizOType ?? GetTypeForNew();
	}

	static Type GetType(string applicationCode)
	{
		if (string.IsNullOrEmpty(applicationCode))
		{
			return null;
		}

		var types = (Hashtable)ObjectFactory.Get("CusTempStorageRegHeaderApplicationCodeTypes");
		var objectHandle = (ObjectHandle)types[applicationCode];
		return objectHandle?.GetObjectType();
	}
}
