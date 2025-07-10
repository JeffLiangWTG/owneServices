using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.ZArchitecture
{
	public abstract class SerializableNoteText : XmlSerializableSetting
	{
		public static bool CanSerialize(StmNote note, Type noteType)
		{
			try
			{
				XmlReader reader = XmlReader.Create(new StringReader(note.GetUnSerializeNoteText()));
				ZXmlSerializer serializer = ZXmlSerializer.New(noteType);
				return serializer.CanDeserialize(reader) || reader.Name == noteType.Name;
			}
			catch (XmlException)
			{
				return false;
			}
		}

		public static T FromXml<T>(StmNote note) where T : SerializableNoteText
		{
			ZString noteText = note.GetUnSerializeNoteText();
			return (T)FromXml(noteText, typeof(T), false);
		}

		public static ZString HumanReadableText(StmNote note, Type noteType)
		{
			var builder = new ZStringBuilder();

			var unSerializedNote = note.GetUnSerializeNoteText();

			try
			{
				if (CanSerialize(note, noteType))
				{
					var xDoc = XDocument.Parse(unSerializedNote);

					var propInfos = noteType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
					var arrayPropInfo = Array.Find(propInfos, obj => obj.PropertyType.IsArray) ?? Array.Find(propInfos,obj => obj.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(obj.PropertyType));
					var noteElementType = arrayPropInfo.PropertyType.GetElementType() ?? arrayPropInfo.PropertyType.GetGenericArguments().Single();

					var b = xDoc.Descendants(noteElementType.Name);

					var properties = new Dictionary<string, PropertyInfo>();
					Array.ForEach(propInfos, delegate (PropertyInfo pi)
					{ properties.Add(GetXmlPropertyName(pi), pi); });

					foreach (var current in xDoc.Descendants(noteElementType.Name))
					{
						if (properties.TryGetValue(current.Name.ToString(), out var propInfo))
						{
							var elementType = propInfo.PropertyType.GetElementType() ?? propInfo.PropertyType.GetGenericArguments().Single();

							var elementPropInfos = elementType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
							var elementProperties = new Dictionary<string, PropertyInfo>();
							Array.ForEach(elementPropInfos, delegate (PropertyInfo pi)
							{ elementProperties.Add(GetXmlPropertyName(pi), pi); });

							foreach (var inner in current.Elements())
							{
								PropertyInfo elementProInfo;
								if (elementProperties.TryGetValue(inner.Name.ToString(), out elementProInfo))
								{
									GetElementDisplayName(elementProInfo, inner.Value, builder);
								}
							}

							if (!builder.IsEmpty)
							{
								builder.Append(" ");
							}
						}
					}
				}
			}
			catch (XmlException) { }

			return (builder.IsEmpty) ? unSerializedNote.ToString() : builder.ToStringWithNewLineBetweenAppends();
		}

		protected static void GetElementDisplayName(PropertyInfo propInfo, string elementValue, ZStringBuilder builder)
		{
			string propertyName;
			object[] attributes = propInfo.GetCustomAttributes(typeof(SerializableNoteElementAttribute), false);
			if (attributes != null && attributes.Length == 1)
			{
				SerializableNoteElementAttribute attribute = (SerializableNoteElementAttribute)attributes[0];

				if (!attribute.HiddenElement)
				{
					builder.Append(attribute.DisplayName + ": " + elementValue);
				}
			}
			else
			{
				propertyName = propInfo.Name;
			}
		}
	}
}
