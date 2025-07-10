using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class NctsDepartureMovementHeaderOrBillPhase5RuleN0002ValidationTest : BusinessObjectValidationTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NctsDepartureMovementHeaderOrBillPhase5RuleN0002Validation(null));
		AssertNoExceptionThrown(() => new NctsDepartureMovementHeaderOrBillPhase5RuleN0002Validation(movementHeader));

		movementHeader = Factory.New<NctsDepartureMovementHeader>();
		AssertExceptionThrown<ArgumentNullException>(() => new NctsDepartureMovementHeaderOrBillPhase5RuleN0002Validation(movementHeader));
	}

	public void TestIsRuleN0002Applicable()
	{
		AssertEquals("When Rule N0002 is active and not applicable", false, NctsDepartureMovementHeaderOrBillPhase5RuleN0002Validation.IsRuleN0002Applicable(movementHeader));

		var container = movementHeader.Header.DepartureHeaderContainers.AddNew();
		container.BC_Mode = "CNT";
		AssertEquals("When Rule N0002 is active, header is containerized", true, NctsDepartureMovementHeaderOrBillPhase5RuleN0002Validation.IsRuleN0002Applicable(movementHeader));

		container.BC_Mode = "NCT";
		var supportingDocument = movementHeader.SupportingDocuments.AddNew();
		supportingDocument.CSI_Code = "66YY";

		var orgHeader = Factory.New<OrgHeader>();
		var customsCode = orgHeader.CustomsCodes.AddNew();
		movementHeader.Header.Principal.OrganisationPK = orgHeader.PK;
		customsCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator;
		customsCode.SecuredCustomsRegNo = "AEOC171368";
		AssertEquals("When Rule N0002 is active, header has supporting document type 66YY, Principal has an AEO registration number containing AEOC string", true, NctsDepartureMovementHeaderOrBillPhase5RuleN0002Validation.IsRuleN0002Applicable(movementHeader));

		customsCode.SecuredCustomsRegNo = "AEOF171368";
		AssertEquals("When Rule N0002 is active, header has supporting document type 66YY, Principal has an AEO registration number containing AEOF string", true, NctsDepartureMovementHeaderOrBillPhase5RuleN0002Validation.IsRuleN0002Applicable(movementHeader));

		supportingDocument.CSI_Code = "99WW";
		AssertEquals("When Rule N0002 is active, header has supporting document type 99WW, Principal has an AEO registration number containing AEOF string", false, NctsDepartureMovementHeaderOrBillPhase5RuleN0002Validation.IsRuleN0002Applicable(movementHeader));

		supportingDocument.CSI_Code = "66YY";
		context.DisableRule(x => x.IsRuleN0002Active);
		AssertEquals("When Rule N0002 is inactive, header has supporting document type 66YY, Principal has an AEO registration number containing AEOF string", false, NctsDepartureMovementHeaderOrBillPhase5RuleN0002Validation.IsRuleN0002Applicable(movementHeader));
	}

	public void TestCheckBM_TransportTypeAtDeparture_ForHeader_RuleN0002()
	{
		AssertRuleN0002_FieldMustBeEmpty_ForHeader("Type of Identification",
			movementHeader.BM_TransportAtDepartureTypeInfo,
			(value) => movementHeader.BM_TransportAtDepartureType = value,
			movementHeader.Validation.ValidateBM_TransportAtDepartureType);
	}

	public void TestCheckBM_TransportAtDeparture_ForHeader_RuleN0002()
	{
		AssertRuleN0002_FieldMustBeEmpty_ForHeader("Transport Identification",
			movementHeader.BM_TransportAtDepartureInfo,
			(value) => movementHeader.BM_TransportAtDeparture = value,
			movementHeader.Validation.ValidateBM_TransportAtDeparture);
	}

	public void TestCheckBM_TransportAtDepartureTrailer1RegNo_ForHeader_RuleN0002()
	{
		AssertRuleN0002_FieldMustBeEmpty_ForHeader("Trailer 1 ID",
			movementHeader.BM_TransportAtDepartureTrailer1RegNoInfo,
			(value) => movementHeader.BM_TransportAtDepartureTrailer1RegNo = value,
			movementHeader.Validation.ValidateBM_TransportAtDepartureTrailer1RegNo);
	}

	public void TestCheckBM_TransportAtDepartureTrailer2RegNo_ForHeader_RuleN0002()
	{
		AssertRuleN0002_FieldMustBeEmpty_ForHeader("Trailer 2 ID",
				movementHeader.BM_TransportAtDepartureTrailer2RegNoInfo,
				(value) => movementHeader.BM_TransportAtDepartureTrailer2RegNo = value,
				movementHeader.Validation.ValidateBM_TransportAtDepartureTrailer2RegNo);
	}

	public void TestCheckBM_RN_NKTransportAtDepartureCountry_ForHeader_RuleN0002()
	{
		AssertRuleN0002_FieldMustBeEmpty_ForHeader("Transport Nationality",
				movementHeader.BM_RN_NKTransportAtDepartureCountryInfo,
				(value) => movementHeader.BM_RN_NKTransportAtDepartureCountry = value,
				movementHeader.Validation.ValidateBM_RN_NKTransportAtDepartureCountry);
	}

	public void TestCheckBM_RN_NKTransportAtDepartureTrailer1Nationality_ForHeader_RuleN0002()
	{
		AssertRuleN0002_FieldMustBeEmpty_ForHeader("Trailer 1 Nationality",
				movementHeader.BM_RN_NKTransportAtDepartureTrailer1NationalityInfo,
				(value) => movementHeader.BM_RN_NKTransportAtDepartureTrailer1Nationality = value,
				movementHeader.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer1Nationality);
	}

	public void TestCheckBM_RN_NKTransportAtDepartureTrailer2Nationality_ForHeader_RuleN0002()
	{
		AssertRuleN0002_FieldMustBeEmpty_ForHeader("Trailer 2 Nationality",
				movementHeader.BM_RN_NKTransportAtDepartureTrailer2NationalityInfo,
				(value) => movementHeader.BM_RN_NKTransportAtDepartureTrailer2Nationality = value,
				movementHeader.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer2Nationality);
	}

	public void TestCheckTransportTypeAtDeparture_ForBill_RuleN0002()
	{
		AssertRuleN0002_FieldMustBeEmpty_ForBill("Type of Identification",
			nctsBill.TransportTypeAtDepartureInfo,
			(value) => nctsBill.TransportTypeAtDeparture = value,
			nctsBill.Validation.ValidateTransportTypeAtDeparture);
	}

	public void TestCheckTransportAtDeparture_ForBill_RuleN0002()
	{
		AssertRuleN0002_FieldMustBeEmpty_ForBill("First Departure Transport Means ID",
			nctsBill.FirstDepartureTransportMeansIDInfo,
			(value) => nctsBill.FirstDepartureTransportMeansID = value,
			nctsBill.Validation.ValidateFirstDepartureTransportMeansID);
	}

	public void TestTransportAtDepartureTrailer1RegNo_ForBill_RuleN0002()
	{
		AssertRuleN0002_FieldMustBeEmpty_ForBill("Second Departure Transport Means ID",
			nctsBill.SecondDepartureTransportMeansIDInfo,
				(value) => nctsBill.SecondDepartureTransportMeansID = value,
				nctsBill.Validation.ValidateSecondDepartureTransportMeansID);
	}

	public void TestTransportAtDepartureTrailer2RegNo_ForBill_RuleN0002()
	{
		AssertRuleN0002_FieldMustBeEmpty_ForBill("Third Departure Transport Means ID",
				nctsBill.ThirdDepartureTransportMeansIDInfo,
				(value) => nctsBill.ThirdDepartureTransportMeansID = value,
				nctsBill.Validation.ValidateThirdDepartureTransportMeansID);
	}

	public void TestCheckNationalityAtDeparture_ForBill_RuleN0002()
	{
		AssertRuleN0002_FieldMustBeEmpty_ForBill("Nationality",
				nctsBill.FirstDepartureTransportMeansNationalityInfo,
				(value) => nctsBill.FirstDepartureTransportMeansNationality = value,
				nctsBill.Validation.ValidateFirstDepartureTransportMeansNationality);
	}

	public void TestCheckTrailer1NationalityAtDeparture_ForBill_RuleN0002()
	{
		AssertRuleN0002_FieldMustBeEmpty_ForBill("Nationality",
				nctsBill.Trailer1NationalityAtDepartureInfo,
				(value) => nctsBill.Trailer1NationalityAtDeparture = value,
				nctsBill.Validation.ValidateSecondDepartureTransportMeansNationality);
	}

	public void TestCheckTrailer2NationalityAtDeparture_ForBill_RuleN0002()
	{
		AssertRuleN0002_FieldMustBeEmpty_ForBill("Nationality",
				nctsBill.Trailer2NationalityAtDepartureInfo,
				(value) => nctsBill.Trailer2NationalityAtDeparture = value,
				nctsBill.Validation.ValidateThirdDepartureTransportMeansNationality);
	}

	void AssertRuleN0002_FieldMustBeEmpty_ForHeader(ZString propertyName, ZPropertyInfo propertyInfo, Action<ZString> setPropertyValue, Action validateProperty)
	{
		var (expectedMessage1, expectedMessage2) = GetMessageError(propertyName);

		context.ClearCachedValidationDecider(movementHeader);
		context.EnableRule(x => x.IsRuleN0002Active);

		CombineAssertions("Rule N0002 active", () =>
		{
			TestFieldEmpty(propertyInfo, setPropertyValue, expectedMessage1);
			TestFieldWithHeaderContainers(propertyInfo, setPropertyValue, validateProperty, expectedMessage1);
			TestFieldWithSupportingDocumentWithPrincipalHeader(propertyInfo, setPropertyValue, validateProperty, expectedMessage1, expectedMessage2);
		});

		context.DisableRule(x => x.IsRuleN0002Active);
		TestFieldWithDisabledRule(propertyInfo, setPropertyValue, validateProperty, expectedMessage1, expectedMessage2);
	}

	void AssertRuleN0002_FieldMustBeEmpty_ForBill(ZString propertyName, ZPropertyInfo propertyInfo, Action<ZString> setPropertyValue, Action validateProperty)
	{
		context.Dispose();
		var (expectedMessage1, expectedMessage2) = GetMessageError(propertyName);

		using (var billTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
		{
			billTestContext.EnableRule(x => x.IsRuleN0002Active);

			CombineAssertions("Rule N0002 active", () =>
			{
				TestFieldEmpty(propertyInfo, setPropertyValue, expectedMessage1);
				TestFieldWithHeaderContainers(propertyInfo, setPropertyValue, validateProperty, expectedMessage1);
				TestFieldWithSupportingDocumentWithPrincipalHeader(propertyInfo, setPropertyValue, validateProperty, expectedMessage1, expectedMessage2);
			});
		}
	}

	void TestFieldEmpty(ZPropertyInfo propertyInfo, Action<ZString> setPropertyValue, string expectedMessage)
	{
		setPropertyValue("AA");
		AssertNoMessageErrorContaining("Transport field is filled", propertyInfo, expectedMessage);
		setPropertyValue(ZString.Empty);
		AssertNoMessageErrorContaining("Transport field is empty", propertyInfo, expectedMessage);
	}

	void TestFieldWithHeaderContainers(ZPropertyInfo propertyInfo, Action<ZString> setPropertyValue, Action validateProperty, string expectedMessage)
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NCNAT);
		Factory.Save();

		var cnt = movementHeader.Header.DepartureHeaderContainers.AddNew();
		cnt.BC_Mode = "CNT";
		cnt.BC_ContainerNum = "ABC123";

		setPropertyValue("DE");
		validateProperty();
		AssertHasMessageErrorContaining("Transport field is filled, header has a containerized container", propertyInfo, expectedMessage);

		setPropertyValue(ZString.Empty);
		validateProperty();
		AssertNoMessageErrorContaining("Transport field is empty, header has a containerized container", propertyInfo, expectedMessage);

		cnt.BC_Mode = "NCT";
		setPropertyValue("DE");
		validateProperty();
		AssertNoMessageErrorContaining("Transport field is filled, header has a non-containerized container", propertyInfo, expectedMessage);

		movementHeader.Header.DepartureHeaderContainers.RemoveAndDeleteAll();
	}

	void TestFieldWithSupportingDocumentWithPrincipalHeader(ZPropertyInfo propertyInfo, Action<ZString> setPropertyValue, Action validateProperty, string expectedMessage1, string expectedMessage2)
	{
		var orgHeader = Factory.New<OrgHeader>();
		var customsCode = orgHeader.CustomsCodes.AddNew();
		customsCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator;
		customsCode.SecuredCustomsRegNo = "AEOC171368";

		var supportingDocument = movementHeader.SupportingDocuments.AddNew();
		supportingDocument.CSI_Code = "66YY";

		movementHeader.Header.Principal.OrganisationPK = orgHeader.PK;

		setPropertyValue("AA");
		AssertHasMessageErrorContaining("Transport field is filled, Supporting document type 66YY, Principal has an AEO registration number containing AEOC string", propertyInfo, expectedMessage1);

		supportingDocument.CSI_Code = "99WW";
		setPropertyValue("AA");
		AssertNoMessageErrorContaining("Transport field is filled, Supporting document type 99WW, Principal has an AEO registration number containing AEOC string", propertyInfo, expectedMessage1);

		supportingDocument.CSI_Code = "66YY";
		customsCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.RegisteredExporterNumber;
		customsCode.SecuredCustomsRegNo = "CDFE171368";
		validateProperty();
		AssertNoMessageErrorContaining("Transport field is filled, Supporting document type 66YY, Principal does not have an AEO registration number", propertyInfo, expectedMessage1);
		AssertHasWarningContaining("Transport field is filled, Supporting document type 66YY, Principal does not have an AEO registration number", propertyInfo, expectedMessage2);

		customsCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator;
		customsCode.SecuredCustomsRegNo = "CDFE171368";
		validateProperty();
		AssertNoMessageErrorContaining("Transport field is filled, Supporting document type 66YY, Principal has an AEO registration number without AEOC/F string", propertyInfo, expectedMessage1);
		AssertHasWarningContaining("Transport field is filled, Supporting document type 66YY, Principal has an AEO registration number without AEOC/F string", propertyInfo, expectedMessage2);
	}

	void TestFieldWithDisabledRule(ZPropertyInfo propertyInfo, Action<ZString> setPropertyValue, Action validateProperty, string expectedMessage1, string expectedMessage2)
	{
		var headerContainers = movementHeader.Header.DepartureHeaderContainers;
		var cnt = headerContainers.AddNew();
		cnt.BC_Mode = "CNT";
		cnt.BC_ContainerNum = "ABC123";
		setPropertyValue("AA");
		AssertNoMessageErrorContaining("Rule N0002 inactive, Transport field is filled, header has a containerized container", propertyInfo, expectedMessage1);

		var supportingDocument = movementHeader.SupportingDocuments.AddNew();
		supportingDocument.CSI_Code = "66YY";

		var orgHeader = Factory.New<OrgHeader>();
		var customsCode = orgHeader.CustomsCodes.AddNew();
		customsCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator;
		customsCode.SecuredCustomsRegNo = "AEOC171368";
		validateProperty();
		AssertNoMessageErrorContaining("Rule N0002 inactive, Transport field is filled, Supporting document type 66YY, Principal has an AEO registration number", propertyInfo, expectedMessage1);

		customsCode.SecuredCustomsRegNo = "CDFE171368";
		validateProperty();
		AssertNoMessageErrorContaining("Rule N0002 inactive, Transport field is filled, Supporting document type 66YY, Principal has an AEO registration number without AEOC/F string", propertyInfo, expectedMessage2);
	}

	(string, string) GetMessageError(string propertyName)
	{
		var message1 = $"[N0002] {propertyName}: This field must be empty.";

		var message2 = $"[N0002] {propertyName}: The type of AEO authorization of the Principal cannot be determined." +
			" For AEOC type authorization, with the presence of the code 66YY in the Supporting documents, this field should not be filled.";

		return (message1, message2);
	}

	protected override void SetUp()
	{
		base.SetUp();
		context = new MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider>(Factory);
		context.EnableRule(x => x.IsRuleN0002Active);

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		movementHeader = nctsHeader.MovementHeader;
		nctsBill = movementHeader.Header.Bills.AddNew();
	}

	NctsDepartureMovementHeader movementHeader;
	NctsBill nctsBill;
	MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider> context;
}
