using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocExchangeRateCollection : DocBaseWrapperCollection<DocExchangeRate>
	{
		public DocExchangeRateCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public DocExchangeRateCollection(ExchangeRatesCollection collection, BusinessObjectFactory factory)
			: base(collection, factory) { }

		protected override DocumentWrapper WrapObject(object objectToWrap)
		{
			return DocExchangeRate.New((ExchangeRate)objectToWrap, Factory);
		}
	}
}
