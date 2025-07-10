using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.GB.Business.NctsHeader;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging.Testing
{
	[TestedType(typeof(CC015BDeclarationWrapper))]
	class CC015BDeclarationWrapperTests : EU.NCTS.Business.Testing.DeclarationWrapperAbstractTest<CC015BDeclarationWrapper>
	{
		public void TestCarrier()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = org.MainAddress;
			org.OH_FullName = "CARRIER NAME";
			orgAddress.CompanyName = "COMPANY NAME";
			orgAddress.Address1 = "ADDRESS1";
			orgAddress.City = "CITY";
			orgAddress.Postcode = "POST1";
			orgAddress.OA_RN_NKCountryCode = "GB";
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "GB954131533000", Core.Constants.CountryCodes.UnitedKingdom);
			header.BH_OH_Carrier = org.PK;

			var carrier = wrapper.Carrier;
			AssertEquals("GB954131533000", carrier.TIN);
			AssertEquals("CARRIER NAME", carrier.Name);
			AssertEquals("ADDRESS1", carrier.StreetAndNumber);
			AssertEquals("POST1", carrier.PostalCode);
			AssertEquals("GB", carrier.CountryCode);
			AssertEquals("CITY", carrier.City);
			AssertEquals("EN", carrier.NameAndAddressLanguage);
		}

		public void TestIsSimplifiedNctsProcedure()
		{
			header.MovementHeader.BM_GONumber = EU.NCTS.Business.NctsControlResult.Codes.AuthorizedTrader;
			var wrapper2 = new CC015BDeclarationWrapper(header);
			AssertEquals("IsSimplifiedNctsProcedure should be true", true, wrapper2.IsSimplifiedNctsProcedure);

			header.MovementHeader.BM_GONumber = EU.NCTS.Business.NctsControlResult.Codes.ChargesCollected;
			wrapper2 = new CC015BDeclarationWrapper(header);
			AssertEquals("IsSimplifiedNctsProcedure should be false", false, wrapper2.IsSimplifiedNctsProcedure);
		}

		public void TestSeals_WithoutSealType()
		{
			header.MovementHeader.BM_SealType = "";

			var container = Factory.New<NctsDepartureHeaderContainer>();
			container.BC_Seal1 = "123456";
			container.BC_Seal2 = "987654";
			var item = header.Bills.AddNew().GoodsItems.AddNew();
			var containerPivot = item.ContainersPivots.AddNew();
			containerPivot.Container = container;
			containerPivot.ContainerSelected = true;
			wrapper = new CC015BDeclarationWrapper(header);
			AssertContainsExactElementsInAnyOrder("Container seals values", new ZString[] { "123456", "987654" }, wrapper.Seals.Select(x => x.SealIdentity));
		}

		public void TestIsConsignorDefinedAtGoodsItemLevel()
		{
			AssertEquals(true, wrapper.IsConsignorDefinedAtGoodsItemLevel);
		}

		public void TestIsConsigneeDefinedAtGoodsItemLevel()
		{
			AssertEquals(true, wrapper.IsConsigneeDefinedAtGoodsItemLevel);
		}

		public void TestHasSecurityAtGoodsItemLevel()
		{
			AssertEquals(false, wrapper.HasSecurityAtGoodsItemLevel);
		}

		protected override CC015BDeclarationWrapper GetProvider() => (CC015BDeclarationWrapper)wrapper;

		protected override void SetUp()
		{
			base.SetUp();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = org.MainAddress;
			orgAddress.OA_Address1 = "123 ABC";

			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goods = header.Bills.AddNew().GoodsItems.AddNew();
			goods.Consignee.OrganisationPK = org.PK;
			goods.Consignor.OrganisationPK = org.PK;

			wrapper = new CC015BDeclarationWrapper(header);
		}

		NctsHeader header;
		ICC015BDeclaration wrapper;
	}
}
