using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	class TS313AndTS315DeclarationTypeProvider : ITS313AndTS315DeclarationType, ITSDeclarationTypeCustomsOffices, ITS313AndTS315DeclarationTypeParties
	{
		internal protected TS313AndTS315DeclarationTypeProvider(TemporaryStorageHeader header)
		{
			this.header = header;
		}
		protected readonly TemporaryStorageHeader header;

		public string MessageType
			=> header.AMA_MessageType == PNTSMessageTypeList.Codes.CombinedTemporaryStorage ? TemporaryStorageDeclarationTypeList.Codes.DeclarationAndPresentationNotification : TemporaryStorageDeclarationTypeList.Codes.Declaration;

		public string LRN => header.LRN;

		public ITSDeclarationTypeCustomsOffices CustomsOffices => this;

		public string SupervisingCustomsOffice => header.AMA_CustomsOffice;

		public string CustomsOfficeLodgement => header.CustomsOfficeOfLodgement;

		public ITS313AndTS315DeclarationTypeParties Parties => this;

		public string Declarant => CachedValueHelper.GetValue(ref declarantCached, () => header.Declarant is OrgAddress orgAddress ? AIS.CusTempStorage.OrganizationProvider.GetEori(orgAddress).ToString() : null);
		CachedValue<string> declarantCached;

		public ITSRepresentative Representative => CachedValueHelper.GetValue(ref representativeCached, () => TSRepresentativeProvider.New(header));
		CachedValue<ITSRepresentative> representativeCached;
	}
}
