using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class ImportJobDeclarationGoodsLocationTest : TestCaseWithFactory
{
	public void TestCheckJE_LocationOfGoods_WhenQualifierIsD()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eun);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT0001", "Office One", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT0002", "Office Two", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		Factory.Save();

		CombineAssertions("When ZG_AuthorisationNumber is empty", () =>
		{
			declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreInCustomsAreaForInspection;
			declaration.JE_LocationOfGoods = ZString.Empty;
			AssertHasMessageErrorContaining("When JE_LocationOfGoods is empty there must be a message error", declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_LocationOfGoods = "YYYYYY";
			AssertHasMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_LocationOfGoods = "IT0001";
			AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);

			var expectedMessage = "Goods Location Customs Office is different from Customs Office of Presentation";
			declaration.JE_CustomsOffice = "IT0002";
			declaration.JE_LocationOfGoods = "IT0001";
			AssertHasWarningContaining("Customs Office must be equal to Office of presentation", declaration.JE_LocationOfGoodsInfo, expectedMessage);

			declaration.JE_LocationOfGoods = "IT0002";
			AssertNoWarningContaining("When Customs Office is equal to Office of presentation, no error is expected", declaration.JE_LocationOfGoodsInfo, expectedMessage);
		});

		CombineAssertions("When ZG_AuthorisationNumber is not empty", () =>
		{
			declaration.ZG_AuthorisationNumber = "1234";
			declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreInCustomsAreaForInspection;
			declaration.JE_LocationOfGoods = ZString.Empty;
			AssertNoMessageErrorContaining("When Authorisation Number is filled, no error for empty JE_LocationOfGoods", declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_LocationOfGoods = "YYYYYY";
			AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);

			var message = "Goods Location Customs Office is different from Customs Office of Presentation";
			declaration.JE_CustomsOffice = "IT0002";
			declaration.JE_LocationOfGoods = "IT0001";
			AssertNoWarningContaining("When Authorisation Number is filled, no error for JE_LocationOfGoods different from Office of presentation", declaration.JE_LocationOfGoodsInfo, message);
		});
	}

	public void TestCheckJE_LocationOfGoods_WhenQualifierIsFC()
	{
		declaration.ZG_AuthorisationNumber = ZString.Empty;
		declaration.JE_LocationQualifier = ZString.Empty;
		declaration.Validation.ValidateJE_LocationOfGoods();
		AssertNoMessageErrors("When ZG_AuthorisationNumber and JE_LocationQualifier are empty there shouldn't be any message error", declaration.JE_LocationOfGoodsInfo);

		declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuit;
		declaration.Validation.ValidateJE_LocationOfGoods();
		AssertNoMessageErrorContaining("When ZG_AuthorisationNumber is empty and  JE_LocationQualifier is 'F', there shouldn't be any message error", declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

		CombineAssertions("When ZG_AuthorisationNumber is empty", () =>
		{
			declaration.ZG_AuthorisationNumber = ZString.Empty;
			declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation;
			declaration.Validation.ValidateJE_LocationOfGoods();
			AssertHasMessageErrorContaining("When ZG_AuthorisationNumber is empty, JE_LocationQualifier is 'FC', JE_LocationOfGoods shouldn't be empty ", declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_LocationOfGoods = "123";
			AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);
		});

		CombineAssertions("When ZG_AuthorisationNumber is not empty", () =>
		{
			declaration.ZG_AuthorisationNumber = "1234";
			declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation;
			declaration.Validation.ValidateJE_LocationOfGoods();
			AssertNoMessageErrorContaining("When ZG_AuthorisationNumber is filled, JE_LocationQualifier is 'FC', JE_LocationOfGoods can be empty ", declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckJE_LocationOfGoods_WhenQualifierIsEmpty()
	{
		CombineAssertions(() =>
		{
			declaration.ZG_AuthorisationNumber = ZString.Empty;
			declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation;
			declaration.Validation.ValidateJE_LocationOfGoods();
			AssertHasMessageErrorContaining("When location qualifier is not empty, JE_LocationQualifier is 'FC', JE_LocationOfGoods gets validated", declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_LocationQualifier = ZString.Empty;
			declaration.Validation.ValidateJE_LocationOfGoods();
			AssertNoMessageErrorContaining("When location qualifier is empty, JE_LocationQualifier is 'FC', JE_LocationOfGoods is not validated", declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckJE_LocationOfGoods_WhenQualifierIsLBOrLC()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var authorisationHeader = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, permitHolder: orgHeader.PK, "999999", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, ITCusAuthorisationRuleTypeList.Codes.Location, "MYLOC1");
		CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, "UND", "UNDEFINED");

		Factory.Save();

		declaration.JE_OH_Importer = orgHeader.PK;

		AssertLocationOfGoodsWhenQualifierIsLBOrLC(ImportGoodsLocationQualifierWithAuthorisationList.Codes.AuthorizedPlace);
		AssertLocationOfGoodsWhenQualifierIsLBOrLC(ImportGoodsLocationQualifierWithAuthorisationList.Codes.ApprovedPlace);

		void AssertLocationOfGoodsWhenQualifierIsLBOrLC(ZString qualifier)
		{
			CombineAssertions("When ZG_AuthorisationNumber is not empty", () =>
			{
				declaration.ZG_AuthorisationNumber = "999999";
				declaration.JE_LocationQualifier = qualifier;
				declaration.JE_LocationOfGoods = ZString.Empty;
				AssertHasMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_LocationOfGoods = "MYLOC1";
				AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_LocationOfGoods = "NOLOCO";
				AssertHasMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);
			});

			CombineAssertions("When ZG_AuthorisationNumber is empty", () =>
			{
				declaration.ZG_AuthorisationNumber = ZString.Empty;
				declaration.JE_LocationQualifier = qualifier;
				declaration.JE_LocationOfGoods = ZString.Empty;
				AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_LocationOfGoods = "MYLOC1";
				AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_LocationOfGoods = "NOLOCO";
				AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);
			});
		}
	}

	public void TestCheckJE_SubLocationOfGoods_WhenQualifierIsEmpty()
	{
		CombineAssertions("When location qualifier is not empty", () =>
		{
			declaration.ZG_AuthorisationNumber = ZString.Empty;
			declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation;
			declaration.ImportJE_SubLocationOfGoods = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.ImportJE_SubLocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);
		});

		CombineAssertions("When location qualifier is empty", () =>
		{
			declaration.ZG_AuthorisationNumber = ZString.Empty;
			declaration.JE_LocationQualifier = ZString.Empty;
			declaration.ImportJE_SubLocationOfGoods = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.ImportJE_SubLocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckJE_SubLocationOfGoods_WhenQualifierIsFC()
	{
		var loader = new RefCountry.Loader(Factory);
		var italy = loader.LoadForCountry("IT");

		CombineAssertions("When ZG_AuthorisationNumber is empty", () =>
		{
			declaration.ZG_AuthorisationNumber = ZString.Empty;
			declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation;
			declaration.ImportJE_SubLocationOfGoods = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.ImportJE_SubLocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.ImportJE_SubLocationOfGoods = "IT";
			AssertNoMessageErrorContaining(declaration.ImportJE_SubLocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.ImportJE_SubLocationOfGoods = "WW";
			AssertHasMessageErrorContaining(declaration.ImportJE_SubLocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);
		});

		CombineAssertions("When ZG_AuthorisationNumber is not empty", () =>
		{
			declaration.ZG_AuthorisationNumber = "1234";
			declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation;
			declaration.ImportJE_SubLocationOfGoods = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.ImportJE_SubLocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.ImportJE_SubLocationOfGoods = "IT";
			AssertNoMessageErrorContaining(declaration.ImportJE_SubLocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.ImportJE_SubLocationOfGoods = "WW";
			AssertNoMessageErrorContaining(declaration.ImportJE_SubLocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);
		});
	}

	public void TestCheckJE_SubLocationOfGoods_WhenQualifierIsLBOrLC()
	{
		var loader = new RefCountry.Loader(Factory);
		var italy = loader.LoadForCountry("IT");

		AssertJE_SubLocationOfGoods_WhenQualifierIsLBOrLC(ImportGoodsLocationQualifierWithAuthorisationList.Codes.AuthorizedPlace);
		AssertJE_SubLocationOfGoods_WhenQualifierIsLBOrLC(ImportGoodsLocationQualifierWithAuthorisationList.Codes.ApprovedPlace);

		void AssertJE_SubLocationOfGoods_WhenQualifierIsLBOrLC(ZString qualifier)
		{
			CombineAssertions("When ZG_AuthorisationNumber is not empty", () =>
			{
				declaration.ZG_AuthorisationNumber = "999999";
				declaration.JE_LocationQualifier = qualifier;
				declaration.ImportJE_SubLocationOfGoods = ZString.Empty;
				AssertHasMessageErrorContaining(declaration.ImportJE_SubLocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.ImportJE_SubLocationOfGoods = "IT";
				AssertNoMessageErrorContaining(declaration.ImportJE_SubLocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.ImportJE_SubLocationOfGoods = "ZZ";
				AssertHasMessageErrorContaining(declaration.ImportJE_SubLocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);
			});

			CombineAssertions("When ZG_AuthorisationNumber is empty", () =>
			{
				declaration.ZG_AuthorisationNumber = ZString.Empty;
				declaration.JE_LocationQualifier = qualifier;
				declaration.ImportJE_SubLocationOfGoods = ZString.Empty;
				AssertNoMessageErrorContaining(declaration.ImportJE_SubLocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.ImportJE_SubLocationOfGoods = "IT";
				AssertNoMessageErrorContaining(declaration.ImportJE_SubLocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.ImportJE_SubLocationOfGoods = "ZZ";
				AssertNoMessageErrorContaining(declaration.ImportJE_SubLocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);
			});
		}
	}

	public void TestCheckJE_LocationOfGoods_LocationQualifierListvalidation()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var authorisationHeader = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, permitHolder: orgHeader.PK, "999999", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, ITCusAuthorisationRuleTypeList.Codes.Location, "MYLOC1");
		CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, "UND", "UNDEFINED");

		declaration.JE_OH_Importer = orgHeader.PK;

		declaration.ZG_AuthorisationNumber = ZString.Empty;
		declaration.JE_LocationQualifier = ZString.Empty;
		declaration.Validation.ValidateJE_LocationOfGoods();
		AssertNoMessageErrors("When ZG_AuthorisationNumber and JE_LocationQualifier are empty there shouldn't be any message error", declaration.JE_LocationOfGoodsInfo);

		declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation;
		declaration.Validation.ValidateJE_LocationOfGoods();
		AssertHasMessageErrorContaining("When ZG_AuthorisationNumber is empty, JE_LocationQualifier is 'FC, JE_LocationOfGoods shouldn't be empty ", declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

		declaration.JE_LocationOfGoods = "111111";
		AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);

		declaration.ZG_AuthorisationNumber = "999999";
		declaration.JE_LocationQualifier = "LB";
		declaration.Validation.ValidateJE_LocationOfGoods();
		AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);

		declaration.JE_LocationOfGoods = "MYLOC1";
		AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);

		declaration.JE_LocationOfGoods = "NEMO01";
		AssertHasMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);
	}

	public void TestCheckJE_LocationQualifier_MandatoryValidation()
	{
		declaration.JE_MessageType = "IMP";
		declaration.ZG_AuthorisationNumber = ZString.Empty;
		declaration.JE_LocationQualifier = ZString.Empty;
		AssertHasMessageErrorContaining(declaration.JE_LocationQualifierInfo, ValidationCaptions.JobDeclaration.LocationQualifierRequired);
		declaration.JE_LocationQualifier = "D";
		AssertNoMessageErrorContaining(declaration.JE_LocationQualifierInfo, ValidationCaptions.JobDeclaration.LocationQualifierRequired);
	}

	public void TestCheckJE_LocationQualifierListValidation()
	{
		const string expectedMessageError = "The code you have selected is not in the list";

		declaration.JE_LocationQualifier = ZString.Empty;
		AssertNoMessageErrorContaining("Empty JE_LocationQualifierInfo", declaration.JE_LocationQualifierInfo, expectedMessageError);

		declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreInCustomsAreaForInspection;
		AssertNoMessageErrorContaining("Code 'D' is in the list", declaration.JE_LocationQualifierInfo, expectedMessageError);

		declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuit;
		AssertNoMessageErrorContaining("Code 'F' is in the list", declaration.JE_LocationQualifierInfo, expectedMessageError);

		declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation;
		AssertNoMessageErrorContaining("Code 'FC' is in the list", declaration.JE_LocationQualifierInfo, expectedMessageError);

		declaration.JE_LocationQualifier = ImportGoodsLocationQualifierWithAuthorisationList.Codes.AuthorizedPlace;
		AssertHasMessageErrorContaining("Code 'LB' is not in the list", declaration.JE_LocationQualifierInfo, expectedMessageError);

		declaration.JE_LocationQualifier = ImportGoodsLocationQualifierWithAuthorisationList.Codes.ApprovedPlace;
		AssertHasMessageErrorContaining("Code 'LC' is not in the list", declaration.JE_LocationQualifierInfo, expectedMessageError);

		declaration.ZG_AuthorisationNumber = "ARG099";

		declaration.JE_LocationQualifier = ImportGoodsLocationQualifierWithAuthorisationList.Codes.AuthorizedPlace;
		AssertNoMessageErrorContaining("Code 'LB' is not in the list", declaration.JE_LocationQualifierInfo, expectedMessageError);

		declaration.JE_LocationQualifier = ImportGoodsLocationQualifierWithAuthorisationList.Codes.ApprovedPlace;
		AssertNoMessageErrorContaining("Code 'LC' is not in the list", declaration.JE_LocationQualifierInfo, expectedMessageError);

		declaration.JE_LocationQualifier = "XX";
		AssertHasMessageErrorContaining("Code 'XX' is not in the list", declaration.JE_LocationQualifierInfo, expectedMessageError);
	}

	public void TestCheckJE_LocationQualifierListWithAuthorizationValidation()
	{
		const string expectedMessageError = "The code you have selected is not in the list";
		declaration.ZG_AuthorisationNumber = "123456";

		declaration.JE_LocationQualifier = ZString.Empty;
		AssertNoMessageErrorContaining("Empty JE_LocationQualifierInfo", declaration.JE_LocationQualifierInfo, expectedMessageError);

		declaration.JE_LocationQualifier = ImportGoodsLocationQualifierWithAuthorisationList.Codes.AuthorizedPlace;
		AssertNoMessageErrorContaining("Code 'LB' is in the list", declaration.JE_LocationQualifierInfo, expectedMessageError);

		declaration.JE_LocationQualifier = ImportGoodsLocationQualifierWithAuthorisationList.Codes.ApprovedPlace;
		AssertNoMessageErrorContaining("Code 'LC' is in the list", declaration.JE_LocationQualifierInfo, expectedMessageError);

		declaration.JE_LocationQualifier = "XX";
		AssertHasMessageErrorContaining("Code 'XX' is not in the list", declaration.JE_LocationQualifierInfo, expectedMessageError);
	}

	public void TestCheckJE_SubLocationOfGoods()
	{
		var expectedMessageError = "Field exceeds the maximum allowed length in the declaration message (17 characters).";

		declaration.JE_SubLocationOfGoods = ZString.Empty;
		AssertNoMessageErrorContaining(declaration.JE_SubLocationOfGoodsInfo, expectedMessageError);

		declaration.JE_SubLocationOfGoods = "0123456789ABCDEFGH";
		AssertHasMessageErrorContaining(declaration.JE_SubLocationOfGoodsInfo, expectedMessageError);

		declaration.JE_SubLocationOfGoods = "0123456789";
		AssertNoMessageErrorContaining(declaration.JE_SubLocationOfGoodsInfo, expectedMessageError);
	}

	public void TestImportJE_SubLocationOfGoodsInfo()
	{
		CombineAssertions("When ZG_AuthorisationNumber is empty", () =>
		{
			declaration.ZG_AuthorisationNumber = ZString.Empty;
			declaration.JE_LocationQualifier = GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation;
			declaration.ImportJE_SubLocationOfGoods = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.ImportJE_SubLocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.JE_SubLocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.ImportJE_SubLocationOfGoods = "IT";
			AssertNoMessageErrorContaining(declaration.JE_SubLocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
	}

	JobDeclaration declaration;
}
