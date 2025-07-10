using System.Collections;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SendOrignalUnderbondMessageAction : BaseMessageAction
	{
		public SendOrignalUnderbondMessageAction()
		{
		}

		protected override Customs.Business.SingleMessageManager[] OnWhichMessagesShouldWeSend(Customs.Business.SingleMessageManager[] allManagers)
		{
			ArrayList result = new ArrayList();
			foreach (Customs.Business.SingleMessageManager manager in allManagers)
			{
				if (manager.GetType() == typeof(CusUnderbondUBMREQManager))
				{
					result.Add(manager);
				}
			}
			return (Customs.Business.SingleMessageManager[])result.ToArray(typeof(Customs.Business.SingleMessageManager));
		}
	}
}
