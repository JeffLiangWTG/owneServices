using System;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(CalloutBulkStatusUpdating))]
	class CalloutBulkStatusUpdatingTest : BulkStatusUpdatingTestCase
	{
		protected override Type ExpectedNonPersistentProcessQueueType
		{
			get
			{
				return typeof(CalloutBulkStatusUpdatingQueue);
			}
		}

		protected override ProcessQueueType.Enum ExpectedQueueTypeToBulkUpdate
		{
			get
			{
				return ProcessQueueType.Enum.Commercial;
			}
		}

		protected override IQueueFilterBusinessObject GetNewQueueFilterBusinessObject()
		{
			return new CalloutFilterBusinessObject();
		}

		protected override IProcessQueueParent GetNewItemToBulkUpdate()
		{
			return Factory.NewWithValidTestData<Callout>();
		}

		protected override ZString ValidQueueNameToBulkUpdateTo
		{
			get
			{
				return CommercialQueueCodeDescriptionPairList.Codes.Finance;
			}
		}

		protected override ZString ValidStatusToBulkUpdateTo
		{
			get
			{
				return ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment;
			}
		}

		protected override ZString ValidSubStatusToBulkUpdateTo
		{
			get
			{
				return "";
			}
		}

		protected override bool ExpectedAssignedToEnabled
		{
			get
			{
				return false;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
		}
	}
}
