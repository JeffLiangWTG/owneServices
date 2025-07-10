using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IL.Business.MessageProcessors;
using Enterprise.Customs.ManifestBase;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.IL.Business.Testing.MessageProcessors
{
	public abstract class BaseILBranchCustomsApplicationTypeMessageProcessorTest<TProccessor, TMessage> : TestCaseWithFactory where TProccessor : ILBranchCustomsApplicationTypeMessageProcessorBase where TMessage : ILEDIResponseMessage
	{
		public void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", ExpectedMessageFriendlyName, Processor.MessageFriendlyName);
		}

		public void TestApplicationCode()
		{
			AssertEquals("ApplicationCode", "ILC", Processor.ApplicationCode);
		}

		public void TestMessageTypesToInclude()
		{
			AssertEquals("MessageTypesToInclude", (ZString)ExpectedMessageTypesToInclude, Processor.MessageTypesToInclude.Single());
		}

		public void TestMessageSubTypesToInclude()
		{
			if (!string.IsNullOrEmpty(ExpectedMessageSubTypesToInclude))
			{
				AssertEquals("MessageSubTypesToInclude", (ZString)ExpectedMessageSubTypesToInclude, Processor.MessageSubTypesToInclude.Single());
			}
			else
			{
				Assert("No MessageSubTypesToInclude", true);
			}
		}

		public void TestLinkedObjectNotAssigned()
		{
			var message = GetBasicSuccessfullMessage();
			Processor.ProcessMessage(message);

			AssertNull("No linked object should be assigned", message.EM_LinkedObject);
		}

		public void TestGetBranch()
		{
			var message = GetBasicSuccessfullMessage();
			((IBusinessObjectState)message).ClearHasChangesIncludingChildren();
			var linkedBusinessObjectBranchPk = ZGuid.NewZGuid();

			var result = Processor.GetBranch(message, new LoggingInformation(), linkedBusinessObjectBranchPk);

			var wasChanged = ((IBusinessObjectState)message).HasChangesNotIncludingChildren;
			Assert("Message should remain unchanged", !wasChanged);

			Assert("DiscardReason should be empty", result.DiscardReason.IsEmpty);
			AssertEquals("Branch equals to linkedBusinessObjectBranchPk", linkedBusinessObjectBranchPk, result.ReturnValue);

			((IBusinessObjectState)message).ClearHasChangesIncludingChildren();

			result = Processor.GetBranch(message, new LoggingInformation(), ZGuid.Empty);

			wasChanged = ((IBusinessObjectState)message).HasChangesNotIncludingChildren;
			Assert("Message should remain unchanged", !wasChanged);

			Assert("DiscardReason should be empty", result.DiscardReason.IsEmpty);
			AssertEquals("Branch to message's branch", message.Branch.PK, result.ReturnValue);
		}

		public void TestGetLinkedBusinessObjectMetaData()
		{
			var message = GetBasicSuccessfullMessage();
			((IBusinessObjectState)message).ClearHasChangesIncludingChildren();
			var result = Processor.GetLinkedBusinessObjectMetaData(message, new LoggingInformation());

			var wasChanged = ((IBusinessObjectState)message).HasChangesNotIncludingChildren;
			Assert("Message should remain unchanged", !wasChanged);

			Assert("DiscardReason should be empty", result.DiscardReason.IsEmpty);

			if (ExpectedLinkedObject != null)
			{
				AssertNotNull("ReturnValue should not be null", result.ReturnValue);

				AssertEquals("LinkTableName should be as expected", ExpectedLinkedObject.TableName, result.ReturnValue.LinkTableName);
				AssertEquals("LinkUniqueID should be as expected", ExpectedLinkedObject.PK, result.ReturnValue.LinkUniqueID);
				AssertEquals("BranchPk should be as expected", ExpectedBranchPk, result.ReturnValue.BranchPk);
			}
		}

		public void TestGetSerializationKeysResult()
		{
			if (ExpectedLinkedObject == null)
			{
				Assert(true);
				return;
			}

			var loggingInformation = new LoggingInformation();
			var message = GetBasicSuccessfullMessage();
			((IBusinessObjectState)message).ClearHasChangesIncludingChildren();

			var linkedObjectMetaData = Processor.GetLinkedBusinessObjectMetaData(message, loggingInformation);

			CombineAssertions("Prerequisites", () =>
			{
				var wasChanged = ((IBusinessObjectState)message).HasChangesNotIncludingChildren;
				Assert("Message should remain unchanged", !wasChanged);

				AssertNotNull("ReturnValue of linked meta data should not be null", linkedObjectMetaData.ReturnValue);
				Assert("DiscardReason of linked meta data should be empty", linkedObjectMetaData.DiscardReason.IsEmpty);
			});

			AssertSerializationKeysResult("When message does not have attached object", message, linkedObjectMetaData.ReturnValue);

			message.EM_LinkedObject = ExpectedLinkedObject;

			AssertSerializationKeysResult("When message does not have attached object", message, new LinkedBusinessObjectMetaData(ZString.Empty, ZGuid.Empty, ZGuid.Empty, ZString.Empty));
		}

		protected virtual TMessage GetBasicSuccessfullMessage()
		{
			var message = Factory.New<TMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = BasicSuccessfulMessageText;

			return message;
		}

		protected TProccessor Processor => processor ??= CreateProcessor(new LoggingInformation());
		TProccessor processor;

		protected abstract TProccessor CreateProcessor(LoggingInformation loggingInformation);
		protected abstract string BasicSuccessfulMessageText { get; }
		protected abstract string ExpectedMessageFriendlyName { get; }
		protected abstract string ExpectedMessageTypesToInclude { get; }
		protected virtual string ExpectedMessageSubTypesToInclude => string.Empty;
		protected abstract BusinessObject ExpectedLinkedObject { get; }
		protected abstract ZGuid ExpectedBranchPk { get; }

		void AssertSerializationKeysResult(string scenarioName, TMessage message, LinkedBusinessObjectMetaData linkedObjectMetaData)
		{
			var expectedSerializationKeys = GetExpectedSerializationKeys();
			CombineAssertions(scenarioName, () =>
			{
				((IBusinessObjectState)message).ClearHasChangesIncludingChildren();
				var result = Processor.GetSerializationKeysResult(message, new LoggingInformation(), linkedObjectMetaData);
				var wasChanged = ((IBusinessObjectState)message).HasChangesNotIncludingChildren;
				Assert("Message should remain unchanged", !wasChanged);
				AssertNotNull("ReturnValue of serialization keys should not be null", result.ReturnValue);
				Assert("DiscardReason of serialization keys should be empty", result.DiscardReason.IsEmpty);

				AssertEquals("ResultType should be as expected", SerializationKeysResult.SerializationKeysResultType.KeysProvided, result.ReturnValue.ResultType);
				AssertContainsExactElementsInAnyOrder("Keys should be as expected", expectedSerializationKeys.ReturnValue.Keys, result.ReturnValue.Keys);
			});
		}

		ProcessingResult<SerializationKeysResult> GetExpectedSerializationKeys()
		{
			var keys = new HashSet<string>();
			switch (ExpectedLinkedObject)
			{
				case AsycudaManifestHeader manifestHeader:
					keys.Add(manifestHeader.AMA_JobReference);
					break;
				case CusEntryHeader entryheader:
					{
						keys.Add(entryheader.DeclarationReference);
						var lrn = entryheader.CH_BGMReference;
						if (!lrn.IsEmpty)
						{
							keys.Add(lrn);
						}

						break;
					}

				case ForwardingShipment shipment:
					keys.Add(shipment.JS_UniqueConsignRef);
					break;
				case ForwardingConsol consol:
					keys.Add(consol.JK_UniqueConsignRef);
					break;
				default:
					return SerializationKeysResult.SerialProcessingInReceivedOrder;
			}

			return new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, keys);
		}
	}
}
