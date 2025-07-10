using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestsSubclassesOf(typeof(BaseMessageProcessor<>))]
public abstract class MessageProcessorTestCase<TMessageProcessor, TInboundProvider> : TestCaseWithFactory
	where TInboundProvider : class
	where TMessageProcessor : BaseMessageProcessor<TInboundProvider>
{
	public void TestMessageFriendlyName()
	{
		AssertEquals(ExpectedMessageFriendlyName, Processor.MessageFriendlyName);
	}

	public void TestMessageInterpreterType()
	{
		AssertEquals(ExpectedMessageInterpreterType, typeof(TMessageProcessor).GetProperty("MessageInterpreterType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Processor));
	}

	protected StmNote GetStmNote(BEMessage message) => GetStmNote(message.PK, EDIMessageSchema.Constants.TableName, "Processing Log");

	protected StmNote GetStmNote(ZGuid parentID, ZString tableName, ZString description)
	{
		var stmNoteQuery = new ZQuery(StmNoteSchema.ST_Table, tableName);
		stmNoteQuery.AddToFilter(StmNoteSchema.ST_ParentID, parentID);
		stmNoteQuery.AddToFilter(StmNoteSchema.ST_Description, description);
		return Factory.LoadTop1<StmNote>(stmNoteQuery);
	}

	protected abstract string ExpectedMessageFriendlyName { get; }

	protected abstract Type ExpectedMessageInterpreterType { get; }

	protected abstract TMessageProcessor Processor { get; }

	protected BEMessage CreateIncomingMessage(BusinessObjectFactory factory)
	{
		var ediMessage = factory.New<BEMessage>();
		ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		return ediMessage;
	}

	protected static bool CheckMessageSequenceIsValid(BEMessage ediMessage, LoggingInformation logger) => ediMessage.EM_Status != EDIMessage.Status.Queued || !logger.Logs.Any(log => log.Type == LogType.Information && log.Message.StartsWith("The message was set back to QUEUED"));
}
