using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business;

public class CusSCAHouseTypeDecider : TypeDecider
{
	public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
	{
		return typeof(CusSCAHouse);
	}

	public override Type GetTypeForBinding()
	{
		return typeof(CusSCAHouse);
	}

	public override Type GetTypeForNew()
	{
		return typeof(CusSCAHouse);
	}
}
