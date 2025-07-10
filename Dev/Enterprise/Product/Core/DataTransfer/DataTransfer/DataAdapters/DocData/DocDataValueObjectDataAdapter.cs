using System.Linq;
using System.Xml.Schema;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.SDF;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class DocDataValueObjectDataAdapter : ValueObjectDataAdapter<DocumentNote, Xsd.ShipmentDocData>
	{
		public override string RootCollectionElementName
		{
			get { return null; }
		}

		public override string RootElementName
		{
			get { return "DocData"; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return null; }
		}

		public override XmlSchema Schema
		{
			get { return XmlSchemaDefinitions.Instance.DocDataSchema; }
		}

		protected override DocumentNote FindBusinessObject(Xsd.ShipmentDocData value, IValueObjectImportContext context)
		{
			return null;
		}

		#region ImportFromValueObjectCore

		public void ImportData(EnterpriseBusinessObject bizObj, Xsd.ShipmentDocData xmlDocData, IValueObjectImportContext context)
		{
			DocumentNote docNote = DocumentNote.LoadNote(bizObj);
			ImportFromValueObjectCore(docNote, xmlDocData, context);
		}

		protected override void ImportFromValueObjectCore(DocumentNote docNote, Xsd.ShipmentDocData xmlDocData, IValueObjectImportContext context)
		{
			foreach (Xsd.SystemDefinedDataType sysDefData in xmlDocData.SystemDefinedData)
			{
				docNote.SetSystemDefinedFieldValue(sysDefData.Name, sysDefData.Value);
			}

			foreach (Xsd.UserDefinedDataType udf in xmlDocData.UserDefinedData)
			{
				docNote.SetFieldValue(udf.Name, udf.Value);
			}
		}

		#endregion

		#region ExportToValueObjectCore

		public void ExportData(EnterpriseBusinessObject bizObj, Xsd.ShipmentDocData xmlDocData, IValueObjectExportContext context)
		{
			DocumentNote docNote = DocumentNote.LoadNote(bizObj);
			ExportToValueObjectCore(docNote, xmlDocData, context);
		}

		protected override void ExportToValueObjectCore(DocumentNote docNote, Xsd.ShipmentDocData xmlDocData, IValueObjectExportContext context)
		{
			foreach (StmSystemDefinedFieldWrapper wrapper in docNote.SystemDefinedFieldWrappers)
			{
				if (!wrapper.S1_Value.IsEmpty)
				{
					Xsd.SystemDefinedDataType sysDefData = xmlDocData.SystemDefinedData.AddNew();
					sysDefData.Category = wrapper.S1_Category;
					sysDefData.Name = wrapper.S1_Name;
					sysDefData.Value = wrapper.S1_Value;
				}
			}
			xmlDocData.SystemDefinedData.IsSpecified = xmlDocData.SystemDefinedData.Count > 0;

			foreach (var field in docNote.UserDefinedFieldList.Cast<FilterField>().OrderBy(x => x.DisplayName))
			{
				if (field.ValueAsObject != null && field.ValueAsObject.ToString().Length > 0)
				{
					Xsd.UserDefinedDataType userData = new Xsd.UserDefinedDataType();
					userData.Name = field.DisplayName;
					userData.Value = field.ValueAsObject.ToString();
					xmlDocData.UserDefinedData.Add(userData);
				}
			}
			xmlDocData.UserDefinedData.IsSpecified = xmlDocData.UserDefinedData.Count > 0;
			xmlDocData.IsSpecified = xmlDocData.SystemDefinedData.IsSpecified || xmlDocData.UserDefinedData.IsSpecified;
		}

		#endregion
	}
}
