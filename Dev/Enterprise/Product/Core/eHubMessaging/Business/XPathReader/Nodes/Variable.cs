using System;

namespace Enterprise.eHubMessaging.Business
{
	class Variable : AstNode
	{
		readonly String _Localname;
		readonly String _Prefix = string.Empty;

		internal Variable(String name, String prefix)
		{
			_Localname = name;
			_Prefix = prefix;
		}

		internal override QueryType TypeOfAst
		{
			get { return QueryType.Variable; }
		}

		internal override XPathResultType ReturnType
		{
			get { return XPathResultType.Error; }
		}

		internal String Name
		{
			get
			{
				if (!string.IsNullOrEmpty(Prefix))
				{
					return _Prefix + ":" + _Localname;
				}
				else
				{
					return _Localname;
				}
			}
		}

		internal String Localname
		{
			get { return _Localname; }
		}

		internal String Prefix
		{
			get { return _Prefix; }
		}
	}
}
