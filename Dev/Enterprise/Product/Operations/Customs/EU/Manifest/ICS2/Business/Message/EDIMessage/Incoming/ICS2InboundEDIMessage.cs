using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ICS2InboundEDIMessage : EDIMessage
	{
		public ICS2InboundEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public const string UndefinedSubType = "XXX";

		protected override bool ResetToQueuedStatusPreservesMessageType => true;

		protected override bool ResetToQueuedStatusPreservesMessageSubType => true;

		protected override EDIMessageLookups GetNewLookups() => new ICS2InboundEDIMessageLookups(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.IC2;
			EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		}

		protected override IEnumerable<Type> GetAdditionalRegisteredLinkedObjectTypes()
		{
			yield return typeof(AsycudaManifestHeader);
		}
	}
}
