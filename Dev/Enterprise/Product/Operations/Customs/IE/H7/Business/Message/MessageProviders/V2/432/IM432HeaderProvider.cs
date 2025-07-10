using System;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.AIS;

namespace Enterprise.Customs.IE.H7.Business
{
	public class IM432HeaderProvider : IIM432Header, IIM432Operation
	{
		public IM432HeaderProvider(MessageSendingObject sendingObject)
		{
			SendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
			Header = (AsycudaManifestHeader)sendingObject.Bill.Header;
			PreparationDateAndTime = DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(ZDateTime.UtcNow, true);
		}
		AsycudaManifestHeader Header { get; }

		MessageSendingObject SendingObject { get; }

		public IIM432Operation ImportOperation => this;

		public string LRN => SendingObject.Bill.LocalReferenceNumber;

		public string MRN => SendingObject.Bill.MovementReferenceNumber;

		public IFallbackProcedure FallbackProcedure => null;

		public string CustomsOfficeOfPresentation => Header.PresentationOffice;

		public string CustomsOfficeLodgement => Header.AMA_CustomsOffice;

		public IMDeclarant Declarant => CachedValueHelper.GetValue(ref declarantCached, () => MDeclarantProvider.New(Header.Declarant));
		CachedValue<IMDeclarant> declarantCached;

		public IMRepresentative Representative => CachedValueHelper.GetValue(ref representativeCached, () => MRepresentativeProvider.NewOrNull(Header));
		CachedValue<IMRepresentative> representativeCached;

		public IIM432GoodsShipment GoodsShipment => CachedValueHelper.GetValue(ref goodsShipmentCached, () => new IM432GoodsShipmentProvider((AsycudaBill)SendingObject.Bill));
		CachedValue<IIM432GoodsShipment> goodsShipmentCached;

		public DateTime PreparationDateAndTime { get; private set; }
	}
}
