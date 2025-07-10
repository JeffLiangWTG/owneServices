using System;
using System.Collections;
using System.Reflection;
using System.Xml.Schema;
using CargoWise.Common.Collections;

namespace Enterprise.DataTransfer.Xml
{
	public class ExpectXmlSchemaContainsRootElementAttribute : Attribute
	{
		public ExpectXmlSchemaContainsRootElementAttribute(string expectedElementName)
		{
			this.ExpectedElementName = expectedElementName;
		}

		public readonly string ExpectedElementName;
	}

	public abstract class XmlSchemaDefinitionsBase
	{
		public const string EdiXmlNamespace = @"http://www.edi.com.au/EnterpriseService/";

		internal int SchemaCacheCount
		{
			get { return fCachedSchemas.Count; }
		}

		public XmlSchema[] AllSchemas
		{
			get
			{
				ArrayList result = new ArrayList();
				foreach (PropertyInfo property in GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					if (property.PropertyType == typeof(XmlSchema))
					{
						result.Add(property.GetValue(this, null));
					}
				}
				return (XmlSchema[])result.ToArray(typeof(XmlSchema));
			}
		}

		public string[] GetAllXsdResourceNames()
		{
			LazyLoadAllSchemaPropertiesNow();
			return (string[])fKnownSchemaResourceNames.ToArray(typeof(string));
		}

		protected XmlSchema GetCompiledSchema(string baseResourceName, string relativeResourceName)
		{
			string resourceName = baseResourceName + "." + relativeResourceName;
			XmlSchema result = (XmlSchema)fCachedSchemas[resourceName];
			if (result == null)
			{
				result = new XmlSchemaResourceLoader().ReadSchema(GetType().Assembly, baseResourceName, relativeResourceName);
				fBuilder.CompileSchema(result);
				if (result.IsCompiled)
				{
					fCachedSchemas[resourceName] = result;
					fCachedSchemaKeys[result] = resourceName;
					fKnownSchemaResourceNames.Add(resourceName);
				}
			}
			return result;
		}

		protected XmlSchema GetCompiledSchemaWithElementOfType(XmlSchema schemaWithFragment, string schemaTypeName, string elementName)
		{
			string key = fCachedSchemaKeys[schemaWithFragment] + "_fragment_" + schemaTypeName + "_" + elementName;
			XmlSchema result = (XmlSchema)fCachedSchemas[key];
			if (result == null)
			{
				result = fBuilder.GetSchemaWithElementOfType(schemaWithFragment, schemaTypeName, elementName);
				result = fBuilder.CloneSchema(result);
				fBuilder.CompileSchema(result);
				if (result.IsCompiled)
				{
					fCachedSchemas[key] = result;
					fCachedSchemaKeys[result] = key;
					RemoveSchemaFromCache(schemaWithFragment);
				}
			}
			return result;
		}

		protected XmlSchema GetCompiledSchemaNestedElement(XmlSchema outerSchema, string outerElementName)
		{
			string key = fCachedSchemaKeys[outerSchema] + "_compilednestedelement_" + outerElementName;
			XmlSchema result = (XmlSchema)fCachedSchemas[key];
			if (result == null)
			{
				result = fBuilder.GetSchemaOfNestedElement(outerSchema, outerElementName);
				fBuilder.CompileSchema(result);
				if (result.IsCompiled)
				{
					fCachedSchemas[key] = result;
					fCachedSchemaKeys[result] = key;
					RemoveSchemaFromCache(outerSchema);
				}
			}
			return result;
		}

		#region Implementation

		readonly XsdSchemaBuilder fBuilder = new XsdSchemaBuilder();
		readonly Hashtable fCachedSchemas = new Hashtable();
		readonly WeakReferencedKeyDictionary<XmlSchema, string> fCachedSchemaKeys = new WeakReferencedKeyDictionary<XmlSchema, string>();
		readonly ArrayList fKnownSchemaResourceNames = new ArrayList();

		// Remove a schema from the cache. Usually this is done when a schema is cloned then modified from another schema. This
		// sometimes causes problems compiling and validating against the schema, for some unknown reason. Looks like a .net bug.
		void RemoveSchemaFromCache(XmlSchema schema)
		{
			foreach (DictionaryEntry entry in fCachedSchemas)
			{
				if (entry.Value == schema)
				{
					fCachedSchemas.Remove(entry.Key);
					break;
				}
			}
		}

		void LazyLoadAllSchemaPropertiesNow()
		{
			XmlSchema[] properties = this.AllSchemas;
		}

		#endregion
	}
}
