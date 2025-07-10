using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransportBookings
{
	public class UpdateServiceTaskCreatorOptionValueFCLToCNT : RegistryDataTransformation
	{
		public override string UserDescription => "Update existing registry value (FCL) for Service Task Target Options to reflect the new value (CNT).";
		public const string RegistryName = "ServiceTaskCreatorOption";

		protected override void OfflinePostUpgradeTransform()
		{
			var selectRegistryItemSql = $"SELECT TOP 1 SD_PK, SD_Name, SD_BinaryValue FROM dbo.StmData WHERE SD_Name = '{RegistryName}' ORDER BY SD_SystemCreateTimeUtc";
			Guid itemPk;
			XDocument parsedXml;

			using (var command = Db.Connection.Command(selectRegistryItemSql))
			using (var reader = command.ExecuteReader())
			{
				var dataExists = reader.Read();

				if (!dataExists)
				{
					ShowInfo($"Registry Entry for '{RegistryName}' has no data attributed.");
					return;
				}

				try
				{
					itemPk = (Guid)reader[StmDataSchema.Constants.PK];

					var binaryValue = reader[StmDataSchema.Constants.SD_BinaryValue];
					if (binaryValue == DBNull.Value)
					{
						ShowInfo($"Registry Entry for '{RegistryName}' has a null value for it's binary value. No changes made.");
						return;
					}

					parsedXml = GetDocument((byte[])binaryValue);

					if (parsedXml.Root.Name != $"ArrayOf{RegistryName}")
					{
						ShowInfo($"Registry Entry for '{RegistryName}' has an unexpected layout for it's XML value. No changes made.");
						return;
					}
				}
				catch (XmlException)
				{
					ShowInfo($"Registry Entry for '{RegistryName}' is in an invalid XML format and was not updated.");
					return;
				}
			}

			var updateMade = UpdateValuesIfNeeded(parsedXml);
			if (updateMade)
			{
				SaveXmlToDb(itemPk, parsedXml);
			}
			else
			{
				ShowInfo($"Registry Entry for '{RegistryName}' was not modified as there was no changes needed to be made.");
			}
		}

		static XDocument GetDocument(byte[] data)
		{
			using (var memoryStream = new MemoryStream(data))
			{
				return XDocument.Load(memoryStream);
			}
		}

		bool UpdateValuesIfNeeded(XDocument xml)
		{
			var newContainerModeValue = "CNT";
			var newIsSystemDefinedValue = "N";
			var serviceTaskOptionToUpdate = GetServiceTaskOptionToUpdate(xml);

			var needToUpdate = serviceTaskOptionToUpdate != null;

			if (needToUpdate)
			{
				var containerMode = serviceTaskOptionToUpdate.Elements("ContainerMode").FirstOrDefault();
				var isSystemDefined = serviceTaskOptionToUpdate.Elements("IsSystemDefined").FirstOrDefault();

				containerMode.Value = newContainerModeValue;
				isSystemDefined.Value = newIsSystemDefinedValue;
			}
			else
			{
				ShowInfo($"Intended value to change in Registry Entry '{RegistryName}' not found.");
			}

			return needToUpdate;
		}

		XElement GetServiceTaskOptionToUpdate(XDocument xml)
		{
			var oldValue = "FCL";

			var serviceTaskCreatorOptions = xml.Root.Elements();

			var serviceTaskOptionToUpdate = serviceTaskCreatorOptions.FirstOrDefault(e => e.Elements().Any(i => i.Name == "ContainerMode" && i.Value == oldValue));

			return serviceTaskOptionToUpdate;
		}

		void SaveXmlToDb(Guid pk, XDocument xml)
		{
			using (var cmd = Db.Connection.Command("UPDATE dbo.StmData SET SD_BinaryValue = @SD_BinaryValue, SD_SystemLastEditUser = '~BP', SD_SystemLastEditTimeUtc = GETUTCDATE() WHERE SD_PK = @SD_PK"))
			using (var ms = new MemoryStream())
			{
				xml.Save(ms, SaveOptions.DisableFormatting);
				ms.Position = 0;
				cmd.AddParameterBasedOnDbColumn("@SD_BinaryValue", ms.ToByteArray(), StmDataSchema.SD_BinaryValue);
				cmd.AddParameterBasedOnDbColumn("@SD_PK", pk, StmDataSchema.PK);

				cmd.ExecuteNonQuery();
			}
		}
	}
}
