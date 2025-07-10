using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI
{
	class EDIUpdateUsageBillingSettingsRegistryTransform : RegistryDataTransformation
	{
		public override string UserDescription => "Add 'RawUsageCategory' column to UsageBillingSettingsRegistryItem";

		const string RegistryName = "UsageBillingSettings";
		const string NewElementName = "RawUsageCategory";

		protected override void OfflinePostUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, "EdiBilledUsage"))
			{
				var helper = new RegistryTransformationHelper();
				var registryValue = helper.GetStmDataValue(RegistryName);
				if (registryValue != null && registryValue.Any())
				{
					var xmlString = Encoding.Unicode.GetString(registryValue);
					var newXmlString = AddRawUsageCategory(xmlString);
					if (!string.IsNullOrWhiteSpace(newXmlString))
					{
						UpdateDatabaseValue(RegistryName, Encoding.Unicode.GetBytes(newXmlString));
					}
				}
			}
		}

		static string AddRawUsageCategory(string xmlString)
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xmlString);

			if (xmlDoc.GetElementsByTagName(NewElementName).Count == 0)
			{
				foreach (XmlNode productCodeNode in xmlDoc.GetElementsByTagName("ProductCode"))
				{
					var rawUsageCategoryElement = xmlDoc.CreateElement(NewElementName);
					rawUsageCategoryElement.InnerText = productCodeNode.InnerText;
					productCodeNode.ParentNode.InsertAfter(rawUsageCategoryElement, productCodeNode);
				}
				return xmlDoc.OuterXml;
			}
			else
			{
				return null;
			}
		}
	}
}
