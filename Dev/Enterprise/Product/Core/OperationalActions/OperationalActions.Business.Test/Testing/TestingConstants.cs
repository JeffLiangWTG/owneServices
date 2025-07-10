using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	public static class TestingConstants
	{
		public const string DummyActionMethodWithGuiText = "e7d84dbc-367f-4778-bb87-1a48ab8314c2";
		public const string DummyActionMethodWithLargeGuiText = "4a121928-ceea-4f37-9e5d-5fb0bb094f14";
		public const string DummyActionMethodWithOversizedGuiText = "8f5e3cd2-8366-4e74-8de7-09fe6e5e8946";
		public const string DummyActionMethodWithoutGuiText = "2b696c51-0243-4ba2-a7c1-1a43b56de2f8";
		public const string DummyActionMethodRunWithoutUIText = "c602bd5b-ac5b-43ea-99fc-0abc95b61b91";
		public static readonly ZGuid DummyActionMethodWithGUI = new ZGuid(DummyActionMethodWithGuiText);
		public static readonly ZGuid DummyActionMethodWithLargeGUI = new ZGuid(DummyActionMethodWithLargeGuiText);
		public static readonly ZGuid DummyActionMethodWithOversizedGUI = new ZGuid(DummyActionMethodWithOversizedGuiText);
		public static readonly ZGuid DummyActionMethodWithoutGUI = new ZGuid(DummyActionMethodWithoutGuiText);
		public static readonly ZGuid DummyActionMethodRunWithoutUI = new ZGuid(DummyActionMethodRunWithoutUIText);
	}
}
