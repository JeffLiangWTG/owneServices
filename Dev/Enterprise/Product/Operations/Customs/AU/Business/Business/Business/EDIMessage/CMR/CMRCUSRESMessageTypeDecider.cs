using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCUSRESMessageTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			string eM_MessageType = row[EDIMessage.Schema.EM_MessageType].ToString().Trim();
			switch (eM_MessageType)
			{
				case CMRMessage.CMRMessageTypes.AIRAAR:
					return typeof(CMRAIRAARRMessage);
				case CMRMessage.CMRMessageTypes.AIRCR:
					return typeof(CMRAIRCRRMessage);
				case CMRMessage.CMRMessageTypes.AIRIAR:
					return typeof(CMRAIRIARRMessage);
				case CMRMessage.CMRMessageTypes.AIRINT:
					return typeof(CMRAIRINTRMessage);
				case CMRMessage.CMRMessageTypes.AIROUT:
					return typeof(CMRAIROUTRMessage);
				case CMRMessage.CMRMessageTypes.CARLST:
					return typeof(CMRCARLSTRMessage);
				//				case CMRMessage.CMRMessageTypes.CARMOV : return typeof(CMRCARMOVRMessage);

				case CMRMessage.CMRMessageTypes.CLNTDUP:
					return typeof(CMRCLNTDUPMessage);
				case CMRMessage.CMRMessageTypes.CLREG:
					return typeof(CMRCLREGRMessage);
				case CMRMessage.CMRMessageTypes.CTOREC:
					return typeof(CMRCTORECRMessage);
				case CMRMessage.CMRMessageTypes.CTOREM:
					return typeof(CMRCTOREMRMessage);
				case CMRMessage.CMRMessageTypes.DEPART:
					return typeof(CMRDEPARTRMessage);
				case CMRMessage.CMRMessageTypes.DEPREC:
					return typeof(CMRDEPRECRMessage);
				case CMRMessage.CMRMessageTypes.DEPREL:
					return typeof(CMRDEPRELRMessage);
				case CMRMessage.CMRMessageTypes.EMM:
					return typeof(CMREMMRMessage);
				case CMRMessage.CMRMessageTypes.ESM:
					return typeof(CMRESMRMessage);
				case CMRMessage.CMRMessageTypes.EXDR:
					return typeof(CMREXDRMessage);
				case CMRMessage.CMRMessageTypes.EXREL:
					return typeof(CMREXRELMessage);
				case CMRMessage.CMRMessageTypes.EXPED1:
					return typeof(CMREXPED1RMessage);
				case CMRMessage.CMRMessageTypes.EXPED2:
					return typeof(CMREXPED2RMessage);
				case CMRMessage.CMRMessageTypes.PRODIS:
					return typeof(CMRPRODISRMessage);
				case CMRMessage.CMRMessageTypes.RACEAN:
					return typeof(CMRRACEANRMessage);
				case CMRMessage.CMRMessageTypes.RCR:
					return typeof(CMRRCRRMessage);
				case CMRMessage.CMRMessageTypes.SEAAAR:
					return typeof(CMRSEAAARRMessage);
				case CMRMessage.CMRMessageTypes.SEACR:
					return typeof(CMRSEACRRMessage);
				case CMRMessage.CMRMessageTypes.SEAIAR:
					return typeof(CMRSEAIARRMessage);
				case CMRMessage.CMRMessageTypes.SEAINT:
					return typeof(CMRSEAINTRMessage);
				case CMRMessage.CMRMessageTypes.SEAOUT:
					return typeof(CMRSEAOUTRMessage);
				case CMRMessage.CMRMessageTypes.STREQ:
					return typeof(CMRSTREQRMessage);
				case CMRMessage.CMRMessageTypes.UBMREQE:
					return typeof(CMRUBMREQEMessage);
				case CMRMessage.CMRMessageTypes.WARREL:
					return typeof(CMRWARRELRMessage);
				case CMRMessage.CMRMessageTypes.WARRET:
					return typeof(CMRWARRETRMessage);

				case CMRMessage.CMRMessageTypes.ATD:
					return typeof(CMRATDMessage);
				case CMRMessage.CMRMessageTypes.CARMOV:
					return typeof(CMRCARMOVMessage);
				case CMRMessage.CMRMessageTypes.CARREP:
					return typeof(CMRCARREPMessage);
				case CMRMessage.CMRMessageTypes.CARST:
					return typeof(CMRCARSTMessage);
				case CMRMessage.CMRMessageTypes.CONREM:
					return typeof(CMRCONREMMessage);
				case CMRMessage.CMRMessageTypes.DEPARR:
					return typeof(CMRDEPARRMessage);
				case CMRMessage.CMRMessageTypes.DOCS:
					return typeof(CMRDOCSMessage);
				case CMRMessage.CMRMessageTypes.DRWBCK:
					return typeof(CMRDRWBCKRMessage);
				case CMRMessage.CMRMessageTypes.EXAM:
					return typeof(CMREXAMMessage);
				case CMRMessage.CMRMessageTypes.IDL:
					return typeof(CMRIDLMessage);
				case CMRMessage.CMRMessageTypes.IMD:
					return typeof(CMRIMDRMessage);
				case CMRMessage.CMRMessageTypes.IMPED1:
					return typeof(CMRIMPED1RMessage);
				case CMRMessage.CMRMessageTypes.IMPED2:
					return typeof(CMRIMPED2RMessage);
				case CMRMessage.CMRMessageTypes.MOVAPP:
					return typeof(CMRMOVAPPRMessage);
				case CMRMessage.CMRMessageTypes.PAYDEF:
					return typeof(CMRPAYDEFMessage);
				case CMRMessage.CMRMessageTypes.PAYEXC:
					return typeof(CMRPAYEXCMessage);
				case CMRMessage.CMRMessageTypes.PAYINV:
					return typeof(CMRPAYINVMessage);
				case CMRMessage.CMRMessageTypes.PAYOUT:
					return typeof(CMRPAYOUTMessage);
				case CMRMessage.CMRMessageTypes.PAYREC:
					return typeof(CMRPAYRECMessage);
				case CMRMessage.CMRMessageTypes.PAYSTD:
					return typeof(CMRPAYSTDRMessage);
				case CMRMessage.CMRMessageTypes.REFACC:
					return typeof(CMRREFACCMessage);
				case CMRMessage.CMRMessageTypes.REFREJ:
					return typeof(CMRREFREJMessage);
				case CMRMessage.CMRMessageTypes.SAC:
					return typeof(CMRSACRMessage);
				case CMRMessage.CMRMessageTypes.SAM:
					return typeof(CMRSAMMessage);
				case CMRMessage.CMRMessageTypes.SEI:
					return typeof(CMRSEIMessage);
				case CMRMessage.CMRMessageTypes.SEQ:
					return typeof(CMRSEQRMessage);
				case CMRMessage.CMRMessageTypes.DSA:
					return typeof(CMRDSAMessage);
				case CMRMessage.CMRMessageTypes.UBMREQR:
					return typeof(CMRUBMREQRMessage);
				case CMRMessage.CMRMessageTypes.XRAYADV:
					return typeof(CMRXRAYADVMessage);

				default:
					return typeof(CMRCUSRESMessage);
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
