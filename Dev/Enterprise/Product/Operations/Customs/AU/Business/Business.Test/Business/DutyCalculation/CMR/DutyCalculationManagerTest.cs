using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DutyCalculationManagerTest : TestCaseWithFactory
	{
		public void TestDutyGSTAndWoodLevyN10()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("Q1A", 10.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q2A", 15.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q1S", 50.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q2S", 55.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			var taxL = helper.CreateTaxOrFee("DAN", 50.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			taxL.ZZF_Threshold = 10000m;
			var taxH = helper.CreateTaxOrFee("DAH", 100.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			Factory.Save();

			var declaration = GetWoodLevyTestDeclaration();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Assert("inv line1 estimated flag is off", !line1.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line1 duty", 750m, line1.JI_Calc_DutyAmount);
			AssertEquals("inv line1 duty estimated", 750m, line1.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line1 GST", 1575m, line1.JI_Calc_GSTVATAmount);
			AssertEquals("inv line1 GST estimated", 1575m, line1.JI_Calc_GSTVATAmountIncludingWHEstimate);
			Assert("inv line2 estimated flag is off", !line2.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line2 duty", 2250m, line2.JI_Calc_DutyAmount);
			AssertEquals("inv line2 duty estimated", 2250m, line2.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line2 GST", 4725m, line2.JI_Calc_GSTVATAmount);
			AssertEquals("inv line2 GST estimated", 4725m, line2.JI_Calc_GSTVATAmountIncludingWHEstimate);

			Assert("inv line3 estimated flag is off", !line3.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line3 duty", 750m, line3.JI_Calc_DutyAmount);
			AssertEquals("inv line3 duty estimated", 750m, line3.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line3 GST", 1575m, line3.JI_Calc_GSTVATAmount);
			AssertEquals("inv line3 GST estimated", 1575m, line3.JI_Calc_GSTVATAmountIncludingWHEstimate);
			Assert("inv line4 estimated flag is off", !line4.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line4 duty", 2250m, line4.JI_Calc_DutyAmount);
			AssertEquals("inv line4 duty estimated", 2250m, line4.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line4 GST", 4725m, line4.JI_Calc_GSTVATAmount);
			AssertEquals("inv line4 GST estimated", 4725m, line4.JI_Calc_GSTVATAmountIncludingWHEstimate);

			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine1 = entry.MergedLines[0];
			var entryLine2 = entry.MergedLines[1];
			Assert("line 1 estimate flag is off", !entryLine1.IsDutyAndTaxEstimatedForWH);
			AssertEquals("line 1 Duty", 3000m, entryLine1.DutyAmount);
			AssertEquals("line 1 Duty Estimate", 3000m, entryLine1.DutyAmountIncludingWHEstimate);
			AssertEquals("line 1 GST", 6300m, entryLine1.GSTVATAmount);
			AssertEquals("line 1 GST Estimate", 6300m, entryLine1.GSTVATAmountIncludingWHEstimate);
			Assert("line 2 estimate flag is off", !entryLine2.IsDutyAndTaxEstimatedForWH);
			AssertEquals("line 2 Duty", 3000m, entryLine2.DutyAmount);
			AssertEquals("line 2 Duty Estimate", 3000m, entryLine2.DutyAmountIncludingWHEstimate);
			AssertEquals("line 2 GST", 6300m, entryLine2.GSTVATAmount);
			AssertEquals("line 2 GST Estimate", 6300m, entryLine2.GSTVATAmountIncludingWHEstimate);
			AssertEquals("entry wood levy", 720m, entry.WoodLevy);
			AssertEquals("entry wood levy estimate", 720m, entry.WoodLevyIncludingWHEstimate);

			AssertEquals("N10CustomsValue", 120000.00m, ((IDeclarationChargeProvider)entry).N10CustomsValue);
			AssertEquals("Quarantine Processing Charge", 10.00m, entry.AQISProcessingCharge);
			AssertEquals("Declaration Processing Charge", 100.00m, entry.DeclarationProcessingCharge);
		}

		public void TestDutyGSTAndWoodLevyN10WithEstimateON()
		{
			var declaration = GetWoodLevyTestDeclaration();
			declaration.Importer.MiscServ.OM_IMShowDutyOnWarehouseEntries = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Assert("inv line1 estimated flag is off", !line1.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line1 duty", 750m, line1.JI_Calc_DutyAmount);
			AssertEquals("inv line1 duty estimated", 750m, line1.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line1 GST", 1575m, line1.JI_Calc_GSTVATAmount);
			AssertEquals("inv line1 GST estimated", 1575m, line1.JI_Calc_GSTVATAmountIncludingWHEstimate);
			Assert("inv line2 estimated flag is off", !line2.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line2 duty", 2250m, line2.JI_Calc_DutyAmount);
			AssertEquals("inv line2 duty estimated", 2250m, line2.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line2 GST", 4725m, line2.JI_Calc_GSTVATAmount);
			AssertEquals("inv line2 GST estimated", 4725m, line2.JI_Calc_GSTVATAmountIncludingWHEstimate);

			Assert("inv line3 estimated flag is off", !line3.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line3 duty", 750m, line3.JI_Calc_DutyAmount);
			AssertEquals("inv line3 duty estimated", 750m, line3.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line3 GST", 1575m, line3.JI_Calc_GSTVATAmount);
			AssertEquals("inv line3 GST estimated", 1575m, line3.JI_Calc_GSTVATAmountIncludingWHEstimate);
			Assert("inv line4 estimated flag is off", !line4.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line4 duty", 2250m, line4.JI_Calc_DutyAmount);
			AssertEquals("inv line4 duty estimated", 2250m, line4.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line4 GST", 4725m, line4.JI_Calc_GSTVATAmount);
			AssertEquals("inv line4 GST estimated", 4725m, line4.JI_Calc_GSTVATAmountIncludingWHEstimate);

			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine1 = entry.MergedLines[0];
			var entryLine2 = entry.MergedLines[1];
			Assert("line 1 estimate flag is off", !entryLine1.IsDutyAndTaxEstimatedForWH);
			AssertEquals("line 1 Duty", 3000m, entryLine1.DutyAmount);
			AssertEquals("line 1 Duty Estimate", 3000m, entryLine1.DutyAmountIncludingWHEstimate);
			AssertEquals("line 1 GST", 6300m, entryLine1.GSTVATAmount);
			AssertEquals("line 1 Duty Estimate", 6300m, entryLine1.GSTVATAmountIncludingWHEstimate);
			Assert("line 2 estimate flag is off", !entryLine2.IsDutyAndTaxEstimatedForWH);
			AssertEquals("line 2 Duty", 3000m, entryLine2.DutyAmount);
			AssertEquals("line 2 Duty Estimate", 3000m, entryLine2.DutyAmountIncludingWHEstimate);
			AssertEquals("line 2 GST", 6300m, entryLine2.GSTVATAmount);
			AssertEquals("line 2 GST Estimate", 6300m, entryLine2.GSTVATAmountIncludingWHEstimate);
			AssertEquals("entry wood levy", 720m, entry.WoodLevy);
			AssertEquals("entry wood levy estimate", 720m, entry.WoodLevyIncludingWHEstimate);
		}

		public void TestDutyGSTAndWoodLevyN20()
		{
			var declaration = GetWoodLevyTestDeclaration();
			declaration.Importer.MiscServ.OM_IMShowDutyOnWarehouseEntries = false;
			line1.JI_IsPackToBondForLine = true;
			line2.JI_IsPackToBondForLine = true;
			line3.JI_IsPackToBondForLine = true;
			line4.JI_IsPackToBondForLine = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CombineAssertions(() =>
			{
				Assert("inv line1 estimated flag is off", !line1.IsDutyAndTaxEstimatedForWH);
				AssertEquals("inv line1 duty", 0m, line1.JI_Calc_DutyAmount);
				AssertEquals("inv line1 duty estimated", 0m, line1.JI_Calc_DutyAmountIncludingWHEstimate);
				AssertEquals("inv line1 GST", 0m, line1.JI_Calc_GSTVATAmount);
				AssertEquals("inv line1 GST estimated", 0m, line1.JI_Calc_GSTVATAmountIncludingWHEstimate);
				Assert("inv line2 estimated flag is off", !line2.IsDutyAndTaxEstimatedForWH);
				AssertEquals("inv line2 duty", 0m, line2.JI_Calc_DutyAmount);
				AssertEquals("inv line2 duty estimated", 0m, line2.JI_Calc_DutyAmountIncludingWHEstimate);
				AssertEquals("inv line2 GST", 0m, line2.JI_Calc_GSTVATAmount);
				AssertEquals("inv line2 GST estimated", 0m, line2.JI_Calc_GSTVATAmountIncludingWHEstimate);

				Assert("inv line3 estimated flag is off", !line3.IsDutyAndTaxEstimatedForWH);
				AssertEquals("inv line3 duty", 0m, line3.JI_Calc_DutyAmount);
				AssertEquals("inv line3 duty estimated", 0m, line3.JI_Calc_DutyAmountIncludingWHEstimate);
				AssertEquals("inv line3 GST", 0m, line3.JI_Calc_GSTVATAmount);
				AssertEquals("inv line3 GST estimated", 0m, line3.JI_Calc_GSTVATAmountIncludingWHEstimate);
				Assert("inv line4 estimated flag is off", !line4.IsDutyAndTaxEstimatedForWH);
				AssertEquals("inv line4 duty", 0m, line4.JI_Calc_DutyAmount);
				AssertEquals("inv line4 duty estimated", 0m, line4.JI_Calc_DutyAmountIncludingWHEstimate);
				AssertEquals("inv line4 GST", 0m, line4.JI_Calc_GSTVATAmount);
				AssertEquals("inv line4 GST estimated", 0m, line4.JI_Calc_GSTVATAmountIncludingWHEstimate);

				var entry = declaration.CustomsEntryHeaders[0];
				var entryLine1 = entry.MergedLines[0];
				var entryLine2 = entry.MergedLines[1];
				Assert("line 1 estimate flag is off", !entryLine1.IsDutyAndTaxEstimatedForWH);
				AssertEquals("line 1 Duty", 0m, entryLine1.DutyAmount);
				AssertEquals("line 1 Duty Estimate", 0m, entryLine1.DutyAmountIncludingWHEstimate);
				AssertEquals("line 1 GST", 0m, entryLine1.GSTVATAmount);
				AssertEquals("line 1 GST Estimate", 0m, entryLine1.GSTVATAmountIncludingWHEstimate);
				Assert("line 2 estimate flag is off", !entryLine2.IsDutyAndTaxEstimatedForWH);
				AssertEquals("line 2 Duty", 0m, entryLine2.DutyAmount);
				AssertEquals("line 2 Duty Estimate", 0m, entryLine2.DutyAmountIncludingWHEstimate);
				AssertEquals("line 2 GST", 0m, entryLine2.GSTVATAmount);
				AssertEquals("line 2 GST Estimate", 0m, entryLine2.GSTVATAmountIncludingWHEstimate);
				AssertEquals("entry wood levy", 0m, entry.WoodLevy);
				AssertEquals("entry wood levy estimate", 0m, entry.WoodLevyIncludingWHEstimate);
			});
		}

		public void TestDutyGSTAndWoodLevyN20WithEstimateON()
		{
			var declaration = GetWoodLevyTestDeclaration();
			declaration.Importer.MiscServ.OM_IMShowDutyOnWarehouseEntries = true;
			line1.JI_IsPackToBondForLine = true;
			line2.JI_IsPackToBondForLine = true;
			line3.JI_IsPackToBondForLine = true;
			line4.JI_IsPackToBondForLine = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Assert("inv line1 estimated flag is on", line1.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line1 duty", 0m, line1.JI_Calc_DutyAmount);
			AssertEquals("inv line1 duty estimated", 750m, line1.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line1 GST", 0m, line1.JI_Calc_GSTVATAmount);
			AssertEquals("inv line1 GST estimated", 1575m, line1.JI_Calc_GSTVATAmountIncludingWHEstimate);
			Assert("inv line2 estimated flag is on", line2.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line2 duty", 0m, line2.JI_Calc_DutyAmount);
			AssertEquals("inv line2 duty estimated", 2250m, line2.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line2 GST", 0m, line2.JI_Calc_GSTVATAmount);
			AssertEquals("inv line2 GST estimated", 4725m, line2.JI_Calc_GSTVATAmountIncludingWHEstimate);

			Assert("inv line3 estimated flag is on", line3.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line3 duty", 0m, line3.JI_Calc_DutyAmount);
			AssertEquals("inv line3 duty estimated", 750m, line3.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line3 GST", 0m, line3.JI_Calc_GSTVATAmount);
			AssertEquals("inv line3 GST estimated", 1575m, line3.JI_Calc_GSTVATAmountIncludingWHEstimate);
			Assert("inv line4 estimated flag is on", line4.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line4 duty", 0m, line4.JI_Calc_DutyAmount);
			AssertEquals("inv line4 duty estimated", 2250m, line4.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line4 GST", 0m, line4.JI_Calc_GSTVATAmount);
			AssertEquals("inv line4 GST estimated", 4725m, line4.JI_Calc_GSTVATAmountIncludingWHEstimate);

			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine1 = entry.MergedLines[0];
			var entryLine2 = entry.MergedLines[1];
			Assert("line 1 estimate flag is on", entryLine1.IsDutyAndTaxEstimatedForWH);
			AssertEquals("line 1 Duty", 0m, entryLine1.DutyAmount);
			AssertEquals("line 1 Duty Estimate", 3000m, entryLine1.DutyAmountIncludingWHEstimate);
			AssertEquals("line 1 GST", 0m, entryLine1.GSTVATAmount);
			AssertEquals("line 1 GST Estimate", 6300m, entryLine1.GSTVATAmountIncludingWHEstimate);
			Assert("line 2 estimate flag is on", entryLine2.IsDutyAndTaxEstimatedForWH);
			AssertEquals("line 2 Duty", 0m, entryLine2.DutyAmount);
			AssertEquals("line 2 Duty Estimate", 3000m, entryLine2.DutyAmountIncludingWHEstimate);
			AssertEquals("line 2 GST", 0m, entryLine2.GSTVATAmount);
			AssertEquals("line 2 GST Estimate", 6300m, entryLine2.GSTVATAmountIncludingWHEstimate);
			AssertEquals("entry wood levy", 0m, entry.WoodLevy);
			AssertEquals("entry wood levy estimate", 720m, entry.WoodLevyIncludingWHEstimate);
		}

		public void TestDutyGSTAndWoodLevyN10N20()
		{
			var declaration = GetWoodLevyTestDeclaration();
			line1.JI_IsPackToBondForLine = true;
			line2.JI_IsPackToBondForLine = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CombineAssertions(() =>
			{
				Assert("inv line1 estimated flag is off", !line1.IsDutyAndTaxEstimatedForWH);
				AssertEquals("inv line1 duty", 0m, line1.JI_Calc_DutyAmount);
				AssertEquals("inv line1 duty estimated", 0m, line1.JI_Calc_DutyAmountIncludingWHEstimate);
				AssertEquals("inv line1 GST", 0m, line1.JI_Calc_GSTVATAmount);
				AssertEquals("inv line1 GST estimated", 0m, line1.JI_Calc_GSTVATAmountIncludingWHEstimate);
				Assert("inv line2 estimated flag is off", !line2.IsDutyAndTaxEstimatedForWH);
				AssertEquals("inv line2 duty", 0m, line2.JI_Calc_DutyAmount);
				AssertEquals("inv line2 duty estimated", 0m, line2.JI_Calc_DutyAmountIncludingWHEstimate);
				AssertEquals("inv line2 GST", 0m, line2.JI_Calc_GSTVATAmount);
				AssertEquals("inv line2 GST estimated", 0m, line2.JI_Calc_GSTVATAmountIncludingWHEstimate);

				Assert("inv line3 estimated flag is off", !line3.IsDutyAndTaxEstimatedForWH);
				AssertEquals("inv line3 duty", 750m, line3.JI_Calc_DutyAmount);
				AssertEquals("inv line3 duty estimated", 750m, line3.JI_Calc_DutyAmountIncludingWHEstimate);
				AssertEquals("inv line3 GST", 1575m, line3.JI_Calc_GSTVATAmount);
				AssertEquals("inv line3 GST estimated", 1575m, line3.JI_Calc_GSTVATAmountIncludingWHEstimate);
				Assert("inv line4 estimated flag is off", !line4.IsDutyAndTaxEstimatedForWH);
				AssertEquals("inv line4 duty", 2250m, line4.JI_Calc_DutyAmount);
				AssertEquals("inv line4 duty estimated", 2250m, line4.JI_Calc_DutyAmountIncludingWHEstimate);
				AssertEquals("inv line4 GST", 4725m, line4.JI_Calc_GSTVATAmount);
				AssertEquals("inv line4 GST estimated", 4725m, line4.JI_Calc_GSTVATAmountIncludingWHEstimate);

				var entry = declaration.CustomsEntryHeaders[0];
				var entryLine1 = entry.MergedLines[0];
				var entryLine2 = entry.MergedLines[1];
				Assert("line 1 estimate flag is off", !entryLine1.IsDutyAndTaxEstimatedForWH);
				AssertEquals("line 1 Duty", 0m, entryLine1.DutyAmount);
				AssertEquals("line 1 Duty Estimate", 0m, entryLine1.DutyAmountIncludingWHEstimate);
				AssertEquals("line 1 GST", 0m, entryLine1.GSTVATAmount);
				AssertEquals("line 1 GST Estimate", 0m, entryLine1.GSTVATAmountIncludingWHEstimate);
				Assert("line 2 estimate flag is off", !entryLine2.IsDutyAndTaxEstimatedForWH);
				AssertEquals("line 2 Duty", 3000m, entryLine2.DutyAmount);
				AssertEquals("line 2 Duty Estimate", 3000m, entryLine2.DutyAmountIncludingWHEstimate);
				AssertEquals("line 2 GST", 6300m, entryLine2.GSTVATAmount);
				AssertEquals("line 2 GST Estimate", 6300m, entryLine2.GSTVATAmountIncludingWHEstimate);
				AssertEquals("entry wood levy", 720m, entry.WoodLevy);
				AssertEquals("entry wood levy estimate", 720m, entry.WoodLevyIncludingWHEstimate);
			});
		}

		public void TestDutyGSTAndWoodLevyN10N20WithEstimateON()
		{
			var declaration = GetWoodLevyTestDeclaration();
			declaration.Importer.MiscServ.OM_IMShowDutyOnWarehouseEntries = true;
			line1.JI_IsPackToBondForLine = true;
			line2.JI_IsPackToBondForLine = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Assert("inv line1 estimated flag is on", line1.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line1 duty", 0m, line1.JI_Calc_DutyAmount);
			AssertEquals("inv line1 duty estimated", 750m, line1.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line1 GST", 0m, line1.JI_Calc_GSTVATAmount);
			AssertEquals("inv line1 GST estimated", 1575m, line1.JI_Calc_GSTVATAmountIncludingWHEstimate);
			Assert("inv line2 estimated flag is on", line2.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line2 duty", 0m, line2.JI_Calc_DutyAmount);
			AssertEquals("inv line2 duty estimated", 2250m, line2.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line2 GST", 0m, line2.JI_Calc_GSTVATAmount);
			AssertEquals("inv line2 GST estimated", 4725m, line2.JI_Calc_GSTVATAmountIncludingWHEstimate);

			Assert("inv line3 estimated flag is off", !line3.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line3 duty", 750m, line3.JI_Calc_DutyAmount);
			AssertEquals("inv line3 duty estimated", 750m, line3.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line3 GST", 1575m, line3.JI_Calc_GSTVATAmount);
			AssertEquals("inv line3 GST estimated", 1575m, line3.JI_Calc_GSTVATAmountIncludingWHEstimate);
			Assert("inv line4 estimated flag is off", !line4.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line4 duty", 2250m, line4.JI_Calc_DutyAmount);
			AssertEquals("inv line4 duty estimated", 2250m, line4.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line4 GST", 4725m, line4.JI_Calc_GSTVATAmount);
			AssertEquals("inv line4 GST estimated", 4725m, line4.JI_Calc_GSTVATAmountIncludingWHEstimate);

			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine1 = entry.MergedLines[0];
			var entryLine2 = entry.MergedLines[1];
			Assert("line 1 estimate flag is on", entryLine1.IsDutyAndTaxEstimatedForWH);
			AssertEquals("line 1 Duty", 0m, entryLine1.DutyAmount);
			AssertEquals("line 1 Duty Estimate", 3000m, entryLine1.DutyAmountIncludingWHEstimate);
			AssertEquals("line 1 GST", 0m, entryLine1.GSTVATAmount);
			AssertEquals("line 1 GST Estimate", 6300m, entryLine1.GSTVATAmountIncludingWHEstimate);
			Assert("line 2 estimate flag is off", !entryLine2.IsDutyAndTaxEstimatedForWH);
			AssertEquals("line 2 Duty", 3000m, entryLine2.DutyAmount);
			AssertEquals("line 2 Duty Estimate", 3000m, entryLine2.DutyAmountIncludingWHEstimate);
			AssertEquals("line 2 GST", 6300m, entryLine2.GSTVATAmount);
			AssertEquals("line 2 GST Estimate", 6300m, entryLine2.GSTVATAmountIncludingWHEstimate);
			AssertEquals("entry wood levy", 720m, entry.WoodLevy);
			AssertEquals("entry wood levy estimate", 720m, entry.WoodLevyIncludingWHEstimate);
		}

		public void TestDutyGSTAndWoodLevyN10N20TempImp()
		{
			var declaration = GetWoodLevyTestDeclaration();
			line1.JI_IsPackToBondForLine = true;
			line2.JI_IsPackToBondForLine = true;
			line1.AddInfo.ZA_TreatmentCode_Hidden = "352";
			line2.AddInfo.ZA_TreatmentCode_Hidden = "352";
			line3.AddInfo.ZA_TreatmentCode_Hidden = "352";
			line4.AddInfo.ZA_TreatmentCode_Hidden = "352";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Assert("inv line1 estimated flag is off", !line1.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line1 duty", 0m, line1.JI_Calc_DutyAmount);
			AssertEquals("inv line1 duty estimated", 0m, line1.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line1 GST", 0m, line1.JI_Calc_GSTVATAmount);
			AssertEquals("inv line1 GST estimated", 0m, line1.JI_Calc_GSTVATAmountIncludingWHEstimate);
			Assert("inv line2 estimated flag is off", !line2.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line2 duty", 0m, line2.JI_Calc_DutyAmount);
			AssertEquals("inv line2 duty estimated", 0m, line2.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line2 GST", 0m, line2.JI_Calc_GSTVATAmount);
			AssertEquals("inv line2 GST estimated", 0m, line2.JI_Calc_GSTVATAmountIncludingWHEstimate);

			Assert("inv line3 estimated flag is off", !line3.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line3 duty", 0m, line3.JI_Calc_DutyAmount);
			AssertEquals("inv line3 duty estimated", 0m, line3.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line3 GST", 0m, line3.JI_Calc_GSTVATAmount);
			AssertEquals("inv line3 GST estimated", 0m, line3.JI_Calc_GSTVATAmountIncludingWHEstimate);
			Assert("inv line4 estimated flag is off", !line4.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line4 duty", 0m, line4.JI_Calc_DutyAmount);
			AssertEquals("inv line4 duty estimated", 0m, line4.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line4 GST", 0m, line4.JI_Calc_GSTVATAmount);
			AssertEquals("inv line4 GST estimated", 0m, line4.JI_Calc_GSTVATAmountIncludingWHEstimate);

			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine1 = entry.MergedLines[0];
			var entryLine2 = entry.MergedLines[1];
			Assert("line 1 estimate flag is off", !entryLine1.IsDutyAndTaxEstimatedForWH);
			AssertEquals("line 1 Duty", 0m, entryLine1.DutyAmount);
			AssertEquals("line 1 Duty Estimate", 0m, entryLine1.DutyAmountIncludingWHEstimate);
			AssertEquals("line 1 GST", 0m, entryLine1.GSTVATAmount);
			AssertEquals("line 1 GST Estimate", 0m, entryLine1.GSTVATAmountIncludingWHEstimate);
			Assert("line 2 estimate flag is off", !entryLine2.IsDutyAndTaxEstimatedForWH);
			AssertEquals("line 2 Duty", 0m, entryLine2.DutyAmount);
			AssertEquals("line 2 Duty Estimate", 0m, entryLine2.DutyAmountIncludingWHEstimate);
			AssertEquals("line 2 GST", 0m, entryLine2.GSTVATAmount);
			AssertEquals("line 2 GST Estimate", 0m, entryLine2.GSTVATAmountIncludingWHEstimate);
			AssertEquals("entry wood levy", 720m, entry.WoodLevy);
			AssertEquals("entry wood levy estimate", 720m, entry.WoodLevyIncludingWHEstimate);
		}

		public void TestDutyGSTAndWoodLevyN10N20TempImpWithEstimateON()
		{
			var declaration = GetWoodLevyTestDeclaration();
			declaration.Importer.MiscServ.OM_IMShowDutyOnWarehouseEntries = true;
			line1.JI_IsPackToBondForLine = true;
			line2.JI_IsPackToBondForLine = true;
			line1.AddInfo.ZA_TreatmentCode_Hidden = "352";
			line2.AddInfo.ZA_TreatmentCode_Hidden = "352";
			line3.AddInfo.ZA_TreatmentCode_Hidden = "352";
			line4.AddInfo.ZA_TreatmentCode_Hidden = "352";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Assert("inv line1 estimated flag is on", line1.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line1 duty", 0m, line1.JI_Calc_DutyAmount);
			AssertEquals("inv line1 duty estimated", 0m, line1.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line1 GST", 0m, line1.JI_Calc_GSTVATAmount);
			AssertEquals("inv line1 GST estimated", 0m, line1.JI_Calc_GSTVATAmountIncludingWHEstimate);
			Assert("inv line2 estimated flag is on", line2.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line2 duty", 0m, line2.JI_Calc_DutyAmount);
			AssertEquals("inv line2 duty estimated", 0m, line2.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line2 GST", 0m, line2.JI_Calc_GSTVATAmount);
			AssertEquals("inv line2 GST estimated", 0m, line2.JI_Calc_GSTVATAmountIncludingWHEstimate);

			Assert("inv line3 estimated flag is off", !line3.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line3 duty", 0m, line3.JI_Calc_DutyAmount);
			AssertEquals("inv line3 duty estimated", 0m, line3.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line3 GST", 0m, line3.JI_Calc_GSTVATAmount);
			AssertEquals("inv line3 GST estimated", 0m, line3.JI_Calc_GSTVATAmountIncludingWHEstimate);
			Assert("inv line4 estimated flag is off", !line4.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line4 duty", 0m, line4.JI_Calc_DutyAmount);
			AssertEquals("inv line4 duty estimated", 0m, line4.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line4 GST", 0m, line4.JI_Calc_GSTVATAmount);
			AssertEquals("inv line4 GST estimated", 0m, line4.JI_Calc_GSTVATAmountIncludingWHEstimate);

			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine1 = entry.MergedLines[0];
			var entryLine2 = entry.MergedLines[1];
			Assert("line 1 estimate flag is on", entryLine1.IsDutyAndTaxEstimatedForWH);
			AssertEquals("line 1 Duty", 0m, entryLine1.DutyAmount);
			AssertEquals("line 1 Duty Estimate", 0m, entryLine1.DutyAmountIncludingWHEstimate);
			AssertEquals("line 1 GST", 0m, entryLine1.GSTVATAmount);
			AssertEquals("line 1 GST Estimate", 0m, entryLine1.GSTVATAmountIncludingWHEstimate);
			Assert("line 2 estimate flag is off", !entryLine2.IsDutyAndTaxEstimatedForWH);
			AssertEquals("line 2 Duty", 0m, entryLine2.DutyAmount);
			AssertEquals("line 2 Duty Estimate", 0m, entryLine2.DutyAmountIncludingWHEstimate);
			AssertEquals("line 2 GST", 0m, entryLine2.GSTVATAmount);
			AssertEquals("line 2 GST Estimate", 0m, entryLine2.GSTVATAmountIncludingWHEstimate);
			AssertEquals("entry wood levy", 720m, entry.WoodLevy);
			AssertEquals("entry wood levy estimate", 720m, entry.WoodLevyIncludingWHEstimate);
		}

		public void TestDutyGSTAndWoodLevyN10N20LowValue()
		{
			TaxOrFeeTestHelper.SetUp();

			var declaration = GetWoodLevyTestDeclaration();
			declaration.Importer.MiscServ.OM_IMShowDutyOnWarehouseEntries = true;
			line1.JI_IsPackToBondForLine = true;
			line2.JI_IsPackToBondForLine = true;
			line1.JI_LinePrice = 100m;
			line2.JI_LinePrice = 100m;
			line3.JI_LinePrice = 100m;
			line4.JI_LinePrice = 100m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Assert("inv line1 estimated flag is on", line1.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line1 duty", 0m, line1.JI_Calc_DutyAmount);
			AssertEquals("inv line1 duty estimated", 0m, line1.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line1 GST", 0m, line1.JI_Calc_GSTVATAmount);
			AssertEquals("inv line1 GST estimated", 0m, line1.JI_Calc_GSTVATAmountIncludingWHEstimate);
			Assert("inv line2 estimated flag is on", line2.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line2 duty", 0m, line2.JI_Calc_DutyAmount);
			AssertEquals("inv line2 duty estimated", 0m, line2.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line2 GST", 0m, line2.JI_Calc_GSTVATAmount);
			AssertEquals("inv line2 GST estimated", 0m, line2.JI_Calc_GSTVATAmountIncludingWHEstimate);

			Assert("inv line3 estimated flag is off", !line3.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line3 duty", 0m, line3.JI_Calc_DutyAmount);
			AssertEquals("inv line3 duty estimated", 0m, line3.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line3 GST", 0m, line3.JI_Calc_GSTVATAmount);
			AssertEquals("inv line3 GST estimated", 0m, line3.JI_Calc_GSTVATAmountIncludingWHEstimate);
			Assert("inv line4 estimated flag is off", !line4.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line4 duty", 0m, line4.JI_Calc_DutyAmount);
			AssertEquals("inv line4 duty estimated", 0m, line4.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line4 GST", 0m, line4.JI_Calc_GSTVATAmount);
			AssertEquals("inv line4 GST estimated", 0m, line4.JI_Calc_GSTVATAmountIncludingWHEstimate);

			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine1 = entry.MergedLines[0];
			var entryLine2 = entry.MergedLines[1];
			AssertEquals("line 1 Duty", 0m, entryLine1.DutyAmount);
			AssertEquals("line 1 Duty Estimate", 0m, entryLine1.DutyAmountIncludingWHEstimate);
			AssertEquals("line 1 GST", 0m, entryLine1.GSTVATAmount);
			AssertEquals("line 1 GST Estimate", 0m, entryLine1.GSTVATAmountIncludingWHEstimate);
			AssertEquals("line 2 Duty", 0m, entryLine2.DutyAmount);
			AssertEquals("line 2 Duty Estimate", 0m, entryLine2.DutyAmountIncludingWHEstimate);
			AssertEquals("line 2 GST", 0m, entryLine2.GSTVATAmount);
			AssertEquals("line 2 GST Estimate", 0m, entryLine2.GSTVATAmountIncludingWHEstimate);
			AssertEquals("entry wood levy", 0m, entry.WoodLevy);
			AssertEquals("entry wood levy estimate", 0m, entry.WoodLevyIncludingWHEstimate);
		}

		public void TestDutyGSTAndWETN10N20()
		{
			var declaration = GetWETTestDeclaration();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CombineAssertions(() =>
			{
				Assert("inv line1 estimated flag is off", !line1.IsDutyAndTaxEstimatedForWH);
				AssertEquals("inv line1 duty", 0m, line1.JI_Calc_DutyAmount);
				AssertEquals("inv line1 duty estimated", 0m, line1.JI_Calc_DutyAmountIncludingWHEstimate);
				AssertEquals("inv line1 GST", 0m, line1.JI_Calc_GSTVATAmount);
				AssertEquals("inv line1 GST estimated", 0m, line1.JI_Calc_GSTVATAmountIncludingWHEstimate);
				AssertEquals("inv line1 WET", 0m, line1.JI_Calc_WETAmount);
				AssertEquals("inv line1 WET estimated", 0m, line1.JI_Calc_WETAmountIncludingWHEstimate);
				Assert("inv line2 estimated flag is off", !line2.IsDutyAndTaxEstimatedForWH);
				AssertEquals("inv line2 duty", 0m, line2.JI_Calc_DutyAmount);
				AssertEquals("inv line2 duty estimated", 0m, line2.JI_Calc_DutyAmountIncludingWHEstimate);
				AssertEquals("inv line2 GST", 0m, line2.JI_Calc_GSTVATAmount);
				AssertEquals("inv line2 GST estimated", 0m, line2.JI_Calc_GSTVATAmountIncludingWHEstimate);
				AssertEquals("inv line2 WET", 0m, line2.JI_Calc_WETAmount);
				AssertEquals("inv line2 WET estimated", 0m, line2.JI_Calc_WETAmountIncludingWHEstimate);

				Assert("inv line3 estimated flag is off", !line3.IsDutyAndTaxEstimatedForWH);
				AssertEquals("inv line3 duty", 750m, line3.JI_Calc_DutyAmount);
				AssertEquals("inv line3 duty estimated", 750m, line3.JI_Calc_DutyAmountIncludingWHEstimate);
				AssertEquals("inv line3 GST", 2031.75m, line3.JI_Calc_GSTVATAmount);
				AssertEquals("inv line3 GST estimated", 2031.75m, line3.JI_Calc_GSTVATAmountIncludingWHEstimate);
				AssertEquals("inv line3 WET", 4567.5m, line3.JI_Calc_WETAmount);
				AssertEquals("inv line3 WET estimated", 4567.5m, line3.JI_Calc_WETAmountIncludingWHEstimate);
				Assert("inv line4 estimated flag is off", !line4.IsDutyAndTaxEstimatedForWH);
				AssertEquals("inv line4 duty", 2250m, line4.JI_Calc_DutyAmount);
				AssertEquals("inv line4 duty estimated", 2250m, line4.JI_Calc_DutyAmountIncludingWHEstimate);
				AssertEquals("inv line4 GST", 6095.25m, line4.JI_Calc_GSTVATAmount);
				AssertEquals("inv line4 GST estimated", 6095.25m, line4.JI_Calc_GSTVATAmountIncludingWHEstimate);
				AssertEquals("inv line4 WET", 13702.5m, line4.JI_Calc_WETAmount);
				AssertEquals("inv line4 WET estimated", 13702.5m, line4.JI_Calc_WETAmountIncludingWHEstimate);

				var entry = declaration.CustomsEntryHeaders[0];
				var entryLine1 = entry.MergedLines[0];
				var entryLine2 = entry.MergedLines[1];
				Assert("line 1 estimate flag is off", !entryLine1.IsDutyAndTaxEstimatedForWH);
				AssertEquals("line 1 Duty", 0m, entryLine1.DutyAmount);
				AssertEquals("line 1 Duty Estimate", 0m, entryLine1.DutyAmountIncludingWHEstimate);
				AssertEquals("line 1 GST", 0m, entryLine1.GSTVATAmount);
				AssertEquals("line 1 GST Estimate", 0m, entryLine1.GSTVATAmountIncludingWHEstimate);
				AssertEquals("line 1 WET", 0m, entryLine1.WETAmount);
				AssertEquals("line 1 WET Estimate", 0m, entryLine1.WETAmountIncludingWHEstimate);
				Assert("line 2 estimate flag is off", !entryLine2.IsDutyAndTaxEstimatedForWH);
				AssertEquals("line 2 Duty", 3000m, entryLine2.DutyAmount);
				AssertEquals("line 2 Duty Estimate", 3000m, entryLine2.DutyAmountIncludingWHEstimate);
				AssertEquals("line 2 GST", 8127m, entryLine2.GSTVATAmount);
				AssertEquals("line 2 GST Estimate", 8127m, entryLine2.GSTVATAmountIncludingWHEstimate);
				AssertEquals("line 2 WET", 18270m, entryLine2.WETAmount);
				AssertEquals("line 2 WET Estimate", 18270m, entryLine2.WETAmountIncludingWHEstimate);
			});
		}

		public void TestDutyGSTAndWETN10N20WithEstimateON()
		{
			var declaration = GetWETTestDeclaration();
			declaration.Importer.MiscServ.OM_IMShowDutyOnWarehouseEntries = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Assert("inv line1 estimated flag is on", line1.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line1 duty", 0m, line1.JI_Calc_DutyAmount);
			AssertEquals("inv line1 duty estimated", 750m, line1.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line1 GST", 0m, line1.JI_Calc_GSTVATAmount);
			AssertEquals("inv line1 GST estimated", 2031.75m, line1.JI_Calc_GSTVATAmountIncludingWHEstimate);
			AssertEquals("inv line1 WET", 0m, line1.JI_Calc_WETAmount);
			AssertEquals("inv line1 WET estimated", 4567.5m, line1.JI_Calc_WETAmountIncludingWHEstimate);
			Assert("inv line2 estimated flag is on", line2.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line2 duty", 0m, line2.JI_Calc_DutyAmount);
			AssertEquals("inv line2 duty estimated", 2250m, line2.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line2 GST", 0m, line2.JI_Calc_GSTVATAmount);
			AssertEquals("inv line2 GST estimated", 6095.25m, line2.JI_Calc_GSTVATAmountIncludingWHEstimate);
			AssertEquals("inv line2 WET", 0m, line2.JI_Calc_WETAmount);
			AssertEquals("inv line2 WET estimated", 13702.5m, line2.JI_Calc_WETAmountIncludingWHEstimate);

			Assert("inv line3 estimated flag is off", !line3.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line3 duty", 750m, line3.JI_Calc_DutyAmount);
			AssertEquals("inv line3 duty estimated", 750m, line3.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line3 GST", 2031.75m, line3.JI_Calc_GSTVATAmount);
			AssertEquals("inv line3 GST estimated", 2031.75m, line3.JI_Calc_GSTVATAmountIncludingWHEstimate);
			AssertEquals("inv line3 WET", 4567.5m, line3.JI_Calc_WETAmount);
			AssertEquals("inv line3 WET estimated", 4567.5m, line3.JI_Calc_WETAmountIncludingWHEstimate);
			Assert("inv line4 estimated flag is off", !line4.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line4 duty", 2250m, line4.JI_Calc_DutyAmount);
			AssertEquals("inv line4 duty estimated", 2250m, line4.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line4 GST", 6095.25m, line4.JI_Calc_GSTVATAmount);
			AssertEquals("inv line4 GST estimated", 6095.25m, line4.JI_Calc_GSTVATAmountIncludingWHEstimate);
			AssertEquals("inv line4 WET", 13702.5m, line4.JI_Calc_WETAmount);
			AssertEquals("inv line4 WET estimated", 13702.5m, line4.JI_Calc_WETAmountIncludingWHEstimate);

			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine1 = entry.MergedLines[0];
			var entryLine2 = entry.MergedLines[1];
			Assert("line 1 estimate flag is on", entryLine1.IsDutyAndTaxEstimatedForWH);
			AssertEquals("line 1 Duty", 0m, entryLine1.DutyAmount);
			AssertEquals("line 1 Duty Estimate", 3000m, entryLine1.DutyAmountIncludingWHEstimate);
			AssertEquals("line 1 GST", 0m, entryLine1.GSTVATAmount);
			AssertEquals("line 1 GST Estimate", 8127m, entryLine1.GSTVATAmountIncludingWHEstimate);
			AssertEquals("line 1 WET", 0m, entryLine1.WETAmount);
			AssertEquals("line 1 WET Estimate", 18270m, entryLine1.WETAmountIncludingWHEstimate);
			Assert("line 2 estimate flag is off", !entryLine2.IsDutyAndTaxEstimatedForWH);
			AssertEquals("line 2 Duty", 3000m, entryLine2.DutyAmount);
			AssertEquals("line 2 Duty Estimate", 3000m, entryLine2.DutyAmountIncludingWHEstimate);
			AssertEquals("line 2 GST", 8127m, entryLine2.GSTVATAmount);
			AssertEquals("line 2 GST Estimate", 8127m, entryLine2.GSTVATAmountIncludingWHEstimate);
			AssertEquals("line 2 WET", 18270m, entryLine2.WETAmount);
			AssertEquals("line 2 WET Estimate", 18270m, entryLine2.WETAmountIncludingWHEstimate);
		}

		public void TestDutyGSTAndWETN10N20TempImp()
		{
			var declaration = GetWETTestDeclaration();
			declaration.Importer.MiscServ.OM_IMShowDutyOnWarehouseEntries = true;
			line1.AddInfo.ZA_TreatmentCode_Hidden = "352";
			line2.AddInfo.ZA_TreatmentCode_Hidden = "352";
			line3.AddInfo.ZA_TreatmentCode_Hidden = "352";
			line4.AddInfo.ZA_TreatmentCode_Hidden = "352";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Assert("inv line1 estimated flag is on", line1.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line1 duty", 0m, line1.JI_Calc_DutyAmount);
			AssertEquals("inv line1 duty estimated", 0m, line1.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line1 GST", 0m, line1.JI_Calc_GSTVATAmount);
			AssertEquals("inv line1 GST estimated", 0m, line1.JI_Calc_GSTVATAmountIncludingWHEstimate);
			AssertEquals("inv line1 WET", 0m, line1.JI_Calc_WETAmount);
			AssertEquals("inv line1 WET estimated", 4567.5m, line1.JI_Calc_WETAmountIncludingWHEstimate);
			Assert("inv line2 estimated flag is on", line2.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line2 duty", 0m, line2.JI_Calc_DutyAmount);
			AssertEquals("inv line2 duty estimated", 0m, line2.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line2 GST", 0m, line2.JI_Calc_GSTVATAmount);
			AssertEquals("inv line2 GST estimated", 0m, line2.JI_Calc_GSTVATAmountIncludingWHEstimate);
			AssertEquals("inv line2 WET", 0m, line2.JI_Calc_WETAmount);
			AssertEquals("inv line2 WET estimated", 13702.5m, line2.JI_Calc_WETAmountIncludingWHEstimate);

			Assert("inv line3 estimated flag is off", !line3.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line3 duty", 0m, line3.JI_Calc_DutyAmount);
			AssertEquals("inv line3 duty estimated", 0m, line3.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line3 GST", 0m, line3.JI_Calc_GSTVATAmount);
			AssertEquals("inv line3 GST estimated", 0m, line3.JI_Calc_GSTVATAmountIncludingWHEstimate);
			AssertEquals("inv line3 WET", 4567.5m, line3.JI_Calc_WETAmount);
			AssertEquals("inv line3 WET estimated", 4567.5m, line3.JI_Calc_WETAmountIncludingWHEstimate);
			Assert("inv line4 estimated flag is off", !line4.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line4 duty", 0m, line4.JI_Calc_DutyAmount);
			AssertEquals("inv line4 duty estimated", 0m, line4.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line4 GST", 0m, line4.JI_Calc_GSTVATAmount);
			AssertEquals("inv line4 GST estimated", 0m, line4.JI_Calc_GSTVATAmountIncludingWHEstimate);
			AssertEquals("inv line4 WET", 13702.5m, line4.JI_Calc_WETAmount);
			AssertEquals("inv line4 WET estimated", 13702.5m, line4.JI_Calc_WETAmountIncludingWHEstimate);

			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine1 = entry.MergedLines[0];
			var entryLine2 = entry.MergedLines[1];
			Assert("line 1 estimate flag is on", entryLine1.IsDutyAndTaxEstimatedForWH);
			AssertEquals("line 1 Duty", 0m, entryLine1.DutyAmount);
			AssertEquals("line 1 Duty Estimate", 0m, entryLine1.DutyAmountIncludingWHEstimate);
			AssertEquals("line 1 GST", 0m, entryLine1.GSTVATAmount);
			AssertEquals("line 1 GST Estimate", 0m, entryLine1.GSTVATAmountIncludingWHEstimate);
			AssertEquals("line 1 WET", 0m, entryLine1.WETAmount);
			AssertEquals("line 1 WET Estimate", 18270m, entryLine1.WETAmountIncludingWHEstimate);
			Assert("line 2 estimate flag is off", !entryLine2.IsDutyAndTaxEstimatedForWH);
			AssertEquals("line 2 Duty", 0m, entryLine2.DutyAmount);
			AssertEquals("line 2 Duty Estimate", 0m, entryLine2.DutyAmountIncludingWHEstimate);
			AssertEquals("line 2 GST", 0m, entryLine2.GSTVATAmount);
			AssertEquals("line 2 GST Estimate", 0m, entryLine2.GSTVATAmountIncludingWHEstimate);
			AssertEquals("line 2 WET", 18270m, entryLine2.WETAmount);
			AssertEquals("line 2 WET Estimate", 18270m, entryLine2.WETAmountIncludingWHEstimate);
		}

		public void TestDutyGSTAndWETN10N20LowValue()
		{
			TaxOrFeeTestHelper.SetUp();

			var declaration = GetWETTestDeclaration();
			declaration.Importer.MiscServ.OM_IMShowDutyOnWarehouseEntries = true;
			line1.JI_LinePrice = 100m;
			line2.JI_LinePrice = 100m;
			line3.JI_LinePrice = 100m;
			line4.JI_LinePrice = 100m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Assert("inv line1 estimated flag is on", line1.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line1 duty", 0m, line1.JI_Calc_DutyAmount);
			AssertEquals("inv line1 duty estimated", 0m, line1.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line1 GST", 0m, line1.JI_Calc_GSTVATAmount);
			AssertEquals("inv line1 GST estimated", 0m, line1.JI_Calc_GSTVATAmountIncludingWHEstimate);
			AssertEquals("inv line1 WET", 0m, line1.JI_Calc_WETAmount);
			AssertEquals("inv line1 WET estimated", 0m, line1.JI_Calc_WETAmountIncludingWHEstimate);
			Assert("inv line2 estimated flag is on", line2.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line2 duty", 0m, line2.JI_Calc_DutyAmount);
			AssertEquals("inv line2 duty estimated", 0m, line2.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line2 GST", 0m, line2.JI_Calc_GSTVATAmount);
			AssertEquals("inv line2 GST estimated", 0m, line2.JI_Calc_GSTVATAmountIncludingWHEstimate);
			AssertEquals("inv line2 WET", 0m, line2.JI_Calc_WETAmount);
			AssertEquals("inv line2 WET estimated", 0m, line2.JI_Calc_WETAmountIncludingWHEstimate);

			Assert("inv line3 estimated flag is off", !line3.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line3 duty", 0m, line3.JI_Calc_DutyAmount);
			AssertEquals("inv line3 duty estimated", 0m, line3.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line3 GST", 0m, line3.JI_Calc_GSTVATAmount);
			AssertEquals("inv line3 GST estimated", 0m, line3.JI_Calc_GSTVATAmountIncludingWHEstimate);
			AssertEquals("inv line3 WET", 0m, line3.JI_Calc_WETAmount);
			AssertEquals("inv line3 WET estimated", 0m, line3.JI_Calc_WETAmountIncludingWHEstimate);
			Assert("inv line4 estimated flag is off", !line4.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line4 duty", 0m, line4.JI_Calc_DutyAmount);
			AssertEquals("inv line4 duty estimated", 0m, line4.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line4 GST", 0m, line4.JI_Calc_GSTVATAmount);
			AssertEquals("inv line4 GST estimated", 0m, line4.JI_Calc_GSTVATAmountIncludingWHEstimate);
			AssertEquals("inv line4 WET", 0m, line4.JI_Calc_WETAmount);
			AssertEquals("inv line4 WET estimated", 0m, line4.JI_Calc_WETAmountIncludingWHEstimate);

			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine1 = entry.MergedLines[0];
			var entryLine2 = entry.MergedLines[1];
			AssertEquals("line 1 Duty", 0m, entryLine1.DutyAmount);
			AssertEquals("line 1 Duty Estimate", 0m, entryLine1.DutyAmountIncludingWHEstimate);
			AssertEquals("line 1 GST", 0m, entryLine1.GSTVATAmount);
			AssertEquals("line 1 GST Estimate", 0m, entryLine1.GSTVATAmountIncludingWHEstimate);
			AssertEquals("line 1 WET", 0m, entryLine1.WETAmount);
			AssertEquals("line 1 WET Estimate", 0m, entryLine1.WETAmountIncludingWHEstimate);
			AssertEquals("line 2 Duty", 0m, entryLine2.DutyAmount);
			AssertEquals("line 2 Duty Estimate", 0m, entryLine2.DutyAmountIncludingWHEstimate);
			AssertEquals("line 2 GST", 0m, entryLine2.GSTVATAmount);
			AssertEquals("line 2 GST Estimate", 0m, entryLine2.GSTVATAmountIncludingWHEstimate);
			AssertEquals("line 2 WET", 0m, entryLine2.WETAmount);
			AssertEquals("line 2 WET Estimate", 0m, entryLine2.WETAmountIncludingWHEstimate);
		}

		public void TestDutyGSTAndLCTN10N20()
		{
			var declaration = GetLCTTestDeclaration();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CombineAssertions(() =>
			{
				Assert("inv line1 estimated flag is off", !line1.IsDutyAndTaxEstimatedForWH);
				AssertEquals("inv line1 duty", 0m, line1.JI_Calc_DutyAmount);
				AssertEquals("inv line1 duty estimated", 0m, line1.JI_Calc_DutyAmountIncludingWHEstimate);
				AssertEquals("inv line1 GST", 0m, line1.JI_Calc_GSTVATAmount);
				AssertEquals("inv line1 GST estimated", 0m, line1.JI_Calc_GSTVATAmountIncludingWHEstimate);
				AssertEquals("inv line1 LCT", 0m, line1.JI_Calc_LCTAmount);
				AssertEquals("inv line1 LCT estimated", 0m, line1.JI_Calc_LCTAmountIncludingWHEstimate);

				Assert("inv line2 estimated flag is off", !line2.IsDutyAndTaxEstimatedForWH);
				AssertEquals("inv line2 duty", 18000m, line2.JI_Calc_DutyAmount);
				AssertEquals("inv line2 duty estimated", 18000m, line2.JI_Calc_DutyAmountIncludingWHEstimate);
				AssertEquals("inv line2 GST", 7800m, line2.JI_Calc_GSTVATAmount);
				AssertEquals("inv line2 GST estimated", 7800m, line2.JI_Calc_GSTVATAmountIncludingWHEstimate);
				AssertEquals("inv line2 LCT", 7645.2m, line2.JI_Calc_LCTAmount);
				AssertEquals("inv line2 LCT estimated", 7645.2m, line2.JI_Calc_LCTAmountIncludingWHEstimate);

				var entry = declaration.CustomsEntryHeaders[0];
				var entryLine1 = entry.MergedLines[0];
				var entryLine2 = entry.MergedLines[1];
				Assert("line 1 estimate flag is off", !entryLine1.IsDutyAndTaxEstimatedForWH);
				AssertEquals("line 1 Duty", 0m, entryLine1.DutyAmount);
				AssertEquals("line 1 Duty Estimate", 0m, entryLine1.DutyAmountIncludingWHEstimate);
				AssertEquals("line 1 GST", 0m, entryLine1.GSTVATAmount);
				AssertEquals("line 1 GST Estimate", 0m, entryLine1.GSTVATAmountIncludingWHEstimate);
				AssertEquals("line 1 LCT", 0m, entryLine1.LCTAmount);
				AssertEquals("line 1 LCT Estimate", 0m, entryLine1.LCTAmountIncludingWHEstimate);
				Assert("line 2 estimate flag is off", !entryLine2.IsDutyAndTaxEstimatedForWH);
				AssertEquals("line 2 Duty", 18000m, entryLine2.DutyAmount);
				AssertEquals("line 2 Duty Estimate", 18000m, entryLine2.DutyAmountIncludingWHEstimate);
				AssertEquals("line 2 GST", 7800m, entryLine2.GSTVATAmount);
				AssertEquals("line 2 GST Estimate", 7800m, entryLine2.GSTVATAmountIncludingWHEstimate);
				AssertEquals("line 2 LCT", 7645.2m, entryLine2.LCTAmount);
				AssertEquals("line 2 LCT Estimate", 7645.2m, entryLine2.LCTAmountIncludingWHEstimate);
			});
		}

		public void TestDutyGSTAndLCTN10N20WithEstimateON()
		{
			var declaration = GetLCTTestDeclaration();
			declaration.Importer.MiscServ.OM_IMShowDutyOnWarehouseEntries = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Assert("inv line1 estimated flag is on", line1.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line1 duty", 0m, line1.JI_Calc_DutyAmount);
			AssertEquals("inv line1 duty estimated", 18000m, line1.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line1 GST", 0m, line1.JI_Calc_GSTVATAmount);
			AssertEquals("inv line1 GST estimated", 7800m, line1.JI_Calc_GSTVATAmountIncludingWHEstimate);
			AssertEquals("inv line1 LCT", 0m, line1.JI_Calc_LCTAmount);
			AssertEquals("inv line1 LCT estimated", 7645.2m, line1.JI_Calc_LCTAmountIncludingWHEstimate);

			Assert("inv line2 estimated flag is off", !line2.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line2 duty", 18000m, line2.JI_Calc_DutyAmount);
			AssertEquals("inv line2 duty estimated", 18000m, line2.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line2 GST", 7800m, line2.JI_Calc_GSTVATAmount);
			AssertEquals("inv line2 GST estimated", 7800m, line2.JI_Calc_GSTVATAmountIncludingWHEstimate);
			AssertEquals("inv line2 LCT", 7645.2m, line2.JI_Calc_LCTAmount);
			AssertEquals("inv line2 LCT estimated", 7645.2m, line2.JI_Calc_LCTAmountIncludingWHEstimate);

			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine1 = entry.MergedLines[0];
			var entryLine2 = entry.MergedLines[1];
			Assert("line 1 estimate flag is on", entryLine1.IsDutyAndTaxEstimatedForWH);
			AssertEquals("line 1 Duty", 0m, entryLine1.DutyAmount);
			AssertEquals("line 1 Duty Estimate", 18000m, entryLine1.DutyAmountIncludingWHEstimate);
			AssertEquals("line 1 GST", 0m, entryLine1.GSTVATAmount);
			AssertEquals("line 1 GST Estimate", 7800m, entryLine1.GSTVATAmountIncludingWHEstimate);
			AssertEquals("line 1 LCT", 0m, entryLine1.LCTAmount);
			AssertEquals("line 1 LCT Estimate", 7645.2m, entryLine1.LCTAmountIncludingWHEstimate);
			Assert("line 2 estimate flag is off", !entryLine2.IsDutyAndTaxEstimatedForWH);
			AssertEquals("line 2 Duty", 18000m, entryLine2.DutyAmount);
			AssertEquals("line 2 Duty Estimate", 18000m, entryLine2.DutyAmountIncludingWHEstimate);
			AssertEquals("line 2 GST", 7800m, entryLine2.GSTVATAmount);
			AssertEquals("line 2 GST Estimate", 7800m, entryLine2.GSTVATAmountIncludingWHEstimate);
			AssertEquals("line 2 LCT", 7645.2m, entryLine2.LCTAmount);
			AssertEquals("line 2 LCT Estimate", 7645.2m, entryLine2.LCTAmountIncludingWHEstimate);
		}

		public void TestDutyGSTAndLCTN10N20TempImp()
		{
			var declaration = GetLCTTestDeclaration();
			declaration.Importer.MiscServ.OM_IMShowDutyOnWarehouseEntries = true;
			line1.AddInfo.ZA_TreatmentCode_Hidden = "352";
			line2.AddInfo.ZA_TreatmentCode_Hidden = "352";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Assert("inv line1 estimated flag is on", line1.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line1 duty", 0m, line1.JI_Calc_DutyAmount);
			AssertEquals("inv line1 duty estimated", 0m, line1.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line1 GST", 0m, line1.JI_Calc_GSTVATAmount);
			AssertEquals("inv line1 GST estimated", 0m, line1.JI_Calc_GSTVATAmountIncludingWHEstimate);
			AssertEquals("inv line1 LCT", 0m, line1.JI_Calc_LCTAmount);
			AssertEquals("inv line1 LCT estimated", 0m, line1.JI_Calc_LCTAmountIncludingWHEstimate);

			Assert("inv line2 estimated flag is off", !line2.IsDutyAndTaxEstimatedForWH);
			AssertEquals("inv line2 duty", 0m, line2.JI_Calc_DutyAmount);
			AssertEquals("inv line2 duty estimated", 0m, line2.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line2 GST", 0m, line2.JI_Calc_GSTVATAmount);
			AssertEquals("inv line2 GST estimated", 0m, line2.JI_Calc_GSTVATAmountIncludingWHEstimate);
			AssertEquals("inv line2 LCT", 0m, line2.JI_Calc_LCTAmount);
			AssertEquals("inv line2 LCT estimated", 0m, line2.JI_Calc_LCTAmountIncludingWHEstimate);

			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine1 = entry.MergedLines[0];
			var entryLine2 = entry.MergedLines[1];
			Assert("line 1 estimate flag is on", entryLine1.IsDutyAndTaxEstimatedForWH);
			AssertEquals("line 1 Duty", 0m, entryLine1.DutyAmount);
			AssertEquals("line 1 Duty Estimate", 0m, entryLine1.DutyAmountIncludingWHEstimate);
			AssertEquals("line 1 GST", 0m, entryLine1.GSTVATAmount);
			AssertEquals("line 1 GST Estimate", 0m, entryLine1.GSTVATAmountIncludingWHEstimate);
			AssertEquals("line 1 LCT", 0m, entryLine1.LCTAmount);
			AssertEquals("line 1 LCT Estimate", 0m, entryLine1.LCTAmountIncludingWHEstimate);
			Assert("line 2 estimate flag is off", !entryLine2.IsDutyAndTaxEstimatedForWH);
			AssertEquals("line 2 Duty", 0m, entryLine2.DutyAmount);
			AssertEquals("line 2 Duty Estimate", 0m, entryLine2.DutyAmountIncludingWHEstimate);
			AssertEquals("line 2 GST", 0m, entryLine2.GSTVATAmount);
			AssertEquals("line 2 GST Estimate", 0m, entryLine2.GSTVATAmountIncludingWHEstimate);
			AssertEquals("line 2 LCT", 0m, entryLine2.LCTAmount);
			AssertEquals("line 2 LCT Estimate", 0m, entryLine2.LCTAmountIncludingWHEstimate);
		}

		public void TestCustomsAndQuarantineFeesForDiplomat()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("Q1A", 10.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q2A", 15.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			var taxL = helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.DAN, 50.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			taxL.ZZF_Threshold = 10000m;
			Factory.Save();

			var declaration = GetImportDeclaration(Core.Constants.TransportModes.Air);
			declaration.AddInfo.ZA_UPEIndicator_Hidden = true;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Quarantine Processing Charge for Diplomat/Embassy should be 0.", 10m, entry.AQISProcessingCharge);
			AssertEquals("Declaration Processing Charge for Diplomat/Embassy should be 0.", 50m, entry.DeclarationProcessingCharge);

			var cusCode = declaration.Importer.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.Diplomat;
			declaration.DoMerge();

			var entry2 = declaration.CustomsEntryHeaders[0];
			AssertEquals("Quarantine Processing Charge for Diplomat/Embassy should be 0.", 0m, entry2.AQISProcessingCharge);
			AssertEquals("Declaration Processing Charge for Diplomat/Embassy should be 0.", 0m, entry2.DeclarationProcessingCharge);
		}

		public void TestCustomsAndQuarantineFeesForUnaccompaniedPersonalEffects()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("Q1A", 10.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("Q2A", 15.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			var taxL = helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.DAN, 50.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			taxL.ZZF_Threshold = 10000m;
			Factory.Save();

			var declaration = GetImportDeclaration(Core.Constants.TransportModes.Air);
			var cusCode = declaration.Importer.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.Diplomat;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Quarantine Processing Charge for Diplomat/Embassy should be 0.", 10m, entry.AQISProcessingCharge);
			AssertEquals("Declaration Processing Charge for Diplomat/Embassy should be 0.", 50m, entry.DeclarationProcessingCharge);

			declaration.AddInfo.ZA_UPEIndicator_Hidden = true;
			declaration.DoMerge();
			entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Quarantine Processing Charge for Unaccompanied Personal Effects should be 0.", 0m, entry.AQISProcessingCharge);
			AssertEquals("Declaration Processing Charge for Unaccompanied Personal Effects should be 0.", 0m, entry.DeclarationProcessingCharge);
		}

		public void TestProcessingCharges_ConsolidatedEntry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.Q1A, 10.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.Q2A, 15.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.Q1S, 50.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.Q2S, 55.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			var taxL = helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.DAN, 50.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			taxL.ZZF_Threshold = 10000m;
			var taxH = helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.DAH, 100.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			Factory.Save();

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "TSTIMP";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			declaration.JE_DeclarationReference = "B001223822";
			declaration.JE_OH_Importer = importer.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1200.0m;
			invoice.JZ_RX_NKInvoice_Currency = "AUD";
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "9999.31.03 03";
			line.JI_Description = "AAAAA";
			line.JI_CustomsUnitQty = "CU";
			line.JI_CustomsQuantity = 100m;
			line.JI_LinePrice = 1200.0m;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 2000m;
			line.JI_CL = entryLine.PK;
			declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration2.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration2.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration2.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			declaration2.JE_DeclarationReference = "B001223823";
			declaration2.JE_OH_Importer = importer.PK;
			var invoice2 = declaration2.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 1200.0m;
			invoice2.JZ_RX_NKInvoice_Currency = "AUD";
			invoice2.AddInfo.ZA_PST = "GEN";
			var line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "4409.10.00 01";
			line2.JI_Description = "AAAAA";
			line2.JI_CustomsUnitQty = "CU";
			line2.JI_CustomsQuantity = 100m;
			line2.JI_LinePrice = 1200.0m;
			var entry2 = declaration2.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			var entryLine2 = entry2.MergedLines.AddNew();
			entryLine2.CL_CustomsValue = 1200m;
			line2.JI_CL = entryLine2.PK;
			declaration2.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;

			var consolidatedDeclaration = Factory.NewWithValidTestData<ConsolidatedDeclaration>();
			consolidatedDeclaration.CRD_JE_LeadDeclaration = declaration.PK;
			consolidatedDeclaration.JobDeclarations.Add(declaration);
			consolidatedDeclaration.JobDeclarations.Add(declaration2);

			Factory.Save();

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CombineAssertions("Value Change After Merge", () =>
			{
				AssertEquals("AQISProcessingCharge", 10.00m, entryHeader.AQISProcessingCharge);
				AssertEquals("DeclarationProcessingCharge", 50m, entryHeader.DeclarationProcessingCharge);
				AssertEquals("AQISContainerCharges", 0.0m, entryHeader.AQISContainerCharges);
				AssertEquals("WoodLevy", 72.0m, entryHeader.WoodLevy);
			});

			declaration2.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration2.DoMerge();
			CombineAssertions("Value Change After Merge", () =>
			{
				AssertEquals("AQISProcessingCharge", 0.00m, entry2.AQISProcessingCharge);
				AssertEquals("DeclarationProcessingCharge", 0.00m, entry2.DeclarationProcessingCharge);
				AssertEquals("AQISContainerCharges", 0.0m, entry2.AQISContainerCharges);
				AssertEquals("WoodLevy", 0.0m, entry2.WoodLevy);
			});

			consolidatedDeclaration.CalculateHeaderFees();
			CombineAssertions("Value Change After Merge", () =>
			{
				AssertEquals("AQISProcessingCharge", 10.00m, entryHeader.AQISProcessingCharge);
				AssertEquals("DeclarationProcessingCharge", 50m, entryHeader.DeclarationProcessingCharge);
				AssertEquals("AQISContainerCharges", 0.0m, entryHeader.AQISContainerCharges);
				AssertEquals("WoodLevy", 72.0m, entryHeader.WoodLevy);

				AssertEquals("entry2 AQISProcessingCharge", 0.00m, entry2.AQISProcessingCharge);
				AssertEquals("entry2 DeclarationProcessingCharge", 0.00m, entry2.DeclarationProcessingCharge);
				AssertEquals("entry2 AQISContainerCharges", 0.0m, entry2.AQISContainerCharges);
				AssertEquals("entry2 WoodLevy", 0.0m, entry2.WoodLevy);
			});
		}

		public void TestCustomsChargesForDeminimusSACWithUPE()
		{
			var universalRefHelper = new UniversalReferenceTestDataHelper(Factory);
			universalRefHelper.CreateTaxOrFee("DEM", 1000m, "AU", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 6, 6), "Deminimus");
			var declaration = GetSAC_UPE_TestDeclaration();
			Factory.Save();

			ZTestHelper helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), 2.7344m, helper.MYRCurrency);
			helper.SetExchangeRate(ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), 0.6495m, helper.USDCurrency);
			declaration.AddInfo.ZA_UPEIndicator_Hidden = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];
			Assert("Duty Charge should be calculated for Unaccompanied Personal Effects", entry.DutyAmount > 0);
			Assert("GST Charge should be calculated for Unaccompanied Personal Effects", entry.GSTAmount > 0);

			AssertEquals("inv line1 duty", 0m, line1.JI_Calc_DutyAmount);
			AssertEquals("inv line1 duty estimated", 00m, line1.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("inv line1 GST", 0m, line1.JI_Calc_GSTVATAmount);
			AssertEquals("inv line1 GST estimated", 0m, line1.JI_Calc_GSTVATAmountIncludingWHEstimate);

			Assert("Dutiable lines should calculate even though below deminimus as entry is UPE entry: inv line2 duty", line2.JI_Calc_DutyAmount > 0);
			Assert("inv line2 duty estimated", line2.JI_Calc_DutyAmountIncludingWHEstimate > 0);
			Assert("inv line2 GST", line2.JI_Calc_GSTVATAmount > 0);
			Assert("inv line2 GST estimated", line2.JI_Calc_GSTVATAmountIncludingWHEstimate > 0);

			Assert("Dutiable lines should calculate even though below deminimus as entry is UPE entry: inv line3 duty", line3.JI_Calc_DutyAmount > 0);
			Assert("inv line3 duty estimated", line3.JI_Calc_DutyAmountIncludingWHEstimate > 0);
			Assert("inv line3 GST", line3.JI_Calc_GSTVATAmount > 0);
			Assert("inv line3 GST estimated", line3.JI_Calc_GSTVATAmountIncludingWHEstimate > 0);
		}

		public void TestTotalDeferredDutyFromCustoms()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.Q1A, 10.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.Q2A, 15.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			var taxL = helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.DAN, 50.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			taxL.ZZF_Threshold = 10000m;
			var taxH = helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.DAH, 100.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			Factory.Save();

			var declaration = GetWoodLevyTestDeclaration();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(0m, declaration.CustomsEntryHeaders[0].TotalDeferredDutyFromCustoms);

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "AUI";
			importer.AUIsDutyDeferred = true;
			declaration.JE_OH_Importer = importer.PK;
			Factory.Save();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals(6000m, entryHeader.DutyAmount);
			AssertEquals(0720m, entryHeader.WoodLevy);
			AssertEquals(0110m, entryHeader.AQISProcessingCharge + entryHeader.DeclarationProcessingCharge);
			AssertEquals(6830m, entryHeader.TotalDeferredDutyFromCustoms);

			var invoice = declaration.Invoices[0];
			invoice.JZ_RX_NKInvoice_Currency = "AUD";
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;
			line.AddInfo.ZA_PST = "GEN";
			line.JI_Tariff = "22042190 35";
			Factory.Save();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[4];
			AssertEquals(true, entryLine.IsExciseEquivalentGoods);
			AssertEquals(6500m, entryHeader.DutyAmount);
			AssertEquals(0720m, entryHeader.WoodLevy);
			AssertEquals(0110m, entryHeader.AQISProcessingCharge + entryHeader.DeclarationProcessingCharge);
			AssertEquals(6830m, entryHeader.TotalDeferredDutyFromCustoms);

			var imdrMessage = Factory.New<CMRIMDRMessage>();
			imdrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			imdrMessage.EM_Status = EDIMessage.Status.Received;
			entryHeader.Messages.Add(imdrMessage);
			entryHeader.Charges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 3000m);
			Factory.Save();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals(6500m, entryHeader.DutyAmount);
			AssertEquals(0720m, entryHeader.WoodLevy);
			AssertEquals(0110m, entryHeader.AQISProcessingCharge + entryHeader.DeclarationProcessingCharge);
			AssertEquals("Merge overwrites the IMDR message value", 6830m, entryHeader.TotalDeferredDutyFromCustoms);
		}

		public void TestProcessingCharges_Air()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.Q1A, 10.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.Q2A, 15.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			var taxL = helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.DAN, 50.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			taxL.ZZF_Threshold = 10000m;
			var taxH = helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.DAH, 100.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			Factory.Save();

			var declaration = GetImportDeclaration(Core.Constants.TransportModes.Air);
			AssertProcessingCharges(declaration);
		}

		public void TestProcessingCharges_Sea()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.Q1F, 33.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.Q1S, 10.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.Q2S, 15.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			var taxL = helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.DSN, 50.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			taxL.ZZF_Threshold = 10000m;
			var taxH = helper.CreateTaxOrFee(CMRDeclarationChargeCalculator.Constants.DSH, 100.00M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FBT", ZDateTime.Today, new ZDateTime(2079, 6, 6));
			Factory.Save();

			var declaration = GetImportDeclaration(Core.Constants.TransportModes.Sea);

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OLCU0000001";
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;

			AssertProcessingCharges(declaration);
		}

		public void AssertProcessingCharges(JobDeclaration declaration)
		{
			JobComInvoiceHeader invoice = declaration.Invoices[0];
			JobComInvoiceLine line = invoice.JobComInvoiceLines[0];

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			CombineAssertions("Low Value Merge", () =>
			{
				AssertEquals("AQISProcessingCharge", 10.00m, entry.AQISProcessingCharge);
				AssertEquals("DeclarationProcessingCharge", 50.00m, entry.DeclarationProcessingCharge);
				AssertEquals("EntryFee", 50.00m, entry.EntryFee);
				AssertEquals("AQISContainerCharges", declaration.IsSea ? 33.00m : 0.0m, entry.AQISContainerCharges);
			});

			entry.CH_BGMReference = "B00122382/1";
			entry.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;

			var lodgeMessage = entry.Messages.AddNew(typeof(CMRIMDMessage));
			lodgeMessage.EM_MessageText = "UNH+1+CUSDEC:D:99B:UN'BGM+929:::IMD+B00122382/1:1+9'UNT+3+1'";
			lodgeMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			lodgeMessage.EM_Status = EDIMessage.Status.Sent;
			Factory.Save();

			var lodgeResponseMessage = entry.Messages.AddNew(typeof(CMRIMDRMessage));
			lodgeResponseMessage.EM_MessageText = "UNH+1+CUSRES:D:99B:UN'BGM+961:::IMDR+2JB1 6CIH 1E50:1+11'UNT+3+1'";
			lodgeResponseMessage.EM_Status = EDIMessage.Status.Received;
			Factory.Save();

			entry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entry.EntryNumber = "TEST001";
			declaration.JE_EntryStatus = "FIN";
			Factory.Save();

			entry = declaration.CustomsEntryHeaders[0];
			Assert("HasBeenLodgedAtCustoms", entry.HasBeenLodgedAtCustoms);
			CombineAssertions("After response has same charges", () =>
			{
				AssertEquals("AQISProcessingCharge", 10.00m, entry.AQISProcessingCharge);
				AssertEquals("DeclarationProcessingCharge", 50.00m, entry.DeclarationProcessingCharge);
				AssertEquals("AQISContainerCharges", declaration.IsSea ? 33.00m : 0.0m, entry.AQISContainerCharges);
			});

			declaration.DoMerge();
			Factory.Save();

			entry = declaration.CustomsEntryHeaders[0];
			CombineAssertions("Re-Merge has same charges", () =>
			{
				AssertEquals("AQISProcessingCharge", 10.00m, entry.AQISProcessingCharge);
				AssertEquals("DeclarationProcessingCharge", 50.00m, entry.DeclarationProcessingCharge);
				AssertEquals("AQISContainerCharges", declaration.IsSea ? 33.00m : 0.0m, entry.AQISContainerCharges);
			});

			invoice.JZ_InvoiceAmount = 20000.0m;
			line.JI_LinePrice = 20000.0m;

			declaration.DoMerge();
			Factory.Save();

			entry = declaration.CustomsEntryHeaders[0];
			CombineAssertions("High Value Merge", () =>
			{
				AssertEquals("AQISProcessingCharge", 10.00m, entry.AQISProcessingCharge);
				AssertEquals("DeclarationProcessingCharge", 100.00m, entry.DeclarationProcessingCharge);
				AssertEquals("AQISContainerCharges", declaration.IsSea ? 33.00m : 0.0m, entry.AQISContainerCharges);
			});

			entry.Charges[CusEntryChargeTypeList.Codes.AQISContainerCharges].C1_ChargeAmount = 0.0m;
			entry.Charges[CusEntryChargeTypeList.Codes.AQISProcessingCharge].C1_ChargeAmount = 0.0m;
			entry.Charges[CusEntryChargeTypeList.Codes.DeclarationProcessingCharge].C1_ChargeAmount = 0.0m;

			declaration.DoMerge();
			Factory.Save();

			entry = declaration.CustomsEntryHeaders[0];
			Assert("HasBeenLodgedAtCustoms", entry.HasBeenLodgedAtCustoms);
			CombineAssertions("Re-Merge after charges cancelled still has cancelled charges", () =>
			{
				AssertEquals("AQISContainerCharges", 0.0m, entry.AQISContainerCharges);
				AssertEquals("AQISProcessingCharge", 0.0m, entry.AQISProcessingCharge);
				AssertEquals("DeclarationProcessingCharge", 0.0m, entry.DeclarationProcessingCharge);
			});
		}

		public JobDeclaration GetImportDeclaration(ZString transportMode)
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_TransportMode = transportMode;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			declaration.JE_DeclarationReference = "B00122382";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1200.0m;
			invoice.JZ_RX_NKInvoice_Currency = "AUD";
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "9999.31.03 03";
			line.JI_Description = "AAAAA";
			line.JI_CustomsUnitQty = "CU";
			line.JI_CustomsQuantity = 100;
			line.JI_LinePrice = 1200.0m;

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TSTIMP";
			declaration.JE_OH_Importer = importer.PK;

			return declaration;
		}

		JobDeclaration GetWoodLevyTestDeclaration()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TSTIMP";
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_OH_Importer = importer.PK;
			declaration.Importer.MiscServ.OM_IMShowDutyOnWarehouseEntries = false;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 120000;
			invoice.JZ_RX_NKInvoice_Currency = "AUD";
			invoice.AddInfo.ZA_PST = "GEN";
			line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 15000m;
			line1.JI_Tariff = "4409.10.00 01";
			line1.JI_Description = "AAAAA";
			line1.JI_CustomsUnitQty = "CU";
			line1.JI_CustomsQuantity = 125;
			line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 45000m;
			line2.JI_Tariff = "4409.10.00 01";
			line2.JI_Description = "AAAAA";
			line2.JI_CustomsUnitQty = "CU";
			line2.JI_CustomsQuantity = 375;
			line3 = invoice.JobComInvoiceLines.AddNew();
			line3.JI_LinePrice = 15000m;
			line3.JI_Tariff = "4409.10.00 01";
			line3.JI_Description = "BBBBB";
			line3.JI_CustomsUnitQty = "CU";
			line3.JI_CustomsQuantity = 125;
			line4 = invoice.JobComInvoiceLines.AddNew();
			line4.JI_LinePrice = 45000m;
			line4.JI_Tariff = "4409.10.00 01";
			line4.JI_Description = "BBBBB";
			line4.JI_CustomsUnitQty = "CU";
			line4.JI_CustomsQuantity = 375;
			return declaration;
		}

		JobDeclaration GetWETTestDeclaration()
		{
			var importer = Factory.New<OrgHeader>();
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_OH_Importer = importer.PK;
			declaration.Importer.MiscServ.OM_IMShowDutyOnWarehouseEntries = false;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 120000;
			invoice.JZ_RX_NKInvoice_Currency = "AUD";
			invoice.AddInfo.ZA_PST = "GEN";
			line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 15000m;
			line1.JI_Tariff = "2204.21.20 71";
			line1.JI_Description = "AAAAA";
			line1.JI_CustomsUnitQty = "L";
			line1.JI_CustomsQuantity = 125;
			line1.JI_IsPackToBondForLine = true;
			line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 45000m;
			line2.JI_Tariff = "2204.21.20 71";
			line2.JI_Description = "AAAAA";
			line2.JI_CustomsUnitQty = "L";
			line2.JI_CustomsQuantity = 375;
			line2.JI_IsPackToBondForLine = true;
			line3 = invoice.JobComInvoiceLines.AddNew();
			line3.JI_LinePrice = 15000m;
			line3.JI_Tariff = "2204.21.20 71";
			line3.JI_Description = "BBBBB";
			line3.JI_CustomsUnitQty = "L";
			line3.JI_CustomsQuantity = 125;
			line4 = invoice.JobComInvoiceLines.AddNew();
			line4.JI_LinePrice = 45000m;
			line4.JI_Tariff = "2204.21.20 71";
			line4.JI_Description = "BBBBB";
			line4.JI_CustomsUnitQty = "L";
			line4.JI_CustomsQuantity = 375;

			var filter = new ZQuery(CMRStatisticalClassificationPeriodSnapshotSchema.SC_TariffClassificationNumber, "22042120");
			filter.AddToFilter(JoinCondition.And, CMRStatisticalClassificationPeriodSnapshotSchema.SC_StatisticalClassificationCode, SQLComparisonOperator.Equal, "71");
			filter.AddToFilter(JoinCondition.And, CMRStatisticalClassificationPeriodSnapshotSchema.SC_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, declaration.EffectiveDutyDate);
			var endDateFilter = new ZQuery(CMRStatisticalClassificationPeriodSnapshotSchema.SC_EndDate, null);
			endDateFilter.AddToFilter(JoinCondition.Or, CMRStatisticalClassificationPeriodSnapshotSchema.SC_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, declaration.EffectiveDutyDate);
			filter.AddToFilter(endDateFilter, JoinCondition.And);
			var classification = Factory.LoadTop1<CMRStatisticalClassificationPeriodSnapshot>(filter);

			if (classification == null)
			{
				classification = Factory.New<CMRStatisticalClassificationPeriodSnapshot>();
				classification.SC_TariffClassificationNumber = "22042120";
				classification.SC_StatisticalClassificationCode = "71";
				classification.SC_StartDate = ZDateTime.BrettsBirthday;
			}

			classification.SC_PeriodIdentifier = 5;

			var characteristicFilter = new ZQuery(CMRStatisticalClassificationPeriodCharacteristicSchema.SH_StatisticalClassificationPeriodSnapshotTariffClassificationNumber, "22042120");
			characteristicFilter.AddToFilter(CMRStatisticalClassificationPeriodCharacteristicSchema.SH_StatisticalClassificationPeriodSnapshotPeriodIdentifier, (short)5);
			characteristicFilter.AddToFilter(CMRStatisticalClassificationPeriodCharacteristicSchema.SH_StatisticalClassificationPeriodSnapshotStatisticalClassificationCode, "71");

			var classificationCharacteristic = Factory.LoadTop1<CMRStatisticalClassificationPeriodCharacteristic>(characteristicFilter);

			if (classificationCharacteristic == null)
			{
				classificationCharacteristic = Factory.New<CMRStatisticalClassificationPeriodCharacteristic>();
				classificationCharacteristic.SH_StatisticalClassificationPeriodSnapshotTariffClassificationNumber = "22042120";
				classificationCharacteristic.SH_StatisticalClassificationPeriodSnapshotStatisticalClassificationCode = "71";
				classificationCharacteristic.SH_StatisticalClassificationPeriodSnapshotPeriodIdentifier = 5;
			}

			classificationCharacteristic.SH_CharacteristicCode = CMRDutyCalculator.WETCharacterCode;

			return declaration;
		}

		JobDeclaration GetLCTTestDeclaration()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("LNT", new ZDecimal(60316), GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("LNT", new ZDecimal(59133), GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(1970, 7, 1), ZDateTime.Today.AddMinutes(-1));
			helper.CreateTaxOrFee("LCT", new ZDecimal(0.33), GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, new ZDateTime(2079, 6, 6));
			Factory.Save();

			var importer = Factory.New<OrgHeader>();
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_OH_Importer = importer.PK;
			declaration.Importer.MiscServ.OM_IMShowDutyOnWarehouseEntries = false;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 120000;
			invoice.JZ_RX_NKInvoice_Currency = "AUD";
			invoice.AddInfo.ZA_PST = "GEN";
			line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 60000m;
			line1.JI_Tariff = "8703.24.11 11";
			line1.JI_Description = "AAAAA";
			line1.JI_CustomsUnitQty = "NO";
			line1.JI_CustomsQuantity = 1;
			line1.JI_IsPackToBondForLine = true;
			line1.AddInfo.ZA_LCTI = "Y";
			line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 60000m;
			line2.JI_Tariff = "8703.24.11 11";
			line2.JI_Description = "AAAAA";
			line2.JI_CustomsUnitQty = "NO";
			line2.JI_CustomsQuantity = 1;
			line2.AddInfo.ZA_LCTI = "Y";

			var finalFilter = new ZQuery(CMRTariffRatePeriodSnapshotSchema.TT_PreferenceSchemeType, SQLComparisonOperator.Equal, "GEN");
			finalFilter.AddToFilter(JoinCondition.And, CMRTariffRatePeriodSnapshotSchema.TT_TariffClassificationNumber, SQLComparisonOperator.Equal, "87032411");
			finalFilter.AddToFilter(JoinCondition.And, CMRTariffRatePeriodSnapshotSchema.TT_RateNumber, SQLComparisonOperator.Equal, "001");
			finalFilter.AddToFilter(JoinCondition.And, CMRTariffRatePeriodSnapshotSchema.TT_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, declaration.EffectiveDutyDate);

			var endDateFilter = new ZQuery(CMRTariffRatePeriodSnapshotSchema.TT_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, declaration.EffectiveDutyDate);
			endDateFilter.AddToFilter(JoinCondition.Or, CMRTariffRatePeriodSnapshotSchema.TT_EndDate, SQLComparisonOperator.Equal, null);

			finalFilter.AddToFilter(endDateFilter, JoinCondition.And);

			var tariffRate = Factory.LoadTop1<CMRTariffRatePeriodSnapshot>(finalFilter);
			if (tariffRate == null)
			{
				tariffRate = Factory.New<CMRTariffRatePeriodSnapshot>();
				tariffRate.TT_TariffClassificationNumber = "87032411";
				tariffRate.TT_RateNumber = "001";
				tariffRate.TT_StartDate = ZDateTime.BrettsBirthday;
				tariffRate.TT_PreferenceSchemeType = "GEN";
				tariffRate.TT_QuantityUnit = "NO";
				tariffRate.TT_CalculationType = "CALC";
			}

			tariffRate.TT_CustomsValueRate = 10m;
			tariffRate.TT_QuantityRate = 12000m;

			var filter = new ZQuery(CMRStatisticalClassificationPeriodSnapshotSchema.SC_TariffClassificationNumber, "87032411");
			filter.AddToFilter(JoinCondition.And, CMRStatisticalClassificationPeriodSnapshotSchema.SC_StatisticalClassificationCode, SQLComparisonOperator.Equal, "11");
			filter.AddToFilter(JoinCondition.And, CMRStatisticalClassificationPeriodSnapshotSchema.SC_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, declaration.EffectiveDutyDate);
			endDateFilter = new ZQuery(CMRStatisticalClassificationPeriodSnapshotSchema.SC_EndDate, null);
			endDateFilter.AddToFilter(JoinCondition.Or, CMRStatisticalClassificationPeriodSnapshotSchema.SC_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, declaration.EffectiveDutyDate);
			filter.AddToFilter(endDateFilter, JoinCondition.And);
			var classification = Factory.LoadTop1<CMRStatisticalClassificationPeriodSnapshot>(filter);

			if (classification == null)
			{
				classification = Factory.New<CMRStatisticalClassificationPeriodSnapshot>();
				classification.SC_TariffClassificationNumber = "87032411";
				classification.SC_StatisticalClassificationCode = "11";
				classification.SC_StartDate = ZDateTime.BrettsBirthday;
			}

			classification.SC_PeriodIdentifier = 5;

			var characteristicFilter = new ZQuery(CMRStatisticalClassificationPeriodCharacteristicSchema.SH_StatisticalClassificationPeriodSnapshotTariffClassificationNumber, "87032411");
			characteristicFilter.AddToFilter(CMRStatisticalClassificationPeriodCharacteristicSchema.SH_StatisticalClassificationPeriodSnapshotPeriodIdentifier, (short)5);
			characteristicFilter.AddToFilter(CMRStatisticalClassificationPeriodCharacteristicSchema.SH_StatisticalClassificationPeriodSnapshotStatisticalClassificationCode, "11");

			var classificationCharacteristic = Factory.LoadTop1<CMRStatisticalClassificationPeriodCharacteristic>(characteristicFilter);

			if (classificationCharacteristic == null)
			{
				classificationCharacteristic = Factory.New<CMRStatisticalClassificationPeriodCharacteristic>();
				classificationCharacteristic.SH_StatisticalClassificationPeriodSnapshotTariffClassificationNumber = "87032411";
				classificationCharacteristic.SH_StatisticalClassificationPeriodSnapshotStatisticalClassificationCode = "11";
				classificationCharacteristic.SH_StatisticalClassificationPeriodSnapshotPeriodIdentifier = 5;
			}

			classificationCharacteristic.SH_CharacteristicCode = 7;
			return declaration;
		}

		JobDeclaration GetSAC_UPE_TestDeclaration()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "AUI";
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_OH_Importer = importer.PK;
			declaration.Importer.MiscServ.OM_IMShowDutyOnWarehouseEntries = false;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			declaration.AddInfo.ZA_UPEIndicator_Hidden = true;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "Inv1";
			invoice1.JZ_InvoiceAmount = 755m;
			invoice1.JZ_RX_NKInvoice_Currency = "AUD";
			invoice1.JZ_RN_NKDefaultOrigin = "SG";
			invoice1.AddInfo.ZA_PST = "GEN";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "Inv2";
			invoice2.JZ_InvoiceAmount = 890m;
			invoice2.JZ_RX_NKInvoice_Currency = "AUD";
			invoice2.JZ_RN_NKDefaultOrigin = "SG";
			invoice2.AddInfo.ZA_PST = "GEN";

			var oFTcharge = invoice1.GroupCharges.AddNew();
			oFTcharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFTcharge.J7_Amount = 24.16m;
			oFTcharge.J7_RX_NKCurrency = "USD";
			var iNScharge = invoice1.GroupCharges.AddNew();
			iNScharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			iNScharge.J7_Amount = 24.16m;
			iNScharge.J7_RX_NKCurrency = "USD";

			line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 755m;
			line1.JI_Tariff = "9999.40.15 41";
			line1.JI_Description = "PERSONAL EFFECTS";
			line1.JI_InvoiceQuantity = 5m;
			line1.JI_InvoiceUQ = "NO";

			line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 688m;
			line2.JI_Tariff = "9401.79.00 09";
			line2.JI_Description = @"18"" STOOL - METAL STOOL NOT UPHOLSTERED";
			line2.JI_InvoiceQuantity = 16m;
			line2.JI_InvoiceUQ = "NO";
			line2.JI_CustomsQuantity = 16;
			line2.JI_CustomsUnitQty = "NO";

			line3 = invoice2.JobComInvoiceLines.AddNew();
			line3.JI_LinePrice = 130m;
			line3.JI_Tariff = "9403.70.00 52";
			line3.JI_Description = "INDOOR FURNITURE";
			line2.JI_InvoiceQuantity = 1m;
			line2.JI_InvoiceUQ = "NO";
			line2.JI_CustomsQuantity = 1;
			line2.JI_CustomsUnitQty = "NO";
			return declaration;
		}

		JobComInvoiceLine line1;
		JobComInvoiceLine line2;
		JobComInvoiceLine line3;
		JobComInvoiceLine line4;
	}
}
