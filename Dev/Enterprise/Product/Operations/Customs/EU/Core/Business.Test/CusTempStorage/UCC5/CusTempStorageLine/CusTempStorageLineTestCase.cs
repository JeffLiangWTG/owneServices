using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing;

[TestsSubclassesOf(typeof(CusTempStorageLine))]
public abstract class CusTempStorageLineTestCase<TCusTempStorageLine> : EnterpriseBusinessObjectTestCase
	where TCusTempStorageLine : CusTempStorageLine
{
	protected TCusTempStorageLine GetNewCusTempStorageLine() => GetNewCusTempStorageLine(Factory);

	public virtual void TestLookupsType()
	{
		var storageLine = GetNewCusTempStorageLine();
		AssertType(GetLookupType(), storageLine.Lookups);
	}

	public virtual void TestValidationType()
	{
		var storageLine = GetNewCusTempStorageLine();
		AssertType(GetValidationType(), storageLine.Validation);
	}

	public virtual void TestDecType()
	{
		var storageLine = GetNewCusTempStorageLine();
		AssertType(GetDecType(), storageLine.Dec);
	}

	public virtual void TestLineItems()
	{
		var storageLine = GetNewCusTempStorageLine();
		AssertNotNull(storageLine.CusTempStorageLineItems);
		AssertType(GetLineItemsType(), storageLine.CusTempStorageLineItems);
	}

	#region Implementation

	protected abstract TCusTempStorageLine GetNewCusTempStorageLine(BusinessObjectFactory factory);

	protected virtual Type GetLookupType() => typeof(CusTempStorageLineLookups);

	protected virtual Type GetValidationType() => typeof(CusTempStorageLineValidation);

	protected virtual Type GetLineItemsType() => typeof(CusTempStorageLineItemCollection<CusTempStorageLineItem>);

	protected virtual Type GetDecType() => typeof(CusTempStorageDec);

	#endregion
}
