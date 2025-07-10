using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(PrintChargesBilledToLocalClientAtDestAsCollectControl))]
	internal class PrintChargesBilledToLocalClientAtDestAsCollectControlTest : RegistryZUserControlTestCase
	{
		#region Implementation
		protected override IBusiness GetNewBusinessEntity()
		{
			var collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
			AddSetting(collection, Core.Constants.TransportModes.Sea, ZString.Empty, ZString.Empty);
			AddSetting(collection, Core.Constants.TransportModes.Sea, Core.Constants.CountryCodes.Belgium, Core.Constants.CountryCodes.Australia);
			return collection;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((PrintChargesBilledToLocalClientAtDestAsCollectCollection)businessEntity).ReadOnly;
		}

		void AddSetting(PrintChargesBilledToLocalClientAtDestAsCollectCollection collection, string mode, ZString exportCountry, ZString importCountry)
		{
			var transportMode = collection.AddNew();
			transportMode.TransportMode = mode;
			transportMode.ExportCountry = exportCountry;
			transportMode.ImportCountry = importCountry;
		}

		#endregion
	}
}
