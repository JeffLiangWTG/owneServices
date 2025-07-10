using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IL.Business
{
	public static class Extensions
	{
		public static T Deserialize<T>(ZString serializedObj)
		{
			using (var reader = new StringReader(serializedObj))
			{
				var serializer = new XmlSerializer(typeof(T));
				return (T)serializer.Deserialize(reader);
			}
		}

		public static T Deserialize<T>(ZString serializedObj, string defaultXmlNamespace)
		{
			using (var reader = new StringReader(serializedObj))
			{
				var serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(defaultXmlNamespace));
				return (T)serializer.Deserialize(reader);
			}
		}

		public static bool HasDefaultNamespace<TMessage>(ZString messageText)
		{
			var messageType = typeof(TMessage);
			var namespaceAttribute = (XmlRootAttribute)Attribute.GetCustomAttribute(messageType, typeof(XmlRootAttribute));

			var doc = XDocument.Parse(messageText);
			var root = doc.Root;
			var hasDefaultNamespace = root.Attributes().Any(a => a.IsNamespaceDeclaration && a.Name == "xmlns");
			var hasCustomNamespacePrefix = namespaceAttribute?.Namespace != null && root.Attributes().Any(a => a.IsNamespaceDeclaration && a.Value == namespaceAttribute?.Namespace);

			return hasDefaultNamespace || hasCustomNamespacePrefix;
		}

		public static string ToCustomsDateTimeString(this ZDateTime dateTime) => dateTime.IsValid ? dateTime.ToString(CustomsDateFormat, CultureInfo.InvariantCulture) : string.Empty;

		public static string GetCodeDescriptionFromRefCusCodeListCombinedCode(this BusinessObjectFactory factory, string codeType, string code)
		{
			var refCusCodeList = GetCachedLoadTop1ByCountryAndAttributes(factory, codeType, code);
			return refCusCodeList != null
				? $"{code} - {refCusCodeList.ZZD_Description}"
				: !code.IsNullOrEmpty() ? $"{code} -" : ZString.Empty;
		}

		public static string GetDescriptionFromRefCusCodeListCombinedCode(this BusinessObjectFactory factory, string codeType, string code)
		{
			var refCusCodeList = GetCachedLoadTop1ByCountryAndAttributes(factory, codeType, code);
			return refCusCodeList?.ZZD_Description;
		}

		public static ZZRefCusCodeListCombined GetCachedLoadTop1ByCountryAndAttributes(this BusinessObjectFactory factory, string codeType, string code)
		{
			return factory.GetCachedValue($"Enterprise.Customs.IL.Business-Extensions-GetCachedLoadTop1ByCountryAndAttributes-{codeType}-{code}", delegate
			{
				if (!codeType.IsEmpty() && !code.IsEmpty())
				{
					return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(factory, code, Core.Constants.CountryCodes.Israel, codeType, ZDateTime.Today);
				}
				return null;
			});
		}

		public static string GetCustomsFeedbackMessageTagName(ZString bodyText)
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(bodyText);
			return xmlDoc.DocumentElement.LocalName;
		}

		public static XDocument TryParseXML(ZString content, bool throwExceptionIfOccurs = false)
		{
			try
			{
				return XDocument.Parse(content);
			}
			catch (XmlException)
			{
				if (throwExceptionIfOccurs)
				{
					throw;
				}
				return null;
			}
		}

		public static string GetRefUnloco(this BusinessObjectFactory factory, string code)
		{
			var refUnloco = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, code);
			return refUnloco != null
				? $"{code} - {refUnloco.Description}"
				: !code.IsNullOrEmpty() ? $"{code} -" : ZString.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string CustomsDateFormat = "yyyy-MM-ddTHH:mm:ss";
	}
}
