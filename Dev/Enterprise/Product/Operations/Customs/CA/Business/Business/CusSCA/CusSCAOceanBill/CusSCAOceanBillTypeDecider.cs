using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business;

public class CusSCAOceanBillTypeDecider : TypeDecider
{
	public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
	{
		return typeof(CusSCAOceanBill);
	}

	public override Type GetTypeForBinding()
	{
		return typeof(CusSCAOceanBill);
	}

	public override Type GetTypeForNew()
	{
		return typeof(CusSCAOceanBill);
	}
}
