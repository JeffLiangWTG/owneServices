namespace Enterprise.eHubMessaging.Business
{
	internal class Filter : AstNode
	{
		readonly AstNode _input;
		readonly AstNode _condition;

		internal Filter(AstNode input, AstNode condition)
		{
			_input = input;
			_condition = condition;
		}

		internal override QueryType TypeOfAst
		{
			get { return QueryType.Filter; }
		}

		internal override XPathResultType ReturnType
		{
			get { return XPathResultType.NodeSet; }
		}

		internal AstNode Input
		{
			get { return _input; }
		}

		internal AstNode Condition
		{
			get { return _condition; }
		}
	}
}
