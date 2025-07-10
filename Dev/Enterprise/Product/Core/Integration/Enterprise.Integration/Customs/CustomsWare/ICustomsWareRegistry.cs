namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class CustomsWare
		{
			public interface ICustomsWareRegistry
			{
				IRegistryItem CustomsWareSiteID { get; }
				IRegistryItem UserName { get; }
				IRegistryItem Password { get; }
				bool SubmitOutOfLine { get; }
			}
		}
	}
}
