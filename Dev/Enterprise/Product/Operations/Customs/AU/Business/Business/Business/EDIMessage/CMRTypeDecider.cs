using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTypeDecider : TypeDecider, Integration.Customs.AU.ICMRTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			if (row[EDIMessage.Schema.EM_ReceiveTransmit].ToString() == EDIMessage.Direction.Receive)
			{
				return CMRIncomingMessage.TypeDecider.GetTypeForLoad(row, factory);
			}
			else
			{
				switch (row[EDIMessage.Schema.EM_MessageType].ToString().Trim())
				{
					case CMRMessage.CMRMessageTypes.CONTRL:
						return typeof(CMRContrlMessage);
					case CMRMessage.CMRMessageTypes.CTOREC:
						return typeof(CMRCTORECMessage);
					case CMRMessage.CMRMessageTypes.CTOREM:
						return typeof(CMRCTOREMMessage);
					case CMRMessage.CMRMessageTypes.DEPART:
						return typeof(CMRDEPARTMessage);
					case CMRMessage.CMRMessageTypes.DEPREC:
						return typeof(CMRDEPRECMessage);
					case CMRMessage.CMRMessageTypes.DEPREL:
						return typeof(CMRDEPRELMessage);
					case CMRMessage.CMRMessageTypes.EMM:
						return typeof(CMREMMMessage);
					case CMRMessage.CMRMessageTypes.ESM:
						return typeof(CMRESMMessage);
					case CMRMessage.CMRMessageTypes.EXD:
						return typeof(CMREXDMessage);
					case CMRMessage.CMRMessageTypes.STREQ:
						return typeof(CMRSTREQMessage);
					case CMRMessage.CMRMessageTypes.WARREL:
						return typeof(CMRWARRELMessage);
					case CMRMessage.CMRMessageTypes.WARRET:
						return typeof(CMRWARRETMessage);
					case CMRMessage.CMRMessageTypes.AIRCR:
						return typeof(CMRAIRCRMessage);
					case CMRMessage.CMRMessageTypes.SEACR:
						return typeof(CMRSEACRMessage);
					case CMRMessage.CMRMessageTypes.CARLST:
						return typeof(CMRCARLSTMessage);
					case CMRMessage.CMRMessageTypes.IMD:
						return typeof(CMRIMDMessage);
					case CMRMessage.CMRMessageTypes.PAYSTD:
						return typeof(CMRPAYSTDMessage);
					case CMRMessage.CMRMessageTypes.SAC:
						return typeof(CMRSACMessage);
					case CMRMessage.CMRMessageTypes.SEQ:
						return typeof(CMRSEQMessage);
					case CMRMessage.CMRMessageTypes.REFACC:
						return typeof(CMRREFACCMessage);
					case CMRMessage.CMRMessageTypes.DRWBCK:
						return typeof(CMRDRWBCKMessage);
					case CMRMessage.CMRMessageTypes.CLREG:
						return typeof(CMRCLREGMessage);
					default:
						return typeof(CMRMessage);
				}
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
