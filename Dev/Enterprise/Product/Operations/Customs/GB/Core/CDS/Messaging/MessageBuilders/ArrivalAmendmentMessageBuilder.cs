using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class ArrivalAmendmentMessageBuilder : AmendmentMessageBuilder, IGbCDSMessageBuilder
	{
		public ArrivalAmendmentMessageBuilder(JobDeclarationMessageSendingObject objectToSend, string functionCode) : base(objectToSend, functionCode)
		{
		}

		public override ZString Build()
		{
			ZString messageText = string.Empty;

			var metaData = CreateMetaData();
			var metaDeclaration = CreateMetaDeclaration();

			var amendments = GetAmendmentWrapper();
			if (amendments?.AmendmentObjects.Any() ?? false)
			{
				metaDeclaration.Amendment = GetDeclarationAmendments(amendments.AmendmentObjects).ToArray();
			}
			else
			{
				metaDeclaration.Amendment = null;
			}

			messageText = metaData
				.SetDeclaration(metaDeclaration)
				.Serialize();

			if (amendments?.AmendmentObjects.Any() ?? false)
			{
				messageText = InsertAmendmentXml(messageText, amendments);
			}

			messageText = EnsureCorrectOrder(messageText);

			return messageText.FormatXml();
		}

		protected override AmendmentObjectWrapper GetAmendmentWrapper()
		{
			if (ObjectToSend.AmendmentDetails == null)
			{
				ObjectToSend.AmendmentDetails = new AmendmentDetails(null, "", "");
				ObjectToSend.AmendmentDetails.Amendments = new AmendmentObjectWrapper();
			}

			return ObjectToSend.AmendmentDetails.Amendments;
		}

		protected override ZString TypeCode => Constants.ThreeCharFunctionCodes.DeclarationArrivalNotification;

		protected override DeclarationAdditionalInformation[] CreateAdditionalInformation() => null;
	}
}
