using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class DataType : ValueProvider, INonVisualisableValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<DataType[({Full name of the data type of current cell}]>",
ResString.GetMultilingualString("62fa4e9d-d008-4f3c-bee3-6334290bfa99", @"Used as a prefix to indicate the value type of the content in current cell.
This is a runtime macro so please don't use it in any template.
The parameter is required for this macro and the doc engine will try to parse the content of the cell to the target type after replacing."),
				new List<(string example, object expectedResult)> { ((NoResString)"<DataType(\"System.Decimal\")><Value>", null) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return string.Empty;
		}

		public override bool IsDocumentationVisible
		{
			get
			{
				return false;
			}
		}

		public override Passes PassToStartReplacingOn
		{
			get { return Passes.SecondPass; }
		}

		public override Regex Regex
		{
			get { return regex; }
		}

		internal static string CreateDataTypeMacro(object cellValue)
		{
			string result = string.Empty;
			if (cellValue != null && !(cellValue is string) && !(cellValue is ZString))
			{
				string cellValueType = string.Empty;
				cellValueType = cellValue.GetType().FullName;
				result = "<DataType(\"" + cellValueType + "\")>";
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods")]
		internal static object ConvertCellValueType(string macro, object cellContent)
		{
			object result = cellContent;
			string typeName = regex.Match(macro).Groups["TypeName"].Value;

			if (!string.IsNullOrEmpty(typeName))
			{
				Type dataType = Type.GetType(typeName) ?? typeof(IZType).Assembly.GetType(typeName);

				if (dataType != null)
				{
					if (dataType == typeof(ZDateTime) || dataType == typeof(ZDate))
					{
						try
						{
							result = dataType.GetConstructor(new[] { typeof(object) }).Invoke(new[] { cellContent.ToString().Trim() });
						}
						catch (TargetInvocationException) { }
					}
					else
					{
						MethodInfo mi = dataType.GetMethod("TryParse", new[] { typeof(string), dataType.MakeByRefType() });

						if (mi != null)
						{
							var parameters = new[] { cellContent.ToString().Trim(), Activator.CreateInstance(dataType) };
							if ((bool)mi.Invoke(null, parameters))
							{
								result = parameters[1];
							}
						}
					}
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Regular Expression")]
		const string RegexPattern = @"<DataType\(""(?<TypeName>.*)""\)>";
		static readonly Regex regex = new Regex("^" + RegexPattern + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		/// <summary>
		/// This is the regex really used to interpret this line in the class Enterprise.DocumentEngine.Areas.Area 
		/// and by the visualiser to remove the macro text before visualising.
		/// </summary>
		internal static readonly Regex RegexToFindMacroAnyWhereInString = new Regex(RegexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		#region INonVisualisableValueProvider

		public Regex RegexToReplaceMacro => RegexToFindMacroAnyWhereInString;

		#endregion
	}
}
