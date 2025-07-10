namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Implement this on a FindBox list.
	/// When a new form is shown from the findbox, AddRelationshipToNewObject
	/// will be called. This allows you to set up any custom relationships between 
	/// the two forms' BusinessObjects. Eg, Pop up shipment form from the
	/// consol form. Using this interface, you can do something like:
	/// ShipmentOnNewForm.Consols.Add(ConsolFromThisForm);
	/// </summary>
	public interface IRelationshipAdderForController
	{
		void AddRelationshipToNewObject(BusinessObject newBusinessObject);
	}
}
