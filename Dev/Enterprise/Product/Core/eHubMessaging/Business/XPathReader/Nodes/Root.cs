namespace Enterprise.eHubMessaging.Business
{
	class Root : AstNode
	{
		internal Root()
		{
		}

		internal override QueryType TypeOfAst
		{
			get { return QueryType.Root; }
		}

		internal override XPathResultType ReturnType
		{
			get { return XPathResultType.NodeSet; }
		}
	}
}
