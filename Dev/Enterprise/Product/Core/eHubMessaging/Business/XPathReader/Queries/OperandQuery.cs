namespace Enterprise.eHubMessaging.Business
{
	class OperandQuery : Query
	{
		readonly object variable;
		readonly XPathResultType type;

		internal OperandQuery(object var, XPathResultType type)
		{
			this.variable = var;
			this.type = type;
		}

		internal override object GetValue(XPathReader reader)
		{
			return (this.variable);
		}

		internal override XPathResultType ReturnType()
		{
			return this.type;
		}
	}
}
