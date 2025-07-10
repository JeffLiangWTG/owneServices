using System;
using System.Data;
using System.Text;
using System.Xml;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry
{
	public class UpdateEnableBoleroEBLAndEHBLIntegrationRegistryDataTransformation : RegistryDataTransformation
	{
		public override string UserDescription => "Update Enable Bolero eBL Integration registry items(Consolidation/Shipment)";

		public readonly string[] RegistryItemNames = { "EnableBoleroEBLIntegration", "EnableBoleroEHBLIntegration" };

		protected override void OfflinePostUpgradeTransform()
		{
			foreach (var registryItemName in RegistryItemNames)
			{
				var table = GetDataTable(registryItemName);
				foreach (DataRow row in table.Rows)
				{
					UpdateDatabaseValue(row);
				}
			}
		}

		void UpdateDatabaseValue(DataRow row)
		{
			var dataRowPk = (Guid)row[StmDataSchema.Constants.PK];
			var binaryValue = row[StmDataSchema.Constants.SD_BinaryValue] as byte[];
			var dataType = row.Field<string>(StmDataSchema.Constants.SD_Type);
			if (binaryValue != null && binaryValue.Length > 0 && dataType == "BIN")
			{
				var xmlString = Encoding.Unicode.GetString(binaryValue);

				try
				{
					var xmlDoc = new XmlDocument();
					xmlDoc.LoadXml(xmlString);

					var isChanged = false;

					var boleroEBLConfigurationNode = xmlDoc.SelectSingleNode("//BoleroEBLConfiguration");
					if (boleroEBLConfigurationNode == null)
					{
						xmlDoc.RemoveAll();
						xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-16", ""));

						boleroEBLConfigurationNode = xmlDoc.CreateElement("BoleroEBLConfiguration");
						xmlDoc.AppendChild(boleroEBLConfigurationNode);

						isChanged = true;
					}

					isChanged |= CreateElementOrSetDefaultValueForEmptyElement(xmlDoc, boleroEBLConfigurationNode, "EnableEBLIntegration", "N");
					isChanged |= CreateElementOrSetDefaultValueForEmptyElement(xmlDoc, boleroEBLConfigurationNode, "GalileoEndPointUrl", "https://galileo.boleroserve.net/galileo-portal/jwt/login");
					isChanged |= CreateElementOrSetDefaultValueForEmptyElement(xmlDoc, boleroEBLConfigurationNode, "GalileoAudience", "ab2e99f4-15c9-41c8-a138-7d873c3c4f9f");
					isChanged |= CreateElementOrSetDefaultValueForEmptyElement(xmlDoc, boleroEBLConfigurationNode, "GalileoTestEndPointUrl", "https://galileo.training.boleroserve.net/galileo-portal/jwt/login");
					isChanged |= CreateElementOrSetDefaultValueForEmptyElement(xmlDoc, boleroEBLConfigurationNode, "GalileoTestAudience", "ef46c619-92d6-4f56-af3d-327cf3646508");

					if (isChanged)
					{
						UpdateDatabaseValue(dataRowPk, "BIN", Encoding.Unicode.GetBytes(xmlDoc.OuterXml));
					}
				}
				catch (XmlException)
				{
					UpdateDatabaseValue(dataRowPk, "BIN", null);
				}
			}
		}

		bool CreateElementOrSetDefaultValueForEmptyElement(XmlDocument xmlDocument, XmlNode parentNode, string elementName, string defaultValue)
		{
			var node = parentNode.SelectSingleNode("//" + elementName);

			if (node == null)
			{
				node = xmlDocument.CreateElement(elementName);
				parentNode.AppendChild(node);
			}

			if (string.IsNullOrEmpty(node.InnerText))
			{
				node.InnerText = defaultValue;

				return true;
			}

			return false;
		}
	}
}
