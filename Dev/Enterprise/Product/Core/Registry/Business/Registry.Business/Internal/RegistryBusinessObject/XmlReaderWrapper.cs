using System;
using System.Xml;
using CargoWise.Types;

namespace Enterprise.Registry.Business
{
	public class XmlReaderWrapper
	{
		public XmlReaderWrapper(XmlReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("Reader");
			}
			this.Reader = reader;
		}

		[System.Diagnostics.DebuggerStepThrough]
		public static implicit operator XmlReaderWrapper(XmlReader value)
		{
			return new XmlReaderWrapper(value);
		}

		[System.Diagnostics.DebuggerStepThrough]
		public static implicit operator XmlReader(XmlReaderWrapper value)
		{
			return value.Reader;
		}

		public ZBool ReadElementStringAsZBool(string name)
		{
			string value = ReadElementString(name);
			return string.IsNullOrEmpty(value) ? ZBool.False : new ZBool(value);
		}

		public ZDecimal ReadElementStringAsZDecimal(string name)
		{
			string value = ReadElementString(name);
			ZDecimal result;
			return ZDecimal.TryParse(value, out result) ? result : ZDecimal.Zero;
		}

		public ZInt ReadElementStringAsZInt(string name)
		{
			string value = ReadElementString(name);
			ZInt result;
			return ZInt.TryParse(value, out result) ? result : ZInt.Zero;
		}

		public ZShort ReadElementStringAsZShort(string name)
		{
			string value = ReadElementString(name);
			ZShort result;
			return ZShort.TryParse(value, out result) ? result : ZShort.Zero;
		}

		public ZDateTime ReadElementStringAsZDateTime(string name, string format)
		{
			string value = ReadElementString(name);
			ZDateTime result;
			return ZDateTime.TryParseExact(value, out result, format) ? result : ZDateTime.Empty;
		}

		public string ReadElementString(string name)
		{
			try
			{
				return (string.IsNullOrEmpty(Reader.Name) || Reader.Name == name) ? Reader.ReadElementString(name) : "";
			}
			catch (XmlException)
			{
				return "";
			}
		}

		public readonly XmlReader Reader;
	}
}
