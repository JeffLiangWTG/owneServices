using System;
using System.Data;
using System.Text;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Accounting
{
	public class UpdateComplianceNumberAllocationDateRegistryItem : RegistryDataTransformation
	{
		public override string UserDescription => "Copy registry 'Compliance Number Allocation Date' values into 'Compliance Number Allocation Date - Payable' registry item";

		protected override void OfflinePostUpgradeTransform()
		{
			CopyExistingRegistryRowsIntoNewRegistryForAP();
			UpdateRegistryItemName(ExistingRegistry, NewRegistryForAR);
		}

		void CopyExistingRegistryRowsIntoNewRegistryForAP()
		{
			foreach (DataRow row in GetDataTable(ExistingRegistry).Rows)
			{
				var binaryValue = row[StmDataSchema.Constants.SD_BinaryValue] as byte[];
				if (binaryValue != null && binaryValue.Length > 0)
				{
					var complianceNumberAllocationDateOption = Encoding.Unicode.GetString(binaryValue).Trim().Trim(new char[] { '\uFEFF', '\u200B' });
					if (complianceNumberAllocationDateOption == "PST")
					{
						UpdateOrCreateDatabaseValue(NewRegistryForAP, (Guid)row[StmDataSchema.Constants.SD_Owner], null, "STR", false, binaryValue, null, false, false);
					}
				}
			}
		}

		const string ExistingRegistry = "ComplianceNumberAllocationDate";
		const string NewRegistryForAP = "ComplianceNumberAllocationDate_AP";
		const string NewRegistryForAR = "ComplianceNumberAllocationDate_AR";
	}
}
