using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;
using Enterprise.Customs.IT.Messaging.MessageStructure;

namespace Enterprise.Customs.IT.MessageBuilders;

public class FlatFileMessageSerializer
{
	public FlatFileMessageSerializer(ZString delimiter)
	{
		this.delimiter = delimiter;
	}
	readonly ZString delimiter;

	public ZString Serialize(ISadCustomsMessage message)
	{
		message = Argument.NotNull(message, "message");
		return SerializeInternal(message);
	}

	#region Implementation

	ZString SerializeInternal(object value, PropertyInfo pInfo = null, bool parentIsFixedLenght = false)
	{
		ZString result = new();
		if (value != null)
		{
			var type = value.GetType();

			if (IsIZTypeAssignable(type))
			{
				result = SerializeField(pInfo, value, parentIsFixedLenght);
			}
			else if (value is IEnumerable list)
			{
				result = SerializeArrayProperty(list, pInfo, parentIsFixedLenght);
			}
			else
			{
				result = SerializeComplexProperty(value, type);
			}
		}
		else
		{
			if (pInfo.GetCustomAttribute<IgnoreSerializationIfNullAttribute>() == null)
			{
				result = delimiter;
			}
		}

		return result;
	}

	ZString SerializeArrayProperty(IEnumerable list, PropertyInfo pInfo, bool parentIsFixedLenght)
	{
		var stringBuilder = new ZStringBuilder();
		foreach (var item in list)
		{
			var itemType = item.GetType();

			ZString result;
			if (IsIZTypeAssignable(itemType))
			{
				result = SerializeField(pInfo, item, parentIsFixedLenght);
			}
			else
			{
				result = SerializeComplexProperty(item, itemType);
			}
			stringBuilder.Append(result);
		}
		return stringBuilder.ToString();
	}

	bool IsIZTypeAssignable(Type type) => typeof(IZType).IsAssignableFrom(type);

	ZString SerializeComplexProperty(object value, Type type)
	{
		var stringBuilder = new ZStringBuilder();
		bool isFixedLenght = Attribute.IsDefined(type, typeof(MessageFixedLengthAttribute));
		var allProperties = GetMessageLayoutPropertiesOrdered(type);
		foreach (var prop in allProperties)
		{
			stringBuilder.Append(SerializeInternal(prop.GetValue(value), prop, isFixedLenght));
		}
		if (typeof(IMessageHeader).IsAssignableFrom(type) || typeof(IMessageContinuation).IsAssignableFrom(type))
		{
			stringBuilder.Append("\r\n");
		}
		return stringBuilder.ToString();
	}

	IEnumerable<PropertyInfo> GetMessageLayoutPropertiesOrdered(Type type)
	{
		var allProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(prop => Attribute.IsDefined(prop, typeof(MessageLayoutAttribute)));
		var orderedPropsDict = new SortedDictionary<ZString, PropertyInfo>();
		foreach (var pInfo in allProperties)
		{
			var messageLayoutAttribute = pInfo.GetCustomAttribute<MessageLayoutAttribute>();
			var key = $"{messageLayoutAttribute.Order.ToString(CultureInfo.InvariantCulture).PadLeft(9, '0')}_{messageLayoutAttribute.Position.ToString(CultureInfo.InvariantCulture).PadLeft(9, '0')}";
			orderedPropsDict.Add(key, pInfo);
		}

		return orderedPropsDict.Values;
	}

	ZString SerializeField(PropertyInfo property, object value, ZBool isFixedLenght)
	{
		if (!(value is IZType zTypeValue))
		{
			throw new InvalidCastException("Value must be IZType");
		}

		var fieldRepresentation = property.GetCustomAttribute<MessageFieldRepresentationAttribute>().SerializeValue(zTypeValue);

		if (!isFixedLenght)
		{
			fieldRepresentation += delimiter;
		}

		return fieldRepresentation;
	}

	#endregion
}
