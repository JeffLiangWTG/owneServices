using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.ZArchitecture.Business.Res;

namespace Enterprise.ZArchitecture.DataMapping
{
	public abstract class XmlSerializableSetting : IXmlSerializable
	{
		public string AsXml()
		{
			return AsXml(true);
		}

		public string AsXml(bool omitXmlDeclaration)
		{
			var encoding = new UTF8Encoding(false);

			XmlWriterSettings writerSettings = new XmlWriterSettings();
			writerSettings.Encoding = encoding;
			writerSettings.OmitXmlDeclaration = omitXmlDeclaration;
			writerSettings.Indent = !omitXmlDeclaration;

			using (MemoryStream ms = new MemoryStream())
			{
				using (XmlWriter writer = XmlWriter.Create(ms, writerSettings))
				{
					writer.WriteStartElement(GetType().Name);
					((IXmlSerializable)this).WriteXml(writer);
					writer.WriteEndElement();
				}

				return encoding.GetString(ms.ToArray());
			}
		}

		public static T FromXml<T>(string xml) where T : XmlSerializableSetting
		{
			return (T)FromXml(xml, typeof(T), false);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Xml Serialization Attribute Syntax, incorrect xml content")]
		public static XmlSerializableSetting FromXml(string xml, Type type, bool throwXmlException)
		{
			if (String.IsNullOrEmpty(xml))
			{
				return null;
			}

			try
			{
				using (XmlReader reader = XmlReader.Create(new StringReader(xml)))
				{
					while (reader.Read())
					{
						if (reader.NodeType == XmlNodeType.Whitespace || reader.NodeType == XmlNodeType.XmlDeclaration)
						{
							continue;
						}

						if (reader.Name.Equals(type.Name))
						{
							XmlSerializableSetting result = (XmlSerializableSetting)Activator.CreateInstance(type);
							((IXmlSerializable)result).ReadXml(reader);
							return result;
						}
						else
						{
							break;
						}
					}
				}
			}
			catch (XmlException ex)
			{
				if (throwXmlException)
				{
					throw;
				}
				else
				{
					string key = String.Format("XmlSerializableSetting.FromXml<{0}>", type.Name);
					string message = String.Format("The Unicode-string value of the binary data was : [{0}]", xml);
					CargoWise.Common.ErrorReporter.ReportOnce(key, key + System.Environment.NewLine + message, ex);
				}
			}

			return null;
		}

		#region IXmlSerializable Members

		System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			string thisElemName = reader.Name;
			bool isEmptyElement = reader.IsEmptyElement;
			reader.ReadStartElement(thisElemName);

			if (isEmptyElement)
			{
				return;
			}

			PropertyInfo[] propInfos = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();
			Array.ForEach(propInfos, delegate (PropertyInfo pi)
			{ properties.Add(GetXmlPropertyName(pi), pi); });

			Dictionary<PropertyInfo, List<object>> lists = new Dictionary<PropertyInfo, List<object>>();
			Array.ForEach(propInfos, delegate (PropertyInfo pi)
				{
					if (pi.PropertyType.IsArray || (pi.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(pi.PropertyType)))
					{
						lists.Add(pi, new List<object>());
					}
				});

			while (reader.NodeType != XmlNodeType.EndElement || reader.Name != thisElemName)
			{
				var readerName = reader.Name;
				try
				{
					if (properties.TryGetValue(reader.Name, out PropertyInfo propInfo))
					{
						if (propInfo.PropertyType.IsArray)
						{
							lists[propInfo].Add(ReadElement(reader, propInfo, propInfo.PropertyType.GetElementType()));
						}
						else if (propInfo.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(propInfo.PropertyType))
						{
							lists[propInfo].Add(ReadElement(reader, propInfo, propInfo.PropertyType.GetGenericArguments().Single()));
						}
						else
						{
							propInfo.SetValue(this, ReadElement(reader, propInfo, propInfo.PropertyType), null);
						}
					}
					else
					{
						reader.Read();
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (ex.GetInnermostException() is FormatException)
					{
						Globals.Message.ShowError(Res.GetString("667fdc78-12ec-4882-9cd5-fe834518b1c0", "Invalid Settings File\r\n\r\nProperty for <{1}> is invalid.\r\n\"{0}\"\r\n\r\nThis mapping has been skipped.\r\nIf you have modified the settings file please correct this.", ex.Message, readerName));
					}
					else
					{
						throw;
					}
				}
			}

			reader.ReadEndElement();
			foreach (KeyValuePair<PropertyInfo, List<object>> kv in lists)
			{
				var list = new List<object>();
				Array array = Array.CreateInstance(kv.Key.PropertyType.GetElementType() ?? kv.Key.PropertyType.GetGenericArguments().Single(), kv.Value.Count);
				for (int i = 0; i < kv.Value.Count; i++)
				{
					array.SetValue(kv.Value[i], i);
				}
				kv.Key.SetValue(this, array, null);
			}
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			foreach (PropertyInfo propInfo in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (propInfo.PropertyType.IsArray)
				{
					Array array = (Array)propInfo.GetValue(this, null);
					if (array != null)
					{
						foreach (object elem in array)
						{
							WriteElement(writer, propInfo, elem);
						}
					}
				}
				else if (propInfo.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(propInfo.PropertyType))
				{
					var list = (IList)propInfo.GetValue(this, null);
					if (list != null)
					{
						foreach (var listElement in list)
						{
							WriteElement(writer, propInfo, listElement);
						}
					}
				}
				else
				{
					WriteElement(writer, propInfo, propInfo.GetValue(this, null));
				}
			}
		}

		static protected string GetXmlPropertyName(PropertyInfo propInfo)
		{
			string propertyName = null;
			object[] attributes = propInfo.GetCustomAttributes(typeof(XmlElementAttribute), false);
			if (attributes != null && attributes.Length == 1)
			{
				propertyName = ((XmlElementAttribute)attributes[0]).ElementName;
			}

			return string.IsNullOrEmpty(propertyName) ? propInfo.Name : propertyName;
		}

		object ReadElement(XmlReader reader, PropertyInfo propertyInfo, Type type)
		{
			if (typeof(IXmlSerializable).IsAssignableFrom(type))
			{
				IXmlSerializable result = (IXmlSerializable)Activator.CreateInstance(type);
				result.ReadXml(reader);
				return result;
			}
			else
			{
				string str = reader.ReadElementString();
				if (IsFullDateTime(propertyInfo))
				{
					return SqlFormatInfo.FromSqlDateTime(str);
				}
				else
				{
					TypeConverter converter = TypeDescriptor.GetConverter(type);
					return converter.ConvertFromString(str);
				}
			}
		}

		void WriteElement(XmlWriter writer, PropertyInfo propertyInfo, object value)
		{
			string name = GetXmlPropertyName(propertyInfo);
			IXmlSerializable xmlSerializable = value as IXmlSerializable;
			if (xmlSerializable == null)
			{
				if (value != null)
				{
					if (IsFullDateTime(propertyInfo))
					{
						writer.WriteElementString(name, SqlFormatInfo.ToSqlDateTimeString((DateTime)value));
					}
					else if (propertyInfo.PropertyType == typeof(double))
					{
						writer.WriteElementString(name, ((double)value).ToString(CultureInfo.InvariantCulture));
					}
					else
					{
						writer.WriteElementString(name, value.ToString());
					}
				}
			}
			else
			{
				writer.WriteStartElement(name);
				xmlSerializable.WriteXml(writer);
				writer.WriteEndElement();
			}
		}

		static bool IsFullDateTime(PropertyInfo propInfo)
		{
			if (propInfo.PropertyType != typeof(DateTime))
			{
				return false;
			}

			object[] attributes = propInfo.GetCustomAttributes(typeof(XmlElementAttribute), false);
			if (attributes == null || attributes.Length != 1)
			{
				return false;
			}

			return ((XmlElementAttribute)attributes[0]).DataType.Equals("dateTime", StringComparison.OrdinalIgnoreCase);
		}

		#endregion
	}
}
