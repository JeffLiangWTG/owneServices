using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRIncomingMessageTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			switch (row[EDIMessage.Schema.EM_MessageType].ToString().Trim())
			{
				case CMRMessage.CMRMessageTypes.CONTRL:
					return typeof(CMRCONTRLMessage);
				case CMRMessage.CMRMessageTypes.ATD:
					return typeof(CMRATDMessage);
				default:
					return new CMRCUSRESMessageTypeDecider().GetTypeForLoad(row, factory);
			}
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}
	}
}
