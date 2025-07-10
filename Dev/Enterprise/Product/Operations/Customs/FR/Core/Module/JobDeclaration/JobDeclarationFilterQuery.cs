using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.FR.Module
{
	public static class JobDeclarationFilterQuery
	{
		public static ZQuery GetDeltaModeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			var referenceDataSubQuery = DeclarationGenAddOnColumnHelper.GetQueryHandlingBlanks(JobDeclaration.Schema.JE_DeltaMode, comparisonOperator, value);
			result.AddToFilter(referenceDataSubQuery);

			return result;
		}

		[ThreadSafe]
		static GenAddOnColumnQueryHelper declarationGenAddOnColumnHelper;
		public static GenAddOnColumnQueryHelper DeclarationGenAddOnColumnHelper =>
			declarationGenAddOnColumnHelper
			?? (declarationGenAddOnColumnHelper = new GenAddOnColumnQueryHelper(typeof(JobDeclaration)));

		public static ZDBOnlySubQuery GetDeltaDQuery(ZBool value)
		{
			var deltaModeQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID, !value);
			deltaModeQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, JobDeclaration.Schema.JE_DeltaMode);
			deltaModeQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, OrgCusAccountDeltaGTypeList.Codes.G2);
			return deltaModeQuery;
		}

		public static ZDBOnlySubQuery GetFallbackEntryNumSubQuery()
		{
			var fallbackEntryNumSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			fallbackEntryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, Core.Constants.CountryCodes.France);
			fallbackEntryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, SQLComparisonOperator.Equal, CusEntryHeaderSchema.Constants.TableName);
			fallbackEntryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypes.France.Fallback);
			return fallbackEntryNumSubQuery;
		}
	}
}
