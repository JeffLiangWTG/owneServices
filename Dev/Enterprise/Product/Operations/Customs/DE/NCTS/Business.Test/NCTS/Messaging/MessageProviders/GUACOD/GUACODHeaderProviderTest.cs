using Enterprise.Customs.DE.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class GUACODHeaderProviderTest : Customs.Business.Testing.DataProviderTestCase<GUACODHeaderProvider>
	{
		public void TestHolderOfTransitProcedure()
		{
			AssertEquals("DEEOR1", Provider.HolderOfTransitProcedure);
		}

		public void TestCustomsOfficeOfGuarantee()
		{
			AssertEquals("DE003302", Provider.CustomsOfficeOfGuarantee);
		}

		public void TestEffectiveDate()
		{
			AssertNull(Provider.EffectiveDate);
		}

		public void TestGRN()
		{
			AssertEquals("GRN12354678", Provider.GRN);
		}

		public void TestAccessCodeCurrent()
		{
			AssertEquals("CURR", Provider.AccessCodeCurrent);
		}

		public void TestAccessCodeNew()
		{
			AssertEquals("NEWC", Provider.AccessCodeNew);
		}

		public void TestAccessCodes()
		{
			AssertSequencesEqual(new[] { "COD1", "COD2" }, Provider.AccessCodes);
		}

		protected override GUACODHeaderProvider GetProvider() => new GUACODHeaderProvider(changeAccessCode);

		protected override void SetUp()
		{
			base.SetUp();

			var permitHolder = Factory.New<OrgHeader>();
			permitHolder.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR1", Core.Constants.CountryCodes.Germany);

			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader.CPH_OH_PermitHolder = permitHolder.PK;
			guaranteeHeader.CPH_Number = "GRN12354678";
			guaranteeHeader.MainAccessCode = "CURR";
			guaranteeHeader.AdditionalAccessCodes.AddNew().CPR_ValueFrom = "COD1";
			guaranteeHeader.AdditionalAccessCodes.AddNew().CPR_ValueFrom = "COD2";

			changeAccessCode = new SendAccessCodeViewModel(guaranteeHeader);
			changeAccessCode.OfficeOfGuarantee = "DE003302";
			changeAccessCode.NewMainAccessCode = "NEWC";
		}

		SendAccessCodeViewModel changeAccessCode;
	}
}
