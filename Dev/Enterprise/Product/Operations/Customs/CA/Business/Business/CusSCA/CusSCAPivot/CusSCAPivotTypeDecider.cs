using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business;

public class CusSCAPivotTypeDecider : TypeDecider
{
	public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
	{
		return typeof(CusSCAPivot);
	}

	public override Type GetTypeForBinding()
	{
		return typeof(CusSCAPivot);
	}

	public override Type GetTypeForNew()
	{
		return typeof(CusSCAPivot);
	}
}
