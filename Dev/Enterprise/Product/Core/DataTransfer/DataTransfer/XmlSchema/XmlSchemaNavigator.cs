using System.Collections;
using System.Xml;
using System.Xml.Schema;

namespace Enterprise.DataTransfer.Xml
{
	public class XmlSchemaNavigator
	{
		public XmlSchemaNavigator(XmlSchema schema)
		{
			this.Schema = schema;
		}

		public readonly XmlSchema Schema;

		public XmlSchemaObject Current
		{
			get { return (CurrentPath.Count == 0 ? Schema : CurrentPath.Peek()) as XmlSchemaObject; }
		}

		public XmlSchemaElement CurrentElement
		{
			get { return Current as XmlSchemaElement; }
		}

		public bool CurrentElementHasAttribute(string attributeName)
		{
			XmlSchemaElement currentElement = this.CurrentElement;
			XmlSchemaComplexType elementType = (currentElement == null) ? null : currentElement.ElementSchemaType as XmlSchemaComplexType;
			return elementType != null && elementType.AttributeUses.Contains(new XmlQualifiedName(attributeName));
		}

		public IEnumerable ChildSchemaObjects
		{
			get
			{
				return GetChildSchemaObjects(Current) ?? (System.Array.Empty<XmlSchemaObject>());
			}
		}

		public bool NavigateToElement(string nextElementName)
		{
			return NavigateToElement(nextElementName, false);
		}

		public bool NavigateToElement(string nextElementName, bool isEmptyElement)
		{
			if (WasEmptyElement)
			{
				NavigateBack();
			}
			WasEmptyElement = isEmptyElement;

			bool foundElement = NavigateToElement(Current, nextElementName);
			if (!foundElement)
			{
				CurrentPath.Push(null);
			}
			return foundElement;
		}

		public void NavigateBack()
		{
			NavigateBack(false);
		}

		public void NavigateBack(bool navigateBackAgainIfWasEmptyElement)
		{
			if (navigateBackAgainIfWasEmptyElement && WasEmptyElement)
			{
				CurrentPath.Pop();
			}
			WasEmptyElement = false;
			CurrentPath.Pop();
		}

		public int Depth
		{
			get { return CurrentPath.Count; }
		}

		#region Implementation

		bool WasEmptyElement;
		readonly Stack CurrentPath = new Stack();

		bool NavigateToElement(XmlSchemaObject schemaObject, string elementName)
		{
			bool result = false;
			IEnumerable nextChildSchemaObjects = GetChildSchemaObjects(schemaObject);
			if (nextChildSchemaObjects != null)
			{
				result = NavigateToElement(nextChildSchemaObjects, elementName);
			}
			return result;
		}

		bool NavigateToElement(IEnumerable schemaObjects, string elementName)
		{
			foreach (XmlSchemaObject nextSchemaObject in schemaObjects)
			{
				XmlSchemaElement element = nextSchemaObject as XmlSchemaElement;
				if (nextSchemaObject is XmlSchemaAny || element != null
									&& (element.Name == elementName || element.RefName.Name == elementName))
				{
					CurrentPath.Push(nextSchemaObject);
					return true;
				}

				XmlSchemaChoice choice = nextSchemaObject as XmlSchemaChoice;
				if (choice != null && NavigateToElement(choice.Items, elementName))
				{
					return true;
				}
			}
			return false;
		}

		IEnumerable GetChildSchemaObjects(XmlSchemaObject schemaObject)
		{
			IEnumerable result = null;
			if (schemaObject != null)
			{
				string typeName = schemaObject.GetType().FullName;
				switch (typeName)
				{
					case "System.Xml.Schema.XmlSchema":
						result = (schemaObject as XmlSchema).Items;
						break;
					case "System.Xml.Schema.XmlSchemaComplexType":
						result = GetChildSchemaObjects((schemaObject as XmlSchemaComplexType).ContentTypeParticle);
						break;
					case "System.Xml.Schema.XmlSchemaSequence":
						result = (schemaObject as XmlSchemaSequence).Items;
						break;
					case "System.Xml.Schema.XmlSchemaElement":
						result = GetChildSchemaObjects((schemaObject as XmlSchemaElement).ElementSchemaType as XmlSchemaComplexType);
						break;
					case "System.Xml.Schema.XmlSchemaChoice":
						result = (schemaObject as XmlSchemaChoice).Items;
						break;
					case "System.Xml.Schema.XmlSchemaAll":
						result = (schemaObject as XmlSchemaAll).Items;
						break;
				}
			}
			return result;
		}

		#endregion
	}
}
