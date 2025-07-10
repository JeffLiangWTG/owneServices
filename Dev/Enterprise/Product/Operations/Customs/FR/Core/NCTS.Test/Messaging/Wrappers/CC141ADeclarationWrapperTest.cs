using CargoWise.Types;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	[TestedType(typeof(CC141AWrapper))]
	class CC141ADeclarationWrapperTest : EU.NCTS.Business.Testing.DeclarationWrapperAbstractTest<CC141AWrapper>
	{
		public void TestDeclarantTIN_IsDepartureFallback()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			org.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			header.Consignor.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("FR12345678900001", wrapper.DeclarantTIN);
		}

		public void TestPrincipalTIN()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			org.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			header.Principal.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("FR12345678900001", wrapper.PrincipalTIN);
		}

		public void TestAgreementNumber()
		{
			var org = DeclarationWrapperHelperTest.CreateOrgHeaderWithDTA(Factory, "CC0002");
			header.Principal.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("CC0002", wrapper.AgreementNumber);
		}

		public void TestIsTC11DeliveredByCustoms()
		{
			AssertEquals(false, wrapper.IsTC11DeliveredByCustoms);
		}

		public void TestTC11Date()
		{
			header.TC11Date = ZDateTime.Today;
			AssertEquals(EU.NCTS.Business.WrapperHelper.GetLongDate(ZDateTime.Today), wrapper.TC11Date);
		}

		public void TestIsQueryAvailableOnPaper()
		{
			AssertEquals(false, wrapper.IsQueryAvailableOnPaper);
		}

		public void TestQueryInformation()
		{
			AssertEquals(ZString.Empty, wrapper.QueryInformation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Departure);
			wrapper = new CC141AWrapper(header);
		}
		CC141AWrapper wrapper;
		NctsHeader header;

		protected override CC141AWrapper GetProvider() => wrapper;
	}
}
