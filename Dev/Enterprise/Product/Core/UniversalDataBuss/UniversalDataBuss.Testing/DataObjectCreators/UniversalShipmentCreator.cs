using System.Linq;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Testing
{
	public static class UniversalShipmentCreator
	{
		public static Shipment Create(IDataContextDataObject dataContext)
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
			};
			return shipment;
		}

		public static void AddValidationRules(Shipment shipment, params ValidationRule[] rules)
		{
			shipment.SetValidationRuleCollection(() => rules.ToList());
		}
	}
}
