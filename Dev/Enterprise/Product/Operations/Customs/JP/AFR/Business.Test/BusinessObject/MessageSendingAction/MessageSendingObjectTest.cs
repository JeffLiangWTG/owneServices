using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(MessageSendingObject))]
	class MessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestJPM_DeleteReasonText()
		{
			PrepareRefCusCodeList();

			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			var sendingBill = new MessageSendingObject(bill, ActionCode.AmendingDelete, TestSendingAction)
			{
				JPM_Send = true,
				JPM_DeleteReasonCode = "1"
			};
			AssertEquals("Cancelation of Loading", sendingBill.JPM_DeleteReasonText);
			sendingBill.JPM_DeleteReasonCode = "3";
			AssertEquals("Change of B/L Number", sendingBill.JPM_DeleteReasonText);
			sendingBill.JPM_DeleteReasonCode = "4";
			AssertEquals("Misregistration", sendingBill.JPM_DeleteReasonText);
			sendingBill.JPM_DeleteReasonCode = "5";
			AssertEquals(ZString.Empty, sendingBill.JPM_DeleteReasonText);

			sendingBill.UpdateAction(ActionCode.AmendingAdd);
			AssertEquals(ZString.Empty, sendingBill.JPM_DeleteReasonCode);
			AssertEquals(ZString.Empty, sendingBill.JPM_DeleteReasonText);
		}

		public void TestDeleteReasonCode()
		{
			PrepareRefCusCodeList();

			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			var sendingBill = new MessageSendingObject(bill, ActionCode.AmendingDelete, TestSendingAction)
			{
				JPM_Send = true,
				JPM_DeleteReasonCode = "1"
			};
			AssertNotNull(sendingBill.DeleteReasonCode);
			AssertEquals("Cancelation of Loading", sendingBill.DeleteReasonCode.ZZD_Description);

			sendingBill.JPM_DeleteReasonCode = "3";
			AssertNotNull(sendingBill.DeleteReasonCode);
			AssertEquals("Change of B/L Number", sendingBill.DeleteReasonCode.ZZD_Description);

			sendingBill.JPM_DeleteReasonCode = "4";
			AssertNotNull(sendingBill.DeleteReasonCode);
			AssertEquals("Misregistration", sendingBill.DeleteReasonCode.ZZD_Description);

			sendingBill.JPM_DeleteReasonCode = "5";
			AssertNotNull(sendingBill.DeleteReasonCode);
			AssertEquals("Other Reason", sendingBill.DeleteReasonCode.ZZD_Description);

			sendingBill.UpdateAction(ActionCode.AmendingAdd);
			AssertNull(sendingBill.DeleteReasonCode);
		}

		public void TestJPM_DeleteReasonCode_ReadOnly()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			var sendingBill = new MessageSendingObject(bill, ActionCode.AmendingDelete, TestSendingAction) { JPM_Send = true };
			AssertEquals(false, sendingBill.JPM_DeleteReasonCodeInfo.ReadOnly);
			sendingBill.JPM_Send = ZBool.False;
			AssertEquals(true, sendingBill.JPM_DeleteReasonCodeInfo.ReadOnly);
		}

		public void TestJPM_DeleteReasonText_ReadOnly()
		{
			PrepareRefCusCodeList();

			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			var sendingBill = new MessageSendingObject(bill, ActionCode.AmendingDelete, TestSendingAction)
			{
				JPM_Send = true,
				JPM_DeleteReasonCode = "5"
			};
			AssertEquals(ZString.Empty, sendingBill.JPM_DeleteReasonText);
			AssertEquals(false, sendingBill.JPM_DeleteReasonTextInfo.ReadOnly);
			sendingBill.JPM_DeleteReasonCode = "1";
			AssertEquals("Cancelation of Loading", sendingBill.JPM_DeleteReasonText);
			AssertEquals(true, sendingBill.JPM_DeleteReasonTextInfo.ReadOnly);
			sendingBill.JPM_DeleteReasonCode = "3";
			AssertEquals("Change of B/L Number", sendingBill.JPM_DeleteReasonText);
			AssertEquals(true, sendingBill.JPM_DeleteReasonTextInfo.ReadOnly);
			sendingBill.JPM_DeleteReasonCode = "4";
			AssertEquals("Misregistration", sendingBill.JPM_DeleteReasonText);
			AssertEquals(true, sendingBill.JPM_DeleteReasonTextInfo.ReadOnly);
		}

		public void TestDeleteReasonCodeList()
		{
			var startDate = ZDateTime.Now.AddDays(-1);
			var endDate = ZDateTime.Now.AddDays(1);
			TestCaseHelper.ClearTable(RefCusCodeListAttribute.Schema.TableName);
			TestCaseHelper.ClearTable(RefCusCodeList.Schema.TableName);
			TestCaseHelper.ClearTable(RefCusCodeType.Schema.TableName);
			var universalReferenceTestHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			universalReferenceTestHelper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanAFRDeleteReason, "Japan AFR Delete Reason");
			universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanAFRDeleteReason, "0", "DESC", startDate, endDate);
			universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanAFRDeleteReason, "1", "DESC", startDate, endDate);
			var jp2dr = universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanAFRDeleteReason, "2", "DESC", startDate, endDate);
			universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanAFRDeleteReason, "3", "DESC", endDate, endDate.AddDays(1));
			universalReferenceTestHelper.CreateCusCodeListAttribute(jp2dr.PK, RefCusCodeListAttributeTypes.Codes.FreeTextRequired, ZString.Empty);
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			var sendingBill = new MessageSendingObject(bill, ActionCode.AmendingDelete, TestSendingAction) { JPM_Send = true };
			AssertEquals(2, sendingBill.DeleteReasonCodeList.Count);
			AssertEquals(true, sendingBill.DeleteReasonCodeList.ContainsOnly("1", "2"));
		}

		public void TestIsBillSendAndDelete()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			var sendingBill = new MessageSendingObject(bill, ActionCode.AmendingDelete, TestSendingAction) { JPM_Send = true };
			AssertEquals(true, sendingBill.IsBillSendAndDelete);
			sendingBill.JPM_Send = ZBool.False;
			AssertEquals(false, sendingBill.IsBillSendAndDelete);
		}

		public void TestIsDeleteReasonCodeFreeTextRequired()
		{
			PrepareRefCusCodeList();

			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			var sendingBill = new MessageSendingObject(bill, ActionCode.AmendingDelete, TestSendingAction)
			{
				JPM_Send = true,
				JPM_DeleteReasonCode = "5"
			};
			AssertEquals(ZString.Empty, sendingBill.JPM_DeleteReasonText);
			AssertEquals(true, sendingBill.IsDeleteReasonCodeFreeTextRequired);
			sendingBill.JPM_DeleteReasonCode = "1";
			AssertEquals("Cancelation of Loading", sendingBill.JPM_DeleteReasonText);
			AssertEquals(false, sendingBill.IsDeleteReasonCodeFreeTextRequired);
		}

		void PrepareRefCusCodeList()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			TestCaseHelper.ClearTable(RefCusCodeListAttribute.Schema.TableName);
			TestCaseHelper.ClearTable(RefCusCodeList.Schema.TableName);
			TestCaseHelper.ClearTable(RefCusCodeType.Schema.TableName);
			var universalReferenceTestHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			universalReferenceTestHelper.CreateCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanAFRDeleteReason, "AFR Delete Reason");
			var jp1dr = universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanAFRDeleteReason, "1", startDate, endDate);
			jp1dr.ZZD_Description = "Cancelation of Loading";
			var jp2dr = universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanAFRDeleteReason, "3", startDate, endDate);
			jp2dr.ZZD_Description = "Change of B/L Number";
			var jp3dr = universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanAFRDeleteReason, "4", startDate, endDate);
			jp3dr.ZZD_Description = "Misregistration";
			var jp4dr = universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanAFRDeleteReason, "5", startDate, endDate);
			jp4dr.ZZD_Description = "Other Reason";
			universalReferenceTestHelper.CreateCusCodeListAttribute(jp4dr.PK, RefCusCodeListAttributeTypes.Codes.FreeTextRequired,
				ZString.Empty);
			Factory.Save();
		}

		public void TestRegisterAndUnRegisterBillAsEditableChildObject()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			var sendingBill = new MessageSendingObject(bill, ActionCode.Registering, TestSendingAction) { JPM_Send = true };
			AssertEquals(true, sendingBill.IsRegisteredEditableChildObject(bill));
			sendingBill.JPM_Send = false;
			AssertEquals(false, sendingBill.IsRegisteredEditableChildObject(bill));
		}

		public void TestUnRegisterBillAsEditableChildObject()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			bill.JPB_BillNumber = ZString.Empty;
			AssertHasMessageErrorContaining(bill.JPB_BillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			var notification = bill.JPB_BillNumberInfo.Notifications.ToUniqueMessageListString();
			var sendingBill = new MessageSendingObject(bill, ActionCode.Registering, TestSendingAction);
			CombineAssertions(() =>
			{
				sendingBill.JPM_Send = false;
				AssertEquals(false, sendingBill.IsRegisteredEditableChildObject(bill));
				AssertEquals(false, sendingBill.NotificationsIncludingChildren.ContainsNotificationContaining(notification));

				sendingBill.JPM_Send = true;
				AssertEquals(true, sendingBill.IsRegisteredEditableChildObject(bill));
				AssertEquals(true, sendingBill.NotificationsIncludingChildren.ContainsNotificationContaining(notification));

				sendingBill.UnRegisterBillAsEditableChildObject();
				AssertEquals(false, sendingBill.IsRegisteredEditableChildObject(bill));
				AssertEquals(false, sendingBill.NotificationsIncludingChildren.ContainsNotificationContaining(notification));

				bill.JPB_BillNumber = "MB224332";
				AssertNoMessageErrorContaining(bill.JPB_BillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestDefaults()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			bill.JPB_BillNumber = "MB10232398";
			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			bill.JPB_MessageStatus = MessageStatusList.Codes.ClearHouseBillRegistration;
			var sendingBill = new MessageSendingObject(bill, ActionCode.AmendingUpdate, TestSendingAction);
			AssertEquals(ActionCode.AmendingUpdate, sendingBill.ActionCode);
			AssertEquals(AFRBillCustomsStatusList.Codes.Registered, sendingBill.JPM_ReleaseStatus);
			AssertEquals(MessageStatusList.Codes.ClearHouseBillRegistration, sendingBill.JPM_MessageStatus);
			AssertEquals(AFRSendingActionCodeList.Codes.Update, sendingBill.JPM_ActionCode);
			AssertEquals("MB10232398", sendingBill.JPM_BillOfLadingNumber);
			AssertEquals(true, sendingBill.JPM_Send);
		}

		public void TestUpdateAction()
		{
			using (var inbondDetailInitiator = new InBondDetailInitiatorTestHelper())
			{
				var consol = Factory.New<ForwardingConsol>();
				var header = Factory.New<JPAFRHeader>();
				header.JPH_ParentId = consol.PK;
				header.JPH_ParentTableCode = consol.TablePrefix;
				var bill = header.Bills.AddNew();
				bill.InBondDetailInitiator = inbondDetailInitiator;

				var testDate = ZDateTime.UtcNow;
				bill.JPB_Calc_ESDT = testDate;
				bill.JPB_Calc_EFDT = testDate;
				bill.JPB_BillNumber = "BOL";

				var sendingBill = new MessageSendingObject(bill, ActionCode.AmendingAdd, TestSendingAction);
				AssertEquals(AFRSendingActionCodeList.Codes.Add, sendingBill.JPM_ActionCode);
				AssertEquals(AFRSendingActionCodeList.Codes.Add, sendingBill.JPM_ActionCode);

				bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
				sendingBill.UpdateAction(ActionCode.AmendingAdd);
				AssertEquals(AFRSendingActionCodeList.Codes.Update, sendingBill.JPM_ActionCode);

				sendingBill.UpdateAction(ActionCode.AmendingDelete);
				AssertEquals(AFRSendingActionCodeList.Codes.Delete, sendingBill.JPM_ActionCode);

				sendingBill.JPM_ActionCode = AFRSendingActionCodeList.Codes.Update;
				sendingBill.UpdateAction(ActionCode.CorrectMasterInformation);
				AssertEquals(AFRSendingActionCodeList.Codes.Update, sendingBill.JPM_ActionCode);

				sendingBill.JPM_ActionCode = AFRSendingActionCodeList.Codes.Add;
				sendingBill.UpdateAction(ActionCode.CorrectMasterInformation);
				AssertEquals(AFRSendingActionCodeList.Codes.Update, sendingBill.JPM_ActionCode);

				sendingBill.JPM_ActionCode = AFRSendingActionCodeList.Codes.Delete;
				sendingBill.UpdateAction(ActionCode.CorrectMasterInformation);
				AssertEquals(AFRSendingActionCodeList.Codes.Delete, sendingBill.JPM_ActionCode);

				sendingBill.UpdateAction(ActionCode.CorrectVesselInformationByAmendment);
				AssertEquals(AFRSendingActionCodeList.Codes.Add, sendingBill.JPM_ActionCode);

				sendingBill.UpdateAction(ActionCode.CorrectVesselInformationByRegistration);
				AssertEquals(AFRSendingActionCodeList.Codes.Register, sendingBill.JPM_ActionCode);

				sendingBill.UpdateAction(ActionCode.ReRegisterMasterAfterATD);
				AssertEquals(AFRSendingActionCodeList.Codes.Register, sendingBill.JPM_ActionCode);

				sendingBill.UpdateAction(ActionCode.ReRegisterMasterBeforeATD);
				AssertEquals(AFRSendingActionCodeList.Codes.Register, sendingBill.JPM_ActionCode);

				sendingBill.UpdateAction(ActionCode.ChangeDepartureTimeAfterATD);
				AssertEquals(AFRSendingActionCodeList.Codes.Update, sendingBill.JPM_ActionCode);
			}
		}

		public void TestJPM_SendReadOnly()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				var sendingObject = new MessageSendingObject(bill, ActionCode.Registering, TestSendingAction);
				AssertEquals("for Registering should be false", false, sendingObject.JPM_SendInfo.ReadOnly);
				sendingObject = new MessageSendingObject(bill, ActionCode.AmendingAdd, TestSendingAction);
				AssertEquals("for AmendingAdd should be false", false, sendingObject.JPM_SendInfo.ReadOnly);
				sendingObject = new MessageSendingObject(bill, ActionCode.CorrectMasterInformation, TestSendingAction);
				AssertEquals("for CorrectMasterInformation should be true", true, sendingObject.JPM_SendInfo.ReadOnly);
				sendingObject = new MessageSendingObject(bill, ActionCode.CorrectVesselInformationByRegistration, TestSendingAction);
				AssertEquals("for CorrectVesselInformationByRegistration should be true", true, sendingObject.JPM_SendInfo.ReadOnly);
				sendingObject = new MessageSendingObject(bill, ActionCode.CorrectVesselInformationByAmendment, TestSendingAction);
				AssertEquals("for CorrectVesselInformationByAmendment should be true", true, sendingObject.JPM_SendInfo.ReadOnly);
				sendingObject = new MessageSendingObject(bill, ActionCode.ReRegisterMasterAfterATD, TestSendingAction);
				AssertEquals("for ReRegisterMasterAfterATD should be true", true, sendingObject.JPM_SendInfo.ReadOnly);
				sendingObject = new MessageSendingObject(bill, ActionCode.ReRegisterMasterBeforeATD, TestSendingAction);
				AssertEquals("for ReRegisterMasterBeforeATD should be true", true, sendingObject.JPM_SendInfo.ReadOnly);
				sendingObject = new MessageSendingObject(bill, ActionCode.ChangeDepartureTimeAfterATD, TestSendingAction);
				AssertEquals("for ChangeDepartureTimeAfterATD should be false", false, sendingObject.JPM_SendInfo.ReadOnly);
			});
		}

		public void TestJPM_ActionCodeReadOnly()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				var sendingObject = new MessageSendingObject(bill, ActionCode.Registering, TestSendingAction);
				AssertEquals("for Registering should be true", true, sendingObject.JPM_ActionCodeInfo.ReadOnly);
				sendingObject = new MessageSendingObject(bill, ActionCode.AmendingAdd, TestSendingAction);
				AssertEquals("for AmendingAdd should be false", false, sendingObject.JPM_ActionCodeInfo.ReadOnly);
				sendingObject = new MessageSendingObject(bill, ActionCode.AmendingUpdate, TestSendingAction);
				AssertEquals("for AmendingUpdate should be false", false, sendingObject.JPM_ActionCodeInfo.ReadOnly);
				sendingObject = new MessageSendingObject(bill, ActionCode.AmendingDelete, TestSendingAction);
				AssertEquals("for AmendingDelete should be false", false, sendingObject.JPM_ActionCodeInfo.ReadOnly);
				sendingObject = new MessageSendingObject(bill, ActionCode.CorrectMasterInformation, TestSendingAction);
				AssertEquals("for CorrectMasterInformation should be false", false, sendingObject.JPM_ActionCodeInfo.ReadOnly);
				sendingObject = new MessageSendingObject(bill, ActionCode.CorrectVesselInformationByRegistration, TestSendingAction);
				AssertEquals("for CorrectVesselInformationByRegistration should be true", true, sendingObject.JPM_ActionCodeInfo.ReadOnly);
				sendingObject = new MessageSendingObject(bill, ActionCode.CorrectVesselInformationByAmendment, TestSendingAction);
				AssertEquals("for CorrectVesselInformationByAmendment should be true", true, sendingObject.JPM_ActionCodeInfo.ReadOnly);
				sendingObject = new MessageSendingObject(bill, ActionCode.RegisterCompletionByAmendment, TestSendingAction);
				AssertEquals("for RegisterCompletionByAmendment should be true", true, sendingObject.JPM_ActionCodeInfo.ReadOnly);
				sendingObject = new MessageSendingObject(bill, ActionCode.RegisterCompletionByRegistration, TestSendingAction);
				AssertEquals("for RegisterCompletionByRegistration should be true", true, sendingObject.JPM_ActionCodeInfo.ReadOnly);
				sendingObject = new MessageSendingObject(bill, ActionCode.ReRegisterMasterAfterATD, TestSendingAction);
				AssertEquals("for ReRegisterMasterAfterATD should be false", false, sendingObject.JPM_ActionCodeInfo.ReadOnly);
				sendingObject = new MessageSendingObject(bill, ActionCode.ReRegisterMasterBeforeATD, TestSendingAction);
				AssertEquals("for ReRegisterMasterBeforeATD should be false", false, sendingObject.JPM_ActionCodeInfo.ReadOnly);
				sendingObject = new MessageSendingObject(bill, ActionCode.ChangeDepartureTimeAfterATD, TestSendingAction);
				AssertEquals("for ChangeDepartureTimeAfterATD should be false", false, sendingObject.JPM_ActionCodeInfo.ReadOnly);
				sendingObject = new MessageSendingObject(bill, ActionCode.RegisterDepartureTime, TestSendingAction);
				AssertEquals("for RegisterDepartureTime should be true", true, sendingObject.JPM_ActionCodeInfo.ReadOnly);
			});
		}

		public void TestActionCodeList()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			var sendingObject = new MessageSendingObject(bill, ActionCode.Registering, TestSendingAction);
			AssertEquals(AFRSendingActionCodeList.GetActionCodeList(Factory, ActionCode.Registering), sendingObject.ActionCodeList);
			AssertEquals(4, sendingObject.ActionCodeList.Count);

			sendingObject = new MessageSendingObject(bill, ActionCode.AmendingAdd, TestSendingAction);
			AssertEquals(AFRSendingActionCodeList.GetActionCodeList(Factory, ActionCode.AmendingAdd), sendingObject.ActionCodeList);
			AssertEquals(3, sendingObject.ActionCodeList.Count);

			sendingObject = new MessageSendingObject(bill, ActionCode.AmendingUpdate, TestSendingAction);
			AssertEquals(AFRSendingActionCodeList.GetActionCodeList(Factory, ActionCode.AmendingAdd), sendingObject.ActionCodeList);
			AssertEquals(3, sendingObject.ActionCodeList.Count);

			sendingObject = new MessageSendingObject(bill, ActionCode.CorrectVesselInformationByAmendment, TestSendingAction);
			AssertEquals(AFRSendingActionCodeList.GetActionCodeList(Factory, ActionCode.CorrectVesselInformationByAmendment), sendingObject.ActionCodeList);
			AssertEquals(1, sendingObject.ActionCodeList.Count);

			sendingObject = new MessageSendingObject(bill, ActionCode.CorrectVesselInformationByRegistration, TestSendingAction);
			AssertEquals(AFRSendingActionCodeList.GetActionCodeList(Factory, ActionCode.CorrectVesselInformationByRegistration), sendingObject.ActionCodeList);
			AssertEquals(1, sendingObject.ActionCodeList.Count);

			sendingObject = new MessageSendingObject(bill, ActionCode.CorrectMasterInformation, TestSendingAction);
			AssertEquals(AFRSendingActionCodeList.GetActionCodeList(Factory, ActionCode.CorrectMasterInformation), sendingObject.ActionCodeList);
			AssertEquals(2, sendingObject.ActionCodeList.Count);

			sendingObject = new MessageSendingObject(bill, ActionCode.ReRegisterMasterAfterATD, TestSendingAction);
			AssertEquals(AFRSendingActionCodeList.GetActionCodeList(Factory, ActionCode.ReRegisterMasterAfterATD), sendingObject.ActionCodeList);
			AssertEquals(2, sendingObject.ActionCodeList.Count);

			sendingObject = new MessageSendingObject(bill, ActionCode.ReRegisterMasterBeforeATD, TestSendingAction);
			AssertEquals(AFRSendingActionCodeList.GetActionCodeList(Factory, ActionCode.ReRegisterMasterBeforeATD), sendingObject.ActionCodeList);
			AssertEquals(2, sendingObject.ActionCodeList.Count);

			sendingObject = new MessageSendingObject(bill, ActionCode.ChangeDepartureTimeAfterATD, TestSendingAction);
			AssertEquals(AFRSendingActionCodeList.GetActionCodeList(Factory, ActionCode.ChangeDepartureTimeAfterATD), sendingObject.ActionCodeList);
			AssertEquals(2, sendingObject.ActionCodeList.Count);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			return new MessageSendingObject(bill, ActionCode.Registering, TestSendingAction);
		}

		MessageSendingAction TestSendingAction
		{
			get { return testSendingAction ?? (testSendingAction = new MessageSendingAction(Factory.New<JPAFRHeader>(), ActionCode.AmendingAdd)); }
		}
		MessageSendingAction testSendingAction;

		#endregion
	}

	[TestedType(typeof(MessageSendingObject.Loader))]
	class LoaderTest : LoaderTestCase
	{
		public void TestLoadOrNew()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_CarrierCode = "OTT1";
			var bill = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var sendingObj = new MessageSendingObject(bill, ActionCode.AmendingAdd, testSendingAction);

			var loader = new MessageSendingObject.Loader(Factory, testSendingAction);

			AssertNotEquals(sendingObj, loader.LoadOrNew(bill2, ActionCode.AmendingAdd));
			AssertEquals(sendingObj, loader.LoadOrNew(bill, ActionCode.AmendingAdd));

			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			AssertNull(loader.LoadOrNew(bill, ActionCode.Registering));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new MessageSendingObject.Loader(Factory, testSendingAction);
		}

		MessageSendingAction testSendingAction
		{
			get { return new MessageSendingAction(Factory.New<JPAFRHeader>(), ActionCode.AmendingAdd); }
		}
	}
}
