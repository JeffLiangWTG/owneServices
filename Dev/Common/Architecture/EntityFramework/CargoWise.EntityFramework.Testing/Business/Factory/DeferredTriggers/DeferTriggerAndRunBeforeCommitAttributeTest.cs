using System;
using System.Linq;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DeferTriggerAndRunBeforeCommitAttributeTest : TestCaseWithFactory
	{
		#region TestConstructorAndDefaults

		public void TestConstructorAndDefaults()
			=> TestConstructorAndDefaultsCore(typeof(DummyClassWithDeferredTrigger), isSuspendOnly: false, extraParams: null);

		public void TestConstructorAndDefaults_TriggerOnly()
			=> TestConstructorAndDefaultsCore(typeof(DummyClassWithDeferredTriggerOnly), isSuspendOnly: true, extraParams: "");

		public void TestConstructorAndDefaults_WithSecondParamForStoredProc()
			=> TestConstructorAndDefaultsCore(typeof(DummyClassWithDeferredTriggerWhichPassesASecondParam), isSuspendOnly: false, extraParams: "1");

		public void TestConstructorAndDefaults_WithExtraParamsForStoredProc()
			=> TestConstructorAndDefaultsCore(typeof(DummyClassWithDeferredTriggerWhichPassesExtraParams), isSuspendOnly: false, extraParams: "1, 2, 3");

		void TestConstructorAndDefaultsCore(Type type, bool isSuspendOnly, string extraParams)
		{
			var attribute = (DeferTriggerAndRunBeforeCommitAttribute)type
				.GetCustomAttributes(typeof(DeferTriggerAndRunBeforeCommitAttribute), false).Single();

			AssertValues(attribute, isSuspendOnly, extraParams);
		}

		#endregion

		#region Test GetAttributes

		public void TestGetAttributes() => TestGetAttributesCore(typeof(DummyClassWithDeferredTrigger), isSuspendOnly: false, extraParams: "");

		public void TestGetAttributes_TriggerOnly() => TestGetAttributesCore(typeof(DummyClassWithDeferredTriggerOnly), isSuspendOnly: true, extraParams: null);

		public void TestGetAttributes_WithSecondParamForStoredProc() => TestGetAttributesCore(typeof(DummyClassWithDeferredTriggerWhichPassesASecondParam), isSuspendOnly: false, extraParams: "1");

		public void TestGetAttributes_WithExtraParamsForStoredProc() => TestGetAttributesCore(typeof(DummyClassWithDeferredTriggerWhichPassesExtraParams), isSuspendOnly: false, extraParams: "1, 2, 3");

		public void TestGetAttributesCore(Type type, bool isSuspendOnly, string extraParams)
		{
			var attributes = DeferTriggerAndRunBeforeCommitAttribute.GetAttributes(type);
			AssertEquals("There should be only 1 attribute", 1, attributes.Length);
			AssertValues(attributes.Single(), isSuspendOnly, extraParams);
			AssertEquals("Array should be cached.", attributes, DeferTriggerAndRunBeforeCommitAttribute.GetAttributes(type));
		}

		#endregion

		#region Implementation

		void AssertValues(DeferTriggerAndRunBeforeCommitAttribute attribute, bool isSuspendOnly, string extraParams)
		{
			AssertEquals(attribute.TriggerName, "Dummy Trigger");
			AssertEquals(attribute.DeferTriggerConditionStrategyType, typeof(DeferTriggerOnDeleteConditionStrategy));
			AssertEquals(attribute.IsSuspendTriggerOnly, isSuspendOnly);
			if (isSuspendOnly)
			{
				AssertNull("Default - not set", attribute.StoredProcName);
				AssertNull("Default - not set", attribute.ColumnWithRowPkToRunStoredProcOn);
				AssertNull("Default - not set", attribute.ExtraParamsForStoredProc);
				AssertEquals("Default - not set", attribute.ValueToRunStoredProcWith, default(ValueVersion));
			}
			else
			{
				AssertEquals(attribute.StoredProcName, "Dummy Stored Proc");
				AssertEquals(attribute.ColumnWithRowPkToRunStoredProcOn, "Dummy Column");
				AssertEquals(attribute.ExtraParamsForStoredProc, string.IsNullOrEmpty(extraParams) ? null : extraParams);
				AssertEquals(attribute.ValueToRunStoredProcWith, ValueVersion.Current);
			}
		}

		[DeferTriggerAndRunBeforeCommit("Dummy Trigger", "Dummy Stored Proc", "Dummy Column", typeof(DeferTriggerOnDeleteConditionStrategy))]
		class DummyClassWithDeferredTrigger
		{
		}

		[DeferTriggerAndRunBeforeCommit("Dummy Trigger", "Dummy Stored Proc", "Dummy Column", typeof(DeferTriggerOnDeleteConditionStrategy), ExtraParamsForStoredProc = "1")]
		class DummyClassWithDeferredTriggerWhichPassesASecondParam
		{
		}

		[DeferTriggerAndRunBeforeCommit("Dummy Trigger", "Dummy Stored Proc", "Dummy Column", typeof(DeferTriggerOnDeleteConditionStrategy), ExtraParamsForStoredProc = "1, 2, 3")]
		class DummyClassWithDeferredTriggerWhichPassesExtraParams
		{
		}

		[DeferTriggerAndRunBeforeCommit("Dummy Trigger", typeof(DeferTriggerOnDeleteConditionStrategy))]
		class DummyClassWithDeferredTriggerOnly
		{
		}

		#endregion
	}
}
