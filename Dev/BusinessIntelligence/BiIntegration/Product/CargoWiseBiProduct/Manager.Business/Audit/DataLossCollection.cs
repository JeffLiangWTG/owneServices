using CargoWise.EntityFramework;

namespace CargoWise.Bi.Product.Manager.Business
{
	public class DataLossCollection : NonPersistentBusinessObjectCollection<DataLoss>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DataLoss("", "");
		}
	}
}
