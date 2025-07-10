using System;
using System.Reflection;
using System.Text;

namespace Enterprise.ZArchitecture.Core
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class UserEventDiagnosticReferenceAttribute : Attribute
	{
		public UserEventDiagnosticReferenceAttribute(string referenceFormat)
		{
			ReferenceFormat = referenceFormat;
		}

		public string ReferenceFormat { get; private set; }

		public static string Render(object value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			UserEventDiagnosticReferenceAttribute att = GetAttribute(value.GetType());

			if (att == null || att.ReferenceFormat == null)
			{
				return null;
			}
			else
			{
				try
				{
					return Render(value, att.ReferenceFormat);
				}
				catch (FormatException ex)
				{
					return ex.Message;
				}
			}
		}

		static string Render(object rootTarget, string format)
		{
			int openBrace = -1;
			bool readingName = false;

			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < format.Length; i++)
			{
				switch (format[i])
				{
					case '{':
						if (!readingName)
						{
							openBrace = i;

							if (i + 1 < format.Length && format[i + 1] == '{')
							{
								builder.Append(@"\{");
								i++;
							}
							else
							{
								readingName = true;
							}
						}
						else
						{
							throw new FormatException("badly placed '{'");
						}
						break;

					case '}':
						if (readingName)
						{
							readingName = false;
							builder.Append(GetValueByPath(rootTarget, format.Substring(openBrace + 1, i - openBrace - 1)));
						}
						else if (i + 1 < format.Length && format[i + 1] == '}')
						{
							builder.Append(@"\}");
							i++;
						}
						else
						{
							throw new FormatException("badly placed '}'");
						}
						break;

					case '\\':
						if (!readingName)
						{
							builder.Append(@"\\");
						}
						else
						{
							throw new FormatException("encountered an invalid character while reading a property name.");
						}
						break;

					default:
						if (!readingName)
						{
							builder.Append(format[i]);
						}
						else if (format[i] != '.' && !char.IsLetterOrDigit(format, i))
						{
							throw new FormatException("encountered an invalid character while reading a property name.");
						}
						break;
				}
			}

			if (readingName)
			{
				throw new FormatException("unclosed token");
			}

			return builder.ToString();
		}

		static string GetValueByPath(object root, string propertyPath)
		{
			string[] path = propertyPath.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
			object current = root;

			for (int i = 0; i < path.Length; i++)
			{
				current = GetValue(current, path[i]);
				if (current == null)
				{
					return string.Empty;
				}
			}

			return current.ToString();
		}

		static object GetValue(object target, string name)
		{
			Type targetType = target.GetType();

			PropertyInfo propertyInfo = targetType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (propertyInfo != null)
			{
				return propertyInfo.GetValue(target, Array.Empty<object>());
			}

			FieldInfo fieldInfo = targetType.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (fieldInfo != null)
			{
				return fieldInfo.GetValue(target);
			}

			return null;
		}

		static UserEventDiagnosticReferenceAttribute GetAttribute(Type type)
		{
			return (UserEventDiagnosticReferenceAttribute)Attribute.GetCustomAttribute(type, typeof(UserEventDiagnosticReferenceAttribute));
		}
	}
}
