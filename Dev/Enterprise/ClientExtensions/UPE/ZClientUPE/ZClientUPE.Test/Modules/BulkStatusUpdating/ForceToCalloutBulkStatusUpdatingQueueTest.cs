using System;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(ForceToCalloutBulkStatusUpdatingQueue))]
	class ForceToCalloutBulkStatusUpdatingQueueTest : CalloutBulkStatusUpdatingQueueTest
	{
		public void TestSetDefaultValues()
		{
			ForceToCalloutBulkStatusUpdatingQueue queue = new ForceToCalloutBulkStatusUpdatingQueue(Factory);
			AssertEquals("QueueName should default to 'Finance'", CommercialQueueCodeDescriptionPairList.Codes.Finance, queue.QueueName);
			AssertEquals("Reason should be defaulted", ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, queue.Status);
			AssertEquals("Remarks should have the constant 'force to finance'", Callout.ForcedToFinanceQueueRemarks, queue.Reason);
		}

		protected override Type ExpectedUPEProcessQueueValidationHelperType
		{
			get
			{
				return typeof(ForceToCalloutBulkStatusUpdatingValidationHelper);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TestForceToCalloutBulkStatusUpdatingQueue(Factory);
		}

		class TestForceToCalloutBulkStatusUpdatingQueue : ForceToCalloutBulkStatusUpdatingQueue
		{
			public TestForceToCalloutBulkStatusUpdatingQueue(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override void SetDefaultValues()
			{
				base.SetDefaultValues();
				Reason = "";
			}
		}
	}
}
