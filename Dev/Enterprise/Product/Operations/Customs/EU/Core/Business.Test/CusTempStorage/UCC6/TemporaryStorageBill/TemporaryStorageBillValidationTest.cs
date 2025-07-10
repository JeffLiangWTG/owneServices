using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	sealed class TemporaryStorageBillValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAnyPackItemsWhenENSIsNotReused()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			tempBill.ABL_BolType = "BOL";
			tempBill.Validation.ValidateAll();
			AssertHasRowMessageError(tempBill, "For a PNTS that includes only Master bill, at least one item must be entered.");

			header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
			tempBill.Validation.ValidateAll();
			AssertNoRowMessageError(tempBill, "For a PNTS that includes only Master bill, at least one item must be entered.");

			header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
			tempBill.Validation.ValidateAll();
			AssertNoRowMessageError(tempBill, "For a PNTS that includes only Master bill, at least one item must be entered.");

			header.AMA_MessageType = string.Empty;
			tempBill.PackedItems.AddNew();
			tempBill.Validation.ValidateAll();
			AssertNoRowMessageError(tempBill, "For a PNTS that includes only Master bill, at least one item must be entered.");

			tempBill.PackedItems.Delete();
			var houseTempBill = header.Bills.AddNew();
			tempBill.Validation.ValidateAll();
			AssertNoRowMessageError(tempBill, "For a PNTS that includes only Master bill, at least one item must be entered.");

			houseTempBill.Validation.ValidateAll();
			AssertHasRowMessageError(houseTempBill, "For a PNTS that includes House bills, all House bills must include at least one item.");

			header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
			houseTempBill.Validation.ValidateAll();
			AssertNoRowMessageError(houseTempBill, "For a PNTS that includes House bills, all House bills must include at least one item.");

			header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
			houseTempBill.Validation.ValidateAll();
			AssertHasRowMessageError(houseTempBill, "For a PNTS that includes House bills, all House bills must include at least one item.");

			header.AMA_MessageType = string.Empty;
			houseTempBill.PackedItems.AddNew();
			houseTempBill.Validation.ValidateAll();
			AssertNoRowMessageError(houseTempBill, "For a PNTS that includes House bills, all House bills must include at least one item.");

			houseTempBill.PackedItems.Delete();
			header.IsENSReuse = true;
			houseTempBill.Validation.ValidateAll();
			AssertNoRowMessageError(houseTempBill, "For a PNTS that includes House bills, all House bills must include at least one item.");

			houseTempBill.Delete();
			tempBill.Validation.ValidateAll();
			AssertNoRowMessageError(tempBill, "For a PNTS that includes only Master bill, at least one item must be entered.");
		}

		public void TestValidateAnyPackItemsWhenENSIsNotReused_IsTransfer()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			tempBill.ABL_BolType = "BOL";
			tempBill.Validation.ValidateAll();
			AssertHasRowMessageError(tempBill, "For a PNTS that includes only Master bill, at least one item must be entered.");

			header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
			tempBill.Validation.ValidateAll();
			AssertNoRowMessageError(tempBill, "For a PNTS that includes only Master bill, at least one item must be entered.");

			header.AMA_MessageType = string.Empty;
			var houseTempBill = header.Bills.AddNew();
			tempBill.Validation.ValidateAll();
			AssertNoRowMessageError(tempBill, "For a PNTS that includes only Master bill, at least one item must be entered.");

			houseTempBill.Validation.ValidateAll();
			AssertHasRowMessageError(houseTempBill, "For a PNTS that includes House bills, all House bills must include at least one item.");

			header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
			houseTempBill.Validation.ValidateAll();
			AssertNoRowMessageError(houseTempBill, "For a PNTS that includes House bills, all House bills must include at least one item.");

			houseTempBill.Delete();
			tempBill.Validation.ValidateAll();
			AssertNoRowMessageError(tempBill, "For a PNTS that includes only Master bill, at least one item must be entered.");
		}

		public void TestValidateAnyPackItemsWhenENSIsNotReused_IsDeconsolidation()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var masterBill = header.Bills.AddNew();
			masterBill.ABL_BolType = "BOL";
			masterBill.Validation.ValidateAll();
			AssertHasRowMessageError(masterBill, "For a PNTS that includes only Master bill, at least one item must be entered.");

			header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
			masterBill.Validation.ValidateAll();
			AssertNoRowMessageError(masterBill, "For a PNTS that includes only Master bill, at least one item must be entered.");

			header.AMA_MessageType = string.Empty;
			var houseTempBill = header.Bills.AddNew();
			masterBill.Validation.ValidateAll();
			AssertNoRowMessageError(masterBill, "For a PNTS that includes only Master bill, at least one item must be entered.");

			houseTempBill.Validation.ValidateAll();
			AssertHasRowMessageError(houseTempBill, "For a PNTS that includes House bills, all House bills must include at least one item.");

			header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
			houseTempBill.Validation.ValidateAll();
			AssertHasRowMessageError(houseTempBill, "For a PNTS that includes House bills, all House bills must include at least one item.");

			houseTempBill.Delete();
			masterBill.Validation.ValidateAll();
			AssertNoRowMessageError(masterBill, "For a PNTS that includes only Master bill, at least one item must be entered.");
		}

		public void TestValidateHouseBillOnDeconsolidation()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
			var bill = header.MasterBill;
			bill.Validation.ValidateAll();
			AssertHasRowError("On Deconsolidation at least 1 House bill is mandatory", bill, "When Deconsolidation, one master bill and at least one house bill must be entered.");
		}

		public void TestValidateConsignorOrgPK_ShouldShowError_IfInvalidConsignorEntered()
		{
			var (consignor, consignee) = SetUpOrganizationData();
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			((IBusinessObjectInternals)tempBill).IsCopying = true;
			CombineAssertions(() =>
			{
				tempBill.ConsignorOrgPK = consignor.PK;
				AssertNoErrors("No errors if a valid consignor code is entered.", tempBill.ConsignorOrgPKInfo);

				tempBill.ConsignorOrgPK = consignee.PK;
				AssertHasErrorContaining("Error if an invalid consignor code is entered.", tempBill.ConsignorOrgPKInfo, ListValidation.InvalidCodeError);
			});
		}

		public void TestValidateConsignorOrgPK_ShouldShowWarning_IfValidConsignorEnteredButNoAddressSelected()
		{
			var (consignor, consignee) = SetUpOrganizationData();
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			((IBusinessObjectInternals)tempBill).IsCopying = true;
			CombineAssertions(() =>
			{
				tempBill.ConsignorOrgPK = consignor.PK;
				tempBill.ABL_OA_Shipper = ZGuid.Empty;
				AssertHasWarning("Warning if valid consignor code is entered but no address is selected.", tempBill.ConsignorOrgPKInfo, TemporaryStorageBillValidation.NoAddressSelectedWarning);

				tempBill.ABL_OA_Shipper = consignor.Addresses[0].PK;
				AssertNoWarnings("No warning if valid consignor code is entered and valid address is selected.", tempBill.ConsignorOrgPKInfo);
			});
		}

		public void TestValidateConsigneeOrgPK_ShouldShowError_IfInvalidConsigneeEntered()
		{
			var (consignor, consignee) = SetUpOrganizationData();
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			((IBusinessObjectInternals)tempBill).IsCopying = true;
			CombineAssertions(() =>
			{
				tempBill.ConsigneeOrgPK = consignee.PK;
				AssertNoErrors("No errors if a valid consignee pk is entered.", tempBill.ConsigneeOrgPKInfo);

				tempBill.ConsigneeOrgPK = consignor.PK;
				AssertHasErrorContaining("Error if an invalid consignee pk is entered.", tempBill.ConsigneeOrgPKInfo, ListValidation.InvalidCodeError);
			});
		}

		public void TestValidateConsigneeOrgPK_ShouldShowWarning_IfValidConsigneeEnteredButNoAddressSelected()
		{
			var (consignor, consignee) = SetUpOrganizationData();
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			((IBusinessObjectInternals)tempBill).IsCopying = true;
			CombineAssertions(() =>
			{
				tempBill.ConsigneeOrgPK = consignee.PK;
				tempBill.ABL_OA_Consignee = ZGuid.Empty;
				AssertHasWarning("Warning if valid consignee code is entered but no address is selected.", tempBill.ConsigneeOrgPKInfo, TemporaryStorageBillValidation.NoAddressSelectedWarning);

				tempBill.ABL_OA_Consignee = consignee.Addresses[0].PK;
				AssertNoWarnings("No warning if valid consignee code is entered and valid address is selected.", tempBill.ConsigneeOrgPKInfo);
			});
		}

		(OrgHeader, OrgHeader) SetUpOrganizationData()
		{
			var header1 = Factory.NewWithValidTestData<OrgHeader>();
			header1.OH_Code = "H1";
			header1.OrganisationTypes = OrganisationTypes.Consignor;
			var address1 = header1.Addresses.AddNew();
			address1.OA_Address1 = "ADD1";
			address1.AddAddressType(OrgAddressType.Office);

			var header2 = Factory.NewWithValidTestData<OrgHeader>();
			header2.OH_Code = "H2";
			header2.OrganisationTypes = OrganisationTypes.Consignee;
			var address2 = header2.Addresses.AddNew();
			address2.OA_Address1 = "ADD2";
			address2.AddAddressType(OrgAddressType.Office);

			Factory.Save();
			return (header1, header2);
		}

		public void TestValidateMaxAmountHwb_BolType()
		{
			const int maxAmountHwb = 9;
			var header = Factory.NewMoq<TemporaryStorageHeader>();
			header.CallBase = true;

			var prop = typeof(AsycudaManifestHeader).GetField("bills", BindingFlags.NonPublic | BindingFlags.Instance);
			prop.SetValue(header.Object, null);

			header.Protected().Setup<ITemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>>("CreateNewTemporaryStorageBillCollection").Returns(() =>
			{
				var billsCollection = new Mock<TemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>>(header.Object);
				billsCollection.CallBase = true;
				billsCollection.Protected().Setup<BusinessObject>("CreateNewBusinessObject").Returns(() =>
				{
					var billMock = billsCollection.Object.Factory.NewMoq<TemporaryStorageBill>();

					billMock.Protected().Setup<AsycudaBillValidation>("GetNewValidation").Returns(() =>
					{
						var billValidationMock = new Mock<TemporaryStorageBillValidation>(billMock.Object);
						billValidationMock.CallBase = true;
						billValidationMock.Protected().Setup<int>("MaximumAllowedHWBCount").Returns(() => maxAmountHwb);

						return billValidationMock.Object;
					});
					return billMock.Object;
				});

				return billsCollection.Object;
			});

			for (var i = 0; i < maxAmountHwb; i++)
			{
				var tempBill = header.Object.Bills.AddNew();
				tempBill.ABL_BolType = TemporaryStorageBillKindList.Codes.HWB;
				AssertNoErrors($"Once the number of HWBs is less than {maxAmountHwb} everything is OK", tempBill.ABL_BolTypeInfo);
			}

			var tempBill1 = header.Object.Bills.AddNew();
			tempBill1.ABL_BolType = TemporaryStorageBillKindList.Codes.MWB;
			AssertNoErrors("Having maximum allowed number of HWBs together with one MWB is OK", tempBill1.ABL_BolTypeInfo);

			var tempBill2 = header.Object.Bills.AddNew();
			tempBill2.ABL_BolType = TemporaryStorageBillKindList.Codes.HWB;
			AssertHasError("There is a requirement stating that up to 99.999 HWBs are allowed per declaration", tempBill2.ABL_BolTypeInfo, $"Only {maxAmountHwb} house waybills allowed in 1 Temporary Storage declaration. Please delete present line.");
		}

		public void TestValidateABL_BolType()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();

			tempBill.ABL_BolType = TemporaryStorageBillKindList.Codes.MWB;
			AssertNoMessageErrors("Bill Type can be filled as MWB, no message error should be reported", tempBill.ABL_BolTypeInfo);

			AssertEquals("Prerequisite: MasterBill type is neither MWB nor HWB by default.", TemporaryStorageBill.ChildBolCode, header.MasterBill.ABL_BolType);
			AssertNoMessageErrors("Bill Type is neither MWB nor HWB, even if the code is not in the TemporaryStorageBillKindList, no message error should be reported", header.MasterBill.ABL_BolTypeInfo);
		}

		public void TestValidateTypeOfBillDocument()
		{
			using var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
			TemporaryStorageTestDataHelper.SetUpBillDocumentTypeCodeList(Factory);
			var header = Factory.New<TemporaryStorageHeader>();
			deciderTestContext.EnableRule(x => x.IsTypeOfBillDocumentCheckSupported);
			deciderTestContext.EnableRule(x => x.IsTypeOfBillDocumentMandatory);
			deciderTestContext.DisableRule(x => x.AllowDuplicateTypeAndNumber);

			CombineAssertions("When IsTypeOfBillDocumentCheckSupported = true", () =>
			{
				var tempBill = header.Bills.AddNew();
				tempBill.TypeOfBillDocument = "NO";
				AssertHasMessageError("Bill Type should have message error if the code is not in the list", tempBill.TypeOfBillDocumentInfo, "The code you have selected is not in the list.");

				tempBill.TypeOfBillDocument = "";
				AssertHasMessageError("Bill Type should have value", tempBill.TypeOfBillDocumentInfo, "You have not entered a Type of Bill Document.");

				tempBill.TypeOfBillDocument = "C624";
				AssertNoMessageErrors("Bill Type should have no message error if the code is one of the document type code of TD44T", tempBill.TypeOfBillDocumentInfo);
			});

			deciderTestContext.DisableRule(x => x.IsTypeOfBillDocumentCheckSupported);
			CombineAssertions("When IsTypeOfBillDocumentCheckSupported = false", () =>
			{
				var tempBill = header.Bills.AddNew();
				tempBill.TypeOfBillDocument = "NO";
				AssertNoMessageError("Bill Type should have message error if the code is not in the list", tempBill.TypeOfBillDocumentInfo, "The code you have selected is not in the list.");

				tempBill.TypeOfBillDocument = "";
				AssertNoMessageError("Bill Type should have value", tempBill.TypeOfBillDocumentInfo, "You have not entered a Type of Bill Document.");

				tempBill.TypeOfBillDocument = "C624";
				AssertNoMessageErrors("Bill Type should have no message error if the code is one of the document type code of TD44T", tempBill.TypeOfBillDocumentInfo);
			});
		}

		public void TestABL_BillNumber()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			tempBill.ABL_BillNumber = "Bill";
			AssertNoMessageError("Bill Number should have no message error if it has value", tempBill.ABL_BillNumberInfo, "You have not entered a Bill Number.");

			tempBill.ABL_BillNumber = "";
			AssertHasMessageError("Bill Number should have message error if it doesn't has value", tempBill.ABL_BillNumberInfo, "You have not entered a Bill Number.");
		}

		public void TestCheckDuplicateTypeAndNumber()
		{
			using var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
			var header = Factory.New<TemporaryStorageHeader>();
			deciderTestContext.EnableRule(x => x.IsTypeOfBillDocumentCheckSupported);
			deciderTestContext.EnableRule(x => x.IsTypeOfBillDocumentMandatory);
			deciderTestContext.EnableRule(x => x.IsABL_BillNumberMandatory);

			deciderTestContext.DisableRule(x => x.AllowDuplicateTypeAndNumber);
			CombineAssertions("When rule AllowDuplicateTypeAndNumber is disabled", () =>
			{
				var tempBill = header.Bills.AddNew();
				tempBill.TypeOfBillDocument = "C624";
				tempBill.ABL_BillNumber = "Bill";

				AssertNoMessageError("Bill Type should not have duplicate message error if there is only one record", tempBill.TypeOfBillDocumentInfo, "Bill Number and Type must be unique.");
				AssertNoMessageError("Bill Number should not have duplicate message error if there is only one record", tempBill.ABL_BillNumberInfo, "Bill Number and Type must be unique.");

				var tempBill2 = header.Bills.AddNew();
				tempBill2.TypeOfBillDocument = "C624";
				tempBill2.ABL_BillNumber = "Bill";
				tempBill2.Validation.ValidateTypeOfBillDocument();
				AssertHasMessageError("Bill Type should have duplicate message error if values are duplicate", tempBill2.TypeOfBillDocumentInfo, "Bill Number and Type must be unique.");
				AssertHasMessageError("Bill Number should have duplicate message error if values are duplicate", tempBill2.ABL_BillNumberInfo, "Bill Number and Type must be unique.");

				tempBill2.TypeOfBillDocument = "C625";
				tempBill2.Validation.ValidateABL_BillNumber();
				AssertNoMessageError("Bill Type should not have duplicate message error if values are not duplicate", tempBill2.TypeOfBillDocumentInfo, "Bill Number and Type must be unique.");
				AssertNoMessageError("Bill Number should not have duplicate message error if values are not duplicate", tempBill2.ABL_BillNumberInfo, "Bill Number and Type must be unique.");
			});

			deciderTestContext.EnableRule(x => x.AllowDuplicateTypeAndNumber);
			CombineAssertions("When rule AllowDuplicateTypeAndNumber is enabled", () =>
			{
				var tempBill = header.Bills.AddNew();
				tempBill.TypeOfBillDocument = "C624";
				tempBill.ABL_BillNumber = "Bill";

				AssertNoMessageError("Bill Type should not have duplicate message error if there is only one record", tempBill.TypeOfBillDocumentInfo, "Bill Number and Type must be unique.");
				AssertNoMessageError("Bill Number should not have duplicate message error if there is only one record", tempBill.ABL_BillNumberInfo, "Bill Number and Type must be unique.");

				var tempBill2 = header.Bills.AddNew();
				tempBill2.TypeOfBillDocument = "C624";
				tempBill2.ABL_BillNumber = "Bill";
				tempBill2.Validation.ValidateTypeOfBillDocument();
				AssertNoMessageError("Bill Type should not have duplicate message error if values are duplicate", tempBill2.TypeOfBillDocumentInfo, "Bill Number and Type must be unique.");
				AssertNoMessageError("Bill Number should not have duplicate message error if values are duplicate", tempBill2.ABL_BillNumberInfo, "Bill Number and Type must be unique.");

				tempBill2.TypeOfBillDocument = "C625";
				tempBill2.Validation.ValidateABL_BillNumber();
				AssertNoMessageError("Bill Type should not have duplicate message error if values are not duplicate", tempBill2.TypeOfBillDocumentInfo, "Bill Number and Type must be unique.");
				AssertNoMessageError("Bill Number should not have duplicate message error if values are not duplicate", tempBill2.ABL_BillNumberInfo, "Bill Number and Type must be unique.");
			});
		}

		public void TestCheckABL_ShipperRegNoType()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				tempBill.Validation.ValidateABL_ShipperRegNoType();
				AssertNoMessageErrorContaining("There is no MandatoryValidation on ABL_ShipperRegNoTypeInfo when ABL_OA_Shipper is null.", tempBill.ABL_ShipperRegNoTypeInfo, MandatoryValidation.YouHaveNotEntered);
				tempBill.ABL_OA_Shipper = ZGuid.NewZGuid();
				tempBill.Validation.ValidateABL_ShipperRegNoType();
				AssertHasMessageErrorContaining("Value is empty.", tempBill.ABL_ShipperRegNoTypeInfo, MandatoryValidation.YouHaveNotEntered);

				tempBill.ABL_ShipperRegNoType = "X";
				tempBill.Validation.ValidateABL_ShipperRegNoType();
				AssertNoMessageErrorContaining("Value is not empty.", tempBill.ABL_ShipperRegNoTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining("Value is not in the list.", tempBill.ABL_ShipperRegNoTypeInfo, ListValidation.InvalidCodeMessageError.ToString());

				tempBill.ABL_ShipperRegNoType = TypeOfPersonList.Codes.NaturalPerson;
				tempBill.Validation.ValidateABL_ShipperRegNoType();
				AssertNoMessageErrorContaining("Value is in the list.", tempBill.ABL_ShipperRegNoTypeInfo, ListValidation.InvalidCodeMessageError.ToString());
			});
		}

		public void TestCheckABL_ShipperRegNoType_IsTransfer()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				tempBill.ABL_OA_Shipper = ZGuid.NewZGuid();
				tempBill.Validation.ValidateABL_ShipperRegNoType();
				AssertHasMessageErrorContaining("Value is empty.", tempBill.ABL_ShipperRegNoTypeInfo, MandatoryValidation.YouHaveNotEntered);

				header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
				tempBill.Validation.ValidateABL_ShipperRegNoType();
				AssertNoMessageErrorContaining("Value is empty when IsTransfer.", tempBill.ABL_ShipperRegNoTypeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckABL_ShipperRegNoType_IsDeconsolidation()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var masterBill = header.Bills.AddNew();
			masterBill.ABL_BolType = "BOL";
			CombineAssertions(() =>
			{
				masterBill.ABL_OA_Shipper = ZGuid.NewZGuid();
				masterBill.Validation.ValidateABL_ShipperRegNoType();
				AssertHasMessageErrorContaining("Value is empty.", masterBill.ABL_ShipperRegNoTypeInfo, MandatoryValidation.YouHaveNotEntered);

				header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
				masterBill.Validation.ValidateABL_ShipperRegNoType();
				AssertNoMessageErrorContaining("Value is empty when IsDeconsolidation.", masterBill.ABL_ShipperRegNoTypeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckABL_ShipperState()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				tempBill.Validation.ValidateABL_ShipperState();
				AssertNoMessageErrorContaining("There is no MandatoryValidation on ABL_ShipperStateInfo when ABL_OA_Shipper is null.", tempBill.ABL_ShipperStateInfo, MandatoryValidation.YouHaveNotEntered);
				tempBill.ABL_OA_Shipper = ZGuid.NewZGuid();
				tempBill.Validation.ValidateABL_ShipperState();
				AssertHasMessageErrorContaining("Value is empty.", tempBill.ABL_ShipperStateInfo, MandatoryValidation.YouHaveNotEntered);

				tempBill.ABL_RN_NKShipperCountry = "US";
				tempBill.ABL_ShipperState = "XX";
				tempBill.Validation.ValidateABL_ShipperState();
				AssertNoMessageErrorContaining("Value is not empty.", tempBill.ABL_ShipperStateInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining("Value is not in the list.", tempBill.ABL_ShipperStateInfo, ListValidation.InvalidCodeMessageError.ToString());

				tempBill.ABL_ShipperState = "CA";
				tempBill.Validation.ValidateABL_ShipperState();
				AssertNoMessageErrorContaining("Value is in the list.", tempBill.ABL_ShipperStateInfo, ListValidation.InvalidCodeMessageError.ToString());
			});
		}

		public void TestCheckABL_ConsigneeRegNoType()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				tempBill.Validation.ValidateABL_ConsigneeRegNoType();
				AssertNoMessageErrorContaining("There is no MandatoryValidation on ABL_ConsigneeRegNoTypeInfo when ABL_OA_Consignee is null.", tempBill.ABL_ConsigneeRegNoTypeInfo, MandatoryValidation.YouHaveNotEntered);
				tempBill.ABL_OA_Consignee = ZGuid.NewZGuid();
				tempBill.Validation.ValidateABL_ConsigneeRegNoType();
				AssertHasMessageErrorContaining("Value is empty.", tempBill.ABL_ConsigneeRegNoTypeInfo, MandatoryValidation.YouHaveNotEntered);

				tempBill.ABL_ConsigneeRegNoType = "X";
				tempBill.Validation.ValidateABL_ConsigneeRegNoType();
				AssertNoMessageErrorContaining("Value is empty.", tempBill.ABL_ConsigneeRegNoTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining("Value is not in the list.", tempBill.ABL_ConsigneeRegNoTypeInfo, ListValidation.InvalidCodeMessageError.ToString());

				tempBill.ABL_ConsigneeRegNoType = TypeOfPersonList.Codes.NaturalPerson;
				tempBill.Validation.ValidateABL_ConsigneeRegNoType();
				AssertNoMessageErrorContaining("Value is in the list.", tempBill.ABL_ConsigneeRegNoTypeInfo, ListValidation.InvalidCodeMessageError.ToString());
			});
		}

		public void TestCheckABL_ConsigneeRegNoType_IsTransfer()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				tempBill.ABL_OA_Consignee = ZGuid.NewZGuid();
				tempBill.Validation.ValidateABL_ConsigneeRegNoType();
				AssertHasMessageErrorContaining("Value is empty.", tempBill.ABL_ConsigneeRegNoTypeInfo, MandatoryValidation.YouHaveNotEntered);

				header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
				tempBill.Validation.ValidateABL_ConsigneeRegNoType();
				AssertNoMessageErrorContaining("Value is empty, but messageType is TF -Transfer", tempBill.ABL_ConsigneeRegNoTypeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckABL_ConsigneeRegNoType_IsDeconsolidation()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var masterBill = header.Bills.AddNew();
			masterBill.ABL_BolType = "BOL";
			CombineAssertions(() =>
			{
				masterBill.ABL_OA_Consignee = ZGuid.NewZGuid();
				masterBill.Validation.ValidateABL_ConsigneeRegNoType();
				AssertHasMessageErrorContaining("Value is empty.", masterBill.ABL_ConsigneeRegNoTypeInfo, MandatoryValidation.YouHaveNotEntered);

				header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
				masterBill.Validation.ValidateABL_ConsigneeRegNoType();
				AssertNoMessageErrorContaining("Value is empty, but messageType is DC - Deconsolidation", masterBill.ABL_ConsigneeRegNoTypeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckABL_ConsigneeState()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				tempBill.Validation.ValidateABL_ConsigneeState();
				AssertNoMessageErrorContaining("There is no MandatoryValidation on ABL_ConsigneeStateInfo when ABL_OA_Consignee is null.", tempBill.ABL_ConsigneeStateInfo, MandatoryValidation.YouHaveNotEntered);
				tempBill.ABL_OA_Consignee = ZGuid.NewZGuid();
				tempBill.Validation.ValidateABL_ConsigneeState();
				AssertHasMessageErrorContaining("Value is empty.", tempBill.ABL_ConsigneeStateInfo, MandatoryValidation.YouHaveNotEntered);

				tempBill.ABL_RN_NKConsigneeCountry = "US";
				tempBill.ABL_ConsigneeState = "XX";
				tempBill.Validation.ValidateABL_ConsigneeState();
				AssertNoMessageErrorContaining("Value is empty.", tempBill.ABL_ConsigneeStateInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining("Value is not in the list.", tempBill.ABL_ConsigneeStateInfo, ListValidation.InvalidCodeMessageError.ToString());

				tempBill.ABL_ConsigneeState = "CA";
				tempBill.Validation.ValidateABL_ConsigneeState();
				AssertNoMessageErrorContaining("Value is in the list.", tempBill.ABL_ConsigneeStateInfo, ListValidation.InvalidCodeMessageError.ToString());
			});
		}

		public void TestCheckABL_NotifyPartyRegNoType()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				tempBill.Validation.ValidateABL_NotifyPartyRegNoType();
				AssertNoMessageErrorContaining("There is no MandatoryValidation on ABL_NotifyPartyRegNoTypeInfo when ABL_NotifyParty is null.", tempBill.ABL_NotifyPartyRegNoTypeInfo, MandatoryValidation.YouHaveNotEntered);
				tempBill.ABL_OA_NotifyParty = ZGuid.NewZGuid();
				tempBill.Validation.ValidateABL_NotifyPartyRegNoType();
				AssertHasMessageErrorContaining("Value is empty.", tempBill.ABL_NotifyPartyRegNoTypeInfo, MandatoryValidation.YouHaveNotEntered);

				tempBill.ABL_NotifyPartyRegNoType = "X";
				tempBill.Validation.ValidateABL_NotifyPartyRegNoType();
				AssertNoMessageErrorContaining("Value is empty.", tempBill.ABL_NotifyPartyRegNoTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining("Value is not in the list.", tempBill.ABL_NotifyPartyRegNoTypeInfo, ListValidation.InvalidCodeMessageError.ToString());

				tempBill.ABL_NotifyPartyRegNoType = TypeOfPersonList.Codes.NaturalPerson;
				tempBill.Validation.ValidateABL_NotifyPartyRegNoType();
				AssertNoMessageErrorContaining("Value is in the list.", tempBill.ABL_NotifyPartyRegNoTypeInfo, ListValidation.InvalidCodeMessageError.ToString());
			});
		}

		public void TestCheckABL_NotifyPartyState()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				tempBill.Validation.ValidateABL_NotifyPartyState();
				AssertNoMessageErrorContaining("There is no MandatoryValidation on ABL_NotifyPartyStateInfo when ABL_NotifyParty is null.", tempBill.ABL_NotifyPartyStateInfo, MandatoryValidation.YouHaveNotEntered);
				tempBill.ABL_OA_NotifyParty = ZGuid.NewZGuid();
				tempBill.Validation.ValidateABL_NotifyPartyState();
				AssertHasMessageErrorContaining("Value is empty.", tempBill.ABL_NotifyPartyStateInfo, MandatoryValidation.YouHaveNotEntered);

				tempBill.ABL_RN_NKNotifyPartyCountry = "US";
				tempBill.ABL_NotifyPartyState = "XX";
				tempBill.Validation.ValidateABL_NotifyPartyState();
				AssertNoMessageErrorContaining("Value is empty.", tempBill.ABL_NotifyPartyStateInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining("Value is not in the list.", tempBill.ABL_NotifyPartyStateInfo, ListValidation.InvalidCodeMessageError.ToString());

				tempBill.ABL_NotifyPartyState = "CA";
				tempBill.Validation.ValidateABL_NotifyPartyState();
				AssertNoMessageErrorContaining("Value is in the list.", tempBill.ABL_NotifyPartyStateInfo, ListValidation.InvalidCodeMessageError.ToString());
			});
		}

		public void TestCheckTypeOfBillDocumentWhenENSIsReused()
		{
			AssertPreviousDocumentRequired("No MessageError is expected when IsENSReuse is false.", false, false, false, false, false);
			AssertPreviousDocumentRequired("MessageError is expected when IsENSReuse is true and no previous document exists in header or bills or all items.", true, false, false, false, true);
			AssertPreviousDocumentRequired("No MessageError is expected when IsENSReuse is true and header has a previous document.", true, true, false, false, false);
			AssertPreviousDocumentRequired("No MessageError is expected when IsENSReuse is true and bill has a previous document.", true, false, true, false, false);
			AssertPreviousDocumentRequired("No MessageError is expected when IsENSReuse is true and all items has a previous document.", true, false, false, true, false);

			void AssertPreviousDocumentRequired(ZString message, bool isENSReUse, bool hasHeaderPreviousDocument, bool hasBillPreviousDocument, bool hasAllItemsPreviousDocument, bool hasMessageError)
			{
				var header = Factory.New<TemporaryStorageHeader>();
				var tempBill = header.Bills.AddNew();
				var item1 = tempBill.PackedItems.AddNew();
				var item2 = tempBill.PackedItems.AddNew();

				if (isENSReUse)
				{
					header.IsENSReuse = isENSReUse;
				}

				if (hasHeaderPreviousDocument)
				{
					header.PreviousDocuments.AddNew();
				}

				if (hasBillPreviousDocument)
				{
					tempBill.PreviousDocuments.AddNew();
				}

				if (hasAllItemsPreviousDocument)
				{
					item1.PreviousDocuments.AddNew();
					item2.PreviousDocuments.AddNew();
				}
				else
				{
					item1.PreviousDocuments.AddNew();
				}

				var messageError = "Please create a previous document.";
				tempBill.Validation.ValidateTypeOfBillDocument();
				AssertEquals(message, hasMessageError, tempBill.TypeOfBillDocumentInfo.HasMessageError(messageError));
			}
		}

		public void TestValidateConsignorOrgPK_ChecksForENSIsReused()
		{
			var (consignor, consignee) = SetUpOrganizationData();
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			((IBusinessObjectInternals)tempBill).IsCopying = true;
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsConsignorOrgPKCheckSupported);
				deciderTestContext.EnableRule(x => x.IsConsignorOrgPKMandatory);

				header.IsENSReuse = true;
				tempBill.Validation?.ValidateConsignorOrgPK();
				AssertNoMessageErrorContaining(tempBill.ConsignorOrgPKInfo, "Consignor must be filled. Select an organization or fill a consignor address in the ‘Bill Parties’ tab.");

				header.IsENSReuse = false;
				tempBill.Validation?.ValidateConsignorOrgPK();
				AssertHasMessageError(tempBill.ConsignorOrgPKInfo, "Consignor must be filled. Select an organization or fill a consignor address in the ‘Bill Parties’ tab.");

				tempBill.ConsignorOrgPK = consignor.PK;
				AssertNoMessageErrorContaining(tempBill.ConsignorOrgPKInfo, "Consignor must be filled. Select an organization or fill a consignor address in the ‘Bill Parties’ tab.");

				deciderTestContext.DisableRule(x => x.IsConsignorOrgPKCheckSupported);
				tempBill.ConsignorOrgPK = ZGuid.Empty;
				AssertNoMessageErrorContaining(tempBill.ConsignorOrgPKInfo, "Consignor must be filled. Select an organization or fill a consignor address in the ‘Bill Parties’ tab.");
			});
		}

		public void TestValidateConsignorOrgPK_IsTransfer()
		{
			var (consignor, consignee) = SetUpOrganizationData();
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			((IBusinessObjectInternals)tempBill).IsCopying = true;
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsConsignorOrgPKCheckSupported);
				deciderTestContext.EnableRule(x => x.IsConsignorOrgPKMandatory);

				header.IsENSReuse = false;
				tempBill.Validation?.ValidateConsignorOrgPK();
				AssertHasMessageError(tempBill.ConsignorOrgPKInfo, "Consignor must be filled. Select an organization or fill a consignor address in the ‘Bill Parties’ tab.");

				header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
				tempBill.Validation?.ValidateConsignorOrgPK();
				tempBill.ConsignorOrgPK = ZGuid.Empty;
				AssertNoMessageError(tempBill.ConsignorOrgPKInfo, "Consignor must be filled. Select an organization or fill a consignor address in the ‘Bill Parties’ tab.");

				deciderTestContext.DisableRule(x => x.IsConsignorOrgPKCheckSupported);
				header.AMA_MessageType = ZString.Empty;
				tempBill.Validation?.ValidateConsignorOrgPK();
				AssertNoMessageError(tempBill.ConsignorOrgPKInfo, "Consignor must be filled. Select an organization or fill a consignor address in the ‘Bill Parties’ tab.");
			});
		}

		public void TestValidateConsignorOrgPK_IsDeconsolidation()
		{
			var (consignor, consignee) = SetUpOrganizationData();
			var header = Factory.New<TemporaryStorageHeader>();
			var masterBill = header.Bills.AddNew();
			masterBill.ABL_BolType = "BOL";
			((IBusinessObjectInternals)masterBill).IsCopying = true;
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsConsignorOrgPKCheckSupported);
				deciderTestContext.EnableRule(x => x.IsConsignorOrgPKMandatory);

				header.IsENSReuse = false;
				masterBill.Validation?.ValidateConsignorOrgPK();
				AssertHasMessageError(masterBill.ConsignorOrgPKInfo, "Consignor must be filled. Select an organization or fill a consignor address in the ‘Bill Parties’ tab.");

				header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
				masterBill.Validation?.ValidateConsignorOrgPK();
				masterBill.ConsignorOrgPK = ZGuid.Empty;
				AssertNoMessageError(masterBill.ConsignorOrgPKInfo, "Consignor must be filled. Select an organization or fill a consignor address in the ‘Bill Parties’ tab.");

				deciderTestContext.DisableRule(x => x.IsConsignorOrgPKCheckSupported);
				header.AMA_MessageType = ZString.Empty;
				masterBill.Validation?.ValidateConsignorOrgPK();
				AssertNoMessageError(masterBill.ConsignorOrgPKInfo, "Consignor must be filled. Select an organization or fill a consignor address in the ‘Bill Parties’ tab.");
			});
		}

		public void TestCheckABL_ShipperName_ForENSIsReused()
		{
			var (consignor, consignee) = SetUpOrganizationData();
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsShipperNameCheckSupported);
				deciderTestContext.EnableRule(x => x.IsShipperNameMandatory);

				header.IsENSReuse = true;
				tempBill.Validation.ValidateABL_ShipperName();
				AssertNoMessageErrorContaining(tempBill.ABL_ShipperNameInfo, "The ‘Name’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				header.IsENSReuse = false;
				tempBill.Validation.ValidateABL_ShipperName();
				AssertHasMessageError(tempBill.ABL_ShipperNameInfo, "The ‘Name’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				tempBill.ABL_ShipperName = "BBB";
				AssertNoMessageErrorContaining(tempBill.ABL_ShipperNameInfo, "The ‘Name’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				deciderTestContext.DisableRule(x => x.IsShipperNameCheckSupported);
				tempBill.ABL_ShipperName = ZString.Empty;
				AssertNoMessageErrorContaining(tempBill.ABL_ShipperNameInfo, "The ‘Name’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
			});
		}

		public void TestCheckABL_ShipperName_Transfer()
		{
			var (consignor, consignee) = SetUpOrganizationData();
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsShipperNameCheckSupported);
				deciderTestContext.EnableRule(x => x.IsShipperNameMandatory);

				header.IsENSReuse = false;
				tempBill.Validation.ValidateABL_ShipperName();
				AssertHasMessageError(tempBill.ABL_ShipperNameInfo, "The ‘Name’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
				tempBill.Validation.ValidateABL_ShipperName();
				AssertNoMessageError(tempBill.ABL_ShipperNameInfo, "The ‘Name’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				deciderTestContext.DisableRule(x => x.IsShipperNameCheckSupported);
				header.AMA_MessageType = ZString.Empty;
				tempBill.Validation.ValidateABL_ShipperName();
				AssertNoMessageError(tempBill.ABL_ShipperNameInfo, "The ‘Name’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
			});
		}

		public void TestCheckABL_ShipperName_Deconsolidation()
		{
			var (consignor, consignee) = SetUpOrganizationData();
			var header = Factory.New<TemporaryStorageHeader>();
			var masterBill = header.Bills.AddNew();
			masterBill.ABL_BolType = "BOL";
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsShipperNameCheckSupported);
				deciderTestContext.EnableRule(x => x.IsShipperNameMandatory);

				header.IsENSReuse = false;
				masterBill.Validation.ValidateABL_ShipperName();
				AssertHasMessageError(masterBill.ABL_ShipperNameInfo, "The ‘Name’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
				masterBill.Validation.ValidateABL_ShipperName();
				AssertNoMessageError(masterBill.ABL_ShipperNameInfo, "The ‘Name’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				deciderTestContext.DisableRule(x => x.IsShipperNameCheckSupported);
				header.AMA_MessageType = ZString.Empty;
				masterBill.Validation.ValidateABL_ShipperName();
				AssertNoMessageError(masterBill.ABL_ShipperNameInfo, "The ‘Name’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
			});
		}

		public void TestCheckABL_ShipperRegNoType_ForENSIsReused()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsShipperRegNoTypeCheckSupported);
				deciderTestContext.EnableRule(x => x.IsShipperRegNoTypeMandatory);

				header.IsENSReuse = true;
				tempBill.Validation.ValidateABL_ShipperRegNoType();
				AssertNoMessageErrorContaining(tempBill.ABL_ShipperRegNoTypeInfo, "The ‘Type of Person’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				header.IsENSReuse = false;
				tempBill.Validation.ValidateABL_ShipperRegNoType();
				AssertHasMessageError(tempBill.ABL_ShipperRegNoTypeInfo, "The ‘Type of Person’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				tempBill.ABL_ShipperRegNoType = TypeOfPersonList.Codes.NaturalPerson;
				AssertNoMessageErrorContaining(tempBill.ABL_ShipperRegNoTypeInfo, "The ‘Type of Person’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				deciderTestContext.DisableRule(x => x.IsShipperRegNoTypeCheckSupported);
				tempBill.ABL_ShipperRegNoType = ZString.Empty;
				AssertNoMessageErrorContaining(tempBill.ABL_ShipperRegNoTypeInfo, "The ‘Type of Person’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
			});
		}

		public void TestCheckABL_RN_NKShipperCountry()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				tempBill.ABL_RN_NKShipperCountry = "XX";
				AssertHasMessageErrorContaining("Value is not in the list.", tempBill.ABL_RN_NKShipperCountryInfo, ListValidation.InvalidCodeMessageError.ToString());
				tempBill.ABL_RN_NKShipperCountry = "US";
				AssertNoMessageErrorContaining("Value is in the list.", tempBill.ABL_RN_NKShipperCountryInfo, ListValidation.InvalidCodeMessageError.ToString());
			});
		}

		public void TestCheckABL_RN_NKShipperCountry_IsTransfer()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsShipperCountryCheckSupported);
				deciderTestContext.EnableRule(x => x.IsShipperCountryMandatory);

				header.IsENSReuse = false;
				tempBill.Validation.ValidateABL_RN_NKShipperCountry();
				AssertHasMessageError(tempBill.ABL_RN_NKShipperCountryInfo, "The ‘Country’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
				AssertNoMessageError(tempBill.ABL_RN_NKShipperCountryInfo, "The ‘Country’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				deciderTestContext.DisableRule(x => x.IsShipperCountryCheckSupported);
				header.AMA_MessageType = ZString.Empty;
				tempBill.Validation.ValidateABL_RN_NKShipperCountry();
				AssertNoMessageError(tempBill.ABL_RN_NKShipperCountryInfo, "The ‘Country’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
			});
		}

		public void TestCheckABL_RN_NKShipperCountry_IsDeconsolidation()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var masterBill = header.Bills.AddNew();
			masterBill.ABL_BolType = "BOL";
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsShipperCountryCheckSupported);
				deciderTestContext.EnableRule(x => x.IsShipperCountryMandatory);

				header.IsENSReuse = false;
				masterBill.Validation.ValidateABL_RN_NKShipperCountry();
				AssertHasMessageError(masterBill.ABL_RN_NKShipperCountryInfo, "The ‘Country’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
				AssertNoMessageError(masterBill.ABL_RN_NKShipperCountryInfo, "The ‘Country’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				deciderTestContext.DisableRule(x => x.IsShipperCountryCheckSupported);
				header.AMA_MessageType = ZString.Empty;
				masterBill.Validation.ValidateABL_RN_NKShipperCountry();
				AssertNoMessageError(masterBill.ABL_RN_NKShipperCountryInfo, "The ‘Country’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
			});
		}

		public void TestCheckABL_RN_NKShipperCountry_ForENSIsReused()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsShipperCountryCheckSupported);
				deciderTestContext.EnableRule(x => x.IsShipperCountryMandatory);

				header.IsENSReuse = true;
				tempBill.Validation.ValidateABL_RN_NKShipperCountry();
				AssertNoMessageErrorContaining(tempBill.ABL_RN_NKShipperCountryInfo, "The ‘Country’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				header.IsENSReuse = false;
				tempBill.Validation.ValidateABL_RN_NKShipperCountry();
				AssertHasMessageError(tempBill.ABL_RN_NKShipperCountryInfo, "The ‘Country’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				tempBill.ABL_RN_NKShipperCountry = "US";
				AssertNoMessageErrorContaining(tempBill.ABL_RN_NKShipperCountryInfo, "The ‘Country’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				deciderTestContext.DisableRule(x => x.IsShipperCountryCheckSupported);
				tempBill.ABL_RN_NKShipperCountry = ZString.Empty;
				AssertNoMessageErrorContaining(tempBill.ABL_RN_NKShipperCountryInfo, "The ‘Country’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
			});
		}

		public void TestCheckABL_ShipperPostcode_ForENSIsReused()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsShipperPostcodeCheckSupported);
				deciderTestContext.EnableRule(x => x.IsShipperPostcodeMandatory);

				header.IsENSReuse = true;
				tempBill.Validation.ValidateABL_ShipperPostcode();
				AssertNoMessageErrorContaining(tempBill.ABL_ShipperPostcodeInfo, "The ‘Postcode’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				header.IsENSReuse = false;
				tempBill.Validation.ValidateABL_ShipperPostcode();
				AssertHasMessageError(tempBill.ABL_ShipperPostcodeInfo, "The ‘Postcode’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				tempBill.ABL_ShipperPostcode = "123456";
				AssertNoMessageErrorContaining(tempBill.ABL_ShipperPostcodeInfo, "The ‘Postcode’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				deciderTestContext.DisableRule(x => x.IsShipperPostcodeCheckSupported);
				tempBill.ABL_ShipperPostcode = ZString.Empty;
				AssertNoMessageErrorContaining(tempBill.ABL_ShipperPostcodeInfo, "The ‘Postcode’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
			});
		}

		public void TestCheckABL_ShipperPostcode_IsTransfer()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsShipperPostcodeCheckSupported);
				deciderTestContext.EnableRule(x => x.IsShipperPostcodeMandatory);

				header.IsENSReuse = false;
				tempBill.Validation.ValidateABL_ShipperPostcode();
				AssertHasMessageError(tempBill.ABL_ShipperPostcodeInfo, "The ‘Postcode’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
				tempBill.Validation.ValidateABL_ShipperPostcode();
				AssertNoMessageErrorContaining(tempBill.ABL_ShipperPostcodeInfo, "The ‘Postcode’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				deciderTestContext.DisableRule(x => x.IsShipperPostcodeCheckSupported);
				header.AMA_MessageType = ZString.Empty;
				tempBill.Validation.ValidateABL_ShipperPostcode();
				AssertNoMessageErrorContaining(tempBill.ABL_ShipperPostcodeInfo, "The ‘Postcode’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
			});
		}

		public void TestCheckABL_ShipperPostcode_IsDeconsolidation()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var masterBill = header.Bills.AddNew();
			masterBill.ABL_BolType = "BOL";
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsShipperPostcodeCheckSupported);
				deciderTestContext.EnableRule(x => x.IsShipperPostcodeMandatory);

				header.IsENSReuse = false;
				masterBill.Validation.ValidateABL_ShipperPostcode();
				AssertHasMessageError(masterBill.ABL_ShipperPostcodeInfo, "The ‘Postcode’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
				masterBill.Validation.ValidateABL_ShipperPostcode();
				AssertNoMessageErrorContaining(masterBill.ABL_ShipperPostcodeInfo, "The ‘Postcode’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				deciderTestContext.DisableRule(x => x.IsShipperPostcodeCheckSupported);
				header.AMA_MessageType = ZString.Empty;
				masterBill.Validation.ValidateABL_ShipperPostcode();
				AssertNoMessageErrorContaining(masterBill.ABL_ShipperPostcodeInfo, "The ‘Postcode’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
			});
		}

		public void TestValidateConsigneeOrgPK_ChecksForENSIsReused()
		{
			var (consignor, consignee) = SetUpOrganizationData();
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			((IBusinessObjectInternals)tempBill).IsCopying = true;
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsConsigneeOrgPKCheckSupported);
				deciderTestContext.EnableRule(x => x.IsConsigneeOrgPKMandatory);

				header.IsENSReuse = true;
				header.AMA_MessageType = string.Empty;
				tempBill.Validation?.ValidateConsigneeOrgPK();
				AssertNoMessageErrorContaining(tempBill.ConsigneeOrgPKInfo, "Consignee must be filled. Select an organization or fill a Consignee address in the ‘Bill Parties’ tab.");

				header.IsENSReuse = false;
				tempBill.Validation?.ValidateConsigneeOrgPK();
				AssertHasMessageError(tempBill.ConsigneeOrgPKInfo, "Consignee must be filled. Select an organization or fill a Consignee address in the ‘Bill Parties’ tab.");

				header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
				tempBill.Validation?.ValidateConsigneeOrgPK();
				AssertNoMessageError(tempBill.ConsigneeOrgPKInfo, "Consignee must be filled. Select an organization or fill a Consignee address in the ‘Bill Parties’ tab.");

				header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
				tempBill.Validation?.ValidateConsigneeOrgPK();
				AssertNoMessageError(tempBill.ConsigneeOrgPKInfo, "Consignee must be filled. Select an organization or fill a Consignee address in the ‘Bill Parties’ tab.");

				header.AMA_MessageType = string.Empty;
				tempBill.Validation?.ValidateConsigneeOrgPK();
				AssertHasMessageError(tempBill.ConsigneeOrgPKInfo, "Consignee must be filled. Select an organization or fill a Consignee address in the ‘Bill Parties’ tab.");

				tempBill.ConsigneeOrgPK = consignee.PK;
				tempBill.Validation?.ValidateConsigneeOrgPK();
				AssertNoMessageErrorContaining(tempBill.ConsigneeOrgPKInfo, "Consignee must be filled. Select an organization or fill a Consignee address in the ‘Bill Parties’ tab.");

				deciderTestContext.DisableRule(x => x.IsConsigneeOrgPKCheckSupported);
				header.IsENSReuse = false;
				tempBill.ConsigneeOrgPK = ZGuid.Empty;
				tempBill.Validation?.ValidateConsigneeOrgPK();
				AssertNoMessageErrorContaining(tempBill.ConsigneeOrgPKInfo, "Consignee must be filled. Select an organization or fill a Consignee address in the ‘Bill Parties’ tab.");
			});
		}

		public void TestValidateConsigneeOrgPK_IsTransfer()
		{
			var (consignor, consignee) = SetUpOrganizationData();
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			((IBusinessObjectInternals)tempBill).IsCopying = true;
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsConsigneeOrgPKCheckSupported);
				deciderTestContext.EnableRule(x => x.IsConsigneeOrgPKMandatory);

				header.IsENSReuse = false;
				tempBill.Validation?.ValidateConsigneeOrgPK();
				AssertHasMessageError(tempBill.ConsigneeOrgPKInfo, "Consignee must be filled. Select an organization or fill a Consignee address in the ‘Bill Parties’ tab.");
				header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
				tempBill.Validation?.ValidateConsigneeOrgPK();
				AssertNoMessageErrorContaining(tempBill.ConsigneeOrgPKInfo, "Consignee must be filled. Select an organization or fill a Consignee address in the ‘Bill Parties’ tab.");

				deciderTestContext.DisableRule(x => x.IsConsigneeOrgPKCheckSupported);
				header.AMA_MessageType = ZString.Empty;
				tempBill.Validation?.ValidateConsigneeOrgPK();
				AssertNoMessageErrorContaining(tempBill.ConsigneeOrgPKInfo, "Consignee must be filled. Select an organization or fill a Consignee address in the ‘Bill Parties’ tab.");
			});
		}

		public void TestValidateConsigneeOrgPK_IsDeconsolidation()
		{
			var (consignor, consignee) = SetUpOrganizationData();
			var header = Factory.New<TemporaryStorageHeader>();
			var masterBill = header.Bills.AddNew();
			masterBill.ABL_BolType = "BOL";
			((IBusinessObjectInternals)masterBill).IsCopying = true;
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsConsigneeOrgPKCheckSupported);
				deciderTestContext.EnableRule(x => x.IsConsigneeOrgPKMandatory);

				header.IsENSReuse = false;
				masterBill.Validation?.ValidateConsigneeOrgPK();
				AssertHasMessageError(masterBill.ConsigneeOrgPKInfo, "Consignee must be filled. Select an organization or fill a Consignee address in the ‘Bill Parties’ tab.");
				header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
				masterBill.Validation?.ValidateConsigneeOrgPK();
				AssertNoMessageErrorContaining(masterBill.ConsigneeOrgPKInfo, "Consignee must be filled. Select an organization or fill a Consignee address in the ‘Bill Parties’ tab.");

				deciderTestContext.DisableRule(x => x.IsConsigneeOrgPKCheckSupported);
				header.AMA_MessageType = ZString.Empty;
				masterBill.Validation?.ValidateConsigneeOrgPK();
				AssertNoMessageErrorContaining(masterBill.ConsigneeOrgPKInfo, "Consignee must be filled. Select an organization or fill a Consignee address in the ‘Bill Parties’ tab.");
			});
		}

		public void TestCheckABL_ConsigneeName_ForENSIsReused()
		{
			var (consignor, consignee) = SetUpOrganizationData();
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsConsigneeNameCheckSupported);
				deciderTestContext.EnableRule(x => x.IsConsigneeNameMandatory);

				header.IsENSReuse = true;
				tempBill.Validation.ValidateABL_ConsigneeName();
				AssertNoMessageErrorContaining(tempBill.ABL_ConsigneeNameInfo, "The ‘Name’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				header.IsENSReuse = false;
				tempBill.Validation.ValidateABL_ConsigneeName();
				AssertHasMessageError(tempBill.ABL_ConsigneeNameInfo, "The ‘Name’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				tempBill.ABL_ConsigneeName = "AAA";
				AssertNoMessageErrorContaining(tempBill.ABL_ConsigneeNameInfo, "The ‘Name’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				deciderTestContext.DisableRule(x => x.IsConsigneeNameCheckSupported);
				tempBill.ABL_ConsigneeName = ZString.Empty;
				AssertNoMessageErrorContaining(tempBill.ABL_ConsigneeNameInfo, "The ‘Name’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
			});
		}

		public void TestCheckABL_ConsigneeName_IsTransfer()
		{
			var (consignor, consignee) = SetUpOrganizationData();
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsConsigneeNameCheckSupported);
				deciderTestContext.EnableRule(x => x.IsConsigneeNameMandatory);

				header.IsENSReuse = false;
				tempBill.Validation.ValidateABL_ConsigneeName();
				AssertHasMessageError(tempBill.ABL_ConsigneeNameInfo, "The ‘Name’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
				tempBill.Validation.ValidateABL_ConsigneeName();
				AssertNoMessageErrorContaining(tempBill.ABL_ConsigneeNameInfo, "The ‘Name’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				deciderTestContext.DisableRule(x => x.IsConsigneeNameCheckSupported);
				header.AMA_MessageType = ZString.Empty;
				tempBill.Validation.ValidateABL_ConsigneeName();
				AssertNoMessageErrorContaining(tempBill.ABL_ConsigneeNameInfo, "The ‘Name’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
			});
		}

		public void TestCheckABL_ConsigneeName_IsDeconsolidation()
		{
			var (consignor, consignee) = SetUpOrganizationData();
			var header = Factory.New<TemporaryStorageHeader>();
			var masterBill = header.Bills.AddNew();
			masterBill.ABL_BolType = "BOL";
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsConsigneeNameCheckSupported);
				deciderTestContext.EnableRule(x => x.IsConsigneeNameMandatory);

				header.IsENSReuse = false;
				masterBill.Validation.ValidateABL_ConsigneeName();
				AssertHasMessageError(masterBill.ABL_ConsigneeNameInfo, "The ‘Name’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
				masterBill.Validation.ValidateABL_ConsigneeName();
				AssertNoMessageErrorContaining(masterBill.ABL_ConsigneeNameInfo, "The ‘Name’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				deciderTestContext.DisableRule(x => x.IsConsigneeNameCheckSupported);
				header.AMA_MessageType = ZString.Empty;
				masterBill.Validation.ValidateABL_ConsigneeName();
				AssertNoMessageErrorContaining(masterBill.ABL_ConsigneeNameInfo, "The ‘Name’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
			});
		}

		public void TestCheckABL_ConsigneeRegNoType_ForENSIsReused()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsConsigneeRegNoTypeCheckSupported);
				deciderTestContext.EnableRule(x => x.IsConsigneeRegNoTypeMandatory);

				header.IsENSReuse = true;
				tempBill.Validation.ValidateABL_ConsigneeRegNoType();
				AssertNoMessageErrorContaining(tempBill.ABL_ConsigneeRegNoTypeInfo, "The ‘Type of Person’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				header.IsENSReuse = false;
				tempBill.Validation.ValidateABL_ConsigneeRegNoType();
				AssertHasMessageError(tempBill.ABL_ConsigneeRegNoTypeInfo, "The ‘Type of Person’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				tempBill.ABL_ConsigneeRegNoType = TypeOfPersonList.Codes.NaturalPerson;
				AssertNoMessageErrorContaining(tempBill.ABL_ConsigneeRegNoTypeInfo, "The ‘Type of Person’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				deciderTestContext.DisableRule(x => x.IsConsigneeRegNoTypeCheckSupported);
				tempBill.ABL_ConsigneeRegNoType = ZString.Empty;
				AssertNoMessageErrorContaining(tempBill.ABL_ConsigneeRegNoTypeInfo, "The ‘Type of Person’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
			});
		}

		public void TestCheckABL_RN_NKConsigneeCountry()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				tempBill.ABL_RN_NKConsigneeCountry = "XX";
				AssertHasMessageErrorContaining("Value is not in the list.", tempBill.ABL_RN_NKConsigneeCountryInfo, ListValidation.InvalidCodeMessageError.ToString());
				tempBill.ABL_RN_NKConsigneeCountry = "US";
				AssertNoMessageErrorContaining("Value is in the list.", tempBill.ABL_RN_NKConsigneeCountryInfo, ListValidation.InvalidCodeMessageError.ToString());
			});
		}

		public void TestCheckABL_RN_NKConsigneeCountry_ForENSIsReused()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsConsigneeCountryCheckSupported);
				deciderTestContext.EnableRule(x => x.IsConsigneeCountryMandatory);

				header.IsENSReuse = true;
				tempBill.Validation.ValidateABL_RN_NKConsigneeCountry();
				AssertNoMessageErrorContaining(tempBill.ABL_RN_NKConsigneeCountryInfo, "The ‘Country’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				header.IsENSReuse = false;
				tempBill.Validation.ValidateABL_RN_NKConsigneeCountry();
				AssertHasMessageError(tempBill.ABL_RN_NKConsigneeCountryInfo, "The ‘Country’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				tempBill.ABL_RN_NKConsigneeCountry = "US";
				AssertNoMessageErrorContaining(tempBill.ABL_RN_NKConsigneeCountryInfo, "The ‘Country’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				deciderTestContext.DisableRule(x => x.IsConsigneeCountryCheckSupported);
				tempBill.ABL_RN_NKConsigneeCountry = ZString.Empty;
				AssertNoMessageErrorContaining(tempBill.ABL_RN_NKConsigneeCountryInfo, "The ‘Country’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
			});
		}

		public void TestCheckABL_RN_NKConsigneeCountry_IsTransfer()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsConsigneeCountryCheckSupported);
				deciderTestContext.EnableRule(x => x.IsConsigneeCountryMandatory);

				header.IsENSReuse = false;
				tempBill.Validation.ValidateABL_RN_NKConsigneeCountry();
				AssertHasMessageError(tempBill.ABL_RN_NKConsigneeCountryInfo, "The ‘Country’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
				tempBill.Validation.ValidateABL_RN_NKConsigneeCountry();
				AssertNoMessageErrorContaining(tempBill.ABL_RN_NKConsigneeCountryInfo, "The ‘Country’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				deciderTestContext.DisableRule(x => x.IsConsigneeCountryCheckSupported);
				header.AMA_MessageType = ZString.Empty;
				tempBill.Validation.ValidateABL_RN_NKConsigneeCountry();
				AssertNoMessageErrorContaining(tempBill.ABL_RN_NKConsigneeCountryInfo, "The ‘Country’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
			});
		}

		public void TestCheckABL_RN_NKConsigneeCountry_IsDeconsolidation()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var masterBill = header.Bills.AddNew();
			masterBill.ABL_BolType = "BOL";
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsConsigneeCountryCheckSupported);
				deciderTestContext.EnableRule(x => x.IsConsigneeCountryMandatory);

				header.IsENSReuse = false;
				masterBill.Validation.ValidateABL_RN_NKConsigneeCountry();
				AssertHasMessageError(masterBill.ABL_RN_NKConsigneeCountryInfo, "The ‘Country’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
				masterBill.Validation.ValidateABL_RN_NKConsigneeCountry();
				AssertNoMessageErrorContaining(masterBill.ABL_RN_NKConsigneeCountryInfo, "The ‘Country’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				deciderTestContext.DisableRule(x => x.IsConsigneeCountryCheckSupported);
				header.AMA_MessageType = ZString.Empty;
				masterBill.Validation.ValidateABL_RN_NKConsigneeCountry();
				AssertNoMessageErrorContaining(masterBill.ABL_RN_NKConsigneeCountryInfo, "The ‘Country’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
			});
		}

		public void TestCheckABL_ConsigneePostcode_ForENSIsReused()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsConsigneePostcodeCheckSupported);
				deciderTestContext.EnableRule(x => x.IsConsigneePostcodeMandatory);

				header.IsENSReuse = true;
				tempBill.Validation.ValidateABL_ConsigneePostcode();
				AssertNoMessageErrorContaining(tempBill.ABL_ConsigneePostcodeInfo, "The ‘Postcode’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				header.IsENSReuse = false;
				tempBill.Validation.ValidateABL_ConsigneePostcode();
				AssertHasMessageError(tempBill.ABL_ConsigneePostcodeInfo, "The ‘Postcode’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				tempBill.ABL_ConsigneePostcode = "123456";
				AssertNoMessageErrorContaining(tempBill.ABL_ConsigneePostcodeInfo, "The ‘Postcode’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				deciderTestContext.DisableRule(x => x.IsConsigneePostcodeCheckSupported);
				tempBill.ABL_ConsigneePostcode = ZString.Empty;
				AssertNoMessageErrorContaining(tempBill.ABL_ConsigneePostcodeInfo, "The ‘Postcode’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
			});
		}

		public void TestCheckABL_ConsigneePostcode_IsTransfer()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var tempBill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsConsigneePostcodeCheckSupported);
				deciderTestContext.EnableRule(x => x.IsConsigneePostcodeMandatory);

				header.IsENSReuse = false;
				tempBill.Validation.ValidateABL_ConsigneePostcode();
				AssertHasMessageError(tempBill.ABL_ConsigneePostcodeInfo, "The ‘Postcode’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
				tempBill.Validation.ValidateABL_ConsigneePostcode();
				AssertNoMessageErrorContaining(tempBill.ABL_ConsigneePostcodeInfo, "The ‘Postcode’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				deciderTestContext.DisableRule(x => x.IsConsigneePostcodeCheckSupported);
				header.AMA_MessageType = ZString.Empty;
				tempBill.Validation.ValidateABL_ConsigneePostcode();
				AssertNoMessageErrorContaining(tempBill.ABL_ConsigneePostcodeInfo, "The ‘Postcode’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
			});
		}

		public void TestCheckABL_ConsigneePostcode_IsDeconsolidation()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var masterBill = header.Bills.AddNew();
			masterBill.ABL_BolType = "BOL";
			CombineAssertions(() =>
			{
				var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				deciderTestContext.EnableRule(x => x.IsConsigneePostcodeCheckSupported);
				deciderTestContext.EnableRule(x => x.IsConsigneePostcodeMandatory);

				header.IsENSReuse = false;
				masterBill.Validation.ValidateABL_ConsigneePostcode();
				AssertHasMessageError(masterBill.ABL_ConsigneePostcodeInfo, "The ‘Postcode’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
				header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
				masterBill.Validation.ValidateABL_ConsigneePostcode();
				AssertNoMessageErrorContaining(masterBill.ABL_ConsigneePostcodeInfo, "The ‘Postcode’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

				deciderTestContext.DisableRule(x => x.IsConsigneePostcodeCheckSupported);
				header.AMA_MessageType = ZString.Empty;
				masterBill.Validation.ValidateABL_ConsigneePostcode();
				AssertNoMessageErrorContaining(masterBill.ABL_ConsigneePostcodeInfo, "The ‘Postcode’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
			});
		}

		public void TestCheckABL_GrossWeight()
		{
			TestABL_GrossWeightInfoNotification(false, PNTSMessageTypeList.Codes.PreLodgedTempStorage, expectMessageError: true);
			TestABL_GrossWeightInfoNotification(false, PNTSMessageTypeList.Codes.CombinedTemporaryStorage, expectMessageError: true);
			TestABL_GrossWeightInfoNotification(false, PNTSMessageTypeList.Codes.PresentationNotification, expectMessageError: false);
			TestABL_GrossWeightInfoNotification(true, PNTSMessageTypeList.Codes.PreLodgedTempStorage, expectMessageError: false);
			TestABL_GrossWeightInfoNotification(true, PNTSMessageTypeList.Codes.CombinedTemporaryStorage, expectMessageError: false);
			TestABL_GrossWeightInfoNotification(true, PNTSMessageTypeList.Codes.PresentationNotification, expectMessageError: false);

			void TestABL_GrossWeightInfoNotification(bool isENSReuse, string messageMode, bool expectMessageError)
			{
				using var deciderTestContext = new TemporaryStorageBillValidationDeciderTestContext<ITemporaryStorageBillValidationDecider>(Factory);
				var header = Factory.New<TemporaryStorageHeader>();

				CombineAssertions("When IsGrossWeightCheckSupported = true", () =>
				{
					var masterBill = header.MasterBill;
					deciderTestContext.EnableRule(x => x.IsGrossWeightCheckSupported);
					ValidationTestHelper.AssertErrorIfValueIsNegative(masterBill.ABL_GrossWeightInfo, "MasterBill.ABL_GrossWeight cannot be negative.");

					masterBill.ABL_GrossWeight = 0m;
					masterBill.Validation.ValidateABL_GrossWeight();
					AssertNoNotifications("MasterBill.ABL_GrossWeight is fine to be zero.", masterBill.ABL_GrossWeightInfo);

					var houseBill = header.Bills.AddNew();
					ValidationTestHelper.AssertErrorIfValueIsNegative(masterBill.ABL_GrossWeightInfo, "HouseBill.ABL_GrossWeight cannot be negative.");

					header.ENSReuse = isENSReuse ? (ZByte)1 : (ZByte)0;
					header.AMA_MessageType = messageMode;
					houseBill.ABL_GrossWeight = 0m;
					houseBill.Validation.ValidateABL_GrossWeight();
					if (expectMessageError)
					{
						AssertHasMessageErrorContaining("HouseBill.ABL_GrossWeight cannot be zero if header.IsENSReuse is true and messageMode is TC/TS", houseBill.ABL_GrossWeightInfo, MandatoryValidation.ValueCannotBeZero);
					}
					else
					{
						AssertNoMessageErrors(houseBill.ABL_GrossWeightInfo);
					}
				});

				CombineAssertions("When IsGrossWeightCheckSupported = false", () =>
				{
					deciderTestContext.DisableRule(x => x.IsGrossWeightCheckSupported);
					var masterBill = header.MasterBill;

					masterBill.ABL_GrossWeight = -1.23m;
					masterBill.Validation.ValidateABL_GrossWeight();
					AssertNoNotifications("Gross Mass cannot be negative.", masterBill.ABL_GrossWeightInfo);

					var houseBill = header.Bills.AddNew();

					header.ENSReuse = isENSReuse ? (ZByte)1 : (ZByte)0;
					header.AMA_MessageType = messageMode;
					houseBill.ABL_GrossWeight = 0m;
					houseBill.Validation.ValidateABL_GrossWeight();
					AssertNoMessageErrors(houseBill.ABL_GrossWeightInfo);
				});
			}
		}

		public void TestCheckABL_GrossWeightUQ()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var masterBill = header.MasterBill;
			masterBill.ABL_GrossWeightUQ = ZString.Empty;
			masterBill.Validation.ValidateAll();
			AssertNoNotifications(masterBill.ABL_GrossWeightUQInfo);

			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(masterBill.ABL_GrossWeightUQInfo, masterBill.ABL_GrossWeightInfo, expectedMessage: $"{MandatoryValidation.YouHaveNotEntered} a {masterBill.ABL_GrossWeightUQInfo.HumanReadableName}.");
		}
	}
}
