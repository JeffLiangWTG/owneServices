using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(GuaranteeAccessCodesSendingObject))]
	sealed class GuaranteeAccessCodesSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new GuaranteeAccessCodesSendingObject(null));
		}

		public void TestHeader() => AssertType<CusGuaranteeHeader>(GetNewGuaranteeAccessCodesSendingObject().CusGuaranteeHeader);

		public void TestLookups()
		{
			var sendingObj = GetNewGuaranteeAccessCodesSendingObject();
			AssertType<GuaranteeAccessCodesSendingObjectLookups>("Should have created a correct Lookups.", sendingObj.Lookups);
		}

		public void TestGRN()
		{
			var sendingObj = GetNewGuaranteeAccessCodesSendingObject();
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingObj.GuaranteeReferenceNumberInfo);
			AssertEquals("GRN caption", "GRN", resData.Caption);
			Assert("GRN Readonly", sendingObj.GuaranteeReferenceNumberInfo.ReadOnly);
			AssertEquals("GRN Setter", "123", sendingObj.GuaranteeReferenceNumber);
		}

		public void TestOfficeOfGuarantee()
		{
			var info = GetNewGuaranteeAccessCodesSendingObject().OfficeOfGuaranteeInfo;
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(info);
			AssertEquals("OfficeOfGuarantee caption", "Office of Guarantee", resData.Caption);
			AssertEquals("OfficeOfGuarantee MaxLength", 8, info.MaxLength);
		}

		public void TestCurrentCode()
		{
			var info = GetNewGuaranteeAccessCodesSendingObject().CurrentCodeInfo;
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(info);
			AssertEquals("CurrentCode caption", "Current Code", resData.Caption);
			AssertEquals("CurrentCode MaxLength", 4, info.MaxLength);
		}

		public void TestNewAccessCode()
		{
			var info = GetNewGuaranteeAccessCodesSendingObject().NewAccessCodeInfo;
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(info);
			AssertEquals("NewAccessCode caption", "New Access Code", resData.Caption);
			AssertEquals("NewAccessCode MaxLength", 4, info.MaxLength);
		}

		public void TestMasterCode()
		{
			var info = GetNewGuaranteeAccessCodesSendingObject().MasterCodeInfo;
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(info);
			AssertEquals("MasterCode caption", "Master Code", resData.Caption);
			AssertEquals("MasterCode MaxLength", 4, info.MaxLength);
		}

		public void TestValidationType()
		{
			var sendingObj = GetNewGuaranteeAccessCodesSendingObject();
			AssertType("Should have created a correct Validation.", typeof(GuaranteeAccessCodesSendingObjectValidation), sendingObj.Validation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			cusGuaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			cusGuaranteeHeader.CPH_Number = "123";
			return new GuaranteeAccessCodesSendingObject(cusGuaranteeHeader);
		}
		CusGuaranteeHeader cusGuaranteeHeader;

		GuaranteeAccessCodesSendingObject GetNewGuaranteeAccessCodesSendingObject() => (GuaranteeAccessCodesSendingObject)GetNewBusinessObject();
	}
}
