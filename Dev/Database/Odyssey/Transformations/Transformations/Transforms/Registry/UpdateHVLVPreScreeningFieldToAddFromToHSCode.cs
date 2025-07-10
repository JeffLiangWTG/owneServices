using System;
using System.Data;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry
{
	public class UpdateHVLVDetailsPreScreeningConfigurationRegistryItemToAddFromToHSCode : RegistryDataTransformation
	{
		public const string RegistryItemName = "HVLVDetailsPreScreeningConfiguration";

		public override string UserDescription => "Add FromHSCode and ToHSCode to HVLVDetailsPreScreeningConfiguration registry.";

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

				var userData = XDocument.Load(new MemoryStream(binaryValue));

				foreach (var hvlvPreScreeningValueElement in userData
					.Descendants("HVLVDetailsPreScreeningConfiguration")
					.Descendants("HVLVPreScreeningRule")
					.Descendants("HVLVPreScreeningField")
					.Descendants("HVLVPreScreeningValue"))
				{
					hvlvPreScreeningValueElement.SetElementValue("FromHSCode", "");
					hvlvPreScreeningValueElement.SetElementValue("ToHSCode", "");
				}

				var dataRowPk = (Guid)row[StmDataSchema.Constants.PK];
				UpdateDatabaseValue(dataRowPk, Encoding.Unicode.GetBytes(userData.ToString()));
			}
		}
	}
}
