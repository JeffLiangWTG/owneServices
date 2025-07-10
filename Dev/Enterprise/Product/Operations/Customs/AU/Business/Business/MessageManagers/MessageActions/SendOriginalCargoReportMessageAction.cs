using System.Collections;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SendOriginalCargoReportMessageAction : BaseMessageAction
	{
		public SendOriginalCargoReportMessageAction()
		{
		}

		protected override SingleMessageManager[] OnWhichMessagesShouldWeSend(SingleMessageManager[] allManagers)
		{
			ArrayList result = new ArrayList();
			foreach (SingleMessageManager manager in allManagers)
			{
				if (manager.GetType() == typeof(CusSCAHouseSEACRManager))
				{
					result.Add(manager);
				}
			}
			return (SingleMessageManager[])result.ToArray(typeof(SingleMessageManager));
		}
	}
}
