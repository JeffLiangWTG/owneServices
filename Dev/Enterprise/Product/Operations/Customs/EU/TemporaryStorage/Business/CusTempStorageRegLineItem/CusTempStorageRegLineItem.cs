using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.TemporaryStorage.Business;

public class CusTempStorageRegLineItem : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineItem, Integration.Customs.EU.ICusTempStorageRegLineItem
{
	public CusTempStorageRegLineItem(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override Customs.Business.TariffFormatter GetNewTariffFormatter() => TariffFormatter.New(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

	protected override Type GetStorageRegLineItemPivotTypeCore() => typeof(CusTempStorageRegLineItemPivot);
}
