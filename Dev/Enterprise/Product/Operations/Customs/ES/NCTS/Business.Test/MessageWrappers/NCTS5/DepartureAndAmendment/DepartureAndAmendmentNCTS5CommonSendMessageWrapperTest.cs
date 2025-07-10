using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class DepartureAndAmendmentNCTS5CommonSendMessageWrapperTest : WrapperHelperTest<DepartureAndAmendmentNCTS5CommonSendMessageWrapper>
	{
		public void TestAuthorisations()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Authorisations", 0, wrapper.Authorisations.Count);

				var auth1 = departureMovement.CusAuthorizationUsages.AddNew();
				auth1.AGC_Code = "TRD";
				var auth2 = departureMovement.CusAuthorizationUsages.AddNew();
				auth2.AGC_Code = "AAA";

				wrapper = GetWrapper(nctsHeader);
				var authorisations = wrapper.Authorisations;
				AssertEquals("Expected filled Authorisations with count 2 (all codes)", 2, authorisations.Count);
				AssertSame("Cached Authorisations", wrapper.Authorisations, authorisations);
			});
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

			wrapper = GetWrapper(nctsHeader);
			AssertEquals("Expected filled CustomsOfficeOfDestinationDeclared with NCTSOfficeOfDestination office code when declared in CustomsOffice list", "DE000002", wrapper.CustomsOfficeOfDestinationDeclared);
		}

		public void TestCustomsOfficeOfTransitDeclared()
		{
			CombineAssertions(() =>
			{
				var customsOffice1 = departureMovement.CustomsOffices.AddNew();
				customsOffice1.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				customsOffice1.CY_Data = "FR008889";

				var customsOffice2 = departureMovement.CustomsOffices.AddNew();
				customsOffice2.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;
				customsOffice2.CY_Data = "DE000002";

				var customsOffice3 = departureMovement.CustomsOffices.AddNew();
				customsOffice3.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
				customsOffice3.CY_Data = "ES000001";

				var customsOffice4 = departureMovement.CustomsOffices.AddNew();
				customsOffice4.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
				customsOffice4.CY_Data = "ES009999";

				departureMovement.BM_InBondEntryType = "TIR";
				wrapper = GetWrapper(nctsHeader);
				AssertEquals("Expected empty CustomsOfficeOfTransitDeclared when declarationType is TIR", 0, wrapper.CustomsOfficeOfTransitDeclared.Count);

				departureMovement.BM_InBondEntryType = "T";
				wrapper = GetWrapper(nctsHeader);
				var customsOfficeOfTransitDeclared = wrapper.CustomsOfficeOfTransitDeclared;
				AssertEquals("Expected filled CustomsOfficeOfTransitDeclared with NCTSOfficeOfTransit office codes when declarationType is not TIR or T2SM", 2, customsOfficeOfTransitDeclared.Count);
				AssertSame("Cached CustomsOfficeOfTransitDeclared", wrapper.CustomsOfficeOfTransitDeclared, customsOfficeOfTransitDeclared);
				AssertContainsExactElementsInAnyOrder("Expected filled CustomsOfficeOfTransitDeclared with NCTSOfficeOfTransit, correct codes", new[] { "ES000001", "ES009999" }, customsOfficeOfTransitDeclared.Select(x => x.ReferenceNumber));

				departureMovement.BM_InBondEntryType = "T2SM";
				wrapper = GetWrapper(nctsHeader);
				AssertEquals("Expected empty CustomsOfficeOfTransitDeclared when declarationType is T2SM", 0, wrapper.CustomsOfficeOfTransitDeclared.Count);
			});
		}

		public void TestCustomsOfficeOfExitForTransitDeclared()
		{
			CombineAssertions(() =>
			{
				var customsOffice1 = departureMovement.CustomsOffices.AddNew();
				customsOffice1.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				customsOffice1.CY_Data = "FR008889";

				var customsOffice2 = departureMovement.CustomsOffices.AddNew();
				customsOffice2.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;
				customsOffice2.CY_Data = "DE000002";

				var customsOffice3 = departureMovement.CustomsOffices.AddNew();
				customsOffice3.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit;
				customsOffice3.CY_Data = "ES000001";

				var customsOffice4 = departureMovement.CustomsOffices.AddNew();
				customsOffice4.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit;
				customsOffice4.CY_Data = "ES009999";

				departureMovement.BM_TypeOfSecurity = "NON";
				wrapper = GetWrapper(nctsHeader);
				AssertEquals("Expected empty CustomsOfficeOfExitForTransitDeclared when securityType is NON (not EXI)", 0, wrapper.CustomsOfficeOfExitForTransitDeclared.Count);

				departureMovement.BM_TypeOfSecurity = "EXI";
				wrapper = GetWrapper(nctsHeader);
				var customsOfficeOfExitForTransitDeclared = wrapper.CustomsOfficeOfExitForTransitDeclared;
				AssertEquals("Expected filled CustomsOfficeOfExitForTransitDeclared with NCTSOfficeOfExitForTransit office codes when securityType is EXI", 2, customsOfficeOfExitForTransitDeclared.Count);
				AssertSame("Cached CustomsOfficeOfExitForTransitDeclared", wrapper.CustomsOfficeOfExitForTransitDeclared, customsOfficeOfExitForTransitDeclared);
				AssertContainsExactElementsInAnyOrder("Expected filled CustomsOfficeOfExitForTransitDeclared with NCTSOfficeOfExitForTransit, correct codes", new[] { "ES000001", "ES009999" }, customsOfficeOfExitForTransitDeclared.Select(x => x.ReferenceNumber));
			});
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
				nctsHeader.Principal.OrganisationPK = orgHeader.PK;
				wrapper = GetWrapper(nctsHeader);
				var holderOfTheTransitProcedure = wrapper.HolderOfTheTransitProcedure;

				AssertNotNull("Expected filled HolderOfTheTransitProcedure", holderOfTheTransitProcedure);
				AssertSame("Cached HolderOfTheTransitProcedure", wrapper.HolderOfTheTransitProcedure, holderOfTheTransitProcedure);
				AssertNotNull("Expected filled HolderOfTheTransitProcedure.ContactPerson when Representative is not declared", holderOfTheTransitProcedure.ContactPerson);

				var orgHeaderRep = Factory.NewWithValidTestData<OrgHeader>();
				nctsHeader.MovementHeader.Representative.OrganisationPK = orgHeaderRep.PK;
				wrapper = GetWrapper(nctsHeader);
				holderOfTheTransitProcedure = wrapper.HolderOfTheTransitProcedure;
				AssertNotNull("Expected filled HolderOfTheTransitProcedure when Representative is declared", holderOfTheTransitProcedure);
				AssertNull("Expected null HolderOfTheTransitProcedure.ContactPerson when Representative is declared", holderOfTheTransitProcedure.ContactPerson);
			});
		}

		public void TestGuarantee()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Guarantees when no data declared", 0, wrapper.Guarantee.Count);

				var guarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
				guarantee1.PW_BondType = "3";
				guarantee1.PW_BondNumber = "GRN1";

				var guarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();
				guarantee2.PW_BondType = "5";
				guarantee2.PW_BondNumber = "GRN2";

				var guarantee3 = nctsHeader.MovementHeader.Guarantees.AddNew();
				guarantee3.PW_BondType = "3";
				guarantee3.PW_BondNumber = "GRN3";

				wrapper = GetWrapper(nctsHeader);
				var guarantee = wrapper.Guarantee;
				AssertEquals("Expected filled Guarantee with the declared guarantees", 2, guarantee.Count);
				AssertSame("Cached Guarantee", wrapper.Guarantee, guarantee);

				var guaranteeList = guarantee.ToArray();
				AssertEquals("For first guarantee expected filled GuaranteeType", "3", guaranteeList[0].GuaranteeType);
				AssertContainsExactElementsInAnyOrder("For first guarantee expected filled GuaranteeReference with correct data", new ZString[] { "GRN1", "GRN3" }, guaranteeList[0].GuaranteeReference.Select(x => x.GRN));
				AssertEquals("For first guarantee expected filled SequenceNumber", "1", guaranteeList[0].SequenceNumber);

				AssertEquals("For second guarantee expected filled GuaranteeType", "5", guaranteeList[1].GuaranteeType);
				AssertContainsExactElementsInAnyOrder("For second guarantee expected filled GuaranteeReference with correct data", new ZString[] { "GRN2" }, guaranteeList[1].GuaranteeReference.Select(x => x.GRN));
				AssertEquals("For second guarantee expected filled SequenceNumber", "2", guaranteeList[1].SequenceNumber);
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

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureMovement = nctsHeader.MovementHeader;

			wrapper = GetWrapper(nctsHeader);
		}

		NctsHeader nctsHeader;
		NctsDepartureMovementHeader departureMovement;
		DepartureAndAmendmentNCTS5CommonSendMessageWrapper wrapper;

		DepartureAndAmendmentNCTS5CommonSendMessageWrapper GetWrapper(NctsHeader header, string messageType = DeclarationMessageTypeList.Codes.Ncts5Departure) => new DepartureAndAmendmentNCTS5CommonSendMessageWrapper(header, Certificate, messageType);

		protected override DepartureAndAmendmentNCTS5CommonSendMessageWrapper GetProvider() => wrapper;
	}
}
