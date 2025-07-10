using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Transformation.DataModification.AddInfoTransformationBase;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing
{
	class CopyAddInfoToRealColumnTestClass : CopyAddInfoToRealColumn
	{
		public override string UserDescription => $"Copy AddInfo value from JI_AddInfo to JobComInvoiceLine (JI_ValuationDateOverride, JI_CustomDate4, JI_OrderNumber, JI_CustomDecimal1)";
		public override string AdditionalSourceTableJoin => @"INNER JOIN dbo.JobComInvoiceHeader ON JI_JZ = JZ_PK AND JI_ClusterKey = JZ_ClusterKey
INNER JOIN dbo.JobDeclaration ON JZ_JE = JE_PK AND JZ_ClusterKey = JE_ClusterKey
INNER JOIN dbo.GlbCompany ON GC_PK = JE_GC AND GC_RN_NKCountryCode = 'JP'";
		public override SchemaStringColumn SourceAddInfoColumn => JobComInvoiceLineSchema.JI_AddInfo;
		protected override IDictionary<SchemaColumn, (string AddInfoPropertyName, string SelectStatementOverride, string AddInfoValueJointStatement)> GetAddInfoColumnMapping()
		{
			var result = new Dictionary<SchemaColumn, (string AddInfoPropertyName, string SelectStatementOverride, string AddInfoValueJointStatement)>(4);
			result.Add(JobComInvoiceLineSchema.JI_ValuationDateOverride, ("DisposalDate", null, null));
			result.Add(JobComInvoiceLineSchema.JI_CustomDate4, ("ScheduledReExportDate", null, null));
			result.Add(JobComInvoiceLineSchema.JI_OrderNumber, ("AdditionalTariffCode", null, null));
			result.Add(JobComInvoiceLineSchema.JI_CustomDecimal1, ("AdditionalDutyRate", null, null));
			return result;
		}
	}
}
