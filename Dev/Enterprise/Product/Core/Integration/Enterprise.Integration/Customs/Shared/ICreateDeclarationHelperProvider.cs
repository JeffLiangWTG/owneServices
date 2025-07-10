namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface ICreateDeclarationHelperProvider
			{
				ICreateDeclarationHelper NewCreateDeclarationHelper(string countryCode);
			}
		}
	}
}
