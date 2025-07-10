using Enterprise.Environment;

namespace Enterprise.Customs.IL.Business
{
	public static class MessageProviderHelper
	{
		#region Constants

		public const string TestMessageTo = "ILCustomsTEST";
		public const string ProdMessageTo = "ILCustoms";

		public static string MessageTo => Env.Instance.IsProductionSystem ? ProdMessageTo : TestMessageTo;

		#endregion Constants
	}
}
