using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SendWithdrawalMessageProcessor : Integration.Customs.AU.ISendWithdrawalMessageProcessor, IProcessor
	{
		public SendWithdrawalMessageProcessor(Customs.Business.CusHAWB cusHawb)
		{
			this.cusHawb = cusHawb as CusHAWB;
		}
		readonly CusHAWB cusHawb;

		public void Process(INotifications notifications, CancellationToken token = default(CancellationToken))
		{
			if (cusHawb != null)
			{
				var collector = new WithdrawalManagerCollector(new[] { new CusHAWBAIRCRMessageManager(cusHawb) }, true);
				collector.DoAction();
			}
		}
	}
}
