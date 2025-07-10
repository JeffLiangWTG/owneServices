using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BE.Business.CusTempStorage;

public class TemporaryStorageHeader : EU.Business.CusTempStorage.TemporaryStorageHeader, Integration.Customs.BE.ITemporaryStorageHeader
{
	public TemporaryStorageHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override EU.Business.CusTempStorage.TemporaryStorageMessageSendingConfiguration GetNewMessageSendingConfiguration() => new TemporaryStorageMessageSendingConfiguration();

	protected override Type GoodsLocationTypeCore => typeof(CusGoodsLocation);
}
