using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Customs.Forwarding.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs.US;

namespace Enterprise.Customs.Forwarding.Module.Testing
{
	class ForwardingConsolModuleCustomsFiltersProviderTest : TestCaseWithFactory
	{
		public void TestLatestAMSDisposition()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, "AMSDD", dataGrouping.ZZZ_DataGrouping);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "3U", "3U DESC", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "3Z", "3Z DESC", startDate, endDate);
			Factory.Save();
			var consol1 = Factory.New<ForwardingConsol>();
			var header1PK = CreateCusInBondHeader(Factory, consol1.PK, CusInBondApplicationCodeList.Codes.AMS);
			var header1Bill = CreateCusInBondBill(Factory, header1PK, ZString.Empty);
			CreateDispositionCode(Factory, header1Bill, "3U", new ZDateTime(2012, 4, 1));
			CreateDispositionCode(Factory, header1Bill, "3Z", new ZDateTime(2011, 4, 1));
			var consol2 = Factory.New<ForwardingConsol>();
			var header2PK = CreateCusInBondHeader(Factory, consol2.PK, CusInBondApplicationCodeList.Codes.AMS);
			var header2Bill1 = CreateCusInBondBill(Factory, header2PK, ZString.Empty);
			CreateDispositionCode(Factory, header2Bill1, "3U", new ZDateTime(2012, 4, 1));
			var header2Bill2 = CreateCusInBondBill(Factory, header2PK, ZString.Empty);
			CreateDispositionCode(Factory, header2Bill2, "3Z", new ZDateTime(2012, 4, 1));
			var consol3 = Factory.New<ForwardingConsol>();
			var header3PK = CreateCusInBondHeader(Factory, consol3.PK, CusInBondApplicationCodeList.Codes.AMS);
			var header3Bill = CreateCusInBondBill(Factory, header3PK, ZString.Empty);
			CreateDispositionCode(Factory, header3Bill, "3Z", new ZDateTime(2012, 4, 1));
			var consol4 = Factory.New<ForwardingConsol>();
			var header4PK = CreateCusInBondHeader(Factory, consol4.PK, CusInBondApplicationCodeList.Codes.AMS);
			var header4Bill = CreateCusInBondBill(Factory, header4PK, ZString.Empty);
			var consol5 = Factory.New<ForwardingConsol>();
			var header5PK = CreateCusInBondHeader(Factory, consol5.PK, CusInBondApplicationCodeList.Codes.AMS);
			var header5Bill = CreateCusInBondBill(Factory, header5PK, "OBT");
			CreateDispositionCode(Factory, header5Bill, "3U", new ZDateTime(2012, 4, 1));
			var consol6 = Factory.New<ForwardingConsol>();
			var header6PK = CreateCusInBondHeader(Factory, consol6.PK, CusInBondApplicationCodeList.Codes.InBond);
			var header6Bill = CreateCusInBondBill(Factory, header6PK, ZString.Empty);
			CreateDispositionCode(Factory, header6Bill, "3U", new ZDateTime(2012, 4, 1));
			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleNkFilter)filterObj[ForwardingConsolModuleCustomsFiltersProvider.Descriptions.LatestAMSDisposition];
			filter.IsActive = true;
			filter.Property = "3U";
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will be matched", consol1.MatchesFilter(filterObj.Filter));
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will be matched", consol2.MatchesFilter(filterObj.Filter));
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will be matched", !consol3.MatchesFilter(filterObj.Filter));
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will be matched", !consol4.MatchesFilter(filterObj.Filter));
			Assert($"Except OceanBill", !consol5.MatchesFilter(filterObj.Filter));
			Assert($"Only AMS will be filtered", !consol6.MatchesFilter(filterObj.Filter));

			filter.Property = "3Z";
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will be matched", !consol1.MatchesFilter(filterObj.Filter));
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will be matched", consol2.MatchesFilter(filterObj.Filter));
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will be matched", consol3.MatchesFilter(filterObj.Filter));
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will be matched", !consol4.MatchesFilter(filterObj.Filter));

			ZGuid CreateCusInBondHeader(BusinessObjectFactory factory, ZGuid parentID, ZString applicationCode)
			{
				var header = factory.New<USAMS.ICusInBondHeader>();
				header.BH_ParentID = parentID;
				header.BH_ApplicationCode = applicationCode;
				return header.PK;
			}

			ZGuid CreateCusInBondBill(BusinessObjectFactory factory, ZGuid parentID, ZString shipmentType)
			{
				var bill = factory.New<USAMS.ICusInBondBill>();
				bill.B0_BH = parentID;
				bill.B0_ShipmentType = shipmentType;
				return bill.PK;
			}

			void CreateDispositionCode(BusinessObjectFactory factory, ZGuid parentID, ZString code, ZDateTime dispositionDate)
			{
				var dispositionCode = factory.New<IDispositionData>();
				dispositionCode.B7_ParentID = parentID;
				dispositionCode.B7_ParentTableCode = "B0";
				dispositionCode.US_Code = code;
				dispositionCode.US_DispositionDate = dispositionDate;
			}
		}

		public void TestAsycudaFilters()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var manifest1 = (BusinessObject)Factory.New<Integration.Customs.ASYCUDA.IAsycudaManifestHeader>();
			manifest1[AsycudaManifestHeaderSchema.AMA_ParentId] = consol1.PK;
			manifest1[AsycudaManifestHeaderSchema.AMA_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			manifest1.FillWithValidTestData();
			manifest1[AsycudaManifestHeaderSchema.AMA_JobReference] = "1";
			var asycudaRegistration1 = CusEntryNumber.LoadOrCreate(manifest1, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.Eritrea);
			asycudaRegistration1.CE_EntryNum = "ABC";
			asycudaRegistration1.CE_EntryStatus = Common.Shared.AsycudaRegistrationStatuses.Codes.Registered;
			asycudaRegistration1.CE_IssueDate = ZDateTime.BrettsBirthday;
			var consol2 = Factory.New<ForwardingConsol>();
			var manifest2 = (BusinessObject)Factory.New<Integration.Customs.ASYCUDA.IAsycudaManifestHeader>();
			manifest2[AsycudaManifestHeaderSchema.AMA_ParentId] = consol2.PK;
			manifest2[AsycudaManifestHeaderSchema.AMA_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			manifest2.FillWithValidTestData();
			manifest2[AsycudaManifestHeaderSchema.AMA_JobReference] = "2";
			var asycudaRegistration2 = CusEntryNumber.LoadOrCreate(manifest2, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.Eritrea);
			asycudaRegistration2.CE_EntryNum = "XYZ";
			asycudaRegistration2.CE_EntryStatus = Common.Shared.AsycudaRegistrationStatuses.Codes.Stored;
			asycudaRegistration2.CE_IssueDate = ZDateTime.BrettsBirthday.AddDays(1);
			var consol3 = Factory.New<ForwardingConsol>();
			var manifest3 = (BusinessObject)Factory.New<Integration.Customs.ASYCUDA.IAsycudaManifestHeader>();
			manifest3[AsycudaManifestHeaderSchema.AMA_ParentId] = consol3.PK;
			manifest3[AsycudaManifestHeaderSchema.AMA_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			manifest3.FillWithValidTestData();
			manifest3[AsycudaManifestHeaderSchema.AMA_JobReference] = "3";
			var asycudaRegistration3 = CusEntryNumber.LoadOrCreate(manifest3, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.Eritrea);
			asycudaRegistration3.CE_EntryNum = "XYZ";
			asycudaRegistration3.CE_EntryStatus = Common.Shared.AsycudaRegistrationStatuses.Codes.NotSent;
			asycudaRegistration3.CE_IssueDate = ZDateTime.BrettsBirthday.AddDays(10);
			var consol4 = Factory.New<ForwardingConsol>();
			var manifest4 = (BusinessObject)Factory.New<Integration.Customs.ZA.IAsycudaManifestHeader>();
			manifest4[AsycudaManifestHeaderSchema.AMA_ParentId] = consol4.PK;
			manifest4[AsycudaManifestHeaderSchema.AMA_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			manifest4.FillWithValidTestData();
			manifest4[AsycudaManifestHeaderSchema.AMA_JobReference] = "4";
			var asycudaRegistration4 = CusEntryNumber.LoadOrCreate(manifest4, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.Eritrea);
			asycudaRegistration4.CE_EntryNum = "XYZ";
			asycudaRegistration4.CE_EntryStatus = Common.Shared.AsycudaRegistrationStatuses.Codes.NotSent;
			asycudaRegistration4.CE_IssueDate = ZDateTime.BrettsBirthday.AddDays(10);
			Factory.Save();
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filterDate = (ModuleDateFilter)filterStripBizObj[ForwardingConsolModuleCustomsFiltersProvider.Descriptions.AsycudaRegistrationDate];
			var filterNumber = (ModuleTextFilter)filterStripBizObj[ForwardingConsolModuleCustomsFiltersProvider.Descriptions.AsycudaRegistrationNumber];
			var filterStatus = (ModuleTextFilter)filterStripBizObj[ForwardingConsolModuleCustomsFiltersProvider.Descriptions.AsycudaRegistrationStatus];
			filterDate.IsActive = true;
			filterDate.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filterDate.Property1 = ZDateTime.BrettsBirthday.AddDays(-1);
			filterDate.Property2 = ZDateTime.BrettsBirthday.AddDays(2);
			var coll = Factory.Load<ForwardingConsol>(filterStripBizObj.Filter);
			AssertCollectionContains(consol1, coll);
			AssertCollectionContains(consol2, coll);
			AssertCollectionNotContains(consol3, coll);
			AssertCollectionNotContains(consol4, coll);
			filterDate.IsActive = false;
			filterNumber.IsActive = true;
			filterNumber.Property = "XYZ";
			coll = Factory.Load<ForwardingConsol>(filterStripBizObj.Filter);
			AssertCollectionNotContains(consol1, coll);
			AssertCollectionContains(consol2, coll);
			AssertCollectionContains(consol3, coll);
			AssertCollectionNotContains(consol4, coll);
			filterNumber.IsActive = false;
			filterStatus.IsActive = true;
			filterStatus.Property = Common.Shared.AsycudaRegistrationStatuses.Codes.Stored;
			coll = Factory.Load<ForwardingConsol>(filterStripBizObj.Filter);
			AssertCollectionNotContains(consol1, coll);
			AssertCollectionContains(consol2, coll);
			AssertCollectionNotContains(consol3, coll);
			AssertCollectionNotContains(consol4, coll);
		}

		public void TestAsycudaRegistrationStatus_List()
		{
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filterStatus = (ModuleTextFilter)filterStripBizObj[ForwardingConsolModuleCustomsFiltersProvider.Descriptions.AsycudaRegistrationStatus];

			var filterStatusList = (CodeDescriptionPairList)filterStatus.List;

			AssertContainsExactElementsInExactOrder(new[] { "ACP", "ADD", "AEO", "ARV", "ASC", "CAN", "CNR", "DNL", "HRC", "INS", "NCN", "NOT", "PND", "RAI", "RAR", "REG", "RHR", "RIR", "STO", "VAL" }, filterStatusList.GetAllCodes());
		}

		public void TestOutwardReportNotActiveForNonNZCompanies()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var consolFilter = (ModuleTextFilter)filterStripBizObj["Outward Report Status"];
			AssertNull("Outward Report Status should only be available in NZ", consolFilter);
		}

		public void TestOutwardReportStatusIsActiveForNZ()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var consolFilter = (ModuleTextFilter)filterStripBizObj["Outward Report Status"];
			AssertNotNull("Outward Report Status should only be available in NZ", consolFilter);
		}

		public void TestOutwardReportStatus()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			var consol1 = NewConsol("C00001111", true, "NZAKL", "SGSIN");
			consol1.JK_TransportMode = Core.Constants.TransportModes.Air;
			var consol1EntryNumber = Factory.New<CusEntryNumber>();
			consol1EntryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			consol1EntryNumber.CE_EntryNum = "48830234";
			consol1EntryNumber.CE_ParentID = consol1.PK;
			consol1EntryNumber.CE_ParentTable = consol1.TableName;
			consol1EntryNumber.CE_EntryStatus = OutwardReportStatusList.Codes.Cleared;
			var consol2 = NewConsol("C00001112", true, "AUBNE", "NZAKL");
			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;
			var consol3 = NewConsol("C00001113", false, "NZWLG", "AUSYD");
			consol3.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var consol4 = NewConsol("C00001114", false, "NZAKL", "AUSYD");
			consol4.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var consol4EntryNumber = Factory.New<CusEntryNumber>();
			consol4EntryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			consol4EntryNumber.CE_EntryNum = "47382902";
			consol4EntryNumber.CE_ParentID = consol4.PK;
			consol4EntryNumber.CE_ParentTable = consol4.TableName;
			consol4EntryNumber.CE_EntryStatus = OutwardReportStatusList.Codes.Cleared;
			var consol5 = NewConsol("C00001115", false, "SGSIN", "NZAKL");
			consol5.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var consol6 = NewConsol("C00001116", false, "NZAKL", "AUSYD");
			consol6.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var consol6EntryNumber = Factory.New<CusEntryNumber>();
			consol6EntryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			consol6EntryNumber.CE_ParentID = consol6.PK;
			consol6EntryNumber.CE_ParentTable = consol6.TableName;
			consol6EntryNumber.CE_EntryStatus = OutwardReportStatusList.Codes.Rejected;
			var consol7 = NewConsol("C00001117", false, "NZAKL", "AUMEL");
			consol7.JK_TransportMode = Core.Constants.TransportModes.Air;
			var consol7EntryNumber = Factory.New<CusEntryNumber>();
			consol7EntryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			consol7EntryNumber.CE_ParentID = consol7.PK;
			consol7EntryNumber.CE_ParentTable = consol7.TableName;
			consol7EntryNumber.CE_EntryStatus = OutwardReportStatusList.Codes.Cancelled;
			Factory.Save();
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var outwardReportStatusFilter = (ModuleTextFilter)filterStripBizObj["Outward Report Status"];
			var results = new MainFormConsolCollection(Factory);
			outwardReportStatusFilter.IsActive = true;
			AssertEquals("Should only be 1 matching option", 1, outwardReportStatusFilter.ComparisonOperator_List.Count);
			outwardReportStatusFilter.Property = OutwardReportStatusList.Codes.Cleared;
			results.Load(filterStripBizObj.Filter);
			AssertEquals("Should have found consol1", true, results.Contains(consol1));
			AssertEquals("consol2 is an import consol, should not have been found in this filter", false, results.Contains(consol2));
			AssertEquals("consol3 has not been sent, should not have been found in this filter", false, results.Contains(consol3));
			AssertEquals("Should have found consol4", true, results.Contains(consol4));
			AssertEquals("consol5 is an import consol, should not have been found in this filter", false, results.Contains(consol5));
			AssertEquals("consol6 has a rejection status, should not have been found in this filter", false, results.Contains(consol6));
			AssertEquals("consol7 has a cancelled status, should not have been found in this filter", false, results.Contains(consol7));
			outwardReportStatusFilter.Property = OutwardReportStatusList.Codes.Rejected;
			results.Load(filterStripBizObj.Filter);
			AssertEquals("consol1 has a cleared status, should not have been found in this filter", false, results.Contains(consol1));
			AssertEquals("consol2 is an import consol, should not have been found in this filter", false, results.Contains(consol2));
			AssertEquals("consol3 has not been sent, should not have been found in this filter", false, results.Contains(consol3));
			AssertEquals("consol4 has a cleared status, should not have been found in this filter", false, results.Contains(consol4));
			AssertEquals("consol5 is an import consol, should not have been found in this filter", false, results.Contains(consol5));
			AssertEquals("Should have found consol6", true, results.Contains(consol6));
			AssertEquals("consol7 has a cancelled status, should not have been found in this filter", false, results.Contains(consol7));
			outwardReportStatusFilter.Property = OutwardReportStatusList.Codes.Cancelled;
			results.Load(filterStripBizObj.Filter);
			AssertEquals("consol1 has a cleared status, should not have been found in this filter", false, results.Contains(consol1));
			AssertEquals("consol2 is an import consol, should not have been found in this filter", false, results.Contains(consol2));
			AssertEquals("consol3 has not been sent, should not have been found in this filter", false, results.Contains(consol3));
			AssertEquals("consol4 has a cleared status, should not have been found in this filter", false, results.Contains(consol4));
			AssertEquals("consol5 is an import consol, should not have been found in this filter", false, results.Contains(consol5));
			AssertEquals("consol6 has a rejection status, should not have been found in this filter", false, results.Contains(consol6));
			AssertEquals("Should have found consol7", true, results.Contains(consol7));
			outwardReportStatusFilter.Property = OutwardReportStatusList.Codes.AwaitingResponse;
			results.Load(filterStripBizObj.Filter);
			AssertEquals("No consols shold be found with this filter", 0, results.Count);
			outwardReportStatusFilter.Property = OutwardReportStatusList.Codes.CustomsInstuctionReceived;
			results.Load(filterStripBizObj.Filter);
			AssertEquals("No consols shold be found with this filter", 0, results.Count);
			outwardReportStatusFilter.Property = ZString.Empty;
			results.Load(filterStripBizObj.Filter);
			AssertEquals("All consols shold be found when no filter", 7, results.Count);
			outwardReportStatusFilter.Property = OutwardReportStatusList.Codes.NotSent;
			results.Load(filterStripBizObj.Filter);
			AssertEquals("consol1 has a cleared status, should not have been found in this filter", false, results.Contains(consol1));
			AssertEquals("consol2 is an import consol, should not have been found in this filter", false, results.Contains(consol2));
			AssertEquals("Should have found consol3 as it is the only export consol that has not sent an Outward Report message", true, results.Contains(consol3));
			AssertEquals("consol4 has a cleared status, should not have been found in this filter", false, results.Contains(consol4));
			AssertEquals("consol5 is an import consol, should not have been found in this filter", false, results.Contains(consol5));
			AssertEquals("consol6 has a rejection status, should not have been found in this filter", false, results.Contains(consol6));
			AssertEquals("consol7 has a cancelled status, should not have been found in this filter", false, results.Contains(consol7));
		}

		[TestDate(2020, 08, 21, 15, 50, 30)]
		public void TestAMSBillStatusFilter_DoNotTrimSCACfromBills()
		{
			var carrier = Factory.New<IUSCarrierCombined>();
			carrier.UI_Code = "XXXX";
			carrier.UI_ModeOfTransportation = "10";
			var carrier2 = Factory.New<IUSCarrierCombined>();
			carrier2.UI_Code = "XXXB";
			carrier2.UI_ModeOfTransportation = "10";
			var carrier3 = Factory.New<IUSCarrierCombined>();
			carrier3.UI_Code = "XXX1";
			carrier3.UI_ModeOfTransportation = "10";
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			company2.Branches.Add(branch2);
			var company2OrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			company2.GC_OH_OrgProxy = company2OrgProxy.PK;
			branch2.GB_OH_OrgProxy = company2OrgProxy.PK;
			company2OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "XXXB", Core.Constants.CountryCodes.UnitedStates);
			var companyOrgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			companyOrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "XXXX", Core.Constants.CountryCodes.UnitedStates);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgCarrierCode = org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "XXX1", Core.Constants.CountryCodes.UnitedStates);
			var existingConsols = Factory.Load<ForwardingConsol>(new ZQuery()
			{
				IgnoreActiveFilter = true
			});
			var consolAMSNotOnFile = CreateSeaConsol("C0001_AMS_NOT");
			consolAMSNotOnFile.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			var consolAMSNotOnFileShipment = CreateShipment(consolAMSNotOnFile, "XXX1BILL12345678");
			var consolAMSNotOnFileHeader = CreateAMSHeader(consolAMSNotOnFile.PK, consolAMSNotOnFile.TablePrefix);
			var consolAMSNotOnFileMoveHeader = CreateAMSMoveHeader(consolAMSNotOnFileHeader.PK);
			var consolAMSNotOnFileBill = CreateAMSBill(consolAMSNotOnFileHeader.PK, "XXX1", "XXX1BILL12345678");
			var consolAMSNotOnFileMoveDetail = CreateAMSMoveDetail(consolAMSNotOnFileMoveHeader.PK, consolAMSNotOnFileBill.PK, AMSBillCustomsStatusList.Codes.NotOnFile);
			var consolAMSOnFile = CreateSeaConsol("C0002_AMS_FIL");
			var consolAMSOnFileShipment = CreateShipment(consolAMSOnFile, "XXXXBILL23456789");
			var consolAMSOnFileHeader = CreateAMSHeader(consolAMSOnFile.PK, consolAMSOnFile.TablePrefix);
			var consolAMSOnFileMoveHeader = CreateAMSMoveHeader(consolAMSOnFileHeader.PK);
			var consolAMSOnFileBill = CreateAMSBill(consolAMSOnFileHeader.PK, "XXXX", "XXXXBILL23456789");
			var consolAMSOnFileMoveDetail = CreateAMSMoveDetail(consolAMSOnFileMoveHeader.PK, consolAMSOnFileBill.PK, AMSBillCustomsStatusList.Codes.OnFile);
			var consolAMSMissingMoveDetail = CreateSeaConsol("C0003_AMS_NO_DETAIL");
			consolAMSMissingMoveDetail.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			var consolAMSMissingMoveDetailShipment = CreateShipment(consolAMSMissingMoveDetail, "XXX1BILL34567890");
			var consolAMSMissingMoveDetailHeader = CreateAMSHeader(consolAMSMissingMoveDetail.PK, consolAMSMissingMoveDetail.TablePrefix);
			var consolAMSMissingMoveDetailMoveHeader = CreateAMSMoveHeader(consolAMSMissingMoveDetailHeader.PK);
			var consolAMSMissingMoveDetailBill = CreateAMSBill(consolAMSMissingMoveDetailHeader.PK, "XXX1", "BILL34567890");
			var secondConsolAMSNotOnFile = CreateSeaConsol("C0004_AMS_NOT");
			secondConsolAMSNotOnFile.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			var secondConsolAMSNotOnFileShipment = CreateShipment(secondConsolAMSNotOnFile, "XXX1BILL12345678");
			var secondConsolAMSNotOnFileHeader = CreateAMSHeader(secondConsolAMSNotOnFile.PK, secondConsolAMSNotOnFile.TablePrefix);
			secondConsolAMSNotOnFileHeader.BH_GB = branch2.PK;
			var secondConsolAMSNotOnFileMoveHeader = CreateAMSMoveHeader(secondConsolAMSNotOnFileHeader.PK);
			var secondConsolAMSNotOnFileBill = CreateAMSBill(secondConsolAMSNotOnFileHeader.PK, "XXX1", "XXX1BILL12345678");
			var secondConsolAMSNotOnFileMoveDetail = CreateAMSMoveDetail(secondConsolAMSNotOnFileMoveHeader.PK, secondConsolAMSNotOnFileBill.PK, AMSBillCustomsStatusList.Codes.NotOnFile);
			var secondConsolAMSOnFile = CreateSeaConsol("C0005_AMS_FIL");
			secondConsolAMSOnFile.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			var secondConsolAMSOnFileShipment = CreateShipment(secondConsolAMSOnFile, "XXX1BILL23456789");
			var secondConsolAMSOnFileHeader = CreateAMSHeader(secondConsolAMSOnFile.PK, secondConsolAMSOnFile.TablePrefix);
			secondConsolAMSOnFileHeader.BH_GB = branch2.PK;
			var secondConsolAMSOnFileMoveHeader = CreateAMSMoveHeader(secondConsolAMSOnFileHeader.PK);
			var secondConsolAMSOnFileBill = CreateAMSBill(secondConsolAMSOnFileHeader.PK, "XXX1", "XXX1BILL23456789");
			var secondConsolAMSOnFileMoveDetail = CreateAMSMoveDetail(secondConsolAMSOnFileMoveHeader.PK, secondConsolAMSOnFileBill.PK, AMSBillCustomsStatusList.Codes.OnFile);
			var secondConsolAMSMissingMoveDetail = CreateSeaConsol("C0006_AMS_NO_DETAIL");
			secondConsolAMSMissingMoveDetail.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			var secondConsolAMSMissingMoveDetailShipment = CreateShipment(secondConsolAMSMissingMoveDetail, "XXX1BILL34567890");
			var secondConsolAMSMissingMoveDetailHeader = CreateAMSHeader(secondConsolAMSMissingMoveDetail.PK, secondConsolAMSMissingMoveDetail.TablePrefix);
			secondConsolAMSMissingMoveDetailHeader.BH_GB = branch2.PK;
			var secondConsolAMSMissingMoveDetailMoveHeader = CreateAMSMoveHeader(secondConsolAMSMissingMoveDetailHeader.PK);
			var secondConsolAMSMissingMoveDetailBill = CreateAMSBill(secondConsolAMSMissingMoveDetailHeader.PK, "XXX1", "BILL34567890");
			var consolAIR = CreateSeaConsol("C0007_AIR");
			consolAIR.JK_TransportMode = Core.Constants.TransportModes.Air;
			var consolAIRShipment = CreateShipment(consolAIR, "XXX1BILL74567890");
			var consolAMSNotOnFileOld = CreateSeaConsol("C0008_AMS_NOT_OLD");
			consolAMSNotOnFileOld.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);
			consolAMSNotOnFileOld.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			var consolAMSNotOnFileOldShipment = CreateShipment(consolAMSNotOnFileOld, "XXX1BILL12345678");
			var consolAMSNotOnFileOldHeader = CreateAMSHeader(consolAMSNotOnFileOld.PK, consolAMSNotOnFile.TablePrefix);
			var consolAMSNotOnFileOldMoveHeader = CreateAMSMoveHeader(consolAMSNotOnFileOldHeader.PK);
			var consolAMSNotOnFileOldBill = CreateAMSBill(consolAMSNotOnFileOldHeader.PK, "XXX1", "XXX1BILL12345678");
			var consolAMSNotOnFileOldMoveDetail = CreateAMSMoveDetail(consolAMSNotOnFileOldMoveHeader.PK, consolAMSNotOnFileOldBill.PK, AMSBillCustomsStatusList.Codes.NotOnFile);
			Factory.Save();
			var consolAMSNotOnFileInformation = new ForwardingConsolCustomsInformation(consolAMSNotOnFile);
			var consolAMSOnFileInformation = new ForwardingConsolCustomsInformation(consolAMSOnFile);
			var consolAMSMissingMoveDetailInformation = new ForwardingConsolCustomsInformation(consolAMSMissingMoveDetail);
			var secondConsolAMSNotOnFileInformation = new ForwardingConsolCustomsInformation(secondConsolAMSNotOnFile);
			var secondConsolAMSOnFileInformation = new ForwardingConsolCustomsInformation(secondConsolAMSOnFile);
			var secondConsolAMSMissingMoveDetailInformation = new ForwardingConsolCustomsInformation(secondConsolAMSMissingMoveDetail);
			var consolAIRInformation = new ForwardingConsolCustomsInformation(consolAIR);
			var consolAMSNotOnFileOldInformation = new ForwardingConsolCustomsInformation(consolAMSNotOnFileOld);
			AssertEquals("Precondition: consolAMSNotOnFile", AMSBillCustomsStatusList.Codes.NotOnFile, consolAMSNotOnFileInformation.AMSBillStatus);
			AssertEquals("Precondition: consolAMSOnFile", AMSBillCustomsStatusList.Codes.OnFile, consolAMSOnFileInformation.AMSBillStatus);
			AssertEquals("Precondition: consolAMSMissingMoveDetail", AMSBillCustomsStatusList.Codes.NotOnFile, consolAMSMissingMoveDetailInformation.AMSBillStatus);
			AssertEquals("Precondition: secondConsolAMSNotOnFile", AMSBillCustomsStatusList.Codes.NotOnFile, secondConsolAMSNotOnFileInformation.AMSBillStatus);
			AssertEquals("Precondition: secondConsolAMSOnFile", AMSBillCustomsStatusList.Codes.OnFile, secondConsolAMSOnFileInformation.AMSBillStatus);
			AssertEquals("Precondition: secondConsolAMSMissingMoveDetail", AMSBillCustomsStatusList.Codes.NotOnFile, secondConsolAMSMissingMoveDetailInformation.AMSBillStatus);
			AssertEquals("Precondition: consolAIR", ZString.Empty, consolAIRInformation.AMSBillStatus);
			AssertEquals("Precondition: consolAMSNotOnFileOld", AMSBillCustomsStatusList.Codes.NotOnFile, consolAMSNotOnFileOldInformation.AMSBillStatus);
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var createdTimeFilter = new ModuleDateFilter(FilterDescriptions.CreatedTime, JobConsolSchema.JK_SystemCreateTimeUtc, false);
			filterStripBizObj.ModuleFilters.AddFilter(createdTimeFilter);
			createdTimeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			createdTimeFilter.Property1 = ZDateTime.Today.AddDays(-1);
			createdTimeFilter.Property2 = ZDateTime.Today.AddDays(1);
			createdTimeFilter.IsActive = true;
			var filter = (ModuleTextFilter)filterStripBizObj[ForwardingConsolModuleCustomsFiltersProvider.Descriptions.AMSBillStatus];
			filter.Property = AMSBillCustomsStatusList.Codes.NotOnFile;
			var queryString = filter.Query.LiteralTextADO;
			AssertContains(@"AND (JK_SystemCreateTimeUtc >= #2020-08-20 00:00:00.000# and JK_SystemCreateTimeUtc < #2020-08-23 00:00:00.000#)", queryString);
			AssertContains(@"AND AMSStatus.Status = 'NOT'", queryString);
			var results = Factory.Load<ForwardingConsol>(filter.Query);
			AssertEquals(4 + existingConsols.Length, results.Length);
			AssertCollectionContains(consolAMSNotOnFile, results);
			AssertCollectionNotContains(consolAMSOnFile, results);
			AssertCollectionContains(consolAMSMissingMoveDetail, results);
			AssertCollectionContains(secondConsolAMSNotOnFile, results);
			AssertCollectionNotContains(secondConsolAMSOnFile, results);
			AssertCollectionContains(secondConsolAMSMissingMoveDetail, results);
			AssertCollectionNotContains(consolAIR, results);
			AssertCollectionNotContains(consolAMSNotOnFileOld, results);
			filter.Property = AMSBillCustomsStatusList.Codes.OnFile;
			results = Factory.Load<ForwardingConsol>(filter.Query);
			AssertEquals(2 + existingConsols.Length, results.Length);
			AssertCollectionNotContains(consolAMSNotOnFile, results);
			AssertCollectionContains(consolAMSOnFile, results);
			AssertCollectionNotContains(consolAMSMissingMoveDetail, results);
			AssertCollectionNotContains(secondConsolAMSNotOnFile, results);
			AssertCollectionContains(secondConsolAMSOnFile, results);
			AssertCollectionNotContains(secondConsolAMSMissingMoveDetail, results);
			AssertCollectionNotContains(consolAIR, results);
			AssertCollectionNotContains(consolAMSNotOnFileOld, results);
			filter.Property = AMSConsolBillCustomsStatusList.Codes.OutOfSync;
			results = Factory.Load<ForwardingConsol>(filter.Query);
			AssertEquals(0 + existingConsols.Length, results.Length);
			AssertCollectionNotContains(consolAMSNotOnFile, results);
			AssertCollectionNotContains(consolAMSOnFile, results);
			AssertCollectionNotContains(consolAMSMissingMoveDetail, results);
			AssertCollectionNotContains(secondConsolAMSNotOnFile, results);
			AssertCollectionNotContains(secondConsolAMSOnFile, results);
			AssertCollectionNotContains(secondConsolAMSMissingMoveDetail, results);
			AssertCollectionNotContains(consolAIR, results);
			AssertCollectionNotContains(consolAMSNotOnFileOld, results);
		}

		[TestDate(2020, 08, 21, 15, 50, 30)]
		public void TestAMSBillStatusFilter_MatchExactOrIssuerAndNumber()
		{
			var existingConsols = Factory.Load<ForwardingConsol>(new ZQuery()
			{
				IgnoreActiveFilter = true
			});
			var consolAMSNotOnFile = CreateSeaConsol("C0001_AMS_NOT");
			var consolAMSNotOnFileShipment = CreateShipment(consolAMSNotOnFile, "XXX1BILL12345678");
			var consolAMSNotOnFileHeader = CreateAMSHeader(consolAMSNotOnFile.PK, consolAMSNotOnFile.TablePrefix);
			var consolAMSNotOnFileMoveHeader = CreateAMSMoveHeader(consolAMSNotOnFileHeader.PK);
			var consolAMSNotOnFileBill = CreateAMSBill(consolAMSNotOnFileHeader.PK, "XXX2", "XXX1BILL12345678");
			var consolAMSNotOnFileMoveDetail = CreateAMSMoveDetail(consolAMSNotOnFileMoveHeader.PK, consolAMSNotOnFileBill.PK, AMSBillCustomsStatusList.Codes.NotOnFile);
			var consolAMSNotOnFileIssuerMatch = CreateSeaConsol("C0002_AMS_NOT_ISSUER");
			var consolAMSNotOnFileIssuerMatchShipmentIssuerMatch = CreateShipment(consolAMSNotOnFileIssuerMatch, "XXX1BILL22345678");
			var consolAMSNotOnFileIssuerMatchHeader = CreateAMSHeader(consolAMSNotOnFileIssuerMatch.PK, consolAMSNotOnFileIssuerMatch.TablePrefix);
			var consolAMSNotOnFileIssuerMatchMoveHeader = CreateAMSMoveHeader(consolAMSNotOnFileIssuerMatchHeader.PK);
			var consolAMSNotOnFileIssuerMatchBill = CreateAMSBill(consolAMSNotOnFileIssuerMatchHeader.PK, "XXX1", "BILL22345678");
			var consolAMSNotOnFileIssuerMatchMoveDetail = CreateAMSMoveDetail(consolAMSNotOnFileIssuerMatchMoveHeader.PK, consolAMSNotOnFileIssuerMatchBill.PK, AMSBillCustomsStatusList.Codes.NotOnFile);
			var consolAMSOutOfSync = CreateSeaConsol("C003_AMS_OUT_OF_SYNC");
			var consolAMSOutOfSyncShipment = CreateShipment(consolAMSOutOfSync, "XXX1BILL12345678");
			var consolAMSOutOfSyncHeader = CreateAMSHeader(consolAMSOutOfSync.PK, consolAMSOutOfSync.TablePrefix);
			var consolAMSOutOfSyncMoveHeader = CreateAMSMoveHeader(consolAMSOutOfSyncHeader.PK);
			var consolAMSOutOfSyncBill = CreateAMSBill(consolAMSOutOfSyncHeader.PK, "XXX2", "BILL12345678");
			var consolAMSOutOfSyncMoveDetail = CreateAMSMoveDetail(consolAMSOutOfSyncMoveHeader.PK, consolAMSOutOfSyncBill.PK, AMSBillCustomsStatusList.Codes.NotOnFile);
			var consol4AMSMultiple = CreateSeaConsol("C0004_AMS_MULTIPLE");
			var consol4AMSMultipleHeader = CreateAMSHeader(consol4AMSMultiple.PK, consol4AMSMultiple.TablePrefix);
			var consol4AMSMultipleMoveHeader = CreateAMSMoveHeader(consol4AMSMultipleHeader.PK);
			var consol4AMSMultipleShipment1 = CreateShipment(consol4AMSMultiple, "XXX1BILL12345678");
			var consol4AMSMultipleBill1 = CreateAMSBill(consol4AMSMultipleHeader.PK, "XXX2", "XXX1BILL12345678");
			var consol4AMSMultipleMoveDetail1 = CreateAMSMoveDetail(consol4AMSMultipleMoveHeader.PK, consol4AMSMultipleBill1.PK, AMSBillCustomsStatusList.Codes.NotOnFile);
			var consol4AMSMultipleShipment2 = CreateShipment(consol4AMSMultiple, "XXX2BILL22345678");
			var consol4AMSMultipleBill2 = CreateAMSBill(consol4AMSMultipleHeader.PK, "XXX2", "BILL22345678");
			var consol4AMSMultipleMoveDetail2 = CreateAMSMoveDetail(consol4AMSMultipleMoveHeader.PK, consol4AMSMultipleBill1.PK, AMSBillCustomsStatusList.Codes.OnFile);
			Factory.Save();
			var consolAMSNotOnFileInformation = new ForwardingConsolCustomsInformation(consolAMSNotOnFile);
			var consolAMSNotOnFileIssuerMatchInformation = new ForwardingConsolCustomsInformation(consolAMSNotOnFileIssuerMatch);
			var consolAMSOutOfSyncInformation = new ForwardingConsolCustomsInformation(consolAMSOutOfSync);
			var consol4AMSMultipleInformation = new ForwardingConsolCustomsInformation(consol4AMSMultiple);
			AssertEquals("Precondition: consolAMSNotOnFile", AMSBillCustomsStatusList.Codes.NotOnFile, consolAMSNotOnFileInformation.AMSBillStatus);
			AssertEquals("Precondition: consolAMSNotOnFileIssuerMatch", AMSBillCustomsStatusList.Codes.NotOnFile, consolAMSNotOnFileIssuerMatchInformation.AMSBillStatus);
			AssertEquals("Precondition: consolAMSOutOfSync", AMSConsolBillCustomsStatusList.Codes.OutOfSync, consolAMSOutOfSyncInformation.AMSBillStatus);
			AssertEquals("Precondition: consol4AMSMultiple", AMSConsolBillCustomsStatusList.Codes.Multiple, consol4AMSMultipleInformation.AMSBillStatus);
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var createdTimeFilter = new ModuleDateFilter(FilterDescriptions.CreatedTime, JobConsolSchema.JK_SystemCreateTimeUtc, false);
			filterStripBizObj.ModuleFilters.AddFilter(createdTimeFilter);
			createdTimeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			createdTimeFilter.Property1 = ZDateTime.Today.AddDays(-1);
			createdTimeFilter.Property2 = ZDateTime.Today.AddDays(1);
			createdTimeFilter.IsActive = true;
			var filter = (ModuleTextFilter)filterStripBizObj[ForwardingConsolModuleCustomsFiltersProvider.Descriptions.AMSBillStatus];
			filter.Property = AMSBillCustomsStatusList.Codes.NotOnFile;
			var queryString = filter.Query.LiteralTextADO;
			AssertContains(@"INNER JOIN
	(
			SELECT BH_PK, BH_ParentID
			FROM dbo.CusInBondHeader
			WHERE BH_ApplicationCode = 'AMS'
	) AS AMSHeader ON (BH_ParentID = JK_PK)", queryString);
			AssertContains(@"AND (JK_SystemCreateTimeUtc >= #2020-08-20 00:00:00.000# and JK_SystemCreateTimeUtc < #2020-08-23 00:00:00.000#)", queryString);
			AssertContains(@"AND AMSStatus.Status = 'NOT'", queryString);
			var results = Factory.Load<ForwardingConsol>(filter.Query);
			AssertEquals(2 + existingConsols.Length, results.Length);
			AssertCollectionContains(consolAMSNotOnFile, results);
			AssertCollectionContains(consolAMSNotOnFileIssuerMatch, results);
			AssertCollectionNotContains(consolAMSOutOfSync, results);
			AssertCollectionNotContains(consol4AMSMultiple, results);
			filter.Property = AMSConsolBillCustomsStatusList.Codes.Multiple;
			results = Factory.Load<ForwardingConsol>(filter.Query);
			queryString = filter.Query.LiteralTextADO;
			AssertContains(@"INNER JOIN
	(
			SELECT BH_PK, BH_ParentID
			FROM dbo.CusInBondHeader
			WHERE BH_ApplicationCode = 'AMS'
	) AS AMSHeader ON (BH_ParentID = JK_PK)", queryString);
			AssertContains(@"AND (JK_SystemCreateTimeUtc >= #2020-08-20 00:00:00.000# and JK_SystemCreateTimeUtc < #2020-08-23 00:00:00.000#)", queryString);
			AssertContains(@"AND AMSStatus.Status = 'MULTIPLE'", queryString);
			AssertEquals(1 + existingConsols.Length, results.Length);
			AssertCollectionNotContains(consolAMSNotOnFile, results);
			AssertCollectionNotContains(consolAMSNotOnFileIssuerMatch, results);
			AssertCollectionNotContains(consolAMSOutOfSync, results);
			AssertCollectionContains(consol4AMSMultiple, results);
			filter.Property = AMSConsolBillCustomsStatusList.Codes.OutOfSync;
			results = Factory.Load<ForwardingConsol>(filter.Query);
			queryString = filter.Query.LiteralTextADO;
			AssertContains(@"LEFT JOIN
	(
			SELECT BH_PK, BH_ParentID
			FROM dbo.CusInBondHeader
			WHERE BH_ApplicationCode = 'AMS'
	) AS AMSHeader ON (BH_ParentID = JK_PK)", queryString);
			AssertContains(@"AND (JK_SystemCreateTimeUtc >= #2020-08-20 00:00:00.000# and JK_SystemCreateTimeUtc < #2020-08-23 00:00:00.000#)", queryString);
			AssertContains(@"AND AMSStatus.Status = 'OUT_OF_SYNC'", queryString);
			AssertEquals(1 + existingConsols.Length, results.Length);
			AssertCollectionNotContains(consolAMSNotOnFile, results);
			AssertCollectionNotContains(consolAMSNotOnFileIssuerMatch, results);
			AssertCollectionContains(consolAMSOutOfSync, results);
			AssertCollectionNotContains(consol4AMSMultiple, results);
		}

		public void TestAMSBillStatusFilter_DateTimeRangeEmpty()
		{
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var createdTimeFilter = new ModuleDateFilter(FilterDescriptions.CreatedTime, JobConsolSchema.JK_SystemCreateTimeUtc, false);
			filterStripBizObj.ModuleFilters.AddFilter(createdTimeFilter);
			createdTimeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			createdTimeFilter.Property1 = ZDateTime.Empty;
			createdTimeFilter.Property2 = ZDateTime.Empty;
			createdTimeFilter.IsActive = true;
			var filter = (ModuleTextFilter)filterStripBizObj[ForwardingConsolModuleCustomsFiltersProvider.Descriptions.AMSBillStatus];
			filter.Property = AMSBillCustomsStatusList.Codes.NotOnFile;
			var queryString = filter.Query.LiteralTextADO;
			var comparisonString = @"
			WHEN EXISTS
			(
					SELECT NULL
					FROM dbo.JobConsolTransport
					WHERE JW_ParentGUID = JK_PK AND SUBSTRING(JW_RL_NKDiscPort, 1, 2) IN ('US', 'PR') AND SUBSTRING(JW_RL_NKLoadPort, 1, 2) <> SUBSTRING(JW_RL_NKDiscPort, 1, 2)
			) THEN 1
			ELSE 0
		END = 1
	)
	
	AND AMSStatus.Status";

			AssertContains(comparisonString, queryString);
		}

		ForwardingShipment CreateShipment(ForwardingConsol consol, ZString houseBill)
		{
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = houseBill;
			return shipment;
		}

		ForwardingConsol CreateSeaConsol(ZString consignRef)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_UniqueConsignRef = consignRef;
			consol.JK_RL_NKDischargePort = "USLAX";
			return consol;
		}

		USAMS.ICusInBondMoveDetail CreateAMSMoveDetail(ZGuid amsMoveHeaderPK, ZGuid amsBillPK, ZString status)
		{
			var query = new ZQuery(CusInBondMoveDetailSchema.B9_BM, amsMoveHeaderPK);
			query.AddToFilter(CusInBondMoveDetailSchema.B9_B0, amsBillPK);
			var moveDetail = Factory.LoadTop1<USAMS.ICusInBondMoveDetail>(query) ?? Factory.New<USAMS.ICusInBondMoveDetail>();
			moveDetail.B9_BM = amsMoveHeaderPK;
			moveDetail.B9_B0 = amsBillPK;
			moveDetail.B9_CustomsStatus = status;
			return moveDetail;
		}

		USAMS.ICusInBondBill CreateAMSBill(ZGuid amsHeaderPK, ZString issuerCode, ZString billOfLading)
		{
			var bill = Factory.New<USAMS.ICusInBondBill>();
			bill.B0_BH = amsHeaderPK;
			bill.B0_IssuerCode = issuerCode;
			bill.B0_MasterBillNumber = billOfLading;
			return bill;
		}

		USAMS.ICusInBondMoveHeader CreateAMSMoveHeader(ZGuid amsHeaderPK)
		{
			var moveHeader = Factory.LoadTop1<USAMS.ICusInBondMoveHeader>(new ZQuery(CusInBondMoveHeaderSchema.BM_BH, amsHeaderPK)) ?? Factory.New<USAMS.ICusInBondMoveHeader>();
			moveHeader.BM_BH = amsHeaderPK;
			return moveHeader;
		}

		USAMS.ICusInBondHeader CreateAMSHeader(ZGuid parentID, ZString parentTableCode)
		{
			var header = Factory.New<USAMS.ICusInBondHeader>();
			header.BH_OverrideFreightDefaults = ZBool.True;
			header.BH_ParentID = parentID;
			header.BH_ParentTableCode = parentTableCode;
			return header;
		}

		public void TestAFRBillStatusFilter_TargetSingleBill_noBill()
		{
			var consolEMPTYnoHeader = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			var consolEMPTYnoBill = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			var headerEMPTYnoBill = CreateAFRTestingHeader(consolEMPTYnoBill);
			var consolEMPTY = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			var headerEMPTY = CreateAFRTestingHeader(consolEMPTY);
			var billEMPTY = CreateAFRTestingBill(headerEMPTY, string.Empty);
			var consolNOT = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			var headerNOT = CreateAFRTestingHeader(consolNOT);
			var billNOT = CreateAFRTestingBill(headerNOT, AFRBillCustomsStatusList.Codes.NotRegistered);
			var consolREG = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			var headerREG = CreateAFRTestingHeader(consolREG);
			var billREG = CreateAFRTestingBill(headerREG, AFRBillCustomsStatusList.Codes.Registered);
			var consolHLD = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			var headerHLD = CreateAFRTestingHeader(consolHLD);
			var billHLD = CreateAFRTestingBill(headerHLD, AFRBillCustomsStatusList.Codes.HLD);
			var consolDNL = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			var headerDNL = CreateAFRTestingHeader(consolDNL);
			var billDNL = CreateAFRTestingBill(headerDNL, AFRBillCustomsStatusList.Codes.DoNotLoad);
			var consolDNU = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			var headerDNU = CreateAFRTestingHeader(consolDNU);
			var billDNU = CreateAFRTestingBill(headerDNU, AFRBillCustomsStatusList.Codes.DoNotUnload);
			Factory.Save();
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBizObj[ForwardingConsolModuleCustomsFiltersProvider.Descriptions.AFRBillStatus];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			CombineAssertions(() =>
			{
				filter.Property = AFRBillCustomsStatusList.Codes.NotRegistered;
				var result = Factory.Load<ForwardingConsol>(filter.Query);
				AssertEquals(4, result.Length);
				AssertCollectionContains(consolEMPTYnoHeader, result);
				AssertCollectionContains(consolEMPTYnoBill, result);
				AssertCollectionContains(consolEMPTY, result);
				AssertCollectionContains(consolNOT, result);
				filter.Property = AFRBillCustomsStatusList.Codes.Registered;
				result = Factory.Load<ForwardingConsol>(filter.Query);
				AssertEquals(1, result.Length);
				AssertCollectionContains(consolREG, result);
				filter.Property = AFRBillCustomsStatusList.Codes.HLD;
				result = Factory.Load<ForwardingConsol>(filter.Query);
				AssertEquals(1, result.Length);
				AssertCollectionContains(consolHLD, result);
				filter.Property = AFRBillCustomsStatusList.Codes.DoNotLoad;
				result = Factory.Load<ForwardingConsol>(filter.Query);
				AssertEquals(1, result.Length);
				AssertCollectionContains(consolDNL, result);
				filter.Property = AFRBillCustomsStatusList.Codes.DoNotUnload;
				result = Factory.Load<ForwardingConsol>(filter.Query);
				AssertEquals(1, result.Length);
				AssertCollectionContains(consolDNU, result);
			});
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			CombineAssertions(() =>
			{
				filter.Property = AFRBillCustomsStatusList.Codes.NotRegistered;
				var result = Factory.Load<ForwardingConsol>(filter.Query);
				AssertEquals(4, result.Length);
				AssertCollectionContains(consolEMPTYnoHeader, result);
				AssertCollectionContains(consolEMPTYnoBill, result);
				AssertCollectionContains(consolEMPTY, result);
				AssertCollectionContains(consolNOT, result);
				filter.Property = AFRBillCustomsStatusList.Codes.Registered;
				result = Factory.Load<ForwardingConsol>(filter.Query);
				AssertEquals(1, result.Length);
				AssertCollectionContains(consolREG, result);
				filter.Property = AFRBillCustomsStatusList.Codes.HLD;
				result = Factory.Load<ForwardingConsol>(filter.Query);
				AssertEquals(1, result.Length);
				AssertCollectionContains(consolHLD, result);
				filter.Property = AFRBillCustomsStatusList.Codes.DoNotLoad;
				result = Factory.Load<ForwardingConsol>(filter.Query);
				AssertEquals(1, result.Length);
				AssertCollectionContains(consolDNL, result);
				filter.Property = AFRBillCustomsStatusList.Codes.DoNotUnload;
				result = Factory.Load<ForwardingConsol>(filter.Query);
				AssertEquals(1, result.Length);
				AssertCollectionContains(consolDNU, result);
			});
		}

		public void TestAFRBillStatusFilter_TargetMultipleBill()
		{
			var consol1 = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			var header1 = CreateAFRTestingHeader(consol1);
			var bill11 = CreateAFRTestingBill(header1, string.Empty);
			var bill12 = CreateAFRTestingBill(header1, string.Empty);
			var consol2 = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			var header2 = CreateAFRTestingHeader(consol2);
			var bill21 = CreateAFRTestingBill(header2, AFRBillCustomsStatusList.Codes.NotRegistered);
			var bill22 = CreateAFRTestingBill(header2, string.Empty);
			var consol3 = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			var header3 = CreateAFRTestingHeader(consol3);
			var bill31 = CreateAFRTestingBill(header3, AFRBillCustomsStatusList.Codes.Registered);
			var bill32 = CreateAFRTestingBill(header3, string.Empty);
			var consol4 = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			var header4 = CreateAFRTestingHeader(consol4);
			var bill41 = CreateAFRTestingBill(header4, AFRBillCustomsStatusList.Codes.Registered);
			var bill42 = CreateAFRTestingBill(header4, AFRBillCustomsStatusList.Codes.Registered);
			var consol5 = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			var header5 = CreateAFRTestingHeader(consol5);
			var bill51 = CreateAFRTestingBill(header5, string.Empty);
			var bill52 = CreateAFRTestingBill(header5, AFRBillCustomsStatusList.Codes.NotRegistered);
			var bill53 = CreateAFRTestingBill(header5, AFRBillCustomsStatusList.Codes.Registered);
			var bill54 = CreateAFRTestingBill(header5, AFRBillCustomsStatusList.Codes.HLD);
			var bill55 = CreateAFRTestingBill(header5, AFRBillCustomsStatusList.Codes.DoNotLoad);
			var bill56 = CreateAFRTestingBill(header5, AFRBillCustomsStatusList.Codes.DoNotUnload);
			var consol6 = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			var consol7 = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "CNSHA");
			var consol8 = CreateAFRTestingConsol(Core.Constants.TransportModes.Air, "JPABA");
			Factory.Save();
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBizObj[ForwardingConsolModuleCustomsFiltersProvider.Descriptions.AFRBillStatus];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			CombineAssertions(() =>
			{
				filter.Property = AFRBillCustomsStatusList.Codes.NotRegistered;
				var result = Factory.Load<ForwardingConsol>(filter.Query);
				AssertEquals("consol all NotRegistered", 3, result.Length);
				AssertCollectionContains(consol1, result);
				AssertCollectionContains(consol2, result);
				AssertCollectionContains(consol6, result);
				filter.Property = AFRBillCustomsStatusList.Codes.Registered;
				result = Factory.Load<ForwardingConsol>(filter.Query);
				AssertEquals("consol all Registered", 1, result.Length);
				AssertCollectionContains(consol4, result);
				filter.Property = AFRBillCustomsStatusList.Codes.HLD;
				result = Factory.Load<ForwardingConsol>(filter.Query);
				AssertEquals("consol all HLD", 0, result.Length);
				filter.Property = AFRBillCustomsStatusList.Codes.DoNotLoad;
				result = Factory.Load<ForwardingConsol>(filter.Query);
				AssertEquals("consol all DNL", 0, result.Length);
				filter.Property = AFRBillCustomsStatusList.Codes.DoNotUnload;
				result = Factory.Load<ForwardingConsol>(filter.Query);
				AssertEquals("consol all DNU", 0, result.Length);
			});
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			CombineAssertions(() =>
			{
				filter.Property = AFRBillCustomsStatusList.Codes.NotRegistered;
				var result = Factory.Load<ForwardingConsol>(filter.Query);
				AssertEquals("consol contains NotRegistered", 5, result.Length);
				AssertCollectionContains(consol1, result);
				AssertCollectionContains(consol2, result);
				AssertCollectionContains(consol3, result);
				AssertCollectionContains(consol5, result);
				AssertCollectionContains(consol6, result);
				filter.Property = AFRBillCustomsStatusList.Codes.Registered;
				result = Factory.Load<ForwardingConsol>(filter.Query);
				AssertEquals("consol contains Registered", 3, result.Length);
				AssertCollectionContains(consol3, result);
				AssertCollectionContains(consol4, result);
				AssertCollectionContains(consol5, result);
				filter.Property = AFRBillCustomsStatusList.Codes.HLD;
				result = Factory.Load<ForwardingConsol>(filter.Query);
				AssertEquals("consol contains HLD", 1, result.Length);
				AssertCollectionContains(consol5, result);
				filter.Property = AFRBillCustomsStatusList.Codes.DoNotLoad;
				result = Factory.Load<ForwardingConsol>(filter.Query);
				AssertEquals("consol contains DNL", 1, result.Length);
				AssertCollectionContains(consol5, result);
				filter.Property = AFRBillCustomsStatusList.Codes.DoNotUnload;
				result = Factory.Load<ForwardingConsol>(filter.Query);
				AssertEquals("consol contains DNU", 1, result.Length);
				AssertCollectionContains(consol5, result);
			});
		}

		ForwardingConsol CreateAFRTestingConsol(string transportMode, string dischargePort)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_RL_NKDischargePort = dischargePort;
			return consol;
		}

		Integration.Customs.JP.AFR.IJPAFRHeader CreateAFRTestingHeader(ForwardingConsol consol)
		{
			var header = Factory.New<Integration.Customs.JP.AFR.IJPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			return header;
		}

		Integration.Customs.JP.AFR.IJPAFRBills CreateAFRTestingBill(Integration.Customs.JP.AFR.IJPAFRHeader header, string billStatus)
		{
			var bill = Factory.New<Integration.Customs.JP.AFR.IJPAFRBills>();
			bill.JPB_JPH_Header = header.PK;
			bill.JPB_ReleaseStatus = billStatus;
			return bill;
		}

		ForwardingConsol NewConsol(string name, bool isLinked, string port1, string port2, params string[] otherports)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = name;
			consol.JK_RL_NKLoadPort = port1;
			consol.JK_RL_NKDischargePort = (otherports.Length == 0 ? port2 : otherports[otherports.Length - 1]);
			var lastTransport = consol.Transports[0];
			lastTransport.JW_RL_NKLoadPort = port1;
			lastTransport.JW_RL_NKDiscPort = port2;
			lastTransport.JW_IsLinked = isLinked;
			foreach (string nextPort in otherports)
			{
				string lastPort = lastTransport.JW_RL_NKDiscPort;
				lastTransport = consol.Transports.AddNew();
				lastTransport.JW_IsLinked = isLinked;
				lastTransport.JW_RL_NKLoadPort = lastPort;
				lastTransport.JW_RL_NKDiscPort = nextPort;
			}

			return consol;
		}

		FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new DummyConsolFilterStripBusinessObject();
		}
	}

	class DummyConsolFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			var filtersProvider = new ForwardingConsolModuleCustomsFiltersProvider(Factory, this);
			filtersProvider.AddFilters(result);
			return result;
		}
	}
}
