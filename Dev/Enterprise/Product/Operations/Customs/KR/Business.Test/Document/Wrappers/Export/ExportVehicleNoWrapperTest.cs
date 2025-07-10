using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ExportVehicleNoWrapper))]
	sealed class ExportVehicleNoWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var vehicleNo = new ExportVehicleNo();

			return new ExportVehicleNoWrapper(vehicleNo);
		}
	}
}
