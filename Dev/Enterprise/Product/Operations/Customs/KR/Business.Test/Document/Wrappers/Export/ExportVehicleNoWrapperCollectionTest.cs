using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ExportVehicleNoWrapperCollection))]
	sealed class ExportVehicleNoWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExportVehicleNoWrapperCollection>
	{
		protected override ExportVehicleNoWrapperCollection GetCollectionToTest() => new ExportVehicleNoWrapperCollection(Enumerable.Empty<IExportEntryLine>());

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ExportVehicleNoWrapper(new ExportVehicleNo());
	}
}
