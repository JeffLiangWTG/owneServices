using CargoWise.Types;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	[TestedType(typeof(CC007ADeclarationWrapper))]
	class CC007ADeclarationWrapperTest : EU.NCTS.Business.Testing.DeclarationWrapperAbstractTest<CC007ADeclarationWrapper>
	{
		public void TestDeclarantTIN_IsArrivalFallback()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			org.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			header.Consignee.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("FR12345678900001", wrapper.DeclarantTIN);
		}

		public void TestAgreementNumber_IsArrival()
		{
			var org = DeclarationWrapperHelperTest.CreateOrgHeaderWithDTA(Factory, "AC0002");
			header.Declarant.E2_OA_Address = ZGuid.Empty;
			header.Consignee.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("AC0002", wrapper.AgreementNumber);
		}

		public void TestArrivalNotificationPlace()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			header.ArrivalMovementHeader.BM_PlaceOfUnloading = "TEST";
			var wrapper = new CC007ADeclarationWrapper(header);
			AssertEquals("TEST", wrapper.ArrivalNotificationPlace);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			wrapper = new CC007ADeclarationWrapper(header);
		}
		NctsHeader header;
		CC007ADeclarationWrapper wrapper;

		protected override CC007ADeclarationWrapper GetProvider() => wrapper;
	}
}
