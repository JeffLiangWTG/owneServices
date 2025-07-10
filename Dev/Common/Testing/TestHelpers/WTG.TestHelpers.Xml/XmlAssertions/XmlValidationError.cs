using System;
using System.Xml.Linq;

namespace NUnit.Framework
{
	abstract class XmlValidationError<TXmlObject> : IXmlValidationError
		where TXmlObject : XObject
	{
		readonly string error;
		readonly string errorDetailedInformation;
		protected readonly TXmlObject actualObject;

		protected XmlValidationError(string error, string errorDetailedInformation, TXmlObject actualObject = null)
		{
			this.error = error ?? string.Empty;
			this.errorDetailedInformation = errorDetailedInformation ?? string.Empty;
			this.actualObject = actualObject;
		}

		public string GetMessage() => $"{error}{NewLineIfNotEmpty(error)}{errorDetailedInformation}{GetActualValueString()}";

		public override string ToString() => GetMessage();

		static string NewLineIfNotEmpty(string value)
		{
			if (value.Length == 0)
			{
				return string.Empty;
			}

			return Environment.NewLine;
		}

		protected virtual string GetActualValueString()
		{
			if (actualObject == null)
			{
				return string.Empty;
			}

			return $"{Environment.NewLine}Actual Value:{Environment.NewLine}{actualObject}";
		}
	}
}
