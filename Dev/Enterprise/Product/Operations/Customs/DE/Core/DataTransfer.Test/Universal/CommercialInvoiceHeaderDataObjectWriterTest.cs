using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DE.DataTransfer.Universal.Testing;

sealed class CommercialInvoiceHeaderDataObjectWriterTest : TestCaseWithFactory
{
	public void TestStateOfOrigin_ShouldReturnCode()
	{
		var testCases = new[]
		{
			(CountryOfOrigin: Core.Constants.CountryCodes.Austria, StateOrRegionOfOrigin: OriginFederalStateList.Codes.Ursprungsausland, ExpectedCode: OriginFederalStateList.Codes.Ursprungsausland),
			(CountryOfOrigin: Core.Constants.CountryCodes.Germany, StateOrRegionOfOrigin: OriginFederalStateList.Codes.Bayern, ExpectedCode: OriginFederalStateList.Codes.Bayern),
			(CountryOfOrigin: Core.Constants.CountryCodes.Germany, StateOrRegionOfOrigin: OriginFederalStateList.Codes.SachsenAnhalt, ExpectedCode: OriginFederalStateList.Codes.SachsenAnhalt),
		};

		var testInstruction = Factory.NewWithValidTestData<Business.Declaration.CusEntryInstruction>();

		var testHeader = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
		var testLine = testHeader.InvoiceLines.AddNew();
		testLine.JI_CEI = testInstruction.PK;

		var helper = new EU.DataTransfer.Universal.UniversalDataObjectWriterHelper(Factory, Core.Constants.CountryCodes.Germany);
		var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, testInstruction)), helper);

		foreach (var (countryOfOrigin, stateOrRegionOfOrigin, expectedCode) in testCases)
		{
			testLine.JI_CountryOfOrigin = countryOfOrigin;
			testLine.JI_StateOrRegionOfOrigin = stateOrRegionOfOrigin;

			var resultHeader = writer.GetDataObject(testHeader);

			var stateOfOrigin = resultHeader.CommercialInvoiceLineCollection[0].StateOfOrigin;
			AssertEquals($"CountryOfOrigin - {countryOfOrigin}, StateOrRegionOfOrigin - {stateOrRegionOfOrigin}", expectedCode, stateOfOrigin.Code);
		}
	}

	public void TestAuthorizationNumber_ShouldBeSet()
	{
		var testCases = new[]
		{
			(AuthorizationNumber: (ZString)"123", ExpectedAuthorizationNumber: "123"),
			(AuthorizationNumber: ZString.Empty, ExpectedAuthorizationNumber: null),
		};

		var testInstruction = Factory.NewWithValidTestData<Business.Declaration.CusEntryInstruction>();

		var testHeader = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
		var testLine = (Business.Declaration.JobComInvoiceLine)testHeader.InvoiceLines.AddNew();
		testLine.JI_CEI = testInstruction.PK;
		var previousProcedure = testLine.PreviousProcedures.AddNew();
		previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATAV;

		var helper = new EU.DataTransfer.Universal.UniversalDataObjectWriterHelper(Factory, Core.Constants.CountryCodes.Germany);
		var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, testInstruction)), helper);

		foreach (var (authorizationNumber, expectedAuthorizationNumber) in testCases)
		{
			testLine.PreviousProcedureMaster.AuthorizationNumber = authorizationNumber;

			var resultHeader = writer.GetDataObject(testHeader);

			var previousProcedureMaster = resultHeader.CommercialInvoiceLineCollection[0].CustomsSupportingInformationCollection[0];
			var authorizationNumberReference = previousProcedureMaster.ReferenceNumberCollection?.SingleOrDefault(rn => rn.Type.Code.GetValueOrDefault() == Constants.ReferenceNumbers.AuthorizationNumberCode);
			if (expectedAuthorizationNumber is null)
			{
				AssertNull("No ReferenceNumber", authorizationNumberReference);
			}
			else
			{
				CombineAssertions(() =>
				{
					AssertNotNull(authorizationNumberReference);
					AssertEquals(expectedAuthorizationNumber, authorizationNumberReference.ReferenceNumber);
					AssertEquals(Constants.ReferenceNumbers.AuthorizationNumberDescription, authorizationNumberReference.Type.Description);
				});
			}
		}
	}

	public void TestUsualProcessingFlag_ShouldBeSet()
	{
		var testInstruction = Factory.NewWithValidTestData<Business.Declaration.CusEntryInstruction>();

		var testHeader = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
		var testLine = (Business.Declaration.JobComInvoiceLine)testHeader.InvoiceLines.AddNew();
		testLine.JI_CEI = testInstruction.PK;
		var previousProcedure1 = testLine.PreviousProcedures.AddNew();
		previousProcedure1.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
		previousProcedure1.UsualProcessingFlag = true;
		var previousProcedure2 = testLine.PreviousProcedures.AddNew();
		previousProcedure2.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
		previousProcedure2.UsualProcessingFlag = false;

		var helper = new EU.DataTransfer.Universal.UniversalDataObjectWriterHelper(Factory, Core.Constants.CountryCodes.Germany);
		var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, testInstruction)), helper);

		var resultHeader = writer.GetDataObject(testHeader);

		var invoiceLine = resultHeader.CommercialInvoiceLineCollection[0];

		var usualProcessingFlagAddInfo1 = invoiceLine.CustomsSupportingInformationCollection[0].AddInfoCollection.Single();
		AssertEquals(YesNoList.Codes.Yes, usualProcessingFlagAddInfo1.Value);
		AssertEquals("UsualProcessingFlag", usualProcessingFlagAddInfo1.Key);

		var usualProcessingFlagAddInfo2 = invoiceLine.CustomsSupportingInformationCollection[1].AddInfoCollection.Single();
		AssertEquals(YesNoList.Codes.No, usualProcessingFlagAddInfo2.Value);
		AssertEquals("UsualProcessingFlag", usualProcessingFlagAddInfo2.Key);
	}

	public void TestIsMainPack_ShouldBeSet()
	{
		var testCases = new[]
		{
			(IsMainPack: true, ExpectedIsMainPack: YesNoList.Codes.Yes),
			(IsMainPack: false, ExpectedIsMainPack: YesNoList.Codes.No),
		};

		var testInstruction = Factory.NewWithValidTestData<Business.Declaration.CusEntryInstruction>();

		var testHeader = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
		var testLine = (Business.Declaration.JobComInvoiceLine)testHeader.InvoiceLines.AddNew();
		testLine.JI_CEI = testInstruction.PK;
		var previousProcedure = testLine.PreviousProcedures.AddNew();
		previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATAV;

		var helper = new EU.DataTransfer.Universal.UniversalDataObjectWriterHelper(Factory, Core.Constants.CountryCodes.Germany);
		var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, testInstruction)), helper);

		foreach (var (isMainPack, expectedIsMainPack) in testCases)
		{
			testLine.JI_IsMainPack = isMainPack;

			var resultHeader = writer.GetDataObject(testHeader);

			var isMainPackAddInfo = resultHeader.CommercialInvoiceLineCollection[0].AddInfoCollection.SingleOrDefault(ai => ai.Key.GetValueOrDefault() == Constants.AddInfoKeys.InvoiceLine.IsMainPack);
			CombineAssertions(() =>
			{
				AssertNotNull(isMainPackAddInfo);
				AssertEquals(expectedIsMainPack, isMainPackAddInfo.Value);
			});
		}
	}
}
