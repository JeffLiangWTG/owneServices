using System;
using System.Data;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry
{
	public class UpdateDefaultValueOfSystemInEventVisibilityOverrideRegistryItem : RegistryDataTransformation
	{
		public const string RegistryItemName = "EventVisibilityOverride";

		public override string UserDescription => "Add System to EventVisibilityOverride registry.";

		protected override void OfflinePostUpgradeTransform()
		{
			if (new RegistryTransformationHelper().GetStmDataRowCount(RegistryItemName) == 0)
			{
				return;
			}

			foreach (DataRow row in GetDataTable(RegistryItemName).Rows)
			{
				if (row[StmDataSchema.Constants.SD_BinaryValue] is not byte[] binaryValue || binaryValue.Length == 0)
				{
					continue;
				}

				using (var ms = new MemoryStream(binaryValue))
				{
					var userData = XDocument.Load(ms);
					var rootElement = userData.Root;

					if (rootElement?.Name == "ArrayOfEventVisibilityOverride")
					{
						var updated = false;
						foreach (var eventVisibilityOverrideSetting in rootElement.XPathSelectElements(
							"//EventVisibilityOverride/ArrayOfEventVisibilityOverrideSetting/EventVisibilityOverrideSetting"))
						{
							eventVisibilityOverrideSetting.SetElementValue("IsSystem", "N");
							updated = true;
						}

						if (updated)
						{
							var dataRowPk = (Guid)row[StmDataSchema.Constants.PK];
							UpdateDatabaseValue(dataRowPk, Encoding.Unicode.GetBytes(userData.ToString()));
						}
					}
				}
			}
		}
	}
}
