using System;
using System.Xml.Schema;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Customs.CustomsWare.Business
{
	public class SchemaDefinitions : XmlSchemaDefinitionsBase
	{
		#region Instance

		protected SchemaDefinitions()
		{
		}

		public static SchemaDefinitions Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new WeakReference(null);
				}
				SchemaDefinitions result = (SchemaDefinitions)fInstance.Target;
				if (result == null)
				{
					result = new SchemaDefinitions();
					fInstance.Target = result;
				}
				return result;
			}
		}

		[CargoWise.Common.Testing.SuppressThreadStaticFieldMessage]
		[ThreadStatic]
		static WeakReference fInstance;

		#endregion

		[ExpectXmlSchemaContainsRootElement("InputDocument")]
		public XmlSchema InputDocumentSchema
		{
			get { return GetCompiledSchema("Enterprise.Customs.CustomsWare.Business.XSD", "CustomsWare API Schema.xsd"); }
		}
	}
}
