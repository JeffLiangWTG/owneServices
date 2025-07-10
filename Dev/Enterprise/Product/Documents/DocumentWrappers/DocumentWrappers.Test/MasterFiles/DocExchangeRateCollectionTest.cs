using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocExchangeRateCollection))]
	public class DocExchangeRateCollectionTest : DocBaseWrapperCollectionTest<DocExchangeRateCollection>
	{
		#region Implementation

		protected override DocExchangeRateCollection GetNewDocumentWrapperCollection()
		{
			return new DocExchangeRateCollection(Factory);
		}

		protected override object GetNewObjectToWrap()
		{
			return Factory.NewJobForTesting<Job>().ExchangeRates.AddNew();
		}

		#endregion
	}
}
