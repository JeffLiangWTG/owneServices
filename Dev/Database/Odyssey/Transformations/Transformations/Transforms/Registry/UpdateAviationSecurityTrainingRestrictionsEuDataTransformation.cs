using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry
{
	public class UpdateAviationSecurityTrainingRestrictionsEuDataTransformation : RegistryDataTransformation
	{
		public override string UserDescription => @"Updating value of AviationSecurityTrainingRestrictions_EU registry item to XML format";

		protected override void OfflinePostUpgradeTransform()
		{
			const string registryItemName = "AviationSecurityTrainingRestrictions_EU";
			var helper = new RegistryTransformationHelper();
			var originalBinaryValue = helper.GetStmDataValue(registryItemName);

			if (originalBinaryValue == null)
			{
				return;
			}
			var newBinaryValue = TransformBinaryValue(originalBinaryValue);

			var updateSqlQuery = $@"UPDATE {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}
						SET {StmDataSchema.Constants.SD_BinaryValue} = @SD_BinaryValue, {StmDataSchema.Constants.SD_Type} = @SD_Type
						WHERE {StmDataSchema.Constants.SD_Name} = @SD_Name;";

			using (var cmd = Db.Connection.Command(updateSqlQuery))
			{
				cmd.AddParameterBasedOnDbColumn("@SD_Name", registryItemName, StmDataSchema.SD_Name);
				cmd.AddParameterBasedOnDbColumn("@SD_BinaryValue", newBinaryValue, StmDataSchema.SD_BinaryValue);
				cmd.AddParameterBasedOnDbColumn("@SD_Type", "BIN", StmDataSchema.SD_Type);
				cmd.ExecuteNonQuery();
			}
		}

		byte[] TransformBinaryValue(byte[] originalValue)
		{
			var unCompressData = Compressor.Uncompress(originalValue);
			var rawText = Encoding.UTF8.GetString(unCompressData);

			if (rawText.StartsWith("T"))
			{
				return GetBytes(XDocument.Parse(EnableAviationSecurityTrainingRestriction));
			}
			else if (rawText.StartsWith("F"))
			{
				return GetBytes(XDocument.Parse(DisableAviationSecurityTrainingRestriction));
			}
			else
			{
				return originalValue;
			}
		}

		byte[] GetBytes(XDocument document)
		{
			Encoding utf16Encoding = new UnicodeEncoding(bigEndian: false, byteOrderMark: false);
			var xmlWriterSettings = new XmlWriterSettings
			{
				Encoding = utf16Encoding,
				OmitXmlDeclaration = false,
				Indent = false
			};

			using (var memoryStream = new MemoryStream())
			using (var xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings))
			{
				document.Save(xmlWriter);
				xmlWriter.Flush();
				memoryStream.Seek(0, SeekOrigin.Begin);
				return memoryStream.ToArray();
			}
		}

		const string EnableAviationSecurityTrainingRestriction = @"<AviationSecurityTrainingRestriction>
																	 <Enabled>Y</Enabled>
																	 <ApplyCertificationRestriction>N</ApplyCertificationRestriction>
																  </AviationSecurityTrainingRestriction>";

		const string DisableAviationSecurityTrainingRestriction = @"<AviationSecurityTrainingRestriction>
																	 <Enabled>N</Enabled>
																	 <ApplyCertificationRestriction>N</ApplyCertificationRestriction>
																  </AviationSecurityTrainingRestriction>";
	}
}
