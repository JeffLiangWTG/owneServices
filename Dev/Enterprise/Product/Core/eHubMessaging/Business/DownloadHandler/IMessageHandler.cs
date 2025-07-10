using System;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Implementor classes referenced dynamically by SupportedSchemaNameAttribute")]
	public interface IMessageHandler
	{
		bool SaveMessage(IeHubMessage message, GlbCompany company, INotifications notifier);
		IMessageHandlerResult SaveMessageFromAdapter(IeHubMessage message);

		event Action<EDIInterchange> InterchangeCreated;
		event Action<EDIMessage> MessageCreated;
		event Action<EDIInterchange> BeforeSavingInterchange;
	}
}
