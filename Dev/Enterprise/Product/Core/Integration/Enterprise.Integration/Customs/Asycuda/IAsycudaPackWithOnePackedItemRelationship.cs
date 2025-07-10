namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ASYCUDA
		{
			public interface IAsycudaPackWithOnePackedItemRelationship : IAsycudaPack
			{
				IAsycudaPackedItem PackedItem { get; }
			}
		}
	}
}