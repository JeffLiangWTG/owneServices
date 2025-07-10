using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.PayableOrder;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class AccPayableOrderFilterBusinessObject : FilterStripBusinessObject
	{
		public AccPayableOrderFilterBusinessObject()
		{ }

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddStatusFilters(filters);
			AddDateFilters(filters);
			AddOrganisationFilters(filters);
			return filters;
		}

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = base.Filter;
				query.AddToFilter(AccPayableOrderHeaderSchema.APH_GC, GlbCompany.CurrentCompany.PK);
				return query;
			}
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter filter = filters.AddTextFilter(Business.AccountingUtils.NumberFilterTypes.OrderNumber, AccPayableOrderHeaderSchema.APH_OrderNumber);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccPayableOrderFilter|OrderNumber", "Order #");
			filter.UseMultiSearch = true;

			filter = filters.AddTextFilter(Business.AccountingUtils.NumberFilterTypes.SupplierCostReference, AccPayableOrderHeaderSchema.APH_BookingConfRef);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccPayableOrderFilter|BookingConfNumber", "Booking Conf. Ref. #");

			filter = filters.AddTextFilter(Business.AccountingUtils.NumberFilterTypes.TransactionNumber, AccPayableOrderHeaderSchema.APH_InvoiceNumber);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccPayableOrderFilter|InvoiceNumber", "Invoice #");
			filter.UseMultiSearch = true;
		}

		void AddStatusFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddTextFilter("Stage", AccPayableOrderHeaderSchema.APH_Stage, new CodeDescriptionPairList(OLookUpEditType.PayableOrderStage));
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccPayableOrderFilter|Stage", "Stage");

			filter = filters.AddTextFilter("Disposition", AccPayableOrderHeaderSchema.APH_Disposition, new CodeDescriptionPairList(OLookUpEditType.PayableOrderDisposition));
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccPayableOrderFilter|Disposition", "Disposition");

			filter = filters.AddTextFilter("Type", AccPayableOrderHeaderSchema.APH_Type, new CodeDescriptionPairList(OLookUpEditType.PayableOrderType));
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccPayableOrderFilter|Type", "Type");

			filter = filters.AddTextFilter("Goods Status", AccPayableOrderHeaderSchema.APH_GoodsReceivedStatus, new CodeDescriptionPairList(OLookUpEditType.PayableOrderGoodsStatus));
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccPayableOrderFilter|GoodsStatus", "Goods Status");
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddDateFilter("Due Date", AccPayableOrderHeaderSchema.APH_DueDate);
			filter.Category = FilterCategories.Dates;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccPayableOrderFilter|DueDate", "Due Date");

			filter = filters.AddDateFilter("Booking Confirmation Date", AccPayableOrderHeaderSchema.APH_BookingConfDate);
			filter.Category = FilterCategories.Dates;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccPayableOrderFilter|BookingConfDate", "Booking Confirmation Date");

			filter = filters.AddDateFilter("Invoice Date", AccPayableOrderHeaderSchema.APH_InvoiceDate);
			filter.Category = FilterCategories.Dates;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccPayableOrderFilter|InvoiceDate", "Invoice Date");

			filter = filters.AddDateFilter("Ready for Delivery Date", AccPayableOrderHeaderSchema.APH_ReadyForDelivery);
			filter.Category = FilterCategories.Dates;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccPayableOrderFilter|ReadyForDeliveryDate", "Ready For Delivery Date");

			filter = filters.AddDateFilter("Expected Delivery Date", AccPayableOrderHeaderSchema.APH_ExpectedDelivery);
			filter.Category = FilterCategories.Dates;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccPayableOrderFilter|ExpectedDeliveryDate", "Expected Delivery Date");

			filter = filters.AddDateFilter("Follow up Date", AccPayableOrderHeaderSchema.APH_FollowupDate);
			filter.Category = FilterCategories.Dates;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccPayableOrderFilter|FollowupDate", "Follow up Date");
		}

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			ModuleGuidFilter orderedByFilter = filters.AddGuidFilter("Ordered By", ModuleIDs.Organisation, GetOrderedByQuery, OrderedByList);
			orderedByFilter.Category = FilterCategories.Organisations;
			orderedByFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccPayableOrderFilter|OrderedBy", "Ordered By");

			ModuleGuidFilter supplierFilter = filters.AddGuidFilter("Supplier", ModuleIDs.Organisation, GetSupplierQuery, SupplierList);
			supplierFilter.Category = FilterCategories.Organisations;
			supplierFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccPayableOrderFilter|Supplier", "Supplier");
		}

		ZQuery GetOrderedByQuery(ZGuid orderedByPK)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(AccPayableOrderHeader));
			ZDBOnlySubQuery addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			addressSubQuery.AddToFilter(JoinCondition.And, OrgAddressSchema.OA_OH, orderedByPK);
			result.AddSubQuery(AccPayableOrderHeaderSchema.APH_OA_Buyer, addressSubQuery, JoinCondition.And);
			return result;
		}

		OrgHeaderCollection orderedByList;
		OrgHeaderCollection OrderedByList
		{
			get { return orderedByList ?? (orderedByList = new OrgHeaderCollection(new BusinessObjectFactory(), GetOrderedByListFilter())); }
		}

		ZQuery GetOrderedByListFilter()
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbCompanySchema.GC_OH_OrgProxy);
			companyQuery.AddToFilter(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK);
			result.AddSubQuery(OrgHeaderSchema.PK, companyQuery, JoinCondition.And);

			ZDBOnlySubQuery branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.GB_OH_OrgProxy);
			branchQuery.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
			result.AddSubQuery(OrgHeaderSchema.PK, branchQuery, JoinCondition.Or);
			return result;
		}

		ZQuery GetSupplierQuery(ZGuid supplierPK)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(AccPayableOrderHeader));
			ZDBOnlySubQuery docAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);

			ZDBOnlySubQuery addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			addressSubQuery.AddToFilter(JoinCondition.And, OrgAddressSchema.OA_OH, supplierPK);

			docAddressSubQuery.AddSubQuery(JobDocAddressSchema.E2_OA_Address, addressSubQuery, JoinCondition.And);
			result.AddSubQuery(docAddressSubQuery, JoinCondition.And);
			return result;
		}

		OrgHeaderCollection supplierList;
		OrgHeaderCollection SupplierList
		{
			get { return supplierList ?? (supplierList = new OrgHeaderCollection(new BusinessObjectFactory(), GetSupplierListFilter())); }
		}

		ZQuery GetSupplierListFilter()
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, true);
			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}
	}
}
