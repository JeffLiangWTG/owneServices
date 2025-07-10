using System;

namespace Enterprise.eHubMessaging.Business
{
	internal class AstNode
	{
		internal enum QueryType
		{
			Axis,
			Operator,
			Filter,
			ConstantOperand,
			Function,
			Group,
			Root,
			Variable,
			Error
		}

		static internal AstNode NewAstNode(String parsestring)
		{
			try
			{
				return (XPathParser.ParseXPathExpresion(parsestring));
			}
			catch (XPathException)
			{
				return null;
			}
		}

		internal virtual QueryType TypeOfAst
		{
			get { return QueryType.Error; }
		}

		internal virtual XPathResultType ReturnType
		{
			get { return XPathResultType.Error; }
		}

		internal virtual double DefaultPriority
		{
			get { return 0.5; }
		}
	}
}
