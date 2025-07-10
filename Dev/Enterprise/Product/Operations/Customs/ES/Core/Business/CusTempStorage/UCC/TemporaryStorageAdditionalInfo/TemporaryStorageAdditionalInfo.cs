using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business.CusTempStorage
{
	public class TemporaryStorageAdditionalInfo : EU.Business.CusTempStorage.TemporaryStorageAdditionalInfo
	{
		public TemporaryStorageAdditionalInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new TemporaryStorageAdditionalInfoLookups Lookups => (TemporaryStorageAdditionalInfoLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new TemporaryStorageAdditionalInfoLookups(this);
	}
}
