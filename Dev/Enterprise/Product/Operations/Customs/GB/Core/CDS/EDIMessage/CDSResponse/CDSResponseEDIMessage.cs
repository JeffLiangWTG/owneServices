using System;
using System.Data;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.CDS.CDSResponse;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSResponseEDIMessage : CDSEDIMessage<Response>
	{
		public CDSResponseEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CDSEDIMessageTypeList.Codes.Response;
			EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		}

		public ResponseFunction ResponseFunction => responseFunction ?? (responseFunction = MessageDataObject.GetResponseFunction());
		ResponseFunction responseFunction;

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

		public CDSResponseEDIMessagePrettier Prettier => prettier ?? (prettier = new CDSResponseEDIMessagePrettier(this));
		CDSResponseEDIMessagePrettier prettier;
	}
}
