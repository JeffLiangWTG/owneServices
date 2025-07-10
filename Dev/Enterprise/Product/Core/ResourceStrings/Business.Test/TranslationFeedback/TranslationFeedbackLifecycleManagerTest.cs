using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWiseOne.ResourceStrings;
using Enterprise.eHubMessaging.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1199:Do Not Use Unnecessary Resource String In Unit Tests", Justification = "WI: WI00900276. These unit tests are for testing translations so changing to hardcoded strings is not appropriate.")]
	sealed class TranslationFeedbackLifecycleManagerTest : TranslationFeedbackTestCase
	{
		[ExpectNoExceptions]
		public void TestTranslationFeedbackUpdateMessageActionThrowsNoExceptionWhenFeedBackNull()
		{
			AddMockTranslation("kdg1", "Dog", "狗");
			var factory = new BusinessObjectFactory();
			var feedbacks = TranslationFeedbackFactory.Get(factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
			feedbacks[0].XT_SuggestedTranslation = "犬";
			feedbacks.Factory.Save();
			TranslationFeedbackLifecycleManager.UpdateCurrentResourceStrings();
			factory = new BusinessObjectFactory();
			var clientFeedback = factory.Load<StmTranslationFeedback>(feedbacks[0].PK);
			feedbacks = new TopLevelTranslationFeedbackCollection(factory);
			feedbacks.Add(clientFeedback);
			clientFeedback.XT_Status = TranslationFeedbackStatusList.Codes.Canceled;
			feedbacks.Factory.Save();
			var interchange = factory.LoadTop1<EDIInterchange>(new ZQuery() { OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " desc" });
			var updateMessage = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
			ResetConnection();

			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				List<ITransactionParticipant> participant;
				new TranslationFeedbackLifecycleManager.TranslationFeedbackUpdateMessageAction().ExecuteAction(updateMessage, new NotificationCollection(), out participant);
			}
		}

		public void TestSendAndCancel()
		{
			AddMockTranslation("kdg1", "Dog", "狗");
			var factory = new BusinessObjectFactory();
			var feedbacks = TranslationFeedbackFactory.Get(factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
			feedbacks[0].XT_SuggestedTranslation = "犬";
			feedbacks.Factory.Save();
			AssertEquals(1, factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = factory.LoadTop1<EDIInterchange>(new ZQuery() { OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " desc" });
			AssertEquals(interchange.EI_From, feedbacks[0].CompanyLicenceCode);
			var entryMessage = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
			AssertEquals(1, ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true)).Length);
			TranslationFeedbackLifecycleManager.UpdateCurrentResourceStrings();
			factory = new BusinessObjectFactory();
			var clientFeedback = factory.Load<StmTranslationFeedback>(feedbacks[0].PK);
			feedbacks = new TopLevelTranslationFeedbackCollection(factory);
			feedbacks.Add(clientFeedback);
			clientFeedback.XT_Status = TranslationFeedbackStatusList.Codes.Canceled;
			feedbacks.Factory.Save();
			interchange = factory.LoadTop1<EDIInterchange>(new ZQuery() { OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " desc" });
			AssertEquals(interchange.EI_From, feedbacks[0].CompanyLicenceCode);
			var updateMessage = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
			AssertEquals(0, ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true)).Length);
			ResetConnection();

			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				List<ITransactionParticipant> participant;
				new TranslationFeedbackLifecycleManager.TranslationFeedbackEntryMessageAction().ExecuteAction(entryMessage, new NotificationCollection(), out participant);
				var masterFeedbacks = new TopLevelTranslationFeedbackCollection(new BusinessObjectFactory());
				masterFeedbacks.Load();
				AssertEquals(1, masterFeedbacks.Count);
				AssertFeedback("kdg1", "Dog", "狗", "犬", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, masterFeedbacks[0]);
				AssertEquals(clientFeedback.PK, masterFeedbacks[0].PK);
				AssertEquals(TranslationFeedbackStatusList.Codes.New, masterFeedbacks[0].XT_Status);
				new TranslationFeedbackLifecycleManager.TranslationFeedbackUpdateMessageAction().ExecuteAction(updateMessage, new NotificationCollection(), out participant);
				masterFeedbacks = new TopLevelTranslationFeedbackCollection(new BusinessObjectFactory());
				masterFeedbacks.Load();
				AssertEquals(1, masterFeedbacks.Count);
				AssertFeedback("kdg1", "Dog", "狗", "犬", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, masterFeedbacks[0]);
				AssertEquals(TranslationFeedbackStatusList.Codes.Canceled, masterFeedbacks[0].XT_Status);
			}
			ResetConnection();

			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				List<ITransactionParticipant> participant;
				new TranslationFeedbackLifecycleManager.TranslationFeedbackEntryMessageAction().ExecuteAction(entryMessage, new NotificationCollection(), out participant);
				var masterFeedbacks = new TopLevelTranslationFeedbackCollection(new BusinessObjectFactory());
				masterFeedbacks.Load();
				AssertEquals(1, masterFeedbacks.Count);
				AssertFeedback("kdg1", "Dog", "狗", "犬", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, masterFeedbacks[0]);
				masterFeedbacks[0].XT_Status = TranslationFeedbackStatusList.Codes.Approved;
				masterFeedbacks[0].Factory.Save();

				new TranslationFeedbackLifecycleManager.TranslationFeedbackUpdateMessageAction().ExecuteAction(updateMessage, new NotificationCollection(), out participant);
				masterFeedbacks = new TopLevelTranslationFeedbackCollection(new BusinessObjectFactory());
				masterFeedbacks.Load();
				AssertEquals(1, masterFeedbacks.Count);
				AssertFeedback("kdg1", "Dog", "狗", "犬", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, masterFeedbacks[0]);
				AssertEquals(TranslationFeedbackStatusList.Codes.Approved, masterFeedbacks[0].XT_Status);
			}
			ResetConnection();
		}

		public void TestSendApproveAndCurrentCW1()
		{
			SendApproveAndCurrent(TranslationFeedbackApplicationsList.Codes.Cargowise, TranslationFeedbackStatusList.Codes.Current, 0);
		}

		public void TestSendApproveAndCurrentGLW()
		{
			SendApproveAndCurrent(TranslationFeedbackApplicationsList.Codes.Glow, TranslationFeedbackStatusList.Codes.Approved, 1);
		}

		void SendApproveAndCurrent(string application, string expectedStatus, int expectedCheckedOutStringsLength)
		{
			AddMockTranslation("kdg1", "Dog", "狗");
			var factory = new BusinessObjectFactory();
			var feedbacks = TranslationFeedbackFactory.Get(factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
			feedbacks[0].XT_SuggestedTranslation = "犬";
			feedbacks[0].XT_Application = application;
			feedbacks.Factory.Save();
			AssertEquals(1, factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = factory.LoadTop1<EDIInterchange>(new ZQuery() { OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " desc" });
			AssertEquals(interchange.EI_From, feedbacks[0].CompanyLicenceCode);
			var entryMessage = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
			var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(1, checkedOutStrings.Length);
			ResetConnection();

			EDIMessage updateMessage;
			List<ITransactionParticipant> participant;

			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				new TranslationFeedbackLifecycleManager.TranslationFeedbackEntryMessageAction().ExecuteAction(entryMessage, new NotificationCollection(), out participant);
				factory = new BusinessObjectFactory();
				var masterFeedbacks = new TopLevelTranslationFeedbackCollection(factory);
				masterFeedbacks.Load();
				AssertEquals(1, masterFeedbacks.Count);
				AssertFeedback("kdg1", "Dog", "狗", "犬", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, masterFeedbacks[0]);
				AssertEquals(TranslationFeedbackStatusList.Codes.New, masterFeedbacks[0].XT_Status);
				masterFeedbacks[0].XT_Status = TranslationFeedbackStatusList.Codes.Approved;
				factory.Save();
				AssertEquals(1, factory.GetDatabaseCount(typeof(EDIInterchange)));
				interchange = factory.LoadTop1<EDIInterchange>(new ZQuery() { OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " desc" });
				AssertEquals(interchange.EI_To, feedbacks[0].CompanyLicenceCode);
				updateMessage = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
				checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(1, checkedOutStrings.Length);
			}
			ResetConnection();

			factory = new BusinessObjectFactory();
			feedbacks = new TopLevelTranslationFeedbackCollection(factory);
			using (var xmlReader = SystemMessage.GetXmlReader(entryMessage))
			{
				var entry = TranslationFeedbackMessagesSerializer.DeserializeTranslationFeedbackEntry(xmlReader);
				feedbacks.Add(StmTranslationFeedback.New(factory, entry));
			}
			factory.Save();
			new TranslationFeedbackLifecycleManager.TranslationFeedbackUpdateMessageAction().ExecuteAction(updateMessage, new NotificationCollection(), out participant);

			factory = new BusinessObjectFactory();
			feedbacks = new TopLevelTranslationFeedbackCollection(factory);
			feedbacks.Load();
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("kdg1", "Dog", "狗", "犬", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			AssertEquals(TranslationFeedbackStatusList.Codes.Approved, feedbacks[0].XT_Status);
			checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(1, checkedOutStrings.Length);

			TranslationFeedbackLifecycleManager.UpdateCurrentResourceStrings();
			checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(1, checkedOutStrings.Length);

			AddMockTranslation("kdg1", "Dog", "犬");
			TranslationFeedbackLifecycleManager.UpdateCurrentResourceStrings();

			feedbacks = new TopLevelTranslationFeedbackCollection(factory);
			feedbacks.Load();
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("kdg1", "Dog", "狗", "犬", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			AssertEquals(expectedStatus, feedbacks[0].XT_Status);
			checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(expectedCheckedOutStringsLength, checkedOutStrings.Length);
		}

		public void TestSendAndReject()
		{
			AddMockTranslation("kdg1", "Dog", "狗");
			var factory = new BusinessObjectFactory();
			var feedbacks = TranslationFeedbackFactory.Get(factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
			feedbacks[0].XT_SuggestedTranslation = "犬";
			feedbacks.Factory.Save();
			AssertEquals(1, factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = factory.LoadTop1<EDIInterchange>(new ZQuery() { OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " desc" });
			AssertEquals(interchange.EI_From, feedbacks[0].CompanyLicenceCode);
			var entryMessage = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
			var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(1, checkedOutStrings.Length);
			var checkedOutString = checkedOutStrings[0];
			ResetConnection();

			EDIMessage updateMessage;
			List<ITransactionParticipant> participant;

			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				new TranslationFeedbackLifecycleManager.TranslationFeedbackEntryMessageAction().ExecuteAction(entryMessage, new NotificationCollection(), out participant);
				factory = new BusinessObjectFactory();
				var masterFeedbacks = new TopLevelTranslationFeedbackCollection(factory);
				masterFeedbacks.Load();
				AssertEquals(1, masterFeedbacks.Count);
				AssertFeedback("kdg1", "Dog", "狗", "犬", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, masterFeedbacks[0]);
				AssertEquals(TranslationFeedbackStatusList.Codes.New, masterFeedbacks[0].XT_Status);
				masterFeedbacks[0].XT_Status = TranslationFeedbackStatusList.Codes.Rejected;
				factory.Save();
				AssertEquals(1, factory.GetDatabaseCount(typeof(EDIInterchange)));
				interchange = factory.LoadTop1<EDIInterchange>(new ZQuery() { OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " desc" });
				AssertEquals(interchange.EI_To, feedbacks[0].CompanyLicenceCode);
				updateMessage = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
			}
			ResetConnection();

			factory = new BusinessObjectFactory();
			feedbacks = new TopLevelTranslationFeedbackCollection(factory);
			using (var xmlReader = SystemMessage.GetXmlReader(entryMessage))
			{
				var entry = TranslationFeedbackMessagesSerializer.DeserializeTranslationFeedbackEntry(xmlReader);
				feedbacks.Add(StmTranslationFeedback.New(factory, entry));
			}
			factory.Save();
			checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(1, checkedOutStrings.Length);

			new TranslationFeedbackLifecycleManager.TranslationFeedbackUpdateMessageAction().ExecuteAction(updateMessage, new NotificationCollection(), out participant);

			factory = new BusinessObjectFactory();
			feedbacks = new TopLevelTranslationFeedbackCollection(factory);
			feedbacks.Load();
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("kdg1", "Dog", "狗", "犬", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			AssertEquals(TranslationFeedbackStatusList.Codes.Rejected, feedbacks[0].XT_Status);
			checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(0, checkedOutStrings.Length);
		}

		public void TestNoMessageOnMasterDatabase()
		{
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				AddMockTranslation("kdg1", "Dog", "狗");
				var feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
				feedbacks[0].XT_SuggestedTranslation = "犬";
				feedbacks.Factory.Save();
				AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));
				var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(1, checkedOutStrings.Length);
				AssertEquals("kdg1", checkedOutStrings[0].HD_Code);
				AssertEquals("犬", checkedOutStrings[0].HD_Caption);

				feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
				feedbacks[0].XT_SuggestedTranslation = "獒";
				feedbacks.Factory.Save();
				AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));
				checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(1, checkedOutStrings.Length);
				AssertEquals("kdg1", checkedOutStrings[0].HD_Code);
				AssertEquals("獒", checkedOutStrings[0].HD_Caption);

				feedbacks = TranslationFeedbackFactory.Get(Factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
				feedbacks[0].XT_Status = TranslationFeedbackStatusList.Codes.Rejected;
				feedbacks.Factory.Save();
				AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			}
		}

		public void TestApproveWithoutInitialTransaltion()
		{
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				ENG.Put("kdg1", new ResourceStringData("kdg1", "Dog"));
				var factory = new BusinessObjectFactory();
				var feedbacks = TranslationFeedbackFactory.Get(factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
				feedbacks[0].XT_SuggestedTranslation = "狗";
				feedbacks.Factory.Save();
				var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(1, checkedOutStrings.Length);
				feedbacks[0].Reload();
				AssertEquals(TranslationFeedbackStatusList.Codes.Approved, feedbacks[0].XT_Status);

				TranslationFeedbackLifecycleManager.UpdateCurrentResourceStrings();
				checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(1, checkedOutStrings.Length);
				feedbacks[0].Reload();
				AssertEquals(TranslationFeedbackStatusList.Codes.Approved, feedbacks[0].XT_Status);

				AddMockTranslation("kdg1", "Dog", "狗");
				TranslationFeedbackLifecycleManager.UpdateCurrentResourceStrings();
				checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(0, checkedOutStrings.Length);
				feedbacks[0].Reload();
				AssertEquals(TranslationFeedbackStatusList.Codes.Current, feedbacks[0].XT_Status);
			}
		}

		public void TestSendThenModify()
		{
			// client modifies an entry after creating it, it is then approved

			AddMockTranslation("kdg1", "Dog", "狗");
			var factory = new BusinessObjectFactory();
			var feedbacks = TranslationFeedbackFactory.Get(factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
			feedbacks[0].XT_SuggestedTranslation = "犬";
			feedbacks.Factory.Save();
			AssertEquals(1, factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = factory.LoadTop1<EDIInterchange>(new ZQuery() { OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " desc" });
			AssertEquals(interchange.EI_From, feedbacks[0].CompanyLicenceCode);
			var entryMessage = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
			var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(1, checkedOutStrings.Length);
			AssertEquals("kdg1", checkedOutStrings[0].HD_Code);
			AssertEquals("犬", checkedOutStrings[0].HD_Caption);
			factory = new BusinessObjectFactory();
			feedbacks = TranslationFeedbackFactory.Get(factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"), "犬");
			AssertFeedback("kdg1", "Dog", "狗", "犬", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			feedbacks[0].XT_SuggestedTranslation = "獒";
			feedbacks.Factory.Save();
			interchange = factory.LoadTop1<EDIInterchange>(new ZQuery() { OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " desc" });
			AssertEquals(interchange.EI_From, feedbacks[0].CompanyLicenceCode);
			var updateMessage = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
			checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(1, checkedOutStrings.Length);
			AssertEquals("kdg1", checkedOutStrings[0].HD_Code);
			AssertEquals("獒", checkedOutStrings[0].HD_Caption);
			ResetConnection();

			List<ITransactionParticipant> participant;

			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				new TranslationFeedbackLifecycleManager.TranslationFeedbackEntryMessageAction().ExecuteAction(entryMessage, new NotificationCollection(), out participant);
				var masterFeedbacks = new TopLevelTranslationFeedbackCollection(new BusinessObjectFactory());
				masterFeedbacks.Load();
				AssertEquals(1, masterFeedbacks.Count);
				AssertFeedback("kdg1", "Dog", "狗", "犬", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, masterFeedbacks[0]);
				AssertEquals(feedbacks[0].PK, masterFeedbacks[0].PK);
				AssertEquals(TranslationFeedbackStatusList.Codes.New, masterFeedbacks[0].XT_Status);
				new TranslationFeedbackLifecycleManager.TranslationFeedbackUpdateMessageAction().ExecuteAction(updateMessage, new NotificationCollection(), out participant);
				masterFeedbacks = new TopLevelTranslationFeedbackCollection(new BusinessObjectFactory());
				masterFeedbacks.Load();
				AssertEquals(1, masterFeedbacks.Count);
				AssertFeedback("kdg1", "Dog", "狗", "獒", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, masterFeedbacks[0]);
				AssertEquals(TranslationFeedbackStatusList.Codes.New, masterFeedbacks[0].XT_Status);
				masterFeedbacks[0].XT_Status = TranslationFeedbackStatusList.Codes.Approved;
				masterFeedbacks.Factory.Save();
				AssertEquals(1, factory.GetDatabaseCount(typeof(EDIInterchange)));
				interchange = factory.LoadTop1<EDIInterchange>(new ZQuery() { OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " desc" });
				AssertEquals(interchange.EI_To, feedbacks[0].CompanyLicenceCode);
				updateMessage = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
				checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(1, checkedOutStrings.Length);
			}
			ResetConnection();

			factory = new BusinessObjectFactory();
			using (var xmlReader = SystemMessage.GetXmlReader(entryMessage))
			{
				var entry = TranslationFeedbackMessagesSerializer.DeserializeTranslationFeedbackEntry(xmlReader);
				var feedback = StmTranslationFeedback.New(factory, entry);
				feedback.XT_SuggestedTranslation = "獒";
				factory.Save();
			}
			new TranslationFeedbackLifecycleManager.TranslationFeedbackUpdateMessageAction().ExecuteAction(updateMessage, new NotificationCollection(), out participant);

			factory = new BusinessObjectFactory();
			feedbacks = new TopLevelTranslationFeedbackCollection(factory);
			feedbacks.Load();
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("kdg1", "Dog", "狗", "獒", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			AssertEquals(TranslationFeedbackStatusList.Codes.Approved, feedbacks[0].XT_Status);
			checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(1, checkedOutStrings.Length);

			TranslationFeedbackLifecycleManager.UpdateCurrentResourceStrings();
			checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(1, checkedOutStrings.Length);

			AddMockTranslation("kdg1", "Dog", "獒");
			TranslationFeedbackLifecycleManager.UpdateCurrentResourceStrings();

			feedbacks = new TopLevelTranslationFeedbackCollection(factory);
			feedbacks.Load();
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("kdg1", "Dog", "狗", "獒", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			AssertEquals(TranslationFeedbackStatusList.Codes.Current, feedbacks[0].XT_Status);
			checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(0, checkedOutStrings.Length);
		}

		public void TestApproveModified()
		{
			// reviewer modifies and entry before approving it

			AddMockTranslation("kdg1", "Dog", "狗");
			var factory = new BusinessObjectFactory();
			var feedbacks = TranslationFeedbackFactory.Get(factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
			feedbacks[0].XT_SuggestedTranslation = "犬";
			feedbacks.Factory.Save();
			AssertEquals(1, factory.GetDatabaseCount(typeof(EDIInterchange)));
			var interchange = factory.LoadTop1<EDIInterchange>(new ZQuery() { OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " desc" });
			AssertEquals(interchange.EI_From, feedbacks[0].CompanyLicenceCode);
			var entryMessage = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
			var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(1, checkedOutStrings.Length);
			ResetConnection();

			EDIMessage updateMessage;
			List<ITransactionParticipant> participant;

			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				new TranslationFeedbackLifecycleManager.TranslationFeedbackEntryMessageAction().ExecuteAction(entryMessage, new NotificationCollection(), out participant);
				factory = new BusinessObjectFactory();
				var masterFeedbacks = new TopLevelTranslationFeedbackCollection(factory);
				masterFeedbacks.Load();
				AssertEquals(1, masterFeedbacks.Count);
				AssertFeedback("kdg1", "Dog", "狗", "犬", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, masterFeedbacks[0]);
				AssertEquals(TranslationFeedbackStatusList.Codes.New, masterFeedbacks[0].XT_Status);
				masterFeedbacks[0].XT_SuggestedTranslation = "獒";
				masterFeedbacks[0].XT_Status = TranslationFeedbackStatusList.Codes.Approved;
				factory.Save();
				AssertEquals(1, factory.GetDatabaseCount(typeof(EDIInterchange)));
				interchange = factory.LoadTop1<EDIInterchange>(new ZQuery() { OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " desc" });
				AssertEquals(interchange.EI_To, feedbacks[0].CompanyLicenceCode);
				updateMessage = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
				checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(1, checkedOutStrings.Length);
			}
			ResetConnection();

			factory = new BusinessObjectFactory();
			feedbacks = new TopLevelTranslationFeedbackCollection(factory);
			using (var xmlReader = SystemMessage.GetXmlReader(entryMessage))
			{
				var entry = TranslationFeedbackMessagesSerializer.DeserializeTranslationFeedbackEntry(xmlReader);
				feedbacks.Add(StmTranslationFeedback.New(factory, entry));
			}
			factory.Save();
			new TranslationFeedbackLifecycleManager.TranslationFeedbackUpdateMessageAction().ExecuteAction(updateMessage, new NotificationCollection(), out participant);

			factory = new BusinessObjectFactory();
			feedbacks = new TopLevelTranslationFeedbackCollection(factory);
			feedbacks.Load();
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("kdg1", "Dog", "狗", "獒", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			AssertEquals(TranslationFeedbackStatusList.Codes.Approved, feedbacks[0].XT_Status);
			checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals("kdg1", checkedOutStrings[0].HD_Code);
			AssertEquals("獒", checkedOutStrings[0].HD_Caption);
			AssertEquals(1, checkedOutStrings.Length);

			AddMockTranslation("kdg1", "Dog", "獒");
			TranslationFeedbackLifecycleManager.UpdateCurrentResourceStrings();

			feedbacks = new TopLevelTranslationFeedbackCollection(factory);
			feedbacks.Load();
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("kdg1", "Dog", "狗", "獒", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
			AssertEquals(TranslationFeedbackStatusList.Codes.Current, feedbacks[0].XT_Status);
			checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(0, checkedOutStrings.Length);
		}

		public void TestNoNullReferenceExceptionThrown()
		{
			AddMockTranslation("kdg1", "Dog", "狗");
			var factory = new BusinessObjectFactory();
			var feedbacks = TranslationFeedbackFactory.Get(factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
			feedbacks[0].XT_SuggestedTranslation = "犬";
			feedbacks[0].AllContexts.RemoveAll();
			feedbacks.Factory.Save();

			var interchange = factory.LoadTop1<EDIInterchange>(new ZQuery() { OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " desc" });
			var entryMessage = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
			ResetConnection();

			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				List<ITransactionParticipant> participant;
				var action = new TranslationFeedbackLifecycleManager.TranslationFeedbackEntryMessageAction();

				AssertNoExceptionThrown(() => action.ExecuteAction(entryMessage, new NotificationCollection(), out participant));
			}

			factory = new BusinessObjectFactory();
			feedbacks = new TopLevelTranslationFeedbackCollection(factory);
			feedbacks.Load();
			AssertEquals(1, feedbacks.Count);
			AssertFeedback("", "Dog", "狗", "犬", "", TranslationFeedbackMatchTypes.Codes.None, feedbacks[0]);
			AssertEquals(TranslationFeedbackStatusList.Codes.New, feedbacks[0].XT_Status);
			AssertEquals(1, feedbacks[0].AllContexts.Count);
		}

		public void TestDoesNotLoadExistingFeedbackFromWrongLanguage()
		{
			AddMockTranslation("kdg1", "Dog", "狗");
			var factory = new BusinessObjectFactory();
			var feedbacks = TranslationFeedbackFactory.Get(factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
			feedbacks[0].XT_SuggestedTranslation = "犬";
			feedbacks.Factory.Save();

			feedbacks = TranslationFeedbackFactory.Get(factory, Core.SharedConstants.Languages.French, Res.GetData("kdg1", "Dog"));
			AssertEquals(Core.SharedConstants.Languages.French, feedbacks[0].XT_Language);
			feedbacks[0].XT_SuggestedTranslation = "Chien";
			feedbacks.Factory.Save();

			feedbacks = TranslationFeedbackFactory.Get(factory, Core.SharedConstants.Languages.French, Res.GetData("kdg1", "Dog"));
			AssertEquals(Core.SharedConstants.Languages.French, feedbacks[0].XT_Language);
			AssertFeedback("kdg1", "Dog", "Dog", "Chien", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);

			feedbacks = TranslationFeedbackFactory.Get(factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
			AssertEquals(Core.SharedConstants.Languages.ChineseTraditional, feedbacks[0].XT_Language);
			AssertFeedback("kdg1", "Dog", "狗", "犬", ResourceStringDataLevels.Codes.Caption, TranslationFeedbackMatchTypes.Codes.Exact, feedbacks[0]);
		}

		public void TestOverridden()
		{
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				AddMockTranslation("kdg1", "Dog", "狗");
				var factory = new BusinessObjectFactory();
				var feedbacks = TranslationFeedbackFactory.Get(factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
				feedbacks[0].XT_SuggestedTranslation = "犬";
				feedbacks[0].XT_Application = TranslationFeedbackApplicationsList.Codes.Cargowise;
				feedbacks.Factory.Save();

				AddMockTranslation("kdg1", "Dog", "犬");
				TranslationFeedbackLifecycleManager.UpdateCurrentResourceStrings();
				var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(0, checkedOutStrings.Length);
				feedbacks[0].Reload();
				AssertEquals(TranslationFeedbackStatusList.Codes.Current, feedbacks[0].XT_Status);

				AddMockTranslation("kdg1", "Dog", "獒");
				TranslationFeedbackLifecycleManager.UpdateCurrentResourceStrings();
				checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(0, checkedOutStrings.Length);
				feedbacks[0].Reload();
				AssertEquals(TranslationFeedbackStatusList.Codes.Overridden, feedbacks[0].XT_Status);

				AddMockTranslation("kdg1", "Dog", "犬");
				TranslationFeedbackLifecycleManager.UpdateCurrentResourceStrings();
				checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(0, checkedOutStrings.Length);
				feedbacks[0].Reload();
				AssertEquals(TranslationFeedbackStatusList.Codes.Current, feedbacks[0].XT_Status);
			}
		}

		public void TestReverted()
		{
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				AddMockTranslation("kdg1", "Dog", "狗");
				var factory = new BusinessObjectFactory();
				var feedbacks = TranslationFeedbackFactory.Get(factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
				feedbacks[0].XT_SuggestedTranslation = "犬";
				feedbacks[0].XT_Application = TranslationFeedbackApplicationsList.Codes.Cargowise;
				feedbacks.Factory.Save();

				AddMockTranslation("kdg1", "Dog", "犬");
				TranslationFeedbackLifecycleManager.UpdateCurrentResourceStrings();
				var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(0, checkedOutStrings.Length);
				feedbacks[0].Reload();
				AssertEquals(TranslationFeedbackStatusList.Codes.Current, feedbacks[0].XT_Status);

				AddMockTranslation("kdg1", "Dog", "狗");
				TranslationFeedbackLifecycleManager.UpdateCurrentResourceStrings();
				checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(0, checkedOutStrings.Length);
				feedbacks[0].Reload();
				AssertEquals(TranslationFeedbackStatusList.Codes.Reverted, feedbacks[0].XT_Status);

				AddMockTranslation("kdg1", "Dog", "犬");
				TranslationFeedbackLifecycleManager.UpdateCurrentResourceStrings();
				checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(0, checkedOutStrings.Length);
				feedbacks[0].Reload();
				AssertEquals(TranslationFeedbackStatusList.Codes.Current, feedbacks[0].XT_Status);
			}
		}

		public void TestObsoleteCW1()
		{
			Obsolete(TranslationFeedbackApplicationsList.Codes.Cargowise, TranslationFeedbackStatusList.Codes.Obsolete);
		}

		public void TestObsoleteGLW()
		{
			Obsolete(TranslationFeedbackApplicationsList.Codes.Glow, TranslationFeedbackStatusList.Codes.Approved);
		}

		void Obsolete(string application, string expectedStatus)
		{
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				AddMockTranslation("kdg1", "Dog", "狗");
				var factory = new BusinessObjectFactory();
				var feedbacks = TranslationFeedbackFactory.Get(factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
				feedbacks[0].XT_SuggestedTranslation = "犬";
				feedbacks[0].XT_Application = application;
				feedbacks.Factory.Save();

				ENG.Put("kdg1", null);
				CHT.Put("kdg1", null);
				TranslationFeedbackLifecycleManager.UpdateCurrentResourceStrings();
				var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(0, checkedOutStrings.Length);
				feedbacks[0].Reload();
				AssertEquals(expectedStatus, feedbacks[0].XT_Status);
			}
		}

		public void TestOverriden_OriginalTranslationDoesNotMatch()
		{
			AddMockTranslation("kdg1", "Dog", "狗");
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				var factory = new BusinessObjectFactory();
				var feedbacks = TranslationFeedbackFactory.Get(factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
				feedbacks[0].XT_SuggestedTranslation = "犬";
				feedbacks[0].XT_OriginalTranslation = "狗狗";
				feedbacks[0].XT_Status = TranslationFeedbackStatusList.Codes.Approved;
				feedbacks.Factory.Save();

				TranslationFeedbackLifecycleManager.UpdateCurrentResourceStrings();
				var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(0, checkedOutStrings.Length);
				feedbacks[0].Reload();
				AssertEquals(TranslationFeedbackStatusList.Codes.Overridden, feedbacks[0].XT_Status);
			}
		}

		public void TestTranslationIsDeleted()
		{
			AddMockTranslation("kdg1", "Dog", "狗");
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				var factory = new BusinessObjectFactory();
				var feedbacks = TranslationFeedbackFactory.Get(factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
				feedbacks[0].XT_SuggestedTranslation = "犬";
				feedbacks.Factory.Save();

				CHT.Put("kdg1", null);
				TranslationFeedbackLifecycleManager.UpdateCurrentResourceStrings();
				var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(0, checkedOutStrings.Length);
				feedbacks[0].Reload();
				AssertEquals(TranslationFeedbackStatusList.Codes.Obsolete, feedbacks[0].XT_Status);
			}
		}

		public void TestTranslationIsDeletedWithDifferentSuggested()
		{
			AddMockTranslation("kdg1", "Dog", "狗");
			using (TranslationFeedbackConfiguration.EnableIsMasterDatabaseForTest())
			{
				var factory = new BusinessObjectFactory();
				var feedbacks = TranslationFeedbackFactory.Get(factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
				feedbacks[0].XT_SuggestedTranslation = "";
				feedbacks.Factory.Save();

				CHT.Put("kdg1", null);
				TranslationFeedbackLifecycleManager.UpdateCurrentResourceStrings();
				var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertEquals(0, checkedOutStrings.Length);
				feedbacks[0].Reload();
				AssertEquals(TranslationFeedbackStatusList.Codes.Current, feedbacks[0].XT_Status);
			}
		}

		public void TestUpdateTranslationWithNEWStatus()
		{
			AddMockTranslation("kdg1", "Dog", "狗");
			var factory = new BusinessObjectFactory();
			var feedbacks = TranslationFeedbackFactory.Get(factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
			feedbacks[0].XT_SuggestedTranslation = "犬";
			feedbacks.Factory.Save();

			AddMockTranslation("kdg1", "Dog", "犬");
			TranslationFeedbackLifecycleManager.UpdateCurrentResourceStrings();
			var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(0, checkedOutStrings.Length);
			feedbacks[0].Reload();
			AssertNotEquals("The translation feed back with NEW status should be updated", TranslationFeedbackStatusList.Codes.New, feedbacks[0].XT_Status);
			AssertEquals(TranslationFeedbackStatusList.Codes.Current, feedbacks[0].XT_Status);

			AddMockTranslation("kdg2", "Cat", "猫");
			feedbacks = TranslationFeedbackFactory.Get(factory, Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg2", "Cat"));
			feedbacks[0].XT_OriginalTranslation = "猫";
			feedbacks[0].XT_SuggestedTranslation = "猫猫";
			feedbacks.Factory.Save();

			TranslationFeedbackLifecycleManager.UpdateCurrentResourceStrings();
			checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			AssertEquals(1, checkedOutStrings.Length);
			feedbacks[0].Reload();
			AssertEquals("The translation feed back with NEW status should not be updated", TranslationFeedbackStatusList.Codes.New, feedbacks[0].XT_Status);
		}
	}
}
