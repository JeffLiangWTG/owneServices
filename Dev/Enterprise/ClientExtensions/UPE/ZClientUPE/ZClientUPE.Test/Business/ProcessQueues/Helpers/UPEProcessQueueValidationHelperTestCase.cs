using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal abstract class UPEProcessQueueValidationHelperTestCase : UPEProcessQueueHelperTestCase
	{
		public void TestValidateQueueName()
		{
			ValidateQueueName("");
			AssertMandatoryValidationError(ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo, !ExpectedAllowEmptyQueueNameAndStatuses);
			AssertMandatoryValidationError(ValidationHelperForNonPersistentQueue.Queue.QueueNameInfo, !ExpectedAllowEmptyQueueNameAndStatuses);
			ValidateQueueName("()*");
			AssertMandatoryValidationError(ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo, false);
			AssertMandatoryValidationError(ValidationHelperForNonPersistentQueue.Queue.QueueNameInfo, false);
			AssertListValidationInvalidCodeError(ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo, true);
			AssertListValidationInvalidCodeError(ValidationHelperForNonPersistentQueue.Queue.QueueNameInfo, true);
			ValidateQueueName(ValidationHelperForUPEProcessQueue.Queue.Lookups.QueueList[0].Code);
			AssertListValidationInvalidCodeError(ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo, false);
			AssertListValidationInvalidCodeError(ValidationHelperForNonPersistentQueue.Queue.QueueNameInfo, false);
		}

		public void TestValidateQueueName_MovingFromQueueToQueue_AdministratorCanMoveQueueAnywhere()
		{
			bool wasController = GlbStaff.CurrentUser.GS_IsController;
			GlbStaff.CurrentUser.GS_IsController = true;
			try
			{
				ValidationHelperForUPEProcessQueue.Queue.QueueName = DefaultQueueCodeDescriptionPairList.Codes.Hold;
				Factory.Save();
				ValidationHelperForUPEProcessQueue.Queue.QueueName = DefaultQueueCodeDescriptionPairList.Codes.Completed;
				AssertNoErrors("Administrator can move the queue to anywhere", ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = wasController;
			}
		}

		public void TestValidateReasonCode()
		{
			if (QueueNamesWhichRequireReasonCode.Length > 0)
			{
				ValidationHelperForUPEProcessQueue.Queue.QueueName = QueueNamesWhichRequireReasonCode[0];
				ValidationHelperForNonPersistentQueue.Queue.QueueName = QueueNamesWhichRequireReasonCode[0];
				ValidateReasonCode(ValidationHelperForUPEProcessQueue.Queue.Lookups.StatusList[0].Code);
				AssertListValidationInvalidCodeError(ValidationHelperForUPEProcessQueue.Queue.StatusInfo, false);
				AssertListValidationInvalidCodeError(ValidationHelperForNonPersistentQueue.Queue.StatusInfo, false);
			}

			ValidateReasonCode("0+@");
			AssertListValidationInvalidCodeError(ValidationHelperForUPEProcessQueue.Queue.StatusInfo, true);
			AssertListValidationInvalidCodeError(ValidationHelperForNonPersistentQueue.Queue.StatusInfo, true);
		}

		public void TestValidateStatusCode()
		{
			if (ReasonCodesWhichRequireStatusCode.Length > 0)
			{
				ValidationHelperForUPEProcessQueue.Queue.Status = ReasonCodesWhichRequireStatusCode[0];
				ValidationHelperForNonPersistentQueue.Queue.Status = ReasonCodesWhichRequireStatusCode[0];
				ValidateStatusCode("");
				AssertMandatoryValidationError(ValidationHelperForUPEProcessQueue.Queue.SubStatusInfo, true);
				AssertMandatoryValidationError(ValidationHelperForNonPersistentQueue.Queue.SubStatusInfo, true);
				ValidateStatusCode(ValidationHelperForUPEProcessQueue.Queue.Lookups.SubStatusList[0].Code);
				AssertListValidationInvalidCodeError(ValidationHelperForUPEProcessQueue.Queue.SubStatusInfo, false);
				AssertListValidationInvalidCodeError(ValidationHelperForNonPersistentQueue.Queue.SubStatusInfo, false);
			}

			ValidateStatusCode("**");
			AssertListValidationInvalidCodeError(ValidationHelperForUPEProcessQueue.Queue.SubStatusInfo, true);
			AssertListValidationInvalidCodeError(ValidationHelperForNonPersistentQueue.Queue.SubStatusInfo, true);
		}

		public void TestValidateTaskAssignedTo()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AAA";
			ValidationHelperForUPEProcessQueue.Queue.Lookups.TaskAssignedToList.AdditionalFilter = new ZQuery(GlbStaffSchema.PK, staff.PK);
			ValidationHelperForNonPersistentQueue.Queue.Lookups.TaskAssignedToList.AdditionalFilter = new ZQuery(GlbStaffSchema.PK, staff.PK);
			ValidateAssignedTo("");
			AssertMandatoryValidationError(ValidationHelperForUPEProcessQueue.Queue.AssignedToInfo, false);
			AssertMandatoryValidationError(ValidationHelperForNonPersistentQueue.Queue.AssignedToInfo, false);
			ValidateAssignedTo("_)(");
			AssertListValidationInvalidCodeError(ValidationHelperForUPEProcessQueue.Queue.AssignedToInfo, true);
			AssertListValidationInvalidCodeError(ValidationHelperForNonPersistentQueue.Queue.AssignedToInfo, true);
			ValidateAssignedTo("AAA");
			AssertListValidationInvalidCodeError(ValidationHelperForUPEProcessQueue.Queue.AssignedToInfo, false);
			AssertListValidationInvalidCodeError(ValidationHelperForNonPersistentQueue.Queue.AssignedToInfo, false);
		}

		protected void ValidateQueueName(string queueName)
		{
			ValidationHelperForUPEProcessQueue.Queue.QueueName = queueName;
			ValidationHelperForUPEProcessQueue.Queue.QueueNameInfo.ClearAllNotifications();
			ValidationHelperForUPEProcessQueue.ValidateQueueName();
			ValidationHelperForNonPersistentQueue.Queue.QueueName = queueName;
			ValidationHelperForNonPersistentQueue.Queue.QueueNameInfo.ClearAllNotifications();
			ValidationHelperForNonPersistentQueue.ValidateQueueName();
		}

		protected void ValidateReasonCode(string reasonCode)
		{
			ValidationHelperForUPEProcessQueue.Queue.Status = reasonCode;
			ValidationHelperForUPEProcessQueue.Queue.StatusInfo.ClearAllNotifications();
			ValidationHelperForUPEProcessQueue.ValidateReasonCode();
			ValidationHelperForNonPersistentQueue.Queue.Status = reasonCode;
			ValidationHelperForNonPersistentQueue.Queue.StatusInfo.ClearAllNotifications();
			ValidationHelperForNonPersistentQueue.ValidateReasonCode();
		}

		protected void ValidateStatusCode(string statusCode)
		{
			ValidationHelperForUPEProcessQueue.Queue.SubStatus = statusCode;
			ValidationHelperForUPEProcessQueue.Queue.SubStatusInfo.ClearAllNotifications();
			ValidationHelperForUPEProcessQueue.ValidateStatusCode();
			ValidationHelperForNonPersistentQueue.Queue.SubStatus = statusCode;
			ValidationHelperForNonPersistentQueue.Queue.SubStatusInfo.ClearAllNotifications();
			ValidationHelperForNonPersistentQueue.ValidateStatusCode();
		}

		protected void ValidateAssignedTo(string assignedTo)
		{
			ValidationHelperForUPEProcessQueue.Queue.AssignedTo = assignedTo;
			ValidationHelperForUPEProcessQueue.Queue.AssignedToInfo.ClearAllNotifications();
			ValidationHelperForUPEProcessQueue.ValidateTaskAssignedTo();
			ValidationHelperForNonPersistentQueue.Queue.AssignedTo = assignedTo;
			ValidationHelperForNonPersistentQueue.Queue.AssignedToInfo.ClearAllNotifications();
			ValidationHelperForNonPersistentQueue.ValidateTaskAssignedTo();
		}

		public void TestRequiresReasonCode()
		{
			ValidateQueueName("");
			ValidateReasonCode("");
			AssertMandatoryValidationError(ValidationHelperForUPEProcessQueue.Queue.StatusInfo, false);
			AssertMandatoryValidationError(ValidationHelperForNonPersistentQueue.Queue.StatusInfo, false);
			foreach (string queueName in QueueNamesWhichRequireReasonCode)
			{
				ValidateQueueName(queueName);
				ValidateReasonCode("");
				AssertMandatoryValidationError(ValidationHelperForUPEProcessQueue.Queue.StatusInfo, true);
				AssertMandatoryValidationError(ValidationHelperForNonPersistentQueue.Queue.StatusInfo, true);
			}
		}

		public void TestRequiresStatusCode()
		{
			ValidateReasonCode("");
			ValidateStatusCode("");
			AssertMandatoryValidationError(ValidationHelperForUPEProcessQueue.Queue.SubStatusInfo, false);
			AssertMandatoryValidationError(ValidationHelperForNonPersistentQueue.Queue.SubStatusInfo, false);
			foreach (string reasonCode in ReasonCodesWhichRequireStatusCode)
			{
				ValidateReasonCode(reasonCode);
				ValidateStatusCode("");
				AssertMandatoryValidationError(ValidationHelperForUPEProcessQueue.Queue.SubStatusInfo, true);
				AssertMandatoryValidationError(ValidationHelperForNonPersistentQueue.Queue.SubStatusInfo, true);
			}
		}

		protected virtual bool ExpectedAllowEmptyQueueNameAndStatuses
		{
			get
			{
				return false;
			}
		}

		#region Implementation
		protected UPEProcessQueueValidationHelper ValidationHelperForUPEProcessQueue
		{
			get
			{
				return (UPEProcessQueueValidationHelper)base.HelperForUPEProcessQueue;
			}
		}

		protected UPEProcessQueueValidationHelper ValidationHelperForNonPersistentQueue
		{
			get
			{
				return (UPEProcessQueueValidationHelper)base.HelperForNonPersistentProcessQueue;
			}
		}

		protected override sealed UPEProcessQueueHelperBase GetNewProcessQueueHelperBaseForUPEProcessQueue(UPEProcessQueue queue)
		{
			return GetNewProcessQueueValidationHelperForUPEProcessQueue(queue);
		}

		protected override sealed UPEProcessQueueHelperBase GetNewProcessQueueHelperBaseForNonPersistentProcessQueue(NonPersistentProcessQueue queue)
		{
			return GetNewProcessQueueValidationHelperForNonPersistentProcessQueue(queue);
		}

		protected void AssertMandatoryValidationError(ZPropertyInfo propertyInfo, bool isExpectingError)
		{
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(propertyInfo, isExpectingError);
		}

		protected void AssertListValidationInvalidCodeError(ZPropertyInfo propertyInfo, bool isExpectingError)
		{
			BusinessObjectValidationTestCase.AssertListValidationInvalidCodeError(propertyInfo, isExpectingError);
		}

		protected abstract string[] QueueNamesWhichRequireReasonCode { get; }

		protected abstract string[] ReasonCodesWhichRequireStatusCode { get; }

		protected abstract UPEProcessQueueValidationHelper GetNewProcessQueueValidationHelperForUPEProcessQueue(UPEProcessQueue queue);
		protected abstract UPEProcessQueueValidationHelper GetNewProcessQueueValidationHelperForNonPersistentProcessQueue(NonPersistentProcessQueue queue);
		bool WasController;
		protected override void SetUp()
		{
			base.SetUp();
			WasController = GlbStaff.CurrentUser.GS_IsController;
			GlbStaff.CurrentUser.GS_IsController = false;
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbStaff.CurrentUser.GS_IsController = WasController;
		}
		#endregion
	}
}
