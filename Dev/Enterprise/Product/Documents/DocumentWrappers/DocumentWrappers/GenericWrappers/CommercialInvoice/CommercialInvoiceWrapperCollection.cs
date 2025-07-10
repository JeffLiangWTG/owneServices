using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class CommercialInvoiceWrapperCollection : GenericWrapperCollection<CommercialInvoiceWrapper>
	{
		public CommercialInvoiceWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CommercialInvoiceWrapperCollection(CommonShipment shipmentBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (shipmentBO != null)
			{
				BaseJobDeclaration declaration = (BaseJobDeclaration)shipmentBO.DeclarationForDocuments;
				if (declaration != null)
				{
					AddInvoicesFrom(declaration);
				}
			}
		}

		public CommercialInvoiceWrapperCollection(BaseJobDeclaration declaration, BusinessObjectFactory factory)
			: base(factory)
		{
			if (declaration != null)
			{
				AddInvoicesFrom(declaration);
			}
		}

		void AddInvoicesFrom(BaseJobDeclaration declaration)
		{
			foreach (BaseJobComInvoiceHeader invoiceHeader in declaration.Invoices)
			{
				Add(new CommercialInvoiceWrapper(invoiceHeader, Factory));
			}
		}
	}
}
