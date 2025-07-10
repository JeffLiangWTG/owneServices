using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	sealed class TemporaryStorageAdditionalInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestMaxCount_TemporaryStorageBill()
		{
			var bill = Factory.New<TemporaryStorageBill>();
			for (var i = 0; i < 99; i++)
			{
				bill.AdditionalInfos.AddNew();
			}

			var addInfo = bill.AdditionalInfos.First();
			addInfo.Validation.ValidateAll();
			AssertNoRowError(addInfo, "You are only allowed a maximum of 99 Additional Informations here.");

			addInfo = bill.AdditionalInfos.AddNew();
			addInfo.Validation.ValidateAll();
			AssertHasRowError(addInfo, "You are only allowed a maximum of 99 Additional Informations here.");
		}

		public void TestMaxCount_TemporaryStoragePackedItem()
		{
			var bill = Factory.New<TemporaryStorageBill>();
			var packedItem = bill.PackedItems.AddNew();

			for (var i = 0; i < 99; i++)
			{
				var tempAddInfo = packedItem.AdditionalInfos.AddNew();
				tempAddInfo.CSI_SubType = "INF";
			}

			var addInfo = packedItem.AdditionalInfos.First();
			addInfo.Validation.ValidateAll();
			AssertNoRowError(addInfo, "You are only allowed a maximum of 99 INF Additional Informations here.");

			var refAddInfo = packedItem.AdditionalInfos.AddNew();
			refAddInfo.CSI_SubType = "REF";
			addInfo.Validation.ValidateAll();
			AssertNoRowError(addInfo, "You are only allowed a maximum of 99 INF Additional Informations here.");

			refAddInfo.CSI_SubType = "INF";
			addInfo.Validation.ValidateAll();
			AssertHasRowError(addInfo, "You are only allowed a maximum of 99 INF Additional Informations here.");
		}

		public void TestCSI_CodeInfoNoMessageError()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia);
			helper.CreateCusCodeType("AI44T", "AI44T", Core.Constants.CountryCodes.Latvia);
			helper.CreateCusCodeType("AR44T", "AR44T", Core.Constants.CountryCodes.Latvia);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, "AI44T", "code1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, "AR44T", "code2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var addInfo = bill.AdditionalInfos.AddNew();
			addInfo.Validation.ValidateCSI_Code();
			AssertNoMessageErrors("No message error should be added to CSI_Code if parent is bill", addInfo.CSI_CodeInfo);

			var packedItem = bill.PackedItems.AddNew();
			var addInfo2 = packedItem.AdditionalInfos.AddNew();
			addInfo2.Validation.ValidateCSI_Code();
			AssertNoMessageErrors("No message error should be added to CSI_Code if parent is packedItem but CSI_SubType is not INF", addInfo2.CSI_CodeInfo);

			addInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			(addInfo2.Lookups.CodeList as ZZRefCusCodeListCombinedCollection).Load();
			addInfo2.Validation.ValidateCSI_Code();
			AssertHasMessageError("Message error should be added to CSI_Code if parent is packedItem and CSI_SubType is INF", addInfo2.CSI_CodeInfo, "You have not entered a Type.");

			addInfo2.CSI_Code = "1";
			AssertNoMessageError("Mandatory message error should not be added to CSI_Code if it is not empty even if parent is packedItem and CSI_SubType is INF", addInfo2.CSI_CodeInfo, "You have not entered a Type.");
			AssertHasMessageError("ListValidation message error should be added to CSI_Code if value is not in the list", addInfo2.CSI_CodeInfo, "The code you have selected is not in the list.");

			addInfo2.CSI_Code = "code2";
			AssertNoMessageError("ListValidation message error should not be added to CSI_Code if value is in the list", addInfo2.CSI_CodeInfo, "The code you have selected is not in the list.");
		}

		public void TestCSI_ReferenceNumber()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BolType = TemporaryStorageBillKindList.Codes.HWB;
			var addInfo = bill.AdditionalInfos.AddNew();
			addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			addInfo.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError(addInfo.CSI_ReferenceNumberInfo, "You have not entered a Reference.");

			var addReference = bill.AdditionalInfos.AddNew();
			addReference.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			addReference.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageError(addReference.CSI_ReferenceNumberInfo, "You have not entered a Reference.");

			addReference.CSI_ReferenceNumber = "Reference";
			AssertNoMessageErrors("No message error should be added to CSI_ReferenceNumber", addReference.CSI_ReferenceNumberInfo);
		}

		public void TestCSI_Description()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var addInfo = packedItem.AdditionalInfos.AddNew();
			addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			addInfo.CSI_Code = "10600";
			addInfo.Validation.ValidateCSI_Description();
			AssertHasMessageError(addInfo.CSI_DescriptionInfo, "You have not entered a Description.");

			addInfo.CSI_Description = "1";
			AssertNoMessageError(addInfo.CSI_DescriptionInfo, "You have not entered a Description.");

			addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			addInfo.Validation.ValidateCSI_Description();
			AssertNoMessageError(addInfo.CSI_DescriptionInfo, "You have not entered a Description.");

			addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			addInfo.CSI_Code = "10601";
			addInfo.Validation.ValidateCSI_Description();
			AssertNoMessageError(addInfo.CSI_DescriptionInfo, "You have not entered a Description.");
		}

		public void TestRuleBR_PN_TS_050_TemporaryStorageBill()
		{
			var bill = Factory.New<TemporaryStorageBill>();
			var packedItem = bill.PackedItems.AddNew();

			bill.AdditionalInfos.AddNew();

			var addInfo = packedItem.AdditionalInfos.AddNew();
			addInfo.Validation.ValidateAll();
			AssertHasRowMessageError("It is prohibited to add additional info when bill already has at least one", addInfo, "The Additional information should be entered at Bill level or Bill Item level, not both.");

			bill.AdditionalInfos.RemoveAndDeleteAll();
			addInfo.Validation.ValidateAll();
			AssertNoRowMessageError("It is allowed to add additional info when bill has no one", addInfo, "The Additional information should be entered at Bill level or Bill Item level, not both.");
		}

		public void TestRuleBR_PN_TS_050_TemporaryStoragePackedItem()
		{
			var bill = Factory.New<TemporaryStorageBill>();
			var packedItem = bill.PackedItems.AddNew();

			packedItem.AdditionalInfos.AddNew();

			var addInfo = bill.AdditionalInfos.AddNew();
			addInfo.Validation.ValidateAll();
			AssertHasRowMessageError("It is prohibited to add additional info when packed item already has at least one", addInfo, "The Additional information should be entered at Bill level or Bill Item level, not both.");

			packedItem.AdditionalInfos.RemoveAndDeleteAll();
			addInfo.Validation.ValidateAll();
			AssertNoRowMessageError("It is allowed to add additional info when packed item has no one", addInfo, "The Additional information should be entered at Bill level or Bill Item level, not both.");
		}

		public void TestCSI_SubTypeMasterBill()
		{
			var bill = Factory.New<TemporaryStorageBill>();
			bill.ABL_BolType = TemporaryStorageBill.ChildBolCode;
			var addInfo = bill.AdditionalInfos.AddNew();
			addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;

			AssertHasRowMessageError("Only INF allowed for master bill additional info entries", addInfo, "Only Additional Information supporting document allowed for Master Bills");
		}
	}
}
