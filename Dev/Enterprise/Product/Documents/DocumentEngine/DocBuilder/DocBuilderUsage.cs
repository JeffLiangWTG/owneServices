using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.DocBuilder
{
	public class DocBuilderUsage : DocumentMacroUsage
	{
		public DocBuilderUsage()
		{ }

		public DocBuilderUsage(string macro, string templateName)
			: base(macro, templateName)
		{ }

		public override ZString AllDocumentNames
		{
			get
			{
				if (!allDocumentNames.HasValue)
				{
					var query = new ZDBOnlyQuery(typeof(StmMenuTemplatePivot));
					ZDBOnlySubQuery configQuery = new ZDBOnlySubQuery(typeof(StmMenuDocumentConfig), StmMenuDocumentConfigSchema.S3_SI);
					configQuery.AddToFilter(StmMenuDocumentConfigSchema.S3_IsSystem, true);
					ZDBOnlySubQuery configItemQuery = new ZDBOnlySubQuery(typeof(StmMenuDocumentConfigItem), StmMenuDocumentConfigItemSchema.S4_S3);
					configItemQuery.AddToFilter(StmMenuDocumentConfigItemSchema.S4_SectionItemName, TemplateName);
					configQuery.AddSubQuery(configItemQuery, JoinCondition.And);
					query.AddSubQuery(configQuery, JoinCondition.And);
					var results = new BusinessObjectFactory().Load<StmMenuTemplatePivot>(query);
					allDocumentNames = string.Join("\r\n", new HashSet<ZString>(Array.ConvertAll(results, item => item.SI_DocumentTitle)));
				}
				return allDocumentNames.Value;
			}
		}
		ZString? allDocumentNames;
	}
}
