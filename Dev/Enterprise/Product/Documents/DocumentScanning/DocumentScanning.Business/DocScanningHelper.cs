using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business
{
	public class DocScanningHelper : IDocumentScanningHelper
	{
		public static CodeDescriptionPairList GetCategoryDocTypesFromJobType(ZString jobType, DocumentFactory factory, bool checkSecurityRights)
		{
			var docTypeQuery = GetDocTypeCategoryQuery(jobType, factory);
			return GetCategoryDocTypesFromJobType(docTypeQuery, factory, checkSecurityRights);
		}

		public static CodeDescriptionPairList GetCategoryDocTypesFromJobType(DocTypeCategoryQuery docTypeQuery, DocumentFactory factory, bool checkSecurityRights)
		{
			var list = new CodeDescriptionPairList();
			var docTypes = GetDocTypesForCategory(docTypeQuery, factory);
			foreach (var docType in docTypes)
			{
				if (!checkSecurityRights || Env.Security.GetDocumentTypeUploadCheckPoint(docType.RT_DocType).IsAllowed)
				{
					list.AddPair(docType.RT_DocType, docType.RT_DescMultilingual);
				}
			}
			return list;
		}

		public IRefDocTypeCollection GetDocTypesFromJobType(ZString jobType, BusinessObjectFactory factory, bool checkSecurityRights)
		{
			var documentFactory = new DbBackendDocumentFactory(factory);
			var docQuery = GetDocTypeCategoryQuery(jobType, documentFactory);
			return GetDocTypesFromJobType(docQuery, documentFactory, checkSecurityRights);
		}

		public IRefDocTypeCollection GetAvailableDocumentTypes(ZString docManagerCode, BusinessObjectFactory factory)
		{
			var referenceType = !docManagerCode.IsEmpty ?
				new ZString(AssemblyDataLookup.GetReferenceTypeFromDocManagerCode(docManagerCode))
				: new ZString(Core.Constants.ReferenceTypes.Unallocated);

			var docTypeQuery = new DocTypeCategoryQuery(factory, referenceType);
			return new RefDocTypeCollection(factory, docTypeQuery.AddToFilter(JoinCondition.And, RefDocTypeSchema.RT_IsActive, ZBool.True));
		}

		static IRefDocTypeCollection GetDocTypesFromJobType(DocTypeCategoryQuery docTypeQuery, DocumentFactory factory, bool checkSecurityRights)
		{
			var docTypes = GetDocTypesForCategory(docTypeQuery, factory);

			if (checkSecurityRights)
			{
				var excludedDocTypes = new List<string>();
				foreach (var docType in docTypes)
				{
					if (!Env.Security.GetDocumentTypeUploadCheckPoint(docType.RT_DocType).IsAllowed)
					{
						excludedDocTypes.Add(docType.RT_DocType);
					}
				}

				if (excludedDocTypes.Any())
				{
					docTypeQuery.AddToFilter(new ZQuery(RefDocTypeSchema.RT_DocType, SQLComparisonOperator.NotEqual, excludedDocTypes));
					return GetDocTypesForCategory(docTypeQuery, factory);
				}
			}
			return docTypes;
		}

		static RefDocTypeCollection GetDocTypesForCategory(DocTypeCategoryQuery docTypeQuery, DocumentFactory factory)
		{
			var visibleQuery = new ZQuery(RefDocTypeSchema.RT_IsActive, ZBool.True);
			docTypeQuery.AddToFilter(visibleQuery, JoinCondition.And);

			var docTypes = new RefDocTypeCollection(factory, docTypeQuery);
			docTypes.ApplySort(RefDocType.Schema.RT_DocType, ListSortDirection.Ascending);
			return docTypes;
		}

		public static DocTypeCategoryQuery GetDocTypeCategoryQuery(ZString jobType, DocumentFactory factory)
		{
			ZString categoryType = (AssemblyDataLookup.IsDocManagerCodeValid(jobType, false)) ?
				AssemblyDataLookup.GetReferenceTypeFromDocManagerCode(jobType) : Core.Constants.ReferenceTypes.Unallocated;

			return new DocTypeCategoryQuery(factory, categoryType);
		}

		public static CodeDescriptionPairList GetDocSources(DocumentFactory factory)
		{
			var visibleQuery = new ZQuery(RefDocSourceSchema.RDS_IsActive, ZBool.True);

			var sources = factory.Load<RefDocSource>(visibleQuery).OrderBy(x => x.RDS_Code);
			var list = new CodeDescriptionPairList();
			foreach (var docSource in sources)
			{
				list.AddPair(docSource.RDS_Code, docSource.RDS_DescMultilingual);
			}

			return list;
		}
	}
}
