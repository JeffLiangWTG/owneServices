using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	sealed class DocBaseJobComInvoiceGroupHeaderTestClass : DocBaseJobComInvoiceGroupHeader
	{
		DocBaseJobComInvoiceGroupHeaderTestClass(BaseJobComInvoiceGroupHeader groupHeader, BusinessObjectFactory factoryToWrap)
			: base(groupHeader, factoryToWrap)
		{
		}

		public static DocBaseJobComInvoiceGroupHeaderTestClass New(BaseJobComInvoiceGroupHeader groupHeader, BusinessObjectFactory factoryToWrap)
		{
			if (groupHeader == null)
			{
				return null;
			}
			else
			{ return new DocBaseJobComInvoiceGroupHeaderTestClass(groupHeader, factoryToWrap); }
		}

		public DocBaseJobComInvoiceHeaderCollection InvoiceHeadersInternalTestMethod
		{
			get { return base.InvoiceHeadersInternal; }
		}

		protected override DocBaseJobComInvoiceHeaderCollection CreateJobComInvoiceHeaderCollection(InvoiceHeaderActiveCollection collectionToWrap)
		{
			return new DocBaseJobComInvoiceHeaderCollectionTestClass(collectionToWrap, Factory);
		}
	}
}
