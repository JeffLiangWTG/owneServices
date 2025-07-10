using System;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class InvalidationWrapper : IInvalidation
	{
		InvalidationWrapper(TP5MessageSendingObject sendingObject)
		{
			this.sendingObject = sendingObject;
		}

		public static InvalidationWrapper New(TP5MessageSendingObject sendingObject) => new InvalidationWrapper(sendingObject);

		public DateTime? RequestDateAndTime => ZDateTime.UtcNow.ToDateTime();

		public DateTime? DecisionDateAndTime => null;

		public bool? Decision => false;

		public bool InitiatedByCustoms => false;

		public string Justification => sendingObject.Justification;

		public string JustificationReglementaire => sendingObject.JustificationCode;

		readonly TP5MessageSendingObject sendingObject;
	}
}
