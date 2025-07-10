using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Schema.OnlineUpgrade;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification.AddInfoTransformationBase;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.NZ;

class CopyAddInfoCountryOfExportToRealColumn : CopyAddInfoToRealColumn
{
	public override string UserDescription => "Copy RN_NKCountryOfExport from NZ JZ_AddInfo to JobComInvoiceHeader Columns";
	public override SchemaStringColumn SourceAddInfoColumn => JobComInvoiceHeaderSchema.JZ_AddInfo;
	public override string SourceTableHint => "WITH (FORCESEEK)";
	public override string AdditionalSourceTableJoin => @"
	INNER JOIN
	(
		SELECT JZ_PK AS PK
		FROM dbo.JobComInvoiceHeader
		WHERE JZ_DataModel = 'NZ' AND JZ_RN_NKCountryOfExport = ''
	) NZHeader ON NZHeader.PK = JZ_PK ";

	protected override IDictionary<SchemaColumn, (string AddInfoPropertyName, string SelectStatementOverride, string AddInfoValueJointStatement)> GetAddInfoColumnMapping()
	{
		return new Dictionary<SchemaColumn, (string AddInfoPropertyName, string SelectStatementOverride, string AddInfoValueJointStatement)>()
		{
			{ JobComInvoiceHeaderSchema.JZ_RN_NKCountryOfExport, ("RN_NKCountryOfExport", null, null) }
		};
	}

	protected override void OnlinePreUpgradeTransform()
	{
		new OnlineCopyAddInfoCountryOfExportToRealColumn(manager, Db.Connection.CurrentDatabase, TemplateDb).Run();
	}

	internal class OnlineCopyAddInfoCountryOfExportToRealColumn(IUpgradeManager manager, string dbBeingUpgraded, string templateDb)
		: OnlineCopyAddInfoToRealColumn<CopyAddInfoCountryOfExportToRealColumn>(manager, dbBeingUpgraded, templateDb)
	{
		protected override string UserDescription => "Populate NZ JobComInvoiceHeader columns from JZ_AddInfo";
	}
}
