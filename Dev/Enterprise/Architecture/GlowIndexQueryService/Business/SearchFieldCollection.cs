using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GlowIndexQueryService.Business
{
	public class SearchFieldCollection
	{
		public SearchFieldCollection(GlowIndexQueryStatus errorStatus)
		{
			if (errorStatus == GlowIndexQueryStatus.Success)
			{
				throw new ArgumentException("errorStatus must be an error status", nameof(errorStatus));
			}
			Status = errorStatus;
			Value = ImmutableArray<SearchField>.Empty;
		}

		public SearchFieldCollection(string entityType, SearchField[] value)
		{
			EntityType = GlowModuleToCW1ModuleConverter.NormalizeTypeName(entityType);
			Value = ImmutableArray.Create(value);
			Status = GlowIndexQueryStatus.Success;
		}

		public SearchField this[string fieldName]
		{
			get
			{
				searchFields ??= GetSearchFields();

				searchFields.TryGetValue(fieldName, out var searchField);

				return searchField;
			}
		}

		Dictionary<string, SearchField> GetSearchFields()
		{
			var fields = new Dictionary<string, SearchField>(Value.Length, StringComparer.InvariantCultureIgnoreCase);
			foreach (var searchField in Value)
			{
				fields[searchField.FieldName] = searchField;
			}

			return fields;
		}

		Dictionary<string, SearchField> searchFields;
		public GlowIndexQueryStatus Status { get; }
		public string EntityType { get; }
		public ImmutableArray<SearchField> Value { get; }
		public SearchField[] DefaultHiddenIndexSearchFields => Value.Where(@field => @field.FieldName.StartsWith("CWDEFAULTHIDDEN") || @field.UIHidden)?.ToArray() ?? Array.Empty<SearchField>();
	}
}
