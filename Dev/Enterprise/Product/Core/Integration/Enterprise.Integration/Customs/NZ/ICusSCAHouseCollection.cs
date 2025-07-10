namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class NZ
		{
			public interface ICusSCAHouseCollection
			{
				ICusSCAHouse this[int index] { get; }

				int Count { get; }
			}
		}
	}
}
