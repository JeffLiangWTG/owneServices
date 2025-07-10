using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eHubMessaging.Business
{
	class Operand : AstNode
	{
		readonly object _var;
		readonly XPathResultType _type;

		internal Operand(String var)
		{
			_var = var;
			_type = XPathResultType.String;
		}

		internal Operand(double var)
		{
			_var = var;
			_type = XPathResultType.Number;
		}

		internal Operand(bool var)
		{
			_var = var;
			_type = XPathResultType.Boolean;
		}

		internal override QueryType TypeOfAst
		{
			get { return QueryType.ConstantOperand; }
		}

		internal override XPathResultType ReturnType
		{
			get { return _type; }
		}

		internal String OperandType
		{
			get
			{
				switch (_type)
				{
					case XPathResultType.Number: return (NoResString)"number";
					case XPathResultType.String: return (NoResString)"string";
					case XPathResultType.Boolean: return (NoResString)"boolean";
				}
				return null;
			}
		}

		internal object OperandValue
		{
			get { return _var; }
		}

		internal String Prefix => string.Empty;
	}
}
