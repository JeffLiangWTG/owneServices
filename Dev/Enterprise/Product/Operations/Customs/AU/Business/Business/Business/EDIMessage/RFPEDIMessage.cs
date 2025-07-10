using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using ReceiveTransmitList = Enterprise.Messaging.Integration.ReceiveTransmitList.Codes;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RFPEDIMessage : EDIMessage
	{
		public RFPEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get => EM_ReceiveTransmit == ReceiveTransmitList.Receive
					&& EM_ApplicationCode == EDIMessage.ApplicationCodes.UniversalDataMessaging
					&& EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalEvent
				? MessageInterpretation
				: base.EM_MessageInterpretation;
			set => base.EM_MessageInterpretation = value;
		}

		ZString MessageInterpretation
		{
			get
			{
				if (messageInterpretation.IsEmpty)
				{
					messageInterpretation = RFPMessageInterpretationGenerator.GetInterpretatedHTML(this);
				}
				return messageInterpretation;
			}
		}
		ZString messageInterpretation;
	}
}
