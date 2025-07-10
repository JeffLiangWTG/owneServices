using System.Collections.Generic;
using System.Text;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(ConstraintJI_ParentTableCode))]
	sealed class ConstraintJI_ParentTableCodeTest : ConstraintBase_ParentTableCodeTest<ConstraintJI_ParentTableCode>
	{
		protected override string TableName => "JobComInvoiceLine";

		protected override string TablePrefix => "JI";

		protected override string[] SupportedParentPrefixes => new[] { "JI", "WOL" };

		protected override bool UseNoCheck => true;

		protected override bool AllowEmptyParentTableCode => true;

		protected override void AppendInsertScript(StringBuilder sqlText, Dictionary<string, string> columnValues)
		{
			columnValues.Add(JobComInvoiceLineSchema.Constants.JI_JZ, "@JobComInvoiceHeaderPK");
			columnValues.Add(JobComInvoiceLineSchema.Constants.JI_DataModel, "'AI'");
			columnValues.Add(JobComInvoiceLineSchema.Constants.JI_ClusterKey, "1234567");
			base.AppendInsertScript(sqlText, columnValues);

			var pk = columnValues[JobComInvoiceLineSchema.Constants.PK];
			AddTableToScript(sqlText, "CO");
			AddTableToScript(sqlText, "JE");
			AddTableToScript(sqlText, "CW");
			AddTableToScript(sqlText, "CUL");
			AddTableToScript(sqlText, "CL");
			sqlText.AppendLine($"INSERT INTO dbo.CusContainerInvoiceLinePivot (C2_PK, C2_SystemCreateTimeUtc, C2_SystemCreateUser, C2_SystemLastEditTimeUtc, C2_SystemLastEditUser, C2_JI, C2_CO, C2_ClusterKey) VALUES (NEWID(), GetUtcDate(), '~BP', GetUtcDate(), '~BP', {pk}, @CusContainerPK, 1234567);");
			sqlText.AppendLine($"INSERT INTO dbo.CusHouseContPackInvoiceLinePivot (CHC_PK, CHC_SystemCreateTimeUtc, CHC_SystemCreateUser, CHC_SystemLastEditTimeUtc, CHC_SystemLastEditUser, CHC_JI, CHC_JE, CHC_CW, CHC_ClusterKey) VALUES (NEWID(), GetUtcDate(), '~BP', GetUtcDate(), '~BP', {pk}, @JobDeclarationPK, @CusDecHouseContainerPackPK, 1234567);");
			sqlText.AppendLine($"INSERT INTO dbo.CusPackableItem (CUI_PK, CUI_SystemCreateTimeUtc, CUI_SystemCreateUser, CUI_SystemLastEditTimeUtc, CUI_SystemLastEditUser, CUI_JI, CUI_CUL, CUI_ClusterKey) VALUES (NEWID(), GetUtcDate(), '~BP', GetUtcDate(), '~BP', {pk}, @CusPackingListPK, 1234567);");
			sqlText.AppendLine($"INSERT INTO dbo.CusRulingConfig (ZZY_PK, ZZY_SystemCreateTimeUtc, ZZY_SystemCreateUser, ZZY_SystemLastEditTimeUtc, ZZY_SystemLastEditUser, ZZY_JI_InvoiceLine, ZZY_Category, ZZY_Type) VALUES (NEWID(), GetUtcDate(), '~BP', GetUtcDate(), '~BP', {pk}, 'DAT', 'AAA');");
			sqlText.AppendLine($"INSERT INTO dbo.CusUnderbondDec (BU_PK, BU_SystemCreateTimeUtc, BU_SystemCreateUser, BU_SystemLastEditTimeUtc, BU_SystemLastEditUser, BU_JI, BU_CL, BU_ClusterKey) VALUES (NEWID(), GetUtcDate(), '~BP', GetUtcDate(), '~BP', {pk}, @CusEntryLinePK, 1234567);");
			sqlText.AppendLine($"INSERT INTO dbo.JobComInvLineComponentInventory (JIV_PK, JIV_SystemCreateTimeUtc, JIV_SystemCreateUser, JIV_SystemLastEditTimeUtc, JIV_SystemLastEditUser, JIV_JI, JIV_ClusterKey) VALUES (NEWID(), GetUtcDate(), '~BP', GetUtcDate(), '~BP', {pk}, 1234567);");
			sqlText.AppendLine($"INSERT INTO dbo.JobComInvLineRefs (JG_PK, JG_SystemCreateTimeUtc, JG_SystemCreateUser, JG_SystemLastEditTimeUtc, JG_SystemLastEditUser, JG_JI, JG_ClusterKey) VALUES (NEWID(), GetUtcDate(), '~BP', GetUtcDate(), '~BP', {pk}, 1234567);");
			sqlText.AppendLine($"INSERT INTO dbo.JobComInvoiceLineTax (JLT_PK, JLT_SystemCreateTimeUtc, JLT_SystemCreateUser, JLT_SystemLastEditTimeUtc, JLT_SystemLastEditUser, JLT_JI, JLT_ClusterKey, JLT_Type) VALUES (NEWID(), GetUtcDate(), '~BP', GetUtcDate(), '~BP', {pk}, 1234567, 'AAA');");
			sqlText.AppendLine($"INSERT INTO dbo.JobTWComInvoiceLine (TWL_PK, TWL_SystemCreateTimeUtc, TWL_SystemCreateUser, TWL_SystemLastEditTimeUtc, TWL_SystemLastEditUser, TWL_JI, TWL_ClusterKey) VALUES (NEWID(), GetUtcDate(), '~BP', GetUtcDate(), '~BP', {pk}, 1234567);");
			sqlText.AppendLine($"INSERT INTO dbo.JobUSComInvoiceLine (USI_PK, USI_JI, USI_ClusterKey) VALUES (NEWID(), {pk}, 1234567);");
			sqlText.AppendLine($"INSERT INTO dbo.QuarantineExDocLine (QL_PK, QL_SystemCreateTimeUtc, QL_SystemCreateUser, QL_SystemLastEditTimeUtc, QL_SystemLastEditUser, QL_JI, QL_ClusterKey) VALUES ({pk}, GetUtcDate(), '~BP', GetUtcDate(), '~BP', {pk}, 1234567);");
			sqlText.AppendLine($"INSERT INTO dbo.QuarantineExDocEstablishmentAndTime (EE_PK, EE_SystemCreateTimeUtc, EE_SystemCreateUser, EE_SystemLastEditTimeUtc, EE_SystemLastEditUser, EE_QL, EE_ClusterKey) VALUES (NEWID(), GetUtcDate(), '~BP', GetUtcDate(), '~BP', {pk}, 1234567);");
		}

		protected override ConstraintJI_ParentTableCode CreateNewTransformationInstance()
		{
			return new ConstraintJI_ParentTableCode(3);
		}
	}
}
