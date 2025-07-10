using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class NilAmendmentMessageBuilder : AmendmentMessageBuilder, IGbCDSMessageBuilder
	{
		public NilAmendmentMessageBuilder(JobDeclarationMessageSendingObject objectToSend, string functionCode) : base(objectToSend, functionCode)
		{
		}

		protected override AmendmentObjectWrapper GetAmendmentWrapper()
		{
			ObjectToSend.AmendmentDetails = new AmendmentDetails(null, "", "");
			ObjectToSend.AmendmentDetails.Amendments = AmendmentMessageHelper.Instance.GetFakeNilAmendment(ObjectToSend);
			return ObjectToSend.AmendmentDetails.Amendments;
		}

		protected override void AddExtraData(CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration metaDeclaration)
		{
			var amendments = ObjectToSend.AmendmentDetails.Amendments;
			var mucr = ObjectToSend.Header.CH_MasterUCR;
			var mucrPreviousDocumentIndex = amendments.Differences?.OfType<NilAmendmentChange>().FirstOrDefault()?.MasterUCRSequence ?? 1;

			var previousDocument = new DeclarationGoodsShipmentPreviousDocument
			{
				ID = new PreviousDocumentIdentificationIDType() { Value = mucr }
			};
			var prevDocsList = new List<DeclarationGoodsShipmentPreviousDocument>() { previousDocument };
			for (var i = 1; i < mucrPreviousDocumentIndex; i++)
			{
				prevDocsList.Insert(0, new DeclarationGoodsShipmentPreviousDocument());
			}

			metaDeclaration.GoodsShipment = new DeclarationGoodsShipment()
			{
				PreviousDocument = prevDocsList.ToArray()
			};
		}
	}
}
