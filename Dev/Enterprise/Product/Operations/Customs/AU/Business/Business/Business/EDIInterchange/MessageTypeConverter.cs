using System;
using System.Collections.Generic;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class MessageTypeConverter
	{
		public MessageTypeConverter()
		{
			dictionary = new Dictionary<string, Type>();
			dictionaryForERM = new Dictionary<string, Type>();
			AddMessageTypeToHashtable(typeof(CMRAIRAARRMessage), CMRMessage.CMRMessageTypes.AIRAAR);
			AddMessageTypeToHashtable(typeof(CMRAIRIARRMessage), CMRMessage.CMRMessageTypes.AIRIAR);
			AddMessageTypeToHashtable(typeof(CMRAIRCRRMessage), CMRMessage.CMRMessageTypes.AIRCR);
			AddMessageTypeToHashtable(typeof(CMRAIRINTRMessage), CMRMessage.CMRMessageTypes.AIRINT);
			AddMessageTypeToHashtable(typeof(CMRAIROUTRMessage), CMRMessage.CMRMessageTypes.AIROUT);
			AddMessageTypeToHashtable(typeof(CMRCARLSTRMessage), CMRMessage.CMRMessageTypes.CARLST);
			AddMessageTypeToHashtable(typeof(CMRCTORECRMessage), CMRMessage.CMRMessageTypes.CTOREC);
			AddMessageTypeToHashtable(typeof(CMRCTOREMRMessage), CMRMessage.CMRMessageTypes.CTOREM);
			AddMessageTypeToHashtable(typeof(CMRDEPARTRMessage), CMRMessage.CMRMessageTypes.DEPART);
			AddMessageTypeToHashtable(typeof(CMRDEPRECRMessage), CMRMessage.CMRMessageTypes.DEPREC);
			AddMessageTypeToHashtable(typeof(CMRDEPRELRMessage), CMRMessage.CMRMessageTypes.DEPREL);
			AddMessageTypeToHashtable(typeof(CMREMMRMessage), CMRMessage.CMRMessageTypes.EMM);
			AddMessageTypeToHashtable(typeof(CMRESMRMessage), CMRMessage.CMRMessageTypes.ESM);
			AddMessageTypeToHashtable(typeof(CMREXDRMessage), CMRMessage.CMRMessageTypes.EXD);
			AddMessageTypeToHashtable(typeof(CMREXRELMessage), CMRMessage.CMRMessageTypes.EXREL);
			AddMessageTypeToHashtable(typeof(CMRCLREGRMessage), CMRMessage.CMRMessageTypes.CLREG);
			AddMessageTypeToHashtable(typeof(CMREXPED1RMessage), CMRMessage.CMRMessageTypes.EXPED1);
			AddMessageTypeToHashtable(typeof(CMREXPED2RMessage), CMRMessage.CMRMessageTypes.EXPED2);
			AddMessageTypeToHashtable(typeof(CMRPRODISRMessage), CMRMessage.CMRMessageTypes.PRODIS);
			AddMessageTypeToHashtable(typeof(CMRRACEANRMessage), CMRMessage.CMRMessageTypes.RACEAN);
			AddMessageTypeToHashtable(typeof(CMRRCRRMessage), CMRMessage.CMRMessageTypes.RCR);
			AddMessageTypeToHashtable(typeof(CMRSEAAARRMessage), CMRMessage.CMRMessageTypes.SEAAAR);
			AddMessageTypeToHashtable(typeof(CMRSEACRRMessage), CMRMessage.CMRMessageTypes.SEACR);
			AddMessageTypeToHashtable(typeof(CMRSEAIARRMessage), CMRMessage.CMRMessageTypes.SEAIAR);
			AddMessageTypeToHashtable(typeof(CMRSEAINTRMessage), CMRMessage.CMRMessageTypes.SEAINT);
			AddMessageTypeToHashtable(typeof(CMRSEAOUTRMessage), CMRMessage.CMRMessageTypes.SEAOUT);
			AddMessageTypeToHashtable(typeof(CMRSTREQRMessage), CMRMessage.CMRMessageTypes.STREQ);
			AddMessageTypeToHashtable(typeof(CMRUBMREQEMessage), CMRMessage.CMRMessageTypes.UBMREQE);
			AddMessageTypeToHashtable(typeof(CMRWARRELRMessage), CMRMessage.CMRMessageTypes.WARREL);
			AddMessageTypeToHashtable(typeof(CMRWARRETRMessage), CMRMessage.CMRMessageTypes.WARRET);
			AddMessageTypeToHashtable(typeof(CMRATDMessage), CMRMessage.CMRMessageTypes.ATD);
			AddMessageTypeToHashtable(typeof(CMRCARMOVMessage), CMRMessage.CMRMessageTypes.CARMOV);
			AddMessageTypeToHashtable(typeof(CMRCARREPMessage), CMRMessage.CMRMessageTypes.CARREP);
			AddMessageTypeToHashtable(typeof(CMRCARSTMessage), CMRMessage.CMRMessageTypes.CARST);
			AddMessageTypeToHashtable(typeof(CMRCONREMMessage), CMRMessage.CMRMessageTypes.CONREM);
			AddMessageTypeToHashtable(typeof(CMRDEPARRMessage), CMRMessage.CMRMessageTypes.DEPARR);
			AddMessageTypeToHashtable(typeof(CMRDOCSMessage), CMRMessage.CMRMessageTypes.DOCS);
			AddMessageTypeToHashtable(typeof(CMRDRWBCKRMessage), CMRMessage.CMRMessageTypes.DRWBCK);
			AddMessageTypeToHashtable(typeof(CMREXAMMessage), CMRMessage.CMRMessageTypes.EXAM);
			AddMessageTypeToHashtable(typeof(CMRIDLMessage), CMRMessage.CMRMessageTypes.IDL);
			AddMessageTypeToHashtable(typeof(CMRIMDRMessage), CMRMessage.CMRMessageTypes.IMD);
			AddMessageTypeToHashtable(typeof(CMRIMPED1RMessage), CMRMessage.CMRMessageTypes.IMPED1);
			AddMessageTypeToHashtable(typeof(CMRIMPED2RMessage), CMRMessage.CMRMessageTypes.IMPED2);
			AddMessageTypeToHashtable(typeof(CMRMOVAPPRMessage), CMRMessage.CMRMessageTypes.MOVAPP);
			AddMessageTypeToHashtable(typeof(CMRPAYDEFMessage), CMRMessage.CMRMessageTypes.PAYDEF);
			AddMessageTypeToHashtable(typeof(CMRPAYEXCMessage), CMRMessage.CMRMessageTypes.PAYEXC);
			AddMessageTypeToHashtable(typeof(CMRPAYINVMessage), CMRMessage.CMRMessageTypes.PAYINV);
			AddMessageTypeToHashtable(typeof(CMRPAYOUTMessage), CMRMessage.CMRMessageTypes.PAYOUT);
			AddMessageTypeToHashtable(typeof(CMRPAYRECMessage), CMRMessage.CMRMessageTypes.PAYREC);
			AddMessageTypeToHashtable(typeof(CMRREFACCMessage), CMRMessage.CMRMessageTypes.REFACC);
			AddMessageTypeToHashtable(typeof(CMRREFREJMessage), CMRMessage.CMRMessageTypes.REFREJ);
			AddMessageTypeToHashtable(typeof(CMRSACRMessage), CMRMessage.CMRMessageTypes.SAC);
			AddMessageTypeToHashtable(typeof(CMRSAMMessage), CMRMessage.CMRMessageTypes.SAM);
			AddMessageTypeToHashtable(typeof(CMRSEQRMessage), CMRMessage.CMRMessageTypes.SEQ);
			AddMessageTypeToHashtable(typeof(CMRSEIMessage), CMRMessage.CMRMessageTypes.SEI);
			AddMessageTypeToHashtable(typeof(CMRDSAMessage), CMRMessage.CMRMessageTypes.DSA);
			AddMessageTypeToHashtable(typeof(CMRUBMREQRMessage), CMRMessage.CMRMessageTypes.UBMREQR);
			AddMessageTypeToHashtable(typeof(CMRXRAYADVMessage), CMRMessage.CMRMessageTypes.XRAYADV);
			AddMessageTypeToHashtable(typeof(CMRPAYSTDRMessage), CMRMessage.CMRMessageTypes.PAYSTD);
			AddMessageTypeToHashtable(typeof(CMRCLNTDUPMessage), CMRMessage.CMRMessageTypes.CLNTDUP);
		}
		readonly Dictionary<string, Type> dictionary;
		readonly Dictionary<string, Type> dictionaryForERM;

		void AddMessageTypeToHashtable(Type typeToAdd, string message3CharacterCode)
		{
			string name = typeToAdd.Name.Substring(3, typeToAdd.Name.Length - 10);
			dictionary.Add(name, typeToAdd);
			dictionaryForERM.Add(message3CharacterCode, typeToAdd);
		}

		internal Type GetTypeForMessageCode(string documentName)
		{
			Type result;
			if (!dictionary.TryGetValue(documentName, out result))
			{
				result = typeof(CMRCUSRESMessage);
			}
			return result;
		}

		internal Type GetTypeForERMMessageCode(string documentName)
		{
			Type result;
			if (!dictionaryForERM.TryGetValue(documentName, out result))
			{
				result = typeof(CMRCUSRESMessage);
			}
			return result;
		}
	}
}
