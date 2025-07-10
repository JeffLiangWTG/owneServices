namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class GB
		{
			public interface ITaxOnlyForPivotCollection
			{
				IGBTaxOnlyForPivot this[int index] { get; }

				IGBTaxOnlyForPivot AddNew();
			}
		}
	}
}
