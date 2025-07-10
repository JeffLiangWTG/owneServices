using System;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(JobDecBulkStatusUpdatingQueue))]
	internal class JobDecBulkStatusUpdatingQueueTest : NonPersistentDeclarationQueueTest
	{
		protected override Type ExpectedUPEProcessQueueValidationHelperType
		{
			get
			{
				return typeof(JobDecBulkStatusUpdatingValidationHelper);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new JobDecBulkStatusUpdatingQueue(Factory);
		}
	}
}
