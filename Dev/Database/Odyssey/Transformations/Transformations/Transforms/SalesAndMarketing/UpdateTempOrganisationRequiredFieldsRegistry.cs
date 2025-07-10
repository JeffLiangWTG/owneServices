using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.SalesAndMarketing
{
	public class UpdateTempOrganisationRequiredFieldsRegistry : RegistryDataTransformation
	{
		public override string UserDescription => "Update TempOrganisationRequiredFields registry item";

		const string RegistryItemName = "TempOrganisationRequiredFields";

		protected override void OfflinePostUpgradeTransform()
		{
			var table = GetDataTable(RegistryItemName);
			if (table.Rows.Count >= 1)
			{
				foreach (DataRow row in table.Rows)
				{
					var dataRowPk = (Guid)row[StmDataSchema.Constants.PK];
					var binaryValue = row[StmDataSchema.Constants.SD_BinaryValue] as byte[];
					if (binaryValue != null && binaryValue.Length > 0)
					{
						var xmlString = Encoding.Unicode.GetString(binaryValue);
						if (!xmlString.Contains("<ArrayOfCodeDescriptionBool"))
						{
							continue;
						}

						try
						{
							var xmlDoc = new XmlDocument();
							xmlDoc.LoadXml(xmlString);
							var newDoc = new XmlDocument();
							newDoc.LoadXml(defaultXml);

							var arrayNode = xmlDoc.SelectSingleNode("//ArrayOfCodeDescriptionBool");
							var newArrayNode = newDoc.SelectSingleNode("//ArrayOfGlowTempOrgRequiredField");

							foreach (var node in arrayNode.SelectNodes("//CodeDescriptionBool").Cast<XmlElement>().ToArray())
							{
								var newNode = newDoc.CreateElement("GlowTempOrgRequiredField");
								CopyElementContents(node, newNode);

								var isMandatoryNode = newDoc.CreateElement("IsMandatory");
								isMandatoryNode.InnerXml = "N";
								newNode.AppendChild(isMandatoryNode);
								newArrayNode.AppendChild(newNode);
							}

							UpdateDatabaseValue(dataRowPk, "BIN", Encoding.Unicode.GetBytes(newDoc.OuterXml));
						}
						catch (XmlException)
						{
							UpdateDatabaseValue(dataRowPk, "BIN", null);
						}
					}
				}
			}
		}

		const string defaultXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfGlowTempOrgRequiredField xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><GlowTempOrgRequiredField><CodeMaxLength>10</CodeMaxLength><Code>Name</Code><Description>Name</Description><Bool>Y</Bool><SystemDefined>True</SystemDefined><IsMandatory>Y</IsMandatory></GlowTempOrgRequiredField></ArrayOfGlowTempOrgRequiredField>";

		static void CopyElementContents(XmlElement source, XmlElement destination)
		{
			foreach (XmlNode node in source.ChildNodes)
			{
				if (node is XmlElement)
				{
					var childElement = (XmlElement)node;
					var newChildElement = destination.OwnerDocument.CreateElement(childElement.Name);

					if (node.Name.Equals("Description"))
					{
						newChildElement.InnerText = childElement.InnerText.Replace("Require ", "");
					}
					else if (node.Name.Equals("SystemDefined"))
					{
						newChildElement.InnerText = "True";
					}
					else
					{
						newChildElement.InnerXml = childElement.InnerXml;
					}

					destination.AppendChild(newChildElement);
				}
			}
		}
	}
}
