namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class NZ
		{
			public interface ICusSCAContainerCollection
			{
				ICusSCAContainer this[int index] { get; }

				ICusSCAContainer AddNew();
			}
		}
	}
}
