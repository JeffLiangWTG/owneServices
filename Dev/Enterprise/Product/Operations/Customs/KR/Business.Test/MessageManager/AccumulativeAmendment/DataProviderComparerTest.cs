using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;
using static Enterprise.Customs.KR.Messaging.DataItemIDAttribute;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class DataProviderComparerTest : TestCaseWithFactory
	{
		public void TestCompareZTypeMembersWhenEqual()
		{
			var dummyHeader1 = new DummyEntryHeader() { EntryNumber = "1", LoadingDate = new DateTime(2020, 1, 1), IsPersonalItem = true, PackageCount = 2, TotalValue = 20.250m };
			var dummyHeader2 = new DummyEntryHeader() { EntryNumber = "1", LoadingDate = new DateTime(2020, 1, 1), IsPersonalItem = true, PackageCount = 2, TotalValue = 20.250m };

			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(ElectronicDocumentTypeList.Codes._830, dummyHeader1, dummyHeader2, Enumerable.Empty<string>());
			Assert("Two data providers are equal", !result.Any());
		}

		public void TestCompareZTypeMembersWhenDifferent()
		{
			var dummyHeader1 = new DummyEntryHeader() { EntryNumber = "1a", LoadingDate = new ZDateTime(2020, 1, 1), PackageCount = 1, IsPersonalItem = true };
			var dummyHeader2 = new DummyEntryHeader() { EntryNumber = "1A", LoadingDate = new ZDateTime(2020, 3, 3), PackageCount = 2, TotalValue = 20.250m };

			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(ElectronicDocumentTypeList.Codes._830, dummyHeader1, dummyHeader2, Enumerable.Empty<string>()).ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Two data providers are different", 5, result.Count);

				AssertEquals(EntityAmendType.Update, result[0].AmendType);
				AssertEquals("string comparison", "X101", result[0].DataItemID);
				AssertEquals("string comparison", ChangeType.Normal, result[0].ChangeType);
				AssertEquals("string comparison", "1a", result[0].BeforeValue);
				AssertEquals("string comparison", "1A", result[0].AfterValue);

				AssertEquals(EntityAmendType.Update, result[1].AmendType);
				AssertEquals("date comparison", "X102", result[1].DataItemID);
				AssertEquals("date comparison", ChangeType.DutyTaxRelated, result[1].ChangeType);
				AssertEquals("date comparison", "01-Jan-20 00:00", result[1].BeforeValue);
				AssertEquals("date comparison", "03-Mar-20 00:00", result[1].AfterValue);

				AssertEquals(EntityAmendType.Update, result[2].AmendType);
				AssertEquals("int comparison", "X103", result[2].DataItemID);
				AssertEquals("int comparison", ChangeType.DutyTaxRelatedAndNormal, result[2].ChangeType);
				AssertEquals("int comparison", "1", result[2].BeforeValue);
				AssertEquals("int comparison", "2", result[2].AfterValue);

				AssertEquals(EntityAmendType.Update, result[3].AmendType);
				AssertEquals("decimal comparison", "X104", result[3].DataItemID);
				AssertEquals("decimal comparison", ChangeType.Normal, result[3].ChangeType);
				AssertEquals("decimal comparison", "0", result[3].BeforeValue);
				AssertEquals("decimal comparison", "20.250", result[3].AfterValue);

				AssertEquals(EntityAmendType.Update, result[4].AmendType);
				AssertEquals("bool comparison", "X105", result[4].DataItemID);
				AssertEquals("bool comparison", ChangeType.Normal, result[4].ChangeType);
				AssertEquals("bool comparison", "Y", result[4].BeforeValue);
				AssertEquals("bool comparison", "N", result[4].AfterValue);
			});
		}

		public void TestCompareOrganisationsWhenEquals()
		{
			var exporter1 = new DummyOrganization() { Role = RoleType.Exporter, CompanyName = "AAA" };
			var importer1 = new DummyOrganization() { Role = RoleType.Importer, CompanyName = "CCC" };
			var manufacturer1 = new DummyOrganization() { Role = RoleType.Manufacturer, CompanyName = "BBB", AddressLine1 = "1 Queen St" };
			var supplier1 = new DummyOrganization() { Role = RoleType.Supplier, CompanyName = "CCC", AddressLine1 = "1 Alpha Rd", AddressLine2 = "2 Alpha Rd", RepresentativeName = "Sill", Postcode = "22222" };
			var dummyHeader1 = new DummyEntryHeader() { Manufacturer = manufacturer1, Supplier = supplier1, Exporter = exporter1, Importer = importer1 };

			var exporter2 = new DummyOrganization() { Role = RoleType.Exporter, CompanyName = "AAA" };
			var importer2 = new DummyOrganization() { Role = RoleType.Importer, CompanyName = "CCC" };
			var manufacturer2 = new DummyOrganization() { Role = RoleType.Manufacturer, CompanyName = "BBB", AddressLine1 = "1 Queen St" };
			var supplier2 = new DummyOrganization() { Role = RoleType.Supplier, CompanyName = "CCC", AddressLine1 = "1 Alpha Rd", AddressLine2 = "2 Alpha Rd", RepresentativeName = "Sill", Postcode = "22222" };
			var dummyHeader2 = new DummyEntryHeader() { Manufacturer = manufacturer2, Supplier = supplier2, Exporter = exporter2, Importer = importer2 };

			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(ElectronicDocumentTypeList.Codes._830, dummyHeader1, dummyHeader2, Enumerable.Empty<string>());
			Assert("Two data providers are equal", !result.Any());
		}

		public void TestCompareOrganisationsWhenDifferent()
		{
			var manufacturer1 = new DummyOrganization() { Role = RoleType.Manufacturer, CompanyName = "BBB" };
			var dummyHeader1 = new DummyEntryHeader() { Manufacturer = manufacturer1 };

			var exporter = new DummyOrganization() { Role = RoleType.Exporter, CompanyName = "AAA" };
			var importer = new DummyOrganization() { Role = RoleType.Importer, CompanyName = "CCC" };
			var supplier = new DummyOrganization() { Role = RoleType.Supplier, CompanyName = "DDD", AddressLine1 = "1 Alpha Rd", AddressLine2 = "2 Alpha Rd", RepresentativeName = "Sill", Postcode = "22222" };
			var manufacturer2 = new DummyOrganization() { Role = RoleType.Manufacturer, CompanyName = "bbb", Postcode = "11111" };
			var dummyHeader2 = new DummyEntryHeader() { Manufacturer = manufacturer2, Supplier = supplier, Exporter = exporter, Importer = importer };

			var exportResult = new DataProviderComparer<IDummyEntryHeader>().Compare(ElectronicDocumentTypeList.Codes._830, dummyHeader1, dummyHeader2, Enumerable.Empty<string>()).ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Two data providers are different", 9, exportResult.Count);

				AssertEquals(EntityAmendType.Add, exportResult[0].AmendType);
				AssertEquals("A201", exportResult[0].DataItemID);
				AssertEquals(ChangeType.Normal, exportResult[0].ChangeType);
				AssertEquals("", exportResult[0].BeforeValue);
				AssertEquals("AAA", exportResult[0].AfterValue);
				AssertEquals("IOrganization", exportResult[0].IDsInList.ElementAt(0).IDType);
				AssertEquals("Exporter", exportResult[0].IDsInList.ElementAt(0).IDValue);

				AssertEquals(EntityAmendType.Add, exportResult[1].AmendType);
				AssertEquals("A501", exportResult[1].DataItemID);
				AssertEquals(ChangeType.Normal, exportResult[1].ChangeType);
				AssertEquals("", exportResult[1].BeforeValue);
				AssertEquals("CCC", exportResult[1].AfterValue);
				AssertEquals("IOrganization", exportResult[1].IDsInList.ElementAt(0).IDType);
				AssertEquals("Importer", exportResult[1].IDsInList.ElementAt(0).IDValue);

				AssertEquals(EntityAmendType.Add, exportResult[2].AmendType);
				AssertEquals("A301", exportResult[2].DataItemID);
				AssertEquals(ChangeType.Normal, exportResult[2].ChangeType);
				AssertEquals("", exportResult[2].BeforeValue);
				AssertEquals("DDD", exportResult[2].AfterValue);
				AssertEquals("IOrganization", exportResult[2].IDsInList.ElementAt(0).IDType);
				AssertEquals("Supplier", exportResult[2].IDsInList.ElementAt(0).IDValue);

				AssertEquals(EntityAmendType.Add, exportResult[3].AmendType);
				AssertEquals("A308", exportResult[3].DataItemID);
				AssertEquals(ChangeType.Normal, exportResult[3].ChangeType);
				AssertEquals("", exportResult[3].BeforeValue);
				AssertEquals("Sill", exportResult[3].AfterValue);
				AssertEquals("IOrganization", exportResult[3].IDsInList.ElementAt(0).IDType);
				AssertEquals("Supplier", exportResult[3].IDsInList.ElementAt(0).IDValue);

				AssertEquals(EntityAmendType.Add, exportResult[4].AmendType);
				AssertEquals("A303", exportResult[4].DataItemID);
				AssertEquals(ChangeType.Normal, exportResult[4].ChangeType);
				AssertEquals("", exportResult[4].BeforeValue);
				AssertEquals("1 Alpha Rd", exportResult[4].AfterValue);
				AssertEquals("IOrganization", exportResult[4].IDsInList.ElementAt(0).IDType);
				AssertEquals("Supplier", exportResult[4].IDsInList.ElementAt(0).IDValue);

				AssertEquals(EntityAmendType.Add, exportResult[5].AmendType);
				AssertEquals("A304", exportResult[5].DataItemID);
				AssertEquals(ChangeType.Normal, exportResult[5].ChangeType);
				AssertEquals("", exportResult[5].BeforeValue);
				AssertEquals("2 Alpha Rd", exportResult[5].AfterValue);
				AssertEquals("IOrganization", exportResult[5].IDsInList.ElementAt(0).IDType);
				AssertEquals("Supplier", exportResult[5].IDsInList.ElementAt(0).IDValue);

				AssertEquals(EntityAmendType.Add, exportResult[6].AmendType);
				AssertEquals("A305", exportResult[6].DataItemID);
				AssertEquals(ChangeType.Normal, exportResult[6].ChangeType);
				AssertEquals("", exportResult[6].BeforeValue);
				AssertEquals("22222", exportResult[6].AfterValue);
				AssertEquals("IOrganization", exportResult[6].IDsInList.ElementAt(0).IDType);
				AssertEquals("Supplier", exportResult[6].IDsInList.ElementAt(0).IDValue);

				AssertEquals(EntityAmendType.Update, exportResult[7].AmendType);
				AssertEquals("A401", exportResult[7].DataItemID);
				AssertEquals(ChangeType.Normal, exportResult[7].ChangeType);
				AssertEquals("BBB", exportResult[7].BeforeValue);
				AssertEquals("bbb", exportResult[7].AfterValue);
				AssertEquals("IOrganization", exportResult[7].IDsInList.ElementAt(0).IDType);
				AssertEquals("Manufacturer", exportResult[7].IDsInList.ElementAt(0).IDValue);

				AssertEquals(EntityAmendType.Update, exportResult[8].AmendType);
				AssertEquals("A406", exportResult[8].DataItemID);
				AssertEquals(ChangeType.Normal, exportResult[8].ChangeType);
				AssertEquals("", exportResult[8].BeforeValue);
				AssertEquals("11111", exportResult[8].AfterValue);
				AssertEquals("IOrganization", exportResult[8].IDsInList.ElementAt(0).IDType);
				AssertEquals("Manufacturer", exportResult[8].IDsInList.ElementAt(0).IDValue);
			});

			var importResult = new DataProviderComparer<IDummyEntryHeader>().Compare(ElectronicDocumentTypeList.Codes._929, dummyHeader1, dummyHeader2, Enumerable.Empty<string>()).ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Two data providers are different", 2, importResult.Count);

				AssertEquals(EntityAmendType.Add, importResult[0].AmendType);
				AssertEquals("A201", importResult[0].DataItemID);
				AssertEquals(ChangeType.Normal, importResult[0].ChangeType);
				AssertEquals("", importResult[0].BeforeValue);
				AssertEquals("CCC", importResult[0].AfterValue);
				AssertEquals("IOrganization", importResult[0].IDsInList.ElementAt(0).IDType);
				AssertEquals("Importer", importResult[0].IDsInList.ElementAt(0).IDValue);

				AssertEquals(EntityAmendType.Add, importResult[1].AmendType);
				AssertEquals("A601", importResult[1].DataItemID);
				AssertEquals(ChangeType.Normal, importResult[1].ChangeType);
				AssertEquals("", importResult[1].BeforeValue);
				AssertEquals("DDD", importResult[1].AfterValue);
				AssertEquals("IOrganization", importResult[1].IDsInList.ElementAt(0).IDType);
				AssertEquals("Supplier", importResult[1].IDsInList.ElementAt(0).IDValue);
			});

			var localExportResult = new DataProviderComparer<IDummyEntryHeader>().Compare(ElectronicDocumentTypeList.Codes._5DP, dummyHeader1, dummyHeader2, Enumerable.Empty<string>()).ToList();
			AssertEquals("DateItemID for organization of LocalExport is not specified, so AmendItem is not added.", 0, localExportResult.Count);
		}

		public void TestCompareEntryLinesSubinesWhenEquals()
		{
			var dummySubLine1 = new DummySubLine() { SubLineNo = 1, Ingredient = "Ingredient", Amount = 112m };
			var dummyLine1 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "11111", CountryOfOrigin = "AU", SubLines = new DummySubLine[] { dummySubLine1 } };
			var dummyHeader1 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine1 } };

			var dummySubLine2 = new DummySubLine() { SubLineNo = 1, Ingredient = "Ingredient", Amount = 112m };
			var dummyLine2 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "11111", CountryOfOrigin = "AU", SubLines = new DummySubLine[] { dummySubLine2 } };
			var dummyHeader2 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine2 } };

			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(ElectronicDocumentTypeList.Codes._830, dummyHeader1, dummyHeader2, Enumerable.Empty<string>());
			Assert("Two data providers are equal", !result.Any());
		}

		public void TestCompareEntryLinesSubLinesWhenDifferent()
		{
			var dummySubLine1 = new DummySubLine() { SubLineNo = 3 };
			var dummySubLine2 = new DummySubLine() { SubLineNo = 4, Ingredient = "Woods", Amount = 112m };
			var dummySubLine3 = new DummySubLine() { SubLineNo = 5, Ingredient = "Plastics", Amount = 335m };
			var dummyLine1 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "11111", SubLines = new DummySubLine[] { dummySubLine1, dummySubLine2 } };
			var dummyLine2 = new DummyEntryLine() { EntryLineNo = 2, HSCode = "33333", SubLines = new DummySubLine[] { dummySubLine3 } };
			var dummyHeader1 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine1, dummyLine2 } };

			var dummySubLine4 = new DummySubLine() { SubLineNo = 3, Ingredient = "Iron", Amount = 61m };
			var dummySubLine5 = new DummySubLine() { SubLineNo = 4 };
			var dummySubLine6 = new DummySubLine() { SubLineNo = 5, Ingredient = "plastics" };
			var dummyLine3 = new DummyEntryLine() { EntryLineNo = 1, CountryOfOrigin = "AU", SubLines = new DummySubLine[] { dummySubLine4, dummySubLine5 } };
			var dummyLine4 = new DummyEntryLine() { EntryLineNo = 2, HSCode = "55555", SubLines = new DummySubLine[] { dummySubLine6 } };
			var dummyHeader2 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine3, dummyLine4 } };

			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(ElectronicDocumentTypeList.Codes._830, dummyHeader1, dummyHeader2, Enumerable.Empty<string>()).ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Two data providers are different", 6, result.Count);

				AssertEquals(EntityAmendType.Update, result[0].AmendType);
				AssertEquals("Z101", result[0].DataItemID);
				AssertEquals(ChangeType.Normal, result[0].ChangeType);
				AssertEquals("", result[0].BeforeValue);
				AssertEquals("AU", result[0].AfterValue);
				AssertEquals("IDs", 1, result[0].IDsInList.Count());
				AssertEquals("IDummyEntryLine", result[0].IDsInList.ElementAt(0).IDType);
				AssertEquals("1", result[0].IDsInList.ElementAt(0).IDValue);

				AssertEquals(EntityAmendType.Update, result[1].AmendType);
				AssertEquals("Z102", result[1].DataItemID);
				AssertEquals(ChangeType.Normal, result[1].ChangeType);
				AssertEquals("11111", result[1].BeforeValue);
				AssertEquals("", result[1].AfterValue);
				AssertEquals("IDs", 1, result[1].IDsInList.Count());
				AssertEquals("IDummyEntryLine", result[1].IDsInList.ElementAt(0).IDType);
				AssertEquals("1", result[1].IDsInList.ElementAt(0).IDValue);

				AssertEquals(EntityAmendType.Update, result[2].AmendType);
				AssertEquals("M101", result[2].DataItemID);
				AssertEquals(ChangeType.Normal, result[2].ChangeType);
				AssertEquals("", result[2].BeforeValue);
				AssertEquals("Iron", result[2].AfterValue);
				AssertEquals("IDs", 2, result[2].IDsInList.Count());
				AssertEquals("IDummyEntryLine", result[2].IDsInList.ElementAt(0).IDType);
				AssertEquals("1", result[2].IDsInList.ElementAt(0).IDValue);
				AssertEquals("IDummySubLine", result[2].IDsInList.ElementAt(1).IDType);
				AssertEquals("3", result[2].IDsInList.ElementAt(1).IDValue);

				AssertEquals(EntityAmendType.Update, result[3].AmendType);
				AssertEquals("M101", result[3].DataItemID);
				AssertEquals(ChangeType.Normal, result[3].ChangeType);
				AssertEquals("Woods", result[3].BeforeValue);
				AssertEquals("", result[3].AfterValue);
				AssertEquals("IDs", 2, result[3].IDsInList.Count());
				AssertEquals("IDummyEntryLine", result[3].IDsInList.ElementAt(0).IDType);
				AssertEquals("1", result[3].IDsInList.ElementAt(0).IDValue);
				AssertEquals("IDummySubLine", result[3].IDsInList.ElementAt(1).IDType);
				AssertEquals("4", result[3].IDsInList.ElementAt(1).IDValue);

				AssertEquals(EntityAmendType.Update, result[4].AmendType);
				AssertEquals("Z102", result[4].DataItemID);
				AssertEquals(ChangeType.Normal, result[4].ChangeType);
				AssertEquals("33333", result[4].BeforeValue);
				AssertEquals("55555", result[4].AfterValue);
				AssertEquals("IDs", 1, result[4].IDsInList.Count());
				AssertEquals("IDummyEntryLine", result[4].IDsInList.ElementAt(0).IDType);
				AssertEquals("2", result[4].IDsInList.ElementAt(0).IDValue);

				AssertEquals(EntityAmendType.Update, result[5].AmendType);
				AssertEquals("M101", result[5].DataItemID);
				AssertEquals(ChangeType.Normal, result[5].ChangeType);
				AssertEquals("Plastics", result[5].BeforeValue);
				AssertEquals("plastics", result[5].AfterValue);
				AssertEquals("IDs", 2, result[5].IDsInList.Count());
				AssertEquals("IDummyEntryLine", result[5].IDsInList.ElementAt(0).IDType);
				AssertEquals("2", result[5].IDsInList.ElementAt(0).IDValue);
				AssertEquals("IDummySubLine", result[5].IDsInList.ElementAt(1).IDType);
				AssertEquals("5", result[5].IDsInList.ElementAt(1).IDValue);
			});
		}

		public void TestCompareEntryLinesSubLinesWhenAdded()
		{
			var dummySubLine1 = new DummySubLine() { SubLineNo = 3 };
			var dummyLine1 = new DummyEntryLine() { EntryLineNo = 2, SubLines = new DummySubLine[] { dummySubLine1 } };
			var dummyHeader1 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine1 } };

			var dummySubLine2 = new DummySubLine() { SubLineNo = 3 };
			var dummySubLine3 = new DummySubLine() { SubLineNo = 4, Ingredient = "Glass", Amount = 112m };
			var dummyLine2 = new DummyEntryLine() { EntryLineNo = 2, SubLines = new DummySubLine[] { dummySubLine2, dummySubLine3 } };
			var dummyLine3 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "11111", CountryOfOrigin = "AU" };
			var dummyHeader2 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine2, dummyLine3 } };

			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(ElectronicDocumentTypeList.Codes._830, dummyHeader1, dummyHeader2, Enumerable.Empty<string>()).ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Two data providers are different", 3, result.Count);

				AssertEquals(EntityAmendType.Add, result[0].AmendType);
				AssertEquals("M101", result[0].DataItemID);
				AssertEquals(ChangeType.Normal, result[0].ChangeType);
				AssertEquals("", result[0].BeforeValue);
				AssertEquals("Glass", result[0].AfterValue);
				AssertEquals("IDs", 2, result[0].IDsInList.Count());
				AssertEquals("IDummyEntryLine", result[0].IDsInList.ElementAt(0).IDType);
				AssertEquals("2", result[0].IDsInList.ElementAt(0).IDValue);
				AssertEquals("IDummySubLine", result[0].IDsInList.ElementAt(1).IDType);
				AssertEquals("4", result[0].IDsInList.ElementAt(1).IDValue);

				AssertEquals(EntityAmendType.Add, result[1].AmendType);
				AssertEquals("Z101", result[1].DataItemID);
				AssertEquals(ChangeType.Normal, result[1].ChangeType);
				AssertEquals("", result[1].BeforeValue);
				AssertEquals("AU", result[1].AfterValue);
				AssertEquals("IDs", 1, result[1].IDsInList.Count());
				AssertEquals("IDummyEntryLine", result[1].IDsInList.ElementAt(0).IDType);
				AssertEquals("1", result[1].IDsInList.ElementAt(0).IDValue);

				AssertEquals(EntityAmendType.Add, result[2].AmendType);
				AssertEquals("Z102", result[2].DataItemID);
				AssertEquals(ChangeType.Normal, result[2].ChangeType);
				AssertEquals("", result[2].BeforeValue);
				AssertEquals("11111", result[2].AfterValue);
				AssertEquals("IDs", 1, result[2].IDsInList.Count());
				AssertEquals("IDummyEntryLine", result[2].IDsInList.ElementAt(0).IDType);
				AssertEquals("1", result[2].IDsInList.ElementAt(0).IDValue);
			});
		}

		public void TestCompareEntryLinesSubLinesWhenDeleted()
		{
			var dummySubLine1 = new DummySubLine() { SubLineNo = 3 };
			var dummySubLine2 = new DummySubLine() { SubLineNo = 4, Ingredient = "Glass" };
			var dummyLine1 = new DummyEntryLine() { EntryLineNo = 2, SubLines = new DummySubLine[] { dummySubLine1, dummySubLine2 } };
			var dummyLine2 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "11111", CountryOfOrigin = "AU" };
			var dummyHeader1 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine1, dummyLine2 } };

			var dummySubLine3 = new DummySubLine() { SubLineNo = 3 };
			var dummyLine3 = new DummyEntryLine() { EntryLineNo = 2, SubLines = new DummySubLine[] { dummySubLine3 } };
			var dummyHeader2 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine3 } };

			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(ElectronicDocumentTypeList.Codes._830, dummyHeader1, dummyHeader2, Enumerable.Empty<string>()).ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Two data providers are different", 2, result.Count);

				AssertEquals(EntityAmendType.Delete, result[0].AmendType);
				AssertEquals("IDs", 2, result[0].IDsInList.Count());
				AssertEquals("IDummyEntryLine", result[0].IDsInList.ElementAt(0).IDType);
				AssertEquals("2", result[0].IDsInList.ElementAt(0).IDValue);
				AssertEquals("IDummySubLine", result[0].IDsInList.ElementAt(1).IDType);
				AssertEquals("4", result[0].IDsInList.ElementAt(1).IDValue);

				AssertEquals(EntityAmendType.Delete, result[1].AmendType);
				AssertEquals("IDs", 1, result[1].IDsInList.Count());
				AssertEquals("IDummyEntryLine", result[1].IDsInList.ElementAt(0).IDType);
				AssertEquals("1", result[1].IDsInList.ElementAt(0).IDValue);
			});
		}

		public void TestCompareOrganisationInIEnumerableList()
		{
			var supplier1 = new DummyOrganization() { Role = RoleType.Supplier, CompanyName = "AAA", AddressLine1 = "1 King St" };
			var dummyLine1 = new DummyEntryLine() { EntryLineNo = 1, Supplier = supplier1 };
			var dummyHeader1 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine1 } };

			var supplier2 = new DummyOrganization() { Role = RoleType.Supplier, CompanyName = "BBB", AddressLine1 = "1 Queen St" };
			var dummyLine2 = new DummyEntryLine() { EntryLineNo = 1, Supplier = supplier2 };
			var dummyHeader2 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine2 } };

			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(ElectronicDocumentTypeList.Codes._830, dummyHeader1, dummyHeader2, Enumerable.Empty<string>()).ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Two data providers are different", 2, result.Count);

				AssertEquals(EntityAmendType.Update, result[0].AmendType);
				AssertEquals("A301", result[0].DataItemID);
				AssertEquals(ChangeType.Normal, result[0].ChangeType);
				AssertEquals("AAA", result[0].BeforeValue);
				AssertEquals("BBB", result[0].AfterValue);
				AssertEquals("IDs", 2, result[0].IDsInList.Count());
				AssertEquals("IDummyEntryLine", result[0].IDsInList.ElementAt(0).IDType);
				AssertEquals("IOrganization", result[0].IDsInList.ElementAt(1).IDType);

				AssertEquals(EntityAmendType.Update, result[1].AmendType);
				AssertEquals("A303", result[1].DataItemID);
				AssertEquals(ChangeType.Normal, result[1].ChangeType);
				AssertEquals("1 King St", result[1].BeforeValue);
				AssertEquals("1 Queen St", result[1].AfterValue);
				AssertEquals("IDs", 2, result[1].IDsInList.Count());
				AssertEquals("IDummyEntryLine", result[1].IDsInList.ElementAt(0).IDType);
				AssertEquals("IOrganization", result[1].IDsInList.ElementAt(1).IDType);
			});
		}

		public void TestCompareOrganisationInSubLines()
		{
			var supplier1 = new DummyOrganization() { Role = RoleType.Supplier, CompanyName = "AAA", AddressLine1 = "1 King St" };
			var dummySubLine1 = new DummySubLine() { SubLineNo = 1, Supplier = supplier1 };
			var dummyLine1 = new DummyEntryLine() { EntryLineNo = 2, SubLines = new DummySubLine[] { dummySubLine1 } };
			var dummyHeader1 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine1 } };

			var supplier2 = new DummyOrganization() { Role = RoleType.Supplier, CompanyName = "BBB", AddressLine1 = "1 Queen St" };
			var dummySubLine2 = new DummySubLine() { SubLineNo = 1, Supplier = supplier2 };
			var dummyLine2 = new DummyEntryLine() { EntryLineNo = 2, SubLines = new DummySubLine[] { dummySubLine2 } };
			var dummyHeader2 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine2 } };

			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(ElectronicDocumentTypeList.Codes._830, dummyHeader1, dummyHeader2, Enumerable.Empty<string>()).ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Two data providers are different", 2, result.Count);

				AssertEquals(EntityAmendType.Update, result[0].AmendType);
				AssertEquals("A301", result[0].DataItemID);
				AssertEquals(ChangeType.Normal, result[0].ChangeType);
				AssertEquals("AAA", result[0].BeforeValue);
				AssertEquals("BBB", result[0].AfterValue);
				AssertEquals("IDs", 3, result[0].IDsInList.Count());
				AssertEquals("IDummyEntryLine", result[0].IDsInList.ElementAt(0).IDType);
				AssertEquals("2", result[0].IDsInList.ElementAt(0).IDValue);
				AssertEquals("IDummySubLine", result[0].IDsInList.ElementAt(1).IDType);
				AssertEquals("1", result[0].IDsInList.ElementAt(1).IDValue);
				AssertEquals("IOrganization", result[0].IDsInList.ElementAt(2).IDType);
				AssertEquals("Supplier", result[0].IDsInList.ElementAt(2).IDValue);

				AssertEquals(EntityAmendType.Update, result[1].AmendType);
				AssertEquals("A303", result[1].DataItemID);
				AssertEquals(ChangeType.Normal, result[1].ChangeType);
				AssertEquals("1 King St", result[1].BeforeValue);
				AssertEquals("1 Queen St", result[1].AfterValue);
				AssertEquals("IDummyEntryLine", result[1].IDsInList.ElementAt(0).IDType);
				AssertEquals("2", result[1].IDsInList.ElementAt(0).IDValue);
				AssertEquals("IDummySubLine", result[1].IDsInList.ElementAt(1).IDType);
				AssertEquals("1", result[1].IDsInList.ElementAt(1).IDValue);
				AssertEquals("IOrganization", result[1].IDsInList.ElementAt(2).IDType);
				AssertEquals("Supplier", result[1].IDsInList.ElementAt(2).IDValue);
			});
		}

		public void TestCompareEntryLinesSubLinesBothAdded_OriginalListIsNull()
		{
			var dummySubLine1 = new DummySubLine() { SubLineNo = 1, Ingredient = "Wood" };
			var dummySubLine2 = new DummySubLine() { SubLineNo = 2, Ingredient = "Glass" };
			var dummyLine1 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine1, dummySubLine2 } };
			var dummySubLine3 = new DummySubLine() { SubLineNo = 1, Ingredient = "Plastics" };
			var dummyLine2 = new DummyEntryLine() { EntryLineNo = 2, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine3 } };
			var dummyHeader1 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine1, dummyLine2 } };

			var dummySubLine2_1 = new DummySubLine() { SubLineNo = 1, Ingredient = "Wood" };
			var dummyLine2_1 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine2_1 } };
			var dummySubLine2_3 = new DummySubLine() { SubLineNo = 1, Ingredient = "Plastics" };
			var dummyLine2_2 = new DummyEntryLine() { EntryLineNo = 2, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine2_3 } };
			var dummySubLine2_2 = new DummySubLine() { SubLineNo = 1, Ingredient = "Glass" };
			var dummyLine2_3 = new DummyEntryLine() { EntryLineNo = 3, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine2_2 } };
			var dummyHeader2 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine2_1, dummyLine2_2, dummyLine2_3 } };

			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(ElectronicDocumentTypeList.Codes._830, dummyHeader1, dummyHeader2, Enumerable.Empty<string>()).ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Two data providers are different", 3, result.Count);

				AssertEquals(EntityAmendType.Delete, result[0].AmendType);
				AssertEquals("IDs", 2, result[0].IDsInList.Count());
				AssertEquals("IDummyEntryLine", result[0].IDsInList.ElementAt(0).IDType);
				AssertEquals("1", result[0].IDsInList.ElementAt(0).IDValue);
				AssertEquals("IDummySubLine", result[0].IDsInList.ElementAt(1).IDType);
				AssertEquals("2", result[0].IDsInList.ElementAt(1).IDValue);

				AssertEquals(EntityAmendType.Add, result[1].AmendType);
				AssertEquals("Z102", result[1].DataItemID);
				AssertEquals(ChangeType.Normal, result[1].ChangeType);
				AssertEquals("", result[1].BeforeValue);
				AssertEquals("1", result[1].AfterValue);
				AssertEquals("IDs", 1, result[1].IDsInList.Count());
				AssertEquals("IDummyEntryLine", result[1].IDsInList.ElementAt(0).IDType);
				AssertEquals("3", result[1].IDsInList.ElementAt(0).IDValue);

				AssertEquals(EntityAmendType.Add, result[2].AmendType);
				AssertEquals("IDs", 2, result[2].IDsInList.Count());
				AssertEquals("IDummyEntryLine", result[2].IDsInList.ElementAt(0).IDType);
				AssertEquals("3", result[2].IDsInList.ElementAt(0).IDValue);
				AssertEquals("IDummySubLine", result[2].IDsInList.ElementAt(1).IDType);
				AssertEquals("1", result[2].IDsInList.ElementAt(1).IDValue);
			});
		}

		public void TestCompareEntryLinesSubLinesBothDeleted()
		{
			var dummySubLine1 = new DummySubLine() { SubLineNo = 1, Ingredient = "Wood" };
			var dummyLine1 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine1 } };
			var dummySubLine3 = new DummySubLine() { SubLineNo = 1, Ingredient = "Plastics" };
			var dummyLine2 = new DummyEntryLine() { EntryLineNo = 2, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine3 } };
			var dummySubLine2 = new DummySubLine() { SubLineNo = 1, Ingredient = "Glass" };
			var dummyLine3 = new DummyEntryLine() { EntryLineNo = 3, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine2 } };
			var dummyHeader1 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine1, dummyLine2, dummyLine3 } };

			var dummySubLine2_1 = new DummySubLine() { SubLineNo = 1, Ingredient = "Wood" };
			var dummySubLine2_2 = new DummySubLine() { SubLineNo = 2, Ingredient = "Glass" };
			var dummyLine2_2 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine2_1, dummySubLine2_2 } };
			var dummySubLine2_3 = new DummySubLine() { SubLineNo = 1, Ingredient = "Plastics" };
			var dummyLine2_1 = new DummyEntryLine() { EntryLineNo = 2, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine2_3 } };
			var dummyHeader2 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine2_1, dummyLine2_2 } };

			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(ElectronicDocumentTypeList.Codes._830, dummyHeader1, dummyHeader2, Enumerable.Empty<string>()).ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Two data providers are different", 2, result.Count);

				AssertEquals(EntityAmendType.Add, result[0].AmendType);
				AssertEquals("M101", result[0].DataItemID);
				AssertEquals(ChangeType.Normal, result[0].ChangeType);
				AssertEquals("", result[0].BeforeValue);
				AssertEquals("Glass", result[0].AfterValue);
				AssertEquals("IDs", 2, result[0].IDsInList.Count());
				AssertEquals("IDummyEntryLine", result[0].IDsInList.ElementAt(0).IDType);
				AssertEquals("1", result[0].IDsInList.ElementAt(0).IDValue);
				AssertEquals("IDummySubLine", result[0].IDsInList.ElementAt(1).IDType);
				AssertEquals("2", result[0].IDsInList.ElementAt(1).IDValue);

				AssertEquals(EntityAmendType.Delete, result[1].AmendType);
				AssertEquals("IDs", 1, result[1].IDsInList.Count());
				AssertEquals("IDummyEntryLine", result[1].IDsInList.ElementAt(0).IDType);
				AssertEquals("3", result[1].IDsInList.ElementAt(0).IDValue);
			});
		}

		public void TestCompareEntryLinesSubLinesWhenDeleted_CurrentListIsNull()
		{
			var dummySubLine1 = new DummySubLine() { SubLineNo = 1, Ingredient = "Plastics" };
			var dummySubLine2 = new DummySubLine() { SubLineNo = 2, Ingredient = "Woods" };
			var dummyLine1 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine1, dummySubLine2 } };
			var dummyHeader1 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine1 } };

			var dummyLine2 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "1" };
			var dummyHeader2 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine2 } };

			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(ElectronicDocumentTypeList.Codes._830, dummyHeader1, dummyHeader2, Enumerable.Empty<string>()).ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Two data providers are different", 2, result.Count);

				AssertEquals(EntityAmendType.Delete, result[0].AmendType);
				AssertEquals("IDs", 2, result[0].IDsInList.Count());
				AssertEquals("IDummyEntryLine", result[0].IDsInList.ElementAt(0).IDType);
				AssertEquals("1", result[0].IDsInList.ElementAt(0).IDValue);
				AssertEquals("IDummySubLine", result[0].IDsInList.ElementAt(1).IDType);
				AssertEquals("1", result[0].IDsInList.ElementAt(1).IDValue);

				AssertEquals(EntityAmendType.Delete, result[1].AmendType);
				AssertEquals("IDs", 2, result[1].IDsInList.Count());
				AssertEquals("IDummyEntryLine", result[1].IDsInList.ElementAt(0).IDType);
				AssertEquals("1", result[1].IDsInList.ElementAt(0).IDValue);
				AssertEquals("IDummySubLine", result[1].IDsInList.ElementAt(1).IDType);
				AssertEquals("2", result[1].IDsInList.ElementAt(1).IDValue);
			});
		}

		public void TestCompareWhenNumericFieldsAddedWithEmpty()
		{
			var dummySubLine11 = new DummySubLine() { SubLineNo = 1, Price = 0 };
			var dummyLine1 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine11 } };
			var dummyHeader1 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine1 } };

			var dummySubLine21 = new DummySubLine() { SubLineNo = 1 };
			var dummySubLine22 = new DummySubLine() { SubLineNo = 2, Price = 0 };
			var dummySubLine23 = new DummySubLine() { SubLineNo = 3, Price = 10 };
			var dummyLine2 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine21, dummySubLine22, dummySubLine23 } };
			var dummyHeader2 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine2 } };

			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(ElectronicDocumentTypeList.Codes._830, dummyHeader1, dummyHeader2, Enumerable.Empty<string>()).ToList();
			CombineAssertions(() =>
			{
				AssertEquals(1, result.Count);

				AssertEquals(EntityAmendType.Add, result[0].AmendType);
				AssertEquals("", result[0].BeforeValue);
				AssertEquals("10", result[0].AfterValue);
				AssertEquals("IDs", 2, result[0].IDsInList.Count());
				AssertEquals("IDummySubLine", result[0].IDsInList.ElementAt(1).IDType);
				AssertEquals("3", result[0].IDsInList.ElementAt(1).IDValue);
			});
		}

		[TestDate(2022, 9, 29)]
		public void TestCompareWhenDateTimeFieldsAddedWithEmpty()
		{
			var dummySubLine11 = new DummySubLine() { SubLineNo = 1, ApprovalDate = ZDateTime.Empty };
			var dummyLine1 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine11 } };
			var dummyHeader1 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine1 } };

			var dummySubLine21 = new DummySubLine() { SubLineNo = 1 };
			var dummySubLine22 = new DummySubLine() { SubLineNo = 2 };
			var dummySubLine23 = new DummySubLine() { SubLineNo = 3, ApprovalDate = ZDateTime.Empty };
			var dummySubLine24 = new DummySubLine() { SubLineNo = 4, ApprovalDate = ZDateTime.Today };
			var dummyLine2 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine21, dummySubLine22, dummySubLine23, dummySubLine24 } };
			var dummyHeader2 = new DummyEntryHeader() { EntryLines = new DummyEntryLine[] { dummyLine2 } };

			var result = new DataProviderComparer<IDummyEntryHeader>().Compare(ElectronicDocumentTypeList.Codes._830, dummyHeader1, dummyHeader2, Enumerable.Empty<string>()).ToList();
			CombineAssertions(() =>
			{
				AssertEquals(1, result.Count);

				AssertEquals(EntityAmendType.Add, result[0].AmendType);
				AssertEquals("", result[0].BeforeValue);
				DateTime.TryParse(result[0].AfterValue, out DateTime dt1);
				AssertEquals("20220929", dt1.ToString(DateFormatType.Date));
				AssertEquals("IDs", 2, result[0].IDsInList.Count());
				AssertEquals("IDummySubLine", result[0].IDsInList.ElementAt(1).IDType);
				AssertEquals("4", result[0].IDsInList.ElementAt(1).IDValue);
			});
		}

		public void TestCompareImportEntryHeaderProperties()
		{
			var importHeader1 = new ImportEntryHeader();
			var importHeader2 = new ImportEntryHeader { ImporterType = ImporterTypeCodeList.Codes.A, OnlineTradeType = OnlineTradeTypeCodeList.Codes.Z, Shipper = new Organisation(RoleType.Shipper), OnlineTradeDistributor = new Organisation(RoleType.OnlineTradeDistributor), OnlineTradeSeller = new Organisation(RoleType.OnlineTradeSeller), OnlineTradeSellingAgent = new Organisation(RoleType.OnlineTradeSellingAgent) };
			importHeader2.Shipper.SetRegistrationIDNumbers(new IDNumberAndType[] { new IDNumberAndType() { Type = IdentificationType.ForeignCompanyID, Number = "CNTOSHIN12345" } });
			importHeader2.OnlineTradeDistributor.SetRegistrationIDNumbers(new IDNumberAndType[] { new IDNumberAndType() { Type = IdentificationType.ForeignCompanyID, Number = "020210001" } });
			importHeader2.OnlineTradeSeller.SetRegistrationIDNumbers(new IDNumberAndType[] { new IDNumberAndType() { Type = IdentificationType.ForeignCompanyID, Number = "A12200001" } });
			importHeader2.OnlineTradeSellingAgent.SetRegistrationIDNumbers(new IDNumberAndType[] { new IDNumberAndType() { Type = IdentificationType.ForeignCompanyID, Number = "D12200001" } });

			var importResult = new DataProviderComparer<IImportEntryHeader>().Compare(ElectronicDocumentTypeList.Codes._929, importHeader1, importHeader2, Enumerable.Empty<string>()).ToList();
			AssertNotNull(importResult.Any(x => x.DataItemID == ImportAmendmentDataItemIDList.Codes.A203 && x.AfterValue == ImporterTypeCodeList.Codes.A));
			AssertNotNull(importResult.Any(x => x.DataItemID == ImportAmendmentDataItemIDList.Codes.A618 && x.AfterValue == "CNTOSHIN12345"));
			AssertNotNull(importResult.Any(x => x.DataItemID == ImportAmendmentDataItemIDList.Codes.A619 && x.AfterValue == "020210001"));
			AssertNotNull(importResult.Any(x => x.DataItemID == ImportAmendmentDataItemIDList.Codes.A621 && x.AfterValue == OnlineTradeTypeCodeList.Codes.Z));
			AssertNotNull(importResult.Any(x => x.DataItemID == ImportAmendmentDataItemIDList.Codes.A622 && x.AfterValue == "A12200001"));
			AssertNotNull(importResult.Any(x => x.DataItemID == ImportAmendmentDataItemIDList.Codes.A624 && x.AfterValue == "D12200001"));
		}

		[TestDate(2024, 2, 26)]
		public void TestCompareDerivedInterface()
		{
			var dummySubLine11 = new DummySubLine() { SubLineNo = 1, ApprovalDate = ZDateTime.Empty };
			var dummyLine1 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine11 } };
			var dummyHeader1 = new DummyEntryDerivedHeader() { EntryLines = new DummyEntryLine[] { dummyLine1 } };

			var dummySubLine21 = new DummySubLine() { SubLineNo = 1 };
			var dummySubLine22 = new DummySubLine() { SubLineNo = 2 };
			var dummySubLine23 = new DummySubLine() { SubLineNo = 3, ApprovalDate = ZDateTime.Empty };
			var dummySubLine24 = new DummySubLine() { SubLineNo = 4, ApprovalDate = ZDateTime.Today };
			var dummyLine2 = new DummyEntryLine() { EntryLineNo = 1, HSCode = "1", SubLines = new DummySubLine[] { dummySubLine21, dummySubLine22, dummySubLine23, dummySubLine24 } };
			var dummyHeader2 = new DummyEntryDerivedHeader() { EntryLines = new DummyEntryLine[] { dummyLine2 } };

			var result = new DataProviderComparer<IDummyEntryDerivedHeader>().Compare(ElectronicDocumentTypeList.Codes._830, dummyHeader1, dummyHeader2, Enumerable.Empty<string>()).ToList();
			CombineAssertions(() =>
			{
				AssertEquals(1, result.Count);

				AssertEquals(EntityAmendType.Add, result[0].AmendType);
				AssertEquals("", result[0].BeforeValue);
				DateTime.TryParse(result[0].AfterValue, out DateTime dt1);
				AssertEquals("20240226", dt1.ToString(DateFormatType.Date));
				AssertEquals("IDs", 2, result[0].IDsInList.Count());
				AssertEquals("IDummySubLine", result[0].IDsInList.ElementAt(1).IDType);
				AssertEquals("4", result[0].IDsInList.ElementAt(1).IDValue);
			});
		}

		public class DummySubLine : IDummySubLine
		{
			public ZInt SubLineNo { get; set; }
			public ZString Ingredient { get; set; }
			public ZDecimal Amount { get; set; }
			public ZDecimal Price { get; set; }
			public ZDateTime ApprovalDate { get; set; }
			public IOrganization Supplier { get; set; }
		}

		public interface IDummySubLine
		{
			[ID()]
			ZInt SubLineNo { get; }
			[DataItemID("M101")]
			ZString Ingredient { get; }
			ZDecimal Amount { get; }
			[DataItemID("C201")]
			ZDecimal Price { get; }
			[DataItemID("G105")]
			ZDateTime ApprovalDate { get; }
			IOrganization Supplier { get; }
		}

		public class DummyEntryLine : IDummyEntryLine
		{
			public ZInt EntryLineNo { get; set; }
			public ZString CountryOfOrigin { get; set; }
			public ZString HSCode { get; set; }
			public IOrganization Supplier { get; set; }
			public IEnumerable<IDummySubLine> SubLines { get; set; }
		}

		public interface IDummyEntryLine
		{
			[ID()]
			ZInt EntryLineNo { get; }
			[DataItemID("Z101")]
			ZString CountryOfOrigin { get; }
			[DataItemID("Z102")]
			ZString HSCode { get; }
			IOrganization Supplier { get; }
			IEnumerable<IDummySubLine> SubLines { get; }
		}

		public class DummyOrganization : IOrganization
		{
			public RoleType Role { get; set; }
			public ZString CompanyName { get; set; }
			public ZString RepresentativeName { get; set; }
			public ZString AddressLine1 { get; set; }
			public ZString AddressLine2 { get; set; }
			public ZString Postcode { get; set; }
			public ZString RoadNameCode { get; set; }
			public ZString BuildingNumber { get; set; }
			public ZString CountryCode { get; set; }
			public ZString PhoneNumber { get; set; }
			public ZString ExtensionNumber { get; set; }
			public ZString Email { get; set; }
			public ZString MobileNumber { get; set; }
			public ZString FaxNumber { get; set; }
			public ZBool IsIndividual { get; set; }
			public ZString BusinessRegNo { get; }
			public ZString KoreanRegNoForResident { get; }
			public ZString UnipassIDForOrganization { get; }
			public ZString BuyerID { get; }
			public ZString OfficeID { get; }
			public ZString KoreanRegNoForForeigner { get; }
			public ZString PassportNo { get; }
			public ZString UnipassIDForIndividual { get; }
			public ZString CertificateOfOriginExporterNumber { get; }
			public ZString CorporationCode { get; }
			public ZString CarrierCode { get; }
			public ZString ECommerceCompanyID { get; }
			public ZString ForeignCompanyID { get; }
		}

		public class DummyEntryHeader : IDummyEntryHeader
		{
			public ZString EntryNumber { get; set; }
			public ZDateTime LoadingDate { get; set; }
			public ZInt PackageCount { get; set; }
			public ZDecimal TotalValue { get; set; }
			public ZBool IsPersonalItem { get; set; }
			public IOrganization Exporter { get; set; }
			public IOrganization Importer { get; set; }
			public IOrganization Supplier { get; set; }
			public IOrganization Manufacturer { get; set; }
			public IEnumerable<IDummyEntryLine> EntryLines { get; set; }
		}

		public interface IDummyEntryHeader
		{
			[DataItemID("X101", ChangeType.Normal)]
			ZString EntryNumber { get; }
			[DataItemID("X102", ChangeType.DutyTaxRelated)]
			ZDateTime LoadingDate { get; }
			[DataItemID("X103", ChangeType.DutyTaxRelatedAndNormal)]
			ZInt PackageCount { get; }
			[DataItemID("X104")]
			ZDecimal TotalValue { get; }
			[DataItemID("X105")]
			ZBool IsPersonalItem { get; }
			IOrganization Exporter { get; }
			IOrganization Importer { get; }
			IOrganization Supplier { get; }
			IOrganization Manufacturer { get; }
			IEnumerable<IDummyEntryLine> EntryLines { get; }
		}

		public class DummyEntryDerivedHeader : DummyEntryHeader, IDummyEntryDerivedHeader
		{
			public IEnumerable<IDummySubLine> DummyLines { get; set; }
		}

		public interface IDummyEntryDerivedHeader : IDummyEntryHeader
		{
			IEnumerable<IDummySubLine> DummyLines { get; }
		}
	}
}
