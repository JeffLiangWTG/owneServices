using System;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(CalloutBulkStatusUpdatingQueue))]
	internal class CalloutBulkStatusUpdatingQueueTest : NonPersistentCalloutQueueTest
	{
		protected override Type ExpectedUPEProcessQueueValidationHelperType
		{
			get
			{
				return typeof(CalloutBulkStatusUpdatingValidationHelper);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CalloutBulkStatusUpdatingQueue(Factory);
		}
	}
}
