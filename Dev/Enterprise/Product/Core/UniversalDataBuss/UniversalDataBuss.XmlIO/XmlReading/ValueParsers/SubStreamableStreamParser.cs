using System;
using System.Reflection;
using CargoWise.IO;
using CargoWise.IO.Streaming;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlReading.ElementReaders;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlReading.ValueParsers
{
	sealed class SubStreamableStreamParser : ValueParser
	{
		internal SubStreamableStreamParser(XmlReader reader) : base(reader) { }

		internal sealed override bool HandlesType(Type targetType)
		{
			return targetType == typeof(SubStreamableStream);
		}

		protected sealed override object TryParseAndValidate(PropertyInfo propertyInfo, Type enumType, object sourceValue, int? lineNumber, string elementFullName, bool isAttribute)
		{
			if (sourceValue == null)
			{
				string valueSourceDescription = Res.GetString("D6B247AE-7E88-4D27-B4B1-59309C4B9284", "Line {0}: {1}", lineNumber ?? reader.CurrentLineNumber, reader.CurrentElementParser.GetFullName());
				reader.Logger.Log(LogType.Warning, Res.GetString("082065B0-3BF4-46E8-8D9E-9FFEE7172259", "{0} - Data should not be empty.", valueSourceDescription));
				return null;
			}

			var stream = (SubStreamableStream)sourceValue;
			bool removeWhitespace = false;
			do
			{
				var outputStream = new CargoWise.IO.Shim.SubStreamableStream();
				try
				{
					using (var inputStream = new Base64StreamAdapter(removeWhitespace ? stream.RemoveWhitespace() : stream, !removeWhitespace))
					{
						inputStream.CopyTo(outputStream);
					}
					outputStream.Position = 0;
					return outputStream;
				}
				catch (FormatException)
				{
					outputStream.Dispose();
					if (removeWhitespace)
					{
						stream.Dispose();
						string valueSourceDescription = Res.GetString("D6B247AE-7E88-4D27-B4B1-59309C4B9284", "Line {0}: {1}", lineNumber ?? reader.CurrentLineNumber, reader.CurrentElementParser.GetFullName());
						reader.Logger.Log(LogType.Warning, Res.GetString("5E4ADD2F-0336-4181-B243-F40C9904063F", "{0} - Invalid Base64 encoded binary data.", valueSourceDescription));
						return null;
					}
					removeWhitespace = true;
				}
			}
			while (removeWhitespace);
			return null;
		}
	}
}
