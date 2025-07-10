using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ShipmentInspectionTypes))]
	sealed class ShipmentInspectionTypesTest : RegistryBusinessObjectTemplateTestCase<ShipmentInspectionTypes>
	{
		protected override ShipmentInspectionTypes GetBusinessObjectToClone()
		{
			var result = new ShipmentInspectionTypes(Constants.CountryCodes.Japan);
			ShipmentInspectionType type = result.Types.AddNew();
			type.Code = "TS1";
			type.Description = (NoResString)"Testing";
			type.AllowedOnPassengerFlights = false;
			type.ShowInList = true;

			return result;
		}

		protected override ShipmentInspectionTypes GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
