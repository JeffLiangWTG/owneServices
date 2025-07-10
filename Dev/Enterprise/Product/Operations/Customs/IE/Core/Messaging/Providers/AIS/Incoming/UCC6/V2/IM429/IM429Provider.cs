using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM429;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Messaging.AIS;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM429Provider : IIM429Provider
	{
		public IM429Provider(Im429 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im429 xmlObject;

		public ZString LocalReferenceNumber => xmlObject.ImportOperation?.Lrn;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn;

		public ZString DeclarationType => xmlObject.ImportOperation?.DeclarationType;

		public ZString AdditionalDeclarationType => xmlObject.ImportOperation?.AdditionalDeclarationType;

		public ZDateTime DeclarationAcceptanceDate => (xmlObject.ImportOperation?.DeclarationAcceptanceDate).ConvertToZDateTime();

		public ZDateTime ReleaseDate => (xmlObject.ImportOperation?.ReleaseDate).ConvertToZDateTime();

		public ZDateTime ResponseDateLimit => (xmlObject.ImportOperation?.ResponseDateLimit).ConvertToZDateTime();

		public ZString PreferredPaymentMethod => xmlObject.ImportOperation?.PreferredPaymentMethod;

		public ZString Remarks => xmlObject.ImportOperation?.Remarks;

		public IReadOnlyCollection<IM429GoodsItemProvider> GoodsItems => goodsItems ?? (goodsItems = xmlObject.GoodsShipment.FirstOrDefault()?.GoodsShipmentItem?.Select(x => new IM429GoodsItemProvider(x)).ToArray() ?? Array.Empty<IM429GoodsItemProvider>());

		public string GetEntryStatus(EDIMessage message)
		{
			if (AdditionalDeclarationType.EqualsIgnoringCase(AdditionalDeclarationTypeList.C) ||
					AdditionalDeclarationType.EqualsIgnoringCase(AdditionalDeclarationTypeList.F))
			{
				return AISEntryStatusList.Codes.AwaitingSupplementaryDeclaration;
			}
			else
			{
				return AISEntryStatusList.Codes.Released;
			}
		}

		IM429GoodsItemProvider[] goodsItems;

		public void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee)
		{
			if (messageAttachee is IAISMessageAttachee aisMessageAttachee)
			{
				aisMessageAttachee.SetEntryReleaseDate(ReleaseDate);
				aisMessageAttachee.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.CustomsCleared, ReleaseDate.ToOffset()));

				foreach (var requestedDocument in aisMessageAttachee.RequestedDocumentsProvider.RequestedDocuments.Where(x => !x.CSI_Status.EqualsIgnoringCase(RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived)))
				{
					requestedDocument.CSI_Status = RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
				}

				aisMessageAttachee.PopulateConfirmedDutiesAndTaxes(GoodsItems);
			}
		}
	}
}
