using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocDetentionInvoice : DocBaseWrapper
	{
		DocDetentionInvoice(ContainerDetention detention, BusinessObjectFactory factory)
			: base(detention, factory) { }

		public static DocDetentionInvoice New(ContainerDetention detention, BusinessObjectFactory factory)
		{
			return detention == null ? null : new DocDetentionInvoice(detention, factory);
		}
	}
}
