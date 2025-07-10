using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocNettingCurrencyCollection : DocumentWrapperCollection<DocNettingCurrency>
	{
		protected DocNettingCurrencyCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static DocNettingCurrencyCollection New(BusinessObjectFactory factory)
		{
			return new DocNettingCurrencyCollection(factory);
		}
	}
}
