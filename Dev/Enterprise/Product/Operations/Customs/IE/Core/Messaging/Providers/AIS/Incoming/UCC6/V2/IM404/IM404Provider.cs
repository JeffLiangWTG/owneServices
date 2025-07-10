using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM404;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM404Provider : IIM404Provider
	{
		public IM404Provider(Im404 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im404 xmlObject;

		public ZString LocalReferenceNumber => xmlObject.ImportOperation?.Lrn;

		public ZString CustomsRegistrationNumber => xmlObject.ImportOperation?.CustomsRegistrationNumber;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn;

		public ZDateTime AmendmentDateAndTime => (xmlObject.ImportOperation?.AmendmentDateAndTime).ConvertToZDateTime();

		public ZDateTime AmendmentAcceptanceDateAndTime => (xmlObject.ImportOperation?.AmendmentAcceptanceDateAndTime).ConvertToZDateTime();

		public ZString PreferredPaymentMethod => xmlObject.ImportOperation?.PreferredPaymentMethod;

		public ZString Remarks => xmlObject.ImportOperation?.Remarks;

		public IReadOnlyCollection<IGoodsItemProvider> GoodsItems => goodsItems ?? (goodsItems = xmlObject.GoodsShipment?.GoodsShipmentItem?.Select(x => new IM404GoodsItemProvider(x)).ToArray() ?? Array.Empty<IM404GoodsItemProvider>());
		IM404GoodsItemProvider[] goodsItems;

		public void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee)
		{
			if (messageAttachee is IAISMessageAttachee aisMessageAttachee)
			{
				aisMessageAttachee.PopulateConfirmedDutiesAndTaxes(GoodsItems);
			}
		}
	}
}
