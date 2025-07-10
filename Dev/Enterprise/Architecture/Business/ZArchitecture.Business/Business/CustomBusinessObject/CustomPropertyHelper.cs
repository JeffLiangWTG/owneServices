using System;
using System.Text;

namespace Enterprise.ZArchitecture.Business
{
	public static class CustomPropertyHelper
	{
		public static string GeneratePropertyIdentifier(string propertyName, Type propertyType)
		{
			if (IsPropertyIdentifier(propertyName))
			{
				return propertyName;
			}

			if (propertyType == null)
			{
				return propertyName;
			}

			var typeName = propertyType.Name;
			var typeLength = typeName.Length;
			StringBuilder stringBuilder = new StringBuilder(propertyName.Length + typeLength + 10);
			stringBuilder.Append("__");

			// We cannot have + or . in the custom field name as these interfere with gui binding, so they are escaped 
			// to maintain uniqueness, and these characters cannot be entered by the user.
			foreach (var originalChar in propertyName)
			{
				string transformation;
				switch (originalChar)
				{
					// Character we are using to escape
					case '`':
						transformation = "``";
						break;
					// + cannot be used in GUI binding so it is escaped
					case '+':
						transformation = "`-";
						break;
					// . cannot be used in GUI binding so it is escaped
					case '.':
						transformation = "`|";
						break;
					// Underscore is used to break up properties in the identifier, so it is escaped
					case '_':
						transformation = "`=";
						break;
					// Identifiers are treated as case insensitive, so we perform a ToUpper
					default:
						transformation = char.ToUpperInvariant(originalChar).ToString();
						break;
				}

				foreach (var transformedChar in transformation)
				{
					stringBuilder.Append(transformedChar);
				}
			}

			stringBuilder.Append("__prop__");
			stringBuilder.Append(typeName);
			return stringBuilder.ToString();
		}

		public static bool IsPropertyIdentifier(string propertyName)
		{
			return propertyName.StartsWith("__", StringComparison.Ordinal) && propertyName.Contains("__prop__");
		}

		internal static bool ExtractPropertyNameAndTypeFromIdentifier(string identifier, out (string PropertyName, string TypeName) result)
		{
			string preAmble = "__";

			result = ("", "");

			if (identifier.Length < preAmble.Length)
			{
				return false;
			}

			for (int i = 0; i < preAmble.Length; ++i)
			{
				if (identifier[i] != preAmble[i])
				{
					return false;
				}
			}

			int index = 2;
			var propertyName = new StringBuilder(identifier.Length);
			for (; index < identifier.Length; ++index)
			{
				var aChar = identifier[index];
				if (aChar == '`' && ((index + 1) < identifier.Length))
				{
					var nextChar = identifier[index + 1];
					switch (nextChar)
					{
						case '`':
							aChar = '`';
							break;
						case '-':
							aChar = '+';
							break;
						case '|':
							aChar = '.';
							break;
						case '=':
							aChar = '_';
							break;
						default:
							return false;
					}

					index++;
				}
				else if (aChar == '_')
				{
					break;
				}

				propertyName.Append(aChar);
			}

			string propSeparator = "__prop__";

			if (index >= (identifier.Length - propSeparator.Length))
			{
				return false;
			}

			for (int i = 0; i < propSeparator.Length; ++i)
			{
				if (identifier[index + i] != propSeparator[i])
				{
					return false;
				}
			}

			index += propSeparator.Length;

			var propertyTypeName = new StringBuilder(3);

			for (; index < identifier.Length; ++index)
			{
				propertyTypeName.Append(identifier[index]);
			}

			result = (propertyName.ToString(), propertyTypeName.ToString());
			return true;
		}
	}
}
