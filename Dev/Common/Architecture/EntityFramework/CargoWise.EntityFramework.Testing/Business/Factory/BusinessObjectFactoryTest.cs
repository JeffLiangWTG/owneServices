using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectFactoryTest : TransactionedTestCase
	{
		public void TestOrderByPK_Big()
		{
			AssertOrderByPK(109);
		}

		public void TestOrderByPK_Small()
		{
			AssertOrderByPK(10);
		}

		public void AssertOrderByPK(int amount)
		{
			for (var i = 0; i < amount; ++i)
			{
				var dummyBizO = Factory.New<DummyBusinessObject>();
				dummyBizO.Z0_NVarChar = "xyz";
				if (i == 0)
				{
					dummyBizO.Z0_Guid = ZGuid.Empty;
				}
				else if (i == 1)
				{
					//nothing
				}
				else
				{
					dummyBizO.Z0_Guid = Guid.NewGuid();
				}
			}
			Factory.Save();

			ZQuery q = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			q.OrderBy = DummyBizoSchema.Z0_Guid.Name;
			q.AddToFilter(DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.Equal, "xyz");
			var q2 = new ZQuery();
			q2.OrderBy = DummyBizoSchema.Z0_Guid.Name;
			q2.AddToFilter(DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.Equal, "xyz");
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var bizos = Factory.Load<DummyBusinessObject>(q).Select(x => x.Z0_Guid).ToArray();
			var bizos2 = Factory.Load<DummyBusinessObject>(q2).Select(x => x.Z0_Guid).ToArray();
			var bizos3 = factory2.Load<DummyBusinessObject>(q2).Select(x => x.Z0_Guid).ToArray();
			var bizos4 = factory2.Load<DummyBusinessObject>(q).Select(x => x.Z0_Guid).ToArray();

			AssertArrayEqualsByElements(bizos, bizos2);
			AssertArrayEqualsByElements(bizos2, bizos3);
			AssertArrayEqualsByElements(bizos3, bizos4);
		}

		public void TestLoadIDontMindLoadingASubclassInstead()
		{
			var subclass = Factory.New<DummyLaxSubclass1BusinessObject>();
			AssertEquals(subclass, Factory.Load<DummyLaxBusinessObject>(subclass.PK));
			AssertNotEquals(subclass, Factory.Load<DummyBaseBusinessObject>(subclass.PK));
			AssertEquals(subclass, Factory.Load<DummyLaxSubclass1BusinessObject>(subclass.PK));
			AssertNotEquals(subclass, Factory.Load<DummyLaxSubclass2BusinessObject>(subclass.PK));
			AssertNotEquals(Factory.Load<DummyBusinessObject>(subclass.PK), Factory.Load<DummyBaseBusinessObject>(subclass.PK));

			var baseclass = Factory.New<DummyLaxBusinessObject>();
			subclass = Factory.Load<DummyLaxSubclass1BusinessObject>(baseclass.PK);
			AssertEquals("We DID say we're okay with it~", subclass, Factory.Load<DummyLaxBusinessObject>(subclass.PK));
			AssertEquals(subclass, Factory.Load<DummyLaxSubclass1BusinessObject>(subclass.PK));
		}

		[IDontMindLoadingASubclassInstead]
		internal class DummyLaxBusinessObject : DummyBaseBusinessObject
		{
			public DummyLaxBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}
		}

		internal class DummyLaxSubclass1BusinessObject : DummyLaxBusinessObject
		{
			public DummyLaxSubclass1BusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}
		}

		internal class DummyLaxSubclass2BusinessObject : DummyLaxBusinessObject
		{
			public DummyLaxSubclass2BusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}
		}

		public void TestLoad_WhenBizOTypeIsNull()
		{
			AssertNull(Factory.Load((Type)null, ZGuid.BrettsGuid));
			AssertEquals("Factory.Load was called with null bizOType", ErrorReporter.LastMessageReported);
			AssertEquals("Factory.Load.TypeIsNull", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestTrackingAfterOnSavingButBeforeCommit()
		{
			Assert(!Factory.IsAfterOnSavingButBeforeCommit);
			using (Factory.TrackingAfterOnSavingButBeforeCommit())
			{
				Assert(Factory.IsAfterOnSavingButBeforeCommit);
			}
		}

		public void TestReaderFromStreamSource()
		{
			var sampleText = "Charles Mingus";
			var bizo = Factory.New<DummyBusinessObject>();
			using (var stream = new MemoryStream())
			{
				var blob = ZBlob.FromUTF8(sampleText);
				for (int i = 0; i < blob.Length; i++)
				{
					stream.WriteByte(blob[i]);
				}
				bizo.SetZ0_VarBinaryMaxSource(new StreamSource(stream));

				Factory.Save();
			}

			var bizo2 = new BusinessObjectFactory().Load<DummyBusinessObject>(bizo.PK);
			AssertEquals(sampleText, bizo2.Z0_VarBinaryMax.ToUTF8());
		}

		class StreamSource : IStreamSource
		{
			public StreamSource(Stream stream)
			{
				this.stream = stream;
			}
			readonly Stream stream;
			public Stream GetStream() => stream;
		}

		public void TestSaveCountIncrementedForAllFactoriesDuringSaveTogether()
		{
			var saveCount = Factory.SaveCount;

			var factory1 = new BusinessObjectFactory();
			var saveCount1 = factory1.SaveCount;
			Factory.ChildFactories.Add(factory1);

			var factory2 = new BusinessObjectFactory();
			factory1.ChildFactories.Add(factory2);
			var saveCount2 = factory2.SaveCount;

			var factory3 = new BusinessObjectFactory();
			Factory.ChildFactories.Add(factory3);
			var saveCount3 = factory3.SaveCount;

			var factory4 = new BusinessObjectFactory();
			factory2.ChildFactories.Add(factory4);
			factory3.ChildFactories.Add(factory4);
			var saveCount4 = factory4.SaveCount;

			BusinessObjectFactory.SaveTogether(Factory, factory1);
			Assert("Save count incremented by 1", Factory.IsEqualToCurrentSaveCount(++saveCount));
			Assert("Save count incremented by 1 only", factory1.IsEqualToCurrentSaveCount(++saveCount1));
			Assert("Save count incremented by 1 only", factory2.IsEqualToCurrentSaveCount(++saveCount2));
			Assert("Save count incremented by 1", factory3.IsEqualToCurrentSaveCount(++saveCount3));
			Assert("Save count incremented by 1 only", factory4.IsEqualToCurrentSaveCount(++saveCount4));
		}

		public void TestIsSavingTogetherAfterSaveTogether()
		{
			var factory1 = new BusinessObjectFactory();
			var saveCount1 = factory1.SaveCount;
			Factory.ChildFactories.Add(factory1);

			var factory2 = new BusinessObjectFactory();
			factory1.ChildFactories.Add(factory2);
			var saveCount2 = factory2.SaveCount;

			var factory3 = new BusinessObjectFactory();
			Factory.ChildFactories.Add(factory3);
			var saveCount3 = factory3.SaveCount;

			var factory4 = new BusinessObjectFactory();
			factory2.ChildFactories.Add(factory4);
			factory3.ChildFactories.Add(factory4);
			var saveCount4 = factory4.SaveCount;

			BusinessObjectFactory.SaveTogether(Factory, factory1);
			AssertEquals("IsSavingTogether is false after SaveTogether method been completed", expected: false, BusinessObjectFactory.IsSavingTogether);
		}

		public void TestSaveCountIncrementedIrrespectiveOfSaveSuccessOrFailure()
		{
			var saveCount = Factory.SaveCount;
			Factory.Save();
			Assert("Save count", Factory.IsEqualToCurrentSaveCount(++saveCount));

			var factory = new SaveAlwaysFailsBusinessObjectFactory();
			saveCount = factory.SaveCount;
			try
			{
				factory.Save();
			}
			catch (ApplicationException)
			{
				Assert("Save count", factory.IsEqualToCurrentSaveCount(++saveCount));
			}
		}

		class SaveAlwaysFailsBusinessObjectFactory : BusinessObjectFactory
		{
			protected override IChangedTableNames SaveInTransactionCore()
			{
				base.SaveInTransactionCore();
				throw new ApplicationException();
			}
		}

		public void TestCreatingNewBizosWhenCheckingIfBizosCanSave()
		{
			var dummy = Factory.New<DummyBizoThatBreedsDuringSaveCheck>();
			dummy.hasBred = false;
			AssertNoExceptionThrown(() => Factory.Save());
		}

		class DummyBizoThatBreedsDuringSaveCheck : DummyBusinessObject
		{
			public DummyBizoThatBreedsDuringSaveCheck(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool hasBred;

			public override bool HasChanges
			{
				get
				{
					if (!hasBred)
					{
						Factory.New<DummyBusinessObject>();
						hasBred = true;
					}
					return base.HasChanges;
				}

				set => base.HasChanges = value;
			}
		}

		public void TestSaveDuringSave()
		{
			bool looped = false;
			var dummy = Factory.New<DummyBizoWithOnSaveActions>();
			Factory.New<DummyBizoWithOnSaveActions>();
			dummy.OnSavedAction = () =>
			{
				if (!looped)
				{
					looped = true;
					Factory.RowFactory.Save();
				}
			};

			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestLastSavingRollbackHadException()
		{
			Factory = new BusinessObjectFactory();
			var bizo = Factory.New<DummyBizoWithOnSaveActions>();
			bizo.OnSavingAction = () => throw new InvalidOperationException();
			bizo.OnSavedAction = () => throw new NullReferenceException();
			AssertExceptionThrown<InvalidOperationException>(() => Factory.Save());
			Assert("OnSavedAction fails.", ((IBusinessObjectFactoryInternals)Factory).LastSavingRollbackHadException);

			Factory = new BusinessObjectFactory();
			bizo = Factory.New<DummyBizoWithOnSaveActions>();
			bizo.OnSavingAction = () => throw new InvalidOperationException();
			AssertExceptionThrown<InvalidOperationException>(() => Factory.Save());
			Assert("OnSavedAction and OnFactorySavedAction work good.", !((IBusinessObjectFactoryInternals)Factory).LastSavingRollbackHadException);

			Factory = new BusinessObjectFactory();
			bizo = Factory.New<DummyBizoWithOnSaveActions>();
			bizo.OnSavingAction = () => throw new InvalidOperationException();
			bizo.OnFactorySavedAction = () => throw new NullReferenceException();
			AssertExceptionThrown<InvalidOperationException>(() => Factory.Save());
			Assert("OnFactorySavedAction fails.", ((IBusinessObjectFactoryInternals)Factory).LastSavingRollbackHadException);

			Factory = new BusinessObjectFactory();
			bizo = Factory.New<DummyBizoWithOnSaveActions>();
			bizo.OnSavingAction = () => throw new InvalidOperationException();
			bizo.OnSavedAction = () => throw new NullReferenceException();
			bizo.OnFactorySavedAction = () => throw new NullReferenceException();
			AssertExceptionThrown<InvalidOperationException>(() => Factory.Save());
			Assert("OnSavedAction and OnFactorySavedAction fail.", ((IBusinessObjectFactoryInternals)Factory).LastSavingRollbackHadException);

			ErrorReporter.Clear();
		}

		public void TestAggregatingOnSaveException()
		{
			Factory = new BusinessObjectFactory();
			int counter = 0;

			var bizo1 = Factory.New<DummyBizoWithOnSaveActions>();
			bizo1.OnSavedAction = () => { counter++; throw new InvalidOperationException(); };

			var bizo2 = Factory.New<DummyBizoWithOnSaveActions>();
			bizo2.OnSavedAction = () => { counter++; };

			var bizo3 = Factory.New<DummyBizoWithOnSaveActions>();
			bizo3.OnSavingAction = () => throw new InvalidOperationException();
			bizo3.OnSavedAction = () => { counter++; throw new NullReferenceException(); };

			AssertExceptionThrown<InvalidOperationException>(() => Factory.Save());
			Assert("OnSavedAction fails.", ((IBusinessObjectFactoryInternals)Factory).LastSavingRollbackHadException);

			AssertEquals("All bizo should invoke OnSaved", 3, counter);

			ErrorReporter.Clear();
		}

		public void TestGetBusinessObjectBaseTypeFromTablePrefix()
		{
			var tableCodesWithoutBusinessObjects = new HashSet<string>
			{
				"A4",
				"AAR",
				"ABF",
				"ACG",
				"ACL",
				"ACQ",
				"ADB",
				"AGC",
				"ASB",
				"ASC",
				"ATQ",
				"B1",
				"B10",
				"B11",
				"BFE",
				"BKH",
				"BKI",
				"BKM",
				"BKO",
				"BKP",
				"BKS",
				"BKX",
				"BMB",
				"BMC",
				"BNT",
				"BPI",
				"BRH",
				"BRI",
				"CAL",
				"CBK",
				"CCE",
				"CED",
				"CEG",
				"CEH",
				"CES",
				"CFR",
				"CKR",
				"CLP",
				"CML",
				"CNP",
				"COL",
				"COO",
				"CPC",
				"CPE",
				"CPL",
				"CPR",
				"CR1",
				"CR2",
				"CR3",
				"CR4",
				"CR5",
				"CR6",
				"CR7",
				"CR8",
				"CR9",
				"CRA",
				"CRC",
				"CRE",
				"CRS",
				"CRT",
				"CXI",
				"CXP",
				"DA",
				"DR",
				"DV1",
				"E2N",
				"EQI",
				"EQS",
				"EUD",
				"EXA",
				"FCR",
				"FIT",
				"FR",
				"FRO",
				"FX",
				"GBP",
				"GCG",
				"GDG",
				"GGC",
				"GM",
				"GMP",
				"GN",
				"GQ",
				"GSK",
				"GSL",
				"GST",
				"GSV",
				"GSW",
				"GSZ",
				"GV",
				"GWP",
				"GWT",
				"H2",
				"H3",
				"HLP",
				"HR",
				"HRC",
				"HOC",
				"HS",
				"HSG",
				"HT",
				"J4",
				"JBB",
				"JDI",
				"JHC",
				"JIC",
				"JIV",
				"JLG",
				"JPZ",
				"JPD",
				"JPE",
				"JPL",
				"KD",
				"KE",
				"KHR",
				"KPT",
				"LBD",
				"LC",
				"LML",
				"LTE",
				"LTG",
				"LTP",
				"OPL",
				"OPS",
				"OPT",
				"OSP",
				"OTT",
				"P4C",
				"P4L",
				"P9H",
				"P9P",
				"PFL",
				"PP",
				"RCD",
				"RCT",
				"REQ",
				"RIT",
				"RQT",
				"RFL",
				"RFT",
				"RLO",
				"RSE",
				"RSR",
				"RST",
				"RVC",
				"SA",
				"SEP",
				"SFL",
				"SGE",
				"SGP",
				"SLQ",
				"SNO",
				"SNS",
				"SPS",
				"SQS",
				"SS",
				"SV",
				"TAS",
				"TDA",
				"TK",
				"TPD",
				"TPE",
				"TPH",
				"TPI",
				"TTE",
				"TTH",
				"USA",
				"V1",
				"V4",
				"V5",
				"V6",
				"V8",
				"V9",
				"VA",
				"VC",
				"VCM",
				"VCT",
				"VD",
				"VE",
				"VF",
				"VG",
				"VH",
				"VI",
				"VIS",
				"VK",
				"VL",
				"VN",
				"VO",
				"VP",
				"VQ",
				"VT",
				"VTR",
				"VWT",
				"VX",
				"VY",
				"WDR",
				"WL",
				"WLD",
				"WTF",
				"WPC",
				"WTE",
				"WUT",
				"WVC",
				"WVN",
				"YCC",
				"ZZC",
				"ZZM",
				"ZZX",
				"ZZY",
				"GSG",
				"GGI",
				"GGD"
			};

			var realTables = GetTablesInDatabase(); // Lots of non persistent bizo schemas we dont care about
			var schemaAssembly = typeof(IncidentApprovalSchema).Assembly;
			var schemas = schemaAssembly.GetTypes()
				.AsParallel()
				.Where(type => type.IsSubclassOf(typeof(Schema.Schema)))
				.Where(type => !type.Name.StartsWith("Dummy")) // Ignore the dummy tables.
				.Select(type => type.GetNestedType("Constants"))
				.Select(constantType => new { Prefix = (string)constantType.GetField("Prefix").GetValue(null), TableName = (string)constantType.GetField("TableName").GetValue(null) })
				.Where(schema => realTables.Contains(schema.TableName))
				.OrderBy(s => s.TableName)
				.ToArray();

			CombineAssertions(() =>
			{
				foreach (var schema in schemas)
				{
					try
					{
						var bizoType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(schema.Prefix, reportUnknownPrefix: false);
						if (!tableCodesWithoutBusinessObjects.Contains(schema.Prefix))
						{
							Assert($"A mapping does not exist for the table code {schema.Prefix} and its business object (Table {schema.TableName}). This can cause problems for some plugins (eg eDocs), or any time we want to load via table prefix and ZArchitecture needs to resolve a type. Please add a mapping in BusinessObjectPrefixTypesConfiguration.xml under EnterpriseBusinessObjectPrefixTypes element.", bizoType != null);
						}
						else
						{
							Assert($"Looks like you've added a Business Object for {schema.TableName}. Congrats, please remove its code ({schema.Prefix}) from this ignore list.", bizoType == null);
						}
					}
					catch (Exception ex)
					{
						Fail($"An exception was thrown trying to retrive {schema.TableName}. You may be missing a mapping in EnterpriseApplicationConfiguration.xml\r\n" + ex.ToString());
					}
				}
			});
		}

		ISet<string> GetTablesInDatabase()
		{
			var set = new HashSet<string>();
			using (var command = Db.Connection.Command("select name from sys.tables"))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					set.Add(reader.GetString(0));
				}
			}

			return set;
		}

		public void TestLoadUsingSL_PK()
		{
			Factory.AddFetchHint(StmALogSchema.PK, ZGuid.NewZGuid());
			AssertEquals(0, Factory.ActiveFetchHintsForTable(StmALogSchema.Constants.TableName));
			Factory.AddFetchHint(StmALogSchema.SL_Parent, ZGuid.NewZGuid());
			AssertEquals(1, Factory.ActiveFetchHintsForTable(StmALogSchema.Constants.TableName));
		}

		public void TestCreateNewFactory_OnSecondThread()
		{
			DbConnection backgroundConnection = null;
			DbConnection desirableBackgroundConnection = null;
			using (var startingConnection = Db.NewExtraConnectionToMainDb())
			{
				var newFactory = new BusinessObjectFactory(startingConnection);
				AssertEquals(newFactory.RowFactory.DbConnection, startingConnection);

				var backgroundThread = new Thread(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						var backgroundFactory = new BusinessObjectFactory();
						backgroundConnection = backgroundFactory.RowFactory.DbConnection;
						desirableBackgroundConnection = Db.Connection;
					}
				});
				backgroundThread.Start();
				backgroundThread.Join(1000);
				AssertNotNull(desirableBackgroundConnection); //Null if backgroundThread didn't run.

				AssertNotEquals(desirableBackgroundConnection, startingConnection);
				AssertEquals(desirableBackgroundConnection, backgroundConnection);
			}
		}

		public void TestIsAccessibleFromCurrentThreadGuardsFactoryFromCrossThreadAccessIfUsed()
		{
			// Arrange
			var factory = new BusinessObjectFactory();
			var accessed = false;

			var requestingThread = new Thread(() =>
			{
				if (factory.IsOwnedByCurrentThread)
				{
					accessed = true;
				}
			});

			// Act
			requestingThread.Start();
			requestingThread.Join();

			// Assert
			AssertEquals(false, accessed);
		}

		public void TestDeactivateIsCalledOnCollections()
		{
			var index = new ActiveBusinessObjectCollectionIndexDataView<DummyDependantBusinessObject>(Factory,
				typeof(DummyBusinessObjectCollection),
				typeof(DummyBusinessObject),
				new ActiveBusinessObjectCollectionIndexTest.RelationshipWithNullRelationshipFilter(),
				ZQuery.EmptyQuery,
				new CaseInsensitiveComparer(),
				null,
				null);

			var collection = new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(Factory.New(typeof(DummyWithDependentsBusinessObject)));
			collection.AddNew();
			((IBusiness)collection).IncrementReadOnlyIncludingChildren();
			index.AddOwner(collection);

			AssertEquals(false, Factory.IsDeactivated);
			Factory.DeactivateActiveCollectionsAndCaches();

			AssertEquals(true, collection.IsDeactivated);
			AssertEquals(true, Factory.IsDeactivated);
		}

		public void TestGetDirtyObjects_ReturnsObjectsWithModifiedRow_EvenWhenHasChangesIsSuspended()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();
			AssertEquals(0, Factory.GetChanges().GetChangedObjects().Length);
			using (dummy.SuspendSettingHasChanges())
			{
				dummy.Z0_AnotherNumber = 108;
				AssertEquals(1, Factory.GetChanges().GetChangedObjects().Length);
			}
			AssertEquals(1, Factory.GetChanges().GetChangedObjects().Length);
		}

		[ExpectNoExceptions]
		public void TestDelayListChangedDuringTransactionCommitRefreshDisabled()
		{
			var isInSave = false;
			var f1 = new BusinessObjectFactory();
			var f2 = new BusinessObjectFactory();

			var collectionF2 = new ActiveBusinessObjectCollection<DummyBusinessObject>(f2);

			ListChangedEventHandler exceptionThrower = (sender, e) =>
			{
				if (!isInSave)
				{
					throw new Exception("Should not have fired");
				}
			};

			((IBindingList)collectionF2).ListChanged += exceptionThrower;
			try
			{
				f1.RefreshEnabled = false;
				f2.RefreshEnabled = true;

				f1.Saved += (s, e) =>
				{
					isInSave = true;
					try
					{
						var bo = f2.New<DummyBusinessObject>();
					}
					finally
					{
						isInSave = false;
					}
				};
				f1.Save();
			}
			finally
			{
				((IBindingList)collectionF2).ListChanged -= exceptionThrower;
			}
		}

		[ExpectNoExceptions]
		public void TestDelayListChangedDuringTransactionCommitRefreshEnabled()
		{
			var isInSave = false;
			var f1 = new BusinessObjectFactory();
			var f2 = new BusinessObjectFactory();

			var collectionF2 = new ActiveBusinessObjectCollection<DummyBusinessObject>(f2);

			ListChangedEventHandler exceptionThrower = (sender, e) =>
			{
				if (isInSave)
				{
					throw new Exception("Should not have fired");
				}
			};

			((IBindingList)collectionF2).ListChanged += exceptionThrower;
			try
			{
				f1.RefreshEnabled = true;
				f2.RefreshEnabled = true;

				f1.Saved += (s, e) =>
				{
					isInSave = true;
					try
					{
						var bo = f2.New<DummyBusinessObject>();
					}
					finally
					{
						isInSave = false;
					}
				};
				f1.Save();
			}
			finally
			{
				((IBindingList)collectionF2).ListChanged -= exceptionThrower;
			}
		}

		public void TestInStatementForUberFactory()
		{
			var bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_Short = 123;
			Factory.Save();
			using (RowFactory.SetCachedTables(DummyBizoSchema.Constants.TableName, ProcessFieldChangeRuleSchema.Constants.TableName, ProcessFieldChangeRuleFieldSchema.Constants.TableName))
			{
				Assert("Should be cached", RowFactory.IsCachedTable(DummyBizoSchema.Constants.TableName));
				Factory.Load<DummyBusinessObject>(new ZQuery());  // gets the uber factory ready

				var f2 = new BusinessObjectFactory() { RefreshEnabled = false };
				f2.AddFetchHint(DummyBizoSchema.PK, bizO.PK);

				AssertEquals(0, f2.DatabaseLoadCount);
				var bizoIn2 = f2.Load<DummyBusinessObject>(bizO.PK);
				AssertEquals("Should have retrieved from uber factory", 0, f2.DatabaseLoadCount);

				bizO.Z0_Short = 456;
				Factory.Save();

				AssertEquals(123, (int)bizoIn2.Z0_Short);
				bizoIn2.Reload();
				AssertEquals(456, (int)bizoIn2.Z0_Short);
			}
		}

		public void TestBusinessObjectsInformationDoesNotThrowOnCrossThreadAccess()
		{
			// Arrange
			var factory = new BusinessObjectFactory();

			factory.New<DummyBusinessObject>();
			factory.AddFetchHint(typeof(DummyBusinessObject), Guid.NewGuid());
			factory.AddFetchHint(typeof(DummyBusinessObject), Guid.NewGuid());

			// Assert
			AssertNoExceptionThrown("No exception should be thrown on cross thread access", GetBusinessObjectInfo);

			void GetBusinessObjectInfo()
			{
				var requestingThread = new Thread(() =>
				{
					var info = factory.BusinessObjectsInformation;
				});

				// Act
				requestingThread.Start();
				requestingThread.Join();
			}
		}

		public void TestBusinessObjectsInformation()
		{
			try
			{
				RowFactory.LoadedFetchHintRecordingEnabled = true;
				var factory = new BusinessObjectFactory();
				AssertEquals("", factory.BusinessObjectsInformation);
				factory.New<DummyBusinessObject>();
				AssertEquals("CargoWise.EntityFramework.Testing.DummyBusinessObject, 1, Table: DummyBizo (Fetch Hints: 0)", factory.BusinessObjectsInformation);
				factory.New<DummyBusinessObject>();
				AssertEquals("CargoWise.EntityFramework.Testing.DummyBusinessObject, 2, Table: DummyBizo (Fetch Hints: 0)", factory.BusinessObjectsInformation);
				factory.New<DummyPivot>();
				AssertEquals("CargoWise.EntityFramework.Testing.DummyBusinessObject, 2, Table: DummyBizo (Fetch Hints: 0)\r\n" +
					"CargoWise.EntityFramework.Testing.DummyPivot, 1, Table: DummyPivot (Fetch Hints: 0)", factory.BusinessObjectsInformation);
			}
			finally
			{
				RowFactory.LoadedFetchHintRecordingEnabled = false;
			}
		}

		public void TestBusinessObjectsInformationDoesNotShowWhenRecordingDisabled()
		{
			RowFactory.LoadedFetchHintRecordingEnabled = false;
			var factory = new BusinessObjectFactory();
			AssertEquals("", factory.BusinessObjectsInformation);
			factory.New<DummyBusinessObject>();
			AssertEquals("CargoWise.EntityFramework.Testing.DummyBusinessObject, 1, Table: DummyBizo (Fetch Hints: -1)", factory.BusinessObjectsInformation);
			factory.New<DummyBusinessObject>();
			AssertEquals("CargoWise.EntityFramework.Testing.DummyBusinessObject, 2, Table: DummyBizo (Fetch Hints: -1)", factory.BusinessObjectsInformation);
			factory.New<DummyPivot>();
			AssertEquals("CargoWise.EntityFramework.Testing.DummyBusinessObject, 2, Table: DummyBizo (Fetch Hints: -1)\r\n" +
				"CargoWise.EntityFramework.Testing.DummyPivot, 1, Table: DummyPivot (Fetch Hints: -1)", factory.BusinessObjectsInformation);
		}

		public void TestBusinessObjectsInformationWithNonPersistentBusinessObject()
		{
			try
			{
				RowFactory.LoadedFetchHintRecordingEnabled = true;
				var factory = new BusinessObjectFactory();
				AssertEquals("", factory.BusinessObjectsInformation);

				new DummyNonPersistentBusinessObject(factory);
				AssertEquals("CargoWise.EntityFramework.Testing.BusinessObjectFactoryTest+DummyNonPersistentBusinessObject, 1, No table", factory.BusinessObjectsInformation);

				new DummyNonPersistentBusinessObject(factory);
				factory.New<DummyPivot>();
				AssertEquals("CargoWise.EntityFramework.Testing.BusinessObjectFactoryTest+DummyNonPersistentBusinessObject, 2, No table\r\n" +
					"CargoWise.EntityFramework.Testing.DummyPivot, 1, Table: DummyPivot (Fetch Hints: 0)", factory.BusinessObjectsInformation);

				new DummyNonPersistentBusinessObject(factory);
				factory.New<DummyPivot>();
				factory.New<DummyPivot>();
				factory.New<DummyPivot>();

				AssertEquals("CargoWise.EntityFramework.Testing.DummyPivot, 4, Table: DummyPivot (Fetch Hints: 0)\r\n" +
					"CargoWise.EntityFramework.Testing.BusinessObjectFactoryTest+DummyNonPersistentBusinessObject, 3, No table", factory.BusinessObjectsInformation);
			}
			finally
			{
				RowFactory.LoadedFetchHintRecordingEnabled = false;
			}
		}

		public void TestBusinessObjectsInformationWithFetchHints()
		{
			try
			{
				RowFactory.LoadedFetchHintRecordingEnabled = true;
				var factory = new BusinessObjectFactory();
				AssertEquals("", factory.BusinessObjectsInformation);

				factory.New<DummyBusinessObject>();
				factory.AddFetchHint(typeof(DummyBusinessObject), Guid.NewGuid());
				factory.AddFetchHint(typeof(DummyBusinessObject), Guid.NewGuid());
				factory.Load<DummyBusinessObject>(Guid.NewGuid());
				AssertEquals("CargoWise.EntityFramework.Testing.DummyBusinessObject, 1, Table: DummyBizo (Fetch Hints: 2)", factory.BusinessObjectsInformation);

				factory.New<DummyBusinessObject>();
				factory.AddFetchHint(typeof(DummyBusinessObject), Guid.NewGuid());
				factory.Load<DummyBusinessObject>(Guid.NewGuid());
				AssertEquals("CargoWise.EntityFramework.Testing.DummyBusinessObject, 2, Table: DummyBizo (Fetch Hints: 3)", factory.BusinessObjectsInformation);

				factory.New<DummyPivot>();
				factory.AddFetchHint(typeof(DummyPivot), Guid.NewGuid());
				factory.AddFetchHint(typeof(DummyPivot), Guid.NewGuid());
				factory.Load<DummyPivot>(Guid.NewGuid());
				AssertEquals("CargoWise.EntityFramework.Testing.DummyBusinessObject, 2, Table: DummyBizo (Fetch Hints: 3)\r\n" +
					"CargoWise.EntityFramework.Testing.DummyPivot, 1, Table: DummyPivot (Fetch Hints: 2)", factory.BusinessObjectsInformation);
			}
			finally
			{
				RowFactory.LoadedFetchHintRecordingEnabled = false;
			}
		}

		public void TestBusinessObjectsInformationWithNonPersistentBusinessObjectWithFetchHints()
		{
			try
			{
				RowFactory.LoadedFetchHintRecordingEnabled = true;
				var factory = new BusinessObjectFactory();
				AssertEquals("", factory.BusinessObjectsInformation);

				factory.New<DummyBusinessObject>();
				AssertEquals("CargoWise.EntityFramework.Testing.DummyBusinessObject, 1, Table: DummyBizo (Fetch Hints: 0)", factory.BusinessObjectsInformation);

				factory.AddFetchHint(typeof(DummyBusinessObject), Guid.NewGuid());
				factory.AddFetchHint(typeof(DummyPivot), Guid.NewGuid());
				factory.New<DummyBusinessObject>();
				AssertEquals("CargoWise.EntityFramework.Testing.DummyBusinessObject, 2, Table: DummyBizo (Fetch Hints: 0)", factory.BusinessObjectsInformation);

				factory.New<DummyPivot>();
				new DummyNonPersistentBusinessObject(factory);
				new DummyNonPersistentBusinessObject(factory);
				new DummyNonPersistentBusinessObject(factory);
				factory.New<DummyBusinessObject>();
				factory.New<DummyBusinessObject>();
				factory.AddFetchHint(typeof(DummyPivot), Guid.NewGuid());
				factory.Load<DummyBusinessObject>(Guid.NewGuid());
				factory.Load<DummyPivot>(Guid.NewGuid());
				AssertEquals("CargoWise.EntityFramework.Testing.DummyBusinessObject, 4, Table: DummyBizo (Fetch Hints: 1)\r\n" +
					"CargoWise.EntityFramework.Testing.BusinessObjectFactoryTest+DummyNonPersistentBusinessObject, 3, No table\r\n" +
					"CargoWise.EntityFramework.Testing.DummyPivot, 1, Table: DummyPivot (Fetch Hints: 2)", factory.BusinessObjectsInformation);
			}
			finally
			{
				RowFactory.LoadedFetchHintRecordingEnabled = false;
			}
		}

		public void TestEventTrackerWithEmptyDates()
		{
			var factory = new BusinessObjectFactory();
			var query = new ZQuery(DummyBizoSchema.Z0_AnotherDate, ZDateTime.Empty);
			factory.Load<DummyBusinessObject>(query);
			Assert("Parameter value", SqlEventTracker.Instance.LastSqlEvent.Contains("Z0_AnotherDate is NULL"));

			AssertExceptionThrown(typeof(ArgumentException), "See WI00114690 for details.", () => query = new ZQuery(DummyBizoSchema.Z0_AnotherDate, ZDateTime.Invalid), true);

			factory.Load<DummyBusinessObject>(query);
			Assert("Parameter value", SqlEventTracker.Instance.LastSqlEvent.Contains("Z0_AnotherDate is NULL"));
			ErrorReporter.Clear();
		}

		public void TestEventTrackerWithZSqlParameterCollectionAndEmptyDates()
		{
			var factory = new BusinessObjectFactory();
			var query = new ZQuery();
			var collection = new ZSqlParameterCollection();
			collection.Add("@P", ZDateTime.Empty, DummyBizoSchema.Z0_AnotherDate);
			query.AddFilterAndZSQLParameterCollection("@P is NULL", collection);
			factory.Load<DummyBusinessObject>(query);
			Assert("Parameter value", SqlEventTracker.Instance.LastSqlEvent.Contains("@P = \"NULL\""));

			query = new ZQuery();
			collection = new ZSqlParameterCollection();
			query.AddFilterAndZSQLParameterCollection("@P is NULL", collection);

			AssertExceptionThrown(typeof(ArgumentException), "See WI00114690 for details.", () => collection.Add("@P", ZDateTime.Invalid, DummyBizoSchema.Z0_AnotherDate), true);
			ErrorReporter.Clear();
		}

		[ExpectNoExceptions]
		public void TestListenerCallback()
		{
			var factory = new BusinessObjectFactory();

			object arr = new ITransactionParticipant[] { factory };
			var mock = new Mock<ITransactionParticipantListener>();
			try
			{
				mock.Setup(m => m.FactorySaveBeginning((ITransactionParticipant[])arr));
				mock.Setup(m => m.FactorySaveCompleted((ITransactionParticipant[])arr, true));
				BusinessObjectFactory.RegisterListener(mock.Object);
				factory.Save();
				mock.Verify();
			}
			finally
			{
				BusinessObjectFactory.UnRegisterListener(mock.Object);
			}
		}

		public void TestFailureWithRollbackIsNotHiddenByExceptionHandling()
		{
			var factory = new BusinessObjectFactory();

			var triggerSql = @"
								CREATE TRIGGER TG_DummyBizo ON DummyBizo FOR INSERT AS
								BEGIN
										RAISERROR('[DummyBusinessObjectErrorWithRollback]', 16, 1);
										ROLLBACK TRANSACTION;
								END;";
			((IDbConnected)factory).Connection.ExecuteNonQuery(triggerSql);

			var bizo = factory.New<DummyBusinessObject>();

			try
			{
				factory.Save();
				Fail("Should thrown an exception");
			}
			catch (Exception ex)
			{
				Assert("Exception caught does not contain original error.\r\n" + ex.Message, ex.Message.Contains("[DummyBusinessObjectErrorWithRollback]"));
				AssertEquals("Is ZSaveException?\r\n" + ex.Message, true, ex is ZSaveException);
				var saveEx = (ZSaveException)ex;
				AssertEquals("Failed Row Table", DummyBizoSchema.Constants.TableName, saveEx.Row.Table.TableName);
				AssertEquals("Failed Row PK", bizo.PK, saveEx.Row[0]);
			}
		}

		public void TestChildParticipantsActions()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			AssertEquals(0, (factory as ITransactionParticipant).ChildParticipants.Length);

			factory.ChildFactories.Add(new BusinessObjectFactory());
			AssertEquals(1, (factory as ITransactionParticipant).ChildParticipants.Length);

			factory.SaveInTransactionActions.Add(new DummyAction());
			AssertEquals(2, (factory as ITransactionParticipant).ChildParticipants.Length);
		}

		class DummyAction : SaveInTransactionActionWithMainConnection
		{
			protected override IChangedTableNames SaveInTransaction()
			{
				throw new NotImplementedException();
			}
		}

		public void TestTransactionID()
		{
			AssertEquals(0, this.Factory.TransactionId);
			Factory.Saving += (a) => Assert(this.Factory.TransactionId != 0);
			Factory.Save();
			AssertEquals(0, this.Factory.TransactionId);
		}

		/// <summary>
		/// This test calls Factory.Save several times in a row to assert that TransactionId always goes up in value.
		/// </summary>
		public void TestTransactionIdIsAlwaysIncremented()
		{
			long previousTransactionId = 0;

			Factory.Saving += (f) =>
			{
				Assert(
					$"Transaction ID [{f.TransactionId}] not incremented from previous value [{previousTransactionId}].",
					f.TransactionId > previousTransactionId
				);
				previousTransactionId = f.TransactionId;
			};

			for (int i = 0; i < 10; i++)
			{
				Factory.Save();
			}

			AssertEquals(0, this.Factory.TransactionId);
		}

		public void TestFetchHintIsFiredPreConstruction()
		{
			var dbo = Factory.New<DummyBusinessObject>();
			Factory.Save();

			AssertExceptionThrown("Dummy got hit", typeof(Exception), delegate
			{ new BusinessObjectFactory().Load<DummyWithFetchStrategy>(new ZQuery()); });
		}

		class DummyFetch : IRowFetchStrategy
		{
			#region IRowFetchStrategy Members

			public void FetchForLoad(BusinessObjectFactory factory, DataRow[] rows)
			{
				throw new Exception("Dummy got hit");
			}

			#endregion
		}

		[RowFetchStrategy(FetchStrategyType = typeof(DummyFetch))]
		class DummyWithFetchStrategy : DummyBusinessObject
		{
			public DummyWithFetchStrategy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		public void TestEachThreadGetsADifferentConnection()
		{
			object instance1 = null, instance2 = null, instance3 = null;

			instance1 = ((IDbConnected)new BusinessObjectFactory()).Connection;

			AssertEquals(instance1, Db.Connection);

			ThreadStart threadstart2 = new ThreadStart(delegate
			{
				using (Db.DisposableActionForDbConnection())
				{
					instance2 = ((IDbConnected)new BusinessObjectFactory()).Connection;
					AssertEquals(instance2, Db.Connection);
				}
			});

			ThreadStart threadstart3 = new ThreadStart(delegate
			{
				using (Db.DisposableActionForDbConnection())
				{
					instance3 = ((IDbConnected)new BusinessObjectFactory()).Connection;
					AssertEquals(instance3, Db.Connection);
				}
			});

			Thread thread2 = new Thread(threadstart2);
			Thread thread3 = new Thread(threadstart3);

			thread2.Start();
			thread3.Start();

			thread2.Join(1000);
			thread3.Join(1000);

			AssertNotNull(instance1);
			AssertNotNull(instance2);
			AssertNotNull(instance3);
			AssertNotEquals("Each thread should get it's own instance", instance1, instance2);
			AssertNotEquals("Each thread should get it's own instance", instance2, instance3);
			AssertNotEquals("Each thread should get it's own instance", instance1, instance3);
		}

		public void TestEachThreadGetsTheSameConnectionInstanceIfAssigned()
		{
			TestEntityFrameworkSettings.Get().ReportCrossThreadFactoryAccess = false;

			object instance1 = null, instance2 = null, instance3 = null;

			using (DbConnection connection = Db.NewExtraConnectionToMainDb())
			{
				BusinessObjectFactory factory = new BusinessObjectFactory(connection);
				instance1 = ((IDbConnected)factory).Connection;

				ThreadStart threadstart2 = new ThreadStart(delegate
				{ instance2 = ((IDbConnected)factory).Connection; });
				ThreadStart threadstart3 = new ThreadStart(delegate
				{ instance3 = ((IDbConnected)factory).Connection; });

				Thread thread2 = new Thread(threadstart2);
				Thread thread3 = new Thread(threadstart3);

				thread2.Start();
				thread3.Start();

				thread2.Join(1000);
				thread3.Join(1000);

				AssertEquals("Each thread should get the same instance", connection, instance1);
				AssertEquals("Each thread should get the same instance", connection, instance2);
				AssertEquals("Each thread should get the same instance", connection, instance3);
			}
		}

		public void TestSaveAndReloadOfLargeBlob()
		{
			int oneMbBlobSize = 1048576;
			AssertEquals("Test BLOB size 4+ times greater than MaxChunkSize", true, oneMbBlobSize >= ZLargeColumnSaver.MaxChunkSize * 4);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummy = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));

			byte[] blobValue = new byte[oneMbBlobSize];
			new Random().NextBytes(blobValue);

			dummy.Z0_VarBinaryMax = blobValue;

			// potentially +1 DB hit to check the Registry
			var useParameters = ObjectFactory.Get<IEntityFrameworkSettings>().ParameterizeInsertAndUpdateStatements;

			int previousExecutedCommandCount = ((IDbConnected)factory).Connection.ExecutedCommandCount;
			factory.Save();
			int numberOfCommandsExecuted = ((IDbConnected)factory).Connection.ExecutedCommandCount - previousExecutedCommandCount;

			BusinessObjectFactory loadFactory = new BusinessObjectFactory();
			DummyBusinessObject loadedDummy = (DummyBusinessObject)loadFactory.Load(typeof(DummyBusinessObject), dummy.PK);

			AssertEquals("Loaded BLOB size", oneMbBlobSize, loadedDummy.Z0_VarBinaryMax.Length);
			AssertEquals("Loaded BLOB contents", dummy.Z0_VarBinaryMax.ToAscii(), loadedDummy.Z0_VarBinaryMax.ToAscii());
			AssertEquals("Number of commands to save Dummy (will be affected by chunk size", 17, numberOfCommandsExecuted);
		}

		public void TestSaveAndReloadOfLargeText()
		{
			int oneMbTextSize = 1048576;
			AssertEquals("Test TEXT size 4+ times greater than MaxChunkSize", true, oneMbTextSize >= ZLargeColumnSaver.MaxChunkSize * 4);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummy = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));

			dummy.Z0_VarCharMax = ZString.Replicate('A', oneMbTextSize);

			int previousExecutedCommandCount = ((IDbConnected)factory).Connection.ExecutedCommandCount;
			factory.Save();
			int numberOfCommandsExecuted = ((IDbConnected)factory).Connection.ExecutedCommandCount - previousExecutedCommandCount;

			Assert("Number of commands to save Dummy <= 5 (was actually:" + numberOfCommandsExecuted.ToString() + ")", numberOfCommandsExecuted <= 5);

			BusinessObjectFactory loadFactory = new BusinessObjectFactory();
			DummyBusinessObject loadedDummy = (DummyBusinessObject)loadFactory.Load(typeof(DummyBusinessObject), dummy.PK);

			AssertEquals("Loaded TEXT size", oneMbTextSize, loadedDummy.Z0_VarCharMax.Length);
			AssertEquals("Loaded TEXT contents", dummy.Z0_VarCharMax, loadedDummy.Z0_VarCharMax);
		}

		public void TestSaveAndReloadOfLargeNText()
		{
			int oneMbNTextSize = 1048576;
			AssertEquals("Test TEXT size 4+ times greater than MaxChunkSize", true, oneMbNTextSize >= ZLargeColumnSaver.MaxChunkSize * 4);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummy = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));

			dummy.Z0_NVarCharMax = ZString.Replicate('\u306E', oneMbNTextSize);

			int previousExecutedCommandCount = ((IDbConnected)factory).Connection.ExecutedCommandCount;
			factory.Save();
			int numberOfCommandsExecuted = ((IDbConnected)factory).Connection.ExecutedCommandCount - previousExecutedCommandCount;

			Assert("Number of commands to save Dummy <= 5 (was actually:" + numberOfCommandsExecuted.ToString() + ")", numberOfCommandsExecuted <= 5);

			BusinessObjectFactory loadFactory = new BusinessObjectFactory();
			DummyBusinessObject loadedDummy = (DummyBusinessObject)loadFactory.Load(typeof(DummyBusinessObject), dummy.PK);

			AssertEquals("Loaded TEXT size", oneMbNTextSize, loadedDummy.Z0_NVarCharMax.Length);
			AssertEquals("Loaded TEXT contents", dummy.Z0_NVarCharMax, loadedDummy.Z0_NVarCharMax);
		}

		public void TestCanSearchOnTextField()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			DummyBusinessObject dummy = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
			dummy.Z0_NVarCharMax = "ntext";

			factory.Save();

			var query = new ZQuery(DummyBizoSchema.Z0_NVarCharMax, "nop");
			Assert(factory.LoadTop1<DummyBusinessObject>(query) == null);

			query = new ZQuery(DummyBizoSchema.Z0_NVarCharMax, "ntext");
			Assert(factory.LoadTop1<DummyBusinessObject>(query) == dummy);
		}

		public void TestFactorySaveASecondTimeStillHasConstraintFailure()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummy = DummyBusinessObject.New(factory);
			DummyDependantBusinessObject dummyDependant = DummyDependantBusinessObject.New(factory);
			dummyDependant.ZD1_Z0 = dummy.PK;
			factory.Save();

			dummy.Delete();

			try
			{
				factory.Save();
				Assert("Should have delete constraint fail first time", false);
			}
			catch (ZSaveException e)
			{
				Assert("Should have delete constraint fail first time", e.Message.IndexOf("DELETE statement conflicted") != -1);
			}

			try
			{
				factory.Save();
				Assert("Should have delete constraint fail second time", false);
			}
			catch (ZSaveException e)
			{
				Assert("Should have delete constraint faill second time", e.Message.IndexOf("DELETE statement conflicted") != -1);
			}
		}

		[UseSnapshotProtection]
		public void TestSaveInPreexistingTransaction()
		{
			using (DbConnection extraConnection = Db.NewExtraConnectionToMainDb())
			{
				extraConnection.BeginTransaction();

				BusinessObjectFactory factory = new BusinessObjectFactory(extraConnection);
				DummyBusinessObject dummy = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
				factory.Save();

				extraConnection.CommitTransaction();

				BusinessObjectFactory newFactory = new BusinessObjectFactory(extraConnection);
				DummyBusinessObject loadedDummy = (DummyBusinessObject)newFactory.Load(typeof(DummyBusinessObject), dummy.PK);
				AssertNotNull("Should be able to load the dummy now", loadedDummy);
			}
		}

		public void TestSaveInPreexistingTransaction_Rollback()
		{
			using (DbConnection extraConnection = Db.NewExtraConnectionToMainDb())
			{
				extraConnection.BeginTransaction();

				BusinessObjectFactory factory = new BusinessObjectFactory(extraConnection);
				DummyBusinessObject dummy = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
				factory.Save();

				extraConnection.RollbackTransaction();

				BusinessObjectFactory newFactory = new BusinessObjectFactory(extraConnection);
				DummyBusinessObject loadedDummy = (DummyBusinessObject)newFactory.Load(typeof(DummyBusinessObject), dummy.PK);
				AssertNull("Dummy should not have saved if transaction was rolled back", loadedDummy);
			}
		}

		#region TestCallingOfOnFactorySavingBeforeTransaction

		public void TestCallingOfOnFactorySavingBeforeTransaction()
		{
			BusinessObjectTest.DummyForOnFactorySavingBeforeTransaction dummy1 = BusinessObjectTest.DummyForOnFactorySavingBeforeTransaction.New(Factory);
			BusinessObjectTest.DummyForOnFactorySavingBeforeTransaction dummy2 = BusinessObjectTest.DummyForOnFactorySavingBeforeTransaction.New(Factory);
			AssertEquals("Dummy1.OnFactorySavingBeforeTransactionCalledCount", 0, dummy1.OnFactorySavingBeforeTransactionCoreCallCount);
			AssertEquals("Dummy2.OnFactorySavingBeforeTransactionCalledCount", 0, dummy2.OnFactorySavingBeforeTransactionCoreCallCount);

			Factory.Save();
			AssertEquals("Dummy1.OnFactorySavingBeforeTransactionCalledCount", 1, dummy1.OnFactorySavingBeforeTransactionCoreCallCount);
			AssertEquals("Dummy2.OnFactorySavingBeforeTransactionCalledCount", 1, dummy2.OnFactorySavingBeforeTransactionCoreCallCount);

			BusinessObjectTest.DummyForOnFactorySavingBeforeTransaction dummy3 = BusinessObjectTest.DummyForOnFactorySavingBeforeTransaction.New(Factory);
			Factory.Save();
			AssertEquals("Dummy1.OnFactorySavingBeforeTransactionCalledCount", 2, dummy1.OnFactorySavingBeforeTransactionCoreCallCount);
			AssertEquals("Dummy2.OnFactorySavingBeforeTransactionCalledCount", 2, dummy2.OnFactorySavingBeforeTransactionCoreCallCount);
			AssertEquals("Dummy3.OnFactorySavingBeforeTransactionCalledCount", 1, dummy3.OnFactorySavingBeforeTransactionCoreCallCount);
		}

		#endregion

		#region TestOnSavingGetsCalledWhenNewBizOIsAddedAndAnotherIsDeletedDuringOnSaving

		class DummyWhichDeletesAndNewsOnSaving : DummyBusinessObject
		{
			public DummyWhichDeletesAndNewsOnSaving(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override void OnSaving()
			{
				NewDummy = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
				OtherDummyToDelete.Delete();
				base.OnSaving();
			}

			public DummyBusinessObject OtherDummyToDelete;
			public DummyBusinessObject NewDummy;
		}

		public void TestOnSavingGetsCalledWhenNewBizOIsAddedAndAnotherIsDeletedDuringOnSaving()
		{
			DummyWhichDeletesAndNewsOnSaving dummy = (DummyWhichDeletesAndNewsOnSaving)Factory.New(typeof(DummyWhichDeletesAndNewsOnSaving));
			dummy.OtherDummyToDelete = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			Factory.Save();
			Assert("New object gets OnSaving", dummy.NewDummy.OnSavingCalled);
			Assert("Main dummy gets OnSaving", dummy.OnSavingCalled);
		}

		#endregion

		#region TestOnSavingIsCalledWhenObjectIsCreatedOnSavingFactory

		public void TestOnSavingIsCalledWhenObjectIsCreatedOnSavingFactory()
		{
			DummyFactoryForDummyCreatedInTran factory = new DummyFactoryForDummyCreatedInTran();
			factory.Saving += new BusinessObjectFactory.SavingEventHandler(factory_Saving);
			factory.Save();
			DummyCreatedInTran obj = factory.Load<DummyCreatedInTran>(factory.ObjPK);
			AssertNotNull("Object should be created in factory", obj);
			Assert("OnSaving should be called - it sets Flag to true", obj.Flag);
		}

		void factory_Saving(BusinessObjectFactory factory)
		{
			DummyCreatedInTran obj = factory.New<DummyCreatedInTran>();
			(factory as DummyFactoryForDummyCreatedInTran).ObjPK = obj.PK;
		}

		class DummyCreatedInTran : DummyBusinessObject
		{
			public DummyCreatedInTran(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool Flag
			{
				get;
				set;
			}

			public override void OnSaving()
			{
				base.OnSaving();
				Flag = true;
			}
		}

		class DummyFactoryForDummyCreatedInTran : BusinessObjectFactory
		{
			public DummyFactoryForDummyCreatedInTran()
			{
			}

			public DummyFactoryForDummyCreatedInTran(DbConnection connection)
				: base(connection)
			{
			}

			protected override bool NotifyOfSaveBeforeAnyFactorySaveBegins()
			{
				return true;
			}

			ZGuid objPK = ZGuid.Empty;
			public ZGuid ObjPK
			{
				get
				{
					return objPK;
				}
				set
				{
					objPK = value;
				}
			}
		}

		#endregion

		#region Index Performance

		public void TestIndexEnabled()
		{
			Factory.IndexingEnabled = true;
			AssertEquals(true, Factory.RowFactory.IndexingEnabled);

			Factory.IndexingEnabled = false;
			AssertEquals(false, Factory.RowFactory.IndexingEnabled);
		}

		public void TestFetchOnlyFromLocalCacheWhenDBOnlyQuery()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "DESC";
			Factory.Save();

			ZQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.AddToFilter(DummyBizoSchema.Z0_Description, "DESC");
			query.FetchOnlyFromLocalCache = true;

			DummyBusinessObject[] objectsInFirstFactory = Factory.Load<DummyBusinessObject>(query);
			AssertEquals(1, objectsInFirstFactory.Length);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject[] objectsInSecondFactory = factory2.Load<DummyBusinessObject>(query);
			AssertEquals(1, objectsInSecondFactory.Length);
		}

		#endregion

		#region Creation Performance

		public void TestCreationPerformance()
		{
			int iterations = 3;
			double maximumAcceptableAverageSeconds = 0.78d;
			double totalSecondsToMakeObjects = 0;
			for (int iterationLoop = 0; iterationLoop <= iterations; iterationLoop++)
			{
				BusinessObjectFactory factoryForTest = new BusinessObjectFactory();
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				var timer = new Stopwatch();
				timer.Start();
				for (int i = 0; i < 1000; i++)
				{
					DummyBusinessObject.New(factoryForTest);
				}
				if (iterationLoop > 0)  // Discard first run
				{
					totalSecondsToMakeObjects += timer.Elapsed.TotalSeconds;
				}
			}

			double averageSeconds = totalSecondsToMakeObjects / iterations;
			if (averageSeconds < maximumAcceptableAverageSeconds)
			{
				Assert(true);
			}
			else
			{
				Fail("Maximum acceptable time to create 1000 DummyBusinessObjects : " + maximumAcceptableAverageSeconds.ToString() + System.Environment.NewLine +
								"Actual time : " + averageSeconds.ToString());
			}
		}

		#endregion

		#region MRU Cache Load Performance

		public void TestLoadMRUCachePerformance()
		{
			int iterations = 1;
			int loadCount = 2000;
			double maximumAcceptableTimeInSeconds = 0.2;

			ZGuid[] bizoPKs = new ZGuid[1000];
			ZString[] bizoNKs = new ZString[1000];

			BusinessObjectFactory factoryForTest = new BusinessObjectFactory();
			for (int i = 0; i < bizoPKs.Length; i++)
			{
				DummyBusinessObject dummyBizO = factoryForTest.NewWithValidTestData<DummyBusinessObject>();
				dummyBizO.Z0_Description = (dummyBizO.Z0_Description + i.ToString()).PadLeft(DummyBizoSchema.Z0_Description.MaxLength);
				bizoPKs[i] = dummyBizO.PK;
				bizoNKs[i] = dummyBizO.Z0_Description;
			}
			factoryForTest.Save();

			CreateAndLoadObjectsByPKUsingFactoryAndMeasureTime(factoryForTest, bizoPKs, loadCount);   // Ensure code is jit'd
			CreateAndLoadObjectsByNaturalKeyUsingFactoryAndMeasureTime(factoryForTest, bizoNKs, loadCount);   // Ensure code is jit'd

			TimeSpan timeTakenByPK = CreateAndLoadObjectsByPKUsingFactoryAndMeasureTime(factoryForTest, bizoPKs, loadCount);
			TimeSpan timeTakenByNK = CreateAndLoadObjectsByNaturalKeyUsingFactoryAndMeasureTime(factoryForTest, bizoNKs, loadCount);

			double averageSecondsByPK = timeTakenByPK.TotalSeconds / iterations;
			double averageSecondsByNK = timeTakenByNK.TotalSeconds / iterations;

			Assert("Maximum acceptable time to load " + loadCount + " random DummyBusinessObjects (including 5 x reload) by PK: " + maximumAcceptableTimeInSeconds.ToString() + " seconds\r\n" +
					"Actual time : " + averageSecondsByPK.ToString(), averageSecondsByPK < maximumAcceptableTimeInSeconds);

			Assert("Maximum acceptable time to load " + loadCount + " random DummyBusinessObjects (including 5 x reload) by Natural Key: " + maximumAcceptableTimeInSeconds.ToString() + " seconds\r\n" +
					"Actual time : " + averageSecondsByNK.ToString(), averageSecondsByNK < maximumAcceptableTimeInSeconds);
		}

		public void TestLoadCacheDoesNotReturnDeletedObjects()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			DummyBusinessObject loadedDummy = Factory.Load<DummyBusinessObject>(dummy.PK);
			loadedDummy.Delete();
			DummyBusinessObject shouldntLoadDeletedDummy = Factory.Load<DummyBusinessObject>(dummy.PK);
			AssertEquals("Deleted business objects should not be loaded from cache", null, shouldntLoadDeletedDummy);
		}

		public void TestLoadNKCacheDoesNotReturnDeletedObjects()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "BAABAA";
			DummyBusinessObject loadedDummy = Factory.LoadFromNaturalKey<DummyBusinessObject>(DummyBizoSchema.Z0_Description, dummy.Z0_Description);
			loadedDummy.Delete();
			DummyBusinessObject shouldntLoadDeletedDummy = Factory.LoadFromNaturalKey<DummyBusinessObject>(DummyBizoSchema.Z0_Description, "BAABAA");
			AssertEquals("Deleted business objects should not be loaded from cache", null, shouldntLoadDeletedDummy);
		}

		#region Implementation

		TimeSpan CreateAndLoadObjectsByPKUsingFactoryAndMeasureTime(BusinessObjectFactory factoryForTest, ZGuid[] bizOGuids, int loadCount)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			Stopwatch timer = Stopwatch.StartNew();

			Random random = new Random();
			int bizOSelector;

			for (int i = 0; i < loadCount; i++)
			{
				bizOSelector = random.Next(0, bizOGuids.Length);
				for (int j = 0; j < 5; j++)
				{
					factoryForTest.Load<DummyBusinessObject>(bizOGuids[bizOSelector]);
				}
			}
			timer.Stop();

			return timer.Elapsed;
		}

		TimeSpan CreateAndLoadObjectsByNaturalKeyUsingFactoryAndMeasureTime(BusinessObjectFactory factoryForTest, ZString[] bizOKeys, int loadCount)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			Stopwatch timer = Stopwatch.StartNew();

			Random random = new Random();
			int bizOSelector;

			for (int i = 0; i < loadCount; i++)
			{
				bizOSelector = random.Next(0, bizOKeys.Length);
				for (int j = 0; j < 5; j++)
				{
					factoryForTest.LoadFromNaturalKey<DummyBusinessObject>(DummyBizoSchema.Z0_Description, bizOKeys[bizOSelector]);
				}
			}
			timer.Stop();

			return timer.Elapsed;
		}

		#endregion

		#endregion

		#region Testing big text inserts / updates

		const string QuoteyText = "110,000 *\" P'IA\"+5+1+6'02.01.''25+01.00+ +54\"\r\n";

		[ExpectNoExceptions()]
		public void TestInsertWithManyQuotesDoesNotHangServer()
		{
			SaveBigText(QuoteyText, false, 200000);
		}

		[ExpectNoExceptions()]
		public void TestUpdateWithManyQuotesDoesNotHangServer()
		{
			SaveBigText(QuoteyText, true, 200000);
		}

		public void TestInsertWithManyQuotesSavesAndReloadsCorrectly()
		{
			SaveBigTextAndCheckOK(QuoteyText, false, 20000);
		}

		public void TestUpdateWithManyQuotesSavesAndReloadsCorrectly()
		{
			SaveBigTextAndCheckOK(QuoteyText, true, 20000);
		}

		void SaveBigTextAndCheckOK(ZString textToBeRepeated, bool isUpdate, int numberOfRepeatsForBigText)
		{
			DummyBusinessObject dummy = SaveBigText(textToBeRepeated, isUpdate, numberOfRepeatsForBigText);

			BusinessObjectFactory reloadFactory = new BusinessObjectFactory();
			DummyBusinessObject dummyReloaded = reloadFactory.Load<DummyBusinessObject>(dummy.PK);

			string original = dummy.Z0_VarCharMax;
			string reloaded = dummyReloaded.Z0_VarCharMax;

			string info = "";
			if (original != reloaded)
			{
				info += InfoAboutFirstDifference(original, reloaded);
				info += "\r\nOriginal: " + GetInfoAboutString(original);
				info += "\r\nReloaded: " + GetInfoAboutString(reloaded);
			}

			AssertMultilineASCIIEquals("Saved and loaded correctly - reloaded text should be equal to saved text\r\n" + info, original, reloaded);
		}

		DummyBusinessObject SaveBigText(ZString textToBeRepeated, bool isUpdate, int numberOfRepeatsForBigText)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummy = DummyBusinessObject.New(factory);
			if (isUpdate)
			{
				factory.Save();
			}

			ZStringBuilder builder = new ZStringBuilder();
			for (int i = 0; i < numberOfRepeatsForBigText; i++)
			{
				builder.Append(textToBeRepeated);
			}
			dummy.Z0_VarCharMax = builder.ToString();
			factory.Save();

			return dummy;
		}

		string InfoAboutFirstDifference(string value1, string value2)
		{
			string result = "\r\nChars are the same in both strings (checking up to the end of the first string)";

			for (int i = 0; i < value1.Length && i < value2.Length; i++)
			{
				if (value1[i] != value2[i])
				{
					result = "\r\nDifference found at offset: " + i;
					result += "\r\nOriginal 1K around area: " + GetIKAroundOffset(value1, i);
					result += "\r\nReloaded 1K around area: " + GetIKAroundOffset(value2, i);
					break;
				}
			}

			return result;
		}

		string GetIKAroundOffset(string value, int offset)
		{
			int startIndex = offset - 500 >= 0 ? offset - 500 : 0;
			int length = 1000;
			if (length + startIndex > value.Length - 1)
			{
				length = value.Length - startIndex - 1;
			}
			return value.Substring(startIndex, length);
		}

		string GetInfoAboutString(string @string)
		{
			string result = "";

			result += "\r\nLength = " + @string.Length;

			result += "\r\nFirst 1K\r\n" + GetIKAroundOffset(@string, 0);
			result += "\r\nLast 1K\r\n" + GetIKAroundOffset(@string, @string.Length - 500);

			return result;
		}

		#endregion

		#region BusinessObjectsInLastSaveOrder

		public void TestBusinessObjectsInLastSaveOrderGetsCorrectOrder()
		{
			SaveCounter.SavingCount = 0;
			DummyDependentCounter child = (DummyDependentCounter)Factory.New(typeof(DummyDependentCounter));
			DummyCounter parent = (DummyCounter)Factory.New(typeof(DummyCounter));
			DummyPivotCounter pivot = (DummyPivotCounter)Factory.New(typeof(DummyPivotCounter));

			pivot.ZDP_Z0 = parent.PK;
			pivot.ZDP_ZD1 = child.PK;
			child.ZD1_Z0 = parent.PK;

			Factory.Save();

			BusinessObject[] saveOrder = Factory.BusinessObjectsInLastSaveOrder;

			AssertEquals("Save order", parent, saveOrder[0]);
			AssertEquals("Save order", child, saveOrder[1]);
			AssertEquals("Save order", pivot, saveOrder[2]);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestBusinessObjectsInLastSaveOrderThrowsExceptionWhenNoSave()
		{
			BusinessObject[] saveOrder = Factory.BusinessObjectsInLastSaveOrder;
		}

		public void TestBusinessObjectsInLastSaveOrderIncludesAllBizOsAroundARow()
		{
			DummyCounter dummy1 = (DummyCounter)Factory.New(typeof(DummyCounter));
			DummyBusinessObject dummy2 = (DummyBusinessObject)Factory.Load(typeof(DummyBusinessObject), dummy1.PK);
			Factory.Save();

			BusinessObject[] saveOrder = Factory.BusinessObjectsInLastSaveOrder;
			AssertEquals("Both objects recorded", 2, saveOrder.Length);

			List<BusinessObject> listSaveOrder = new List<BusinessObject>(saveOrder);
			Assert("In list", listSaveOrder.Contains(dummy1));
			Assert("In list", listSaveOrder.Contains(dummy2));
		}

		#endregion

		[ExpectNoExceptions]
		public void TestRunPresaveValidationDuringPropertyValidation_NoException()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithPreSaveValidation>();
			dummy.Validation.ValidateZ0_Decimal();
		}

		class DummyWithPreSaveValidation : DummyWithValidation
		{
			public DummyWithPreSaveValidation(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new DummyWithPreSaveZValidation Validation
			{
				get { return (DummyWithPreSaveZValidation)base.Validation; }
			}

			protected override DummyBizoValidation GetNewValidation()
			{
				return (DummyBizoValidation)Activator.CreateInstance(ValidationType, new object[] { this });
			}

			public new Type ValidationType
			{
				get { return fValidationType; }
				set { fValidationType = value; }
			}
			Type fValidationType = typeof(DummyWithPreSaveZValidation);
		}

		class DummyWithPreSaveZValidation : DummyZValidation
		{
			public DummyWithPreSaveZValidation(DummyWithPreSaveValidation parent)
				: base(parent)
			{
			}

			protected override void CheckZ0_Decimal()
			{
				Parent.RunPreSaveValidation();
				base.CheckZ0_Decimal();
			}
		}

		class DummyForMultipleGetNullTest : DummyBusinessObject
		{
			public bool IsConstructingNullBusinessObjectBeforeFinalizer { get; }

			public static BusinessObjectFactory TestFactory { get; set; }

			public DummyForMultipleGetNullTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
				TestFactory.GetNull(typeof(DummyBusinessObject));
				IsConstructingNullBusinessObjectBeforeFinalizer = factory.IsConstructingNullBusinessObject;
			}
		}

		public void TestChangeNumber()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			AssertEquals("CurrentChangeNumber", 0, factory.LastChangeNumber);
			AssertEquals("First change is 1", 1, factory.GetNextChangeNumber());
			AssertEquals("Next is 2", 2, factory.GetNextChangeNumber());
			AssertEquals("Next is 3", 3, factory.GetNextChangeNumber());
			AssertEquals("CurrentChangeNumber", 3, factory.LastChangeNumber);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			AssertEquals("Factory change numbers are not related", 1, factory2.GetNextChangeNumber());
			AssertEquals("CurrentChangeNumber", 1, factory2.LastChangeNumber);
		}

		public void TestGetNullUsesDifferentFactory()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.GetNull(typeof(DummyBusinessObject));
			Assert("Should be different factories; we don't want to save the null object to the database!", dummy.Factory != Factory);
		}

		public void TestMultipleGetNullCalls_IsConstructingBusinessObjectReturnsTrueFromSecondCall()
		{
			DummyForMultipleGetNullTest.TestFactory = Factory;
			var dummy = Factory.GetNull(typeof(DummyForMultipleGetNullTest)) as DummyForMultipleGetNullTest;

			AssertEquals(true, dummy.IsNull);
			AssertEquals(true, dummy.IsConstructingNullBusinessObjectBeforeFinalizer);
		}

		public void TestDefaultAllowMultipleBusinessObjectsAroundOneRowValue()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			Assert("Test default for AllowMultipleBusinessObjectsAroundOneRow", factory.AllowMultipleBusinessObjectsAroundOneRow);
		}

		#region Test SingleObjectAroundARow

		[SingleObjectAroundARow]
		class DummySub : DummyBusinessObject
		{
			public DummySub(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		class DummySubSub : DummySub
		{
			public DummySubSub(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		public void TestSubclassReturnedWhenSingleObjectAroundARowImplemented()
		{
			DummySubSub bizO1 = (DummySubSub)Factory.New(typeof(DummySubSub));
			DummySub bizO2 = (DummySub)Factory.Load(typeof(DummySub), bizO1.PK);
			AssertEquals(bizO1, bizO2);
		}

		public void TestAdditionalDebuggingInfoOnIncorrectReturnType()
		{
			var collection = new ActiveBusinessObjectCollection<DummySubSub>(Factory, new ZQuery(DummyBizoSchema.Z0_Code, "A"));
			_ = collection.Count;
			var biz01 = Factory.New<DummySub>();
			var ex = AssertExceptionThrown<ApplicationException>("Incorrect return", () => biz01.Z0_Code = "A");
			var message = ex.Message;
			AssertStartsWith("ex.Message start", "Error has occur while creating Business object in CargoWise.EntityFramework.ActiveBusinessObjectCollectionIndexDataView`1[[CargoWise.EntityFramework.Testing.BusinessObjectFactoryTest+DummySubSub", message);
			AssertEndsWith("ex.Message end", "]] (ElementType:CargoWise.EntityFramework.Testing.BusinessObjectFactoryTest+DummySubSub, Complete Filter:Z0_Code = 'A')", message);
			var innerEx = (ApplicationException)ex.InnerException;
			AssertEquals("innerEx.Message", "Attempted to return a CargoWise.EntityFramework.Testing.BusinessObjectFactoryTest+DummySub when a CargoWise.EntityFramework.Testing.BusinessObjectFactoryTest+DummySubSub was requested.(AllowMultipleBusinessObjectsAroundOneRow=True, SingleObjectAroundARow=True)", innerEx.Message);
		}

		#endregion

		public void TestDeveloperErrorWhenMultipleObjectsAlreadyAroundOneRowAndAllowSetToFalse()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			DummyBusinessObject dummy = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
			DummyBaseBusinessObject dummyBase = (DummyBaseBusinessObject)factory.Load(typeof(DummyBaseBusinessObject), dummy.PK);

			factory.AllowMultipleBusinessObjectsAroundOneRow = false;

			AssertNotNull("Should be loaded", dummy);
			AssertNotNull("Should be loaded", dummyBase);
			Assert("AllowMultipleBusinessObjectsAroundOneRow not set", factory.AllowMultipleBusinessObjectsAroundOneRow);

			Assert("Correct error message shown", ErrorReporter.LastMessageReported.Contains("AllowMultipleBusinessObjectsAroundOneRow cannot be set"));
			ErrorReporter.Clear();
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestExceptionWhenMultipleObjectsLoadedAroundOneRow()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			factory.AllowMultipleBusinessObjectsAroundOneRow = false;

			DummyBaseBusinessObject dummy = factory.New<DummyBaseBusinessObject>();
			DummyBusinessObject dummyBase = factory.Load<DummyBusinessObject>(dummy.PK);
		}

		public void TestAllowMultipleBusinessObjectsAroundOneRowGetSet()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			factory.AllowMultipleBusinessObjectsAroundOneRow = true;
			Assert("Set", factory.AllowMultipleBusinessObjectsAroundOneRow);
			factory.AllowMultipleBusinessObjectsAroundOneRow = false;
			Assert("Set", !factory.AllowMultipleBusinessObjectsAroundOneRow);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestCanSave()
		{
			Assert("Can save by default", ((IBusinessObjectFactoryInternals)Factory).CanSave);
			((IBusinessObjectFactoryInternals)Factory).CanSave = false;
			Assert("CanSave value set", !((IBusinessObjectFactoryInternals)Factory).CanSave);
			Factory.Save();
		}

		public void TestReadOnly()
		{
			Assert("!ReadOnly by default", !((IBusinessObjectFactoryInternals)Factory).ReadOnly);
			((IBusinessObjectFactoryInternals)Factory).ReadOnly = true;
			Assert("CanSave value set", ((IBusinessObjectFactoryInternals)Factory).ReadOnly);
			((IBusinessObjectFactoryInternals)Factory).ReadOnly = false;
			Assert("!ReadOnly set", !((IBusinessObjectFactoryInternals)Factory).ReadOnly);
		}

		#region Calculated Property Cache

		public void TestSuspendHasChangesIncreasesCacheVersion()
		{
			DummyBusinessObject bizObj = DummyBusinessObject.New(Factory);
			using (bizObj.SuspendSettingHasChanges())
			{
				int initialValue = Factory.CacheVersion;
				bizObj.Z0_Code = "12345";
				Assert(initialValue != Factory.CacheVersion);
			}
		}

		public void TestInvalidateCachedPropertiesIncreasesCacheVersion()
		{
			int initialValue = Factory.CacheVersion;
			Factory.InvalidateCachedProperties();
			Assert(initialValue != Factory.CacheVersion);
		}

		public void TestNewObjectIncreasesCacheVersion()
		{
			int initialValue = Factory.CacheVersion;
			Factory.New<DummyBusinessObject>();
			Assert(initialValue != Factory.CacheVersion);
		}

		public void TestDeleteObjectIncreasesCacheVersion()
		{
			DummyBusinessObject bizObj = DummyBusinessObject.New(Factory);
			int initialValue = Factory.CacheVersion;
			bizObj.Delete();
			Assert(initialValue != Factory.CacheVersion);
		}

		public void TestDeletedObjectIsRemovedFromCache()
		{
			var dummy = DummyBusinessObject.New(Factory);
			AssertCollectionContains("Dummy should be cached in factory.", dummy, ((IBusinessObjectFactoryInternals)Factory).AllBusinessObjects);

			dummy.Delete();
			AssertCollectionContains("Deleted dummy still should remain in factory's cache.", dummy, ((IBusinessObjectFactoryInternals)Factory).AllBusinessObjects);

			Factory.Save();
			AssertCollectionNotContains("Deleted and committed dummy should be removed from factory's cache.", dummy, ((IBusinessObjectFactoryInternals)Factory).AllBusinessObjects);
		}

		public void TestLoadObjectDoesNotIncreaseCacheVersion()
		{
			DummyBusinessObject bizObj = DummyBusinessObject.New(Factory);
			Factory.Save();
			int initalValue = Factory.CacheVersion;
			Factory.Load(typeof(DummyBusinessObject), bizObj.PK);
			AssertEquals(initalValue, Factory.CacheVersion);
		}

		public void TestDataRefreshBusGoesTootToot_AndDoesntPublishNonChangedObjects()
		{
			DummyBusinessObject bizObj1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject bizObj2 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject bizObj3 = Factory.New<DummyBusinessObject>();
			Factory.Save();

			bizObj1.Z0_Code = "123";
			Factory.Save();
			AssertEquals(2, Factory.PublishCountForLastSave);

			Factory.Save();
			AssertEquals(0, Factory.PublishCountForLastSave);
		}

		/// <summary>
		/// This test is to simulate that a business object generated from a database view will be published by data refresh bus
		/// </summary>
		public void TestUnsavablePersistentObjectIsPublished()
		{
			var bizObj = Factory.New<DummyUnsavablePersistentObjectThatRequiresRefresh>();
			Factory.Save();

			AssertEquals("Pre condition", 0, Factory.PublishCountForLastSave);

			bizObj.Z0_Description = "Changed";
			Factory.Save();
			AssertEquals("bizObj is published", 2, Factory.PublishCountForLastSave);

			Factory.Save();
			AssertEquals("Nothing has been changed so nothing is published", 0, Factory.PublishCountForLastSave);
		}

		public void TestZQueryNoResultQueryFetchHintNotAdded()
		{
			ZQuery query = new ZQuery();
			query.IsNoResultQuery = true;

			ZQuery query2 = new ZQuery();

			Factory.AddFetchHint(DummyBizoSchema.Instance, query);
			AssertEquals("NoResultQuery fetch hints ignored", 0, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));

			Factory.AddFetchHint(DummyBizoSchema.Instance, query2);
			AssertEquals("Empty, but not no-result query fetch hints added", 1, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
		}

		public void TestNoResultQueryThenValidQueryNotCached()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "ABC";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var noResultQuery = new ZQuery() { IsNoResultQuery = true };
			AssertEquals(0, newFactory.Load<DummyBusinessObject>(noResultQuery).Length);

			var validResultQuery = new ZQuery(DummyBizoSchema.Z0_Code, "ABC");
			AssertEquals("Results from no result query are not cached", 1, newFactory.Load<DummyBusinessObject>(validResultQuery).Length);
		}

		public void TestZQueryFetchHintDecreasesDbHits()
		{
			ZGuid zGuid1 = ZGuid.NewZGuid();
			ZGuid zGuid2 = ZGuid.NewZGuid();
			ZGuid zGuid3 = ZGuid.NewZGuid();
			ZGuid zGuid4 = ZGuid.NewZGuid();
			ZQuery query1 = new ZQuery(DummyBizoSchema.Z0_Guid, zGuid1);
			query1.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Guid, SQLComparisonOperator.Equal, zGuid2);

			ZQuery query2 = new ZQuery(DummyBizoSchema.Z0_Guid, zGuid3);
			query2.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Guid, SQLComparisonOperator.Equal, zGuid4);

			Factory.AddFetchHint(DummyBizoSchema.Instance, query1);
			Factory.AddFetchHint(DummyBizoSchema.Instance, query2);

			AssertEquals(0, Factory.DatabaseLoadCount);
			Factory.Load<DummyBusinessObject>(query1);
			AssertEquals(1, Factory.DatabaseLoadCount);
			Factory.Load<DummyBusinessObject>(query2);
			AssertEquals(1, Factory.DatabaseLoadCount);
		}

		public void TestPartlyMatchedFetchHints()
		{
			var factory2 = new BusinessObjectFactory();
			var bizO1 = factory2.New<DummyBusinessObject>();
			bizO1.Z0_Code = "1";
			factory2.New<DummyBusinessObject>().Z0_Code = "2";
			factory2.New<DummyBusinessObject>().Z0_Code = "3";
			factory2.Save();

			// This functionally test BusinessObjectFetcher so that the factory level contract is met
			AssertEquals(0, Factory.DatabaseLoadCount);
			int totalObjectsLoaded = 0;
			Factory.Loaded += (s, e) =>
			{
				totalObjectsLoaded += e.NewObjects.Length;
			};

			Factory.AddFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code, (ZString)"1");
			Factory.AddFetchHint(DummyBizoSchema.Z0_Code, (ZString)"2");  // row hint only
			Factory.AddFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code, (ZString)"3");

			Factory.Load<DummyBusinessObject>(bizO1.PK);
			AssertEquals(1, totalObjectsLoaded);
			AssertEquals(1, Factory.DatabaseLoadCount);
			AssertEquals(0, Factory.ActiveFetchHintsForTable("DummyBizO")); // data layer fetch hints have fired
																			// The first db hit should query all three codes
																			// but not all business objects should be created
			Factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, "1"));
			AssertEquals(1, totalObjectsLoaded);          // this is actually the '3' object
			AssertEquals(1, Factory.DatabaseLoadCount);   // but needed to go to the db to be sure it had all '1's
														  // The second should make no db hits, but will make the '2' object
			Factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, "2"));
			AssertEquals(2, totalObjectsLoaded);
			AssertEquals(1, Factory.DatabaseLoadCount);
			Factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, "3"));
			AssertEquals(2, totalObjectsLoaded);
			AssertEquals(1, Factory.DatabaseLoadCount);
			AssertEquals(0, Factory.businessObjectFetchHintManager.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
		}

		public void TestOrFilterUsesPreviousCacheHintLoads()
		{
			ZGuid zGuid1 = ZGuid.NewZGuid();
			ZGuid zGuid2 = ZGuid.NewZGuid();
			ZGuid zGuid3 = ZGuid.NewZGuid();
			ZGuid zGuid4 = ZGuid.NewZGuid();

			DummyBusinessObject.New(Factory).Z0_Guid = zGuid1;
			DummyBusinessObject.New(Factory).Z0_Guid = zGuid2;
			DummyBusinessObject.New(Factory).Z0_Guid = zGuid3;

			Factory.AddFetchHint(DummyBizoSchema.Z0_Guid, zGuid1);
			Factory.AddFetchHint(DummyBizoSchema.Z0_Guid, zGuid2);
			Factory.AddFetchHint(DummyBizoSchema.Z0_Guid, zGuid3);
			ZQuery missFilter = new ZQuery(DummyBizoSchema.Z0_Guid, zGuid4);
			string cachedKeysBefore = string.Join("\r\n", Factory.RowFactory.QueryCache.CachedKeys);
			AssertEquals(0, Factory.Load<DummyBusinessObject>(missFilter).Length);  // Complete miss - should force load of hints and database miss.
			AssertEquals(2, Factory.DatabaseLoadCount);
			string cachedKeysAfter = string.Join("\r\n", Factory.RowFactory.QueryCache.CachedKeys);

			ZQuery hitFilter = new ZQuery();
			hitFilter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Guid, SQLComparisonOperator.Equal, zGuid1);
			hitFilter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Guid, SQLComparisonOperator.Equal, zGuid2);
			AssertEquals(2, Factory.Load<DummyBusinessObject>(hitFilter).Length); // hit, but should use previous hints
			AssertEquals(2, Factory.DatabaseLoadCount); // second query should not make db hit - all 'ors' are satisfied by previous hint
		}

		public void TestTableHits_Exists_ShouldIncrementHitCounter()
		{
			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();

			AssertEquals(true, newFactory.Exists(dummy1.GetType(), new ZQuery(DummyBizoSchema.PK, dummy1.PK), mergeDbAndCacheResult: false));
			TestCaseWithFactory.AssertDbHits(new Dictionary<string, int> { { DummyBizoSchema.Constants.TableName, 1 } }, newFactory);

			AssertEquals(true, newFactory.Exists(dummy1.GetType(), new ZQuery(DummyBizoSchema.PK, dummy1.PK), mergeDbAndCacheResult: false));
			TestCaseWithFactory.AssertDbHits(new Dictionary<string, int> { { DummyBizoSchema.Constants.TableName, 2 } }, newFactory);

			AssertEquals(true, newFactory.Exists(dummy2.GetType(), new ZQuery(DummyBizoSchema.PK, dummy2.PK), mergeDbAndCacheResult: false));
			TestCaseWithFactory.AssertDbHits(new Dictionary<string, int> { { DummyBizoSchema.Constants.TableName, 3 } }, newFactory);
		}

		public void TestTableHits_ExistsInDatabase_ShouldIncrementHitCounter()
		{
			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();

			AssertEquals(true, newFactory.ExistsInDatabase(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.PK, dummy1.PK)));
			TestCaseWithFactory.AssertDbHits(new Dictionary<string, int> { { DummyBizoSchema.Constants.TableName, 1 } }, newFactory);

			AssertEquals(true, newFactory.ExistsInDatabase(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.PK, dummy1.PK)));
			TestCaseWithFactory.AssertDbHits(new Dictionary<string, int> { { DummyBizoSchema.Constants.TableName, 2 } }, newFactory);

			AssertEquals(true, newFactory.ExistsInDatabase(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.PK, dummy2.PK)));
			TestCaseWithFactory.AssertDbHits(new Dictionary<string, int> { { DummyBizoSchema.Constants.TableName, 3 } }, newFactory);
		}

		public void TestEnableFetchHintsProcessingWithoutTableHitCounter()
		{
			var factory1 = new BusinessObjectFactory();
			var dummy1 = factory1.New<DummyBusinessObject>();
			dummy1.Z0_Code = "AAA";
			var dummy2 = factory1.New<DummyBusinessObject>();
			dummy2.Z0_Code = "ZZZ";
			var dummy3 = factory1.New<DummyBusinessObject>();
			dummy3.Z0_Code = "ZAZ";
			factory1.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.AddFetchHint(DummyBizoSchema.PK, dummy1.PK);
			factory2.AddFetchHint(DummyBizoSchema.PK, dummy2.PK);
			AssertEquals(0, factory2.DatabaseLoadCount);
			AssertNotNull("Loading dummy1 in factory2", factory2.Load<DummyBusinessObject>(dummy1.PK));
			var tableSelect = factory2.GetTableHitCount(DummyBizoSchema.Constants.TableName);
			AssertEquals(1, tableSelect);
			AssertEquals(1, factory2.DatabaseLoadCount);
			AssertNotNull("Loading dummy2 in factory2", factory2.Load<DummyBusinessObject>(dummy2.PK));
			tableSelect = factory2.GetTableHitCount(DummyBizoSchema.Constants.TableName);
			AssertEquals(1, tableSelect);
			AssertEquals(1, factory2.DatabaseLoadCount);
			AssertNotNull("Loading dummy3 in factory2", factory2.Load<DummyBusinessObject>(dummy3.PK));
			tableSelect = factory2.GetTableHitCount(DummyBizoSchema.Constants.TableName);
			AssertEquals("1 extra as it was not part of fetch hint", 2, tableSelect);
			AssertEquals(2, factory2.DatabaseLoadCount);

			var factory3 = new BusinessObjectFactory();
			factory3.AddFetchHint(DummyBizoSchema.PK, dummy1.PK);
			factory3.AddFetchHint(DummyBizoSchema.PK, dummy2.PK);
			AssertEquals(0, factory3.DatabaseLoadCount);
			factory3.ExecuteAllFetchHints();
			tableSelect = factory3.GetTableHitCount(DummyBizoSchema.Constants.TableName);
			AssertEquals(1, tableSelect);
			AssertEquals(1, factory3.DatabaseLoadCount);
			AssertNotNull("Loading dummy1 in factory2", factory3.Load<DummyBusinessObject>(dummy1.PK));
			tableSelect = factory3.GetTableHitCount(DummyBizoSchema.Constants.TableName);
			AssertEquals(1, tableSelect);
			AssertEquals(1, factory3.DatabaseLoadCount);
			AssertNotNull("Loading dummy2 in factory2", factory3.Load<DummyBusinessObject>(dummy2.PK));
			tableSelect = factory3.GetTableHitCount(DummyBizoSchema.Constants.TableName);
			AssertEquals(1, tableSelect);
			AssertEquals(1, factory3.DatabaseLoadCount);
			AssertNotNull("Loading dummy3 in factory2", factory3.Load<DummyBusinessObject>(dummy3.PK));
			tableSelect = factory3.GetTableHitCount(DummyBizoSchema.Constants.TableName);
			AssertEquals("1 extra as it was not part of fetch hint", 2, tableSelect);
			AssertEquals(2, factory3.DatabaseLoadCount);

			var factory4 = new BusinessObjectFactory();
			factory4.AddFetchHint(DummyBizoSchema.PK, dummy1.PK);
			factory4.AddFetchHint(DummyBizoSchema.PK, dummy2.PK);
			AssertEquals(0, factory4.DatabaseLoadCount);
			using (factory4.EnableFetchHintsProcessingWithoutTableHitCounter())
			{
				AssertNotNull("Loading dummy1 in factory4", factory4.Load<DummyBusinessObject>(dummy1.PK));
				tableSelect = factory4.GetTableHitCount(DummyBizoSchema.Constants.TableName);
				AssertEquals("Load via fetch hint should not be counted", 0, tableSelect);
				AssertEquals(1, factory4.DatabaseLoadCount);

				AssertNotNull("Loading dummy3 in factory4", factory4.Load<DummyBusinessObject>(dummy3.PK));
				tableSelect = factory4.GetTableHitCount(DummyBizoSchema.Constants.TableName);
				AssertEquals("dummy3 is not part of fetch hint, so it should count", 1, tableSelect);
				AssertEquals(2, factory4.DatabaseLoadCount);
			}
			AssertNotNull("Loading dummy2 in factory4", factory4.Load<DummyBusinessObject>(dummy2.PK));
			tableSelect = factory4.GetTableHitCount(DummyBizoSchema.Constants.TableName);
			AssertEquals("Shoubl be one from dummy3", 1, tableSelect);
			AssertEquals(2, factory4.DatabaseLoadCount);
		}

		#region OnSaving for Multiple Objects around a row

		class DummyBusinessObjectThatCreatesObjectInOnSaving : DummyBusinessObject
		{
			public DummyBusinessObjectThatCreatesObjectInOnSaving(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override void OnSaving()
			{
				MultiObjectAroundARow = (DummyBusinessObject)Factory.Load(typeof(DummyBusinessObject), PK);
				base.OnSaving();
			}

			public DummyBusinessObject MultiObjectAroundARow;
		}

		public void TestOnSavingCalledForMultipleObjectsAroundARowWhenObjectCreatedInOnSaving()
		{
			DummyBusinessObjectThatCreatesObjectInOnSaving bizO = (DummyBusinessObjectThatCreatesObjectInOnSaving)Factory.New(typeof(DummyBusinessObjectThatCreatesObjectInOnSaving));
			AssertNull("Precondition", bizO.MultiObjectAroundARow);
			Assert("Precondition", !bizO.OnSavingCalled);
			Factory.Save();
			Assert(bizO.OnSavingCalled);
			Assert(bizO.MultiObjectAroundARow.OnSavingCalled);
		}

		#endregion

		public void TestOnDataRefreshBusIncreasesCacheVersion()
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject bizo = DummyBusinessObject.New(factory2);
			factory2.Save();

			DummyBusinessObject bizoFactory1 = (DummyBusinessObject)Factory.Load(typeof(DummyBusinessObject), bizo.PK);
			bizo.Z0_Code = "XYZ";
			int initialValue = bizoFactory1.Factory.CacheVersion;
			factory2.Save();
			AssertEquals("Precondition", "XYZ", bizoFactory1.Z0_Code);
			Assert(initialValue != bizoFactory1.Factory.CacheVersion);
		}

		public void TestModifiedObjectIncreasesCacheVersion()
		{
			DummyBusinessObject bizObj = DummyBusinessObject.New(Factory);
			int initialValue = Factory.CacheVersion;
			bizObj.Z0_Code = "XYZ";
			Assert(initialValue != Factory.CacheVersion);
		}

		public void TestAddedToNonDependentCollectionIncreasesCacheVersion()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObject bizObj = DummyBusinessObject.New(Factory);
			int initialValue = Factory.CacheVersion;
			collection.Add(bizObj);
			Assert(initialValue != Factory.CacheVersion);
		}

		public void TestRemoveFromNonDependentCollectionIncreasesCacheVersion()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObject bizObj = DummyBusinessObject.New(Factory);
			collection.Add(bizObj);

			int initialValue = Factory.CacheVersion;
			collection.Remove(bizObj);
			Assert(initialValue != Factory.CacheVersion);
		}

		#endregion

		public void TestCachingOfQueriesIncludesOrderBy()
		{
			TestCaseHelper.ClearTable(DummyBizoSchema.Constants.TableName);

			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = (DummyBusinessObject)factory1.New(typeof(DummyBusinessObject));
			dummy1.Z0_Code = "AAA";
			DummyBusinessObject dummy2 = (DummyBusinessObject)factory1.New(typeof(DummyBusinessObject));
			dummy2.Z0_Code = "ZZZ";
			factory1.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			ZQuery query = new ZQuery();
			query.MaximumRows = 1;

			query.OrderBy = DummyBizoSchema.Constants.Z0_Code + " ASC";
			AssertEquals("Right order", dummy1.PK, factory2.LoadTop1(typeof(DummyBusinessObject), query).PK);

			query.OrderBy = DummyBizoSchema.Constants.Z0_Code + " DESC";
			AssertEquals("Right order", dummy2.PK, factory2.LoadTop1(typeof(DummyBusinessObject), query).PK);
		}

		public void TestChineseCharactersCanBeSavedAndRetrieved()
		{
			const string TestChineseChars = "\u5432\u5e4b";

			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
			dummy1.Z0_NVarChar = TestChineseChars;

			factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummy1Reloaded = (DummyBusinessObject)factory2.Load(typeof(DummyBusinessObject), dummy1.PK);
			AssertEquals("Z0_NVarChar", TestChineseChars, dummy1Reloaded.Z0_NVarChar);
		}

		public void TestUpdateHasChangesOnOtherBusinessObjectsAroundThisRow()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			DummyBusinessObject dummy = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
			DummyBaseBusinessObject dummyBase = (DummyBaseBusinessObject)factory.Load(typeof(DummyBaseBusinessObject), dummy.PK);
			dummy.HasChanges = true;
			Assert("HasChanges", dummyBase.HasChanges);

			dummy.HasChanges = false;
			Assert("HasChanges", !dummyBase.HasChanges);
		}

		public void TestHasChangesGetsSetCorrectlyForNewObjectsAroundSameRow()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			DummyBusinessObject dummy = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
			Assert("No changes", !dummy.HasChanges);

			dummy.Z0_AnotherDecimal = 23;
			Assert("HasChanges", dummy.HasChanges);

			DummyBaseBusinessObject dummyBase = (DummyBaseBusinessObject)factory.Load(typeof(DummyBaseBusinessObject), dummy.PK);

			Assert("HasChanges", dummyBase.HasChanges);
		}

		public void TestEscapingOfSquareBrackets()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummy = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
			dummy.Z0_Description = "[Teapot]*%[Noodle[[";
			factory.Save();

			DummyBusinessObject localReload = (DummyBusinessObject)factory.LoadTop1(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Description, dummy.Z0_Description));
			AssertNotNull("Found Dummy", localReload);
			AssertEquals("Reloaded correctly", dummy.Z0_Description, localReload.Z0_Description);

			DummyBusinessObject dbReload = (DummyBusinessObject)new BusinessObjectFactory().LoadTop1(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Description, dummy.Z0_Description));
			AssertNotNull("Found Dummy", dbReload);
			AssertEquals("Reloaded correctly", dummy.Z0_Description, dbReload.Z0_Description);
		}

		public void TestEscapingOfSquareBracketsWithContainsStatement()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummy = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
			dummy.Z0_Description = "[Teapot]*%[Noodle[[";
			factory.Save();

			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Description, SQLComparisonOperator.Contains, dummy.Z0_Description);

			DummyBusinessObject localReload = (DummyBusinessObject)factory.LoadTop1(typeof(DummyBusinessObject), filter);
			AssertNotNull("Found Dummy", localReload);
			AssertEquals("Reloaded correctly", dummy.Z0_Description, localReload.Z0_Description);

			DummyBusinessObject dbReload = (DummyBusinessObject)new BusinessObjectFactory().LoadTop1(typeof(DummyBusinessObject), filter);
			AssertNotNull("Found Dummy", dbReload);
			AssertEquals("Reloaded correctly", dummy.Z0_Description, dbReload.Z0_Description);
		}

		public void TestAddNewBusinessObjectToCache()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			// bit of a trick here - just to get the dummy not added to the factory. Don't do this otherwise.
			DummyBusinessObject dummy = new BusinessObjectFactory().New<DummyBusinessObject>();

			AssertEquals("Not Found dummy", 0, factory.GetBizOsForPK(dummy.PK.ToGuid()).Length);

			factory.AddNewBusinessObjectToCache(dummy);
			AssertEquals("Found dummy", 1, factory.GetBizOsForPK(dummy.PK.ToGuid()).Length);
			AssertEquals("Found dummy", dummy, factory.GetBizOsForPK(dummy.PK.ToGuid())[0]);
		}

		public void TestLoadByEmptyGuidDoesntHitDatabase()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			int commandCountBefore = Db.Connection.ExecutedCommandCount;
			newFactory.Load(typeof(DummyBaseBusinessObject), ZGuid.Empty);
			int commandCountAfter = Db.Connection.ExecutedCommandCount;
			AssertEquals("Should not execute any commands on the db", commandCountBefore, commandCountAfter);
		}

		public void TestMultipleInsertsAndUpdatesThenDelete()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = (DummyBusinessObject)factory1.New(typeof(DummyBusinessObject));
			dummy1.Z0_AnotherDate = ZDateTime.Now;
			dummy1.Z0_AnotherDecimal = new ZDecimal(4.44);
			dummy1.Z0_Bool = true;
			dummy1.Z0_Byte = 5;
			dummy1.Z0_VarBinaryMax = ZBlob.FromUTF8(ZString.Replicate('a', 500));
			dummy1.Z0_Code = "abc";
			dummy1.Z0_Date = ZDateTime.Now;
			dummy1.Z0_Decimal = new ZDecimal(4.0); // 0 dp
			dummy1.Z0_Description = "blah";
			dummy1.Z0_FK_Code = "zf";
			dummy1.Z0_Guid = Guid.NewGuid();
			dummy1.Z0_Number = 4;
			dummy1.Z0_Short = 44;
			dummy1.Z0_Money = new ZDecimal(4.2242);
			dummy1.Z0_SmallDateTime = ZDateTime.MaxSmallDateTime;
			dummy1.Z0_NVarChar = "dingle" + ZString.Replicate('n', 10);
			dummy1.Z0_NVarCharMax = "dangle" + ZString.Replicate('n', 500);
			dummy1.Z0_VarCharMax = ZString.Replicate('g', 500);
			dummy1.Z0_AnotherNumber = 243;

			factory1.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummy2 = (DummyBusinessObject)factory2.Load(typeof(DummyBusinessObject), dummy1.PK);
			AssertHasSamePropertyValues(dummy1, dummy2);

			dummy2.Z0_AnotherDate = ZDateTime.Now;
			dummy2.Z0_AnotherDecimal = new ZDecimal(14.44);
			dummy2.Z0_Bool = false;
			dummy2.Z0_Byte = 55;
			dummy2.Z0_VarBinaryMax = ZBlob.FromUTF8(ZString.Replicate('b', 1000));
			dummy2.Z0_Code = "def";
			dummy2.Z0_Date = ZDateTime.Now;
			dummy2.Z0_Decimal = new ZDecimal(2.0); // 0 dp
			dummy2.Z0_Description = "teapot blah";
			dummy2.Z0_FK_Code = "uuu";
			dummy2.Z0_Guid = Guid.NewGuid();
			dummy2.Z0_Number = 423;
			dummy2.Z0_Short = 244;
			dummy2.Z0_Money = new ZDecimal(5487.3);
			dummy2.Z0_SmallDateTime = ZDateTime.MaxSmallDateTime.AddHours(-100);
			dummy2.Z0_NVarChar = "dingle" + ZString.Replicate('z', 10);
			dummy2.Z0_NVarCharMax = "dangle" + ZString.Replicate('e', 500);
			dummy2.Z0_VarCharMax = ZString.Replicate('u', 500);
			dummy2.Z0_AnotherNumber = 5243;
			factory2.Save();

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			DummyBusinessObject dummy3 = (DummyBusinessObject)factory3.Load(typeof(DummyBusinessObject), dummy1.PK);
			AssertHasSamePropertyValues(dummy2, dummy3);

			dummy3.Z0_AnotherDate = ZDateTime.Now;
			dummy3.Z0_AnotherDecimal = new ZDecimal(914.44);
			dummy3.Z0_Bool = true;
			dummy3.Z0_Byte = 12;
			dummy3.Z0_VarBinaryMax = ZBlob.FromUTF8(ZString.Replicate('z', 1000));
			dummy3.Z0_Code = "fff";
			dummy3.Z0_Date = ZDateTime.Now;
			dummy3.Z0_Decimal = new ZDecimal(122.0); // 0 dp
			dummy3.Z0_Description = " ick'ley tea'pot 'blah";
			dummy3.Z0_FK_Code = "juj";
			dummy3.Z0_Guid = Guid.NewGuid();
			dummy3.Z0_Number = 32;
			dummy3.Z0_Short = 534;
			dummy3.Z0_Money = new ZDecimal(32.3);
			dummy3.Z0_SmallDateTime = ZDateTime.MaxSmallDateTime.AddHours(-900);
			dummy3.Z0_NVarChar = "dingle" + ZString.Replicate('!', 10);
			dummy3.Z0_NVarCharMax = "dangle" + ZString.Replicate('@', 500);
			dummy3.Z0_VarCharMax = ZString.Replicate('\'', 500);
			dummy3.Z0_AnotherNumber = 5243;
			factory3.Save();

			BusinessObjectFactory factory4 = new BusinessObjectFactory();
			DummyBusinessObject dummy4 = (DummyBusinessObject)factory4.Load(typeof(DummyBusinessObject), dummy1.PK);
			AssertHasSamePropertyValues(dummy3, dummy4);
			dummy4.Delete();
			factory4.Save();

			BusinessObjectFactory factory5 = new BusinessObjectFactory();
			AssertNull("Should be deleted", factory5.Load(typeof(DummyBusinessObject), dummy1.PK));
		}

		void AssertHasSamePropertyValues(DummyBusinessObject dummy1, DummyBusinessObject dummy2)
		{
			foreach (ZPropertyInfo info in dummy1.ZPropertyInfoHash)
			{
				AssertEquals("Property is not equal: " + info.Name, dummy1[info.Name], dummy2[info.Name]);
			}
		}

		#region Testing Deletes that rely on updates or other deletes

		[ExpectException(typeof(ZSaveException))]
		public void TestDummyDependentHasRelevantConstraints()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			DummyDependantBusinessObject dummyDependent = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			dummyDependent.ZD1_Z0 = dummy.PK;
			Factory.Save();

			dummy.Delete();
			Factory.Save();
		}

		[ExpectNoExceptions()]
		public void TestDeletesThatOccurBeforeUpdates()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			DummyDependantBusinessObject dummyDependent = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			dummyDependent.ZD1_Z0 = dummy.PK;
			Factory.Save();

			dummy.Delete();
			dummyDependent.Delete();
			Factory.Save();
		}

		[ExpectNoExceptions()]
		public void TestDeletesThatRequireUpdatesFirst()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			DummyDependantBusinessObject dummyDependent = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			dummyDependent.ZD1_Z0 = dummy.PK;
			Factory.Save();

			dummy.Delete();
			dummyDependent.ZD1_Z0 = ZGuid.Empty;
			Factory.Save();
		}

		[ExpectNoExceptions()]
		public void TestTableThatShouldHaveDeletesBothBeforeAndAfterUpdates()
		{
			DummyBusinessObject dummyToDeleteBeforeUpdate = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			DummyBusinessObject dummyToDeleteAfterUpdate = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			DummyDependantBusinessObject dummyDependent = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			dummyDependent.ZD1_Z0 = dummyToDeleteAfterUpdate.PK;
			Factory.Save();

			dummyToDeleteAfterUpdate.Delete();
			dummyToDeleteBeforeUpdate.Delete();
			dummyDependent.ZD1_Z0 = ZGuid.Empty;
			Factory.Save();
		}

		#endregion

		#region Firing PersistentFactorySaving
		int PersistentFactorySavingCount;

		void HandleFactorySaving(BusinessObjectFactory factory)
		{
			PersistentFactorySavingCount++;
			AssertEquals(this.Factory, factory);
			AssertEquals("Transaction level", 2, Db.Connection.AppTransactionCount);
			AssertEquals("Called before OnFactorySaving in BizOs", false, Counter.OnFactorySavingCalled);
		}

		DummyCounter Counter;
		public void TestPersistentFactorySavingEventFired()
		{
			Factory.Saving += new BusinessObjectFactory.SavingEventHandler(HandleFactorySaving);
			AssertEquals("Transaction level", 1, Db.Connection.AppTransactionCount);
			AssertEquals("Event count", 0, PersistentFactorySavingCount);
			Counter = (DummyCounter)Factory.New(typeof(DummyCounter));
			Factory.Save();
			AssertEquals("Event count", 1, PersistentFactorySavingCount);
		}

		#endregion

		#region Calling OnSavingByFactory, OnSavedByFactory, OnSaving and OnSaved On BizOs

		public void TestOnSavingCallOrder()
		{
			SaveCounter.SavingCount = 0;
			DummyDependentCounter child = (DummyDependentCounter)Factory.New(typeof(DummyDependentCounter));
			DummyCounter parent = (DummyCounter)Factory.New(typeof(DummyCounter));
			DummyPivotCounter pivot = (DummyPivotCounter)Factory.New(typeof(DummyPivotCounter));

			pivot.ZDP_Z0 = parent.PK;
			pivot.ZDP_ZD1 = child.PK;
			child.ZD1_Z0 = parent.PK;

			Factory.Save();

			AssertEquals("Save order", 0, parent.SavingOrderCounter);
			AssertEquals("Save order", 1, child.SavingOrderCounter);
			AssertEquals("Save order", 2, pivot.SavingOrderCounter);
		}

		public void TestOnSavedCallOrder()
		{
			SaveCounter.SavedCount = 0;
			DummyDependentCounter child = (DummyDependentCounter)Factory.New(typeof(DummyDependentCounter));
			DummyCounter parent = (DummyCounter)Factory.New(typeof(DummyCounter));
			DummyPivotCounter pivot = (DummyPivotCounter)Factory.New(typeof(DummyPivotCounter));

			pivot.ZDP_Z0 = parent.PK;
			pivot.ZDP_ZD1 = child.PK;
			child.ZD1_Z0 = parent.PK;

			Factory.Save();

			AssertEquals("Save order", 0, parent.SavedOrderCounter);
			AssertEquals("Save order", 1, child.SavedOrderCounter);
			AssertEquals("Save order", 2, pivot.SavedOrderCounter);
		}

		public void TestOnSavedSuccessReporting()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			AssertEquals("initial state", false, dummy.OnSavedCalled);
			AssertEquals("initial state", false, dummy.SavedSucceded);

			Factory.Save();
			AssertEquals("Save should have succeded", true, dummy.SavedSucceded);

			dummy.Z0_Description = "need to have some changes so we'll be saved";
			bool gotException = false;
			try
			{
				BusinessObjectFactory.SaveTogether(Factory, new SaveAlwaysFailsFactory());
			}
			catch { gotException = true; }
			AssertEquals("Should have got save exception", true, gotException);

			AssertEquals("Save should have failed", false, dummy.SavedSucceded);
		}

		public void TestOnFactorySavedSuccessReporting()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			Factory.Save();
			dummy.OnFactorySavedCalled = false;
			dummy.FactorySavedSucceded = false;
			AssertEquals("initial state", false, dummy.HasChanges);

			Factory.Save();
			AssertEquals("Save should have succeded", true, dummy.FactorySavedSucceded);

			bool gotException = false;
			try
			{
				BusinessObjectFactory.SaveTogether(Factory, new SaveAlwaysFailsFactory());
			}
			catch { gotException = true; }
			AssertEquals("Should have got save exception", true, gotException);

			AssertEquals("Save should have failed", false, dummy.FactorySavedSucceded);
		}

		public void TestRollbackWithMultipleParticipants()
		{
			var dummy = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			dummy.FactorySavedSucceded = false;
			Factory.Save();
			Factory.Saved += Factory_SavedForMultipleParticipantsTest;
			Factory.SaveInTransactionActions.Add(new DummyAction());

			bool gotException = false;
			try
			{
				Factory.Save();
			}
			catch { gotException = true; }

			AssertEquals("Should have got save exception", true, gotException);
			AssertEquals("Save should have failed", false, dummy.FactorySavedSucceded);
			AssertEquals("Shouldn't throw pre-exsiting transaction exception when rolling back transactions", typeof(NotImplementedException), ExceptionReporterTestListener.Instance[0].InnerException.GetType());
			ExceptionReporterTestListener.Instance.Clear();
		}

		void Factory_SavedForMultipleParticipantsTest(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			factory.Saved -= Factory_SavedForMultipleParticipantsTest;
			factory.Save();
		}

		public void TestAddSqlLockToTransaction()
		{
			var dummy = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			SqlApplicationLock lockie = null;
			Db.Connection.TryGetLock("adfghjk", out lockie);
			AssertEquals("No locks", 0, Factory.SqlLockCount);
			Factory.AddSqlLockToTransaction(lockie);
			AssertEquals("Lock added", 1, Factory.SqlLockCount);
			Factory.Save();
			AssertEquals("Locks disposed on commit", 0, Factory.SqlLockCount);
			Assert(lockie.IsDisposed);

			Db.Connection.TryGetLock("adfghjk", out lockie);
			Factory.AddSqlLockToTransaction(lockie);
			AssertEquals("Lock added", 1, Factory.SqlLockCount);
			Factory.ReleaseSqlLocks();
			AssertEquals("Locks disposed on explicit release call", 0, Factory.SqlLockCount);
			Assert(lockie.IsDisposed);

			Db.Connection.TryGetLock("adfghjk", out lockie);
			Factory.AddSqlLockToTransaction(lockie);
			AssertEquals("Lock added", 1, Factory.SqlLockCount);

			Factory.Saved += Factory_SavedForMultipleParticipantsTest;
			Factory.SaveInTransactionActions.Add(new DummyAction());

			bool gotException = false;
			try
			{
				Factory.Save();
			}
			catch { gotException = true; }
			AssertEquals("Should have got save exception", true, gotException);
			AssertEquals("Save should have failed", false, dummy.FactorySavedSucceded);
			ExceptionReporterTestListener.Instance.Clear();
			AssertEquals("Locks disposed on rollback", 0, Factory.SqlLockCount);
			Assert(lockie.IsDisposed);
		}

		public void TestDoNotCallOnSavingAndOnSavedOnBizOsWithNoChanges()
		{
			DummyCounter dummy1 = (DummyCounter)Factory.New(typeof(DummyCounter));

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyCounter dummy2 = (DummyCounter)factory2.New(typeof(DummyCounter));
			DummyCounter dummy1Reloaded = (DummyCounter)factory2.Load(typeof(DummyCounter), dummy1.PK);

			AssertEquals("initial state", false, dummy1Reloaded.OnSavedCalled);
			AssertEquals("initial state", false, dummy1Reloaded.OnSavingCalled);
			AssertEquals("initial state", false, dummy2.OnSavedCalled);
			AssertEquals("initial state", false, dummy2.OnSavingCalled);

			factory2.Save();

			AssertEquals("Dummy1Reloaded has no changes so should have OnSaving called", false, dummy1Reloaded.OnSavingCalled);
			AssertEquals("Dummy1Reloaded has no changes so should have OnSaved called", false, dummy1Reloaded.OnSavedCalled);

			AssertEquals("Dummy2 should have OnSaving called", true, dummy2.OnSavingCalled);
			AssertEquals("Dummy2 OnSaved called", true, dummy2.OnSavedCalled);
		}

		public void TestCallOnFactorySavingOnAllBizOs()
		{
			DummyCounter dummy1 = (DummyCounter)Factory.New(typeof(DummyCounter));

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyCounter dummy2 = (DummyCounter)factory2.New(typeof(DummyCounter));
			DummyCounter dummy1Reloaded = (DummyCounter)factory2.Load(typeof(DummyCounter), dummy1.PK);

			AssertEquals("initial state", false, dummy1Reloaded.OnFactorySavingCalled);
			AssertEquals("initial state", false, dummy2.OnFactorySavingCalled);

			factory2.Save();

			AssertEquals("Dummy2 should have OnFactorySaving called", true, dummy2.OnFactorySavingCalled);
			AssertEquals("Dummy1Reloaded should have OnFactorySaving called", true, dummy1Reloaded.OnFactorySavingCalled);

			dummy2.OnFactorySavingCalled = false;
			dummy1Reloaded.OnFactorySavingCalled = false;
			dummy2.Delete();
			factory2.Save();
			AssertEquals("Deleted Dummy2 should not have OnFactorySaving called", false, dummy2.OnFactorySavingCalled);
			AssertEquals("Not deleted, OnFactorySaving should be called", true, dummy1Reloaded.OnFactorySavingCalled);
		}

		public void TestOnFactorySavingBeforeTransaction_CalledForBizOsCreatedOrLoadedDuringOnFactorySavingBeforeTransaction()
		{
			bool newDummy2OnFactorySavingBeforeTransactionCalled = false;
			bool loadedDummy2OnFactorySavingBeforeTransactionCalled = false;
			ZGuid dummy2PK = ZGuid.Empty;

			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.FactorySavingBeforeTransaction += delegate
			{
				DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
				dummy2PK = dummy2.PK;
				dummy2.FactorySavingBeforeTransaction += delegate
				{ newDummy2OnFactorySavingBeforeTransactionCalled = true; };
			};
			Factory.Save();
			AssertEquals("OnFactorySaving called for dummy CREATED during OnFactorySaving", true, newDummy2OnFactorySavingBeforeTransactionCalled);

			DummyBusinessObject anotherDummy = Factory.New<DummyBusinessObject>();
			anotherDummy.FactorySavingBeforeTransaction += delegate
			{
				DummyBusinessObject dummy2 = Factory.Load<DummyBusinessObject>(dummy2PK);
				dummy2.FactorySavingBeforeTransaction += delegate
				{ loadedDummy2OnFactorySavingBeforeTransactionCalled = true; };
			};
			Factory.Save();
			AssertEquals("OnFactorySaving called for dummy LOADED during OnFactorySaving", true, loadedDummy2OnFactorySavingBeforeTransactionCalled);
		}

		public void TestOnFactorySaving_CalledForBizOsCreatedOrLoadedDuringOnFactorySaving()
		{
			bool newDummy2OnFactorySavingCalled = false;
			bool loadedDummy2OnFactorySavingCalled = false;
			ZGuid dummy2PK = ZGuid.Empty;

			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.FactorySaving += delegate
			{
				DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
				dummy2PK = dummy2.PK;
				dummy2.FactorySaving += delegate
				{ newDummy2OnFactorySavingCalled = true; };
			};
			Factory.Save();
			AssertEquals("OnFactorySaving called for dummy CREATED during OnFactorySaving", true, newDummy2OnFactorySavingCalled);

			DummyBusinessObject anotherDummy = Factory.New<DummyBusinessObject>();
			anotherDummy.FactorySaving += delegate
			{
				DummyBusinessObject dummy2 = Factory.Load<DummyBusinessObject>(dummy2PK);
				dummy2.FactorySaving += delegate
				{ loadedDummy2OnFactorySavingCalled = true; };
			};
			Factory.Save();
			AssertEquals("OnFactorySaving called for dummy LOADED during OnFactorySaving", true, loadedDummy2OnFactorySavingCalled);
		}

		public void TestCallOnFactorySavedOnAllBizOs()
		{
			DummyCounter dummy1 = (DummyCounter)Factory.New(typeof(DummyCounter));

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyCounter dummy2 = (DummyCounter)factory2.New(typeof(DummyCounter));
			DummyCounter dummy1Reloaded = (DummyCounter)factory2.Load(typeof(DummyCounter), dummy1.PK);

			AssertEquals("initial state", false, dummy1Reloaded.OnFactorySavedCalled);
			AssertEquals("initial state", false, dummy2.OnFactorySavedCalled);

			factory2.Save();

			AssertEquals("Dummy2 should have OnFactorySaved called", true, dummy2.OnFactorySavedCalled);
			AssertEquals("Dummy1Reloaded should have OnFactorySaved called", true, dummy1Reloaded.OnFactorySavedCalled);

			dummy2.OnFactorySavedCalled = false;
			dummy1Reloaded.OnFactorySavedCalled = false;
			dummy2.Delete();
			factory2.Save();
			AssertEquals("Deleted Dummy2 should not have OnFactorySaved called", false, dummy2.OnFactorySavedCalled);
			AssertEquals("Not deleted, OnFactorySaved should be called", true, dummy1Reloaded.OnFactorySavedCalled);
		}

		public void TestNotifyOfSaveBeforeAnyFactorySaveBegins_DefaultValueIsFalse()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			TestNotifyOfSaveBeforeAnyFactorySaveBegins(factory1, factory2, false);
		}

		public void TestNotifyOfSaveBeforeAnyFactorySaveBegins_True()
		{
			TestBusinessObjectFactory factory1 = new TestBusinessObjectFactory();
			TestBusinessObjectFactory factory2 = new TestBusinessObjectFactory();
			factory1.SetNotifyOfSaveBeforeAnyFactorySaveBegins(true);
			factory2.SetNotifyOfSaveBeforeAnyFactorySaveBegins(true);
			TestNotifyOfSaveBeforeAnyFactorySaveBegins(factory1, factory2, true);
		}

		public void TestNotifyOfSaveBeforeAnyFactorySaveBegins_False()
		{
			TestBusinessObjectFactory factory1 = new TestBusinessObjectFactory();
			TestBusinessObjectFactory factory2 = new TestBusinessObjectFactory();
			factory1.SetNotifyOfSaveBeforeAnyFactorySaveBegins(false);
			factory2.SetNotifyOfSaveBeforeAnyFactorySaveBegins(false);
			TestNotifyOfSaveBeforeAnyFactorySaveBegins(factory1, factory2, false);
		}

		void TestNotifyOfSaveBeforeAnyFactorySaveBegins(BusinessObjectFactory factory1, BusinessObjectFactory factory2, bool notifyOfSaveBeforeAnyFactorySaveBegins)
		{
			DummyForNotifyOfSaveBeforeAnyFactorySaveBegins dummyInFactory1 = factory1.New<DummyForNotifyOfSaveBeforeAnyFactorySaveBegins>();
			DummyForNotifyOfSaveBeforeAnyFactorySaveBegins dummyInFactory2 = factory2.New<DummyForNotifyOfSaveBeforeAnyFactorySaveBegins>();
			dummyInFactory1.OtherDummy = dummyInFactory2;
			dummyInFactory2.OtherDummy = dummyInFactory1;
			BusinessObjectFactory.SaveTogether(factory1, factory2);

			if (notifyOfSaveBeforeAnyFactorySaveBegins)
			{
				AssertEquals("1st factory should not yet be saved before OnFactorySaving of the 2nd factory bizo", false, dummyInFactory2.IsOtherDummyInDBOnFactorySaving);
				AssertEquals("1st factory should not yet be saved before OnSaving of the 2nd factory bizo", false, dummyInFactory2.IsOtherDummyInDBOnSaving);
			}
			else
			{
				AssertEquals("1st factory should be saved before OnFactorySaving of the 2nd factory bizo", true, dummyInFactory2.IsOtherDummyInDBOnFactorySaving);
				AssertEquals("1st factory should be saved before OnSaving of the 2nd factory bizo", true, dummyInFactory2.IsOtherDummyInDBOnSaving);
			}
		}

		class DummyForNotifyOfSaveBeforeAnyFactorySaveBegins : DummyBusinessObject
		{
			public DummyForNotifyOfSaveBeforeAnyFactorySaveBegins(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DummyForNotifyOfSaveBeforeAnyFactorySaveBegins OtherDummy;

			public bool IsOtherDummyInDBOnFactorySaving;
			protected override void OnFactorySaving()
			{
				base.OnFactorySaving();
				IsOtherDummyInDBOnFactorySaving = IsOtherDummyInDB;
			}

			public bool IsOtherDummyInDBOnSaving;
			public override void OnSaving()
			{
				base.OnSaving();
				IsOtherDummyInDBOnSaving = IsOtherDummyInDB;
			}

			bool IsOtherDummyInDB
			{
				get { return new BusinessObjectFactory().Load<DummyForNotifyOfSaveBeforeAnyFactorySaveBegins>(OtherDummy.PK) != null; }
			}
		}

		class SaveAlwaysFailsFactory : ITransactionParticipant
		{
			#region ITransactionParticipant Members

			public ITransactionManager BeginTransactionWithManager()
			{
				return new StubTransactionManager();
			}

			public void OnAllTransactionsBeginning()
			{
			}

			public void OnAllTransactionsCommitted(IChangedTableNames changedTableNames)
			{
			}

			public void OnAllTransactionsRolledBack()
			{
			}

			public IChangedTableNames SaveInTransaction()
			{
				throw new ApplicationException();
			}

			public bool IsInTransaction
			{
				get { return false; }
			}

			ITransactionParticipant[] ITransactionParticipant.ChildParticipants
			{
				get { return Array.Empty<ITransactionParticipant>(); }
			}

			bool ITransactionParticipant.AllowTransactionWithOtherParticipant => false;

			IEnumerable<ISqlApplicationLock> ITransactionParticipant.TransactionLocks => Enumerable.Empty<ISqlApplicationLock>();

			#endregion
		}

		class SaveCounter
		{
			public static int SavingCount;
			public static int SavedCount;
		}

		class DummyCounter : DummyBusinessObject
		{
			public DummyCounter(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public int SavedOrderCounter = -1;
			public int SavingOrderCounter = -1;

			public override void OnSaving()
			{
				base.OnSaving();
				SavingOrderCounter = BusinessObjectFactoryTest.SaveCounter.SavingCount++;
			}

			public override void OnSaved(bool saveSucceeded)
			{
				base.OnSaved(saveSucceeded);
				SavedOrderCounter = BusinessObjectFactoryTest.SaveCounter.SavedCount++;
			}
		}

		class DummyDependentCounter : DummyDependantBusinessObject
		{
			public DummyDependentCounter(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public int SavedOrderCounter = -1;
			public int SavingOrderCounter = -1;

			public override void OnSaving()
			{
				base.OnSaving();
				SavingOrderCounter = BusinessObjectFactoryTest.SaveCounter.SavingCount++;
			}

			public override void OnSaved(bool saveSucceeded)
			{
				base.OnSaved(saveSucceeded);
				SavedOrderCounter = BusinessObjectFactoryTest.SaveCounter.SavedCount++;
			}
		}

		class DummyPivotCounter : DummyPivot
		{
			public DummyPivotCounter(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public int SavedOrderCounter = -1;
			public int SavingOrderCounter = -1;

			public override void OnSaving()
			{
				base.OnSaving();
				SavingOrderCounter = BusinessObjectFactoryTest.SaveCounter.SavingCount++;
			}

			public override void OnSaved(bool saveSucceeded)
			{
				base.OnSaved(saveSucceeded);
				SavedOrderCounter = BusinessObjectFactoryTest.SaveCounter.SavedCount++;
			}
		}

		#endregion

		#region PersistentChangingDummy

		class PersistentChangingDummy : DummyBusinessObject
		{
			public PersistentChangingDummy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			internal bool fIsSavedByFactory = true;

			public override bool IsSavedByFactory
			{
				get { return fIsSavedByFactory; }
			}
		}

		#endregion

		public void TestSmallDateTimeInsertAndUpdate()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = factory1.NewWithValidTestData<DummyBusinessObject>();

			dummy1.Z0_SmallDateTime = new ZDateTime(2004, 1, 1, 11, 59, 59);
			factory1.Save();

			dummy1.Z0_VarCharMax = "Noodle"; // make a change so it will be saved

			factory1.Save(); // if there's truncation / rounding problems between insert and update this will blow up

			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			DummyBusinessObject dummy2 = factory2.Load<DummyBusinessObject>(dummy1.PK);

			AssertEquals("Date has been truncated on insert", new ZDateTime(2004, 1, 1, 11, 59, 00), dummy2.Z0_SmallDateTime);

			dummy2.Z0_SmallDateTime = new ZDateTime(2004, 2, 2, 11, 59, 59);
			factory2.Save();

			BusinessObjectFactory factory3 = new BusinessObjectFactory();

			DummyBusinessObject dummy3 = factory3.Load<DummyBusinessObject>(dummy1.PK);

			AssertEquals("Date has been truncated on update", new ZDateTime(2004, 2, 2, 11, 59, 00), dummy3.Z0_SmallDateTime);
		}

		public void TestDateTimeTruncationWithSaveAndLoad()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = factory1.NewWithValidTestData<DummyBusinessObject>();

			dummy1.Z0_NVarChar = "Text";
			dummy1.Z0_SmallDateTime = new ZDateTime(2004, 1, 1, 11, 59, 59);
			factory1.Save();

			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_SmallDateTime, dummy1.Z0_SmallDateTime);//new ZDateTime(2004, 1, 1, 11, 59, 59));
			filter.AddToFilter(DummyBizoSchema.Z0_NVarChar, dummy1.Z0_NVarChar);
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			DummyBusinessObject[] records = factory2.Load<DummyBusinessObject>(filter);

			AssertEquals("Search with non-truncated value will find truncated value in DB", 1, records.Length);

			filter = new ZQuery(DummyBizoSchema.Z0_SmallDateTime, new ZDateTime(2004, 1, 1, 11, 59, 00));
			filter.AddToFilter(DummyBizoSchema.Z0_NVarChar, dummy1.Z0_NVarChar);
			BusinessObjectFactory factory3 = new BusinessObjectFactory();

			records = factory3.Load<DummyBusinessObject>(filter);

			AssertEquals("Search with truncated value will find truncated value in DB", 1, records.Length);
		}

		public void TestQuoteEscaping()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = (DummyBusinessObject)factory1.New(typeof(DummyBusinessObject));
			dummy1.Z0_Description = "I '''Have 'lots of '";
			dummy1.Z0_Code = "''";
			factory1.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummy2 = (DummyBusinessObject)factory2.Load(typeof(DummyBusinessObject), dummy1.PK);
			AssertEquals("Quotes OK", dummy1.Z0_Description, dummy2.Z0_Description);
			AssertEquals("Quotes OK", dummy1.Z0_Code, dummy2.Z0_Code);

			dummy2.Z0_Description = "Yeah ''''''' \" '' ";
			dummy2.Z0_Code = "''''";
			factory2.Save();

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			DummyBusinessObject dummy3 = (DummyBusinessObject)factory3.Load(typeof(DummyBusinessObject), dummy1.PK);
			AssertEquals("Quotes OK", dummy2.Z0_Description, dummy3.Z0_Description);
			AssertEquals("Quotes OK", dummy2.Z0_Code, dummy3.Z0_Code);
		}

		public void TestUpdateWithDecimalWithScaleOfZero()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = (DummyBusinessObject)factory1.New(typeof(DummyBusinessObject));
			dummy1.Z0_Decimal = new ZDecimal(5.823456789);
			factory1.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummy2 = (DummyBusinessObject)factory2.Load(typeof(DummyBusinessObject), dummy1.PK);
			AssertEquals("Should have truncated value to 0dp", new ZDecimal(6), dummy2.Z0_Decimal);

			dummy1.Z0_Decimal = new ZDecimal(1.845345345);
			factory1.Save();
			// if scale is not taken into account, this will fail as 5.823456789 will not be truncated when put in param
			// in the where section of the update clause.

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			DummyBusinessObject dummy3 = (DummyBusinessObject)factory3.Load(typeof(DummyBusinessObject), dummy1.PK);
			AssertEquals("Should have truncated value to 0dp", new ZDecimal(2), dummy3.Z0_Decimal);
		}

		public void TestUpdateOfDecimalWithNonZeroScale()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = (DummyBusinessObject)factory1.New(typeof(DummyBusinessObject));
			dummy1.Z0_AnotherDecimal = new ZDecimal(5.823856789);
			factory1.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummy2 = (DummyBusinessObject)factory2.Load(typeof(DummyBusinessObject), dummy1.PK);
			AssertEquals("Should have truncated value to 3dp", new ZDecimal(5.824), dummy2.Z0_AnotherDecimal);

			dummy1.Z0_AnotherDecimal = new ZDecimal(1.845845345);
			factory1.Save();
			// if scale is not taken into account, this will fail as 5.823856789 will not be truncated when put in param
			// in the where section of the update clause.

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			DummyBusinessObject dummy3 = (DummyBusinessObject)factory3.Load(typeof(DummyBusinessObject), dummy1.PK);
			AssertEquals("Should have truncated value to 3dp", new ZDecimal(1.846), dummy3.Z0_AnotherDecimal);
		}

		public void TestDecimalRoundingWithSaveAndLoad()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = (DummyBusinessObject)factory1.New(typeof(DummyBusinessObject));
			dummy1.Z0_AnotherDecimal = new ZDecimal(5.823456789);
			factory1.Save();

			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_AnotherDecimal, new ZDecimal(5.823456789));
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject[] dummies = (DummyBusinessObject[])factory2.Load(typeof(DummyBusinessObject), filter);
			AssertEquals("Search with non-truncated value will find truncated value in DB", 1, dummies.Length);

			filter = new ZQuery(DummyBizoSchema.Z0_AnotherDecimal, new ZDecimal(5.823));
			dummies = (DummyBusinessObject[])factory2.Load(typeof(DummyBusinessObject), filter);
			AssertEquals("Search with truncated value will find record", 1, dummies.Length);
			DummyBusinessObject dummy2 = dummies[0];
			AssertEquals("Value truncated", new ZDecimal(5.823), dummy2.Z0_AnotherDecimal);
		}

		[ExpectNoExceptions]
		public void TestSavingWithChangesThatCancelThemselvesOut()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			DummyBusinessObject dummy1 = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
			DummyBusinessObject dummy2 = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));

			dummy1.Z0_Decimal = 1;
			dummy2.Z0_Decimal = 1;

			factory.Save();

			dummy1.Z0_Decimal = 42;
			dummy1.Z0_Decimal = 1; // revert back to original

			dummy2.Z0_Decimal = 42; // should be saved

			factory.Save();
		}

		public void TestRollbackDeletes()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummy = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
			factory.Save();

			Assert("Dummy not deleted", !dummy.IsDeleted);
			dummy.Delete();
			Assert("Dummy deleted", dummy.IsDeleted);
			((IBusinessObjectFactoryInternals)factory).Rollback();
			Assert("Dummy not deleted", !dummy.IsDeleted);
		}

		#region TestRollbackDeletesWithOnSaveRollback

		[ExpectNoExceptions]
		public void TestRollbackDeletesWithOnSaveRollback()
		{
			var dummy1 = Factory.New<DummyRollback>();
			var dummy2 = Factory.New<DummyRollback>();
			var dummy3 = Factory.New<DummyRollback>();
			dummy1.Z0_Code = "BAK";
			dummy2.Z0_Code = "BAK";
			dummy3.Z0_Code = "BAK";
			Factory.Save();

			dummy1.Delete();
			dummy2.Delete();
			dummy3.Delete();
			((IBusinessObjectFactoryInternals)Factory).Rollback();

			AssertEquals(3, dummy1.Dummies.Count);
			AssertEquals(3, dummy2.Dummies.Count);
			AssertEquals(3, dummy3.Dummies.Count);
		}

		class DummyRollback : DummyBusinessObject
		{
			public DummyRollback(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected internal override void OnSaveRollback()
			{
				base.OnSaveRollback();
				Dummies = new DummyRollbackCollection(Factory, new ZQuery(DummyBizoSchema.Z0_Code, "BAK"));
				Dummies.Load();
			}

			public DummyRollbackCollection Dummies { get; set; }
		}

		class DummyRollbackCollection : BusinessObjectCollection<DummyRollback>
		{
			public DummyRollbackCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter) { }
		}

		#endregion

		public void TestRollbackPropertyChanges()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummy = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
			dummy.Z0_Number = 5;
			factory.Save();

			dummy.Z0_Number = 10;
			AssertEquals("Value changed", 10, dummy.Z0_Number);

			((IBusinessObjectFactoryInternals)factory).Rollback();
			AssertEquals("Value rolled back", 5, dummy.Z0_Number);
		}

		public void TestRollbackNewRows()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummy = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
			Assert("Row is OK", DataUtils.IsDataInRowAccessible(dummy.Row));

			((IBusinessObjectFactoryInternals)factory).Rollback();
			Assert("New Row is blown away", !DataUtils.IsDataInRowAccessible(dummy.Row));
		}

		public void TestGetBackSameObjectEachLoadForSameRowAndType()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			DummyBusinessObject dummy = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
			DummyBusinessObject dummyReloaded = (DummyBusinessObject)factory.Load(typeof(DummyBusinessObject), dummy.PK);

			AssertEquals("Should be same object back again", dummy, dummyReloaded);
		}

		public void TestGetBackSameObjectEachNKLoadForSameRowAndType()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			DummyBusinessObject dummy = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
			dummy.Z0_Description = "BAABAA";
			DummyBusinessObject dummyReloaded = (DummyBusinessObject)factory.LoadFromNaturalKey(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description, dummy.Z0_Description);

			AssertEquals("Should be same object back again", dummy, dummyReloaded);
		}

		public void TestTwoBizOsAroundSameRowGetBackSame()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			DummyBusinessObject dummy = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
			DummyBaseBusinessObject dummyBase = (DummyBaseBusinessObject)factory.Load(typeof(DummyBaseBusinessObject), dummy.PK);

			DummyBusinessObject dummyReloaded = (DummyBusinessObject)factory.Load(typeof(DummyBusinessObject), dummy.PK);
			AssertEquals("Should be same object back again", dummy, dummyReloaded);

			DummyBaseBusinessObject dummyBaseReloaded = (DummyBaseBusinessObject)factory.Load(typeof(DummyBaseBusinessObject), dummy.PK);
			AssertEquals("Should be same object back again", dummyBase, dummyBaseReloaded);
		}

		public void TestTwoBizOsAroundSameRowCacheCount()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			DummyBusinessObject dummy = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
			AssertEquals("Cache number bizo", 1, ((IBusinessObjectFactoryInternals)factory).NumberOfBusinessObjects);

			DummyBaseBusinessObject dummyBase = (DummyBaseBusinessObject)factory.Load(typeof(DummyBaseBusinessObject), dummy.PK);
			AssertNotNull(dummyBase);
			AssertEquals("Cache number bizo", 2, ((IBusinessObjectFactoryInternals)factory).NumberOfBusinessObjects);

			factory.New(typeof(DummyBusinessObject));
			AssertEquals("Cache number bizo", 3, ((IBusinessObjectFactoryInternals)factory).NumberOfBusinessObjects);
		}

		public void TestNumberOfBusinessObjects()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			factory.New(typeof(DummyBusinessObject));
			factory.New(typeof(DummyBusinessObject));
			AssertEquals("Factory.NumberOfBusinessObjects", 2, ((IBusinessObjectFactoryInternals)factory).NumberOfBusinessObjects);
		}

		public void TestUsingAnotherDBConnection()
		{
			using (DbConnection connection = Db.NewExtraConnectionToMainDb())
			{
				try
				{
					connection.BeginTransaction();

					BusinessObjectFactory factory = new BusinessObjectFactory(connection);
					BusinessObject bubble = factory.New(typeof(DummyBusinessObject));
					factory.Save();

					BusinessObjectFactory factory2 = new BusinessObjectFactory(connection);
					BusinessObject bubbleReloaded = factory2.Load(typeof(DummyBusinessObject), bubble.PK);
					AssertNotNull("BizO should be saved and loaded using multiple connections", bubbleReloaded);
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestGetTableNameFromType()
		{
			AssertEquals(DummyBizoSchema.Constants.TableName, BusinessObjectFactory.GetTableNameFromType(typeof(DummyBusinessObject)));
		}

		public void TestGetTableNameFromType_InvalidType()
		{
			AssertExceptionThrown("Throw Exception when type is not a BusinessObject", typeof(ZException), delegate
			{
				BusinessObjectFactory.GetTableNameFromType(typeof(BusinessObjectFactory));
			});
		}

		public void TestGetTableNameFromType_InvalidTypeAndThrowException()
		{
			AssertExceptionThrown("Throw Exception when type is not a BusinessObject", typeof(ZException), delegate
			{
				BusinessObjectFactory.GetTableNameFromType(typeof(BusinessObjectFactory), true);
			});
		}

		public void TestGetTableNameFromType_InvalidTypeAndNotThrowException()
		{
			AssertEquals("return empty string when type is not a BusinessObject", null, BusinessObjectFactory.GetTableNameFromType(typeof(BusinessObjectFactory), false));
		}

		public void TestHasTableName()
		{
			AssertEquals(true, BusinessObjectFactory.HasTableName(typeof(DummyBusinessObject)));
			AssertEquals(false, BusinessObjectFactory.HasTableName(typeof(NonPersistentBusinessObject)));
			AssertEquals(false, BusinessObjectFactory.HasTableName(typeof(IDisposable)));
		}

		public void TestHasTableCode()
		{
			AssertEquals(true, BusinessObjectFactory.HasTableCode(typeof(DummyBusinessObject)));
			AssertEquals(true, BusinessObjectFactory.HasTableName(typeof(DummyBusinessObjectWithNoSchema)));
			AssertEquals(false, BusinessObjectFactory.HasTableCode(typeof(DummyBusinessObjectWithNoSchema)));
			AssertEquals(false, BusinessObjectFactory.HasTableCode(typeof(NonPersistentBusinessObject)));
			AssertEquals(false, BusinessObjectFactory.HasTableCode(typeof(IDisposable)));
		}

		class DummyBusinessObjectWithNoSchema : DummyBusinessObject
		{
			public DummyBusinessObjectWithNoSchema(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public new class Schema
			{
				public const string TableName = "Vasya";
			}
		}

		public void TestGetTableSchemaFromType()
		{
			ITableSchema schema = BusinessObjectFactory.GetTableSchemaFromType(typeof(DummyBusinessObject));
			AssertEquals("Schema is DummyBizoSchema", DummyBizoSchema.Instance, schema);
			AssertEquals("Schema TableName", DummyBizoSchema.Constants.TableName, schema.TableName);
		}

		public void TestGetDatabaseCount()
		{
			// clear out the factory of any dummy rows before testing the count
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			collection.Load();
			collection.RemoveAndDeleteAll();

			Factory.Save();
			AssertEquals("GetDatabaseCount()", 0, Factory.GetDatabaseCount(typeof(DummyBaseBusinessObject)));

			Factory.New(typeof(DummyBaseBusinessObject));
			Factory.Save();
			AssertEquals("GetDatabaseCount()", 1, Factory.GetDatabaseCount(typeof(DummyBaseBusinessObject)));
		}

		public void TestGetDatabaseCountWithWhereClause()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			collection.Load();
			collection.RemoveAndDeleteAll();

			Factory.Save();
			AssertEquals("GetDatabaseCount()", 0, Factory.GetDatabaseCount(typeof(DummyBaseBusinessObject)));
			DummyBusinessObject bizO = collection.AddNew();
			bizO.Z0_Number = 123;

			ZQuery filterToReturnZeroRows = new ZQuery(DummyBizoSchema.Z0_Number, 124);
			ZQuery filterToReturnOneRow = new ZQuery(DummyBizoSchema.Z0_Number, 123);
			AssertEquals(0, Factory.GetDatabaseCount(typeof(DummyBaseBusinessObject), filterToReturnZeroRows));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(DummyBaseBusinessObject), filterToReturnOneRow));
			Factory.Save();
			AssertEquals(1, Factory.GetDatabaseCount(typeof(DummyBaseBusinessObject), filterToReturnOneRow));
		}

		public void TestSettingBlobLoadsOriginalValue()
		{
			string firstText = new string('1', 1050);
			const string secondText = "text2";

			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_VarCharMax = firstText;
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject bizOInFactory2 = factory2.Load<DummyBusinessObject>(bizO.PK);
			AssertEquals(1, factory2.DatabaseLoadCount);
			bizOInFactory2.Z0_VarCharMax = secondText;
			AssertEquals(2, factory2.DatabaseLoadCount);
			AssertEquals(firstText, bizOInFactory2.Z0_VarCharMaxInfo.OriginalValue);
			AssertEquals(secondText, bizOInFactory2.Z0_VarCharMax);
		}

		public void TestGetDatabaseCountWithFilterAndOrderByStatement()
		{
			TestGetDatabaseCountWithFilter(true);
		}

		public void TestGetDatabaseCountWithFilter()
		{
			TestGetDatabaseCountWithFilter(false);
		}

		public void TestGetDatabaseCountWithNoResultsFilter()
		{
			ZQuery noResultsFilter = new ZQuery();
			noResultsFilter.IsNoResultQuery = true;

			// clear out the factory of any dummy rows before testing the count
			BusinessObject[] dummies = Factory.Load(typeof(DummyBaseBusinessObject), new ZQuery());
			foreach (BusinessObject dummy in dummies)
			{
				dummy.Delete();
			}
			Factory.Save();
			AssertEquals("GetDatabaseCount() with a normal ZQuery()", 0, Factory.GetDatabaseCount(typeof(DummyBaseBusinessObject), new ZQuery()));
			AssertEquals("GetDatabaseCount() with No Results ZQuery()", 0, Factory.GetDatabaseCount(typeof(DummyBaseBusinessObject), noResultsFilter));

			Factory.New(typeof(DummyBaseBusinessObject));
			Factory.Save();
			AssertEquals("GetDatabaseCount()", 1, Factory.GetDatabaseCount(typeof(DummyBaseBusinessObject), new ZQuery()));
			AssertEquals("GetDatabaseCount()", 0, Factory.GetDatabaseCount(typeof(DummyBaseBusinessObject), noResultsFilter));
		}

		void TestGetDatabaseCountWithFilter(bool includeOrderBy)
		{
			ZQuery filter = new ZQuery();
			if (includeOrderBy)
			{
				filter.OrderBy = AutoDummyBizo.Schema.Z0_Code;
			}

			// clear out the factory of any dummy rows before testing the count
			BusinessObject[] dummies = Factory.Load(typeof(DummyBaseBusinessObject), new ZQuery());
			foreach (BusinessObject dummy in dummies)
			{
				dummy.Delete();
			}
			Factory.Save();
			AssertEquals("GetDatabaseCount()", 0, Factory.GetDatabaseCount(typeof(DummyBaseBusinessObject), filter));

			filter.AddToFilter(DummyBizoSchema.Z0_Description, "moo");
			DummyBaseBusinessObject newBizO = (DummyBaseBusinessObject)Factory.New(typeof(DummyBaseBusinessObject));
			Factory.Save();
			AssertEquals("GetDatabaseCount()", 0, Factory.GetDatabaseCount(typeof(DummyBaseBusinessObject), filter));

			newBizO.Z0_Description = "moo";
			Factory.Save();
			AssertEquals("GetDatabaseCount()", 1, Factory.GetDatabaseCount(typeof(DummyBaseBusinessObject), filter));
		}

		public void TestGetDatabaseCountUsesActiveFilter()
		{
			ZQuery filter = new ZQuery();
			TestCaseHelper.ClearTable(DummyBizoSchema.Constants.TableName);
			DummyBusinessObjectWithActiveFilter dummy1 = Factory.New<DummyBusinessObjectWithActiveFilter>();
			dummy1.IsActive = true;
			DummyBusinessObjectWithActiveFilter dummy2 = Factory.New<DummyBusinessObjectWithActiveFilter>();
			dummy2.IsActive = false;
			DummyBusinessObjectWithActiveFilter dummy3 = Factory.New<DummyBusinessObjectWithActiveFilter>();
			dummy3.IsActive = false;

			Factory.Save();

			DummyBusinessObjectWithActiveFilterCollection collection = new DummyBusinessObjectWithActiveFilterCollection(Factory);

			collection.Load(filter);

			AssertEquals("Collection should load only active records", 1, collection.Count);
			AssertEquals("GetDatabaseCount with Filter should not count inactive records", 1, Factory.GetDatabaseCount(typeof(DummyBusinessObjectWithActiveFilter), filter));
			AssertEquals("GetDatabaseCount without Filter should count inactive records", 3, Factory.GetDatabaseCount(typeof(DummyBusinessObjectWithActiveFilter)));

			filter.IgnoreActiveFilter = ZBool.True;

			collection.Load(filter);

			AssertEquals("Collection should load all active records", 3, collection.Count);
			AssertEquals("GetDatabaseCount with Filter should count inactive records", 3, Factory.GetDatabaseCount(typeof(DummyBusinessObjectWithActiveFilter), filter));
			AssertEquals("GetDatabaseCount without Filter should count inactive records", 3, Factory.GetDatabaseCount(typeof(DummyBusinessObjectWithActiveFilter)));
		}

		public void TestContentsAsXMLForDebugging()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			dummy.Z0_Description = "ALI";

			string xMLDump = ((IBusinessObjectFactoryInternals)Factory).ContentsAsXMLForDebugging;
			AssertContains("<Z0_Description>ALI</Z0_Description>", xMLDump);
		}

		public void TestMarkNonPersistentNewRows()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();

			PersistentChangingDummy dummy1 = (PersistentChangingDummy)factory1.New(typeof(PersistentChangingDummy));
			PersistentChangingDummy dummy2 = (PersistentChangingDummy)factory1.New(typeof(PersistentChangingDummy));

			dummy1.fIsSavedByFactory = true;
			dummy2.fIsSavedByFactory = false;

			factory1.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			AssertNotNull(factory2.Load(typeof(PersistentChangingDummy), dummy1.PK));
			AssertNull(factory2.Load(typeof(PersistentChangingDummy), dummy2.PK));
		}

		public void TestMarkNonPersistentLoadedRows()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();

			PersistentChangingDummy dummy1 = (PersistentChangingDummy)factory1.New(typeof(PersistentChangingDummy));
			PersistentChangingDummy dummy2 = (PersistentChangingDummy)factory1.New(typeof(PersistentChangingDummy));

			dummy1.Z0_Number = 100;
			dummy2.Z0_Number = 100;

			dummy1.fIsSavedByFactory = true;
			dummy2.fIsSavedByFactory = true;

			factory1.Save();

			dummy1.fIsSavedByFactory = false;
			dummy1.Z0_Number = 500;
			dummy2.Z0_Number = 500;

			factory1.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			PersistentChangingDummy reloadedDummy1 = (PersistentChangingDummy)factory2.Load(typeof(PersistentChangingDummy), dummy1.PK);
			PersistentChangingDummy reloadedDummy2 = (PersistentChangingDummy)factory2.Load(typeof(PersistentChangingDummy), dummy2.PK);

			AssertEquals(100, reloadedDummy1.Z0_Number);
			AssertEquals(500, reloadedDummy2.Z0_Number);

			dummy1.fIsSavedByFactory = true;
			factory1.Save();

			BusinessObjectFactory factory3 = new BusinessObjectFactory();

			reloadedDummy1 = (PersistentChangingDummy)factory3.Load(typeof(PersistentChangingDummy), dummy1.PK);
			reloadedDummy2 = (PersistentChangingDummy)factory3.Load(typeof(PersistentChangingDummy), dummy2.PK);

			AssertEquals(500, reloadedDummy1.Z0_Number);
			AssertEquals(500, reloadedDummy2.Z0_Number);
		}

		public void TestNew()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			AssertEquals(typeof(DummyBusinessObject), dummy.GetType());
			AssertEquals(false, dummy.OnLoadedCalled);
		}

		public void TestNewMoq()
		{
			var dummy = Factory.NewMoq<DummyBusinessObject>();
			Assert(dummy.Object.GetType().IsSubclassOf(typeof(DummyBusinessObject)));
			Assert(dummy.CallBase);
		}

		public void TestNewMoq_FactoryLoadingByPKReturnsCorrectObject()
		{
			var dummyMoq = Factory.NewMoq<DummyBusinessObject>();
			var dummy = dummyMoq.Object;
			var dummyInFactory = Factory.Load<DummyBusinessObject>(dummy.PK);

			AssertSame(dummy, dummyInFactory);
		}

		public void TestNewMoq_FactoryLoadingByQueryReturnsCorrectObject()
		{
			var dummyMoq = Factory.NewMoq<DummyBusinessObject>();
			var dummy = dummyMoq.Object;
			dummy.Z0_Guid = ZGuid.BrettsGuid;
			var dummyInFactory = Factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Guid, ZGuid.BrettsGuid)).First();

			AssertSame(dummy, dummyInFactory);
		}

		public void TestNewWithPrimaryKeyT()
		{
			DummyBusinessObject dummy1 = Factory.NewWithPrimaryKey<DummyBusinessObject>(Guid.Empty);
			AssertEquals(typeof(DummyBusinessObject), dummy1.GetType());
			AssertEquals("Dummy PK empty?", false, dummy1.PK == ZGuid.Empty);

			Guid pk = Guid.NewGuid();
			DummyBusinessObject dummy2 = Factory.NewWithPrimaryKey<DummyBusinessObject>(pk);
			AssertEquals(typeof(DummyBusinessObject), dummy2.GetType());
			AssertEquals("Initialised Dummy PK", pk, dummy2.PK.ToGuid());
		}

		public void TestNew_ITypeDeciderContext()
		{
			var typeDeciderContextMock = new Mock<ITypeDeciderContext>();
			typeDeciderContextMock.Setup(k => k.Country).Returns("XX");
			var dummyBusinessObject = Factory.New<DummyDependantBusinessObject>(typeDeciderContextMock.Object);
			AssertEquals(typeof(DummyDependantBusinessObject.DummyDependantBusinessObjectXXCountry), dummyBusinessObject.GetType());
		}

		public void TestCanSetPropertiesOfNew()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			dummy.Z0_Description = "RarNoodle";
			AssertEquals("RarNoodle", dummy.Z0_Description);
		}

		public void TestLoadFromPK()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.Load(typeof(DummyBusinessObject), PK1);
			Assert(dummy.GetType() == typeof(DummyBusinessObject));
			AssertEquals("COMRADE", dummy.Z0_Description.Trim());
			AssertEquals(5, dummy.Z0_Number);
			AssertEquals(10, dummy.Z0_AnotherNumber);

			dummy = (DummyBusinessObject)Factory.Load(typeof(DummyBusinessObject), PK2);
			Assert(dummy.GetType() == typeof(DummyBusinessObject));
			AssertEquals("NOODLE", dummy.Z0_Description.Trim());
			AssertEquals(100, dummy.Z0_Number);
			AssertEquals(200, dummy.Z0_AnotherNumber);
		}

		public void TestLoadMoqTFromPK()
		{
			var mockDummy = Factory.LoadMoq<DummyBusinessObject>(PK1);
			var dummy = mockDummy.Object;
			AssertNotEquals("Dummy.Type", typeof(DummyBusinessObject), dummy.GetType());
			AssertEquals("COMRADE", dummy.Z0_Description.Trim());
			AssertEquals(5, dummy.Z0_Number);
			AssertEquals(10, dummy.Z0_AnotherNumber);
			AssertSame(dummy, Factory.Load<DummyBusinessObject>(PK1));

			mockDummy = Factory.LoadMoq<DummyBusinessObject>(PK2);
			dummy = mockDummy.Object;
			AssertNotEquals("Dummy.Type", typeof(DummyBusinessObject), dummy.GetType());
			AssertEquals("NOODLE", dummy.Z0_Description.Trim());
			AssertEquals(100, dummy.Z0_Number);
			AssertEquals(200, dummy.Z0_AnotherNumber);
			AssertSame(dummy, Factory.Load<DummyBusinessObject>(PK2));
		}

		public void TestLoadMoqTFromPK_SameFactory()
		{
			var businessObject = Factory.Load<DummyBusinessObject>(PK2);
			var exception = AssertExceptionThrown<InvalidOperationException>(() => Factory.LoadMoq<DummyBusinessObject>(PK2));
			AssertEquals("The requested BusinessObject (CargoWise.EntityFramework.Testing.DummyBusinessObject) already exists in the factory and cannot be reloaded as a Mock.", exception.Message);
		}

		public void TestLoadAndNewMoqFactorySave()
		{
			var mockDummy = Factory.NewMoq<DummyBusinessObject>();
			var mockDummyPk = mockDummy.Object.PK;
			mockDummy.Object.Z0_Description = "Test";

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var loadedMockDummy = otherFactory.LoadMoq<DummyBusinessObject>(mockDummyPk).Object;
			AssertEquals("Test", loadedMockDummy.Z0_Description);
		}

		public void TestLoadFromPK_NonPersistentBusinessObject()
		{
			var factory1 = new BusinessObjectFactory();
			var dummy1 = new DummyNonPersistentBusinessObject(factory1);

			factory1.Save();

			var factory2 = new BusinessObjectFactory();

			var loadedDummy = factory2.Load<DummyNonPersistentBusinessObject>(dummy1.PK);
			AssertNull("Should not load dummy, as no strategy found.", loadedDummy);

			var mockLoadStrategy = new Mock<IBusinessObjectLoadStrategy>();

			var dummy2 = new DummyNonPersistentBusinessObject(factory1);

			mockLoadStrategy
				.Setup(m => m.Load(factory2, dummy1.PK))
				.Returns(dummy2);

			var loadStrategies = new Hashtable
			{
				{ typeof(DummyNonPersistentBusinessObject).FullName, new TestObjectHandle(mockLoadStrategy.Object) }
			};

			using (ObjectFactory.Substitute("BusinessObjectLoadStrategyList", loadStrategies))
			{
				loadedDummy = factory2.Load<DummyNonPersistentBusinessObject>(dummy1.PK);
				AssertNotNull("Should have loaded dummy.", loadedDummy);
				AssertEquals("Should have use strategy to load business object.", dummy2, loadedDummy);
			}
		}

		public void TestLoadUsingNoResultQuery()
		{
			DummyBusinessObject[] noDummies = (DummyBusinessObject[])Factory.Load(typeof(DummyBusinessObject), ZQuery.NoResultQuery);
			AssertEquals("NoResultQuery should return no results", 0, noDummies.Length);
		}

		public void TestLoadTop1UsingNoResultQuery()
		{
			DummyBusinessObject nullDummy = (DummyBusinessObject)Factory.LoadTop1(typeof(DummyBusinessObject), ZQuery.NoResultQuery);
			AssertNull("NoResultQuery should return no results", nullDummy);
		}

		public void TestSameMultiPartQueryLoadTop1IsCached()
		{
			ZQuery multiPartFilter = new ZQuery(DummyBizoSchema.Z0_Code, "A");
			multiPartFilter.AddToFilter(DummyBizoSchema.Z0_Decimal, 3);
			Factory.LoadTop1(typeof(DummyBusinessObject), multiPartFilter);
			AssertEquals(1, Factory.DatabaseLoadCount);
			Factory.LoadTop1(typeof(DummyBusinessObject), multiPartFilter);
			AssertEquals(1, Factory.DatabaseLoadCount);
		}

		public void TestSecondLoadTopOneDoesntPickupCachedFilterParameterFromPreviousLoadTop1()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			DummyBusinessObject newDummy1 = newFactory.New<DummyBusinessObject>();
			DummyBusinessObject newDummy2 = newFactory.New<DummyBusinessObject>();
			newDummy1.Z0_Bool = true;
			newDummy2.Z0_Bool = true;
			newFactory.Save();

			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Bool, "Y");
			BusinessObject dummy = Factory.LoadTop1<DummyBusinessObject>(filter);

			ZQuery anotherFilter = new ZQuery(DummyBizoSchema.PK, SQLComparisonOperator.NotEqual, dummy.PK);
			anotherFilter.AddToFilter(filter, JoinCondition.And);
			BusinessObject anotherDummy = Factory.LoadTop1<DummyBusinessObject>(anotherFilter);

			AssertNotNull("Should Return a second Supplier", anotherDummy);
		}

		public void TestLoadFromNaturalKeyUsingSchemaColumn()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.LoadFromNaturalKey(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description, new ZString("COMRADE"));
			Assert(dummy.GetType() == typeof(DummyBusinessObject));
			AssertEquals("COMRADE", dummy.Z0_Description.Trim());
			AssertEquals(5, dummy.Z0_Number);
			AssertEquals(10, dummy.Z0_AnotherNumber);

			dummy = (DummyBusinessObject)Factory.LoadFromNaturalKey(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description, new ZString("NOODLE"));
			Assert(dummy.GetType() == typeof(DummyBusinessObject));
			AssertEquals("NOODLE", dummy.Z0_Description.Trim());
			AssertEquals(100, dummy.Z0_Number);
			AssertEquals(200, dummy.Z0_AnotherNumber);
		}

		/// <summary>
		/// This test relies on the "IDummy" entries in EnterpriseApplicationConfiguration.xml (EnterpriseBusinessObjectPrefixTypes section).
		/// </summary>
		public void TestLoadUsingTableCodeAndPK()
		{
			AssertEquals(PK1, Factory.Load<DummyBaseBusinessObject>(DummyBizoSchema.Constants.Prefix, PK1).PK);
			AssertNull(Factory.Load<DummyBaseBusinessObject>(DummyBizoSchema.Constants.Prefix, ZGuid.NewZGuid()));
		}

		public void TestCanSetPropertiesOfLoaded()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.Load(typeof(DummyBusinessObject), PK1);
			dummy.Z0_Description = "RarNoodle";
			AssertEquals("RarNoodle", dummy.Z0_Description);
		}

		public void TestOnLoadedCalled()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.Load(typeof(DummyBusinessObject), PK1);
			AssertEquals("Loaded", true, dummy.OnLoadedCalled);
		}

		//		public void TestLoadDecimalEqualsZero()
		//		{
		//			DummyBusinessObject bizO = DummyBusinessObject.New(Factory);
		//			bizO.Z0_Code = "123";
		//			bizO.Z0_Decimal = 0m;
		//			Factory.Save();
		//			BusinessObjectFactory factory2 = new BusinessObjectFactory();
		//			ZQuery filter = new ZQuery();
		//			filter.AddToFilter(DummyBizoSchema.Z0_Decimal, bizO.Z0_Decimal);
		//			filter.AddToFilter(DummyBizoSchema.Z0_Code, bizO.Z0_Code);
		//			BusinessObject secondFactoryBizO = factory2.LoadTop1(typeof(DummyBusinessObject), filter);
		//			AssertEquals(bizO.PK, secondFactoryBizO.PK);
		//		}
		//
		//		public void TestLoadSQL()
		//		{
		//			DbCommand Cmd = Db.Connection.Command("INSERT INTO dbo.DummyBizo (Z0_PK, Z0_Guid, Z0_Decimal) VALUES (@Guid, @Guid, @Decimal)");
		//			Guid PK = Guid.NewGuid();
		//			Cmd.AddParameter("@Guid", SqlDbType.UniqueIdentifier, PK);
		//			Cmd.AddParameter("@Decimal", SqlDbType.Decimal, 0m);
		//			Cmd.ExecuteNonQuery();
		//
		//			Cmd = Db.Connection.Command("SELECT Count(*) FROM dbo.DummyBizo WHERE Z0_Guid = @Guid AND Z0_Decimal = @Decimal");
		//			Cmd.AddParameter("@Guid", SqlDbType.UniqueIdentifier, PK);
		//			Cmd.AddParameter("@Decimal", SqlDbType.Decimal, 0m);
		//			int Result = (int) Cmd.ExecuteScalar();
		//			AssertEquals("Failed to return row", 1, Result);
		//		}

		public void TestLoadTopN()
		{
			DummyTableCreator.AddRow(Guid.NewGuid(), "Goodo", 1, 1, Array.Empty<byte>());
			DummyTableCreator.AddRow(Guid.NewGuid(), "Goodo", 2, 1, Array.Empty<byte>());
			DummyTableCreator.AddRow(Guid.NewGuid(), "Goodo", 3, 1, Array.Empty<byte>());

			ZQuery sQLFilter;

			sQLFilter = new ZQuery(DummyBizoSchema.Z0_Description, "sdfasfasdf");
			sQLFilter.OrderBy = "Z0_Number";
			sQLFilter.MaximumRows = 10;
			BusinessObject[] dummies = Factory.Load(typeof(DummyBusinessObject), sQLFilter);
			AssertEquals(typeof(DummyBusinessObject[]), dummies.GetType());
			AssertEquals("Retrieve with non-existent Z0_Description", 0, dummies.Length);

			sQLFilter = new ZQuery(DummyBizoSchema.Z0_Description, "Goodo");
			sQLFilter.OrderBy = "Z0_Number";
			sQLFilter.MaximumRows = 2;
			dummies = Factory.Load(typeof(DummyBusinessObject), sQLFilter);
			AssertEquals("Should only get 2", 2, dummies.Length);

			sQLFilter.MaximumRows = 10;
			dummies = Factory.Load(typeof(DummyBusinessObject), sQLFilter);
			AssertEquals("Retrieve top 10 == retrieve all 3", 3, dummies.Length);
			AssertEquals(1, ((DummyBusinessObject)dummies[0]).Z0_Number);
			AssertEquals(2, ((DummyBusinessObject)dummies[1]).Z0_Number);
			AssertEquals(3, ((DummyBusinessObject)dummies[2]).Z0_Number);
		}

		public void TestLoadTop1WithSort()
		{
			DummyBusinessObject biz1 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			DummyBusinessObject biz2 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			DummyBusinessObject biz3 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));

			biz1.Z0_Code = "A";
			biz2.Z0_Code = "B";
			biz3.Z0_Code = "C";

			ZQuery sQLFilter = new ZQuery();
			sQLFilter.OrderBy = "Z0_Code";
			sQLFilter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.GreaterThanOrEqualTo, "A");
			sQLFilter.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.LessThanOrEqualTo, "C");
			DummyBusinessObject dummy = (DummyBusinessObject)(Factory).LoadTop1(typeof(DummyBusinessObject), sQLFilter);
			AssertEquals("Retreived Dummy Code", "A", dummy.Z0_Code);

			sQLFilter.OrderBy = "Z0_Code desc";
			dummy = (DummyBusinessObject)(Factory).LoadTop1(typeof(DummyBusinessObject), sQLFilter);
			AssertEquals("Retreived Dummy Code", "C", dummy.Z0_Code);
		}

		public void TestLoadTop1FromDBWithContainsClause1()
		{
			DummyBusinessObject biz1 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			biz1.Z0_VarCharMax = "ABBA";
			Factory.Save();
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_VarCharMax, SQLComparisonOperator.Contains, "ABBA");
			DummyBusinessObject biz2 = (DummyBusinessObject)secondFactory.LoadTop1(typeof(DummyBusinessObject), filter);
			AssertNotNull(biz2);
		}

		public void TestStupidBlobBug()
		{
			DummyBusinessObject biz1 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			biz1.Z0_VarCharMax = new string('L', 1050);
			Factory.Save();

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			DummyBusinessObject biz2 = (DummyBusinessObject)secondFactory.Load(typeof(DummyBusinessObject), biz1.PK);
			AssertNotNull("Precondition - the business object was loaded back in correctly (load by PK).", biz2);
			Assert("Precondition - the business object's blobs were not loaded in (load by PK).", LazyLoading.LoadRequired(biz2.Row[DummyBizoSchema.Constants.Z0_VarCharMax]));

			ZQuery pkAndBlobFilter = new ZQuery();
			pkAndBlobFilter.AddToFilter(DummyBizoSchema.PK, biz2.PK); // the RowFactory will try to be smart and use the cache because of the PK
			pkAndBlobFilter.AddToFilter(DummyBizoSchema.Z0_VarCharMax, SQLComparisonOperator.Equal, new string('L', 1050));

			DummyBusinessObject bizLoadedWithBlobFilter = (DummyBusinessObject)secondFactory.LoadTop1(typeof(DummyBusinessObject), pkAndBlobFilter);
			AssertNotNull(bizLoadedWithBlobFilter);
			AssertEquals(bizLoadedWithBlobFilter.PK, biz2.PK);
		}

		public void TestLoadTop1FromDBWithContainsClause2()
		{
			DummyBusinessObject biz1 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			biz1.Z0_VarCharMax = "ABBA";
			Factory.Save();
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_VarCharMax, SQLComparisonOperator.Contains, "BB");
			DummyBusinessObject biz2 = (DummyBusinessObject)secondFactory.LoadTop1(typeof(DummyBusinessObject), filter);
			AssertNotNull(biz2);
		}

		public void TestLoadTop1FromDBWithStartsWithClause()
		{
			DummyBusinessObject biz1 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			biz1.Z0_VarCharMax = "ABBA";
			Factory.Save();
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_VarCharMax, SQLComparisonOperator.StartsWith, "ABB");
			DummyBusinessObject biz2 = (DummyBusinessObject)secondFactory.LoadTop1(typeof(DummyBusinessObject), filter);
			AssertNotNull(biz2);
		}

		public void TestLoadTop1FromDBWithStartsWithClauseAfterAlreadyLoadingTheBizo()
		{
			DummyBusinessObject biz1 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			biz1.Z0_VarCharMax = "ABBA";
			Factory.Save();
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();

			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(secondFactory);
			collection.Load();

			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_VarCharMax, SQLComparisonOperator.StartsWith, "ABB");
			DummyBusinessObject biz2 = (DummyBusinessObject)secondFactory.LoadTop1(typeof(DummyBusinessObject), filter);
			AssertNotNull(biz2);
		}

		public void TestLoadWithFilter()
		{
			ZQuery sQLFilter;

			sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(DummyBizoSchema.Z0_Description, "sadfasfdasdf");
			BusinessObject[] dummies = Factory.Load(typeof(DummyBusinessObject), sQLFilter);
			AssertEquals(0, dummies.Length);

			sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(DummyBizoSchema.Z0_Description, "COMRADE");
			dummies = Factory.Load(typeof(DummyBusinessObject), sQLFilter);
			AssertEquals(1, dummies.Length);

			sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(DummyBizoSchema.Z0_Description, "NOODLE");
			dummies = Factory.Load(typeof(DummyBusinessObject), sQLFilter);
			AssertEquals(1, dummies.Length);

			DummyBusinessObject dummy = (DummyBusinessObject)dummies[0];
			Assert(dummy.GetType() == typeof(DummyBusinessObject));
			AssertEquals("NOODLE", dummy.Z0_Description.Trim());
			AssertEquals(100, dummy.Z0_Number);
			AssertEquals(200, dummy.Z0_AnotherNumber);

			sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(DummyBizoSchema.Z0_Description, "NOODLE");
			sQLFilter.AddToFilter(DummyBizoSchema.Z0_Number, SQLComparisonOperator.GreaterThan, 50);
			dummies = Factory.Load(typeof(DummyBusinessObject), sQLFilter);
			AssertEquals(1, dummies.Length);

			sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(DummyBizoSchema.Z0_Description, "NOODLE");
			sQLFilter.AddToFilter(DummyBizoSchema.Z0_Number, SQLComparisonOperator.LessThan, 50);
			dummies = Factory.Load(typeof(DummyBusinessObject), sQLFilter);
			AssertEquals(0, dummies.Length);

			dummies = Factory.Load(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Number, SQLComparisonOperator.GreaterThan, 1));
			AssertEquals(2, dummies.Length);
		}

		#region Load Geography Data

		public void TestLoadGeographyPopulated()
		{
			// Arrange
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.FillWithValidTestData();
			dummy.Z0_Geography = new ZGeography("POINT (-152.22 58.88 10.5)");
			Factory.Save();

			var factory = new BusinessObjectFactory();

			// Act
			var geo = factory.Load<DummyBusinessObject>(dummy.PK).Z0_Geography;

			// Assert
			AssertEquals(-152.22, geo.Longitude);
			AssertEquals(58.88, geo.Latitude);
			AssertEquals(10.5, geo.Elevation);
		}

		public void TestLoadGeographyEmpty()
		{
			// Arrange
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.FillWithValidTestData();
			dummy.Z0_Geography = ZGeography.Empty;
			Factory.Save();

			var factory = new BusinessObjectFactory();

			// Act
			var geo = factory.Load<DummyBusinessObject>(dummy.PK).Z0_Geography;

			// Assert
			AssertEquals(null, geo.Longitude);
			AssertEquals(null, geo.Latitude);
			AssertEquals(null, geo.Elevation);
		}

		#endregion

		public void TestNoExceptionWasThrownIfDbOnlyQueryCacheHasDetachedRows()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "ABC";
			factory.Save();

			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			var factory2 = new BusinessObjectFactory();
			factory2.RowFactory.Load("DummyBizo", query).ForEach(row => row.Table.Clear());

			AssertNoExceptionThrown(() => factory2.Load(typeof(DummyBusinessObject), query));
		}

		public void TestNoExceptionWasThrownIfZDataTableHasDetachedRowsWhenFactoryLoad()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "ABC";
			factory.Save();

			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			var factory2 = new BusinessObjectFactory();
			var rows = factory2.RowFactory.Load("DummyBizo", query);
			rows.ForEach(row => row.Delete());
			rows.ForEach(row => row.Table.Clear());

			AssertNoExceptionThrown(() => factory2.Load(typeof(DummyBusinessObject), query.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "ABC")));
		}

		public void TestNoExceptionWasThrownIfZDataTableHasDetachedRowsWhenFactoryLoad_ReLoadExistingRowsIsTrue()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "ABC";
			factory.Save();

			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.ReLoadExistingRows = true;
			var factory2 = new BusinessObjectFactory();
			var rows = factory2.RowFactory.Load("DummyBizo", query);
			rows.ForEach(row => row.Delete());
			rows.ForEach(row => row.Table.Clear());

			AssertNoExceptionThrown(() => factory2.Load(typeof(DummyBusinessObject), query));
		}

		public void TestReload()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.Load(typeof(DummyBusinessObject), PK1);
			AssertEquals("Loaded OK", "COMRADE", dummy.Z0_Description.Trim());

			string sql = "UPDATE " + DummyBizoSchema.Constants.TableName + " SET Z0_Description='TEAPOT'";
			Db.Connection.ExecuteNonQuery(sql);

			Factory.Reload(dummy, true);
			AssertEquals("Reloaded OK", "TEAPOT", dummy.Z0_Description.Trim());

			Factory.Reload(dummy, true);
			AssertEquals("Reloaded OK a second time", "TEAPOT", dummy.Z0_Description.Trim());

			sql = "DELETE FROM " + DummyBizoSchema.Constants.TableName;
			Db.Connection.ExecuteNonQuery(sql);
			Factory.Reload(dummy, true);
			AssertEquals("No change as record is not in the DB any more.", "TEAPOT", dummy.Z0_Description.Trim());
		}

		public void TestReloadSafe()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			Factory.Reload(dummy, false);
			AssertNotEquals("Argh", ErrorReporter.LastKeyReported);
		}

		public void TestReloadUnsafe()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			Factory.Reload(dummy, true);
			AssertEquals("Argh", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestReloadAll()
		{
			TestReloadAll(false);
		}

		public void TestReloadAllSafe()
		{
			TestReloadAll(true);
		}

		void TestReloadAll(bool useSafe)
		{
			DummyBusinessObject dummy2 = Factory.NewWithValidTestData<DummyBusinessObject2ForReloadAllTest>();
			DummyBusinessObject dummy3 = Factory.NewWithValidTestData<DummyBusinessObject2ForReloadAllTest>();
			dummy2.Z0_Description = "COMRADE2";
			dummy3.Z0_Description = "COMRADE3";
			Factory.Save();

			DummyBusinessObject dummy = (DummyBusinessObject)Factory.Load(typeof(DummyBusinessObject), PK1);
			AssertEquals("Loaded OK", "COMRADE", dummy.Z0_Description.Trim());

			string sql = "UPDATE " + DummyBizoSchema.Constants.TableName + " SET Z0_Description='TEAPOT'";
			Db.Connection.ExecuteNonQuery(sql);

			int dbHits = Factory.DatabaseLoadCount;
			if (!useSafe)
			{
				Factory.ReloadAll<DummyBusinessObject>();
			}
			else
			{
				Factory.ReloadAllSafe<DummyBusinessObject>();
			}

			AssertEquals("Reloaded OK", "TEAPOT", dummy.Z0_Description.Trim());
			AssertEquals("Should not be reloaded", "COMRADE2", dummy2.Z0_Description.Trim());
			AssertEquals("Should not be reloaded", "COMRADE3", dummy3.Z0_Description.Trim());
			AssertEquals("ReloadAll should have only hit the db once.", dbHits + 1, Factory.DatabaseLoadCount);

			dummy.Z0_Description = "COMRADE";
			dbHits = Factory.DatabaseLoadCount;

			if (!useSafe)
			{
				Factory.ReloadAll<DummyBusinessObject2ForReloadAllTest>();
			}
			else
			{
				Factory.ReloadAllSafe<DummyBusinessObject2ForReloadAllTest>();
			}
			AssertEquals("ReloadAll should have only hit the db once even though it's reloading 2 objects.", dbHits + 1, Factory.DatabaseLoadCount);

			AssertEquals("Should not be reloaded", "COMRADE", dummy.Z0_Description.Trim());
			AssertEquals("Reloaded OK", "TEAPOT", dummy2.Z0_Description.Trim());
			AssertEquals("Reloaded OK", "TEAPOT", dummy3.Z0_Description.Trim());

			sql = "DELETE FROM " + DummyBizoSchema.Constants.TableName;
			Db.Connection.ExecuteNonQuery(sql);

			if (!useSafe)
			{
				Factory.ReloadAll<DummyBusinessObject>();
				Factory.ReloadAll<DummyBusinessObject2ForReloadAllTest>();
			}
			else
			{
				Factory.ReloadAllSafe<DummyBusinessObject>();
				Factory.ReloadAllSafe<DummyBusinessObject2ForReloadAllTest>();
			}

			AssertEquals("No change as record is not in the DB any more.", "COMRADE", dummy.Z0_Description.Trim());
			AssertEquals("No change as record is not in the DB any more.", "TEAPOT", dummy2.Z0_Description.Trim());
			AssertEquals("No change as record is not in the DB any more.", "TEAPOT", dummy3.Z0_Description.Trim());
		}

		public void TestReloadAllSafeWithBizos()
		{
			DummyBusinessObject dummyBO1 = Factory.NewWithValidTestData<DummyBusinessObject2ForReloadAllTest>();
			dummyBO1.Z0_Description = "Dummy1";
			DummyBusinessObject dummyBO2 = Factory.NewWithValidTestData<DummyBusinessObject2ForReloadAllTest>();
			dummyBO2.Z0_Description = "Dummy2";
			Factory.Save();

			var dummyBOs = new List<DummyBusinessObject>();
			dummyBOs.Add(dummyBO1);
			dummyBOs.Add(dummyBO2);

			string sql = "UPDATE " + DummyBizoSchema.Constants.TableName + " SET Z0_Description='TEAPOT'";
			Db.Connection.ExecuteNonQuery(sql);

			int dbHits = Factory.DatabaseLoadCount;
			Factory.ReloadAllSafe(dummyBOs);
			AssertEquals("Should have only hit the db once", dbHits + 1, Factory.DatabaseLoadCount);
			AssertEquals("TEAPOT", dummyBO1.Z0_Description.Trim());
			AssertEquals("TEAPOT", dummyBO1.Z0_Description.Trim());
		}

		[StressTest]
		public void TestReloadAllSafeWithLargeNumber()
		{
			TestReloadAllWithLargeNumber(true);
		}

		[StressTest]
		public void TestReloadAllithLargeNumber()
		{
			TestReloadAllWithLargeNumber(false);
		}

		void TestReloadAllWithLargeNumber(bool useSafe)
		{
			List<DummyBusinessObject> dummies = new List<DummyBusinessObject>();
			for (int i = 0; i < 50000; i++)
			{
				DummyBusinessObject dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
				dummy.Z0_Description = "Dummy";
				dummies.Add(dummy);
			}
			Factory.Save();

			string sql = "UPDATE " + DummyBizoSchema.Constants.TableName + " SET Z0_Description='Smarty'";
			Db.Connection.ExecuteNonQuery(sql);

			int dbHits = Factory.DatabaseLoadCount;
			if (useSafe)
			{
				Factory.ReloadAllSafe<DummyBusinessObject>();
			}
			else
			{
				Factory.ReloadAll<DummyBusinessObject>();
			}

			foreach (var bizo in dummies)
			{
				AssertEquals("Each Dummy should be reloaded", "Smarty", bizo.Z0_Description);
			}

			int expectedHits = 50; //it loads 1000 per hit
			AssertEquals("ReloadAll should have only hit the db once.", dbHits + expectedHits, Factory.DatabaseLoadCount);
		}

		public void TestReloadAllIsNotSafe()
		{
			DummyBusinessObject dummy2 = Factory.NewWithValidTestData<DummyBusinessObject2ForReloadAllTest>();
			dummy2.Z0_Description = "COMRADE2";
			Factory.Save();

			DummyBusinessObject dummy3 = Factory.NewWithValidTestData<DummyBusinessObject2ForReloadAllTest>();
			dummy3.Z0_Description = "COMRADE3";

			Factory.ReloadAll<DummyBusinessObject2ForReloadAllTest>();
			AssertEquals("ReloadAll", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestReloadAllSafeIsSafe()
		{
			DummyBusinessObject dummy2 = Factory.NewWithValidTestData<DummyBusinessObject2ForReloadAllTest>();
			dummy2.Z0_Description = "COMRADE2";
			Factory.Save();

			DummyBusinessObject dummy3 = Factory.NewWithValidTestData<DummyBusinessObject2ForReloadAllTest>();
			dummy3.Z0_Description = "COMRADE3";

			Factory.ReloadAllSafe<DummyBusinessObject2ForReloadAllTest>();
			AssertNotEquals("ReloadAll", ErrorReporter.LastKeyReported);
		}

		public void TestImportingToItemArrayWithNoDbTouches_ShouldNotAccessDB()
		{
			var bizO = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizO.Z0_Code = "PNI";

			var result = (DummyBusinessObject)Factory.CreateNewFactory().ImportFromItemArray(bizO.GetType(), bizO.Row.ItemArray);

			AssertEquals("Accessing Database unnecessarily", 0, result.Factory.DatabaseLoadCount);
			AssertEquals("Row should actually import. Unfortunately, it is not.", bizO.Z0_Code, result.Z0_Code);
		}

		public void TestImportingToAnotherFactoryWithNoDbTouches_ShouldNotAccessDB()
		{
			var bizO = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizO.Z0_Code = "PNI";

			var result = (DummyBusinessObject)Factory.CreateNewFactory().ImportFromAnotherFactory(bizO, bizO.GetType());

			AssertEquals("Accessing Database unnecessarily", 0, result.Factory.DatabaseLoadCount);
			AssertEquals("Expect the row to actually import :)", bizO.Z0_Code, result.Z0_Code);
		}

		#region TestClearQueryCache

		public void TestClearQueryCache_ForSpecificTable()
		{
			var factory = new BusinessObjectFactory();
			var query1 = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "AAA");
			var query2 = new ZQuery(DummyDependentBizoSchema.ZD1_Code, SQLComparisonOperator.StartsWith, "BBB");

			int dBhits = factory.DatabaseLoadCount;
			factory.Load<DummyBusinessObject>(query1);
			factory.Load<DummyDependantBusinessObject>(query2);
			AssertEquals(dBhits + 2, factory.DatabaseLoadCount);

			factory.Load<DummyBusinessObject>(query1);
			factory.Load<DummyDependantBusinessObject>(query2);
			AssertEquals("Queries should be cached after 1-st run, no new DB hits should happen.", dBhits + 2, factory.DatabaseLoadCount);

			factory.ClearQueryCache(DummyBizoSchema.Constants.TableName);
			factory.Load<DummyBusinessObject>(query1);
			factory.Load<DummyDependantBusinessObject>(query2);
			AssertEquals("Cache for 1 query was cleared, only 1 additional DB hit expected.", dBhits + 3, factory.DatabaseLoadCount);
		}

		public void TestClearQueryCache_ForSpecificTable_DBOnlyQuery()
		{
			var factory = new BusinessObjectFactory();
			var query1 = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query1.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "AAA");
			var query2 = new ZDBOnlyQuery(typeof(DummyDependantBusinessObject));
			query2.AddToFilter(DummyDependentBizoSchema.ZD1_Code, SQLComparisonOperator.StartsWith, "BBB");

			int dBhits = factory.DatabaseLoadCount;
			factory.Load<DummyBusinessObject>(query1);
			factory.Load<DummyDependantBusinessObject>(query2);
			AssertEquals(dBhits + 2, factory.DatabaseLoadCount);

			factory.Load<DummyBusinessObject>(query1);
			factory.Load<DummyDependantBusinessObject>(query2);
			AssertEquals("Queries should be cached after 1-st run, no new DB hits should happen.", dBhits + 2, factory.DatabaseLoadCount);

			factory.ClearQueryCache(DummyBizoSchema.Constants.TableName);
			factory.Load<DummyBusinessObject>(query1);
			factory.Load<DummyDependantBusinessObject>(query2);
			AssertEquals("Cache for 1 query was cleared, only 1 additional DB hit expected.", dBhits + 3, factory.DatabaseLoadCount);
		}

		#endregion

		public void TestLoadWithConsistentViewOfDataSetAndDatabase()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			Assert(dummy != null);

			dummy = (DummyBusinessObject)Factory.Load(typeof(DummyBusinessObject), dummy.PK);
			AssertNotNull("Loaded from DataSet OK", dummy);
		}

		public void TestGetNullGivesNullObject()
		{
			BusinessObject bizO = Factory.GetNull(typeof(DummyBusinessObject));
			AssertEquals(true, bizO.IsNull);
		}

		public void TestGetNullTwiceGivesSameObject()
		{
			BusinessObject bizO1 = Factory.GetNull(typeof(DummyBusinessObject));
			BusinessObject bizO2 = Factory.GetNull(typeof(DummyBusinessObject));
			AssertEquals(bizO1, bizO2);
		}

		public void TestNullFactoryUseSameThreadSentry()
		{
			var dummyNull = Factory.GetNull(typeof(DummyBusinessObject));
			AssertEquals("NullObjectFactory should use the same ThreadSentry as the main Factory to work property with multiple threads.", Factory.ThreadSentry, dummyNull.Factory.ThreadSentry);
			AssertEquals(Factory.ThreadSentry, dummyNull.Factory.RowFactory.QueryCache.sentry);
			AssertEquals(Factory.ThreadSentry, dummyNull.Factory.RowFactory.dbOnlyQueryCache.sentry);
			AssertEquals(Factory.ThreadSentry, dummyNull.Factory.RowFactory.narrowDbOnlyQueryCache.sentry);
			AssertEquals(Factory.AllowChangingThreadOwnership, dummyNull.Factory.AllowChangingThreadOwnership);
			AssertEquals(Factory.CrossThreadErrorReportingEnabled, dummyNull.Factory.CrossThreadErrorReportingEnabled);
		}

		public void TestLoadAlwaysReturnsTheSameBusinessObjectForAGivenRow()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			Assert(dummy != null);

			BusinessObject loadedDummy = (DummyBusinessObject)Factory.Load(typeof(DummyBusinessObject), dummy.PK);
			AssertEquals(loadedDummy, dummy);
		}

		public void TestSave()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			AssertNotNull(dummy);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BusinessObject dummyReloaded = factory2.Load(typeof(DummyBusinessObject), dummy.PK);

			AssertNotNull(dummyReloaded);
		}

		public void TestSaveTogether()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = (DummyBusinessObject)factory1.New(typeof(DummyBusinessObject));
			AssertNotNull(dummy1);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummy2 = (DummyBusinessObject)factory2.New(typeof(DummyBusinessObject));
			AssertNotNull(dummy2);

			BusinessObjectFactory.SaveTogether(factory1, factory2);

			BusinessObjectFactory loader = new BusinessObjectFactory();

			BusinessObject dummy1Reloaded = loader.Load(typeof(DummyBusinessObject), dummy1.PK);
			AssertNotNull("Should be loadable form DB", dummy1Reloaded);

			BusinessObject dummy2Reloaded = loader.Load(typeof(DummyBusinessObject), dummy2.PK);
			AssertNotNull("Should be loadable form DB", dummy2Reloaded);
		}

		public void TestSavingDeletedBizOResetsHasChangesToFalse()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			Assert("Dummy.HasChanges", !dummy.HasChanges);

			dummy.Delete();
			Assert("Dummy.HasChanges", dummy.HasChanges);

			Factory.Save();
			Assert("Dummy.HasChanges", !dummy.HasChanges);
		}

		public void TestSuspendResumeValidation_OnBusinessObject()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			AssertEquals("Validation isn't suspended initially", false, dummy.IsValidationSuspended);
			Factory.SuspendValidation();
			Factory.SuspendValidation();
			AssertEquals("Validation should be suspended as it is on the factory", true, dummy.IsValidationSuspended);
			Factory.ResumeValidation();
			AssertEquals("Validation should be suspended as it is on the factory", true, dummy.IsValidationSuspended);
			Factory.ResumeValidation();
			AssertEquals("Validation should no longer be suspended", false, dummy.IsValidationSuspended);
		}

		public void TestSuspendResumeValidation_OnBusinessObjectCollection()
		{
			DummyBusinessObjectCollection dummy = new DummyBusinessObjectCollection(Factory);
			AssertEquals("Validation isn't suspended initially", false, dummy.IsValidationSuspended);
			Factory.SuspendValidation();
			Factory.SuspendValidation();
			AssertEquals("Validation should be suspended as it is on the factory", true, dummy.IsValidationSuspended);
			Factory.ResumeValidation();
			AssertEquals("Validation should be suspended as it is on the factory", true, dummy.IsValidationSuspended);
			Factory.ResumeValidation();
			AssertEquals("Validation should no longer be suspended", false, dummy.IsValidationSuspended);
		}

		public void TestGetBusinessObjectBaseTypeFromTablePrefixForNonSpringifiedTable()
		{
			var type = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix("SUC");
			AssertNotNull(type);
			Assert("Type should not be abstract", !type.IsAbstract);

			type = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix("TZ");
			AssertNotNull(type);
			Assert("Type should not be abstract", !type.IsAbstract);
		}

		#region FetchHints

		public void TestSeedQueryCache()
		{
			ZGuid guid1 = ZGuid.NewZGuid();
			Factory.SeedQueryCache(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Guid, guid1));
			Factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Guid, guid1));
			AssertEquals(0, Factory.DatabaseLoadCount);
			Factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Guid, ZGuid.NewZGuid()));
			AssertEquals(1, Factory.DatabaseLoadCount);
		}

		public void TestAddFetchHintForImmediateZQuery()
		{
			ZString value = "123";
			AssertExceptionThrown(typeof(InvalidOperationException), "Attempted to add a fetch hint for non-persistent business object.", () => Factory.AddFetchHint(typeof(DummyNonPersistentBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, value)));
			AssertEquals("Should be no fetch hints for non-persistent business objects.", 0, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
			Factory.AddFetchHint(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, value));
			AssertEquals(1, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
		}

		public void TestAddFetchHintForColumnForType()
		{
			ZString value = "123";
			Factory.AddFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code, ZGuid.Empty);
			AssertEquals(0, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
			AssertExceptionThrown(typeof(InvalidOperationException), "Attempted to add a fetch hint for non-persistent business object.", () => Factory.AddFetchHint(typeof(DummyNonPersistentBusinessObject), DummyBizoSchema.Z0_Code, value));
			AssertEquals("Should be no fetch hints for non-persistent business objects.", 0, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
			Factory.AddFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code, value);
			AssertEquals(1, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
		}

		public void TestAddFetchHintForColumnForTable()
		{
			ZString value = "123";
			Factory.AddFetchHint(DummyBizoSchema.Z0_Code, ZGuid.Empty);
			AssertEquals(0, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
			Factory.AddFetchHint(DummyBizoSchema.Z0_Code, value);
			AssertEquals(1, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
		}

		public void TestAddFetchHintForPKForType()
		{
			Factory.AddFetchHint(typeof(DummyBusinessObject), ZGuid.Empty);
			AssertEquals(0, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
			AssertExceptionThrown(typeof(InvalidOperationException), "Attempted to add a fetch hint for non-persistent business object.", () => Factory.AddFetchHint(typeof(DummyNonPersistentBusinessObject), ZGuid.NewZGuid()));
			AssertEquals("Should be no fetch hints for non-persistent business objects.", 0, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
			Factory.AddFetchHint(typeof(DummyBusinessObject), ZGuid.NewZGuid());
			AssertEquals(1, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
		}

		public void TestAddFetchHintForPKForTable()
		{
			Factory.AddFetchHint(DummyBizoSchema.PK, ZGuid.Empty);
			AssertEquals(0, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
			Factory.AddFetchHint(DummyBizoSchema.PK, ZGuid.NewZGuid());
			AssertEquals(1, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
		}

		public void TestAddFetchHintWithMaxRows()
		{
			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			factory1.New<DummyBusinessObject>().Z0_Code = "AAA";
			factory1.New<DummyBusinessObject>().Z0_Code = "AAA";
			factory1.New<DummyBusinessObject>().Z0_Code = "AAA";
			factory1.New<DummyBusinessObject>().Z0_Code = "BBB";
			factory1.New<DummyBusinessObject>().Z0_Code = "BBB";
			factory1.Save();

			var allInMemoryDummiesQuery = new ZQuery { FetchOnlyFromLocalCache = true };

			AssertEquals("Precondition", 0, Factory.Load<DummyBusinessObject>(allInMemoryDummiesQuery).Length);

			Factory.AddFetchHint(DummyBizoSchema.Instance, new ZQuery(DummyBizoSchema.Z0_Code, "AAA") { MaximumRows = 1 });
			Factory.AddFetchHint(DummyBizoSchema.Instance, new ZQuery(DummyBizoSchema.Z0_Code, "BBB") { MaximumRows = 1 });
			Factory.ExecuteAllFetchHints();

			AssertEquals("Only 2 rows should be loaded", 2, Factory.Load<DummyBusinessObject>(allInMemoryDummiesQuery).Length);
			AssertEquals(1, Factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, "AAA") { FetchOnlyFromLocalCache = true }).Length);
			AssertEquals(1, Factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, "BBB") { FetchOnlyFromLocalCache = true }).Length);
		}

		#endregion

		#region TestActiveFilter

		public void TestLoadByPKDoesNotUseActiveFilter()
		{
			TestCaseHelper.ClearTable(DummyBizoSchema.Constants.TableName);

			DummyBusinessObjectWithActiveFilter activeDummy = (DummyBusinessObjectWithActiveFilter)Factory.New(typeof(DummyBusinessObjectWithActiveFilter));
			DummyBusinessObjectWithActiveFilter inactiveDummy = (DummyBusinessObjectWithActiveFilter)Factory.New(typeof(DummyBusinessObjectWithActiveFilter));
			activeDummy.IsActive = true;
			inactiveDummy.IsActive = false;

			for (int i = 0; i < 2; i++)
			{
				AssertNotNull(Factory.Load(typeof(DummyBusinessObjectWithActiveFilter), activeDummy.PK));
				AssertNotNull(Factory.Load(typeof(DummyBusinessObjectWithActiveFilter), inactiveDummy.PK));

				// reset the factory so we're loading from the db
				Factory.Save();
				Factory = new BusinessObjectFactory();
			}
		}

		public void TestLoadByUniqueKeyWithEmptyKey()
		{
			var emptyDummy = Factory.LoadTop1<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, ZString.Empty));
			AssertNotNull(emptyDummy);
			AssertNull(Factory.LoadFromUniqueKey<DummyBusinessObject>(DummyBizoSchema.Z0_Code, ZString.Empty));
		}

		public void TestLoadByUniqueOrNaturalKeyDoesNotUseActiveFilter()
		{
			TestCaseHelper.ClearTable(DummyBizoSchema.Constants.TableName);

			DummyBusinessObjectWithActiveFilter activeDummy = (DummyBusinessObjectWithActiveFilter)Factory.New(typeof(DummyBusinessObjectWithActiveFilter));
			DummyBusinessObjectWithActiveFilter inactiveDummy = (DummyBusinessObjectWithActiveFilter)Factory.New(typeof(DummyBusinessObjectWithActiveFilter));

			activeDummy.Z0_Description = "Active Dummy";
			activeDummy.IsActive = ZBool.True;
			inactiveDummy.Z0_Description = "Inactive Dummy";
			inactiveDummy.IsActive = ZBool.False;

			for (int i = 0; i < 2; i++)
			{
				AssertNotNull(Factory.LoadFromUniqueKey(typeof(DummyBusinessObjectWithActiveFilter), DummyBizoSchema.Z0_Description, new ZString("Active Dummy")));
				AssertNotNull(Factory.LoadFromUniqueKey(typeof(DummyBusinessObjectWithActiveFilter), DummyBizoSchema.Z0_Description, new ZString("Inactive Dummy")));
				AssertNotNull(Factory.LoadFromNaturalKey(typeof(DummyBusinessObjectWithActiveFilter), DummyBizoSchema.Z0_Description, new ZString("Active Dummy")));
				AssertNotNull(Factory.LoadFromNaturalKey(typeof(DummyBusinessObjectWithActiveFilter), DummyBizoSchema.Z0_Description, new ZString("Inactive Dummy")));

				// reset the factory so we're loading from the db
				Factory.Save();
				Factory = new BusinessObjectFactory();
			}
		}

		public void TestActiveFilter_WithLoadCollection()
		{
			TestCaseHelper.ClearTable(DummyBizoSchema.Constants.TableName);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObjectWithActiveFilter activeDummy = (DummyBusinessObjectWithActiveFilter)factory.New(typeof(DummyBusinessObjectWithActiveFilter));
			DummyBusinessObjectWithActiveFilter inactiveDummy = (DummyBusinessObjectWithActiveFilter)factory.New(typeof(DummyBusinessObjectWithActiveFilter));
			activeDummy.IsActive = true;
			inactiveDummy.IsActive = false;

			for (int i = 0; i < 2; i++)
			{
				DummyBusinessObjectWithActiveFilter[] loadedActiveDummies = (DummyBusinessObjectWithActiveFilter[])factory.Load(typeof(DummyBusinessObjectWithActiveFilter), new ZQuery());
				AssertEquals("Only the active one should be loaded", 1, loadedActiveDummies.Length);
				AssertEquals("Only the active one should be loaded", true, loadedActiveDummies[0].Z0_Bool);

				ZQuery allFilter = new ZQuery();
				allFilter.IgnoreActiveFilter = true;
				DummyBusinessObjectWithActiveFilter[] loadedAllDummies = (DummyBusinessObjectWithActiveFilter[])factory.Load(typeof(DummyBusinessObjectWithActiveFilter), allFilter);
				AssertEquals("All records should be loaded", 2, loadedAllDummies.Length);

				// reset the factory so we're loading from the db
				factory.Save();
				factory = new BusinessObjectFactory();
			}
		}

		public void TestActiveFilter_WithLoadTop1()
		{
			TestCaseHelper.ClearTable(DummyBizoSchema.Constants.TableName);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObjectWithActiveFilter activeDummy = (DummyBusinessObjectWithActiveFilter)factory.New(typeof(DummyBusinessObjectWithActiveFilter));
			DummyBusinessObjectWithActiveFilter inactiveDummy = (DummyBusinessObjectWithActiveFilter)factory.New(typeof(DummyBusinessObjectWithActiveFilter));
			activeDummy.IsActive = true;
			activeDummy.Z0_Description = "Active Dummy";
			inactiveDummy.IsActive = false;
			inactiveDummy.Z0_Description = "Inactive Dummy";

			for (int i = 0; i < 2; i++)
			{
				DummyBusinessObjectWithActiveFilter loadedInactiveDummy = (DummyBusinessObjectWithActiveFilter)factory.LoadTop1(typeof(DummyBusinessObjectWithActiveFilter), new ZQuery(DummyBizoSchema.Z0_Description, "Inactive Dummy"));
				AssertNull("Inactive items cannot be loaded", loadedInactiveDummy);

				DummyBusinessObjectWithActiveFilter loadedActiveDummy = (DummyBusinessObjectWithActiveFilter)factory.LoadTop1(typeof(DummyBusinessObjectWithActiveFilter), new ZQuery());
				AssertEquals("Active can be loaded", true, loadedActiveDummy.Z0_Bool);

				// reset the factory so we're loading from the db
				factory.Save();
				factory = new BusinessObjectFactory();
			}
		}

		public void TestActiveFilter_WithDBOnlyQuery()
		{
			TestCaseHelper.ClearTable(DummyBizoSchema.Constants.TableName);

			DummyBusinessObjectWithActiveFilter activeDummy = (DummyBusinessObjectWithActiveFilter)Factory.New(typeof(DummyBusinessObjectWithActiveFilter));
			activeDummy.IsActive = true;
			DummyBusinessObjectWithActiveFilter inactiveDummy = (DummyBusinessObjectWithActiveFilter)Factory.New(typeof(DummyBusinessObjectWithActiveFilter));
			inactiveDummy.IsActive = false;

			DummyDependantBusinessObject activeDummyDependent = activeDummy.Dependents.AddNew();
			activeDummyDependent.ZD1_Code = "activ";
			DummyDependantBusinessObject inactiveDummyDependent = inactiveDummy.Dependents.AddNew();
			inactiveDummyDependent.ZD1_Code = "inact";
			Factory.Save();

			BusinessObjectFactory retrievingFactory = new BusinessObjectFactory();
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyDependantBusinessObject));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(DummyBusinessObjectWithActiveFilter), DummyDependentBizoSchema.ZD1_Z0);
			subQuery.AddToFilter(new ZQuery()); // we want to bring back 'everything' (that is, everything that is active)
			query.AddSubQuery(subQuery, JoinCondition.And);

			DummyDependantBusinessObject[] activeDummies = (DummyDependantBusinessObject[])retrievingFactory.Load(typeof(DummyDependantBusinessObject), query);
			AssertEquals(1, activeDummies.Length);
			AssertEquals("Only the active one should be returned", "activ", activeDummies[0].ZD1_Code);

			query = new ZDBOnlyQuery(typeof(DummyDependantBusinessObject));
			query.IgnoreActiveFilter = true;
			subQuery = new ZDBOnlySubQuery(typeof(DummyBusinessObjectWithActiveFilter), DummyDependentBizoSchema.ZD1_Z0);
			subQuery.AddToFilter(new ZQuery()); // we want to bring back 'everything' (that is, everything that is active)
			query.AddSubQuery(subQuery, JoinCondition.And);

			DummyDependantBusinessObject[] allDummies = (DummyDependantBusinessObject[])retrievingFactory.Load(typeof(DummyDependantBusinessObject), query);
			AssertEquals("Both should be returned when ignoring the ActiveFilter", 2, allDummies.Length);
		}

		#endregion

		public void TestImportFromAnotherFactoryWithTypeOverload()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = (DummyBusinessObject)factory1.New(typeof(DummyBusinessObject));
			dummy1.Z0_Description = "Dummy1";
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBaseBusinessObject baseBizO = (DummyBaseBusinessObject)factory2.ImportFromAnotherFactory(dummy1, typeof(DummyBaseBusinessObject));
			AssertEquals(typeof(DummyBaseBusinessObject), baseBizO.GetType());
		}

		public void TestImportFromAnotherFactory_NonPersistent()
		{
			var nomo = new Nomo();
			var factory1 = new BusinessObjectFactory();
			AssertNull(factory1.ImportFromAnotherFactory(nomo));
		}

		class Nomo : NonPersistentBusinessObject
		{
		}

		public void TestImportFromAnotherFactoryWithNewObject()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = (DummyBusinessObject)factory1.New(typeof(DummyBusinessObject));
			dummy1.Z0_Description = "Dummy1";

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummy2 = (DummyBusinessObject)factory2.ImportFromAnotherFactory(dummy1);
			AssertEquals("Z0_Description", dummy1.PK, dummy2.PK);
			AssertEquals("Z0_Description", dummy1.Z0_Description, dummy2.Z0_Description);

			AssertEquals("Factory", factory1, dummy1.Factory);
			AssertEquals("Factory", factory2, dummy2.Factory);

			DataRow rowInFactory2 = ((INeedDataSet)factory2).Data.Tables[DummyBizoSchema.Constants.TableName].Rows.Find(dummy2.PK.ToGuid());
			AssertEquals("Row is same", rowInFactory2, dummy2.Row);

			DataRow rowInFactory1 = ((INeedDataSet)factory1).Data.Tables[DummyBizoSchema.Constants.TableName].Rows.Find(dummy1.PK.ToGuid());
			AssertEquals("Row is same", rowInFactory1, dummy1.Row);

			AssertEquals("RowState", rowInFactory1.RowState, rowInFactory2.RowState);
		}

		public void TestImportFromAnotherFactoryWithSavedObject()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = (DummyBusinessObject)factory1.New(typeof(DummyBusinessObject));
			dummy1.Z0_Description = "Dummy1";
			factory1.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummy2 = (DummyBusinessObject)factory2.ImportFromAnotherFactory(dummy1);
			AssertEquals("Z0_Description", dummy1.PK, dummy2.PK);
			AssertEquals("Z0_Description", dummy1.Z0_Description, dummy2.Z0_Description);
			AssertEquals("Original Description", dummy1.Row[dummy1.Z0_DescriptionInfo.Name, DataRowVersion.Original], dummy2.Row[dummy2.Z0_DescriptionInfo.Name, DataRowVersion.Original]);

			AssertEquals("Factory", factory1, dummy1.Factory);
			AssertEquals("Factory", factory2, dummy2.Factory);

			DataRow rowInFactory2 = ((INeedDataSet)factory2).Data.Tables[DummyBizoSchema.Constants.TableName].Rows.Find(dummy2.PK.ToGuid());
			AssertEquals("Row is same", rowInFactory2, dummy2.Row);

			DataRow rowInFactory1 = ((INeedDataSet)factory1).Data.Tables[DummyBizoSchema.Constants.TableName].Rows.Find(dummy1.PK.ToGuid());
			AssertEquals("Row is same", rowInFactory1, dummy1.Row);

			AssertEquals("RowState", rowInFactory1.RowState, rowInFactory2.RowState);
		}

		public void TestEnsureConnectionIsOpen()
		{
			using (DbConnection connection = new ConnectionWithTinyTimeoutForTest(Db.ServerName, Db.DatabaseName))
			{
				BusinessObjectFactory testFactory = new BusinessObjectFactory(connection);
				testFactory.EnsureConnectionIsOpen();
				AssertEquals(ConnectionState.Open, connection.State);
				connection.CloseConnection();
				AssertEquals(ConnectionState.Closed, connection.State);
				testFactory.EnsureConnectionIsOpen();
				AssertEquals(ConnectionState.Open, connection.State);
			}
		}

		public void TestLoadTop1CachingWithLocalDataOnly()
		{
			DummyBusinessObject bizO1 = Factory.New<DummyBusinessObject>();
			bizO1.Z0_Number = 1;
			DummyBusinessObject bizO2 = Factory.New<DummyBusinessObject>();
			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Number, 1);
			AssertEquals(bizO1, Factory.LoadTop1<DummyBusinessObject>(query));
			bizO1.Z0_Number = 2;
			bizO2.Z0_Number = 1;
			AssertEquals(bizO2, Factory.LoadTop1<DummyBusinessObject>(query));
		}

		public void TestLoadFromPKWithNonExistentGuid()
		{
			ZGuid pk = ZGuid.NewZGuid();
			AssertEquals(0, Factory.DatabaseLoadCount);
			AssertEquals(null, Factory.Load<DummyBusinessObject>(pk));
			AssertEquals(1, Factory.DatabaseLoadCount);
			AssertEquals(null, Factory.Load<DummyBusinessObject>(pk));
			AssertEquals(1, Factory.DatabaseLoadCount);
		}

		#region DeferredTriggerRunner Tests

		public void TestValuesReturnedByDeferAndReturnTriggersArePassedIntoRunDeferredTriggersInSave()
		{
			var mockTriggerRunner = new Mock<IDeferredTriggerRunner>();
			using (ObjectFactory.Substitute(mockTriggerRunner.Object))
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (MockProcedures.CreateNewMockTrigger(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTrigger.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTrigger.StoredProc))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				var expectedDeferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
					{
						{ DummyBizOWithDeferredUpdateTrigger.TriggerName, DeferredTriggerRunnerTest.GetTriggerMetaData(typeof(DummyBizOWithDeferredUpdateTrigger), DummyBizOWithDeferredUpdateTrigger.TriggerName, DummyBizoSchema.PK, new [] { bizO1.PK }) }
					};

				mockTriggerRunner.Setup(m => m.DeferAndReturnTriggers(Factory, It.IsAny<IEnumerable<BusinessObject>>()))
					.Returns(expectedDeferredTriggers);
				mockTriggerRunner.Setup(m => m.RunDeferredTriggers(It.IsAny<IReadOnlyDictionary<string, IReadOnlyCollection<TriggerMetaData>>>(), It.IsAny<IDbConnected>()))
					.Callback<IReadOnlyDictionary<string, IReadOnlyCollection<TriggerMetaData>>, IDbConnected>((arg1, arg2) =>
					{
						var deferredTriggers = arg1;
						DeferredTriggerRunnerTest.AssertDeferredTriggerContents(expectedDeferredTriggers, deferredTriggers);
					});

				Factory.Save();
				mockTriggerRunner.Verify();
			}
		}

		#endregion

		#region SuspendTrigger Tests

		public void TestSaveSuspendsCorrectInsertTriggers()
		{
			var connection = ((IDbConnected)Factory).Connection;
			using (MockProcedures.MockExistingProcedure(connection, TriggerProcedure.SuspendTrigger))
			using (MockProcedures.CreateNewMockProcedure(connection, DummyBizOWithDeferredUpdateTrigger.StoredProc))
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (MockProcedures.CreateNewMockTrigger(connection, DummyBizOWithDeferredUpdateTrigger.TriggerName))
			using (MockProcedures.MockExistingProcedure(connection, TriggerProcedure.ResumeTrigger))
			{
				Factory.Save();
				AssertEquals("Insert trigger should not be suspended when no bizO created", 0,
					MockProcedures.ReadValuesPassedToProcedure(connection, TriggerProcedure.SuspendTrigger).Count());

				var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				Factory.Save();
				AssertContainsExactElementsInAnyOrder("Insert trigger should be suspended when bizO created", new[] { DummyBizOWithDeferredUpdateTrigger.TriggerName },
					MockProcedures.ReadValuesPassedToProcedure(connection, TriggerProcedure.SuspendTrigger));
			}
		}

		public void TestSaveSuspendsCorrectInsertTriggersOnMultipleInserts()
		{
			var connection = ((IDbConnected)Factory).Connection;
			using (MockProcedures.MockExistingProcedure(connection, TriggerProcedure.SuspendTrigger))
			using (MockProcedures.CreateNewMockProcedure(connection, DummyBizOWithDeferredUpdateTrigger.StoredProc))
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (MockProcedures.CreateNewMockTrigger(connection, DummyBizOWithDeferredUpdateTrigger.TriggerName))
			using (MockProcedures.MockExistingProcedure(connection, TriggerProcedure.ResumeTrigger))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				var bizO2 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();

				Factory.Save();
				AssertContainsExactElementsInAnyOrder("Insert trigger should be suspended when multiple bizOs created", new[] { DummyBizOWithDeferredUpdateTrigger.TriggerName },
					MockProcedures.ReadValuesPassedToProcedure(connection, TriggerProcedure.SuspendTrigger));
			}
		}

		public void TestSaveSuspendsCorrectUpdateTriggers()
		{
			var connection = ((IDbConnected)Factory).Connection;
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (MockProcedures.CreateNewMockTrigger(connection, DummyBizOWithDeferredUpdateTrigger.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(connection, DummyBizOWithDeferredUpdateTrigger.StoredProc))
			using (MockProcedures.MockExistingProcedure(connection, TriggerProcedure.ResumeTrigger))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				using (MockProcedures.MockExistingProcedure(connection, TriggerProcedure.SuspendTrigger))
				{
					Factory.Save();
				}
				using (MockProcedures.MockExistingProcedure(connection, TriggerProcedure.SuspendTrigger))
				{
					bizO1.ColumnThatDoesNotRequireTriggerDeferral = 1;
					Factory.Save();
					AssertEquals("Update trigger should not be suspended when field that does not require trigger deferral is updated", 0,
						MockProcedures.ReadValuesPassedToProcedure(connection, TriggerProcedure.SuspendTrigger).Count());

					bizO1.ColumnThatRequiresTriggerDeferral = 3;
					Factory.Save();
					AssertContainsExactElementsInAnyOrder("Update trigger should be suspended when field that requires trigger deferral is updated", new[] { DummyBizOWithDeferredUpdateTrigger.TriggerName },
						MockProcedures.ReadValuesPassedToProcedure(connection, TriggerProcedure.SuspendTrigger));
				}
			}
		}

		public void TestSaveSuspendsCorrectUpdateTriggersOnMultipleUpdates()
		{
			var connection = ((IDbConnected)Factory).Connection;
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (MockProcedures.CreateNewMockTrigger(connection, DummyBizOWithDeferredUpdateTrigger.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(connection, DummyBizOWithDeferredUpdateTrigger.StoredProc))
			using (MockProcedures.MockExistingProcedure(connection, TriggerProcedure.ResumeTrigger))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				var bizO2 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				using (MockProcedures.MockExistingProcedure(connection, TriggerProcedure.SuspendTrigger))
				{
					Factory.Save();
				}

				using (MockProcedures.MockExistingProcedure(connection, TriggerProcedure.SuspendTrigger))
				{
					bizO1.ColumnThatRequiresTriggerDeferral = 3;
					bizO2.ColumnThatRequiresTriggerDeferral = 3;
					Factory.Save();
					AssertContainsExactElementsInAnyOrder("Update trigger should be suspended when multiple bizOs that require trigger deferral are updated", new[] { DummyBizOWithDeferredUpdateTrigger.TriggerName },
						MockProcedures.ReadValuesPassedToProcedure(connection, TriggerProcedure.SuspendTrigger));
				}
			}
		}

		public void TestSaveSuspendsCorrectDeleteTriggers()
		{
			var connection = ((IDbConnected)Factory).Connection;
			using (MockProcedures.MockExistingProcedure(connection, TriggerProcedure.SuspendTrigger))
			using (MockProcedures.CreateNewMockTrigger(connection, DummyBizOWithDeferredDeleteTrigger.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(connection, DummyBizOWithDeferredDeleteTrigger.StoredProc))
			using (MockProcedures.MockExistingProcedure(connection, TriggerProcedure.ResumeTrigger))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredDeleteTrigger>();
				Factory.Save();
				AssertEquals("Delete trigger should not be suspended when no bizO deleted", 0,
					MockProcedures.ReadValuesPassedToProcedure(connection, TriggerProcedure.SuspendTrigger).Count());
				bizO1.Delete();
				Factory.Save();
				AssertContainsExactElementsInAnyOrder("Delete trigger should be suspended when bizO deleted", new[] { DummyBizOWithDeferredDeleteTrigger.TriggerName },
					MockProcedures.ReadValuesPassedToProcedure(connection, TriggerProcedure.SuspendTrigger));
			}
		}

		public void TestSaveSuspendsCorrectDeleteTriggersOnMultipleDeletes()
		{
			var connection = ((IDbConnected)Factory).Connection;
			using (MockProcedures.MockExistingProcedure(connection, TriggerProcedure.SuspendTrigger))
			using (MockProcedures.CreateNewMockTrigger(connection, DummyBizOWithDeferredDeleteTrigger.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(connection, DummyBizOWithDeferredDeleteTrigger.StoredProc))
			using (MockProcedures.MockExistingProcedure(connection, TriggerProcedure.ResumeTrigger))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredDeleteTrigger>();
				var bizO2 = Factory.New<DummyBizOWithDeferredDeleteTrigger>();
				Factory.Save();
				bizO1.Delete();
				bizO2.Delete();
				Factory.Save();
				AssertContainsExactElementsInAnyOrder("Delete trigger should be suspended when multiple bizOs deleted", new[] { DummyBizOWithDeferredDeleteTrigger.TriggerName },
					MockProcedures.ReadValuesPassedToProcedure(connection, TriggerProcedure.SuspendTrigger));
			}
		}

		public void TestSaveSuspendsCorrectTriggersWhenMultipleTriggersDeferred()
		{
			var connection = ((IDbConnected)Factory).Connection;
			using (MockProcedures.MockExistingProcedure(connection, TriggerProcedure.SuspendTrigger))
			using (MockProcedures.CreateNewMockTrigger(connection, DummyBizOWithDeferredDeleteTrigger.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(connection, DummyBizOWithDeferredDeleteTrigger.StoredProc))
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (MockProcedures.CreateNewMockTrigger(connection, DummyBizOWithDeferredUpdateTrigger.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(connection, DummyBizOWithDeferredUpdateTrigger.StoredProc))
			using (MockProcedures.MockExistingProcedure(connection, TriggerProcedure.ResumeTrigger))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredDeleteTrigger>();
				Factory.Save();
				var bizO2 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				bizO1.Delete();
				Factory.Save();
				var valuePassedToProcedure = MockProcedures.ReadValuesPassedToProcedure(connection, TriggerProcedure.SuspendTrigger);
				AssertContainsExactElementsInAnyOrder("Delete and Update trigger should be suspended",
					new[] { DummyBizOWithDeferredDeleteTrigger.TriggerName, DummyBizOWithDeferredUpdateTrigger.TriggerName }, valuePassedToProcedure);
			}
		}

		#endregion

		public void TestSaveThrowsSaveExceptionWhenDeferringTriggerWithValidStoredProcThatThrows()
		{
			using (MockProcedures.CreateNewMockTrigger(((IDbConnected)Factory).Connection, DummyBizOWithDeferredDeleteTrigger.TriggerName))
			using (MockProcedures.CreateFailingMockProcedure(((IDbConnected)Factory).Connection, DummyBizOWithDeferredDeleteTrigger.StoredProc))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredDeleteTrigger>();
				Factory.Save();
				bizO1.Delete();
				AssertExceptionThrown<ZSaveException>("Expect exception to bubble up from erroneous stored proc", () => Factory.Save());
			}
		}

		#region tests that should pass but do not yet do so - old bug discovered by Brendon
		//public void TestLoadTop1CachingWithSaveToFactory()
		//{
		//	DummyBusinessObject bizO1 = Factory.New<DummyBusinessObject>();
		//	bizO1.Z0_Number = 1;
		//	DummyBusinessObject bizO2 = Factory.New<DummyBusinessObject>();
		//	bizO2.Z0_Number = 1;
		//	Factory.Save();

		//	BusinessObjectFactory factory2 = new BusinessObjectFactory();
		//	ZQuery query = new ZQuery(DummyBizoSchema.Z0_Number, 1);

		//	DummyBusinessObject bizOInSecondFactory = factory2.LoadTop1<DummyBusinessObject>(query);
		//	AssertNotNull(bizOInSecondFactory);
		//	bizOInSecondFactory.Z0_Number = 2;
		//	AssertNotNull(factory2.LoadTop1<DummyBusinessObject>(query));
		//}

		//public void TestLoadTop1WithObjectAlreadyModifiedInMemory()
		//{
		//	DummyBusinessObject bizO1 = Factory.New<DummyBusinessObject>();
		//	bizO1.Z0_Number = 1;
		//	DummyBusinessObject bizO2 = Factory.New<DummyBusinessObject>();
		//	bizO2.Z0_Number = 1;
		//	Factory.Save();

		//	BusinessObjectFactory factory2 = new BusinessObjectFactory();
		//	DummyBusinessObject bizO1InSecondFactory = factory2.Load<DummyBusinessObject>(bizO1.PK);
		//	bizO1InSecondFactory.Z0_Number = 2;
		//	ZQuery query = new ZQuery(DummyBizoSchema.Z0_Number, 1);

		//	DummyBusinessObject bizOInSecondFactory = factory2.LoadTop1<DummyBusinessObject>(query);
		//	AssertEquals(bizO2.PK, bizOInSecondFactory.PK);
		//}
		#endregion

		public void TestLoadTop1DoesNotHitDatabaseWhenMatchFoundInMemory()
		{
			DummyBusinessObject bizO1 = Factory.New<DummyBusinessObject>();
			bizO1.Z0_Number = 1;
			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Number, 1);
			AssertEquals("Precondition", 0, Factory.DatabaseLoadCount);
			Factory.LoadTop1<DummyBusinessObject>(query);
			AssertEquals(0, Factory.DatabaseLoadCount);
		}

		public void TestChildFactoriesAndParticipants()
		{
			BusinessObjectFactory parentFactory = new BusinessObjectFactory();
			BusinessObjectFactory childFactory1 = new BusinessObjectFactory();
			BusinessObjectFactory childFactory2 = new BusinessObjectFactory();

			parentFactory.ChildFactories.Add(childFactory1);
			parentFactory.ChildFactories.Add(childFactory2);

			ITransactionParticipant[] childParticpants = ((ITransactionParticipant)parentFactory).ChildParticipants;
			AssertEquals("ChildParticipants.Length", 2, childParticpants.Length);
			AssertEquals("ChildParticipants[0]", childFactory1, childParticpants[0]);
			AssertEquals("ChildParticipants[1]", childFactory2, childParticpants[1]);
		}

		public void TestTableCodeFromType()
		{
			AssertEquals("Z0", BusinessObjectFactory.GetTableCodeFromType(typeof(DummyBusinessObject)));
			AssertEquals("ZD1", BusinessObjectFactory.GetTableCodeFromType(typeof(DummyDependantBusinessObject)));
		}

		public void TestSaveClearsOnlyRelatedQueriesFromCache()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			factory1.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, "XXX"));
			factory1.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, "YYY"));
			factory1.Load<DummyDependantBusinessObject>(new ZQuery(DummyDependentBizoSchema.ZD1_Code, "XXX"));
			factory1.Load<DummyDependantBusinessObject>(new ZQuery(DummyDependentBizoSchema.ZD1_Code, "YYY"));

			factory2.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, "AAA"));
			factory2.Load<DummyDependantBusinessObject>(new ZQuery(DummyDependentBizoSchema.ZD1_Code, "BBB"));

			factory1.NewWithValidTestData<DummyBusinessObject>();

			AssertEquals(4, factory1.RowFactory.QueryCache.CachedKeys.Length);
			AssertEquals(2, factory2.RowFactory.QueryCache.CachedKeys.Length);

			factory1.Save();

			AssertNotEquals("Query cache is lazily cleared", 2, factory1.RowFactory.QueryCache.CachedKeys.Length);
			AssertNotEquals("Query cache is lazily cleared", 1, factory2.RowFactory.QueryCache.CachedKeys.Length);

			factory1.RowFactory.QueryCache.IsCached("derp", new ZQuery());
			factory2.RowFactory.QueryCache.IsCached("derp", new ZQuery());

			AssertEquals(2, factory1.RowFactory.QueryCache.CachedKeys.Length);
			AssertEquals(1, factory2.RowFactory.QueryCache.CachedKeys.Length);
		}

		public void TestLoadFromNaturalKeyOrUniqueKeyUsingInterface()
		{
			var factory1 = new BusinessObjectFactory();
			var consolInFactory1 = factory1.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consolInFactory1.JK_UniqueConsignRef = "JK2343423424";
			consolInFactory1.JK_TransportMode = "SEA";
			factory1.Save();

			var factory2 = new BusinessObjectFactory();
			var consolInFactory2 = factory2.LoadFromNaturalKey<Enterprise.Integration.Forwarding.IForwardingConsol>(JobConsolSchema.JK_UniqueConsignRef, "JK2343423424");
			AssertEquals(consolInFactory1.PK, consolInFactory2.PK);

			var factory3 = new BusinessObjectFactory();
			var consolInFactory3 = factory3.LoadFromUniqueKey<Enterprise.Integration.Forwarding.IForwardingConsol>(JobConsolSchema.JK_UniqueConsignRef, consolInFactory1.JK_UniqueConsignRef);
			AssertEquals(consolInFactory1.PK, consolInFactory3.PK);
		}

		public void TestFactoryRecordsCreationThreadID()
		{
			AssertEquals(Thread.CurrentThread.ManagedThreadId, new BusinessObjectFactory().ThreadSentry.CreationThread.ThreadID);
		}

		public void TestImplementsICacheProvider()
		{
			Assert("BusinessObjectFactory should implement the ICacheProvider Interface",
				new BusinessObjectFactory() is ICacheProvider);
		}

		public void TestGetConcreteBusinessObjectTypeDoesNotThrowNullReferenceException()
		{
			AssertExceptionThrown(typeof(ArgumentException), "type could not be null.", () => new ZDBOnlyQuery(null));
		}

		#region Test StringInterning

		public void TestStringInterningOff()
		{
			DummyBaseBusinessObject bizO = (DummyBaseBusinessObject)Factory.New(typeof(DummyBaseBusinessObject));
			object result1 = Factory.InternValue(bizO.Z0_DescriptionInfo, (ZString)string.Format("my {0} string", "test"));
			object result2 = Factory.InternValue(bizO.Z0_DescriptionInfo, (ZString)string.Format("my {0} string", "test"));

			Assert(!ReferenceEquals(result1.ToString(), result2.ToString()));
		}

		public void TestStringInterningOn()
		{
			Factory.ActivateStringInterning();
			try
			{
				DummyBaseBusinessObject bizO = (DummyBaseBusinessObject)Factory.New(typeof(DummyBaseBusinessObject));
				object result1 = Factory.InternValue(bizO.Z0_DescriptionInfo, (ZString)string.Format("my {0} string", "test"));
				object result2 = Factory.InternValue(bizO.Z0_DescriptionInfo, (ZString)string.Format("my {0} string", "test"));

				Assert(ReferenceEquals(result1.ToString(), result2.ToString()));
			}
			finally
			{
				Factory.DeactivateStringInterning();
			}
		}

		#endregion

		#region Test SaveInTransactionCore fires ActiveBusinessObjectCollection.ListChanged only once

		public void TestSaveFilesActiveBusinessObjectCollection_ListChangedOnlyOnce()
		{
			int counter = 0;

			ActiveBusinessObjectCollection<BizOForListChangedTest> collection = new ActiveBusinessObjectCollection<BizOForListChangedTest>(Factory);
			((IActiveBusinessObjectCollection)collection).ListChanged += delegate
			{ counter++; };

			collection.AddNew();
			collection.AddNew();
			collection.AddNew();

			counter = 0;
			Factory.Save();
			AssertEquals(2, counter); // 1 for SaveInTransaction, 1 for AllTransactionsSaved
		}

		public class BizOForListChangedTest : DummyBusinessObject
		{
			public BizOForListChangedTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override void OnFactorySaving()
			{
				base.OnFactorySaving();
				ActiveBusinessObjectCollection.RefreshAll(Factory);
			}
		}

		#endregion

		#region Test Save Events

		public void TestSavedEventCalled()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			AssertNotNull(dummy);

			Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
			SavedEventCalled = false;
			Factory.Save();

			AssertEquals("Saved should be called", true, SavedEventCalled);
			AssertEquals("Saved should be called after the transaction is committed", true, SavedEventCalledOutsideTransaction);
			AssertEquals("Saved should report success", true, Success);
		}

		public void TestSavedEventReportsFailure()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			AssertNotNull(dummy);

			dummy.Row[DummyBusinessObject.Schema.PK] = DBNull.Value; // break it so post fails!

			Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
			SavedEventCalled = false;

			try
			{
				Factory.Save();
			}
			catch
			{
				// expecting save to fail
			}

			AssertEquals("Saved should be called", true, SavedEventCalled);
			AssertEquals("Saved should report failure", false, Success);
		}

		bool SavedEventCalled;
		bool SavedEventCalledOutsideTransaction;
		bool Success;

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			Success = savedSuccessfully;
			SavedEventCalled = true;

			// The testcase transaction should be open, but no others should be
			SavedEventCalledOutsideTransaction = (Db.Connection.AppTransactionCount == 1);
		}

		#region TestOnSavedWithMultipleFactorySave

		public void TestOnSavedWithMultipleFactorySave()
		{
			DummyWithFactorySaveInFactorySaved dummy = Factory.New<DummyWithFactorySaveInFactorySaved>();
			Assert("Precondition - OnSaved() was not yet called", !dummy.OnSavedCalled);

			Factory.Save();
			Assert("OnSaved() should have been called", dummy.OnSavedCalled);
		}

		class DummyWithFactorySaveInFactorySaved : DummyBusinessObject
		{
			public DummyWithFactorySaveInFactorySaved(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override void OnFactorySaved(bool saveSucceeded)
			{
				base.OnFactorySaved(saveSucceeded);

				if (doFactorySave)
				{
					doFactorySave = false;
					Factory.Save();
				}
				else
				{
					doFactorySave = true;
				}
			}

			bool doFactorySave = true;
		}

		#endregion

		#endregion

		#region Test Classes

		class TestBusinessObjectFactory : BusinessObjectFactory
		{
			public TestBusinessObjectFactory()
			{
			}

			public TestBusinessObjectFactory(DbConnection connection)
				: base(connection)
			{
			}

			public void SetNotifyOfSaveBeforeAnyFactorySaveBegins(bool value)
			{
				NotifyOfSaveBeforeAnyFactorySaveBeginsOverride = value;
			}

			protected override bool NotifyOfSaveBeforeAnyFactorySaveBegins()
			{
				return NotifyOfSaveBeforeAnyFactorySaveBeginsOverride;
			}

			bool NotifyOfSaveBeforeAnyFactorySaveBeginsOverride;
		}

		#endregion

		#region TestCompressDecompress

		public void TestCompressDecompress()
		{
			var bizO = Factory.New<DummyBusinessObject>();
			var text = "There once was a man from Nantucket who got his foot stuck in a bucket.";
			bizO.Z0_VarBinaryMax = Encoding.UTF8.GetBytes(text);
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var bizO2 = factory2.Load<DummyBusinessObject>(bizO.PK);
			AssertEquals(text, Encoding.UTF8.GetString(bizO2.Z0_VarBinaryMax));
		}

		#endregion

		#region TestAfterOnSavingBOProcessingService

		public void TestAfterOnSavingBOProcessingService_ObjectUpdatedByTheServiceIsActuallySaved()
		{
			var bizo1 = Factory.New<DummyBusinessObject>();
			Factory.Save();
			Factory.ServiceContainer.AddAfterOnSavingService(new LambdaService(() => bizo1.Z0_Code = "PIG"));
			Factory.Save();
			var freshDummy = new BusinessObjectFactory().Load<DummyBusinessObject>(bizo1.PK);
			AssertEquals("PIG", freshDummy.Z0_Code);
		}

		class LambdaService : IAfterOnSavingBOProcessingService
		{
			readonly Func<ZString> p;
			public LambdaService(Func<ZString> p)
			{
				this.p = p;
			}

			public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				p.Invoke();
			}
		}

		public void TestAfterOnSavingBOProcessingService()
		{
			var bizo1 = Factory.New<DummyForAfterOnSavingBOProcessing>();
			var bizo2 = Factory.New<DummyForAfterOnSavingBOProcessing>();
			Factory.ServiceContainer.AddAfterOnSavingService(new TestService1());
			Factory.ServiceContainer.AddAfterOnSavingService(new TestService2());
			Factory.Save();

			AssertEquals("AfterOnSavingService shoud be called for each object after all OnSaving happened and one time during Factory.Save.", 1, bizo1.AfterOnSavingCallAmount1);
			AssertEquals("AfterOnSavingService shoud be called for each object after all OnSaving happened and one time during Factory.Save.", 1, bizo1.AfterOnSavingCallAmount2);
			AssertEquals("AfterOnSavingService shoud be called for each object after all OnSaving happened and one time during Factory.Save.", 1, bizo2.AfterOnSavingCallAmount1);
			AssertEquals("AfterOnSavingService shoud be called for each object after all OnSaving happened and one time during Factory.Save.", 1, bizo2.AfterOnSavingCallAmount2);
		}

		class DummyForAfterOnSavingBOProcessing : DummyBusinessObject
		{
			public DummyForAfterOnSavingBOProcessing(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override void OnSaving()
			{
				base.OnSaving();

				AfterOnSavingCallAmount1 = 0;
				AfterOnSavingCallAmount2 = 0;
			}

			public int AfterOnSavingCallAmount1 = -10;
			public int AfterOnSavingCallAmount2 = -10;
		}

		class TestService1 : IAfterOnSavingBOProcessingService
		{
			public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				foreach (var bizo in businessObjectsInOnSavingOrder)
				{
					var castedBizo = bizo as DummyForAfterOnSavingBOProcessing;
					if (castedBizo != null)
					{
						castedBizo.AfterOnSavingCallAmount1++;
					}
				}
			}
		}

		class TestService2 : IAfterOnSavingBOProcessingService
		{
			public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				foreach (var bizo in businessObjectsInOnSavingOrder)
				{
					var castedBizo = bizo as DummyForAfterOnSavingBOProcessing;
					if (castedBizo != null)
					{
						castedBizo.AfterOnSavingCallAmount2++;
					}
				}
			}
		}

		#endregion

		#region TestAfterCommittedService

		public void TestAfterCommittedService()
		{
			var bizo1 = Factory.New<DummyForAfterCommittedService>();
			var bizo2 = Factory.New<DummyForAfterCommittedService>();
			Factory.ServiceContainer.AddAfterCommittedService(new TestAfterCommittedService1());
			Factory.ServiceContainer.AddAfterCommittedService(new TestAfterCommittedService2());
			Factory.Save();

			AssertEquals("AfterCommittedService shoud be called one time during Factory.Save.", 1, bizo1.AfterCommittedCallAmount1);
			AssertEquals("AfterCommittedService shoud be called one time during Factory.Save.", 1, bizo1.AfterCommittedCallAmount2);
			AssertEquals("AfterCommittedService shoud be called one time during Factory.Save.", 1, bizo2.AfterCommittedCallAmount1);
			AssertEquals("AfterCommittedService shoud be called one time during Factory.Save.", 1, bizo2.AfterCommittedCallAmount2);
		}

		class DummyForAfterCommittedService : DummyBusinessObject
		{
			public DummyForAfterCommittedService(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void OnFactorySaved(bool saveSucceeded)
			{
				base.OnFactorySaved(saveSucceeded);

				AfterCommittedCallAmount1 = 0;
				AfterCommittedCallAmount2 = 0;
			}

			public int AfterCommittedCallAmount1 = -10;
			public int AfterCommittedCallAmount2 = -10;
		}

		class TestAfterCommittedService1 : IAfterCommittedService
		{
			public void DoAfterCommitted(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				foreach (var bizo in businessObjectsInOnSavingOrder)
				{
					var castedBizo = bizo as DummyForAfterCommittedService;
					if (castedBizo != null)
					{
						castedBizo.AfterCommittedCallAmount1++;
					}
				}
			}
		}

		class TestAfterCommittedService2 : IAfterCommittedService
		{
			public void DoAfterCommitted(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				foreach (var bizo in businessObjectsInOnSavingOrder)
				{
					var castedBizo = bizo as DummyForAfterCommittedService;
					if (castedBizo != null)
					{
						castedBizo.AfterCommittedCallAmount2++;
					}
				}
			}
		}

		public void TestAfterCommittedService_ExecutedAnywayWhenTransactionIsCommitted()
		{
			var bizo1 = Factory.New<DummyForAfterCommittedServiceThrows>();
			Factory.ServiceContainer.AddAfterCommittedService(new TestAfterCommittedService3());
			AssertExceptionThrown<ArgumentException>(Factory.Save);

			AssertEquals("AfterCommittedService shoud be called after the transaction is committed, even if sth has failed in OnAllTransactionsCommitted.", 1, bizo1.AfterCommittedCallAmount);
		}

		class DummyForAfterCommittedServiceThrows : DummyBusinessObject
		{
			public DummyForAfterCommittedServiceThrows(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void OnFactorySaved(bool saveSucceeded)
			{
				throw new ArgumentException("Invalid argument", nameof(saveSucceeded));
			}

			public int AfterCommittedCallAmount;
		}

		class TestAfterCommittedService3 : IAfterCommittedService
		{
			public void DoAfterCommitted(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				foreach (var bizo in businessObjectsInOnSavingOrder)
				{
					var castedBizo = bizo as DummyForAfterCommittedServiceThrows;
					if (castedBizo != null)
					{
						castedBizo.AfterCommittedCallAmount++;
					}
				}
			}
		}

		#endregion

		#region TestUpdateMRUCache_Concurrency

		public void TestUpdateMRUCache_Concurrency()
		{
			BusinessObjectFactoryForConcurrencyTest factory = new BusinessObjectFactoryForConcurrencyTest();
			DummyBusinessObject dummy0 = factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy1 = factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy3 = factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy4 = factory.New<DummyBusinessObject>();
			bool b1 = false, b2 = false, b3 = false, b4 = false;
			factory.cache9Objects(dummy0);
			var t1 = new Thread(() => b1 = factory.addACache(dummy1));
			var t2 = new Thread(() => b2 = factory.addACache(dummy2));
			var t3 = new Thread(() => b3 = factory.addACache(dummy3));
			var t4 = new Thread(() => b4 = factory.addACache(dummy4));

			t1.Start();
			t2.Start();
			t3.Start();
			t4.Start();
			t1.Join();
			t2.Join();
			t3.Join();
			t4.Join();
			Assert("addACache operation on all threads must both be successful", b1 && b2 && b3 && b4);
			AssertContainsExactElementsInAnyOrder("All 4 new bizOs inserted", new BusinessObject[] { dummy1, dummy2, dummy3, dummy4, dummy0, dummy0, dummy0, dummy0, dummy0, dummy0 }, factory.last10BizObjsLoadedExposed);
		}

		public void TestIfCachePinterIsNullDueToConcurrency()
		{
			//arrange
			var factory = new BusinessObjectFactoryForConcurrencyTest();

			var bizoPK = new ZGuid();
			var dummyBizO = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizO.Z0_Description = "Description!!";
			bizoPK = dummyBizO.PK;
			factory.Save();

			//act
			factory.Load<DummyBusinessObject>(bizoPK);
			factory.MakeCacheMemberNullForIndex(0);
			var result = factory.Load<DummyBusinessObject>(bizoPK);

			//assert
			AssertEquals(bizoPK, result.PK);
		}

		#endregion

		#region TestExcludeDeletedBusinessObjectsFromMethodCaller

		public void TestExcludeDeletedBusinessObjectsFromMethodCaller()
		{
			var dummy = Factory.New<DummyBuinessObject2>();

			AssertNoExceptionThrown(Factory.Save);
		}

		class DummyBuinessObject2 : DummyBusinessObject
		{
			public DummyBuinessObject2(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void OnFactorySavingBeforeTransactionCore()
			{
				base.OnFactorySavingBeforeTransactionCore();

				var dummier = Factory.New<DummyBuinessObject3>();

				dummier.Delete();
			}
		}

		class DummyUnsavablePersistentObjectThatRequiresRefresh : DummyBusinessObject
		{
			public DummyUnsavablePersistentObjectThatRequiresRefresh(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override bool IsSavedByFactory
			{
				get
				{
					return false;
				}
			}
			protected override bool IsForcedPublish
			{
				get
				{
					return true;
				}
			}
		}

		class DummyBuinessObject3 : DummyBusinessObject
		{
			public DummyBuinessObject3(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void OnFactorySavingBeforeTransactionCore()
			{
				Z0_Description = Z0_Code;
			}
		}

		#endregion

		#region TestDelayListChangedIsDisabledDuringFetchingRowsFromDb

		public void TestDelayListChangedIsDisabledDuringFetchingRowsFromDb()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Number = 111;
			dummy.Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };

			otherFactory.AddFetchHint(DummyBizoSchema.Instance, new ZQuery(DummyBizoSchema.Z0_Number, 111));
			otherFactory.ExecuteAllFetchHints();

			var otherRow = otherFactory.RowFactory.GetRow(DummyBizoSchema.Constants.TableName, dummy.PK);
			AssertNotNull("Data row should be loaded.", otherRow);
			AssertNull("Business object should not be created yet.", otherFactory.GetBizOsForDataRow(otherRow));

			var filter = new ZQuery(DummyBizoSchema.Z0_Number, 222);
			var collectionInOtherFactory = new DummyWithDbAccessOnCreationCollection(otherFactory, filter);
			AssertEquals(0, collectionInOtherFactory.Count); // Initialize collection, index, and DataView

			dummy.Z0_Number = 222;
			dummy.Factory.Save();

			AssertEquals("Collection should not be refreshed yet.", 0, collectionInOtherFactory.Count);

			otherFactory.AddFetchHint(DummyBizoSchema.Instance, new ZQuery(filter) { ReLoadExistingRows = true });
			otherFactory.ExecuteAllFetchHints();

			AssertEquals("Business object should be loaded into collection.", 1, collectionInOtherFactory.Count);

			Assert(collectionInOtherFactory[0].CreationTimeDbException, string.IsNullOrEmpty(collectionInOtherFactory[0].CreationTimeDbException));
			Assert("Db access during creation time should return correct value.", collectionInOtherFactory[0].CreationTimeDbCount > 0);
		}

		class DummyWithDbAccessOnCreation : DummyBusinessObject
		{
			public DummyWithDbAccessOnCreation(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				try
				{
					CreationTimeDbCount = factory.GetDatabaseCount(GetType());
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					CreationTimeDbException = ex.Message;
				}
			}

			public int CreationTimeDbCount { get; private set; }
			public string CreationTimeDbException { get; private set; }
		}

		class DummyWithDbAccessOnCreationCollection : ActiveBusinessObjectCollection<DummyWithDbAccessOnCreation>
		{
			public DummyWithDbAccessOnCreationCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter) { }
		}

		#endregion

		#region TestExceptionAfterCommit

		public void TestExceptionAfterCommit()
		{
			var dummy = Factory.New<DummyBusinessObject>();

			EventHandler failureCauser = (_, x_) =>
			{
				var error = SqlExceptionBuilder.CreateSqlError(-2, byte.MaxValue, byte.MinValue, Db.Connection.ServerName, "Timeout expired", "", 0);
				var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
				var sqlException = SqlExceptionBuilder.CreateSqlException(errors);

				throw new ZDataException(sqlException, ((INeedRow)dummy).Row, Db.Connection);
			};

			Factory.CheckSaveButNotUpdatedInSameConnectionForTest = true;
			((IBusinessObjectFactoryInternals)Factory).RowFactory.CommittingTransaction += failureCauser;

			var ex = AssertExceptionThrown<ZSaveErrorAfterCommitInDbException>(Factory.Save);

			Assert("Dummy should not be marked as saved.", !dummy.IsInDatabase);
			AssertNotNull("Dummy should have been actually saved to db.", new BusinessObjectFactory { RefreshEnabled = false }.Load<DummyBusinessObject>(dummy.PK));
			AssertNoExceptionThrown("This exception should be handled", () => ZExceptionReporting.HandleSaveException(ex));
		}

		public void TestDeletedBizOIsNotReloaded()
		{
			var dummy = Factory.New<DummyReloadableBizO>();

			EventHandler failureCauser = (_, x_) =>
			{
				var error = SqlExceptionBuilder.CreateSqlError(-2, byte.MaxValue, byte.MinValue, Db.Connection.ServerName, "Timeout expired", "", 0);
				var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
				var sqlException = SqlExceptionBuilder.CreateSqlException(errors);

				Factory.BusinessObjectsInLastSaveOrder = new BusinessObject[] { dummy };

				throw new ZDataException(sqlException, ((INeedRow)dummy).Row, Db.Connection);
			};

			Factory.CheckSaveButNotUpdatedInSameConnectionForTest = true;
			dummy.Delete();
			((IBusinessObjectFactoryInternals)Factory).RowFactory.CommittingTransaction += failureCauser;

			var ex = AssertExceptionThrown<ZSaveException>("This should not throw a ZSaveErrorAfterCommitInDbException which would occur if a reloadable BizO was reloaded", Factory.Save);

			Assert("The deleted business object should not be reloaded", !dummy.IsReloadCalled);
			AssertNoExceptionThrown("This exception should be handled", () => ZExceptionReporting.HandleSaveException(ex));
		}

		#endregion

		public void TestCleanUp()
		{
			// Arrange
			var factory = new BusinessObjectFactory();
			var filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, "AAA");
			var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(factory, filter);
			var index = collection.IndexExposed;
			var factoryIndices = ActiveBusinessObjectCollectionIndexCache.GetInstance(factory).All.ToList();

			Assert(!index.IsDisposed);
			AssertEquals(1, factoryIndices.Count);
			AssertNotNull("Active Owner should be not be null", ((ActiveBusinessObjectCollectionIndex<DummyBusinessObject>)factoryIndices.First()).ActiveOwnerExposed);

			// Act
			factory.CleanUp();

			// Assert
			Assert("All ActiveBusinessObjectCollectionIndices should be disposed.", index.IsDisposed);
			Assert("DataRefreshBus should be disabled.", !factory.RefreshEnabled);
			AssertNull("Active Owner should be null after cleanup", ((ActiveBusinessObjectCollectionIndex<DummyBusinessObject>)factoryIndices.First()).ActiveOwnerExposed);
		}

		public void TestGettingIndexAfterCleanUpGeneratesNewIndex()
		{
			// Arrange
			var factory = new BusinessObjectFactory();
			var filter = new ZQuery();
			var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(factory, filter);
			var originalIndex = collection.IndexExposed;

			// Act
			factory.CleanUp();

			// Assert
			var indexAfterCleanupUp = collection.IndexExposed;
			AssertEquals("Original Indexshould be disposed.", true, originalIndex.IsDisposed);
			AssertEquals("Retrieved Index after cleanup should not be disposed.", false, indexAfterCleanupUp.IsDisposed);
			AssertNotEquals("Indices pre and post CleanUp should be different.", originalIndex, indexAfterCleanupUp);
		}

		public void TestOnSaveDelayerNotRunWhenThrowing()
		{
			var dummy = Factory.New<DummyBizoWithOnSaveActions>();

			dummy.OnSavingAction = () =>
			{
				Factory.OnSaveDelayer.Do(() =>
				{
					Fail("Should not run OnSaveDelayer when throwing out of OnSaving");
				});
				((IDbConnected)Factory).Connection.ExecuteScalar("SELECT blah from blah");
			};

			AssertExceptionThrown<SqlException>(Factory.Save);
		}

		#region Test CreateBusinessObjectsFromRows with incompatible type

		public void TestCreateBusinessObjectsFromRowsWithIncompaticbleType()
		{
			DummyC1.TypeDecider.TypeForLoadOverride = typeof(DummyC2);

			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "X01";
			dummy1.Z0_Number = 37897;

			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Code = "X02";
			dummy2.Z0_Number = 37897;

			Factory.Save();

			var dummiesReloaded = new BusinessObjectFactory { RefreshEnabled = false }.Load<DummyC1>(new ZQuery(DummyBizoSchema.Z0_Number, 37897));
			AssertNotNull(dummiesReloaded);
			AssertEquals(0, dummiesReloaded.Length);

			var expectedErrorMessage =
@"CreateBusinessObjectsFromRows attempted to return a business object of incompatible type. 
Expected type: CargoWise.EntityFramework.Testing.BusinessObjectFactoryTest+DummyC1; 
Actual object type: CargoWise.EntityFramework.Testing.BusinessObjectFactoryTest+DummyC2; 
Actual object returned by: BusinessObjectFactory.CreateBusinessObject(); 
Data row PK: ";
			AssertStartsWith("Should report incompatible type", expectedErrorMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		abstract class DummyC1 : DummyBusinessObject
		{
			protected DummyC1(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public new static readonly DummyBaseTypeDecider TypeDecider = new DummyBaseTypeDecider();
		}

		class DummyC2 : DummyBusinessObject
		{
			public DummyC2(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		#endregion

		#region helper methods

		public void TestFactoryExistsInDatabase()
		{
			var factory1 = new BusinessObjectFactory();
			var org1 = factory1.NewWithValidTestData<DummyBusinessObject>();
			org1.Z0_Code = "Z0_1";
			factory1.Save();
			var org2 = factory1.NewWithValidTestData<DummyBusinessObject>();
			org2.Z0_Code = "Z0_2";

			var factory2 = new BusinessObjectFactory();
			var org3 = factory2.NewWithValidTestData<DummyBusinessObject>();
			org3.Z0_Code = "Z0_3";
			factory2.Save();
			var org4 = factory2.NewWithValidTestData<DummyBusinessObject>();
			org4.Z0_Code = "Z0_4";

			AssertEquals(true, factory2.ExistsInDatabase(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Code, "Z0_1")));
			AssertEquals(false, factory2.ExistsInDatabase(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Code, "Z0_2")));
			AssertEquals(true, factory2.ExistsInDatabase(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Code, "Z0_3")));
			AssertEquals(false, factory2.ExistsInDatabase(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Code, "Z0_4")));

			org1.Z0_Code = "Z0_A";
			org2.Z0_Code = "Z0_B";
			org3.Z0_Code = "Z0_C";
			org4.Z0_Code = "Z0_D";

			AssertEquals(true, factory2.ExistsInDatabase(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Code, "Z0_1")));
			AssertEquals(false, factory2.ExistsInDatabase(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Code, "Z0_2")));
			AssertEquals(true, factory2.ExistsInDatabase(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Code, "Z0_3")));
			AssertEquals(false, factory2.ExistsInDatabase(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Code, "Z0_4")));

			AssertEquals(false, factory2.ExistsInDatabase(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Code, "Z0_A")));
			AssertEquals(false, factory2.ExistsInDatabase(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Code, "Z0_B")));
			AssertEquals(false, factory2.ExistsInDatabase(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Code, "Z0_C")));
			AssertEquals(false, factory2.ExistsInDatabase(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Code, "Z0_D")));

			factory1.Save();
			factory2.Save();

			AssertEquals(true, factory2.ExistsInDatabase(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Code, "Z0_A")));
			AssertEquals(true, factory2.ExistsInDatabase(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Code, "Z0_B")));
			AssertEquals(true, factory2.ExistsInDatabase(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Code, "Z0_C")));
			AssertEquals(true, factory2.ExistsInDatabase(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Code, "Z0_D")));
		}

		public void TestFactoryExists()
		{
			var factory1 = new BusinessObjectFactory();
			var org1 = factory1.NewWithValidTestData<DummyBusinessObject>();
			org1.Z0_Code = "Z0_1";
			factory1.Save();
			var org2 = factory1.NewWithValidTestData<DummyBusinessObject>();
			org2.Z0_Code = "Z0_2";

			var factory2 = new BusinessObjectFactory();
			var org3 = factory2.NewWithValidTestData<DummyBusinessObject>();
			org3.Z0_Code = "Z0_3";
			factory2.Save();
			var org4 = factory2.NewWithValidTestData<DummyBusinessObject>();
			org4.Z0_Code = "Z0_4";

			AssertEquals(true, factory2.Exists(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "Z0_1")));
			AssertEquals(false, factory2.Exists(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "Z0_2")));
			AssertEquals(true, factory2.Exists(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "Z0_3")));
			AssertEquals(true, factory2.Exists(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "Z0_4")));

			org1.Z0_Code = "Z0_A";
			org2.Z0_Code = "Z0_B";
			org3.Z0_Code = "Z0_C";
			org4.Z0_Code = "Z0_D";

			AssertEquals(true, factory2.Exists(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "Z0_1")));
			AssertEquals(false, factory2.Exists(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "Z0_2")));
			AssertEquals(false, factory2.Exists(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "Z0_3")));
			AssertEquals(false, factory2.Exists(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "Z0_4")));

			AssertEquals(false, factory2.Exists(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "Z0_A")));
			AssertEquals(false, factory2.Exists(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "Z0_B")));
			AssertEquals(true, factory2.Exists(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "Z0_C")));
			AssertEquals(true, factory2.Exists(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "Z0_D")));

			factory1.Save();
			factory2.Save();

			AssertEquals(true, factory2.Exists(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "Z0_A")));
			AssertEquals(true, factory2.Exists(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "Z0_B")));
			AssertEquals(true, factory2.Exists(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "Z0_C")));
			AssertEquals(true, factory2.Exists(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "Z0_D")));
		}

		public void TestFactoryExists_MergeDbAndCacheResult()
		{
			var factory = new BusinessObjectFactory();
			var org3 = factory.NewWithValidTestData<DummyBusinessObject>();
			org3.Z0_Code = "Z0_3";
			factory.Save();

			AssertEquals(true, factory.Exists(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "Z0_3")));

			org3.Z0_Code = "Z0_C";

			AssertEquals("GIVEN Z0_3 exists in database and modified to Z0_C3 in cache, WHEN execute factory2.Exists with mergeDbAndCacheResult=TRUE SHOULD return false - because it recognised the record has been modified",
				false,
				factory.Exists(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "Z0_3"), mergeDbAndCacheResult: true));

			AssertEquals("GIVEN Z0_3 exists in database and modified to Z0_C3 in cache, WHEN execute factory2.Exists with mergeDbAndCacheResult=FALSE SHOULD return true - because it still exists in Db and doesn't recognised the record has been modified",
				true,
				factory.Exists(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "Z0_3"), mergeDbAndCacheResult: false));
		}

		public void TestFactoryExists_DBHits()
		{
			var factory1 = new BusinessObjectFactory();

			var org1 = factory1.NewWithValidTestData<DummyBusinessObject>();
			org1.Z0_Code = "Z0_1";

			AssertEquals("Initially no database hits", 0, factory1.DatabaseLoadCount);

			AssertEquals(true, factory1.Exists(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "Z0_1"), mergeDbAndCacheResult: false));
			AssertEquals("GIVEN BizO in cache WHEN execute factory1.Exist SHOULD not have dbHit", 0, factory1.DatabaseLoadCount);

			factory1.Save();

			AssertEquals(true, factory1.Exists(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "Z0_1"), mergeDbAndCacheResult: false));
			AssertEquals("GIVEN BizO in cache and saved to db WHEN execute factory1.Exist SHOULD not have dbHit", 0, factory1.DatabaseLoadCount);

			var factory2 = new BusinessObjectFactory();

			AssertEquals(true, factory2.Exists(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "Z0_1"), mergeDbAndCacheResult: false));
			AssertEquals("GIVEN BizO not in cache WHEN execute factory2.Exist SHOULD have dbHit because it is returning ROW (SELECT TOP 1)", 1, factory2.DatabaseLoadCount);
		}

		#endregion // helper methods

		#region Implementation

		class DummyNonPersistentBusinessObject : NonPersistentBusinessObject
		{
			public DummyNonPersistentBusinessObject()
			{
			}

			public DummyNonPersistentBusinessObject(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		BusinessObjectFactory Factory;
		Guid PK1;
		Guid PK2;

		protected override void SetUp()
		{
			// We want to ensure the ProcessFieldChangeRules are pre-loaded in the UberCache so that HitCounts are accurate
			new BusinessObjectFactory().New<DummyBusinessObject>();
			base.SetUp();

			PK1 = Guid.NewGuid();
			PK2 = Guid.NewGuid();
			DummyTableCreator.AddDummyBusinessObjectsToDB(PK1, PK2);
			Factory = new BusinessObjectFactory();
		}

		class DummyBizoWithOnSaveActions : DummyBusinessObject
		{
			public DummyBizoWithOnSaveActions(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public Action OnSavingAction;
			public Action OnSavedAction;
			public Action OnFactorySavedAction;

			public override void OnSaving()
			{
				base.OnSaving();

				OnSavingAction?.Invoke();
			}

			public override void OnSaved(bool saveSucceeded)
			{
				base.OnSaved(saveSucceeded);

				OnSavedAction?.Invoke();
			}

			protected override void OnFactorySaved(bool saveSucceeded)
			{
				base.OnFactorySaved(saveSucceeded);

				OnFactorySavedAction?.Invoke();
			}
		}

		class DummyReloadableBizO : DummyBusinessObject, IBusinessObjectReload
		{
			public DummyReloadableBizO(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public SchemaColumn ReloadPerformanceIncreaseColumn => throw new NotImplementedException();

			public object ReloadPerformanceIncreaseColumnValue => throw new NotImplementedException();

			public BusinessObject Reload(BusinessObjectFactory loadingFactory)
			{
				isReloadCalled = true;
				return Factory.New<DummyBusinessObject>();
			}

			public bool IsReloadCalled => isReloadCalled;
			bool isReloadCalled;
		}

		#endregion
	}

	sealed class BusinessObjectFactoryWithoutTransactionTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestFailedSaveOfDeferredDeleteTriggerBizODefersOnResave()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			using (MockProcedures.MockExistingProcedure(connection, TriggerProcedure.SuspendTrigger))
			using (MockProcedures.CreateNewMockTrigger(connection, DummyBizOWithDeferredDeleteTrigger.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(connection, DummyBizOWithDeferredDeleteTrigger.StoredProc))
			using (MockProcedures.MockExistingProcedure(connection, TriggerProcedure.ResumeTrigger))
			{
				var factory = new BusinessObjectFactory(connection);
				var bizO1 = factory.New<DummyBizOWithDeferredDeleteTrigger>();
				factory.Save();
				AssertEquals("Delete trigger should not be suspended when no bizO deleted", 0,
					MockProcedures.ReadValuesPassedToProcedure(connection, TriggerProcedure.SuspendTrigger).Count());

				var service = new TestAfterSaveInTransactionService2();
				factory.ServiceContainer.AddAfterSaveInTransactionService(service);
				bizO1.Delete();

				// Perform Save that fails
				var mockTriggerRunner = new Mock<IDeferredTriggerRunner>();
				IDeferredTriggerRunner realTriggerRunner = new DeferredTriggerRunner();
				using (ObjectFactory.Substitute(mockTriggerRunner.Object))
				{
					mockTriggerRunner.Setup(m => m.DeferAndReturnTriggers(It.IsAny<BusinessObjectFactory>(), It.IsAny<IEnumerable<BusinessObject>>()))
						.Callback<BusinessObjectFactory, IEnumerable<BusinessObject>>((arg1, arg2) =>
						{
							var bizOFactory = arg1;
							var bizOs = arg2;
							var deferredTriggers = realTriggerRunner.DeferAndReturnTriggers(bizOFactory, bizOs);
							AssertContainsExactElementsInAnyOrder("Delete trigger should be suspended when bizO deleted", deferredTriggers.Keys, new[] { DummyBizOWithDeferredDeleteTrigger.StoredProc + "|" });
						})
						.Returns(It.IsAny<IReadOnlyDictionary<string, IReadOnlyCollection<TriggerMetaData>>>());

					AssertExceptionThrown<ZSaveException>(() => factory.Save());
					mockTriggerRunner.Verify();

					AssertEquals("Saving transaction must be rolled back", 0, connection.AppTransactionCount);
					AssertEquals("Dummy bizO must still be in database after rollback", 1, connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.DummyBizo"));
				}

				// Perform a save again and assert that the trigger is deferred once more
				factory.ServiceContainer.RemoveAfterSaveInTransactionService<TestAfterSaveInTransactionService2>();
				factory.Save();

				AssertContainsExactElementsInAnyOrder("Delete trigger should be suspended when bizO deleted", new[] { DummyBizOWithDeferredDeleteTrigger.TriggerName },
					MockProcedures.ReadValuesPassedToProcedure(connection, TriggerProcedure.SuspendTrigger));
				AssertEquals("Dummy bizO must not be in database after completed save", 0, connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.DummyBizo"));
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionService_MultipleInternalSaves_OnlyOneTransaction()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var outerFactory = new BusinessObjectFactory(connection1);
				AssertEquals(0, outerFactory.Load<DummyBusinessObject>(new ZQuery()).Length);

				using (var trans = outerFactory.DelayedTransaction())
				{
					var bizo = outerFactory.New<DummyBusinessObject>();
					bizo.Z0_Number = 42;

					var innerFactory1 = new BusinessObjectFactory(connection1);
					var innerBizo1 = innerFactory1.New<DummyBusinessObject>();
					innerBizo1.Z0_Number = 5;
					innerFactory1.Save();

					var innerFactory2 = new BusinessObjectFactory(connection1);
					var innerBizo2 = innerFactory2.New<DummyBusinessObject>();
					innerBizo2.Z0_Number = 10;
					innerFactory2.Save();

					outerFactory.Save();
					AssertEquals("No transaction until end of scope", 0, new BusinessObjectFactory(connection2).Load<DummyBusinessObject>(new ZQuery()).Length);
					trans.CommitTransaction();
				}

				AssertEquals("All transactions should now be commited", 3, new BusinessObjectFactory(connection2).Load<DummyBusinessObject>(new ZQuery()).Length);
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionService_OuterFactoryHadNoChanges_OnlyOneTransaction()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var outerFactory = new BusinessObjectFactory(connection1);
				AssertEquals(0, outerFactory.Load<DummyBusinessObject>(new ZQuery()).Length);

				using (var trans = outerFactory.DelayedTransaction())
				{
					var innerFactory1 = new BusinessObjectFactory(connection1);
					var innerBizo1 = innerFactory1.New<DummyBusinessObject>();
					innerBizo1.Z0_Number = 5;
					innerFactory1.Save();

					var innerFactory2 = new BusinessObjectFactory(connection1);
					var innerBizo2 = innerFactory2.New<DummyBusinessObject>();
					innerBizo2.Z0_Number = 10;
					innerFactory2.Save();

					outerFactory.Save();
					AssertEquals("No transaction until end of scope", 0, new BusinessObjectFactory(connection2).Load<DummyBusinessObject>(new ZQuery()).Length);
					trans.CommitTransaction();
				}

				AssertEquals("All transactions should now be commited", 2, new BusinessObjectFactory(connection2).Load<DummyBusinessObject>(new ZQuery()).Length);
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionService_HandlesReconnect_WhenTransactionNotYetStarted()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var outerFactory = new BusinessObjectFactory(connection1);
				AssertEquals(0, outerFactory.Load<DummyBusinessObject>(new ZQuery()).Length);

				using (var trans = outerFactory.DelayedTransaction())
				{
					connection1.CloseConnection();

					var bizo = outerFactory.New<DummyBusinessObject>();
					bizo.Z0_Number = 42;

					var innerFactory1 = new BusinessObjectFactory(connection1);
					var innerBizo1 = innerFactory1.New<DummyBusinessObject>();
					innerBizo1.Z0_Number = 5;
					innerFactory1.Save();

					var innerFactory2 = new BusinessObjectFactory(connection1);
					var innerBizo2 = innerFactory2.New<DummyBusinessObject>();
					innerBizo2.Z0_Number = 10;
					innerFactory2.Save();

					outerFactory.Save();
					AssertEquals("No transaction until end of scope", 0, new BusinessObjectFactory(connection2).Load<DummyBusinessObject>(new ZQuery()).Length);
					trans.CommitTransaction();
				}

				AssertEquals("All transactions should now be commited", 3, new BusinessObjectFactory(connection2).Load<DummyBusinessObject>(new ZQuery()).Length);
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionService_Fails_WhenTransactionHasBeenStarted()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var outerFactory = new BusinessObjectFactory(connection1);
				AssertEquals(0, outerFactory.Load<DummyBusinessObject>(new ZQuery()).Length);

				using (var trans = outerFactory.DelayedTransaction())
				{
					var bizo = outerFactory.New<DummyBusinessObject>();
					bizo.Z0_Number = 42;

					var innerFactory1 = new BusinessObjectFactory(connection1);
					var innerBizo1 = innerFactory1.New<DummyBusinessObject>();
					innerBizo1.Z0_Number = 5;
					innerFactory1.Save();

					var innerFactory2 = new BusinessObjectFactory(connection1);
					var innerBizo2 = innerFactory2.New<DummyBusinessObject>();
					innerBizo2.Z0_Number = 10;
					innerFactory2.Save();

					connection1.CloseConnection();

					AssertExceptionThrown<InvalidOperationException>(() => outerFactory.Save());
				}

				AssertEquals("No transaction should be commited", 0, new BusinessObjectFactory(connection2).Load<DummyBusinessObject>(new ZQuery()).Length);
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionService_MultipleInternalSaves_OnlyOneTransaction_Nested()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var outerFactory = new BusinessObjectFactory(connection1);
				AssertEquals(0, outerFactory.Load<DummyBusinessObject>(new ZQuery()).Length);

				using (var trans = outerFactory.DelayedTransaction())
				{
					var bizo = outerFactory.New<DummyBusinessObject>();
					bizo.Z0_Number = 42;

					var innerFactory1 = new BusinessObjectFactory(connection1);
					using (innerFactory1.DelayedTransaction())
					{
						var innerBizo1 = innerFactory1.New<DummyBusinessObject>();
						innerBizo1.Z0_Number = 5;
						innerFactory1.Save();
					}

					var innerFactory2 = new BusinessObjectFactory(connection1);
					var innerBizo2 = innerFactory2.New<DummyBusinessObject>();
					innerBizo2.Z0_Number = 10;
					innerFactory2.Save();

					outerFactory.Save();
					AssertEquals("No transaction until end of outermost scope", 0, new BusinessObjectFactory(connection2).Load<DummyBusinessObject>(new ZQuery()).Length);
					trans.CommitTransaction();
				}

				AssertEquals("All transactions should now be commited", 3, new BusinessObjectFactory(connection2).Load<DummyBusinessObject>(new ZQuery()).Length);
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionService_MultipleInternalSaves_OneRollback_AllRollback()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var outerFactory = new BusinessObjectFactory(connection1);
				AssertEquals(0, outerFactory.Load<DummyBusinessObject>(new ZQuery()).Length);

				using (var trans = outerFactory.DelayedTransaction())
				{
					var bizo = outerFactory.New<DummyBusinessObject>();
					bizo.Z0_Number = 42;

					var innerFactory1 = new BusinessObjectFactory(connection1);
					var innerBizo1 = innerFactory1.New<DummyBusinessObject>();
					innerBizo1.Z0_Number = 5;
					innerFactory1.Save();

					var innerFactory2 = new BusinessObjectFactory(connection1);
					var innerBizo2 = innerFactory2.New<DummyBusinessObject>();
					innerBizo2.Z0_Number = 10;
					innerFactory2.Save();

					((INeedRow)bizo).Row[DummyBusinessObject.Schema.PK] = innerBizo1.PK.ToGuid();

					AssertExceptionThrown<ZSaveException>(outerFactory.Save);
					AssertExceptionThrown<TransactionException>(trans.CommitTransaction);
				}

				AssertEquals("All saves should be rolled back", 0, new BusinessObjectFactory(connection2).Load<DummyBusinessObject>(new ZQuery()).Length);

				using (var trans = new BusinessObjectFactory(connection1).DelayedTransaction())
				{
					Assert(!trans.IsRollingback);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionService_MultipleInternalSaves_ExceptionRollsBackAll()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var outerFactory = new BusinessObjectFactory(connection1);
				AssertEquals(0, outerFactory.Load<DummyBusinessObject>(new ZQuery()).Length);

				try
				{
					using (outerFactory.DelayedTransaction())
					{
						var bizo = outerFactory.New<DummyBusinessObject>();
						bizo.Z0_Number = 42;

						var innerFactory1 = new BusinessObjectFactory(connection1);
						var innerBizo1 = innerFactory1.New<DummyBusinessObject>();
						innerBizo1.Z0_Number = 5;
						innerFactory1.Save();

						var innerFactory2 = new BusinessObjectFactory(connection1);
						var innerBizo2 = innerFactory2.New<DummyBusinessObject>();
						innerBizo2.Z0_Number = 10;
						innerFactory2.Save();

						throw new InvalidOperationException();
					}
				}
				catch (InvalidOperationException)
				{
				}

				AssertEquals("All saves should be rolled back", 0, new BusinessObjectFactory(connection2).Load<DummyBusinessObject>(new ZQuery()).Length);
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionService_MultipleInternalSaves_ExceptionRollsBackAll_Nested()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var outerFactory = new BusinessObjectFactory(connection1);
				AssertEquals(0, outerFactory.Load<DummyBusinessObject>(new ZQuery()).Length);

				using (outerFactory.DelayedTransaction())
				{
					var bizo = outerFactory.New<DummyBusinessObject>();
					bizo.Z0_Number = 42;

					var innerFactory1 = new BusinessObjectFactory(connection1);
					var innerBizo1 = innerFactory1.New<DummyBusinessObject>();
					innerBizo1.Z0_Number = 5;
					innerFactory1.Save();

					var innerFactory2 = new BusinessObjectFactory(connection1);
					try
					{
						using (innerFactory2.DelayedTransaction())
						{
							var innerBizo2 = innerFactory2.New<DummyBusinessObject>();
							innerBizo2.Z0_Number = 10;
							((INeedRow)innerBizo2).Row[DummyBusinessObject.Schema.PK] = innerBizo1.PK.ToGuid();
							innerFactory2.Save();
							Fail("Where is that exception?");
						}
					}
					catch (ZSaveException)
					{
					}

					outerFactory.Save();
				}

				AssertEquals("All saves should be rolled back", 0, new BusinessObjectFactory(connection2).Load<DummyBusinessObject>(new ZQuery()).Length);
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionService_MultipleInternalSaves_SqlTransactionLocksAreScopedToOuterTransaction()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var outerFactory = new BusinessObjectFactory(connection1);
				AssertEquals(0, outerFactory.Load<DummyBusinessObject>(new ZQuery()).Length);

				SqlApplicationLock bizoMainLock = null;
				SqlApplicationLock bizo1Lock = null;
				SqlApplicationLock bizo2Lock = null;
				using (var trans = outerFactory.DelayedTransaction())
				{
					var bizoMain = outerFactory.New<DummyBusinessObject>();
					bizoMain.Z0_Number = 42;
					outerFactory.Saving += (_) =>
					{
						connection1.TryGetLock("bizoMainLock", out bizoMainLock);
						outerFactory.AddSqlLockToTransaction(bizoMainLock);
					};

					var innerFactory1 = new BusinessObjectFactory(connection1);
					var innerBizo1 = innerFactory1.New<DummyBusinessObject>();
					innerBizo1.Z0_Number = 5;
					innerFactory1.Saving += (_) =>
					{
						connection1.TryGetLock("bizo1Lock", out bizo1Lock);
						innerFactory1.AddSqlLockToTransaction(bizo1Lock);
					};
					innerFactory1.Save();
					Assert("Lock needs to be held for length of DelayedTransaction", bizo1Lock.IsHoldingLock());

					var innerFactory2 = new BusinessObjectFactory(connection1);
					var innerBizo2 = innerFactory2.New<DummyBusinessObject>();
					innerBizo2.Z0_Number = 10;
					innerFactory2.Saving += (_) =>
					{
						connection1.TryGetLock("bizo2Lock", out bizo2Lock);
						innerFactory2.AddSqlLockToTransaction(bizo2Lock);
					};
					innerFactory2.Save();
					Assert("Lock needs to be held for length of DelayedTransaction", bizo2Lock.IsHoldingLock());

					outerFactory.Save();
					Assert("Lock needs to be held for length of DelayedTransaction", bizoMainLock.IsHoldingLock());
					trans.CommitTransaction();
				}

				AssertEquals("Lock needs to be released at end of DelayedTransaction", false, bizo1Lock.IsHoldingLock());
				AssertEquals("Lock needs to be released at end of DelayedTransaction", false, bizo2Lock.IsHoldingLock());
				AssertEquals("Lock needs to be released at end of DelayedTransaction", false, bizoMainLock.IsHoldingLock());
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionService_MultipleInternalSaves_SqlTransactionLocksAreScopedToOuterTransaction_Nested()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var outerFactory = new BusinessObjectFactory(connection1);
				AssertEquals(0, outerFactory.Load<DummyBusinessObject>(new ZQuery()).Length);

				SqlApplicationLock bizoMainLock = null;
				SqlApplicationLock bizo1Lock = null;
				SqlApplicationLock bizo2Lock = null;
				using (var trans = outerFactory.DelayedTransaction())
				{
					var bizoMain = outerFactory.New<DummyBusinessObject>();
					bizoMain.Z0_Number = 42;
					outerFactory.Saving += (_) =>
					{
						connection1.TryGetLock("bizoMainLock", out bizoMainLock);
						outerFactory.AddSqlLockToTransaction(bizoMainLock);
					};

					var innerFactory1 = new BusinessObjectFactory(connection1);
					using (var trans2 = innerFactory1.DelayedTransaction())
					{
						var innerBizo1 = innerFactory1.New<DummyBusinessObject>();
						innerBizo1.Z0_Number = 5;
						innerFactory1.Saving += (_) =>
						{
							connection1.TryGetLock("bizo1Lock", out bizo1Lock);
							innerFactory1.AddSqlLockToTransaction(bizo1Lock);
						};
						innerFactory1.Save();
						trans2.CommitTransaction();
					}
					Assert("Lock needs to be held for length of DelayedTransaction", bizo1Lock.IsHoldingLock());

					var innerFactory2 = new BusinessObjectFactory(connection1);
					var innerBizo2 = innerFactory2.New<DummyBusinessObject>();
					innerBizo2.Z0_Number = 10;
					innerFactory2.Saving += (_) =>
					{
						connection1.TryGetLock("bizo2Lock", out bizo2Lock);
						innerFactory2.AddSqlLockToTransaction(bizo2Lock);
					};
					innerFactory2.Save();
					Assert("Lock needs to be held for length of DelayedTransaction", bizo2Lock.IsHoldingLock());

					outerFactory.Save();
					Assert("Lock needs to be held for length of DelayedTransaction", bizoMainLock.IsHoldingLock());
					trans.CommitTransaction();
				}

				AssertEquals("Lock needs to be released at end of DelayedTransaction", false, bizo1Lock.IsHoldingLock());
				AssertEquals("Lock needs to be released at end of DelayedTransaction", false, bizo2Lock.IsHoldingLock());
				AssertEquals("Lock needs to be released at end of DelayedTransaction", false, bizoMainLock.IsHoldingLock());
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionService_MultipleInternalSaves_SqlTransactionLocksAreReleasedOnRollback()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var outerFactory = new BusinessObjectFactory(connection1);
				AssertEquals(0, outerFactory.Load<DummyBusinessObject>(new ZQuery()).Length);

				SqlApplicationLock bizoMainLock = null;
				SqlApplicationLock bizo1Lock = null;
				SqlApplicationLock bizo2Lock = null;
				using (outerFactory.DelayedTransaction())
				{
					var bizoMain = outerFactory.New<DummyBusinessObject>();
					bizoMain.Z0_Number = 42;
					outerFactory.Saving += (_) =>
					{
						connection1.TryGetLock("bizoMainLock", out bizoMainLock);
						outerFactory.AddSqlLockToTransaction(bizoMainLock);
					};

					var innerFactory1 = new BusinessObjectFactory(connection1);
					var innerBizo1 = innerFactory1.New<DummyBusinessObject>();
					innerBizo1.Z0_Number = 5;
					innerFactory1.Saving += (_) =>
					{
						connection1.TryGetLock("bizo1Lock", out bizo1Lock);
						innerFactory1.AddSqlLockToTransaction(bizo1Lock);
					};
					innerFactory1.Save();
					Assert("Lock needs to be held for length of DelayedTransaction", bizo1Lock.IsHoldingLock());

					var innerFactory2 = new BusinessObjectFactory(connection1);
					var innerBizo2 = innerFactory2.New<DummyBusinessObject>();
					innerBizo2.Z0_Number = 10;
					innerFactory2.Saving += (_) =>
					{
						connection1.TryGetLock("bizo2Lock", out bizo2Lock);
						innerFactory2.AddSqlLockToTransaction(bizo2Lock);
					};
					innerFactory2.Save();
					Assert("Lock needs to be held for length of DelayedTransaction", bizo2Lock.IsHoldingLock());

					((INeedRow)bizoMain).Row[DummyBusinessObject.Schema.PK] = innerBizo1.PK.ToGuid();
					try
					{
						outerFactory.Save();
						Fail("Where is the exception?");
					}
					catch (ZSaveException)
					{
					}

					Assert("Lock needs to be held for length of DelayedTransaction", bizoMainLock.IsHoldingLock());
				}

				AssertEquals("Lock needs to be released at end of DelayedTransaction", false, bizo1Lock.IsHoldingLock());
				AssertEquals("Lock needs to be released at end of DelayedTransaction", false, bizo2Lock.IsHoldingLock());
				AssertEquals("Lock needs to be released at end of DelayedTransaction", false, bizoMainLock.IsHoldingLock());
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionService_MultipleInternalSaves_SqlTransactionLocksAreReleasedOnException()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var outerFactory = new BusinessObjectFactory(connection1);
				AssertEquals(0, outerFactory.Load<DummyBusinessObject>(new ZQuery()).Length);

				SqlApplicationLock bizoMainLock = null;
				SqlApplicationLock bizo1Lock = null;
				SqlApplicationLock bizo2Lock = null;
				try
				{
					using (outerFactory.DelayedTransaction())
					{
						var bizoMain = outerFactory.New<DummyBusinessObject>();
						bizoMain.Z0_Number = 42;
						outerFactory.Saving += (_) =>
						{
							connection1.TryGetLock("bizoMainLock", out bizoMainLock);
							outerFactory.AddSqlLockToTransaction(bizoMainLock);
						};

						var innerFactory1 = new BusinessObjectFactory(connection1);
						var innerBizo1 = innerFactory1.New<DummyBusinessObject>();
						innerBizo1.Z0_Number = 5;
						innerFactory1.Saving += (_) =>
						{
							connection1.TryGetLock("bizo1Lock", out bizo1Lock);
							innerFactory1.AddSqlLockToTransaction(bizo1Lock);
						};
						innerFactory1.Save();
						Assert("Lock needs to be held for length of DelayedTransaction", bizo1Lock.IsHoldingLock());

						var innerFactory2 = new BusinessObjectFactory(connection1);
						var innerBizo2 = innerFactory2.New<DummyBusinessObject>();
						innerBizo2.Z0_Number = 10;
						innerFactory2.Saving += (_) =>
						{
							connection1.TryGetLock("bizo2Lock", out bizo2Lock);
							innerFactory2.AddSqlLockToTransaction(bizo2Lock);
						};
						innerFactory2.Save();
						Assert("Lock needs to be held for length of DelayedTransaction", bizo2Lock.IsHoldingLock());

						((INeedRow)bizoMain).Row[DummyBusinessObject.Schema.PK] = innerBizo1.PK.ToGuid();
						outerFactory.Save();
						Fail("Where is the exception?");
					}
				}
				catch (ZSaveException)
				{
				}

				AssertEquals("Lock needs to be released at end of DelayedTransaction", false, bizo1Lock.IsHoldingLock());
				AssertEquals("Lock needs to be released at end of DelayedTransaction", false, bizo2Lock.IsHoldingLock());
				AssertEquals("Lock needs to be released at end of DelayedTransaction", false, bizoMainLock.IsHoldingLock());
			}
		}

		[UseSnapshotProtection]
		public void TestDelayedTransactionDoesntQuietlyRollback()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(connection);
				using (var delayedTransaction = factory.DelayedTransaction())
				{
					{
						//Simulate some process saving and causing the transation to require rollback
						var dummy1 = factory.New<DummyBusinessObject>();
						factory.Save();
						connection.RollbackTransaction();
					}

					AssertEquals(true, delayedTransaction.IsRollingback);
					AssertExceptionThrown<TransactionException>(delayedTransaction.CommitTransaction);
				}
			}
		}

		#region TestAfterSaveInTransactionService

		[UseSnapshotProtection]
		public void TestAfterSaveInTransactionService()
		{
			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(conn);
				var bizo1 = factory.New<DummyForAfterSaveInTransactionAction>();
				var bizo2 = factory.New<DummyForAfterSaveInTransactionAction>();
				var service1 = new TestAfterSaveInTransactionService1();
				var service2 = new TestAfterSaveInTransactionService2();
				factory.ServiceContainer.AddAfterSaveInTransactionService(service1);
				factory.ServiceContainer.AddAfterSaveInTransactionService(service2);
				AssertExceptionThrown<ZSaveException>(() => factory.Save());

				AssertEquals("AfterSaveInTransactionService must be called once during Factory.Save", 1, service1.CallAmount);
				AssertEquals("AfterSaveInTransactionService must be called once during Factory.Save", 1, service2.CallAmount);
				AssertEquals("Saving transaction must be rolled back", 0, conn.AppTransactionCount);
				AssertEquals("Dummy bizOs must not be in database after rollback", 0, conn.ExecuteScalar("SELECT COUNT(*) FROM dbo.DummyBizo"));
			}
		}

		class DummyForAfterSaveInTransactionAction : DummyBusinessObject
		{
			public DummyForAfterSaveInTransactionAction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}

		class TestAfterSaveInTransactionService1 : IAfterSaveInTransactionService
		{
			public int CallAmount { get; private set; }

			public void DoFinalCheckBeforeCommit(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				CallAmount++;
				var connection = ((IDbConnected)businessObjectsInOnSavingOrder.First().Factory).Connection;
				AssertEquals("Transaction must not yet be committed", 1, connection.AppTransactionCount);
				AssertEquals("Dummy bizOs must be already saved to database", 2, connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.DummyBizo"));
			}
		}

		class TestAfterSaveInTransactionService2 : IAfterSaveInTransactionService
		{
			public int CallAmount { get; private set; }

			public void DoFinalCheckBeforeCommit(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				CallAmount++;
				var dummyBizO = businessObjectsInOnSavingOrder.First();
				throw new ZDataException(null, ((INeedRow)dummyBizO).Row, ((IDbConnected)dummyBizO.Factory).Connection);
			}
		}

		#endregion
	}

	sealed class DataRefreshTest : TestCase
	{
		public void TestUpdateForDataRefresh_ShouldNotThrow_WhenColumnDoesNotAcceptNull()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();

			var items = dummy.Row.ItemArray;

			var anotherDecimalColumnIndex = dummy.Table.Columns.IndexOf(nameof(dummy.Z0_AnotherDecimal));
			var anotherNumberColumnIndex = dummy.Table.Columns.IndexOf(nameof(dummy.Z0_AnotherNumber));
			var descriptionColumnIndex = dummy.Table.Columns.IndexOf(nameof(dummy.Z0_Description));

			items[anotherDecimalColumnIndex] = null;
			items[anotherNumberColumnIndex] = DBNull.Value;
			items[descriptionColumnIndex] = "I'm valid!";

			AssertNoExceptionThrown(() => dummy.PerformRefresh(dummy.PK, items));
			AssertEquals("I'm valid!", dummy.Z0_Description);
		}

		[UseSnapshotProtection]
		public void TestUpdateForDataRefresh_ShouldNotThrow_WhenBusinessObjectIsDeleted()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();

			factory.Save();

			var items = dummy.Row.ItemArray;

			var anotherDecimalColumnIndex = dummy.Table.Columns.IndexOf(nameof(dummy.Z0_AnotherDecimal));
			var anotherNumberColumnIndex = dummy.Table.Columns.IndexOf(nameof(dummy.Z0_AnotherNumber));

			dummy.Table.RowChanged += (sender, e) =>
			{
				dummy.Delete();
			};

			items[anotherDecimalColumnIndex] = 99m;
			items[anotherNumberColumnIndex] = 99;

			AssertNoExceptionThrown(() => dummy.PerformRefresh(dummy.PK, items));
		}

		[UseSnapshotProtection]
		public void TestUpdateForDataRefresh_ShouldNotThrow_WhenBusinessObjectIsDeletedInOtherThread()
		{
			ErrorReporter.Clear();

			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "DUM";
			dummy.Z0_Description = "Description";
			var autoResetEvent = new AutoResetEvent(false);

			factory.Save();

			var items = dummy.Row.ItemArray;

			var anotherDecimalColumnIndex = dummy.Table.Columns.IndexOf(nameof(dummy.Z0_AnotherDecimal));
			var anotherNumberColumnIndex = dummy.Table.Columns.IndexOf(nameof(dummy.Z0_AnotherNumber));

			items[anotherDecimalColumnIndex] = 99m;
			items[anotherNumberColumnIndex] = 99;

			DummyBusinessObject dummyInThread = null;
			var synchronizationContext = new SingleThreadSynchronizationContext();

			var task = Task.Run(() =>
			{
				SynchronizationContext.SetSynchronizationContext(synchronizationContext);

				using (Db.DisposableActionForDbConnection())
				{
					var factoryInThread = new BusinessObjectFactory();
					dummyInThread = factoryInThread.Load<DummyBusinessObject>(dummy.PK);

					dummy.Table.RowChanged += (sender, e) =>
					{
						dummy.Delete();
					};

					AssertNoExceptionThrown(() => dummyInThread.PerformRefresh(dummy.PK, items));
					synchronizationContext.RunOnCurrentThread();
				}
			});

			factory.Save();

			synchronizationContext.Complete();
			task.Wait();

			AssertNull(ErrorReporter.LastExceptionReported);
		}

		[UseSnapshotProtection]
		public void TestUpdateThreadSentrySyncContext()
		{
			var factory = new BusinessObjectFactory();
			Exception exInThread = null;
			var stepFinished = 0;

			var thread = new Thread(() =>
			{
				try
				{
					try
					{
						using (Db.DisposableActionForDbConnection())
						{
							var dummy = factory.New<DummyBusinessObject>();
							dummy.Z0_Code = "CDA";
							factory.Save();
						}
					}
					finally
					{
						stepFinished = 1;
						AssertContains("Attempted to access an object owned by another thread", ErrorReporter.LastMessageReported);
						ErrorReporter.Clear();
					}

					while (stepFinished != 2)
					{
						Thread.Sleep(100);
					}

					factory.ThreadSentry.TakeThreadOwnership();
				}
				catch (Exception ex)
				{
					exInThread = ex;
				}
			});

			thread.Start();

			while (stepFinished != 1)
			{
				Thread.Sleep(100);
			}

			if (exInThread != null)
			{
				thread.Join();
				Assert(exInThread.ToString(), false);
			}

			factory.ThreadSentry.RelinquishThreadOwnership();
			stepFinished = 2;

			thread.Join();

			if (exInThread != null)
			{
				Assert(exInThread.ToString(), false);
			}
		}

		[UseSnapshotProtection]
		public void TestUpdateForDataRefreshCrossThreadWithoutSyncContext()
		{
			var factory = new BusinessObjectFactory();

			var dummy1 = factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "";
			dummy1.Z0_Description = "";

			factory.Save();

			dummy1.Z0_Code = "Code";
			dummy1.Z0_Description = "Desc";

			BusinessObjectFactory factory2;
			DummyBusinessObject dummy2 = null;
			var t = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					factory2 = new BusinessObjectFactory();
					dummy2 = factory2.Load<DummyBusinessObject>(dummy1.PK);
				}
			}
				);
			t.Start();
			t.Join();

			dummy2.UpdateForDataRefresh(dummy1);
			AssertEquals("Code", "", dummy2.Z0_Code);
		}

		class SingleThreadSynchronizationContext : SynchronizationContext
		{
			readonly Queue<(SendOrPostCallback, object)> _queue = new();
			readonly AutoResetEvent _workItemsWaiting = new(false);
			volatile bool _done;

			public override void Post(SendOrPostCallback d, object state)
			{
				lock (_queue)
				{
					_queue.Enqueue((d, state));
				}
				_workItemsWaiting.Set();
			}

			public void RunOnCurrentThread()
			{
				while (!_done)
				{
					(SendOrPostCallback, object) work = default;
					lock (_queue)
					{
						if (_queue.Count > 0)
						{
							work = _queue.Dequeue();
						}
						else
						{
							work = default;
						}
					}
					if (work.Item1 != null)
					{
						work.Item1(work.Item2);
					}
					else
					{
						_workItemsWaiting.WaitOne(5000);
					}
				}
			}

			public void Complete() => _done = true;
		}

		[UseSnapshotProtection]
		public void TestUpdateForDataRefreshCrossThreadSynchronizationContext_ShouldNotReportException()
		{
			ErrorReporter.Clear();
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "DUM";
			dummy.Z0_Description = "Description";

			var autoResetEvent = new AutoResetEvent(false);

			factory.Save();

			dummy.Z0_Description = "Changed";

			BusinessObjectFactory factoryInThread = null;
			DummyBusinessObject dummyInThread = null;
			var synchronizationContext = new SingleThreadSynchronizationContext();
			Async.DefaultAsyncStrategy.Get().DoAsyncAsThread(() =>
			{
				SynchronizationContext.SetSynchronizationContext(synchronizationContext);
				using (Db.DisposableActionForDbConnection())
				{
					factoryInThread = new BusinessObjectFactory();
					dummyInThread = factoryInThread.Load<DummyBusinessObject>(dummy.PK);

					dummyInThread.BeforeUpdatedByDataRefresh += (_, __) =>
					{
						autoResetEvent.Set();
						throw new NullReferenceException("Oh no!");
					};

					autoResetEvent.Set();
					synchronizationContext.RunOnCurrentThread();
				}
			}, ApartmentState.STA);

			autoResetEvent.WaitOne(5000);
			autoResetEvent.Reset();

			dummyInThread.UpdateForDataRefresh(dummy);

			autoResetEvent.WaitOne(5000);

			Thread.Sleep(800); // give it time to report exception

			AssertNull(ErrorReporter.LastExceptionReported);

			synchronizationContext.Complete();
			ErrorReporter.Clear();
		}

		[UseSnapshotProtection]
		public void TestDeleteForDataRefreshCrossThreadSynchronizationContext_ShouldNotReportException()
		{
			ErrorReporter.Clear();
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "DUM";
			dummy.Z0_Description = "Description";

			var autoResetEvent = new AutoResetEvent(false);

			factory.Save();

			dummy.Z0_Description = "Changed";

			BusinessObjectFactory factoryInThread = null;
			DummyBusinessObject dummyInThread = null;
			var synchronizationContext = new SingleThreadSynchronizationContext();
			Async.DefaultAsyncStrategy.Get().DoAsyncAsThread(() =>
			{
				SynchronizationContext.SetSynchronizationContext(synchronizationContext);
				using (Db.DisposableActionForDbConnection())
				{
					factoryInThread = new BusinessObjectFactory();
					dummyInThread = factoryInThread.Load<DummyBusinessObject>(dummy.PK);

					dummyInThread.BeforeDeleteByDataRefresh += (_, __) =>
					{
						autoResetEvent.Set();
						throw new NullReferenceException("Oh no!");
					};

					autoResetEvent.Set();
					synchronizationContext.RunOnCurrentThread();
				}
			}, ApartmentState.STA);

			autoResetEvent.WaitOne(5000);
			autoResetEvent.Reset();

			dummyInThread.DeleteForDataRefreshThreadSafe(dummy);

			autoResetEvent.WaitOne(5000);

			Thread.Sleep(800); // give it time to report exception

			AssertNull(ErrorReporter.LastExceptionReported);

			synchronizationContext.Complete();
			ErrorReporter.Clear();
		}

		[UseSnapshotProtection]
		[RequiresSTA]
		public void TestUpdateForDataRefreshCrossThreadWithSynchronizationContext()
		{
			var factory = new BusinessObjectFactory();

			var dummy1 = factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "";
			factory.Save();

			BusinessObjectFactory factory2 = null;
			DummyBusinessObject dummy2 = null;
			var setupComplete = new AutoResetEvent(false);
			var threadExitSignal = new AutoResetEvent(false);
			var synchronizationContext = new SingleThreadSynchronizationContext();
			Async.DefaultAsyncStrategy.Get().DoAsyncAsThread(() =>
			{
				SynchronizationContext.SetSynchronizationContext(synchronizationContext);
				using (Db.DisposableActionForDbConnection())
				{
					factory2 = new BusinessObjectFactory();
					dummy2 = factory2.Load<DummyBusinessObject>(dummy1.PK);
					setupComplete.Set();
					synchronizationContext.RunOnCurrentThread();
				}
			}, ApartmentState.STA);

			setupComplete.WaitOne(5000);

			dummy1.Z0_Code = "Code";
			AssertEquals("Code", "", dummy2.Z0_Code);
			dummy2.UpdateForDataRefresh(dummy1);

			Thread.Sleep(3000); // give it time to go through message pump
			AssertEquals("Code", "Code", dummy2.Z0_Code);
			synchronizationContext.Complete();
		}

		[UseSnapshotProtection]
		public void TestAddDiagnosisForFactoryQueryCache()
		{
			ErrorReporter.Clear();
			var factory = new BusinessObjectFactory();
			var dummyBo = factory.NewWithValidTestData<DummyBusinessObject>();
			factory.Save();

			var factoryForReload = new BusinessObjectFactory() { RefreshEnabled = false };
			var sQLFilter = new ZQuery(DummyBizoSchema.PK, dummyBo.PK);
			foreach (var compositePart in sQLFilter.GetCompositeParts())
			{
				factoryForReload.RowFactory.QueryCache.Store("DummyBizo", compositePart);
			}

			using (factoryForReload.AddDiagnosisForFactoryQueryCacheWhenLoadingEnvCurrentBranchOrCompany())
			{
				var bizoReload = factoryForReload.Load<DummyBusinessObject>(dummyBo.PK);
				AssertNull(bizoReload);
				AssertContains(@$"filterIsPKAndInRow: False
queryCacheIsCached: True
filterIsPKAndInRow_UberFactory: False
queryCacheIsCached_UberFactory: False
filter: Z0_PK = '{dummyBo.PK}'", ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();

				var bizoReload2 = factoryForReload.Load<DummyBusinessObject>(dummyBo.PK);
				AssertNull(bizoReload);
				AssertEquals("Add Diagnosis For Factory QueryCache only once", string.Empty, ErrorReporter.LastMessageReported);
			}
		}
	}

	class BusinessObjectFactoryForConcurrencyTest : BusinessObjectFactory
	{
		public BusinessObjectFactoryForConcurrencyTest()
		{
		}

		public void cache9Objects(BusinessObject dummy)
		{
			for (var i = 0; i < 9; i++)
			{
				last10BizObjsLoaded.Add(dummy);
			}
		}

		public List<BusinessObject> last10BizObjsLoadedExposed
		{
			get
			{
				return last10BizObjsLoaded;
			}
		}

		public bool addACache(BusinessObject bizObject)
		{
			try
			{
				UpdateMRUCache(bizObject);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return false;
			}
			return true;
		}

		public new void UpdateMRUCache(BusinessObject bizObject)
		{
			base.UpdateMRUCache(bizObject);
		}

		public void MakeCacheMemberNullForIndex(int index)
		{
			last10BizObjsLoaded[0] = null;
		}
	}

	#region MultiDB tests

	sealed class BizOFactoryMultiDBTest : TestCase
	{
		public void TestWithExplicitDBName()
		{
			string testDbName = "MultiDBTestDatabase";

			try
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					adminConnection.ExecuteNonQuery("IF EXISTS(SELECT null from sys.databases where name = '" + testDbName + "') drop database " + testDbName);
					adminConnection.CreateDatabase(testDbName);
				}

				// Create DummyBizo table in other DB
				DummyTableCreator.CreateDummyTableInAnotherDb(testDbName);

				BusinessObjectFactory factory = new BusinessObjectFactory(testDbName);
				factory.RefreshEnabled = false;
				DummyBusinessObject dummy = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject));
				dummy.Z0_Number = 555;

				factory.Save();

				BusinessObjectFactory factory2 = new BusinessObjectFactory(testDbName);
				AssertEquals("Should find one dummy in the new db", 1, factory2.GetDatabaseCount(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Number, 555)));
				DummyBusinessObject dummyReloaded = (DummyBusinessObject)factory2.Load(typeof(DummyBusinessObject), dummy.PK);

				AssertEquals("Saved and loaded OK", dummy.Z0_Number, dummyReloaded.Z0_Number);

				BusinessObjectFactory factory3 = new BusinessObjectFactory();
				DummyBusinessObject noDummy = (DummyBusinessObject)factory3.Load(typeof(DummyBusinessObject), dummy.PK);
				AssertNull("Saved record should not exist in the MainDB", noDummy);
			}
			finally
			{
				using (DbConnection adminConnection = Db.NewAdminConnection())
				{
					adminConnection.ExecuteNonQuery("drop database " + testDbName);
				}
				DbCommitTracker.Ignore(testDbName);
			}
		}
	}

	#endregion

	#region DataRefreshIntegrationTests

	sealed class BusinessObjectFactoryDataRefreshTest : TestCaseWithDummy
	{
		public void TestDataRefreshBusPublishIsBasedOnHasChangesWithNonOverridenIsSavedByFactory()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = (DummyBusinessObject)factory1.New(typeof(DummyBusinessObject));
			dummy1.Z0_Description = "G'day Henry";
			factory1.Save();

			dummy1.Z0_Description = "Oh no!";
			dummy1.HasChanges = false;

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummy2 = (DummyBusinessObject)factory2.Load(typeof(DummyBusinessObject), dummy1.PK);
			dummy2.Z0_AnotherNumber = 34234;

			factory1.Save();
			factory2.Save(); // will have concurrency error if Dummy1 was saved to DB but not published by data refresh bus

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			DummyBusinessObject dummy3 = (DummyBusinessObject)factory3.Load(typeof(DummyBusinessObject), dummy1.PK);
			AssertEquals("Z0_Description", "G'day Henry", dummy3.Z0_Description);
			AssertEquals("Z0_AnotherNumber", 34234, dummy3.Z0_AnotherNumber);

			dummy1.Delete();
			dummy1.HasChanges = false;

			factory1.Save();
			factory2.Save();

			Assert("Dummy1 has been deleted but has changes is false so it shouldn't be published", dummy1.IsDeleted);
			Assert("Dummy1 has been deleted but has changes is false so it shouldn't be published", !dummy2.IsDeleted);
			Assert("Dummy1 has been deleted but has changes is false so it shouldn't be published", !dummy3.IsDeleted);
			AssertNotNull("Dummy1 has been deleted but has changes is false so it shouldn't be deleted in the DB",
							new BusinessObjectFactory().Load(typeof(DummyBusinessObject), dummy1.PK));
		}

		public void TestDataRefreshBusWithOverridenIsSavedByFactory()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = (DummyBusinessObject)factory1.New(typeof(DummyBusinessObject));
			dummy1.Z0_Description = "G'day Henry";
			factory1.Save();

			dummy1.Z0_Description = "Oh no!";
			dummy1.OverrideIsSavedByFactory = true;
			dummy1.IsSavedByFactoryOverride = false;

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummy2 = (DummyBusinessObject)factory2.Load(typeof(DummyBusinessObject), dummy1.PK);

			factory1.Save();
			AssertEquals("Should not be published", "G'day Henry", dummy2.Z0_Description);

			dummy1.IsSavedByFactoryOverride = true;
			factory1.Save();
			AssertEquals("Should be published", "Oh no!", dummy2.Z0_Description);
		}

		public void TestDataRefreshUpdate()
		{
			ZGuid pK = Dummy.PK;
			Dummy.Z0_Code = "abc";
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummy2 = factory2.Load(typeof(DummyBusinessObject), pK) as DummyBusinessObject;
			AssertEquals("Dummy2.Z0_Code", "abc", dummy2.Z0_Code.ToString().Trim());

			Dummy.Z0_Code = "cba";
			Factory.Save();
			AssertEquals("Dummy2.Z0_Code", "cba", dummy2.Z0_Code.ToString().Trim());

			dummy2.Z0_Code = "zzz";
			factory2.Save();
			AssertEquals("Dummy.Z0_Code", "zzz", Dummy.Z0_Code.ToString().Trim());

			Dummy.Z0_Code = "abc"; // try again.
			Factory.Save();
			AssertEquals("Dummy2.Z0_Code", "abc", dummy2.Z0_Code.ToString().Trim());
		}

		public void TestSubscribeToDataRefreshOnInstantiation()
		{
			DummyWithNoDataRefreshSubsciption noSubscribe = Factory.New<DummyWithNoDataRefreshSubsciption>();
			noSubscribe.Z0_Description = "comrade";
			Factory.Save();

			BusinessObjectFactory reloadFactory = new BusinessObjectFactory();
			DummyWithNoDataRefreshSubsciption noSubscribeReloaded = reloadFactory.Load<DummyWithNoDataRefreshSubsciption>(noSubscribe.PK);
			noSubscribeReloaded.Z0_Description = "noodle";
			reloadFactory.Save();

			AssertEquals("comrade", noSubscribe.Z0_Description);
			AssertEquals("noodle", noSubscribeReloaded.Z0_Description);
		}

		public void TestDataRefreshDelete()
		{
			ZGuid pK = Dummy.PK;
			Dummy.Z0_Code = "abc";
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummy2 = factory2.Load(typeof(DummyBusinessObject), pK) as DummyBusinessObject;
			AssertEquals("Dummy2.Z0_Code", "abc", dummy2.Z0_Code.ToString().Trim());

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			DummyBusinessObject dummy3 = factory3.Load(typeof(DummyBusinessObject), pK) as DummyBusinessObject;
			AssertEquals("Dummy3.Z0_Code", "abc", dummy3.Z0_Code.ToString().Trim());

			Dummy.Delete();
			Factory.Save();

			AssertEquals("Dummy2.IsDeleted", true, dummy2.IsDeleted);
			AssertEquals("Dummy3.IsDeleted", true, dummy3.IsDeleted);

			factory2.Save();
			AssertEquals("Dummy2.IsDeleted", true, dummy2.IsDeleted);
			AssertEquals("Dummy3.IsDeleted", true, dummy3.IsDeleted);
		}

		public void TestManageCollectionForRefresh()
		{
			DummyBusinessObjectCollection dummyCollection = new DummyBusinessObjectCollection(Factory);
			dummyCollection.Load();
			Factory.StartManagingCollectionForAddingBusinessObjects(dummyCollection);

			AssertEquals("DummyCollection.Count", 1, dummyCollection.Count);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummy3 = (DummyBusinessObject)factory2.New(typeof(DummyBusinessObject));
			dummy3.Z0_Code = "123";
			factory2.Save();

			AssertEquals("DummyCollection.Count", 2, dummyCollection.Count);

			Factory.StopManagingCollectionForAddingBusinessObjects(dummyCollection);
			DummyBusinessObject dummy4 = (DummyBusinessObject)factory2.New(typeof(DummyBusinessObject));
			dummy4.Z0_Code = "333";
			factory2.Save();

			AssertEquals("DummyCollection.Count", 2, dummyCollection.Count);
		}

		public void TestNotifyDeletedRemovesWrapperFromCollection()
		{
			DummyBusinessObject bizO = DummyBusinessObject.New(Factory);
			DummyBusinessObjectWrapper wrapper1 = DummyBusinessObjectWrapper.Load(bizO);
			AssertEquals("Precondition", 1, Factory.WrapperManager.GetCount(bizO));
			Factory.NotifyDeleted(bizO);
			AssertEquals(0, Factory.WrapperManager.GetCount(bizO));
		}

		public void TestNotifyDeleted()
		{
			DummyBusinessObjectCollection collection1 = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObjectCollection collection2 = new DummyBusinessObjectCollection(Factory);

			DummyBaseBusinessObject dummyBase = (DummyBaseBusinessObject)Factory.Load(typeof(DummyBaseBusinessObject), Dummy.PK);

			collection1.Add(Dummy);
			collection2.Add(dummyBase);

			Dummy.Delete();

			AssertEquals("DummyBase should be Deleted when Dummy was deleted", true, dummyBase.IsDeleted);
			AssertEquals("Dummy should be removed from Collection1 when deleted", 0, collection1.Count);
			AssertEquals("DummyBase should be removed from Collection2 when Dummy was deleted", 0, collection2.Count);
		}

		public void TestForcePublishForDataRefresh()
		{
			Dummy.Z0_Code = "aaa";
			Factory.Save();
			Factory.RefreshEnabled = false;

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummyCopy = (DummyBusinessObject)factory2.Load(typeof(DummyBusinessObject), Dummy.PK);

			Dummy.Z0_Code = "acc";
			Factory.Save();

			AssertEquals("Code", "aaa", dummyCopy.Z0_Code);
			Factory.ForcePublishForDataRefresh(Dummy);
			AssertEquals("Code", "acc", dummyCopy.Z0_Code);
		}

		public void TestForcePublishForDataRefreshByTableName()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			collection.IsManagedForDataRefresh = true;
			collection.Load();
			AssertEquals("Collection.Count", 1, collection.Count);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummyNew = (DummyBusinessObject)factory2.New(typeof(DummyBusinessObject));
			dummyNew.Z0_Code = "abc";
			factory2.Save();

			factory2.ForcePublishForDataRefreshByTableName(dummyNew);

			AssertEquals("Collection.Count", 2, collection.Count);
		}

		public void TestGetRowsUsingMixedOrAndAndStatementWithBrackets()
		{
			DummyBusinessObject bizO1 = Factory.New<DummyBusinessObject>();
			bizO1.Z0_Code = "2";

			DummyBusinessObject bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_Code = "3";
			bizO2.Z0_Number = 3;
			bizO2.Z0_AnotherNumber = 4;

			DummyBusinessObject bizO3 = Factory.New<DummyBusinessObject>();
			bizO3.Z0_Number = 3;
			bizO3.Z0_AnotherNumber = 3;

			Factory.RowFactory.MaximumRowsBeforeUsingIndex = 0;

			ZQuery andFilter = new ZQuery();
			andFilter.AddToFilter(DummyBizoSchema.Z0_Number, 3);
			andFilter.AddToFilter(DummyBizoSchema.Z0_AnotherNumber, 4);

			ZQuery query = new ZQuery();
			query.AddToFilter(DummyBizoSchema.Z0_Code, "2");
			query.AddToFilter(andFilter, JoinCondition.Or);
			DummyBusinessObject[] bizOs = Factory.Load<DummyBusinessObject>(query);
			Assert(((ICollection<DummyBusinessObject>)bizOs).Contains(bizO1));
			Assert(((ICollection<DummyBusinessObject>)bizOs).Contains(bizO2));
			Assert(!((ICollection<DummyBusinessObject>)bizOs).Contains(bizO3));
		}

		public void TestGetCachedValueSimple()
		{
			AssertNotNull(Factory.GetCachedValue<object>());
		}

		public void TestGetCachedValueReturnsSameObject()
		{
			object result1 = Factory.GetCachedValue<object>();
			object result2 = Factory.GetCachedValue<object>();
			AssertEquals(result1, result2);
		}

		public void TestGetCachedValueReturnsSameObjectWithKey()
		{
			object result1 = Factory.GetCachedValue("Key", delegate
			{ return new object(); });
			object result2 = Factory.GetCachedValue("Key", delegate
			{ return new object(); });
			AssertEquals(result1, result2);
		}

		public void TestGetCachedValueReturnsDifferentObjectWithDifferentKey()
		{
			object result1 = Factory.GetCachedValue("Key1", delegate
			{ return new object(); });
			object result2 = Factory.GetCachedValue("Key2", delegate
			{ return new object(); });
			AssertNotEquals(result1, result2);
		}

		public void TestTryGetValueFromCacheOnly()
		{
			object result1;
			var isKeyCached = Factory.TryGetValueFromCacheOnly("KEY", out result1);
			AssertEquals("[Before Caching] KEY cached?", false, isKeyCached);
			AssertNull(nameof(result1), result1);

			var result2 = Factory.GetCachedValue("KEY", () => new object());
			AssertNotNull(nameof(result2), result2);

			object result3;
			isKeyCached = Factory.TryGetValueFromCacheOnly("KEY", out result3);
			AssertEquals("[After Caching] KEY cached?", true, isKeyCached);
			AssertEquals("Value read from cache", result2, result3);
		}

		public void TestOnLoadCalledOnceOnlyForLoadOriginatingFromDifferentTypes()
		{
			DummyBaseBusinessObject bizOInFactory1 = Factory.New<DummyBaseBusinessObject>();
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBaseBusinessObject bizO1 = factory2.Load<DummyBusinessObject>(bizOInFactory1.PK);
			AssertEquals(true, bizO1.OnLoadedCalled);
			bizO1.OnLoadedCalled = false;
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyBusinessObject);
			DummyBaseBusinessObject bizO2;
			try
			{
				ZQuery query = new ZQuery(DummyBizoSchema.PK, bizOInFactory1.PK);
				bizO2 = factory2.Load<DummyBaseBusinessObject>(query)[0];
			}
			finally
			{
				DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = null;
			}
			AssertEquals(bizO1, bizO2);
			AssertEquals(false, bizO2.OnLoadedCalled);
		}

		public void TestDelayListChangesOnAllFactoriesReturnsOnlyFactoriesOnwedByCurrentThread()
		{
			var f1 = new BusinessObjectFactory();

			var filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, "AAA");
			var c1 = new ActiveBusinessObjectCollection<DummyBusinessObject>(f1, filter);

			var dummy = f1.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Code = "AAA";
			f1.Save();

			var f2 = new BusinessObjectFactory();
			var c2 = new ActiveBusinessObjectCollection<DummyBusinessObject>(f2, filter);
			var dummy2 = f2.Load<DummyBusinessObject>(dummy.PK);

			AssertCollectionContains(dummy, c1);
			AssertCollectionContains(dummy.PK, c2.Select(s => s.PK));

			((ThreadSentry)f2.ThreadSentry).RelinquishThreadOwnership();

			c2.CollectionCountChange += (s, e) =>
			{
				var rf = c2.Factory.RowFactory; // Poke the row factory so that ThreadSentry is bothered.
			};

			f1.Saved += (s, e) =>
			{
				// We are simulating that something has happened on a background thread whilst this operation was running.
				((ThreadSentry)f2.ThreadSentry).TakeThreadOwnership();
				dummy2.Z0_Code = "SSS";
				((ThreadSentry)f2.ThreadSentry).RelinquishThreadOwnership();
			};

			AssertNoExceptionThrown(() =>
			{
				f1.Save();
			});
		}

		public void TestGetReadOnlyFactory()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			BusinessObjectFactory readOnlyFactory = factory.GetCachedReadOnlyFactory();
			AssertNotNull("ReadOnlyFactory", readOnlyFactory);
			AssertNotEquals("Should not be the original Factory", readOnlyFactory, factory);
			AssertEquals("Should be the same", readOnlyFactory, factory.GetCachedReadOnlyFactory());
		}

		public void TestChangeThreadOwnership_ShouldChangeReadOnlyFactoryAsWell()
		{
			var factory = new BusinessObjectFactory();
			AssertEquals(true, factory.ThreadSentry.IsOwner);

			var readOnlyFactory = factory.GetCachedReadOnlyFactory();
			AssertEquals(true, factory.ThreadSentry.IsOwner);
			AssertEquals(true, readOnlyFactory.ThreadSentry.IsOwner);

			factory.ThreadSentry.RelinquishThreadOwnership();
			AssertEquals(false, factory.ThreadSentry.IsOwner);
			AssertEquals(false, readOnlyFactory.ThreadSentry.IsOwner);

			Task.Factory.StartNew(() =>
			{
				AssertEquals(false, factory.ThreadSentry.IsOwner);
				AssertEquals(false, readOnlyFactory.ThreadSentry.IsOwner);

				factory.ThreadSentry.TakeThreadOwnership();
				AssertEquals(true, factory.ThreadSentry.IsOwner);
				AssertEquals(true, readOnlyFactory.ThreadSentry.IsOwner);
			}).Wait();

			factory = new BusinessObjectFactory();
			factory.ThreadSentry.RelinquishThreadOwnership();
			readOnlyFactory = factory.GetCachedReadOnlyFactory();

			AssertEquals(true, readOnlyFactory.ThreadSentry.IsOwner);
			AssertNoExceptionThrown("Should not attempt to re-take ownership of the read-only factory, and yet...", () => factory.ThreadSentry.TakeThreadOwnership());

			readOnlyFactory.ThreadSentry.RelinquishThreadOwnership();
			AssertNoExceptionThrown("Should not attempt to re-relinquish ownership of the read-only factory, and yet...", () => factory.ThreadSentry.RelinquishThreadOwnership());
		}

		public void TestRelinquishAndTakeThreadOwnership()
		{
			var factory = new BusinessObjectFactory();
			AssertEquals(true, factory.IsOwnedByCurrentThread);

			factory.RelinquishThreadOwnership();
			AssertEquals(false, factory.IsOwnedByCurrentThread);

			factory.TakeThreadOwnership();
			AssertEquals(true, factory.IsOwnedByCurrentThread);
		}

		class DummyWithNoDataRefreshSubsciption : DummyBusinessObject
		{
			public DummyWithNoDataRefreshSubsciption(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override bool SubscribeToDataRefreshOnInstantiationCore
			{
				get { return false; }
			}
		}
	}

	#endregion

	#region Stored Procedure Tests

	[UseSnapshotProtection]
	sealed class BusinessObjectFactoryForStoredProcedureTest : TestCaseWithFactory
	{
		public void TestLoadBusinessObjectsFromStoredProcedure()
		{
			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());
			CreateStoredProcedure();
			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description) };
			var query = new ZStoredProcedureQuery("TestStoredProcedure", parameter);
			var bizObjs = Factory.Load<DummyBusinessObject>(query);
			AssertEquals(1, bizObjs.Length);
		}

		public void TestIncorrectStoredProcedureSQL()
		{
			var query = new ZStoredProcedureQuery("TestStoredProcedure", Array.Empty<ZSqlParameter>());
			Db.Connection.ExecuteNonQuery("CREATE PROCEDURE TestStoredProcedure AS BEGIN SELECT IN_INVALID FROM NONEXISTENTTABLE END");
			AssertExceptionThrown(typeof(SqlException), () => Factory.Load<DummyBusinessObject>(query));
		}

		public void TestCallingStoredProcedureWithInvalidParameters()
		{
			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());
			Db.Connection.ExecuteNonQuery("CREATE PROCEDURE TestStoredProcedure @Desc VARCHAR(" + DummyBizoSchema.Z0_Description.MaxLength + "), @Number int AS BEGIN SELECT " + GetTableColumnsForStoredProcedure() +
				" FROM " + DummyBizoSchema.Constants.TableName + " WHERE " + DummyBizoSchema.Z0_Description.Name + "=@Desc and " + DummyBizoSchema.Z0_Number.Name + "=@Number END");

			var parameters = new ZSqlParameterCollection();
			parameters.Add(ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description));
			var query = new ZStoredProcedureQuery("TestStoredProcedure", parameters.ToArray());
			AssertExceptionThrown(typeof(SqlException), () => Factory.Load<DummyBusinessObject>(query));

			parameters = new ZSqlParameterCollection();
			parameters.Add(ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description));
			parameters.Add(ZSqlParameter.New("@Number", DummyTableCreator.DummyNumber1, DummyBizoSchema.Z0_Number));
			parameters.Add(ZSqlParameter.New("@Invalid", "BOOM", DummyBizoSchema.Z0_VarCharMax));
			query = new ZStoredProcedureQuery("TestStoredProcedure", parameters.ToArray());
			AssertExceptionThrown(typeof(SqlException), () => Factory.Load<DummyBusinessObject>(query));
		}

		public void TestNoStoredProcedureExists()
		{
			var query = new ZStoredProcedureQuery("TestStoredProcedure", Array.Empty<ZSqlParameter>());
			AssertExceptionThrown<SqlException>(() => Factory.Load<DummyBusinessObject>(query));
		}

		public void TestNoBusinessObjectsReturnedFromStoredProcedure()
		{
			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());
			CreateStoredProcedure();
			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", "NOTTHERE", DummyBizoSchema.Z0_Description) };
			var query = new ZStoredProcedureQuery("TestStoredProcedure", parameter);
			var bizObjs = Factory.Load<DummyBusinessObject>(query);
			AssertEquals(0, bizObjs.Length);
		}

		public void TestLoadFromStoredProcedureAndSaveAndReloadDifferently()
		{
			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());
			CreateStoredProcedure();
			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description) };
			var query = new ZStoredProcedureQuery("TestStoredProcedure", parameter);
			var bizObjs = Factory.Load<DummyBusinessObject>(query);
			AssertEquals(1, bizObjs.Length);
			bizObjs[0].Z0_AnotherDecimal = 69.99m;
			var pk = bizObjs[0].PK;
			Factory.Save();
			var bizOAfterSave = Factory.Load<DummyBusinessObject>(pk);
			AssertEquals(69.99m, bizOAfterSave.Z0_AnotherDecimal);
			AssertEquals(bizObjs[0], bizOAfterSave);
		}

		public void TestLoadBusinessObjectModifyRunStoredProcedureConfirmLoadFromCache()
		{
			var pk1 = Guid.NewGuid();
			var pk2 = Guid.NewGuid();
			DummyTableCreator.AddDummyBusinessObjectsToDB(pk1, pk2);
			CreateStoredProcedure();
			var bizO = Factory.Load<DummyBusinessObject>(pk1);
			bizO.Z0_Code = "MOD";
			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description) };
			var query = new ZStoredProcedureQuery("TestStoredProcedure", parameter);
			var bizObjs = Factory.Load<DummyBusinessObject>(query);
			AssertEquals(1, bizObjs.Length);
			AssertEquals("MOD", bizObjs[0].Z0_Code);
		}

		public void TestLoadFromStoredProcedureTwice()
		{
			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());
			CreateStoredProcedure();
			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description) };
			var query = new ZStoredProcedureQuery("TestStoredProcedure", parameter);
			var bizObjs = Factory.Load<DummyBusinessObject>(query);
			bizObjs[0].Z0_Description = "CHANGED";
			var bizObj2 = Factory.Load<DummyBusinessObject>(query);
			AssertEquals(bizObjs[0], bizObj2[0]);
			AssertEquals("CHANGED", bizObj2[0].Z0_Description);
		}

		public void TestLoadFromStoredProcedureReturnsCollection()
		{
			for (var i = 0; i < 5; i++)
			{
				DummyTableCreator.AddRow(Guid.NewGuid(), "SAME", i, i + 10, DummyTableCreator.DummyByteArray1);
			}
			CreateStoredProcedure();
			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", "SAME", DummyBizoSchema.Z0_Description) };
			var query = new ZStoredProcedureQuery("TestStoredProcedure", parameter);
			var bizObjs = Factory.Load<DummyBusinessObject>(query);
			AssertEquals(5, bizObjs.Length);
		}

		public void TestLoadingFromDBOnlyCache()
		{
			CreateStoredProcedure();
			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", "SAME", DummyBizoSchema.Z0_Description) };
			var query = new ZStoredProcedureQuery("TestStoredProcedure", parameter);
			var pk1 = Guid.NewGuid();
			var pk2 = Guid.NewGuid();

			DummyTableCreator.AddRow(pk1, "SAME", 0, 10, DummyTableCreator.DummyByteArray1);
			var bizObjs = Factory.Load<DummyBusinessObject>(query);
			AssertContainsExactElementsInAnyOrder(new[] { pk1 }, bizObjs.Select(x => x.PK.ToGuid()));

			DummyTableCreator.AddRow(pk2, "SAME", 1, 11, DummyTableCreator.DummyByteArray2);
			bizObjs = Factory.Load<DummyBusinessObject>(query);
			AssertContainsExactElementsInAnyOrder(new[] { pk1 }, bizObjs.Select(x => x.PK.ToGuid()));

			query.ReLoadExistingRows = true;
			bizObjs = Factory.Load<DummyBusinessObject>(query);
			AssertContainsExactElementsInAnyOrder(new[] { pk1, pk2 }, bizObjs.Select(x => x.PK.ToGuid()));
		}

		public void TestParameterInvalid()
		{
			CreateStoredProcedure();
			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Invalid", "Invalid", DummyBizoSchema.Z0_Description) };
			var query = new ZStoredProcedureQuery("TestStoredProcedure", parameter);
			AssertExceptionThrown<SqlException>(() => Factory.Load<DummyBusinessObject>(query));
		}

		public void TestParameterExpectedButEmpty()
		{
			CreateStoredProcedure();
			var parameter = Array.Empty<ZSqlParameter>();
			var query = new ZStoredProcedureQuery("TestStoredProcedure", parameter);
			AssertExceptionThrown<SqlException>(() => Factory.Load<DummyBusinessObject>(query));
		}

		public void TestStoredProcedureWithLessFieldsThanInTable()
		{
			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());
			Db.Connection.ExecuteNonQuery("CREATE PROCEDURE TestStoredProcedure @Desc VARCHAR(" + DummyBizoSchema.Z0_Description.MaxLength + @") AS BEGIN SELECT
				Z0_PK FROM "
				+ DummyBizoSchema.Constants.TableName + " WHERE " + DummyBizoSchema.Z0_Description.Name + "=@Desc END");

			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description) };
			var query = new ZStoredProcedureQuery("TestStoredProcedure", parameter);
			AssertExceptionThrown<ZException>("cannot have a stored procedure with less columns than the table",
				"Data reader contains a different number of fields when compared to the table columns. Table: DummyBizo, Field Count: 1, Table Column Count: 51",
				() => Factory.Load<DummyBusinessObject>(query));
		}

		public void TestStoredProcedureWithDifferentGeographyTypeThanInTableButCorrectColumnCountAndNames()
		{
			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());
			Db.Connection.ExecuteNonQuery("CREATE PROCEDURE TestStoredProcedure @Desc VARCHAR(" + DummyBizoSchema.Z0_Description.MaxLength + ") AS BEGIN SELECT "
				+ GetTableColumnsForStoredProcedure(DummyBizoSchema.Z0_Geography.Name, "'INVALID' ") + " FROM "
				+ DummyBizoSchema.Constants.TableName + " WHERE " + DummyBizoSchema.Z0_Description.Name + "=@Desc END");

			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description) };
			var query = new ZStoredProcedureQuery("TestStoredProcedure", parameter);
			AssertExceptionThrown<ArgumentException>("Confirm Stored Procedure errors for Geography",
				"Type of value has a mismatch with column typeCouldn't store <INVALID> in Z0_Geography Column.  Expected type is SqlGeography.",
				() => Factory.Load<DummyBusinessObject>(query));
		}

		public void TestStoredProcedureWithDifferentBoolTypeThanInTableButCorrectColumnCountAndNames()
		{
			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());
			Db.Connection.ExecuteNonQuery("CREATE PROCEDURE TestStoredProcedure @Desc VARCHAR(" + DummyBizoSchema.Z0_Description.MaxLength + @") AS BEGIN SELECT " + GetTableColumnsForStoredProcedure(DummyBizoSchema.Z0_Bool.Name, "1 ") + " FROM "
				+ DummyBizoSchema.Constants.TableName + " WHERE " + DummyBizoSchema.Z0_Description.Name + "=@Desc END");

			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description) };
			var query = new ZStoredProcedureQuery("TestStoredProcedure", parameter);
			var objs = Factory.Load<DummyBusinessObject>(query);
			AssertEquals(1, objs.Length);
			objs[0].Z0_Description = "THIS IS GREAT";
			AssertExceptionThrown<ZSaveConcurrencyException>(() => Factory.Save());
		}

		public void TestStoredProcedureWithLargerDataThanColumnSize()
		{
			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());
			Db.Connection.ExecuteNonQuery("CREATE PROCEDURE TestStoredProcedure @Desc VARCHAR(" + DummyBizoSchema.Z0_Description.MaxLength + @") AS BEGIN SELECT "
				+ GetTableColumnsForStoredProcedure(DummyBizoSchema.Z0_NVarChar.Name, "'THIS WILL NOT FIT IN 20 NVARCHAR'") + " FROM "
				+ DummyBizoSchema.Constants.TableName + " WHERE " + DummyBizoSchema.Z0_Description.Name + "=@Desc END");

			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description) };
			var query = new ZStoredProcedureQuery("TestStoredProcedure", parameter);
			AssertExceptionThrown<ArgumentException>("Cannot load data larger than column size",
				"Cannot set column 'Z0_NVarChar'. The value violates the MaxLength limit of this column.",
				() => Factory.Load<DummyBusinessObject>(query));
		}

		public void TestStoredProcedureWithInvalidBoolString()
		{
			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());
			Db.Connection.ExecuteNonQuery("CREATE PROCEDURE TestStoredProcedure @Desc VARCHAR(" + DummyBizoSchema.Z0_Description.MaxLength + @") AS BEGIN SELECT "
				+ GetTableColumnsForStoredProcedure(DummyBizoSchema.Z0_Bool.Name, "'YEEEES' ") + " FROM "
				+ DummyBizoSchema.Constants.TableName + " WHERE " + DummyBizoSchema.Z0_Description.Name + "=@Desc END");

			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description) };
			var query = new ZStoredProcedureQuery("TestStoredProcedure", parameter);
			AssertExceptionThrown<ZTypeValueException>("Invalid bool string will not work",
				"Cannot initialise a CargoWise.Types.ZBool with <YEEEES> (System.String).",
				() => Factory.Load<DummyBusinessObject>(query));
		}

		public void TestStoredProcedureWithAcceptedBoolCharNotN()
		{
			var pk1 = Guid.NewGuid();
			DummyTableCreator.AddDummyBusinessObjectsToDB(pk1, Guid.NewGuid());
			Db.Connection.ExecuteNonQuery("CREATE PROCEDURE TestStoredProcedure @Desc VARCHAR(" + DummyBizoSchema.Z0_Description.MaxLength + @") AS BEGIN SELECT "
				+ GetTableColumnsForStoredProcedure(DummyBizoSchema.Z0_Bool.Name, "'F' ") + " FROM "
				+ DummyBizoSchema.Constants.TableName + " WHERE " + DummyBizoSchema.Z0_Description.Name + "=@Desc END");

			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description) };
			var query = new ZStoredProcedureQuery("TestStoredProcedure", parameter);
			var objs = Factory.Load<DummyBusinessObject>(query);
			AssertEquals(1, objs.Length);
			objs[0].Z0_NVarChar = "THIS IS GREAT";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var reloadedObj = newFactory.Load<DummyBusinessObject>(pk1);
			AssertEquals("THIS IS GREAT", reloadedObj.Z0_NVarChar);
		}

		public void TestStoredProcedureWithInvalidBoolChar()
		{
			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());
			Db.Connection.ExecuteNonQuery("CREATE PROCEDURE TestStoredProcedure @Desc VARCHAR(" + DummyBizoSchema.Z0_Description.MaxLength + @") AS BEGIN SELECT "
				+ GetTableColumnsForStoredProcedure(DummyBizoSchema.Z0_Bool.Name, "'A' ") + " FROM "
				+ DummyBizoSchema.Constants.TableName + " WHERE " + DummyBizoSchema.Z0_Description.Name + "=@Desc END");

			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description) };
			var query = new ZStoredProcedureQuery("TestStoredProcedure", parameter);
			AssertExceptionThrown<ZTypeValueException>("Invalid bool character will not work",
				"Cannot initialise a CargoWise.Types.ZBool with <A> (System.String).",
				() => Factory.Load<DummyBusinessObject>(query));
		}

		public void TestStoredProcedureWithNoPKAndInvalidColumns()
		{
			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());
			Db.Connection.ExecuteNonQuery("CREATE PROCEDURE TestStoredProcedure @Desc VARCHAR(" + DummyBizoSchema.Z0_Description.MaxLength + @") AS BEGIN SELECT
				Z0_AnotherDate FROM "
				+ DummyBizoSchema.Constants.TableName + " WHERE " + DummyBizoSchema.Z0_Description.Name + "=@Desc END");

			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description) };
			var query = new ZStoredProcedureQuery("TestStoredProcedure", parameter);
			AssertExceptionThrown<ZException>("Invalid Number of columns",
				"Data reader contains a different number of fields when compared to the table columns. Table: DummyBizo, Field Count: 1, Table Column Count: 51",
				() => Factory.Load<DummyBusinessObject>(query));
		}

		public void TestStoredProcedureWithNoPKAndCorrectColumnNumberWithOneDuplicated()
		{
			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());
			Db.Connection.ExecuteNonQuery("CREATE PROCEDURE TestStoredProcedure @Desc VARCHAR(" + DummyBizoSchema.Z0_Description.MaxLength + ") AS BEGIN SELECT "
				+ GetTableColumnsForStoredProcedure().Replace(DummyBizoSchema.PK.Name, DummyBizoSchema.Z0_AnotherDate.Name) + " FROM "
				+ DummyBizoSchema.Constants.TableName + " WHERE " + DummyBizoSchema.Z0_Description.Name + "=@Desc END");

			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description) };
			var query = new ZStoredProcedureQuery("TestStoredProcedure", parameter);
			AssertExceptionThrown<IndexOutOfRangeException>("No PK column", () => Factory.Load<DummyBusinessObject>(query));
		}

		public void TestStoredProcedureWithPKAndCorrectColumnNumberWithOneDuplicated()
		{
			var pk1 = Guid.NewGuid();
			DummyTableCreator.AddDummyBusinessObjectsToDB(pk1, Guid.NewGuid());

			Db.Connection.ExecuteNonQuery("CREATE PROCEDURE TestStoredProcedure @Desc VARCHAR(" + DummyBizoSchema.Z0_Description.MaxLength + ") AS BEGIN SELECT "
				+ GetTableColumnsForStoredProcedure().Replace(DummyBizoSchema.Z0_Xml.Name, DummyBizoSchema.Z0_VarCharMax.Name) + " FROM "
				+ DummyBizoSchema.Constants.TableName + " WHERE " + DummyBizoSchema.Z0_Description.Name + "=@Desc END");

			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description) };
			var query = new ZStoredProcedureQuery("TestStoredProcedure", parameter);
			var objs = Factory.Load<DummyBusinessObject>(query);
			AssertEquals(1, objs.Length);
			objs[0].Z0_Xml = "Didn't Load column from Stored Procedure but this will save";
			objs[0].Z0_VarCharMax = "THIS WILL POPULATE INTO XML ON RELOAD";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var reloadedObj = newFactory.Load<DummyBusinessObject>(pk1);
			AssertEquals("Didn't Load column from Stored Procedure but this will save", reloadedObj.Z0_Xml);
			AssertEquals("THIS WILL POPULATE INTO XML ON RELOAD", reloadedObj.Z0_VarCharMax);
			var newStoredProcFactory = new BusinessObjectFactory();
			var storedProcObjs = newStoredProcFactory.Load<DummyBusinessObject>(query);
			AssertEquals(ZString.Empty, storedProcObjs[0].Z0_Xml);
			AssertEquals("THIS WILL POPULATE INTO XML ON RELOAD", storedProcObjs[0].Z0_VarCharMax);
		}

		public void TestStoredProcedureWithMoreFieldsThanInTable()
		{
			AssertExceptionThrown<SqlException>(() =>
			Db.Connection.ExecuteNonQuery("CREATE PROCEDURE TestStoredProcedure @Desc VARCHAR(" + DummyBizoSchema.Z0_Description.MaxLength + @") AS BEGIN SELECT "
				+ GetTableColumnsForStoredProcedure() + ",Z0_InvalidColumn FROM "
				+ DummyBizoSchema.Constants.TableName + " WHERE " + DummyBizoSchema.Z0_Description.Name + "=@Desc END"));
		}

		public void TestFactoryLoadStoredProcedureWithDowncast()
		{
			var pk1 = Guid.NewGuid();
			DummyTableCreator.AddDummyBusinessObjectsToDB(pk1, Guid.NewGuid());
			CreateStoredProcedure();
			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description) };
			var query = new ZStoredProcedureQuery("TestStoredProcedure", parameter);
			var downcastQuery = (ZQuery)query;
			var objs = Factory.Load<DummyBusinessObject>(downcastQuery);
			AssertEquals(1, objs.Length);
			AssertEquals(pk1, objs[0].PK);
		}

		void CreateStoredProcedure()
		{
			Db.Connection.ExecuteNonQuery("CREATE PROCEDURE TestStoredProcedure @Desc VARCHAR(" + DummyBizoSchema.Z0_Description.MaxLength + ") AS BEGIN SELECT " + GetTableColumnsForStoredProcedure() +
				" FROM " + DummyBizoSchema.Constants.TableName + " WHERE " + DummyBizoSchema.Z0_Description.Name + "=@Desc END");
		}

		ZString GetTableColumnsForStoredProcedure()
		{
			return GetTableColumnsForStoredProcedure(ZString.Empty, ZString.Empty);
		}

		ZString GetTableColumnsForStoredProcedure(string columnToUpdate, string columnPrefix)
		{
			var result = string.Join(",", ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumns(DummyBizoSchema.Constants.TableName).Select(t => t.Name));
			if (!string.IsNullOrEmpty(columnToUpdate))
			{
				result = result.Replace(columnToUpdate, columnPrefix + columnToUpdate);
			}
			return result;
		}
	}

	#endregion
}
