using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	internal static class CMRAutoUnderbondSender
	{
		public static void CheckUnderbondsAndSend(IEnumerable<CusUnderbond> underbonds)
		{
			underbonds.Where(AutoUnderbondShouldBeSent).ForEach(SendOriginalUnderbond);
		}

		static bool AutoUnderbondShouldBeSent(CusUnderbond underbond)
		{
			return
				!underbond.HasAlreadyBeenSent &&
				underbond.C4_Status == CMRUnderbondStatuses.Codes.UnderbondSendingDelayed &&
				CheckUnderbondStatusCode(underbond.UnderbondStatus.Code);
		}

		static bool CheckUnderbondStatusCode(ZString code)
		{
			return
				code == CMRBaseStatuses.Codes.NotSent ||
				code == CMRBaseStatuses.Codes.OriginalRejected ||
				code.IsEmpty;
		}

		static void SendOriginalUnderbond(CusUnderbond underbond)
		{
			new CusUnderbondUBMREQManager(underbond).GenerateOriginalMessages(underbond);
			if (underbond.UnderbondStatus.Code == CMRBaseStatuses.Codes.OriginalRejected)
			{
				underbond.C4_Status = ZString.Empty;
			}
			underbond.HasAlreadyBeenSent = true;
		}
	}
}
