using System;
using System.Data;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Business.Testing
{
	abstract class ILEDIMessageTestBase<T> : EDIMessageTest where T : ILEDIMessage
	{
		public void TestDefaultValues()
		{
			var message = Factory.New<T>();
			CombineAssertions("Default Values", () =>
			{
				AssertEquals("EM_ApplicationCode", ILEDIInterchange.ApplicationCodes.ILCustoms, message.EM_ApplicationCode);

				var expectedMessageType = GetExpectedMessageType();
				if (!expectedMessageType.IsNullOrEmpty())
				{
					AssertEquals("Message type", expectedMessageType, message.EM_MessageType);
				}

				var expectedMessageSubType = GetExpectedMessageSubType();
				if (!expectedMessageSubType.IsNullOrEmpty())
				{
					AssertEquals("Message sub-type", expectedMessageSubType, message.EM_MessageSubType);
				}
			});
		}

		public void TestReceivingMessageDataObject()
		{
			var message = (T)GetNewMessage();
			var expectedType = GetExpectedTypeOfMessageDataObject();

			if (expectedType is not null)
			{
				AssertType("Message should reference the expected Data Object", expectedType, message.MessageDataObject);
			}
			else
			{
				AssertNull("Message should not reference a Data Object", message.MessageDataObject);
			}
		}

		public void TestValidation()
		{
			AssertType("Message should have the expected Validation", GetTypeOfValidation(), message.Validation);
		}

		public void TestGetMessageReferenceNumber()
		{
			Env.NumberFountains.ILMessageControlNumber(GlbCompany.CurrentCompany.PK.ToGuid()).SetNext(Factory, 1234);
			AssertNoExceptionThrown("GetMessageReferenceNumber must be implemented", () => message.AssignMessageNumber());
			AssertEquals("MessageNum should become 1234", "00000000001234", message.EM_MessageNum);
		}

		public void TestEM_MessageInterpretation()
		{
			SetupReferenceDataForInterpretation();
			var messageText = GetMessageText();
			message.EM_MessageText = messageText;

			var expected = GetExpectedMessageInterpretation().Replace("\r\n", "");

			if (expected.IsEmpty)
			{
				expected = messageText.Replace("\r\n", "");
			}

			AssertEquals("Message should be human readable.", expected, message.EM_MessageInterpretation.Replace("\r\n", ""));
		}

		public void TestMessageSubTypeList()
		{
			var message = Factory.New<ILEDIMessageForTesting>();
			AssertType<ILEDIMessageSubTypeList>("MessageSubTypeList should be the expected type", message.MessageSubTypeList_Expose);
		}

		public void TestEM_MessageSubTypeDescription()
		{
			var message = Factory.New<T>();
			message.EM_MessageSubType = GetExpectedMessageSubType();
			AssertEquals("EM_MessageSubTypeDescription", GetExpectedMessageSubTypeDescription(), message.EM_MessageSubTypeDescription);
		}

		public void TestShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride()
		{
			var message = (ILEDIMessage)GetNewMessage();
			var methodInfo = typeof(T).GetMethod("ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride", BindingFlags.NonPublic | BindingFlags.Instance);

			var result = methodInfo.Invoke(message, null);
			AssertEquals("ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride must be true", true, result);
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = (T)GetNewMessage();
		}

		protected abstract Type GetExpectedTypeOfMessageDataObject();
		protected virtual Type GetTypeOfValidation() => typeof(EDIMessageValidation);
		protected abstract ZString GetExpectedMessageInterpretation();
		protected abstract ZString GetMessageText();
		protected abstract string GetExpectedMessageType();
		protected abstract string GetExpectedMessageSubType();
		protected abstract string GetMessageReceiveTransmit();
		protected abstract ZString GetExpectedMessageSubTypeDescription();

		protected readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		protected virtual void SetupReferenceDataForInterpretation()
		{
		}

		T message;
	}

	class ILEDIMessageForTesting : ILEDIMessage
	{
		public ILEDIMessageForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public CodeDescriptionPairList MessageSubTypeList_Expose => MessageSubTypeList;
	}
}
