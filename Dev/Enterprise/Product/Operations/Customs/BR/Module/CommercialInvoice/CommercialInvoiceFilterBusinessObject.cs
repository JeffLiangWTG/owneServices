using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Module
{
	public class CommercialInvoiceFilterBusinessObject : Customs.Module.CommercialInvoiceFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AddLPCOFilter(filters);
			AddImportLicenseFilter(filters);
			AddForeignDeclarationFilter(filters);
			return filters;
		}

		void AddLPCOFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(CommercialInvoiceFilterConstants.Lpco, GetLPCOQuery);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("CommercialInvoiceFilterConstants|Lpco", CommercialInvoiceFilterConstants.Lpco);
			filter.SubGroup = JobComInvLineRefsSubGroup;
		}

		ZQuery GetLPCOQuery(SQLComparisonOperator @operator, ZString value)
		{
			var lpcoQuery = new ZQuery(JobComInvLineRefsSchema.JG_ReferenceType, JobComInvLineRefsType.Codes.Lpco);
			lpcoQuery.AddToFilter_PossiblyCommaSeparated(JobComInvLineRefsSchema.JG_ReferenceNumber, @operator, value);

			return lpcoQuery;
		}

		void AddImportLicenseFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(CommercialInvoiceFilterConstants.ImportLicense, GetImportLicenseNumber);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("CommercialInvoiceFilterConstants|ImportLicense", CommercialInvoiceFilterConstants.ImportLicense);
			filter.MaxLength = ImportLicenseInfo.Schema.CSI_ReferenceNumberMaxLength;
			filter.SubGroup = InvoiceLineCusSupportingInfoSubGroup;
		}

		void AddForeignDeclarationFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(CommercialInvoiceFilterConstants.ForeignDeclaration, GetForeignDeclarationQuery);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("CommercialInvoiceFilterConstants|ForeignDeclaration", CommercialInvoiceFilterConstants.ForeignDeclaration);
			filter.MaxLength = MercosulForeignDeclaration.Schema.DescriptionMaxLength;
			filter.SubGroup = InvoiceLineCusSupportingInfoSubGroup;
		}

		ZQuery GetForeignDeclarationQuery(SQLComparisonOperator @operator, ZString value)
		{
			var linkedDocQuery = new ZQuery(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.MercosulForeignDeclaration);
			linkedDocQuery.AddToFilter(CusSupportingInfoSchema.CSI_Description, @operator, value);

			return linkedDocQuery;
		}

		ZQuery GetImportLicenseNumber(SQLComparisonOperator @operator, ZString value)
		{
			var linkedDocQuery = new ZQuery(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.ImportLicense);
			linkedDocQuery.AddToFilter(CusSupportingInfoSchema.CSI_ReferenceNumber, @operator, value);

			return linkedDocQuery;
		}

		#region Sub Groups

		InvoiceLineCusSupportingInfoFilterSubGroup InvoiceLineCusSupportingInfoSubGroup => invoiceLineCusSupportingInfoSubGroup ?? (invoiceLineCusSupportingInfoSubGroup = new InvoiceLineCusSupportingInfoFilterSubGroup());
		InvoiceLineCusSupportingInfoFilterSubGroup invoiceLineCusSupportingInfoSubGroup;

		class InvoiceLineCusSupportingInfoFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var invoiceQuery = new ZDBOnlyQuery(typeof(JobComInvoiceHeader));
				var invoiceLineQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceLine), JobComInvoiceLineSchema.JI_ClusterKey, JobComInvoiceHeaderSchema.JZ_ClusterKey);
				var supportingInfoQuery = new ZDBOnlySubQuery(typeof(CusSupportingInfo), CusSupportingInfoSchema.CSI_ParentID);
				supportingInfoQuery.AddToFilter(filter);
				invoiceLineQuery.AddSubQuery(supportingInfoQuery, JoinCondition.And);
				invoiceQuery.AddSubQuery(invoiceLineQuery, JoinCondition.And);
				return invoiceQuery;
			}
		}

		JobComInvLineRefsFilterSubGroup JobComInvLineRefsSubGroup => invLineRefsFilterSubGroup ?? (invLineRefsFilterSubGroup = new JobComInvLineRefsFilterSubGroup());
		JobComInvLineRefsFilterSubGroup invLineRefsFilterSubGroup;

		class JobComInvLineRefsFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var invoiceQuery = new ZDBOnlyQuery(typeof(JobComInvoiceHeader));
				var invoiceLineQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceLine), JobComInvoiceLineSchema.JI_ClusterKey, JobComInvoiceHeaderSchema.JZ_ClusterKey);
				var supportingInfoQuery = new ZDBOnlySubQuery(typeof(JobComInvLineRefs), JobComInvLineRefsSchema.JG_JI);
				supportingInfoQuery.AddToFilter(filter);
				invoiceLineQuery.AddSubQuery(supportingInfoQuery, JoinCondition.And);
				invoiceQuery.AddSubQuery(invoiceLineQuery, JoinCondition.And);
				return invoiceQuery;
			}
		}

		#endregion
	}
}
