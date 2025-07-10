using System;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class SubmitAVSQueryProcessorTest : TestCaseWithFactory
	{
		[TestDate(2016, 2, 19)]
		public void TestProcess()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00000001";
			declaration.CustomsEntryHeaders.AddNew().CH_MessageType = MessageTypeList.Codes.EDIRelease;

			using (CACustomsDataRegistry.Instance.AIRSValidationKey.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ""))
			{
				var processor = new SubmitAVSQueryProcessor(declaration);
				var logger = new Notifications();
				processor.Process(logger);

				AssertStartsWith("Log for no AIRS Validation Key", "Submit AIRS Validation Query for Declaration B00000001 aborted. AIRS Validation Key hasn't been setup for Company EDI.", logger.ToString());
			}

			using (CACustomsDataRegistry.Instance.AIRSValidationKey.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "TESTKEY"))
			{
				var processor = new SubmitAVSQueryProcessor(declaration);
				processor.Process(new Notifications());

				var messages = Factory.Load<AVSQueryMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CFIAQuery));
				AssertEquals("An AVS Query message should be added", 1, messages.Length);
				var message = messages[0];
				AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.CFIAQuery, message.EM_ApplicationCode);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.AVSQuery, message.EM_MessageType);
				AssertEquals("EM_MessageSubType", MessageTypeList.Codes.AVSQuery, message.EM_MessageSubType);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("EM_ApplicationReference", declaration.JE_DeclarationReference, message.EM_ApplicationReference);
				AssertEquals("EM_HeldUntilDate", ZDateTime.UtcNow, message.EM_HeldUntilDate);
				AssertEquals("EM_LinkedObject", declaration.ReleaseEntryHeader, message.EM_LinkedObject);
				AssertEquals("EM_MessageText", ZString.Empty, message.EM_MessageText);

				message.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(5);
				message.EM_MessageText = "Query Failed";

				processor.Process(new Notifications());
				AssertEquals("No AVS Query message should be added", 1, declaration.ReleaseEntryHeader.Messages.Count);
				AssertEquals("EM_HeldUntilDate", ZDateTime.UtcNow, message.EM_HeldUntilDate);
				AssertEquals("EM_MessageText", ZString.Empty, message.EM_MessageText);

				message.EM_Status = EDIMessage.Status.Acknowledged;

				processor.Process(new Notifications());
				messages = Factory.Load<AVSQueryMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CFIAQuery));
				AssertEquals("An AVS Query message should be added", 2, messages.Length);
			}
		}

		class Notifications : INotifications
		{
			public void Add(INotification notification)
			{
				text.AppendLine(notification.Message);
			}

			readonly StringBuilder text = new StringBuilder();

			public override string ToString()
			{
				return text.ToString();
			}
		}
	}
}
