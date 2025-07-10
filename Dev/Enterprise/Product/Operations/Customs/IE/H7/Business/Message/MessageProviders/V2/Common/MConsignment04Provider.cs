using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.AIS;

namespace Enterprise.Customs.IE.H7.Business
{
	public class MConsignment04Provider : IMConsignment04
	{
		public MConsignment04Provider(AsycudaBill bill)
		{
			this.bill = bill;
		}

		readonly AsycudaBill bill;

		public string ContainerIndicator => null;

		public string InlandModeOfTransport => null;

		public string ModeOfTransportAtTheBorder => null;

		public decimal GrossMass => bill.MassInKilos;

		public int TotalPackageNumber => bill.ABL_ManifestQty;

		public string ReferenceNumberUCR => bill.ABL_UCRNumber;

		public IReadOnlyCollection<IMTransportEquipment> TransportEquipments => Array.Empty<IMTransportEquipment>();

		public IGoodsLocation LocationOfGoods => CachedValueHelper.GetValue(ref locationOfGoodsCached, () => new GoodsLocationProvider(bill.CusGoodsLocation));
		CachedValue<IGoodsLocation> locationOfGoodsCached;

		public IIdType ArrivalTransportMeans => null;

		public string ActiveBorderTransportMeansNationality => null;

		public IReadOnlyCollection<IDocument> TransportDocuments => transportDocuments ?? (transportDocuments =
			bill.AdditionalDocuments.Where(x => x.IsATransportDocument)
			.Select(x => new CcQualifierDocumentProvider(x)).ToArray<ICcQualifierDocument>());
		IReadOnlyCollection<IDocument> transportDocuments;

		public ITransportCosts TransportAndInsuranceCostsToTheDestination => CachedValueHelper.GetValue(ref transportCostsCached, () => TransportCostsProvider.NewOrNull(bill));
		CachedValue<ITransportCosts> transportCostsCached;
	}
}
