using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	class JobDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAdditionalReferenceNumbers()
		{
			var dec = Factory.New<JobDeclaration>();
			var additionalReferenceNumber = dec.AdditionalReferenceNumbers.AddNew();
			var arnTypes = additionalReferenceNumber.Lookups.AdditionalReferenceNumberTypes;
			AssertListContains(arnTypes, "PSL", "建议书编号");
			AssertListContains(arnTypes, "WGQ", "外高桥进出库单号");
			AssertListContains(arnTypes, "DTD", "转关申报单预录入号");
			AssertListContains(arnTypes, "GCL", "载货清单号");
			Assert("Additional Reference number types should be untranslatable", arnTypes is UntranslatableCodeDescriptionPairList);
		}

		public void TestPortList()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, dec.CountryCode));
			var foreignPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, dec.CountryCode));
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			dec.JE_MessageSubType = DecTypeList.Codes.Both;
			var filter = dec.Lookups.FinalDestinations.CompleteFilter;
			AssertEquals(true, localPort.MatchesFilter(filter));
			AssertEquals(false, foreignPort.MatchesFilter(filter));
			filter = dec.Lookups.DestinationList.CompleteFilter;
			AssertEquals(true, localPort.MatchesFilter(filter));
			AssertEquals(false, foreignPort.MatchesFilter(filter));
			filter = dec.Lookups.Origins.CompleteFilter;
			AssertEquals(true, localPort.MatchesFilter(filter));
			AssertEquals(false, foreignPort.MatchesFilter(filter));
			filter = dec.Lookups.PortOfArrivals.CompleteFilter;
			AssertEquals(true, localPort.MatchesFilter(filter));
			AssertEquals(false, foreignPort.MatchesFilter(filter));
			dec.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			filter = dec.Lookups.FinalDestinations.CompleteFilter;
			AssertEquals(false, localPort.MatchesFilter(filter));
			AssertEquals(true, foreignPort.MatchesFilter(filter));
			filter = dec.Lookups.DestinationList.CompleteFilter;
			AssertEquals(false, localPort.MatchesFilter(filter));
			AssertEquals(true, foreignPort.MatchesFilter(filter));
			filter = dec.Lookups.Origins.CompleteFilter;
			AssertEquals(true, localPort.MatchesFilter(filter));
			AssertEquals(false, foreignPort.MatchesFilter(filter));
			filter = dec.Lookups.PortOfArrivals.CompleteFilter;
			AssertEquals(false, localPort.MatchesFilter(filter));
			AssertEquals(true, foreignPort.MatchesFilter(filter));
		}

		public void TestPorts()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(typeof(RefUNLOCOCollection), declaration.Lookups.Ports.GetType());
		}

		public void TestDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(declaration.Lookups.Declaration, declaration);
		}

		public void TestEntryStatusList()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(typeof(Common.CN.EntryStatusList), declaration.Lookups.EntryStatusList.GetType());
		}

		public void TestMessageSubTypeList()
		{
			using (CNCustomsDataRegistry.Instance.CNBTHFunctionActive.SetTemporaryValue(new System.Guid(), new System.Guid(), new System.Guid(), true))
			{
				var declaration1 = Factory.New<JobDeclaration>();
				declaration1.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var list = declaration1.Lookups.MessageSubTypeList;
				AssertEquals(3, list.Count);
				AssertListContains(list, DecTypeList.Codes.Both, "进口报关单+出境备案清单");
				AssertListContains(list, DecTypeList.Codes.CustomsEntry, "进口报关单");
				AssertListContains(list, DecTypeList.Codes.RecordListing, "进境备案清单");
				declaration1.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				list = declaration1.Lookups.MessageSubTypeList;
				AssertListContains(list, DecTypeList.Codes.Both, "进境备案清单+出口报关单");
				AssertListContains(list, DecTypeList.Codes.CustomsEntry, "出口报关单");
				AssertListContains(list, DecTypeList.Codes.RecordListing, "出境备案清单");
				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertSame("Should have been cached", declaration1.Lookups.MessageSubTypeList, declaration2.Lookups.MessageSubTypeList);
			}

			using (CNCustomsDataRegistry.Instance.CNBTHFunctionActive.SetTemporaryValue(new System.Guid(), new System.Guid(), new System.Guid(), false))
			{
				var declaration1 = new BusinessObjectFactory().New<JobDeclaration>();
				declaration1.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var list = declaration1.Lookups.MessageSubTypeList;
				AssertEquals(2, list.Count);
				AssertListContains(list, DecTypeList.Codes.CustomsEntry, "进口报关单");
				AssertListContains(list, DecTypeList.Codes.RecordListing, "进境备案清单");
				declaration1.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				list = declaration1.Lookups.MessageSubTypeList;
				AssertEquals(2, list.Count);
				AssertListContains(list, DecTypeList.Codes.CustomsEntry, "出口报关单");
				AssertListContains(list, DecTypeList.Codes.RecordListing, "出境备案清单");
			}
		}

		[TestDate(2017, 12, 26)]
		public void TestCustomsOfficeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSOF", "CNJ", "Nanjing Office", new ZDateTime(2017, 12, 25), new ZDateTime(2017, 12, 30));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSOF", "CSH", "Shanghai Office", new ZDateTime(2017, 12, 25), new ZDateTime(2017, 12, 25));
			helper.CreateNewOrGetExistingCusCodeList("US", "CUSOF", "CNY", "New York Office", new ZDateTime(2017, 12, 25), new ZDateTime(2017, 12, 30));
			Factory.Save();
			var dec = Factory.New<JobDeclaration>();
			var offices = dec.Lookups.CustomsOfficeList;
			offices.Load();
			AssertEquals(1, offices.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "CNJ" }, offices.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
			var filter = offices.FilterBusinessObjectDefaults["Description:Property"];
			AssertNotNull(filter);
		}

		public void TestTransportTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertContainsExactElementsInAnyOrder(new[] { TransportTypeList.Codes.Air, TransportTypeList.Codes.Sea, TransportTypeList.Codes.Road, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Mail, TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.PassengerCarried }, declaration.Lookups.TransportTypeList.GetAllCodes());
		}

		public void TestCargoIdTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = "AIR";
			AssertContainsExactElementsInAnyOrder(new[] { Core.Constants.ContainerModes.Containerised, }, declaration.Lookups.CargoIdTypeList.GetAllCodes());
			declaration.JE_TransportMode = "MAI";
			AssertContainsExactElementsInAnyOrder(new[] { Core.Constants.ContainerModes.Containerised, }, declaration.Lookups.CargoIdTypeList.GetAllCodes());
			declaration.JE_TransportMode = "SEA";
			AssertContainsExactElementsInAnyOrder(new[] { Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModes.Liquid }, declaration.Lookups.CargoIdTypeList.GetAllCodes());
		}

		public void TestMergeByList()
		{
			var testList = Factory.New<JobDeclaration>().Lookups.MergeByList;
			AssertEquals("Should have 6 items", 6, testList.Count);
			Assert("Should contain code: " + OrgConstants.MergeInvoiceLines.NotMerge, testList.ContainsCode(OrgConstants.MergeInvoiceLines.NotMerge));
			Assert("Should contain code: " + OrgConstants.MergeInvoiceLines.NotMergeUsingProductNumberInDescription, testList.ContainsCode(OrgConstants.MergeInvoiceLines.NotMergeUsingProductNumberInDescription));
			Assert("Should contain code: " + OrgConstants.MergeInvoiceLines.Tariff, testList.ContainsCode(OrgConstants.MergeInvoiceLines.Tariff));
			Assert("Should contain code: " + OrgConstants.MergeInvoiceLines.TariffAndDescription, testList.ContainsCode(OrgConstants.MergeInvoiceLines.TariffAndDescription));
			Assert("Should contain code: " + OrgConstants.MergeInvoiceLines.PartNumber, testList.ContainsCode(OrgConstants.MergeInvoiceLines.PartNumber));
			Assert("Should contain code: " + OrgConstants.MergeInvoiceLines.PartNumberUsingProductNumberInDescription, testList.ContainsCode(OrgConstants.MergeInvoiceLines.PartNumberUsingProductNumberInDescription));
		}

		public void TestTransportModeInlandList()
		{
			var list = Factory.New<JobDeclaration>().Lookups.TransportModeInlandList;
			AssertContainsExactElementsInAnyOrder(new[] { "IWT", "ROA", "RAI" }, list.GetAllCodes());
			AssertEquals("Waterway Transport", list.GetDescriptionFromCode("IWT"));
			AssertEquals("Road Transport", list.GetDescriptionFromCode("ROA"));
			AssertEquals("Rail Transport", list.GetDescriptionFromCode("RAI"));
			AssertSame("Should be cached.", list, Factory.GetCachedValue<CodeDescriptionPairList>("CNJobDeclarationLookupsTransportModeInlandList", () => null));
		}

		public void TestMessageStatusList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var testList = Factory.GetCachedValue<JobMessageStatusList>();
			AssertSame(testList, declaration.Lookups.MessageStatusList);
		}

		public void TestCNTransportModeCodes()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("CNTransportModeCodes.Count should be 12", 12, declaration.Lookups.CNTransportModeCodes.Count);
			AssertContainsExactElementsInAnyOrder(new[] { CNTransportModeList.Codes.NonBondedArea, CNTransportModeList.Codes.SupervisedWarehouse, CNTransportModeList.Codes.BondedArea, CNTransportModeList.Codes.BondedWarehouse, CNTransportModeList.Codes.Others, CNTransportModeList.Codes.CrossBorder, CNTransportModeList.Codes.Comprehensive, CNTransportModeList.Codes.LogisticCenter, CNTransportModeList.Codes.LogisticPark, CNTransportModeList.Codes.BondedPort, CNTransportModeList.Codes.ExportProcessing, CNTransportModeList.Codes.YangpuBondedPort, }, (declaration.Lookups.CNTransportModeCodes as CodeDescriptionPairList).GetAllCodes());
		}

		public void TestClearanceModeCodes()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("ClearanceModeCodes.Count should be 4", 4, declaration.Lookups.ClearanceModeCodes.Count);
			AssertContainsExactElementsInAnyOrder(new[] { ClearanceModeList.Codes.Integrated, ClearanceModeList.Codes.TwoStep, ClearanceModeList.Codes.TwoStepManual, ClearanceModeList.Codes.TwoStepAuto }, ((CodeDescriptionPairList)declaration.Lookups.ClearanceModeCodes).GetAllCodes());
		}

		public void TestTransitModeCodes()
		{
			var transitModeCodes = Factory.New<JobDeclaration>().Lookups.TransitModeCodes;
			AssertEquals("transitModeCodes.Count should be 3", 3, transitModeCodes.Count);
			AssertContainsExactElementsInAnyOrder(new[] { TransitModeList.Codes.DeclaringInAdvance, TransitModeList.Codes.DirectTransition, TransitModeList.Codes.Transshipment, }, ((CodeDescriptionPairList)transitModeCodes).GetAllCodes());
			AssertSame(Factory.GetCachedValue<TransitModeList>(), transitModeCodes);
		}

		static void AssertListContains(CodeDescriptionPairList list, string code, string description)
		{
			Assert(list.ToArray().Any(pair => pair.Code == code && pair.Description == description));
		}
	}
}
