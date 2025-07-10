using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.XPath;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Integration;
using Enterprise.eHubMessaging.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.eHubMessaging.Tests
{
	public class ImportMessageActionTest : MessageActionTest
	{
		public void TestExecuteAction_Import()
		{
			var factoryProvider = new BusinessObjectFactoryProvider(Factory);

			var notifications = new Mock<INotifications>(MockBehavior.Strict);

			int callCount = 0;
			notifications.Setup(m => m.Add(It.IsAny<INotification>()))
				.Callback(new NotificationDelegate((INotification notification) =>
			{
				callCount++;
				if (callCount == 1)
				{
					Assert(notification.GetType().IsAssignableFrom(typeof(InfoNotification)));
					Assert(notification.Message.StartsWith("Combining message text"));
				}
				else if (callCount == 2)
				{
					Assert(notification.GetType().IsAssignableFrom(typeof(WarningNotification)));
					AssertEquals("Warning: Message is empty.", notification.Message);
				}
				else if (callCount == 3)
				{
					Assert(notification.GetType().IsAssignableFrom(typeof(InfoNotification)));
					Assert(notification.Message.StartsWith("Combining message text"));
				}
				else if (callCount == 4)
				{
					Assert(notification.GetType().IsAssignableFrom(typeof(InfoNotification)));
					Assert(notification.Message.StartsWith("Running import"));
				}
				else if (callCount == 5)
				{
					Assert(notification.GetType().IsAssignableFrom(typeof(InfoNotification)));
					Assert(notification.Message.StartsWith("Link message to Job"));
				}
				else if (callCount == 6)
				{
					Assert(notification.GetType().IsAssignableFrom(typeof(InfoNotification)));
					Assert(notification.Message.StartsWith("Import finished"));
				}
			}));

			var dataImporter = new Mock<IDataImporterControllingSave>(MockBehavior.Strict);
			dataImporter.SetupProperty(m => m.OnlySaveDataWhenNoRecordsHaveErrors, false);

			ITransactionParticipant[] forSave;
			dataImporter.Setup(m => m.ImportDataToFactory(It.IsAny<TextReader>(), It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<ISourceInfo>(), out forSave))
				.Returns(new ImportDataDelegate((TextReader dataReader, string attachmentFileName, INotifications notify, ISourceInfo info, out ITransactionParticipant[] forSaveLocal) =>
			{
				AssertEquals("header-text-footer", dataReader.ReadToEnd());
				Assert(string.IsNullOrEmpty(attachmentFileName));
				forSaveLocal = Array.Empty<ITransactionParticipant>();
				return true;
			}));

			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			var declaration = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			var stmALog = Factory.New<StmALog>();
			BusinessObject[] importedBusinessObjects = new BusinessObject[] { consol, declaration, stmALog };

			dataImporter.Setup(m => m.ImportedBusinessObjects).Returns(importedBusinessObjects);

			var action = new Mock<ImportMessageAction>(factoryProvider) { CallBase = true };
			action.Setup(m => m.GetDataImporter()).Returns(dataImporter.Object);

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_HeaderText = "header";
			interchange.EI_FooterText = "footer";

			var message1 = EDIMessageTestFactory.New(Factory);
			var message2 = EDIMessageTestFactory.New(Factory);
			message2.EM_MessageText = "-text-";
			message2.EM_EI = interchange.PK;

			List<ITransactionParticipant> participants;
			Assert(!action.Object.ExecuteAction(message1, notifications.Object, out participants));
			Assert(action.Object.ExecuteAction(message2, notifications.Object, out participants));

			AssertPivot(consol.PK, message2);
			AssertPivot(declaration.PK, message2);
			AssertNull(FindMostRecentDataImportLog(stmALog.PK));

			notifications.VerifyAll();
			dataImporter.VerifyAll();
			action.VerifyAll();
		}

		public void TestExecuteAction_Import_SubTypes()
		{
			var factoryProvider = new BusinessObjectFactoryProvider(Factory);

			var notifications = new Mock<INotifications>(MockBehavior.Strict);
			notifications.Setup(m => m.Add(It.IsAny<INotification>()));

			var dataImporter = new Mock<IDataImporterControllingSave>(MockBehavior.Strict);
			dataImporter.SetupProperty(m => m.OnlySaveDataWhenNoRecordsHaveErrors, false);

			string interchangeMsg = "<XmlInterchange><InterchangeInfo></InterchangeInfo><Payload><{0}>{1}</{0}></Payload></XmlInterchange>";

			string cartageJobMsg = GetTestFileBody("CartageJob.xml");
			string declarationMsg = GetTestFileBody("Declaration.xml");

			ITransactionParticipant[] forSave;
			int callCount = 0;
			dataImporter.Setup(m => m.ImportDataToFactory(It.IsAny<TextReader>(), It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<ISourceInfo>(), out forSave))
				.Returns(new ImportDataDelegate((TextReader dataReader, string attachmentFileName, INotifications notify, ISourceInfo info, out ITransactionParticipant[] forSaveLocal) =>
				{
					callCount++;
					if (callCount <= 2)
					{
						string expected = string.Format(interchangeMsg, "CartageJobs", cartageJobMsg);
						string actual = dataReader.ReadToEnd();
						AssertEquals("SubMessageType_CartageJobs", expected, actual);
						forSaveLocal = Array.Empty<ITransactionParticipant>();
						return true;
					} else
					{
						string expected = string.Format(interchangeMsg, "Consols", declarationMsg);
						string actual = dataReader.ReadToEnd();
						AssertEquals("SubMessageType_Declaration", expected, actual);
						forSaveLocal = Array.Empty<ITransactionParticipant>();
						return true;
					}
				}));

			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			var declaration = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			var stmALog = Factory.New<StmALog>();
			BusinessObject[] importedBusinessObjects = new BusinessObject[] { consol, declaration, stmALog };

			dataImporter.Setup(m => m.ImportedBusinessObjects).Returns(importedBusinessObjects);

			var action = new Mock<ImportMessageAction>(factoryProvider) { CallBase = true };
			action.Setup(m => m.GetDataImporter()).Returns(dataImporter.Object);

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.XMS;
			interchange.EI_BodyText = string.Format(interchangeMsg, "Body", string.Empty);

			var message1 = EDIMessageTestFactory.New(Factory);
			message1.EM_MessageText = cartageJobMsg;
			message1.EM_EI = interchange.PK;
			message1.EM_MessageSubType = EDIMessageSubTypeList.Codes.LocalCartageStatus;

			var message2 = EDIMessageTestFactory.New(Factory);
			message2.EM_MessageText = cartageJobMsg;
			message2.EM_EI = interchange.PK;
			message2.EM_MessageSubType = EDIMessageSubTypeList.Codes.LocalCartageBooking;

			var message3 = EDIMessageTestFactory.New(Factory);
			message3.EM_MessageText = declarationMsg;
			message3.EM_EI = interchange.PK;
			message3.EM_MessageSubType = EDIMessageSubTypeList.Codes.Brokerage;

			List<ITransactionParticipant> participants;
			Assert(action.Object.ExecuteAction(message1, notifications.Object, out participants));
			Assert(action.Object.ExecuteAction(message2, notifications.Object, out participants));
			Assert(action.Object.ExecuteAction(message3, notifications.Object, out participants));

			notifications.Verify(m => m.Add(It.IsAny<INotification>()));
			notifications.VerifyAll();

			dataImporter.VerifySet(m => m.OnlySaveDataWhenNoRecordsHaveErrors = false, Times.Exactly(3));
			dataImporter.VerifyAll();
			action.VerifyAll();
		}

		public void TestUpdateInterchangeHeaderText()
		{
			var interchangeMsg = "<XmlInterchange><InterchangeInfo></InterchangeInfo><Payload><{0}>{1}</{0}></Payload></XmlInterchange>";
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.ZACustoms;
			interchange.EI_BodyText = string.Format(interchangeMsg, "Body", string.Empty);
			interchange.UpdateInterchangeHeaderText();
			AssertEquals(interchange.EI_HeaderText, "");

			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.XMS;
			interchange.UpdateInterchangeHeaderText();
			AssertEquals(interchange.EI_HeaderText, "<InterchangeInfo></InterchangeInfo>");
		}

		void AssertPivot(ZGuid pk, EDIMessage message)
		{
			var dataImportLog = FindMostRecentDataImportLog(pk);
			new GenPivotTestHelper().AssertPivotExists(StmALogSchema.Constants.Prefix, dataImportLog.PK, EDIMessageSchema.Constants.Prefix, message.PK, Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage, Factory);
		}

		StmALog FindMostRecentDataImportLog(ZGuid parentPK)
		{
			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_Parent, parentPK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code);
			query.OrderBy = StmALogSchema.SL_PostedTimeUtc.Name + " DESC";
			var result = Factory.LoadTop1<StmALog>(query);
			return result;
		}

		string GetTestFileBody(string resourceName)
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var resourceStream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".TestFiles." + resourceName))
			{
				var doc = new XPathDocument(resourceStream);
				var nav = doc.CreateNavigator();
				string bodyPath = "/*[local-name()='XmlInterchange']/*[local-name()='Payload']/*[1]";
				return nav.SelectSingleNode(bodyPath).InnerXml;
			}
		}

		protected EDIMessage CreateMessage(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<EDIMessage>();
			var interchange = factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = "blah";
			interchange.EI_To = "blah blah";
			result.EM_EI = interchange.PK;

			return result;
		}

		protected delegate bool ImportDataDelegate(TextReader dataReader, string attachmentFileName, INotifications notifications, ISourceInfo info, out ITransactionParticipant[] forSave);
	}
}
