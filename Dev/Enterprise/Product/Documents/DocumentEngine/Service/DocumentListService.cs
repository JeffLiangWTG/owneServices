using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Service
{
	public class DocumentListService : IDocumentListService
	{
		public DocumentListItem[] GetDocumentList(string businessContext, OrgContact contact, ZGuid entityPK, string entityTableCode)
		{
			var query = new ZQuery(StmMenuItemSchema.SU_BusinessContext, businessContext);
			var webReportFilter = new ZQuery(StmMenuItemSchema.SU_IsVisibleOnWeb, ZBool.True);

			var publishedFilter = new ZQuery(StmMenuItemSchema.SU_GS_NKStaffCode, "");
			var staffCode = GlbStaff.CurrentUser.GS_Code;
			if (!string.IsNullOrEmpty(staffCode))
			{
				var privateFilter = new ZQuery(StmMenuItemSchema.SU_GS_NKStaffCode, staffCode);
				publishedFilter.AddToFilter(privateFilter, JoinCondition.Or);
			}
			query.AddToFilter(publishedFilter, JoinCondition.And);

			if (contact != null)
			{
				query.AddToFilter(webReportFilter);
			}
			else
			{
				var documentFilter = new ZQuery(StmMenuItemSchema.SU_MenuType, Core.Constants.StmMenuItemTypes.Documents);
				var combinedFilter = new ZQuery(documentFilter, JoinCondition.Or, webReportFilter);
				query.AddToFilter(combinedFilter, JoinCondition.And);
			}

			var factory = new BusinessObjectFactory { NameForDebugging = "DocumentList WebService" };
			var parent = GetParent(factory, entityPK, entityTableCode);

			var rawDocuments = factory.Load<DocumentCommand>(query);
			var documents = new List<DocumentListItem>();
			var eDocsSecurity = new eDocsWebSecurity(factory, contact);

			foreach (var rawDoc in rawDocuments)
			{
				rawDoc.Parent = parent;

				if (contact != null)
				{
					var docTypes = rawDoc.Documents
						.OfType<StmMenuTemplatePivotBase>().Where(x => x.DocType != null)
						.Select(x => x.DocType);

					if (docTypes.All(docType => !eDocsSecurity.CanViewDocument(docType)))
					{
						continue;
					}
				}

				documents.Add(new DocumentListItem
				{
					Id = rawDoc.PK.ToGuid(),
					Name = rawDoc.SU_MenuName,
					Summary = rawDoc.SU_Hint,
					Path = rawDoc.SU_MenuPath,
					Index = rawDoc.SU_MenuIndex,
					DownloadOnly = rawDoc.SU_DownloadOnly,
					IsApplicable = rawDoc.IsApplicable
				});
			}

			return documents.ToArray();
		}
		IDocumentSupportable GetParent(BusinessObjectFactory factory, ZGuid entityPK, string entityTableCode)
		{
			return factory.Load(entityTableCode, entityPK) as IDocumentSupportable;
		}
	}

	#region Types
	public class DocumentListItem
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string Summary { get; set; }
		public string Path { get; set; }
		public int Index { get; set; }
		public bool DownloadOnly { get; set; }
		public bool IsApplicable { get; set; }
	}

	#endregion
}
