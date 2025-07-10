using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class DeclarationWrapperBaseOnlyTest : Customs.Business.Testing.DataProviderTestCase<DeclarationWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new DeclarationWrapperBase(null));
		}

		public void TestIsProduction()
		{
			AssertEquals(false, wrapper.IsProduction);
		}

		public void TestLocalReferenceNumber()
		{
			AssertEquals(header.BH_JobReference, wrapper.LocalReferenceNumber);
		}

		public void TestMovementReferenceNumber()
		{
			var entryNum = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, header.CountryCode);
			entryNum.CE_EntryNum = "123456";
			AssertEquals("123456", wrapper.MovementReferenceNumber);
		}

		public void TestPrincipal()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "PRINCIPAL NAME";
			header.Principal.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("PRINCIPAL NAME", wrapper.Principal.Name);
		}

		public void TestConsignee()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "CONSIGNEE NAME";
			header.Consignee.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("CONSIGNEE NAME", wrapper.Consignee.Name);
		}

		public void TestDepartureCustomsOfficeReferenceNumber()
		{
			var cusOffice = header.MovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			cusOffice.CY_Data = "DE0345";
			AssertEquals("DE0345", wrapper.DepartureCustomsOfficeReferenceNumber);
		}

		public void TestDestinationCustomsOfficeReferenceNumber()
		{
			var cusOffice = header.MovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			cusOffice.CY_Data = "FR0599";
			AssertEquals("FR0599", wrapper.DestinationCustomsOfficeReferenceNumber);
		}

		public void TestIsAddressExtended()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				AssertEquals("IsAddressExtended", true, wrapper.IsAddressExtended);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				AssertEquals("IsAddressExtended", false, wrapper.IsAddressExtended);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			wrapper = new DeclarationWrapperBase(header);
		}
		NctsHeader header;
		DeclarationWrapperBase wrapper;

		protected override DeclarationWrapper GetProvider()
		{
			header.Principal.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			header.Consignee.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			return wrapper;
		}

		protected override IEnumerable<Expression<Func<DeclarationWrapper, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Principal;
			yield return x => x.Consignee;
		}
	}

	class DeclarationWrapperBase : DeclarationWrapper
	{
		public DeclarationWrapperBase(NctsHeader header) : base(header)
		{
		}

		public new ZBool IsAddressExtended => base.IsAddressExtended;
	}
}
