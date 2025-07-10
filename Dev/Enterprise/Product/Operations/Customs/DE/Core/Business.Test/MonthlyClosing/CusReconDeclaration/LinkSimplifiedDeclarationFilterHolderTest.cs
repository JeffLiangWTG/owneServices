using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing;

[TestedType(typeof(LinkSimplifiedDeclarationFilterHolder))]
class LinkSimplifiedDeclarationFilterHolderTest : TestCaseWithFactory
{
	public void TestSetDefaultFilter_LocalClearanceDate()
	{
		var collection = new ActiveBusinessObjectCollection<CusReconEntry>(Factory);
		var declaration = Factory.New<CusReconDeclaration>();
		declaration.CRD_PeriodFrom = new ZDate(2024, 04, 18);
		declaration.CRD_PeriodTo = new ZDate(2024, 04, 19);

		LinkSimplifiedDeclarationFilterHolder.SetDefaultFilter(collection, declaration);

		CombineAssertions(() =>
		{
			AssertFilterDefaultProperty("Search", collection.FilterBusinessObjectDefaults["Local Clearance Date:PropertySearch"], ModuleDateFilter.SpecifiedDateRange);
			AssertFilterDefaultProperty("From", collection.FilterBusinessObjectDefaults["Local Clearance Date:Property1"], new ZDate(2024, 04, 18));
			AssertFilterDefaultProperty("To", collection.FilterBusinessObjectDefaults["Local Clearance Date:Property2"], new ZDate(2024, 04, 19));
		});
	}

	public void TestSetDefaultFilter_EntryType()
	{
		var collection = new ActiveBusinessObjectCollection<CusReconEntry>(Factory);
		var declaration = Factory.New<CusReconDeclaration>();
		declaration.CRD_DeclarationType = MonthlyClosingDeclarationTypeList.Codes.AZL;

		LinkSimplifiedDeclarationFilterHolder.SetDefaultFilter(collection, declaration);

		AssertTextFilterDefaults(collection, "Entry Type", ModuleTextFilter.ComparisonConstants.Exact, MonthlyClosingDeclarationTypeList.Codes.AZL);
	}

	public void TestSetDefaultFilter_RepType()
	{
		var collection = new ActiveBusinessObjectCollection<CusReconEntry>(Factory);
		var declaration = Factory.New<CusReconDeclaration>();
		declaration.CRD_DeclarantType = RepresentationTypeList.Codes._2Direct;

		LinkSimplifiedDeclarationFilterHolder.SetDefaultFilter(collection, declaration);

		AssertTextFilterDefaults(collection, "Rep. Type", ModuleTextFilter.ComparisonConstants.Exact, RepresentationTypeList.Codes._2Direct);
	}

	public void TestSetDefaultFilter_Declarant()
	{
		var collection = new ActiveBusinessObjectCollection<CusReconEntry>(Factory);
		var declaration = Factory.New<CusReconDeclaration>();
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "DECLARANT123";
		declaration.CRD_OA_DeclarantAddress = orgHeader.MainAddress.PK;

		LinkSimplifiedDeclarationFilterHolder.SetDefaultFilter(collection, declaration);

		AssertTextFilterDefaults(collection, "Declarant", ModuleTextFilter.ComparisonConstants.Exact, "DECLARANT123");
	}

	public void TestSetDefaultFilter_ImporterNull()
	{
		var collection = new ActiveBusinessObjectCollection<CusReconEntry>(Factory);
		var declaration = Factory.New<CusReconDeclaration>();
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "DEC123";
		declaration.CRD_OA_DeclarantAddress = orgHeader.MainAddress.PK;
		declaration.IsDeclarantImporter = ZBool.False;

		LinkSimplifiedDeclarationFilterHolder.SetDefaultFilter(collection, declaration);

		AssertTextFilterDefaults(collection, "Importer", ModuleTextFilter.ComparisonConstants.NotEqual, "DEC123");
	}

	public void TestSetDefaultFilter_ImporterNotNull()
	{
		var collection = new ActiveBusinessObjectCollection<CusReconEntry>(Factory);
		var declaration = Factory.New<CusReconDeclaration>();
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "DEC123";
		declaration.CRD_OA_DeclarantAddress = orgHeader.MainAddress.PK;
		declaration.IsDeclarantImporter = ZBool.True;

		LinkSimplifiedDeclarationFilterHolder.SetDefaultFilter(collection, declaration);

		AssertTextFilterDefaults(collection, "Importer", ModuleTextFilter.ComparisonConstants.Exact, "DEC123");
	}

	public void TestSetDefaultFilter_Representative_DeclarantTypeDIR()
	{
		var collection = new ActiveBusinessObjectCollection<CusReconEntry>(Factory);
		var declaration = Factory.New<CusReconDeclaration>();
		declaration.CRD_DeclarantType = RepresentationTypeList.Codes._2Direct;
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "REP123";
		declaration.CRD_OA_RepresentativeAddress = orgHeader.MainAddress.PK;

		LinkSimplifiedDeclarationFilterHolder.SetDefaultFilter(collection, declaration);

		AssertTextFilterDefaults(collection, "Representative", ModuleTextFilter.ComparisonConstants.Exact, "REP123");
	}

	public void TestSetDefaultFilter_Representative_DeclarantTypeNotDIR()
	{
		var collection = new ActiveBusinessObjectCollection<CusReconEntry>(Factory);
		var declaration = Factory.New<CusReconDeclaration>();
		declaration.CRD_DeclarantType = RepresentationTypeList.Codes._3Indirect;
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "REP123";
		declaration.CRD_OA_RepresentativeAddress = orgHeader.MainAddress.PK;

		LinkSimplifiedDeclarationFilterHolder.SetDefaultFilter(collection, declaration);

		AssertEquals(false, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Representative:Property"));
	}

	public void TestSetDefaultFilter_RepresentedParty_DeclarantTypeIND()
	{
		var collection = new ActiveBusinessObjectCollection<CusReconEntry>(Factory);
		var declaration = Factory.New<CusReconDeclaration>();
		declaration.CRD_DeclarantType = RepresentationTypeList.Codes._3Indirect;
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "REPPARTY123";
		declaration.CRD_OA_BuyingAgentAddress = orgHeader.MainAddress.PK;

		LinkSimplifiedDeclarationFilterHolder.SetDefaultFilter(collection, declaration);

		AssertTextFilterDefaults(collection, "Represented Party", ModuleTextFilter.ComparisonConstants.Exact, "REPPARTY123");
	}

	public void TestSetDefaultFilter_RepresentedParty_DeclarantTypeNotIND()
	{
		var collection = new ActiveBusinessObjectCollection<CusReconEntry>(Factory);
		var declaration = Factory.New<CusReconDeclaration>();
		declaration.CRD_DeclarantType = RepresentationTypeList.Codes._2Direct;
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "REPPARTY123";
		declaration.CRD_OA_BuyingAgentAddress = orgHeader.MainAddress.PK;

		LinkSimplifiedDeclarationFilterHolder.SetDefaultFilter(collection, declaration);

		AssertEquals(false, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Represented Party:Property"));
	}

	public void TestSetDefaultFilter_BranchCode()
	{
		var collection = new ActiveBusinessObjectCollection<CusReconEntry>(Factory);
		var declaration = Factory.New<CusReconDeclaration>();
		declaration.CRD_GB_Branch = GlbBranch.CurrentBranch.PK;

		LinkSimplifiedDeclarationFilterHolder.SetDefaultFilter(collection, declaration);

		AssertModuleGuidFilterDefaults(collection, "Branch", ModuleTextFilter.ComparisonConstants.Exact, GlbBranch.CurrentBranch.PK);
	}

	public void TestSetDefaultFilter_NumberOfRows()
	{
		var collection = new ActiveBusinessObjectCollection<CusReconEntry>(Factory);
		var declaration = Factory.New<CusReconDeclaration>();
		declaration.CusReconEntries.AddNew();

		LinkSimplifiedDeclarationFilterHolder.SetDefaultFilter(collection, declaration);

		AssertEquals("Precondition", 998, declaration.CusReconEntries.MaximumAvailableToAdd);
		AssertTextFilterDefaults(collection, "Number Of Rows", ModuleTextFilter.ComparisonConstants.Exact, "998");
	}

	void AssertTextFilterDefaults(IFilterBusinessObjectDefaultsProvider collection, string filterName, ZString expectedComparisonOperator, ZString expectedValue)
	{
		CombineAssertions(() =>
		{
			AssertFilterDefaultProperty("ComparisonOperator", collection.FilterBusinessObjectDefaults[$"{filterName}:ComparisonOperator"], expectedComparisonOperator);
			AssertFilterDefaultProperty("Value", collection.FilterBusinessObjectDefaults[$"{filterName}:Property"], expectedValue);
		});
	}

	void AssertModuleGuidFilterDefaults(IFilterBusinessObjectDefaultsProvider collection, string filterName, ZString expectedComparisonOperator, ZGuid expectedValue)
	{
		CombineAssertions(() =>
		{
			AssertFilterDefaultProperty("ComparisonOperator", collection.FilterBusinessObjectDefaults[$"{filterName}:ComparisonOperator"], expectedComparisonOperator);
			AssertFilterDefaultProperty("Value", collection.FilterBusinessObjectDefaults[$"{filterName}:Property"], expectedValue);
		});
	}

	void AssertFilterDefaultProperty(string message, FilterBusinessObjectDefault property, IZType expectedValue)
	{
		AssertEquals($"{message} -> IsRemovable", false, property.IsRemovable);
		AssertEquals($"{message} -> value", expectedValue, property.Value);
	}
}
