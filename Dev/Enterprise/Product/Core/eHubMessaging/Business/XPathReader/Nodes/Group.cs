namespace Enterprise.eHubMessaging.Business
{
	class Group : AstNode
	{
		readonly AstNode _groupNode;

		internal Group(AstNode groupNode)
		{
			_groupNode = groupNode;
		}
		internal override QueryType TypeOfAst
		{
			get { return QueryType.Group; }
		}
		internal override XPathResultType ReturnType
		{
			get { return XPathResultType.NodeSet; }
		}

		internal AstNode GroupNode
		{
			get { return _groupNode; }
		}

		internal override double DefaultPriority
		{
			get { return 0; }
		}
	}
}
