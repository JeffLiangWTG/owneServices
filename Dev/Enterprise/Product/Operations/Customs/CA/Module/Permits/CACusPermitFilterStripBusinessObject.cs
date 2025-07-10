using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class CACusPermitFilterStripBusinessObject : CusPermitFilterStripBusinessObject
	{
		public new static class Schema
		{
			public const string DIFURN = "DIF URN";
			public const string DIFMessageStatus = "DIF Message Status";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			var difUrnFilter = filters.AddNumberFilter(Schema.DIFURN, (sqloperator, value) => GetDIFQuery(JobRequiredDocumentAddInfoSchema.EX_ReferenceNumber, value, sqloperator))
				.WithMaxLengthOf<ModuleNumberFilter>(JobRequiredDocumentAddInfoSchema.EX_ReferenceNumber);
			difUrnFilter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.StartsWith);
			difUrnFilter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.Contains);
			difUrnFilter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.NotEqual);
			difUrnFilter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.NotStartsWith);
			difUrnFilter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.NotContain);
			difUrnFilter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.IsBlank);
			difUrnFilter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.IsNotBlank);
			difUrnFilter.MultilingualDescription = ResString.GetMultilingualString("CA|CACusPermitFilterStripBusinessObject|DIFURN", Schema.DIFURN);
			var difMessageFilter = filters.AddTextFilter(Schema.DIFMessageStatus, value => GetDIFQuery(JobRequiredDocumentAddInfoSchema.EX_Status, value), DIFMessageStatusList)
				.WithMaxLengthOf<ModuleTextFilter>(JobRequiredDocumentAddInfoSchema.EX_Status);
			difMessageFilter.Category = FilterCategories.StatusAndFlags;
			difMessageFilter.MultilingualDescription = ResString.GetMultilingualString("CA|CACusPermitFilterStripBusinessObject|DIFMessageStatus", Schema.DIFMessageStatus);
			return filters;
		}

		ZQuery GetDIFQuery(SchemaStringColumn column, ZString value, SQLComparisonOperator @operator)
		{
			var result = new ZDBOnlyQuery(typeof(CusPermitHeader));
			var documentQuery = new ZDBOnlySubQuery(typeof(JobRequiredDocument), JobRequiredDocumentSchema.EQ_ParentID);
			var addinfoQuery = new ZDBOnlySubQuery(typeof(JobRequiredDocumentAddInfo), JobRequiredDocumentAddInfoSchema.EX_EQ_RequiredDocument);
			addinfoQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, column, @operator, value);
			documentQuery.AddSubQuery(addinfoQuery, JoinCondition.And);
			result.AddSubQuery(documentQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetDIFQuery(SchemaStringColumn column, ZString value)
		{
			return GetDIFQuery(column, value, SQLComparisonOperator.Equal);
		}

		public CodeDescriptionPairList DIFMessageStatusList => Factory.GetCachedValue<Common.CA.DIF.StatusList>();
	}
}
