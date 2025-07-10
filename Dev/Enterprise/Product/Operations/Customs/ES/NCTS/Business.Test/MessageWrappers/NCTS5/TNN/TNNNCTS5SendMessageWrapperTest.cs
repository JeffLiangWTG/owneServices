using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class TNNNCTS5SendMessageWrapperTest : WrapperHelperTest<TNNNCTS5SendMessageWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if ArrivalMovementHeader is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "ArrivalMovementHeader"), () => GetWrapper(Factory.New<NctsHeader>()));

				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

				AssertExceptionThrown("Constructor Throws Exception if HeaderTNN is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "HeaderTNN"), () => GetWrapper(nctsHeader));
			});
		}

		public void TestTransitOperation()
		{
			var transitOperation = wrapper.TransitOperation;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled TransitOperation", transitOperation);
				AssertSame("Cached TransitOperation", wrapper.TransitOperation, transitOperation);
			});
		}

		public void TestCustomsOfficeOfDeparture()
		{
			departureMovement.CustomsOffices.RemoveAndDeleteAll();

			var customsOffice1 = departureMovement.CustomsOffices.AddNew();
			customsOffice1.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
			customsOffice1.CY_Data = "FR008889";

			var customsOffice2 = departureMovement.CustomsOffices.AddNew();
			customsOffice2.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;
			customsOffice2.CY_Data = "DE000002";

			wrapper = GetWrapper(nctsHeaderArrival);
			AssertEquals("Expected filled CustomsOfficeOfDeparture with NCTSOfficeOfDeparture office code when declared in CustomsOffice list", "FR008889", wrapper.CustomsOfficeOfDeparture);
		}

		public void TestCustomsOfficeOfDestinationDeclared()
		{
			departureMovement.CustomsOffices.RemoveAndDeleteAll();

			var customsOffice1 = departureMovement.CustomsOffices.AddNew();
			customsOffice1.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
			customsOffice1.CY_Data = "FR008889";

			var customsOffice2 = departureMovement.CustomsOffices.AddNew();
			customsOffice2.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;
			customsOffice2.CY_Data = "DE000002";

			wrapper = GetWrapper(nctsHeaderArrival);
			AssertEquals("Expected filled CustomsOfficeOfDestinationDeclared with NCTSOfficeOfDestination office code when declared in CustomsOffice list", "DE000002", wrapper.CustomsOfficeOfDestinationDeclared);
		}

		public void TestCustomsOfficeOfDestinationActual()
		{
			departureMovement.CustomsOffices.RemoveAndDeleteAll();

			var customsOffice1 = departureMovement.CustomsOffices.AddNew();
			customsOffice1.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
			customsOffice1.CY_Data = "FR008889";

			var customsOffice2 = departureMovement.CustomsOffices.AddNew();
			customsOffice2.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;
			customsOffice2.CY_Data = "DE000002";

			nctsHeaderArrival.ArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival = "ES002800";

			wrapper = GetWrapper(nctsHeaderArrival);
			AssertEquals("Expected filled CustomsOfficeOfDestinationActual with DestinationCustomsOfficeCodeForArrival", "ES002800", wrapper.CustomsOfficeOfDestinationActual);
		}

		public void TestNullHolderOfTheTransitProcedure()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.HolderOfTheTransitProcedure.ToString());
		}

		public void TestHolderOfTheTransitProcedure()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				nctsHeaderTNN.Principal.OrganisationPK = orgHeader.PK;
				wrapper = GetWrapper(nctsHeaderArrival);
				var holderOfTheTransitProcedure = wrapper.HolderOfTheTransitProcedure;

				AssertNotNull("Expected filled HolderOfTheTransitProcedure", holderOfTheTransitProcedure);
				AssertSame("Cached HolderOfTheTransitProcedure", wrapper.HolderOfTheTransitProcedure, holderOfTheTransitProcedure);
			});
		}

		public void TestNullTraderAtDestination()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.TraderAtDestination.ToString());
		}

		public void TestTraderAtDestination()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				nctsHeaderArrival.DestinationTrader.OrganisationPK = orgHeader.PK;

				wrapper = GetWrapper(nctsHeaderArrival);
				var traderAtDestination = wrapper.TraderAtDestination;
				AssertNotNull("Expected filled TraderAtDestination", wrapper.TraderAtDestination);
				AssertSame("Cached TraderAtDestination", wrapper.TraderAtDestination, traderAtDestination);
			});
		}

		public void TestNullRepresentativeAtDestination()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.RepresentativeAtDestination.ToString());
		}

		public void TestRepresentativeAtDestination()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				nctsHeaderArrival.ArrivalMovementHeader.Representative.OrganisationPK = orgHeader.PK;

				wrapper = GetWrapper(nctsHeaderArrival);
				var representative = wrapper.RepresentativeAtDestination;
				AssertNotNull("Expected filled Representative", wrapper.RepresentativeAtDestination);
				AssertSame("Cached Representative", wrapper.RepresentativeAtDestination, representative);

				nctsHeaderTNN.Principal.OrganisationPK = orgHeader.PK;
				wrapper = GetWrapper(nctsHeaderArrival);
				AssertNull("Expected null RepresentativeAtDestination when its the same as the departure's Principal", wrapper.RepresentativeAtDestination);
			});
		}

		public void TestDigitizedDocument()
		{
			CombineAssertions(() =>
			{
				AssertNull("Expected empty DigitizedDocument when no annex is declared", wrapper.DigitizedDocument);

				var eDoc1 = nctsHeaderArrival.DocManagerInfo.AddFileOrDocument(new byte[1], "file.pdf", "CIV");
				var pivot1 = nctsHeaderTNN.EDocPivotCollection.AddNew();
				pivot1.CSD_StorageDocReference = eDoc1.UniqueKey;

				wrapper = GetWrapper(nctsHeaderArrival);
				var digitizedDocument = wrapper.DigitizedDocument;
				AssertNotNull("Expected filled DigitizedDocument", digitizedDocument);
				AssertSame("Cached DigitizedDocument", wrapper.DigitizedDocument, digitizedDocument);
			});
		}

		public void TestConsignment()
		{
			var consignment = wrapper.Consignment;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Consignment", consignment);
				AssertSame("Cached Consignment", wrapper.Consignment, consignment);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var mrnCode = "1234567890";

			nctsHeaderArrival = Factory.New<NctsHeader>();
			nctsHeaderArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderArrival.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderArrival.ArrivalMrnFromUser = mrnCode;
			nctsHeaderArrival.ESNctsHeader.CEN_TNNArrival = true;

			nctsHeaderTNN = Factory.New<NctsHeader>();
			nctsHeaderTNN.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderTNN.ESNctsHeader.CEN_TNNArrival = true;
			nctsHeaderTNN.MovementReferenceEntryNumber.CE_EntryNum = mrnCode;
			var tnnMovement = nctsHeaderTNN.MovementHeader;
			tnnMovement.BM_Phase = DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration;

			departureMovement = nctsHeaderTNN.MovementHeader;

			nctsHeaderArrival.ArrivalMovementHeader.BM_BM_DepartureMovement = tnnMovement.PK;

			wrapper = GetWrapper(nctsHeaderArrival);
		}

		NctsHeader nctsHeaderArrival;
		NctsHeader nctsHeaderTNN;
		NctsDepartureMovementHeader departureMovement;
		TNNNCTS5SendMessageWrapper wrapper;

		TNNNCTS5SendMessageWrapper GetWrapper(NctsHeader header) => new TNNNCTS5SendMessageWrapper(header, Certificate);

		protected override TNNNCTS5SendMessageWrapper GetProvider() => wrapper;
	}
}
