using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

[TestedType(typeof(TemporaryStorageBill))]
sealed class TemporaryStorageBillTest : TemporaryStorageBillAbstractTest<TemporaryStorageBill, TemporaryStorageHeader>
{
	public void TestValidation()
	{
		AssertType<TemporaryStorageBillValidation>(bill.Validation);
	}

	public void TestPreviousDocuments()
	{
		AssertType<EU.Business.CusTempStorage.TemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>>(GetNewBill().PreviousDocuments);
	}

	public void TestAdditionalInfos()
	{
		AssertType<EU.Business.CusTempStorage.TemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo>>(GetNewBill().AdditionalInfos);
	}

	public void TestPackedItems()
	{
		AssertType<TemporaryStoragePackedItemCollection>(GetNewBill().PackedItems);
	}

	public void TestGetCusSupportingInfoTypes()
	{
		var supportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)GetNewBill()).GetCusSupportingInfoTypes();

		CombineAssertions(() =>
		{
			AssertEquals("SupportingInfoTypes Count", 3, supportingInfoTypes.Count);
			AssertEquals("Contains AdditionalInfo?", expected: true, supportingInfoTypes.ContainsKey(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo));
			AssertEquals("Contains SupportingDocument?", expected: true, supportingInfoTypes.ContainsKey(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument));
			AssertEquals("Contains PreviousDocument?", expected: true, supportingInfoTypes.ContainsKey(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument));

			AssertEquals("Expected type for AdditionalInfo", typeof(TemporaryStorageAdditionalInfo), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
			AssertEquals("Expected type for SupportingDocument", typeof(EU.Business.CusTempStorage.TemporaryStorageSupportingDocument), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
			AssertEquals("Expected type for PreviousDocument", typeof(TemporaryStoragePreviousDocument), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
		});
	}

	public void TestTypeOfBillDocumentCaptions()
	{
		CombineAssertions(() =>
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(bill.TypeOfBillDocumentInfo);
			AssertEquals("Caption", "Transport Document Type", captionResourceString.Caption);
			AssertEquals("MediumCaption", "Transport Document Type", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Transp. Doc Type", captionResourceString.ShortCaption);
			AssertEquals("FullDescription", "Type of Transport Document", captionResourceString.FullDescription);
		});
	}

	public void TestABL_BillNumberCaptions()
	{
		CombineAssertions(() =>
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(bill.ABL_BillNumberInfo);
			AssertEquals("Caption", "Transport Document", captionResourceString.Caption);
			AssertEquals("MediumCaption", "Transport Document", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Transp. Doc", captionResourceString.ShortCaption);
			AssertEquals("FullDescription", "Transport Document Number", captionResourceString.FullDescription);
		});
	}

	public void TestShipperRegNo()
	{
		CombineAssertions(() =>
		{
			bill.ABL_OA_Shipper = address.PK;
			AssertEquals("Expected empty Id", ZString.Empty, bill.ABL_ShipperRegNo);

			bill.ABL_OA_Shipper = ZGuid.Empty;
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
			bill.ABL_OA_Shipper = address.PK;
			AssertEquals("Expected PAS Id with country code when no NIF or EORI declared", "GB333333333", bill.ABL_ShipperRegNo);

			bill.ABL_OA_Shipper = ZGuid.Empty;
			orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			bill.ABL_OA_Shipper = address.PK;
			AssertEquals("Expected NIF Id", "NIF22222222", bill.ABL_ShipperRegNo);

			bill.ABL_OA_Shipper = ZGuid.Empty;
			orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			orgHeader.OH_Category = OrgConstants.Category.Government;
			bill.ABL_OA_Shipper = address.PK;
			AssertEquals("Expected Country+NIF for non NAT Organizations", "ESNIF22222222", bill.ABL_ShipperRegNo);

			bill.ABL_OA_Shipper = ZGuid.Empty;
			OrgCusCode eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
			bill.ABL_OA_Shipper = address.PK;
			AssertEquals("Expected EORI Id with country code", "FR22222222", bill.ABL_ShipperRegNo);

			bill.ABL_OA_Shipper = ZGuid.Empty;
			eoriCusCode.OK_CustomsRegNo = "ES22222222";
			eoriCusCode.OK_RN_NKCodeCountry = "ES";
			bill.ABL_OA_Shipper = address.PK;
			AssertEquals("Expected EORI Id with country code not repeated", "ES22222222", bill.ABL_ShipperRegNo);
		});
	}

	public void TestConsigneeRegNo()
	{
		CombineAssertions(() =>
		{
			bill.ABL_OA_Consignee = address.PK;
			AssertEquals("Expected empty Id", ZString.Empty, bill.ABL_ConsigneeRegNo);

			bill.ABL_OA_Consignee = ZGuid.Empty;
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
			bill.ABL_OA_Consignee = address.PK;
			AssertEquals("Expected PAS Id with country code when no NIF or EORI declared", "GB333333333", bill.ABL_ConsigneeRegNo);

			bill.ABL_OA_Consignee = ZGuid.Empty;
			orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			bill.ABL_OA_Consignee = address.PK;
			AssertEquals("Expected NIF Id", "NIF22222222", bill.ABL_ConsigneeRegNo);

			bill.ABL_OA_Consignee = ZGuid.Empty;
			orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			orgHeader.OH_Category = OrgConstants.Category.Government;
			bill.ABL_OA_Consignee = address.PK;
			AssertEquals("Expected Country+NIF for non NAT Organizations", "ESNIF22222222", bill.ABL_ConsigneeRegNo);

			bill.ABL_OA_Consignee = ZGuid.Empty;
			OrgCusCode eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
			bill.ABL_OA_Consignee = address.PK;
			AssertEquals("Expected EORI Id with country code", "FR22222222", bill.ABL_ConsigneeRegNo);

			bill.ABL_OA_Consignee = ZGuid.Empty;
			eoriCusCode.OK_CustomsRegNo = "ES22222222";
			eoriCusCode.OK_RN_NKCodeCountry = "ES";
			bill.ABL_OA_Consignee = address.PK;
			AssertEquals("Expected EORI Id with country code not repeated", "ES22222222", bill.ABL_ConsigneeRegNo);
		});
	}

	public void TestClone()
	{
		bill.TypeOfBillDocument = "A001";

		var clonedBill = (TemporaryStorageBill)bill.Clone();
		AssertEquals("TypeOfBillDocument is cloned", "A001", clonedBill.TypeOfBillDocument);
	}

	protected override void SetUp()
	{
		base.SetUp();
		bill = (TemporaryStorageBill)GetNewBusinessObject();

		orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "TestOrg";

		address = orgHeader.Addresses.AddNew();
	}

	TemporaryStorageBill bill;
	OrgHeader orgHeader;
	OrgAddress address;
}
