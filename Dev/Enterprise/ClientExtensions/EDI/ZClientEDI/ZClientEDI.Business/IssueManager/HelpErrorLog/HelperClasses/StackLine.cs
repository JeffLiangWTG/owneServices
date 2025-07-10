using System;
using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	public class StackLine : IStackLine
	{
		public StackLine(string assembly, string type, string method, string parameters, string fullStackLine)
			: this(fullStackLine)
		{
			Assembly = StackLineAssemblyLookupHelper.TruncateAssembly(assembly);
			if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(method))
			{
				var fullPathMethodName = StackLineAssemblyLookupHelper.GetFullMethodNameWithoutParameters(fullStackLine);
				var (className, methodName) = StackLineAssemblyLookupHelper.ParseFullMethodName(fullPathMethodName);
				type = string.IsNullOrEmpty(type) ? className : type;
				method = string.IsNullOrEmpty(method) ? methodName : method;
			}
			Type = StackLineAssemblyLookupHelper.TruncateType(type);
			Method = StackLineAssemblyLookupHelper.TruncateMethod(method);
			Parameters = StackLineAssemblyLookupHelper.TruncateParameters(parameters);
		}

		public StackLine(string fullStackLine)
		{
			Argument.NotNull(fullStackLine, nameof(fullStackLine));

			FullStackLine = StackLineAssemblyLookupHelper.TruncateStackLine(fullStackLine);
			var fullPathMethodName = StackLineAssemblyLookupHelper.GetFullMethodNameWithoutParameters(fullStackLine);
			var (className, methodName) = StackLineAssemblyLookupHelper.ParseFullMethodName(fullPathMethodName);
			Type = StackLineAssemblyLookupHelper.TruncateType(className);
			Method = StackLineAssemblyLookupHelper.TruncateMethod(methodName);
		}

		string assembly;
		string type;
		string method;

		public string Assembly
		{
			get => assembly;
			set => assembly = value == null ? string.Empty : StackLineAssemblyLookupHelper.TruncateAssembly(value.Trim());
		}

		public string Type
		{
			get => type;
			set => type = value == null ? string.Empty : StackLineAssemblyLookupHelper.TruncateType(value.Trim());
		}

		public string Method
		{
			get => method;
			set => method = value == null ? string.Empty : StackLineAssemblyLookupHelper.TruncateMethod(value.Trim());
		}
		public string Parameters { get; }
		public string FullStackLine { get; }
	}

	public class StackLineEqualityComparer : IEqualityComparer<StackLine>
	{
		public bool Equals(StackLine x, StackLine y)
		{
			return x.FullStackLine.ToUpperInvariant().Equals(y.FullStackLine.ToUpperInvariant(), StringComparison.Ordinal);
		}

		public int GetHashCode(StackLine obj)
		{
			return obj.FullStackLine.ToUpperInvariant().GetHashCode();
		}
	}
}
