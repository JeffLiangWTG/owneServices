using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	public class UpdateListResponseTokenTest : WebServiceResponseActionTokenTest
	{
		#region Implementation

		protected override WebServiceResponseToken GetResponseTokenForTesting()
		{
			var values = new CodeDescriptionPairList();
			values.AddPair("TST", "TEST");
			values.AddPair("NON", "NONE");

			return new UpdateListResponseToken("ControlID", values);
		}

		protected override string GetExpectedTokenValue() => @"[{""Code"":""TST"",""Description"":""TEST""},{""Code"":""NON"",""Description"":""NONE""}]";

		protected override string GetExpectedAction() => WebServiceResponseActions.UpdateList;

		#endregion
	}
}
