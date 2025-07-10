namespace Enterprise.eHubMessaging.Business
{
	public enum XPathResultType
	{
		Number = 0,
		String = 1,
		Boolean = 2,
		NodeSet = 3,
		Navigator = XPathResultType.String,
		Any = 5,
		Error
	}

	internal abstract class Query
	{
		int positionCount;

		internal virtual object GetValue(XPathReader reader)
		{
			return null;
		}

		internal virtual int PositionCount
		{
			get { return this.positionCount; }
			set { this.positionCount = value; }
		}

		internal virtual bool MatchNode(XPathReader reader)
		{
			return false;
		}

		internal abstract XPathResultType ReturnType();
	}
}
