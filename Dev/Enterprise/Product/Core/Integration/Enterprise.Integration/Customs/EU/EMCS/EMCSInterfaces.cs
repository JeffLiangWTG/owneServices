namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EUEMCS
		{
			public interface IJobComInvoiceLine : IBaseJobComInvoiceLine { }

			public interface IJobComInvoiceHeader : Shared.IBaseJobComInvoiceHeader { }

			public interface IJobComInvoiceGroupHeader : Shared.IBaseJobComInvoiceGroupHeader { }

			public interface ICusContainer : Shared.IBaseCusContainer { }

			public interface IInvoiceLineCusOutturn { }
		}
	}
}
