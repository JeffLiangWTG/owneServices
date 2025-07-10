using System;
using System.Xml.Schema;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Client.WFN
{
	public class WFNSchemaDefinition : XmlSchemaDefinitionsBase
	{
		protected WFNSchemaDefinition()
		{
		}

		public static WFNSchemaDefinition Instance
		{
			get
			{
				WFNSchemaDefinition result = (WFNSchemaDefinition)WeakReference.Target;
				if (result == null)
				{
					result = new WFNSchemaDefinition();
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

		[ExpectXmlSchemaContainsRootElement(null)]
		public XmlSchema WFNSchema
		{
			get { return GetCompiledSchema("Enterprise.Client.WFN.Definition", "WFNManifest.xsd"); }
		}
	}
}
