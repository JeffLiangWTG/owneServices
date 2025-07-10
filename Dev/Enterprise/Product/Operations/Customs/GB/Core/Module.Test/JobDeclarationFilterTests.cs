using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration.Testing;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Wizards.CFSP;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.Testing
{
	[TestedType(typeof(JobDeclarationModule))]
	sealed class JobDeclarationModuleTest : Customs.Module.Testing.JobDeclarationModuleAbstractTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.UnitedKingdom;

		protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

		protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

		protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			return factory.NewWithValidTestData<JobDeclaration>();
		}

		protected override void SetJE_ApplicationCode(BaseJobDeclaration declaration)
		{
			base.SetJE_ApplicationCode(declaration);
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
		}

		protected override BaseJobDeclaration CreateDeclarationForFetchHintTest(BusinessObjectFactory factory, string messageType, int i)
		{
			// If the GB fectch hints for (GB)CusAddInfo don't work then the test that calls this guy will fail with an error about 60+ hits on CusAddInfo.
			var declaration = (JobDeclaration)base.CreateDeclarationForFetchHintTest(factory, messageType, i);
			declaration.JE_LocationOfGoods = "LHR";
			declaration.JE_LocationOtherInformation = "LHR 123456";
			declaration.JE_LocationQualifier = "L";
			declaration.JE_DeclarationType = "EL";
			declaration.JE_EntryStatus = "AWR";
			declaration.JE_MasterUCR = "12345";

			declaration.Logs.AddNew(Events.CustomsCleared, ZDateTimeOffset.Now);
			declaration.SubLocation = "CAX";
			declaration.SingleEntry.CH_ImportClearanceStatusICS = "1";
			declaration.SingleEntry.CH_StyleOfEntrySOE = "07";
			declaration.SingleEntry.CH_RouteOfEntry = "H";
			declaration.SingleEntry.CH_IrcInventoryReturnCode = InventoryReturnCodesCCS.Codes.CcsUkDatabaseEntryVersionIsLaterThanChief;
			declaration.JE_CustomsProfile = "AAA";
			declaration.ZG_Gateway = "MCP";
			declaration.ZG_HouseSplitReference = "01";
			declaration.ZG_VATDeferType = "A";
			declaration.ZG_VATDeferNumber = "1234567";
			declaration.JE_PaymentMethod = "B";
			declaration.JE_EntryStatus = "ACC";
			declaration.JE_DefermentAccountNumber = "7654321";
			declaration.SingleEntry.CH_IrcInventoryReturnCode = GB.Business.CodeDescriptionPairLists.InventoryReturnCodesCCS.Codes.CcsUkDatabaseEntryVersionIsLaterThanChief;
			CreateExitReportData(declaration, i);
			return declaration;
		}

		void CreateExitReportData(JobDeclaration declaration, int i)
		{
			var exitReportStatuses = new List<ZString>();
			switch (i)
			{
				case 0:
					exitReportStatuses.Add("EXR");
					exitReportStatuses.Add("COX");
					break;
				case 1:
					exitReportStatuses.Add("COX");
					exitReportStatuses.Add("COX");
					break;
				default:
					exitReportStatuses.Add("EXR");
					break;
			}
			ExitControlTestHelper.CreateCusExitReportWithStatus(declaration, exitReportStatuses.ToArray());
		}

		public void TestImportFromXmlMenu()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			using (JobDeclarationModule module = new JobDeclarationModule())
			{
				AssertNotNull(module.FormActionMenu.FindByText("Actions").MenuItems.FindByText("Data Transfer").MenuItems.FindByText("Import From XML"));
			}
		}
	}

	[TestedType(typeof(JobDeclarationFilterBusinessObject))]
	sealed class JobDeclarationFilterTests : FilterStripBusinessObjectTestCase
	{
		public void TestEntryStatusForIntegratedCountry()
		{
			var declaration0 = Factory.New<BaseJobDeclaration>();
			declaration0.JE_EntryStatus = "SUB";
			var declaration1 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_EntryStatus = "SUB";
			var entryHeader0 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader0.CH_EntryStatus = "DUT";
			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_EntryStatus = "SUB";
			var entryHeader1 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_EntryStatus = "DUT";
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_EntryStatus = "SUB";
			var declaration3 = Factory.New<BaseJobDeclaration>();
			declaration3.JE_EntryStatus = "SUB";
			var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
			entryHeader3.CH_EntryStatus = "DUT";
			var entryHeader4 = declaration3.CustomsEntryHeaders.AddNew();
			entryHeader4.CH_EntryStatus = "CSN";
			var declaration4 = Factory.New<BaseJobDeclaration>();
			var entryHeader5 = declaration4.CustomsEntryHeaders.AddNew();
			entryHeader5.CH_EntryStatus = ZString.Empty;
			var entryHeader6 = declaration4.CustomsEntryHeaders.AddNew();
			entryHeader6.CH_EntryStatus = ZString.Empty;
			declaration4.JE_EntryStatus = "SUB";
			Factory.Save();
			AssertEquals("SUB", declaration4.JE_EntryStatus);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (EntryStatusFilter)stripBO[stripBO.EntryStatusText];
			filter.IsActive = true;
			filter.Property = "SUB";
			var filteredDecs = Factory.Load<BaseJobDeclaration>(stripBO.Filter);
			AssertEquals(declaration0.PK, filteredDecs[0].PK);
			AssertEquals(declaration4.PK, filteredDecs[1].PK);
			filter.Property = "DUT";
			filteredDecs = Factory.Load<BaseJobDeclaration>(stripBO.Filter);
			AssertEquals(declaration1.PK, filteredDecs[0].PK);
			filter.Property = "CSN";
			filteredDecs = Factory.Load<BaseJobDeclaration>(stripBO.Filter);
			AssertEquals(0, filteredDecs.Length);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			AssertEquals("ComparisonOperator should have been reset.", filter.ComparisonOperator, ModuleTextFilter.ComparisonConstants.Exact);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			AssertEquals("ComparisonOperator should have been changed.", filter.ComparisonOperator, ModuleTextFilter.ComparisonConstants.NotEqual);
		}

		public void TestSuppDecDueDateFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_SuppDecDueDate = new ZDateTime(2020, 12, 03);
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_SuppDecDueDate = new ZDateTime(2020, 11, 01);
			Factory.Save();

			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleDateFilter)stripBO["Supplementary Declaration Due Date"];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2020, 12, 01);
			filter.Property2 = new ZDateTime(2020, 12, 01).AddDays(7);

			Assert(declaration1.MatchesFilter(stripBO.Filter));
			Assert(!declaration2.MatchesFilter(stripBO.Filter));
		}

		public void TestJE_EntryAuthorisationDate()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_EntryAuthorisationDate = new ZDateTime(2020, 12, 03);
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_EntryAuthorisationDate = new ZDateTime(2020, 11, 01);
			Factory.Save();

			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleDateFilter)stripBO["Tax Point"];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2020, 12, 01);
			filter.Property2 = new ZDateTime(2020, 12, 01).AddDays(7);

			Assert(declaration1.MatchesFilter(stripBO.Filter));
			Assert(declaration1.MatchesFilter(stripBO.Filter));
		}

		public void TestActualOfficeOfExitFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_ExitActualOffice = "GB000084";

			var declaration2 = Factory.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_ExitActualOffice = "XI000084";
			Factory.Save();
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)stripBO[Module.EntryHeaderFilterBusinessObject.FilterConstants.ActualOfficeOfExit];
			AssertNotNull("Filter " + Module.EntryHeaderFilterBusinessObject.FilterConstants.ActualOfficeOfExit, filter);

			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "GB";
			Assert(declaration1.MatchesFilter(stripBO.Filter));
			Assert(!declaration2.MatchesFilter(stripBO.Filter));
		}

		public void TestDateOfExitFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_ExitDate = new ZDateTime(2020, 12, 03);

			var declaration2 = Factory.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_ExitDate = new ZDateTime(2020, 11, 01);
			Factory.Save();

			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleSingleDateFilter)stripBO[Module.EntryHeaderFilterBusinessObject.FilterConstants.DateOfExit];
			AssertNotNull("Filter " + Module.EntryHeaderFilterBusinessObject.FilterConstants.DateOfExit, filter);
			filter.IsActive = true;
			filter.Property1 = new ZDateTime(2020, 12, 03);
			Assert(declaration1.MatchesFilter(stripBO.Filter));
			Assert(!declaration2.MatchesFilter(stripBO.Filter));
		}

		public void TestJobEntrySubStyleFilter()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = "LV";
			company.GC_Code = "DJC";
			var branch = company.Branches.AddNew();
			branch.GB_RL_NKHomePort = "LVRIX";
			branch.GB_Code = "DJC";
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.CustomsCodes.AddNew("EOR", "123456789000");
			branch.GB_OH_OrgProxy = orgProxy.PK;

			Factory.Save();

			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var declarationDummy = Factory.New<JobDeclaration>();
				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var line1 = invoice.InvoiceLines.AddNew();
				line1.JI_Procedure = "4000000";

				declaration.JE_MessageType = "IMP";
				declaration.CusEntryInstruction.CEI_SubStyle = "A";
				declaration.JE_EntryStyle = "IM";
				declaration.JE_LocationOfGoods = "goods";
				declaration.ZG_ShipmentType = EU.Business.ShipmentTypeList.Codes.BackToBack;
				declaration.ZG_CTStatusID = Common.EU.ImportCommunityTransitStatusList.Codes.T2;
				var testDate = new ZDateTimeOffset(1998, 8, 9);
				var log = declaration.Logs.AddNew(Events.CustomsCleared, testDate);
				Factory.Save();

				CheckTextFilter("Entry Sub-style", "A", declaration, isExpensive: false);
			}
		}

		public void TestApplicationCodeFilterCaption()
		{
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			AssertEquals("Messaging System", stripBO.ApplicationCodeFilterCaption.ToString());
		}

		public void TestLocationOfGoodsAndShedFilters()
		{
			var dec = Factory.New<JobDeclaration>();
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_LocationOfGoods = "BHX";
			dec1.SubLocation = "";
			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_LocationOfGoods = "LHR";
			dec2.SubLocation = "BAC";
			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_LocationOfGoods = "";
			dec3.SubLocation = "SLS";
			var dec4 = Factory.New<JobDeclaration>();
			dec4.JE_LocationOfGoods = "LHR";
			dec4.SubLocation = "BAC";
			var dec5 = Factory.New<JobDeclaration>();
			dec5.JE_LocationOfGoods = "XXX";
			dec5.SubLocation = "CCC";

			Factory.Save();

			CheckTextFilter("Location of Goods", "LHR", null, true, 2, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains);
			CheckTextFilter("Location of Goods", "LHR", null, true, 4, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotContain);
			CheckTextFilter("Location of Goods", "LHR", null, false, 2, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);
			CheckTextFilter("Location of Goods", "LHR", null, false, 4, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual);
			CheckTextFilter("Shed", "B", null, false, 2, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith);
			CheckTextFilter("Shed", "B", null, false, 4, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotStartsWith);
			CheckTextFilter("Shed", "B", null, true, 2, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains);
			CheckTextFilter("Shed", "B", null, true, 4, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotContain);
			CheckTextFilter("Shed", "BAC", null, false, 2, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);
			CheckTextFilter("Shed", "BAC", null, false, 4, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual);
		}

		public void TestFiltersInCusEntryHeaderAddInfo()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.CustomsEntryHeaders.AddNew();
			dec1.SingleEntry.CH_RouteOfEntry = "2X";
			dec1.SingleEntry.CH_IrcInventoryReturnCode = "049";
			dec1.SingleEntry.CH_ImportClearanceStatusICS = "X9";

			var dec2 = Factory.New<JobDeclaration>();
			dec2.CustomsEntryHeaders.AddNew();
			dec2.SingleEntry.CH_RouteOfEntry = "2X";

			var dec3 = Factory.New<JobDeclaration>();
			dec3.CustomsEntryHeaders.AddNew();
			dec3.SingleEntry.CH_ImportClearanceStatusICS = "X9";

			var dec4 = Factory.New<JobDeclaration>();
			dec4.CustomsEntryHeaders.AddNew();
			dec4.SingleEntry.CH_ImportClearanceStatusICS = "X9";

			var dec5 = Factory.New<JobDeclaration>();
			dec5.CustomsEntryHeaders.AddNew();
			dec5.SingleEntry.CH_IrcInventoryReturnCode = "049";

			Factory.Save();

			CheckTextFilter("Route Of Entry", "2X", null, true, 2, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains);
			CheckTextFilter("IRC Inventory Return Code", "049", null, true, 2, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains);
			CheckTextFilter("ICS", "X9", null, true, 3, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains);
		}

		public void TestJobDeclarationFilter()
		{
			var declarationDummy = Factory.New<JobDeclaration>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_LocationOfGoods = "LOC";
			declaration.SubLocation = "SHD";
			declaration.SingleEntry.CH_ImportClearanceStatusICS = "A1";
			declaration.SingleEntry.CH_RouteOfEntry = "H";
			declaration.JE_CustomsProfile = "BDG";
			declaration.ZG_HouseSplitReference = "01";
			declaration.ZG_VATDeferType = "A";
			declaration.ZG_VATDeferNumber = "4567890";
			declaration.JE_PaymentMethod = "B";
			declaration.JE_DefermentAccountNumber = "0987654";
			declaration.SingleEntry.CH_IrcInventoryReturnCode = GB.Business.CodeDescriptionPairLists.InventoryReturnCodesCCS.Codes.CcsUkDatabaseEntryVersionIsLaterThanChief;
			declaration.ZG_Gateway = "CCSUK"; //this needs to be last one set otherwise it gets cleared by something !!!
			declaration.JE_EidrType = EidrTypeList.Codes.DEL;
			Factory.Save();

			CheckTextFilter("Location of Goods", "LOC", declaration, false);
			CheckTextFilter("Shed", "SHD", declaration, false);
			CheckTextFilter("ICS", "A1", declaration, false);
			CheckTextFilter("CSP", "CCSUK", declaration);
			CheckTextFilter("Route Of Entry", "H", declaration, isExpensive: false); // not expensive becuase it's not in JE_AddInfo
			CheckTextFilter("Badge", "BDG", declaration, false);
			CheckTextFilter("House Split Reference", "01", declaration);
			CheckTextFilter("VAT Defer Type", "A", declaration);
			CheckTextFilter("VAT Defer Number", "4567890", declaration);
			CheckTextFilter("Other Defer Type", "B", declaration, false);
			CheckTextFilter("Other Defer Number", "0987654", declaration, false);
			CheckTextFilter("IRC Inventory Return Code", "053", declaration, false);
			CheckTextFilter("EIDR Type", "DEL", declaration, false);
		}

		public void TestLookups()
		{
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var pairs = new Dictionary<string, string>();
			pairs.Add("Route Of Entry", "3");
			pairs.Add("ICS", "23");
			pairs.Add("CSP", "NES");
			pairs.Add("VAT Defer Type", "A");
			pairs.Add("Other Defer Type", "A");
			pairs.Add("IRC Inventory Return Code", "053");
			foreach (var pair in pairs)
			{
				var filter = (ModuleTextFilter)stripBO[pair.Key];
				AssertNotNull(filter.List);
				Assert(pair.Key + " list should contain " + pair.Value, ((CodeDescriptionPairList)filter.List).ContainsCode(pair.Value));
			}
		}

		public void TestNIModeLookupDoesNotContainBlank()
		{
			AssertFilterNotContains("Northern Ireland Mode", NIModeList.Codes.NotToOrFromNi);
		}

		public void TestLocationOfGoodsForCDSFilter()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_ApplicationCode = "CHF";
			dec1.JE_LocationOfGoods = "BHX";
			dec1.JE_SubLocationOfGoods = "";
			dec1.JE_LocationOtherInformation = "Location A 1234";

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_ApplicationCode = "CHF";
			dec2.JE_LocationOfGoods = "LHR";
			dec2.JE_SubLocationOfGoods = "BAC";
			dec2.JE_LocationOtherInformation = "Location B";

			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_ApplicationCode = "CHF";
			dec3.JE_LocationOfGoods = "";
			dec3.JE_SubLocationOfGoods = "SLS";
			dec3.JE_LocationOtherInformation = "Location C - 12   34    ";

			var dec4 = Factory.New<JobDeclaration>();
			dec4.JE_ApplicationCode = "CHF";
			dec4.JE_LocationOfGoods = "LHR";
			dec4.JE_SubLocationOfGoods = "BAC";
			dec4.JE_LocationOtherInformation = "LocationA1234";

			var dec5 = Factory.New<JobDeclaration>();
			dec5.JE_ApplicationCode = "CHF";
			dec5.JE_LocationOfGoods = "XXX";
			dec5.JE_SubLocationOfGoods = "CCC";
			dec5.JE_LocationOtherInformation = "   A1234   ";

			Factory.Save();

			const string containsOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains;
			const string notContainOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotContain;

			const string shortLocation = "Location (short) of Goods for CDS";
			CheckTextFilter(shortLocation, "AC", null, true, 2, containsOperator);
			CheckTextFilter(shortLocation, "AC", null, true, 3, notContainOperator);
			CheckTextFilter(shortLocation, "BAC", null, true, 2, containsOperator);
			CheckTextFilter(shortLocation, "BAC", null, true, 3, notContainOperator);

			const string fullLocation = "Location (full) of Goods for CDS";
			CheckTextFilter(fullLocation, "A1234", null, true, 3, containsOperator);
			CheckTextFilter(fullLocation, "A1234", null, true, 2, notContainOperator);
			CheckTextFilter(fullLocation, "A  12   34", null, true, 3, containsOperator);
			CheckTextFilter(fullLocation, "A  12   34", null, true, 2, notContainOperator);
			CheckTextFilter(fullLocation, "1234", null, true, 4, containsOperator);
			CheckTextFilter(fullLocation, "1234", null, true, 1, notContainOperator);
			CheckTextFilter(fullLocation, " 1 2 3 4 ", null, true, 4, containsOperator);
			CheckTextFilter(fullLocation, " 1 2 3 4 ", null, true, 1, notContainOperator);
		}

		void AssertFilterNotContains(string key, string value, string description = null)
		{
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)stripBO[key];
			AssertNotNull(filter.List);
			if (description != null)
			{
				Assert(key + " list should not contain code " + value + " with description " + description, ((CodeDescriptionPairList)filter.List).GetDescriptionFromCode(value) != description);
			}
			else
			{
				Assert(key + " list should not contain code " + value, !((CodeDescriptionPairList)filter.List).ContainsCode(value));
			}
		}

		void CheckTextFilter(ZString filterField, ZString filterValue, JobDeclaration declaration, bool isExpensive = true
			, int expectedCount = 1
			, string comparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith)
		{
			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)stripBO[filterField];
			filter.Property = filterValue;
			filter.ComparisonOperator = comparisonOperator;
			filter.IsActive = true;
			declarationCollection.Load(stripBO.Filter);
			AssertEquals(expectedCount, declarationCollection.Count);
			if (declaration != null)
			{
				Assert(declarationCollection.Contains(declaration));
			}

			AssertEquals(isExpensive, filter.IsExpensiveQuery);
		}

		public void TestGetModuleFilters_Description()
		{
			var stripBO = GetNewFilterStripBusinessObject();
			CombineAssertions(() =>
			{
				foreach (var key in new[] {
					JobDeclarationFilterBusinessObject.FilterConstants.LocationOfGoods,
					JobDeclarationFilterBusinessObject.FilterConstants.Shed,
					JobDeclarationFilterBusinessObject.FilterConstants.CSP,
					JobDeclarationFilterBusinessObject.FilterConstants.RouteOfEntry,
					JobDeclarationFilterBusinessObject.FilterConstants.IRC,
					JobDeclarationFilterBusinessObject.FilterConstants.ICS,
					JobDeclarationFilterBusinessObject.FilterConstants.Badge,
					JobDeclarationFilterBusinessObject.FilterConstants.HouseSplitReference,
					JobDeclarationFilterBusinessObject.FilterConstants.VATDeferType,
					JobDeclarationFilterBusinessObject.FilterConstants.VATDeferNumber,
					JobDeclarationFilterBusinessObject.FilterConstants.OtherDeferType,
					JobDeclarationFilterBusinessObject.FilterConstants.OtherDeferNumber,
					JobDeclarationFilterBusinessObject.FilterConstants.SuppDecsDeclaredPackages,
					JobDeclarationFilterBusinessObject.FilterConstants.SuppDecsOutstanding,
					JobDeclarationFilterBusinessObject.FilterConstants.SuppDecsOutstandingPackages,
					JobDeclarationFilterBusinessObject.FilterConstants.EIDRType,
					JobDeclarationFilterBusinessObject.FilterConstants.SupplementaryDeclarationDueDate,
					JobDeclarationFilterBusinessObject.FilterConstants.TaxPoint,
					JobDeclarationFilterBusinessObject.FilterConstants.DateOfExit,
					JobDeclarationFilterBusinessObject.FilterConstants.NorthernIrelandMode,
					JobDeclarationFilterBusinessObject.FilterConstants.EUSubsidy,
					JobDeclarationFilterBusinessObject.FilterConstants.AreGoodsAtRisk,
					JobDeclarationFilterBusinessObject.FilterConstants.ShortLocationOfGoodsForCDS,
					JobDeclarationFilterBusinessObject.FilterConstants.FullLocationOfGoodsForCDS,
					JobDeclarationFilterBusinessObject.FilterConstants.GVMSEnabledLocations,
					JobDeclarationFilterBusinessObject.FilterConstants.HasInventoryReference,
					JobDeclarationFilterBusinessObject.FilterConstants.PrelodgedVersusLodged,
				})
				{
					var filter = stripBO[key];
					AssertEquals(key, filter.LocalizedDescription);
				}
			});
		}

		public void TestSupplementaryDeclarationFilters()
		{
			var parentDec = Factory.New<JobDeclaration>();
			parentDec.JE_MessageType = "IMP";
			parentDec.JE_DeclarationType = "IFD";
			parentDec.JE_EntrySubStyle = "C";
			parentDec.JE_TotalNoOfPacks = 6;
			Factory.Save();
			// This dec shoudl be found when querying those with 0+ declared packs
			var collectionLoaded = LoadSupplementaryFilterTestNumbers("SuppDecs' declared #packages", 0, 9);
			AssertCollectionContains(parentDec, collectionLoaded);
			// Not found when looking for 1+, as none are declared
			collectionLoaded = LoadSupplementaryFilterTestNumbers("SuppDecs' declared #packages", 1, 9);
			AssertCollectionNotContains(parentDec, collectionLoaded);

			// Found when querying for 0+ already declared
			collectionLoaded = LoadSupplementaryFilterTestNumbers("SuppDecs' outstanding #packages", 0, 9);
			AssertCollectionContains(parentDec, collectionLoaded);
			collectionLoaded = LoadSupplementaryFilterTestNumbers("SuppDecs' outstanding #packages", 6, 6);
			AssertCollectionContains(parentDec, collectionLoaded);
			// 6 are still outstanding, so not found when looking for 8-9 and 0-5
			collectionLoaded = LoadSupplementaryFilterTestNumbers("SuppDecs' outstanding #packages", 8, 9);
			AssertCollectionNotContains(parentDec, collectionLoaded);
			collectionLoaded = LoadSupplementaryFilterTestNumbers("SuppDecs' outstanding #packages", 0, 5);
			AssertCollectionNotContains(parentDec, collectionLoaded);

			collectionLoaded = LoadSupplementaryFilterTestHasOutstanding(false);
			AssertCollectionContains(parentDec, collectionLoaded); // Found when asking to see those with outstanding (unticked)
			collectionLoaded = LoadSupplementaryFilterTestHasOutstanding(true);
			AssertCollectionNotContains(parentDec, collectionLoaded);  // Not found wehn asking to see those that are exhaused

			// Make a supp dec for 4 of 6 packs
			var wizardManager = new SuppDecWizardManager(parentDec);
			var presenterForTest = new Customs.Business.Testing.RelatedDeclarationControllerTest.ZFormPresenterForTest();
			var relatedDeclarationHelper = new RelatedDeclarationHelper(presenterForTest);
			wizardManager.SuppDecWizard.DeclarationType = "ISD";
			wizardManager.SuppDecWizard.NumberPackagesToDeclare = 4;
			wizardManager.SuppDecWizard.SupplementaryProcedure = "Y";
			var suppDecOne = wizardManager.CreateAndShowSupplemenaryDeclaration(relatedDeclarationHelper);
			suppDecOne.Factory.Save();

			collectionLoaded = LoadSupplementaryFilterTestNumbers("SuppDecs' declared #packages", 1, 9);
			AssertCollectionContains(parentDec, collectionLoaded);
			collectionLoaded = LoadSupplementaryFilterTestNumbers("SuppDecs' declared #packages", 4, 4);
			AssertCollectionContains(parentDec, collectionLoaded);
			collectionLoaded = LoadSupplementaryFilterTestNumbers("SuppDecs' declared #packages", 5, 5);
			AssertCollectionNotContains(parentDec, collectionLoaded);

			collectionLoaded = LoadSupplementaryFilterTestNumbers("SuppDecs' outstanding #packages", 0, 9);
			AssertCollectionContains(parentDec, collectionLoaded);
			collectionLoaded = LoadSupplementaryFilterTestNumbers("SuppDecs' outstanding #packages", 8, 9);
			AssertCollectionNotContains(parentDec, collectionLoaded);
			collectionLoaded = LoadSupplementaryFilterTestNumbers("SuppDecs' outstanding #packages", 2, 2);
			AssertCollectionContains(parentDec, collectionLoaded);

			collectionLoaded = LoadSupplementaryFilterTestHasOutstanding(false);
			AssertCollectionContains(parentDec, collectionLoaded);
			collectionLoaded = LoadSupplementaryFilterTestHasOutstanding(true);
			AssertCollectionNotContains(parentDec, collectionLoaded);

			// Remaining 2 packs
			wizardManager = new SuppDecWizardManager(parentDec);
			presenterForTest = new Customs.Business.Testing.RelatedDeclarationControllerTest.ZFormPresenterForTest();
			relatedDeclarationHelper = new RelatedDeclarationHelper(presenterForTest);
			wizardManager.SuppDecWizard.DeclarationType = "ISD";
			wizardManager.SuppDecWizard.NumberPackagesToDeclare = 2;
			wizardManager.SuppDecWizard.SupplementaryProcedure = "Y";
			var suppDecTwo = wizardManager.CreateAndShowSupplemenaryDeclaration(relatedDeclarationHelper);
			suppDecTwo.Factory.Save();

			collectionLoaded = LoadSupplementaryFilterTestNumbers("SuppDecs' declared #packages", 6, 6);
			AssertCollectionContains(parentDec, collectionLoaded);

			collectionLoaded = LoadSupplementaryFilterTestNumbers("SuppDecs' outstanding #packages", 0, 0);
			AssertCollectionContains(parentDec, collectionLoaded);
			collectionLoaded = LoadSupplementaryFilterTestNumbers("SuppDecs' outstanding #packages", 1, 9);
			AssertCollectionNotContains(parentDec, collectionLoaded);
			collectionLoaded = LoadSupplementaryFilterTestHasOutstanding(false);
			AssertCollectionNotContains(parentDec, collectionLoaded);
			collectionLoaded = LoadSupplementaryFilterTestHasOutstanding(true);
			AssertCollectionContains(parentDec, collectionLoaded);  // exhausted
		}

		public void TestNorthernIrelandModeFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_NorthernIrelandMode = NIModeList.Codes.MovementFromGreatBritainToNi;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_NorthernIrelandMode = NIModeList.Codes.MovementFromNiToGreatBritain;
			Factory.Save();

			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)stripBO["Northern Ireland Mode"];
			filter.IsActive = true;
			filter.Property = NIModeList.Codes.MovementFromGreatBritainToNi;

			Assert(declaration1.MatchesFilter(stripBO.Filter));
			Assert(!declaration2.MatchesFilter(stripBO.Filter));
		}

		public void TestClaimEuSubsidy()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ClaimEuSubsidy = true;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ClaimEuSubsidy = false;
			Factory.Save();

			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)stripBO["EU Subsidy"];
			filter.IsActive = true;
			filter.Property0 = true;

			Assert(declaration1.MatchesFilter(stripBO.Filter));
			Assert(!declaration2.MatchesFilter(stripBO.Filter));
		}

		public void TestGoodsAtRiskOfMovingToROI()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_NiGoodsAtRiskOfMovingToROI = true;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_NiGoodsAtRiskOfMovingToROI = false;
			Factory.Save();

			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)stripBO["Are Goods At Risk?"];
			filter.IsActive = true;
			filter.Property0 = true;

			Assert(declaration1.MatchesFilter(stripBO.Filter));
			Assert(!declaration2.MatchesFilter(stripBO.Filter));
		}

		JobDeclarationCollection LoadSupplementaryFilterTestNumbers(string numberFilterName, int numberFilterValueLow, int numberFilterValueHigh)
		{
			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var numberFilter = (ModuleNumberRangeFilter)stripBO[numberFilterName];
			numberFilter.Property1 = numberFilterValueLow;
			numberFilter.Property2 = numberFilterValueHigh;
			numberFilter.IsActive = true;
			declarationCollection.Load(stripBO.Filter);
			return declarationCollection;
		}

		JobDeclarationCollection LoadSupplementaryFilterTestHasOutstanding(bool flag)
		{
			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var numberFilter = (ModuleFlagsFilter)stripBO["SuppDecs outstanding"];
			numberFilter.Property0 = flag;
			numberFilter.IsActive = true;
			declarationCollection.Load(stripBO.Filter);
			return declarationCollection;
		}

		public void TestPrelodgedVsLodged()
		{
			var dec1 = Factory.New<JobDeclaration>();
			var cei = dec1.CustomsEntryInstructions.AddNew();
			cei.CEI_SubStyle = EntrySubStyleListExport.Codes.C21_GoodsArrived_IECR;

			var dec2 = Factory.New<JobDeclaration>();
			cei = dec2.CustomsEntryInstructions.AddNew();
			cei.CEI_SubStyle = EntrySubStyleListExport.Codes.FullDeclarationGoodsNotArrived_IEFD;
			cei = dec2.CustomsEntryInstructions.AddNew();
			cei.CEI_SubStyle = EntrySubStyleListExport.Codes.SDP_PSA_GoodsArrived_IESP;

			var dec3 = Factory.New<JobDeclaration>();
			cei = dec3.CustomsEntryInstructions.AddNew();
			cei.CEI_SubStyle = EntrySubStyleListExport.Codes.C21_GoodsNotArrived_IECR;
			cei = dec3.CustomsEntryInstructions.AddNew();
			cei.CEI_SubStyle = EntrySubStyleListExport.Codes.SDP_and_LCP_PSA_GoodsNotArrived_IELP_IESP;
			Factory.Save();

			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)stripBO["Pre-lodged versus lodged"];
			filter.IsActive = true;
			filter.Property0 = true;

			Assert(dec1.MatchesFilter(stripBO.Filter));
			Assert(dec2.MatchesFilter(stripBO.Filter));
			Assert(!dec3.MatchesFilter(stripBO.Filter));

			filter.Property0 = false;
			Assert(dec1.MatchesFilter(stripBO.Filter));
			Assert(dec2.MatchesFilter(stripBO.Filter));
			Assert(dec3.MatchesFilter(stripBO.Filter));
		}

		public void TestIsGVMSEnabled()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_IsGvmsPort = true;
			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_IsGvmsPort = false;
			Factory.Save();

			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)stripBO["GVMS-enabled locations"];
			filter.IsActive = true;
			filter.Property0 = true;

			Assert(dec1.MatchesFilter(stripBO.Filter));
			Assert(!dec2.MatchesFilter(stripBO.Filter));

			filter.Property0 = false;
			Assert(dec1.MatchesFilter(stripBO.Filter));
			Assert(dec2.MatchesFilter(stripBO.Filter));
		}

		public void TestHasInventoryReference()
		{
			var dec1 = Factory.New<JobDeclaration>();
			var cusEntryNumber1 = Factory.New<CusEntryNumber>();
			cusEntryNumber1.CE_ParentID = dec1.PK;
			cusEntryNumber1.CE_ParentTable = JobDeclaration.Schema.TableName;
			cusEntryNumber1.CE_EntryNum = "12344321";
			cusEntryNumber1.CE_EntryType = CusEntryNumberTypes.EU.MasterUCR;

			var dec2 = Factory.New<JobDeclaration>();
			var cusEntryNumber2 = Factory.New<CusEntryNumber>();
			cusEntryNumber2.CE_ParentID = dec2.PK;
			cusEntryNumber2.CE_ParentTable = JobDeclaration.Schema.TableName;
			cusEntryNumber2.CE_EntryNum = "23455432";
			cusEntryNumber2.CE_EntryType = CusEntryNumberTypes.EU.LocalReferenceNumber;
			Factory.Save();

			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)stripBO["Has Inventory Reference"];
			filter.IsActive = true;
			filter.Property0 = true;

			Assert(dec1.MatchesFilter(stripBO.Filter));
			Assert(!dec2.MatchesFilter(stripBO.Filter));

			filter.Property0 = false;
			Assert(dec1.MatchesFilter(stripBO.Filter));
			Assert(dec2.MatchesFilter(stripBO.Filter));
		}

		public void TestInventoryConsignmentReferenceFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MasterUCR = "HBAC5554444444433";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MasterUCR = "HBAC6664444444411";
			Factory.Save();

			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)stripBO["Inventory Consignment Reference (MUCR)"];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "HBAC5";

			AssertEquals("InventoryConsignmentReference Multilingual Description", JobDeclarationFilterBusinessObject.FilterConstants.InventoryConsignmentReference, filter.MultilingualDescription);
			Assert(declaration1.MatchesFilter(stripBO.Filter));
			Assert(!declaration2.MatchesFilter(stripBO.Filter));
		}

		public void TestDUCRJobDeclarationFilterBusinessObject()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_UCR = "DUCR123";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_UCR = "DUCR456";

			Factory.Save();

			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip["DUCR (Declaration Unique Consignment Reference)"];
			AssertNotNull("Filter " + "DUCR (Declaration Unique Consignment Reference)", filter);

			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "DUCR123";

			CombineAssertions(() =>
			{
				AssertEquals("Entry of Declaration1, MatchesFilter", expected: true, declaration1.MatchesFilter(filterStrip.Filter));
				AssertEquals("Entry of Declaration2, MatchesFilter", expected: false, declaration2.MatchesFilter(filterStrip.Filter));
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new JobDeclarationFilterBusinessObject();
	}
}
