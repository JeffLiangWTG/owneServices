using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	/// <summary>
	/// Used in conjuntion with EventDatePropertyAttribute.
	/// Some BusinessObjects handle logs for properties on other related objects.
	///		Ie JobShipment handles JobDocsAndCartage events.
	///	Example: Adding a DeliveryCartageCompleteFinalised event to a shipment should update JobDocsAndCartage.JP_DeliveryCartageCompleted.
	/// </summary>
	public interface IHandleEventsForOtherObjects
	{
		BusinessObject[] GetHandledObjects();
	}
}
