using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal abstract class NonPersistentProcessQueueTestCase : NonPersistentBusinessObjectTestCase
	{
		#region IActiveProcessQueue Members test
		public abstract void TestQueueName();

		public abstract void TestStatus();

		public abstract void TestSubStatus();

		public abstract void TestReason();

		public abstract void TestAssignedTo();

		public abstract void TestP4_CustomDate4();

		public abstract void TestP4_CustomAttrib8();

		public abstract void TestLookups();

		#endregion
		#region Validation Tests
		public abstract void TestValidateQueueNameCalledInTheSetter();

		public abstract void TestValidateStatusCalledInTheSetter();

		public abstract void TestValidateSubStatusCalledInTheSetter();

		public abstract void TestValidateAssignedToCalledInTheSetter();

		public abstract void TestRunPreSaveValidation();

		public abstract void TestValidationsNotCalledWhenSuspended();

		public abstract void TestHasSubStatuses();

		#endregion
		#region Implementation
		protected void AssertMandatoryValidationError(ZPropertyInfo propertyInfo, bool isExpectingError)
		{
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(propertyInfo, isExpectingError);
		}

		protected void AssertListValidationError(ZPropertyInfo propertyInfo, bool isExpectingError)
		{
			BusinessObjectValidationTestCase.AssertListValidationInvalidCodeError(propertyInfo, isExpectingError);
		}

		protected abstract SchemaStringColumn ExpectedQueueNameSchemaColumn { get; }

		protected abstract SchemaStringColumn ExpectedStatusSchemaColumn { get; }

		protected abstract SchemaStringColumn ExpectedSubStatusSchemaColumn { get; }

		protected abstract SchemaStringColumn ExpectedReasonSchemaColumn { get; }

		protected abstract SchemaStringColumn ExpectedAssignedToSchemaColumn { get; }

		protected abstract Type ExpectedNonPersistentProcessQueueLookupsType { get; }

		protected abstract Type ExpectedUPEProcessQueueValidationHelperType { get; }
		#endregion
	}
}
