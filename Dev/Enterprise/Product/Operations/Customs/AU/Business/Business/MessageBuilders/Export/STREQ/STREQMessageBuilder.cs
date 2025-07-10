using System;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.GENRAL;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class STREQMessageBuilder : CMRGENRALMessageBuilder
	{
		public STREQMessageBuilder(ExportCustomsManifestLines exportLine)
			: this(exportLine.EL_CAN)
		{
			Messages = exportLine.Messages;
		}

		STREQMessageBuilder(ZString cAN)
		{
			MessageSubType = Common.MessageBuilders.MessageSubTypes.Request;
			this.cAN = cAN;
		}

		#region Implementation

		protected override int GetNumberOfOriginals(EDIMessageCollection messages, ZString messageCode, Common.MessageBuilders.MessageSubTypes messageSubType)
		{
			var originalsInDB = 0;
			foreach (var message in messages.GetMatchingMessages(EDIMessage.ApplicationCodes.CMR, new ZString[] { messageCode }, EDIMessage.Direction.Transmit))
			{
				if (message.IsInDatabase)
				{
					originalsInDB++;
				}
			}
			return originalsInDB + 1;
		}

		protected internal override void GenerateMessageText()
		{
			if (gENRAL == null)
			{
				MessageSubType = Common.MessageBuilders.MessageSubTypes.Request;
				gENRAL = new GENRALMessage();
				PopulateUNH();
				PopulateBGM();
				PopulateSegmentGroup1();
				PopulateUNT();
			}
		}

		protected internal override DocumentNameCodeList DocumentNameCode
		{
			get
			{
				return DocumentNameCodeList.StatusInformation;
			}
		}

		protected internal override ZString DocumentName
		{
			get
			{
				return "STREQ";
			}
		}

		protected internal override Type TypeOfMessage
		{
			get
			{
				return typeof(CMRSTREQMessage);
			}
		}

		protected internal override ZString EM_MessageType
		{
			get
			{
				return CMRMessage.CMRMessageTypes.STREQ;
			}
		}

		protected void PopulateSegmentGroup1()
		{
			if (!cAN.IsEmpty)
			{
				gENRAL.Group1[0].RFF[0].Reference.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.TransactionReferenceNumber;
				gENRAL.Group1[0].RFF[0].Reference.ReferenceIdentifier = cAN;
			}
		}

		protected ZString cAN;

		#endregion
	}
}
