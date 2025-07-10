using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Accounting;

public class UpdateRegistryImageRegistryItemClass : RegistryDataTransformation
{
	public override string UserDescription => "Update DocumentImages registry item by changing class in use from RegistryImage to SystemDefinableRegistryImage";

	const string RegistryItemName = "DocumentImages";

	protected override void OfflinePostUpgradeTransform()
	{
		var table = GetDataTable(RegistryItemName);
		foreach (DataRow row in table.Rows)
		{
			var dataRowPk = (Guid)row[StmDataSchema.Constants.PK];
			var binaryValue = row[StmDataSchema.Constants.SD_BinaryValue] as byte[];
			if (binaryValue != null && binaryValue.Length > 0)
			{
				var xmlString = Encoding.Unicode.GetString(binaryValue);
				if (!xmlString.Contains("<ArrayOfRegistryImage"))
				{
					continue; // Nothing to transform
				}

				try
				{
					var xmlDoc = new XmlDocument();
					xmlDoc.LoadXml(xmlString);

					var newDoc = new XmlDocument();
					newDoc.LoadXml(DefaultXml);

					var arrayNode = xmlDoc.SelectSingleNode("//ArrayOfRegistryImage");
					var newArrayNode = newDoc.SelectSingleNode("//ArrayOfSystemDefinableRegistryImage");
					foreach (var node in arrayNode.SelectNodes("//RegistryImage").Cast<XmlElement>().ToArray())
					{
						var newNode = newDoc.CreateElement("SystemDefinableRegistryImage");
						CopyElementContents(node, newNode);
						newArrayNode.AppendChild(newNode);
					}

					UpdateDatabaseValue(dataRowPk, "BIN", Encoding.Unicode.GetBytes(newDoc.OuterXml));
				}
				catch (XmlException)
				{
					ShowInfo($"Registry Entry for '{RegistryItemName}' is in an invalid XML format and will be removed.");
					UpdateDatabaseValue(dataRowPk, "BIN", null);
				}
			}
		}
	}

	const string DefaultXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfSystemDefinableRegistryImage xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"></ArrayOfSystemDefinableRegistryImage>";

	static void CopyElementContents(XmlElement source, XmlElement destination)
	{
		foreach (XmlNode node in source.ChildNodes)
		{
			if (node is XmlElement)
			{
				var childElement = (XmlElement)node;
				var newChildElement = destination.OwnerDocument.CreateElement(childElement.Name);
				newChildElement.InnerXml = childElement.InnerXml;
				destination.AppendChild(newChildElement);
			}
		}
	}
}
