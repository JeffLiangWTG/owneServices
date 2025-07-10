namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class GB
		{
			public interface ISupportingDocumentCollection
			{
				ISupportingDocument this[int index] { get; }

				ISupportingDocument AddNew();
			}
		}
	}
}
