using System;
using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS;

namespace Enterprise.Customs.IE.H7.Business
{
	public class IM413_414_415HeaderProvider
	{
		public IM413_414_415HeaderProvider(MessageSendingObject messageSendingObject)
		{
			this.messageSendingObject = messageSendingObject;
		}

		protected MessageSendingObject messageSendingObject;

		public string DeclarationType => ImportDeclarationTypeList.Codes.H7;

		public IReadOnlyCollection<IAuthorisation> Authorisations => Array.Empty<IAuthorisation>();

		public IAuth8F Authorisation8F => null;

		public string CustomsOfficeOfPresentation => null;

		public string SupervisingCustomsOffice => null;

		public string CustomsOfficeLodgement => messageSendingObject.Bill.Header.AMA_CustomsOffice;

		public IImporter Importer => CachedValueHelper.GetValue(ref importerCached, () => new ImporterProvider((AsycudaBill)messageSendingObject.Bill));
		CachedValue<IImporter> importerCached;

		public IMDeclarant Declarant => CachedValueHelper.GetValue(ref declarantCached, () => MDeclarantProvider.New(messageSendingObject.Bill.Header.Declarant));
		CachedValue<IMDeclarant> declarantCached;

		public string PersonProvidingAGuaranteeID => null;

		public string PersonPayingCustomsDutyID => null;

		public IMRepresentative Representative => CachedValueHelper.GetValue(ref representativeCached, () => MRepresentativeProvider.NewOrNull((AsycudaManifestHeader)messageSendingObject.Bill.Header));
		CachedValue<IMRepresentative> representativeCached;

		public IReadOnlyCollection<IGuarantee> Guarantees => Array.Empty<IGuarantee>();

		public string CurrencyExchange => null;

		public IReadOnlyCollection<IDeferredPayment> DeferredPayments => deferredPayments ?? (deferredPayments = new[] { new DeferredPaymentProvider((AsycudaManifestHeader)messageSendingObject.Bill.Header) });
		IReadOnlyCollection<IDeferredPayment> deferredPayments;

		public IReadOnlyCollection<IGoodsShipment> GoodsShipments => goodsShipments ?? (goodsShipments = GetGoodsShipments());
		IReadOnlyCollection<IGoodsShipment> goodsShipments;

		IReadOnlyCollection<IGoodsShipment> GetGoodsShipments()
		{
			return new GoodsShipmentProvider[] { new GoodsShipmentProvider((AsycudaBill)messageSendingObject.Bill) };
		}

		public IFallbackProcedure FallbackProcedure => null;

		public DateTime PreparationDateAndTime => DateTime.MinValue;
	}
}
