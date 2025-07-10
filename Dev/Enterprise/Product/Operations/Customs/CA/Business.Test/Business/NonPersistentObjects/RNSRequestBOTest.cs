using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(RNSRequestBO))]
	sealed class RNSRequestBOTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2011, 1, 1, 1, 1, 1)]
		public void TestProperties()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "TEST1234567";
			var messaging = new RNSMessagingBO(new RNSPlugInSupportShipmentWrapper(shipment));
			CACustomsDataRegistry.Instance.DefaultRNSOffice.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0497");
			var testBo = new RNSRequestBO(messaging, RNSMessageTypes.Codes.ArrivalCertification, Factory, true, true);
			AssertEquals("0497", testBo.OfficeCode);
			AssertEquals(new ZDateTime(2011, 1, 1, 1, 1, 1), testBo.DateOfArrival);
			AssertEquals("Warehouse Arrival Certification Message", testBo.MessageDescription);
			AssertEquals("TEST1234567", testBo.HouseBillNumber);
			Assert(!testBo.DateOfArrivalInfo.ReadOnly);
			Assert(!testBo.OfficeCodeInfo.ReadOnly);
			testBo.OfficeCode = "707";
			AssertEquals("0707", testBo.OfficeCode);
			testBo = new RNSRequestBO(RNSMessageTypes.Codes.StatusQuery, Factory);
			Assert(testBo.DateOfArrivalInfo.ReadOnly);
			Assert(testBo.OfficeCodeInfo.ReadOnly);
			Assert(testBo.HouseBillNumberInfo.ReadOnly);
		}

		public void TestValidateDateOfArrival()
		{
			RNSRequestBO.ValidateDateOfArrival();
			AssertNoNotifications(RNSRequestBO.DateOfArrivalInfo);
			RNSRequestBO.DateOfArrival = ZDateTime.Now.AddDays(2);
			AssertNoNotifications(RNSRequestBO.DateOfArrivalInfo);
			RNSRequestBO.DateOfArrival = ZDateTime.Invalid;
			AssertHasNotifications(RNSRequestBO.DateOfArrivalInfo);
		}

		public void TestValidateCargoControlNumber()
		{
			RNSRequestBO.CargoControlNumber = "X";
			AssertNoErrors(RNSRequestBO.CargoControlNumberInfo);
			RNSRequestBO.CargoControlNumber = "";
			AssertHasError(RNSRequestBO.CargoControlNumberInfo, RNSRequestBO.CCNRequiredErrorText);
			RNSRequestBO.CargoControlNumber = "X";
			AssertNoErrors(RNSRequestBO.CargoControlNumberInfo);
		}
		public void TestValidateTransactionNumber()
		{
			RNSRequestBO.TransactionNumber = "Y";
			AssertNoErrors(RNSRequestBO.TransactionNumberInfo);
			RNSRequestBO.TransactionNumber = "";
			AssertHasError(RNSRequestBO.TransactionNumberInfo, RNSRequestBO.NumberRequiredErrorText);
			RNSRequestBO.TransactionNumber = "Y";
			AssertNoErrors(RNSRequestBO.TransactionNumberInfo);
		}

		public void TestValidateOfficeCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0495", "0495", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			CACustomsDataRegistry.Instance.DefaultPortOfClearance.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0497");
			var testBo = new RNSRequestBO(RNSMessageTypes.Codes.ArrivalCertification, Factory);
			testBo.OfficeCode = "";
			AssertHasMessageErrors(testBo.OfficeCodeInfo);
			testBo.OfficeCode = "495";
			AssertNoMessageErrors(testBo.OfficeCodeInfo);
			testBo.OfficeCode = "XXX";
			AssertHasMessageErrors(testBo.OfficeCodeInfo);
			testBo = new RNSRequestBO(RNSMessageTypes.Codes.StatusQuery, Factory);
			testBo.OfficeCode = "XXX";
			AssertNoMessageErrors(testBo.OfficeCodeInfo);
		}

		public void TestValidateSubLocationCode()
		{
			CACustomsDataRegistry.Instance.DefaultPortOfClearance.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0497");
			var subLocation1 = CACSubLocationTest.CreateSubLocation(Factory, "1212", port: "00X2");
			var subLocation2 = CACSubLocationTest.CreateSubLocation(Factory, "3252", port: "0497");
			Factory.Save();

			var testBo = new RNSRequestBO(RNSMessageTypes.Codes.ArrivalCertification, Factory);
			testBo.ValidateSubLocationCode();
			AssertHasMessageError(testBo.SubLocationCodeInfo, "You have not entered a value.");

			testBo.OfficeCode = "0497";
			testBo.SubLocationCode = "1212";
			AssertHasMessageErrorContaining(testBo.SubLocationCodeInfo, "The Sub-Location entered is not valid for the CBSA office entered.");

			testBo.SubLocationCode = "3252";
			AssertNoMessageErrors(testBo.SubLocationCodeInfo);

			testBo.OfficeCode = "00X2";
			AssertHasMessageErrorContaining(testBo.SubLocationCodeInfo, "The Sub-Location entered is not valid for the CBSA office entered.");

			testBo.SubLocationCode = "1111";
			AssertHasMessageErrorContaining(testBo.SubLocationCodeInfo, "The Sub-Location entered is not valid for the CBSA office entered.");
		}

		public void TestIsCargoControlNumberEditable()
		{
			var testBo = new RNSRequestBO(RNSMessageTypes.Codes.ArrivalCertification, Factory, false, true);
			Assert("IsCargoControlNumberEditable should be false", !testBo.IsCargoControlNumberEditable);
			Assert("CargoControlNumberInfo should be readonly", testBo.CargoControlNumberInfo.ReadOnly);
		}

		public void TestIsTransactionNumberApplicable()
		{
			var testBo = new RNSRequestBO(RNSMessageTypes.Codes.ArrivalCertification, Factory, true, false);

			testBo.TransactionNumber = "Y";
			AssertEquals("TransactionNumber should be empty", ZString.Empty, testBo.TransactionNumber);

			testBo.CargoControlNumber = "";
			testBo.TransactionNumber = "";
			Assert("CargoControlNumberInfo should has message error", testBo.CargoControlNumberInfo.HasNotification(RNSRequestBO.CCNRequiredErrorText));
			AssertNoNotifications("CargoControlNumberInfo should not has message error", testBo.TransactionNumberInfo);

			testBo.CargoControlNumber = "X";
			AssertNoNotifications("CargoControlNumberInfo should not has message error", testBo.CargoControlNumberInfo);
			AssertNoNotifications("TransactionNumberInfo should not has message error", testBo.TransactionNumberInfo);
		}

		public void TestTopLevelBusinessObject()
		{
			AssertSame("TopLevelBusinessObject should be this", RNSRequestBO, ((IEDIFACTMessageAttachee)RNSRequestBO).TopLevelBusinessObject);

			var shipment = Factory.New<CFSShipment>();
			var messaging = RNSMessagingBOTest.GetRNSMessagingBO(shipment);

			var testBo = new RNSRequestBO(messaging, RNSMessageTypes.Codes.ArrivalCertification, Factory, true, false);
			AssertSame("TopLevelBusinessObject should be the shipment", shipment, ((IEDIFACTMessageAttachee)testBo).TopLevelBusinessObject);
		}

		#region Implementation

		RNSRequestBO RNSRequestBO
		{
			get { return rnsRequestBO ?? (rnsRequestBO = new RNSRequestBO(RNSMessageTypes.Codes.StatusQuery, Factory)); }
		}
		RNSRequestBO rnsRequestBO;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RNSRequestBO(RNSMessageTypes.Codes.StatusQuery, Factory);
		}

		#endregion
	}
}
