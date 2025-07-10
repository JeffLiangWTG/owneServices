using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class CustomsOffices02Provider : ICustomsOffices02
	{
		public CustomsOffices02Provider(AsycudaManifestHeader header)
		{
			this.header = header;
		}

		readonly AsycudaManifestHeader header;

		public string CustomsOfficeLodgement => header.AMA_CustomsOffice;

		public string PresentationCustomsOffice => header.PresentationOffice;
	}
}
