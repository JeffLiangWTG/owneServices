using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class RepresentativeProvider : OrganizationProvider, IRepresentative, IRepresentativeType05
	{
		public static RepresentativeProvider New(TemporaryStorageHeader temporaryStorageHeader)
			=> temporaryStorageHeader?.Representative is OrgAddress orgAddress
			? new RepresentativeProvider(orgAddress, GetEori(orgAddress), MessageProviderHelper.GetRepresentativeStatus(orgAddress.PK == temporaryStorageHeader.AMA_OA_Declarant))
			: null;

		RepresentativeProvider(OrgAddress orgAddress, string id, string status) : base(orgAddress, id)
		{
			Status = status;
		}

		public string Status { get; }
	}
}
