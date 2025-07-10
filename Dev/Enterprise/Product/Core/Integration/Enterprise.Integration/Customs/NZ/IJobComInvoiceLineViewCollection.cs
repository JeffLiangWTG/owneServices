namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class NZ
		{
			public interface IJobComInvoiceLineViewCollection
			{
				IJobComInvoiceLine this[int index] { get; }

				int Count { get; }
			}
		}
	}
}
