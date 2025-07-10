using System;
using System.Diagnostics;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngineCore.DocumentParsing
{
	[DebuggerDisplay("FieldName={FieldName}, FieldDescription={FieldDescription}, FieldTypeAsText={FieldTypeAsText}")]
	public class DocumentFieldDefinition : NonPersistentBusinessObject, IDocumentField, IObsoleteValidation
	{
		public DocumentFieldDefinition(ZString fieldName, ZString fieldDescription, FieldTypes fieldType, Type relatedDocWrapperType = null)
		{
			this.fieldName = fieldName;
			this.fieldDescription = fieldDescription;
			this.fieldType = fieldType;
			this.realtedDocWrapperType = relatedDocWrapperType;
		}

		readonly ZString fieldName;
		readonly ZString fieldDescription;
		readonly FieldTypes fieldType;
		readonly Type realtedDocWrapperType;

		#region Properties

		#region Field Name

		public ZString FieldName
		{
			get { return fieldName; }
		}

		public ZString FieldNameForDisplay
		{
			get
			{
				return fieldName + AdditionalFieldInfo;
			}
		}

		public ZPropertyInfo FieldNameInfo
		{
			get { return GetZPropertyInfo(nameof(FieldName)); }
		}

		public ZString AdditionalFieldInfo { get; set; }

		#endregion

		#region Field Description

		public ZString FieldDescription
		{
			get { return fieldDescription; }
		}

		public ZPropertyInfo FieldDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(FieldDescription)); }
		}

		#endregion

		#region Field Type

		public enum FieldTypes
		{
			Property,
			Method,
			RelatedDocumentWrapper,
			RelatedBusinessObject
		}

		public FieldTypes FieldType
		{
			get { return fieldType; }
		}

		public ZString FieldTypeAsText
		{
			get { return fieldType.ToString(); }
		}

		public ZPropertyInfo FieldTypeAsTextPropertyInfo
		{
			get { return GetZPropertyInfo(nameof(FieldTypeAsText)); }
		}

		#endregion

		#region Related DocWrapper Type

		public Type RelatedDocWrapperType
		{
			get { return realtedDocWrapperType; }
		}

		#endregion

		#endregion
	}
}
