namespace Enterprise.ZArchitecture.Web.ServerServices
{
	public class ResponseConditionEqualToken : WebServiceResponseConditionToken
	{
		#region Constructors

		public ResponseConditionEqualToken(string controlID, object value)
			: base(WebServiceResponseConditions.Equal, controlID, value)
		{
		}

		#endregion

		#region Overrides

		protected override string GetCompareJavaScriptToken()
		{
			return "==";
		}

		#endregion
	}
}
