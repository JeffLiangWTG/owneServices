using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry
{
	public class AddDefaultIncidentClosedEmailTemplate : RegistryDataTransformation
	{
		public override string UserDescription => "Add new default incident closed email template with code = NEV for CustomerServiceIncidentClosedNotificationEmailTemplates registry item";

		const string RegistryItemName = "CustomerServiceIncidentClosedNotificationEmailTemplates";

		protected override void OfflinePostUpgradeTransform()
		{
			using (var table = GetDataTable(RegistryItemName))
			{
				foreach (DataRow row in table.Rows)
				{
					var pk = row.Field<Guid>(StmDataSchema.Constants.PK);
					var originalBinaryValue = row.Field<byte[]>(StmDataSchema.Constants.SD_BinaryValue);

					if (originalBinaryValue == null)
					{
						continue;
					}

					var newBinaryValue = TransformBinaryValue(originalBinaryValue);
					UpdateDatabaseValue(pk, newBinaryValue);
				}
			}
		}

		byte[] TransformBinaryValue(byte[] originalValue)
		{
			var parsedXml = GetDocument(originalValue);
			if (parsedXml.Root.Name != "ArrayOfCodeDescriptionIncidentEmailTemplatePair")
			{
				return originalValue;
			}

			var alreadyHaveNewTemplate = parsedXml.Root
							.Elements("CodeDescriptionIncidentEmailTemplatePair")
							.Any(c => c.Elements("Code").SingleOrDefault()?.Value == "NEV");

			if (alreadyHaveNewTemplate)
			{
				return originalValue;
			}

			var defaultTemplateNode = parsedXml.Root
						.Elements("CodeDescriptionIncidentEmailTemplatePair")
						.FirstOrDefault(c => c.Elements("Code").SingleOrDefault()?.Value == "DFT");

			if (defaultTemplateNode == null)
			{
				return originalValue;
			}

			parsedXml.Root.Add(CreateNewEmailTemplate(defaultTemplateNode));
			return Encoding.Unicode.GetBytes(parsedXml.ToString());
		}

		XElement CreateNewEmailTemplate(XElement defaultTemplateNode)
		{
			var categorisedWorkflowTaskTypes = new XElement("CodeDescriptionIncidentEmailTemplatePair");

			var incidentEmailTemplatePair = defaultTemplateNode.Elements("IncidentEmailTemplatePair").FirstOrDefault();

			categorisedWorkflowTaskTypes.Add(
				new XElement("CodeMaxLength") { Value = "3" },
				new XElement("Code") { Value = "NEV" },
				new XElement("Description") { Value = "Do Not Reopen Default Close Notification Email Template" },
				new XElement("Product"),
				incidentEmailTemplatePair);

			return categorisedWorkflowTaskTypes;
		}

		static XDocument GetDocument(byte[] data)
		{
			using (var memoryStream = new MemoryStream(data))
			{
				return XDocument.Load(memoryStream);
			}
		}
	}
}
