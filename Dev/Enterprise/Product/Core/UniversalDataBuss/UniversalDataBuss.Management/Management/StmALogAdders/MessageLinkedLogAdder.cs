using CargoWise.Common;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using EventDataObject = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.UniversalDataBuss.Management
{
	class MessageLinkedLogAdder : StmALogAdder
	{
		internal MessageLinkedLogAdder(
			IEDIMessage message,
			Event eventType,
			EventDataObject eventDataObject,
			IEventDataContextManager dataContextManager,
			UniversalObjectFactory factory,
			ISimpleLogger logger)
			: base(eventType, eventDataObject, dataContextManager, factory, logger)
		{
			this.message = Argument.NotNull(message, "IEDIMessage message");
		}
		readonly IEDIMessage message;

		protected override StmALog AddNewLogToParentCore(IStmALogParent logParent)
		{
			var stmALog = base.AddNewLogToParentCore(logParent);
			message.AddUniversalDataLink(stmALog);
			return stmALog;
		}
	}
}
