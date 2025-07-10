using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM428;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Messaging.AIS;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM428Provider : IIM428Provider
	{
		public IM428Provider(Im428 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im428 xmlObject;

		public ZString LocalReferenceNumber => xmlObject.ImportOperation?.Lrn;

		public ZString CustomsRegistrationNumber => xmlObject.ImportOperation?.CustomsRegistrationNumber;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn;

		public ZDateTime DeclarationAcceptanceDateAndTime => (xmlObject.ImportOperation?.DeclarationAcceptanceDateAndTime).ConvertToZDateTime();

		public ZString DeclarationType => xmlObject.ImportOperation?.DeclarationType;

		public ZString AdditionalDeclarationType => xmlObject.ImportOperation?.AdditionalDeclarationType;

		public ZDateTime ResponseDateLimit => (xmlObject.ImportOperation?.ResponseDateLimit).ConvertToZDateTime();

		public ZString PreferredPaymentMethod => xmlObject.ImportOperation?.PreferredPaymentMethod;

		public ZString Remarks => xmlObject.ImportOperation?.Remarks;

		public IReadOnlyCollection<IM428GoodsItemProvider> GoodsItems => goodsItems ?? (goodsItems = xmlObject.GoodsShipment?.GoodsShipmentItem?.Select(x => new IM428GoodsItemProvider(x)).ToArray() ?? Array.Empty<IM428GoodsItemProvider>());
		IM428GoodsItemProvider[] goodsItems;

		public void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee)
		{
			if (messageAttachee is IAISMessageAttachee aisMessageAttachee)
			{
				if (AdditionalDeclarationType.EqualsIgnoringCase(AdditionalDeclarationTypeList.C) ||
					AdditionalDeclarationType.EqualsIgnoringCase(AdditionalDeclarationTypeList.F))
				{
					aisMessageAttachee.SetSimplifiedDeclarationMRN(aisMessageAttachee.MovementReferenceNumber);
				}

				aisMessageAttachee.MovementReferenceNumberSetter(MovementReferenceNumber, DeclarationAcceptanceDateAndTime);
				aisMessageAttachee.PopulateConfirmedDutiesAndTaxes(GoodsItems);
			}
		}
	}
}
