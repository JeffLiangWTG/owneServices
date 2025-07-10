using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class CUSPRLCusTempStorageLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTSL_UnionStatus()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(storageLine.TSL_UnionStatusInfo, "~", DEUnionStatusList.Codes.C);
		}

		public void TestCheckTSL_GrossWeight_NotZero()
		{
			ValidationTestHelper.AssertValueCannotBeZeroMessageError(storageLine.TSL_GrossWeightInfo);
		}

		public void TestCheckTSL_GrossWeight_MaxAllowed()
		{
			const string messageError = "Maximum value allowed is 99999999999.999";
			CombineAssertions(() =>
			{
				storageLine.TSL_GrossWeight = 99999999999.999m;
				AssertNoMessageError("Upper bound", storageLine.TSL_GrossWeightInfo, messageError);

				storageLine.TSL_GrossWeight = 999999999991m;
				AssertHasMessageError("Too big", storageLine.TSL_GrossWeightInfo, messageError);
			});
		}

		public void TestCheckTSL_PackageQty_Between()
		{
			CusTempStorageLineValidationTestHelper.AssertPackageQtyIsBetween1And99999(storageLine.TSL_PackageQtyInfo);
		}

		public void TestCheckTSL_PackageQty_IsULDRequiresPackageCountToBe1()
		{
			CusTempStorageLineValidationTestHelper.AssertPackageQtyIs1IfIsULD(storageLine.TSL_PackageQtyInfo, () => storageLine.Validation.ValidateTSL_PackageQty());
		}

		public void TestCheckTSL_PackageQty_IsSingleCountPackageTypeRequiresPackageCountToBe1()
		{
			CusTempStorageLineValidationTestHelper.AssertPackageQtyIs1IfIsSingleCountPackageType(storageLine.TSL_PackageQtyInfo);
		}

		public void TestCheckTSL_RN_NKDepartureCountry()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_RN_NKDepartureCountryInfo);
		}

		public void TestCheckTSL_GoodsDescription()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_GoodsDescriptionInfo);
		}

		public void TestCheckTSL_OwnerReferenceNumber_OwnerReferenceTypeNotEmpty()
		{
			storageLine.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.AWB;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_OwnerReferenceNumberInfo);
		}

		public void TestCheckTSL_OwnerReferenceNumber_OwnerReferenceTypeEmpty()
		{
			storageLine.TSL_OwnerReferenceType = ZString.Empty;
			ValidationTestHelper.AssertFieldIsNotMandatory(storageLine.TSL_OwnerReferenceNumberInfo);
		}

		public void TestCheckTSL_ReferenceNumber_MRNFormat_ESUMA()
		{
			AssertReferenceNumberMRNFormat(
				PreviousReferenceType.Codes._ESUMA,
				storageLine.TSL_ReferenceNumberInfo,
				() => storageLine.Validation.ValidateTSL_ReferenceNumber(),
				"When SumA previous reference type is 'ESUMA', 'ENST2L' or 'N355',");
		}

		public void TestCheckTSL_ReferenceNumber_Mandatory_ESUMA()
		{
			AssertReferenceNumberMandatory(PreviousReferenceType.Codes._ESUMA, storageLine.TSL_ReferenceNumberInfo);
		}

		public void TestCheckTSL_ReferenceNumber_MRNFormat_ENST2L()
		{
			AssertReferenceNumberMRNFormat(
				PreviousReferenceType.Codes._ENST2L,
				storageLine.TSL_ReferenceNumberInfo,
				() => storageLine.Validation.ValidateTSL_ReferenceNumber(),
				"When SumA previous reference type is 'ESUMA', 'ENST2L' or 'N355',");
		}

		public void TestCheckTSL_ReferenceNumber_Mandatory_ENST2L()
		{
			AssertReferenceNumberMandatory(PreviousReferenceType.Codes._ENST2L, storageLine.TSL_ReferenceNumberInfo);
		}

		public void TestCheckTSL_ReferenceNumber_MRNFormat_N355()
		{
			AssertReferenceNumberMRNFormat(
				PreviousReferenceType.Codes._N355,
				storageLine.TSL_ReferenceNumberInfo,
				() => storageLine.Validation.ValidateTSL_ReferenceNumber(),
				"When SumA previous reference type is 'ESUMA', 'ENST2L' or 'N355',");
		}

		public void TestCheckTSL_ReferenceNumber_Mandatory_N355()
		{
			AssertReferenceNumberMandatory(PreviousReferenceType.Codes._N355, storageLine.TSL_ReferenceNumberInfo);
		}

		public void TestCheckTSL_ReferenceNumberLine_Mandatory_ESUMA()
		{
			AssertReferenceNumberLineMandatory(PreviousReferenceType.Codes._ESUMA, storageLine.TSL_ReferenceNumberLineInfo);
		}

		public void TestCheckTSL_ReferenceNumberLine_Mandatory_ENST2L()
		{
			AssertReferenceNumberLineMandatory(PreviousReferenceType.Codes._ENST2L, storageLine.TSL_ReferenceNumberLineInfo);
		}

		public void TestCheckTSL_ReferenceNumberLine_NoMandatory_N355()
		{
			AssertReferenceNumberLineNoMandatory(PreviousReferenceType.Codes._N355, storageLine.TSL_ReferenceNumberLineInfo);
		}

		public void TestCheckTSL_CustodianIdentifierBranchNoMandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_CustodianIdentifierBranchNoInfo);
		}

		public void TestCheckTSL_GoodsOwnerIdentifierBranchNoMandatory()
		{
			CombineAssertions(() =>
			{
				storageLine.Validation.ValidateTSL_GoodsOwnerIdentifierBranchNo();
				AssertNoNotifications("Empty Identifier and branch", storageLine.TSL_GoodsOwnerIdentifierBranchNoInfo);

				storageLine.TSL_GoodsOwnerIdentifier = "DE12345";
				AssertNoNotifications("Entered Identifier, empty branch", storageLine.TSL_GoodsOwnerIdentifierBranchNoInfo);
			});
		}

		public void TestCheckTSL_LineNo()
		{
			const string messageError = "Line No. 1 already exists for this declaration";
			CombineAssertions(() =>
			{
				storageLine.TSL_LineNo = 1;
				var storageLine2 = storageDec.CusTempStorageLines.AddNew();
				storageLine2.TSL_LineNo = 1;
				AssertHasMessageError("Not unique", storageLine2.TSL_LineNoInfo, messageError);

				storageLine2.TSL_LineNo = 2;
				AssertNoMessageError("Unique", storageLine2.TSL_LineNoInfo, messageError);
			});
		}

		public void TestCheckTSL_PackageType()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_PackageTypeInfo);
		}

		public void TestCheckTSL_ReferenceNumber2_MRNFormat_ENST2L()
		{
			AssertReferenceNumberMRNFormat(
				PreviousReferenceType.Codes._ENST2L,
				storageLine.TSL_ReferenceNumber2Info,
				() => storageLine.Validation.ValidateTSL_ReferenceNumber2(),
				"When SumA previous reference type is 'ENST2L', 'POUS',");
		}

		public void TestCheckTSL_ReferenceNumber2_Mandatory_ENST2L()
		{
			AssertReferenceNumberMandatory(PreviousReferenceType.Codes._ENST2L, storageLine.TSL_ReferenceNumber2Info);
		}

		public void TestCheckTSL_ReferenceNumber2_MRNFormat_POUS()
		{
			AssertReferenceNumberMRNFormat(
				PreviousReferenceType.Codes._POUS,
				storageLine.TSL_ReferenceNumber2Info,
				() => storageLine.Validation.ValidateTSL_ReferenceNumber2(),
				"When SumA previous reference type is 'ENST2L', 'POUS',");
		}

		public void TestCheckTSL_ReferenceNumber2_Mandatory_POUS()
		{
			AssertReferenceNumberMandatory(PreviousReferenceType.Codes._POUS, storageLine.TSL_ReferenceNumber2Info);
		}

		public void TestCheckTSL_ReferenceNumber2Line_Mandatory_ENST2L()
		{
			AssertReferenceNumberLineMandatory(PreviousReferenceType.Codes._ENST2L, storageLine.TSL_ReferenceNumber2LineInfo);
		}

		public void TestCheckTSL_ReferenceNumber2Line_Mandatory_POUS()
		{
			AssertReferenceNumberLineMandatory(PreviousReferenceType.Codes._POUS, storageLine.TSL_ReferenceNumber2LineInfo);
		}

		public void TestCheckTSL_TransportNumberType_MandatoryNotN355()
		{
			storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._199;
			storageLine.TSL_TransportNumber = "123";
			storageLine.Receptacle = "123";
			ValidationTestHelper.AssertFieldIsNotMandatory(storageLine.TSL_TransportNumberTypeInfo, MandatoryValidation.YouHaveNotEntered, "SJH_PreviousReferenceType isn't N355");
		}

		public void TestCheckTSL_TransportNumberType_MandatoryN355()
		{
			storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._N355;
			storageLine.TSL_TransportNumber = "123";
			storageLine.Receptacle = "123";
			ValidationTestHelper.AssertFieldIsNotMandatory(storageLine.TSL_TransportNumberTypeInfo, MandatoryValidation.YouHaveNotEntered, "SJH_PreviousReferenceType is N355");
		}

		public void TestCheckTSL_TransportNumberType_MandatoryN355WithEmptyTransportNumber()
		{
			storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._N355;

			storageLine.TSL_TransportNumber = "";
			storageLine.Receptacle = "123";
			ValidationTestHelper.AssertFieldIsNotMandatory(storageLine.TSL_TransportNumberTypeInfo, MandatoryValidation.YouHaveNotEntered, "TSL_TransportNumber is empty, Receptacle is non-empty");
		}

		public void TestCheckTSL_TransportNumberType_Mandatory()
		{
			storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._N355;
			storageLine.TSL_TransportNumber = "123";
			storageLine.Receptacle = "";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_TransportNumberTypeInfo, MandatoryValidation.YouHaveNotEntered, "SJH_PreviousReferenceType is N355, TSL_TransportNumber is non-empty and Receptacle is empty");
		}

		public void TestCheckTSL_TransportNumberType_ListValidation()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			storageHeader.SJH_OH_Customer = organisation.PK;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_C0754, "C0754");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_C0754, "DE01", "DE01 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(storageLine.TSL_TransportNumberTypeInfo, "XXX", "DE01");
		}

		public void TestCheckTSL_TransportNumber_EmptyTSL_TransportNumberType()
		{
			storageLine.TSL_TransportNumberType = ZString.Empty;
			ValidationTestHelper.AssertFieldIsNotMandatory(storageLine.TSL_TransportNumberInfo, MandatoryValidation.YouHaveNotEntered, "TSL_TransportNumberType is empty");
		}

		public void TestCheckTSL_TransportNumber_NotEmptyTSL_TransportNumberType()
		{
			storageLine.TSL_TransportNumberType = "XXX";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_TransportNumberInfo, MandatoryValidation.YouHaveNotEntered, "TSL_TransportNumberType isn't empty");
		}

		protected override void SetUp()
		{
			base.SetUp();
			storageHeader = Factory.New<CusTempStorageJobHeader>();
			storageDec = CUSPRLCusTempStorageDec.LoadOrCreate(storageHeader);
			storageLine = storageDec.CusTempStorageLines.AddNew();
		}
		CusTempStorageJobHeader storageHeader;
		CUSPRLCusTempStorageLine storageLine;
		CUSPRLCusTempStorageDec storageDec;

		void AssertReferenceNumberMandatory(ZString previousReferenceType, ZPropertyInfo referenceNumberInfo)
		{
			CombineAssertions(() =>
			{
				var storageHeader = storageLine.Dec.StorageHeader;
				storageHeader.SJH_PreviousReferenceType = previousReferenceType;
				storageHeader.SJH_PreviousReferenceNumber = ZString.Empty;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(referenceNumberInfo, MandatoryValidation.YouHaveNotEntered, GetTestCase());

				storageHeader.SJH_PreviousReferenceNumber = "11DE11111111111115";
				ValidationTestHelper.AssertIfIsEnteredMessageError(referenceNumberInfo, MandatoryValidation.DoNotEntered, GetTestCase());
			});
			string GetTestCase() => $"PreviousReferenceType = '{storageHeader.SJH_PreviousReferenceType}'; PreviousReferenceNumber = '{storageHeader.SJH_PreviousReferenceNumber}'";
		}

		void AssertReferenceNumberMRNFormat(ZString previousReferenceType, ZPropertyInfo referenceNumberInfo, Action validator, ZString mrnValidationMessage)
		{
			CombineAssertions(() =>
			{
				validator();
				AssertNoMessageErrorContaining(GetTestCase(), referenceNumberInfo, mrnValidationMessage);

				storageHeader.SJH_PreviousReferenceType = previousReferenceType;
				validator();
				AssertNoMessageErrorContaining(GetTestCase(), referenceNumberInfo, mrnValidationMessage);

				referenceNumberInfo.Value = (ZString)"INVALIDCODE";
				AssertHasMessageErrorContaining(GetTestCase(), referenceNumberInfo, mrnValidationMessage);

				referenceNumberInfo.Value = (ZString)"11DE11111111111115";
				AssertNoMessageErrorContaining(GetTestCase(), referenceNumberInfo, mrnValidationMessage);
			});
			string GetTestCase() => $"PreviousReferenceType = '{storageHeader.SJH_PreviousReferenceType}'; ReferenceNumber = '{referenceNumberInfo.Value}'";
		}

		void AssertReferenceNumberLineMandatory(ZString previousReferenceType, ZPropertyInfo referenceNumberInfo)
		{
			storageLine.Dec.StorageHeader.SJH_PreviousReferenceType = previousReferenceType;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(referenceNumberInfo, MandatoryValidation.YouHaveNotEntered, $"PreviousReferenceType = {previousReferenceType}");
		}

		void AssertReferenceNumberLineNoMandatory(ZString previousReferenceType, ZPropertyInfo referenceNumberInfo)
		{
			storageLine.Dec.StorageHeader.SJH_PreviousReferenceType = previousReferenceType;
			ValidationTestHelper.AssertFieldIsNotMandatory(referenceNumberInfo, MandatoryValidation.YouHaveNotEntered, $"PreviousReferenceType = {previousReferenceType}");
		}
	}
}
