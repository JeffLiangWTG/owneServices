using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using static Enterprise.ZArchitecture.Schema.CusEntryHeaderSchema.Constants;
using static Enterprise.ZArchitecture.Schema.JobDeclarationSchema.Constants;
namespace Enterprise.Customs.Forwarding.Module;

static class CustomsEntryStatusFilterHelper
{
	const string JE_PK = JobDeclarationSchema.Constants.PK;
	const string JS_PK = JobShipmentSchema.Constants.PK;
	const string CusEntryHeader = CusEntryHeaderSchema.Constants.TableName;
	const string JobDeclaration = JobDeclarationSchema.Constants.TableName;
	const string JobShipment = JobShipmentSchema.Constants.TableName;

	const string PositiveSql = $@"
{JS_PK} IN (
	SELECT {JS_PK}
	FROM {JobShipment}
	LEFT JOIN {JobDeclaration} allDeclarationsInCurrentCompany ON {JE_JS} = {JS_PK} AND {JE_GC} = @CurrentCompany
	LEFT JOIN {CusEntryHeader} ON {CH_ClusterKey} = {JE_ClusterKey}
	WHERE (ISNULL({CH_EntryStatus}, '') = @customsStatus)
		OR (@customsStatus = '' AND {JE_PK} IS NULL)
)
";
	const string NegativeSql = $@"
{JS_PK} IN (
	SELECT {JS_PK}
	FROM {JobShipment}
	LEFT JOIN {JobDeclaration} allDeclarationsInCurrentCompany ON {JE_JS} = {JS_PK} AND {JE_GC} = @CurrentCompany
	LEFT JOIN {CusEntryHeader} ON {CH_ClusterKey} = {JE_ClusterKey}
	WHERE ISNULL({CH_EntryStatus}, '') <> @customsStatus
)
";

	public static ZQuery GetEntryStatusQuery(SQLComparisonOperator filterOperator, ZString customsEntryStatus)
	{
		var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
		if (!customsEntryStatus.IsEmpty || filterOperator.In(SQLComparisonOperator.IsBlank, SQLComparisonOperator.IsNotBlank))
		{
			var sql = filterOperator.In(SQLComparisonOperator.NotEqual, SQLComparisonOperator.IsNotBlank) ? NegativeSql : PositiveSql;

			result.AddFilterAndZSQLParameterCollection(
				sql,
				new ZSqlParameterCollection(
					ZSqlParameter.New("@customsStatus", customsEntryStatus, CusEntryHeaderSchema.CH_EntryStatus, filterOperator),
					ZSqlParameter.New("@CurrentCompany", GlbCompany.CurrentCompany.PK, JobDeclarationSchema.JE_GC)
				)
			);
		}
		return result;
	}

	public static ZQuery GetEntryStatusQueryAllEntries(SQLComparisonOperator comparisonOperator, ZString value)
	{
		var shipmentQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
		const string whereShipmentDoestHaveDeclOrEntryInCurrentCompany = $@"
{JS_PK} IN (
	SELECT {JS_PK}
	FROM {JobShipment}
	LEFT JOIN {JobDeclaration} allDeclarationsInCurrentCompany ON {JE_JS} = {JS_PK} AND {JE_GC} = @CurrentCompany
	WHERE {JE_PK} IS NULL
)
";

		var jobDeclarationWithEntryStatusQuery = JobDeclarationFilter.GetEntryStatusQueryAllEntries(comparisonOperator, value);

		var declarationsInCurrentCompanyWithEntry = new ZDBOnlySubQuery(typeof(IBaseJobDeclaration), JobDeclarationSchema.JE_JS);
		declarationsInCurrentCompanyWithEntry.AddToFilter(jobDeclarationWithEntryStatusQuery);
		declarationsInCurrentCompanyWithEntry.AddToFilter(JobDeclarationSchema.JE_GC, GlbCompany.CurrentCompany.PK);

		shipmentQuery.AddSubQuery(declarationsInCurrentCompanyWithEntry, JoinCondition.And);

		if (value.IsEmpty)
		{
			var whereShipmentDoesNotHaveDeclOrEntryInCurrentCompanyZQuery =
				new ZDBOnlyQuery(typeof(ForwardingShipment));
			whereShipmentDoesNotHaveDeclOrEntryInCurrentCompanyZQuery.AddFilterAndZSQLParameterCollection(
				whereShipmentDoestHaveDeclOrEntryInCurrentCompany, new ZSqlParameterCollection(
						ZSqlParameter.New("@CurrentCompany", GlbCompany.CurrentCompany.PK,
							JobDeclarationSchema.JE_GC)
					));
			shipmentQuery.AddToFilter(whereShipmentDoesNotHaveDeclOrEntryInCurrentCompanyZQuery, JoinCondition.Or);
		}

		return shipmentQuery;
	}
}

