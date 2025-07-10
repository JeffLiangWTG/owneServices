using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Analyzer suggests BaseSupplementaryCode.Loader, which is less readible")]
	class SupplementaryCodeHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSupplementaryCodeSetter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var count = 0;
			invoiceLine.JI_SupplementaryCode1Info.ValueChanged += (s, e) =>
			{
				if (e is ValueChangedEventArgs ve)
				{
					count += ve.OldValue.Equals("S001") ? 2 : 1;
				}
			};

			var handler = new BaseSupplementaryCodeHandler<SupplementaryCode>();
			handler.LoadOrCreate("S001", invoiceLine, 1, invoiceLine.JI_SupplementaryCode1Info);

			var loader = new SupplementaryCode.Loader(Factory);
			var supplementaryCode = loader.Load<SupplementaryCode, JobComInvoiceLine>(invoiceLine, 1);
			NUnit.Framework.Assert.That(supplementaryCode, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.EU.Business.SupplementaryCode)), "SupplementaryCode - should not be [null]");
			CombineAssertions("Testing SupplementaryCode property", () =>
			{
				NUnit.Framework.Assert.That(supplementaryCode.CY_Code, NUnit.Framework.Is.EqualTo("S001").Using(CustomComparers.TypeComparison), "CY_Code");
				NUnit.Framework.Assert.That(invoiceLine.PK, NUnit.Framework.Is.EqualTo(supplementaryCode.CY_ParentID), "CY_ParentID");
				NUnit.Framework.Assert.That(invoiceLine.TablePrefix, NUnit.Framework.Is.EqualTo(supplementaryCode.CY_ParentTableCode).Using(CustomComparers.TypeComparison), "CY_ParentTableCode");
			});
			NUnit.Framework.Assert.That(count, NUnit.Framework.Is.EqualTo(1), "refreshBinding Called on JI_SupplementaryCode1Info");

			supplementaryCode = handler.LoadOrCreate("S002", invoiceLine, 1, invoiceLine.JI_SupplementaryCode1Info);
			NUnit.Framework.Assert.That(supplementaryCode.CY_Code, NUnit.Framework.Is.EqualTo("S002").Using(CustomComparers.TypeComparison), "CY_Code");
			NUnit.Framework.Assert.That(count, NUnit.Framework.Is.EqualTo(3), "refreshBinding Called on JI_SupplementaryCode1Info");

			supplementaryCode = handler.LoadOrCreate("", invoiceLine, 1, invoiceLine.JI_SupplementaryCode1Info);
			NUnit.Framework.Assert.That(supplementaryCode.IsDeleted, NUnit.Framework.Is.EqualTo(true), "IsDeleted");
			NUnit.Framework.Assert.That(count, NUnit.Framework.Is.EqualTo(4), "refreshBinding Called on JI_SupplementaryCode1Info");
		}

		[ExpectNoExceptions]
		public void TestGetCodeList()
		{
			SetUpTariffAndRate();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234512345";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_SecondaryPreference = "";
			invoiceLine.JI_ConcessionOrder = "";
			var loader = new SupplementaryCode.Loader(Factory);
			var code = loader.Load<SupplementaryCode, JobComInvoiceLine>(invoiceLine, 1);

			var list = SupplementaryCodeHelper.GetCodeList(invoiceLine);

			CombineAssertions(() => {
				NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(1), "Number of codes in list");
				NUnit.Framework.Assert.That(list.ContainsCode("additionalcode"), NUnit.Framework.Is.True, "List contains additionalcode");
				NUnit.Framework.Assert.That(list.GetDescriptionFromCode("additionalcode"), NUnit.Framework.Is.EqualTo("Additional Code 1 Descriptions"), "Additional code description");
			});
		}

		void SetUpTariffAndRate()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);
			var testHelper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;

			var tradeGroupStandard = testHelper.CreateTradeGroup(currentCountry, "STANDARD", date1, date4);
			testHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Australia, date1, date4);
			Factory.Save();

			var hsnTariffType = testHelper.CreateNewOrGetExistingTariffType(currentCountry, "IMP");
			Factory.Save();
			var dutyRateType = testHelper.CreateNewOrGetExistingRateType(currentCountry, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = testHelper.LoadOrCreateNewCusRateCode(Factory, "A00", dutyRateType.PK);
			Factory.Save();
			var preferenceSTD = testHelper.CreatePreferenceForCountry("STD", "Standard", currentCountry);
			Factory.Save();

			var cusTariff = testHelper.CreateTariff(currentCountry, hsnTariffType.PK, "1234512345", date1, date4, "dummy Description 0");
			Factory.Save();

			var testRate1 = testHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceSTD.PK);
			testHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, "additionalcode", "ordernumber");
			Factory.Save();

			testHelper.CreateCusCodeType("ADDCD", "Additional Codes");
			Factory.Save();
			testHelper.CreateCusCodeList(currentCountry, "ADDCD", "additionalcode", "Additional Code 1 Descriptions", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();
		}
	}
}
