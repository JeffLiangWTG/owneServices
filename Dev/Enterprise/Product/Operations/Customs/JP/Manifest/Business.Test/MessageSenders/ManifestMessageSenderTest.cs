using System;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(ManifestMessageSender))]
	sealed class ManifestMessageSenderTest : Customs.Business.Testing.MessageSenderTest
	{
		public void TestSendHCH01Message_DummyHAWBForEnd()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			var bill = header.Bills.AddNew();
			bill.ABL_GrossWeight = 36;
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HCH01 }))
			{
				var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				sendingObjectParent.EndSendMessage = true;
				sendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().FirstOrDefault().ShouldSend = true;

				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				header.MessageInitiator = messageInitiator;
				header.AMA_JobReference = "MAN000024";
				header.AMA_InputReference = "3456789012";

				var sender = new ManifestMessageSender(sendingObjectParent.SendingObjectsCollection.Where(x => x.ShouldSend), header, JPProcedureCodeList.Codes.HCH01);
				sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(Factory.Save);

				sender.SendMessage();
				AssertEquals(1, header.Messages.Count);
				var expectedMessageText = "   HCH01                                                                                  " +
											"                                                                                      " +
											"                                           HCH01345678901200000000001        3456789012" +
											"                                                                                       " +
											"              1                           000988\r\n     \r\n  \r\n                    " +
											"\r\n \r\n      \r\n     \r\n   \r\n   \r\n \r\n                    \r\n      \r\n      " +
											"36\r\nKGM\r\n                     \r\n   \r\n   \r\n     \r\n \r\n   \r\n              " +
											"                                                        \r\n                           " +
											"                                                                              \r\n     " +
											"         \r\n                 \r\n                                                     " +
											"                 \r\n                                                                  " +
											"                                       \r\n              \r\nEND                 \r\n";
				Assert(header.Messages[0].EM_MessageText.Equals(expectedMessageText));
			}
		}

		public void TestSendHCH01Message()
		{
			CombineAssertions(() =>
			{
				TestCanSendMessage(JPProcedureCodeList.Codes.HCH01);
			});
		}

		public void TestSendHDF01Message_UpdateBillStatusAfterSending() 
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = JPJobMessageTypeList.Codes.Export;
			header.AMA_TransportMode = TransportTypeList.Codes.Air;

			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var bill3 = header.Bills.AddNew();
			var bill4 = header.Bills.AddNew();
			bill4.ABL_BillStatus = JPCustomsStatusList.Codes.Deleted;

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HDF01 }))
			{
				var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				var sendingObjects = sendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().ToList();
				sendingObjects.ForEach(x => x.ShouldSend = true);
				sendingObjects[0].Action = "";
				sendingObjects[1].Action = JPMessageActionList.Codes.C;
				sendingObjects[2].Action = JPMessageActionList.Codes.D;
				sendingObjects[3].Action = JPMessageActionList.Codes.X;

				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				header.MessageInitiator = messageInitiator;
				header.AMA_JobReference = "MAN000024";
				header.AMA_InputReference = "3456789012";

				var sender = new ManifestMessageSender(sendingObjectParent.SendingObjectsCollection.Where(x => x.ShouldSend), header, JPProcedureCodeList.Codes.HDF01);
				sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(Factory.Save);

				sender.ManualExportMessage();
				AssertEquals(ZString.Empty, bill1.ABL_BillStatus);
				AssertEquals(ZString.Empty, bill2.ABL_BillStatus);
				AssertEquals(ZString.Empty, bill3.ABL_BillStatus);
				AssertEquals(JPCustomsStatusList.Codes.Deleted, bill4.ABL_BillStatus);

				sender.SendMessageAndUpdateBillMessageStatus();
				AssertEquals(JPCustomsStatusList.Codes.AWR, bill1.ABL_BillStatus);
				AssertEquals(JPCustomsStatusList.Codes.AWC, bill2.ABL_BillStatus);
				AssertEquals(JPCustomsStatusList.Codes.AWD, bill3.ABL_BillStatus);
				AssertEquals(JPCustomsStatusList.Codes.Deleted, bill4.ABL_BillStatus);
			}
		}

		public void TestSendHDF01Message()
		{
			CombineAssertions(() =>
			{
				TestCanSendMessage(JPProcedureCodeList.Codes.HDF01);
			});
		}

		void TestCanSendMessage(string procedureCode)
		{
			var header = Factory.New<AsycudaManifestHeader>();

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = procedureCode }))
			{
				var bill = header.Bills.AddNew();
				var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				sendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().FirstOrDefault().ShouldSend = true;

				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				header.MessageInitiator = messageInitiator;
				header.AMA_JobReference = "MAN000024";
				header.AMA_InputReference = "3456789012";

				var sender = new ManifestMessageSender(sendingObjectParent.SendingObjectsCollection.Where(x => x.ShouldSend), header, procedureCode);
				sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(Factory.Save);

				AssertEquals(0, header.Messages.Count);
				sender.SendMessage();
				AssertEquals(1, header.Messages.Count);

				var message = (EDIMessage)header.Messages.Single();
				var interchange = Factory.Load<EDIInterchange>(message.EM_EI);

				var messageNum = $"{procedureCode.PadRight(5, '0')}345678901200000000001";

				AssertEquals(nameof(message.EM_MessageType), procedureCode.Substring(0, 3), message.EM_MessageType);
				AssertEquals(nameof(EDIMessage.EM_MessageSubType), procedureCode.Substring(3, 2), message.EM_MessageSubType);
				AssertContains("Procedure Code", procedureCode, message.EM_FormattedMessageText);
				AssertEquals(nameof(message.EM_MessageNum), messageNum, message.EM_MessageNum);
				AssertContains("Message Reference", messageNum, message.EM_MessageInterpretation);
				AssertContains("Input Reference", "3456789012", message.EM_MessageInterpretation);
				AssertContains("Message length", message.EM_MessageData.Length.ToString(), message.EM_FormattedMessageText);

				AssertNotNull(interchange);

				var regKey = ObjectFactory.Get<IProductRegistration>().Key;
				AssertEquals($"{regKey.EnterpriseCode}{regKey.ServerCode}_JPC", interchange.EI_To);

				AssertEquals(GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
				AssertEquals(message.EM_MessageNum, interchange.EI_InterchangeNum);
				AssertEquals(message.EM_ApplicationCode, interchange.EI_ApplicationCode);
				AssertEquals(ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
				AssertEquals(EDIInterchangeTransportTypeList.Codes.xT, interchange.EI_TransportType);
				AssertEquals(EDIInterchangeStatusList.Codes.Queued, interchange.EI_Status);
				AssertEquals("Message data is copied to interchange", message.EM_MessageData.Length, interchange.EI_BodyData.Length);
			}
		}

		public void TestUpdateBillStatus_SendHCH01()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HCH01 }))
			{
				var bill = header.Bills.AddNew();
				var bill2 = header.Bills.AddNew();
				var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				var sendingObjects = sendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().ToList();
				sendingObjects[0].ShouldSend = true;
				sendingObjects[1].ShouldSend = false;

				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				header.MessageInitiator = messageInitiator;
				header.AMA_JobReference = "MAN000024";
				header.AMA_InputReference = "3456789012";

				var sender = new ManifestMessageSender(sendingObjectParent.SendingObjectsCollection.Where(x => x.ShouldSend), header, JPProcedureCodeList.Codes.HCH01);
				sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(Factory.Save);

				sender.ManualExportMessage();
				CombineAssertions(() =>
				{
					AssertEquals(ZString.Empty, bill.ABL_BillStatus);
					AssertEquals(ZString.Empty, header.MasterBill.ABL_BillStatus);
					AssertEquals(ZString.Empty, bill2.ABL_BillStatus);
				});

				sendingObjectParent.EndSendMessage = true;
				sender.SendMessageAndUpdateBillMessageStatus();
				AssertEquals(JPCustomsStatusList.Codes.AWR, bill.ABL_BillStatus);
				AssertEquals(JPMasterBillStatusList.Codes.AWE, header.MasterBill.ABL_BillStatus);
				AssertEquals(ZString.Empty, bill2.ABL_BillStatus);
			}
		}

		public void TestUpdateMasterBillStatus_SendHDE()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			header.MessageInitiator = messageInitiator;
			header.AMA_MasterBill = "123456";
			var context = new MessageSendingContext();
			context.ProcedureCode = JPProcedureCodeList.Codes.HDE;

			using (header.SetCurrentMessageSendingContext(context))
			{
				var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				var sendingObjects = sendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().FirstOrDefault().ShouldSend = true;
				var sender = new ManifestMessageSender(sendingObjectParent.SendingObjectsCollection.Where(x => x.ShouldSend), header, JPProcedureCodeList.Codes.HDE);
				sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(Factory.Save);

				sender.ManualExportMessage();
				AssertEquals(ZString.Empty, header.MasterBill.ABL_BillStatus);

				sender.SendMessageAndUpdateBillMessageStatus();
				AssertEquals(JPMasterBillStatusList.Codes.AWE, header.MasterBill.ABL_BillStatus);
			}
		}

		public void TestSendMessageFromVisualObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = JPJobMessageTypeList.Codes.Import;

			header.Bills.AddNew();

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext()))
			{
				var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				sendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().FirstOrDefault().ShouldSend = true;

				var contentProviderMock = new Mock<IMessageContentProvider>();
				contentProviderMock.Setup(m => m.Factory).Returns(Factory);
				contentProviderMock.Setup(m => m.ProcedureCode).Returns(JPProcedureCodeList.Codes.HCH01);
				contentProviderMock.Setup(m => m.GetMessageData()).Returns(() => Array.Empty<byte>());

				var visualObject = new MessageVisualObject(contentProviderMock.Object);
				var lastMessageDataProperty = visualObject.GetType().GetProperty("LastMessageData", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				lastMessageDataProperty.SetValue(visualObject, new byte[] { 20 });

				var sender = new ManifestMessageSender(visualObject, header, JPProcedureCodeList.Codes.HCH01);
				var message = sender.ManualExportMessage();

				AssertArrayEqualsByElements("Should export the message content from the visual object.", new byte[] { 20 }, message.EM_MessageData);
			}
		}

		public void TestSendingWithOverriddenValuesLog()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = JPJobMessageTypeList.Codes.Import;

			var bill = header.Bills.AddNew();
			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HCH01 }))
			{
				var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				var sendingObject = sendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().FirstOrDefault();
				sendingObject.ShouldSend = true;

				var contentProvider = new ManifestMessageContentProvider(Factory, [sendingObject]);
				var visualObject = new MessageVisualObject(contentProvider);
				visualObject.Initialize();

				var sender = new ManifestMessageSender(visualObject, header, JPProcedureCodeList.Codes.HCH01);
				sender.SendMessageAndUpdateBillMessageStatus();

				var sendingWithOverriddenValuesLogs = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.Authorised.Code && log.SL_Reference.StartsWith("Sending with overridden values|EDIMessage Number="));
				AssertEquals(0, sendingWithOverriddenValuesLogs.Count());

				visualObject.Header.Cast<EditableFieldBizObject>().FirstOrDefault().OverrideValue = "X";
				sender.SendMessageAndUpdateBillMessageStatus();
				sendingWithOverriddenValuesLogs = header.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.Authorised.Code && log.SL_Reference.StartsWith("Sending with overridden values|EDIMessage Number="));
				AssertEquals(1, sendingWithOverriddenValuesLogs.Count());
			}
		}
	}
}
