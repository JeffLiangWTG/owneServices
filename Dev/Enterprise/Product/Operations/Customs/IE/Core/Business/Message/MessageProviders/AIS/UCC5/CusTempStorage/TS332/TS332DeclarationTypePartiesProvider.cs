using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.AIS.CusTempStorage;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	public class TS332DeclarationTypePartiesProvider : ITS332DeclarationTypeParties
	{
		public static TS332DeclarationTypePartiesProvider New(TemporaryStorageHeader header)
			=> (header == null) ? null : new TS332DeclarationTypePartiesProvider(header);

		public string PresenterID => CachedValueHelper.GetValue(ref presenterEORIIdCached, () => OrganizationProvider.GetEori(header.Presenter));
		CachedValue<string> presenterEORIIdCached;

		public ITSRepresentative Representative => CachedValueHelper.GetValue(ref representativeCache, () => TSRepresentativeProvider.New(header));
		CachedValue<ITSRepresentative> representativeCache;

		TS332DeclarationTypePartiesProvider(TemporaryStorageHeader header)
		{
			this.header = header;
		}

		readonly TemporaryStorageHeader header;
	}
}
