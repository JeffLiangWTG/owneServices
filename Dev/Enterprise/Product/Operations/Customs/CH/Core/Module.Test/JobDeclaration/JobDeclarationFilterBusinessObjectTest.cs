using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.SwissCustomsConstants;

namespace Enterprise.Customs.CH.Module.Testing;

[TestedType(typeof(JobDeclarationFilterBusinessObject))]
sealed class JobDeclarationFilterBusinessObjectTest : Customs.Module.Testing.JobDeclarationFilterBusinessObjectTest
{
	public void TestLookups()
	{
		var filterBizObj = new JobDeclarationFilterBusinessObject();
		AssertType<JobDeclarationFilterLookups>("Lookups of correct type", filterBizObj.Lookups);
	}

	public void TestFilters() => CombineAssertions(() =>
	{
		var filter = GetNewFilterStripBusinessObject();

		AssertNotNull(filter[DeclarationFilterConstants.MessageStatusText]);
		AssertNotNull(filter[JobDeclarationFilterBusinessObject.CHFilterConstants.PhaseStatus]);
		AssertNotNull(filter[JobDeclarationFilterBusinessObject.CHFilterConstants.SelectionResult]);
	});

	public void TestPhaseStatusFilter() => CombineAssertions(() =>
	{
		var declaration1 = CreateDeclarationWithHeader(PassarDeclarationPhaseList.Codes.Amendment);
		var declaration2 = CreateDeclarationWithHeader(PassarDeclarationPhaseList.Codes.Activation);
		declaration2.ActiveEntryHeaders.AddNew().CH_PhaseStatus = PassarDeclarationPhaseList.Codes.Amendment;
		CreateDeclarationWithHeader(PassarDeclarationPhaseList.Codes.Activation);

		Factory.Save();

		var filterBizObj = CreateAndConfigurefilterBizObj(JobDeclarationFilterBusinessObject.CHFilterConstants.PhaseStatus, PassarDeclarationPhaseList.Codes.Amendment);

		var filteredDecs = Factory.Load<BaseJobDeclaration>(filterBizObj.Filter);

		AssertEquals("Total Count", 2, filteredDecs.Length);
		AssertEquals("Declaration with single entry header found.", true, filteredDecs.Any(d => d.PK == declaration1.PK));
		AssertEquals("Declaration with multiple entry headers found.", true, filteredDecs.Any(d => d.PK == declaration2.PK));

		JobDeclaration CreateDeclarationWithHeader(string phaseStatus)
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_PhaseStatus = phaseStatus;
			return declaration;
		}

		JobDeclarationFilterBusinessObject CreateAndConfigurefilterBizObj(string moduleTextFilterName, string propertyValue)
		{
			var filterBizObj = new JobDeclarationFilterBusinessObject();
			var phaseStatusFilter = (ModuleTextFilter)filterBizObj[moduleTextFilterName];
			phaseStatusFilter.IsActive = true;
			phaseStatusFilter.Property = propertyValue;
			return filterBizObj;
		}
	});

	public void TestEntryStatusFilter_BLT() => AssertEntryStatusFilter("BLT");

	public void TestEntryStatusFilter_BTH() => AssertEntryStatusFilter("BTH");

	void AssertEntryStatusFilter(string submissionType) => CombineAssertions(() =>
	{
		CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new LocalCountryCustomsInterface() { RecipientID = "RecipientID", SubmissionType = submissionType, });

		var declaration1 = CreateDeclarationWithHeader(SwissCustomsConstants.CustomsStatusCodes.ShipmentRelease);
		var declaration2 = CreateDeclarationWithHeader(SwissCustomsConstants.CustomsStatusCodes.CustomsDeclarationReceived);
		var declaration3 = CreateDeclarationWithHeader(SwissCustomsConstants.CustomsStatusCodes.CustomsDeclarationReceived);
		declaration3.ActiveEntryHeaders.AddNew().CH_EntryStatus = SwissCustomsConstants.CustomsStatusCodes.SubmittedToTaxud;

		Factory.Save();

		var filterBizObj1 = CreateAndConfigurefilterBizObj(DeclarationFilterConstants.EntryStatusText, SwissCustomsConstants.CustomsStatusCodes.CustomsDeclarationReceived, EntryStatusFilterTypeList.Codes.Any);
		var filteredDecs1 = Factory.Load<BaseJobDeclaration>(filterBizObj1.Filter);
		AssertEquals("Total Count", 2, filteredDecs1.Length);
		AssertEquals("Declaration with single entry header found.", true, filteredDecs1.Any(d => d.PK == declaration2.PK));
		AssertEquals("Declaration with multiple entry headers found.", true, filteredDecs1.Any(d => d.PK == declaration3.PK));

		var filterBizObj2 = CreateAndConfigurefilterBizObj(DeclarationFilterConstants.EntryStatusText, SwissCustomsConstants.CustomsStatusCodes.CustomsDeclarationReceived, EntryStatusFilterTypeList.Codes.All);
		var filteredDecs2 = Factory.Load<BaseJobDeclaration>(filterBizObj2.Filter);
		AssertEquals("Total Count", 1, filteredDecs2.Length);
		AssertEquals("Declaration with single entry header found.", true, filteredDecs2.Any(d => d.PK == declaration2.PK));
		AssertEquals("Declaration with multiple entry headers not found.", false, filteredDecs2.Any(d => d.PK == declaration3.PK));

		JobDeclaration CreateDeclarationWithHeader(string entryStatus)
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = entryStatus;
			return declaration;
		}

		JobDeclarationFilterBusinessObject CreateAndConfigurefilterBizObj(string moduleTextFilterName, string propertyValue, string filterType)
		{
			var filterBizObj = new JobDeclarationFilterBusinessObject();
			var entryStatusFilter = (EntryStatusFilter)filterBizObj[moduleTextFilterName];
			entryStatusFilter.IsActive = true;
			entryStatusFilter.Property = propertyValue;
			entryStatusFilter.FilterType = filterType;
			return filterBizObj;
		}
	});

	public void TestSelectionResultFilterAny() => AssertSelectionResultFilter(SelectionResultCodes.FreeWith, EntryStatusFilterTypeList.Codes.Any, 2);

	public void TestSelectionResultFilterAll() => AssertSelectionResultFilter(SelectionResultCodes.FreeWith, EntryStatusFilterTypeList.Codes.All, 1);

	void AssertSelectionResultFilter(string propertyValue, string filterType, int expectedDecs)
	{
		var declaration1 = CreateDeclarationWithHeader(SelectionResultCodes.FreeWith);
		_ = CreateDeclarationWithHeader(SelectionResultCodes.FreeWith, declaration1);
		_ = CreateDeclarationWithHeader(SelectionResultCodes.FreeWith, declaration1);

		var declaration2 = CreateDeclarationWithHeader(SelectionResultCodes.FreeWith);
		_ = CreateDeclarationWithHeader(SelectionResultCodes.FreeWithout, declaration2);
		_ = CreateDeclarationWithHeader(SelectionResultCodes.Blocked, declaration2);

		var declaration3 = CreateDeclarationWithHeader(SelectionResultCodes.Blocked);
		_ = CreateDeclarationWithHeader(SelectionResultCodes.Blocked, declaration3);
		_ = CreateDeclarationWithHeader(SelectionResultCodes.Blocked, declaration3);

		Factory.Save();

		var filterBizObj = CreateAndConfigurefilterBizObj(JobDeclarationFilterBusinessObject.CHFilterConstants.SelectionResult, propertyValue, filterType);

		var filteredDecs = Factory.Load<JobDeclaration>(filterBizObj.Filter);

		AssertEquals($"Filtered Declarations / Filter Type: {filterType}", expectedDecs, filteredDecs.Length);

		JobDeclaration CreateDeclarationWithHeader(string selectionResult, JobDeclaration jobDeclaration = null)
		{
			if (jobDeclaration == null)
			{
				jobDeclaration = Factory.New<JobDeclaration>();
			}
			var entryHeader = jobDeclaration.ActiveEntryHeaders.AddNew();

			var cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNumber.CE_ParentID = entryHeader.PK;
			cusEntryNumber.CE_ParentTable = entryHeader.TableName;
			cusEntryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			cusEntryNumber.CE_EntryNum = "123456";
			cusEntryNumber.CE_IssueDate = ZDateTime.Today;
			cusEntryNumber.CE_EntryStatus = selectionResult;

			return jobDeclaration;
		}

		JobDeclarationFilterBusinessObject CreateAndConfigurefilterBizObj(string moduleTextFilterName, string propertyValue, string filterType)
		{
			var filterBizObj = new JobDeclarationFilterBusinessObject();
			var selectionResultfilter = (EntryStatusFilter)filterBizObj[moduleTextFilterName];
			selectionResultfilter.IsActive = true;
			selectionResultfilter.Property = propertyValue;
			selectionResultfilter.FilterType = filterType;
			return filterBizObj;
		}
	}
}
