using System;
using System.Data;
using System.Text;
using System.Xml;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry
{
	public class CopyEnableAddAndEditLogsToEnableAddEditAndDeleteLogsItems : RegistryDataTransformation
	{
		public override string UserDescription => "Copy the customer data from the existing EnableAddAndEditLogs registry to the new EnableAddEditAndDeleteLogsItems registry item.";

		const string SourceRegistryItemName = "EnableAddAndEditLogs";

		const string DestinationRegistryItemName = "EnableAddEditAndDeleteLogsItems";

		protected override void OfflinePostUpgradeTransform()
		{
			using (var table = GetDataTable(SourceRegistryItemName))
			{
				Helper.DeleteStmDataRow(DestinationRegistryItemName);

				if (table.Rows.Count == 0)
				{
					return;
				}

				var originalBinaryValue = table.Rows[0].Field<byte[]>(StmDataSchema.Constants.SD_BinaryValue);
				if (originalBinaryValue != null)
				{
					var newBinaryValue = TransformBinaryValue(originalBinaryValue);
					Helper.InsertStmDataRow(DestinationRegistryItemName, "BIN", newBinaryValue);
				}
				Helper.DeleteStmDataRow(SourceRegistryItemName);
			}
		}

		byte[] TransformBinaryValue(byte[] originalValue)
		{
			var currentXML = new XmlDocument();
			try
			{
				currentXML.LoadXml(Encoding.Unicode.GetString(originalValue));
			}
			catch (Exception)
			{
				return null;
			}

			var enabled = currentXML
				.SelectSingleNode("ArrayOfCodeDescriptionBoolDisallowNewCodeReadOnly")
				.SelectSingleNode("CodeDescriptionBoolDisallowNewCodeReadOnly")
				.SelectSingleNode("Bool").InnerText;

			var newXML = new XmlDocument();

			var xmlDeclaration = currentXML.FirstChild as XmlDeclaration;
			if (xmlDeclaration != null)
			{
				var newXmlDeclaration = newXML.CreateXmlDeclaration(xmlDeclaration.Version, xmlDeclaration.Encoding, xmlDeclaration.Standalone);
				newXML.AppendChild(newXmlDeclaration);
			}

			var xmlNode = newXML.CreateElement("ArrayOfEnableAddEditAndDeleteLogsItem");
			newXML.AppendChild(xmlNode);

			var enableAddEditAndDeleteLogsItem = xmlNode.AppendChild(newXML.CreateElement("EnableAddEditAndDeleteLogsItem"));

			var table = newXML.CreateElement("Table");
			table.InnerText = ProcessTasksSchema.Constants.TableName;

			var enableADDLogs = newXML.CreateElement("EnableADDLogs");
			enableADDLogs.InnerText = enabled;

			var enableEDTLogs = newXML.CreateElement("EnableEDTLogs");
			enableEDTLogs.InnerText = enabled;

			var enableDELLogs = newXML.CreateElement("EnableDELLogs");
			enableDELLogs.InnerText = enabled;

			enableAddEditAndDeleteLogsItem.AppendChild(table);
			enableAddEditAndDeleteLogsItem.AppendChild(enableADDLogs);
			enableAddEditAndDeleteLogsItem.AppendChild(enableEDTLogs);
			enableAddEditAndDeleteLogsItem.AppendChild(enableDELLogs);

			foreach (XmlAttribute attribute in currentXML.DocumentElement.Attributes)
			{
				newXML.DocumentElement.SetAttribute(attribute.Name, attribute.Value);
			}

			return Encoding.Unicode.GetBytes(newXML.InnerXml);
		}
	}
}
