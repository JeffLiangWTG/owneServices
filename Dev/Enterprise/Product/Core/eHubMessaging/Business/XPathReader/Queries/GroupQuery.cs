namespace Enterprise.eHubMessaging.Business
{
	internal sealed class GroupQuery : BaseAxisQuery
	{
		internal GroupQuery(Query queryInput)
			: base(queryInput)
		{
		}

		internal override object GetValue(XPathReader reader)
		{
			return base.QueryInput.GetValue(reader);
		}

		internal override XPathResultType ReturnType()
		{
			return base.QueryInput.ReturnType();
		}
	}
}
