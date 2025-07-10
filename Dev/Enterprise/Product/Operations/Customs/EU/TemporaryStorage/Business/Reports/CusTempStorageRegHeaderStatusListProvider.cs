using System;
using System.Collections;
using System.Collections.Immutable;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.TemporaryStorage.Business;

public sealed class CusTempStorageRegHeaderStatusListProvider :
	Integration.Customs.EU.ICusTempStorageRegHeaderStatusListProvider,
	DocumentEngine.RuntimeOptions.IDependenceCodeDescriptionPairListProvider
{
	public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
	{
		var result = new CodeDescriptionPairList();

		var types = (Hashtable)ObjectFactory.Get("CusTempStorageRegHeaderApplicationCodeTypes");
		var factory = new BusinessObjectFactory();

		foreach (var code in euApplicationCodeTypes.Value)
		{
			if (!types.ContainsKey(code) || types[code] is not ObjectHandle handle)
			{
				continue;
			}

			var type = handle.GetObjectType();
			result.AddPairsIfNotExist(((CusTempStorageRegHeader)factory.GetNull(type)).Lookups.StatusList.ToArray());
		}

		return result;
	}

	public ReadOnlyCodeDescriptionPairList GetDependenceCodeDescriptionPairList(string applicationCode)
	{
		if (string.IsNullOrEmpty(applicationCode))
		{
			return GetCodeDescriptionPairList();
		}

		ReadOnlyCodeDescriptionPairList result = null;
		var types = (Hashtable)ObjectFactory.Get("CusTempStorageRegHeaderApplicationCodeTypes");
		var objectHandle = (ObjectHandle)types[applicationCode];

		var type = objectHandle?.GetObjectType() ?? typeof(CusTempStorageRegHeader);

		var factory = new BusinessObjectFactory();
		var lookups = ((CusTempStorageRegHeader)factory.GetNull(type)).Lookups;

		if (lookups is IRegisterReportStatusListProvider reportStatusListProvider)
		{
			result = reportStatusListProvider.ReportStatusList;
		}
		else
		{
			result = lookups.StatusList;
		} 
		
		return result ?? new CodeDescriptionPairList();
	}

	static readonly Lazy<ImmutableHashSet<string>> euApplicationCodeTypes = new (() => ImmutableHashSet.Create(
		CusTempStorageRegHeaderApplicationCodeList.Codes.ADT,
		CusTempStorageRegHeaderApplicationCodeList.Codes.IST,
		CusTempStorageRegHeaderApplicationCodeList.Codes.SUM,
		CusTempStorageRegHeaderApplicationCodeList.Codes.TSR
	));
}
