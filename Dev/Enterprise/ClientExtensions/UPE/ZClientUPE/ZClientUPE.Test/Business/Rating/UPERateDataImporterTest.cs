using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPERateDataImporter))]
	sealed class UPERateDataImporterTest : NonPersistentBusinessObjectTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2007, 01, 01)]
		public void TestDoImportBlankColumns()
		{
			string fileName = UPETestHelper.TestFiles.Rating.Folder + "BlankColumnsSample.xlsx";
			using (var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				DataImporter.ImportIATA_TACT(fileName, reader);
			}

			var reloadedCompanyTariff = Factory.Load<CompanyTariff>(CompanyTariff.PK);
			RateEntryCollection airRateEntries = reloadedCompanyTariff.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection;
			AssertEquals(10, airRateEntries.Count);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[0], "0001", UPERatingConstants.ServiceLevels.ExpeditedPackages);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[1], "0002", UPERatingConstants.ServiceLevels.ExpeditedPackages);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[2], "0003", UPERatingConstants.ServiceLevels.ExpeditedPackages);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[3], "0004", UPERatingConstants.ServiceLevels.ExpeditedPackages);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[4], "0005", UPERatingConstants.ServiceLevels.ExpeditedPackages);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[5], "0006", UPERatingConstants.ServiceLevels.ExpeditedPackages);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[6], "0007", UPERatingConstants.ServiceLevels.ExpeditedPackages);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[7], "0008", UPERatingConstants.ServiceLevels.ExpeditedPackages);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[8], "0009", UPERatingConstants.ServiceLevels.ExpeditedPackages);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[9], "0010", UPERatingConstants.ServiceLevels.ExpeditedPackages);
			foreach (RateEntry airRateEntry in airRateEntries)
			{
				AssertEquals(104, airRateEntry.RateLines[0].RateLineItems.Count);
			}

			AssertEquals("Import Completed.", DataImporter.ImportReport);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2007, 01, 01)]
		public void TestDoImportExpressLetterSavedToDatabase()
		{
			string fileName = UPETestHelper.TestFiles.Rating.Folder + "LettersSample.xls";
			using (var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				DataImporter.ImportIATA_TACT(fileName, reader);
			}

			var reloadedCompanyTariff = Factory.Load<CompanyTariff>(CompanyTariff.PK);
			RateEntryCollection airRateEntries = reloadedCompanyTariff.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection;
			AssertEquals(11, airRateEntries.Count);
			foreach (RateEntry airRateEntry in airRateEntries)
			{
				AssertEquals(1, airRateEntry.RateLines[0].RateLineItems.Count);
			}

			AssertEquals("Import Completed.", DataImporter.ImportReport);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2007, 01, 01)]
		public void TestDoImportDuplicatedRateEntry()
		{
			string fileName = UPETestHelper.TestFiles.Rating.Folder + "LettersSample.xls";
			using (var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				DataImporter.ImportIATA_TACT(fileName, reader);
			}

			AssertEquals(11, Factory.GetDatabaseCount(typeof(RateEntry)));
			fileName = UPETestHelper.TestFiles.Rating.Folder + "LettersSample2007.xls";
			using (var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				DataImporter.ImportIATA_TACT(fileName, reader);
			}

			AssertEquals(11, Factory.GetDatabaseCount(typeof(RateEntry)));
			fileName = UPETestHelper.TestFiles.Rating.Folder + "LettersSample20181234.xls";
			using (var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				DataImporter.ImportIATA_TACT(fileName, reader);
			}

			AssertEquals("Malformatted date so current date is used", 11, Factory.GetDatabaseCount(typeof(RateEntry)));
			AssertEquals("Import Completed.", DataImporter.ImportReport);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2009, 01, 02)]
		public void TestDoImportOverlappings()
		{
			string fileName = UPETestHelper.TestFiles.Rating.Folder + "LettersSample2007.xls";
			using (var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				DataImporter.ImportIATA_TACT(fileName, reader);
			}

			ReleaseFactory();
			var rateEntriesQuery = new ZDBOnlyQuery(typeof(RateEntry));
			rateEntriesQuery.OrderBy = RateEntrySchema.Constants.TI_LineOrder;
			var rateEntries = Factory.Load<RateEntry>(rateEntriesQuery);
			AssertEquals(11, rateEntries.Length);
			AssertEquals(new ZDate(2007, 1, 1), rateEntries[0].TI_RateStartDate);
			AssertEquals(new ZDate(2009, 1, 1), rateEntries[0].TI_RateEndDate);
			fileName = UPETestHelper.TestFiles.Rating.Folder + "LettersSample20070102.xls";
			using (var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				DataImporter.ImportIATA_TACT(fileName, reader);
			}

			ReleaseFactory();
			rateEntries = Factory.Load<RateEntry>(rateEntriesQuery);
			AssertEquals("Old rates shouldn't be deleted", 22, rateEntries.Length);
			AssertEquals(new ZDate(2007, 1, 1), rateEntries[0].TI_RateStartDate);
			AssertEquals(new ZDate(2007, 1, 1), rateEntries[0].TI_RateEndDate);
			AssertEquals(new ZDate(2007, 1, 2), rateEntries.Last().TI_RateStartDate);
			AssertEquals(new ZDate(2009, 1, 2), rateEntries.Last().TI_RateEndDate);
			fileName = UPETestHelper.TestFiles.Rating.Folder + "LettersSample.xls";
			using (var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				DataImporter.ImportIATA_TACT(fileName, reader);
			}

			ReleaseFactory();
			rateEntries = Factory.Load<RateEntry>(rateEntriesQuery);
			AssertEquals("hasStartDate and RateEntryCollection should be reset", 33, rateEntries.Length);
			AssertEquals(new ZDate(2007, 1, 1), rateEntries[0].TI_RateStartDate);
			AssertEquals(new ZDate(2007, 1, 1), rateEntries[0].TI_RateEndDate);
			AssertEquals(new ZDate(2007, 1, 2), rateEntries[11].TI_RateStartDate);
			AssertEquals(new ZDate(2009, 1, 1), rateEntries[11].TI_RateEndDate);
			AssertEquals("new rates should be today's date again", new ZDate(2009, 1, 2), rateEntries.Last().TI_RateStartDate);
			AssertEquals(new ZDate(2011, 1, 2), rateEntries.Last().TI_RateEndDate);
			TestDateAttribute.AddYears(-1);
			fileName = UPETestHelper.TestFiles.Rating.Folder + "LettersSample.xls";
			using (var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				DataImporter.ImportIATA_TACT(fileName, reader);
			}

			ReleaseFactory();
			rateEntries = Factory.Load<RateEntry>(rateEntriesQuery);
			AssertEquals(33, rateEntries.Length);
			AssertEquals("no changes", new ZDate(2007, 1, 1), rateEntries[0].TI_RateStartDate);
			AssertEquals("no changes", new ZDate(2007, 1, 1), rateEntries[0].TI_RateEndDate);
			AssertEquals("overlapping rates should be udpated", new ZDate(2007, 1, 2), rateEntries[11].TI_RateStartDate);
			AssertEquals("overlapping rates should be udpated", new ZDate(2008, 1, 1), rateEntries[11].TI_RateEndDate);
			AssertEquals("future rates should be replaced", new ZDate(2008, 1, 2), rateEntries.Last().TI_RateStartDate);
			AssertEquals("future rates should be replaced", new ZDate(2010, 1, 2), rateEntries.Last().TI_RateEndDate);
			AssertEquals("Import Completed.", DataImporter.ImportReport);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2008, 01, 02)]
		public void TestGivenOverlappingRateEntry_WhenDoImport_ThenRateLinesShouldBeUpdatedToo()
		{
			string fileName = UPETestHelper.TestFiles.Rating.Folder + "LettersSample2007.xls";
			using (var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				DataImporter.ImportIATA_TACT(fileName, reader);
			}

			ReleaseFactory();
			var rateEntriesQuery = new ZDBOnlyQuery(typeof(RateEntry));
			rateEntriesQuery.OrderBy = RateEntrySchema.Constants.TI_LineOrder;
			var rateEntries = Factory.Load<RateEntry>(rateEntriesQuery);
			AssertEquals(11, rateEntries.Length);
			AssertEquals(new ZDate(2007, 1, 1), rateEntries[0].TI_RateStartDate);
			AssertEquals(new ZDate(2009, 1, 1), rateEntries[0].TI_RateEndDate);

			var rateLine1 = rateEntries[0].AddRateLine("FRT", FlatCalculator.Code);
			rateLine1.TL_RateStartDate = new ZDate(2007, 1, 1);
			rateLine1.TL_RateEndDate = new ZDate(2007, 6, 1);

			var rateLine2 = rateEntries[1].AddRateLine("FRT", FlatCalculator.Code);
			rateLine2.TL_RateStartDate = new ZDate(2007, 6, 1);
			rateLine2.TL_RateEndDate = new ZDate(2008, 6, 1);

			var rateLine3 = rateEntries[2].AddRateLine("FRT", FlatCalculator.Code);
			rateLine3.TL_RateStartDate = new ZDate(2008, 6, 1);
			rateLine3.TL_RateEndDate = new ZDate(2009, 1, 1);

			var rateLine4 = rateEntries[3].AddRateLine("FRT", FlatCalculator.Code);
			rateLine4.TL_RateEndDate = new ZDate(2009, 1, 1);

			var rateLine5 = rateEntries[4].AddRateLine("FRT", FlatCalculator.Code);

			Factory.Save();

			fileName = UPETestHelper.TestFiles.Rating.Folder + "LettersSample.xls";
			using (var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				DataImporter.ImportIATA_TACT(fileName, reader);
			}

			ReleaseFactory();
			rateEntries = Factory.Load<RateEntry>(rateEntriesQuery);
			AssertEquals(22, rateEntries.Length);
			AssertEquals("overlapping rate entry date should be updated", new ZDate(2007, 1, 1), rateEntries[0].TI_RateStartDate);
			AssertEquals("overlapping rate entry date should be updated", new ZDate(2008, 1, 1), rateEntries[0].TI_RateEndDate);

			var newRateLine1 = Factory.Load<RateLine>(rateLine1.PK);
			AssertEquals("non-overlapping rate lines should not be updated", new ZDate(2007, 1, 1), newRateLine1.TL_RateStartDate);
			AssertEquals("non-overlapping rate lines should not be updated", new ZDate(2007, 6, 1), newRateLine1.TL_RateEndDate);

			var newRateLine2 = Factory.Load<RateLine>(rateLine2.PK);
			AssertEquals("overlapping rate lines should be updated", new ZDate(2007, 6, 1), newRateLine2.TL_RateStartDate);
			AssertEquals("overlapping rate lines should be updated", new ZDate(2008, 1, 1), newRateLine2.TL_RateEndDate);

			var newRateLine3 = Factory.Load<RateLine>(rateLine3.PK);
			AssertEquals("non-overlapping rate lines should not be updated", new ZDate(2008, 6, 1), newRateLine3.TL_RateStartDate);
			AssertEquals("non-overlapping rate lines should not be updated", new ZDate(2009, 1, 1), newRateLine3.TL_RateEndDate);

			var newRateLine4 = Factory.Load<RateLine>(rateLine4.PK);
			AssertEquals("overlapping rate lines should be updated", ZDate.Empty, newRateLine4.TL_RateStartDate);
			AssertEquals("overlapping rate lines should be updated", new ZDate(2008, 1, 1), newRateLine4.TL_RateEndDate);

			var newRateLine5 = Factory.Load<RateLine>(rateLine5.PK);
			AssertEquals("non-overlapping rate lines should not be updated", ZDate.Empty, newRateLine5.TL_RateStartDate);
			AssertEquals("non-overlapping rate lines should not be updated", ZDate.Empty, newRateLine5.TL_RateEndDate);

			AssertEquals("new rate entries should be added", new ZDate(2008, 1, 2), rateEntries.Last().TI_RateStartDate);
			AssertEquals("new rate entries should be added", new ZDate(2010, 1, 2), rateEntries.Last().TI_RateEndDate);
			AssertEquals("Import Completed.", DataImporter.ImportReport);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2007, 01, 01)]
		public void TestDoImportExpressLetter()
		{
			string fileName = UPETestHelper.TestFiles.Rating.Folder + "LettersSample.xls";
			using (var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				DataImporter.ImportIATA_TACT(fileName, reader);
			}

			RateEntryCollection airRateEntries = CompanyTariff.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection;
			AssertEquals(11, airRateEntries.Count);
			AssertRateEntryDataWithFlatRateCalc(airRateEntries[0], "0001", 29m, UPERatingConstants.ServiceLevels.Envelopes);
			AssertRateEntryDataWithFlatRateCalc(airRateEntries[1], "0002", 40m, UPERatingConstants.ServiceLevels.Envelopes);
			AssertRateEntryDataWithFlatRateCalc(airRateEntries[2], "0003", 46m, UPERatingConstants.ServiceLevels.Envelopes);
			AssertRateEntryDataWithFlatRateCalc(airRateEntries[3], "0004", 47m, UPERatingConstants.ServiceLevels.Envelopes);
			AssertRateEntryDataWithFlatRateCalc(airRateEntries[4], "0005", 47m, UPERatingConstants.ServiceLevels.Envelopes);
			AssertRateEntryDataWithFlatRateCalc(airRateEntries[5], "0006", 59m, UPERatingConstants.ServiceLevels.Envelopes);
			AssertRateEntryDataWithFlatRateCalc(airRateEntries[6], "0007", 67m, UPERatingConstants.ServiceLevels.Envelopes);
			AssertRateEntryDataWithFlatRateCalc(airRateEntries[7], "0008", 88m, UPERatingConstants.ServiceLevels.Envelopes);
			AssertRateEntryDataWithFlatRateCalc(airRateEntries[8], "0009", 87m, UPERatingConstants.ServiceLevels.Envelopes);
			AssertRateEntryDataWithFlatRateCalc(airRateEntries[9], "0010", 121m, UPERatingConstants.ServiceLevels.Envelopes);
			AssertRateEntryDataWithFlatRateCalc(airRateEntries[10], "0011", 124m, UPERatingConstants.ServiceLevels.Envelopes);
			AssertEquals("Import Completed.", DataImporter.ImportReport);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2007, 01, 01)]
		public void TestDoImportExpressDocument()
		{
			string fileName = UPETestHelper.TestFiles.Rating.Folder + "DocumentsSample.xls";
			using (var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				DataImporter.ImportIATA_TACT(fileName, reader);
			}

			RateEntryCollection airRateEntries = CompanyTariff.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection;
			AssertEquals(11, airRateEntries.Count);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[0], "0001", UPERatingConstants.ServiceLevels.Documents);
			AssertEquals(1, airRateEntries[0].RateLines.Count);
			RateLine rateLine = airRateEntries[0].RateLines[0];
			AssertEquals(7, rateLine.RateLineItems.Count);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[4], "-", 0.5m, 0m, 29m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[5], "+", 0.5m, 0m, 41m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[6], "+", 1m, 0m, 51m);
			AssertRateEntryDataCorrect(airRateEntries[0], "0001", UPERatingConstants.ServiceLevels.Documents);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[1], "0002", UPERatingConstants.ServiceLevels.Documents);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[2], "0003", UPERatingConstants.ServiceLevels.Documents);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[3], "0004", UPERatingConstants.ServiceLevels.Documents);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[4], "0005", UPERatingConstants.ServiceLevels.Documents);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[5], "0006", UPERatingConstants.ServiceLevels.Documents);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[6], "0007", UPERatingConstants.ServiceLevels.Documents);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[7], "0008", UPERatingConstants.ServiceLevels.Documents);
			rateLine = airRateEntries[7].RateLines[0];
			AssertEquals(7, rateLine.RateLineItems.Count);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[4], "-", 0.5m, 0m, 103m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[5], "+", 0.5m, 0m, 119m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[6], "+", 1m, 0m, 134m);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[8], "0009", UPERatingConstants.ServiceLevels.Documents);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[9], "0010", UPERatingConstants.ServiceLevels.Documents);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[10], "0011", UPERatingConstants.ServiceLevels.Documents);
			rateLine = airRateEntries[10].RateLines[0];
			AssertEquals(7, rateLine.RateLineItems.Count);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[4], "-", 0.5m, 0m, 140m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[5], "+", 0.5m, 0m, 171m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[6], "+", 1m, 0m, 202m);
			AssertEquals("Import Completed.", DataImporter.ImportReport);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2007, 01, 01)]
		public void TestDoImportExpressPackage()
		{
			string fileName = UPETestHelper.TestFiles.Rating.Folder + "PackagesSample.xls";
			using (var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				DataImporter.ImportIATA_TACT(fileName, reader);
			}

			RateEntryCollection airRateEntries = CompanyTariff.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection;
			AssertEquals(11, airRateEntries.Count);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[0], "0001", UPERatingConstants.ServiceLevels.ExpressPackages);
			AssertEquals(1, airRateEntries[0].RateLines.Count);
			RateLine rateLine = airRateEntries[0].RateLines[0];
			AssertEquals(8, rateLine.RateLineItems.Count);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[4], "-", 0.5m, 0m, 48m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[5], "+", 0.5m, 0m, 60m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[6], "+", 1m, 0m, 72m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[7], "+", 1.5m, 12.3m, 871m);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[1], "0002", UPERatingConstants.ServiceLevels.ExpressPackages);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[2], "0003", UPERatingConstants.ServiceLevels.ExpressPackages);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[3], "0004", UPERatingConstants.ServiceLevels.ExpressPackages);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[4], "0005", UPERatingConstants.ServiceLevels.ExpressPackages);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[5], "0006", UPERatingConstants.ServiceLevels.ExpressPackages);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[6], "0007", UPERatingConstants.ServiceLevels.ExpressPackages);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[7], "0008", UPERatingConstants.ServiceLevels.ExpressPackages);
			rateLine = airRateEntries[7].RateLines[0];
			AssertEquals(8, rateLine.RateLineItems.Count);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[4], "-", 0.5m, 0m, 127m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[5], "+", 0.5m, 0m, 145m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[6], "+", 1m, 0m, 161m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[7], "+", 1.5m, 19.9m, 1436m);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[8], "0009", UPERatingConstants.ServiceLevels.ExpressPackages);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[9], "0010", UPERatingConstants.ServiceLevels.ExpressPackages);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[10], "0011", UPERatingConstants.ServiceLevels.ExpressPackages);
			rateLine = airRateEntries[10].RateLines[0];
			AssertEquals(8, rateLine.RateLineItems.Count);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[4], "-", 0.5m, 0m, 155m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[5], "+", 0.5m, 0m, 187m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[6], "+", 1m, 0m, 219m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[7], "+", 1.5m, 37.7m, 2641m);
			AssertEquals("Import Completed.", DataImporter.ImportReport);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2007, 01, 01)]
		public void TestDoImportExpressSaver()
		{
			string fileName = UPETestHelper.TestFiles.Rating.Folder + "ExpressSaverSample.xls";
			using (var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				DataImporter.ImportIATA_TACT(fileName, reader);
			}

			RateEntryCollection airRateEntries = CompanyTariff.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection;
			AssertEquals(9, airRateEntries.Count);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[0], "0001", UPERatingConstants.ServiceLevels.ExpressSaver);
			AssertEquals(1, airRateEntries[0].RateLines.Count);
			RateLine rateLine = airRateEntries[0].RateLines[0];
			AssertEquals(8, rateLine.RateLineItems.Count);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[4], "-", 0.5m, 0m, 43m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[5], "+", 0.5m, 0m, 45m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[6], "+", 1m, 0m, 50m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[7], "+", 1.5m, 9.9m, 705m);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[1], "0002", UPERatingConstants.ServiceLevels.ExpressSaver);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[2], "0003", UPERatingConstants.ServiceLevels.ExpressSaver);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[3], "0004", UPERatingConstants.ServiceLevels.ExpressSaver);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[4], "0005", UPERatingConstants.ServiceLevels.ExpressSaver);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[5], "0006", UPERatingConstants.ServiceLevels.ExpressSaver);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[6], "0007", UPERatingConstants.ServiceLevels.ExpressSaver);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[7], "0008", UPERatingConstants.ServiceLevels.ExpressSaver);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[8], "0009", UPERatingConstants.ServiceLevels.ExpressSaver);
			rateLine = airRateEntries[8].RateLines[0];
			AssertEquals(8, rateLine.RateLineItems.Count);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[4], "-", 0.5m, 0m, 135.104m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[5], "+", 0.5m, 0m, 165.98m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[6], "+", 1m, 0m, 194.36m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[7], "+", 1.5m, 33.1044m, 2512.3875m);
			AssertEquals("Import Completed.", DataImporter.ImportReport);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2007, 01, 01)]
		public void TestDoImportExpeditedPackage()
		{
			string fileName = UPETestHelper.TestFiles.Rating.Folder + "ExpeditedPackagesSample.xls";
			using (var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				DataImporter.ImportIATA_TACT(fileName, reader);
			}

			RateEntryCollection airRateEntries = CompanyTariff.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection;
			AssertEquals(5, airRateEntries.Count);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[0], "0007", UPERatingConstants.ServiceLevels.ExpeditedPackages);
			RateLine rateLine = airRateEntries[0].RateLines[0];
			AssertEquals(8, rateLine.RateLineItems.Count);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[4], "-", 1m, 0m, 108m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[5], "+", 1m, 0m, 136m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[6], "+", 2m, 0m, 163m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[7], "+", 3m, 15.3m, 1540m);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[1], "0008", UPERatingConstants.ServiceLevels.ExpeditedPackages);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[2], "0009", UPERatingConstants.ServiceLevels.ExpeditedPackages);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[3], "0010", UPERatingConstants.ServiceLevels.ExpeditedPackages);
			AssertRateEntryDataWithCombinedRateCalc(airRateEntries[4], "0011", UPERatingConstants.ServiceLevels.ExpeditedPackages);
			rateLine = airRateEntries[4].RateLines[0];
			AssertEquals(8, rateLine.RateLineItems.Count);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[4], "-", 1m, 0m, 180m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[5], "+", 1m, 0m, 238m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[6], "+", 2m, 0m, 285m);
			AssertRateLineItemDataCorrect(rateLine.RateLineItems[7], "+", 3m, 24.7m, 2475m);
			AssertEquals("Import Completed.", DataImporter.ImportReport);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportEmptyFile()
		{
			string fileName = UPETestHelper.TestFiles.Rating.Folder + "EmptySample.xls";
			using (var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				DataImporter.ImportIATA_TACT(fileName, reader);
			}

			AssertEquals("The file you have chosen to import is invalid.", ((UPERateDataImporter)DataImporter).ImportReport);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportInvalidFile()
		{
			string fileName = UPETestHelper.TestFiles.Rating.Folder + "InvalidSample.xls";
			using (var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				DataImporter.ImportIATA_TACT(fileName, reader);
			}

			AssertEquals("The file you have chosen to import is invalid.", ((UPERateDataImporter)DataImporter).ImportReport);
		}

		public void TestIsStandardTACTImport()
		{
			AssertEquals(false, DataImporter.IsStandardTACTImport);
		}

		#region Assertions
		void AssertRateEntryDataWithFlatRateCalc(RateEntry rateEntry, string originRC, ZDecimal flatRateAmount, string serviceLevel)
		{
			AssertEquals(1, rateEntry.RateLines.Count);
			RateLine rateLine = rateEntry.RateLines[0];
			AssertEquals(FlatCalculator.Code, rateLine.TL_RateCalculator);
			AssertEquals(flatRateAmount, ((FlatCalculator)rateLine.Calculator).BaseRate);
			AssertEquals(Env.Registry.FreightChargeCode, rateLine.TL_AC);
			AssertRateEntryDataCorrect(rateEntry, originRC, serviceLevel);
		}

		void AssertRateEntryDataCorrect(RateEntry rateEntry, string originRC, string serviceLevel)
		{
			AssertEquals(originRC, rateEntry.TI_OriginLRC);
			AssertEquals(Core.Constants.CountryCodes.Australia, rateEntry.TI_DestinationLRC);
			AssertEquals(Core.Constants.CurrencyCodes.Australia, rateEntry.TI_RX_NKCurrency);
			AssertEquals(serviceLevel, rateEntry.TI_RS_NKServiceLevel_NI);
			AssertEquals(new ZDateTime(2007, 01, 01), rateEntry.TI_RateStartDate);
			AssertEquals(new ZDateTime(2009, 01, 01), rateEntry.TI_RateEndDate);
		}

		void AssertRateEntryDataWithCombinedRateCalc(RateEntry rateEntry, string originRC, string serviceLevel)
		{
			AssertEquals(1, rateEntry.RateLines.Count);
			RateLine rateLine = rateEntry.RateLines[0];
			AssertEquals(CombinedCalculator.Code, rateLine.TL_RateCalculator);
			AssertEquals(Env.Registry.FreightChargeCode, rateLine.TL_AC);
			AssertEquals(true, rateLine.Calculator.UseInclusiveBreaks);
			AssertEquals(true, rateLine.Calculator.IsAccumulated);
			AssertEquals(RatingRoundingTypes.UpTo1, rateLine.TL_Rounding);
			AssertEquals(Core.Constants.Weight.Kilograms, rateLine.TL_WeightVolume);
			AssertRateEntryDataCorrect(rateEntry, originRC, serviceLevel);
		}

		void AssertRateLineItemDataCorrect(RateLineItem rateLineItem, string operationType, ZDecimal @break, ZDecimal rate, ZDecimal flatAmount)
		{
			AssertEquals(operationType, rateLineItem.TM_Type);
			AssertEquals(@break, rateLineItem.TM_Break);
			AssertEquals(rate, rateLineItem.TM_Value);
			AssertEquals(flatAmount, rateLineItem.TM_FlatAmount);
		}

		#endregion
		#region Setup
		void SetUpImportRegions()
		{
			AddZone("001");
			AddZone("002");
			AddZone("003");
			AddZone("004");
			AddZone("005");
			AddZone("006");
			AddZone("007");
			AddZone("008");
			AddZone("009");
			AddZone("010");
			AddZone("011");
		}

		void AddZone(string zoneCode)
		{
			RefZoneHeader zone = Factory.NewWithValidTestData<RefZoneHeader>();
			zone.FZ_Code = zoneCode;
			zone.FZ_Description = zoneCode;
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetUpImportRegions();
			UPERateDataImporter.RegisterType(typeof(UPERateDataImporter));
			CompanyTariff = Factory.New<CompanyTariff>();
			DataImporter = UPERateDataImporter.New(CompanyTariff);
		}

		CompanyTariff CompanyTariff;
		RateDataImporter DataImporter;
		#endregion
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return UPERateDataImporter.New(Factory.New<CompanyTariff>());
		}
		#endregion
	}
}
