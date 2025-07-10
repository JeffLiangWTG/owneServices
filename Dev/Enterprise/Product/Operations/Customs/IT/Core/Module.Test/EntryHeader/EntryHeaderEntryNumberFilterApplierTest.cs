using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.Customs.IT.Module.EntryHeaderFilterBusinessObject;

namespace Enterprise.Customs.IT.Module.Testing;

[TestedType(typeof(EntryHeaderFilterBusinessObject))]
sealed class EntryHeaderEntryNumberFilterApplierTest : FilterStripBusinessObjectTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new EntryHeaderEntryNumberFilterApplier(null));
	}

	public void TestAddFilters()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new EntryHeaderEntryNumberFilterApplier(new EntryHeaderFilterBusinessObject()).AddFilters(null));
	}

	public void TestReleaseCodeFilter()
	{
		entryHeader1.EntryNumbersProvider.InsertOrUpdateReleaseCode("XXX", ZDateTime.Today);
		Factory.Save();

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection Count", 2, entryHeaderCollection.Count);

		var releaseCodeFilter = (ModuleTextFilter)filterStripBusinessObject[ITFilterConstants.ReleaseCode];
		AssertNotNull("Release Code Filter", releaseCodeFilter);
		releaseCodeFilter.Property = "XXX";
		releaseCodeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
		releaseCodeFilter.IsActive = true;

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When Release Filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader1.PK, entryHeaderCollection[0].PK);
		});

		releaseCodeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When 'IsBlank' filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader2.PK, entryHeaderCollection[0].PK);
		});
	}

	public void TestRegistrationNumberFilter()
	{
		entryHeader1.EntryNumbersProvider.InsertOrUpdateEntryNum(CusEntryNumberConstants.EntryTypes.RegistrationNumber, "REG-NUMB", ZDateTime.Today);
		Factory.Save();

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection Count", 2, entryHeaderCollection.Count);

		var registrationNumberFilter = (ModuleTextFilter)filterStripBusinessObject[ITFilterConstants.RegistrationNumber];
		AssertNotNull("Registration Number Filter", registrationNumberFilter);
		registrationNumberFilter.Property = "REG";
		registrationNumberFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
		registrationNumberFilter.IsActive = true;

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When Registration Number Filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader1.PK, entryHeaderCollection[0].PK);
		});

		registrationNumberFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When 'IsBlank' filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader2.PK, entryHeaderCollection[0].PK);
		});
	}

	public void TestExitDateFilter()
	{
		var ivistoEntryNumber = CreateCusEntryNumber(entryHeader1, CusEntryNumberConstants.EntryTypes.Ivisto);
		ivistoEntryNumber.CE_IssueDate = ZDateTime.Today;
		Factory.Save();

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection Count", 2, entryHeaderCollection.Count);

		var exitDateNumberFilter = (ModuleDateFilter)filterStripBusinessObject[ITFilterConstants.ExitDateFilter];
		AssertNotNull("Exit Date Filter", exitDateNumberFilter);
		exitDateNumberFilter.Property1 = ZDateTime.Today.AddDays(-2);
		exitDateNumberFilter.Property2 = ZDateTime.Today.AddDays(2);
		exitDateNumberFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
		exitDateNumberFilter.IsActive = true;

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When Exit Date filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader1.PK, entryHeaderCollection[0].PK);
		});

		exitDateNumberFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When 'IsBlank' filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader2.PK, entryHeaderCollection[0].PK);
		});
	}

	public void TestExitProcessingDateFilter()
	{
		var ivistoEntryNumber = CreateCusEntryNumber(entryHeader1, CusEntryNumberConstants.EntryTypes.Ivisto);
		ivistoEntryNumber.CE_SystemCreateTimeUtc = ZDateTime.UtcNow;
		Factory.Save();

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection Count", 2, entryHeaderCollection.Count);

		var exitProcessingDateNumberFilter = (ModuleDateFilter)filterStripBusinessObject[ITFilterConstants.ExitProcessingDateFilter];
		AssertNotNull("Exit Processing Date Filter", exitProcessingDateNumberFilter);
		exitProcessingDateNumberFilter.Property1 = ivistoEntryNumber.CE_SystemCreateTimeUtc.AddDays(-2);
		exitProcessingDateNumberFilter.Property2 = ivistoEntryNumber.CE_SystemCreateTimeUtc.AddDays(2);
		exitProcessingDateNumberFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
		exitProcessingDateNumberFilter.IsActive = true;

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When Exit Processing Date filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader1.PK, entryHeaderCollection[0].PK);
		});

		exitProcessingDateNumberFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When 'IsBlank' filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader2.PK, entryHeaderCollection[0].PK);
		});
	}

	public void TestExitProcessingDateFilterConvertFromLocalToUTC()
	{
		var exitProcessingDateNumberFilter = (ModuleDateFilter)filterStripBusinessObject[ITFilterConstants.ExitProcessingDateFilter];
		AssertEquals(nameof(exitProcessingDateNumberFilter.ConvertFromLocalToUTC), true, exitProcessingDateNumberFilter.ConvertFromLocalToUTC);
	}

	public void TestExitOfficeFilter()
	{
		var ivistoEntryNumber = CreateCusEntryNumber(entryHeader1, CusEntryNumberConstants.EntryTypes.Ivisto);
		ivistoEntryNumber.CE_EntryLineReference = "IT275100";
		Factory.Save();

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection Count", 2, entryHeaderCollection.Count);

		var exitOfficeNumberFilter = (ModuleNkFilter)filterStripBusinessObject[ITFilterConstants.ExitOfficeFilter];
		AssertNotNull("Exit Office filter", exitOfficeNumberFilter);
		exitOfficeNumberFilter.Property = "IT275100";
		exitOfficeNumberFilter.IsActive = true;

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When Exit Office filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader1.PK, entryHeaderCollection[0].PK);
		});

		exitOfficeNumberFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When 'IsBlank' filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader2.PK, entryHeaderCollection[0].PK);
		});
	}

	public void TestExitStatusFilter()
	{
		var ivistoEntryNumber = CreateCusEntryNumber(entryHeader1, CusEntryNumberConstants.EntryTypes.Ivisto);
		ivistoEntryNumber.CE_EntryStatus = "EXI";
		Factory.Save();

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection Count", 2, entryHeaderCollection.Count);

		var exitStatusFilter = (ModuleTextFilter)filterStripBusinessObject[ITFilterConstants.ExitStatusFilter];
		AssertNotNull("Exit Status Filter", exitStatusFilter);
		exitStatusFilter.Property = "EXI";
		exitStatusFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
		exitStatusFilter.IsActive = true;

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When Exit Status Filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader1.PK, entryHeaderCollection[0].PK);
		});

		exitStatusFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When 'IsBlank' filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader2.PK, entryHeaderCollection[0].PK);
		});
	}

	public void TestArrivalDateFilter()
	{
		var irildesEntryNumber = CreateCusEntryNumber(entryHeader1, CusEntryNumberConstants.EntryTypes.Irildes);
		irildesEntryNumber.CE_IssueDate = ZDateTime.Now;
		Factory.Save();

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection Count", 2, entryHeaderCollection.Count);

		var arrivalDateNumberFilter = (ModuleDateFilter)filterStripBusinessObject[ITFilterConstants.ArrivalDateFilter];
		AssertNotNull("Arrival Date Filter", arrivalDateNumberFilter);
		arrivalDateNumberFilter.Property1 = ZDateTime.Today.AddDays(-2);
		arrivalDateNumberFilter.Property2 = ZDateTime.Today.AddDays(2);
		arrivalDateNumberFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
		arrivalDateNumberFilter.IsActive = true;

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When Arrival Date filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader1.PK, entryHeaderCollection[0].PK);
		});

		arrivalDateNumberFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When 'HasNoDateEntered' filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader2.PK, entryHeaderCollection[0].PK);
		});
	}

	public void TestArrivalOfficeFilter()
	{
		var irildesEntryNumber = CreateCusEntryNumber(entryHeader1, CusEntryNumberConstants.EntryTypes.Irildes);
		irildesEntryNumber.CE_EntryLineReference = "IT445100";
		Factory.Save();

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection Count", 2, entryHeaderCollection.Count);

		var arrivalOfficeNumberFilter = (ModuleNkFilter)filterStripBusinessObject[ITFilterConstants.ArrivalOfficeFilter];
		AssertNotNull("Arrival Office Filter", arrivalOfficeNumberFilter);
		arrivalOfficeNumberFilter.Property = "IT445100";
		arrivalOfficeNumberFilter.IsActive = true;

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When Arrival filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader1.PK, entryHeaderCollection[0].PK);
		});

		arrivalOfficeNumberFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When 'IsBlank' filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader2.PK, entryHeaderCollection[0].PK);
		});
	}

	public void TestArrivalStatusFilter()
	{
		var irildesEntryNumber = CreateCusEntryNumber(entryHeader1, CusEntryNumberConstants.EntryTypes.Irildes);
		irildesEntryNumber.CE_EntryStatus = "IRI";
		Factory.Save();

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection Count", 2, entryHeaderCollection.Count);

		var arrivalStatusFilter = (ModuleTextFilter)filterStripBusinessObject[ITFilterConstants.ArrivalStatusFilter];
		AssertNotNull("Exit Status Filter", arrivalStatusFilter);
		arrivalStatusFilter.Property = "IRI";
		arrivalStatusFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
		arrivalStatusFilter.IsActive = true;

		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When Arrival Status filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader1.PK, entryHeaderCollection[0].PK);
		});

		arrivalStatusFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
		entryHeaderCollection.Load(filterStripBusinessObject.Filter);
		CombineAssertions("[POST-CONDITION] When 'IsBlank' filter is applied", () =>
		{
			AssertEquals("EntryHeaderCollection Count", 1, entryHeaderCollection.Count);
			AssertEquals("Single EntryHeader found PK", entryHeader2.PK, entryHeaderCollection[0].PK);
		});
	}

	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EntryHeaderFilterBusinessObject();

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader2 = declaration.CustomsEntryHeaders.AddNew();

		entryHeaderCollection = new CusEntryHeaderCollection(declaration, Factory);
		filterStripBusinessObject = GetNewFilterStripBusinessObject();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader1;
	CusEntryHeader entryHeader2;
	CusEntryHeaderCollection entryHeaderCollection;
	FilterStripBusinessObject filterStripBusinessObject;

	CusEntryNumber CreateCusEntryNumber(CusEntryHeader entryHeader, ZString entryType)
	{
		var entryNum = Factory.New<CusEntryNumber>();
		entryNum.CE_ParentID = entryHeader.PK;
		entryNum.CE_ParentTable = entryHeader.TableName;
		entryNum.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		entryNum.CE_EntryType = entryType;
		entryNum.CE_RN_NKCountryCode = "IT";
		return entryNum;
	}
}
