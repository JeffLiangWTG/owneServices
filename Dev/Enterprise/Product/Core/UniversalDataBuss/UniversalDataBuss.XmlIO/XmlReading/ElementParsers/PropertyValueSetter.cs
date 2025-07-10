using System;
using System.Reflection;
using CargoWise.Common;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlReading.ElementReaders
{
	class PropertyValueSetter
	{
		internal PropertyValueSetter(XmlReader reader)
		{
			this.reader = Argument.NotNull(reader, "reader");
		}
		readonly XmlReader reader;

		internal object SetValueOnTarget(string content, object targetDataObject, PropertyInfo propertyInfo, bool sourceIsAttribute, int lineNumber)
		{
			var targetType = propertyInfo.PropertyType.GetGenericArguments()[0];
			foreach (var parser in reader.ValueParsers)
			{
				if (parser.HandlesType(targetType))
				{
					return parser.Process(propertyInfo, targetDataObject, targetType, content, lineNumber, string.Empty, sourceIsAttribute);
				}
			}

			throw new InvalidOperationException("Unhandled property type: " + targetType.FullName);
		}
	}
}
