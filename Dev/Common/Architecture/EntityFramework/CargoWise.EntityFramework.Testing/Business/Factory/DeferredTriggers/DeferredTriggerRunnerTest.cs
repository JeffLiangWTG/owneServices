using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DeferredTriggerRunnerTest : TestCaseWithFactory
	{
		#region DeferredTriggerRunner Tests

		#region DeferAndReturnTriggers Tests

		public void TestDeferAndReturnTriggers_ReturnsCorrectInsertTriggers()
		{
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (MockProcedures.CreateNewMockTrigger(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTrigger.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTrigger.StoredProc))
			{
				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();

				var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();

				var deferredTriggers = deferredTriggerRunner.DeferAndReturnTriggers(Factory, new[] { bizO1 });
				var expectedDeferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredUpdateTrigger.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredUpdateTrigger), DummyBizOWithDeferredUpdateTrigger.TriggerName, DummyBizoSchema.PK, new [] { bizO1.PK }) }
				};
				AssertDeferredTriggerContents(expectedDeferredTriggers, deferredTriggers);
			}
		}

		public void TestDeferAndReturnTriggers_ReturnsMultipleInsertTriggers()
		{
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (MockProcedures.CreateNewMockTrigger(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTrigger.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTrigger.StoredProc))
			{
				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();

				var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				var bizO2 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();

				var deferredTriggers = deferredTriggerRunner.DeferAndReturnTriggers(Factory, new[] { bizO1, bizO2 });
				var expectedDeferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredUpdateTrigger.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredUpdateTrigger), DummyBizOWithDeferredUpdateTrigger.TriggerName, DummyBizoSchema.PK, new [] { bizO1.PK, bizO2.PK }) }
				};
				AssertDeferredTriggerContents(expectedDeferredTriggers, deferredTriggers);
			}
		}

		public void TestDeferAndReturnTriggers_ReturnsCorrectUpdateTriggers()
		{
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (MockProcedures.CreateNewMockTrigger(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTrigger.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTrigger.StoredProc))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				var bizO2 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				Factory.Save();

				bizO1.ColumnThatRequiresTriggerDeferral = 3;

				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();
				var deferredTriggers = deferredTriggerRunner.DeferAndReturnTriggers(Factory, new[] { bizO1, bizO2 });
				var expectedDeferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredUpdateTrigger.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredUpdateTrigger), DummyBizOWithDeferredUpdateTrigger.TriggerName, DummyBizoSchema.PK, new [] { bizO1.PK }) }
				};
				AssertDeferredTriggerContents(expectedDeferredTriggers, deferredTriggers);
			}
		}

		public void TestDeferAndReturnTriggers_ReturnsMultipleUpdateTriggers()
		{
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (MockProcedures.CreateNewMockTrigger(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTrigger.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTrigger.StoredProc))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				var bizO2 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				var bizO3 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				var bizO4 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				Factory.Save();

				bizO1.ColumnThatRequiresTriggerDeferral = 3;
				bizO2.ColumnThatRequiresTriggerDeferral = 4;
				bizO3.ColumnThatRequiresTriggerDeferral = 5;
				bizO4.ColumnThatDoesNotRequireTriggerDeferral = 5;

				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();
				var deferredTriggers = deferredTriggerRunner.DeferAndReturnTriggers(Factory, new[] { bizO1, bizO2, bizO3, bizO4 });

				var expectedDeferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredUpdateTrigger.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredUpdateTrigger), DummyBizOWithDeferredUpdateTrigger.TriggerName, DummyBizoSchema.PK, new [] { bizO1.PK, bizO2.PK, bizO3.PK }) }
				};
				AssertDeferredTriggerContents(expectedDeferredTriggers, deferredTriggers);
			}
		}

		public void TestDeferAndReturnTriggers_ReturnsCorrectDeleteTriggers()
		{
			using (MockProcedures.CreateNewMockTrigger(((IDbConnected)Factory).Connection, DummyBizOWithDeferredDeleteTrigger.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(((IDbConnected)Factory).Connection, DummyBizOWithDeferredDeleteTrigger.StoredProc))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredDeleteTrigger>();
				var bizO2 = Factory.New<DummyBizOWithDeferredDeleteTrigger>();
				var bizO3 = Factory.New<DummyBizOWithDeferredDeleteTrigger>();
				var bizO4 = Factory.New<DummyBizOWithDeferredDeleteTrigger>();
				Factory.Save();
				var expectedDeferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredDeleteTrigger.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredDeleteTrigger), DummyBizOWithDeferredDeleteTrigger.TriggerName, DummyBizoSchema.PK, new [] { bizO1.PK }) }
				};

				bizO1.Delete();

				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();
				var deferredTriggers = deferredTriggerRunner.DeferAndReturnTriggers(Factory, new[] { bizO1, bizO2, bizO3, bizO4 });

				AssertDeferredTriggerContents(expectedDeferredTriggers, deferredTriggers);
			}
		}

		public void TestDeferAndReturnTriggers_ReturnsMultipleDeleteTriggers()
		{
			using (MockProcedures.CreateNewMockTrigger(((IDbConnected)Factory).Connection, DummyBizOWithDeferredDeleteTrigger.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(((IDbConnected)Factory).Connection, DummyBizOWithDeferredDeleteTrigger.StoredProc))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredDeleteTrigger>();
				var bizO2 = Factory.New<DummyBizOWithDeferredDeleteTrigger>();
				var bizO3 = Factory.New<DummyBizOWithDeferredDeleteTrigger>();
				var bizO4 = Factory.New<DummyBizOWithDeferredDeleteTrigger>();
				Factory.Save();
				var expectedDeferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredDeleteTrigger.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredDeleteTrigger), DummyBizOWithDeferredDeleteTrigger.TriggerName, DummyBizoSchema.PK, new [] { bizO1.PK, bizO2.PK }) }
				};

				bizO1.Delete();
				bizO2.Delete();

				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();
				var deferredTriggers = deferredTriggerRunner.DeferAndReturnTriggers(Factory, new[] { bizO1, bizO2, bizO3, bizO4 });

				AssertDeferredTriggerContents(expectedDeferredTriggers, deferredTriggers);
			}
		}

		public void TestDeferAndReturnTriggers_MultipleDeferTrigerAttributesForSameTrigger()
		{
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (MockProcedures.CreateNewMockTrigger(((IDbConnected)Factory).Connection, DummyBizO_MultipleDeferTriggerAttributesForSameTrigger.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(((IDbConnected)Factory).Connection, DummyBizO_MultipleDeferTriggerAttributesForSameTrigger.StoredProc))
			{
				var guid1 = ZGuid.NewZGuid();
				var guid2 = ZGuid.NewZGuid();
				var guid3 = ZGuid.NewZGuid();
				var guid4 = ZGuid.NewZGuid();

				var bizO1 = Factory.New<DummyBizO_MultipleDeferTriggerAttributesForSameTrigger>();
				var bizO2 = Factory.New<DummyBizO_MultipleDeferTriggerAttributesForSameTrigger>();
				bizO1.Z0_Guid = guid1;
				bizO2.Z0_Guid = guid2;
				Factory.Save();

				var bizOType = typeof(DummyBizO_MultipleDeferTriggerAttributesForSameTrigger);
				var attributes = DeferTriggerAndRunBeforeCommitAttribute.GetAttributes(bizOType).Where(attr => attr.TriggerName.Equals(DummyBizO_MultipleDeferTriggerAttributesForSameTrigger.TriggerName));
				var expectedDeferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{
						DummyBizO_MultipleDeferTriggerAttributesForSameTrigger.StoredProc + "|",
						new[]
						{
							new TriggerMetaData(attributes.Single(attr => attr.ValueToRunStoredProcWith == ValueVersion.Current), bizOType, DummyBizoSchema.PK, new[] { bizO1.PK }),  // on insert/update validate current record
							new TriggerMetaData(attributes.Single(attr => attr.ValueToRunStoredProcWith == ValueVersion.Both), bizOType, DummyBizoSchema.Z0_Guid, new[] { guid1, guid3 }), // on insert/update validate related records
							new TriggerMetaData(attributes.Single(attr => attr.ValueToRunStoredProcWith == ValueVersion.Original), bizOType, DummyBizoSchema.Z0_Guid, new[] { guid2 }),    // on delete validate related records
						}
					}
				};

				bizO1.Z0_Guid = guid3;
				bizO2.Z0_Guid = guid4;
				bizO2.Delete();

				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();
				var deferredTriggers = deferredTriggerRunner.DeferAndReturnTriggers(Factory, new[] { bizO1, bizO2 });
				AssertDeferredTriggerContents(expectedDeferredTriggers, deferredTriggers);
			}
		}

		public void TestSuspendTriggers_MultipleConditionalSuspendTriggerAttributesForSameTrigger()
		{
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (MockProcedures.CreateNewMockTrigger(((IDbConnected)Factory).Connection, DummyBizO_DeferTriggerOnlyAttributesForSameTrigger.TriggerName))
			{
				var guid1 = ZGuid.NewZGuid();

				var bizO1 = Factory.New<DummyBizO_DeferTriggerOnlyAttributesForSameTrigger>();
				var bizO2 = Factory.New<DummyBizO_DeferTriggerOnlyAttributesForSameTrigger>();
				Factory.Save();

				var bizOType = typeof(DummyBizO_DeferTriggerOnlyAttributesForSameTrigger);
				var attributes = DeferTriggerAndRunBeforeCommitAttribute.GetAttributes(bizOType).Where(attr => attr.TriggerName.Equals(DummyBizO_DeferTriggerOnlyAttributesForSameTrigger.TriggerName));
				var expectedSuspendTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{
						DummyBizO_DeferTriggerOnlyAttributesForSameTrigger.TriggerName,
						new[]
						{
							// As this attribute does not run anything after resume, we do not need to keep PKs (ZGuid[0]).
							new TriggerMetaData(attributes.Single(attr => attr.DeferTriggerConditionStrategyType == typeof(IDummyUpdateConditionStrategy)), bizOType, null, Array.Empty<ZGuid>()),
							new TriggerMetaData(attributes.Single(attr => attr.DeferTriggerConditionStrategyType == typeof(IDeferTriggerOnDeleteConditionStrategy)), bizOType, null, Array.Empty<ZGuid>()),
						}
					}
				};

				bizO1.Z0_Guid = guid1;
				bizO2.Delete();

				var suspendTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();
				var suspendTriggers = suspendTriggerRunner.DeferAndReturnTriggers(Factory, new[] { bizO1 });
				AssertDeferredTriggerContents(expectedSuspendTriggers, suspendTriggers);
			}
		}

		public void TestDeferAndReturnTriggers_DeferTriggerBothAttributesForSameTrigger()
		{
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (MockProcedures.CreateNewMockTrigger(((IDbConnected)Factory).Connection, DummyBizO_DeferTriggerBothAttributesForSameTrigger.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(((IDbConnected)Factory).Connection, DummyBizO_DeferTriggerBothAttributesForSameTrigger.StoredProc))
			{
				var guid1 = ZGuid.NewZGuid();
				var guid2 = ZGuid.NewZGuid();
				var guid3 = ZGuid.NewZGuid();
				var guid4 = ZGuid.NewZGuid();

				var bizO1 = Factory.New<DummyBizO_DeferTriggerBothAttributesForSameTrigger>();
				var bizO2 = Factory.New<DummyBizO_DeferTriggerBothAttributesForSameTrigger>();
				bizO1.Z0_Guid = guid1;
				bizO2.Z0_Guid = guid2;
				Factory.Save();

				var bizOType = typeof(DummyBizO_DeferTriggerBothAttributesForSameTrigger);
				var attributes = DeferTriggerAndRunBeforeCommitAttribute.GetAttributes(bizOType).Where(attr => attr.TriggerName.Equals(DummyBizO_DeferTriggerBothAttributesForSameTrigger.TriggerName));
				var expectedDeferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{
						DummyBizO_DeferTriggerBothAttributesForSameTrigger.StoredProc + "|",
						new[]
						{
							new TriggerMetaData(attributes.Single(attr => attr.ValueToRunStoredProcWith == ValueVersion.Current && !attr.IsSuspendTriggerOnly), bizOType, DummyBizoSchema.PK, new[] { bizO1.PK }),  // on insert/update validate current record
							new TriggerMetaData(attributes.Single(attr => attr.ValueToRunStoredProcWith == ValueVersion.Both && !attr.IsSuspendTriggerOnly), bizOType, DummyBizoSchema.Z0_Guid, new[] { guid1, guid3 }), // on insert/update validate related records
							new TriggerMetaData(attributes.Single(attr => attr.ValueToRunStoredProcWith == ValueVersion.Original && !attr.IsSuspendTriggerOnly), bizOType, DummyBizoSchema.Z0_Guid, new[] { guid2 }),    // on delete validate related records
						}
					},
					{
						DummyBizO_DeferTriggerBothAttributesForSameTrigger.TriggerName,
						new[]
						{
							new TriggerMetaData(attributes.Single(attr => attr.DeferTriggerConditionStrategyType == typeof(IDummyUpdateConditionStrategy) && attr.IsSuspendTriggerOnly), bizOType, null, Array.Empty<ZGuid>()),
							new TriggerMetaData(attributes.Single(attr => attr.DeferTriggerConditionStrategyType == typeof(IDeferTriggerOnDeleteConditionStrategy) && attr.IsSuspendTriggerOnly), bizOType, null, Array.Empty<ZGuid>()),
						}
					}
				};

				bizO1.Z0_Guid = guid3;
				bizO2.Z0_Guid = guid4;
				bizO2.Delete();

				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();
				var deferredTriggers = deferredTriggerRunner.DeferAndReturnTriggers(Factory, new[] { bizO1, bizO2 });
				AssertDeferredTriggerContents(expectedDeferredTriggers, deferredTriggers);
			}
		}

		public void TestDeferAndReturnTriggers_GroupsDeferralByCheckProcedure()
		{
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (ObjectFactory.Substitute<IDummyDependentUpdateConditionStrategy>(new DummyDependentUpdateConditionStrategy()))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				var childBizO1 = Factory.New<DummyChildBizOWithDeferredUpdateTrigger>();
				var childBizO2 = Factory.New<DummyChildBizOWithDeferredUpdateTrigger>();
				childBizO1.ZD1_Z0 = bizO1.PK;
				childBizO2.ZD1_Z0 = bizO1.PK;

				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();
				var deferredTriggers = deferredTriggerRunner.DeferAndReturnTriggers(Factory, new BusinessObject[] { bizO1, childBizO1, childBizO2 });
				var parentAttributes = DeferTriggerAndRunBeforeCommitAttribute.GetAttributes(typeof(DummyBizOWithDeferredUpdateTrigger));
				var childAttributes = DeferTriggerAndRunBeforeCommitAttribute.GetAttributes(typeof(DummyChildBizOWithDeferredUpdateTrigger));

				var expectedDeferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{
						DummyChildBizOWithDeferredUpdateTrigger.StoredProc + "|",
						new[]
						{
							new TriggerMetaData(childAttributes.Single(a => a.TriggerName == DummyChildBizOWithDeferredUpdateTrigger.TriggerName1), typeof(DummyChildBizOWithDeferredUpdateTrigger), DummyDependentBizoSchema.PK, new[] { childBizO1.PK, childBizO2.PK }),
							new TriggerMetaData(childAttributes.Single(a => a.TriggerName == DummyChildBizOWithDeferredUpdateTrigger.TriggerName2), typeof(DummyChildBizOWithDeferredUpdateTrigger), DummyDependentBizoSchema.ZD1_Z0, new[] { bizO1.PK }),
							new TriggerMetaData(parentAttributes.Single(), typeof(DummyBizOWithDeferredUpdateTrigger), DummyBizoSchema.PK, new[] { bizO1.PK })
						}
					}
				};
				AssertDeferredTriggerContents(expectedDeferredTriggers, deferredTriggers);
			}
		}

		#endregion

		#region DeferAndReturnTriggers Edge Cases

		public void TestDeferAndReturnTriggersReturnsCorrectDeleteTriggersOnNonPKColumns()
		{
			using (MockProcedures.CreateNewMockTrigger(((IDbConnected)Factory).Connection, DummyBizOWithDeferredDeleteTriggerOnNonPKColumn.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(((IDbConnected)Factory).Connection, DummyBizOWithDeferredDeleteTriggerOnNonPKColumn.StoredProc))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredDeleteTriggerOnNonPKColumn>();
				var bizO2 = Factory.New<DummyBizOWithDeferredDeleteTriggerOnNonPKColumn>();
				var bizO3 = Factory.New<DummyBizOWithDeferredDeleteTriggerOnNonPKColumn>();
				var bizO4 = Factory.New<DummyBizOWithDeferredDeleteTriggerOnNonPKColumn>();
				Factory.Save();
				var expectedDeferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredDeleteTriggerOnNonPKColumn.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredDeleteTriggerOnNonPKColumn), DummyBizOWithDeferredDeleteTriggerOnNonPKColumn.TriggerName, DummyBizoSchema.Z0_Guid, new [] { bizO1.Z0_Guid }) }
				};

				bizO1.Delete();

				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();
				var deferredTriggers = deferredTriggerRunner.DeferAndReturnTriggers(Factory, new[] { bizO1, bizO2, bizO3, bizO4 });

				AssertDeferredTriggerContents(expectedDeferredTriggers, deferredTriggers);
			}
		}

		const string GuidValueForTesting1 = "831c4ec7-a9bd-4097-8945-a85aa089159b";
		const string GuidValueForTesting2 = "831c4ec7-a9bd-4097-8945-a85aa089159a";

		public void TestDeferAndReturnTriggersReturnsMultipleUpdateTriggersOnNonPkColumns()
		{
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (MockProcedures.CreateNewMockTrigger(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTriggerOnNonPkColumn.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTriggerOnNonPkColumn.StoredProc))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTriggerOnNonPkColumn>();
				bizO1.Z0_Guid = new ZGuid(GuidValueForTesting1);
				var bizO2 = Factory.New<DummyBizOWithDeferredUpdateTriggerOnNonPkColumn>();
				var bizO3 = Factory.New<DummyBizOWithDeferredUpdateTriggerOnNonPkColumn>();
				var bizO4 = Factory.New<DummyBizOWithDeferredUpdateTriggerOnNonPkColumn>();
				Factory.Save();

				var expectedDeferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredUpdateTriggerOnNonPkColumn.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredUpdateTriggerOnNonPkColumn), DummyBizOWithDeferredUpdateTriggerOnNonPkColumn.TriggerName, DummyBizoSchema.Z0_Guid, new [] { bizO1.Z0_Guid, bizO2.Z0_Guid, bizO3.Z0_Guid }) }
				};

				bizO1.ColumnThatRequiresTriggerDeferral = 3;
				bizO1.Z0_Guid = new ZGuid(GuidValueForTesting2);
				bizO2.ColumnThatRequiresTriggerDeferral = 4;
				bizO3.ColumnThatRequiresTriggerDeferral = 5;
				bizO4.ColumnThatDoesNotRequireTriggerDeferral = 5;

				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();
				var deferredTriggers = deferredTriggerRunner.DeferAndReturnTriggers(Factory, new[] { bizO1, bizO2, bizO3, bizO4 });
				AssertDeferredTriggerContents(expectedDeferredTriggers, deferredTriggers);
			}
		}

		public void TestDeferAndReturnTriggersReturnsCorrectTriggersPKOriginal()
		{
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (MockProcedures.CreateNewMockTrigger(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTriggerOnPkColumnOriginalValueVersion.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTriggerOnPkColumnOriginalValueVersion.StoredProc))
			{
				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();

				var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTriggerOnPkColumnOriginalValueVersion>();
				var bizO2 = Factory.New<DummyBizOWithDeferredUpdateTriggerOnPkColumnOriginalValueVersion>();
				var expectedDeferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredUpdateTriggerOnPkColumnOriginalValueVersion.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredUpdateTriggerOnPkColumnOriginalValueVersion), DummyBizOWithDeferredUpdateTriggerOnPkColumnOriginalValueVersion.TriggerName, DummyBizoSchema.PK, new [] { bizO1.PK, bizO2.PK }) }
				};

				var deferredTriggers = deferredTriggerRunner.DeferAndReturnTriggers(Factory, new[] { bizO1, bizO2 });
				AssertDeferredTriggerContents(expectedDeferredTriggers, deferredTriggers);

				Factory.Save();
				bizO1.ColumnThatRequiresTriggerDeferral = 5;
				expectedDeferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredUpdateTriggerOnPkColumnOriginalValueVersion.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredUpdateTriggerOnPkColumnOriginalValueVersion), DummyBizOWithDeferredUpdateTriggerOnPkColumnOriginalValueVersion.TriggerName, DummyBizoSchema.PK, new [] { bizO1.PK }) }
				};

				deferredTriggers = deferredTriggerRunner.DeferAndReturnTriggers(Factory, new[] { bizO1, bizO2 });
				AssertDeferredTriggerContents(expectedDeferredTriggers, deferredTriggers);
			}
		}

		public void TestDeferAndReturnTriggersReturnsCorrectTriggersPKBoth()
		{
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (MockProcedures.CreateNewMockTrigger(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTriggerOnPkColumnBothValueVersions.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTriggerOnPkColumnBothValueVersions.StoredProc))
			{
				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();

				var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTriggerOnPkColumnBothValueVersions>();
				var bizO2 = Factory.New<DummyBizOWithDeferredUpdateTriggerOnPkColumnBothValueVersions>();
				var expectedDeferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredUpdateTriggerOnPkColumnBothValueVersions.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredUpdateTriggerOnPkColumnBothValueVersions), DummyBizOWithDeferredUpdateTriggerOnPkColumnBothValueVersions.TriggerName, DummyBizoSchema.PK, new [] { bizO1.PK, bizO2.PK }) }
				};

				var deferredTriggers = deferredTriggerRunner.DeferAndReturnTriggers(Factory, new[] { bizO1, bizO2 });
				AssertDeferredTriggerContents(expectedDeferredTriggers, deferredTriggers);

				Factory.Save();
				bizO1.ColumnThatRequiresTriggerDeferral = 5;
				expectedDeferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredUpdateTriggerOnPkColumnBothValueVersions.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredUpdateTriggerOnPkColumnBothValueVersions), DummyBizOWithDeferredUpdateTriggerOnPkColumnBothValueVersions.TriggerName, DummyBizoSchema.PK, new [] { bizO1.PK }) }
				};

				deferredTriggers = deferredTriggerRunner.DeferAndReturnTriggers(Factory, new[] { bizO1, bizO2 });
				AssertDeferredTriggerContents(expectedDeferredTriggers, deferredTriggers);
			}
		}

		public void TestDeferAndReturnTriggersReturnsCorrectTriggersNonPKCurrent()
		{
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (MockProcedures.CreateNewMockTrigger(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTriggerOnNonPkColumnCurrentValueVersion.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTriggerOnNonPkColumnCurrentValueVersion.StoredProc))
			{
				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();

				var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTriggerOnNonPkColumnCurrentValueVersion>();
				var bizO2 = Factory.New<DummyBizOWithDeferredUpdateTriggerOnNonPkColumnCurrentValueVersion>();
				bizO1.Z0_Guid = new ZGuid(GuidValueForTesting1);
				var expectedDeferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredUpdateTriggerOnNonPkColumnCurrentValueVersion.StoredProc + "|", GetTriggerMetaData(
						typeof(DummyBizOWithDeferredUpdateTriggerOnNonPkColumnCurrentValueVersion),
						DummyBizOWithDeferredUpdateTriggerOnNonPkColumnCurrentValueVersion.TriggerName,
						DummyBizoSchema.Z0_Guid,
						new [] { bizO1.Z0_Guid, bizO2.Z0_Guid }) }
				};

				var deferredTriggers = deferredTriggerRunner.DeferAndReturnTriggers(Factory, new[] { bizO1, bizO2 });
				AssertDeferredTriggerContents(expectedDeferredTriggers, deferredTriggers);

				Factory.Save();
				bizO1.ColumnThatRequiresTriggerDeferral = 5;
				bizO1.Z0_Guid = new ZGuid(GuidValueForTesting2);
				expectedDeferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredUpdateTriggerOnNonPkColumnCurrentValueVersion.StoredProc + "|", GetTriggerMetaData(
						typeof(DummyBizOWithDeferredUpdateTriggerOnNonPkColumnCurrentValueVersion),
						DummyBizOWithDeferredUpdateTriggerOnNonPkColumnCurrentValueVersion.TriggerName,
						DummyBizoSchema.Z0_Guid,
						new [] { bizO1.Z0_Guid }) }
				};

				deferredTriggers = deferredTriggerRunner.DeferAndReturnTriggers(Factory, new[] { bizO1, bizO2 });
				AssertDeferredTriggerContents(expectedDeferredTriggers, deferredTriggers);
			}
		}

		public void TestDeferAndReturnTriggersReturnsCorrectTriggersNonPKBoth()
		{
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (MockProcedures.CreateNewMockTrigger(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTriggerOnNonPkColumnBothValueVersions.TriggerName))
			using (MockProcedures.CreateNewMockProcedure(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTriggerOnNonPkColumnBothValueVersions.StoredProc))
			{
				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();

				var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTriggerOnNonPkColumnBothValueVersions>();
				var bizO2 = Factory.New<DummyBizOWithDeferredUpdateTriggerOnNonPkColumnBothValueVersions>();
				var expectedDeferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{
						DummyBizOWithDeferredUpdateTriggerOnNonPkColumnBothValueVersions.StoredProc + "|",
						GetTriggerMetaData(typeof(DummyBizOWithDeferredUpdateTriggerOnNonPkColumnBothValueVersions),
							DummyBizOWithDeferredUpdateTriggerOnNonPkColumnBothValueVersions.TriggerName,
							DummyBizoSchema.Z0_Guid,
							new [] { new ZGuid(GuidValueForTesting1), bizO2.Z0_Guid })
					}
				};
				bizO1.Z0_Guid = new ZGuid(GuidValueForTesting1);

				var deferredTriggers = deferredTriggerRunner.DeferAndReturnTriggers(Factory, new[] { bizO1, bizO2 });
				AssertDeferredTriggerContents(expectedDeferredTriggers, deferredTriggers);

				Factory.Save();
				bizO1.ColumnThatRequiresTriggerDeferral = 5;
				bizO1.Z0_Guid = new ZGuid(GuidValueForTesting2);
				expectedDeferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{
						DummyBizOWithDeferredUpdateTriggerOnNonPkColumnBothValueVersions.StoredProc + "|",
						GetTriggerMetaData(typeof(DummyBizOWithDeferredUpdateTriggerOnNonPkColumnBothValueVersions),
							DummyBizOWithDeferredUpdateTriggerOnNonPkColumnBothValueVersions.TriggerName,
							DummyBizoSchema.Z0_Guid,
							new [] { new ZGuid(GuidValueForTesting1), new ZGuid(GuidValueForTesting2) })
					}
				};

				deferredTriggers = deferredTriggerRunner.DeferAndReturnTriggers(Factory, new[] { bizO1, bizO2 });
				AssertDeferredTriggerContents(expectedDeferredTriggers, deferredTriggers);
			}
		}

		public void TestSuspendTriggers_AttributeUsedOnBaseClassHasMultipleSubclasses_SuspendSameTriggerOnlyOnce()
		{
			var triggerName = DummyBizO_DeferTriggerOnlyAttributesForSameTrigger.TriggerName;
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (MockProcedures.CreateNewMockTrigger(((IDbConnected)Factory).Connection, triggerName))
			{
				var guid1 = ZGuid.NewZGuid();
				var guid2 = ZGuid.NewZGuid();
				var bizO1 = Factory.New<DummyBizO_DeferTriggerAttributeForSubclassA>();
				var bizO2 = Factory.New<DummyBizO_DeferTriggerAttributeForSubclassB>();
				Factory.Save();

				var bizOType1 = typeof(DummyBizO_DeferTriggerAttributeForSubclassA);
				var bizOType2 = typeof(DummyBizO_DeferTriggerAttributeForSubclassB);
				var attribute1 = DeferTriggerAndRunBeforeCommitAttribute.GetAttributes(bizOType1).Single(attr => attr.TriggerName.Equals(triggerName));
				var attribute2 = DeferTriggerAndRunBeforeCommitAttribute.GetAttributes(bizOType2).Single(attr => attr.TriggerName.Equals(triggerName));
				var expectedSuspendTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{
						triggerName,
						new[]
						{
							new TriggerMetaData(attribute1, bizOType1, null, Array.Empty<ZGuid>()),
							new TriggerMetaData(attribute2, bizOType2, null, Array.Empty<ZGuid>()),
						}
					}
				};
				bizO1.Z0_Guid = guid1;
				bizO2.Z0_Guid = guid2;
				var checkSQL = $"select APPLOCK_MODE('public', '{triggerName}', 'Transaction')";
				var resumeSQL = $"EXEC dbo.ResumeTrigger '{triggerName}'";
				var suspendTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();
				AssertEquals("Precondition - Trigger is NOT suspended", "NoLock", TestConnection.ExecuteScalar(checkSQL));
				var suspendTriggers = suspendTriggerRunner.DeferAndReturnTriggers(Factory, new DummyBizO_DeferTriggerAttributeForBaseclass[] { bizO1, bizO2 });
				AssertDeferredTriggerContents(expectedSuspendTriggers, suspendTriggers);

				AssertEquals("Trigger is suspended.", "Shared", TestConnection.ExecuteScalar(checkSQL));
				suspendTriggerRunner.RunDeferredTriggers(suspendTriggers, Factory);
				AssertEquals("Trigger is NOT suspended.", "NoLock", TestConnection.ExecuteScalar(checkSQL));
			}
		}

		#endregion

		#region RunDeferredTriggers Tests

		public void TestRunDeferredTriggers_RunsStoredProcWithCorrectArgumentsOnDeferredInsertOrUpdateTrigger()
		{
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (MockProcedures.MockExistingProcedure(((IDbConnected)Factory).Connection, TriggerProcedure.ResumeTrigger))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				var deferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredUpdateTrigger.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredUpdateTrigger), DummyBizOWithDeferredUpdateTrigger.TriggerName, DummyBizoSchema.PK, new [] { bizO1.PK }) }
				};
				AssertDeferProcedureIsRunWithCorrectArguments(DummyBizOWithDeferredUpdateTrigger.StoredProc, deferredTriggers);
			}
		}

		public void TestRunDeferredTriggers_RunsStoredProcsWithCorrectArgumentsOnMultipleDeferredInsertOrUpdateTriggers()
		{
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (MockProcedures.MockExistingProcedure(((IDbConnected)Factory).Connection, TriggerProcedure.ResumeTrigger))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				var bizO2 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				var deferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredUpdateTrigger.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredUpdateTrigger), DummyBizOWithDeferredUpdateTrigger.TriggerName, DummyBizoSchema.PK, new [] { bizO1.PK, bizO2.PK }) }
				};
				AssertDeferProcedureIsRunWithCorrectArguments(DummyBizOWithDeferredUpdateTrigger.StoredProc, deferredTriggers);
			}
		}

		public void TestRunDeferredTriggers_RunsStoredProcWithCorrectArgumentsOnDeferredDeleteTrigger()
		{
			using (MockProcedures.MockExistingProcedure(((IDbConnected)Factory).Connection, TriggerProcedure.ResumeTrigger))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredDeleteTrigger>();
				var deferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredDeleteTrigger.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredDeleteTrigger), DummyBizOWithDeferredDeleteTrigger.TriggerName, DummyBizoSchema.PK, new [] { bizO1.PK }) }
				};
				AssertDeferProcedureIsRunWithCorrectArguments(DummyBizOWithDeferredDeleteTrigger.StoredProc, deferredTriggers);
			}
		}

		public void TestRunDeferredTriggers_RunsStoredProcWithCorrectArgumentsMultipleTriggers()
		{
			using (ObjectFactory.Substitute<IDummyUpdateConditionStrategy>(new DummyUpdateConditionStrategy()))
			using (MockProcedures.MockExistingProcedure(((IDbConnected)Factory).Connection, TriggerProcedure.ResumeTrigger))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				var bizO2 = Factory.New<DummyBizOWithDeferredDeleteTrigger>();
				var deferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredUpdateTrigger.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredUpdateTrigger), DummyBizOWithDeferredUpdateTrigger.TriggerName, DummyBizoSchema.PK, new [] { bizO1.PK }) },
					{ DummyBizOWithDeferredDeleteTrigger.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredDeleteTrigger), DummyBizOWithDeferredDeleteTrigger.TriggerName, DummyBizoSchema.PK, new [] { bizO2.PK }) }
				};
				using (MockProcedures.CreateNewMockProcedure(((IDbConnected)Factory).Connection, DummyBizOWithDeferredUpdateTrigger.StoredProc))
				using (MockProcedures.CreateNewMockProcedure(((IDbConnected)Factory).Connection, DummyBizOWithDeferredDeleteTrigger.StoredProc))
				{
					var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();
					deferredTriggerRunner.RunDeferredTriggers(deferredTriggers, Factory);

					var storedUpdateArguments = MockProcedures.ReadValuesPassedToProcedure(TestConnection, DummyBizOWithDeferredUpdateTrigger.StoredProc);
					var expectedUpdatePKInTheProcedure = deferredTriggers[DummyBizOWithDeferredUpdateTrigger.StoredProc + "|"].First().Values;
					var actualUpdatePKsInTheProcedure = GetGuidsFromStrings(storedUpdateArguments);
					AssertContainsExactElementsInAnyOrder("PKs in procedure did not match", expectedUpdatePKInTheProcedure, actualUpdatePKsInTheProcedure);

					var storedDeleteArguments = MockProcedures.ReadValuesPassedToProcedure(TestConnection, DummyBizOWithDeferredDeleteTrigger.StoredProc);
					var expectedDeletePKInTheProcedure = deferredTriggers[DummyBizOWithDeferredDeleteTrigger.StoredProc + "|"].First().Values;
					var actualDeletePKsInTheProcedure = GetGuidsFromStrings(storedDeleteArguments);
					AssertContainsExactElementsInAnyOrder("PKs in procedure did not match", expectedDeletePKInTheProcedure, actualDeletePKsInTheProcedure);
				}
			}
		}

		public void TestRunDeferredTriggers_RunsStoredProcsWithCorrectArgumentsOnMultipleDeferredDeleteTriggers()
		{
			using (MockProcedures.MockExistingProcedure(((IDbConnected)Factory).Connection, TriggerProcedure.ResumeTrigger))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredDeleteTrigger>();
				var bizO2 = Factory.New<DummyBizOWithDeferredDeleteTrigger>();
				var deferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{
						DummyBizOWithDeferredDeleteTrigger.StoredProc + "|",
						GetTriggerMetaData(typeof(DummyBizOWithDeferredDeleteTrigger), DummyBizOWithDeferredDeleteTrigger.TriggerName, DummyBizoSchema.PK, new [] { bizO1.PK, bizO2.PK })
					}
				};
				AssertDeferProcedureIsRunWithCorrectArguments(DummyBizOWithDeferredDeleteTrigger.StoredProc, deferredTriggers);
			}
		}

		public void TestRunDeferredTriggers_RunsStoredProcWithCorrectArguments_MultipleDeferTrigerAttributesForSameTrigger()
		{
			using (MockProcedures.MockExistingProcedure(((IDbConnected)Factory).Connection, TriggerProcedure.ResumeTrigger))
			{
				var bizO1 = Factory.New<DummyBizO_MultipleDeferTriggerAttributesForSameTrigger>();
				var bizO2 = Factory.New<DummyBizO_MultipleDeferTriggerAttributesForSameTrigger>();
				var bizO3 = Factory.New<DummyBizO_MultipleDeferTriggerAttributesForSameTrigger>();

				var bizOType = typeof(DummyBizO_MultipleDeferTriggerAttributesForSameTrigger);
				var attributes = DeferTriggerAndRunBeforeCommitAttribute.GetAttributes(bizOType).Where(attr => attr.TriggerName.Equals(DummyBizO_MultipleDeferTriggerAttributesForSameTrigger.TriggerName));
				var deferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{
						DummyBizO_MultipleDeferTriggerAttributesForSameTrigger.StoredProc + "|",
						new[]
						{
							new TriggerMetaData(attributes.Single(attr => attr.ValueToRunStoredProcWith == ValueVersion.Current), bizOType, DummyBizoSchema.PK, new[] { bizO1.PK }),  // on insert/update validate current record
							new TriggerMetaData(attributes.Single(attr => attr.ValueToRunStoredProcWith == ValueVersion.Both), bizOType, DummyBizoSchema.Z0_Guid, new[] { bizO2.PK }), // on insert/update validate related records
							new TriggerMetaData(attributes.Single(attr => attr.ValueToRunStoredProcWith == ValueVersion.Original), bizOType, DummyBizoSchema.Z0_Guid, new[] { bizO3.PK }),    // on delete validate related records
						}
					}
				};

				AssertDeferProcedureIsRunWithCorrectArguments(DummyBizO_MultipleDeferTriggerAttributesForSameTrigger.StoredProc, deferredTriggers);
			}
		}

		public void TestRunDeferredTriggers_RunsStoredProcWithCorrectArguments_HasSecondParamForStoredProc()
		{
			using (MockProcedures.MockExistingProcedure(((IDbConnected)Factory).Connection, TriggerProcedure.ResumeTrigger))
			{
				var bizO1 = Factory.New<DummyBizO_DeferTriggerWithSecondParamForStoredProc>();

				var bizOType = typeof(DummyBizO_DeferTriggerWithSecondParamForStoredProc);
				var attributes = DeferTriggerAndRunBeforeCommitAttribute.GetAttributes(bizOType).Where(attr => attr.TriggerName.Equals(DummyBizO_DeferTriggerWithSecondParamForStoredProc.TriggerName));
				var deferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{
						DummyBizO_DeferTriggerWithSecondParamForStoredProc.StoredProc + "|" + DummyBizO_DeferTriggerWithSecondParamForStoredProc.SecondParamForStoredProc,
						new[]
						{
							new TriggerMetaData(attributes.Single(), bizOType, DummyBizoSchema.Z0_Guid, new[] { bizO1.PK }),
						}
					}
				};

				AssertDeferProcedureIsRunWithCorrectArguments(DummyBizO_DeferTriggerWithSecondParamForStoredProc.StoredProc, deferredTriggers, DummyBizO_DeferTriggerWithSecondParamForStoredProc.SecondParamForStoredProc);
			}
		}

		public void TestRunDeferredTriggers_RunsStoredProcWithCorrectArguments_HasMultipleParamsForStoredProc()
		{
			using (MockProcedures.MockExistingProcedure(((IDbConnected)Factory).Connection, TriggerProcedure.ResumeTrigger))
			{
				var bizO1 = Factory.New<DummyBizO_DeferTriggerWithMultipleParamsForStoredProc>();

				var bizOType = typeof(DummyBizO_DeferTriggerWithMultipleParamsForStoredProc);
				var attributes = DeferTriggerAndRunBeforeCommitAttribute.GetAttributes(bizOType).Where(attr => attr.TriggerName.Equals(DummyBizO_DeferTriggerWithMultipleParamsForStoredProc.TriggerName));
				var deferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{
						DummyBizO_DeferTriggerWithMultipleParamsForStoredProc.StoredProc + "|" + DummyBizO_DeferTriggerWithMultipleParamsForStoredProc.MultipleParamsForStoredProc,
						new[]
						{
							new TriggerMetaData(attributes.Single(), bizOType, DummyBizoSchema.Z0_Guid, new[] { bizO1.PK }),
						}
					}
				};

				AssertDeferProcedureIsRunWithCorrectArguments(DummyBizO_DeferTriggerWithMultipleParamsForStoredProc.StoredProc, deferredTriggers, DummyBizO_DeferTriggerWithMultipleParamsForStoredProc.MultipleParamsForStoredProc);
			}
		}

		public void TestRunDeferredTriggers_ResumesDeferredInsertOrUpdateTriggers()
		{
			var connection = ((IDbConnected)Factory).Connection;
			using (MockProcedures.MockExistingProcedure(connection, TriggerProcedure.ResumeTrigger))
			using (MockProcedures.CreateNewMockProcedure(connection, DummyBizOWithDeferredUpdateTrigger.StoredProc))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				var bizO2 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				var bizO3 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				var deferredTriggers1 = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredUpdateTrigger.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredUpdateTrigger), DummyBizOWithDeferredUpdateTrigger.TriggerName, DummyBizoSchema.PK, new [] { bizO1.PK }) }
				};
				var deferredTriggers2 = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredUpdateTrigger.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredUpdateTrigger), DummyBizOWithDeferredUpdateTrigger.TriggerName, DummyBizoSchema.PK, new [] { bizO2.PK , bizO3.PK }) }
				};

				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();

				deferredTriggerRunner.RunDeferredTriggers(deferredTriggers1, Factory);
				AssertContainsExactElementsInAnyOrder("Insert or update trigger should be resumed after bizO inserted", new[] { DummyBizOWithDeferredUpdateTrigger.TriggerName },
					MockProcedures.ReadValuesPassedToProcedure(connection, TriggerProcedure.ResumeTrigger));

				deferredTriggerRunner.RunDeferredTriggers(new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>(), Factory);
				AssertEquals("Insert or update trigger should not be resumed when no bizO inserted", 0,
					MockProcedures.ReadValuesPassedToProcedure(connection, TriggerProcedure.ResumeTrigger).Count());

				deferredTriggerRunner.RunDeferredTriggers(deferredTriggers2, Factory);
				AssertContainsExactElementsInAnyOrder("Insert or update trigger should be resumed after multiple bizO inserted", new[] { DummyBizOWithDeferredUpdateTrigger.TriggerName },
					MockProcedures.ReadValuesPassedToProcedure(connection, TriggerProcedure.ResumeTrigger));
			}
		}

		public void TestRunDeferredTriggers_ResumesDeferredDeleteTriggers()
		{
			var connection = ((IDbConnected)Factory).Connection;
			using (MockProcedures.MockExistingProcedure(connection, TriggerProcedure.ResumeTrigger))
			using (MockProcedures.CreateNewMockProcedure(connection, DummyBizOWithDeferredDeleteTrigger.StoredProc))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredDeleteTrigger>();
				var bizO2 = Factory.New<DummyBizOWithDeferredDeleteTrigger>();
				var bizO3 = Factory.New<DummyBizOWithDeferredDeleteTrigger>();
				var deferredTriggers1 = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredDeleteTrigger.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredDeleteTrigger), DummyBizOWithDeferredDeleteTrigger.TriggerName, DummyBizoSchema.PK, new [] { bizO1.PK }) }
				};
				var deferredTriggers2 = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredDeleteTrigger.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredDeleteTrigger), DummyBizOWithDeferredDeleteTrigger.TriggerName, DummyBizoSchema.PK, new [] { bizO2.PK, bizO3.PK }) }
				};

				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();

				deferredTriggerRunner.RunDeferredTriggers(deferredTriggers1, Factory);
				AssertContainsExactElementsInAnyOrder("Delete trigger should be resumed after bizO deleted", new[] { DummyBizOWithDeferredDeleteTrigger.TriggerName },
					MockProcedures.ReadValuesPassedToProcedure(connection, TriggerProcedure.ResumeTrigger));

				deferredTriggerRunner.RunDeferredTriggers(deferredTriggers2, Factory);
				AssertContainsExactElementsInAnyOrder("Delete trigger should be resumed after multiple bizO deleted", new[] { DummyBizOWithDeferredDeleteTrigger.TriggerName },
					MockProcedures.ReadValuesPassedToProcedure(connection, TriggerProcedure.ResumeTrigger));
			}
		}

		public void TestRunDeferredTriggers_ResumesMultipleTriggers()
		{
			var connection = ((IDbConnected)Factory).Connection;
			using (MockProcedures.MockExistingProcedure(connection, TriggerProcedure.ResumeTrigger))
			using (MockProcedures.CreateNewMockProcedure(connection, DummyBizOWithDeferredUpdateTrigger.StoredProc))
			using (MockProcedures.CreateNewMockProcedure(connection, DummyBizOWithDeferredDeleteTrigger.StoredProc))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
				var bizO2 = Factory.New<DummyBizOWithDeferredDeleteTrigger>();
				var deferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredUpdateTrigger.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredUpdateTrigger), DummyBizOWithDeferredUpdateTrigger.TriggerName, DummyBizoSchema.PK, new [] { bizO1.PK }) },
					{ DummyBizOWithDeferredDeleteTrigger.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredDeleteTrigger), DummyBizOWithDeferredDeleteTrigger.TriggerName, DummyBizoSchema.PK, new [] { bizO2.PK }) }
				};

				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();

				deferredTriggerRunner.RunDeferredTriggers(deferredTriggers, Factory);
				var valuePassedToProcedure = MockProcedures.ReadValuesPassedToProcedure(connection, TriggerProcedure.ResumeTrigger);
				AssertContainsExactElementsInAnyOrder("Insert or update trigger and delete trigger should be resumed after bizO inserted",
					new[] { DummyBizOWithDeferredUpdateTrigger.TriggerName, DummyBizOWithDeferredDeleteTrigger.TriggerName }, valuePassedToProcedure);
			}
		}

		public void TestRunDeferredTriggers_Batch_100()
		{
			TestRunDeferredTriggers_Batch(100);
		}

		public void TestRunDeferredTriggers_Batch_500()
		{
			TestRunDeferredTriggers_Batch(500);
		}

		public void TestRunDeferredTriggers_Batch_1100()
		{
			TestRunDeferredTriggers_Batch(1100);
		}

		void TestRunDeferredTriggers_Batch(int pkCount)
		{
			var batchSize = 500;
			var batchCount = (int)Math.Ceiling((decimal)pkCount / batchSize);

			var connection = ((IDbConnected)Factory).Connection;
			using (MockProcedures.MockExistingProcedure(connection, TriggerProcedure.ResumeTrigger))
			using (MockProcedures.CreateNewMockProcedure(connection, DummyBizOWithDeferredUpdateTrigger.StoredProc))
			{
				var values = new ZGuid[pkCount];
				for (int i = 0; i < pkCount; i++)
				{
					values[i] = ZGuid.NewZGuid();
				}
				var deferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredUpdateTrigger.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredUpdateTrigger), DummyBizOWithDeferredUpdateTrigger.TriggerName, DummyBizoSchema.PK, values) }
				};

				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();
				deferredTriggerRunner.RunDeferredTriggers(deferredTriggers, Factory);

				var valuePassedToProcedure = MockProcedures.ReadValuesPassedToProcedure(connection, DummyBizOWithDeferredUpdateTrigger.StoredProc).ToArray();
				AssertEquals("batchCount should be correct", batchCount, valuePassedToProcedure.Length);

				var valuesEachBatch = new string[batchCount][];
				var allValues = new List<string>();
				var batchSmallerThan500Count = 0;
				for (int i = 0; i < batchCount; i++)
				{
					valuesEachBatch[i] = valuePassedToProcedure[i].Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
					allValues.AddRange(valuesEachBatch[i]);

					AssertEquals("Each batch should have processed 1-500 PKs", true, valuesEachBatch[i].Length <= 500);
					if (valuesEachBatch[i].Length < 500)
					{
						batchSmallerThan500Count++;
					}
					AssertEquals("There can only be one batch that processed < 500 PKs", true, batchSmallerThan500Count <= 1);
				}

				AssertContainsExactElementsInAnyOrder("All PKs should be passed to procedure", values.Select(zguid => zguid.ToString().ToUpper()), allValues);
			}
		}

		public void TestRunDeferredTriggers_ThrowsDataExceptionWhenRunningStoredProcThatThrows()
		{
			TestRunDeferredTriggers_ThrowsDataExceptionWhenRunningStoredProcThatThrowsCore<ZDataException>(string.Empty);
		}

		public void TestRunDeferredTriggers_ThrowsDataExceptionWhenRunningStoredProcThatThrows_ConcurrentTrigger()
		{
			TestRunDeferredTriggers_ThrowsDataExceptionWhenRunningStoredProcThatThrowsCore<ZConcurrencyCheckFailureException>("TriggerLikelyConcurrencyError: ");
		}

		void TestRunDeferredTriggers_ThrowsDataExceptionWhenRunningStoredProcThatThrowsCore<T>(string errorPrefix) where T : Exception
		{
			using (MockProcedures.MockExistingProcedure(((IDbConnected)Factory).Connection, TriggerProcedure.ResumeTrigger))
			using (MockProcedures.CreateNewMockTrigger(((IDbConnected)Factory).Connection, DummyBizOWithDeferredDeleteTrigger.TriggerName))
			using (MockProcedures.CreateFailingMockProcedure(((IDbConnected)Factory).Connection, DummyBizOWithDeferredDeleteTrigger.StoredProc, errorPrefix))
			{
				var bizO1 = Factory.New<DummyBizOWithDeferredDeleteTrigger>();
				var deferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
				{
					{ DummyBizOWithDeferredDeleteTrigger.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredDeleteTrigger), DummyBizOWithDeferredDeleteTrigger.TriggerName, DummyBizoSchema.PK, new [] { bizO1.PK }) }
				};

				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();
				AssertExceptionThrown<T>("Expect exception to bubble up from erroneous stored proc", () => deferredTriggerRunner.RunDeferredTriggers(deferredTriggers, Factory));
			}
		}

		public static void AssertDeferredTriggerContents(IReadOnlyDictionary<string, IReadOnlyCollection<TriggerMetaData>> expectedDeferredTriggers, IReadOnlyDictionary<string, IReadOnlyCollection<TriggerMetaData>> actualDeferredTriggers)
		{
			AssertEquals("Incorrect number of deferred triggers", expectedDeferredTriggers.Count, actualDeferredTriggers.Count);
			foreach (var expectedDeferredTrigger in expectedDeferredTriggers)
			{
				var first = expectedDeferredTrigger.Value.First();
				var firstA = actualDeferredTriggers
					.Select(o => o.Value.SingleOrDefault(v => v.Attribute == first.Attribute))
					.First(o => o != null);
				Assert("Trigger Missing", actualDeferredTriggers.TryGetValue(expectedDeferredTrigger.Key, out var actualTriggerMetaData));
				AssertEquals("Incorrect BizO Type", first.BizOType, firstA.BizOType);
				AssertContainsExactElementsInAnyOrder("Deferred Trigger Values did not match", first.Values, firstA.Values);
			}
		}

		#endregion

		#endregion

		#region Null Arguments Tests

		public void TestDeferAndReturnTriggersWithNullArguments()
		{
			var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();
			var bizOCollection = new[] { Factory.New<DummyBizOWithDeferredUpdateTrigger>() };
			AssertExceptionThrown<ArgumentNullException>(() => deferredTriggerRunner.DeferAndReturnTriggers(Factory, null));
			AssertExceptionThrown<ArgumentNullException>(() => deferredTriggerRunner.DeferAndReturnTriggers(null, bizOCollection));
		}

		public void TestRunDeferredTriggersWithNullArguments()
		{
			var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();
			var bizO1 = Factory.New<DummyBizOWithDeferredUpdateTrigger>();
			var deferredTriggers = new Dictionary<string, IReadOnlyCollection<TriggerMetaData>>()
			{
				{ DummyBizOWithDeferredUpdateTrigger.StoredProc + "|", GetTriggerMetaData(typeof(DummyBizOWithDeferredUpdateTrigger), DummyBizOWithDeferredUpdateTrigger.TriggerName, DummyBizoSchema.PK, new [] { bizO1.PK }) }
			};
			AssertExceptionThrown<ArgumentNullException>(() => deferredTriggerRunner.RunDeferredTriggers(null, Factory));
			AssertExceptionThrown<ArgumentNullException>(() => deferredTriggerRunner.RunDeferredTriggers(deferredTriggers, null));
		}

		#endregion

		#region Implementation

		void AssertDeferProcedureIsRunWithCorrectArguments(string storedProc, Dictionary<string, IReadOnlyCollection<TriggerMetaData>> deferredTriggers, string extraParamsForStoredProc = "")
		{
			var extraParamsArray = string.IsNullOrEmpty(extraParamsForStoredProc) ? Array.Empty<string>() : extraParamsForStoredProc.Split(new[] { "," } , StringSplitOptions.RemoveEmptyEntries);
			using (MockProcedures.CreateNewMockProcedure(TestConnection, storedProc, extraParamsArray.Length))
			{
				var expectedPKsInTheProcedure = deferredTriggers.Values.SelectMany(v => v.SelectMany(iv => iv.Values));
				var deferredTriggerRunner = ObjectFactory.Get<IDeferredTriggerRunner>();
				deferredTriggerRunner.RunDeferredTriggers(deferredTriggers, Factory);

				var storedArguments = MockProcedures.ReadValuesPassedToProcedure(TestConnection, storedProc);

				if (extraParamsArray.Length > 0)
				{
					var extraParams = MockProcedures.ReadExtraParamsPassedToProcedure(TestConnection, storedProc);
					AssertEquals(extraParams, string.Join(",", extraParamsArray.Select(s => s.Trim())));
				}

				var actualPKsInTheProcedure = GetGuidsFromStrings(storedArguments);
				AssertContainsExactElementsInAnyOrder("PKs in procedure did not match", expectedPKsInTheProcedure, actualPKsInTheProcedure);
			}
		}

		static IEnumerable<ZGuid> GetGuidsFromStrings(IEnumerable<string> storedArguments)
		{
			return storedArguments.SelectMany(sa => sa.Split(new[] { ", ", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
				.Select(s => ZGuid.IsGuid(s) ? new ZGuid(s) : ZGuid.Empty)
				.Where(g => !g.IsEmpty)
				.ToArray());
		}

		public static TriggerMetaData[] GetTriggerMetaData(Type type, string triggerName, SchemaColumn column, IEnumerable<ZGuid> values)
		{
			return DeferTriggerAndRunBeforeCommitAttribute.GetAttributes(type).Where(attr => attr.TriggerName.Equals(triggerName)).Select(attr => new TriggerMetaData(attr, type, column, values)).ToArray();
		}

		[DeferTriggerAndRunBeforeCommit(TriggerName, StoredProc, DummyBizoSchema.Constants.Z0_Guid, typeof(IDeferTriggerOnDeleteConditionStrategy), ValueToRunStoredProcWith = ValueVersion.Original)]
		public class DummyBizOWithDeferredDeleteTriggerOnNonPKColumn : DummyBusinessObject
		{
			public const string StoredProc = "DummyDeleteStoredProc";
			public const string TriggerName = "TG_DummyDeleteTrigger";

			public DummyBizOWithDeferredDeleteTriggerOnNonPKColumn(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		[DeferTriggerAndRunBeforeCommit(TriggerName, StoredProc, DummyBizoSchema.Constants.Z0_Guid, typeof(IDummyUpdateConditionStrategy), ValueToRunStoredProcWith = ValueVersion.Original)]
		public class DummyBizOWithDeferredUpdateTriggerOnNonPkColumn : DummyBusinessObject
		{
			public const string StoredProc = "DummyUpdateStoredProc";
			public const string TriggerName = "TG_DummyUpdateTrigger";

			public DummyBizOWithDeferredUpdateTriggerOnNonPkColumn(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZDecimal ColumnThatRequiresTriggerDeferral
			{
				get => Z0_Decimal;
				set => Z0_Decimal = value;
			}
			public ZDecimal ColumnThatDoesNotRequireTriggerDeferral
			{
				get => Z0_AnotherDecimal;
				set => Z0_AnotherDecimal = value;
			}
		}

		[DeferTriggerAndRunBeforeCommit(TriggerName, StoredProc, DummyBizoSchema.Constants.PK, typeof(IDummyUpdateConditionStrategy), ValueToRunStoredProcWith = ValueVersion.Original)]
		public class DummyBizOWithDeferredUpdateTriggerOnPkColumnOriginalValueVersion : DummyBusinessObject
		{
			public const string StoredProc = "DummyUpdateStoredProc";
			public const string TriggerName = "TG_DummyUpdateTrigger";

			public DummyBizOWithDeferredUpdateTriggerOnPkColumnOriginalValueVersion(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZDecimal ColumnThatRequiresTriggerDeferral
			{
				get => Z0_Decimal;
				set => Z0_Decimal = value;
			}
		}

		[DeferTriggerAndRunBeforeCommit(TriggerName, StoredProc, DummyBizoSchema.Constants.PK, typeof(IDummyUpdateConditionStrategy), ValueToRunStoredProcWith = ValueVersion.Both)]
		public class DummyBizOWithDeferredUpdateTriggerOnPkColumnBothValueVersions : DummyBusinessObject
		{
			public const string StoredProc = "DummyUpdateStoredProc";
			public const string TriggerName = "TG_DummyUpdateTrigger";

			public DummyBizOWithDeferredUpdateTriggerOnPkColumnBothValueVersions(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZDecimal ColumnThatRequiresTriggerDeferral
			{
				get => Z0_Decimal;
				set => Z0_Decimal = value;
			}
		}

		[DeferTriggerAndRunBeforeCommit(TriggerName, StoredProc, DummyBizoSchema.Constants.Z0_Guid, typeof(IDummyUpdateConditionStrategy), ValueToRunStoredProcWith = ValueVersion.Current)]
		public class DummyBizOWithDeferredUpdateTriggerOnNonPkColumnCurrentValueVersion : DummyBusinessObject
		{
			public const string StoredProc = "DummyUpdateStoredProc";
			public const string TriggerName = "TG_DummyUpdateTrigger";

			public DummyBizOWithDeferredUpdateTriggerOnNonPkColumnCurrentValueVersion(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZDecimal ColumnThatRequiresTriggerDeferral
			{
				get => Z0_Decimal;
				set => Z0_Decimal = value;
			}
		}

		[DeferTriggerAndRunBeforeCommit(TriggerName, StoredProc, DummyBizoSchema.Constants.Z0_Guid, typeof(IDummyUpdateConditionStrategy), ValueToRunStoredProcWith = ValueVersion.Both)]
		public class DummyBizOWithDeferredUpdateTriggerOnNonPkColumnBothValueVersions : DummyBusinessObject
		{
			public const string StoredProc = "DummyUpdateStoredProc";
			public const string TriggerName = "TG_DummyUpdateTrigger";

			public DummyBizOWithDeferredUpdateTriggerOnNonPkColumnBothValueVersions(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZDecimal ColumnThatRequiresTriggerDeferral
			{
				get => Z0_Decimal;
				set => Z0_Decimal = value;
			}
		}

		[DeferTriggerAndRunBeforeCommit(TriggerName, StoredProc, DummyBizoSchema.Constants.PK, typeof(IDummyUpdateConditionStrategy), ValueToRunStoredProcWith = ValueVersion.Current)]
		[DeferTriggerAndRunBeforeCommit(TriggerName, StoredProc, DummyBizoSchema.Constants.Z0_Guid, typeof(IDummyUpdateConditionStrategy), ValueToRunStoredProcWith = ValueVersion.Both)]
		[DeferTriggerAndRunBeforeCommit(TriggerName, StoredProc, DummyBizoSchema.Constants.Z0_Guid, typeof(IDeferTriggerOnDeleteConditionStrategy), ValueToRunStoredProcWith = ValueVersion.Original)]
		public class DummyBizO_MultipleDeferTriggerAttributesForSameTrigger : DummyBusinessObject
		{
			public const string StoredProc = "DummyDeferedTriggerStoredProc";
			public const string TriggerName = "TG_DummyDeferedTrigger";

			public DummyBizO_MultipleDeferTriggerAttributesForSameTrigger(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		[DeferTriggerAndRunBeforeCommit(TriggerName, typeof(IDummyUpdateConditionStrategy))]
		[DeferTriggerAndRunBeforeCommit(TriggerName, typeof(IDeferTriggerOnDeleteConditionStrategy))]
		public class DummyBizO_DeferTriggerOnlyAttributesForSameTrigger : DummyBusinessObject
		{
			public const string TriggerName = "TG_DummyDeferedTrigger";

			public DummyBizO_DeferTriggerOnlyAttributesForSameTrigger(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		[DeferTriggerAndRunBeforeCommit(TriggerName, typeof(IDummyUpdateConditionStrategy))]
		public abstract class DummyBizO_DeferTriggerAttributeForBaseclass : DummyBusinessObject
		{
			public const string TriggerName = "TG_DummyDeferedTrigger";

			public DummyBizO_DeferTriggerAttributeForBaseclass(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		public class DummyBizO_DeferTriggerAttributeForSubclassA : DummyBizO_DeferTriggerAttributeForBaseclass
		{
			public DummyBizO_DeferTriggerAttributeForSubclassA(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			// A uses B
			public DummyBizO_DeferTriggerAttributeForSubclassB SubclassB { get; set; }
		}

		public class DummyBizO_DeferTriggerAttributeForSubclassB : DummyBizO_DeferTriggerAttributeForBaseclass
		{
			public DummyBizO_DeferTriggerAttributeForSubclassB(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		[DeferTriggerAndRunBeforeCommit(TriggerName, typeof(IDummyUpdateConditionStrategy))]
		[DeferTriggerAndRunBeforeCommit(TriggerName, typeof(IDeferTriggerOnDeleteConditionStrategy))]
		[DeferTriggerAndRunBeforeCommit(TriggerName, StoredProc, DummyBizoSchema.Constants.PK, typeof(IDummyUpdateConditionStrategy), ValueToRunStoredProcWith = ValueVersion.Current)]
		[DeferTriggerAndRunBeforeCommit(TriggerName, StoredProc, DummyBizoSchema.Constants.Z0_Guid, typeof(IDummyUpdateConditionStrategy), ValueToRunStoredProcWith = ValueVersion.Both)]
		[DeferTriggerAndRunBeforeCommit(TriggerName, StoredProc, DummyBizoSchema.Constants.Z0_Guid, typeof(IDeferTriggerOnDeleteConditionStrategy), ValueToRunStoredProcWith = ValueVersion.Original)]
		public class DummyBizO_DeferTriggerBothAttributesForSameTrigger : DummyBusinessObject
		{
			public const string StoredProc = "DummyDeferedTriggerStoredProc";
			public const string TriggerName = "TG_DummyDeferedTrigger";

			public DummyBizO_DeferTriggerBothAttributesForSameTrigger(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		[DeferTriggerAndRunBeforeCommit(TriggerName, StoredProc, DummyBizoSchema.Constants.Z0_Guid, typeof(IDummyUpdateConditionStrategy), ExtraParamsForStoredProc = SecondParamForStoredProc)]
		public class DummyBizO_DeferTriggerWithSecondParamForStoredProc : DummyBusinessObject
		{
			public const string StoredProc = "DummyDeferedTriggerStoredProc";
			public const string TriggerName = "TG_DummyDeferedTrigger";
			public const string SecondParamForStoredProc = "2";

			public DummyBizO_DeferTriggerWithSecondParamForStoredProc(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		[DeferTriggerAndRunBeforeCommit(TriggerName, StoredProc, DummyBizoSchema.Constants.Z0_Guid, typeof(IDummyUpdateConditionStrategy), ExtraParamsForStoredProc = MultipleParamsForStoredProc)]
		public class DummyBizO_DeferTriggerWithMultipleParamsForStoredProc : DummyBusinessObject
		{
			public const string StoredProc = "DummyDeferedTriggerStoredProc";
			public const string TriggerName = "TG_DummyDeferedTrigger";
			public const string MultipleParamsForStoredProc = "2, 3, 4";

			public DummyBizO_DeferTriggerWithMultipleParamsForStoredProc(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		#endregion
	}
}
