using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Intrastat.Module
{
	public class IntrastatTransactionsFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			// Numbers
			public const string TradersReference = "Traders Reference";

			// Locations
			public const string CountryOfSupply = "Country Of Supply";
			public const string CountryOfReceipt = "Country Of Receipt";

			// Dates
			public const string TransactionDate = "Transaction Date";

			// Organisations / Staff
			public const string Consignee = "Consignee";
			public const string Supplier = "Supplier";

			// Modes and Types
			public const string TransportMode = "Transport Mode";

			// Text
			public const string ConsigneeName = "Consignee Name";
			public const string SupplierName = "Supplier Name";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			// -- Numbers --
			var tradersReferenceFilter = filters.AddTextFilter(Schema.TradersReference, CusIntrastatHeaderSchema.CIH_TradersReference);
			tradersReferenceFilter.Category = FilterCategories.NumbersAndReferences;
			tradersReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("04618C7C-7209-4EC5-A22C-CB14FF2CDC16", Schema.TradersReference);

			// -- Dates --
			var transactionDateFilter = filters.AddDateFilter(Schema.TransactionDate, CusIntrastatHeaderSchema.CIH_TransactionDate);
			transactionDateFilter.MultilingualDescription = ResString.GetMultilingualString("2502C7C8-0496-4D2C-8063-A031E94C81DB", Schema.TransactionDate);

			//--Modes--
			var transportModeFilter = filters.AddTextFilter(Schema.TransportMode, TransportModeQuery, Lookups.TransportModes);
			transportModeFilter.Category = FilterCategories.ModesAndTypes;
			transportModeFilter.MultilingualDescription = ResString.GetMultilingualString("37636014-0A50-4F51-881A-893E04D0CA3A", Schema.TransportMode);

			// -- Orgs --
			var consigneeFilter = filters.AddGuidFilter(Schema.Consignee, ModuleIDs.Organisation, CusIntrastatHeaderSchema.CIH_OH_Consignee, Lookups.Consignees);
			consigneeFilter.Category = FilterCategories.Organisations;
			consigneeFilter.MultilingualDescription = ResString.GetMultilingualString("3C1CB1D8-BC96-414A-AC2D-B3E2051B0D6C", Schema.Consignee);

			var supplierFilter = filters.AddGuidFilter(Schema.Supplier, ModuleIDs.Organisation, CusIntrastatHeaderSchema.CIH_OH_Supplier, Lookups.Consignors);
			supplierFilter.Category = FilterCategories.Organisations;
			supplierFilter.MultilingualDescription = ResString.GetMultilingualString("5BB49FC9-DA08-45B3-92E4-48B0F443B3C0", Schema.Supplier);

			var consigneeNameFilter = filters.AddTextFilter(Schema.ConsigneeName, CusIntrastatHeaderSchema.CIH_ConsigneeName, Lookups.Consignees);
			consigneeNameFilter.Category = FilterCategories.TextSearch;
			consigneeNameFilter.MultilingualDescription = ResString.GetMultilingualString("97985F59-9027-4A38-8D0B-A424946BC668", Schema.ConsigneeName);

			var supplierNameFilter = filters.AddTextFilter(Schema.SupplierName, CusIntrastatHeaderSchema.CIH_SupplierName, Lookups.Consignors);
			supplierNameFilter.Category = FilterCategories.TextSearch;
			supplierNameFilter.MultilingualDescription = ResString.GetMultilingualString("ADA3CA07-5C7D-407B-8D0F-67BB439FCEB0", Schema.SupplierName);

			// -- Locations --
			var countryOfSupplyFilter = filters.AddTextFilter(Schema.CountryOfSupply, CusIntrastatHeaderSchema.CIH_CountryOfSupply);
			countryOfSupplyFilter.Category = FilterCategories.Locations;
			countryOfSupplyFilter.MultilingualDescription = ResString.GetMultilingualString("020B0D70-8391-409F-A94C-544DF994F6DD", Schema.CountryOfSupply);

			var countryOfReceiptFilter = filters.AddTextFilter(Schema.CountryOfReceipt, CusIntrastatHeaderSchema.CIH_CountryOfReceipt);
			countryOfReceiptFilter.Category = FilterCategories.Locations;
			countryOfReceiptFilter.MultilingualDescription = ResString.GetMultilingualString("34FACE03-1589-40BF-9A38-B86BD8C0297B", Schema.CountryOfReceipt);

			return filters;
		}

		public IntrastatTransactionsFilterLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = GetNewLookups();
				}

				return lookups;
			}
		}
		IntrastatTransactionsFilterLookups lookups;

		protected virtual IntrastatTransactionsFilterLookups GetNewLookups() => new IntrastatTransactionsFilterLookups(this);

		#region Filter Queries

		ZQuery TransportModeQuery(ZString value)
		{
			return new ZQuery(CusIntrastatHeaderSchema.CIH_ModeOfTransport, value);
		}

		#endregion Filter Queries

		protected override void AddInitialAuditFilters(ModuleFilterCollection filters)
		{
			base.AddInitialAuditFilters(filters);

			var createdTimeFilter = filters[FilterDescriptions.CreatedTime] as ModuleDateFilter;
			if (createdTimeFilter != null)
			{
				createdTimeFilter.Visibility = FilterVisibility.AlwaysVisible;
				createdTimeFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last3Mths;
			}
		}
	}
}
