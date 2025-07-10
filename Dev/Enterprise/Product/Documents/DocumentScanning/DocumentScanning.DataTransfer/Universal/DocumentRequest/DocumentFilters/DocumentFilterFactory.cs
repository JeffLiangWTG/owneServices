using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.DocumentScanning.DataTransfer.Universal
{
	static class DocumentFilterFactory
	{
		public static IEnumerable<DocumentFilterBase> GetDocumentFilters(IEnumerable<DocumentFilter> filterDataObjects)
		{
			if (filterDataObjects == null)
			{
				return null;
			}

			var filters = new Dictionary<DocumentFilterType, DocumentFilterBase>();

			foreach (var filterDataObject in filterDataObjects)
			{
				if (filterDataObject.Type.HasValue && filterDataObject.Value.HasValue)
				{
					DocumentFilterBase filter;
					if (!filters.TryGetValue(filterDataObject.Type.Value, out filter))
					{
						filter = CreateDocumentFilter(filterDataObject.Type.Value);
						filters.Add(filterDataObject.Type.Value, filter);
					}

					filter.AddFilterValue(filterDataObject.Value.Value);
				}
			}

			return filters.Values;
		}

		internal static DocumentFilterBase CreateDocumentFilter(DocumentFilterType filterType)
		{
			switch (filterType)
			{
				case DocumentFilterType.SaveDateUTCFrom:
					return new DocumentFilterDateTime(DocumentFilterDateTime.ComparisonOption.From);
				case DocumentFilterType.SaveDateUTCTo:
					return new DocumentFilterDateTime(DocumentFilterDateTime.ComparisonOption.To);
				case DocumentFilterType.DocumentType:
					return new DocumentFilterCode(e => e.DocType);
				case DocumentFilterType.CompanyCode:
					return new DocumentFilterCode(e => e.VisibleCompanyCode);
				case DocumentFilterType.BranchCode:
					return new DocumentFilterCode(e => e.VisibleBranchCode);
				case DocumentFilterType.DepartmentCode:
					return new DocumentFilterCode(e => e.VisibleDepartmentCode);
				case DocumentFilterType.IsPublished:
					return new DocumentFilterBool(nameof(DocumentFilterType.IsPublished), e => e.IsPublished);
				case DocumentFilterType.FileName:
					return new DocumentFilterCode(e => e.FileName);
				case DocumentFilterType.DocumentID:
					return new DocumentFilterGuid(nameof(DocumentFilterType.DocumentID), e => e.UniqueKey);
				case DocumentFilterType.RelatedEDoc:
					return new DocumentFilterRelatedEDoc();
				default:
					return null;
			}
		}
	}
}
