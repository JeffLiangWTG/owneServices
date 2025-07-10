using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class IE170RepresentativeProvider : NctsDepartureHeaderMessageProvider, IRepresentative
	{
		public IE170RepresentativeProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public string Status => Id == null ? null : MovementHeader.Representative.E2_OA_Address == NctsHeader.Principal.E2_OA_Address ? "2" : "3";

		public string Id
		{
			get
			{
				var org = MovementHeader.Representative?.Organisation;
				var eori = org.GetEoriDetails();
				if (!eori.IsEmpty)
				{
					return eori;
				}
				return null;
			}
		}

		public IContact Contact => Id == null ? null : contact ?? (contact = ContactProvider.New(MovementHeader.Representative.Organisation));
		IContact contact;
	}
}
