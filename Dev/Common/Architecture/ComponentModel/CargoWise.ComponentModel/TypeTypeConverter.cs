using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using AppDomainWrappers.Net;
using CargoWise.Common;
using Microsoft.CSharp;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// A type converter for converting between Type and string. A TypeNameHolder is created if the real
	/// type cannot be found.
	/// </summary>
	public class TypeTypeConverter : TypeConverter
	{
		#region TemporarilyResolveTypeOnlyFromCurrentlyLoadedAssemblies

		public static IDisposable TemporarilyResolveTypeOnlyFromCurrentlyLoadedAssemblies()
		{
			resolveOnlyFromCurrentlyLoadedAssemblies++;
			return new DisposableAction(delegate
			{ resolveOnlyFromCurrentlyLoadedAssemblies--; });
		}
		[ThreadStatic]
		static int resolveOnlyFromCurrentlyLoadedAssemblies;

		#endregion

		#region ConvertFrom

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			object result = null;
			var valueString = value as string;
			if (valueString != null)
			{
				result = ConvertFromStringImpl(context, valueString);
			}
			else
			{
				result = base.ConvertFrom(context, culture, value);
			}
			return result;
		}

		Type ConvertFromStringImpl(ITypeDescriptorContext context, string value)
		{
			Argument.NotNull(value, nameof(value));
			Type result = null;
			if (value.Length > 0)
			{
				result = ParseTypeName(context, value);
			}
			return result;
		}

		#endregion

		#region ConvertTo

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(Type);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			object result = null;
			var type = value as Type;
			if (destinationType == typeof(string))
			{
				result = (type == null) ? "" : ConvertTo(context, culture, type);
			}
			else
			{
				result = base.ConvertTo(context, culture, value, destinationType);
			}
			return result;
		}

		protected virtual string ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Type value)
		{
			Argument.NotNull(value, nameof(value)); // Suggested By ReviewBot 
			var result = value.IsGenericType ? value.GetGenericTypeDefinition().FullName : value.FullName;
			var genericArguments = value.GetGenericArguments();
			if (!value.IsGenericTypeDefinition && genericArguments != null && genericArguments.Length > 0)
			{
				result += "[";
				for (int i = 0; i < genericArguments.Length; i++)
				{
					if (i != 0)
					{
						result += ",";
					}
					var genericArgument = genericArguments[i];
					if (genericArgument != null)
					{
						result += ConvertTo(context, culture, genericArgument);
					}
				}
				result += "]";
			}
			return result;
		}

		#endregion

		#region ParseTypeName

		Type ParseTypeName(IServiceProvider serviceProvider, string typeName)
		{
			Argument.NotNull(typeName, nameof(typeName));
			var index = 0;
			var convertedTypeName = ConvertPrimitiveNamesAndNullablesIfRequired(typeName);
			return ParseTypeName(serviceProvider, convertedTypeName, true, ref index);
		}

		Type ParseTypeName(IServiceProvider serviceProvider, string typeName, bool allowCommas, ref int index)
		{
			Argument.NotNull(typeName, nameof(typeName)); // Suggested By ReviewBot 
			var hasOpenBracket = ReadOpenBracket(typeName, ref index);
			SkipWhitespaces(typeName, ref index);

			int genericStart = index < typeName.Length ? typeName.IndexOfAny(new char[] { '[', '<' }, index) : -1;
			int endBracket = LocateEndOfGenericTypeOrArgument(typeName, allowCommas, hasOpenBracket, index);
			var hasArguments = genericStart != -1 && (endBracket == -1 || genericStart < endBracket);

			Type result = null;
			var name = typeName;
			if (hasArguments)
			{
				if (genericStart - index >= 0)
				{
					name = typeName.Substring(index, genericStart - index).Trim();
				}
				var arguments = Array.Empty<Type>();
				index = genericStart + 1;
				if (index < typeName.Length)
				{
					arguments = ParseGenericTypeArguments(serviceProvider, typeName, ref index);
				}
				SkipWhitespaces(typeName, ref index);
				index++;
				name = TypeNameHolder.ChangeTypeNameGenericArgumentCount(name, arguments.Length);
				var genericType = ResolveType(serviceProvider, name);
				result = arguments.Length == 0 ? genericType : TypeNameHolder.MakeGenericType(genericType, arguments);

				SkipWhitespaces(typeName, ref index);
				if (hasOpenBracket)
				{
					index++;
				}
			}
			else
			{
				if (index <= typeName.Length)
				{
					if (endBracket == -1)
					{
						name = typeName.Substring(index);
					}
					else if (index <= endBracket)
					{
						name = typeName.Substring(index, endBracket - index);
					}
				}
				name = name.Trim();
				if (name.Length > 0)
				{
					result = ResolveType(serviceProvider, name);
					index = hasOpenBracket ? endBracket + 1 : endBracket;
				}
			}
			return result;
		}

		static int LocateEndOfGenericTypeOrArgument(string str, bool allowCommas, bool hasOpenBracket, int index)
		{
			Argument.NotNull(str, nameof(str)); // Suggested By ReviewBot 
			int result = -1;
			if (index <= str.Length)
			{
				if (hasOpenBracket)
				{
					result = str.IndexOf(']', index);
				}
				else if (index < str.Length)
				{
					if (allowCommas)
					{
						result = str.IndexOfAny(new char[] { ']', '>' }, index);
					}
					else
					{
						result = str.IndexOfAny(new char[] { ',', ']', '>' }, index);
					}
				}
			}
			return result;
		}

		static bool ReadOpenBracket(string str, ref int index)
		{
			Argument.NotNull(str, nameof(str)); // Suggested By ReviewBot 
			var result = false;
			SkipWhitespaces(str, ref index);
			if (index < str.Length && str[index] == '[')
			{
				result = true;
				index++;
			}
			return result;
		}

		Type[] ParseGenericTypeArguments(IServiceProvider serviceProvider, string str, ref int index)
		{
			Argument.NotNull(str, nameof(str)); // Suggested By ReviewBot 
			var result = new List<Type>();
			do
			{
				SkipWhitespaces(str, ref index);
				var argument = ParseTypeName(serviceProvider, str, false, ref index);
				result.Add(argument);
				SkipWhitespaces(str, ref index);
			}
			while (index < str.Length && str[index++] == ',');
			index--;
			return result.ToArray();
		}

		static void SkipWhitespaces(string str, ref int index)
		{
			Argument.NotNull(str, nameof(str)); // Suggested By ReviewBot 
			if (index < 0)
			{
				index = 0;
			}
			while (index < str.Length && char.IsWhiteSpace(str[index]))
			{
				index++;
			}
		}

		#endregion

		#region ResolveType

		protected virtual Type ResolveTypeCore(IServiceProvider serviceProvider, string typeName)
		{
			Argument.NotNull(typeName, nameof(typeName));
			Type result = null;
			if (resolveOnlyFromCurrentlyLoadedAssemblies == 0 || TypeNameHolder.IsTypeExistsInSolution(typeName, serviceProvider))
			{
				result = Type.GetType(typeName);
			}
			else
			{
				if (!typeName.Contains(","))
				{
					var appDomainWrapper = new AppDomainWrapper();
					var assemblies = appDomainWrapper.GetAssemblies();

					foreach (var assembly in assemblies)
					{
						if (assembly.GetType(typeName) != null)
						{
							result = Type.GetType(typeName += ", " + assembly.GetName().Name);
							break;
						}
					}
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Type name")]
		Type ResolveType(IServiceProvider serviceProvider, string typeName)
		{
			Argument.NotNull(typeName, nameof(typeName));
			typeName = ConvertPrimitiveNamesAndNullablesIfRequired(typeName);
			var result = FindTypeIncludingNested(serviceProvider, typeName);
			if (result == null)
			{
				if (typeName.Equals("void", StringComparison.OrdinalIgnoreCase))
				{
					result = typeof(void);
				}
				else
				{
					result = new TypeNameHolder(typeName, serviceProvider);
				}
			}
			return result;
		}

		Type FindTypeIncludingNested(IServiceProvider serviceProvider, string typeName)
		{
			Argument.NotNull(typeName, nameof(typeName)); // Suggested By ReviewBot 
			Type result = null;
			while (result == null)
			{
				result = ResolveTypeCore(serviceProvider, typeName);
				int index = typeName.LastIndexOf('.');
				if (index == -1)
				{
					break;
				}
				else
				{
					typeName = typeName.Substring(0, index) + "+" + typeName.Substring(index + 1);
				}
			}
			return result;
		}

		string ConvertPrimitiveNamesAndNullablesIfRequired(string typeName)
		{
			Argument.NotNull(typeName, nameof(typeName));
			var result = typeName;
			result = ConvertPrimitiveNameIfRequired(result);
			result = ConvertNullableTypeNameIfRequired(result);
			return result;
		}

		string ConvertPrimitiveNameIfRequired(string primitiveName)
		{
			Argument.NotNull(primitiveName, nameof(primitiveName));
			foreach (var type in GetTypesWithCSharpAbbreviatedTypeNames())
			{
				if (primitiveName == CSharpCodeProvider.GetTypeOutput(new CodeTypeReference(type.FullName)))
				{
					return type.FullName;
				}
			}
			return primitiveName;
		}

		string ConvertNullableTypeNameIfRequired(string nullableName)
		{
			Argument.NotNull(nullableName, nameof(nullableName)); // Suggested By ReviewBot 
			var result = nullableName;
			if (nullableName.EndsWith("?", StringComparison.Ordinal))
			{
				string innerType = ConvertPrimitiveNamesAndNullablesIfRequired(nullableName.Substring(0, nullableName.Length - 1));
				result = typeof(Nullable<>).FullName + "[" + innerType + "]";
			}
			return result;
		}

		CSharpCodeProvider CSharpCodeProvider
		{
			get
			{
				if (cSharpCodeProvider == null)
				{
					cSharpCodeProvider = new CSharpCodeProvider();
				}
				return cSharpCodeProvider;
			}
		}
		CSharpCodeProvider cSharpCodeProvider;

		static Type[] GetTypesWithCSharpAbbreviatedTypeNames()
		{
			var result = new List<Type>();
			result.Add(typeof(bool));
			result.Add(typeof(byte));
			result.Add(typeof(char));
			result.Add(typeof(double));
			result.Add(typeof(short));
			result.Add(typeof(int));
			result.Add(typeof(long));
			result.Add(typeof(sbyte));
			result.Add(typeof(float));
			result.Add(typeof(ushort));
			result.Add(typeof(uint));
			result.Add(typeof(ulong));
			result.Add(typeof(decimal));
			result.Add(typeof(string));
			result.Add(typeof(object));
			return result.ToArray();
		}

		#endregion
	}
}
