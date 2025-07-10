using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationAdditionTaxProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.Fees.GetOrAddFeeByFeeType(Constants.RateTypes.IPI);
			entryLine.Fees.GetOrAddFeeByFeeType(Constants.RateTypes.Cofins);
			entryLine.Fees.GetOrAddFeeByFeeType(Constants.RateTypes.PIS);
			entryLine.Fees.GetOrAddFeeByFeeType(Constants.RateTypes.ICMS);
			entryLine.Fees.GetOrAddFeeByFeeType(ChargeTypesList.Codes.DTY);
			var antidumping = entryLine.Fees.GetOrAddFeeByFeeType(Constants.RateTypes.Antidumping);

			CombineAssertions(() =>
			{
				AssertEquals(0, DeclarationAdditionTaxProvider.New(null, ChargeTypesList.Codes.VAT).Count());
				AssertEquals(0, DeclarationAdditionTaxProvider.New(entryLine, ChargeTypesList.Codes.VAT).Count());
				AssertType<DeclarationIPITaxProvider>("IPI DeclarationIPITaxProvider", DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.IPI).ElementAt(0));
				AssertType<DeclarationPISCofinsTaxProvider>("COFINS DeclarationPISCofinsTaxProvider", DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.Cofins).ElementAt(0));
				AssertType<DeclarationPISCofinsTaxProvider>("PIS DeclarationPISCofinsTaxProvider", DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.PIS).ElementAt(0));
				AssertType<DeclarationDutyFeeProvider>("Duty DeclarationDutyFeeProvider", DeclarationAdditionTaxProvider.New(entryLine, ChargeTypesList.Codes.DTY).ElementAt(0));
				AssertNull("Antidumping should be null when CF_Rate = 0", DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.Antidumping).ElementAtOrDefault(0));
				antidumping.CF_Rate = 50m;
				AssertType<DeclarationAntiDumpingTaxProvider>("Antidumping should be DeclarationDutyFeeProvider", DeclarationAdditionTaxProvider.New(entryLine, Constants.RateTypes.Antidumping).ElementAt(0));
			});
		}
	}
}
