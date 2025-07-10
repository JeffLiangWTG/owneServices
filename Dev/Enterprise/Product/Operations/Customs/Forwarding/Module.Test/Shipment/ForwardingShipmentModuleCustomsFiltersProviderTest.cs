using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using USISF = Enterprise.Customs.Common.US.ISF;

namespace Enterprise.Customs.Forwarding.Module.Testing
{
	class ForwardingShipmentModuleCustomsFiltersProviderTest : TestCaseWithFactory
	{
		#region Test US Filters

		public void TestUSFilterModuleAreAvailableToPRCompany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.PuertoRico))
			{
				var filterStripBizObj = GetNewFilterStripBusinessObject();
				AssertNotNull(filterStripBizObj["Entry Type"]);
				AssertNotNull(filterStripBizObj["Customs Entry #"]);
				AssertNotNull(filterStripBizObj["IT Number"]);
				AssertNotNull(filterStripBizObj["IT Type"]);
			}
		}

		public void TestUSEntryTypeFilter()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
				var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
				var shipments = new ForwardingShipmentCollection(Factory);

				var jobDeclaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Integration.Customs.US.IJobDeclaration)));
				jobDeclaration[JobDeclarationSchema.JE_JS] = shipment1.PK;
				jobDeclaration[JobDeclarationSchema.JE_AgentsReference] = "Entry Type 01";
				jobDeclaration[JobDeclarationSchema.JE_AddInfo] = "EntryFilerCode=GAZ*EnableCRL=Y*DateOfExport=2008-01-15 00:00:00.000*EnableENS=Y*EntryType=01*EstimatedEntryDate=2008-01-22 00:00:00.000*GeneralOrderNo=AAAA*HazardousCargo=N";
				Factory.Save();

				var filterStripBizObj = GetNewFilterStripBusinessObject();
				var filter = (ModuleTextFilter)filterStripBizObj["Entry Type"];
				filter.IsActive = true;
				filter.Property = "01";
				shipments.Load(filterStripBizObj.Filter);
				AssertEquals("Should contain shipment1", true, shipments.Contains(shipment1));
				AssertEquals("Should not contain shipment2", false, shipments.Contains(shipment2));
			}
		}

		public void TestUSEntryNumberAttachedToDeclaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
				var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

				var jobDeclaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Integration.Customs.US.IJobDeclaration)));
				jobDeclaration[JobDeclarationSchema.JE_JS] = shipment1.PK;
				jobDeclaration[JobDeclarationSchema.JE_MessageType] = "IMP";
				jobDeclaration[JobDeclarationSchema.JE_AddInfo] = "EntryFilerCode=GAZ*EnableENS=Y";

				var entryNumber = Factory.New<CusEntryNumber>();
				entryNumber.CE_EntryNum = "12345678";
				entryNumber.CE_ParentID = jobDeclaration.PK;
				entryNumber.CE_ParentTable = JobDeclarationSchema.Constants.TableName;

				Factory.Save();

				var filterStripBizObj = GetNewFilterStripBusinessObject();
				var filter1 = (ModuleTextFilter)filterStripBizObj["Customs Entry #"];
				filter1.Property = "12345678";
				filter1.IsActive = true;

				Assert("shipment1 has a declaration with the number", shipment1.MatchesFilter(filter1.Query));
				Assert(!shipment2.MatchesFilter(filter1.Query));
			}
		}

		public void TestUSCustomsSimplifiedEntryFilters()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var usTestBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			usTestBranch1.GB_Code = "CHI";
			usTestBranch1.GB_BranchName = "CHICAGO";
			usTestBranch1.GB_IsActive = true;

			var shipment1 = Factory.New<ForwardingShipment>();

			var shipment2 = Factory.New<ForwardingShipment>();
			var declaration2 = Factory.New<Integration.Customs.US.IJobDeclaration>();
			declaration2.JE_JS = shipment2.PK;
			declaration2.JE_GB = usTestBranch1.PK;
			declaration2.JE_MessageType = "IMP";
			var entry2 = Factory.New<Integration.Customs.US.ICusEntryHeader>();
			entry2.CH_JE = declaration2.PK;
			entry2.CH_MessageType = "SE";
			entry2.CH_Status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd;

			var shipment3 = Factory.New<ForwardingShipment>();
			var declaration3 = Factory.New<Integration.Customs.US.IJobDeclaration>();
			declaration3.JE_JS = shipment3.PK;
			declaration3.JE_GB = usTestBranch1.PK;
			declaration3.JE_MessageType = "IMP";
			var entry3 = Factory.New<Integration.Customs.US.ICusEntryHeader>();
			entry3.CH_JE = declaration3.PK;
			entry3.CH_MessageType = "SE";
			entry3.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;

			var shipment4 = Factory.New<ForwardingShipment>();
			var declaration4 = Factory.New<Integration.Customs.US.IJobDeclaration>();
			declaration4.JE_JS = shipment4.PK;
			declaration4.JE_GB = usTestBranch1.PK;
			declaration4.JE_MessageType = "IMP";
			var entry4 = Factory.New<Integration.Customs.US.ICusEntryHeader>();
			entry4.CH_JE = declaration4.PK;
			entry4.CH_MessageType = "SE";
			entry4.CH_Status = "";

			var shipment5 = Factory.New<ForwardingShipment>();
			var declaration5 = Factory.New<Integration.Customs.US.IJobDeclaration>();
			declaration5.JE_JS = shipment5.PK;
			declaration5.JE_GB = usTestBranch1.PK;
			declaration5.JE_MessageType = "IMP";
			var entry5 = Factory.New<Integration.Customs.US.ICusEntryHeader>();
			entry5.CH_JE = declaration5.PK;
			entry5.CH_MessageType = "SE";
			entry5.CH_Status = ImportMessageStatusList.Codes.ErrorACECargoReleaseAdd;

			var shipment6 = Factory.New<ForwardingShipment>();
			var declaration6 = Factory.New<Integration.Customs.US.IJobDeclaration>();
			declaration6.JE_JS = shipment6.PK;
			declaration6.JE_GB = usTestBranch1.PK;
			declaration6.JE_MessageType = "IMP";
			var entry6 = Factory.New<Integration.Customs.US.ICusEntryHeader>();
			entry6.CH_JE = declaration6.PK;
			entry6.CH_MessageType = "SE";
			entry6.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;

			var bill6_1 = Factory.New<Integration.Customs.US.IBill>();
			bill6_1.CU_JE = declaration6.PK;
			bill6_1.CU_BillNum = "MB1";

			var bill6_2 = Factory.New<Integration.Customs.US.IBill>();
			bill6_2.CU_JE = declaration6.PK;
			bill6_2.CU_BillNum = "MB2";

			var shipment7 = Factory.New<ForwardingShipment>();
			var declaration7 = Factory.New<Integration.Customs.US.IJobDeclaration>();
			declaration7.JE_JS = shipment7.PK;
			declaration7.JE_GB = usTestBranch1.PK;
			declaration7.JE_MessageType = "IMP";
			var entry7 = Factory.New<Integration.Customs.US.ICusEntryHeader>();
			entry7.CH_JE = declaration7.PK;
			entry7.CH_MessageType = "SE";
			entry7.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;

			var bill7 = Factory.New<Integration.Customs.US.IBill>();
			bill7.CU_JE = declaration7.PK;
			bill7.CU_BillNum = "MB001";

			Factory.Save();

			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filterModule = filterStripBizObj["CRL (Cargo Release) Status"] as ModuleTextFilter;
			filterModule.Property = ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd;
			filterModule.IsActive = true;

			Assert(!shipment1.MatchesFilter(filterModule.Query));
			Assert(shipment2.MatchesFilter(filterModule.Query));
			Assert(!shipment3.MatchesFilter(filterModule.Query));
			Assert(!shipment4.MatchesFilter(filterModule.Query));
			Assert(!shipment5.MatchesFilter(filterModule.Query));

			filterModule.Property = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;

			Assert(!shipment1.MatchesFilter(filterModule.Query));
			Assert(!shipment2.MatchesFilter(filterModule.Query));
			Assert(shipment3.MatchesFilter(filterModule.Query));
			Assert(!shipment4.MatchesFilter(filterModule.Query));
			Assert(!shipment5.MatchesFilter(filterModule.Query));

			filterModule.Property = ImportMessageStatusList.Codes.ErrorACECargoReleaseAdd;

			Assert(!shipment1.MatchesFilter(filterModule.Query));
			Assert(!shipment2.MatchesFilter(filterModule.Query));
			Assert(!shipment3.MatchesFilter(filterModule.Query));
			Assert(!shipment4.MatchesFilter(filterModule.Query));
			Assert(shipment5.MatchesFilter(filterModule.Query));

			filterModule.Property = "NOT";

			Assert(!shipment1.MatchesFilter(filterModule.Query));
			Assert(!shipment2.MatchesFilter(filterModule.Query));
			Assert(!shipment3.MatchesFilter(filterModule.Query));
			Assert(shipment4.MatchesFilter(filterModule.Query));
			Assert(!shipment5.MatchesFilter(filterModule.Query));
		}

		public void TestUSCustomsExportStatusFilter()
		{
			CreateUSExportEntryJobs();

			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filter = filterStripBizObj["EXP (Export) Status"] as ModuleTextFilter;
			filter.Property = "OSC";
			filter.IsActive = true;

			Assert("shipment 1 does not have an export declaration - should not match", !uSShipment1.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 2 export declaration has cleared status - should match", uSShipment2.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 3 export declaration has relplacement cleared status - should not match", !uSShipment3.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 4 export declaration has not been sent - should not match filter", !uSShipment4.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 5 export declaration has multiple status - one of which is OSC, therefore should not match", uSShipment5.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 6 export declaration has multiple export entries but all with cleared status therefore - should  match", uSShipment6.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 7 export declaration has multiple status - one of which is OSC, therefore should not match", uSShipment7.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 8 export declaration has warning status - should not match", !uSShipment8.MatchesFilter(filterStripBizObj.Filter));

			filter.Property = "RSC";

			Assert("shipment 1 does not have an export declaration - should not match", !uSShipment1.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 2 export declaration has cleared status - should not match", !uSShipment2.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 3 export declaration has relplacement cleared status - should match", uSShipment3.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 4 export declaration has not been sent - should not match filter", !uSShipment4.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 5 export declaration has multiple status - should not match", !uSShipment5.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 6 export declaration has multiple export entries all with cleared status - should not match", !uSShipment6.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 7 export declaration has multiple status - should not match", !uSShipment7.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 8 export declaration has warning status - should not match", !uSShipment8.MatchesFilter(filterStripBizObj.Filter));

			filter.Property = "WRN";

			Assert("shipment 1 does not have an export declaration - should not match", !uSShipment1.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 2 export declaration has cleared status - should not match", !uSShipment2.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 3 export declaration has relplacement cleared status - should not match", !uSShipment3.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 4 export declaration has not been sent - should not match filter", !uSShipment4.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 5 export declaration has multiple status - should not match", !uSShipment5.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 6 export declaration has multiple export entries all with cleared status - should not match", !uSShipment6.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 7 export declaration has multiple status - should not match", !uSShipment7.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 8 export declaration has warning status - should match", uSShipment8.MatchesFilter(filterStripBizObj.Filter));
		}

		public void TestUSCustomsExportStatusForNotSentFilter()
		{
			CreateUSExportEntryJobs();

			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filter = filterStripBizObj["EXP (Export) Status"] as ModuleTextFilter;
			filter.Property = "NOT";
			filter.IsActive = true;

			Assert("shipment 1 does not have an export declaration - should not match", !uSShipment1.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 2 export declaration has cleared status - should not match", !uSShipment2.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 3 export declaration has relplacement cleared status - should not match", !uSShipment3.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 4 export declaration has not been sent - should match filter", uSShipment4.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 5 export declaration has multiple status - should not match", !uSShipment5.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 6 export declaration has multiple export entries - should not match", !uSShipment6.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 7 export declaration has multiple status - should not match", !uSShipment7.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 8 export declaration has warning status - should not match", !uSShipment8.MatchesFilter(filterStripBizObj.Filter));
		}

public void TestUSCustomsExportStatusForMultipleEntryFilter()
		{
			CreateUSExportEntryJobs();

			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filter = filterStripBizObj["EXP (Export) Status"] as ModuleTextFilter;
			filter.Property = "MES";
			filter.IsActive = true;

			Assert("shipment 1 does not have an export declaration - should not match", !uSShipment1.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 2 export declaration has cleared status - should not match", !uSShipment2.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 3 export declaration has relplacement cleared status - should not match", !uSShipment3.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 4 export declaration has not been sent - should not match filter", !uSShipment4.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 5 export declaration has multiple status - should match", uSShipment5.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 6 export declaration has multiple export entries, but all with cleared status therefore - should not match", !uSShipment6.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 7 export declaration has multiple status - should match", uSShipment7.MatchesFilter(filterStripBizObj.Filter));
			Assert("shipment 8 export declaration has warning status - should not match", !uSShipment8.MatchesFilter(filterStripBizObj.Filter));
		}

		#endregion

		#region IT (US In-Bond) Filters

		public void TestITNumberFilterOptions()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var filterStripBizObj = GetNewFilterStripBusinessObject();
				var iTNumberFilter = (ModuleNumberFilter)filterStripBizObj["IT Number"];
				AssertEquals("Filter list should only have 2 options", 2, iTNumberFilter.ComparisonOperator_List.Count);
				AssertEquals("Filter list should have 'starts with' as default", ModuleTextFilter.ComparisonConstants.StartsWith, iTNumberFilter.ComparisonOperator);
				AssertEquals("Filter list should have 'starts with' as default", true, iTNumberFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.StartsWith));
				AssertEquals("Filter list should have 'exact' as other option", true, iTNumberFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.Exact));
			}
		}

		public void TestITTypeFilterOptions()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var filterStripBizObj = GetNewFilterStripBusinessObject();
				var iTTypeFilter = (ModuleTextFilter)filterStripBizObj["IT Type"];
				AssertEquals("Filter list should only have 2 options", 2, iTTypeFilter.ComparisonOperator_List.Count);
				AssertEquals("Filter list should have 'exact' as default", ModuleTextFilter.ComparisonConstants.Exact, iTTypeFilter.ComparisonOperator);
				AssertEquals("Filter list should have 'exact' as default", true, iTTypeFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.Exact));
				AssertEquals("Filter list should have 'starts wit' as other option", true, iTTypeFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.StartsWith));
			}
		}

		public void TestITTypeFilter()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
				var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
				var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
				var shipments = new ForwardingShipmentCollection(Factory);

				var declaration1 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Integration.Customs.US.IJobDeclaration)));
				declaration1[JobDeclarationSchema.JE_JS] = shipment1.PK;
				declaration1[JobDeclarationSchema.JE_AgentsReference] = "IT Type";
				declaration1[JobDeclarationSchema.JE_AddInfo] = "EntryFilerCode=GAZ*EnableCRL=Y*DateOfExport=2008-01-15 00:00:00.000*EnableENS=Y*InbondType=61*EstimatedEntryDate=2008-01-22 00:00:00.000*GeneralOrderNo=AAAA*HazardousCargo=N";

				var declaration2 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Integration.Customs.US.IJobDeclaration)));
				declaration2[JobDeclarationSchema.JE_JS] = shipment2.PK;
				declaration2[JobDeclarationSchema.JE_AgentsReference] = "No IT Type";

				var declaration3 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Integration.Customs.US.IJobDeclaration)));
				declaration3[JobDeclarationSchema.JE_JS] = shipment3.PK;
				declaration3[JobDeclarationSchema.JE_AddInfo] = "EntryFilerCode=GAZ*EnableCRL=Y*DateOfExport=2008-01-15 00:00:00.000*EnableENS=Y*InbondType=62*EstimatedEntryDate=2008-01-22 00:00:00.000";
				Factory.Save();

				var filterStripBizObj = GetNewFilterStripBusinessObject();
				var filter = (ModuleTextFilter)filterStripBizObj["IT Type"];
				filter.IsActive = true;

				shipments.Load(filterStripBizObj.Filter);
				Assert("Empty IT Type search should have found all 3 shipments", shipments.Count > 2);

				filter.Property = "6";
				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				shipments.Load(filterStripBizObj.Filter);
				AssertEquals("IT Type search should have found 2 Shipments", 2, shipments.Count);
				AssertEquals("shipment1 should be there", true, shipments.Contains(shipment1));
				AssertEquals("shipment3 should be there", true, shipments.Contains(shipment3));

				filter.Property = "62";
				shipments.Load(filterStripBizObj.Filter);
				AssertEquals("Should now have found only 1 Shipment", 1, shipments.Count);
				AssertEquals("Shipment3 should be there", true, shipments.Contains(shipment3));

				filter.Property = "51";
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				shipments.Load(filterStripBizObj.Filter);
				AssertEquals("Should now have found 0 shipments", 0, shipments.Count);
			}
		}

		#endregion

		#region Customs Entry and Customs Status

		public void TestAUCustomsEntryStatusNotSentFilter()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var decType = ObjectFactory.GetType<Integration.Customs.IBaseJobDeclaration>();

			var shipment1 = Factory.New<ForwardingShipment>();

			var shipment2 = Factory.New<ForwardingShipment>();
			var declaration2 = Factory.New(decType);
			declaration2[JobDeclarationSchema.JE_EntryStatus] = Common.AU.CustomsEntryStatus.NotSent.Code;
			declaration2[JobDeclarationSchema.JE_GB] = GlbBranch.CurrentBranch.PK;
			declaration2[JobDeclarationSchema.JE_JS] = shipment2.PK;

			var shipment3 = Factory.New<ForwardingShipment>();
			var declaration3 = Factory.New(decType);
			declaration3[JobDeclarationSchema.JE_EntryStatus] = Common.AU.CustomsEntryStatus.CargoCleared.Code;
			declaration3[JobDeclarationSchema.JE_GB] = GlbBranch.CurrentBranch.PK;
			declaration3[JobDeclarationSchema.JE_JS] = shipment3.PK;

			var shipment4 = Factory.New<ForwardingShipment>();
			var declaration4 = Factory.New(decType);
			declaration4[JobDeclarationSchema.JE_EntryStatus] = Common.AU.CustomsEntryStatus.NotSent.Code;
			declaration4[JobDeclarationSchema.JE_ApplicationCode] = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration4[JobDeclarationSchema.JE_GB] = GlbBranch.CurrentBranch.PK;
			declaration4[JobDeclarationSchema.JE_JS] = shipment4.PK;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			var filterModule = filter["Customs Entry Status"] as EntryStatusFilter;
			filterModule.Property = "NOT";
			filterModule.IsActive = true;

			Assert("shipment 1 does not match", !shipment1.MatchesFilter(filter.Filter));
			Assert("shipment 2 does not match", shipment2.MatchesFilter(filter.Filter));
			Assert("shipment 3 does not match", !shipment3.MatchesFilter(filter.Filter));
			Assert("shipment 4 does not match", shipment4.MatchesFilter(filter.Filter));
		}

		public void TestNotSentStatus()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var usTestBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			usTestBranch1.GB_Code = "CHI";
			usTestBranch1.GB_BranchName = "CHICAGO";
			usTestBranch1.GB_IsActive = true;

			var shipment1 = Factory.New<ForwardingShipment>();

			var shipment2 = Factory.New<ForwardingShipment>();
			var declaration2 = Factory.New<Integration.Customs.US.IJobDeclaration>();
			declaration2.JE_JS = shipment2.PK;
			declaration2.JE_GB = usTestBranch1.PK;
			declaration2.JE_MessageType = "IMP";
			var entry2 = Factory.New<Integration.Customs.US.ICusEntryHeader>();
			entry2.CH_JE = declaration2.PK;
			entry2.CH_MessageType = "ENS";
			entry2.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;

			var shipment3 = Factory.New<ForwardingShipment>();
			var declaration3 = Factory.New<Integration.Customs.US.IJobDeclaration>();
			declaration3.JE_JS = shipment3.PK;
			declaration3.JE_GB = usTestBranch1.PK;
			declaration3.JE_MessageType = "IMP";
			var entry3 = Factory.New<Integration.Customs.US.ICusEntryHeader>();
			entry3.CH_JE = declaration3.PK;
			entry3.CH_MessageType = "ENS";
			entry3.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			var shipment4 = Factory.New<ForwardingShipment>();
			var declaration4 = Factory.New<Integration.Customs.US.IJobDeclaration>();
			declaration4.JE_JS = shipment4.PK;
			declaration4.JE_GB = usTestBranch1.PK;
			declaration4.JE_MessageType = "IMP";
			var entry4 = Factory.New<Integration.Customs.US.ICusEntryHeader>();
			entry4.CH_JE = declaration4.PK;
			entry4.CH_MessageType = "ENS";
			entry4.CH_Status = "";

			var shipment5 = Factory.New<ForwardingShipment>();
			var declaration5 = Factory.New<Integration.Customs.US.IJobDeclaration>();
			declaration5.JE_JS = shipment5.PK;
			declaration5.JE_GB = usTestBranch1.PK;
			declaration5.JE_MessageType = "IMP";
			var entry5 = Factory.New<Integration.Customs.US.ICusEntryHeader>();
			entry5.CH_JE = declaration5.PK;
			entry5.CH_MessageType = "ENS";
			entry5.CH_Status = ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal;

			Factory.Save();

			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filter = filterStripBizObj["ENS (Entry Summary) Status"] as ModuleTextFilter;
			filter.Property = "NOT";
			filter.IsActive = true;

			Assert(!shipment1.MatchesFilter(filterStripBizObj.Filter));
			Assert(!shipment2.MatchesFilter(filterStripBizObj.Filter));
			Assert(!shipment3.MatchesFilter(filterStripBizObj.Filter));
			Assert(shipment4.MatchesFilter(filterStripBizObj.Filter));
			Assert(!shipment5.MatchesFilter(filterStripBizObj.Filter));

			entry2.CH_MessageType = "ITN";
			declaration2.JE_MessageType = "EXP";
			entry3.CH_MessageType = "ITN";
			declaration3.JE_MessageType = "EXP";
			entry4.CH_MessageType = "ITN";
			declaration4.JE_MessageType = "EXP";
			entry5.CH_MessageType = "ITN";
			declaration5.JE_MessageType = "EXP";

			Factory.Save();

			filterStripBizObj = GetNewFilterStripBusinessObject();
			filter = filterStripBizObj["EXP (Export) Status"] as ModuleTextFilter;
			filter.Property = "NOT";
			filter.IsActive = true;

			Assert(!shipment1.MatchesFilter(filterStripBizObj.Filter));
			Assert(!shipment2.MatchesFilter(filterStripBizObj.Filter));
			Assert(!shipment3.MatchesFilter(filterStripBizObj.Filter));
			Assert(shipment4.MatchesFilter(filterStripBizObj.Filter));
			Assert(!shipment5.MatchesFilter(filterStripBizObj.Filter));
		}

		public void TestCustomsMessageStatusFilters()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				var makeShipment = new Func<string, ForwardingShipment>(delegate(string jobNumber)
				{
					var s = Factory.New<ForwardingShipment>();
					s.JS_UniqueConsignRef = jobNumber;
					var declaration = Factory.New<Integration.Customs.IBaseJobDeclaration>();
					declaration.JE_JS = s.PK;
					var ceh = Factory.New<Integration.Customs.ICusEntryHeader>();
					ceh.CH_JE = declaration.PK;
					ceh.CH_EntryStatus = "ES" + jobNumber;
					ceh.CH_Status = "S" + jobNumber;
					return s;
				});
				var shipment1 = makeShipment("1");
				var shipment2 = makeShipment("2");
				var shipment3 = makeShipment("3");
				Factory.Save();

				var filterStripBizObj = GetNewFilterStripBusinessObject();
				var entryStatusFilter = filterStripBizObj["Customs Entry Status"] as EntryStatusFilter;
				var messageStatusFilter = filterStripBizObj["Customs Message Status"] as ModuleTextFilter;
				entryStatusFilter.IsActive = true;
				messageStatusFilter.IsActive = true;

				messageStatusFilter.Property = "S1";
				var collection = new ForwardingShipmentCollection(Factory);
				collection.Load(filterStripBizObj.Filter);
				AssertCollectionContains(shipment1, collection);
				AssertCollectionNotContains(shipment2, collection);
				AssertCollectionNotContains(shipment3, collection);

				messageStatusFilter.Property = "S2";

				collection = new ForwardingShipmentCollection(Factory);
				collection.Load(filterStripBizObj.Filter);
				AssertCollectionContains(shipment2, collection);
				AssertCollectionNotContains(shipment1, collection);
				AssertCollectionNotContains(shipment3, collection);
			}
		}

		public void TestEntryStatusFilterMaxLength()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				var filterStripBizObj = GetNewFilterStripBusinessObject();
				var entryStatusFilter = filterStripBizObj["Customs Entry Status"] as EntryStatusFilter;
				AssertEquals(CusEntryHeaderSchema.CH_EntryStatus.MaxLength, entryStatusFilter.MaxLength);
			}
		}

		public void TestEntryStatusFilter_FilterTypeVisibilityDefault()
		{
			AssertEntryStatusFilter_FilterTypeVisibilityForCountry(Core.Constants.CountryCodes.Latvia, true);
		}

		public void TestEntryStatusFilter_FilterTypeVisibilitySG()
		{
			AssertEntryStatusFilter_FilterTypeVisibilityForCountry(Core.Constants.CountryCodes.Singapore, false);
		}

		public void TestEntryStatusFilter_FilterTypeVisibilityNZ()
		{
			AssertEntryStatusFilter_FilterTypeVisibilityForCountry(Core.Constants.CountryCodes.NewZealand, false);
		}

		public void TestCustomsMessageStatusCodeListForVariousCountries()
		{
			var originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			RunCustomsStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.UnitedKingdom, new string[] { "AWR", "OK" }, new string[] { "RT1", "CLR" }, "Customs Message Status");
			RunCustomsStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.Lithuania, new string[] { "AWR", "OK" }, new string[] { "RT1", "CLR" }, "Customs Message Status");
			GlbCompany.CurrentCompany.SetCountry(originalCountry);
		}

		public void TestCustomsEntryStatusCodeListForVariousCountries()
		{
			var originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			RunCustomsStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.Australia, new string[] { "WTO", "CLR" }, new string[] { "RT1", "ROK", "CEO" });
			RunCustomsStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.Singapore, new string[] { "ROK", "RRJ" }, new string[] { "WTO", "RT1", "CEO" });
			RunCustomsStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.UnitedStates, new string[] { "CEO", "EEO" }, new string[] { "WTO", "RT1", "CLR" }, "ENS (Entry Summary) Status");
			RunCustomsStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.UnitedKingdom, new string[] { "AWR", "B" }, new string[] { "WTO", "ROK", "CEO" });
			RunCustomsStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.France, new string[] { "010", "040" }, new string[] { "WTO", "ROK", "CEO" });
			RunCustomsStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.Lithuania, new string[] { "RT1", "CLR" }, new string[] { "WTO", "ROK", "CEO" });
			RunCustomsStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.NewZealand, new string[] { "ADJ", "ARP", "CLR" }, new string[] { "WTO", "ROK", "CEO" });
			RunCustomsStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.Canada, new string[] { "ERR", "CLR", "WTA", "Y51" }, new string[] { "WTO", "ROK", "CEO" });
			RunCustomsStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.HongKong, new string[] { "SUB", "ACK" }, new string[] { "WTO", "ROK", "CEO" });
			GlbCompany.CurrentCompany.SetCountry(originalCountry);
		}

		void RunCustomsStatusCodeListForVariousCountriesTester(string countryCode, string[] expectedStrings, string[] shouldNotContainStrings, string filterName = "Customs Entry Status")
		{
			GlbCompany.CurrentCompany.SetCountry(countryCode);
			var filter = GetNewFilterStripBusinessObject();

			ICodeDescriptionPairList iList;

			if (filter[filterName] is EntryStatusFilter entryStatusFilter)
			{
				iList = entryStatusFilter.EntryStatusList;
			}
			else
			{
				var filterModule = filter[filterName] as ModuleTextFilter;
				iList = ((ICodeDescriptionPairList)filterModule.List);
			}

			foreach (var expectedString in expectedStrings)
			{
				Assert(string.Format("Customs entry status list for country {0} should contain code value {1}", countryCode, expectedString), iList.ContainsCode(expectedString));
			}
			foreach (var shouldNotContainString in shouldNotContainStrings)
			{
				Assert(string.Format("Customs entry status list for country {0} should NOT contain code value {1}", countryCode, shouldNotContainString), !iList.ContainsCode(shouldNotContainString));
			}
		}

		public void TestZACusCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Entry Status");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "1", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "2", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "3", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "4", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry((Core.Constants.CountryCodes.SouthAfrica));
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filter = filterStripBizObj["Customs Entry Status"] as EntryStatusFilter;
			var iList = ((ICodeDescriptionPairList)filter.EntryStatusList);
			string[] expectedStrings = new string[] { "1", "2", "3", "4" };
			string[] shouldNotContainStrings = { "CAN", "CLR", "ERR" };

			foreach (var expectedString in expectedStrings)
			{
				Assert(string.Format("Customs entry status list for country {0} should contain code value {1}", Core.Constants.CountryCodes.SouthAfrica, expectedString), iList.ContainsCode(expectedString));
			}
			foreach (var shouldNotContainString in shouldNotContainStrings)
			{
				Assert(string.Format("Customs entry status list for country {0} should NOT contain code value {1}", Core.Constants.CountryCodes.SouthAfrica, shouldNotContainString), !iList.ContainsCode(shouldNotContainString));
			}
		}

		public void TestCusEntryNumberAttachedToOtherCountryDeclaration()
		{
			var caCompany = Factory.NewWithValidTestData<GlbCompany>();
			caCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var caBranch = Factory.NewWithValidTestData<GlbBranch>();
			caBranch.GB_Code = "BLO";
			caBranch.GB_BranchName = "Beaverlodge";
			caBranch.GB_IsActive = true;
			caCompany.Branches.Add(caBranch);
			Factory.Save();

			var shipment1 = Factory.New<ForwardingShipment>();

			var caDeclaration1 = Factory.New<Integration.Customs.CA.IJobDeclaration>();
			caDeclaration1.JE_JS = shipment1.PK;
			caDeclaration1.JE_GB = caBranch.PK;
			caDeclaration1.JE_MessageType = "IMP";

			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryNum = "123";
			entryNumber.CE_ParentID = caDeclaration1.PK;
			entryNumber.CE_ParentTable = "JobDeclaration";
			entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var usBranch = Factory.NewWithValidTestData<GlbBranch>();
			usBranch.GB_Code = "CHI";
			usBranch.GB_BranchName = "CHICAGO";
			usBranch.GB_IsActive = true;

			Factory.Save();

			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filter = filterStripBizObj["Customs Entry #"] as ModuleTextFilter;
			filter.Property = "123";
			filter.IsActive = true;

			Assert(!shipment1.MatchesFilter(filterStripBizObj.Filter));
		}

		#endregion

		#region ISF Bill Status

		public void TestISFBillStatusFilterScript()
		{
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filter1 = (ModuleTextFilter)filterStripBizObj["ISF Bill Status"];
			filter1.IsActive = true;
			filter1.Property = FreightConstants.CoLoadStatus.All;
			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

			var query = filterStripBizObj.ModuleFilters.GetFilterQuery(new[] { filter1 });
			AssertContains("Use Union instead of Left Join",
@"				WHERE JS_IsForwardRegistered = 1 AND JS_IsCancelled = 0 AND JS_TransportMode IN ('SEA', 'FAS', 'FSA')

				UNION

				SELECT JS_PK AS ShipmentPK, BB_PK, BB_CustomsStatus
				FROM dbo.JobShipment
				JOIN dbo.CusISFBill ON BB_BillNum IN (JS_HouseBill)
									AND BB_BillType IN ('OB', 'BM')
									AND BB_BF IN (SELECT BF_PK
													FROM dbo.CusISFHeader
													WHERE BF_SystemCreateTimeUtc >= CASE WHEN",
				query.LiteralTextADO);
		}

		public void TestISFBillStatusFilter_MultipleSameStatus()
		{
			var isfCreateTime = ZDateTime.Now;
			var shipmentWithMultipleMatchS3 = Factory.New<ForwardingShipment>();
			shipmentWithMultipleMatchS3.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentWithMultipleMatchS3.JS_UniqueConsignRef = "S00004_S3";
			shipmentWithMultipleMatchS3.JS_HouseBill = "BILL4";
			var isfBILL4S3_1 = CreateISFJob(isfCreateTime.AddSeconds(-7), "BILL4", USISF.BillTypeList.Codes.HouseBillOfLading, USISF.DispositionCodeList.Codes.S3);
			var isfBILL4S3_2 = CreateISFJob(isfCreateTime.AddSeconds(-6), "BILL4", USISF.BillTypeList.Codes.HouseBillOfLading, USISF.DispositionCodeList.Codes.S3);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Multiple ISF bill jobs even though the bill staus and number are the same", USISF.ISFStatusHelper.Multiple, shipmentWithMultipleMatchS3.ISFBillStatus);

				var filterStripBizObj = GetNewFilterStripBusinessObject();
				var filter = (ModuleTextFilter)filterStripBizObj["ISF Bill Status"];
				filter.IsActive = true;
				filter.Property = USISF.ISFStatusHelper.Multiple;
				var results = Factory.Load<ForwardingShipment>(filterStripBizObj.Filter);
				AssertEquals("The multiple record should be searched out", 1, results.Length);
				AssertCollectionContains(shipmentWithMultipleMatchS3, results);
			});
		}

		public void TestISFBillStatusFilter_MultipleSameStatusAndEmptyBillNumber()
		{
			var isfCreateTime = ZDateTime.Now;
			var shipmentWithMultipleMatchS3 = Factory.New<ForwardingShipment>();
			shipmentWithMultipleMatchS3.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentWithMultipleMatchS3.JS_UniqueConsignRef = "S00004_S3";
			shipmentWithMultipleMatchS3.JS_HouseBill = "BILL4";
			var isfBILL4S3_1 = CreateISFJob(isfCreateTime.AddSeconds(-7), "", USISF.BillTypeList.Codes.HouseBillOfLading, USISF.DispositionCodeList.Codes.S3);
			var isfBILL4S3_2 = CreateISFJob(isfCreateTime.AddSeconds(-6), "BILL4", USISF.BillTypeList.Codes.HouseBillOfLading, USISF.DispositionCodeList.Codes.S3);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Single status tgough there are multiple ISF bill jobs but only one has the bill number", "S3", shipmentWithMultipleMatchS3.ISFBillStatus);

				var filterStripBizObj = GetNewFilterStripBusinessObject();
				var filter = (ModuleTextFilter)filterStripBizObj["ISF Bill Status"];
				filter.IsActive = true;
				filter.Property = USISF.ISFStatusHelper.Multiple;
				var results = Factory.Load<ForwardingShipment>(filterStripBizObj.Filter);
				AssertEquals("No multiple record should be searched out", 0, results.Length);

				filter.Property = "S3";
				results = Factory.Load<ForwardingShipment>(filterStripBizObj.Filter);
				AssertEquals("The S3 e record should be searched out", 1, results.Length);
				AssertCollectionContains(shipmentWithMultipleMatchS3, results);
			});
		}

		public void TestISFBillStatusFilter()
		{
			var existingShipments = Factory.Load<ForwardingShipment>(new ZQuery() { IgnoreActiveFilter = true });
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			company2.Branches.Add(branch2);
			var company2OrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			company2.GC_OH_OrgProxy = company2OrgProxy.PK;
			company2OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "XXXB", Core.Constants.CountryCodes.UnitedStates);
			Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy).CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "XXXX", Core.Constants.CountryCodes.UnitedStates);

			var shipmentMatchAMSBillNumber = Factory.New<ForwardingShipment>();
			shipmentMatchAMSBillNumber.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentMatchAMSBillNumber.JS_UniqueConsignRef = "S00001_AMS_S2";
			shipmentMatchAMSBillNumber.JS_HouseBill = "BILL4";
			var amsBillNumber = shipmentMatchAMSBillNumber.Numbers.AddNew();
			amsBillNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			amsBillNumber.CE_EntryNum = "XXXABILL1";
			var isfCreateTime = ZDateTime.Now;
			var isfXXXABILL1S2 = CreateISFJob(isfCreateTime.AddSeconds(-10), "XXXABILL1", USISF.BillTypeList.Codes.HouseBillOfLading, USISF.DispositionCodeList.Codes.S2);

			var shipmentMatchS3 = Factory.New<ForwardingShipment>();
			shipmentMatchS3.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentMatchS3.JS_UniqueConsignRef = "S00002_S3";
			shipmentMatchS3.JS_HouseBill = "BILL2";
			var isfXXXBBill2S3 = CreateISFJob(isfCreateTime.AddSeconds(-9), "XXXBBILL2", USISF.BillTypeList.Codes.OceanBillOfLading, USISF.DispositionCodeList.Codes.S3);

			var shipmentMatchS4 = Factory.New<ForwardingShipment>();
			shipmentMatchS4.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentMatchS4.JS_UniqueConsignRef = "S00003_S4";
			shipmentMatchS4.JS_HouseBill = "XXXBBILL3";
			var isfXXXBBILL3S4 = CreateISFJob(isfCreateTime.AddSeconds(-8), "XXXBBILL3", USISF.BillTypeList.Codes.HouseBillOfLading, USISF.DispositionCodeList.Codes.S4);

			var shipmentWithMultipleMatchS2_S3 = Factory.New<ForwardingShipment>();
			shipmentWithMultipleMatchS2_S3.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentWithMultipleMatchS2_S3.JS_UniqueConsignRef = "S00004_S2_S3";
			shipmentWithMultipleMatchS2_S3.JS_HouseBill = "BILL4";
			var isfXXXXBILL4S2 = CreateISFJob(isfCreateTime.AddSeconds(-7), "XXXXBILL4", USISF.BillTypeList.Codes.HouseBillOfLading, USISF.DispositionCodeList.Codes.S2);
			var isfBILL4S3 = CreateISFJob(isfCreateTime.AddSeconds(-6), "BILL4", USISF.BillTypeList.Codes.HouseBillOfLading, USISF.DispositionCodeList.Codes.S3);

			var isfBILL6S5 = CreateISFJob(isfCreateTime.AddSeconds(-5), "BILL6", USISF.BillTypeList.Codes.HouseBillOfLading, USISF.DispositionCodeList.Codes.S5);

			var shipmentTooOld = Factory.New<ForwardingShipment>();
			shipmentTooOld.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentTooOld.JS_UniqueConsignRef = "S00005_OLD";
			shipmentTooOld.JS_HouseBill = "BILL4";
			shipmentTooOld.JS_SystemCreateTimeUtc = ZDateTime.Now.AddYears(-2);

			var shipmentWithNoISFMatch = Factory.New<ForwardingShipment>();
			shipmentWithNoISFMatch.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentWithNoISFMatch.JS_UniqueConsignRef = "S00006_NOMATCH";
			shipmentWithNoISFMatch.JS_HouseBill = "BILL5";

			var airShipment = Factory.New<ForwardingShipment>();
			airShipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			airShipment.JS_UniqueConsignRef = "S00007_AIR";
			airShipment.JS_HouseBill = "BILL7";
			_ = CreateISFJob(isfCreateTime.AddSeconds(-4), "XXXBBILL7", USISF.BillTypeList.Codes.OceanBillOfLading, USISF.DispositionCodeList.Codes.S3);

			var seaAirShipment = Factory.New<ForwardingShipment>();
			seaAirShipment.JS_TransportMode = Core.Constants.TransportModes.SeaAir;
			seaAirShipment.JS_UniqueConsignRef = "S000015_SEAAIR";
			seaAirShipment.JS_HouseBill = "BILL15";
			_ = CreateISFJob(isfCreateTime.AddSeconds(-4), "XXXBBILL15", USISF.BillTypeList.Codes.OceanBillOfLading, USISF.DispositionCodeList.Codes.S6);

			var airSeaShipment = Factory.New<ForwardingShipment>();
			airSeaShipment.JS_TransportMode = Core.Constants.TransportModes.AirSea;
			airSeaShipment.JS_UniqueConsignRef = "S000016_AIRSEA";
			airSeaShipment.JS_HouseBill = "BILL16";
			_ = CreateISFJob(isfCreateTime.AddSeconds(-4), "XXXBBILL16", USISF.BillTypeList.Codes.OceanBillOfLading, USISF.DispositionCodeList.Codes.S6);

			var secondShipmentMatchS3 = Factory.New<ForwardingShipment>();
			secondShipmentMatchS3.JS_TransportMode = Core.Constants.TransportModes.Sea;
			secondShipmentMatchS3.JS_UniqueConsignRef = "S00008_S3";
			secondShipmentMatchS3.JS_HouseBill = "BILL8";
			var isfXXXBBILL8S3 = CreateISFJob(isfCreateTime.AddSeconds(-4), "XXXBBILL8", USISF.BillTypeList.Codes.OceanBillOfLading, USISF.DispositionCodeList.Codes.S3);

			var directShipmentMatchS4 = Factory.New<ForwardingShipment>();
			directShipmentMatchS4.JS_TransportMode = Core.Constants.TransportModes.Sea;
			directShipmentMatchS4.JS_UniqueConsignRef = "S00009_S4";
			directShipmentMatchS4.JS_HouseBill = ZString.Empty;
			var directShipmentMatchS4Consol = directShipmentMatchS4.Consols.AddNew();
			directShipmentMatchS4Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			directShipmentMatchS4Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			directShipmentMatchS4Consol.JK_MasterBillNum = "XXXBBILL9";
			var isfXXXBBILL9S4 = CreateISFJob(isfCreateTime.AddSeconds(-10), "XXXBBILL9", USISF.BillTypeList.Codes.OceanBillOfLading, USISF.DispositionCodeList.Codes.S4);

			var directShipmentMatchS3WithSCAC = Factory.New<ForwardingShipment>();
			directShipmentMatchS3WithSCAC.JS_TransportMode = Core.Constants.TransportModes.Sea;
			directShipmentMatchS3WithSCAC.JS_UniqueConsignRef = "S00010_S3";
			directShipmentMatchS3WithSCAC.JS_HouseBill = ZString.Empty;
			var directShipmentMatchS3WithSCACConsol = directShipmentMatchS3WithSCAC.Consols.AddNew();
			directShipmentMatchS3WithSCACConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			directShipmentMatchS3WithSCACConsol.JK_AgentType = Core.Constants.AgentType.Direct;
			directShipmentMatchS3WithSCACConsol.JK_MasterBillNum = "BILL10";
			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_Code = "SH1234";
			shippingLine.OH_FullName = "SHIPPING LINE";
			shippingLine.MainAddress.OA_Address1 = "ADDRESS 1";
			shippingLine.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "XXXC", Core.Constants.CountryCodes.UnitedStates);
			directShipmentMatchS3WithSCACConsol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;
			var isfXXXCBILL10S3 = CreateISFJob(isfCreateTime.AddSeconds(-10), "XXXCBILL10", USISF.BillTypeList.Codes.OceanBillOfLading, USISF.DispositionCodeList.Codes.S3);

			var directShipmentMatchS2WithAMS = Factory.New<ForwardingShipment>();
			directShipmentMatchS2WithAMS.JS_TransportMode = Core.Constants.TransportModes.Sea;
			directShipmentMatchS2WithAMS.JS_UniqueConsignRef = "S00011_S2";
			directShipmentMatchS2WithAMS.JS_HouseBill = ZString.Empty;
			var directShipmentMatchS2WithAMSConsol = directShipmentMatchS2WithAMS.Consols.AddNew();
			directShipmentMatchS2WithAMSConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			directShipmentMatchS2WithAMSConsol.JK_AgentType = Core.Constants.AgentType.Direct;
			directShipmentMatchS2WithAMSConsol.JK_MasterBillNum = "XXXBBILL11";
			var directShipmentMatchS2WithAMSConsolAMSBillNumber = directShipmentMatchS2WithAMSConsol.Numbers.AddNew();
			directShipmentMatchS2WithAMSConsolAMSBillNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			directShipmentMatchS2WithAMSConsolAMSBillNumber.CE_EntryNum = "XXXABILL11";
			var isfXXXBBILL11S4 = CreateISFJob(isfCreateTime.AddSeconds(-10), "XXXBBILL11", USISF.BillTypeList.Codes.OceanBillOfLading, USISF.DispositionCodeList.Codes.S4);
			var isfXXXABILL11S2 = CreateISFJob(isfCreateTime.AddSeconds(-10), "XXXABILL11", USISF.BillTypeList.Codes.OceanBillOfLading, USISF.DispositionCodeList.Codes.S2);

			var directShipmentWithAirConsol = Factory.New<ForwardingShipment>();
			directShipmentWithAirConsol.JS_TransportMode = Core.Constants.TransportModes.Sea;
			directShipmentWithAirConsol.JS_UniqueConsignRef = "S00012_AIR";
			var directShipmentWithAirConsolConsol = directShipmentWithAirConsol.Consols.AddNew();
			directShipmentWithAirConsolConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			directShipmentWithAirConsolConsol.JK_AgentType = Core.Constants.AgentType.Direct;
			directShipmentWithAirConsolConsol.JK_MasterBillNum = "XXXBBILL12";
			directShipmentWithAirConsol.JS_HouseBill = ZString.Empty;
			var isfXXXBBILL12S4 = CreateISFJob(isfCreateTime.AddSeconds(-10), "XXXBBILL12", USISF.BillTypeList.Codes.OceanBillOfLading, USISF.DispositionCodeList.Codes.S4);

			var shipmentWithNonDirectConsol = Factory.New<ForwardingShipment>();
			shipmentWithNonDirectConsol.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentWithNonDirectConsol.JS_UniqueConsignRef = "S00013_AGT";
			shipmentWithNonDirectConsol.JS_HouseBill = ZString.Empty;
			var shipmentWithNonDirectConsolConsol = shipmentWithNonDirectConsol.Consols.AddNew();
			shipmentWithNonDirectConsolConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentWithNonDirectConsolConsol.JK_AgentType = Core.Constants.AgentType.Agent;
			shipmentWithNonDirectConsolConsol.JK_MasterBillNum = "XXXBBILL13";
			var isfXXXBBILL13S4 = CreateISFJob(isfCreateTime.AddSeconds(-10), "XXXBBILL13", USISF.BillTypeList.Codes.OceanBillOfLading, USISF.DispositionCodeList.Codes.S4);

			var shipmentWithDirectConsol = Factory.New<ForwardingShipment>();
			shipmentWithDirectConsol.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentWithDirectConsol.JS_UniqueConsignRef = "S00014_AGT";
			shipmentWithDirectConsol.JS_HouseBill = "TEST";
			var shipmentWithDirectConsolConsol = shipmentWithDirectConsol.Consols.AddNew();
			shipmentWithDirectConsolConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentWithDirectConsolConsol.JK_AgentType = Core.Constants.AgentType.Direct;
			shipmentWithDirectConsolConsol.JK_MasterBillNum = "XXXBBILL14";
			var isfXXXBBILL14S4 = CreateISFJob(isfCreateTime.AddSeconds(-10), "XXXBBILL14", USISF.BillTypeList.Codes.OceanBillOfLading, USISF.DispositionCodeList.Codes.S4);

			Factory.Save();
			AssertEquals(USISF.DispositionCodeList.Codes.S2, shipmentMatchAMSBillNumber.ISFBillStatus);
			AssertEquals(USISF.DispositionCodeList.Codes.S3, shipmentMatchS3.ISFBillStatus);
			AssertEquals(USISF.DispositionCodeList.Codes.S4, shipmentMatchS4.ISFBillStatus);
			AssertEquals(USISF.ISFStatusHelper.Multiple, shipmentWithMultipleMatchS2_S3.ISFBillStatus);
			AssertEquals("No matching based on Shipment create time", "", shipmentTooOld.ISFBillStatus);
			AssertEquals("", shipmentWithNoISFMatch.ISFBillStatus);
			AssertEquals("Non Sea shipment are not valid for ISF", "", airShipment.ISFBillStatus);
			AssertEquals(USISF.DispositionCodeList.Codes.S3, secondShipmentMatchS3.ISFBillStatus);
			AssertEquals(USISF.DispositionCodeList.Codes.S4, directShipmentMatchS4.ISFBillStatus);
			AssertEquals(USISF.DispositionCodeList.Codes.S3, directShipmentMatchS3WithSCAC.ISFBillStatus);
			AssertEquals(USISF.DispositionCodeList.Codes.S2, directShipmentMatchS2WithAMS.ISFBillStatus);
			AssertEquals("shipment with Direct Air consol are not valid for ISF", "", directShipmentWithAirConsol.ISFBillStatus);
			AssertEquals("shipment with non-direct consol are not valid for ISF", "", shipmentWithNonDirectConsol.ISFBillStatus);

			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBizObj["ISF Bill Status"];
			filter.IsActive = true;
			filter.Property = FreightConstants.CoLoadStatus.All;
			var comparisonOperatorList = filter.ComparisonOperator_List;
			AssertEquals(4, comparisonOperatorList.Count);
			AssertEquals(true, comparisonOperatorList.ContainsCode(ModuleTextFilter.ComparisonConstants.Exact));
			AssertEquals(true, comparisonOperatorList.ContainsCode(ModuleTextFilter.ComparisonConstants.NotEqual));
			AssertEquals(true, comparisonOperatorList.ContainsCode(ModuleTextFilter.ComparisonConstants.IsBlank));
			AssertEquals(true, comparisonOperatorList.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank));
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;

			var results = Factory.Load<ForwardingShipment>(filterStripBizObj.Filter);
			AssertCollectionNotContains(shipmentWithDirectConsol, results);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;

			results = Factory.Load<ForwardingShipment>(filterStripBizObj.Filter);
			AssertEquals(11, results.Length);
			AssertCollectionContains(shipmentMatchAMSBillNumber, results);
			AssertCollectionContains(shipmentMatchS3, results);
			AssertCollectionContains(shipmentMatchS4, results);
			AssertCollectionContains(shipmentWithMultipleMatchS2_S3, results);
			AssertCollectionContains(secondShipmentMatchS3, results);
			AssertCollectionContains(directShipmentMatchS4, results);
			AssertCollectionContains(directShipmentMatchS3WithSCAC, results);
			AssertCollectionContains(directShipmentMatchS2WithAMS, results);
			AssertCollectionContains(shipmentWithDirectConsol, results);
			AssertCollectionContains(airSeaShipment, results);
			AssertCollectionContains(seaAirShipment, results);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			results = Factory.Load<ForwardingShipment>(filterStripBizObj.Filter);
			AssertEquals(5 + existingShipments.Length, results.Length);
			AssertCollectionContains(shipmentTooOld, results);
			AssertCollectionContains(shipmentWithNoISFMatch, results);
			AssertCollectionContains("Air shipment are not valid for ISF Bill Status", airShipment, results);
			AssertCollectionContains(directShipmentWithAirConsol, results);
			AssertCollectionContains(shipmentWithNonDirectConsol, results);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = USISF.DispositionCodeList.Codes.S2;
			results = Factory.Load<ForwardingShipment>(filterStripBizObj.Filter);
			AssertEquals(2, results.Length);
			AssertCollectionContains(shipmentMatchAMSBillNumber, results);
			AssertCollectionContains(directShipmentMatchS2WithAMS, results);

			filter.Property = USISF.DispositionCodeList.Codes.S3;
			results = Factory.Load<ForwardingShipment>(filterStripBizObj.Filter);
			AssertEquals(3, results.Length);
			AssertCollectionContains(shipmentMatchS3, results);
			AssertCollectionContains(secondShipmentMatchS3, results);
			AssertCollectionContains(directShipmentMatchS3WithSCAC, results);

			filter.Property = USISF.DispositionCodeList.Codes.S4;
			results = Factory.Load<ForwardingShipment>(filterStripBizObj.Filter);
			AssertEquals(3, results.Length);
			AssertCollectionContains(shipmentMatchS4, results);
			AssertCollectionContains(directShipmentMatchS4, results);
			AssertCollectionContains(shipmentWithDirectConsol, results);

			filter.Property = USISF.DispositionCodeList.Codes.S5;
			results = Factory.Load<ForwardingShipment>(filterStripBizObj.Filter);
			AssertEquals(0, results.Length);

			filter.Property = USISF.ISFStatusHelper.Multiple;
			results = Factory.Load<ForwardingShipment>(filterStripBizObj.Filter);
			AssertEquals(1, results.Length);
			AssertCollectionContains(shipmentWithMultipleMatchS2_S3, results);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter.Property = USISF.DispositionCodeList.Codes.S2;
			results = Factory.Load<ForwardingShipment>(filterStripBizObj.Filter);
			AssertEquals(14 + existingShipments.Length, results.Length);
			AssertCollectionContains(shipmentMatchS3, results);
			AssertCollectionContains(shipmentMatchS4, results);
			AssertCollectionContains(secondShipmentMatchS3, results);
			AssertCollectionContains(shipmentWithMultipleMatchS2_S3, results);
			AssertCollectionContains(shipmentTooOld, results);
			AssertCollectionContains(shipmentWithNoISFMatch, results);
			AssertCollectionContains("Air shipment are not valid for ISF Bill Status", airShipment, results);
			AssertCollectionContains(directShipmentMatchS4, results);
			AssertCollectionContains(directShipmentMatchS3WithSCAC, results);
			AssertCollectionContains(directShipmentWithAirConsol, results);
			AssertCollectionContains(shipmentWithNonDirectConsol, results);
			AssertCollectionContains(shipmentWithDirectConsol, results);
			AssertCollectionContains(airSeaShipment, results);
			AssertCollectionContains(seaAirShipment, results);
		}

		public void TestISFBillStatusFilter_UseSCACFromBillIssuingParty()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TEST1";
			org.OH_FullName = "Test House Bill Issuing Party";
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "ABCD", Core.Constants.CountryCodes.UnitedStates);

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_SystemCreateTimeUtc = ZDateTime.Now;
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment1.JS_HouseBill = "BILL2";
			shipment1.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var consol = shipment2.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "BILL3";
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
			shipment2.JS_HouseBill = ZString.Empty;

			var isfCreateTime = ZDateTime.Now;
			CreateISFJob(isfCreateTime.AddSeconds(-10), "ABCDBILL2", USISF.BillTypeList.Codes.HouseBillOfLading, USISF.DispositionCodeList.Codes.S2);
			CreateISFJob(isfCreateTime.AddSeconds(-10), "ABCDBILL3", USISF.BillTypeList.Codes.OceanBillOfLading, USISF.DispositionCodeList.Codes.S2);

			Factory.Save();

			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBizObj["ISF Bill Status"];
			filter.IsActive = true;
			filter.Property = USISF.DispositionCodeList.Codes.S2;

			var results = Factory.Load<ForwardingShipment>(filterStripBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, results);
		}

		public void TestISFBillStatusFilter_UseSCACFromSendingForwarder()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TEST1";
			org.OH_FullName = "Test Sending Forwarder";
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "ABCD", Core.Constants.CountryCodes.UnitedStates);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "MATERBILL1";
			consol.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			shipment.JS_HouseBill = "HOUSEBILL1";

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var consol2 = shipment2.Consols.AddNew();
			consol2.JK_AgentType = Core.Constants.AgentType.Direct;
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_MasterBillNum = "MASTERBILL2";
			consol2.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			shipment2.JS_HouseBill = "HOUSEBILL2";

			var isfCreateTime = ZDateTime.Now;
			CreateISFJob(isfCreateTime.AddSeconds(-10), "ABCDHOUSEBILL1", USISF.BillTypeList.Codes.HouseBillOfLading, USISF.DispositionCodeList.Codes.S2);
			CreateISFJob(isfCreateTime.AddSeconds(-10), "ABCDMASTERBILL2", USISF.BillTypeList.Codes.OceanBillOfLading, USISF.DispositionCodeList.Codes.S2);

			Factory.Save();

			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBizObj["ISF Bill Status"];
			filter.IsActive = true;
			filter.Property = USISF.DispositionCodeList.Codes.S2;

			var results = Factory.Load<ForwardingShipment>(filterStripBizObj.Filter);
			AssertEquals("shipment should be searched out, while shipment2 should not be searched out", 1, results.Length);
			AssertCollectionContains(shipment, results);
		}

		Integration.Customs.US.ISF.ICusISFHeader CreateISFJob(ZDateTime createTime, ZString billNumber, ZString billType, ZString customsStatus)
		{
			var header = Factory.New<Integration.Customs.US.ISF.ICusISFHeader>();
			header.BF_SystemCreateTimeUtc = createTime;
			var bill = CreateISFBill(header.PK, billNumber, billType, customsStatus);
			return header;
		}

		Integration.Customs.US.ISF.ICusISFBill CreateISFBill(ZGuid headerPK, ZString billNumber, ZString billType, ZString customsStatus)
		{
			var bill = Factory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill.BB_BF = headerPK;
			bill.BB_BillType = billType;
			bill.BB_BillNum = billNumber;
			bill.BB_CustomsStatus = customsStatus;
			return bill;
		}

		#endregion

		#region AFR Bill Status

		public void TestAFRBillStatusFilter()
		{
			var shipment0 = CreateAFRTestingShipment("HB0");
			var consol0 = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			consol0.Shipments.Add(shipment0);

			var shipment00 = CreateAFRTestingShipment("HB00");
			var consol00 = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			consol00.Shipments.Add(shipment00);
			var header00 = CreateAFRTestingHeader(consol00);

			var shipment1 = CreateAFRTestingShipment("HB1");
			var consol1 = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			consol1.Shipments.Add(shipment1);
			var header1 = CreateAFRTestingHeader(consol1);
			var bill1 = CreateAFRTestingBill(header1, string.Empty, "HB1");

			var shipment2 = CreateAFRTestingShipment("HB2");
			var consol2 = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			consol2.Shipments.Add(shipment2);
			var header2 = CreateAFRTestingHeader(consol2);
			var bill21 = CreateAFRTestingBill(header2, string.Empty, "HB2");
			var bill22 = CreateAFRTestingBill(header2, AFRBillCustomsStatusList.Codes.NotRegistered, "xxxxHB2");

			var shipment3 = CreateAFRTestingShipment("HB3");
			var consol3 = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			consol3.Shipments.Add(shipment3);
			var header3 = CreateAFRTestingHeader(consol3);
			var bill31 = CreateAFRTestingBill(header3, string.Empty, "HB3");
			var bill32 = CreateAFRTestingBill(header3, AFRBillCustomsStatusList.Codes.Registered, "xxxHB3");

			var shipment4 = CreateAFRTestingShipment("HB4");
			var consol41 = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			consol41.Shipments.Add(shipment4);
			var header41 = CreateAFRTestingHeader(consol41);
			var bill41 = CreateAFRTestingBill(header41, string.Empty, "xxxxHB4");
			var consol42 = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			consol42.Shipments.Add(shipment4);
			var header42 = CreateAFRTestingHeader(consol42);
			var bill42 = CreateAFRTestingBill(header42, AFRBillCustomsStatusList.Codes.Registered, "xxxxHB4");

			var shipment5 = CreateAFRTestingShipment("HB5");
			var consol51 = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			consol51.Shipments.Add(shipment5);
			var header51 = CreateAFRTestingHeader(consol51);
			var bill51 = CreateAFRTestingBill(header51, string.Empty, "xxxxHB5");
			var consol52 = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			consol52.Shipments.Add(shipment5);
			var header52 = CreateAFRTestingHeader(consol52);
			var bill52 = CreateAFRTestingBill(header52, AFRBillCustomsStatusList.Codes.NotRegistered, "xxxxHB5");

			var shipment6 = CreateAFRTestingShipment("HB6");
			var consol61 = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			consol61.Shipments.Add(shipment6);
			var header61 = CreateAFRTestingHeader(consol61);
			var bill61 = CreateAFRTestingBill(header61, string.Empty, "xxxxHB6");
			var consol62 = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			consol62.Shipments.Add(shipment6);
			var header62 = CreateAFRTestingHeader(consol62);
			var bill621 = CreateAFRTestingBill(header62, AFRBillCustomsStatusList.Codes.NotRegistered, "xxxxHB6");
			var bill622 = CreateAFRTestingBill(header62, AFRBillCustomsStatusList.Codes.Registered, "xxxxHB6");
			var bill623 = CreateAFRTestingBill(header62, AFRBillCustomsStatusList.Codes.HLD, "xxxxHB6");
			var bill624 = CreateAFRTestingBill(header62, AFRBillCustomsStatusList.Codes.DoNotLoad, "xxxxHB6");
			var bill625 = CreateAFRTestingBill(header62, AFRBillCustomsStatusList.Codes.DoNotUnload, "xxxxHB6");

			var shipment7 = CreateAFRTestingShipment("HB7");

			var shipment8 = CreateAFRTestingShipment("HB8");
			var consol81 = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			consol81.Shipments.Add(shipment8);
			var consol82 = CreateAFRTestingConsol(Core.Constants.TransportModes.Air, "JPTKY");
			consol82.Shipments.Add(shipment8);

			var shipment9 = CreateAFRTestingShipment("HB9");
			var consol91 = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			consol91.Shipments.Add(shipment9);
			var consol92 = CreateAFRTestingConsol(Core.Constants.TransportModes.Sea, "JPTKY");
			consol92.Shipments.Add(shipment9);
			var header92 = CreateAFRTestingHeader(consol92);
			var bill921 = CreateAFRTestingBill(header92, AFRBillCustomsStatusList.Codes.Registered, "xxxxHB9");

			Factory.Save();
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBizObj["AFR Bill Status"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			CombineAssertions(() =>
			{
				filter.Property = AFRBillCustomsStatusList.Codes.NotRegistered;
				var result = Factory.Load<ForwardingShipment>(filter.Query);
				AssertEquals(7, result.Length);
				AssertCollectionContains(shipment0, result);
				AssertCollectionContains(shipment00, result);
				AssertCollectionContains(shipment1, result);
				AssertCollectionContains(shipment2, result);
				AssertCollectionContains(shipment3, result);
				AssertCollectionContains(shipment5, result);
				AssertCollectionContains(shipment8, result);

				filter.Property = AFRBillCustomsStatusList.Codes.Registered;
				result = Factory.Load<ForwardingShipment>(filter.Query);
				AssertEquals("Equal REG", 1, result.Length);
				AssertCollectionContains(shipment9, result);

				filter.Property = AFRBillCustomsStatusList.Codes.HLD;
				result = Factory.Load<ForwardingShipment>(filter.Query);
				AssertEquals("Equal HLD", 0, result.Length);

				filter.Property = AFRBillCustomsStatusList.Codes.DoNotLoad;
				result = Factory.Load<ForwardingShipment>(filter.Query);
				AssertEquals("Equal DNL", 0, result.Length);

				filter.Property = AFRBillCustomsStatusList.Codes.DoNotUnload;
				result = Factory.Load<ForwardingShipment>(filter.Query);
				AssertEquals("Equal DNU", 0, result.Length);
			});

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			CombineAssertions(() =>
			{
				filter.Property = AFRBillCustomsStatusList.Codes.NotRegistered;
				var result = Factory.Load<ForwardingShipment>(filter.Query);
				AssertEquals("Contain NOT", 10, result.Length);
				AssertCollectionContains(shipment0, result);
				AssertCollectionContains(shipment00, result);
				AssertCollectionContains(shipment1, result);
				AssertCollectionContains(shipment2, result);
				AssertCollectionContains(shipment3, result);
				AssertCollectionContains(shipment4, result);
				AssertCollectionContains(shipment5, result);
				AssertCollectionContains(shipment6, result);
				AssertCollectionContains(shipment8, result);
				AssertCollectionContains(shipment9, result);

				filter.Property = AFRBillCustomsStatusList.Codes.Registered;
				result = Factory.Load<ForwardingShipment>(filter.Query);
				AssertEquals("Contain REG", 3, result.Length);
				AssertCollectionContains(shipment4, result);
				AssertCollectionContains(shipment6, result);
				AssertCollectionContains(shipment9, result);

				filter.Property = AFRBillCustomsStatusList.Codes.HLD;
				result = Factory.Load<ForwardingShipment>(filter.Query);
				AssertEquals("Contain HLD", 1, result.Length);
				AssertCollectionContains(shipment6, result);

				filter.Property = AFRBillCustomsStatusList.Codes.DoNotLoad;
				result = Factory.Load<ForwardingShipment>(filter.Query);
				AssertEquals("Contain DNL", 1, result.Length);
				AssertCollectionContains(shipment6, result);

				filter.Property = AFRBillCustomsStatusList.Codes.DoNotUnload;
				result = Factory.Load<ForwardingShipment>(filter.Query);
				AssertEquals("Contain DNU", 1, result.Length);
				AssertCollectionContains(shipment6, result);
			});
		}

		ForwardingShipment CreateAFRTestingShipment(string billnumber)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = billnumber ?? string.Empty;
			return shipment;
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

		Integration.Customs.JP.AFR.IJPAFRBills CreateAFRTestingBill(Integration.Customs.JP.AFR.IJPAFRHeader header, string billStatus, string billnumber)
		{
			var bill = Factory.New<Integration.Customs.JP.AFR.IJPAFRBills>();
			bill.JPB_JPH_Header = header.PK;
			bill.JPB_ReleaseStatus = billStatus ?? string.Empty;
			bill.JPB_BillNumber = billnumber ?? string.Empty;
			return bill;
		}

		#endregion

		#region TestSGCustomsURN

		public void TestSGCustomsURN()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
				ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
				ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory);

				BusinessObject jobDeclaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Integration.Customs.IBaseJobDeclaration)));
				BusinessObject cusEntryHeader = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Integration.Customs.ICusEntryHeader)));
				EDIMessage message = Factory.New<EDIMessage>();
				ZDBOnlySubQuery declarationQuery = new ZDBOnlySubQuery(typeof(Integration.Customs.IBaseJobDeclaration), JobDeclarationSchema.JE_JS);
				ZDBOnlySubQuery cusEntriesSubQuery = new ZDBOnlySubQuery(typeof(Integration.Customs.ICusEntryHeader), CusEntryHeaderSchema.CH_JE);
				message.EM_ApplicationReference = "TSTREF001";
				message.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
				message.EM_LinkUniqueID = cusEntryHeader.PK;
				message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
				cusEntryHeader[CusEntryHeaderSchema.CH_JE] = jobDeclaration.PK;
				jobDeclaration[JobDeclarationSchema.JE_JS] = shipment1.PK;
				Factory.Save();

				var filterStripBizObj = GetNewFilterStripBusinessObject();
				var filter = (ModuleTextFilter)filterStripBizObj["Customs URN"];
				filter.Property = "TSTREF001";
				filter.IsActive = true;
				shipments.Load(filterStripBizObj.Filter);
				AssertEquals("Should contain shipment1", true, shipments.Contains(shipment1));
				AssertEquals("Should not contain shipment2", false, shipments.Contains(shipment2));
				filter.IsActive = false;
				shipments.Load(filterStripBizObj.Filter);
				AssertEquals("Should contain shipment2", true, shipments.Contains(shipment2));
				Assert("Should contain 2 shipments or more", shipments.Count > 1);
			}
		}

		#endregion

		#region Entry Number #

		public void TestCustomsEntryNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				ZString manuallyGeneratedMarker = "Manually generated";

				Action<BusinessObject, string, string> createCusEntryNumber = (parent, entryNumValue, entryCategory) =>
				{
					CusEntryNumber result = Factory.New<CusEntryNumber>();
					result.Parent = parent;
					result.CE_EntryNum = entryNumValue;
					result.CE_Category = entryCategory;
					result.CE_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;

					if (result.CE_EntryNum == manuallyGeneratedMarker || (parent is ForwardingShipment && result.CE_Category == CusEntryNumber.Categories.CustomsPermitClearanceNumber))
					{
						result.CE_EntryIsSystemGenerated = false;
					}
				};

				createCusEntryNumber(shipment1, "AAA", CusEntryNumber.Categories.CustomsPermitClearanceNumber);
				createCusEntryNumber(shipment2, "AAA", CusEntryNumber.Categories.AdditionalReferenceNumber);
				createCusEntryNumber(shipment3, "", CusEntryNumber.Categories.CustomsPermitClearanceNumber);
				createCusEntryNumber(shipment4, "", CusEntryNumber.Categories.AdditionalReferenceNumber);
				createCusEntryNumber(shipment5, manuallyGeneratedMarker, CusEntryNumber.Categories.CustomsPermitClearanceNumber);

				var shipmentWithManualCusEntryNum = Factory.NewWithValidTestData<ForwardingShipment>();
				var manualDeclaration = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
				manualDeclaration[JobDeclarationSchema.JE_JS] = shipmentWithManualCusEntryNum.PK;
				createCusEntryNumber(manualDeclaration, manuallyGeneratedMarker, CusEntryNumber.Categories.CustomsPermitClearanceNumber);

				var shipmentWithDeclaration = Factory.NewWithValidTestData<ForwardingShipment>();
				var declaration = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
				declaration[JobDeclarationSchema.JE_JS] = shipmentWithDeclaration.PK;
				createCusEntryNumber(declaration, "BBB", CusEntryNumber.Categories.CustomsPermitClearanceNumber);

				var shipmentWithDeclarationWithEntryHeader = Factory.NewWithValidTestData<ForwardingShipment>();
				var declarationWithEntryHeader = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
				declarationWithEntryHeader[JobDeclarationSchema.JE_JS] = shipmentWithDeclarationWithEntryHeader.PK;
				var entryHeader = (BusinessObject)Factory.New<Integration.Customs.ICusEntryHeader>();
				entryHeader[CusEntryHeaderSchema.CH_JE] = declarationWithEntryHeader.PK;
				createCusEntryNumber(entryHeader, "CCC", CusEntryNumber.Categories.CustomsPermitClearanceNumber);

				var shipmentForBlankJE_JS = Factory.NewWithValidTestData<ForwardingShipment>();
				var declarationForBlankJE_JS = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
				declarationForBlankJE_JS[JobDeclarationSchema.JE_JS] = null;//shipmentWithDeclarationWithEntryHeader.PK;
				var entryHeaderForBlankJE_JS = (BusinessObject)Factory.New<Integration.Customs.ICusEntryHeader>();
				entryHeaderForBlankJE_JS[CusEntryHeaderSchema.CH_JE] = declarationForBlankJE_JS.PK;
				createCusEntryNumber(entryHeaderForBlankJE_JS, "CCC", CusEntryNumber.Categories.CustomsPermitClearanceNumber);

				var shipmentWithEntryException = Factory.NewWithValidTestData<ForwardingShipment>();
				var entryException = Factory.New<CusEntryNumber>();
				entryException.CE_EntryIsSystemGenerated = false;
				entryException.Parent = shipmentWithEntryException;
				entryException.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				entryException.CE_EntryType = CMRExportExemptionCodes.Get3CharCode(CMRExportExemptionCodes.EXSP.Code);

				var shipmentWithNonCusDeclaration = Factory.NewWithValidTestData<ForwardingShipment>();
				var otherDeclaration = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
				otherDeclaration[JobDeclarationSchema.JE_JS] = shipmentWithNonCusDeclaration.PK;
				createCusEntryNumber(otherDeclaration, "DDD", CusEntryNumber.Categories.AdditionalReferenceNumber);

				var shipmentWithEmptyEntryOtherCountry = Factory.NewWithValidTestData<ForwardingShipment>();
				var emptyEntryOtherCountry = Factory.New<CusEntryNumber>();
				emptyEntryOtherCountry.Parent = shipmentWithEmptyEntryOtherCountry;
				emptyEntryOtherCountry.CE_EntryNum = "";
				emptyEntryOtherCountry.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				emptyEntryOtherCountry.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

				var shipmentWithEntryOtherCountry = Factory.NewWithValidTestData<ForwardingShipment>();
				var entryOtherCountry = Factory.New<CusEntryNumber>();
				entryOtherCountry.Parent = shipmentWithEntryOtherCountry;
				entryOtherCountry.CE_EntryNum = "Other";
				entryOtherCountry.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				entryOtherCountry.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

				Factory.Save();

				Action<string, SQLComparisonOperator, BusinessObject[]> assertFiltering = (entryNumber, comparisonOperator, expectedShipments) =>
				{
					var filterStripBizObj = GetNewFilterStripBusinessObject();
					var filter = (ModuleTextFilter)filterStripBizObj["Customs Entry #"];
					filter.Property = entryNumber;
					filter.IsActive = true;
					filter.SqlComparisonOperator = comparisonOperator;

					ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory);
					shipments.Load(filterStripBizObj.Filter);

					AssertContainsExactElementsInAnyOrder(expectedShipments, shipments);
				};

				assertFiltering("AAA", SQLComparisonOperator.StartsWith, new BusinessObject[] { shipment1 });
				assertFiltering("BBB", SQLComparisonOperator.Equal, new BusinessObject[] { shipmentWithDeclaration });
				assertFiltering("CCC", SQLComparisonOperator.StartsWith, new BusinessObject[] { shipmentWithDeclarationWithEntryHeader });
				assertFiltering("XXX", SQLComparisonOperator.Contains, Array.Empty<BusinessObject>());
				assertFiltering(ZString.Empty, SpecialComparisonOperator.IsNotBlank, new BusinessObject[] { shipment1, shipment5, shipmentWithDeclaration, shipmentWithDeclarationWithEntryHeader });
				assertFiltering(ZString.Empty, SpecialComparisonOperator.IsBlank, new BusinessObject[] { shipment2, shipment3, shipment4, shipment6, shipmentWithManualCusEntryNum, shipmentWithEntryException, shipmentWithNonCusDeclaration, shipmentForBlankJE_JS, shipmentWithEmptyEntryOtherCountry, shipmentWithEntryOtherCountry });
			}
		}

		#endregion

		#region Not published on Web Filters

		public void TestNotPublishedOnWebFilters()
		{
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			AssertEquals(false, filterStripBizObj["Customs Entry #"].IsPublishedOnWeb);
		}

		#endregion

		#region TestCountrySpecificFilterList

		public void TestCountrySpecificFilterList()
		{
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			AssertNotNull(filterStripBizObj["Air Cargo Message Status"]);
			AssertNotNull(filterStripBizObj["Air Cargo Customs Status"]);
			AssertNotNull(filterStripBizObj["Sea Cargo Message Status"]);
			AssertNotNull(filterStripBizObj["Sea Cargo Customs Status"]);
			ZString originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			try
			{
				filterStripBizObj = GetNewFilterStripBusinessObject();
				AssertNull(filterStripBizObj["Air Cargo Message Status"]);
				AssertNull(filterStripBizObj["Air Cargo Customs Status"]);
				AssertNull(filterStripBizObj["Sea Cargo Message Status"]);
				AssertNull(filterStripBizObj["Sea Cargo Customs Status"]);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = originalCountryCode;
			}
		}

		#endregion

		#region Air Cargo Filter Tests

		public void TestAirCargoNotClearCargoStatus()
		{
			SetupAirCargoTestData();
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBizObj["Air Cargo Customs Status"];
			filter.Property = "NCL";
			filter.IsActive = true;

			var shipments = new ForwardingShipmentCollection(Factory, filterStripBizObj.Filter);
			shipments.Load();

			Assert("Expect collection not to contain Shipment1", !shipments.Contains(shipment1));
			Assert("Expect collection not to contain Shipment2", !shipments.Contains(shipment2));
			Assert("Expect collection not to contain Shipment3", !shipments.Contains(shipment3));
			Assert("Expect collection to contain Shipment4", shipments.Contains(shipment4));
		}

		public void TestAirCargoClearCargoStatus()
		{
			SetupAirCargoTestData();
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBizObj["Air Cargo Customs Status"];
			filter.Property = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			filter.IsActive = true;

			var shipments = new ForwardingShipmentCollection(Factory, filterStripBizObj.Filter);
			shipments.Load();

			Assert("Expect collection to contain Shipment1", shipments.Contains(shipment1));
			Assert("Expect collection not to contain Shipment2", !shipments.Contains(shipment2));
			Assert("Expect collection not to contain Shipment3", !shipments.Contains(shipment3));
			Assert("Expect collection not to contain Shipment4", !shipments.Contains(shipment4));
		}

		public void TestAirCargoMessageStatusACO()
		{
			SetupAirCargoTestData();
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBizObj["Air Cargo Message Status"];
			filter.Property = CMRBaseStatuses.Codes.OriginalAccepted;
			filter.IsActive = true;

			var shipments = new ForwardingShipmentCollection(Factory, filterStripBizObj.Filter);
			shipments.Load();

			Assert("Expect collection to contain Shipment1", shipments.Contains(shipment1));
			Assert("Expect collection to contain Shipment2", shipments.Contains(shipment2));
			Assert("Expect collection to contain Shipment3", shipments.Contains(shipment3));
			Assert("Expect collection to contain Shipment4", shipments.Contains(shipment4));
		}

		public void TestAirCargoMessageStatusNotSent()
		{
			SetupAirCargoTestData();
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBizObj["Air Cargo Message Status"];
			filter.Property = CMRBaseStatuses.Codes.NotSent;
			filter.IsActive = true;

			var shipments = new ForwardingShipmentCollection(Factory, filterStripBizObj.Filter);
			shipments.Load();

			Assert("Expect collection not to contain Shipment1", !shipments.Contains(shipment1));
			Assert("Expect collection not to contain Shipment2", !shipments.Contains(shipment2));
			Assert("Expect collection not to contain Shipment3", !shipments.Contains(shipment3));
			Assert("Expect collection not to contain Shipment4", !shipments.Contains(shipment4));
		}

		public void TestAirCargoMessageStatusWithMultipleFilters()
		{
			SetupAirCargoTestData();

			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBizObj["Air Cargo Message Status"];
			var filter1 = (ModuleTextFilter)filterStripBizObj["Air Cargo Message Status"];
			filter.Property = CMRBaseStatuses.Codes.OriginalAccepted;

			ZQuery query = new ZQuery();
			query.AddToFilter(filter.Query);
			filter.IsActive = true;
			filter1.Property = CMRBaseStatuses.Codes.AmendmentAccepted;
			filter1.IsActive = true;
			query.AddToFilter(filter1.Query, JoinCondition.Or);

			ForwardingShipment[] results = Factory.Load<ForwardingShipment>(query);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2, shipment3, shipment4, shipment6 }, results);

			filter.Property = CMRBaseStatuses.Codes.OriginalAccepted;

			query = new ZQuery();
			query.AddToFilter(filter.Query);
			filter.IsActive = true;
			filter1.Property = CMRBaseStatuses.Codes.AmendmentAccepted;
			filter1.IsActive = true;
			query.AddToFilter(filter1.Query, JoinCondition.And);

			results = Factory.Load<ForwardingShipment>(query);
			AssertCollectionNotContains("No Shipment is filtered", shipment1, results);
			AssertCollectionNotContains("No Shipment is filtered", shipment2, results);
			AssertCollectionNotContains("No Shipment is filtered", shipment3, results);
			AssertCollectionNotContains("No Shipment is filtered", shipment4, results);
			AssertCollectionNotContains("No Shipment is filtered", shipment6, results);
		}

		#endregion

		#region Sea Cargo Filter Tests

		public void TestSeaCargoNotClearCargoStatus()
		{
			SetupSeaCargoTestData();
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBizObj["Sea Cargo Customs Status"];
			filter.Property = "NCL";
			filter.IsActive = true;

			var shipments = new ForwardingShipmentCollection(Factory, filterStripBizObj.Filter);
			shipments.Load();

			Assert("Expect collection not to contain Shipment1", !shipments.Contains(shipment1));
			Assert("Expect collection not to contain Shipment2", !shipments.Contains(shipment2));
			Assert("Expect collection not to contain Shipment3", !shipments.Contains(shipment3));
			Assert("Expect collection to contain Shipment4", shipments.Contains(shipment4));
		}

		public void TestSeaCargoClearCargoStatus()
		{
			SetupSeaCargoTestData();
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBizObj["Sea Cargo Customs Status"];
			filter.Property = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			filter.IsActive = true;

			var shipments = new ForwardingShipmentCollection(Factory, filterStripBizObj.Filter);
			shipments.Load();

			Assert("Expect collection not to contain Shipment1", shipments.Contains(shipment1));
			Assert("Expect collection not to contain Shipment2", !shipments.Contains(shipment2));
			Assert("Expect collection not to contain Shipment3", !shipments.Contains(shipment3));
			Assert("Expect collection to contain Shipment4", !shipments.Contains(shipment4));
		}

		public void TestSeaCargoMessageStatusACO()
		{
			SetupSeaCargoTestData();
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBizObj["Sea Cargo Message Status"];
			filter.Property = CMRBaseStatuses.Codes.OriginalAccepted;
			filter.IsActive = true;

			var shipments = new ForwardingShipmentCollection(Factory, filterStripBizObj.Filter);
			shipments.Load();

			Assert("Expect collection not to contain Shipment1", shipments.Contains(shipment1));
			Assert("Expect collection not to contain Shipment2", shipments.Contains(shipment2));
			Assert("Expect collection not to contain Shipment3", shipments.Contains(shipment3));
			Assert("Expect collection to contain Shipment4", shipments.Contains(shipment4));
		}

		public void TestSeaCargoMessageStatusNotSent()
		{
			SetupSeaCargoTestData();
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBizObj["Sea Cargo Message Status"];
			filter.Property = CMRBaseStatuses.Codes.NotSent;
			filter.IsActive = true;

			var shipments = new ForwardingShipmentCollection(Factory, filterStripBizObj.Filter);
			shipments.Load();

			Assert("Expect collection not to contain Shipment1", !shipments.Contains(shipment1));
			Assert("Expect collection not to contain Shipment2", !shipments.Contains(shipment2));
			Assert("Expect collection not to contain Shipment3", !shipments.Contains(shipment3));
			Assert("Expect collection to contain Shipment4", !shipments.Contains(shipment4));
		}

		#endregion

		FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new DummyFilterStripBusinessObject();
		}

		void SetupSeaCargoTestData()
		{
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;

			cusOBL = (BusinessObject)Factory.New<Integration.Customs.AU.ICusSCAOceanBill>();

			var container = (BusinessObject)Factory.New<Integration.Customs.AU.ICusSCAContainer>();
			container[CusSCAContainerSchema.CN_CB] = cusOBL.PK;

			var cusSCAHouse1 = (BusinessObject)Factory.New<Integration.Customs.AU.ICusSCAHouse>();
			cusSCAHouse1[CusSCAHouseSchema.Constants.CA_HouseBill] = "H1";
			cusSCAHouse1[CusSCAHouseSchema.Constants.CA_JS] = shipment1.PK;
			cusSCAHouse1[CusSCAHouseSchema.Constants.CA_CB] = cusOBL.PK;
			cusSCAHouse1[CusSCAHouseSchema.Constants.CA_MessageStatus] = CMRBaseStatuses.Codes.OriginalAccepted;
			var cusSCAPivot1 = (BusinessObject)Factory.New<Integration.Customs.AU.ICusSCAPivot>();
			cusSCAPivot1[CusSCAPivotSchema.Constants.CV_CA] = cusSCAHouse1.PK;
			cusSCAPivot1[CusSCAPivotSchema.Constants.CV_CN] = container.PK;
			cusSCAPivot1[CusSCAPivotSchema.Constants.CV_CargoStatus] = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;

			var cusSCAHouse2 = (BusinessObject)Factory.New<Integration.Customs.AU.ICusSCAHouse>();
			cusSCAHouse2[CusSCAHouseSchema.Constants.CA_HouseBill] = "H2";
			cusSCAHouse2[CusSCAHouseSchema.Constants.CA_JS] = shipment2.PK;
			cusSCAHouse2[CusSCAHouseSchema.Constants.CA_CB] = cusOBL.PK;
			cusSCAHouse2[CusSCAHouseSchema.Constants.CA_MessageStatus] = CMRBaseStatuses.Codes.OriginalAccepted;
			var cusSCAPivot2 = (BusinessObject)Factory.New<Integration.Customs.AU.ICusSCAPivot>();
			cusSCAPivot2[CusSCAPivotSchema.Constants.CV_CN] = container.PK;
			cusSCAPivot2[CusSCAPivotSchema.Constants.CV_CA] = cusSCAHouse2.PK;
			cusSCAPivot2[CusSCAPivotSchema.Constants.CV_CargoStatus] = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;

			var cusSCAHouse3 = (BusinessObject)Factory.New<Integration.Customs.AU.ICusSCAHouse>();
			cusSCAHouse3[CusSCAHouseSchema.Constants.CA_HouseBill] = "H3";
			cusSCAHouse3[CusSCAHouseSchema.Constants.CA_JS] = shipment3.PK;
			cusSCAHouse3[CusSCAHouseSchema.Constants.CA_CB] = cusOBL.PK;
			cusSCAHouse3[CusSCAHouseSchema.Constants.CA_MessageStatus] = CMRBaseStatuses.Codes.OriginalAccepted;
			var cusSCAPivot3 = (BusinessObject)Factory.New<Integration.Customs.AU.ICusSCAPivot>();
			cusSCAPivot3[CusSCAPivotSchema.Constants.CV_CN] = container.PK;
			cusSCAPivot3[CusSCAPivotSchema.Constants.CV_CA] = cusSCAHouse3.PK;
			cusSCAPivot3[CusSCAPivotSchema.Constants.CV_CargoStatus] = CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;

			var cusSCAHouse4 = (BusinessObject)Factory.New<Integration.Customs.AU.ICusSCAHouse>();
			cusSCAHouse4[CusSCAHouseSchema.Constants.CA_HouseBill] = "H4";
			cusSCAHouse4[CusSCAHouseSchema.Constants.CA_JS] = shipment4.PK;
			cusSCAHouse4[CusSCAHouseSchema.Constants.CA_CB] = cusOBL.PK;
			cusSCAHouse4[CusSCAHouseSchema.Constants.CA_ShipmentStatus] = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			cusSCAHouse4[CusSCAHouseSchema.Constants.CA_MessageStatus] = CMRBaseStatuses.Codes.OriginalAccepted;
			var cusSCAPivot4 = (BusinessObject)Factory.New<Integration.Customs.AU.ICusSCAPivot>();
			cusSCAPivot4[CusSCAPivotSchema.Constants.CV_CN] = container.PK;
			cusSCAPivot4[CusSCAPivotSchema.Constants.CV_CA] = cusSCAHouse4.PK;
			cusSCAPivot4[CusSCAPivotSchema.Constants.CV_CargoStatus] = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;

			Factory.Save();
		}

		void SetupAirCargoTestData()
		{
			consol1.JK_TransportMode = Core.Constants.TransportModes.Air;

			cusMAWB1 = (BusinessObject)Factory.New<Integration.Customs.AU.ICusMAWB>();

			cusHAWB1 = (BusinessObject)Factory.New<Integration.Customs.AU.ICusHAWB>();

			cusHAWB1[CusHAWBSchema.Constants.CS_HAWB] = "HAWB1";
			cusHAWB1[CusHAWBSchema.Constants.CS_JS] = shipment1.PK;
			cusHAWB1[CusHAWBSchema.Constants.CS_CM] = cusMAWB1.PK;
			cusHAWB1[CusHAWBSchema.Constants.CS_CustomsStatus] = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			cusHAWB1[CusHAWBSchema.Constants.CS_MsgStatus] = CMRBaseStatuses.Codes.OriginalAccepted;

			cusHAWB2 = (BusinessObject)Factory.New<Integration.Customs.AU.ICusHAWB>();

			cusHAWB2[CusHAWBSchema.Constants.CS_HAWB] = "HAWB2";
			cusHAWB2[CusHAWBSchema.Constants.CS_JS] = shipment2.PK;
			cusHAWB2[CusHAWBSchema.Constants.CS_CM] = cusMAWB1.PK;
			cusHAWB2[CusHAWBSchema.Constants.CS_CustomsStatus] = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;
			cusHAWB2[CusHAWBSchema.Constants.CS_MsgStatus] = CMRBaseStatuses.Codes.OriginalAccepted;

			cusHAWB3 = (BusinessObject)Factory.New<Integration.Customs.AU.ICusHAWB>();

			cusHAWB3[CusHAWBSchema.Constants.CS_HAWB] = "HAWB3";
			cusHAWB3[CusHAWBSchema.Constants.CS_JS] = shipment3.PK;
			cusHAWB3[CusHAWBSchema.Constants.CS_CM] = cusMAWB1.PK;
			cusHAWB3[CusHAWBSchema.Constants.CS_CustomsStatus] = CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;
			cusHAWB3[CusHAWBSchema.Constants.CS_MsgStatus] = CMRBaseStatuses.Codes.OriginalAccepted;

			cusHAWB4 = (BusinessObject)Factory.New<Integration.Customs.AU.ICusHAWB>();

			cusHAWB4[CusHAWBSchema.Constants.CS_HAWB] = "HAWB4";
			cusHAWB4[CusHAWBSchema.Constants.CS_JS] = shipment4.PK;
			cusHAWB4[CusHAWBSchema.Constants.CS_CM] = cusMAWB1.PK;
			cusHAWB4[CusHAWBSchema.Constants.CS_CustomsStatus] = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			cusHAWB4[CusHAWBSchema.Constants.CS_MsgStatus] = CMRBaseStatuses.Codes.OriginalAccepted;

			cusHAWB5 = (BusinessObject)Factory.New<Integration.Customs.AU.ICusHAWB>();

			cusHAWB5[CusHAWBSchema.Constants.CS_HAWB] = "HAWB5";
			cusHAWB5[CusHAWBSchema.Constants.CS_JS] = shipment6.PK;
			cusHAWB5[CusHAWBSchema.Constants.CS_CM] = cusMAWB1.PK;
			cusHAWB5[CusHAWBSchema.Constants.CS_CustomsStatus] = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			cusHAWB5[CusHAWBSchema.Constants.CS_MsgStatus] = CMRBaseStatuses.Codes.AmendmentAccepted;

			Factory.Save();
		}

		void CreateUSExportEntryJobs()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var usTestBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			usTestBranch1.GB_Code = "CHI";
			usTestBranch1.GB_BranchName = "CHICAGO";
			usTestBranch1.GB_IsActive = true;

			var decType = ObjectFactory.GetType<Integration.Customs.IBaseJobDeclaration>();
			var entryType = ObjectFactory.GetType<Integration.Customs.ICusEntryHeader>();

			uSShipment1 = Factory.New<ForwardingShipment>();

			uSShipment2 = Factory.New<ForwardingShipment>();
			var declaration2 = Factory.New(decType);
			declaration2[JobDeclarationSchema.JE_JS] = uSShipment2.PK;
			declaration2[JobDeclarationSchema.JE_GB] = usTestBranch1.PK;
			declaration2[JobDeclarationSchema.JE_MessageType] = "EXP";
			var entry2 = Factory.New(entryType);
			entry2[CusEntryHeaderSchema.CH_JE] = declaration2.PK;
			entry2[CusEntryHeaderSchema.CH_MessageType] = "ITN";
			entry2[CusEntryHeaderSchema.CH_Status] = "OSC";

			uSShipment3 = Factory.New<ForwardingShipment>();
			var declaration3 = Factory.New(decType);
			declaration3[JobDeclarationSchema.JE_JS] = uSShipment3.PK;
			declaration3[JobDeclarationSchema.JE_GB] = usTestBranch1.PK;
			declaration3[JobDeclarationSchema.JE_MessageType] = "EXP";
			var entry3 = Factory.New(entryType);
			entry3[CusEntryHeaderSchema.CH_JE] = declaration3.PK;
			entry3[CusEntryHeaderSchema.CH_MessageType] = "ITN";
			entry3[CusEntryHeaderSchema.CH_Status] = "RSC";

			uSShipment4 = Factory.New<ForwardingShipment>();
			var declaration4 = Factory.New(decType);
			declaration4[JobDeclarationSchema.JE_JS] = uSShipment4.PK;
			declaration4[JobDeclarationSchema.JE_GB] = usTestBranch1.PK;
			declaration4[JobDeclarationSchema.JE_MessageType] = "EXP";
			var entry4 = Factory.New(entryType);
			entry4[CusEntryHeaderSchema.CH_JE] = declaration4.PK;
			entry4[CusEntryHeaderSchema.CH_MessageType] = "ITN";
			entry4[CusEntryHeaderSchema.CH_Status] = "";

			uSShipment5 = Factory.New<ForwardingShipment>();
			var declaration5 = Factory.New(decType);
			declaration5[JobDeclarationSchema.JE_JS] = uSShipment5.PK;
			declaration5[JobDeclarationSchema.JE_GB] = usTestBranch1.PK;
			declaration5[JobDeclarationSchema.JE_MessageType] = "EXP";
			var entry5_1 = Factory.New(entryType);
			entry5_1[CusEntryHeaderSchema.CH_JE] = declaration5.PK;
			entry5_1[CusEntryHeaderSchema.CH_MessageType] = "ITN";
			entry5_1[CusEntryHeaderSchema.CH_Status] = "OSC";
			var entry5_2 = Factory.New(entryType);
			entry5_2[CusEntryHeaderSchema.CH_JE] = declaration5.PK;
			entry5_2[CusEntryHeaderSchema.CH_MessageType] = "ITN";
			entry5_2[CusEntryHeaderSchema.CH_Status] = "ARR";

			uSShipment6 = Factory.New<ForwardingShipment>();
			var declaration6 = Factory.New(decType);
			declaration6[JobDeclarationSchema.JE_JS] = uSShipment6.PK;
			declaration6[JobDeclarationSchema.JE_GB] = usTestBranch1.PK;
			declaration6[JobDeclarationSchema.JE_MessageType] = "EXP";
			var entry6_1 = Factory.New(entryType);
			entry6_1[CusEntryHeaderSchema.CH_JE] = declaration6.PK;
			entry6_1[CusEntryHeaderSchema.CH_MessageType] = "ITN";
			entry6_1[CusEntryHeaderSchema.CH_Status] = "OSC";
			var entry6_2 = Factory.New(entryType);
			entry6_2[CusEntryHeaderSchema.CH_JE] = declaration6.PK;
			entry6_2[CusEntryHeaderSchema.CH_MessageType] = "ITN";
			entry6_2[CusEntryHeaderSchema.CH_Status] = "OSC";

			uSShipment7 = Factory.New<ForwardingShipment>();
			var declaration7 = Factory.New(decType);
			declaration7[JobDeclarationSchema.JE_JS] = uSShipment7.PK;
			declaration7[JobDeclarationSchema.JE_GB] = usTestBranch1.PK;
			declaration7[JobDeclarationSchema.JE_MessageType] = "EXP";
			var entry7_1 = Factory.New(entryType);
			entry7_1[CusEntryHeaderSchema.CH_JE] = declaration7.PK;
			entry7_1[CusEntryHeaderSchema.CH_MessageType] = "ITN";
			entry7_1[CusEntryHeaderSchema.CH_Status] = "VRN";
			var entry7_2 = Factory.New(entryType);
			entry7_2[CusEntryHeaderSchema.CH_JE] = declaration7.PK;
			entry7_2[CusEntryHeaderSchema.CH_MessageType] = "ITN";
			entry7_2[CusEntryHeaderSchema.CH_Status] = "OSC";

			uSShipment8 = Factory.New<ForwardingShipment>();
			var declaration8 = Factory.New(decType);
			declaration8[JobDeclarationSchema.JE_JS] = uSShipment8.PK;
			declaration8[JobDeclarationSchema.JE_GB] = usTestBranch1.PK;
			declaration8[JobDeclarationSchema.JE_MessageType] = "EXP";
			var entry8 = Factory.New(entryType);
			entry8[CusEntryHeaderSchema.CH_JE] = declaration8.PK;
			entry8[CusEntryHeaderSchema.CH_MessageType] = "ITN";
			entry8[CusEntryHeaderSchema.CH_Status] = "WRN";

			Factory.Save();
		}

		void AssertEntryStatusFilter_FilterTypeVisibilityForCountry(string countryCode, bool expectedVisibility)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var filterStripBizObj = GetNewFilterStripBusinessObject();
				var entryStatusFilter = filterStripBizObj["Customs Entry Status"] as EntryStatusFilter;
				AssertEquals("Customs Entry Status filter should have multilingual description for translation.", "Customs Entry Status", entryStatusFilter.MultilingualDescription.GetUnresolvedString());
				AssertEquals(expectedVisibility, entryStatusFilter.ShowFilterType);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			shipment1 = Factory.New<ForwardingShipment>();
			shipment2 = Factory.New<ForwardingShipment>();
			shipment3 = Factory.New<ForwardingShipment>();
			shipment4 = Factory.New<ForwardingShipment>();
			shipment5 = Factory.New<ForwardingShipment>();
			shipment6 = Factory.New<ForwardingShipment>();

			consol1 = shipment1.Consols.AddNew();
			shipment2.Consols.Add(consol1);
			shipment3.Consols.Add(consol1);
			shipment4.Consols.Add(consol1);
			shipment5.Consols.Add(consol1);
			shipment6.Consols.Add(consol1);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";

			Factory.Save();
		}

		ForwardingConsol consol1;
		protected ForwardingShipment shipment1;
		protected ForwardingShipment shipment2;
		protected ForwardingShipment shipment3;
		protected ForwardingShipment shipment4;
		protected ForwardingShipment shipment5;
		protected ForwardingShipment shipment6;
		BusinessObject cusMAWB1;
		BusinessObject cusHAWB1;
		BusinessObject cusHAWB2;
		BusinessObject cusHAWB3;
		BusinessObject cusHAWB4;
		BusinessObject cusHAWB5;
		BusinessObject cusOBL;

		ForwardingShipment uSShipment1;
		ForwardingShipment uSShipment2;
		ForwardingShipment uSShipment3;
		ForwardingShipment uSShipment4;
		ForwardingShipment uSShipment5;
		ForwardingShipment uSShipment6;
		ForwardingShipment uSShipment7;
		ForwardingShipment uSShipment8;
	}

	class DummyFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			var filtersProvider = new ForwardingShipmentModuleCustomsFiltersProvider(Factory);
			filtersProvider.AddFilters(result);
			return result;
		}
	}
}
