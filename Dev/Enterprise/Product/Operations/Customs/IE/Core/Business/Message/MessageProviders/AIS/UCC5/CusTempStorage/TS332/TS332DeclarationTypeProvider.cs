using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	public  class TS332DeclarationTypeProvider : ITS332DeclarationType
	{
		public static TS332DeclarationTypeProvider New(TemporaryStorageHeader header)
			=> (header == null) ? null : new TS332DeclarationTypeProvider(header);

		public string MRN => header.MRN;

		public string LRN => header.LRN;

		public ITS332DeclarationTypeCustomsOffices CustomsOffices
			=> CachedValueHelper.GetValue(ref customsOfficesCached, () => TS332DeclarationTypeCustomsOfficesProvider.New(header));
		CachedValue<ITS332DeclarationTypeCustomsOffices> customsOfficesCached;

		public ITS332DeclarationTypeParties Parties => CachedValueHelper.GetValue(ref partiesCached, () => TS332DeclarationTypePartiesProvider.New(header));
		CachedValue<ITS332DeclarationTypeParties> partiesCached;

		TS332DeclarationTypeProvider(TemporaryStorageHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}

		readonly TemporaryStorageHeader header;
	}
}
