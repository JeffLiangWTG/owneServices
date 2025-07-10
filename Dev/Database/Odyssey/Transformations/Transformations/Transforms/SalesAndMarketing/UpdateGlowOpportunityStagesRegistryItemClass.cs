using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.SalesAndMarketing
{
	public class UpdateGlowOpportunityStagesRegistryItemClass : RegistryDataTransformation
	{
		public override string UserDescription => "Update GlowOpportunityStages registry item by changing Class in use";

		const string RegistryItemName = "GlowOpportunityStages";

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
							continue; // Nothing to transform
						}

						try
						{
							var xmlDoc = new XmlDocument();
							xmlDoc.LoadXml(xmlString);
							var newDoc = new XmlDocument();
							newDoc.LoadXml(defaultXml);

							var arrayNode = xmlDoc.SelectSingleNode("//ArrayOfCodeDescriptionBool");
							var newArrayNode = newDoc.SelectSingleNode("//ArrayOfGlowOpportunityStage");
							foreach (var node in arrayNode.SelectNodes("//CodeDescriptionBool").Cast<XmlElement>().ToArray())
							{
								var newNode = newDoc.CreateElement("GlowOpportunityStage");
								CopyElementContents(node, newNode);
								var winProbabilityNode = newDoc.CreateElement("WinProbability");
								winProbabilityNode.InnerXml = "0";
								newNode.AppendChild(winProbabilityNode);
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

		const string defaultXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfGlowOpportunityStage xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"></ArrayOfGlowOpportunityStage>";

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
}
