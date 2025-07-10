using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Types;

namespace Enterprise.Messaging.Business.AWB
{
	public class ElementList
	{
		public int Count
		{
			get { return Elements.Count; }
		}

		#region ToString

		public override string ToString()
		{
			var elementCount = Count;
			var result = new StringBuilder(elementCount);

			if (elementCount > 0)
			{
				var columnElements = new List<ValueElement>();
				var valueElements = new List<ValueElement>();
				var lastElement = Elements[elementCount - 1];
				ValueElement currentColumnElement = null;

				foreach (var element in Elements)
				{
					var vElement = element as ValueElement;
					var isLastElementOrIsHeader = element == lastElement || vElement == null;
					bool isValueType = vElement?.IsValueType ?? false;

					if (isValueType)
					{
						if (vElement.IsMandatory || vElement.ToStringValueTypes().Length > 0)
						{
							valueElements.Add(vElement);
						}

						if (!isLastElementOrIsHeader)
						{
							continue;
						}
					}

					var hasPendingValues = valueElements.Any();
					if (currentColumnElement != null)
					{
						if (currentColumnElement.IsConditional)
						{
							columnElements.Add(currentColumnElement);
						}
						else
						{
							columnElements.Clear();

							if (currentColumnElement.IsMandatory || (currentColumnElement.IsOptional && hasPendingValues))
							{
								columnElements.Add(currentColumnElement);
							}
						}
					}

					if (hasPendingValues || (currentColumnElement?.IsMandatory ?? false))
					{
						foreach (var value in columnElements)
						{
							result.Append(value.ToString());
						}
						columnElements.Clear();

						foreach (var value in valueElements)
						{
							result.Append(value.ToString());
						}
						valueElements.Clear();
					}

					if (!isValueType)
					{
						currentColumnElement = vElement;

						if (isLastElementOrIsHeader)
						{
							result.Append(element.ToString());
						}
					}
				}
			}

			return result.ToString();
		}

		public string ToStringValueTypes()
		{
			var stringBuilder = new StringBuilder(Count);
			foreach (Element element in Elements)
			{
				if (element is ValueElement vElement)
				{
					stringBuilder.Append(vElement.ToStringValueTypes());
				}
				else if (element is HeaderElement hElement)
				{
					stringBuilder.Append(hElement.Elements.ToStringValueTypes());
				}
			}
			return stringBuilder.ToString();
		}

		#endregion

		#region Add

		public void Add(Element element)
		{
			Elements.Add(element);
		}

		public void AddLineIdentifier(ZString lineIdentifier)
		{
			ValueElement element = new ValueElement(StatusType.Mandatory, new Format(3, CharType.Alpha), lineIdentifier, ValueType.LineIdentifier);
			Add(element);
		}

		public void AddColumnIdentifier(ZString columnIdentifier)
		{
			ValueElement element = new ValueElement(StatusType.Mandatory, new Format(columnIdentifier.Length, CharType.Alpha), columnIdentifier, ValueType.ColumnIdentifier);
			Add(element);
		}

		public void AddSlant()
		{
			AddSlant(StatusType.Mandatory);
		}

		public void AddSlant(StatusType status)
		{
			ValueElement element = new ValueElement(status, new Format(1, CharType.Special), SpecialChars.Slant, ValueType.Special);
			Add(element);
		}

		public void AddCRLF()
		{
			ValueElement element = new ValueElement(StatusType.Mandatory, new Format(1, CharType.Special), SpecialChars.CRLF, ValueType.Special);
			Add(element);
		}

		public void AddHyphen()
		{
			ValueElement element = new ValueElement(StatusType.Mandatory, new Format(1, CharType.Special), SpecialChars.Hyphen, ValueType.Special);
			Add(element);
		}

		public void AddHyphen(StatusType status)
		{
			ValueElement element = new ValueElement(status, new Format(1, CharType.Special), SpecialChars.Hyphen, ValueType.Special);
			Add(element);
		}

		public void AddValue(Format format, ZString value)
		{
			ValueElement element = new ValueElement(StatusType.Mandatory, format, value, ValueType.Value);
			Add(element);
		}

		public void AddValue(Format format, ZInt value)
		{
			AddValue(format, value.ToString());
		}

		public void AddValue(Format format, ZDecimal value)
		{
			string valueAsString = value.ToString();
			if (valueAsString.IndexOf('.') > -1)
			{
				valueAsString = valueAsString.TrimEnd(new char[] { '0' });
				valueAsString = valueAsString.TrimEnd(new char[] { '.' });
			}

			AddValue(format, valueAsString);
		}

		public void AddOptionalValue(Format format, ZString value)
		{
			ValueElement element = new ValueElement(StatusType.Optional, format, value, ValueType.Value);
			Add(element);
		}

		public void AddConditionalValue(Format format, ZString value)
		{
			ValueElement element = new ValueElement(StatusType.Conditional, format, value, ValueType.Value);
			Add(element);
		}

		public void AddHeader(ElementList elementList)
		{
			HeaderElement element = new HeaderElement(StatusType.Mandatory, elementList);
			Add(element);
		}

		public void AddOptionalHeader(ElementList elementList)
		{
			HeaderElement element = new HeaderElement(StatusType.Optional, elementList);
			Add(element);
		}

		public void AddConditionalHeader(ElementList elementList)
		{
			HeaderElement element = new HeaderElement(StatusType.Conditional, elementList);
			Add(element);
		}

		public Element this[int index]
		{
			get { return (Element)Elements[index]; }
		}

		#endregion

		#region Elements

		ArrayList Elements
		{
			get
			{
				if (fElements == null)
				{
					fElements = new ArrayList();
				}

				return fElements;
			}
		}

		ArrayList fElements;

		#endregion
	}
}
