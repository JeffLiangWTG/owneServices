using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PrintChargesBilledToLocalClientAtDestAsCollectCollection))]
	internal class PrintChargesBilledToLocalClientAtDestAsCollectCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<PrintChargesBilledToLocalClientAtDestAsCollectCollection>
	{
		public void TestPerformPostCloneAction()
		{
			var transportMode = Collection.AddNew();
			transportMode.TransportMode = Core.Constants.TransportModes.Sea;
			transportMode.ExportCountry = Core.Constants.CountryCodes.Belgium;
			transportMode.ImportCountry = Core.Constants.CountryCodes.Australia;

			var collection = Collection.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory) as PrintChargesBilledToLocalClientAtDestAsCollectCollection;
			AssertEquals(Core.Constants.TransportModes.Sea, string.Join(",", collection.Cast<PrintChargesBilledToLocalClientAtDestAsCollect>().Select(x => x.TransportMode)));
			AssertEquals(Core.Constants.CountryCodes.Belgium, string.Join(",", collection.Cast<PrintChargesBilledToLocalClientAtDestAsCollect>().Select(x => x.ExportCountry)));
			AssertEquals(Core.Constants.CountryCodes.Australia, string.Join(",", collection.Cast<PrintChargesBilledToLocalClientAtDestAsCollect>().Select(x => x.ImportCountry)));
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		protected override PrintChargesBilledToLocalClientAtDestAsCollectCollection GetCollectionToTest()
		{
			return new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PrintChargesBilledToLocalClientAtDestAsCollect();
		}

		#endregion
	}
}
