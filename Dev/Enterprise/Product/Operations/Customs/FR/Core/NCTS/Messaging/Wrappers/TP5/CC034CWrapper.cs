using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class CC034CWrapper : ICC034C
	{
		public CC034CWrapper(TP5MessageSendingObject sendingObject)
		{
			this.sendingObject = sendingObject;
			this.nctsHeader = Argument.NotNull((NctsHeader)sendingObject.NctsHeader, nameof(sendingObject));
		}

		readonly NctsHeader nctsHeader;
		readonly TP5MessageSendingObject sendingObject;

		public static CC034CWrapper New(TP5MessageSendingObject sendingObject) => sendingObject == null ? null : new CC034CWrapper(sendingObject);

		public IMessageEnveloppe MessageEnveloppe => messageEnveloppe ??= MessageEnveloppeWrapper.New(nctsHeader, "IE034");
		IMessageEnveloppe messageEnveloppe;

		public string MessageSender => FRConstants.NCTSMessage.Operator;

		public string MessageRecipient => FRConstants.NCTSMessage.NationalAdministration;

		public DateTime PreparationDateAndTime => ZDateTime.Now.ToDateTime();

		public string CorrelationIdentifier => ZString.Empty;

		public IOrganization Requester => requester ??= OrganizationWrapper.New(nctsHeader.MovementHeader.Representative.Organisation ?? nctsHeader.Principal.Organisation, sendingObject.RequesterRole);
		IOrganization requester;

		public ICollection<IGuaranteeReferenceForQuery> GuaranteeReference => guaranteeReference ??= nctsHeader.MovementHeader.Guarantees.Select(x => GuaranteeReferenceForQueryWrapper.New(sendingObject, x)).ToArray();
		ICollection<IGuaranteeReferenceForQuery> guaranteeReference;
	}
}
