using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class TransitOperationWrapperTest : Customs.Business.Testing.DataProviderTestCase<TransitOperationWrapper>
	{
		public void TestLRN()
		{
			AssertEquals("LRN should be mapped to BM_PaperlessInbondNum.", "LRN001", Provider.LRN);
		}

		public void TestMRN()
		{
			AssertEquals("MRN should be mapped to MovementReferenceNumber.", "MRN001", Provider.MRN);
		}

		public void TestEORIOperateurBeneficiaireAgrement_PrincipalWithOrganisation()
		{
			GetProvider();
			var principal = Factory.New<OrgHeader>();
			principal.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			principal.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			nctsHeader.Principal.OrganisationPK = principal.PK;
			var provider = TransitOperationWrapper.New(new TP5MessageSendingObject(nctsHeader));
			AssertEquals("EORIOperateurBeneficiaireAgrement should be mapped to Princpal EORI.", "FR12345678900001", provider.EORIOperateurBeneficiaireAgrement);
		}

		public void TestEORIOperateurBeneficiaireAgrement_PrincipalWithoutOrganisation()
		{
			AssertEquals("EORIOperateurBeneficiaireAgrement should be empty when Princioapl has no organisation (not likely to happen).", string.Empty, Provider.EORIOperateurBeneficiaireAgrement);
		}

		public void TestDeclarationType()
		{
			AssertEquals("DeclarationType should be mapped to BM_EntryType.", "A", Provider.DeclarationType);
		}

		public void TestAdditionalDeclarationType()
		{
			AssertEquals("AdditionalDeclarationType should be mapped to BM_AdditionalDeclarationType.", "B", Provider.AdditionalDeclarationType);
		}

		public void TestTIRCarnetNumber()
		{
			AssertEquals("TIRCarnetNumber should be mapped to movement TIRCarnetNumber.", "TIR001", Provider.TIRCarnetNumber);
		}

		public void TestPresentationOfTheGoodsDateAndTime()
		{
			AssertEquals("PresentationOfTheGoodsDateAndTime should be mapped to BM_PresentationDateTime.", new DateTime(2024, 01, 01), Provider.PresentationOfTheGoodsDateAndTime);
		}

		public void TestSecurity()
		{
			GetProvider();
			movementHeader.BM_TypeOfSecurity = EU.NCTS.Business.NctsTypeOfSecurityList.Codes.NON;
			var provider = TransitOperationWrapper.New(new TP5MessageSendingObject(nctsHeader));
			AssertEquals("Security should be 0 when BM_TypeOfSecurity is 'NON'.", "0", provider.Security);

			movementHeader.BM_TypeOfSecurity = EU.NCTS.Business.NctsTypeOfSecurityList.Codes.ENT;
			provider = TransitOperationWrapper.New(new TP5MessageSendingObject(nctsHeader));
			AssertEquals("Security should be 1 when BM_TypeOfSecurity is 'ENT'.", "1", provider.Security);

			movementHeader.BM_TypeOfSecurity = EU.NCTS.Business.NctsTypeOfSecurityList.Codes.EXI;
			provider = TransitOperationWrapper.New(new TP5MessageSendingObject(nctsHeader));
			AssertEquals("Security should be 2 when BM_TypeOfSecurity is 'EXI'.", "2", provider.Security);

			movementHeader.BM_TypeOfSecurity = EU.NCTS.Business.NctsTypeOfSecurityList.Codes.BTH;
			provider = TransitOperationWrapper.New(new TP5MessageSendingObject(nctsHeader));
			AssertEquals("Security should be 3 when BM_TypeOfSecurity is 'BTH'.", "3", provider.Security);
		}

		public void TestReducedDatasetIndicator()
		{
			GetProvider();
			movementHeader.BM_ReducedDatasetIndicator = false;
			AssertEquals("ReducedDatasetIndicator should be mapped to BM_ReducedDatasetIndicator.", false, Provider.ReducedDatasetIndicator);

			movementHeader.BM_ReducedDatasetIndicator = true;
			AssertEquals("ReducedDatasetIndicator should be mapped to BM_ReducedDatasetIndicator.", true, Provider.ReducedDatasetIndicator);
		}

		public void TestSpecificCircumstanceIndicator()
		{
			AssertEquals("SpecificCircumstanceIndicator should be mapped to BM_SpecificCircumstance.", "2", Provider.SpecificCircumstanceIndicator);
		}

		public void TestCommunicationLanguageAtDeparture()
		{
			AssertEquals("CommunicationLanguageAtDeparture should always be fr", "fr", Provider.CommunicationLanguageAtDeparture);
		}

		public void TestBindingItinerary()
		{
			GetProvider();
			movementHeader.BM_TypeOfSecurity = EU.NCTS.Business.NctsTypeOfSecurityList.Codes.NON;
			AssertEquals("BindingItinerary should be 0 when BM_TypeOfSecurity is 'NON'.", false, Provider.BindingItinerary);

			movementHeader.BM_TypeOfSecurity = EU.NCTS.Business.NctsTypeOfSecurityList.Codes.ENT;
			AssertEquals("BindingItinerary should be 1 when BM_TypeOfSecurity is 'ENT'.", true, Provider.BindingItinerary);

			movementHeader.BM_TypeOfSecurity = EU.NCTS.Business.NctsTypeOfSecurityList.Codes.EXI;
			AssertEquals("BindingItinerary should be 1 when BM_TypeOfSecurity is 'EXI'.", true, Provider.BindingItinerary);

			movementHeader.BM_TypeOfSecurity = EU.NCTS.Business.NctsTypeOfSecurityList.Codes.BTH;
			AssertEquals("BindingItinerary should be 1 when BM_TypeOfSecurity is 'BTH'.", true, Provider.BindingItinerary);
		}

		public void TestLimitDate()
		{
			AssertEquals("LimitDate should be null when BM_ExportDate is null", null, Provider.LimitDate);
			movementHeader.BM_ExportDate = new DateTime(2024, 02, 02);
			AssertEquals("LimitDate should be mapped to BM_ExportDate", new DateTime(2024, 02, 02), Provider.LimitDate);
		}

		public void TestOtherThingsToReport()
		{
			AssertEquals("OtherThingsToReport is not yet mapped.", null, Provider.OtherThingsToReport);
		}

		public void TestAmendmentTypeFlag()
		{
			AssertEquals("AmendmentTypeFlag defaults to false.", false, Provider.AmendmentTypeFlag);

			var wrapper = TransitOperationWrapper.New(new TP5MessageSendingObject(nctsHeader), true);
			AssertEquals(true, wrapper.AmendmentTypeFlag);
		}

		public void TestMotif()
		{
			AssertEquals(string.Empty, Provider.Motif); // TODO will be done in WI00710850 - FR - TP5 - IE013 motif field
		}

		protected override TransitOperationWrapper GetProvider()
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_PaperlessInbondNum = "LRN001";
			movementHeader.BM_InBondEntryType = "A";
			movementHeader.BM_AdditionalDeclarationType = "B";
			movementHeader.BM_PresentationDateTime = new CargoWise.Types.ZDateTimeOffset(2024, 01, 01);
			movementHeader.BM_SpecificCircumstance = "2";

			var tirCarnetNumber = CusEntryNumber.LoadOrCreate(movementHeader, OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, Core.Constants.CountryCodes.France);
			tirCarnetNumber.CE_EntryNum = "TIR001";

			var mrn = CusEntryNumber.LoadOrCreate(nctsHeader, "MRN", Core.Constants.CountryCodes.France);
			mrn.CE_EntryNum = "MRN001";

			return TransitOperationWrapper.New(new TP5MessageSendingObject(nctsHeader));
		}

		NctsHeader nctsHeader;
		NctsDepartureMovementHeader movementHeader;
	}
}
