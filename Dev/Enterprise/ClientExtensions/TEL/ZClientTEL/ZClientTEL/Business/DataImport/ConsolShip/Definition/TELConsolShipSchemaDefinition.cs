using System;
using System.Xml.Schema;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Client.TEL.Definition
{
	class TELConsolShipSchemaDefinition : XmlSchemaDefinitionsBase
	{
		protected TELConsolShipSchemaDefinition()
		{
		}

		public static TELConsolShipSchemaDefinition Instance
		{
			get
			{
				TELConsolShipSchemaDefinition result = (TELConsolShipSchemaDefinition)WeakReference.Target;
				if (result == null)
				{
					result = new TELConsolShipSchemaDefinition();
					WeakReference.Target = result;
				}
				return result;
			}
		}
		static WeakReference WeakReference
		{
			get { return weakReference ?? (weakReference = new WeakReference(null)); }
		}
		[ThreadStatic]
		static WeakReference weakReference;

		[ExpectXmlSchemaContainsRootElement("SeaMasterBills")]
		public XmlSchema ConsolShipSchema
		{
			get
			{
				return GetCompiledSchema("Enterprise.Client.TEL.Business.DataImport.ConsolShip.Definition", "TELConsolShipDefinition.xsd");
			}
		}
	}
}
