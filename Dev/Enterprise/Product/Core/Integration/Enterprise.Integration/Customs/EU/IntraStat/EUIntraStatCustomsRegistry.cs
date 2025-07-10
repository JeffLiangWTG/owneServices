namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EU
		{
			public interface IEUIntrastatCustomsRegistry
			{
				bool IsIntrastatEnabled { get; }
			}
		}
	}
}
