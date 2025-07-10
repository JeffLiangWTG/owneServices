using System;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.OceanCarrier;

class RenameCarrierShipmentToCarrierShipmentHeaderInGlowUseIndexingForModuleList : DataTransformation
{
	// see Enterprise/Architecture/Core/Core/Environment/Registry/DataTypesAndValidators/DelimitedStringArrayRegistryDataType.cs
	const string Separator = "◄◘►";

	public override string UserDescription => "Renames the registry configuration from carrierShipment to CarrierShipmentHeader for registry setting GlowUseIndexingForModuleList";

	protected override void OfflinePreUpgradeTransform()
	{
		// no row -> null
		// no SD_BinaryValue -> DBNull.Value
		if (Db.Connection.ExecuteScalar("SELECT SD_BinaryValue FROM dbo.StmData WHERE SD_Name = 'GlowUseIndexingForModuleList'") is not byte[] registrySetting)
		{
			return;
		}

		var settingCollection = Encoding.Unicode.GetString(registrySetting)
			.Split([Separator,], StringSplitOptions.RemoveEmptyEntries)
			.ToList();

		if (settingCollection.RemoveAll(setting => setting == "CarrierShipment") <= 0)
		{
			return;
		}

		settingCollection.Add("CarrierShipmentHeader");
		Db.Connection.ExecuteNonQuery("UPDATE dbo.StmData SET SD_BinaryValue = @binaryParameter WHERE SD_Name = 'GlowUseIndexingForModuleList'",
			cmd => cmd.AddParameter("@binaryParameter", SqlDbType.Binary, Encoding.Unicode.GetBytes(string.Join(Separator, settingCollection))));
	}
}
