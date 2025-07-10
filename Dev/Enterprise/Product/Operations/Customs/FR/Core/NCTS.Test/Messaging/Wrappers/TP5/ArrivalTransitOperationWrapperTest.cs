using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using NctsHeader = Enterprise.Customs.FR.Business.NCTS.NctsHeader;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class ArrivalTransitOperationWrappertest : Customs.Business.Testing.DataProviderTestCase<ArrivalTransitOperationWrapper>
	{
		public void TestEORIOperateurBeneficiaireAgrement()
		{
			AssertEquals("EORIOperateurBeneficiaireAgrement should be the EORI of the Destination Trader", "FR12345678900001", Provider.EORIOperateurBeneficiaireAgrement);
		}

		public void TestMRN()
		{
			AssertEquals("MRN should be equal to NCTS Header MovementReferenceNumber.", "MRN001", Provider.MRN);
		}

		public void TestArrivalNotificationDateAndTime()
		{
			AssertEquals("ArrivalNotificationDateAndTime should equal BM_ArrivalDate.", ZDateTime.Today.AddDays(1), Provider.ArrivalNotificationDateAndTime);
		}

		public void TestSimplifiedProcedure()
		{
			AssertEquals("SimplifiedProcedure should be equal to true if AuthorizationCode is not empty.", true, Provider.SimplifiedProcedure);

			var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
			arrivalMovementHeader.AuthorizationCode = ZString.Empty;

			AssertEquals("SimplifiedProcedure should be equal to false if AuthorizationCode is empty.", false, Provider.SimplifiedProcedure);
		}

		public void TestIncidentFlag()
		{
			AssertEquals("IncidentFlag should be true if BH_ExportFlag equals yes.", true, Provider.IncidentFlag);

			nctsHeader.BH_ExportFlag = YesNoList.Codes.No;
			AssertEquals("IncidentFlag should be false if BH_ExportFlag equals NO.", false, Provider.IncidentFlag);
		}

		public void TestDestinationDouaniere()
		{
			AssertEquals("TODO: should be mapped in WI00710803", ZString.Empty, Provider.DestinationDouaniere);
		}

		public void TestOtherThingsToReport()
		{
			AssertEquals("OtherThingsToReport should equal to ArrivalMovementHeader.OtherThingsToReport.", "OtherThingsToReport", Provider.OtherThingsToReport);
		}

		protected override ArrivalTransitOperationWrapper GetProvider()
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			movementHeader.BM_ArrivalDate = ZDateTime.Today.AddDays(1);
			movementHeader.AuthorizationCode = "AA";
			movementHeader.OtherThingsToReport = "OtherThingsToReport";
			nctsHeader.BH_ExportFlag = YesNoList.Codes.Yes;

			var destinationTrader = Factory.New<OrgHeader>();
			destinationTrader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			destinationTrader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			nctsHeader.DestinationTrader.OrganisationPK = destinationTrader.PK;

			var mrn = CusEntryNumber.LoadOrCreate(nctsHeader, "MRN", Core.Constants.CountryCodes.France);
			mrn.CE_EntryNum = "MRN001";

			return ArrivalTransitOperationWrapper.New(nctsHeader);
		}

		NctsHeader nctsHeader;
	}
}
