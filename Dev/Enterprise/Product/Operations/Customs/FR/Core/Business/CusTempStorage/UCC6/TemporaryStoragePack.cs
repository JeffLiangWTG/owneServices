using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class TemporaryStoragePack : EU.Business.CusTempStorage.TemporaryStoragePack
	{
		public TemporaryStoragePack(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Type GetPackedItemTypeCore() => typeof(TemporaryStoragePackedItem);
	}
}
