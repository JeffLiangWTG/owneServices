using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Registry.Testing
{
	[TestedType(typeof(CusHAWBAutoQueueMovement))]
	public class CusHAWBAutoQueueMovementTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestQueue()
		{
			AssertEquals(typeof(NonPersistentCargoReportQueue), AutoQueueMovement.Queue.GetType());
			AssertEquals("Has to be registered as editable child object", true, AutoQueueMovement.IsRegisteredEditableChildObject(AutoQueueMovement.Queue));
		}

		#region Validation Test
		public void TestValidateFreeTextSegmentName()
		{
			AutoQueueMovement.FreeTextSegmentName = "";
			AssertMandatoryValidationError(AutoQueueMovement.FreeTextSegmentNameInfo, true);
			AssertEquals(true, AutoQueueMovement.FreeTextSegmentNameInfo.HasErrors());
			AutoQueueMovement.FreeTextSegmentName = "SOME SEGMENT";
			AssertMandatoryValidationError(AutoQueueMovement.FreeTextSegmentNameInfo, false);
			AssertEquals(false, AutoQueueMovement.FreeTextSegmentNameInfo.HasErrors());
		}

		public void TestValidateFreeTextSegmentValue()
		{
			AutoQueueMovement.FreeTextSegmentValue = "";
			AssertMandatoryValidationError(AutoQueueMovement.FreeTextSegmentValueInfo, true);
			AssertEquals(true, AutoQueueMovement.FreeTextSegmentValueInfo.HasErrors());
			AutoQueueMovement.FreeTextSegmentValue = "SOME VALUE";
			AssertMandatoryValidationError(AutoQueueMovement.FreeTextSegmentValueInfo, false);
			AssertEquals(false, AutoQueueMovement.FreeTextSegmentValueInfo.HasErrors());
		}

		public void TestValidateFreeTextSegmentNameAndValue_InCollection()
		{
			CusHAWBAutoQueueMovementCollection collection = new CusHAWBAutoQueueMovementCollection();
			CusHAWBAutoQueueMovement movement1 = collection.AddNew();
			CusHAWBAutoQueueMovement movement2 = collection.AddNew();
			movement1.FreeTextSegmentName = "SEGMENT1";
			movement1.FreeTextSegmentValue = "VALUE1";
			AssertEquals(false, movement1.HasErrors);
			movement2.FreeTextSegmentName = "SEGMENT2";
			movement2.FreeTextSegmentValue = "VALUE2";
			AssertEquals(false, movement2.HasErrors);
			movement2.FreeTextSegmentValue = "VALUE1";
			AssertEquals("Should not have any errors. Name-value combination is still unique", false, movement2.HasErrors);
			movement2.FreeTextSegmentName = "SEGMENT1";
			AssertPropertyIsUniqueInCollectionValidationError(movement2.FreeTextSegmentNameInfo, true);
			movement2.RunPreSaveValidation();
			movement1.RunPreSaveValidation();
			AssertPropertyIsUniqueInCollectionValidationError(movement2.FreeTextSegmentValueInfo, true);
			AssertPropertyIsUniqueInCollectionValidationError(movement1.FreeTextSegmentNameInfo, true);
			AssertPropertyIsUniqueInCollectionValidationError(movement1.FreeTextSegmentValueInfo, true);
			movement2.FreeTextSegmentName = "                 sEGmEnT1                        ";
			AssertPropertyIsUniqueInCollectionValidationError(movement2.FreeTextSegmentValueInfo, true);
		}

		public void TestValidatePriority()
		{
			AutoQueueMovement.Priority = -1;
			AssertEquals("Priority value has to be greater than 0", AutoQueueMovement.PriorityInfo.GetErrors().GetFirstMessage());
			AutoQueueMovement.Priority = 2;
			AssertEquals(false, AutoQueueMovement.PriorityInfo.HasErrors());
			CusHAWBAutoQueueMovementCollection collection = new CusHAWBAutoQueueMovementCollection();
			collection.Add(AutoQueueMovement);
			CusHAWBAutoQueueMovement movement2 = collection.AddNew();
			movement2.Priority = 3;
			AssertEquals(false, movement2.PriorityInfo.HasErrors());
			movement2.Priority = 2;
			AutoQueueMovement.RunPreSaveValidation();
			AssertPropertyIsUniqueInCollectionValidationError(movement2.PriorityInfo, true);
			AssertPropertyIsUniqueInCollectionValidationError(AutoQueueMovement.PriorityInfo, true);
		}

		public void TestPreSaveValidation()
		{
			AutoQueueMovement.FreeTextSegmentName = "";
			AutoQueueMovement.FreeTextSegmentValue = "";
			AutoQueueMovement.Priority = -1;
			AutoQueueMovement.ClearAllNotifications();
			AssertEquals("Should not have any errors before RunPreSaveValidation is called", false, AutoQueueMovement.HasErrors);
			AutoQueueMovement.RunPreSaveValidation();
			AssertEquals(true, AutoQueueMovement.FreeTextSegmentNameInfo.HasErrors());
			AssertEquals(true, AutoQueueMovement.FreeTextSegmentValueInfo.HasErrors());
			AssertEquals(true, AutoQueueMovement.PriorityInfo.HasErrors());
		}

		public void TestNoValidationsWhenSuspended()
		{
			using (AutoQueueMovement.GetValidationSuspender())
			{
				AutoQueueMovement.FreeTextSegmentName = "";
				AutoQueueMovement.FreeTextSegmentValue = "";
				AutoQueueMovement.Priority = -1;
				AssertEquals("Should not have any errors. Validation is suspended", false, AutoQueueMovement.HasErrors);
			}
		}

		#endregion
		#region Implementation
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			AutoQueueMovement.FreeTextSegmentName = "CONSOLIDATED STATUS";
			AutoQueueMovement.FreeTextSegmentValue = "HOLD";
			AutoQueueMovement.Queue.QueueName = CargoReportQueueCodeDescriptionPairList.Codes.Hold;
			AutoQueueMovement.Queue.Status = ReasonCodeDescriptionPairList.Codes.BP_PhoneNumberMissing;
			AutoQueueMovement.Queue.SubStatus = StatusCodeDescriptionPairList.Codes.KO_ReceiverContacted;
			AutoQueueMovement.Priority = 12;
			return AutoQueueMovement;
		}

		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		CusHAWBAutoQueueMovement AutoQueueMovement
		{
			get
			{
				return (CusHAWBAutoQueueMovement)base.BizObj;
			}
		}

		void AssertMandatoryValidationError(ZPropertyInfo propertyInfo, bool isExpectingError)
		{
			BusinessObjectValidationTestCase.AssertMandatoryValidationError(propertyInfo, isExpectingError);
		}

		void AssertPropertyIsUniqueInCollectionValidationError(ZPropertyInfo propertyInfo, bool isExpectingError)
		{
			BusinessObjectValidationTestCase.AssertPropertyIsUniqueInCollectionValidationError(propertyInfo, isExpectingError);
		}
		#endregion
	}
}
