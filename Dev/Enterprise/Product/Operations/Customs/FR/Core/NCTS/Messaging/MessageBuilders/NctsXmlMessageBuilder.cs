using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.TCL;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public abstract class NctsXmlMessageBuilder<T, F, M>
		where T : IDeclaration
		where F : NctsMessageFunctionSet
		where M : INctsXmlMessage, new()
	{
		protected NctsXmlMessageBuilder(T wrapper, F messageFunction, ErrorCollector errorCollector)
		{
			this.wrapper = Argument.NotNull(wrapper, nameof(wrapper));
			this.messageFunction = Argument.NotNull(messageFunction, nameof(messageFunction));
			this.errorCollector = Argument.NotNull(errorCollector, nameof(errorCollector));
		}

		protected readonly ErrorCollector errorCollector;
		protected readonly T wrapper;
		protected readonly F messageFunction;

		public const string MessageID = "#ID_MESSAGE#";

		protected M GenerateMessage()
		{
			var message = new M();
			ValidateMessage();
			PopulateMessageHeader(message);
			PopulateMessageBody(message);
			return message;
		}

		public ZString GetXMLMessage()
		{
			var message = GenerateMessage();
			return message == null ? string.Empty : System.Text.Encoding.UTF8.GetString(System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(Extensions.Serialize(message)));
		}
		public ZString GetXMLMessageWithoutNamespaces()
		{
			var message = GetXMLMessage();
			return RemoveAllNamespaces(message);
		}

		static ZString RemoveAllNamespaces(ZString xmlDocument)
		{
			var element = XElement.Parse(xmlDocument);
			if (element != null)
			{
				var xmlDocumentWithoutNs = RemoveAllNamespaces(element);
				return xmlDocumentWithoutNs.ToString();
			}
			return xmlDocument;
		}

		//Recursive function
		static XElement RemoveAllNamespaces(XElement e)
		{
			return new XElement(e.Name.LocalName,
			  (from n in e.Nodes()
			   let e1 = n as XElement
			   select (e1 != null ? RemoveAllNamespaces(e1) : n)),
				  (e.HasAttributes) ?
					(from a in e.Attributes()
					 where (!a.IsNamespaceDeclaration)
					 select new XAttribute(a.Name.LocalName, a.Value)) : null);
		}

		// This method is used to set fields of type such as SyntaxIdentifier and SyntaxVersionNumber as each message class has its own enum type
		// 0 reflects the first (and often only) item in the list.
		protected void SetFlagEnumPropertyValue(object obj, ZString enumProperty, int indexOfItemWithinEnum)
		{
			var propertyInfo = typeof(M).GetProperty(enumProperty);
			propertyInfo?.SetValue(obj, indexOfItemWithinEnum, null);
		}

		protected void SetNullableFlagEnumPropertyValue(object obj, ZString enumProperty, Flag flag)
		{
			var propertyInfo = typeof(M).GetProperty(enumProperty);
			if (propertyInfo != null)
			{
				propertyInfo.SetValue(obj, Convert.ChangeType(flag, Nullable.GetUnderlyingType(propertyInfo.PropertyType)), null);
			}
		}

		protected virtual void PopulateMessageHeader(M message)
		{
			var dateTimeNow = ZDateTime.Now;
			SetFlagEnumPropertyValue(message, "SynIdeMES1", 0);
			SetFlagEnumPropertyValue(message, "SynVerNumMES2", 0);
			SetNullableFlagEnumPropertyValue(message, "TesIndMes18", Flag.Item0);
			message.MesSenMes3 = "OPE.FR";
			message.SenIdeCodQuaMes4 = null;
			message.MesRecMes6 = "NTA.FR";
			message.RecIdeCodQuaMes7 = null;
			message.DatOfPreMes9 = dateTimeNow.ToString("yyMMdd");
			message.TimOfPreMes10 = dateTimeNow.ToString("HHmm");
			message.RecRefMes12 = null;
			message.RecRefQuaMes13 = null;
			message.AppRefMes14 = null;
			message.PriMes15 = null;
			message.AckReqMes16ValueSpecified = false;
			message.ComAgrIdMes17 = null;
			message.TesIndMes18ValueSpecified = true;
			message.MesIdeMes19 = MessageID;
			message.ComAccRefMes21 = null;
			message.MesSeqNumMes22 = null;
			message.FirAndLasTraMes23ValueSpecified = false;
			message.IntConRefMes11 = MessageID;
		}

		protected virtual void PopulateMessageBody(M message)
		{
		}

		protected virtual void ValidateMessage()
		{
		}

		#region Enum Checkers and Simple Enum Conversions

		protected static TEnum MapValueToEnum<TEnum>(string input, TEnum defaultValue) where TEnum : struct
		{
			if (Enum.TryParse(input, out TEnum result) && Enum.IsDefined(typeof(TEnum), result))
			{
				return result;
			}
			return defaultValue;
		}

		protected static TEnum MapValueToEnumWithPrefix<TEnum>(string input, TEnum defaultValue) where TEnum : struct
		{
			if (Enum.TryParse((NoResString)"Item" + input, out TEnum result) && Enum.IsDefined(typeof(TEnum), result))
			{
				return result;
			}
			return defaultValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004")]
		protected static bool IsEnumValueValid<TEnum>(string input) where TEnum : struct
		{
			return Enum.TryParse(input, out TEnum result) && Enum.IsDefined(typeof(TEnum), result);
		}

		#endregion Enum Checkers and Simple Enum Conversions

		protected static (bool shouldWriteReferenceToRef1, bool shouldWriteReferenceToOther, bool shouldWriteLiability, bool shouldWriteExtraInfo) GetGuaranteeMapOptions(ZString guaranteeType)
		{
			switch (guaranteeType)
			{
				case "0":
				case "1":
				case "2":
				case "4":
				case "9":
					return (true, false, true, true);
				case "3":
					return (false, true, false, false);
				case "B":
					return (false, true, true, false);
				default:
					return (false, false, false, false);
			}
		}
	}
}
