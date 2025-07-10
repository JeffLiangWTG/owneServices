using System;
using System.Xml.Schema;

namespace Enterprise.DataTransfer.Xml
{
	public class CustomsXmlSchemaDefinitions : XmlSchemaDefinitionsBase
	{
		#region Instance

		protected CustomsXmlSchemaDefinitions()
		{
		}

		public static CustomsXmlSchemaDefinitions Instance
		{
			get
			{
				CustomsXmlSchemaDefinitions result = (CustomsXmlSchemaDefinitions)WeakInstance.Target;
				if (result == null)
				{
					result = new CustomsXmlSchemaDefinitions();
					WeakInstance.Target = result;
				}
				return result;
			}
		}

		static WeakReference WeakInstance
		{
			get { return weakInstance ?? (weakInstance = new WeakReference(null)); }
		}
		[ThreadStatic] static WeakReference weakInstance;

		#endregion

		[ExpectXmlSchemaContainsRootElement("Invoices")]
		public XmlSchema InvoicesSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "Invoice.xsd"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		[ExpectXmlSchemaContainsRootElement("InvoiceHeader")]
		public XmlSchema SingleInvoiceSchema
		{
			get { return GetCompiledSchemaNestedElement(FreightXmlSchemaDefinitions.Instance.ShipmentsSchema, "Invoices"); }
		}

		[ExpectXmlSchemaContainsRootElement("CusImportManifests")]
		public XmlSchema CusImportManifestsSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "CusImportManifest.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("CusImportManifest")]
		public XmlSchema SingleCusImportManifestSchema
		{
			get { return GetCompiledSchemaNestedElement(CusImportManifestsSchema, "CusImportManifests"); }
		}

		[ExpectXmlSchemaContainsRootElement("RootSendung")]
		public XmlSchema AdvantageCustomsSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "AdvantageCustoms.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("RootZollstatus")]
		public XmlSchema AdvantageCustomsResponseStatusSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "AdvantageCustomsResponseStatus.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("Products")]
		public XmlSchema ProductsSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "Product.xsd"); }
		}

		const string DefaultBaseResourceName = "Enterprise.DataTransfer.DataFileDefinitions.Xml.Version1";
	}
}
