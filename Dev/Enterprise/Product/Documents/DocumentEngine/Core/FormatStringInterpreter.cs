using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine
{
	public class FormatStringInterpreter : IFormatStringInterpreter, IDisposable
	{
		#region Ctor

		public FormatStringInterpreter()
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		#endregion

		#region Properties / Fields

		IBODocDataProvider dataProvider;
		readonly FormatStringFunction[] functions = new FormatStringFunction[] { new FormatStringCount(), new FormatStringSum() };

		public Func<IList> GetCollectionScopeStrategy
		{
			get;
			set;
		}

		Regex PropertyNameWithFormatRegex
		{
			get { return propertyNameWithFormatRegex ?? (propertyNameWithFormatRegex = new Regex(@"^(?<FieldOrFunctionName>[\w\.\(\)]+):(?<FormatString>[\w]+)|(?<FieldOrFunctionName>[\w\.\(\)]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)); }
		}
		[ThreadStatic]
		static Regex propertyNameWithFormatRegex;

		internal static Regex FieldNamesRegex
		{
			get { return fieldNamesRegex ?? (fieldNamesRegex = new Regex(@"\{(?<FieldName>[^\{\}]+)\}", RegexOptions.Compiled | RegexOptions.IgnoreCase)); }
		}
		[ThreadStatic]
		static Regex fieldNamesRegex;

		#endregion

		public void Dispose()
		{
			GetCollectionScopeStrategy = null;
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}

		public static ZString Format(BusinessObject parent, ZString formatString)
		{
			using (var interpreter = new FormatStringInterpreter())
			{
				return interpreter.Interpret(BODocDataProvider.Get(parent), formatString);
			}
		}

		public static ZString Format(IBODocDataProvider docDataProvider, ZString formatString)
		{
			using (var interpreter = new FormatStringInterpreter())
			{
				return interpreter.Interpret(docDataProvider, formatString);
			}
		}

		ZString IFormatStringInterpreter.Format(BusinessObject parent, ZString formatString)
		{
			return Interpret(BODocDataProvider.Get(parent), formatString);
		}

		ZString IFormatStringInterpreter.Format(IBODocDataProvider docDataProvider, ZString formatString)
		{
			return Interpret(docDataProvider, formatString);
		}

		public ZString Interpret(IBODocDataProvider docDataProvider, string formatString)
		{
			dataProvider = docDataProvider;

			return FieldNamesRegex.Replace(formatString, (match) =>
				{
					string fieldName = match.Groups["FieldName"].Value;
					string valueFromField = !String.IsNullOrEmpty(fieldName) ? GetValue(fieldName) : null;
					return valueFromField ?? String.Concat("{", fieldName, "}");
				}
			);
		}

		string GetValue(string field)
		{
			var match = PropertyNameWithFormatRegex.Match(field);

			string fieldName = match.Groups["FieldOrFunctionName"].Value;
			string formatString = match.Groups["FormatString"].Value;

			return dataProvider != null ? GetValue(fieldName, formatString) : fieldName;
		}

		string GetValue(string field, string formatString)
		{
			string result = null;

			var function = (from f in functions
							where f.Match(field)
							select f).FirstOrDefault();

			object data = function != null ? GetValueFromFunction(function) : GetValueFromField(field);

			if (data != null && typeof(IZType).IsAssignableFrom(data.GetType()))
			{
				data = new DocumentIZTypeFieldFormatter((IZType)data).ToString(formatString);
			}

			result = data != null ? data.ToString() : null;

			return result;
		}

		IZType GetValueFromFunction(FormatStringFunction function)
		{
			IList collectionScope = GetCollectionScopeStrategy != null ? GetCollectionScopeStrategy() : dataProvider as IList;

			return function.Execute(collectionScope);
		}

		object GetValueFromField(string propertyName)
		{
			var reflector = new BusinessObjectReflector();
			var businessObjectDataProvider = new BusinessObjectDataProvider();

			var obj = BODocDataProvider.GetObject(dataProvider);

			var methodInfoChain = !String.IsNullOrEmpty(propertyName) ? reflector.GetMethodInfoChain(obj.GetType(), obj, propertyName) : null;

			return methodInfoChain != null ? businessObjectDataProvider.GetFieldValueFromMethodInfoChain(obj, methodInfoChain) : null;
		}
	}
}
