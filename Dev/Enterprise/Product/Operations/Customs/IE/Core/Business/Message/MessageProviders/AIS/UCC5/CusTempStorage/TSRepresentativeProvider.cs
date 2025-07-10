using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.IE.Business.AIS.CusTempStorage;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	public class TSRepresentativeProvider : OrganizationProvider, ITSRepresentative
	{
		public static TSRepresentativeProvider New(TemporaryStorageHeader header)
			=> header?.Representative is OrgAddress orgAddress
			? new TSRepresentativeProvider(orgAddress, GetEori(orgAddress), header.AMA_AgentType)
			: null;

		TSRepresentativeProvider(OrgAddress orgAddress, string id, string status) : base(orgAddress, id)
		{
			ID = id;
			Status = status;
		}

		public string Status { get; }

		public string ID { get; }
	}
}
