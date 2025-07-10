using System;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(ForceToCalloutBulkStatusUpdating))]
	sealed class ForceToCalloutBulkStatusUpdatingTest : CalloutBulkStatusUpdatingTest
	{
		public void TestIsExcludedFromBISIWarning_SetToTrue()
		{
			Callout callout = Factory.NewWithValidTestData<Callout>();
			Factory.Save();
			ForceToCalloutBulkStatusUpdating bulkUpdater = new ForceToCalloutBulkStatusUpdating(Factory, new Callout[] { callout });
			bulkUpdater.BulkUpdate(new NotificationBuffer());
			AssertEquals("IsExcludedFromBISIWarning should be set to true", true, callout.IsExcludedFromBISIWarning);
		}

		protected override Type ExpectedNonPersistentProcessQueueType
		{
			get
			{
				return typeof(ForceToCalloutBulkStatusUpdatingQueue);
			}
		}

		protected override ProcessQueueType.Enum ExpectedQueueTypeToBulkUpdate
		{
			get
			{
				return ProcessQueueType.Enum.Commercial;
			}
		}

		protected override bool ExpectedQueueNameMandatory
		{
			get
			{
				return true;
			}
		}

		protected override BulkStatusUpdating GetNewBulkUpdatingBusinessObject(IProcessQueueParent[] itemsToBulkUpdate)
		{
			return new TestForceToCalloutBulkStatusUpdating(Factory, itemsToBulkUpdate);
		}

		protected override IQueueFilterBusinessObject GetNewQueueFilterBusinessObject()
		{
			return new EnquiryFilterBusinessObject();
		}

		#region Test Classes
		class TestForceToCalloutBulkStatusUpdating : ForceToCalloutBulkStatusUpdating
		{
			public TestForceToCalloutBulkStatusUpdating(BusinessObjectFactory factory, IProcessQueueParent[] itemsToBulkUpdate) : base(factory, itemsToBulkUpdate)
			{
			}

			protected override NonPersistentProcessQueue GetNewNonPersistentProcessQueue()
			{
				return new TestForceToCalloutBulkStatusUpdatingQueue(Factory);
			}
		}

		class TestForceToCalloutBulkStatusUpdatingQueue : ForceToCalloutBulkStatusUpdatingQueue
		{
			public TestForceToCalloutBulkStatusUpdatingQueue(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override UPEProcessQueueValidationHelper GetNewUPEProcessQueueValidationHelper()
			{
				return new TestForceToCalloutBulkStatusUpdatingValidationHelper(this);
			}

			protected override void SetDefaultValues()
			{
				base.SetDefaultValues();
				Reason = "";
			}
		}

		class TestForceToCalloutBulkStatusUpdatingValidationHelper : ForceToCalloutBulkStatusUpdatingValidationHelper
		{
			public TestForceToCalloutBulkStatusUpdatingValidationHelper(CalloutBulkStatusUpdatingQueue queue) : base(queue)
			{
			}

			public override void ValidateRemarks()
			{
				// need to suppress this so the base tests pass
			}
		}
		#endregion
	}
}
