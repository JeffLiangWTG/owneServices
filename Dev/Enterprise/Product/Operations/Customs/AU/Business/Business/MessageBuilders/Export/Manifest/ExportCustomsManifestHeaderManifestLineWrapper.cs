using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ExportCustomsManifestHeaderManifestLineWrapper : IManifestLineWrapper
	{
		public ExportCustomsManifestHeaderManifestLineWrapper(ExportCustomsManifestHeader header)
		{
			this.header = header;
		}

		public int PackCount => header.ED_NoOfPacks;

		public int ContainerCount => header.ED_NoOfContainer;

		public ZString CAN => ZString.Empty;

		public ZString CCAN => header.ED_CCAN;

		public ZString CountryOfDestination => ZString.Empty;

		public ZString GoodsOwner => ZString.Empty;

		public ZString GoodsOwnerPartyID => ZString.Empty;

		public ZString GoodsDescription => ZString.Empty;

		public ZString AirWaybillNumber => ZString.Empty;

		public ZString ExemptionCode => ZString.Empty;

		public int LineNumber => 1;

		public ZString LineActionCode => ZString.Empty;

		public ZString Reference => header.ED_BGMReference;

		public ZString HouseBillNumber => string.Empty;

		protected ExportCustomsManifestHeader header;
	}
}
