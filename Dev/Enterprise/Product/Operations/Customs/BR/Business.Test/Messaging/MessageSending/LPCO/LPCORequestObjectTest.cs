using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(LPCORequestObject))]
	class LPCORequestObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestReason()
		{
			AssertPropertyReadOnly(MessageSendingObject.RequestObject.ReasonInfo, (ZString)"reason",
				new[] {
					LPCOEntryActionCodeList.Codes.RCA,
					LPCOEntryActionCodeList.Codes.REQ,
					LPCOEntryActionCodeList.Codes.ALE,
					LPCOEntryActionCodeList.Codes.COM,
				});
		}

		public void TestRequirement()
		{
			AssertPropertyReadOnly(MessageSendingObject.RequestObject.RequirementInfo, (ZInt)444, new[] { LPCOEntryActionCodeList.Codes.REQ });
		}

		public void TestNewEffectiveDate()
		{
			AssertPropertyReadOnly(MessageSendingObject.RequestObject.NewEffectiveDateInfo, ZDate.Today, new[] { LPCOEntryActionCodeList.Codes.ALE });
		}

		public void TestDocumentNumberAndVersion()
		{
			AssertPropertyReadOnly(MessageSendingObject.RequestObject.DocumentNumberInfo, (ZString)"123456", new[] { LPCOEntryActionCodeList.Codes.COM });
			AssertPropertyReadOnly(MessageSendingObject.RequestObject.DocumentItemNumberInfo, (ZInt)111, new[] { LPCOEntryActionCodeList.Codes.COM });
			AssertPropertyReadOnly(MessageSendingObject.RequestObject.VersionInfo, (ZString)"1", new[] { LPCOEntryActionCodeList.Codes.COM });
		}

		public void TestMessage()
		{
			AssertPropertyReadOnly(MessageSendingObject.RequestObject.MessageInfo, (ZString)"TEST MESSAGE", new[] { LPCOEntryActionCodeList.Codes.MSG });
		}

		void AssertPropertyReadOnly(ZPropertyInfo propertyInfo, IZType nonEmptyValue, IEnumerable<string> availableMessageTypes)
		{
			AssertPropertyReadOnly(MessageSendingObject.MessageTypeInfo as ZPropertyInfoString, propertyInfo, nonEmptyValue, MessageSendingObject.MessageTypesList.GetAllCodes(), availableMessageTypes);
		}

		public static void AssertPropertyReadOnly(ZPropertyInfoString messageTypePropertyInfo, ZPropertyInfo propertyInfo, IZType nonEmptyValue, IEnumerable<string> allMessageTypes, IEnumerable<string> availableMessageTypes)
		{
			messageTypePropertyInfo.Value = ZString.Empty;

			foreach (var messageType in allMessageTypes)
			{
				var readOnly = !availableMessageTypes.Contains(messageType);
				var message = $"Message Type = {messageType} {propertyInfo.Name} should be {(readOnly ? "" : "NOT")}";

				propertyInfo.Value = nonEmptyValue;
				messageTypePropertyInfo.Value = messageType;

				AssertEquals($"{message} ReadOnly", readOnly, propertyInfo.ReadOnly);
				AssertEquals($"{message} Cleared", readOnly, propertyInfo.Value.IsEmpty);
			}
		}

		LPCODeclarationMessageSendingObject MessageSendingObject => messageSendingObject ??= CreateNewMessageSendingObject();
		LPCODeclarationMessageSendingObject messageSendingObject;

		LPCODeclarationMessageSendingObject CreateNewMessageSendingObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var messageSendingObject = new LPCODeclarationMessageSendingObject(entryHeader);
			return messageSendingObject;
		}

		protected override BusinessObject GetNewBusinessObject() => MessageSendingObject.RequestObject;
	}
}
