using System;
using System.Runtime.CompilerServices;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.CH.NCTS.Business.UniversalReferenceConstants;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsDepartureMovementHeaderValidation))]
sealed class NctsDepartureMovementHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckBM_PaperlessInbondNum()
	{
		const string errorMessage = "Company or Branch must have a Business Partner ID (BID) in order to generate the LRN number.";

		CombineAssertions(() =>
		{
			using (EU.Registry.EUCustomsDataRegistry.Instance.NctsIsManualDepartureCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				DepartureMovement.Validation.ValidateBM_PaperlessInbondNum();
				AssertNoMessageError("Departure Customer Reference is not auto generated", DepartureMovement.BM_PaperlessInbondNumInfo, errorMessage);
			}

			using (EU.Registry.EUCustomsDataRegistry.Instance.NctsIsManualDepartureCustomerReferenceEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				DepartureMovement.Validation.ValidateBM_PaperlessInbondNum();
				AssertHasMessageError("Company or Branch does not have a Business Partner ID (BID)", DepartureMovement.BM_PaperlessInbondNumInfo, errorMessage);

				DepartureMovement.BM_PaperlessInbondNum = "123";
				AssertNoMessageError("Departure Customer Reference has been generated", DepartureMovement.BM_PaperlessInbondNumInfo, errorMessage);

				DepartureMovement.BM_PaperlessInbondNum = ZString.Empty;
				GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SwissCodeTypes.BID, "1000088059", Core.Constants.CountryCodes.Switzerland);
				DepartureMovement.Validation.ValidateBM_PaperlessInbondNum();
				AssertNoMessageError("Company or Branch has a Business Partner ID (BID)", DepartureMovement.BM_PaperlessInbondNumInfo, errorMessage);
			}
		});
	}

	public void TestCheckBM_InBondEntryType()
	{
		new RefDataTestHelper(Factory).CreateNctsDeclarationTypeList();
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(DepartureMovement.BM_InBondEntryTypeInfo, "X", NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland);
	}

	public void TestCheckBM_InBondEntryType_NS30068() => CombineAssertions(() =>
	{
		const string messageError = "[NS30068] You need to supply at least one House Consignment or an Export Declaration Reference.";

		AssertMessage(true);
		AssertMessage(false, hasBill: true);
		AssertMessage(false, hasAttachedDelaration: true);
		AssertMessage(false, hasExportDocument: true);
		AssertMessage(true, hasOtherDocument: true);

		void AssertMessage(bool expectMessage, bool hasBill = false, bool hasAttachedDelaration = false, bool hasExportDocument = false, bool hasOtherDocument = false, [CallerLineNumber] int lineNumber = 0)
		{
			DepartureMovement.Header.Bills.DeleteAll();
			if (hasBill)
			{
				DepartureMovement.Header.Bills.AddNew();
			}
			DepartureMovement.RelatedExportEntryHeaders.RemoveAndDeleteAll();
			if (hasAttachedDelaration)
			{
				departureMovement.RelatedExportEntryHeaders.AddNew();
			}
			DepartureMovement.Header.PreviousDocuments.RemoveAndDeleteAll();
			if (hasOtherDocument)
			{
				DepartureMovement.Header.PreviousDocuments.AddNew();
			}
			if (hasExportDocument)
			{
				DepartureMovement.Header.PreviousDocuments.AddNew().CSI_Code = PreviousDocumentCodes.Export;
			}
			DepartureMovement.Validation.ValidateBM_InBondEntryType();
			var assertionMessage = $"[{lineNumber}] HasBill={hasBill} HasAttachedDeclaration={hasAttachedDelaration} HasExportDocument={hasExportDocument} HasOtherDocument={hasOtherDocument}";
			if (expectMessage)
			{
				AssertHasMessageError(assertionMessage, DepartureMovement.BM_InBondEntryTypeInfo, messageError);
			}
			else
			{
				AssertNoMessageError(assertionMessage, DepartureMovement.BM_InBondEntryTypeInfo, messageError);
			}
		}
	});

	public void TestCheckBM_InBondEntryType_NS30163() => CombineAssertions(() =>
	{
		var messageErrorNS30163 = PassarValidationMessages.MessageNS30163NP70176(PassarValidationMessages.NS30163, SwissCustomsConstants.Limits.HouseConsignmentCardinalityLimitV4);

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;

		for (int i = 0; i < 50; ++i)
		{
			nctsHeader.Bills.AddNew();
		}
		for (int i = 0; i < 25; ++i)
		{
			var doc = nctsHeader.PreviousDocuments.AddNew();
			doc.CSI_Code = PreviousDocumentCodes.Export;
		}
		for (int i = 0; i < 24; ++i)
		{
			nctsHeader.MovementHeader.RelatedExportEntryHeaders.AddNew();
		}

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.CHNT515V4, Core.Constants.CountryCodes.Switzerland, ZDate.Today, true))
		{
			nctsHeader.MovementHeader.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("Limit 99 achieved but not exceeded", nctsHeader.MovementHeader.BM_InBondEntryTypeInfo, messageErrorNS30163);
			var doc200 = nctsHeader.PreviousDocuments.AddNew();
			nctsHeader.MovementHeader.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("Limit 99 exceeded with non EXPO previous document", nctsHeader.MovementHeader.BM_InBondEntryTypeInfo, messageErrorNS30163);
			doc200.CSI_Code = PreviousDocumentCodes.Export;
			nctsHeader.MovementHeader.Validation.ValidateBM_InBondEntryType();
			AssertHasMessageError("Limit 99 exceeded with EXPO previous document", nctsHeader.MovementHeader.BM_InBondEntryTypeInfo, messageErrorNS30163);
		}

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.CHNT515V4, Core.Constants.CountryCodes.Switzerland, ZDate.Today, false))
		{
			nctsHeader.MovementHeader.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("CHNT015V4 not active", nctsHeader.MovementHeader.BM_InBondEntryTypeInfo, messageErrorNS30163);
		}
	});

	public void TestCheckBM_InBondEntryType_NP70136() => CombineAssertions(() =>
	{
		var messageErrorNS70136 = PassarValidationMessages.MessageNS30163NP70176(PassarValidationMessages.NP70176, SwissCustomsConstants.Limits.HouseConsignmentCardinalityLimitV5);

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;

		for (int i = 0; i < 1000; ++i)
		{
			nctsHeader.Bills.AddNew();
		}
		for (int i = 0; i < 500; ++i)
		{
			var doc = nctsHeader.PreviousDocuments.AddNew();
			doc.CSI_Code = PreviousDocumentCodes.Export;
		}
		for (int i = 0; i < 499; ++i)
		{
			nctsHeader.MovementHeader.RelatedExportEntryHeaders.AddNew();
		}

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.CHNT515V4, Core.Constants.CountryCodes.Switzerland, ZDate.Today, false))
		{
			nctsHeader.MovementHeader.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("Limit 1999 achieved but not exceeded", nctsHeader.MovementHeader.BM_InBondEntryTypeInfo, messageErrorNS70136);
			var doc2000 = nctsHeader.PreviousDocuments.AddNew();
			nctsHeader.MovementHeader.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("Limit 1999 exceeded with non EXPO previous document", nctsHeader.MovementHeader.BM_InBondEntryTypeInfo, messageErrorNS70136);
			doc2000.CSI_Code = PreviousDocumentCodes.Export;
			nctsHeader.MovementHeader.Validation.ValidateBM_InBondEntryType();
			AssertHasMessageError("Limit 1999 exceeded with EXPO previous document", nctsHeader.MovementHeader.BM_InBondEntryTypeInfo, messageErrorNS70136);
		}

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.CHNT515V4, Core.Constants.CountryCodes.Switzerland, ZDate.Today, true))
		{
			nctsHeader.MovementHeader.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("CHNT015V4 active", nctsHeader.MovementHeader.BM_InBondEntryTypeInfo, messageErrorNS70136);
		}
	});

	public void TestCheckBM_InBondEntryType_CheckCustomsOffices() => CombineAssertions(() =>
	{
		const string expectedError = "The declaration requires an office of type NCTS Office of transit with purpose TRA.";
		new RefDataTestHelper(Factory).CreateNctsDeclarationTypeList();

		DepartureMovement.Validation.ValidateBM_InBondEntryType();
		AssertHasMessageError("Initial state", DepartureMovement.BM_InBondEntryTypeInfo, expectedError);

		DepartureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
		AssertNoMessageError($"BM_InBondEntryType = '{DepartureMovement.BM_InBondEntryType}' and 'TRA' missing", DepartureMovement.BM_InBondEntryTypeInfo, expectedError);

		DepartureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;
		AssertHasMessageError($"BM_InBondEntryType = '{DepartureMovement.BM_InBondEntryType}' and 'TRA' missing", DepartureMovement.BM_InBondEntryTypeInfo, expectedError);

		DepartureMovement.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).CY_Data = "CH012345";
		DepartureMovement.Validation.ValidateBM_InBondEntryType();
		AssertNoMessageError($"BM_InBondEntryType = '{DepartureMovement.BM_InBondEntryType}' and 'TRA' available", DepartureMovement.BM_InBondEntryTypeInfo, expectedError);
	});

	public void TestCheckBM_InBondEntryType_NP70278() => CombineAssertions(() =>
	{
		for (var i = 0; i < 500; ++i)
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.MergedLines.AddNew();
			entryHeader.MergedLines.AddNew();
			DepartureMovement.RelatedExportEntryHeaders.AddPivotFor(entryHeader);
		}

		for (var i = 0; i < 499; ++i)
		{
			var bill = DepartureMovement.Header.Bills.AddNew();
			bill.GoodsItems.AddNew();
			bill.GoodsItems.AddNew();
		}

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CHNT015V4, "CH", ZDate.Today, false))
		{
			DepartureMovement.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("Limit 1999 achieved but not exceeded - CHNT015V4 period ended", DepartureMovement.BM_InBondEntryTypeInfo, PassarValidationMessages.MessageNP70278);
			var bill = DepartureMovement.Header.Bills.AddNew();
			bill.GoodsItems.AddNew();
			bill.GoodsItems.AddNew();
			DepartureMovement.Validation.ValidateBM_InBondEntryType();
			AssertHasMessageError("Limit 1999 exceeded - CHNT015V4 period ended", DepartureMovement.BM_InBondEntryTypeInfo, PassarValidationMessages.MessageNP70278);
		}
		ZDate tomorrow = new ZDate(DateTime.Today + TimeSpan.FromDays(1));
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CHNT015V4, "CH", ZDate.Today, true))
		{
			DepartureMovement.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageError("Limit 1999 exceeded - CHNT015V4 period in progress", DepartureMovement.BM_InBondEntryTypeInfo, PassarValidationMessages.MessageNP70278);
		}
	});

	public void TestCheckBM_SpecificCircumstance()
	{
		new RefDataTestHelper(Factory).CreateGlobalCodeN0296List();
		ValidationTestHelper.AssertInvalidCodeMessageError(DepartureMovement.BM_SpecificCircumstanceInfo, "AAA", "A20");
	}

	public void TestCheckBM_TypeOfSecurity() => CombineAssertions(() =>
	{
		AssertBM_TypeOfSecurity("TIR - mrn empty - ENT", NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration, ZString.Empty, false, NctsTypeOfSecurityList.Codes.ENT);
		AssertBM_TypeOfSecurity("TIR - mrn with J - NON", NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration, "21CH116360164625J6.1", true, NctsTypeOfSecurityList.Codes.NON);
		AssertBM_TypeOfSecurity("TIR - mrn with K - EXI", NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration, "21CH116360164625K6.1", true, NctsTypeOfSecurityList.Codes.EXI);
		AssertBM_TypeOfSecurity("TIR - mrn with L - ENT", NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration, "21CH116360164625L6.1", true, NctsTypeOfSecurityList.Codes.ENT);
		AssertBM_TypeOfSecurity("TIR - mrn with M - BTH", NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration, "21CH116360164625M6.1", true, NctsTypeOfSecurityList.Codes.BTH);
		AssertBM_TypeOfSecurity("TIR - mrn with N - BTH", NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration, "21CH116360164625N6.1", false, NctsTypeOfSecurityList.Codes.BTH);
		AssertBM_TypeOfSecurity("T-CH - mrn with J - NON", NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland, "21CH116360164625J6.1", false, NctsTypeOfSecurityList.Codes.NON);
	});

	void AssertBM_TypeOfSecurity(string hint, string typeOfDeclaration, ZString mrn, bool shouldHave, string typeOfSecurityCode)
	{
		DepartureMovement.BM_InBondEntryType = typeOfDeclaration;
		DepartureMovement.Header.MovementReferenceNumberSetter(mrn);
		if (shouldHave)
		{
			DepartureMovement.BM_TypeOfSecurity = ZString.Empty;
			AssertHasMessageError($"{hint} - error", DepartureMovement.BM_TypeOfSecurityInfo, PassarValidationMessages.MessageNP70061(typeOfSecurityCode));
		}
		DepartureMovement.BM_TypeOfSecurity = typeOfSecurityCode;
		AssertNoMessageErrorContaining($"{hint} - no error", DepartureMovement.BM_TypeOfSecurityInfo, PassarValidationMessages.MessageNP70061(typeOfSecurityCode));
	}

	public void TestCheckBM_ActiveBorderIdentificationType()
	{
		DepartureMovement.BM_ExportTransportMode = ZString.Empty;
		DepartureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
		ValidationTestHelper.AssertFieldIsNotMandatory(DepartureMovement.BM_ActiveBorderIdentificationTypeInfo);

		DepartureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		DepartureMovement.Validation.ValidateBM_ActiveBorderIdentificationType();
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(DepartureMovement.BM_ActiveBorderIdentificationTypeInfo);
	}

	public void TestBM_RN_NKTOLCarrierNationality()
	{
		DepartureMovement.BM_ExportTransportMode = ZString.Empty;
		DepartureMovement.Validation.ValidateBM_RN_NKTOLCarrierNationality();
		ValidationTestHelper.AssertFieldIsNotMandatory(DepartureMovement.BM_RN_NKTOLCarrierNationalityInfo);

		DepartureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		DepartureMovement.Validation.ValidateBM_RN_NKTOLCarrierNationality();
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(DepartureMovement.BM_RN_NKTOLCarrierNationalityInfo);
	}

	public void TestCheckMandatoryGuaranteeIfNeeded()
	{
		const string errorMessage = "You need to supply a valid guarantee.";

		CombineAssertions("When ShouldCheckGuarantees", () =>
		{
			DepartureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			AssertHasMessageErrorContaining("No Guarantees, InBondEntryType != T-CH", DepartureMovement.BM_InBondEntryTypeInfo, errorMessage);

			DepartureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
			AssertNoMessageError("No Guarantees, InBondEntryType = T-CH", DepartureMovement.BM_InBondEntryTypeInfo, errorMessage);
		});
	}

	public void TestCheckBM_ExportTimeLimit() => CombineAssertions(() =>
	{
		AssertNoMessageError("Initial state", DepartureMovement.BM_ExportTimeLimitInfo, PassarValidationMessages.MessageNP70123);

		DepartureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
		DepartureMovement.BM_ExportTimeLimit = 11;
		AssertNoMessageError($"BM_InBondEntryType='{DepartureMovement.BM_InBondEntryType}'; BM_ExportTimeLimit='{DepartureMovement.BM_ExportTimeLimit}'", DepartureMovement.BM_ExportTimeLimitInfo, PassarValidationMessages.MessageNP70123);

		DepartureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
		DepartureMovement.Validation.ValidateBM_ExportTimeLimit();
		AssertHasMessageError($"BM_InBondEntryType='{DepartureMovement.BM_InBondEntryType}'; BM_ExportTimeLimit='{DepartureMovement.BM_ExportTimeLimit}'", DepartureMovement.BM_ExportTimeLimitInfo, PassarValidationMessages.MessageNP70123);

		DepartureMovement.BM_ExportTimeLimit = 10;
		AssertNoMessageError($"BM_InBondEntryType='{DepartureMovement.BM_InBondEntryType}'; BM_ExportTimeLimit='{DepartureMovement.BM_ExportTimeLimit}'", DepartureMovement.BM_ExportTimeLimitInfo, PassarValidationMessages.MessageNP70123);
	});

	NctsDepartureMovementHeader CreateNctsDepartureMovementHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader.MovementHeader;
	}

	NctsDepartureMovementHeader DepartureMovement => departureMovement ?? (departureMovement = CreateNctsDepartureMovementHeader());
	NctsDepartureMovementHeader departureMovement;
}
