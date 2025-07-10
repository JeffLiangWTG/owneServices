using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DocDeliveryContactCollection : MasterFiles.Business.DocDeliveryContactCollection
	{
		public DocDeliveryContactCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocDeliveryContactCollection(IStmMenuItem menuItem, IDocAddress overridenAddress, BusinessObjectFactory factory)
			: base(menuItem, overridenAddress, factory)
		{
		}

		public DocDeliveryContactCollection(IStmMenuItem menuItem, IDocAddress overridenAddress, BusinessObjectFactory factory, DocAutoDelivery docAutoDelivery)
			: base(menuItem, overridenAddress, factory, docAutoDelivery)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DocDeliveryContact(Factory);
		}
	}
}
