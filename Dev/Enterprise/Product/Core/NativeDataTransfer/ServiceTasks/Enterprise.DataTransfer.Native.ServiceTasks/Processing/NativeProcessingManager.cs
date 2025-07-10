using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.DataTransfer.Native.ServiceTasks
{
	class NativeProcessingManager : BaseMessageProcessor<XmlEDIMessage>
	{
		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			var result = base.GetMessageProcessors();
			result.Add(new NativeMessageProcessor(Logger));
			return result;
		}

		protected override ZQuery ValidBranchesForMessageFilter
		{
			get { return new ZQuery(); }
		}

		protected override IDisposable TrySwitchUserContext(XmlEDIMessage message)
		{
			return NativeHandler.SetUserContext(message.Factory, null, new XmlSessionTracker(Logger), message);
		}
	}
}
