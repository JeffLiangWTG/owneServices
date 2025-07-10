using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class JobDeclarationFilterLookupsTest : TestCaseWithFactory
	{
		public void TestB2Types()
		{
			var filterBO = new JobDeclarationFilterBusinessObject();
			AssertEquals(typeof(B2TypeList), filterBO.Lookups.B2Types.GetType());
		}

		public void TestContainerModeList()
		{
			JobDeclarationFilterBusinessObject filterBO = new JobDeclarationFilterBusinessObject();
			CodeDescriptionPairList list = filterBO.Lookups.ContainerModeList;
			AssertEquals(4, list.Count);
			AssertContainerMode(list[0], Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
			AssertContainerMode(list[1], Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk);
			AssertContainerMode(list[2], Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModeDescriptions.Bulk);
			AssertContainerMode(list[3], Core.Constants.ContainerModes.Liquid, Core.Constants.ContainerModeDescriptions.Liquid);
		}

		public void TestCBSAOffices()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1111", "CA Customs Office Code", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1111", "US Customs Office Code", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "1111", "CA Customs Port Code", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1234", "CA Customs Office Code (Expired)", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(-1));
			Factory.Save();
			var filterBO = new JobDeclarationFilterBusinessObject();
			var list = filterBO.Lookups.CBSAOffices;
			list.Load();

			AssertEquals(1, list.Count);
			AssertType<ZZRefCusCodeListCombinedCollection>(list);
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "1111"));
			AssertEquals("CA Customs Office Code", list.Cast<ZZRefCusCodeListCombined>().FirstOrDefault(x => x.ZZD_Code == "1111").ZZD_Description);
			Assert(!list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "1234"));
		}

		public void TestTypes()
		{
			JobDeclarationFilterBusinessObject filterBO = new JobDeclarationFilterBusinessObject();
			AssertEquals(typeof(JobMessageTypeList), filterBO.Lookups.MessageTypeList.GetType());
			AssertEquals(typeof(CodeDescriptionPairList), filterBO.Lookups.ReleaseStatusList().GetType());
			AssertEquals(typeof(B3EntryTypeList), filterBO.Lookups.MessageSubTypeList().GetType());
			AssertEquals(typeof(TransportTypeList), filterBO.Lookups.TransportTypeList.GetType());
			AssertEquals(typeof(ZZRefCarrierCombinedCollection), filterBO.Lookups.CarrierCodes.GetType());
			AssertEquals(typeof(CACSubLocationCollection), filterBO.Lookups.SubLocationCodes.GetType());
			AssertEquals(typeof(ACROSSServiceOptions), filterBO.Lookups.ServiceOptions.GetType());
			AssertEquals(typeof(CodeDescriptionPairList), filterBO.Lookups.EntryStatusList().GetType());
			AssertEquals(typeof(CAInitiatedByList), filterBO.Lookups.CAInitiatedByList.GetType());
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), filterBO.Lookups.CBSAOffices.GetType());
		}

		public void TestReleaseStatusList()
		{
			var filterBO = new JobDeclarationFilterBusinessObject();
			var entryStatusList = filterBO.Lookups.ReleaseStatusList();
			AssertCollectionNotContains(ZString.Empty, entryStatusList.GetAllCodes());
			AssertEquals("Release Canceled", entryStatusList.GetDescriptionFromCode(CA.Business.EDIReleaseImportEntryStatusList.Codes.Cancelled));

			var cancelledList = entryStatusList.GetAllCodes().Where(x => x == Common.CA.EDIReleaseImportEntryStatusList.Codes.Cancelled);
			AssertEquals(1, cancelledList.Count());
		}

		public void TestMessageStatusList()
		{
			var filterBO = new JobDeclarationFilterBusinessObject();
			var list = filterBO.Lookups.MessageStatusList();
			AssertCollectionNotContains(ZString.Empty, list.GetAllCodes());
			AssertEquals("Not Sent", list.GetDescriptionFromCode(DeclarationFilterConstants.EntryStatus.NotSentForFilter));
		}

		public void TestMessageStatusListForFilter()
		{
			var filterBO = new JobDeclarationFilterBusinessObject();
			var list2 = filterBO.Lookups.MessageStatusListForFilter(MessageTypeList.Codes.G7Export);
			AssertCollectionNotContains(ZString.Empty, list2.GetAllCodes());
			AssertEquals("Not Sent", list2.GetDescriptionFromCode(DeclarationFilterConstants.EntryStatus.NotSentForFilter));
			AssertEquals("Awaiting G7 Export Message Amendment", list2.GetDescriptionFromCode(MessageStatusList.Codes.AwaitingReplace));
			AssertEquals("Acknowledged G7 Export Message Amendment", list2.GetDescriptionFromCode(MessageStatusList.Codes.AcknowledgedReplace));
			AssertEquals("Awaiting G7 Export Message Original", list2.GetDescriptionFromCode(MessageStatusList.Codes.AwaitingOriginal));

			var list3 = filterBO.Lookups.MessageStatusListForFilter(null);
			AssertCollectionNotContains(ZString.Empty, list3.GetAllCodes());
			AssertEquals("Not Sent", list3.GetDescriptionFromCode(DeclarationFilterConstants.EntryStatus.NotSentForFilter));
			AssertEquals("Awaiting Amendment", list3.GetDescriptionFromCode(MessageStatusList.Codes.AwaitingReplace));
			AssertEquals("Acknowledged Amendment", list3.GetDescriptionFromCode(MessageStatusList.Codes.AcknowledgedReplace));
			AssertEquals("Awaiting Original", list3.GetDescriptionFromCode(MessageStatusList.Codes.AwaitingOriginal));

			var list4 = filterBO.Lookups.MessageStatusListForFilter(MessageTypeList.Codes.EDIRelease);
			AssertCollectionNotContains(ZString.Empty, list4.GetAllCodes());
			AssertEquals("Not Sent", list4.GetDescriptionFromCode(DeclarationFilterConstants.EntryStatus.NotSentForFilter));
			AssertEquals("Awaiting Across/IID EDI Release Amendment", list4.GetDescriptionFromCode(MessageStatusList.Codes.AwaitingReplace));
			AssertEquals("Acknowledged Across/IID EDI Release Amendment", list4.GetDescriptionFromCode(MessageStatusList.Codes.AcknowledgedReplace));
			AssertEquals("Awaiting Across/IID EDI Release Original", list4.GetDescriptionFromCode(MessageStatusList.Codes.AwaitingOriginal));
		}

		public void TestOrganisationList()
		{
			JobDeclarationFilterBusinessObject filterBO = new JobDeclarationFilterBusinessObject();
			OrganisationsFindBoxCollection collection = filterBO.Lookups.OrganisationList;
			AssertNotNull(collection);
			AssertEquals("Should have been cached", collection, filterBO.Lookups.OrganisationList);
		}

		public void TestJobDocAddressList()
		{
			JobDeclarationFilterBusinessObject filterBO = new JobDeclarationFilterBusinessObject();
			DeclarationJobDocAddressCollection collection = filterBO.Lookups.JobDocAddressList;
			AssertNotNull(collection);
			AssertEquals("Should have been cached", collection, filterBO.Lookups.JobDocAddressList);
		}

		public void TestMessageSubTypeList()
		{
			var filterBO = new JobDeclarationFilterBusinessObject();
			var pairList = filterBO.Lookups.MessageSubTypeList();
			AssertEquals(32, pairList.Count);
			Assert("Does not contain F", !pairList.ContainsCode(B3EntryTypeList.Codes.LowValueShipments));
			Assert("Does contain MSI", pairList.ContainsCode(LowValueShipmentsTypes.Codes.ConsolidationByImporter));

			Assert(pairList.ContainsCode(CADEntryTypeList.Codes.Warehouse101));
			Assert(pairList.ContainsCode(CADEntryTypeList.Codes.Warehouse102));
			Assert(pairList.ContainsCode(CADEntryTypeList.Codes.ReWarehouse131));
			Assert(pairList.ContainsCode(CADEntryTypeList.Codes.ReWarehouse132));
			Assert(pairList.ContainsCode(CADEntryTypeList.Codes.ExWarehouse201));
			Assert(pairList.ContainsCode(CADEntryTypeList.Codes.ExWarehouse211));
			Assert(pairList.ContainsCode(CADEntryTypeList.Codes.ExWarehouse212));
			Assert(pairList.ContainsCode(CADEntryTypeList.Codes.ExWarehouse213));
			Assert(pairList.ContainsCode(CADEntryTypeList.Codes.ExWarehouse214));
			Assert(pairList.ContainsCode(CADEntryTypeList.Codes.ExWarehouse215));
			Assert(pairList.ContainsCode(CADEntryTypeList.Codes.ExWarehouse216));
			Assert(pairList.ContainsCode(CADEntryTypeList.Codes.TransferOfGoods301));
			Assert(pairList.ContainsCode(CADEntryTypeList.Codes.TransferOfGoods302));
		}

		public void TestMessageTypeList()
		{
			var filterBO = new JobDeclarationFilterBusinessObject();
			var pairList = filterBO.Lookups.MessageTypeList;
			AssertEquals(8, pairList.Count);
			Assert("Contain LVX (for Reports)", pairList.ContainsCode(JobMessageTypeList.Codes.LVSForConsolidation));
			Assert("Contain IM2 (for Reports)", pairList.ContainsCode(JobMessageTypeList.Codes.ImportCopyforB2));
			Assert("Contain B3X (for Reports)", pairList.ContainsCode(JobMessageTypeList.Codes.XTypeEntry));
			var list2 = filterBO.Lookups.MessageTypeList;
			AssertEquals("Should be cached", true, object.ReferenceEquals(pairList, list2));
		}

		public void TestCAInitiatedByList()
		{
			JobDeclarationFilterBusinessObject filterBO = new JobDeclarationFilterBusinessObject();
			var pairList = filterBO.Lookups.CAInitiatedByList;
			AssertEquals(4, pairList.Count);
		}

		public void TestExceptionCodesList()
		{
			JobDeclarationFilterBusinessObject filterBO = new JobDeclarationFilterBusinessObject();
			var pairList = filterBO.Lookups.ExceptionCodes;
			AssertEquals(7, pairList.Count);
		}

		public void TestTransportTypeList()
		{
			var filterBO = new JobDeclarationFilterBusinessObject();
			var pairList = filterBO.Lookups.TransportTypeList;
			AssertEquals("AIR, IWT, FIX, RAI, ROA, SEA, MAI, NOC", pairList.CodesAsString);
		}

		void AssertContainerMode(ICodeDescription codeDescription, string code, string description)
		{
			AssertEquals("Code", code, codeDescription.Code);
			AssertEquals("Description", description, codeDescription.Description);
		}

		public void TestBondTypeList()
		{
			var originalList = new BondTypeList();
			var filterBO = new JobDeclarationFilterBusinessObject();
			var bondTypeList = filterBO.Lookups.BondTypeList;
			AssertEquals(originalList, bondTypeList);

			var list2 = Factory.GetCachedValue<BondTypeList>();
			AssertEquals("Should have been cached", bondTypeList, list2);
		}
	}
}
