using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(NctsDepartureHeaderProvider))]
	internal class NctsHeaderProviderBaseOnlyTest : NctsHeaderProviderAbstractTest<NctsDepartureHeaderProvider>
	{
		public void TestRepresentative()
		{
			AssertNull("not available", Provider.Representative);
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "REP", nctsHeader.MovementHeader.Representative, "1", traderTir: "GBR/022/1234567");
			var provider = new NctsDepartureHeaderProvider(nctsHeader);
			AssertNotNull("available", provider.Representative);
		}

		public void TestHolderOfTheTransitProcedure()
		{
			AssertNull(Provider.HolderOfTheTransitProcedure);
			CreatePrincipal();
			AssertNotNull(Provider.HolderOfTheTransitProcedure);
		}

		public void TestCustomsOfficeOfDeparture()
		{
			var customsOfficeOfDeparture = nctsHeader.MovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			customsOfficeOfDeparture.CY_Data = "DepID";

			AssertEquals("DepID", Provider.CustomsOfficeOfDeparture);
		}

		public void TestCustomsOfficeOfDestination()
		{
			var customsOfficeOfDestination = nctsHeader.MovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			customsOfficeOfDestination.CY_Data = "DesID";

			AssertEquals("DesID", Provider.CustomsOfficeOfDestination);
		}

		public void TestLRN()
		{
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRNOfTheNCT";
			AssertEquals("LRNOfTheNCT", Provider.LRN);
		}

		public void TestHolderOfTheTransitProcedureIdentificationNumber()
		{
			CreatePrincipal();
			AssertEquals("GBHOLDERID", Provider.HolderOfTheTransitProcedureIdentificationNumber);
		}

		public void TestMRN()
		{
			nctsHeader.ArrivalMrnFromUser = "MRNOfTheNCTS";
			AssertEquals("MRNOfTheNCTS", Provider.MRN);
		}

		public void TestHolderOfTheTransitProcedureTIRNumber()
		{
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			CreatePrincipal();
			AssertEquals("TIRNumber", Provider.HolderOfTheTransitProcedureTIRNumber);
		}

		public void TestHolderOfTheTransitProcedureTIRNumberNoTIRInBondEntryType()
		{
			nctsHeader.MovementHeader.BM_InBondEntryType = "OTH";
			AssertNull(Provider.HolderOfTheTransitProcedureTIRNumber);
		}

		public void TestMessageSender()
		{
			var companyOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = companyOrgProxy.PK;

			var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchOrgProxy.PK;

			var company = Factory.New<GlbCompany>();
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;

			var branch = company.Branches.AddNew();
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			nctsHeader.BH_GB = branch.PK;

			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("No Eori", string.Empty, Provider.MessageSender);

				var companyOrg = GlbCompany.CurrentCompany.OrgProxy;
				companyOrg.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "111", Core.Constants.CountryCodes.UnitedKingdom);
				AssertEquals("GlbCompany Eori exists", "GB111", GetProvider().MessageSender);

				companyOrg.CustomsCodes.RemoveAll();

				var branchOrg = branchOrgProxy;
				branchOrg.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "222", Core.Constants.CountryCodes.UnitedKingdom);
				AssertEquals("GlbBranch Eori exists", "GB222", GetProvider().MessageSender);
			});
		}

		public void TestMessageRecipient()
		{
			var movementHeader = nctsHeader.MovementHeader;
			AssertEquals("No DepartureCustomsOffice", ZString.Empty, nctsHeader.DepartureCustomsOfficeCode);
			NCTSTestHelper.CreateCustomsOfficeForTest(movementHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "GB000011", ZDateTime.Empty, true);
			AssertEquals("Has DepartureCustomsOffice", "GB000011", movementHeader.DepartureCustomsOfficeCode);
			AssertEquals("NTA.GB", Provider.MessageRecipient);
		}

		[TestDate(2020, 10, 28, 11, 38, 00)]
		public void TestPreparationDateTime()
		{
			AssertEquals(new DateTime(2020, 10, 28, 11, 38, 00), Provider.PreparationDateTime);
		}

		public void TestMessageIdentification()
		{
			AssertEquals("<<SENDERS REFERENCE PLACE HOLDER>>", Provider.MessageIdentification);
		}

		public void TestCorrelationIdentifierWithCIDEntryNum()
		{
			AssertEquals(Provider.MessageIdentification, Provider.CorrelationIdentifier);
		}

		protected override string MessageType => ZString.Empty;
		protected override string MovementType => NctsMovementType.Codes.Departure;
	}
}
