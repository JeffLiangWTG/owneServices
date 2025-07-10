using CargoWise.Customs.FR.MessageDefinitions.DeltaT.TCL_NATIONAL;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	[TestedType(typeof(CC014AWrapper))]
	class CC014ADeclarationWrapperTest : EU.NCTS.Business.Testing.DeclarationWrapperAbstractTest<CC014AWrapper>
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
			var org = DeclarationWrapperHelperTest.CreateOrgHeaderWithDTA(Factory, "AC0004");
			header.Principal.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("AC0004", wrapper.AgreementNumber);
		}

		public void TestCancellationRegularJustification()
		{
			var wrapper = new CC014AWrapper(header);
			AssertEquals(JustificationReglementaireInvalidation.Item1, wrapper.CancellationRegularJustification);

			var log = header.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.CustomsEntryStatus.Code;
				log.SL_Reference = NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;
				log.SL_EventTime = new ZDateTime(2021, 3, 12);
			}
			wrapper = new CC014AWrapper(header);
			AssertEquals(JustificationReglementaireInvalidation.Item2, wrapper.CancellationRegularJustification);
		}

		public void TestCancellationDate()
		{
			AssertEquals(EU.NCTS.Business.WrapperHelper.GetLongDate(ZDateTime.Today), wrapper.CancellationDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			wrapper = new CC014AWrapper(header);
		}
		NctsHeader header;
		CC014AWrapper wrapper;

		protected override CC014AWrapper GetProvider() => wrapper;
	}
}
