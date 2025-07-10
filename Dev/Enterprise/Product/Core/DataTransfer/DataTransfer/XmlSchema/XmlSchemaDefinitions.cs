using System;
using System.Xml.Schema;

namespace Enterprise.DataTransfer.Xml
{
	public class XmlSchemaDefinitions : XmlSchemaDefinitionsBase
	{
		#region Instance

		protected XmlSchemaDefinitions()
		{
		}

		public static XmlSchemaDefinitions Instance
		{
			get
			{
				XmlSchemaDefinitions result = (XmlSchemaDefinitions)WeakInstance.Target;
				if (result == null)
				{
					result = new XmlSchemaDefinitions();
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

		[ExpectXmlSchemaContainsRootElement("Organisation")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public XmlSchema SingleOrganisationSchema
		{
			get { return GetCompiledSchemaWithElementOfType(ElementsSchema, "Organisation", "Organisation"); }
		}

		[ExpectXmlSchemaContainsRootElement("DocumentMessages")]
		public XmlSchema DocumentMessagesSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "StorageDocs.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("DocumentMessage")]
		public XmlSchema SingleDocumentMessageSchema
		{
			get { return GetCompiledSchemaNestedElement(DocumentMessagesSchema, "DocumentMessages"); }
		}

		[ExpectXmlSchemaContainsRootElement("BondDetail")]
		public XmlSchema SingleBondDetailSchema
		{
			get { return GetCompiledSchemaWithElementOfType(ElementsSchema, "BondDetail", "BondDetail"); }
		}

		[ExpectXmlSchemaContainsRootElement("Contact")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public XmlSchema SingleContactSchema
		{
			get { return GetCompiledSchemaWithElementOfType(ElementsSchema, "OrgContact", "Contact"); }
		}

		[ExpectXmlSchemaContainsRootElement("Notes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public XmlSchema NotesSchema
		{
			get { return GetCompiledSchemaWithElementOfType(ElementsSchema, "Notes", "Notes"); }
		}

		[ExpectXmlSchemaContainsRootElement("Note")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public XmlSchema SingleNoteSchema
		{
			get { return GetCompiledSchemaNestedElement(NotesSchema, "Notes"); }
		}

		[ExpectXmlSchemaContainsRootElement("")]
		public XmlSchema ElementsSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "Elements.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("Documents")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public XmlSchema DocumentsSchema
		{
			get { return GetCompiledSchemaWithElementOfType(ElementsSchema, "Documents", "Documents"); }
		}

		[ExpectXmlSchemaContainsRootElement("Document")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public XmlSchema SingleDocumentSchema
		{
			get { return GetCompiledSchemaNestedElement(DocumentsSchema, "Documents"); }
		}

		[ExpectXmlSchemaContainsRootElement("DocumentLinks")]
		public XmlSchema DocumentLinksSchema
		{
			get { return GetCompiledSchemaWithElementOfType(ElementsSchema, "DocumentLinks", "DocumentLinks"); }
		}

		[ExpectXmlSchemaContainsRootElement("DocumentLink")]
		public XmlSchema SingleDocumentLinkSchema
		{
			get { return GetCompiledSchemaNestedElement(DocumentLinksSchema, "DocumentLinks"); }
		}

		[ExpectXmlSchemaContainsRootElement("XmlInterchange")]
		public XmlSchema XmlInterchangeSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "XmlInterchange.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("Events")]
		public XmlSchema EventsSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "Event.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("Event")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public XmlSchema SingleEventSchema
		{
			get { return GetCompiledSchemaNestedElement(EventsSchema, "Events"); }
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

		[ExpectXmlSchemaContainsRootElement("InvoiceLinks")]
		public XmlSchema InvoiceLinksSchema
		{
			get { return GetCompiledSchemaWithElementOfType(ElementsSchema, "InvoiceLinks", "InvoiceLinks"); }
		}

		[ExpectXmlSchemaContainsRootElement("InvoiceLink")]
		public XmlSchema SingleInvoiceLinkSchema
		{
			get { return GetCompiledSchemaNestedElement(InvoiceLinksSchema, "InvoiceLinks"); }
		}

		[ExpectXmlSchemaContainsRootElement("ISFs")]
		public XmlSchema ImporterSecurityFilingsSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "ImporterSecurityFiling.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("ISF")]
		public XmlSchema SingleImporterSecurityFilingSchema
		{
			get { return GetCompiledSchemaNestedElement(ImporterSecurityFilingsSchema, "ISFs"); }
		}

		[ExpectXmlSchemaContainsRootElement("Workflow")] 
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public XmlSchema SingleWorkflowSchema
		{
			get { return GetCompiledSchemaWithElementOfType(ElementsSchema, "Workflow", "Workflow"); }
		}

		[ExpectXmlSchemaContainsRootElement("")]
		public XmlSchema ShipmentsSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "Shipment.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("DocData")]
		public XmlSchema DocDataSchema
		{
			get { return GetCompiledSchemaWithElementOfType(ShipmentsSchema, "", "DocData"); }
		}

		const string DefaultBaseResourceName = "Enterprise.DataTransfer.DataFileDefinitions.Xml.Version1";
	}
}
