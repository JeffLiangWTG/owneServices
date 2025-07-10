using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business;

public class CusSCAContainerTypeDecider : TypeDecider
{
	public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
	{
		return typeof(CusSCAContainer);
	}

	public override Type GetTypeForBinding()
	{
		return typeof(CusSCAContainer);
	}

	public override Type GetTypeForNew()
	{
		return typeof(CusSCAContainer);
	}
}
