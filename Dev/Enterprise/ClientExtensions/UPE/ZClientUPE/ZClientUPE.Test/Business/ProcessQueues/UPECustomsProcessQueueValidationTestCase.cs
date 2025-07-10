using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal abstract class UPECustomsProcessQueueValidationTestCase : BusinessObjectValidationTestCase
	{
		public abstract void TestValidateP4_CustomsQueue();

		public abstract void TestValidateP4_CustomsStatus();

		public abstract void TestValidateP4_CustomsSubStatus();

		public abstract void TestValidateP4_GS_NKCustomsTaskAssignedTo();

		public void TestCustomsValidationHelperType()
		{
			UPECustomsProcessQueueValidation validation = GetNewValidation();
			AssertEquals(ExpectedCustomsValidationHelperType, validation.CustomsQueueValidationHelper.GetType());
		}

		#region Implementation
		protected UPEProcessQueue Queue
		{
			get
			{
				if (fQueue == null)
				{
					fQueue = GetNewUPEProcessQueue();
				}

				return fQueue;
			}
		}

		protected abstract Type ExpectedCustomsValidationHelperType { get; }

		protected abstract Type UPECustomsProcessQueueValidationTypeToTest { get; }

		protected abstract UPECustomsProcessQueueValidation GetNewValidation();
		protected abstract UPEProcessQueue GetNewUPEProcessQueue();
		UPEProcessQueue fQueue;
		#endregion
	}
}
