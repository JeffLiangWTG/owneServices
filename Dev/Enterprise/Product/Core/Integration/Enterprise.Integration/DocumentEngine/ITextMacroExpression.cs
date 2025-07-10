
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Enterprise.Integration.DocumentEngine
{
	public interface ITextMacroExpression
	{
		public ITextMacroValueProvider ValueProvider { get; }
		public List<ITextMacroExpression> Expressions { get; }
		public MacroExpressionType ExpressionType { get; }
		public MethodInfo MethodInfo { get; }
		public string TextExpression { get; }
		public Type DataSourceType { get; }
		public Type ReturnType { get; }
	}

	public interface ITextMacroValueProvider
	{
	}

	public enum MacroExpressionType
	{
		Unknown,
		Property,
		Method,
		Constant,
		Variable,
		ValueProvider
	}
}
