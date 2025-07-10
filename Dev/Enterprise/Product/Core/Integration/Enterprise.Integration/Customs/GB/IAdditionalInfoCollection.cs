namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class GB
		{
			public interface IAdditionalInfoCollection
			{
				IAdditionalInfo this[int index] { get; }

				IAdditionalInfo AddNew();
			}
		}
	}
}
