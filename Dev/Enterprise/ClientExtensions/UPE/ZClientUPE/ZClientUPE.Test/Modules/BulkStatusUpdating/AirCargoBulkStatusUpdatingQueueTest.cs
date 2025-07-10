using System;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(AirCargoBulkStatusUpdatingQueue))]
	internal class AirCargoBulkStatusUpdatingQueueTest : NonPersistentCargoReportQueueTest
	{
		protected override Type ExpectedUPEProcessQueueValidationHelperType
		{
			get
			{
				return typeof(AirCargoBulkStatusUpdatingValidationHelper);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AirCargoBulkStatusUpdatingQueue(Factory);
		}
	}
}
