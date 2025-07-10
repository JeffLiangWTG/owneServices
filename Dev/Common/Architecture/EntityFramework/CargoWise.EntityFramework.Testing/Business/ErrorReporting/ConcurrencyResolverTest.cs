using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Env = System.Environment;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ConcurrencyResolverTest : TestCaseWithFactory
	{
		void SetChangeSetReturnExpectation(Mock<IFactoryChangeSet> factoryChangeSet, Mock<IObjectChangeSet> objectChangeSet)
		{
			factoryChangeSet.Setup(m => m.GetChangedObjects()).Returns(new[] { objectChangeSet.Object });
		}

		IApplicationSchemaResolver GetNewSchemaResolver() => ObjectFactory.Get<IApplicationSchemaResolver>();

		public void TestAdditionalDebugInfoIfCantResolve()
		{
			DummyBusinessObject bizo1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizo1.Z0_VarCharMax = "hello";
			Factory.Save();
			bizo1.Z0_VarCharMax = "preved";

			DummyBusinessObject bizo2 = new BusinessObjectFactory() { RefreshEnabled = false }.Load<DummyBusinessObject>(bizo1.PK);
			bizo2.Z0_VarCharMax = "preved";

			DummyBusinessObject bizo3 = new BusinessObjectFactory() { RefreshEnabled = false }.Load<DummyBusinessObject>(bizo1.PK);
			bizo3.Z0_VarCharMax = "boola";
			bizo3.Factory.Save();

			var innerException = new Exception("MESSAGE");
			var concurrencyException = new ZDataConcurrencyException(innerException, bizo2.Row, Db.Connection);
			var ex = new ZSaveConcurrencyException(concurrencyException, Factory);

			var mockery = new MockRepository(MockBehavior.Default);
			var changeSet = mockery.Create<IFactoryChangeSet>(MockBehavior.Strict);
			var notifier = mockery.Create<INotificationHandler>(MockBehavior.Strict);
			var factoryChangeSet = mockery.Create<IFactoryChangeSet>(MockBehavior.Strict);
			var resolver = new ConcurrencyResolver(null, factoryChangeSet.Object, notifier.Object);
			resolver = new ConcurrencyResolver(ex, changeSet.Object, notifier.Object);

			changeSet.Setup(m => m.GetChangedObjects())
				.Returns(new IObjectChangeSet[] { new ObjectChangeSet(bizo1, bizo3, GetNewSchemaResolver()) });

			try
			{
				resolver.Resolve();
				Fail("Should re-throw an exception");
			}
			catch (ZSaveConcurrencyException thrown)
			{
				AssertNotEquals("Don't throw the original exception so we don't lose data.", thrown, ex);
				AssertCollectionContains(ex, thrown.Data.Values.Cast<object>());
			}

			AssertEquals("ConcurrencyResolver_ResolverException", ErrorReporter.LastKeyReported);
			Assert("Should show correct report", ErrorReporter.LastMessageReported.Contains("ConcurrencyResolver could not find any differences between session and database versions of a row."));
			Assert("Should show correct report", ErrorReporter.LastMessageReported.Contains("Session instance Current and Original comparison:"));
			Assert("Should show correct report", ErrorReporter.LastMessageReported.Contains("Row Session CURRENT:"));
			Assert("Should show correct report", ErrorReporter.LastMessageReported.Contains("Row Session ORIGINAL:"));
			Assert("Should show correct report", ErrorReporter.LastMessageReported.Contains("Database instance Current and Original comparison:"));
			Assert("Should show correct report", ErrorReporter.LastMessageReported.Contains("Row DataBase CURRENT:"));
			Assert("Should show correct report", ErrorReporter.LastMessageReported.Contains("Row DataBase ORIGINAL:"));
			Assert("Should show correct report", ErrorReporter.LastMessageReported.Contains("Database UP TO DATE CURRENT row:"));
			Assert("Should show correct report", ErrorReporter.LastMessageReported.Contains("Inner Message = MESSAGE"));
			Assert("Should show correct report", ErrorReporter.LastMessageReported.Contains("Database instance CURRENT and Session instance ORIGINAL comparison:"));
			Assert("Should show correct report", ErrorReporter.LastMessageReported.Contains("Database UP TO DATE ORIGINAL and Session instance ORIGINAL comparison:"));
			Assert("Should show correct report", ErrorReporter.LastMessageReported.Contains("Z0_VarCharMax\r\nSession CURRENT: preved (System.String"));
			Assert("Should show correct report", ErrorReporter.LastMessageReported.Contains("Session ORIGINAL: hello (System.String,"));
			Assert("Should show correct report", ErrorReporter.LastMessageReported.Contains("Row DataBase CURRENT: Version: Current State: Unchanged Z0_PK:"));

			AssertNotContains("Should show correct report", "Column 'Z0_AddInfo' does not belong to table DummyBizo.", ErrorReporter.LastMessageReported);

			AssertContains("LastMessageReported content", "CargoWise.EntityFramework.ZSaveConcurrencyException:", ErrorReporter.LastMessageReported);
			AssertContains("LastMessageReported content", "---> CargoWise.EntityFramework.ZDataConcurrencyException:", ErrorReporter.LastMessageReported);

			AssertEquals("Is issue really reported", 1, ExceptionReporterTestListener.Instance.Count);
			AssertContains("Reported issue is our issue", "ConcurrencyResolver could not find any differences between session and database versions of a row.", ExceptionReporterTestListener.Instance[0].ToString());

			ErrorReporter.Clear();
		}

		public void TestAdditionalDebugInfoIfCantResolve_DbBizoIsSame()
		{
			var bizo1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizo1.Z0_VarCharMax = "hello";
			Factory.Save();
			bizo1.Z0_VarCharMax = "preved";

			var innerException = new Exception("MESSAGE");
			var concurrencyException = new ZDataConcurrencyException(innerException, bizo1.Row, Db.Connection);
			var ex = new ZSaveConcurrencyException(concurrencyException, Factory);

			var mockery = new MockRepository(MockBehavior.Default);
			var changeSet = mockery.Create<IFactoryChangeSet>(MockBehavior.Strict);
			var notifier = mockery.Create<INotificationHandler>(MockBehavior.Strict);
			var resolver = new ConcurrencyResolver(ex, changeSet.Object, notifier.Object);

			changeSet.Setup(m => m.GetChangedObjects())
				.Returns(new IObjectChangeSet[] { new ObjectChangeSet(bizo1, bizo1, GetNewSchemaResolver()) });

			try
			{
				resolver.Resolve();
				Fail("Should re-throw an exception");
			}
			catch (ZSaveConcurrencyException thrown)
			{
				AssertNotEquals("Don't throw the original exception so we don't lose data.", thrown, ex);
				AssertCollectionContains(ex, thrown.Data.Values.Cast<object>());
			}

			AssertEquals("ConcurrencyResolver_ResolverException", ErrorReporter.LastKeyReported);
			Assert("Should show correct report", ErrorReporter.LastMessageReported.Contains("ConcurrencyResolver could not find any differences between session and database versions of a row."));
			Assert("Should show correct report", ErrorReporter.LastMessageReported.Contains("Session instance Current and Original comparison:"));
			Assert("Should show correct report", ErrorReporter.LastMessageReported.Contains("Database instance Current and Original comparison:"));
			Assert("Should show correct report", ErrorReporter.LastMessageReported.Contains("Database instance is same object as session instance"));
			Assert("Should show correct report", ErrorReporter.LastMessageReported.Contains("Database UP TO DATE CURRENT row:"));
			Assert("Should show correct report", !ErrorReporter.LastMessageReported.Contains("Database instance CURRENT and Session instance ORIGINAL comparaison:"));

			ErrorReporter.Clear();
		}

		public void TestResolve_NoErrorReportIfDatabaseValueChangesWereRollback()
		{
			var bizo1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizo1.Z0_VarCharMax = "hello";
			Factory.Save();

			var bizo2 = new BusinessObjectFactory() { RefreshEnabled = false }.Load<DummyBusinessObject>(bizo1.PK);
			bizo2.Delete();

			var innerException = new Exception("MESSAGE");
			var concurrencyException = new ZDataConcurrencyException(innerException, bizo2.Row, Db.Connection);
			var ex = new ZSaveConcurrencyException(concurrencyException, Factory);

			var mockery = new MockRepository(MockBehavior.Default);
			var changeSet = mockery.Create<IFactoryChangeSet>(MockBehavior.Strict);
			var notifier = mockery.Create<INotificationHandler>(MockBehavior.Strict);
			var resolver = new ConcurrencyResolver(ex, changeSet.Object, notifier.Object);

			changeSet.Setup(m => m.GetChangedObjects())
				.Returns(new IObjectChangeSet[] { new ObjectChangeSet(bizo2, bizo1, GetNewSchemaResolver()) });

			var exception = AssertExceptionThrown<ZSaveConcurrencyException>(() => resolver.Resolve());
			AssertEquals("", ErrorReporter.LastKeyReported);
			AssertEquals(true, exception.NotifyUserWithoutErrorReport);
		}

		public void TestResolve_NoErrorReportIfDatabaseValuesWereChangedAgain_StringValue()
		{
			var bizo1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizo1.Z0_VarCharMax = "hello";
			Factory.Save();
			bizo1.Z0_VarCharMax = "newValue";

			var bizo2 = new BusinessObjectFactory() { RefreshEnabled = false }.Load<DummyBusinessObject>(bizo1.PK);
			bizo2.Z0_VarCharMax = "newValue2";
			bizo2.Factory.Save();

			var innerException = new Exception("MESSAGE");
			var concurrencyException = new ZDataConcurrencyException(innerException, bizo1.Row, Db.Connection);
			var ex = new ZSaveConcurrencyException(concurrencyException, Factory);

			var bizo3 = new BusinessObjectFactory() { RefreshEnabled = false }.Load<DummyBusinessObject>(bizo1.PK);
			bizo3.Z0_VarCharMax = "newValue3";
			bizo3.Factory.Save();

			var mockery = new MockRepository(MockBehavior.Default);
			var changeSet = mockery.Create<IFactoryChangeSet>(MockBehavior.Strict);
			var notifier = mockery.Create<INotificationHandler>(MockBehavior.Strict);
			var resolver = new ConcurrencyResolver(ex, changeSet.Object, notifier.Object);

			changeSet.Setup(m => m.GetChangedObjects())
				.Returns(new IObjectChangeSet[] { new ObjectChangeSet(bizo1, bizo3, GetNewSchemaResolver()) });

			AssertExceptionThrown(typeof(ZSaveConcurrencyException), () => resolver.Resolve());
			AssertEquals("", ErrorReporter.LastKeyReported);
		}

		public void TestResolve_NoErrorReportIfDatabaseValuesWereChangedAgain_BoolValue()
		{
			var bizo1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizo1.Z0_Bool = true;
			Factory.Save();
			bizo1.Z0_Bool = false;

			var bizo2 = new BusinessObjectFactory() { RefreshEnabled = false }.Load<DummyBusinessObject>(bizo1.PK);
			bizo2.Z0_Bool = false;
			bizo2.Factory.Save();

			var innerException = new Exception("MESSAGE");
			var concurrencyException = new ZDataConcurrencyException(innerException, bizo1.Row, Db.Connection);
			var ex = new ZSaveConcurrencyException(concurrencyException, Factory);

			var bizo3 = new BusinessObjectFactory() { RefreshEnabled = false }.Load<DummyBusinessObject>(bizo1.PK);
			bizo3.Z0_Bool = true;
			bizo3.Factory.Save();

			var mockery = new MockRepository(MockBehavior.Default);
			var changeSet = mockery.Create<IFactoryChangeSet>(MockBehavior.Strict);
			var notifier = mockery.Create<INotificationHandler>(MockBehavior.Strict);
			var resolver = new ConcurrencyResolver(ex, changeSet.Object, notifier.Object);

			changeSet.Setup(m => m.GetChangedObjects())
				.Returns(new IObjectChangeSet[] { new ObjectChangeSet(bizo1, bizo3, GetNewSchemaResolver()) });

			try
			{
				resolver.Resolve();
				Fail("Should re-throw an exception");
			}
			catch (ZSaveConcurrencyException thrown)
			{
				AssertNotEquals("Don't throw the original exception so we don't lose data.", thrown, ex);
				AssertCollectionContains(ex, thrown.Data.Values.Cast<object>());
			}

			AssertEquals("", ErrorReporter.LastKeyReported);
		}

		public void TestReportsMergeAndDeletesRecordDeletedInDatabase()
		{
			var mockery = new MockRepository(MockBehavior.Default);
			var objectChangeSet = mockery.Create<IObjectChangeSet>(MockBehavior.Strict);
			var factoryChangeSet = mockery.Create<IFactoryChangeSet>(MockBehavior.Strict);
			var notifier = mockery.Create<INotificationHandlerWithMessageOverride>();
			var resolver = new ConcurrencyResolver(null, factoryChangeSet.Object, notifier.Object);
			SetChangeSetReturnExpectation(factoryChangeSet, objectChangeSet);
			string deleteMessage = ConcurrencyResolver.MergeWarningMessage + Env.NewLine + Env.NewLine +
								   ConcurrencyResolver.DeletedObjectsHeader + Env.NewLine +
								   "TYPE (USR)" + Env.NewLine;
			AssertReportsMergeAndDeletesRecordDeletedInDatabase(deleteMessage, objectChangeSet, resolver, notifier);

			mockery = new MockRepository(MockBehavior.Default);
			var notificationHandlerWithMessageOverride = mockery.Create<INotificationHandlerWithMessageOverride>(MockBehavior.Strict);
			notificationHandlerWithMessageOverride.Setup(m => m.MergeWarningMessage).Returns("Merge Warning Message");
			notificationHandlerWithMessageOverride.Setup(m => m.MergedObjectsHeader).Returns("Merged Objects Header");
			notificationHandlerWithMessageOverride.Setup(m => m.DeletedObjectsHeader).Returns("Deleted Objects Header");
			factoryChangeSet = mockery.Create<IFactoryChangeSet>(MockBehavior.Strict);
			objectChangeSet = mockery.Create<IObjectChangeSet>(MockBehavior.Strict);
			resolver = new ConcurrencyResolver(null, factoryChangeSet.Object, notificationHandlerWithMessageOverride.Object);
			SetChangeSetReturnExpectation(factoryChangeSet, objectChangeSet);
			deleteMessage = "Merge Warning Message" + Env.NewLine + Env.NewLine +
							"Deleted Objects Header" + Env.NewLine +
							"TYPE (USR)" + Env.NewLine;
			AssertReportsMergeAndDeletesRecordDeletedInDatabase(deleteMessage, objectChangeSet, resolver, notificationHandlerWithMessageOverride);

			Assert(true);
		}

		void AssertReportsMergeAndDeletesRecordDeletedInDatabase(string deleteMessage, Mock<IObjectChangeSet> objectChangeSet, ConcurrencyResolver resolver, Mock<INotificationHandlerWithMessageOverride> notifier)
		{
			objectChangeSet.Setup(m => m.IsExistsInDatabase).Returns(false);
			objectChangeSet.Setup(m => m.DisplayName).Returns("TYPE");
			objectChangeSet.Setup(m => m.LastModified).Returns("USR");
			objectChangeSet.Setup(m => m.SessionInstance).Returns((BusinessObject)null);
			notifier.Setup(m => m.ReportInformation(deleteMessage, "WARNING"));
			objectChangeSet.Setup(m => m.Delete());

			resolver.Resolve();
		}

		public void TestReportsMergeAndCallsMergeForModifiedRecords()
		{
			var mockery = new MockRepository(MockBehavior.Default);
			var objectChangeSet = mockery.Create<IObjectChangeSet>(MockBehavior.Strict);
			var factoryChangeSet = mockery.Create<IFactoryChangeSet>(MockBehavior.Strict);
			var notifier = mockery.Create<INotificationHandlerWithMessageOverride>();
			var resolver = new ConcurrencyResolver(null, factoryChangeSet.Object, notifier.Object);
			SetChangeSetReturnExpectation(factoryChangeSet, objectChangeSet);
			string mergeMessage = ConcurrencyResolver.MergeWarningMessage + Env.NewLine + Env.NewLine +
								  ConcurrencyResolver.MergedObjectsHeader + Env.NewLine +
								  "TYPE (USR)" + Env.NewLine +
								  "\tPROPERTY" + Env.NewLine;
			AssertReportsMergeAndCallsMergeForModifiedRecords(mergeMessage, objectChangeSet, resolver, notifier);

			mockery = new MockRepository(MockBehavior.Default);
			var notificationHandlerWithMessageOverride = mockery.Create<INotificationHandlerWithMessageOverride>(MockBehavior.Strict);
			notificationHandlerWithMessageOverride.Setup(m => m.MergeWarningMessage).Returns("Merge Warning Message");
			notificationHandlerWithMessageOverride.Setup(m => m.MergedObjectsHeader).Returns("Merged Objects Header");
			factoryChangeSet = mockery.Create<IFactoryChangeSet>(MockBehavior.Strict);
			objectChangeSet = mockery.Create<IObjectChangeSet>(MockBehavior.Strict);
			resolver = new ConcurrencyResolver(null, factoryChangeSet.Object, notificationHandlerWithMessageOverride.Object);
			SetChangeSetReturnExpectation(factoryChangeSet, objectChangeSet);
			mergeMessage = "Merge Warning Message" + Env.NewLine + Env.NewLine +
							"Merged Objects Header" + Env.NewLine +
							"TYPE (USR)" + Env.NewLine +
							"\tPROPERTY" + Env.NewLine;
			AssertReportsMergeAndCallsMergeForModifiedRecords(mergeMessage, objectChangeSet, resolver, notificationHandlerWithMessageOverride);

			Assert(true);
		}

		public void TestReportsCannotDeleted()
		{
			var mockery = new MockRepository(MockBehavior.Default);
			var objectChangeSet = mockery.Create<IObjectChangeSet>(MockBehavior.Strict);
			var factoryChangeSet = mockery.Create<IFactoryChangeSet>(MockBehavior.Strict);
			var dummyBusinessObejct = Factory.New<DummyBusinessObjectForConcurrencyCannotDeleteTest>();
			var notifier = mockery.Create<INotificationHandlerWithMessageOverride>();
			var resolver = new ConcurrencyResolver(null, factoryChangeSet.Object, notifier.Object);
			objectChangeSet.Setup(m => m.DisplayName).Returns("TYPE");
			objectChangeSet.Setup(m => m.LastModified).Returns("USR");
			SetChangeSetReturnExpectation(factoryChangeSet, objectChangeSet);
			var expectedMessage = ConcurrencyResolver.CannotDeleteMessage + Env.NewLine + Env.NewLine +
								   ConcurrencyResolver.CannotDeleteObjectsHeader + Env.NewLine +
								   "TYPE (USR)" + Env.NewLine;
			string result = string.Empty;
			objectChangeSet.Setup(m => m.IsExistsInDatabase).Returns(false);
			objectChangeSet.Setup(m => m.SessionInstance).Returns(dummyBusinessObejct);
			objectChangeSet.Setup(m => m.IsModifiedInDatabase).Returns(true);

			notifier.Setup(x => x.ReportError(It.IsAny<string>(), It.IsAny<string>(), null, null)).Callback<string, string, string, Exception>((s1, s2, s3, s4) => result = s1);

			resolver.Resolve();
			AssertEquals(expectedMessage, result);
		}

		void AssertReportsMergeAndCallsMergeForModifiedRecords(string mergeMessage, Mock<IObjectChangeSet> objectChangeSet, ConcurrencyResolver resolver, Mock<INotificationHandlerWithMessageOverride> notifier, BusinessObject sessionInstance = null)
		{
			var mockery = new MockRepository(MockBehavior.Default);
			var mockRecord = mockery.Create<IPropertyRecord>(MockBehavior.Strict);

			objectChangeSet.Setup(m => m.IsExistsInDatabase).Returns(true);
			objectChangeSet.Setup(m => m.IsModifiedInDatabase).Returns(true);
			objectChangeSet.Setup(m => m.CanMerge()).Returns(true);
			objectChangeSet.Setup(m => m.DisplayName).Returns("TYPE");
			objectChangeSet.Setup(m => m.LastModified).Returns("USR");
			objectChangeSet.Setup(m => m.SessionInstance).Returns(sessionInstance);
			objectChangeSet.Setup(m => m.MergeableProperties).Returns(new List<IPropertyRecord>(new[] { mockRecord.Object }));
			mockRecord.Setup(m => m.HasChangedInDatabase).Returns(true);
			mockRecord.Setup(m => m.DisplayName).Returns("PROPERTY");
			notifier.Setup(m => m.ReportInformation(mergeMessage, "WARNING"));
			objectChangeSet.Setup(m => m.Merge());

			resolver.Resolve();
		}

		public void TestReportsMergeAndCallsMergeForModifiedRecords_Decorator()
		{
			var mockery = new MockRepository(MockBehavior.Default);
			var objectChangeSet = mockery.Create<IObjectChangeSet>(MockBehavior.Strict);
			var factoryChangeSet = mockery.Create<IFactoryChangeSet>(MockBehavior.Strict);
			var sessionInstance = Factory.New<DummyBusinessObjectWithConcurrencyExceptionDecorator>();
			var notifier = mockery.Create<INotificationHandlerWithMessageOverride>();
			var resolver = new ConcurrencyResolver(null, factoryChangeSet.Object, notifier.Object);
			SetChangeSetReturnExpectation(factoryChangeSet, objectChangeSet);
			var mergeMessage = ConcurrencyResolver.MergeWarningMessage + Env.NewLine + Env.NewLine +
								  ConcurrencyResolver.MergedObjectsHeader + Env.NewLine +
								  "TYPE (USR)" + Env.NewLine +
								  "\tDECORATED" + Env.NewLine;
			AssertReportsMergeAndCallsMergeForModifiedRecords(mergeMessage, objectChangeSet, resolver, notifier, sessionInstance);

			Assert(true);
		}

		class DummyBusinessObjectWithConcurrencyExceptionDecorator : DummyBusinessObject, IConcurrencyExceptionDecorator
		{
			public DummyBusinessObjectWithConcurrencyExceptionDecorator(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public void AppendDecoratedDisplayName(StringBuilder stringBuilder, IPropertyRecord record)
			{
				stringBuilder.AppendLine("\tDECORATED");
			}
		}

		public void TestReportsCantMergeOnCriticalFieldsAndShowsReport()
		{
			var mockery = new MockRepository(MockBehavior.Default);

			var notifier = mockery.Create<INotificationHandlerWithMessageOverride>();
			var factoryChangeSet = mockery.Create<IFactoryChangeSet>(MockBehavior.Strict);
			var objectChangeSet = mockery.Create<IObjectChangeSet>(MockBehavior.Strict);

			var resolver = new ConcurrencyResolver(null, factoryChangeSet.Object, notifier.Object);

			factoryChangeSet.Setup(m => m.GetChangedObjects()).Returns(new[] { objectChangeSet.Object });

			string warningMessage = ConcurrencyResolver.CriticalWarningMessage + Env.NewLine + Env.NewLine +
									ConcurrencyResolver.CriticalObjectsHeader + Env.NewLine +
									"TYPE (USR)" + Env.NewLine +
									"\tPROPERTY (Critical change)" + Env.NewLine;
			AssertReportsCantMergeOnCriticalFieldsAndShowsReport(objectChangeSet, notifier, resolver, warningMessage);

			mockery = new MockRepository(MockBehavior.Default);
			var notificationHandlerWithMessageOverride = mockery.Create<INotificationHandlerWithMessageOverride>();
			notificationHandlerWithMessageOverride.Setup(m => m.CriticalWarningMessage).Returns("Critical Warning Message");
			notificationHandlerWithMessageOverride.Setup(m => m.CriticalObjectsHeader).Returns("Critical Objects Header");
			factoryChangeSet = mockery.Create<IFactoryChangeSet>();
			objectChangeSet = mockery.Create<IObjectChangeSet>();
			resolver = new ConcurrencyResolver(null, factoryChangeSet.Object, notificationHandlerWithMessageOverride.Object);
			factoryChangeSet.Setup(m => m.GetChangedObjects()).Returns(new[] { objectChangeSet.Object });
			warningMessage = "Critical Warning Message" + Env.NewLine + Env.NewLine +
							"Critical Objects Header" + Env.NewLine +
							"TYPE (USR)" + Env.NewLine +
							"\tPROPERTY (Critical change)" + Env.NewLine;
			AssertReportsCantMergeOnCriticalFieldsAndShowsReport(objectChangeSet, notificationHandlerWithMessageOverride, resolver, warningMessage);
			Assert(true);
		}

		void AssertReportsCantMergeOnCriticalFieldsAndShowsReport(Mock<IObjectChangeSet> objectChangeSet, Mock<INotificationHandlerWithMessageOverride> notifier, ConcurrencyResolver resolver, string warningMessage)
		{
			var mockRecord = new Mock<IPropertyRecord>(MockBehavior.Strict);

			objectChangeSet.Setup(m => m.IsExistsInDatabase).Returns(true);
			objectChangeSet.Setup(m => m.IsModifiedInDatabase).Returns(true);
			objectChangeSet.Setup(m => m.CanMerge()).Returns(false);
			objectChangeSet.Setup(m => m.DisplayName).Returns("TYPE");
			objectChangeSet.Setup(m => m.LastModified).Returns("USR");
			objectChangeSet.SetupSequence(m => m.SessionInstance).Returns((BusinessObject)null).Returns((BusinessObject)null).Throws(new Exception("too many calls"));
			objectChangeSet.Setup(m => m.MergeableProperties).Returns(new List<IPropertyRecord>(Array.Empty<IPropertyRecord>()));
			objectChangeSet.Setup(m => m.NonMergeableProperties).Returns(new List<IPropertyRecord>(new[] { mockRecord.Object }));
			mockRecord.Setup(m => m.HasChangedInDatabase).Returns(true);
			mockRecord.Setup(m => m.DisplayName).Returns("PROPERTY");
			notifier.Setup(m => m.ReportInformation(warningMessage, "WARNING"));

			resolver.Resolve();
		}

		public void TestReportsCantMergeOnCriticalFieldsAndShowsReport_WhenNoUserIsAvailable()
		{
			var mockery = new MockRepository(MockBehavior.Default);
			var objectChangeSet = mockery.Create<IObjectChangeSet>(MockBehavior.Strict);
			var factoryChangeSet = mockery.Create<IFactoryChangeSet>(MockBehavior.Strict);
			var notifier = mockery.Create<INotificationHandlerWithMessageOverride>();
			var resolver = new ConcurrencyResolver(null, factoryChangeSet.Object, notifier.Object);
			SetChangeSetReturnExpectation(factoryChangeSet, objectChangeSet);
			string warningMessage = ConcurrencyResolver.CriticalWarningMessage + Env.NewLine + Env.NewLine +
									ConcurrencyResolver.CriticalObjectsHeader + Env.NewLine +
									"TYPE" + Env.NewLine +
									"\tPROPERTY (Critical change)" + Env.NewLine;
			AssertReportsCantMergeOnCriticalFieldsAndShowsReport_WhenNoUserIsAvailable(mockery, warningMessage, objectChangeSet, resolver, notifier);

			mockery = new MockRepository(MockBehavior.Default);
			var notificationHandlerWithMessageOverride = mockery.Create<INotificationHandlerWithMessageOverride>(MockBehavior.Strict);
			notificationHandlerWithMessageOverride.Setup(m => m.CriticalWarningMessage).Returns("Critical Warning Message");
			notificationHandlerWithMessageOverride.Setup(m => m.CriticalObjectsHeader).Returns("Critical Objects Header");
			factoryChangeSet = mockery.Create<IFactoryChangeSet>(MockBehavior.Strict);
			objectChangeSet = mockery.Create<IObjectChangeSet>(MockBehavior.Strict);
			resolver = new ConcurrencyResolver(null, factoryChangeSet.Object, notificationHandlerWithMessageOverride.Object);
			SetChangeSetReturnExpectation(factoryChangeSet, objectChangeSet);
			warningMessage = "Critical Warning Message" + Env.NewLine + Env.NewLine +
							"Critical Objects Header" + Env.NewLine +
							"TYPE" + Env.NewLine +
							"\tPROPERTY (Critical change)" + Env.NewLine;
			AssertReportsCantMergeOnCriticalFieldsAndShowsReport_WhenNoUserIsAvailable(mockery, warningMessage, objectChangeSet, resolver, notificationHandlerWithMessageOverride);

			Assert(true);
		}

		void AssertReportsCantMergeOnCriticalFieldsAndShowsReport_WhenNoUserIsAvailable(MockRepository mockery, string warningMessage, Mock<IObjectChangeSet> objectChangeSet, ConcurrencyResolver resolver, Mock<INotificationHandlerWithMessageOverride> notifier)
		{
			var mockRecord = new Mock<IPropertyRecord>(MockBehavior.Strict);

			objectChangeSet.Setup(m => m.IsExistsInDatabase).Returns(true);
			objectChangeSet.Setup(m => m.IsModifiedInDatabase).Returns(true);
			objectChangeSet.Setup(m => m.CanMerge()).Returns(false);

			objectChangeSet.Setup(m => m.DisplayName).Returns("TYPE");
			objectChangeSet.Setup(m => m.LastModified).Returns("");
			objectChangeSet.Setup(m => m.SessionInstance).Returns((BusinessObject)null);
			objectChangeSet.Setup(m => m.MergeableProperties).Returns(new List<IPropertyRecord>(Array.Empty<IPropertyRecord>()));
			objectChangeSet.Setup(m => m.NonMergeableProperties).Returns(new List<IPropertyRecord>(new[] { mockRecord.Object }));
			mockRecord.Setup(m => m.HasChangedInDatabase).Returns(true);
			mockRecord.Setup(m => m.DisplayName).Returns("PROPERTY");
			notifier.Setup(m => m.ReportInformation(warningMessage, "WARNING"));

			resolver.Resolve();
			resolver.OnMergeFailure = ConcurrencyResolver.OnMergeFailureAction.None;
			resolver.Resolve();

			mockery.VerifyAll();

			objectChangeSet.Verify(m => m.IsExistsInDatabase, Times.Exactly(2));
			objectChangeSet.Verify(m => m.IsModifiedInDatabase, Times.Exactly(2));
			objectChangeSet.Verify(m => m.CanMerge(), Times.Exactly(2));
			objectChangeSet.Verify(m => m.SessionInstance, Times.Exactly(3));
		}

		public void TestDoNothingForObjectsNotModifiedInDatabase()
		{
			var mockery = new MockRepository(MockBehavior.Default);
			var objectChangeSet = mockery.Create<IObjectChangeSet>(MockBehavior.Strict);
			objectChangeSet.Setup(m => m.IsExistsInDatabase).Returns(true);
			objectChangeSet.Setup(m => m.IsModifiedInDatabase).Returns(false);
			var notifier = mockery.Create<INotificationHandler>(MockBehavior.Strict);
			var factoryChangeSet = mockery.Create<IFactoryChangeSet>(MockBehavior.Strict);

			var resolver = new ConcurrencyResolver(null, factoryChangeSet.Object, notifier.Object);
			SetChangeSetReturnExpectation(factoryChangeSet, objectChangeSet);

			resolver.Resolve();

			Assert(true);
		}

		[ExpectNoExceptions]
		public void TestResolve_NullObject()
		{
			DummyBusinessObject sessionInstance = Factory.NewWithValidTestData<DummyBusinessObject>();
			sessionInstance.Z0_VarCharMax = "hello";
			Factory.Save();
			DummyBusinessObject databaseInstance = new BusinessObjectFactory() { RefreshEnabled = false }.Load<DummyBusinessObject>(sessionInstance.PK);
			databaseInstance.Z0_VarCharMax = "hello";

			var innerException = new Exception("exception message");
			var concurrencyException = new ZDataConcurrencyException(innerException, databaseInstance.Row, Db.Connection);
			var ex = new ZSaveConcurrencyException(concurrencyException, Factory);

			var mockery = new MockRepository(MockBehavior.Default);
			var changeSet = mockery.Create<IFactoryChangeSet>(MockBehavior.Strict);
			var notifier = mockery.Create<INotificationHandler>(MockBehavior.Strict);
			var resolver = new ConcurrencyResolver(ex, changeSet.Object, notifier.Object);

			var objectChangeSet = new ObjectChangeSet(sessionInstance, databaseInstance, GetNewSchemaResolver());
			objectChangeSet.SessionInstance.Row = null;

			changeSet.Setup(m => m.GetChangedObjects()).Returns(new ObjectChangeSet[1] { objectChangeSet });

			try
			{
				resolver.Resolve();
			}
			catch (ZSaveConcurrencyException thrown)
			{
				AssertNotEquals("Don't throw the original exception so we don't lose its stacktrace.", thrown, ex);
				AssertCollectionContains(ex, thrown.Data.Values.Cast<object>());
			}

			ErrorReporter.Clear();
		}

		[ExpectNoExceptions]
		public void TestResolve_InvalidOperationException()
		{
			var mockery = new MockRepository(MockBehavior.Default);
			var objectChangeSet = mockery.Create<IObjectChangeSet>(MockBehavior.Strict);
			var sessionInstance = Factory.NewWithValidTestData<DummyBusinessObject>();
			sessionInstance.Z0_VarCharMax = "hello";
			Factory.Save();
			var databaseInstance = new BusinessObjectFactory() { RefreshEnabled = false }.Load<DummyBusinessObject>(sessionInstance.PK);
			databaseInstance.Z0_VarCharMax = "hello";

			var warningMessage = "InvalidOperationException message";
			var innerException = new InvalidOperationException(warningMessage);
			var concurrencyException = new ZDataConcurrencyException(innerException, databaseInstance.Row, Db.Connection);
			var ex = new ZSaveConcurrencyException(concurrencyException, Factory);
			var notifier = mockery.Create<INotificationHandler>(MockBehavior.Strict);
			var factoryChangeSet = mockery.Create<IFactoryChangeSet>(MockBehavior.Strict);
			var resolver = new ConcurrencyResolver(ex, factoryChangeSet.Object, notifier.Object);
			SetChangeSetReturnExpectation(factoryChangeSet, objectChangeSet);

			objectChangeSet.Setup(m => m.IsExistsInDatabase).Returns(true);
			objectChangeSet.Setup(m => m.IsModifiedInDatabase).Returns(false);
			objectChangeSet.Setup(m => m.DatabaseInstance).Returns(databaseInstance);
			objectChangeSet.Setup(m => m.SessionInstance).Returns(sessionInstance);
			notifier.Setup(m => m.ReportInformation(warningMessage, "WARNING"));

			resolver.Resolve();
			mockery.VerifyAll();

			objectChangeSet.Verify(m => m.SessionInstance, Times.Exactly(2));
		}

		#region Test IConflictWithCriticalFields

		public void TestSetConflictWithCriticalFieldsBusinessContext()
		{
			var sessionInstance = Factory.NewWithValidTestData<DummyObjectImplConflictWithCriticalFields>();
			sessionInstance.Z0_VarCharMax = "hello";
			Factory.Save();
			var databaseInstance = new BusinessObjectFactory() { RefreshEnabled = false }.Load<DummyObjectImplConflictWithCriticalFields>(sessionInstance.PK);
			databaseInstance.Z0_VarCharMax = "world";
			databaseInstance.Factory.Save();

			sessionInstance.Z0_VarCharMax = "!";

			var mockery = new MockRepository(MockBehavior.Default);
			var changeSet = mockery.Create<IFactoryChangeSet>(MockBehavior.Strict);
			var notifier = mockery.Create<INotificationHandler>(MockBehavior.Strict);
			var resolver = new ConcurrencyResolver(null, changeSet.Object, notifier.Object) { OnMergeFailure = ConcurrencyResolver.OnMergeFailureAction.None };
			var objectChangeSet = new ObjectChangeSet(sessionInstance, databaseInstance, GetNewSchemaResolver());
			objectChangeSet.Populate();

			changeSet.Setup(m => m.GetChangedObjects()).Returns(new ObjectChangeSet[1] { objectChangeSet });

			Assert(!sessionInstance.HasRun);
			resolver.Resolve();
			Assert(sessionInstance.HasRun);
		}

		class DummyObjectImplConflictWithCriticalFields : DummyBusinessObject, IConflictWithCriticalFields
		{
			public DummyObjectImplConflictWithCriticalFields(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				ConcurrencyInfo.SetConcurrencyPolicy(this.Row, "Z0_VarCharMax", ConcurrencyPolicy.Strict);
			}

			void IConflictWithCriticalFields.SetConflictWithCriticalFieldsBusinessContext()
			{
				HasRun = true;
			}

			public bool HasRun
			{
				get;
				private set;
			}
		}

		#endregion

		#region CustomConcurrencyResolve

		public void TestCustomConcurrencyResolve_ShouldCallCustomResolverOnBusinessObject()
		{
			var dummy = Factory.New<DummyBusinessObjectForConcurrencyTest>();
			var mockery = new MockRepository(MockBehavior.Default);
			var objectChangeSet = mockery.Create<IObjectChangeSet>(MockBehavior.Strict);

			objectChangeSet.Setup(o => o.IsExistsInDatabase).Returns(true);
			objectChangeSet.Setup(o => o.IsModifiedInDatabase).Returns(false);
			objectChangeSet.Setup(o => o.SessionInstance).Returns(dummy);
			objectChangeSet.Setup(o => o.MergeableProperties).Returns(new List<IPropertyRecord>());
			objectChangeSet.Setup(o => o.Merge());

			var notifier = mockery.Create<INotificationHandler>(MockBehavior.Strict);
			var factoryChangeSet = mockery.Create<IFactoryChangeSet>(MockBehavior.Strict);
			var resolver = new ConcurrencyResolver(null, factoryChangeSet.Object, notifier.Object);
			SetChangeSetReturnExpectation(factoryChangeSet, objectChangeSet);
			resolver.OnMergeFailure = ConcurrencyResolver.OnMergeFailureAction.None;
			resolver.Mergeable_ForTest.Add(objectChangeSet.Object);

			resolver.Resolve();
			Assert("Should have called custom resolver on the Business Object", dummy.HasBeenCalled);
			Assert("Should have called custom resolver After Merge on the Business Object", dummy.HasBeenCalledAfterMerge);

			objectChangeSet.Verify(o => o.SessionInstance, Times.Exactly((2)));
			objectChangeSet.Verify(o => o.MergeableProperties, Times.Exactly((2)));

			Assert(true);
		}

		#endregion

		#region TestResolveDeleteWithChangesInDb

		public void TestResolveDeleteWithChangesInDb()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Description = "xyz";
			Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var otherDummy = otherFactory.Load<DummyBusinessObject>(dummy.PK);
			otherDummy.Z0_Description = "abc";
			otherFactory.Save();

			dummy.Delete();

			try
			{
				Factory.Save();
				Fail("Should throw concurrency exception");
			}
			catch (ZSaveConcurrencyException ex)
			{
				var notificationHandler = new TestNotificationHandler();
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, notificationHandler);

				AssertContains("The following objects have changes and will be merged:", notificationHandler.ReportedMessage);
				AssertEquals(true, dummy.IsDeleted);
				AssertEquals("Should be merged", "abc", dummy.Row[DummyBizoSchema.Constants.Z0_Description, DataRowVersion.Original].ToString());
				Factory.Save();
			}
		}

		public void TestResolveDeleteWithProtectedChangesInDb()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			ConcurrencyInfo.SetConcurrencyPolicy(dummy, "Z0_Description", ConcurrencyPolicy.Protect);
			dummy.Z0_Description = "xyz";
			Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var otherDummy = otherFactory.Load<DummyBusinessObject>(dummy.PK);
			otherDummy.Z0_Description = "abc";
			otherFactory.Save();

			dummy.Delete();

			try
			{
				Factory.Save();
				Fail("Should throw concurrency exception");
			}
			catch (ZSaveConcurrencyException ex)
			{
				var notificationHandler = new TestNotificationHandler();
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, notificationHandler);

				AssertContains("The following objects have critical changes and cannot be merged:", notificationHandler.ReportedMessage);

				AssertExceptionThrown<ZSaveConcurrencyException>(Factory.Save);
			}

			AssertEquals(true, dummy.IsDeleted);
			AssertEquals("Should not be merged", "xyz", dummy.Row[DummyBizoSchema.Constants.Z0_Description, DataRowVersion.Original].ToString());
		}

		class TestNotificationHandler : INotificationHandler
		{
			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
			}

			public void ReportInformation(string message, string caption)
			{
				ReportedMessage = message;
			}

			public string ReportedMessage { get; private set; }
		}

		#endregion

		public class DummyBusinessObjectForConcurrencyTest : DummyBusinessObject
		{
			bool hasBeenCalled;
			bool hasBeenCalledAfterMerge;
			public bool HasBeenCalled => hasBeenCalled;
			public bool HasBeenCalledAfterMerge => hasBeenCalledAfterMerge;

			public DummyBusinessObjectForConcurrencyTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void OnConcurrencyExceptionCore(IEnumerable<IPropertyRecord> propertyRecords)
			{
				hasBeenCalled = true;
			}

			protected override void OnConcurrencyExceptionAfterMergeCore(IEnumerable<IPropertyRecord> propertyRecords)
			{
				hasBeenCalledAfterMerge = true;
			}
		}

		public class DummyBusinessObjectForConcurrencyCannotDeleteTest : DummyBusinessObject
		{
			public override bool CanDelete => false;

			public DummyBusinessObjectForConcurrencyCannotDeleteTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}
	}
}
