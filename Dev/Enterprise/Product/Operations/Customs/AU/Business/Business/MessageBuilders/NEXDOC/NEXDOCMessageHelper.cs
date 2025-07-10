using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class NEXDOCMessageHelper
	{
		public static ZString Serialize<T>(this T dataObj)
		{
			var settings = new XmlWriterSettings()
			{
				OmitXmlDeclaration = true,
				Indent = true
			};

			using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
			using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
			{
				int namespaceNum = 1;
				var namespaces = new XmlSerializerNamespaces();
				AddXmlTypeNamespaces(namespaces, typeof(T), ref namespaceNum);

				ZXmlSerializer.New(typeof(T)).Serialize(xmlWriter, dataObj, namespaces);
				return stringWriter.ToString();
			}
		}

		static void AddXmlTypeNamespaces(XmlSerializerNamespaces namespaces, Type owner, ref int namespaceNum)
		{
			var attribs = owner.GetCustomAttributes(typeof(XmlTypeAttribute), false);
			if (attribs.Any())
			{
				foreach (XmlTypeAttribute attrib in attribs)
				{
					namespaces.Add(ZString.Format("NST{0}", namespaceNum++), attrib.Namespace);
				}

				foreach (var field in owner.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					AddXmlTypeNamespaces(namespaces, field.FieldType, ref namespaceNum);
				}
			}
		}

		public static T TryDeserialize<T>(ZString serializedObj) where T : class
		{
			return TryDeserialize<T>(serializedObj, typeof(T).Name);
		}

		public static T TryDeserialize<T>(ZString serializedObj, ZString rootNodeName) where T : class
		{
			try
			{
				if (rootNodeName.IsEmpty || serializedObj.IndexOf(rootNodeName, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					using (var textReader = new StringReader(serializedObj))
					{
						return (T)ZXmlSerializer.New(typeof(T))?.Deserialize(textReader);
					}
				}
			}
			catch (InvalidOperationException invalidOperationException)
			{
				var errorMessage = "Corrupted or Malformed Response Message. Cannot Process." + System.Environment.NewLine +
					invalidOperationException.InnerException.Message;
				throw new InvalidFormatException(errorMessage);
			}

			return null;
		}
	}
}
