using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.Common;

namespace Enterprise.DataTransfer.Native.Business.Xsd.Type
{
	public class XsString : XsdDataType, INamedXsdDataType
	{
		public XsString(int maxLength)
		{
			this.maxLength = maxLength;
		}

		public override string Name
		{
			get { return "xs:string"; }
		}

		public override XObject ToXsdElement()
		{
			var simpleTypeElement = new XElement(xs + Tag.SimpleType);
			AddLengthRestriction(simpleTypeElement);
			return simpleTypeElement;
		}

		void AddLengthRestriction(XElement simpleTypeElement)
		{
			var restrictionElement = new XElement(xs + Tag.Restriction);
			simpleTypeElement.Add(restrictionElement);

			restrictionElement.Add(new XAttribute(Tag.Base, Name));

			if (maxLength > 0)
			{
				var maxLengthElement = new XElement(xs + Tag.MaxLength);
				restrictionElement.Add(maxLengthElement);
				maxLengthElement.Add(new XAttribute(Tag.ColumnValue, maxLength.ToString()));
			}
		}

		public (XObject ReferenceObject, XElement NamedObject) ToXsdElementWithAttributes(string referenceName, IEnumerable<XObject> mandatoryAttributes)
		{
			var reference = CreateReference(referenceName, mandatoryAttributes);
			var namedObject = CreateNamedType(referenceName);

			return (reference, namedObject);
		}

		XElement CreateNamedType(string referenceName)
		{
			var simpleType = new XElement(xs + Tag.SimpleType);
			simpleType.Add(new XAttribute(Tag.AttributeName, referenceName));

			AddLengthRestriction(simpleType);
			return simpleType;
		}

		XObject CreateReference(string referenceName, IEnumerable<XObject> mandatoryAttributes)
		{
			var complexType = new XElement(xs + Tag.ComplexType);
			var simpleContent = new XElement(xs + Tag.SimpleContent);
			var extension = new XElement(xs + Tag.Extension);
			extension.Add(new XAttribute(Tag.Base, referenceName));
			simpleContent.Add(extension);
			complexType.Add(simpleContent);

			mandatoryAttributes.ForEach(x => extension.Add(x));

			return complexType;
		}

		readonly int maxLength;
	}
}
