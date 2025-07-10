using CargoWise.EntityFramework;

namespace Enterprise.DocumentWrappers
{
	public class DocVoyageAccountDisbursementAmountCollection : DocBaseWrapperCollection<DocVoyageAccountDisbursementAmount>
	{
		public DocVoyageAccountDisbursementAmountCollection(BusinessObjectFactory factory)
			: base(factory) { }
	}
}
