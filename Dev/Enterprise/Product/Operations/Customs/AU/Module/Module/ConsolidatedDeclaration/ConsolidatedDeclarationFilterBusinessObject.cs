using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module
{
	public sealed class ConsolidatedDeclarationFilterBusinessObject : Customs.Module.ConsolidatedDeclarationFilterBusinessObject
	{
		protected override CodeDescriptionPairList MessageStatusList => JobDeclarationFilterLookups.CMRMessageStatusList;

		JobDeclarationFilterLookups JobDeclarationFilterLookups => jobDeclarationFilterBusinessObject.Lookups as JobDeclarationFilterLookups;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();
			AddPaymentStatusFilter(result);
			return result;
		}

		void AddPaymentStatusFilter(ModuleFilterCollection filters)
		{
			var paymentStatusFilter = filters.AddTextFilter(DeclarationFilterConstants.PaymentStatusText, GetPaymentStatusQuery, JobDeclarationFilterLookups.PaymentStatusList);
			paymentStatusFilter.Category = FilterCategories.StatusAndFlags;
		}

		ZQuery GetPaymentStatusQuery(ZString value)
		{
			var result = new ZQuery();
			if (value == DeclarationFilterConstants.PaymentStatus.Paid)
			{
				var headerQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
				headerQuery.AddToFilter(JoinCondition.Or, CusEntryHeaderSchema.CH_AddInfo, SQLComparisonOperator.Contains, AUAddInfoSchema.ZA_PaymentStatus_Hidden.Name.Substring(3) + "=" + CMREntryPaymentStatusList.Codes.Paid);
				headerQuery.AddToFilter(JoinCondition.Or, CusEntryHeaderSchema.CH_AddInfo, SQLComparisonOperator.Contains, AUAddInfoSchema.ZA_PaymentStatus_Hidden.Name.Substring(3) + "=" + CMREntryPaymentStatusList.Codes.Refunded);
				var jobDecQuery = new ZDBOnlyQuery(typeof(ConsolidatedDeclaration));
				jobDecQuery.AddSubQuery(CusReconDeclarationSchema.CRD_JE_LeadDeclaration, headerQuery, JoinCondition.And);
				result.AddToFilter(jobDecQuery);
			}
			else if (value == DeclarationFilterConstants.PaymentStatus.NotPaid)
			{
				var headerQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
				var allHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE, true);
				headerQuery.AddToFilter(JoinCondition.And, CusEntryHeaderSchema.CH_AddInfo, SQLComparisonOperator.NotContains, AUAddInfoSchema.ZA_PaymentStatus_Hidden.Name.Substring(3) + "=" + CMREntryPaymentStatusList.Codes.Paid);
				headerQuery.AddToFilter(JoinCondition.And, CusEntryHeaderSchema.CH_AddInfo, SQLComparisonOperator.NotContains, AUAddInfoSchema.ZA_PaymentStatus_Hidden.Name.Substring(3) + "=" + CMREntryPaymentStatusList.Codes.Refunded);
				var jobDecQuery = new ZDBOnlyQuery(typeof(ConsolidatedDeclaration));
				jobDecQuery.AddSubQuery(CusReconDeclarationSchema.CRD_JE_LeadDeclaration, headerQuery, JoinCondition.And);
				jobDecQuery.AddSubQuery(CusReconDeclarationSchema.CRD_JE_LeadDeclaration, allHeaderQuery, JoinCondition.Or);
				result.AddToFilter(jobDecQuery);
			}
			return result;
		}
	}
}
