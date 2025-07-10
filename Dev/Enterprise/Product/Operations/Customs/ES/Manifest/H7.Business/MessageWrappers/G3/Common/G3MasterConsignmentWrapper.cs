using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public abstract class G3MasterConsignmentWrapper : IG3MasterConsignment
	{
		protected G3MasterConsignmentWrapper(IEnumerable<AsycudaBill> bills)
		{
			this.bills = Argument.NotNull(bills, nameof(bills));
			header = Argument.NotNull(bills?.FirstOrDefault()?.Header, "bills[0].Header");
		}

		protected readonly IEnumerable<AsycudaBill> bills;

		protected readonly AsycudaManifestHeader header;

		public IReadOnlyCollection<ICommonDocumentGoodsItemId> PreviousDocument => PreviousDocumentCore();

		protected IReadOnlyCollection<ICommonDocumentGoodsItemId> previousDocument;

		public IDocumentsCommon TransportDocument => CachedValueHelper.GetValue(ref transportDocument, () => new DocumentCommonWrapper(header.TransportDocumentType, header.TransportDocumentReference));
		CachedValue<IDocumentsCommon> transportDocument;

		public ZString Receptacle => header.MasterBill.ABL_BillNumber;

		public IGenericLocation LocationOfGoods => CachedValueHelper.GetValue(ref location, () => GetLocationOfGoods(header));
		CachedValue<IGenericLocation> location;

		IGenericLocation GetLocationOfGoods(AsycudaManifestHeader header)
		{
			return header.CusGoodsLocation.Address.AuthorisationNumber.IsEmpty ? null : new G3GenericLocationWrapper(header);
		}

		public IReadOnlyCollection<ZString> TransportEquipmentContainers => Array.Empty<ZString>();

		protected virtual IReadOnlyCollection<ICommonDocumentGoodsItemId> PreviousDocumentCore()
		{
			return previousDocument ?? (previousDocument = new List<ICommonDocumentGoodsItemId> { new G3CommonPreviousDocumentWrapper(header, true) }.AsReadOnly());
		}
	}
}
