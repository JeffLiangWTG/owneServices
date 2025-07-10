using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class CusLineTariffDetailHelperTest : TestCaseWithFactory
	{
		public void TestIsPercentAlcoholMandatory()
		{
			const string tariffCode20 = "2050";
			const string tariffCode30 = "3050";
			const string tariffCode40 = "4050";
			const string tariffCode50 = "5050";
			const string tariffCodeWithNonMatchingEXCAttribute = "1050";

			var percentAlcoholMandatoryMatrix = new Dictionary<string, string[]>()
			{
				{ tariffCode20, new[] { CusLineTariffDetailHelper.KnownUoMs.HLT } },
				{ tariffCode30, new[] { CusLineTariffDetailHelper.KnownUoMs.HLT, CusLineTariffDetailHelper.KnownUoMs.HLT6 } },
				{ tariffCode40, new[] { CusLineTariffDetailHelper.KnownUoMs.HLT } },
				{ tariffCode50, new[] { CusLineTariffDetailHelper.KnownUoMs.HLT } }
			};

			var universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);

			var exciseTariffTypePK = TestHelper.CreateExciseTariffType(universalReferenceTestDataHelper).PK;
			var tariff20 = universalReferenceTestDataHelper.CreateTariff(Core.Constants.CountryCodes.Germany, exciseTariffTypePK, tariffCode20, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff30 = universalReferenceTestDataHelper.CreateTariff(Core.Constants.CountryCodes.Germany, exciseTariffTypePK, tariffCode30, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff40 = universalReferenceTestDataHelper.CreateTariff(Core.Constants.CountryCodes.Germany, exciseTariffTypePK, tariffCode40, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff50 = universalReferenceTestDataHelper.CreateTariff(Core.Constants.CountryCodes.Germany, exciseTariffTypePK, tariffCode50, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariffWithoutnonMatchingEXCAttribute = universalReferenceTestDataHelper.CreateTariff(Core.Constants.CountryCodes.Germany, exciseTariffTypePK, tariffCodeWithNonMatchingEXCAttribute, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var attribute10 = universalReferenceTestDataHelper.CreateTariffAttribute("ExciseType", CusLineTariffDetailHelper.ExciseTypes._10, tariffWithoutnonMatchingEXCAttribute);
			var attribute20 = universalReferenceTestDataHelper.CreateTariffAttribute("ExciseType", CusLineTariffDetailHelper.ExciseTypes._20, tariff20);
			var attribute30 = universalReferenceTestDataHelper.CreateTariffAttribute("ExciseType", CusLineTariffDetailHelper.ExciseTypes._30, tariff30);
			var attribute40 = universalReferenceTestDataHelper.CreateTariffAttribute("ExciseType", CusLineTariffDetailHelper.ExciseTypes._40, tariff40);
			var attribute50 = universalReferenceTestDataHelper.CreateTariffAttribute("ExciseType", CusLineTariffDetailHelper.ExciseTypes._50, tariff50);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invHeader.InvoiceLines.AddNew();
			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();

			tariffDetail.BZ_Type = Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Excise;
			CombineAssertions(() =>
			{
				foreach (var mandatoryEntry in percentAlcoholMandatoryMatrix)
				{
					tariffDetail.BZ_Tariff = mandatoryEntry.Key;

					tariffDetail.BZ_UQ1 = mandatoryEntry.Value[0];
					AssertEquals(GetTestCaseDescription("Tariff code with matching EXC attribute exists and UQ1 matches: "), expected: true, CusLineTariffDetailHelper.IsPercentAlcoholMandatory(tariffDetail));

					if (mandatoryEntry.Value.Length == 2)
					{
						tariffDetail.BZ_UQ1 = mandatoryEntry.Value[1];
						AssertEquals(GetTestCaseDescription("Tariff code with matching EXC attribute exists and UQ1 matches: "), expected: true, CusLineTariffDetailHelper.IsPercentAlcoholMandatory(tariffDetail));
					}

					tariffDetail.BZ_UQ1 = "KGM";
					AssertEquals(GetTestCaseDescription("Tariff code with matching EXC attribute exists but UQ1 doesn't match: "), expected: false, CusLineTariffDetailHelper.IsPercentAlcoholMandatory(tariffDetail));
				}

				tariffDetail.BZ_Tariff = "1010";
				AssertEquals(GetTestCaseDescription("Non present tariff code: "), expected: false, CusLineTariffDetailHelper.IsPercentAlcoholMandatory(tariffDetail));

				tariffDetail.BZ_Tariff = tariffCodeWithNonMatchingEXCAttribute;
				AssertEquals(GetTestCaseDescription("Tariff code exists but non matching EXC attribute: "), expected: false, CusLineTariffDetailHelper.IsPercentAlcoholMandatory(tariffDetail));
			});

			string GetTestCaseDescription(string prefix) => $"{prefix}BZ_Tariff {tariffDetail.BZ_Tariff}, BZ_UQ1 {tariffDetail.BZ_UQ1}";
		}
	}
}
