using System;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class BaseStmALogTest : EnterpriseBusinessObjectTestCase
	{
		public void TestEventsForBindingLookupWhenProductivityWiseModeEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			var log = Factory.New<BaseStmALog>();

			var codeList = log.EventsForBinding.GetAllCodes();
			AssertCollectionContains("Valid PW event, Valid CW1 event", Events.AddedARecordToTheSystem.Code, codeList);
			AssertCollectionNotContains("Invalid PW event, Valid CW1 event", Events.CanadianCADLateSendingWarning.Code, codeList);
			AssertCollectionNotContains("Invalid PW event, Invalid CW1 event", "Z0Z", codeList); // This is a code for a non-existent event for testing
		}

		public void TestErrorReportSL_TableIsDifferentToMasterTableInOnSaving()
		{
			using (SubstituteEnvironmentForServiceTask())
			{
				var factory = new BusinessObjectFactory();
				var note = factory.NewWithValidTestData<StmNote>();
				var log = factory.New<BaseStmALog>();
				log.Master = note;

				var obj = factory.New<DummyBizOWithAutoLogs>() as IStmALogParent;
				log.SL_Table = obj.LogsParentTableName;
				note.ST_Table = obj.LogsParentTableName;

				AssertEquals("Setting SL_Table to 'DummyBizo' which does not match the master TableName 'StmNote'", ErrorReporter.LastMessageReported);
				AssertEquals("ChangingSL_Table_NotMatchMaster_PRD", ErrorReporter.LastKeyReported);

				AssertNoExceptionThrown(() => factory.Save());

				AssertEquals("SL_Table is different to Master table in OnSaving. Master table 'StmNote', SL_Table 'DummyBizo', Event code ''.", ErrorReporter.LastMessageReported);
				AssertEquals("ChangingSL_Table_PRD", ErrorReporter.LastKeyReported);

				ErrorReporter.Clear();
			}
		}

		[TestDate(2024, 03, 04, 00, 0, 0)]
		public void TestErrorReportWhenSettingSL_Table()
		{
			using (SubstituteEnvironmentForServiceTask())
			{
				var log = Factory.New<BaseStmALog>();

				log.SL_Table = "TT";
				AssertEquals("Setting SL_Table to a short 'non tablename' value - TT", ErrorReporter.LastMessageReported);
				AssertEquals("ShortSL_Table_PRD", ErrorReporter.LastKeyReported);

				ErrorReporter.Clear();

				var obj = Factory.New<DummyBizOWithAutoLogs>() as IStmALogParent;
				log.Master = obj;

				AssertNoExceptionThrown(() => Factory.Save());
				AssertEquals("log should be already in database", true, log.IsInDatabase);

				log.SL_Table = "StmNode";
				AssertEquals("SL_Table may not be changed once a log record has been saved. Changed from 'DummyBizo' to 'StmNode', Event code '', Posted 04-Mar-24 00:00.", ErrorReporter.LastMessageReported);
				AssertEquals("ChangingSL_Table_TryingToSetAfterSaved_PRD", ErrorReporter.LastKeyReported);

				log.SL_Table = obj.LogsParentTableName;
				AssertEquals("SL_Table for a saved log record does not match master TableName: changing 'StmNode' to 'DummyBizo', Event code '', Posted 04-Mar-24 00:00.", ErrorReporter.LastMessageReported);
				AssertEquals("ChangingSL_Table_OldValueExists_PRD", ErrorReporter.LastKeyReported);

				ErrorReporter.Clear();
			}
		}

		IDisposable SubstituteEnvironmentForServiceTask()
		{
			var environment = new Mock<IEnvironment>();
			environment.Setup(e => e.ServiceTaskCode).Returns("PRD");

			var env = new Mock<IEnv>();
			env.Setup(e => e.Instance).Returns(environment.Object);

			return ObjectFactory.Substitute(env.Object);
		}

		public void TestConcurrencyErrorDoesNotOccurWhen2PeopleCancelTheSameEvent()
		{
			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var dummyBO = factory1.New<DummyEnterpriseBusinessObject>();
			var log = dummyBO.Logs.AddNew(Events.Arrival);
			factory1.Save();

			var dummyBOReloaded = factory2.Load<DummyEnterpriseBusinessObject>(dummyBO.PK);
			var logReloaded = dummyBOReloaded.Logs.Find(o => o.SL_SE_NKEvent == Events.ArrivalCode).Single();
			logReloaded.Cancel();
			factory2.Save();

			log.Cancel();
			AssertNoExceptionThrown(delegate
			{
				factory1.Save();
			});
		}

		public void TestCompanyCode()
		{
			var log = Factory.New<BaseStmALog>();
			AssertEquals(ZString.Empty, log.CompanyCode);
			log.SL_GB_NKBranch = EnvProxy.Instance.CurrentBranch.Code;
			AssertEquals(EnvProxy.Instance.CurrentCompany.Code, log.CompanyCode);
		}

		public void TestSL_ReferenceForBinding()
		{
			var log = Factory.New<BaseStmALog>();
			log.SL_Reference = "11";
			AssertEquals("11", log.SL_ReferenceForBinding);

			string guid = Guid.NewGuid().ToString();
			log.SL_Reference = guid;
			AssertEquals("", log.SL_ReferenceForBinding);
			AssertEquals(guid, log.SL_Reference);

			log.SL_ReferenceForBinding = "z";
			AssertEquals("z", log.SL_ReferenceForBinding);
			AssertEquals("z", log.SL_Reference);

			guid = Guid.NewGuid().ToString();
			log.SL_Reference = string.Concat("ABC|", guid);
			AssertEquals("ABC", log.SL_ReferenceForBinding);
			AssertEquals(String.Concat("ABC|", guid), log.SL_Reference);
		}

		public void TestSL_Reference_StripsNonWesternEuropeanCharacters()
		{
			var log = Factory.New<BaseStmALog>();
			log.SL_Reference = "Aşkın";
			AssertNoErrors(log.SL_ReferenceInfo);
			AssertEquals("Non Western-European characters shoud be removed", "Akn", log.SL_Reference);
		}

		public void TestCheckNotEqualParentAndMaster()
		{
			AssertCheckNotEqualParentAndMaster(true);
			AssertCheckNotEqualParentAndMaster(false);
		}

		public void AssertCheckNotEqualParentAndMaster(bool masterFirst)
		{
			var log = Factory.New<BaseStmALog>();
			var obj1 = Factory.New<DummyBizOWithAutoLogs>() as IStmALogParent;
			var obj2 = Factory.New<DummyEnterpriseBusinessObject>();

			if (masterFirst)
			{
				log.Master = obj1;
			}
			else
			{
				log.SL_Parent = obj2.PK;
			}

			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			if (masterFirst)
			{
				log.SL_Parent = obj2.PK;
			}
			else
			{
				log.Master = obj1;
			}

			var tableName = masterFirst ? "DummyBizo" : "";
			var masterType = "Enterprise.ZArchitecture.Business.Testing.DummyBizOWithAutoLogs";
			var masterPK = (obj1 as DummyBizOWithAutoLogs).PK;
			var expected = string.Format(@"Trying to set different SL_Parent and Master.
Values are:
PK:          {0}
SL_Parent:   {1}
SL_Table:    {2}
MasterType:  {3}
MasterPK:    {4}
MasterTable: {5}", log.PK, obj2.PK, tableName, masterType, masterPK, "DummyBizo");

			AssertMultilineASCIIEquals("Should report correct error when master and sl_parent are not the same", expected, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("SaveAndDelete not supported", true);
		}

		public void TestLoadByQueryStmaLog()
		{
			ZQuery query = new ZQuery(StmALogSchema.PK, ZGuid.NewZGuid());
			AssertExceptionThrown<InvalidOperationException>(() => Factory.Load<StmALog>(query));
		}

		public void TestLoadByPKWithQueryStmaLog()
		{
			AssertExceptionThrown<InvalidOperationException>(() => Factory.Load<BaseStmALog>(ZGuid.NewZGuid()));
		}

		public void TestReloadPerformanceIncreaseColumnValue_PostedTimeEmpty()
		{
			var log = Factory.New<BaseStmALog>();
			log.SL_Reference = "orig";
			log.SL_SE_NKEvent = Events.AddedARecordToTheSystem.Code;
			object postedTime = log.ReloadPerformanceIncreaseColumnValue;

			AssertType(typeof(ZDateTime), postedTime);
			AssertEquals("Default Posted Time valid", true, ((ZDateTime)postedTime).IsValid);
		}

		public void TestReloadPerformanceIncreaseColumnValue_PostedTimeIsValid()
		{
			var log = Factory.NewWithValidTestData<BaseStmALog>();
			log.SL_Reference = "orig";
			log.SL_SE_NKEvent = Events.AddedARecordToTheSystem.Code;
			Factory.Save();

			var logInDatabase = Factory.LoadTop1<BaseStmALog>(new ZQuery(StmALogSchema.PK, log.PK));

			object postedTime = logInDatabase.ReloadPerformanceIncreaseColumnValue;

			AssertType(typeof(ZDateTime), postedTime);
			AssertEquals("Saved Posted Time valid", true, ((ZDateTime)postedTime).IsValid);
		}

		public void TestMasterIsReturnedFromSettingSL_ParentAndSL_TableOnly()
		{
			var bizO = Factory.New<DummyEnterpriseBusinessObject>();
			var log = Factory.New<BaseStmALog>();
			log.SL_Parent = bizO.PK;
			log.SL_Table = bizO.TableName;
			AssertEquals(bizO, log.Master);
		}

		public void TestCanBeCancelledByUser()
		{
			var log = Factory.New<BaseStmALog>();
			log.Master = Factory.New<DummyEnterpriseBusinessObject>();
			log.SL_SE_NKEvent = Events.AddedARecordToTheSystem.Code;
			Assert(!log.CanBeCancelledByUser);

			log.SL_SE_NKEvent = Events.EditedARecord.Code;
			Assert(!log.CanBeCancelledByUser);

			log.SL_SE_NKEvent = Events.DeletedARecordInTheSystem.Code;
			Assert(!log.CanBeCancelledByUser);

			log.SL_SE_NKEvent = Events.ReadRelatedNotes.Code;
			Assert(!log.CanBeCancelledByUser);

			log.SL_SE_NKEvent = Events.RelatedNotesNotRead.Code;
			Assert(!log.CanBeCancelledByUser);

			log.SL_SE_NKEvent = Events.DocumentSent.Code;
			Assert(!log.CanBeCancelledByUser);

			log.SL_SE_NKEvent = Events.DocumentDelivered.Code;
			Assert(!log.CanBeCancelledByUser);

			log.SL_SE_NKEvent = Events.DocumentNotDelivered.Code;
			Assert(!log.CanBeCancelledByUser);

			log.SL_SE_NKEvent = Events.Arrival.Code;
			Assert(log.CanBeCancelledByUser);
		}

		public void TestReload_UsingIndex()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			var log = dummy.GetLogs().AddNew(Events.Authorised);
			Factory.Save();

			var loadingFactory = new BusinessObjectFactory();
			var reloadedLog = (BaseStmALog)((IBusinessObjectReload)log).Reload(loadingFactory);
			AssertEquals("Loaded in the right factory", loadingFactory, reloadedLog.Factory);
			AssertEquals("Loaded the correct item", log.PK, reloadedLog.PK);
		}

		public void TestReload_WhenIndexCannotBeUsed()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			var log = dummy.GetLogs().AddNew(Events.Authorised);
			Factory.Save();
			Db.Connection.ExecuteNonQuery("update dbo.StmALog set SL_SE_NKEvent='XXX' where SL_PK='" + log.PK + "'");

			var loadingFactory = new BusinessObjectFactory();
			var reloadedLog = (BaseStmALog)((IBusinessObjectReload)log).Reload(loadingFactory);
			AssertEquals("No error expected", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			AssertEquals("Loaded in the right factory", loadingFactory, reloadedLog.Factory);
			AssertEquals("Loaded the correct item", log.PK, reloadedLog.PK);
		}

		public void TestGetLogMaster_ShouldReloadIfNotInFactoryCache()
		{
			var declaration = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			var log = declaration.GetLogs().AddNew(Events.Authorised);
			Factory.Save();

			log = new BusinessObjectFactory().LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Parent, declaration.PK));
			AssertNotNull(log.Master);
		}

		public void TestGetLogMasterForTableWithNoBizo_ShouldNotCreateDeveloperException()
		{
			var stmEvent = Factory.New<StmEvent>();
			stmEvent.SE_Code = "ZZZ";
			var log = stmEvent.GetLogs().AddNew(Events.Authorised);
			Factory.Save();

			log = new BusinessObjectFactory().LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Parent, stmEvent.PK));
			AssertNoExceptionThrown(() => { var var = log.Master; });
		}

		public void TestSettingSL_TableDifferentToMaster()
		{
			var log = Factory.New<StmALog>();
			var obj1 = Factory.New<DummyBizOWithReferenceFileTable>() as IStmALogParent;

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.Master = obj1;
				log.SL_Table = "ViewQuotedBooking";
			}
			AssertEquals("Setting SL_Table to 'ViewQuotedBooking' which does not match the master TableName 'DummyBizo'", ErrorReporter.LastMessageReported);
			ErrorReporter.Instance.Clear();
		}

		public void TestGetMasterNotThrowExceptionWithInvalidParentGuid()
		{
			AssertNoExceptionThrown(() => BaseStmALog.GetMaster(ZGuid.Invalid, "Dummy", Factory));
			AssertNoExceptionThrown(() => BaseStmALog.GetMaster(ZGuid.Empty, "Dummy", Factory));
		}

		#region ReferenceForBinding

		public void TestReferenceForBinding_ShouldRemoveGUIDParameter_NotRemoveEverythingAfterGUID()
		{
			var guid = Guid.NewGuid();
			var log = Factory.New<StmALog>();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = "ZIT|" + guid + "|Nope";
			}

			AssertEquals("ZIT|Nope", log.SL_ReferenceForBinding);
			AssertEquals(guid, StmALog.GetGuid(log.SL_Reference));

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = "STAHP|" + guid + "|Pliz";
			}

			AssertEquals("STAHP|Pliz", log.SL_ReferenceForBinding);
			AssertEquals(guid, StmALog.GetGuid(log.SL_Reference));

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = "OOE|" + guid + "|ERR";
			}

			AssertEquals("OOE|ERR", log.SL_ReferenceForBinding);
			AssertEquals(guid, StmALog.GetGuid(log.SL_Reference));

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = guid + "||TYP=MANUAL";
			}

			AssertEquals("|TYP=MANUAL", log.SL_ReferenceForBinding);
			AssertEquals(guid, StmALog.GetGuid(log.SL_Reference));

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = guid + "|TYP=MANUAL";
			}

			AssertEquals("|TYP=MANUAL", log.SL_ReferenceForBinding);
			AssertEquals(guid, StmALog.GetGuid(log.SL_Reference));
		}

		#endregion

		#region Business Object Overrides

		public void TestSettingDefaultUser()
		{
			AssertEquals("A new StmALog should have default user set (SL_GS_NKUser)", StaticCurrentFetcher.Instance.CurrentUser.GS_Code, Log.SL_GS_NKUser);
		}

		[ExpectNoExceptions]
		public void TestSettingDefaultUserIfThereIsNoCurrentUser()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext("XXX", Guid.Empty, Guid.Empty))
			{
				bool originalIsInterative = Globals.IsUserInteractive;
				using (new DisposableAction(() => Globals.IsUserInteractive = originalIsInterative))
				{
					Globals.IsUserInteractive = true;
					Assert(Factory.New<StmALog>().SL_GS_NKUser.IsEmpty);
				}
				using (new DisposableAction(() => Globals.IsUserInteractive = originalIsInterative))
				{
					Globals.IsUserInteractive = false;
					AssertEquals(User.ServiceUserCode, Factory.New<StmALog>().SL_GS_NKUser);
				}
			}
		}

		public void TestDelete()
		{
			var obj1 = Factory.New<DummyBizOWithAutoLogs>() as IStmALogParent;
			var logNotInDB = obj1.Logs.AddNew(Events.CustomisableEvent00);
			logNotInDB.Delete();
			AssertEquals("Log is deleted?", true, logNotInDB.IsDeleted);

			var logInDB = obj1.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();

			try
			{
				logInDB.Delete();
				Fail("Should Throw Exception");
			}
			catch (InvalidOperationException ex)
			{
				Assert("Exception caught (below) is not the one expected.\r\n" + ex.Message, ex.Message.StartsWith("Disallowed attempt to delete an StmALog object already in database"));
			}
		}

		public void TestMultipleDelete()
		{
			var log = Factory.NewWithValidTestData<BaseStmALogForTest>();
			AssertNoExceptionThrown(log.Delete);
		}

		class BaseStmALogForTest : BaseStmALog
		{
			public BaseStmALogForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override void OnDelete()
			{
				Delete();
				base.OnDelete();
			}
		}

		#endregion

		#region Overridden Properties

		public void TestInvalidEventIsSavedAndReportSilentError()
		{
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

			Log.SL_SE_NKEvent = Events.AddedARecordToTheSystem.Code;
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

			Log.SL_SE_NKEvent = "~ZZ"; // purposefully setting an invalid event code for testing
			AssertEquals("Setting Log.SL_SE_NKEvent to an invalid StmEvent code should report an error", 1, ExceptionReporterTestListener.Instance.Count);

			Factory.Save();
			ExceptionReporterTestListener.Instance.Clear();
			var otherFactory = new BusinessObjectFactory();
			AssertNotNull("Should have saved invalid Event code", otherFactory.LoadTop1<BaseStmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, "~ZZ")));
		}

		public void TestSL_Table()
		{
			StmALog log = Factory.New<StmALog>();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Table = "StandardTableName";
			}
			AssertEquals("StandardTableName", log.SL_Table);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Table = "SomeReferenceDatabase.dbo.ERFTableName";
			}
			AssertEquals("ERFTableName", log.SL_Table);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Table = "SomeOtherReferenceDatabase..TRFTableName";
			}
			AssertEquals("TRFTableName", log.SL_Table);
		}

		#endregion

		#region New Properties

		public void TestSL_EventDescription()
		{
			AssertEquals("Log.SL_EventDescription should be empty", ZString.Empty, Log.SL_EventDescription);
			Log.SL_SE_NKEvent = Events.AddedARecordToTheSystem.Code;
			AssertEquals("Log.SL_EventDescription should be set", Event.SE_Desc, Log.SL_EventDescription);
		}

		public void TestSL_EventDescription_ForLegacyDeprecatedEventType()
		{
			Log.SL_SE_NKEvent = "CAP"; // deprecated event code... no constant defined
			AssertEquals("Deprecated event code still produces a description", "Cartage Advise Printed", Log.SL_EventDescription);
			ErrorReporter.Clear();
		}

		public void TestSL_TableFriendlyName()
		{
			var dummyWithRelatedNotes = Factory.New<DummyBizOWithRelatedNotes>();

			// SL_TableFriendlyName is only valid when the base bizO's notes are in an StmALogCollectionView (as Log.IsRelated is dependent on it's parent view collection)
			var logsView = new StmALogCollectionView(dummyWithRelatedNotes);

			var log = dummyWithRelatedNotes.Logs.AddNew(Events.Arrival);
			AssertEquals("Not a related Log.", "This " + dummyWithRelatedNotes.TableName, log.SL_TableFriendlyName);
			log.SuffixForTableFriendlyName = "ZZZ";
			AssertEquals("Not a related Log.", "This " + dummyWithRelatedNotes.TableName + " ZZZ", log.SL_TableFriendlyName);

			var relatedLog = dummyWithRelatedNotes.RelatedDummy.Logs.AddNew();
			AssertEquals("A related Log.", dummyWithRelatedNotes.TableName, relatedLog.SL_TableFriendlyName);
		}

		public void TestSL_TableFriendlyNameNotAffectedByTranslationOnLanguageChange()
		{
			var dummyStaff = Factory.New<IGlbStaff>();
			dummyStaff.GS_WorkingLanguage = Enterprise.Core.SharedConstants.Languages.Spanish;
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(dummyStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				AssertEquals("This Note", Factory.New<DummyWithRelatedNameProvider>().Logs.AddNew().SL_TableFriendlyName);
				AssertEquals(dummyStaff.GS_WorkingLanguage, ObjectFactory.Get<IResourceStrings>().CurrentLanguage);
			}
		}

		public void TestSL_TableFriendlyNameForBindingAffectedByTranslationOnLanguageChange()
		{
			var dummyStaff = Factory.New<IGlbStaff>();
			dummyStaff.GS_WorkingLanguage = Enterprise.Core.SharedConstants.Languages.Spanish;
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(dummyStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				AssertEquals("Este/a Nota", Factory.New<DummyWithRelatedNameProvider>().Logs.AddNew().SL_TableFriendlyNameForBinding.ToString());
				AssertEquals(dummyStaff.GS_WorkingLanguage, ObjectFactory.Get<IResourceStrings>().CurrentLanguage);
			}
		}

		#region TestSL_ContextDescription

		public void TestSL_ContextDescription()
		{
			var dummyWithRelatedNameProvider = Factory.New<DummyWithRelatedNameProvider>();
			dummyWithRelatedNameProvider.Z0_Description = "Main Dummy";

			var dummy1 = Factory.New<DummyEnterpriseBusinessObject>();
			dummy1.Z0_Description = "Dummy 1";
			var dummy2 = Factory.New<DummyEnterpriseBusinessObject>();
			dummy2.Z0_Description = "Dummy 2";

			var log0 = dummyWithRelatedNameProvider.Logs.AddNew();
			var log1 = dummy1.Logs.AddNew();
			var log2 = dummy2.Logs.AddNew();

			AssertEquals(ZString.Empty, log0.SL_ContextDescription);
			AssertEquals(ZString.Empty, log1.SL_ContextDescription);
			AssertEquals(ZString.Empty, log2.SL_ContextDescription);

			var logsCollection = new StmALogCollectionWithMaster(dummyWithRelatedNameProvider) { log0, log1, log2 };
			AssertEquals(3, logsCollection.Count);

			AssertEquals(ZString.Empty, log0.SL_ContextDescription);
			AssertEquals("Related Dummy 1", log1.SL_ContextDescription);
			AssertEquals("Related Dummy 2", log2.SL_ContextDescription);
		}

		class DummyWithRelatedNameProvider : DummyEnterpriseBusinessObject, IRelatedItemsNameProvider
		{
			public DummyWithRelatedNameProvider(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			ZString IRelatedItemsNameProvider.GetNameOfRelatedItem(IBusiness relatedItem)
			{
				var relatedDummy = relatedItem as DummyEnterpriseBusinessObject;
				if (relatedDummy != null)
				{
					return "Related " + relatedDummy.Z0_Description;
				}
				return ZString.Empty;
			}

			protected override ZString HumanReadableNameCore
			{
				get
				{
					return Res.GetString("11044606-2fa7-4860-9d90-a99821c85536", "Note");
				}
			}
		}

		#endregion

		public void TestSL_UserNameAndInitials()
		{
			Log.SL_GS_NKUser = "";
			AssertEquals("Log.SL_UserNameAndInitials should be empty", ZString.Empty, Log.SL_UserNameAndInitials);

			IGlbStaff user = Factory.New<IGlbStaff>();

			user.GS_FullName = "Fred Flintstone";
			user.GS_Code = "FF";

			Log.SL_GS_NKUser = user.GS_Code;
			AssertEquals("Log.SL_UserNameAndInitials should be set", "Fred Flintstone (FF)", Log.SL_UserNameAndInitials);
		}

		public void TestSL_TableErrorReporter()
		{
			var obj1 = Factory.New<DummyBizOWithAutoLogs>() as IStmALogParent;
			Log.Master = obj1;
			Log.SL_Table = "SomeOtherTable";
			Factory.Save();
			ErrorReporter.Clear();

			Log.SL_Table = obj1.TableName;
			AssertEquals("ChangingSL_Table_OldValueExists", ErrorReporter.LastKeyReported);
			string expected = string.Format("SL_Table for a saved log record does not match master TableName: changing '{0}' to '{1}', Event code '{2}', Posted {3}.",
												"SomeOtherTable", obj1.TableName, Log.SL_SE_NKEvent, Log.SL_PostedTimeUtc.ToLongTimeString());
			AssertMultilineASCIIEquals("Should report correct error when SL_Table changed", expected, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
			Log.SL_Table = "NotTheMasterTableName";
			AssertEquals("ChangingSL_Table_TryingToSetAfterSaved", ErrorReporter.LastKeyReported);
			expected = string.Format("SL_Table may not be changed once a log record has been saved. Changed from '{0}' to '{1}', Event code '{2}', Posted {3}.",
												obj1.TableName, "NotTheMasterTableName", Log.SL_SE_NKEvent, Log.SL_PostedTimeUtc.ToLongTimeString());
			AssertMultilineASCIIEquals("Should report correct error when SL_Table changed", expected, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		[TestDate]
		public void TestChangingSL_Table_OldValueExistsReportsOnlyNewLogs()
		{
			TestDateAttribute.Date = DateTime.UtcNow.AddYears(-2);

			var firstMaster = (IStmALogParent)Factory.New<DummyEnterpriseBusinessObject>();

			Log.SL_Parent = firstMaster.LogsParentPK;
			Log.SL_Table = "SomeOtherTable";
			Log.SL_EventTime = ZDateTime.Now;

			Factory.Save();
			ErrorReporter.Clear();

			TestDateAttribute.Date = DateTime.UtcNow;

			Log.SL_Table = firstMaster.TableName;
			AssertEquals("No error should be reported when SL_Table changed", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestNoErrorReporterForReferenceFileTable()
		{
			var dummy = Factory.New<DummyBizOWithReferenceFileTable>();
			var log = Factory.New<BaseStmALog>();
			AssertEquals("Odyssey_RefDb_Ent_US.DummyBizo", dummy.TableName);
			log.Master = dummy;
			AssertEquals("DummyBizo", log.SL_Table);
			Factory.Save();
			AssertEquals("", ErrorReporter.LastKeyReported);
		}

		class DummyBizOWithReferenceFileTable : DummyEnterpriseBusinessObject, IStmALogParent
		{
			public DummyBizOWithReferenceFileTable(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override string TableName
			{
				get { return "Odyssey_RefDb_Ent_US.DummyBizo"; }
			}

			string IStmALogParent.LogsParentTableName
			{
				get { return "DummyBizo"; }
			}
		}

		#endregion

		#region Related Business Objects

		public void TestUser()
		{
			AssertEquals("User default value", StaticCurrentFetcher.Instance.CurrentUser.PK, Log.User.PK);

			IGlbStaff staff = Factory.New<IGlbStaff>();
			staff.GS_Code = "NEW";

			Log.SL_GS_NKUser = staff.GS_Code;
			AssertNotNull("After setting user code, user not null", Log.User);
			AssertEquals("After setting user code, user not null", Log.SL_GS_NKUser, Log.User.GS_Code);
		}

		#endregion

		#region SL_PostedTimeUtc

		[ExpectExceptionMessage(typeof(NotSupportedException), "Setting SL_PostedTimeUtc is not supported")]
		public void TestSettingSL_PostedTimeIsNotSupported()
		{
			Log.SL_PostedTimeUtc = ZDateTime.Now;
		}

		[TestDate(2019, 4, 15, 13, 53, 59)]
		public void TestSL_PostedTime_IsSetBySave()
		{
			Assert(!Log.SL_PostedTimeUtc.IsValid);

			Factory.Save();

			Assert("The value of Log.SL_PostedTimeUtc should be available after save", Log.SL_PostedTimeUtc.IsValid);
			AssertEquals("Log.SL_PostedTimeUtc should be set to a test time", new ZDateTime(2019, 4, 15, 13, 53, 59), Log.SL_PostedTimeUtc);
		}

		public void TestSL_PostedTime_NotSetOnSavingIfLogIsInDatabase()
		{
			Assert(!Log.SL_PostedTimeUtc.IsValid);

			Factory.Save();
			Assert("Log.SL_PostedTimeUtc.IsValid", Log.SL_PostedTimeUtc.IsValid);

			ZDateTime postedTime = Log.SL_PostedTimeUtc;
			Log.Cancel();
			Factory.Save();
			AssertEquals("Log.SL_PostedTimeUtc should not have changed", postedTime, Log.SL_PostedTimeUtc);
		}

		public void TestSL_PostedTime_NoChangesToLogAfterFactorySave()
		{
			Factory.Save();
			AssertEquals("Log should not have changes after it has saved", false, Log.HasChanges);

			var matches = Factory.Load<BaseStmALog>(new ZQuery(StmALogSchema.SL_PostedTimeUtc, ZDateTime.Now));
			AssertEquals("Log should still have no changes after it has saved and a Factory.Load has occurred", false, Log.HasChanges);
			AssertEquals("Log should still have no changes after it has saved and a Factory.Load has occurred", DataRowState.Unchanged, ((INeedRow)Log).Row.RowState);
		}

		public void TestSL_PostedTime_NumCommandsExecuted()
		{
			Factory.Save();
			var commandCountBefore = Db.Connection.ExecutedCommandCount;

			ZDateTime postedTime = Log.SL_PostedTimeUtc;
			AssertEquals("Getting Log.SL_PostedTimeUtc shouldn't query database", commandCountBefore, Db.Connection.ExecutedCommandCount);
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestPostedLocalBranchTime()
		{
			IEnvironment env = EnvProxy.Instance;
			Factory.Save();

			IBusiness ediHQBranch = (IBusiness)Factory.LoadTop1<IGlbBranch>(new ZQuery(Schema.GlbBranchSchema.GB_BranchName, "EDIHQ"));
			IBusiness singaporeBranch = (IBusiness)Factory.LoadTop1<IGlbBranch>(new ZQuery(Schema.GlbBranchSchema.GB_BranchName, "Singapore Branch"));
			IBusiness brisbaneBranch = (IBusiness)Factory.LoadTop1<IGlbBranch>(new ZQuery(Schema.GlbBranchSchema.GB_BranchName, "BN - AUBNE"));

			ZDateTime time = Log.PostedLocalBranchTime;
			ZDateTime utcTime = Log.SL_PostedTimeUtc.ToDateTime();
			AssertEquals(utcTime.AddHours(10), time);
		}

		[TestDate(2050, 1, 2)]
		public void TestSL_PostedTime_WhenTestDateAttributeApplied()
		{
			Factory.Save();
			AssertEquals(2050, Log.SL_PostedTimeUtc.Year);
			AssertEquals(1, Log.SL_PostedTimeUtc.Month);
			AssertEquals(2, Log.SL_PostedTimeUtc.Day);
		}

		public void TestFilteringBy_SL_PostedTime_AfterSave()
		{
			var beforeSave = (ZDateTime)EnvProxy.Instance.Time.CurrentUtcDateTime.AddSeconds(-5);
			Factory.Save();
			var afterSave = (ZDateTime)EnvProxy.Instance.Time.CurrentUtcDateTime.AddSeconds(5);

			var filter = new ZQuery();
			filter.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThan, beforeSave);
			filter.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.LessThan, afterSave);

			var matches = Factory.Load<BaseStmALog>(filter);
			// Solve the problem that the data generated by running two single test instances of net48 and net8 at the same time is greater than one
			Assert("Should be able to factory load the log even though it is in the local DataTable with an out-of-date SL_PostedTimeUtc", matches.Length <= 2);
			Assert("Should be able to factory load the log even though it is in the local DataTable with an out-of-date SL_PostedTimeUtc", matches.Any(m => m.PK.Equals(Log.PK)));
		}

		public void TestSL_PostedTime_PreSaveValidationDoesNotValidate()
		{
			Assert(!Log.SL_PostedTimeUtc.IsValid);
			Assert(!Log.SL_PostedTimeUtcInfo.HasErrors());

			Log.SL_SE_NKEvent = Events.AddedARecordToTheSystem.Code;
			Log.RunPreSaveValidation();
			Assert(!Log.SL_PostedTimeUtcInfo.HasErrors());
		}

		[ExpectNoExceptions]
		public void TestSL_PostedTimeConcurrencyPolicy()
		{
			ZGuid parentGuid = Factory.New<DummyEnterpriseBusinessObject>().PK;
			Factory.Save();

			var log1 = new BusinessObjectFactory { RefreshEnabled = false }.New<SimpleStmALog>();
			log1.SL_Parent = parentGuid;
			log1.SL_Table = "DummyBizo";
			log1.SL_SE_NKEvent = Events.EditedARecordCode;
			log1.SL_EventTime = ZDateTime.UtcNow;
			log1.SL_GS_NKUser = "M.K";
			log1.SL_PostedTimeUtc = ZDateTime.UtcNow;
			log1.Factory.Save();

			var log2 = new BusinessObjectFactory { RefreshEnabled = false }.LoadTop1<SimpleStmALog>(new ZQuery(StmALogSchema.SL_Parent, parentGuid));
			var log3 = new BusinessObjectFactory { RefreshEnabled = false }.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Parent, parentGuid));
			log3.Factory.Load<DummyEnterpriseBusinessObject>(parentGuid);

			log2.SL_PostedTimeUtc = ZDateTime.UtcNow.AddMinutes(5);
			log2.Factory.Save();

			log3.Cancel();
			log3.Factory.Save();
		}

		class SimpleStmALog : AutoStmALog
		{
			public SimpleStmALog(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		[TestDate(2050, 11, 24, 15, 47, 0)]
		public void TestSL_PostedTimeOriginalValueNotPopulatedOnDataRefresh()
		{
			var parentGuid = Factory.New<DummyEnterpriseBusinessObject>().PK;
			Factory.Save();

			var factory1 = new BusinessObjectFactory { RefreshEnabled = true };
			var factory2 = new BusinessObjectFactory { RefreshEnabled = true };

			var collection = new ActiveBusinessObjectCollection<StmALog>(factory2, new ZQuery(StmALogSchema.SL_Parent, parentGuid));
			AssertEquals(0, collection.Count);

			var log1 = factory1.New<StmALog>();
			log1.Factory.Load<DummyEnterpriseBusinessObject>(parentGuid);
			using (log1.LockForUpdatingKeyFields(false))
			{
				log1.SL_Parent = parentGuid;
				log1.SL_Table = "DummyBizo";
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log1.SL_SE_NKEvent = Events.EditedARecordCode;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log1.SL_EventTime = ZDateTime.UtcNow;
				log1.SL_GS_NKUser = "M.K";
			}

			Assert("SL_PostedTimeUtc.IsEmpty", log1.SL_PostedTimeUtc.IsEmpty);
			log1.Factory.Save();
			AssertEquals(TestDateAttribute.Date, log1.SL_PostedTimeUtc);

			AssertEquals(1, collection.Count);
			var log2 = collection[0];
			AssertEquals(TestDateAttribute.Date, log2.SL_PostedTimeUtc);
			AssertEquals(TestDateAttribute.Date, log2.SL_PostedTimeUtcInfo.Value);
			Assert("Original value should be updated during data refresh", !log2.SL_PostedTimeUtcInfo.OriginalValue.IsEmpty);
		}

		[ExpectNoExceptions]
		public void TestSaveLogAfterDataRefresh()
		{
			var parentGuid = Factory.New<DummyEnterpriseBusinessObject>().PK;
			Factory.Save();

			var factory1 = new BusinessObjectFactory { RefreshEnabled = true };

			var factory2 = new BusinessObjectFactory { RefreshEnabled = true };

			factory1.Load<DummyEnterpriseBusinessObject>(parentGuid);
			factory2.Load<DummyEnterpriseBusinessObject>(parentGuid);
			var collection = new ActiveBusinessObjectCollection<StmALog>(factory2, new ZQuery(StmALogSchema.SL_Parent, parentGuid));
			AssertEquals(0, collection.Count);

			StmALog log1 = factory1.New<StmALog>();
			using (log1.LockForUpdatingKeyFields(false))
			{
				log1.SL_Parent = parentGuid;
				log1.SL_Table = "DummyBizo";
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log1.SL_SE_NKEvent = Events.EditedARecordCode;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log1.SL_EventTime = ZDateTime.UtcNow;
				log1.SL_GS_NKUser = "M.K";
			}
			log1.Factory.Save();

			AssertEquals(1, collection.Count);

			StmALog log2 = collection[0];
			log2.Cancel();
			log2.Factory.Save();
		}

		public void TestGet_SL_PostedTimeUtcFromDb_SL_ParentValueChangesAfterRecordIsSavedButNewValueIsNotUpdatedInDB()
		{
			var factoryforTest = new BusinessObjectFactory { RefreshEnabled = false };
			var logForTest = factoryforTest.New<StmALog>();

			using (logForTest.LockForUpdatingKeyFields(false))
			{
				logForTest.SL_Parent = new ZGuid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC");
				logForTest.SL_EventTime = ZDateTime.UtcNow;
				logForTest.SL_Table = StmDataSchema.Constants.TableName;
			}
			logForTest.Factory.Save();

			using (logForTest.LockForUpdatingKeyFields(false))
			{
				logForTest.SL_Parent = new ZGuid("75366AA0-2314-4F44-97E0-6571E470BC33");
				ErrorReporter.Clear();
			}

			AssertNoExceptionThrown(() => { var a = logForTest.SL_PostedTimeUtc; });
		}

#endregion

		#region Test Classes

		class TestBaseStmALog : BaseStmALog
		{
			public TestBaseStmALog(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZDateTime OnSaving_CompleteTime;

			public override void OnSaving()
			{
				base.OnSaving();
				OnSaving_CompleteTime = DateTime.UtcNow; // latency between ZDateTime and DateTime.Now will affect the outcome of the test. This test may be unreliable if run on a remote database.
				Thread.Sleep(50);
			}
		}

		#endregion

		#region Implementation

		public override void TestSettingDateTimeFieldsWithInvalidDateDoesntCauseTheInvalidDateToGetDefaultedToOtherFields()
		{
			Assert("This should not be run on StmALog as it tries to set fields it can't on this type of object.", true);
		}

		protected override bool CanPersistedObjectBeDeleted
		{
			get { return false; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			Event = Factory.LoadTop1<StmEvent>(new ZQuery(StmEventSchema.SE_Code, "ADD"));

			var obj1 = Factory.New<DummyEnterpriseBusinessObject>();
			Log = Factory.New<TestBaseStmALog>();
			Log.SL_Parent = obj1.PK;
			Log.SL_Table = obj1.TableName;
		}

		protected override BusinessObject GetBusinessObjectInNewFactory(BusinessObject bizObj, BusinessObjectFactory newFactory)
		{
			return newFactory.LoadTop1(bizObj.GetType(), new ZQuery(StmALogSchema.SL_Parent, ((BaseStmALog)bizObj).SL_Parent));
		}

		StmEvent Event;
		TestBaseStmALog Log;
		#endregion

		public void TestEventTimeUtcHasValue()
		{
			var dummy1 = Factory.New<DummyEnterpriseBusinessObject>();
			var date = ZDateTimeOffset.Now;
			var log = dummy1.Logs.AddNew(Events.CustomisableEvent00, date);
			AssertEquals(date.ToUtcDateTime(), log.SL_EventTimeUtc);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestEventTimeOffsetNotUsingBranch()
		{
			var dummy1 = Factory.New<DummyEnterpriseBusinessObject>();
			var date = ZDateTimeOffset.Now;
			var log = dummy1.Logs.AddNew(Events.CustomisableEvent00, date);
			log.SL_GB_NKBranch = "SIN";
			AssertEquals(date, log.SL_EventTimeOffset);
		}

		[TestTimeZoneUNLOCO("TRIST")]
		public void TestEventTimePlacesCorrectTimeInEventTimeUtc()
		{
			var eventTimeLocal = ZDateTime.Now.AddDays(-1);
			var log1 = Factory.New<StmALog>();
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_EventTime = eventTimeLocal.ToZDateTime();
			}
			var utcOffset = TimeFactory.Instance.GetUtcOffsetBasedOnLocal(eventTimeLocal.ToDateTime());
			AssertEquals(eventTimeLocal.AddHours(-utcOffset.TotalHours).ToZDateTime(), log1.SL_EventTimeUtc);
		}

		[TestTimeZoneUNLOCO("TRIST")]
		public void TestEventTimeUtcPlacesCorrectTimeInEventTime()
		{
			var eventTimeUtc = ZDateTime.Now.AddDays(-1);
			var log1 = Factory.New<StmALog>();
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_EventTimeUtc = eventTimeUtc.ToZDateTime();
			}
			var utcOffset = TimeFactory.Instance.GetUtcOffsetBasedOnUtc(eventTimeUtc.ToDateTime());
			AssertEquals(eventTimeUtc.AddHours(utcOffset.TotalHours).ToZDateTime(), log1.SL_EventTime);
		}

		[TestUtcOffset(5, 0, 0)]
		public void TestSettingEventTimeOffsetNotSameAsSettingEventTime()
		{
			var date = ZDateTimeOffset.Now.AddDays(-1); 
			var log1 = Factory.New<StmALog>();
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_EventTime = date.ToZDateTime(); 
			}
			var log2 = Factory.New<StmALog>();
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_EventTimeOffset = date;
			}
			AssertEquals(log1.SL_EventTimeUtc, log2.SL_EventTimeUtc);
		}
	}
}
