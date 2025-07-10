using System;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public static class UniversalXmlSchemaExtensions
	{
		public static bool IsValidUniversalXmlNamespace(this string @namespace)
		{
			return !string.IsNullOrWhiteSpace(@namespace)
				&& (string.Compare(UniversalXmlInfo.Namespace_2012_11, @namespace, StringComparison.Ordinal) == 0
				|| string.Compare(UniversalXmlInfo.Namespace_2011_11, @namespace, StringComparison.Ordinal) == 0);
		}

		public static IUniversalXmlSchema GetUniversalXmlSchema(this string @namespace)
		{
			if (string.IsNullOrWhiteSpace(@namespace))
			{
				return null;
			}

			switch (@namespace)
			{
				case UniversalXmlInfo.Namespace_2012_11:
					return UniversalXmlSchema.Version_2012_11_DO_NOT_USE;

				case UniversalXmlInfo.Namespace_2011_11:
					return UniversalXmlSchema.Version_2011_11;

				default:
					throw new InvalidOperationException(string.Format("{0} is not a valid Universal Xml namespace", @namespace));
			}
		}
	}
}
