using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using BusinessObject = CargoWise.EntityFramework.BusinessObject;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DummyDocumentWrapperWithDeliveryContact : DummyDocumentWrapper
	{
		public DummyDocumentWrapperWithDeliveryContact(BusinessObject bizToWrap, BusinessObjectFactory factoryToWrap)
			: base(bizToWrap, factoryToWrap)
		{
		}

		public override BusinessObject DeliveryContact
		{
			get
			{
				if (deliveryContact == null)
				{
					OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
					deliveryContact = new DocAutoDelivery().GetDeliveryDetailsForContact(contact);
				}
				return deliveryContact;
			}
		}
		DocDeliveryContact deliveryContact;
	}
}
