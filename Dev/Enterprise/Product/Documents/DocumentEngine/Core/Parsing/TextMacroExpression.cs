using System;
using System.Collections.Generic;
using System.Reflection;
using Enterprise.Integration.DocumentEngine;

namespace Enterprise.DocumentEngine
{
	public class TextMacroExpression : ITextMacroExpression
	{
		public TextMacroExpression()
		{
		}

		public ITextMacroValueProvider ValueProvider { get; set; }
		public List<ITextMacroExpression> Expressions { get; set; }
		public MethodInfo MethodInfo { get; set; }
		public string TextExpression { get; set; }
		public MacroExpressionType ExpressionType { get; set; }
		public Type DataSourceType { get; set; }
		public Type ReturnType { get; set; }
	}
}
