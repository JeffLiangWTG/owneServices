using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngine.Testing
{
	public class MockAutoDocumentDelivery : DocAutoDelivery
	{
		public override DocDeliveryContactCollection GetDeliveryContactsForDocPack(IStmMenuItem menuItem, DocumentSupporter deliveryFilter, ZString documentGroup, IStmMenuItem parentMenuCommand = null)
		{
			DocDeliveryContactCollection contacts = new DocDeliveryContactCollection(Factory);
			DocDeliveryContact lorenzo = contacts.AddNew();
			lorenzo.Name = "Lorenzo";
			lorenzo.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;

			return contacts;
		}

		public override DocDeliveryContact GetDeliveryDetailsForContact(OrgContact contact)
		{
			DocDeliveryContact jimmy = new DocDeliveryContact(new BusinessObjectFactory());
			jimmy.Name = "Jimmy";
			jimmy.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;

			return jimmy;
		}

		public override DocDeliveryContact GetDeliveryDetailsForContact(OrgContact contact, IStmMenuItem menuItem)
		{
			return GetDeliveryDetailsForContact(contact);
		}
	}
}
