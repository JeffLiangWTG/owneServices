using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IInvoiceHeaderActiveCollection : IBusinessObjectCollection
		{
			new Shared.IBaseJobComInvoiceHeader AddNew();
		}
	}
}
