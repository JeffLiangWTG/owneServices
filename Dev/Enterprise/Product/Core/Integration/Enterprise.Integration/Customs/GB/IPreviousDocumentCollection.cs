namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class GB
		{
			public interface IPreviousDocumentCollection
			{
				IPreviousDocument this[int index] { get; }

				IPreviousDocument AddNew();
			}
		}
	}
}
