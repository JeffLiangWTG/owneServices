using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.GDM.Testing
{
	[TestedType(typeof(GuidedDecisionMakingBasic))]
	sealed class GuidedDecisionMakingBasicTest : NonPersistentBusinessObjectTestCase
	{
		public void TestRegionOrTerritoryOfDestination()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_RegionOrTerritoryOfDestination = "AAA";
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var gdmSource = new GuidedDecisionMakingSingleInvoiceLineSource(invoiceLine);
			var gdmBasic = new GuidedDecisionMakingBasic(gdmSource, Factory);
			AssertEquals("GDMBasic can get the RegionOrTerritoryOfDestination from parent declaration of the invoice line", "AAA", gdmBasic.RegionOrTerritoryOfDestination);

			AssertEquals("Caption of RegionOrTerritoryOfDestination", "Region", DataBoundResourceStrings.GetDataForProperty(typeof(GuidedDecisionMakingBasic), nameof(GuidedDecisionMakingBasic.RegionOrTerritoryOfDestination)).Caption);
			AssertEquals("MaxLength of RegionOrTerritoryOfDestination", 5, gdmBasic.RegionOrTerritoryOfDestinationInfo.MaxLength);
		}

		public void TestTariffAdditionalCodeSelectionCriteria()
		{
			AssertTariffAdditionalCodeSelectionCriterias(JobMessageTypeList.Codes.Import);
			AssertTariffAdditionalCodeSelectionCriterias(JobMessageTypeList.Codes.Export);
		}

		void AssertTariffAdditionalCodeSelectionCriterias(ZString direction)
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			guidedDecisionMakingBasic.CountryOfOrigin = "DE";
			guidedDecisionMakingBasic.CountryOfDestination = "US";
			guidedDecisionMakingBasic.DataGrouping = "FR";
			guidedDecisionMakingBasic.EffectiveDate = new ZDate(2024, 03, 22);
			guidedDecisionMakingBasic.IsImport = direction == JobMessageTypeList.Codes.Import;
			guidedDecisionMakingBasic.IsExport = direction == JobMessageTypeList.Codes.Export;
			var tariffAdditionalCodeSelectionCriterias = guidedDecisionMakingBasic.TariffAdditionalCodeSelectionCriteria;

			AssertEquals("Category", direction == JobMessageTypeList.Codes.Import ? "SIP" : "SEP", tariffAdditionalCodeSelectionCriterias.Category);
			AssertEquals("EffectiveDate", new ZDate(2024, 03, 22), tariffAdditionalCodeSelectionCriterias.EffectiveDate);
			AssertEquals("TradeGroupCountry", direction == JobMessageTypeList.Codes.Export ? "US" : "DE", tariffAdditionalCodeSelectionCriterias.TradeGroupCountry);
			AssertEquals("DataGrouping", "FR", tariffAdditionalCodeSelectionCriterias.DataGrouping);
		}

		public void TestLoadGuidedDecisionMakingAdditionalCodeCollection()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var date1 = new ZDate(2010, 12, 10);
			var date2 = new ZDate(2079, 06, 06);
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			helper.CreateCusTariffAdditionalCodeCategory(dataGrouping, "SIP");
			helper.CreateCusTariffAdditionalCodeCategory(dataGrouping, "SEP");
			var tradeGroup = helper.CreateTradeGroup(dataGrouping, "CN-FR", date1, date2, "STANDARD DEC");
			helper.AddCountry(tradeGroup, "CN");
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, "EXP");
			Factory.Save();

			var cusTariff = helper.CreateTariff(dataGrouping, tariffType.PK, "1000000000", date1, date2, "dummy Description 0");
			var additionalCode1 = helper.CreateTariffAdditionalCodeView(cusTariff, "SEP", "SEP1");
			helper.CreateCusApplicability(additionalCode1, tradeGroup, date1, date2);
			var additionalCode2 = helper.CreateTariffAdditionalCodeView(cusTariff, "SIP", "SIP1", dataGrouping);
			helper.CreateCusApplicability(additionalCode2, tradeGroup, date1, date2);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_Tariff = "1000000000";
			var guidedDecisionMakingBasic = new GuidedDecisionMakingBasic(new GuidedDecisionMakingSingleInvoiceLineSource(invoiceLine), Factory);
			var additionalCodes = new GuidedDecisionMakingAdditionalCodeCollection(guidedDecisionMakingBasic);
			AssertEquals("Count", 1, additionalCodes.Count);
			AssertAdditionalCode(additionalCodes[0], "SIP", "Statistical Add. Codes: SIP SIP DESC", "SIP1");

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var invoice2 = declaration2.Invoices.AddNew();
			invoice2.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CountryOfOrigin = "CN";
			invoiceLine2.JI_Tariff = "1000000000";
			guidedDecisionMakingBasic = new GuidedDecisionMakingBasic(new GuidedDecisionMakingSingleInvoiceLineSource(invoiceLine2), Factory);
			guidedDecisionMakingBasic.CountryOfDestination = "CN";
			additionalCodes = new GuidedDecisionMakingAdditionalCodeCollection(guidedDecisionMakingBasic);
			AssertEquals("Count", 1, additionalCodes.Count);
			AssertAdditionalCode(additionalCodes[0], "SEP", "Statistical Add. Codes: SEP SEP DESC", "SEP1");
		}

		void AssertAdditionalCode(GuidedDecisionMakingAdditionalCode gDMAdditionalCode, ZString applicableToType, ZString applicableToDescription, ZString additionalCode)
		{
			AssertEquals("ApplicableToType", applicableToType, gDMAdditionalCode.ApplicableToType);
			AssertEquals("ApplicableToDescription", applicableToDescription, gDMAdditionalCode.ApplicableToDescription);
			AssertEquals("AdditionalCode", additionalCode, gDMAdditionalCode.AdditionalCode);
		}

		public void TestTransferToTarget()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			guidedDecisionMakingBasic.RegionOrTerritoryOfDestination = "REG11";
			var add1 = guidedDecisionMakingBasic.AdditionalCodes.AddNew();
			add1.IsTicked = true;
			add1.AdditionalCode = "ADD1";
			guidedDecisionMakingBasic.MeursingResult = "7010";
			var docA = guidedDecisionMakingBasic.DocumentConditions.AddNew();
			var con1 = docA.ConditionDetails.AddNew();
			con1.Code = "4001";
			con1.Reference = "REF001";
			con1.DateOfIssue = new ZDateTime(2023, 06, 22);
			con1.IsTicked = true;

			var target = new Mock<IGuidedDecisionMakingTarget>();
			guidedDecisionMakingBasic.TransferToTarget(target.Object);
			target.Verify(x => x.SetAdditionalCodes(It.Is<List<ZString>>(y => y.ContainsSameElementsInAnyOrder(new ZString[]
			{
				"ADD1", "7010"
			}))), Times.Once);
			var expectedSupportingDocuments = new List<(ZString, ZString, ZDateTime DateOfIssue)>
			{
				("4001", "REF001", new ZDateTime(2023, 06, 22))
			};
			target.Verify(x => x.SetSupportingAndAdditionalDocuments(It.Is<List<(ZString, ZString, ZDateTime DateOfIssue)>>(y => y.ContainsSameElementsInAnyOrder(expectedSupportingDocuments))), Times.Once);
			target.VerifySet(x => x.CountryOfOrigin = "FR", Times.Once);
			target.VerifySet(x => x.Preference = "P1", Times.Once);
			target.VerifySet(x => x.QuotaOrderNumber = "Number1", Times.Once);
			target.VerifySet(x => x.TariffCode = "1111111111", Times.Once);
			target.VerifySet(x => x.CustomsFirstQuantity = 100m, Times.Once);
			target.VerifySet(x => x.CustomsFirstUnitQty = "KGM", Times.Once);
			target.VerifySet(x => x.CustomsSecondQuantity = 200m, Times.Once);
			target.VerifySet(x => x.CustomsSecondUnitQty = "LPA", Times.Once);
			target.VerifySet(x => x.CustomsThirdQuantity = 300m, Times.Once);
			target.VerifySet(x => x.CustomsThirdUnitQty = "HLT", Times.Once);
			target.VerifySet(x => x.RegionOrTerritoryOfDestination = "REG11", Times.Once);
			Assert(true);
		}

		public void TestRateSelectionCriteria()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			guidedDecisionMakingBasic.RegionOrTerritoryOfDestination = "CONTI";
			var rateSelectionCriteria = guidedDecisionMakingBasic.RateSelectionCriteria;
			CombineAssertions("RateSelectionCriteria default value", () =>
			{
				AssertType<SpecificRateSelectionCriteria>(rateSelectionCriteria);
				AssertEquals("TradeGroupCountry", guidedDecisionMakingBasic.CountryOfOrigin, rateSelectionCriteria.TradeGroupCountry);
				AssertEquals("DataGrouping", guidedDecisionMakingBasic.DataGrouping, rateSelectionCriteria.DataGrouping);
				AssertEquals("PrimaryPreference", guidedDecisionMakingBasic.Preference, rateSelectionCriteria.PrimaryPreference);
				AssertEquals("ConcessionOrder", guidedDecisionMakingBasic.QuotaOrderNumber, rateSelectionCriteria.ConcessionOrder);
				AssertContainsExactElementsInAnyOrder("AdditionalCodes", guidedDecisionMakingBasic.AdditionalCodes.Select(x => x.AdditionalCode), rateSelectionCriteria.AdditionalCodes.ToList());
				AssertEquals("EffectiveDate", guidedDecisionMakingBasic.EffectiveDate, rateSelectionCriteria.EffectiveDate);
				AssertEquals("RateType", "", rateSelectionCriteria.RateType);
				AssertEquals("RateCode", "", rateSelectionCriteria.RateCode);
				AssertContainsExactElementsInAnyOrder("SecondaryTradeGroup", new string[] { "CONTI", "METRO" }, rateSelectionCriteria.SecondTradeGroups);
				AssertEquals("Direction", RateDirection.Import, rateSelectionCriteria.Direction);
			});
		}

		public void TestVATApplicabilities()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			var vats = guidedDecisionMakingBasic.VATApplicabilities;
			AssertType<GuidedDecisionMakingVATCollection>(vats);
		}

		public void TestPreferredLanguage()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkingLanguage = Core.SharedConstants.Languages.German;
			var homeBranch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), homeBranch.PK.ToGuid(), Guid.Empty))
			{
				var gdmBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
				AssertEquals("PreferredLanguage should always return French.", Core.SharedConstants.Languages.French, gdmBasic.PreferredLanguage);
			}
		}

		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			if (info.Name.Equals("TariffCode"))
			{
				return;
			}
			base.TestBizObjectField(info);
		}

		protected override BusinessObject GetNewBusinessObject() => GuidedDecisionMakingBasicTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory);
	}
}
