using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class CusTempStorageRegLine : EU.TemporaryStorage.Business.CusTempStorageRegLine
	, Integration.Customs.IT.ICusTempStorageRegLine
	, ICusCodeDataTypeSupporter
{
	public CusTempStorageRegLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[ChildEditable(true)]
	public CusTempStorageContainerCollection Containers => containers ??= GetCusTempStorageContainers();
	CusTempStorageContainerCollection containers;

	CusTempStorageContainerCollection GetCusTempStorageContainers()
	{
		var result = new CusTempStorageContainerCollection(this);
		result.Load();
		RegisterEditableChildObject(result);
		return result;
	}

	public IDictionary<ZString, Type> GetCusCodeDataTypes()
	{
		var result = new Dictionary<ZString, Type>
		{
			{ CusCodeDataTypeList.Codes.TemporaryStorageContainer, typeof(CusTempStorageContainer) }
		};
		return result;
	}

	public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
	{
		yield return new CusCodeDataTypeSupporterFetchStrategy(this);
	}

	public override void Delete()
	{
		if (!IsDeleted)
		{
			FetchForLoadChildEditableObjectsIfNeeded();
			this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
		}
		base.Delete();
	}
}
