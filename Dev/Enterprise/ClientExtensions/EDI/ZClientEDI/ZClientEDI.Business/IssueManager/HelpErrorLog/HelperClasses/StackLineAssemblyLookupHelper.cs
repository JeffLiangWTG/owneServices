using System;
using System.Text.RegularExpressions;
using CargoWise.Common;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	static class StackLineAssemblyLookupHelper
	{
		internal const int ClassNameMaxLength = 512;
		internal const int MethodNameMaxLength = 512;

		internal static string GetFullMethodNameWithoutParameters(string fullStackLine)
		{
			if (fullStackLine.IsNullOrEmpty())
			{
				return string.Empty;
			}

			var index = fullStackLine.IndexOf('(');
			if (index < 1)
			{
				return string.Empty;
			}

			return fullStackLine.Substring(0, index).Trim();
		}

		internal static (string className, string methodName) ParseFullMethodName(string fullMethodName)
		{
			var i = fullMethodName.LastIndexOf('.');
			var length = fullMethodName.Length;

			if (i == 0 && length == 1)
			{
				return (null, string.Empty);
			}

			if (i > 0)
			{
				var ctor = fullMethodName.LastIndexOf(".ctor", StringComparison.CurrentCultureIgnoreCase);
				if (i == ctor)
				{
					i--;
					if (i > 0 && fullMethodName[i] == '<')
					{
						i--;
					}
				}

				var className = fullMethodName.Substring(0, i);
				var methodName = fullMethodName.Substring(i + 1);
				className = RemoveAssemblyInformation(className);
				methodName = RemoveAssemblyInformation(methodName);

				return (className, methodName);
			}

			return (null, fullMethodName);
		}

		static string RemoveAssemblyInformation(string value)
		{
			if (value.EndsWith("]"))
			{
				var regex = new Regex("]([^,\\[\\]]*)\\[");
				var match = regex.Match(value);
				if (match.Success)
				{
					ErrorReporter.ReportOnce("MissingInformationWhenRemoveAssemblyInformation", "In RemoveAssemblyInformation method, our truncate algorithm is not enough which make us missing some information, please check it.");
				}
				value = value.Substring(0, value.IndexOf("[", StringComparison.InvariantCulture)).Trim();
			}

			return value;
		}

		internal static string Truncate(string value, int length)
		{
			if (value != null && value.Length > length)
			{
				value = value.Substring(0, length);
			}
			return value;
		}

		internal static string TruncateStackLine(string stackLine)
		{
			return Truncate(stackLine, AutoHelpErrorStackLineCount.Schema.HSL_StackLineMaxLength);
		}

		internal static string TruncateAssembly(string assembly)
		{
			return Truncate(assembly, AutoHelpErrorStackLineCount.Schema.HSL_AssemblyMaxLength);
		}

		internal static string TruncateType(string type)
		{
			return Truncate(type, ClassNameMaxLength);
		}

		internal static string TruncateMethod(string method)
		{
			return Truncate(method, MethodNameMaxLength);
		}

		internal static string TruncateParameters(string parameter)
		{
			return Truncate(parameter, AutoHelpErrorStackLineCount.Schema.HSL_ParametersMaxLength);
		}

		internal static bool IsWeightCalculableStackLine(IStackLine stackLine)
		{
			return stackLine != null && !string.IsNullOrEmpty(stackLine.Type) && !string.IsNullOrEmpty(stackLine.Method);
		}
	}
}
