using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	public class TS332GoodsShipmentTypeProvider : ITS332GoodsShipmentType
	{
		public static TS332GoodsShipmentTypeProvider New(TemporaryStorageHeader header)
			=> (header == null) ? null : new TS332GoodsShipmentTypeProvider(header);

		public IReadOnlyCollection<ISimplifiedDeclarationDocumentWritingOff> DocumentsAuthorisations
			=> documentsAuthorisations ??= header.PreviousDocuments.Where(x => x != null).Select(x => new SimplifiedDeclarationDocumentWritingOffProvider(x)).ToArray();
		IReadOnlyCollection<ISimplifiedDeclarationDocumentWritingOff> documentsAuthorisations;
		public IGoodsLocation GoodsLocation => CachedValueHelper.GetValue(ref locationOfGoods, () => new GoodsLocationProvider((CusGoodsLocation)header.GoodsLocation));
		CachedValue<IGoodsLocation> locationOfGoods;
		public IMeansIdentityAtBorderMandatory TransportInformation
			=> CachedValueHelper.GetValue(ref transportInformation, () => MeansIdentityAtBorderMandatoryProvider.New(header));
		CachedValue<IMeansIdentityAtBorderMandatory> transportInformation;

		TS332GoodsShipmentTypeProvider(TemporaryStorageHeader header)
		{
			this.header = header;
		}

		readonly TemporaryStorageHeader header;
	}
}
