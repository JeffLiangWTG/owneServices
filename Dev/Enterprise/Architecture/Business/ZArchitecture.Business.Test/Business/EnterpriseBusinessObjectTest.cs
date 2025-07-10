using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class EnterpriseBusinessObjectTest : TestCaseWithFactory
	{
		public void TestAuditColumnsValueFilledForBOThatGeneratedAfterOnSaving()
		{
			var service = new TestAfterOnSavingBOService();
			Factory.ServiceContainer.AddAfterOnSavingService(service);
			var dummyLogged = Factory.NewWithValidTestData<DummyLogged>();
			AssertNullOrEmpty(dummyLogged.ZL2_SystemCreateUser);
			Factory.Save();

			AssertNotNullOrEmpty(dummyLogged.ZL2_SystemCreateUser);
			AssertNotNull(dummyLogged.ZL2_SystemCreateTimeUtc);
			AssertNotNullOrEmpty(dummyLogged.ZL2_SystemLastEditUser);
			AssertNotNull(dummyLogged.ZL2_SystemLastEditTimeUtc);

			var dummies = Factory.Load<DummyLogged>(new ZQuery());
			AssertEquals(2, dummies.Length);

			var newSavedDummy = dummies.Except(dummyLogged).First();
			AssertNotNullOrEmpty(newSavedDummy.ZL2_SystemCreateUser);
			AssertNotNull(newSavedDummy.ZL2_SystemCreateTimeUtc);
			AssertNotNullOrEmpty(newSavedDummy.ZL2_SystemLastEditUser);
			AssertNotNull(newSavedDummy.ZL2_SystemLastEditTimeUtc);

			Factory.ServiceContainer.RemoveAfterOnSavingService<TestAfterOnSavingBOService>();
			Factory.NewWithValidTestData<DummyLogged>();
			Factory.Save();
			dummies = Factory.Load<DummyLogged>(new ZQuery());
			AssertEquals(3, dummies.Length);
		}

		public void TestIOptionalClusterKeyOnSaving()
		{
			var dummyOptionalClusterKeyEntityNotUsingClusterKey = Factory.New<OptionalClusterKeyEntityForTest>();
			var dummyOptionalClusterKeyEntityUsingClusterKey = Factory.New<OptionalClusterKeyEntityForTest>();
			dummyOptionalClusterKeyEntityNotUsingClusterKey.UseClusterKey = false;
			dummyOptionalClusterKeyEntityUsingClusterKey.UseClusterKey = true;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("UseClusterKey = false", 0, dummyOptionalClusterKeyEntityNotUsingClusterKey.ZD1_Number);
				AssertEquals("UseClusterKey = true", 1, dummyOptionalClusterKeyEntityUsingClusterKey.ZD1_Number);
			});
		}

		public void TestPopulateSingleZ0CodeIfNeeded()
		{
			var dummyObj = Factory.New<DummyEnterpriseBizo>();
			dummyObj.PopulateZ0_CodeIfRequired();
			AssertEquals(true, !string.IsNullOrEmpty(dummyObj.Z0_Code));
		}

		public void TestPopulateMultiZ0CodeIfNeeded()
		{
			var dummies = new DummyEnterpriseBizo[10];
			var props = new ZPropertyInfo[10];
			for (var i = 0; i < 10; i++)
			{
				var dummyObj = Factory.New<DummyEnterpriseBizo>();
				dummies[i] = dummyObj;
				props[i] = dummyObj.Z0_CodeInfo;
			}

			dummies[0].PopulateZ0_CodeIfRequired(props);

			CombineAssertions(() =>
			{
				for (var i = 0; i < 10; i++)
				{
					AssertEquals(true, !string.IsNullOrEmpty(dummies[i].Z0_Code));
				}
			});
		}

		public void TestPopulateMultiZ0CodeIfNeeded_ErrorMessage()
		{
			var dummies = new DummyEnterpriseBizo[10];
			var props = new ZPropertyInfo[10];
			for (var i = 0; i < 10; i++)
			{
				var dummyObj = Factory.New<DummyEnterpriseBizo>();
				dummies[i] = dummyObj;
				props[i] = dummyObj.Z0_CodeInfo;
			}

			ErrorReporter.Clear();

			dummies[0].PopulateZ0_CodeIfRequired(props, (factory, properties) => new List<ZString> { "Fail0001" });

			CombineAssertions(() =>
			{
				AssertEquals("reported error key comparison", 1, ErrorReporter.TotalErrorCount);
				AssertEquals("reported error message comparison", "Number of values returned by 'calculateValues' method is unexpected(1 returned but 10 required).", ErrorReporter.LastMessageReported);
			});
			ErrorReporter.Clear();
		}

		public void TestPopulateMultiZ0CodeIfNeeded_NothingDoneAndNoErrorMessage()
		{
			var dummyObj = Factory.New<DummyEnterpriseBizo>();
			Factory.Save();

			ErrorReporter.Clear();

			dummyObj.PopulateZ0_CodeIfRequired();

			AssertEquals("reported error key comparison", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestExtraNotesToRemoveOnDelete()
		{
			const string noteTypeThatIsntIncludedInNotesObject = "ASS";

			var parent = Factory.NewWithValidTestData<DummyBizo>();
			var noteToRemove = Factory.NewWithValidTestData<StmNote>();
			noteToRemove.ST_ParentID = parent.PK;
			noteToRemove.ST_Table = parent.TableName;
			noteToRemove.ST_NoteType = noteTypeThatIsntIncludedInNotesObject;
			noteToRemove.ST_NoteDataAsText = "Hey look now I'm not blank!";

			var noteNotRemoved = Factory.NewWithValidTestData<StmNote>();
			noteNotRemoved.ST_ParentID = parent.PK;
			noteNotRemoved.ST_Table = parent.TableName;
			noteNotRemoved.ST_NoteType = noteTypeThatIsntIncludedInNotesObject;
			noteNotRemoved.ST_NoteDataAsText = "Hey look now I'm not blank!";

			parent.NotesToRemoveOnDelete.Add(noteToRemove);

			parent.Delete();

			Assert("PRE: We aren't just loading/deleting deleting ALL notes as StmNote, if they're loaded as a different type in the factory (DocumentNote/WorkflowNote) there can be issues on Save", !noteNotRemoved.IsDeleted);
			Assert("The notes returned from ExtraNotesToRemoveOnDelete should be deleted with their parent", noteToRemove.IsDeleted);
		}

		[ExpectNoExceptions]
		public void TestDeleteBizoWithLargeBinaryDataShouldNotThrowConcurrencyError()
		{
			var bizo = Factory.NewWithValidTestData<DummyBusinessObjectConcurrencyCheck>();

			var content = "";
			for (var i = 0; i < 1024; i++)
			{
				content += "a";
			}

			var contentBytes = (byte[])ZBlob.FromAscii(content);
			contentBytes[0] = 255;
			contentBytes[1] = 216;
			contentBytes[2] = 255;
			contentBytes[3] = 224; // Do not compress the content

			bizo.Z0_VarBinaryMax = contentBytes;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var bizo2 = newFactory.Load<DummyBusinessObjectConcurrencyCheck>(bizo.PK);
			bizo2.Delete();
			newFactory.Save();
		}

		public void TestDeleteBizoWithLargeBinaryData_DbHits()
		{
			var bizo = Factory.NewWithValidTestData<DummyBusinessObject>();

			var content = "";
			for (var i = 0; i < 1024; i++)
			{
				content += "a";
			}

			var contentBytes = (byte[])ZBlob.FromAscii(content);
			contentBytes[0] = 255;
			contentBytes[1] = 216;
			contentBytes[2] = 255;
			contentBytes[3] = 224; // Do not compress the content

			bizo.Z0_VarBinaryMax = contentBytes;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var bizo2 = newFactory.Load<DummyBusinessObject>(bizo.PK);
			bizo2.Delete();

			var hits = new Dictionary<string, int> {
				{ DummyBizoSchema.Constants.TableName, 1 },
				{ StmDocDataOverrideSchema.Constants.TableName, 1 },
				{ StmUniversalCopySchema.Constants.TableName, 1 },
			};
			AssertDbHits(hits, newFactory);
		}

		class DummyBusinessObjectConcurrencyCheck : DummyBusinessObject
		{
			public DummyBusinessObjectConcurrencyCheck(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
				ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(Z0_VarBinaryMax), ConcurrencyPolicy.Observe);
			}
		}

		public void TestExtraNotesToRemoveOnDeleteDoesntDeleteEmptyNotes()
		{
			const string noteTypeThatIsntIncludedInNotesObject = "ASS";

			var parent = Factory.NewWithValidTestData<DummyBizo>();
			var emptyNote = Factory.NewWithValidTestData<StmNote>();
			emptyNote.ST_ParentID = parent.PK;
			emptyNote.ST_Table = parent.TableName;
			emptyNote.ST_NoteType = noteTypeThatIsntIncludedInNotesObject;
			emptyNote.ST_NoteDataAsText = string.Empty;

			var notEmptyNote = Factory.NewWithValidTestData<StmNote>();
			notEmptyNote.ST_ParentID = parent.PK;
			notEmptyNote.ST_Table = parent.TableName;
			notEmptyNote.ST_NoteType = noteTypeThatIsntIncludedInNotesObject;
			notEmptyNote.ST_NoteDataAsText = "Hey look now I'm not blank!";

			parent.NotesToRemoveOnDelete.Add(emptyNote);
			parent.NotesToRemoveOnDelete.Add(notEmptyNote);

			parent.Delete();

			Assert("We should have deleted the non-empty note with the parent", notEmptyNote.IsDeleted);
			Assert("We should not delete the empty note. Because it is empty it will not be IsSavedByFactory is false and it will never be in the db. If we try to delete it we will earn ourselves concurrency errors.", !emptyNote.IsDeleted);
		}

		public void TestNoteContextsForRelatedNotes()
		{
			DummyWithRemoveNotesOnDelete bizO = Factory.NewWithValidTestData<DummyWithRemoveNotesOnDelete>();
			StmNoteContexts expectedResult = new StmNoteContexts();
			expectedResult.Module = StmNoteContextModule.A;
			expectedResult.Direction = StmNoteContextDirection.A;
			expectedResult.FreightMode = StmNoteContextFreightMode.A;
			AssertEquals("Initial version of NoteContextsForRelatedNotes should return Module=A, Direction=A, FreightMode=A .", expectedResult.ToString(), bizO.NoteContextsForRelatedNotes.ToString());
		}

		class DummyWithRemoveNotesOnDelete : DummyEnterpriseBusinessObject
		{
			public DummyWithRemoveNotesOnDelete(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new StmNoteContexts NoteContextsForRelatedNotes
			{
				get { return base.NoteContextsForRelatedNotes; }
			}
		}

		public void TestDeleteDeletesNotes()
		{
			AssertEquals("Precondition - Dummy.SupportsNotes is true.", true, Dummy.SupportsNotes);

			Dummy.Notes.AddNew();
			AssertEquals("Dummy.Notes should contain 1 note.", 1, Dummy.Notes.GetAllNotes().Count);

			Dummy.Delete();
			AssertEquals("Dummy was deleted, Dummy.Notes should contain 0 notes.", 0, Dummy.Notes.GetAllNotes().Count);

			StmEvent dummy2 = Factory.New<StmEvent>();
			dummy2.SE_Code = "???";
			dummy2.Notes.AddNew();
			Factory.Save();
			AssertEquals("Dummy.Notes should contain 1 note.", 1, dummy2.Notes.GetAllNotes().Count);

			dummy2.Delete();
			Factory.Save();
			AssertEquals("Dummy was deleted, Dummy.Notes should contain 0 notes.", 0, dummy2.Notes.GetAllNotes().Count);
		}

		public void TestDeleteStmALogsInLocalCache_WhenDeletingBusinessObject()
		{
			var dummy2 = Factory.New<DummyBizo>();
			dummy2.SE_Code = "???";
			var dummy2PK = dummy2.PK;
			Factory.Save();

			dummy2.Logs.AddNew(Events.Departure);
			dummy2.Logs.AddNew(Events.Arrival);
			dummy2.Delete();

			var dummyPK = Dummy.PK;
			Dummy.Logs.AddNew(Events.Departure);
			Dummy.Logs.AddNew(Events.Arrival);
			Dummy.Delete();

			Factory.Save();

			var logs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, dummyPK));
			AssertEquals("StmALog in local cache should be deleted.", 0, logs.Length);

			var logs2 = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, dummy2PK));
			Assert("Departure event log should be saved into DB.", logs2.Any(s => s.SL_SE_NKEvent == Events.DepartureCode));
			Assert("Arrival event log should be saved into DB.", logs2.Any(s => s.SL_SE_NKEvent == Events.ArrivalCode));
		}

		public void TestCustomNoteTypesDelegate()
		{
			string autoRatingLogCode = PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description;

			AssertEquals("The note types list contains 1 item", 1, Dummy.NoteTypes.Count);
			Assert("Item is not AutoratingAuditLog", !Dummy.NoteTypes.List.ContainsCode(autoRatingLogCode));

			NoteTypeCollection testColl = new NoteTypeCollection();
			testColl.Add(PredefinedNoteTypes.Instance.AutoRatingAuditLog);
			((IStmNoteParent)Dummy).CustomNoteTypesDelegate += new GetValueDelegate<NoteTypeCollection>(delegate
			{ return testColl; });
			AssertEquals("The note types list contains 2 items", 2, Dummy.NoteTypes.Count);
			Assert("List contains AutoratingAuditLog", Dummy.NoteTypes.List.ContainsCode(autoRatingLogCode));
		}

		public void TestDeleteDoesNotAccessNotesIfBizONotInDbAndNotesNotAccessed()
		{
			Notes dummyfNotes = (Notes)Dummy.GetType().GetField("fNotes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Dummy);
			AssertEquals("Precondition - Dummy.fNotes is null.", null, dummyfNotes);
			AssertEquals("Precondition - Dummy.IsInDatabase is false.", false, Dummy.IsInDatabase);
			AssertEquals("Precondition - Dummy.SupportsNotes is true.", true, Dummy.SupportsNotes);

			Dummy.Delete();
			dummyfNotes = (Notes)Dummy.GetType().GetField("fNotes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Dummy);
			AssertEquals("Dummy.fNotes was not accessed during Dummy.Delete().", null, dummyfNotes);
		}

		public void TestSecurityOverrideProvider()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			AssertEquals(typeof(DefaultAccessSecurityProvider), SecurityOverrideProviderSource.Get(bizO).Provider.GetType());

			Mock<ISecurityOverrideProvider> mockProvider1 = new Mock<ISecurityOverrideProvider>();
			mockProvider1.CallBase = true;
			SecurityOverrideProviderSource.Get(bizO).Provider = mockProvider1.Object;
			AssertEquals(mockProvider1.Object, SecurityOverrideProviderSource.Get(bizO).Provider);

			Mock<ISecurityOverrideProvider> mockProvider2 = new Mock<ISecurityOverrideProvider>();
			mockProvider2.CallBase = true;
			SecurityOverrideProviderSource.Get(bizO).Provider = mockProvider2.Object;
			AssertEquals(mockProvider2.Object, SecurityOverrideProviderSource.Get(bizO).Provider);
		}

		public void TestRegisterCustomFieldBusinessObjectAsChildWhenRunPreSaveValidation()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObjectImplementCustomFieldProvider>();
			dummy.RunPreSaveValidation();
			var children = ((IBusiness)dummy).Children;
			Assert(children.Any(x => x is CustomBusinessObject));

			dummy = Factory.NewWithValidTestData<DummyBusinessObjectImplementCustomFieldProviderButNotEnableRegisterCustomBizoAsChild>();
			dummy.RunPreSaveValidation();
			children = ((IBusiness)dummy).Children;
			Assert(!children.Any(x => x is CustomBusinessObject));
		}

		class DummyBusinessObjectImplementCustomFieldProvider : DummyEnterpriseBizo, ICustomFieldProvider
		{
			public DummyBusinessObjectImplementCustomFieldProvider(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public CustomBusinessObject GetCustomBusinessObject(bool shouldRefresh = false)
			{
				return new CustomBusinessObject(this, GetCustomPropertyCollection());
			}

			protected override bool RegisterCustomBizoAsChild => true;

			ICustomPropertyCollection GetCustomPropertyCollection()
			{
				var values = new Dictionary<string, object>();

				return CustomPropertyCollectionBuilder.GetCustomProperties(
					propertyName =>
					{
						object value;
						return values.TryGetValue(propertyName, out value) ? value : null;
					},
					(propertyName, value) =>
					{
						values[propertyName] = value;
						return true;
					});
			}
		}

		class DummyBusinessObjectImplementCustomFieldProviderButNotEnableRegisterCustomBizoAsChild : DummyBusinessObjectImplementCustomFieldProvider
		{
			public DummyBusinessObjectImplementCustomFieldProviderButNotEnableRegisterCustomBizoAsChild(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool RegisterCustomBizoAsChild => false;
		}

		#region Test Lookups

		public void TestIsLookupsCached()
		{
			PropertyInfo info = typeof(BusinessObject).GetProperty("IsLookupsCachedInBase", BindingFlags.NonPublic | BindingFlags.Instance);
			AssertNotNull("Precondition - ensure BusinessObject.IsLookupsCachedInBase property found", info);
			AssertEquals("BusinessObject.IsLookupsCachedInBase should return true by default.", true, (bool)info.GetValue(Dummy, null));
		}

		public void TestLookupsCachedByDefault()
		{
			AssertEquals("Accessing Dummy.Lookups multiple times should return the same object.", Dummy.Lookups, Dummy.Lookups);
		}

		public void TestLookupsNotCachedWhenIsLookupsCachedReturnsFalse()
		{
			DummyWithoutCachedLookups dummyWithoutCachedLookups = DummyWithoutCachedLookups.New(Factory);
			Assert("Accessing Dummy.Lookups multiple times should return a new object each time.", dummyWithoutCachedLookups.Lookups != dummyWithoutCachedLookups.Lookups);
		}

		class DummyWithoutCachedLookups : DummyBizo
		{
			public static DummyWithoutCachedLookups New(BusinessObjectFactory factory)
			{
				return factory.New<DummyWithoutCachedLookups>();
			}

			public DummyWithoutCachedLookups(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override bool IsLookupsCachedInBase
			{
				get { return false; }
			}
		}

		#endregion

		#region Implementation

		DummyBizo Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyBizo>()); }
		}
		DummyBizo dummy;

		#endregion

		#region Test Classes

		class DummyBizo : StmEvent, IAdditionalNoteProvider
		{
			public DummyBizo(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override NoteTypeCollection NoteTypesCore
			{
				get
				{
					NoteTypeCollection result = new NoteTypeCollection();
					result.Add(PredefinedNoteTypes.Instance.DetailedGoodsDescription);
					return result;
				}
			}

			public DummyLookups Lookups
			{
				get
				{
					if (lookups == null || !IsLookupsCachedInBase)
					{
						lookups = new DummyLookups(this);
					}
					return lookups;
				}
			}
			DummyLookups lookups;

			public ICollection<StmNote> NotesToRemoveOnDelete { get; } = new List<StmNote>();

			IEnumerable<StmNote> IAdditionalNoteProvider.AdditionalNotes => NotesToRemoveOnDelete;
			protected override IAdditionalNoteProvider GetAdditionalNoteProvider() => this;
		}

		class OptionalClusterKeyEntityForTest : DummyClusterKeyChildBizo, IOptionalClusterKeyEntity
		{
			public OptionalClusterKeyEntityForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool UseClusterKey { get; set; }
		}

		#endregion
	}

	class TestAfterOnSavingBOService : IAfterOnSavingBOProcessingService
	{
		public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			businessObjectsInOnSavingOrder.First().Factory.NewWithValidTestData<DummyLogged>();
		}
	}

	[UseSnapshotProtection]
	sealed class EnterpriseBusinessObjectSnapshotTest : TestCase
	{
		#region TestPopulateNumberPropertyIfRequired

		public void TestPopulateNumberPropertyIfRequired()
		{
			var bizoStrategies = new Hashtable();
			var dummyStrategy = new JobNumberTestStrategy();
			bizoStrategies.Add(DummyBizoSchema.Constants.TableName, new TestObjectHandle(new ArrayList() { dummyStrategy }));
			using (ObjectFactory.Substitute("BusinessObjectStrategies", bizoStrategies))
			{
				var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
				var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

				var repository = new MockRepository(MockBehavior.Default);
				var mockFountain1 = repository.Create<INumberFountainProxy>();
				var mockFountain2 = repository.Create<INumberFountainProxy>();

				mockFountain1.Setup(x => x.GetNextFormatted(factory2)).Returns("A0001");
				mockFountain2.Setup(x => x.GetNext(factory2)).Returns(1);

				mockFountain1.Setup(x => x.GetNextFormatted(factory1)).Returns("A0001");
				mockFountain2.Setup(x => x.GetNext(factory1)).Returns(1);

				mockFountain1.Setup(x => x.GetNextFormatted(factory2)).Returns("A0002");
				mockFountain2.Setup(x => x.GetNext(factory2)).Returns(2);

				var biz1 = factory1.New<BusinessObjectForJobNumberTest>();
				var biz2 = factory2.New<BusinessObjectForJobNumberTest>();
				factory2.Saving += (x) => { biz2.SetJobNumber1IfRequired(); };
				biz1.Fountain1 = mockFountain1.Object;
				biz1.Fountain2 = mockFountain2.Object;
				biz2.Fountain1 = mockFountain1.Object;
				biz2.Fountain2 = mockFountain2.Object;
				try
				{
					factory2.Save();
				}
				catch (NotImplementedException) { }
				AssertEquals("Exception was thrown", false, dummyStrategy.ShouldThrowOnSaved);
				AssertEquals("Exception was thrown", false, dummyStrategy.ShouldThrowOnSaving);
				dummyStrategy.ShouldSaveAgainOnSaving = true;
				factory1.Save();
				factory2.Save();
				ExceptionReporterTestListener.Instance.Clear();
				CombineAssertions(() =>
				{
					AssertEquals("biz1 #1", "A0001", biz1.Code1);
					AssertEquals("biz1 #2", 0.1m, biz1.Code2);

					AssertEquals("biz2 #1", "A0002", biz2.Code1);
					AssertEquals("biz2 #2", 0.2m, biz2.Code2);
				});
				AssertEquals(4, dummyStrategy.CallsToOnSaving);
				mockFountain1.VerifyAll();
				mockFountain2.VerifyAll();
			}
		}

		public void TestPopulateNumberPropertyIfRequiredAfterLostConnection()
		{
			// Arrange
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				const string fountainName = nameof(TestPopulateNumberPropertyIfRequiredAfterLostConnection);

				// Using the fake strategy we throw an exception after the numbers have been populated from the Fountain
				var dummyStrategy = new JobNumberTestStrategy();
				using (ObjectFactory.Substitute(
					name: "BusinessObjectStrategies",
					obj: new Hashtable
					{
						{   DummyBizoSchema.Constants.TableName, new TestObjectHandle(new [] { dummyStrategy }) }
					}))
				{
					var factory = new BusinessObjectFactory(connection) { RefreshEnabled = false };
					var fountainProxyMoq = new Mock<INumberFountainProxy>();

					fountainProxyMoq.Setup(m => m.GetNext(It.IsAny<IDbConnected>()))
						.Returns((IDbConnected invocation) =>
						{
							return GetNext((BusinessObjectFactory)invocation, fountainName);
						});
					fountainProxyMoq.Setup(m => m.GetNextFormatted(It.IsAny<IDbConnected>()))
						.Returns((IDbConnected invocation) =>
						{
							return GetNext((BusinessObjectFactory)invocation, fountainName).ToString();
						});

					var businessObject = factory.New<BusinessObjectForJobNumberTest>();
					businessObject.Fountain1 = fountainProxyMoq.Object;
					businessObject.Fountain2 = fountainProxyMoq.Object;

					// Kill our factory's connection and use fountain while our factory is trying to reconnect
					AssertExceptionThrown<NotImplementedException>(() => factory.Save());
					AdoTestUtils.KillConnection(((IDbConnected)factory).Connection);

					using (var extraConnectionToMainDb = Db.NewExtraConnectionToMainDb())
					using (var transaction = extraConnectionToMainDb.BeginTransactionWithManager())
					{
						AssertEquals(1, GetNext(extraConnectionToMainDb, fountainName));
						AssertEquals(2, GetNext(extraConnectionToMainDb, fountainName));
						AssertEquals(3, GetNext(extraConnectionToMainDb, fountainName));
						transaction.CommitTransaction();
					}

					dummyStrategy.ShouldSaveAgainOnSaving = true;
					ExceptionReporterTestListener.Instance.Clear();

					// Act
					factory.Save();

					// Assert
					CombineAssertions(() =>
					{
						AssertEquals("biz #1", "4", businessObject.Code1);
						AssertEquals("biz #2", 0.5m, businessObject.Code2);
					});
					fountainProxyMoq.VerifyAll();
				}
			}
		}

		public void TestPopulateNumberPropertyIfRequiredDoesNotPopulateManuallySetValues()
		{
			// Arrange
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var fountainProxyMock = new Mock<INumberFountainProxy>();
			var businessObject = factory.New<BusinessObjectForJobNumberTest>();
			businessObject.Fountain1 = fountainProxyMock.Object;
			businessObject.Fountain2 = fountainProxyMock.Object;
			businessObject.Code1 = "code";
			businessObject.Code2 = 1;

			// Act
			for (var i = 1; i < 5; i++)
			{
				businessObject.Code1 += i.ToString();
				businessObject.Code2 *= i;

				factory.Save();
			}

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals("biz #1", "code1234", businessObject.Code1);
				AssertEquals("biz #2", 24m, businessObject.Code2);
			});
			fountainProxyMock.VerifyAll();
		}

		public void TestDoNotPopulateNumberPropertyIfAlreadyPopulatedAndSavedToTheDatabase()
		{
			// Arrange
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var fountainProxyMock = new Mock<INumberFountainProxy>();
			var businessObject = factory.New<BusinessObjectForJobNumberTest>();
			businessObject.ignoreInDatabaseCheck = true;
			businessObject.Fountain1 = fountainProxyMock.Object;
			businessObject.Fountain2 = fountainProxyMock.Object;
			fountainProxyMock.Setup(m => m.GetNextFormatted(factory)).Returns("A0001");
			fountainProxyMock.Setup(m => m.GetNext(factory)).Returns(1);

			// Act
			factory.Save();

			businessObject.Z0_Number = 1;
			factory.Save();

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals("biz #1", "A0001", businessObject.Code1);
				AssertEquals("biz #2", 0.1m, businessObject.Code2);
			});
			fountainProxyMock.VerifyAll();
		}

		static long GetNext(IDbConnected dbConnected, string fountainName)
		{
			using (var cmd = dbConnected.Connection.Command(""))
			{
				cmd.CommandText = "FountainGetNexts";
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@Name", SqlDbType.VarChar, fountainName);
				cmd.AddParameter("@Owner", SqlDbType.UniqueIdentifier, Guid.Empty);
				cmd.AddParameter("@Amount", SqlDbType.Int, 1);
				cmd.AddParameter("@MinValue", SqlDbType.BigInt, 1);
				cmd.AddParameter("@MaxValue", SqlDbType.BigInt, 10000);
				cmd.AddParameter("@CanRollover", SqlDbType.Bit, false);
				cmd.AddParameter("@CallInSameTransaction", SqlDbType.Bit, false);

				return (long)cmd.ExecuteScalar();
			}
		}

		class BusinessObjectForJobNumberTest : DummyEnterpriseBizo
		{
			public BusinessObjectForJobNumberTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public INumberFountainProxy Fountain1 { get; set; }
			public INumberFountainProxy Fountain2 { get; set; }

			public override void OnSaving()
			{
				SetJobNumber1IfRequired();
				SetJobNumber2IfRequired();
				base.OnSaving();
			}

			internal bool ignoreInDatabaseCheck { get; set; }

			public void SetJobNumber1IfRequired() => PopulateFormattedNumberPropertyIfRequired(Code1Info, Fountain1, ignoreInDatabaseCheck);
			public void SetJobNumber2IfRequired() => PopulateNumberPropertyIfRequired(Code2Info, objectFactory => new ZDecimal(Fountain2.GetNext(objectFactory) / 10.0), ignoreInDatabaseCheck);

			public ZString Code1
			{
				get => code1;
				set => SetNonPersistentPropertyValue(Code1Info, ref code1, value);
			}
			ZString code1;

			public ZPropertyInfo Code1Info => GetZPropertyInfo(nameof(Code1));

			public ZDecimal Code2
			{
				get => code2;
				set => SetNonPersistentPropertyValue(Code2Info, ref code2, value);
			}
			ZDecimal code2;

			public ZPropertyInfo Code2Info => GetZPropertyInfo(nameof(Code2));

			public ZBool Z0_Bool
			{
				get { return new ZBool(GetValueFromRowSafely(DummyBizoSchema.Z0_Bool)); }
				set
				{
					SetPropertyValue(Z0_BoolInfo, value);
				}
			}

			public ZPropertyInfo Z0_BoolInfo
			{
				get { return GetZPropertyInfo(DummyBizoSchema.Constants.Z0_Bool); }
			}

			public ZInt Z0_Number
			{
				get { return new ZInt(GetValueFromRowSafely(DummyBizoSchema.Z0_Number)); }
				set
				{
					SetPropertyValue(Z0_NumberInfo, value);
				}
			}

			public virtual ZPropertyInfo Z0_NumberInfo
			{
				get { return GetZPropertyInfo(DummyBizoSchema.Constants.Z0_Number); }
			}
		}

		class JobNumberTestStrategy : IBusinessObjectStrategy
		{
			internal bool ShouldThrowOnSaving = true;
			internal bool ShouldSaveAgainOnSaving;
			internal int CallsToOnSaving;
			public void OnSaving(BusinessObject businessObject)
			{
				var biz = businessObject as BusinessObjectForJobNumberTest;
				if (biz != null)
				{
					++CallsToOnSaving;

					if (ShouldThrowOnSaving)
					{
						ShouldThrowOnSaving = false;
						throw new NotImplementedException();
					}

					if (ShouldSaveAgainOnSaving)
					{
						ShouldSaveAgainOnSaving = false;
						biz.Z0_Number = biz.Z0_Number + 1;
						biz.Factory.Save();
						biz.Z0_Number = biz.Z0_Number + 1;
					}
				}
			}

			internal bool ShouldThrowOnSaved = true;
			public void OnSaved(BusinessObject businessObject, bool saveSucceeded)
			{
				var biz = businessObject as BusinessObjectForJobNumberTest;
				if (biz != null)
				{
					if (ShouldThrowOnSaved)
					{
						ShouldThrowOnSaved = false;
						throw new NotImplementedException();
					}
				}
			}

			public void OnFactorySaving(BusinessObject businessObject) { }
			public void BeforeSuccessfulDelete(BusinessObject businessObject) { }
			public DeleteDetails DeleteDetails(BusinessObject businessObject) => null;
			public void FetchForLoad(BusinessObject businessObject) { }
			public void OnDelete(BusinessObject businessObject) { }
			public void OnFactorySaved(BusinessObject businessObject, bool saveSucceeded) { }
			public void OnSaveRollback(BusinessObject businessObject) { }
			public void OnSavingInObjectsWithLateChanges(BusinessObject businessObject) { }
		}

		#endregion TestSetJobNumberIfRequired
	}

	#region DummyEnterpriseBizo

	public class DummyEnterpriseBizo : EnterpriseBusinessObject
	{
		public DummyEnterpriseBizo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Schema

		public abstract class Schema
		{
			public const string TableName = "DummyBizo";
			public const string PK = "Z0_PK";
		}

		#endregion

		public ZString Z0_Code { get; set; }

		#region PropertyInfos
		public ZPropertyInfo Z0_CodeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get => GetZPropertyInfo(DummyBusinessObjectSchema.Constants.Z0_Code);
		}
		#endregion

		public void PopulateZ0_CodeIfRequired()
		{
			var fountain = Env.NumberFountains.JobComInvoiceLineMatchingKey("TEST", JobComInvoiceLineSchema.JI_MatchingKey.MaxLength);
			PopulateFormattedNumberPropertyIfRequired(Z0_CodeInfo, fountain);
		}

		public void PopulateZ0_CodeIfRequired(ZPropertyInfo[] props)
		{
			var fountain = Env.NumberFountains.JobComInvoiceLineMatchingKey("TEST", JobComInvoiceLineSchema.JI_MatchingKey.MaxLength);
			PopulateFormattedNumberPropertyIfRequired(props, fountain, (sourceKeys, factory) => sourceKeys.Count > 1 ? new List<string> { sourceKeys[0] } : new List<string>());
		}

		public void PopulateZ0_CodeIfRequired(ZPropertyInfo[] props, Func<BusinessObjectFactory, ZPropertyInfo[], IList<ZString>> calculateValues)
		{
			PopulateNumberPropertyIfRequired(props, calculateValues);
		}

		public override SchemaGuidColumn PKSchemaColumn => DummyBusinessObjectSchema.PK;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Testing")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CA1810:Initialize reference type static fields inline", Justification = "Testing")]
	public sealed class DummyBusinessObjectSchema : CargoWise.Schema.Schema, ITableSchema
	{
		static DummyBusinessObjectSchema()
		{
			Instance = new DummyBusinessObjectSchema();
			PK = new SchemaPKColumn(Instance, Constants.PK);
			Z0_Code = new SchemaStringColumn(Instance, Constants.Z0_Code, 2, SqlDbType.VarChar, "", !IsNullable, 20);
		}

		DummyBusinessObjectSchema()
		{
		}

		#region SchemaColumns

		public static readonly SchemaPKColumn PK;
		public static readonly SchemaStringColumn Z0_Code;

		#endregion

		#region All

		public static SchemaColumnCollection All => AllHolder.all;

		class AllHolder
		{
			static AllHolder()
			{
				// Empty constructor to prevent initialisation until first member access.
			}

			public static readonly SchemaColumnCollection all = new SchemaColumnCollection(PK, Array.Empty<SchemaColumn>());
		}

		#endregion

		#region Constants

		public static class Constants
		{
			public const string SqlSchemaName = "dbo";
			public const string TableName = "DummyBizo";
			public const string Prefix = "Z0";
			public const string PK = "Z0_PK";
			public const string Z0_Code = "Z0_Code";

			#region Indexes

			public const string PkIndex = "PK_UX__Z0_PK";

			#endregion // Indexes
		}

		#endregion

		#region ITableSchema

		public static readonly DummyBusinessObjectSchema Instance;

		string ITableSchema.SqlSchemaName => Constants.SqlSchemaName;

		string ITableSchema.TableName => Constants.TableName;

		SchemaPKColumn ITableSchema.PK => PK;

		string ITableSchema.PkIndexName => Constants.PkIndex;

		SchemaColumn ITableSchema.GetSchemaColumn(string columnName)
		{
			throw new NotImplementedException();
		}

		SchemaColumnCollection ITableSchema.All => All;

		#endregion
	}

	#endregion
}
