using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSDeclarationInfoResponseEDIMessage : CDSEDIMessage<CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.DeclarationStatusResponse>
	{
		public CDSDeclarationInfoResponseEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CDSEDIMessageTypeList.Codes.QueryResponse;
			EM_MessageSubType = CDSEDIMessageTypeList.Codes.ConversationID;
			EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get => !EM_MessageText.IsEmpty ? Prettier.MakeHumanReadable() : base.EM_MessageInterpretation;
			set
			{
				if (!EM_MessageText.IsEmpty)
				{
					throw new NotSupportedException("Setting EM_MessageInterpretation is not supported.");
				}
				base.EM_MessageInterpretation = value;
			}
		}

		protected override ZGuid EHubTrackingId
		{
			get
			{
				if (ZGuid.TryParse(EM_ApplicationReference, out var zGuid))
				{
					return zGuid;
				}

				return base.EHubTrackingId;
			}
		}

		public CDSEDIMessagePrettier Prettier => prettier ?? (prettier = DeclarationInfoResponse.GetPrettier(this));
		CDSEDIMessagePrettier prettier;

		public new DeclarationInfoResponse MessageDataObject => messageDataObject ?? (messageDataObject = DeclarationInfoResponse.GetResponse(EM_MessageText));
		DeclarationInfoResponse messageDataObject;
	}
}
