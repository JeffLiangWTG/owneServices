using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.MasterDataComplianceWise
{
	sealed class CopyCommodityRiskCheckBypassFeatureToCommodityRiskAssessmentDecisionRegistryItem : RegistryDataTransformation
	{
		public override string UserDescription => "Copy CommodityRiskCheckBypass feature  to CommodityRiskAssessmentDecision registry item";

		protected override void OfflinePostUpgradeTransform()
		{
			var existingRegistryTable = GetDataTableFull(ExistingRegistryItemName);
			var isCommodityRiskCheckBypassEnabled = false;

			if (existingRegistryTable.Rows.Count != 1)
			{
				return;
			}

			var binaryValue = existingRegistryTable.Rows[0][StmDataSchema.Constants.SD_BinaryValue] as byte[];
			if (binaryValue == null)
			{
				return;
			}

			var encodedValue = Encoding.Unicode.GetString(binaryValue);
			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var compressedAndEncodedString = encoder.Decrypt(encodedValue);
			var xmlValue = DecodeAndDecompressString(compressedAndEncodedString);
			var featureValue = GetFeatureControlValue(xmlValue, "MDMCPWCRC");

			if (featureValue == null)
			{
				return;
			}

			isCommodityRiskCheckBypassEnabled = ParseEnabledParameterJson(featureValue);

			if (!isCommodityRiskCheckBypassEnabled)
			{
				return;
			}

			var registryBinaryValue = Encoding.Unicode.GetBytes("False");
			UpdateOrCreateDatabaseValue(NewRegistryItemName, null, null, "BOL", true, registryBinaryValue, null, false, false);
			AddStmALog(NewRegistryItemName);
		}

		void AddStmALog(string registryItemName)
		{
			var sql = @"-- AddStmALogForRegistryItem
DECLARE @selected table(
	SD_PK uniqueidentifier,
	SD_BinaryValue varbinary(max)
);

INSERT @Selected(SD_PK, SD_BinaryValue)
SELECT SD_PK, SD_BinaryValue
FROM dbo.StmData
WHERE SD_Name = @SD_Name

INSERT INTO dbo.StmALog (SL_Table, SL_Parent, SL_Reference, SL_SE_NKEvent, SL_EventTime, SL_GS_NKUser, SL_EventTimeUtc)
SELECT 'StmData', selected.SD_PK, SUBSTRING(cast(selected.SD_BinaryValue as nvarchar(max)), 0, 1023), 'EDT', getDate(), '~BP', getUtcDate()
FROM @Selected as selected;";

			using var cmd = Db.Connection.Command(sql);
			cmd.AddParameterBasedOnDbColumn("@SD_Name", registryItemName, StmDataSchema.SD_Name);
			cmd.ExecuteNonQuery();
		}

		public static string DecodeAndDecompressString(string compressedAndEncodedString)
		{
			var compressedBytes = Convert.FromBase64String(compressedAndEncodedString);
			using var ms = new MemoryStream(compressedBytes);
			using var zipStream = new GZipStream(ms, CompressionMode.Decompress);
			using var resultStream = new MemoryStream();
			zipStream.CopyTo(resultStream);
			return Encoding.UTF8.GetString(resultStream.ToArray());
		}

		public static string GetFeatureControlValue(string xml, string featureControlCode)
		{
			var doc = XDocument.Parse(xml);
			var fcrParameters = doc.Descendants()
				.Where(x => x.Name.LocalName == "Rule" && (string)x.Element(x.Name.Namespace + "FCM_FeatureControlCode") == featureControlCode)
				.Select(x => (string)x.Element(x.Name.Namespace + "FCR_Parameters"))
				.FirstOrDefault();
			return fcrParameters;
		}

		public static bool ParseEnabledParameterJson(string json)
		{
			var jsonDocument = JsonDocument.Parse(json);
			var root = jsonDocument.RootElement;
			return root.GetProperty("Enabled").GetBoolean();
		}

		public const string ExistingRegistryItemName = "FeatureControlRuleContent";
		public const string NewRegistryItemName = "AllowComplianceCommodityRiskAssessment";
	}
}
