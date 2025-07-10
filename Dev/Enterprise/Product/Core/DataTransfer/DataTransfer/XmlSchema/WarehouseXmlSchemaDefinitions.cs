using System;
using System.Xml.Schema;

namespace Enterprise.DataTransfer.Xml
{
	public class WarehouseXmlSchemaDefinitions : XmlSchemaDefinitionsBase
	{
		#region Instance

		protected WarehouseXmlSchemaDefinitions()
		{
		}

		public static WarehouseXmlSchemaDefinitions Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new WeakReference(null);
				}

				WarehouseXmlSchemaDefinitions result = (WarehouseXmlSchemaDefinitions)fInstance.Target;
				if (result == null)
				{
					result = new WarehouseXmlSchemaDefinitions();
					fInstance.Target = result;
				}
				return result;
			}
		}

		[ThreadStatic]
		static WeakReference fInstance;

		#endregion

		[ExpectXmlSchemaContainsRootElement("WhsDockets")]
		public XmlSchema WhsDocketsSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "WhsDocket.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("WhsDocket")]
		public XmlSchema SingleWhsDocketSchema
		{
			get { return GetCompiledSchemaNestedElement(WhsDocketsSchema, "WhsDockets"); }
		}

		[ExpectXmlSchemaContainsRootElement("ConNotes")]
		public XmlSchema WhsDocketsIFSSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "WhsDocketIFS.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("ConNote")]
		public XmlSchema SingleWhsDocketIFSSchema
		{
			get { return GetCompiledSchemaNestedElement(WhsDocketsIFSSchema, "ConNotes"); }
		}

		const string DefaultBaseResourceName = "Enterprise.DataTransfer.DataFileDefinitions.Xml.Version1";
	}
}
