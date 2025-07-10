using System;
using System.Linq;
#if NETFRAMEWORK
using Enterprise.ZArchitecture.Core;
#endif

namespace GlowIndexQueryService.Common
{
	class SearchFieldDto
	{
		public SearchFieldDto(string fieldName, string description, bool isKeyField, bool isSortable, bool uiHidden, FieldTypeDto[] fieldTypes)
		{
			FieldName = fieldName;
			Description = description;
			IsKeyField = isKeyField;
			IsSortable = isSortable;
			UIHidden = uiHidden;
			FieldTypes = fieldTypes;
		}
		public string FieldName { get; }
		public string Description { get; }
		public bool IsKeyField { get; }
		public bool IsSortable { get; }
		public bool UIHidden { get; }
		public FieldTypeDto[] FieldTypes { get; }

		public FieldTypeDto FieldType => FieldTypes?.FirstOrDefault();

		public Type DataType
		{
			get
			{
				var fieldType = FieldType?.DataType;
				if (string.IsNullOrEmpty(fieldType))
				{
					return null;
				}
				var dataType = Type.GetType(fieldType);
				return dataType;
			}
		}

		public bool IsUtcTime => FieldType?.DateTimeType?.Contains("UTC", StringComparison.OrdinalIgnoreCase) ?? false;

		public int Scale => FieldType?.Scale ?? 0;
	}
}
