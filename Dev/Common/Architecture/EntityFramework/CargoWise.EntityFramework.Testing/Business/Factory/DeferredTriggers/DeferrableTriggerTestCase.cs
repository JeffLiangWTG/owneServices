using System;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	[TestsClassesWithAttributes(typeof(DeferTriggerAndRunBeforeCommitAttribute), typeof(ExcludeDeferrableTriggerTestCaseAttribute))]
	public abstract class DeferrableTriggerTestCase<T> : TestCaseWithFactory
	{
		#region ITestsClassesWithAttribute

		protected Type ClassWithAttributeType
		{
			get { return typeof(T); }
		}

		#endregion

		#region Tests

		public void TestClassHasDeferTriggerAndRunBeforeCommitAttribute()
		{
			var attributes = (ClassWithAttributeType.GetCustomAttributes(typeof(DeferTriggerAndRunBeforeCommitAttribute), true));
			Assert("There must be at least 1 DeferTriggerAndRunBeforeCommitAttribute for each IDeferrableTrigger", attributes.Length > 0);
		}

		public void TestTriggersAreValid()
		{
			foreach (var attribute in GetAttributes())
			{
				AssertTriggerIsValid(attribute);
			}
		}

		public void TestStoredProcsAreValid()
		{
			foreach (var attribute in GetAttributes())
			{
				if (!attribute.IsSuspendTriggerOnly)
				{
					AssertStoredProcIsValid(attribute);
				}
				else
				{
					AssertNull("Is not going to run Stored Procedures after saving, Therefore should not be set.", attribute.StoredProcName);
				}
			}
		}

		public void TestColumnWithRowPkToRunStoredProcOnAreValid()
		{
			foreach (var attribute in GetAttributes())
			{
				if (!attribute.IsSuspendTriggerOnly)
				{
					AssertColumnIsValid(attribute);
				}
				else
				{
					AssertNull("Is not going to run Stored Procedures after saving, Therefore should not be set.", attribute.ColumnWithRowPkToRunStoredProcOn);
				}
			}
		}

		public void TestStrategiesAreValid()
		{
			foreach (var attribute in GetAttributes())
			{
				AssertStrategyIsValid(attribute);
			}
		}

		public void TestStrategyHasNoFields()
		{
			foreach (var attribute in GetAttributes())
			{
				AssertStrategyHasNoFields(attribute);
			}
		}

		public void TestStrategyIsSingleton()
		{
			foreach (var attribute in GetAttributes())
			{
				AssertStrategyIsSingleton(attribute);
			}
		}

		public void TestStrategyHasPrivateConstructor()
		{
			foreach (var attribute in GetAttributes())
			{
				AssertStrategyHasPrivateConstructor(attribute);
			}
		}

		#endregion

		#region Implementation

		DeferTriggerAndRunBeforeCommitAttribute[] GetAttributes()
		{
			return (DeferTriggerAndRunBeforeCommitAttribute[])(ClassWithAttributeType.GetCustomAttributes(typeof(DeferTriggerAndRunBeforeCommitAttribute), true));
		}

		void AssertTriggerIsValid(DeferTriggerAndRunBeforeCommitAttribute attribute)
		{
			Assert($"Trigger {attribute.TriggerName} could not be found.", ((IDbConnected)Factory).Connection.Exists($"FROM sys.objects WHERE type = 'TR' and name = '{attribute.TriggerName}'"));
			Assert($"Trigger {attribute.TriggerName} is not suspendable.", ((IDbConnected)Factory).Connection.Exists($"FROM dbo.SuspendedTriggers WHERE TriggerName = '{attribute.TriggerName}'"));
		}

		void AssertStoredProcIsValid(DeferTriggerAndRunBeforeCommitAttribute attribute)
		{
			Assert($"Stored Proc {attribute.StoredProcName} could not be found.", ((IDbConnected)Factory).Connection.Exists($"FROM sys.objects WHERE type IN ('P', 'PC') AND object_id = OBJECT_ID(N'[dbo].[{attribute.StoredProcName}]')"));
		}

		void AssertColumnIsValid(DeferTriggerAndRunBeforeCommitAttribute attribute)
		{
			var schemaColumn = BusinessObjectFactory.GetTableSchemaFromType(ClassWithAttributeType).GetSchemaColumn(attribute.ColumnWithRowPkToRunStoredProcOn);
			AssertNotNull($"Column {attribute.ColumnWithRowPkToRunStoredProcOn} could not be found.", schemaColumn);
			AssertEquals($"Column {attribute.ColumnWithRowPkToRunStoredProcOn} is not a guid column.", SchemaColumnType.Guid, schemaColumn.ColumnType);
		}

		void AssertStrategyIsValid(DeferTriggerAndRunBeforeCommitAttribute attribute)
		{
			var strategy = ObjectFactory.Get(attribute.DeferTriggerConditionStrategyType.Name);
			Assert($"Strategy {attribute.DeferTriggerConditionStrategyType.Name} is not an IDeferTriggerConditionStrategy.", strategy is IDeferTriggerConditionStrategy);
		}

		void AssertStrategyHasNoFields(DeferTriggerAndRunBeforeCommitAttribute attribute)
		{
			var strategy = ObjectFactory.Get(attribute.DeferTriggerConditionStrategyType.Name);
			Assert("Strategy must not have any fields", strategy.GetType().GetFields().Length == 0);
		}

		void AssertStrategyIsSingleton(DeferTriggerAndRunBeforeCommitAttribute attribute)
		{
			var strategy = ObjectFactory.Get(attribute.DeferTriggerConditionStrategyType.Name);
			AssertEquals("Strategy must be a singleton", strategy, ObjectFactory.Get(attribute.DeferTriggerConditionStrategyType.Name));
		}

		void AssertStrategyHasPrivateConstructor(DeferTriggerAndRunBeforeCommitAttribute attribute)
		{
			var strategy = ObjectFactory.Get(attribute.DeferTriggerConditionStrategyType.Name);
			Assert("Strategy must have no public constructor", strategy.GetType().GetConstructors().Length == 0);
		}

		#endregion
	}
}
