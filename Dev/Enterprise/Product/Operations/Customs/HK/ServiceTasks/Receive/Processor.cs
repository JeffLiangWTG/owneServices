using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.HK.ServiceTasks
{
	public class Processor : BaseMessageProcessor
	{
		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			var result = base.GetMessageProcessors();
			result.Add(new TraxonResponseProcessor(Logger));
			return result;
		}

		protected override ZQuery ValidBranchesForMessageFilter => new ZQuery();

		public override void Dispose()
		{
			MessageProcessors.OfType<IDisposable>().ForEach(x => x.Dispose());
			base.Dispose();
		}
	}
}
