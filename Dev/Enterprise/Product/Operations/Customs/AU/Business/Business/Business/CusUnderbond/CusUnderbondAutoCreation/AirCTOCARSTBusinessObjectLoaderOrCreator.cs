using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirCTOCARSTBusinessObjectLoaderOrCreator : CARSTBusinessObjectLoaderOrCreator
	{
		protected internal override bool IsInterestedInCARST(CMRCARSTMessage message)
		{
			bool result = message.IsAir && (StatusAdviceRelatedToCTO(message) || !message.MAWB.IsEmpty && CTOMAWBRecordExists(message));
			return result;
		}

		protected internal override BusinessObject[] LoadOrCreateRecordForMessageCore(CMRCARSTMessage message)
		{
			var hawb = CTOCusHAWB.Load(message.Factory, message);
			if (hawb == null)
			{
				if ((bool)Env.Registry.RawRegistry.AUCAutoSendUnderbondOnCARST.Value)
				{
					CMRAutoUnderbondSender.CheckUnderbondsAndSend(GetUnderbondsForMAWB(message.MAWB, message.Factory));
				}
			}
			else if (!message.TranshipmentNumber.IsEmpty)
			{
				hawb.CS_TranshipmentEntryNum = message.TranshipmentNumber;
			}
			return new BusinessObject[] { hawb }; // Consider if you need to match more than one.
		}

		protected bool CTOMAWBRecordExists(CMRCARSTMessage message)
		{
			bool result = false;
			if (!message.MAWB.IsEmpty)
			{
				ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(CusHAWBBase));
				dbOnlyQuery.AddToFilter(CusHAWBSchema.CS_HAWB, message.MAWB);
				ZDBOnlySubQuery cusMAWBQuery = new ZDBOnlySubQuery(typeof(CusMAWBBase), CusHAWBSchema.CS_CM);
				cusMAWBQuery.AddToFilter(CusMAWBSchema.CM_ApplicationCode, CusMAWBBase.Loader.CMRApplicationCodes);
				cusMAWBQuery.AddToFilter(CusMAWBSchema.CM_IsCTOMAWB, true);
				dbOnlyQuery.AddSubQuery(cusMAWBQuery, JoinCondition.And);
				result = message.Factory.LoadTop1<CusHAWBBase>(dbOnlyQuery) != null;
			}
			return result;
		}

		static IEnumerable<CusUnderbond> GetUnderbondsForMAWB(ZString mawbNumber, BusinessObjectFactory factory)
		{
			var mawbSubQuery = new ZDBOnlySubQuery(typeof(CusMAWBBase), CusHAWBSchema.CS_CM);
			mawbSubQuery.AddToFilter(CusMAWBSchema.CM_MAWB, mawbNumber);
			mawbSubQuery.AddToFilter(CusMAWBSchema.CM_ApplicationCode, CusMAWBBase.Loader.CMRApplicationCodes);
			mawbSubQuery.AddToFilter(CusMAWBBase.Loader.DateQuery);

			var hawbSubQuery = new ZDBOnlySubQuery(typeof(CusHAWBBase), EDIMessageSchema.EM_LinkUniqueID);
			hawbSubQuery.AddSubQuery(mawbSubQuery, JoinCondition.And);

			var messagesSubQuery = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_LinkUniqueID);
			messagesSubQuery.AddToFilter(EDIMessageSchema.EM_MessageType, CMRMessage.CMRMessageTypes.CARST);
			messagesSubQuery.AddToFilter(EDIMessageSchema.EM_LinkTable, CusHAWBSchema.Constants.TableName);
			messagesSubQuery.AddSubQuery(hawbSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(CusUnderbond));
			query.AddToFilter(CusUnderbondSchema.C4_ParentTableCode, CusHAWBSchema.Constants.Prefix);
			query.AddToFilter(CusUnderbondSchema.C4_Status, CMRUnderbondStatuses.Codes.UnderbondSendingDelayed);
			query.AddSubQuery(CusUnderbondSchema.C4_ParentID, messagesSubQuery, JoinCondition.And);

			return factory.Load<CusUnderbond>(query);
		}
	}
}
