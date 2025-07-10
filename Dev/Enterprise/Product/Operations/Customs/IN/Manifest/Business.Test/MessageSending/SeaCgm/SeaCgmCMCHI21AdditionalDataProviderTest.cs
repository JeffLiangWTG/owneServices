using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.MessageSending.SeaCgm.Testing;

[TestedType(typeof(SeaCgmCMCHI21AdditionalDataProvider))]
sealed class SeaCgmCMCHI21AdditionalDataProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new SeaCgmCMCHI21AdditionalDataProvider(null));
			AssertNoExceptionThrown(() => new SeaCgmCMCHI21AdditionalDataProvider(MessageSendingObject));
		});
	}

	public void TestGetConsigneeAddress3()
	{
		CombineAssertions(() =>
		{
			AssertNullOrEmpty("Before entering address", AdditionalDataProvider.GetConsigneeAddress3(Bill));

			Bill.ABL_ConsigneeCity = "Banglore";
			Bill.ABL_RN_NKConsigneeCountry = "IN";
			Bill.ABL_ConsigneeState = "KA";
			Bill.ABL_ConsigneePostcode = "560037";

			AssertEquals("After address field manual entry", "Banglore IN KA 560037", AdditionalDataProvider.GetConsigneeAddress3(Bill));

			Bill.ABL_OA_Consignee = GetSampleOrgAddress().PK;
			AssertEquals("After Consignee selection", "Bengaluru IN KA 560036", AdditionalDataProvider.GetConsigneeAddress3(Bill));
		});
	}

	public void TestGetImcoCode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("When UNDG missing", "ZZZ", AdditionalDataProvider.GetImcoCode(Bill));

			var dgSubstance = Factory.New<UNDGSubstance>();
			dgSubstance.DG_Class = "XYZ";
			var undg = Bill.Packs.AddNew().UNDGs.AddNew();
			undg.DI_DG = dgSubstance.PK;
			AssertEquals("After UNDG added", "XYZ", AdditionalDataProvider.GetImcoCode(Bill));
		});
	}

	public void TestGetImporterAddress3()
	{
		CombineAssertions(() =>
		{
			AssertNullOrEmpty("Before entering address", AdditionalDataProvider.GetImporterAddress3(Bill));

			Bill.ABL_BuyerCity = "Banglore";
			Bill.ABL_RN_NKBuyerCountry = "IN";
			Bill.ABL_BuyerState = "KA";
			Bill.ABL_BuyerPostcode = "560037";

			AssertEquals("After address field manual entry", "Banglore IN KA 560037", AdditionalDataProvider.GetImporterAddress3(Bill));

			Bill.ABL_OA_Buyer = GetSampleOrgAddress().PK;
			AssertEquals("After Buyer selection", "Bengaluru IN KA 560036", AdditionalDataProvider.GetImporterAddress3(Bill));
		});
	}

	public void TestGetMessageType()
	{
		CombineAssertions(() =>
		{
			MessageSendingObject.MessageType = "D";
			AssertEquals("For Bill", "D", AdditionalDataProvider.GetMessageType(Bill));

			var pack = Bill.Packs.AddNew();
			AssertEquals("For Pack", "D", AdditionalDataProvider.GetMessageType(pack));
		});
	}

	public void TestGetUnoCode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("When UNDG missing", "ZZZZZ", AdditionalDataProvider.GetUnoCode(Bill));

			var dgSubstance = Factory.New<UNDGSubstance>();
			dgSubstance.DG_UNNO = "XYZ";
			var undg = bill.Packs.AddNew().UNDGs.AddNew();
			undg.DI_DG = dgSubstance.PK;
			AssertEquals("After UNDG added", "XYZ", AdditionalDataProvider.GetUnoCode(Bill));
		});
	}

	OrgAddress GetSampleOrgAddress()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "Brigade";
		var orgAddress = orgHeader.MainAddress;
		orgAddress.OA_Address1 = "Kundalahalli";
		orgAddress.OA_Address2 = "Brookefield";
		orgAddress.OA_City = "Bengaluru";
		orgAddress.OA_RN_NKCountryCode = "IN";
		orgAddress.OA_State = "KA";
		orgAddress.OA_PostCode = "560036";

		return orgAddress;
	}

	ManifestMessageSendingObject MessageSendingObject => messageSendingObject ??= new ManifestMessageSendingObject(Header);
	ManifestMessageSendingObject messageSendingObject;

	CGMAsycudaBill Bill => bill ??= Header.Bills.AddNew();
	CGMAsycudaBill bill;

	CGMAsycudaManifestHeader Header => header ??= Factory.New<CGMAsycudaManifestHeader>();
	CGMAsycudaManifestHeader header;

	ISeaCgmCMCHI21AdditionalDataProvider AdditionalDataProvider => additionalDataProvider ??= new SeaCgmCMCHI21AdditionalDataProvider(MessageSendingObject);
	ISeaCgmCMCHI21AdditionalDataProvider additionalDataProvider;
}
