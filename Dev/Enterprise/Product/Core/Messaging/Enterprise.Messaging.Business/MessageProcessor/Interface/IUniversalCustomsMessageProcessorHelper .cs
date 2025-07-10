using System;

namespace Enterprise.Messaging.Business.MessageProcessor
{
	public interface IUniversalCustomsMessageProcessorHelper
	{
		IDisposable SuspendReportSettingEM_LinkedObject();
	}
}
