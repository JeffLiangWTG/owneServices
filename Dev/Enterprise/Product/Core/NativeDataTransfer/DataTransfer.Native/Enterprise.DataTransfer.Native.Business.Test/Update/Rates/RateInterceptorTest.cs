using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Business.Xml.Deserializers;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.Rates
{
	[SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
	public class RateInterceptorTest : TransactionedTestCase
	{
		public void TestRateLinesWithCompanyTariffLevelHigherThanOneAreSkippedForCost()
		{
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.CostingWithLevel2RateLines.xml"));
			manager.Import(data.BaseStream);
			var logs = GetLogsMessage();
			AssertContains("Skipped Rate Line with Tariff Level 2", logs);

			var rateLinesHighLevelQuery = new ZDBOnlyQuery(typeof(RateLine));
			rateLinesHighLevelQuery.AddToFilter(RateLinesSchema.TL_CompanyTariffLevel, SQLComparisonOperator.GreaterThan, (byte)1);
			var rateLinesHighLevel = factory.Load<RateLine>(rateLinesHighLevelQuery);
			AssertEquals("Should be no RateLines with Tariff Level > 1", 0, rateLinesHighLevel.Length);

			var rateLinesLowLevelQuery = new ZDBOnlyQuery(typeof(RateLine));
			rateLinesLowLevelQuery.AddToFilter(RateLinesSchema.TL_CompanyTariffLevel, SQLComparisonOperator.Equal, (byte)1);
			var rateLinesLowLevel = factory.Load<RateLine>(rateLinesLowLevelQuery);
			AssertNotEquals("Should be RateLines with Tariff Level  1", 0, rateLinesLowLevel.Length);
		}

		public void TestCostLinesWithBadFormatCompanyTariffLevelAreHandledInStandardWay()
		{
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.CostingWithLevel2RateLines.xml"));
			var xml = data.ReadToEnd();
			var badXml = xml.Replace("<CompanyTariffLevel>2", "<CompanyTariffLevel>X");
			var stream = new MemoryStream(Encoding.ASCII.GetBytes(badXml));
			manager.Import(stream);
			var logs = GetLogsMessage();
#if NET
			var expectedMessage =
				@"Test error: [RateLines.CompanyTariffLevel] : The input string 'X' was not in a correct format.Couldn't store <X> in CompanyTariffLevel Column.  Expected type is Byte.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.";
#else
			var expectedMessage =
				@"Test error: [RateLines.CompanyTariffLevel] : Input string was not in a correct format.Couldn't store <X> in CompanyTariffLevel Column.  Expected type is Byte.
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.";
#endif
			AssertContains(expectedMessage, logs);
			AssertNull("should be no error report", ErrorReporter.LastExceptionReported);
		}

		public void TestRateLinesWithoutCompanyTariffLevelInformation_NoExceptionOccurs()
		{
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.CostingWithoutCompanyTariffLevelProperty.xml"));
			manager.Import(data.BaseStream);
			var logs = GetLogsMessage();

			Assert("Should not contain error", !logs.Contains("Test error: Can not find property CompanyTariffLevel"));
		}

		public void TestRateLinesWithCompanyTariffLevelHigherThanOneAreNotSkippedForTariff()
		{
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.CompanyTariffWithLeve2RateLines.xml"));
			manager.Import(data.BaseStream);
			var logs = GetLogsMessage();
			AssertCollectionNotContains("Skipped Rate Line with Tariff Level 2", logs);
			var rateLinesQuery = new ZDBOnlyQuery(typeof(RateLine));
			rateLinesQuery.AddToFilter(RateLinesSchema.TL_CompanyTariffLevel, SQLComparisonOperator.GreaterThan, (byte)1);
			var rateLines = factory.Load<RateLine>(rateLinesQuery);
			AssertNotEquals("There should be RateLines with Tariff Level > 1", 0, rateLines.Length);
		}

		public void TestRateLinesForErrorTypeLogs()
		{
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.CostingWithLevel2RateLines.xml"));
			manager.Import(data.BaseStream);
			var errorLogs = dummyLogger.Buffer.Logs().Where(log => log.Type == LogType.Error).ToList();
			AssertEquals("There should be no Error Logs", 0, errorLogs.Count);
		}

		public void TestCarrierServiceLevelMatchesCarrierOrg()
		{
			var carrier1 = factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_Code = "CHISHI_WW";
			carrier1.OH_IsShippingLine = true;
			var carrierServiceLevel1 = carrier1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel1.PL_Code = "URG";
			carrierServiceLevel1.PL_CarrierServiceLevelDescription = "Urgent";

			var carrier2 = factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_Code = "SEASTA_WW";
			carrier2.OH_IsShippingLine = true;
			var carrierServiceLevel2 = carrier2.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel2.PL_Code = "URG";
			carrierServiceLevel2.PL_CarrierServiceLevelDescription = "Urgent";
			var carrierServiceLevel3 = carrier2.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel3.PL_Code = "BUL";
			carrierServiceLevel3.PL_CarrierServiceLevelDescription = "Bulk";
			factory.Save();

			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.ClientRateWithCarrierServiceLevels.xml"));
			manager.Import(data.BaseStream);

			var actualLogs = GetLogsMessage();
			var expectedLogs = @"RatingHeader - 1 inserts, 0 updates, 0 deletes
RateEntry - 3 inserts, 0 updates, 0 deletes
RateLines - 3 inserts, 0 updates, 0 deletes
RateLineItems - 3 inserts, 0 updates, 0 deletes";

			AssertMultilineASCIIEquals("Should insert three rate entries", expectedLogs, actualLogs);

			var rateEntries = factory.Load<RateEntry>(new ZQuery());

			CombineAssertions(() =>
			{
				AssertEquals("Should should have created three rate entries", 3, rateEntries.Length);

				var urgentServiceEntry = rateEntries.Where(entry => entry[RateEntrySchema.TI_PL_NKCarrierServiceLevel].ToString() == "URG");
				var bulkServiceEntry = rateEntries.Where(entry => entry[RateEntrySchema.TI_PL_NKCarrierServiceLevel].ToString() == "BUL");
				var emptyServiceEntry = rateEntries.Where(entry => string.IsNullOrEmpty(entry[RateEntrySchema.TI_PL_NKCarrierServiceLevel].ToString()));

				AssertEquals("Should be able to import 'URG' carrier srv. lvl. without getting confused between the two carriers", 1, urgentServiceEntry.Count());
				AssertEquals(1, bulkServiceEntry.Count());
				AssertEquals("Should not cause problems with an empty service entry", 1, emptyServiceEntry.Count());
			});
		}

		public void TestRateLinesWithoutOwnerCode_NoExceptionOccurs()
		{
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.CostingWithoutOwnerCode.xml"));
			manager.Import(data.BaseStream);
			var errorLogs = dummyLogger.Buffer.Logs().Where(log => log.Type == LogType.Error).ToArray();
			AssertEquals(0, errorLogs.Length);

			var ratingHeaders = factory.Load<RatingHeader>(new ZQuery());

			AssertEquals(1, ratingHeaders.Length);
		}

		public void TestRateXMLImportsIntoCompanyFromXML()
		{
			var expectedCompanyPK = factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "EDI")).PK;

			var anotherCompany = factory.NewWithValidTestData<GlbCompany>();
			var anotherBranch = anotherCompany.Branches.AddNew().PK.ToGuid();
			factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, anotherBranch, Env.CurrentDepartmentPK))
			{
				var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.CostingWithoutOwnerCode.xml"));
				manager.Import(data.BaseStream);

				var actualLogs = GetLogsMessage();
				var expectedLogs = @"RatingHeader - 1 inserts, 0 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 2 inserts, 0 updates, 0 deletes
RateLineItems - 11 inserts, 0 updates, 0 deletes";

				AssertMultilineASCIIEquals("Should insert three rate entries", expectedLogs, actualLogs);

				var ratingHeaders = factory.Load<RatingHeader>(new ZQuery());

				CombineAssertions(() =>
				{
					AssertEquals("Should should have created the Costing", 1, ratingHeaders.Length);

					var actualCompany = ratingHeaders[0].TH_GC;
					AssertNotEquals("Should not import into current company", anotherCompany.PK, actualCompany);
					AssertEquals("Should import company from the XML", expectedCompanyPK, actualCompany);
				});
			}
		}

		public void TestRateLinesContainingInvalidConversionFactor_FailsToImport()
		{
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.InvalidConversionFactor.xml"));
			manager.Import(data.BaseStream);

			var actualLogs = GetLogsMessage();
			var expectedLogs = "Test error: Expected: conversion factor, numerator and denominator to be all set or all empty. Actual: 0.000 KG/M3";

			var ratingHeaders = factory.Load<RatingHeader>(new ZQuery());

			CombineAssertions(() =>
			{
				AssertContains("There is a validation message", expectedLogs, actualLogs);
				AssertEquals("Should NOT have created the Costing", 0, ratingHeaders.Length);
			});
		}

		public void TestRateLinesContainingNoConversionFactor_ShouldImport()
		{
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.NoConversionFactor.xml"));
			manager.Import(data.BaseStream);

			var ratingHeaders = factory.Load<RatingHeader>(new ZQuery());

			AssertEquals("Should have created the Costing", 1, ratingHeaders.Length);
		}

		public void TestGlobalCostingSecurityIsAllowed_CostingXMLWithoutCompany_ImportsAsGlobalCosting()
		{
			var isAllowed = Env.Security.GlobalCostingRates.IsAllowed;
			try
			{
				AccChargeCodeTest.SetupGlobalChargeCodeScenario(factory, out _, out _, out _, true, false, "FRT", "FRT");

				Env.Security.GlobalCostingRates.IsAllowed = true;
				var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.CostingWithoutGlbCompany.xml"));
				manager.Import(data.BaseStream);

				var actualLogs = GetLogsMessage();
				var expectedLogs = @"RatingHeader - 1 inserts, 0 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 1 inserts, 0 updates, 0 deletes
RateLineItems - 1 inserts, 0 updates, 0 deletes";

				var ratingHeaders = factory.Load<RatingHeader>(new ZQuery());

				CombineAssertions(() =>
				{
					AssertMultilineASCIIEquals("Expected to have successfully imported the Costing", expectedLogs, actualLogs);

					var globalCosting = ratingHeaders.Single();
					AssertNotNull("Expected a single result", globalCosting);
					Assert("Should be a global costing", globalCosting.IsGlobalCostRate());

					var rateEntry = globalCosting.AllEntries.Single();
					AssertNotNull("Expected a single RateEntry", rateEntry);
					AssertEquals(RatingConstants.RateCategory.LCL, rateEntry.TI_RateCategory);
					AssertEquals(Core.Constants.RateMode.LCL, rateEntry.TI_Mode);
					AssertEquals(Env.CurrentCompanyPK, rateEntry.TI_GC_Publisher);
					AssertEquals("AU", rateEntry.TI_OriginLRC);
					AssertEquals(new ZDate(2017, 01, 01), rateEntry.TI_RateStartDate);
					AssertEquals(ZDate.Empty, rateEntry.TI_RateEndDate);

					var rateLine = (RateLine)rateEntry.RateLines.Single();
					AssertNotNull("Expected a single RateLine", rateLine);
					AssertEquals("FLT", rateLine.TL_RateCalculator);
					AssertEquals("FRT", rateLine.ChargeCode.AC_Code);

					rateEntry.TI_RateCategory = RatingConstants.RateCategory.ORG;
					factory.Save();
				});

				data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.CostingWithoutGlbCompany.xml"));
				manager.Import(data.BaseStream);

				var newFactory = new BusinessObjectFactory();
				ratingHeaders = newFactory.Load<RatingHeader>(new ZQuery());

				var reloadedRateEntries = ratingHeaders.Single().AllEntries;
				AssertEquals(2, reloadedRateEntries.Count());
			}
			finally
			{
				Env.Security.GlobalCostingRates.IsAllowed = isAllowed;
			}
		}

		public void TestGlobalCostingSecurityNotAllowed_CostingXMLWithoutCompany_ImportsAsGlobalCosting()
		{
			var isAllowed = Env.Security.GlobalCostingRates.IsAllowed;
			try
			{
				Env.Security.GlobalCostingRates.IsAllowed = false;
				var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.CostingWithoutGlbCompany.xml"));
				manager.Import(data.BaseStream);

				var actualLogs = GetLogsMessage().Trim();
				var expectedLogs = @"Test error: You do not have security rights to import Costing XMLs without a Company. Either add a GlbCompany to import as Local Costing or enable security: Manage -> Tariffs & Rates -> Costing -> Global Costings
Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.".Trim();

				var ratingHeaders = factory.Load<RatingHeader>(new ZQuery());

				CombineAssertions(() =>
				{
					AssertMultilineASCIIEquals("There is a validation message", expectedLogs, actualLogs);
					AssertEquals("Should NOT should have created the Costing", 0, ratingHeaders.Length);
				});
			}
			finally
			{
				Env.Security.GlobalCostingRates.IsAllowed = isAllowed;
			}
		}

		public void TestRateEntriesUpdateCreationSource()
		{
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.CostingWithDefaultCarrierServiceLevels.xml"));
			manager.Import(data.BaseStream);

			var ratingHeader = factory.LoadTop1<RatingHeader>(new ZQuery());
			var rateEntries = ratingHeader.AllEntries;
			Assert("There should be some RateEntries", rateEntries.Any());
			Assert("All rate entries should have marked their creation source as XML", rateEntries.All(x => x.TI_CreationSource == "XML"));
		}

		public void TestRateEntriesContainingInvalidRateStartDate_FailsToImport()
		{
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.InvalidRateStartDate.xml"));
			manager.Import(data.BaseStream);

			var actualLogs = GetLogsMessage();
			var expectedLogs = "RatingHeader.RateEntry.RateStartDate validation failed: '0201-01-19' is not within the range for date (1/01/1753 - 31/12/9999)";

			var ratingHeaders = factory.Load<RatingHeader>(new ZQuery());

			CombineAssertions(() =>
			{
				AssertContains("There is a validation message", expectedLogs, actualLogs);
				AssertEquals("Should NOT have created the Costing", 0, ratingHeaders.Length);
			});
		}

		public void TestRateEntriesContainingInvalidRateEndDate_FailsToImport()
		{
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.InvalidRateEndDate.xml"));
			manager.Import(data.BaseStream);

			var actualLogs = GetLogsMessage();
			var expectedLogs = "RatingHeader.RateEntry.RateEndDate validation failed: '0201-01-31' is not within the range for date (1/01/1753 - 31/12/9999)";

			var ratingHeaders = factory.Load<RatingHeader>(new ZQuery());

			CombineAssertions(() =>
			{
				AssertContains("There is a validation message", expectedLogs, actualLogs);
				AssertEquals("Should NOT have created the Costing", 0, ratingHeaders.Length);
			});
		}

		public void TestRateEntriesContainingInvalidConditionalExpression_FailsToImport()
		{
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.InvalidConditionalExpression.xml"));
			manager.Import(data.BaseStream);

			var actualLogs = GetLogsMessage();
			var expectedLogs = "RateLines.ConditionalExpression validation failed: ConditionalExpression can only be set when Condition is set to USR";

			var ratingHeaders = factory.Load<RatingHeader>(new ZQuery());

			CombineAssertions(() =>
			{
				AssertContains("There is a validation message", expectedLogs, actualLogs);
				AssertEquals("Should NOT have created the Costing", 0, ratingHeaders.Length);
			});
		}

		public void TestRateEntriesContainingInvalidCondition_FailsToImport()
		{
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.InvalidCondition.xml"));
			manager.Import(data.BaseStream);

			var actualLogs = GetLogsMessage();
			var expectedLogs = "RateLines.ConditionalExpression validation failed: ConditionalExpression should be specified when Condition is set to USR";

			var ratingHeaders = factory.Load<RatingHeader>(new ZQuery());

			CombineAssertions(() =>
			{
				AssertContains("There is a validation message", expectedLogs, actualLogs);
				AssertEquals("Should NOT have created the Costing", 0, ratingHeaders.Length);
			});
		}

		public void TestRateEntriesContainingEmptyRateEndDate_ShouldImport()
		{
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.EmptyRateEndDate.xml"));
			manager.Import(data.BaseStream);

			var ratingHeaders = factory.Load<RatingHeader>(new ZQuery());

			AssertEquals("Should have created the Costing", 1, ratingHeaders.Length);
		}

		[TestDate(2017, 04, 26)]
		public void TestQuotationXMLWithoutCompany_FailsToImport()
		{
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.QuotationWithoutGlbCompany.xml"));
			manager.Import(data.BaseStream);

			var actualLogs = GetLogsMessage();
			var expectedLogs = "Test error: To import this Quotation XML, please add the GlbCompany.";

			var ratingHeaders = factory.Load<RatingHeader>(new ZQuery());

			CombineAssertions(() =>
			{
				AssertContains("Should NOT save the Quotation", expectedLogs, actualLogs);
				AssertEquals("Should NOT save the Quotation", 0, ratingHeaders.Length);
			});
		}

		public void TestImportNativeXML_RateHasStandardCarrierServiceLevel_ShouldImportWithoutErrors()
		{
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.CostingWithDefaultCarrierServiceLevels.xml"));
			manager.Import(data.BaseStream);

			var actualLogs = GetLogsMessage();
			var expectedLogs = @"RatingHeader - 1 inserts, 0 updates, 0 deletes
RateEntry - 3 inserts, 0 updates, 0 deletes
RateLines - 3 inserts, 0 updates, 0 deletes
RateLineItems - 19 inserts, 0 updates, 0 deletes";

			AssertMultilineASCIIEquals("Should insert rate entries", expectedLogs, actualLogs);

			var rateEntries = factory.Load<RateEntry>(new ZQuery());
			AssertEquals(true, rateEntries.All(entry => entry[RateEntrySchema.TI_PL_NKCarrierServiceLevel].ToString() == "STD"));
		}

		public void TestImportNativeXML_TM_AC()
		{
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.ClientRateImportWithTM_AC.xml"));
			manager.Import(data.BaseStream);

			var actualLogs = GetLogsMessage();
			var expectedLogs = @"RatingHeader - 1 inserts, 0 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 1 inserts, 0 updates, 0 deletes
RateLineItems - 1 inserts, 0 updates, 0 deletes";
			AssertMultilineASCIIEquals(expectedLogs, actualLogs);

			dummyLogger.Clear();

			var rateLineItem = factory.LoadTop1<RateLineItem>(new ZQuery());
			AssertEquals(new ZGuid("8319278c-e149-4895-bc52-114e69e069d9"), rateLineItem.TM_AC);
		}

		public void TestImportNativeXML_RatingHeaderCompanySetToAnotherOne_ShouldCreateNewRates()
		{
			var dataAsString = ImportNativeXML_RatingHeaderCompanySetToAnotherOne_PrepareData(false, true);

			using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(dataAsString)))
			{
				manager.Import(stream);
			}

			var actualLogs =  GetLogsMessage();

			var expectedLogs = @"RatingHeader - 1 inserts, 0 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 1 inserts, 0 updates, 0 deletes
RateLineItems - 1 inserts, 0 updates, 0 deletes";
			AssertMultilineASCIIEquals(expectedLogs, actualLogs);

			AssertChargeCodesForCompany(ediCompanyPK, true, "should not delete the existing rating header");
			AssertChargeCodesForCompany(demoCompanyPK, true, "should create new rating header");
			AssertChargeCodesForCompany(Guid.Empty, false, "no global rate created");
		}

		public void TestImportNativeXML_OnlyActionsModifiedToINSERT_SameCompany_ShouldShowError()
		{
			var dataAsString = ImportNativeXML_RatingHeaderCompanySetToAnotherOne_PrepareData(false, false, "INSERT");

			using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(dataAsString)))
			{
				manager.Import(stream);
			}

			var actualLogs = GetLogsMessage();

			// TH_OH, TH_GC, TH_RateType, TH_QuoteNumber, and TH_GlobalRateLevel haven't changed so the rate could not be inserted.
			var expectedLogs = "Test error: Error from Data layer: TableName=RatingHeader";
			Assert("Should have error", actualLogs.Contains(expectedLogs));
		}

		public void TestImportNativeXML_OnlyActionsModifiedToINSERT_LoginToAnotherCompany_ShouldCreateNewRate()
		{
			var dataAsString = ImportNativeXML_RatingHeaderCompanySetToAnotherOne_PrepareData(false, false, "INSERT");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, demoBranchPK, Env.CurrentDepartmentPK))
			{
				using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(dataAsString)))
				{
					manager.Import(stream);
				}

				var actualLogs = GetLogsMessage();

				var expectedLogs = @"RatingHeader - 1 inserts, 0 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 1 inserts, 0 updates, 0 deletes
RateLineItems - 1 inserts, 0 updates, 0 deletes";
				AssertMultilineASCIIEquals(expectedLogs, actualLogs);

				AssertChargeCodesForCompany(ediCompanyPK, true, "should not delete the existing rating header");
				AssertChargeCodesForCompany(demoCompanyPK, true, "should create new rating header");
				AssertChargeCodesForCompany(Guid.Empty, false, "no global rate created");
			}
		}

		public void TestImportNativeXML_RatingHeaderCompanySetToAnotherOne_RemovePKs_ShouldCreateNewRates()
		{
			var dataAsString = ImportNativeXML_RatingHeaderCompanySetToAnotherOne_PrepareData(true, true);

			using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(dataAsString)))
			{
				manager.Import(stream);
			}

			var actualLogs = GetLogsMessage();

			var expectedLogs = @"RatingHeader - 1 inserts, 0 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 1 inserts, 0 updates, 0 deletes
RateLineItems - 1 inserts, 0 updates, 0 deletes";
			AssertMultilineASCIIEquals(expectedLogs, actualLogs);

			AssertChargeCodesForCompany(ediCompanyPK, true, "should not delete the existing rating header");
			AssertChargeCodesForCompany(demoCompanyPK, true, "should create new rating header");
			AssertChargeCodesForCompany(Guid.Empty, false, "no global rate created");
		}

		public void TestImportNativeXML_OnlyRemovePKs_ShouldUpdateExistingRates_EvenWhenLoggingToAnotherCompany()
		{
			var dataAsString = ImportNativeXML_RatingHeaderCompanySetToAnotherOne_PrepareData(true, false);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, demoBranchPK, Env.CurrentDepartmentPK))
			{
				using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(dataAsString)))
				{
					manager.Import(stream);
				}

				var actualLogs = GetLogsMessage();

				var expectedLogs = @"RatingHeader - 0 inserts, 0 updates, 0 deletes
RateEntry - 0 inserts, 0 updates, 0 deletes
RateLines - 0 inserts, 0 updates, 0 deletes
RateLineItems - 0 inserts, 0 updates, 0 deletes";
				AssertMultilineASCIIEquals(expectedLogs, actualLogs);

				AssertChargeCodesForCompany(ediCompanyPK, true, "should update the existing rating header for company in XML");
				AssertChargeCodesForCompany(demoCompanyPK, false, "should not create new rating header for current company");
				AssertChargeCodesForCompany(Guid.Empty, false, "no global rate created");
			}
		}

		public void TestImportNativeXML_ShouldCreateNewRatesForNewTargetCompany()
		{
			var dataAsString = ImportNativeXML_RatingHeaderCompanySetToAnotherOne_PrepareData(false, false);

			using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(dataAsString)))
			{
				ImportNativeXMLWithCustomizedHeaderData(stream);
			}

			var actualLogs = GetLogsMessage();

			var expectedLogs = $@"GlbCompany set by TargetCompanyPK ({demoCompanyPK}) from ediMessage.";
			AssertMultilineASCIIEquals(expectedLogs, actualLogs);

			AssertChargeCodesForCompany(ediCompanyPK, true, "should not delete the existing rating header");
			AssertChargeCodesForCompany(demoCompanyPK, true, "should create new rating header");
			AssertChargeCodesForCompany(Guid.Empty, false, "no global rate created");
		}

		public void TestImportNativeXML_ShouldConsiderCarrierContractNumberAsDifferentiatorForRateEntry()
		{
			var carrier = factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "SEASTA_WW";
			carrier.OH_IsShippingLine = true;
			factory.Save();

			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.CarrierCostingWithMultileCarrierContractNumberEntries.xml"));
			manager.Import(data.BaseStream);

			var actualLogs = GetLogsMessage();
			var expectedLogs = @"RatingHeader - 1 inserts, 0 updates, 0 deletes
RateEntry - 3 inserts, 0 updates, 0 deletes
RateLines - 3 inserts, 0 updates, 0 deletes
RateLineItems - 6 inserts, 0 updates, 0 deletes";

			AssertMultilineASCIIEquals("Should insert three rate entries", expectedLogs, actualLogs);

			var rateEntries = factory.Load<RateEntry>(new ZQuery());

			CombineAssertions(() =>
			{
				AssertEquals("Should should have created three rate entries", 3, rateEntries.Length);

				var first = rateEntries.Where(entry => entry[RateEntrySchema.TI_ContractNumber].ToString() == "TEST1");
				var second = rateEntries.Where(entry => entry[RateEntrySchema.TI_ContractNumber].ToString() == "TEST2");
				var third = rateEntries.Where(entry => entry[RateEntrySchema.TI_ContractNumber].ToString() == "TEST3");

				AssertEquals("Should import all entries with different carrier contract number : TEST1", 1, first.Count());
				AssertEquals("Should import all entries with different carrier contract number : TEST2", 1, second.Count());
				AssertEquals("Should import all entries with different carrier contract number : TEST3", 1, third.Count());
			});
		}

		void ImportNativeXMLWithCustomizedHeaderData(Stream stream)
		{
			var requestDeserializer = RequestDeserializerBuilder.GetDeserializer(stream);
			var request = requestDeserializer.Deserialize(stream);
			request.Settings = new HeaderData_Versioned_Native
			{
				EnableCodeMapping = true,
				OwnerCode = "DEMOORG",
				TargetCompanyPK = demoCompanyPK
			};
			var exportImportRequest = new ExportImportRequest(request);
			var parser = new EntitySetXmlDeserializer
			{
				DefinitionFinder = new DefinitionFinder { Cache = EntitySetDefinitionCache.GetInstance() }
			};
			var entitySet = parser.Deserialize(exportImportRequest.EntitySets.First(), sessionServices);

			var context = manager.RequestConverter.Convert(request.Settings);
			context.Import(entitySet);
			context.Save();
		}

		string ImportNativeXML_RatingHeaderCompanySetToAnotherOne_PrepareData(bool removePKs, bool replaceCompany, string action = "MERGE")
		{
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.ClientRateImportToEDI.xml"));
			manager.Import(data.BaseStream);

			var actualLogs = GetLogsMessage();
			var expectedLogs = @"RatingHeader - 1 inserts, 0 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 1 inserts, 0 updates, 0 deletes
RateLineItems - 1 inserts, 0 updates, 0 deletes";
			AssertMultilineASCIIEquals(expectedLogs, actualLogs);

			dummyLogger.Clear();

			string result = "";
			data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.ClientRateImportToEDI_ModifiedToImportToDEMTemplate.xml"));
			var template = data.ReadToEnd();

			if (removePKs)
			{
				result = template.Replace("{RATINGHEADER_PK}", string.Empty);
				result = result.Replace("{RATEENTRY_PK}", string.Empty);
				result = result.Replace("{RATELINES_PK}", string.Empty);
				result = result.Replace("{RATELINEITEMS_PK}", string.Empty);
			}
			else
			{
				var rateLineItems = factory.LoadTop1<RateLineItem>(new ZQuery());
				var rateLine = rateLineItems.Parent;
				var rateEntry = rateLine.Parent;
				var ratingHeader = rateEntry.Parent;

				result = template.Replace("{RATINGHEADER_PK}", "<PK>" + ratingHeader.PK.ToString() + "</PK>");
				result = result.Replace("{RATEENTRY_PK}", "<PK>" + rateEntry.PK.ToString() + "</PK>");
				result = result.Replace("{RATELINES_PK}", "<PK>" + rateLine.PK.ToString() + "</PK>");
				result = result.Replace("{RATELINEITEMS_PK}", "<PK>" + rateLineItems.PK.ToString() + "</PK>");
			}

			if (replaceCompany)
			{
				result = result.Replace("{GLBCOMPANY_CODE}", "DEM");
				result = result.Replace("{GLBCOMPANY_PK}", demoCompanyPK.ToString());
			}
			else
			{
				result = result.Replace("{GLBCOMPANY_CODE}", "EDI");
				result = result.Replace("{GLBCOMPANY_PK}", ediCompanyPK.ToString());
			}

			result = result.Replace("{RATINGHEADER_ACTION}", action);
			result = result.Replace("{RATEENTRY_ACTION}", action);
			result = result.Replace("{RATELINES_ACTION}", action);
			result = result.Replace("{RATELINEITEMS_ACTION}", action);

			return result;
		}

		void AssertChargeCodesForCompany(Guid companyPK, bool shouldHaveRatingHeader, string message)
		{
			var query = new ZQuery(RatingHeaderSchema.TH_GC, SQLComparisonOperator.Equal, companyPK);
			var ratingHeaders = factory.Load<RatingHeader>(query);

			if (!shouldHaveRatingHeader)
			{
				AssertEquals(message, false, ratingHeaders.Any());
				return;
			}

			AssertEquals("There should be rating header for company", true, ratingHeaders.Any());
			AssertEquals(message, true, ratingHeaders.All(ratingHeader =>
			{
				return ratingHeader.AllEntries.All(rateEntry =>
				{
					return rateEntry.ChildRateLines.All(rateLine => rateLine.ChargeCode.AC_GC == ratingHeader.TH_GC);
				});
			}));
		}

		readonly struct RateTestData
		{
			public RateTestData(Dictionary<string, string> replacingValues)
			{
				ReplacingValues = replacingValues;
			}

			public static RateTestData NewWithValidTestDataForProviderReferenceIDTests()
			{
				var replacingValues = new Dictionary<string, string>
				{
					// Header
					{ "{RATINGHEADER_PK}", ToPkTag("def96545-7c3b-4ad1-8ad2-87ca340dfd53") },
					{ "{RATETYPE}", "<RateType>SAL</RateType>" },
					{ "{RATELEVEL}", "0" },
					// ====================
					// RateEntry 1
					{ "{RATEENTRY_ACTION1}", "MERGE" },
					{ "{RATEENTRY_PK1}", ToPkTag("78757535-af18-4c06-95f5-b94df4e66295") },
					{ "{RATEENTRY_PROVIDER_REFERENCE_ID1}", "" },
					{ "{RATE_START_DATE1}", "2016-01-01T00:00:00" },
					{ "{ORIGIN1}", "AUSYD" },
					// RateLine 1
					{ "{RATELINE_ACTION1}", "MERGE" },
					{ "{RATELINE_PK1}", ToPkTag("645806d9-8481-4c1a-ab53-f8ff9e4b7f63") },
					{ "{ROUNDING1}", "DEF" },
					{ "{RATELINE_PROVIDER_REFERENCE_ID1}", "" },
					// RateLineItem 1
					{ "{RATELINEITEM_ACTION1}", "MERGE" },
					{ "{RATELINEITEM_PK1}", ToPkTag("b63a7346-4ef6-4768-b42d-ba3b38ada92d") },
					{ "{BREAK_MINIMUM1}", "0" },
					// ====================
					// RateEntry 2
					{ "{RATEENTRY_ACTION2}", "MERGE" },
					{ "{RATEENTRY_PK2}", ToPkTag("eb916f9b-1697-456a-a5de-7709fddf3c98") },
					{ "{RATEENTRY_PROVIDER_REFERENCE_ID2}", "" },
					{ "{ORIGIN2}", "AUSYD" },
					// RateLine 2
					{ "{RATELINE_ACTION2}", "MERGE" },
					{ "{RATELINE_PK2}", ToPkTag("527788bd-8470-4b0e-9e3f-81def7ac5c54") },
					{ "{ROUNDING2}", "DEF" },
					{ "{RATELINE_PROVIDER_REFERENCE_ID2}", "" },
					// RateLineItem 21
					{ "{RATELINEITEM_ACTION21}", "MERGE" },
					{ "{RATELINEITEM_PK21}", ToPkTag("4ba48ed5-47d4-4331-b394-577c2bfc01b1") },
					{ "{BREAK_MINIMUM21}", "0" },
					// RateLineItem 22
					{ "{RATELINEITEM_ACTION22}", "MERGE" },
					{ "{RATELINEITEM_PK22}", ToPkTag("a03fff7a-783d-45ac-9497-9388fc567379") },
					{ "{BREAK_MINIMUM22}", "0" },
					// ====================
					// All RateLineItems
					{ "{RATELINEITEM_VALUE}", "10" },
					// Other stuffs
					{ "{GLBCOMPANY_CODE}", "EDI" },
				};

				return new RateTestData(replacingValues);
			}

			/// <summary>
			///		Generate XML from a template with the replacements defined in ReplacingValues dictionary.
			/// </summary>
			public (string originalTemplate, string outputXML) GenerateXMLFromTemplateFile(string templateFile)
			{
				if (ReplacingValues is not { Count: > 0 })
				{
					throw new InvalidOperationException("ReplacingValues must be set before calling this method.");
				}

				var data = new StreamReader(GetType().Assembly.GetManifestResourceStream(templateFile));
				var template = data.ReadToEnd();

				return (template, GenerateXMLFromTemplate(template));
			}

			public string GenerateXMLFromTemplate(string templateString)
			{
				var outputXml = new StringBuilder(templateString);

				foreach (var pair in ReplacingValues)
				{
					outputXml.Replace(pair.Key, pair.Value);
				}

				return outputXml.ToString();
			}

			public void Replace(string templateString, string templateValue)
			{
				ReplacingValues[templateString] = templateValue;
			}

			Dictionary<string, string> ReplacingValues { get; }
		}

		void Import(string xml)
		{
			using MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(xml));
			manager.Import(stream);
		}

		const string ProviderRefIdTestFile = "Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.ClientRate_ProviderRefId.xml";

		public void TestImportNativeXML_WhenNoProviderReferenceId_ShouldUpdateRateLineItemsByPks()
		{
			// First, import new rates

			var testData = RateTestData.NewWithValidTestDataForProviderReferenceIDTests();
			var (template, xml) = testData.GenerateXMLFromTemplateFile(ProviderRefIdTestFile);
			Import(xml);

			var allRateLines = factory.Load<RateLine>(new ZQuery());

			var allRateLineItems = factory.Load<RateLineItem>(new ZQuery());
			AssertContainsExactElementsInAnyOrder(
				"Precondition: RateLineItems should have initial test values",
				new ZDecimal[] { 0, 0, 0 },
				allRateLineItems.Select(x => x.TM_BreakMinimum));

			// Second, import new XML with the updated PKs to update the correct RateLineItems

			var rateLine1 = allRateLines.First(x => x.RateLineItems.Count == 1);
			var rateLine2 = allRateLines.First(x => x.RateLineItems.Count == 2);

			testData.Replace("{RATINGHEADER_PK}", ToPkTag(rateLine1.Parent.Parent.PK));

			testData.Replace("{RATEENTRY_PK1}", ToPkTag(rateLine1.Parent.PK));
			testData.Replace("{RATEENTRY_PK2}", ToPkTag(rateLine2.Parent.PK));

			testData.Replace("{RATELINE_PK1}", ToPkTag(rateLine1.PK));
			testData.Replace("{RATELINE_PK2}", ToPkTag(rateLine2.PK));

			testData.Replace("{RATELINEITEM_PK1}", ToPkTag(rateLine1.RateLineItems[0].PK));
			var rateLineItems2 = rateLine2.RateLineItems.OfType<RateLineItem>().ToArray();
			testData.Replace("{RATELINEITEM_PK21}", ToPkTag(rateLineItems2.Single(x => x.TM_LineOrder == 0).PK));
			testData.Replace("{RATELINEITEM_PK22}", ToPkTag(rateLineItems2.Single(x => x.TM_LineOrder == 1).PK));

			testData.Replace("{BREAK_MINIMUM1}", "11");
			testData.Replace("{BREAK_MINIMUM21}", "21");
			testData.Replace("{BREAK_MINIMUM22}", "22");

			xml = testData.GenerateXMLFromTemplate(template);
			Import(xml);

			// Load all items in a new factory to avoid caching
			allRateLineItems = factory.CreateNewFactory().Load<RateLineItem>(new ZQuery());
			AssertContainsExactElementsInAnyOrder(
				"RateLineItems should have the new values",
				new ZDecimal[] { 11, 21, 22 },
				allRateLineItems.Select(x => x.TM_BreakMinimum));
		}

		public void TestImportNativeXML_ShouldUpdateBlankProviderReferenceIds()
		{
			// Prepare: rates with blank ProviderReferenceIds
			var testData = RateTestData.NewWithValidTestDataForProviderReferenceIDTests();
			var (template, xml) = testData.GenerateXMLFromTemplateFile(ProviderRefIdTestFile);
			Import(xml);

			// Test: same data but with non-blank ProviderReferenceIds
			testData.Replace("{RATINGHEADER_PK}", string.Empty);
			// Entry 1
			testData.Replace("{RATEENTRY_ACTION1}", "INSERT");
			testData.Replace("{RATE_START_DATE1}", "2016-02-01T00:00:00"); // +1 month
			testData.Replace("{RATEENTRY_PK1}", string.Empty);
			testData.Replace("{RATEENTRY_PROVIDER_REFERENCE_ID1}", "QA_ENTRY_ID1");
			testData.Replace("{RATELINE_PK1}", string.Empty);
			testData.Replace("{RATELINE_PROVIDER_REFERENCE_ID1}", "QA_LINE_ID1");
			testData.Replace("{RATELINEITEM_PK1}", string.Empty);
			testData.Replace("{BREAK_MINIMUM1}", "11");
			// Entry 2
			testData.Replace("{RATEENTRY_PK2}", string.Empty);
			testData.Replace("{RATEENTRY_PROVIDER_REFERENCE_ID2}", "QA_ENTRY_ID2");
			testData.Replace("{RATELINE_PK2}", string.Empty);
			testData.Replace("{RATELINE_PROVIDER_REFERENCE_ID2}", "QA_LINE_ID2");
			testData.Replace("{RATELINEITEM_PK21}", string.Empty);
			testData.Replace("{BREAK_MINIMUM21}", "21");
			testData.Replace("{RATELINEITEM_PK22}", string.Empty);
			testData.Replace("{BREAK_MINIMUM22}", "22");

			xml = testData.GenerateXMLFromTemplate(template);
			Import(xml);

			var allRatingHeaders = factory.Load<RatingHeader>(new ZQuery());
			AssertContainsExactElementsInAnyOrder(
				"For insert action, change date range of the old entry and insert a new one. For merge action, update the entry with ProviderReferenceIds.",
				new[]
				{
					"EDI,QUETUB|01-Jan-16,31-Jan-16,111,AUSYD,|FRT,,DEF|0,10",                // old entry with a new date range
					"EDI,QUETUB|01-Feb-16,,111,AUSYD,QA_ENTRY_ID1|FRT,QA_LINE_ID1,DEF|11,10", // new entry
					"EDI,QUETUB|01-Jan-16,,222,AUSYD,QA_ENTRY_ID2|FRT,QA_LINE_ID2,DEF|21,10",
					"EDI,QUETUB|01-Jan-16,,222,AUSYD,QA_ENTRY_ID2|FRT,QA_LINE_ID2,DEF|22,10",
				},
				allRatingHeaders.SelectMany(ToStrings));
		}

		public void TestImportNativeXML_WithProviderReferenceId_ShouldUpdateCorrectRates()
		{
			// ========================================================
			// First, import new CW rates which have all PKs but no ProviderReferenceIDs

			var testData = RateTestData.NewWithValidTestDataForProviderReferenceIDTests();
			var (template, xml) = testData.GenerateXMLFromTemplateFile(ProviderRefIdTestFile);
			Import(xml);

			// ========================================================
			// Second, import new QA rates which do not have PKs but have ProviderReferenceIDs, same RatingHeader

			testData.Replace("{RATINGHEADER_PK}", string.Empty);
			// Entry 1
			testData.Replace("{RATEENTRY_PK1}", string.Empty);
			testData.Replace("{RATEENTRY_PROVIDER_REFERENCE_ID1}", "QA_ENTRY_ID1");
			testData.Replace("{ORIGIN1}", "SGSIN");  // avoid duplicate rate entries because ProviderReferenceID is not a rate differentiator
			testData.Replace("{RATELINE_PK1}", string.Empty);
			testData.Replace("{RATELINE_PROVIDER_REFERENCE_ID1}", "QA_LINE_ID1");
			testData.Replace("{RATELINEITEM_PK1}", string.Empty);
			testData.Replace("{BREAK_MINIMUM1}", "11");
			// Entry 2
			testData.Replace("{RATEENTRY_PK2}", string.Empty);
			testData.Replace("{RATEENTRY_PROVIDER_REFERENCE_ID2}", "QA_ENTRY_ID2");
			testData.Replace("{ORIGIN2}", "HKHKG");  // avoid duplicate rate entries because ProviderReferenceID is not a rate differentiator
			testData.Replace("{RATELINE_PK2}", string.Empty);
			testData.Replace("{RATELINE_PROVIDER_REFERENCE_ID2}", "QA_LINE_ID2");
			testData.Replace("{RATELINEITEM_PK21}", string.Empty);
			testData.Replace("{BREAK_MINIMUM21}", "21");
			testData.Replace("{RATELINEITEM_PK22}", string.Empty);
			testData.Replace("{BREAK_MINIMUM22}", "22");

			xml = testData.GenerateXMLFromTemplate(template);
			Import(xml);

			var allRatingHeaders = factory.Load<RatingHeader>(new ZQuery());
			AssertContainsExactElementsInAnyOrder(
				"There should be CW and QA rates",
				new[]
				{
					"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
					"EDI,QUETUB|01-Jan-16,,222,AUSYD,|FRT,,DEF|0,10",
					"EDI,QUETUB|01-Jan-16,,222,AUSYD,|FRT,,DEF|0,10",

					"EDI,QUETUB|01-Jan-16,,111,SGSIN,QA_ENTRY_ID1|FRT,QA_LINE_ID1,DEF|11,10",
					"EDI,QUETUB|01-Jan-16,,222,HKHKG,QA_ENTRY_ID2|FRT,QA_LINE_ID2,DEF|21,10",
					"EDI,QUETUB|01-Jan-16,,222,HKHKG,QA_ENTRY_ID2|FRT,QA_LINE_ID2,DEF|22,10",
				},
				allRatingHeaders.SelectMany(ToStrings));

			// ========================================================
			// #3, import the same QA rates again, but with different values on RateEntries, RateLines, and RateLineItems

			testData.Replace("{ORIGIN1}", "DEFRA");
			testData.Replace("{ORIGIN2}", "AUBNE");
			testData.Replace("{ROUNDING1}", "NOR");
			testData.Replace("{ROUNDING2}", "CUS");
			testData.Replace("{BREAK_MINIMUM1}", "15");
			testData.Replace("{BREAK_MINIMUM21}", "25");
			testData.Replace("{BREAK_MINIMUM22}", "26");

			xml = testData.GenerateXMLFromTemplate(template);
			Import(xml);

			var newFactory = factory.CreateNewFactory();

			allRatingHeaders = newFactory.Load<RatingHeader>(new ZQuery());
			AssertContainsExactElementsInAnyOrder(
				"Only QA RateEntries, RateLines, and RateLineItems should be updated",
				new[]
				{
					"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
					"EDI,QUETUB|01-Jan-16,,222,AUSYD,|FRT,,DEF|0,10",
					"EDI,QUETUB|01-Jan-16,,222,AUSYD,|FRT,,DEF|0,10",

					"EDI,QUETUB|01-Jan-16,,111,DEFRA,QA_ENTRY_ID1|FRT,QA_LINE_ID1,NOR|15,10",
					"EDI,QUETUB|01-Jan-16,,222,AUBNE,QA_ENTRY_ID2|FRT,QA_LINE_ID2,CUS|25,10",
					"EDI,QUETUB|01-Jan-16,,222,AUBNE,QA_ENTRY_ID2|FRT,QA_LINE_ID2,CUS|26,10",
				},
				allRatingHeaders.SelectMany(ToStrings));

			// ========================================================
			// #4, change RateLineItem TM_Values only and expect them to be updated (by removing all items and force to insert new ones)

			testData.Replace("{RATELINEITEM_VALUE}", "20");
			xml = testData.GenerateXMLFromTemplate(template);
			Import(xml);

			newFactory = factory.CreateNewFactory();
			allRatingHeaders = newFactory.Load<RatingHeader>(new ZQuery());
			AssertContainsExactElementsInAnyOrder(
				"QA RateLineItems should be updated",
				new[]
				{
					"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
					"EDI,QUETUB|01-Jan-16,,222,AUSYD,|FRT,,DEF|0,10",
					"EDI,QUETUB|01-Jan-16,,222,AUSYD,|FRT,,DEF|0,10",

					"EDI,QUETUB|01-Jan-16,,111,DEFRA,QA_ENTRY_ID1|FRT,QA_LINE_ID1,NOR|15,20",
					"EDI,QUETUB|01-Jan-16,,222,AUBNE,QA_ENTRY_ID2|FRT,QA_LINE_ID2,CUS|25,20",
					"EDI,QUETUB|01-Jan-16,,222,AUBNE,QA_ENTRY_ID2|FRT,QA_LINE_ID2,CUS|26,20",
				},
				allRatingHeaders.SelectMany(ToStrings));
		}

		public void TestImportNativeXML_ProviderReferenceId_WhenActionsAreDelete()
		{
			// Import new CW rates and QA rates
			// Same first 2 steps in the above test, don't need to assert.
			var testData = RateTestData.NewWithValidTestDataForProviderReferenceIDTests();
			var (template, xml) = testData.GenerateXMLFromTemplateFile(ProviderRefIdTestFile);
			Import(xml);

			testData.Replace("{RATINGHEADER_PK}", string.Empty);
			testData.Replace("{RATEENTRY_PK1}", string.Empty);
			testData.Replace("{RATEENTRY_PROVIDER_REFERENCE_ID1}", "QA_ENTRY_ID1");
			testData.Replace("{ORIGIN1}", "SGSIN");
			testData.Replace("{RATELINE_PK1}", string.Empty);
			testData.Replace("{RATELINE_PROVIDER_REFERENCE_ID1}", "QA_LINE_ID1");
			testData.Replace("{RATELINEITEM_PK1}", string.Empty);
			testData.Replace("{BREAK_MINIMUM1}", "11");
			testData.Replace("{RATEENTRY_PK2}", string.Empty);
			testData.Replace("{RATEENTRY_PROVIDER_REFERENCE_ID2}", "QA_ENTRY_ID2");
			testData.Replace("{ORIGIN2}", "HKHKG");
			testData.Replace("{RATELINE_PK2}", string.Empty);
			testData.Replace("{RATELINE_PROVIDER_REFERENCE_ID2}", "QA_LINE_ID2");
			testData.Replace("{RATELINEITEM_PK21}", string.Empty);
			testData.Replace("{BREAK_MINIMUM21}", "21");
			testData.Replace("{RATELINEITEM_PK22}", string.Empty);
			testData.Replace("{BREAK_MINIMUM22}", "22");

			xml = testData.GenerateXMLFromTemplate(template);
			Import(xml);

			// DELETE action on a RateLineItem - Only the RateLineItem should be deleted
			testData.Replace("{RATELINEITEM_ACTION1}", "DELETE");

			xml = testData.GenerateXMLFromTemplate(template);
			Import(xml);

			var newFactory = factory.CreateNewFactory();

			var allRatingHeaders = newFactory.Load<RatingHeader>(new ZQuery());
			AssertContainsExactElementsInAnyOrder(
				"QA RateLineItem1 should be deleted",
				new[]
				{
					"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
					"EDI,QUETUB|01-Jan-16,,222,AUSYD,|FRT,,DEF|0,10",
					"EDI,QUETUB|01-Jan-16,,222,AUSYD,|FRT,,DEF|0,10",

					"EDI,QUETUB|01-Jan-16,,111,SGSIN,QA_ENTRY_ID1|FRT,QA_LINE_ID1,DEF|No RateLineItem",
					"EDI,QUETUB|01-Jan-16,,222,HKHKG,QA_ENTRY_ID2|FRT,QA_LINE_ID2,DEF|21,10",
					"EDI,QUETUB|01-Jan-16,,222,HKHKG,QA_ENTRY_ID2|FRT,QA_LINE_ID2,DEF|22,10",
				},
				allRatingHeaders.SelectMany(ToStrings));

			testData.Replace("{RATELINEITEM_ACTION1}", "MERGE");
			// DELETE actions on a RateLine and its child RateLineItem - the RateLine should be deleted, hence its 2 RateLineItems
			testData.Replace("{RATELINE_ACTION2}", "DELETE");
			testData.Replace("{RATELINEITEM_ACTION22}", "DELETE");  // The other is still MERGE

			xml = testData.GenerateXMLFromTemplate(template);
			Import(xml);

			newFactory = factory.CreateNewFactory();

			allRatingHeaders = newFactory.Load<RatingHeader>(new ZQuery());
			AssertContainsExactElementsInAnyOrder(
				"QA RateLine2 should be deleted. So its RateLineItems",
				new[]
				{
					"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
					"EDI,QUETUB|01-Jan-16,,222,AUSYD,|FRT,,DEF|0,10",
					"EDI,QUETUB|01-Jan-16,,222,AUSYD,|FRT,,DEF|0,10",

					"EDI,QUETUB|01-Jan-16,,111,SGSIN,QA_ENTRY_ID1|FRT,QA_LINE_ID1,DEF|11,10",
					"EDI,QUETUB|01-Jan-16,,222,HKHKG,QA_ENTRY_ID2|No RateLine|No RateLineItem",
				},
				allRatingHeaders.SelectMany(ToStrings));

			testData.Replace("{RATELINE_ACTION2}", "MERGE");
			testData.Replace("{RATELINEITEM_ACTION22}", "MERGE");
			// DELETE actions on a RateEntry, its child RateLine, and child RateLineItem
			testData.Replace("{RATEENTRY_ACTION1}", "DELETE");
			testData.Replace("{RATELINE_ACTION1}", "DELETE");
			testData.Replace("{RATELINEITEM_ACTION1}", "DELETE");

			xml = testData.GenerateXMLFromTemplate(template);
			Import(xml);

			newFactory = factory.CreateNewFactory();
			allRatingHeaders = newFactory.Load<RatingHeader>(new ZQuery());
			AssertContainsExactElementsInAnyOrder(
				"",
				new[]
				{
					"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
					"EDI,QUETUB|01-Jan-16,,222,AUSYD,|FRT,,DEF|0,10",
					"EDI,QUETUB|01-Jan-16,,222,AUSYD,|FRT,,DEF|0,10",

					"EDI,QUETUB|01-Jan-16,,222,HKHKG,QA_ENTRY_ID2|FRT,QA_LINE_ID2,DEF|21,10",
					"EDI,QUETUB|01-Jan-16,,222,HKHKG,QA_ENTRY_ID2|FRT,QA_LINE_ID2,DEF|22,10",
				},
				allRatingHeaders.SelectMany(ToStrings));

			testData.Replace("{RATEENTRY_ACTION1}", "MERGE");
			testData.Replace("{RATELINE_ACTION1}", "MERGE");
			testData.Replace("{RATELINEITEM_ACTION1}", "MERGE");
			// DELETE action on a RateEntry
			testData.Replace("{RATEENTRY_ACTION2}", "DELETE");

			xml = testData.GenerateXMLFromTemplate(template);
			Import(xml);

			newFactory = factory.CreateNewFactory();
			allRatingHeaders = newFactory.Load<RatingHeader>(new ZQuery());
			AssertContainsExactElementsInAnyOrder(
				"There should be rates from the first company",
				new[]
				{
					"EDI,QUETUB|01-Jan-16,,111,AUSYD,|FRT,,DEF|0,10",
					"EDI,QUETUB|01-Jan-16,,222,AUSYD,|FRT,,DEF|0,10",
					"EDI,QUETUB|01-Jan-16,,222,AUSYD,|FRT,,DEF|0,10",

					"EDI,QUETUB|01-Jan-16,,111,SGSIN,QA_ENTRY_ID1|FRT,QA_LINE_ID1,DEF|11,10",
				},
				allRatingHeaders.SelectMany(ToStrings));
		}

		public void TestImportNativeXML_ProviderReferenceIdsCanBeTheSameAcrossDifferentParents()
		{
			// ========================================================
			// Import QA rates into default login company (EDI)

			var testData = RateTestData.NewWithValidTestDataForProviderReferenceIDTests();
			testData.Replace("{RATINGHEADER_PK}", string.Empty);
			// Entry 1
			testData.Replace("{RATEENTRY_PK1}", string.Empty);
			testData.Replace("{RATEENTRY_PROVIDER_REFERENCE_ID1}", "QA_ENTRY_ID1");
			testData.Replace("{RATELINE_PK1}", string.Empty);
			testData.Replace("{RATELINE_PROVIDER_REFERENCE_ID1}", "QA_LINE_ID");  // Same RateLine ProviderReferenceID, different RateEntry
			testData.Replace("{RATELINEITEM_PK1}", string.Empty);
			// Entry 2
			testData.Replace("{RATEENTRY_PK2}", string.Empty);
			testData.Replace("{RATEENTRY_PROVIDER_REFERENCE_ID2}", "QA_ENTRY_ID2");
			testData.Replace("{RATELINE_PK2}", string.Empty);
			testData.Replace("{RATELINE_PROVIDER_REFERENCE_ID2}", "QA_LINE_ID");  // Same RateLine ProviderReferenceID, different RateEntry
			testData.Replace("{RATELINEITEM_PK21}", string.Empty);
			testData.Replace("{RATELINEITEM_PK22}", string.Empty);

			var (template, xml) = testData.GenerateXMLFromTemplateFile(ProviderRefIdTestFile);
			Import(xml);

			var allRatingHeaders = factory.Load<RatingHeader>(new ZQuery());
			AssertContainsExactElementsInAnyOrder(
				"There should be rates from the first company",
				new[]
				{
					"EDI,QUETUB|01-Jan-16,,111,AUSYD,QA_ENTRY_ID1|FRT,QA_LINE_ID,DEF|0,10",
					"EDI,QUETUB|01-Jan-16,,222,AUSYD,QA_ENTRY_ID2|FRT,QA_LINE_ID,DEF|0,10",
					"EDI,QUETUB|01-Jan-16,,222,AUSYD,QA_ENTRY_ID2|FRT,QA_LINE_ID,DEF|0,10",
				},
				allRatingHeaders.SelectMany(ToStrings));

			// ========================================================
			// Import the same QA rates into another company with the same RateEntry ProviderReferenceIDs, in new RatingHeader.

			using var temporaryUserContext = Env.SetTemporaryUserContext(Env.CurrentUserPK, demoBranchPK, Env.CurrentDepartmentPK);

			testData.Replace("{GLBCOMPANY_CODE}", "DEM");
			xml = testData.GenerateXMLFromTemplate(template);
			Import(xml);

			// New factory to avoid caching
			var newFactory = factory.CreateNewFactory();

			allRatingHeaders = newFactory.Load<RatingHeader>(new ZQuery());
			AssertContainsExactElementsInAnyOrder(
				"There should be rates, that have the same ProviderReferenceIDs, from the 2 companies",
				new[]
				{
					"EDI,QUETUB|01-Jan-16,,111,AUSYD,QA_ENTRY_ID1|FRT,QA_LINE_ID,DEF|0,10",
					"EDI,QUETUB|01-Jan-16,,222,AUSYD,QA_ENTRY_ID2|FRT,QA_LINE_ID,DEF|0,10",
					"EDI,QUETUB|01-Jan-16,,222,AUSYD,QA_ENTRY_ID2|FRT,QA_LINE_ID,DEF|0,10",

					"DEM,QUETUB|01-Jan-16,,111,AUSYD,QA_ENTRY_ID1|FRT,QA_LINE_ID,DEF|0,10",
					"DEM,QUETUB|01-Jan-16,,222,AUSYD,QA_ENTRY_ID2|FRT,QA_LINE_ID,DEF|0,10",
					"DEM,QUETUB|01-Jan-16,,222,AUSYD,QA_ENTRY_ID2|FRT,QA_LINE_ID,DEF|0,10",
				},
				allRatingHeaders.SelectMany(ToStrings));

			// ========================================================
			// Update rates in the new company

			testData.Replace("{ROUNDING1}", "NOR");
			testData.Replace("{ROUNDING2}", "CUS");
			testData.Replace("{RATELINEITEM_VALUE}", "20");
			testData.Replace("{BREAK_MINIMUM1}", "15");
			testData.Replace("{BREAK_MINIMUM21}", "25");
			testData.Replace("{BREAK_MINIMUM22}", "26");

			xml = testData.GenerateXMLFromTemplate(template);
			Import(xml);

			newFactory = factory.CreateNewFactory();
			allRatingHeaders = newFactory.Load<RatingHeader>(new ZQuery());
			AssertContainsExactElementsInAnyOrder(
				"There should be rates, that have the same ProviderReferenceIDs, from the 2 companies",
				new[]
				{
					"EDI,QUETUB|01-Jan-16,,111,AUSYD,QA_ENTRY_ID1|FRT,QA_LINE_ID,DEF|0,10",
					"EDI,QUETUB|01-Jan-16,,222,AUSYD,QA_ENTRY_ID2|FRT,QA_LINE_ID,DEF|0,10",
					"EDI,QUETUB|01-Jan-16,,222,AUSYD,QA_ENTRY_ID2|FRT,QA_LINE_ID,DEF|0,10",

					"DEM,QUETUB|01-Jan-16,,111,AUSYD,QA_ENTRY_ID1|FRT,QA_LINE_ID,NOR|15,20",
					"DEM,QUETUB|01-Jan-16,,222,AUSYD,QA_ENTRY_ID2|FRT,QA_LINE_ID,CUS|25,20",
					"DEM,QUETUB|01-Jan-16,,222,AUSYD,QA_ENTRY_ID2|FRT,QA_LINE_ID,CUS|26,20",
				},
				allRatingHeaders.SelectMany(ToStrings));
		}

		static string ToPkTag(string pk) => $"<PK>{pk}</PK>";
		static string ToPkTag(ZGuid pk) => ToPkTag(pk.ToString());

		/// <summary>
		/// Each string represents a RateLineItem and its parent RateLine, RateEntry, and RatingHeader.
		/// </summary>
		static IEnumerable<string> ToStrings(RatingHeader ratingHeader)
		{
			var rateEntries = ratingHeader.AllEntriesCollection.OfType<RateEntry>().ToArray();
			foreach (var rateEntry in rateEntries)
			{
				var rateLines = rateEntry.RateLines.OfType<RateLine>().ToArray();
				if (rateLines.Length == 0)
				{
					yield return ToString(ratingHeader, rateEntry);
					continue;
				}

				foreach (var rateLine in rateLines)
				{
					var rateLineItems = rateLine.RateLineItems.OfType<RateLineItem>().ToArray();
					if (rateLineItems.Length == 0)
					{
						yield return ToString(ratingHeader, rateEntry, rateLine);
						continue;
					}

					foreach (var rateLineItem in rateLineItems)
					{
						yield return ToString(ratingHeader, rateEntry, rateLine, rateLineItem);
					}
				}
			}
		}

		static string ToString(RatingHeader ratingHeader, RateEntry rateEntry, RateLine rateLine = null, RateLineItem rateLineItem = null)
		{
			var result = new StringBuilder();

			result
				.Append(ratingHeader.Company.GC_Code).Append(",").Append(ratingHeader.Header.OH_Code).Append("|")
				.Append(rateEntry.TI_RateStartDate).Append(",").Append(rateEntry.TI_RateEndDate).Append(",")
				.Append(rateEntry.TI_ContractNumber).Append(",").Append(rateEntry.TI_OriginLRC).Append(",").Append(rateEntry.TI_ProviderReferenceID).Append("|");

			if (rateLine == null)
			{
				result.Append("No RateLine|");
			}
			else
			{
				result.Append(rateLine.ChargeCode.AC_Code).Append(",").Append(rateLine.TL_ProviderReferenceID).Append(",").Append(rateLine.TL_Rounding).Append("|");
			}

			if (rateLineItem == null)
			{
				result.Append("No RateLineItem");
			}
			else
			{
				result.Append(rateLineItem.TM_BreakMinimum.ToString(0)).Append(",").Append(rateLineItem.TM_Value.ToString(0));
			}

			return result.ToString();
		}

		/// <summary>
		/// BIG NOTE: This test has a good side effect for testing a new column added to RateEntry.
		/// When it fails, please check that:
		/// <ul>
		/// <li>GetRateEntryChecksum.sql: Add the new column to the checksum calculation IN THE SAME ORDER in the RateEntry table definition.</li>
		/// <li>GetOverlappingRateEntries.sql is updated to include the new column.</li>
		/// <li>TG_CheckNoRateEntryOverlaps.sql is updated to include the new column.</li>
		/// </ul>
		/// </summary>
		public void TestServiceUpdateRateWithNativeXML_WhenRatesOverlap_ShouldHandleSqlException()
		{
			var costing = factory.New<Costing>();
			var entry1 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX");
			var today = ZDate.Today;
			entry1.TI_RateStartDate = today;
			entry1.TI_RateEndDate = today.AddDays(10);

			var entry2 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX");
			entry2.TI_RateStartDate = today.AddDays(11);
			entry2.TI_RateEndDate = today.AddDays(20);

			factory.Save();

			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.ServiceRateUpdateMessageDuplicateRates.xml"));
			var template = data.ReadToEnd();

			// modify template to update entry1 to have the date range overlapping entry2's one.
			var xmlMessage = template
				.Replace("{ratingHeaderPK}", costing.PK.ToString())
				.Replace("{rateEntryPK}", entry1.PK.ToString())
				.Replace("{rateLinePK}", entry1.RateLines[0].PK.ToString())
				.Replace("{rateStartDate}", today.AddDays(12).ToString())
				.Replace("{rateEndDate}", today.AddDays(15).ToString());

			using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(xmlMessage)))
			{
				manager.Import(stream);
			}

			var actualLogs = GetLogsMessage();
			var expectedLogs = "Test error: Rates cannot be imported as duplicated rates existed.";
			AssertContains("Rating custom sql exceptions should be handled and show user friendly message", expectedLogs, actualLogs);
		}

		public void TestDeleteRateEntryByPKOnly()
		{
			var globalFRTChargeCode = factory.NewWithValidTestData<AccChargeCode>();
			globalFRTChargeCode.AC_GC = ZGuid.Empty;
			globalFRTChargeCode.AC_Code = "FRT";
			globalFRTChargeCode.AC_Desc = "Global FRT";
			globalFRTChargeCode.AC_ChargeType = "MRG";

			var globalClientRate = factory.New<ClientRate>();
			globalClientRate.TH_GC = ZGuid.Empty;
			globalClientRate.TH_OH = factory.NewWithValidTestData<OrgHeader>().PK;

			globalClientRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX")
				.AddRateLine(globalFRTChargeCode, UnitCalculator.Code, "KG", "AUD");

			var rateEntry1 = globalClientRate.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL");
			rateEntry1.AddRateLine(globalFRTChargeCode, UnitCalculator.Code, "KG", "AUD");

			globalClientRate.AddRateEntry("FCL", "SEA", "AUSYD", "HKHKG")
				.AddRateLine(globalFRTChargeCode, UnitCalculator.Code, "KG", "AUD");

			factory.Save();

			string ratingXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUSSYD</OwnerCode>
    <EnableCodeMapping>false</EnableCodeMapping>
  </Header>
  <Body>
    <Rate version=""2.0"">
      <RatingHeader Action=""MERGE"">
        <PK>{globalClientRate.PK.ToString()}</PK>
        <RateType>SAL</RateType>
        <RateEntryCollection>
          <RateEntry Action=""DELETE"">
            <PK>{rateEntry1.PK.ToString()}</PK>
          </RateEntry>
        </RateEntryCollection>
      </RatingHeader>
    </Rate>
  </Body>
</Native>";

			using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(ratingXml)))
			{
				manager.Import(stream);
			}

			var actualLogs = GetLogsMessage();
			var expectedLogs = @"RatingHeader - 0 inserts, 0 updates, 0 deletes
RateEntry - 0 inserts, 0 updates, 1 deletes";

			AssertMultilineASCIIEquals("Should delete one rate entry", expectedLogs, actualLogs);

			var rateEntries = new BusinessObjectFactory().Load<RateEntry>(new ZQuery());

			CombineAssertions(() =>
			{
				AssertEquals("rateEntry1 should be deleted", false, rateEntries.Any(x => x.PK == rateEntry1.PK));
				AssertEquals("Should be rate entries left", 2, rateEntries.Length);
			});
		}

		public void TestCostUpload_WithMultipleBatchMerges()
		{
			// Setup
			// The merge count should be minimal for the performance gain.
			const string expectedErrorMessages = @"===== INSERT =====
RatingHeader - 1 inserts, 0 updates, 0 deletes
RateEntry - 5 inserts, 0 updates, 0 deletes
RateLines - 15 inserts, 0 updates, 0 deletes
RateLineItems - 38 inserts, 0 updates, 0 deletes
StmNote - 5 inserts, 0 updates, 0 deletes
===== MERGE =====
RatingHeader - 0 inserts, 0 updates, 0 deletes
RateEntry - 0 inserts, 0 updates, 0 deletes
RateLines - 0 inserts, 0 updates, 0 deletes
RateLineItems - 0 inserts, 5 updates, 0 deletes
StmNote - 0 inserts, 0 updates, 0 deletes";

			var expectedBatchMergeCount1 = new ConcurrentDictionary<string, int>();
			expectedBatchMergeCount1.TryAdd("RateEntry", 1);
			expectedBatchMergeCount1.TryAdd("RateLines", 4);
			expectedBatchMergeCount1.TryAdd("RateLineItems", 4);
			expectedBatchMergeCount1.TryAdd("StmNote", 2);

			var expectedBatchMergeCount2 = new ConcurrentDictionary<string, int>();
			expectedBatchMergeCount2.TryAdd("RateEntry", 1);
			expectedBatchMergeCount2.TryAdd("RateLines", 6);
			expectedBatchMergeCount2.TryAdd("RateLineItems", 6);
			expectedBatchMergeCount2.TryAdd("StmNote", 3);
			//using (StreamReader reader = new StreamReader(data))
			//string result = data.ReadToEnd();

			//Execute
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.UpdateOperation_ProcessChildrenInBatches.xml"));
			dummyLogger.Log(LogType.Information, "===== INSERT =====");
			manager.Import(data.BaseStream);

			var actualBatchMergeCount1 = manager.BatchMergeEntityCountForTest;

			data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.UpdateOperation_ProcessChildrenInBatches.xml"));
			dummyLogger.Log(LogType.Information, "===== MERGE =====");
			manager.Import(data.BaseStream);

			var actualBatchMergeCount2 = manager.BatchMergeEntityCountForTest;
			var actualErrorMessages = GetLogsMessage();
			var actualErrorLogs = dummyLogger.Buffer.Logs().Where(log => log.Type == LogType.Error).ToArray();

			// Assert
			AssertContainsExactElementsInAnyOrder(expectedBatchMergeCount1, actualBatchMergeCount1);
			AssertContainsExactElementsInAnyOrder(expectedBatchMergeCount2, actualBatchMergeCount2);
			AssertEquals(0, actualErrorLogs.Length);
			AssertMultilineASCIIEquals("Should insert three rate entries", expectedErrorMessages, actualErrorMessages);
		}

		public void TestCostUpload_ShouldMaintainOrderInEachParentPk()
		{
			//Execute
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.UpdateOperation_WithCorrectOrder.xml"));
			manager.Import(data.BaseStream);

			var query = new ZQuery();
			query.OrderBy = RateLineItemsSchema.Constants.TM_Value;
			var allItems = factory.Load<RateLineItem>(query);

			// See comments in XML test file...
			// After first entry insert should have:
			// - BAF line with RateLineItem applying to FRT
			// - CAF line with RateLineItem applying to WAR
			// After second entry is merged should have:
			// - BAF line with RateLineItem applying to FRT with Value 1
			// - CAF line with RateLineItem applying to WAR with value 0
			// - CAF line with RateLineItem applying to FSC with value 2
			AssertEquals(3, allItems.Length);
			AssertEquals(0m, allItems[0].TM_Value);
			AssertEquals(1m, allItems[1].TM_Value);
			AssertEquals(2m, allItems[2].TM_Value);
			AssertEquals("WAR", allItems[0].ChargeCode.AC_Code);
			AssertEquals("FRT", allItems[1].ChargeCode.AC_Code);
			AssertEquals("FSC", allItems[2].ChargeCode.AC_Code);

			var actualLogs = GetLogsMessage();
			var expectedLogs = @"RatingHeader - 1 inserts, 0 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 2 inserts, 0 updates, 0 deletes
RateLineItems - 3 inserts, 0 updates, 0 deletes";

			AssertMultilineASCIIEquals("Should insert 3 RateLineItems since all merges are done in memory", expectedLogs, actualLogs);

			var actualBatchMergeCount = manager.BatchMergeEntityCountForTest;
			AssertEquals("rate line items merged in batches for each signature", 2, actualBatchMergeCount[RateLineItemsSchema.Constants.TableName]);
		}

		public void TestInvoke_LinesHaveNoCurrency_FailImport()
		{
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.RateWithoutCurrency.xml"));
			manager.Import(data.BaseStream);

			var actualLogs = GetLogsMessage();
			var ratingHeaders = factory.Load<RatingHeader>(new ZQuery());

			CombineAssertions(() =>
			{
				AssertContains("There is a validation message", "Test error: The rate has no currency", actualLogs);
				AssertEquals("Should NOT have created the Costing", 0, ratingHeaders.Length);
			});
		}

		public void TestImportCarrierServiceXMLWith_UseTVPTrue()
		{
			EnvProxy.Instance.Registry.RawRegistry.TVPRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1");
			var carrier1 = factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_Code = "CHISHI_WW";
			carrier1.OH_IsShippingLine = true;
			var carrierServiceLevel1 = carrier1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel1.PL_Code = "URG";
			carrierServiceLevel1.PL_CarrierServiceLevelDescription = "Urgent";

			var carrier2 = factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_Code = "SEASTA_WW";
			carrier2.OH_IsShippingLine = true;
			var carrierServiceLevel2 = carrier2.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel2.PL_Code = "URG";
			carrierServiceLevel2.PL_CarrierServiceLevelDescription = "Urgent";
			var carrierServiceLevel3 = carrier2.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel3.PL_Code = "BUL";
			carrierServiceLevel3.PL_CarrierServiceLevelDescription = "Bulk";
			factory.Save();

			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.ClientRateWithCarrierServiceLevels.xml"));
			manager.Import(data.BaseStream);

			var actualLogs = GetLogsMessage();
			var expectedLogs = @"RatingHeader - 1 inserts, 0 updates, 0 deletes
RateEntry - 3 inserts, 0 updates, 0 deletes
RateLines - 3 inserts, 0 updates, 0 deletes
RateLineItems - 3 inserts, 0 updates, 0 deletes";

			AssertMultilineASCIIEquals("Should insert three rate entries", expectedLogs, actualLogs);
		}

		public void TestRateLineItemsHasInvalidMoneyValue_FailsToImport()
		{
			var data = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.Business.Update.Rates.TestFiles.InvalidMoneyValueInRateLineItems.xml"));
			manager.Import(data.BaseStream);

			var actualLogs = GetLogsMessage();
			var expectedLogs = "Test error: [RatingHeader.RateEntry.RateLines.RateLineItems.Value] : Value was too large for a money type. Couldn't store <762,5699999999999> in money column. Limit is 15 digits before the decimal point but 16 were provided.";
			var ratingHeaders = factory.Load<RatingHeader>(new ZQuery());

			CombineAssertions(() =>
			{
				AssertContains("There is a validation message", expectedLogs, actualLogs);
				AssertEquals("Should NOT have created the Costing", 0, ratingHeaders.Length);
			});
		}

		string GetLogsMessage()
		{
			return string.Join(System.Environment.NewLine, dummyLogger.Buffer.Logs().Select(log => log.Message));
		}

		void ErrorOccur(XElement source, Exception ex)
		{
			dummyLogger.Error("Test error: " + ex.Message);
		}

		protected override void SetUp()
		{
			base.SetUp();

			sessionServices = new AncillaryImportServices();
			dummyLogger = sessionServices.Logger as MemoryLogger;
			manager = new ImportHandler(sessionServices)
			{
				ErrorOccur = ErrorOccur
			};
			factory = new BusinessObjectFactory();

			ediCompanyPK = factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "EDI")).PK.ToGuid();
			demoCompanyPK = factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM")).PK.ToGuid();
			var query = new ZQuery(GlbBranchSchema.GB_GC, demoCompanyPK);
			query.AddToFilter(GlbBranchSchema.GB_Code, "DEM");
			demoBranchPK = factory.LoadTop1<GlbBranch>(query).PK.ToGuid();
		}

		AncillaryImportServices sessionServices;
		MemoryLogger dummyLogger;
		ImportHandler manager;
		BusinessObjectFactory factory;
		Guid ediCompanyPK;
		Guid demoCompanyPK;
		Guid demoBranchPK;
	}
}
