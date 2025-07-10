using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Module.DeclarationFilterConstants;

namespace Enterprise.Customs.CN.Module.Testing
{
	[TestedType(typeof(JobDeclarationFilterBusinessObject))]
	class BaseJobDeclarationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestLookups()
		{
			var filterBizObj = new JobDeclarationFilterBusinessObject();
			AssertEquals("Lookups of correct type", typeof(JobDeclarationFilterLookups), filterBizObj.Lookups.GetType());
		}

		public void TestPSLNumberFilter()
		{
			CheckNumberForEntryTypeFilter(AdditionalReferenceNumberTypes.Codes.ProposalNo, DeclarationFilterConstants.CusEntryNumberFilterTypes.PSLNumber);
		}

		public void TestNoPackagesFilter()
		{
			var filter = new JobDeclarationFilterBusinessObject();
			var queryFilter = (ModuleNumberRangeFilter)filter[DeclarationFilterConstants.NoPackages];
			var declaration1 = Factory.New<JobDeclaration>();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration1.JE_TotalNoOfPacks = 0;
			declaration2.JE_TotalNoOfPacks = 87;
			Factory.Save();
			queryFilter.Decimals = 0;
			queryFilter.BetweenDefaultProperty1 = 0;
			queryFilter.BetweenDefaultProperty2 = 0;
			queryFilter.IsActive = true;
			var declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			queryFilter.BetweenDefaultProperty1 = 50;
			queryFilter.BetweenDefaultProperty2 = 100;
			queryFilter.IsActive = true;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
			queryFilter.BetweenDefaultProperty1 = 100;
			queryFilter.BetweenDefaultProperty2 = 200;
			queryFilter.IsActive = true;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
		}

		public void TestWGQNumberFilter()
		{
			CheckNumberForEntryTypeFilter(AdditionalReferenceNumberTypes.Codes.WGQWarehouseNumber, DeclarationFilterConstants.CusEntryNumberFilterTypes.WGQNumber);
		}

		public void TestTotalWeightFilter()
		{
			var filter = new JobDeclarationFilterBusinessObject();
			var queryFilter = (ModuleNumberRangeFilter)filter[DeclarationFilterConstants.TotalWeight];
			var declaration1 = Factory.New<JobDeclaration>();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration1.JE_TotalWeight = 0M;
			declaration2.JE_TotalWeight = 87M;
			Factory.Save();
			queryFilter.Decimals = 3;
			queryFilter.BetweenDefaultProperty1 = 0m;
			queryFilter.BetweenDefaultProperty2 = 0m;
			queryFilter.IsActive = true;
			var declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			queryFilter.BetweenDefaultProperty1 = 50.1m;
			queryFilter.BetweenDefaultProperty2 = 100.2m;
			queryFilter.IsActive = true;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
			queryFilter.BetweenDefaultProperty1 = 100.1m;
			queryFilter.BetweenDefaultProperty2 = 200.2m;
			queryFilter.IsActive = true;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
		}

		public void TestDeclarationTypeFilter()
		{
			var filter = new JobDeclarationFilterBusinessObject();
			var queryFilster = (ModuleTextFilter)filter[DeclarationFilterConstants.DeclarationType];
			var declaration1 = Factory.New<JobDeclaration>();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageSubType = "CUS";
			declaration2.JE_MessageSubType = "REC";
			Factory.Save();
			queryFilster.IsActive = true;
			queryFilster.Property = "CUS";
			var declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			queryFilster.Property = "AAA";
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			queryFilster.Property = string.Empty;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
		}

		public void TestOfficeOfEntryExitFilter()
		{
			var filter = new JobDeclarationFilterBusinessObject();
			var queryFilster = (ModuleNkFilter)filter[DeclarationFilterConstants.OfficeOfEntryExit];
			var declaration1 = Factory.New<JobDeclaration>();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration1.JE_OfficeOfEntryExit = "1500";
			declaration2.JE_OfficeOfEntryExit = "2000";
			Factory.Save();
			queryFilster.IsActive = true;
			queryFilster.Property = "1500";
			var declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			queryFilster.Property = "2000";
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
			queryFilster.Property = string.Empty;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
		}

		public void TestCustomsOfficeFilter()
		{
			var filter = new JobDeclarationFilterBusinessObject();
			var queryFilster = (ModuleNkFilter)filter[DeclarationFilterConstants.CustomsOffice];
			var declaration1 = Factory.New<JobDeclaration>();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration1.JE_CustomsOffice = "1500";
			declaration2.JE_CustomsOffice = "2000";
			Factory.Save();
			queryFilster.IsActive = true;
			queryFilster.Property = "1500";
			var declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			queryFilster.Property = "2000";
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
			queryFilster.Property = string.Empty;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
		}

		public void TestEntryInstructionCPCFilter()
		{
			RunEntryInstructionFilterTest(DeclarationFilterConstants.EntryInstructionFilterTypes.CPC, (JobDeclaration je, ZString value) =>
			{
				je.CustomsEntryInstructions.AddNew().CEI_Style = value;
			});
		}

		public void TestEntryInstructionBLNoFilter()
		{
			RunEntryInstructionFilterTest(DeclarationFilterConstants.EntryInstructionFilterTypes.BLNo, (JobDeclaration je, ZString value) =>
			{
				je.CustomsEntryInstructions.AddNew().BillOfLading = value;
			});
		}

		public void TestEntryInstructionRelatedEntryNoFilter()
		{
			RunEntryInstructionFilterTest(DeclarationFilterConstants.EntryInstructionFilterTypes.RelatedEntryNo, (JobDeclaration je, ZString value) =>
			{
				je.CustomsEntryInstructions.AddNew().CEI_RelatedMRN = value;
			});
		}

		public void TestManufacturerBuyerFilter()
		{
			var filter = new JobDeclarationFilterBusinessObject();
			var queryFilter = (ModuleGuidsFilter)filter[DeclarationFilterConstants.ManufacturerBuyer];
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "AAA";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "BBB";
			Factory.Save();
			var declaration1 = Factory.New<JobDeclaration>();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration1.JE_OH_Manufacturer = orgHeader1.PK;
			declaration2.JE_OH_Buyer = orgHeader2.PK;
			Factory.Save();
			queryFilter.IsActive = true;
			queryFilter.Property1 = orgHeader1.PK;
			queryFilter.Property2 = ZGuid.Empty;
			var declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			queryFilter.Property1 = ZGuid.Empty;
			queryFilter.Property2 = orgHeader2.PK;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
			queryFilter.Property1 = ZGuid.Empty;
			queryFilter.Property2 = ZGuid.Empty;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
		}

		public void TestReadyForCompleteDeclarationFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration1.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
			declaration2.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry1.CH_Status = JobMessageStatusList.Codes.ClearedPreliminaryDeclaration;
			entry2.CH_Status = JobMessageStatusList.Codes.AwaitingResponsePreliminaryDeclaration;
			Factory.Save();
			var filterObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EntryHeaderFilterConstants.ReadyForCompleteDeclaration];
			filter.IsActive = true;
			filter.Property = ReadyForCompleteDeclarationFilterHelper.Options.Ready;
			var declarations = new BaseJobDeclarationCollection(Factory, filterObj.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			filter.Property = ReadyForCompleteDeclarationFilterHelper.Options.NotReady;
			declarations = new BaseJobDeclarationCollection(Factory, filterObj.Filter);
			declarations.Load();
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is  in Collection", true, declarations.Contains(declaration2.PK));
			filter.Property = ReadyForCompleteDeclarationFilterHelper.Options.All;
			declarations = new BaseJobDeclarationCollection(Factory, filterObj.Filter);
			declarations.Load();
			AssertEquals("declaration1 is  in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is  in Collection", true, declarations.Contains(declaration2.PK));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new JobDeclarationFilterBusinessObject();

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheck();
			result.Add(TableFilter(CusEntryHeaderSchema.Constants.TableName, NumberFilterTypes.EntryNumber));
			result.Add(TableFilter(CusEntryNumSchema.Constants.TableName, NumberFilterTypes.EntryNumber));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, NumberFilterTypes.EntryNumber));
			result.Add(TableFilter(CusDecHouseBillSchema.Constants.TableName, NumberFilterTypes.Common));
			result.Add(TableFilter(CusEntryHeaderSchema.Constants.TableName, NumberFilterTypes.Common));
			result.Add(TableFilter(CusEntryNumSchema.Constants.TableName, NumberFilterTypes.Common));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, NumberFilterTypes.Common));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, NumberFilterTypes.InvoiceLineProductCode));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, NumberFilterTypes.InvoiceLineProductCode));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, NumberFilterTypes.OrderNumberOwnersRef));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, NumberFilterTypes.OrderNumberOwnersRef));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, NumberFilterTypes.OrderNumberOwnersRef));
			result.Add(TableFilter(JobDocsAndCartageSchema.Constants.TableName, NumberFilterTypes.OrderNumberOwnersRef));
			result.Add(TableFilter(JobOrderHeaderSchema.Constants.TableName, NumberFilterTypes.OrderNumberOwnersRef));
			result.Add(TableFilter(JobOrderItemSchema.Constants.TableName, NumberFilterTypes.OrderNumberOwnersRef));
			result.Add(TableFilter(CusEntryHeaderSchema.Constants.TableName, "Entry Status"));
			result.Add(TableFilter(GlbBranchSchema.Constants.TableName, "Country"));
			result.Add(TableFilter(JobHeaderSchema.Constants.TableName, "Billing Branch"));
			result.Add(TableFilter(JobHeaderSchema.Constants.TableName, "Billing Department"));
			result.Add(TableFilter(JobHeaderSchema.Constants.TableName, "Billing Operator"));
			result.Add(TableFilter(GlbStaffSchema.Constants.TableName, "Cartage Coordinator"));
			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, "Cartage Coordinator"));
			result.Add(TableFilter(OrgStaffAssignmentsSchema.Constants.TableName, "Cartage Coordinator"));
			result.Add(TableFilter(OrgStaffAssignmentsSchema.Constants.TableName, "Client Assigned Staff"));
			result.Add(TableFilter(CusEntryNumSchema.Constants.TableName, "Additional Reference Number"));
			result.Add(TableFilter(JobHeaderSchema.Constants.TableName, "Job Status"));
			result.Add(TableFilter(AccTransactionHeaderSchema.Constants.TableName, "AP Invoice #"));
			result.Add(TableFilter(AccTransactionLinesSchema.Constants.TableName, "AP Invoice #"));
			result.Add(TableFilter(JobHeaderSchema.Constants.TableName, "AP Invoice #"));
			result.Add(TableFilter(AccTransactionHeaderSchema.Constants.TableName, "AR Transaction #"));
			result.Add(TableFilter(AccTransactionLinesSchema.Constants.TableName, "AR Transaction #"));
			result.Add(TableFilter(JobHeaderSchema.Constants.TableName, "AR Transaction #"));
			result.Add(TableFilter(JobChargeSchema.Constants.TableName, "Supplier Cost Reference"));
			result.Add(TableFilter(JobHeaderSchema.Constants.TableName, "Supplier Cost Reference"));
			result.Add(TableFilter(JobShipmentSchema.Constants.TableName, "Milestone Date"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Milestone Date"));
			result.Add(TableFilter(JobShipmentSchema.Constants.TableName, "Milestone Completed"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Milestone Completed"));
			result.Add(TableFilter(JobShipmentSchema.Constants.TableName, "Next Milestone"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Next Milestone"));
			result.Add(TableFilter(JobShipmentSchema.Constants.TableName, "Last Completed Milestone"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Last Completed Milestone"));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, "Any Open Task Assigned To"));
			result.Add(TableFilter(JobShipmentSchema.Constants.TableName, "Any Open Task Assigned To"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Any Open Task Assigned To"));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, "Next Task Assigned To"));
			result.Add(TableFilter(JobShipmentSchema.Constants.TableName, "Next Task Assigned To"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Next Task Assigned To"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Tasks"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Exceptions"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Milestones"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Triggers"));
			return result;
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();
			result.Add(TableFilter(CusDecHouseBillSchema.Constants.TableName, NumberFilterTypes.Common));
			result.Add(TableFilter(CusEntryHeaderSchema.Constants.TableName, NumberFilterTypes.Common));
			result.Add(TableFilter(CusEntryNumSchema.Constants.TableName, NumberFilterTypes.Common));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, NumberFilterTypes.Common));
			result.Add(TableFilter(JobHeaderSchema.Constants.TableName, "Billing Operator"));
			result.Add(TableFilter(CusEntryHeaderSchema.Constants.TableName, "Entry Status"));
			result.Add(TableFilter(JobHeaderSchema.Constants.TableName, "Job Status"));
			result.Add(TableFilter(AccTransactionHeaderSchema.Constants.TableName, "AR Transaction #"));
			result.Add(TableFilter(AccTransactionLinesSchema.Constants.TableName, "AR Transaction #"));
			result.Add(TableFilter(JobHeaderSchema.Constants.TableName, "AR Transaction #"));
			result.Add(TableFilter(CusEntryNumSchema.Constants.TableName, "Additional Reference Number"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, NumberFilterTypes.OrderNumberOwnersRef));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, NumberFilterTypes.OrderNumberOwnersRef));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, NumberFilterTypes.OrderNumberOwnersRef));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, "Any Open Task Assigned To"));
			result.Add(TableFilter(JobShipmentSchema.Constants.TableName, "Any Open Task Assigned To"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Any Open Task Assigned To"));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, "Next Task Assigned To"));
			result.Add(TableFilter(JobShipmentSchema.Constants.TableName, "Next Task Assigned To"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Next Task Assigned To"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Milestone Completed"));
			result.Add(TableFilter(JobShipmentSchema.Constants.TableName, "Milestone Completed"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Next Milestone"));
			result.Add(TableFilter(JobShipmentSchema.Constants.TableName, "Next Milestone"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Last Completed Milestone"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Tasks"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Exceptions"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Milestones"));
			result.Add(TableFilter(ProcessTasksSchema.Constants.TableName, "Triggers"));
			result.Add(TableFilter(JobHeaderSchema.Constants.TableName, "Billing Department"));
			result.Add(TableFilter(JobHeaderSchema.Constants.TableName, "AP Invoice #"));
			result.Add(TableFilter(JobHeaderSchema.Constants.TableName, "Supplier Cost Reference"));
			result.Add(TableFilter(OrgStaffAssignmentsSchema.Constants.TableName, "Client Assigned Staff"));
			result.Add(TableFilter(JobShipmentSchema.Constants.TableName, "Last Completed Milestone"));
			result.Add(TableFilter(CusDecHouseBillSchema.Constants.TableName, "Master Bill"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "Invoice #"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "Part Attribute 1"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "Payment #"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute1"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "Part Attribute 1"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute1"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "Invoice Amount"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "Part Attribute 2"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute2"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "Part Attribute 2"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute2"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "Part Attribute 3"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "Payment Amount"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute3"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "Part Attribute 3"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute3"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "Tariff - Inv Line"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute4"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "Tariff - Inv Line"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute4"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "Description - Inv Line"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute5"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "Description - Inv Line"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute5"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute6"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomAttribute6"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomText1"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomText1"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomDate1"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomDate1"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomDate2"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomDate2"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomDate3"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomDate3"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomNumber1"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomNumber1"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomNumber2"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomNumber2"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomNumber3"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomNumber3"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomFlag1"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomFlag1"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomFlag2"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomFlag2"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomFlag3"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomFlag3"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomDecimal1"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomDecimal1"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomDecimal2"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomDecimal2"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "ComInvoiceLine.CustomDecimal3"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "ComInvoiceLine.CustomDecimal3"));
			result.Add(TableFilter(JobComInvoiceHeaderSchema.Constants.TableName, "Any Commercial Invoice Text Attribute"));
			result.Add(TableFilter(JobComInvoiceLineSchema.Constants.TableName, "Any Commercial Invoice Text Attribute"));
			return result;
		}

		void CheckNumberForEntryTypeFilter(ZString entryType, string filterName)
		{
			var filter = new JobDeclarationFilterBusinessObject();
			AssertNotNull(filter[filterName]);
			var queryFilter = (ModuleTextFilter)filter[filterName];
			var declaration1 = Factory.New<JobDeclaration>();
			var declaration2 = Factory.New<JobDeclaration>();
			var declaration3 = Factory.New<JobDeclaration>();
			var declaration4 = Factory.New<JobDeclaration>();
			var number1 = declaration1.AdditionalReferenceNumbers.AddNew();
			var number2 = declaration2.AdditionalReferenceNumbers.AddNew();
			var number3 = declaration3.AdditionalReferenceNumbers.AddNew();
			var number4 = declaration4.AdditionalReferenceNumbers.AddNew();
			number1.CE_EntryNum = ZString.Empty;
			number2.CE_EntryNum = "123";
			number3.CE_EntryNum = "456";
			number4.CE_EntryNum = "123";
			number1.CE_EntryType = entryType;
			number2.CE_EntryType = entryType;
			number3.CE_EntryType = entryType;
			number4.CE_EntryType = "EV1";
			Factory.Save();
			queryFilter.Property = "123";
			queryFilter.IsActive = true;
			var declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
			AssertEquals("declaration3 is not in Collection", false, declarations.Contains(declaration3.PK));
			AssertEquals("declaration4 is not in Collection", false, declarations.Contains(declaration4.PK));
			queryFilter.Property = ZString.Empty;
			queryFilter.IsActive = true;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
			AssertEquals("declaration3 is in Collection", true, declarations.Contains(declaration3.PK));
			AssertEquals("declaration4 is in Collection", true, declarations.Contains(declaration4.PK));
		}

		void RunEntryInstructionFilterTest(string filterName, Action<JobDeclaration, ZString> setter)
		{
			var filter = new JobDeclarationFilterBusinessObject();
			var queryFilter = (ModuleTextFilter)filter[filterName];
			var declaration1 = Factory.New<JobDeclaration>();
			setter(declaration1, "1234");
			var declaration2 = Factory.New<JobDeclaration>();
			setter(declaration1, "2468");
			Factory.Save();
			queryFilter.IsActive = true;
			queryFilter.Property = "1234";
			var declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			queryFilter.Property = "AAA";
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is not in Collection", false, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is not in Collection", false, declarations.Contains(declaration2.PK));
			queryFilter.Property = string.Empty;
			declarations = new BaseJobDeclarationCollection(Factory, filter.Filter);
			declarations.Load();
			AssertEquals("declaration1 is in Collection", true, declarations.Contains(declaration1.PK));
			AssertEquals("declaration2 is in Collection", true, declarations.Contains(declaration2.PK));
		}
	}
}
