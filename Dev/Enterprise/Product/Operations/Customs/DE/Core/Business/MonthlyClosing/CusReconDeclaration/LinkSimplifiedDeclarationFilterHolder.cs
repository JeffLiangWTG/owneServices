using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.DE.Business;

public static class LinkSimplifiedDeclarationFilterHolder
{
	public static void SetDefaultFilter(IFilterBusinessObjectDefaultsProvider collection, CusReconDeclaration declaration)
	{
		AddLocalClearanceDateFilter(collection);
		AddEntryTypeFilter(collection);
		AddRepTypeFilter(collection);
		AddDeclarantFilter(collection);
		AddImporterFilter(collection);
		AddRepresentativeFilterIfNecessary(collection);
		AddRepresentedPartyFilterIfNecessary(collection);
		AddBranchFilter(collection);
		AddNumberOfRowsFilter(collection);

		void AddLocalClearanceDateFilter(IFilterBusinessObjectDefaultsProvider collection)
		{
			AddModuleDateRangeFilter(collection, SimplifiedDeclarationFilterStripBusinessObject.Schema.LocalClearanceDate, declaration.CRD_PeriodFrom, declaration.CRD_PeriodTo);
		}

		void AddEntryTypeFilter(IFilterBusinessObjectDefaultsProvider collection)
		{
			AddModuleTextFilter(collection, SimplifiedDeclarationFilterStripBusinessObject.Schema.EntryType, declaration.CRD_DeclarationType);
		}

		void AddRepTypeFilter(IFilterBusinessObjectDefaultsProvider collection)
		{
			AddModuleTextFilter(collection, SimplifiedDeclarationFilterStripBusinessObject.Schema.RepresentationType, declaration.CRD_DeclarantType);
		}

		void AddDeclarantFilter(IFilterBusinessObjectDefaultsProvider collection)
		{
			AddModuleTextFilter(collection, SimplifiedDeclarationFilterStripBusinessObject.Schema.Declarant, declaration.DeclarantAddress?.Header.OH_Code ?? ZString.Empty);
		}

		void AddImporterFilter(IFilterBusinessObjectDefaultsProvider collection)
		{
			var comparisonOperator = declaration.CRD_OA_ImporterAddress.IsEmpty ? ModuleTextFilter.ComparisonConstants.NotEqual : ModuleTextFilter.ComparisonConstants.Exact;
			AddModuleTextFilter(collection, SimplifiedDeclarationFilterStripBusinessObject.Schema.Importer, declaration.DeclarantAddress?.Header.OH_Code ?? ZString.Empty, comparisonOperator);
		}

		void AddRepresentativeFilterIfNecessary(IFilterBusinessObjectDefaultsProvider collection)
		{
			if (declaration.CRD_DeclarantType == EU.Business.RepresentationTypeList.Codes._2Direct)
			{
				AddModuleTextFilter(collection, SimplifiedDeclarationFilterStripBusinessObject.Schema.Representative, declaration.RepresentativeAddress?.Header.OH_Code ?? ZString.Empty);
			}
		}

		void AddRepresentedPartyFilterIfNecessary(IFilterBusinessObjectDefaultsProvider collection)
		{
			if (declaration.CRD_DeclarantType == EU.Business.RepresentationTypeList.Codes._3Indirect)
			{
				AddModuleTextFilter(collection, SimplifiedDeclarationFilterStripBusinessObject.Schema.RepresentedParty, declaration.BuyingAgentAddress?.Header.OH_Code ?? ZString.Empty);
			}
		}

		void AddBranchFilter(IFilterBusinessObjectDefaultsProvider collection)
		{
			AddModuleGuidFilter(collection, SimplifiedDeclarationFilterStripBusinessObject.Schema.Branch, declaration.CRD_GB_Branch);
		}

		void AddNumberOfRowsFilter(IFilterBusinessObjectDefaultsProvider collection)
		{
			AddModuleTextFilter(collection, SimplifiedDeclarationFilterStripBusinessObject.Schema.NumberOfRows, declaration.CusReconEntries.MaximumAvailableToAdd.ToString());
		}
	}

	static void AddModuleDateRangeFilter(IFilterBusinessObjectDefaultsProvider collection, string filterName, ZDate value1, ZDate value2)
	{
		collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(filterName, "PropertySearch", ModuleDateFilter.SpecifiedDateRange, false));
		collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(filterName, "Property1", value1, false));
		collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(filterName, "Property2", value2, false));
	}

	static void AddModuleTextFilter(IFilterBusinessObjectDefaultsProvider collection, string filterName, ZString value, string comparisonOperator = ModuleTextFilter.ComparisonConstants.Exact)
	{
		collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(filterName, "ComparisonOperator", (ZString)comparisonOperator, false));
		collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(filterName, "Property", value, false));
	}

	static void AddModuleGuidFilter(IFilterBusinessObjectDefaultsProvider collection, string filterName, ZGuid value, string comparisonOperator = ModuleTextFilter.ComparisonConstants.Exact)
	{
		collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(filterName, "ComparisonOperator", (ZString)comparisonOperator, false));
		collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(filterName, "Property", value, false));
	}
}
