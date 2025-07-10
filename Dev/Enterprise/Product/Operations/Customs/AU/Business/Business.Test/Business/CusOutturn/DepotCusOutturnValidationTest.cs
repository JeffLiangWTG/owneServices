using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DepotCusOutturnValidationTest : BaseCusOutturnValidationTest
	{
		public void TestMessageStatusValidation()
		{
			Outturn.C5_MessageStatus = CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived;

			AssertHasWarning(Outturn.C5_CustomsStatusInfo, "Outturn has been rescinded");
			AssertHasWarning(Outturn.C5_MessageStatusInfo, "Outturn has been rescinded");
		}

		public void TestValidationOnPacksIfOnlyReceptDate()
		{
			Outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			Outturn.C5_CargoReceiptDate = ZDateTime.BrettsBirthday;
			Outturn.Validation.ValidateAll();
			AssertNoNotifications(Outturn.C5_PackagesOutturnedInfo);
			AssertNoNotifications(Outturn.C5_PackagesUnitsInfo);
			AssertNoNotifications(Outturn.C5_OuterPacksInfo);
			AssertNoNotifications(Outturn.C5_OuterPackUnitsInfo);

			Outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills;
			Outturn.C5_CargoReceiptDate = ZDateTime.BrettsBirthday;
			Outturn.Validation.ValidateAll();
			AssertNoNotifications(Outturn.C5_PackagesOutturnedInfo);
			AssertNoNotifications(Outturn.C5_PackagesUnitsInfo);
			AssertNoNotifications(Outturn.C5_OuterPacksInfo);
			AssertNoNotifications(Outturn.C5_OuterPackUnitsInfo);

			Outturn.C5_CargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			Outturn.C5_CargoUnpackDate = ZDateTime.BrettsBirthday;
			Outturn.Validation.ValidateAll();
			AssertHasMessageErrors(Outturn.C5_PackagesOutturnedInfo);
			AssertHasMessageErrors(Outturn.C5_PackagesUnitsInfo);
			AssertNoNotifications(Outturn.C5_OuterPacksInfo);
			AssertNoNotifications(Outturn.C5_OuterPackUnitsInfo);
		}

		public void TestValidationOnHousebill()
		{
			Outturn.C5_CargoType = CMRImportCargoTypes.Codes.Bulk;
			Outturn.Validation.ValidateAll();
			AssertHasNotifications(Outturn.C5_HouseBillInfo);

			Outturn.C5_CargoType = CMRImportCargoTypes.Codes.BreakBulk;
			Outturn.Validation.ValidateAll();
			AssertHasNotifications(Outturn.C5_HouseBillInfo);

			Outturn.C5_CargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			Outturn.Validation.ValidateAll();
			AssertHasNotifications(Outturn.C5_HouseBillInfo);

			Outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills;
			Outturn.Validation.ValidateAll();
			AssertNoNotifications(Outturn.C5_HouseBillInfo);

			Outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			Outturn.Validation.ValidateAll();
			AssertNoNotifications(Outturn.C5_HouseBillInfo);
		}

		public void TestCheckC5_ContainerNumber()
		{
			AssertHasMessageErrors("by default when empty", Outturn.C5_ContainerNumberInfo);

			Outturn.C5_ContainerNumber = "foo";
			AssertNoNotifications("when filled", Outturn.C5_ContainerNumberInfo);

			Outturn.C5_CargoType = CMRCargoTypes.Codes.BreakBulk;
			Outturn.C5_ContainerNumber = ZString.Empty;
			AssertNoNotifications("when empty and b/b", Outturn.C5_ContainerNumberInfo);

			Outturn.C5_CargoType = Core.Constants.ContainerModes.FCL;
			AssertHasMessageErrors("when empty and fcl", Outturn.C5_ContainerNumberInfo);

			Outturn.C5_CargoType = Core.Constants.ContainerModes.Bulk;
			AssertNoNotifications("when empty and blk", Outturn.C5_ContainerNumberInfo);
		}

		public void TestCheckC5_ContainerSeal()
		{
			Outturn.C5_ContainerSeal = "1234567890";
			AssertNoMessageErrorContaining(Outturn.C5_ContainerSealInfo, "Seal number should not be mre than 10 characters.");

			Outturn.C5_ContainerSeal = "12345678901";
			AssertHasMessageErrorContaining(Outturn.C5_ContainerSealInfo, "Seal number should not be mre than 10 characters.");
		}

		public void TestCheckC5_OutturnResultType()
		{
			Outturn.C5_OutturnResultType = "";
			AssertHasMessageErrors("by when blank", Outturn.C5_OutturnResultTypeInfo);

			Outturn.C5_OutturnResultType = "SC";
			AssertNoNotifications("when set to valid value", Outturn.C5_OutturnResultTypeInfo);

			Outturn.C5_OutturnResultType = "FO";
			AssertHasMessageErrors("when set to invalid value", Outturn.C5_OutturnResultTypeInfo);
		}

		public void TestCheckC5_HouseBill()
		{
			AssertNoNotifications("by default when empty", Outturn.C5_HouseBillInfo);

			Outturn.C5_CargoType = Core.Constants.ContainerModes.LCL;
			AssertHasMessageErrors("when lcl and empty", Outturn.C5_HouseBillInfo);

			Outturn.C5_HouseBill = "foo";
			AssertNoNotifications("when lcl and set", Outturn.C5_HouseBillInfo);

			Outturn.C5_HouseBill = ZString.Empty;

			Outturn.C5_CargoType = Core.Constants.ContainerModes.FCL;
			AssertNoNotifications("when fcl and empty", Outturn.C5_HouseBillInfo);

			Outturn.C5_HouseBill = "foo";
			AssertHasMessageErrors("when fcl and not empty", Outturn.C5_HouseBillInfo);

			Outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills;
			AssertHasMessageErrors("when fcx and not empty", Outturn.C5_HouseBillInfo);
		}

		public void TestCheckC5_MasterBill()
		{
			AssertHasMessageErrors("by default when empty", Outturn.C5_MasterBillInfo);

			Outturn.C5_CargoType = Core.Constants.ContainerModes.FCL;
			AssertNoNotifications("when fcl and empty", Outturn.C5_MasterBillInfo);

			Outturn.C5_CargoType = "FCX";
			AssertNoNotifications("when fcx and empty", Outturn.C5_MasterBillInfo);

			Outturn.C5_CargoType = Core.Constants.ContainerModes.LCL;
			AssertHasMessageErrors("when lcl and empty", Outturn.C5_MasterBillInfo);

			Outturn.C5_MasterBill = "foo";
			AssertNoNotifications("when lcl and set", Outturn.C5_MasterBillInfo);

			Outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			AssertHasMessageErrors("when fcl and set", Outturn.C5_MasterBillInfo);

			Outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills;
			AssertHasMessageErrors("when fcx and set", Outturn.C5_MasterBillInfo);
		}

		public void TestCheckC5_PackagesOutturned()
		{
			List<string> cargoTypeList = new List<string>()
			{
				Core.Constants.ContainerModes.FCL,
				Core.Constants.ContainerModes.FCLMixedShipper,
				Core.Constants.ContainerModes.Bulk,
				Core.Constants.ContainerModes.LCL,
				Core.Constants.ContainerModes.BreakBulk,
			};

			Outturn.C5_CargoUnpackDate = ZDateTime.Now;
			foreach (var cargoType in cargoTypeList)
			{
				Outturn.C5_CargoType = cargoType;
				Outturn.Validation.ValidateC5_PackagesOutturned();
				Outturn.Validation.ValidateC5_PackagesUnits();
				AssertHasMessageErrors(string.Format("when {0} line and unpacked have error", cargoType), Outturn.C5_PackagesOutturnedInfo);
				AssertHasMessageErrors(string.Format("when {0} line and unpacked have error", cargoType), Outturn.C5_PackagesUnitsInfo);
				Outturn.Notes.ClearAllNotifications();
			}

			Outturn.C5_CargoUnpackDate = ZDateTime.Empty;
			foreach (var cargoType in cargoTypeList)
			{
				Outturn.C5_CargoType = cargoType;
				Outturn.Validation.ValidateC5_PackagesOutturned();
				Outturn.Validation.ValidateC5_PackagesUnits();
				AssertNoMessageErrors(string.Format("when {0} line and don't unpacked no error", cargoType), Outturn.C5_PackagesOutturnedInfo);
				AssertNoMessageErrors(string.Format("when {0} line and don't unpacked no error", cargoType), Outturn.C5_PackagesUnitsInfo);
				Outturn.Notes.ClearAllNotifications();
			}
		}

		public void TestCheckC5_PackagesOutturnedCanBeZeroWhenShortLanded()
		{
			Outturn.C5_CargoType = Core.Constants.ContainerModes.FCL;
			Outturn.C5_CargoUnpackDate = ZDateTime.BrettsBirthday;
			Outturn.C5_PackagesOutturned = 0;

			AssertHasMessageErrors("precondition", Outturn.C5_PackagesOutturnedInfo);

			Outturn.C5_OutturnResultType = CMROutturnResultType.Codes.ShortLanded;
			AssertNoNotifications("when shortlanded, outturnresulttype shouldn't have any message errors", Outturn.C5_PackagesOutturnedInfo);
		}

		public void TestCheckC5_CargoType()
		{
			AssertHasMessageErrors("by default when empty", Outturn.C5_CargoTypeInfo);

			Outturn.C5_CargoType = Core.Constants.ContainerModes.FCL;
			AssertNoNotifications("when set", Outturn.C5_CargoTypeInfo);

			Outturn.C5_CargoType = "FOO";
			AssertHasMessageErrors("when invalid", Outturn.C5_CargoTypeInfo);
		}

		public void TestCheckC5_PackagesUnits()
		{
			Outturn.C5_CargoUnpackDate = ZDateTime.Now;
			Outturn.Validation.ValidateC5_PackagesUnits();
			AssertHasMessageErrors("by default when empty and unpack date set", Outturn.C5_PackagesUnitsInfo);

			Outturn.C5_CargoType = Core.Constants.ContainerModes.Bulk;
			AssertHasMessageErrors("when blk and empty", Outturn.C5_PackagesUnitsInfo);

			Outturn.C5_CargoType = Core.Constants.ContainerModes.FCL;
			Outturn.C5_PackagesUnits = "~~";
			AssertHasMessageErrors("when fcl and invalid", Outturn.C5_PackagesUnitsInfo);

			Outturn.C5_PackagesUnits = "XB";
			AssertNoNotifications("when fcl and set", Outturn.C5_PackagesUnitsInfo);
		}

		public void TestCheckC5_GoodsDescription()
		{
			AssertNoNotifications("by default when empty", Outturn.C5_GoodsDescriptionInfo);

			Outturn.C5_OutturnResultType = "SH";
			AssertNoNotifications("SH - Short Landed should not error", Outturn.C5_GoodsDescriptionInfo);

			Outturn.C5_OutturnResultType = "SC";
			AssertHasMessageErrors("when sc and empty", Outturn.C5_GoodsDescriptionInfo);

			Outturn.C5_OutturnResultType = "SU";
			AssertHasMessageErrors("when su and empty", Outturn.C5_GoodsDescriptionInfo);

			Outturn.C5_GoodsDescription = "foo";
			AssertNoNotifications("when su and set", Outturn.C5_GoodsDescriptionInfo);
		}

		public void TestCheckC5_MarksAndNumbers()
		{
			AssertNoNotifications("by default when empty", Outturn.C5_MarksAndNumbersInfo);

			Outturn.C5_CargoType = Core.Constants.ContainerModes.LCL;
			Outturn.C5_OutturnResultType = "SC";
			AssertHasMessageErrors("when lcl and sc", Outturn.C5_MarksAndNumbersInfo);

			Outturn.C5_CargoType = Core.Constants.ContainerModes.LCL;
			Outturn.C5_OutturnResultType = "SU";
			AssertHasMessageErrors("when lcl and su", Outturn.C5_MarksAndNumbersInfo);

			Outturn.C5_CargoType = CMRCargoTypes.Codes.BreakBulk;
			Outturn.C5_OutturnResultType = "SC";
			AssertHasMessageErrors("when b/b and sc", Outturn.C5_MarksAndNumbersInfo);

			Outturn.C5_CargoType = CMRCargoTypes.Codes.BreakBulk;
			Outturn.C5_OutturnResultType = "SU";
			AssertHasMessageErrors("when b/b and su", Outturn.C5_MarksAndNumbersInfo);

			Outturn.C5_MarksAndNumbers = "foo";
			AssertNoNotifications("when b/b and su and set", Outturn.C5_MarksAndNumbersInfo);

			Outturn.C5_MarksAndNumbers = ZString.Empty;
			Outturn.C5_CargoType = Core.Constants.ContainerModes.FCL;
			AssertNoNotifications("when fcl and su", Outturn.C5_MarksAndNumbersInfo);

			Outturn.C5_CargoType = Core.Constants.ContainerModes.LCL;
			Outturn.C5_OutturnResultType = "NIL";
			AssertNoNotifications("when lcl and nil", Outturn.C5_MarksAndNumbersInfo);

			Outturn.C5_CargoType = CMRCargoTypes.Codes.BreakBulk;
			Outturn.C5_OutturnResultType = "FOO";
			AssertHasMessageErrors("when b/b and anything", Outturn.C5_MarksAndNumbersInfo);
		}

		public void TestCheckC5_CargoUnpackDate()
		{
			Outturn.C5_ContainerNumber = "OCLU8911239";

			AssertNoNotifications("by default when empty", Outturn.C5_CargoUnpackDateInfo);

			Outturn.C5_ReceiptOnlyIndicator = true;
			Outturn.Validation.ValidateAll();
			AssertNoNotifications("when receipt only and empty", Outturn.C5_CargoUnpackDateInfo);

			Outturn.C5_CargoUnpackDate = ZDateTime.Now;
			AssertHasMessageErrors("when receipt only and entered", Outturn.C5_CargoUnpackDateInfo);

			Outturn.C5_ReceiptOnlyIndicator = false;
			Outturn.Validation.ValidateAll();
			AssertNoNotifications("when not receipt only and entered", Outturn.C5_CargoUnpackDateInfo);

			Outturn.C5_CargoReceiptDate = ZDateTime.Now.AddHours(-5);
			Outturn.C5_CargoUnpackDate = ZDateTime.Now.AddHours(-7);
			AssertHasMessageError(Outturn.C5_CargoUnpackDateInfo, "Unpack date must be after receipt date.");
			Outturn.C5_CargoReceiptDate = Outturn.C5_CargoUnpackDate.AddHours(-2);
			AssertNoMessageError(Outturn.C5_CargoUnpackDateInfo, "Unpack date must be after receipt date.");

			AssertNoMessageError(Outturn.C5_CargoUnpackDateInfo, "Bills with Freight Forwarder Indicator set are not normally outturned.");

			CMRSEIMessage seiMessage = (CMRSEIMessage)Header.Messages.AddNew(typeof(CMRSEIMessage));
			seiMessage.EM_MessageText = CMRSEIMessageTest.SampleSEIMessage;
			seiMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			seiMessage.EM_MessageType = CMRMessage.CMRMessageTypes.SEI;
			Outturn.ResetCachedValuesForTesting();
			Header.ResetCachedValuesForTesting();

			Outturn.C5_CargoUnpackDate = ZDateTime.Now;
			AssertHasMessageError(Outturn.C5_CargoUnpackDateInfo, "Bills with Freight Forwarder Indicator set are not normally outturned.");
		}

		protected override CusOutturn GetNewOutturn()
		{
			var outturn = Header.Outturns.AddNew();
			outturn.Validation.ValidateAll();
			return outturn;
		}

		new DepotCusOutturn Outturn => base.Outturn as DepotCusOutturn;
	}
}
