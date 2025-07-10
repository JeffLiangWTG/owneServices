using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.DocBuilder
{
	class DocumentConfigPicker
	{
		internal DocumentConfigPicker(StmMenuTemplatePivot document, bool ignoreUserCustomizedTemplatesAndConfigurations)
		{
			this.document = document;
			this.ignoreUserCustomizedTemplatesAndConfigurations = ignoreUserCustomizedTemplatesAndConfigurations;
		}

		readonly StmMenuTemplatePivot document;
		readonly bool ignoreUserCustomizedTemplatesAndConfigurations;

		internal StmMenuDocumentConfig GetDocumentConfig(OrgHeader client, GlbCompany company)
		{
			var matchQuery = new ZQuery();

			if (ignoreUserCustomizedTemplatesAndConfigurations)
			{
				var innerMatchQuery = new ZQuery();
				innerMatchQuery.AddToFilter(StmMenuDocumentConfigSchema.S3_OH, null);
				innerMatchQuery.AddToFilter(StmMenuDocumentConfigSchema.S3_GC, null);
				innerMatchQuery.AddToFilter(StmMenuDocumentConfigSchema.S3_IsSystem, ZBool.True);
				matchQuery.AddToFilter(innerMatchQuery, JoinCondition.Or);
			}
			else
			{
				if (client != null && company != null)
				{
					var innerMatchQuery = new ZQuery();
					innerMatchQuery.AddToFilter(StmMenuDocumentConfigSchema.S3_OH, client.PK);
					innerMatchQuery.AddToFilter(StmMenuDocumentConfigSchema.S3_GC, company.PK);
					innerMatchQuery.AddToFilter(StmMenuDocumentConfigSchema.S3_IsSystem, ZBool.False);
					matchQuery.AddToFilter(innerMatchQuery, JoinCondition.Or);
				}

				if (client != null)
				{
					var innerMatchQuery = new ZQuery();
					innerMatchQuery.AddToFilter(StmMenuDocumentConfigSchema.S3_OH, client.PK);
					innerMatchQuery.AddToFilter(StmMenuDocumentConfigSchema.S3_GC, null);
					innerMatchQuery.AddToFilter(StmMenuDocumentConfigSchema.S3_IsSystem, ZBool.False);
					matchQuery.AddToFilter(innerMatchQuery, JoinCondition.Or);
				}

				if (company != null)
				{
					var innerMatchQuery = new ZQuery();
					innerMatchQuery.AddToFilter(StmMenuDocumentConfigSchema.S3_OH, null);
					innerMatchQuery.AddToFilter(StmMenuDocumentConfigSchema.S3_GC, company.PK);
					innerMatchQuery.AddToFilter(StmMenuDocumentConfigSchema.S3_IsSystem, ZBool.False);
					matchQuery.AddToFilter(innerMatchQuery, JoinCondition.Or);
				}

				{
					var innerMatchQuery = new ZQuery();
					innerMatchQuery.AddToFilter(StmMenuDocumentConfigSchema.S3_OH, null);
					innerMatchQuery.AddToFilter(StmMenuDocumentConfigSchema.S3_GC, null);
					matchQuery.AddToFilter(innerMatchQuery, JoinCondition.Or);
				}
			}

			var configQuery = new ZQuery(StmMenuDocumentConfigSchema.S3_SI, document.PK);
			configQuery.AddToFilter(matchQuery, JoinCondition.And);
			configQuery.AddToFilter(StmMenuDocumentConfigSchema.S3_ExcludedFromDocPack, false);
			configQuery.OrderBy = StmMenuDocumentConfigSchema.S3_OH.Name + " desc, "
								+ StmMenuDocumentConfigSchema.S3_GC.Name + " desc, "
								+ StmMenuDocumentConfigSchema.S3_IsSystem.Name + ", "
								+ StmMenuDocumentConfigSchema.S3_IsTemplate.Name + " asc";

			return document.Factory.LoadTop1<StmMenuDocumentConfig>(configQuery);
		}
	}
}
