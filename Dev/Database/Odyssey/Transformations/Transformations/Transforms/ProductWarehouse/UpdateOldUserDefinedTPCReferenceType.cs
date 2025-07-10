using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class UpdateOldUserDefinedTPCReferenceType : RegistryDataTransformation
	{
		public override string UserDescription => "Update old TPC type in WarehouseDocketReferenceType registry item";

		public const string RegistryItemName = "WarehouseDocketReferenceType";

		protected override void OfflinePostUpgradeTransform()
		{
			var table = GetDataTable(RegistryItemName);
			var isTPCReferenceTypeReplaced = false;
			var replaceCode = "";

			if (table.Rows.Count > 0)
			{
				var row = table.Rows[0];
				var dataRowPk = (Guid)row[StmDataSchema.Constants.PK];

				if (row[StmDataSchema.Constants.SD_BinaryValue] is byte[] binaryValue && binaryValue.Length > 0)
				{
					var xmlString = Encoding.Unicode.GetString(binaryValue);
					try
					{
						var xmlDoc = new XmlDocument();
						xmlDoc.LoadXml(xmlString);

						var referenceTypeNodes = xmlDoc.SelectNodes("//Code").Cast<XmlNode>().ToArray();
						var oldTPCNode = referenceTypeNodes.FirstOrDefault(node => node.InnerText == "TPC");
						if (oldTPCNode != null)
						{
							var existingCodeList = referenceTypeNodes.Select(node => node.InnerText).Distinct().ToList();
							replaceCode = FindNewAppropriateAdditionalReferenceTypeCode(existingCodeList);
							if (!string.IsNullOrEmpty(replaceCode))
							{
								oldTPCNode.InnerText = replaceCode;
								UpdateDatabaseValue(dataRowPk, "BIN", Encoding.Unicode.GetBytes(xmlDoc.OuterXml));
								isTPCReferenceTypeReplaced = true;
							}
						}
					}
					catch (XmlException)
					{
						UpdateDatabaseValue(dataRowPk, "BIN", binaryValue);
					}
				}
			}

			if (isTPCReferenceTypeReplaced)
			{
				var sql = $@"
UPDATE dbo.WhsDocketReference 
SET
	WX_RefType = '{replaceCode}',
	WX_SystemLastEditTimeUtc = GetUtcDate(),
	WX_SystemLastEditUser = '~BP'
WHERE WX_RefType = 'TPC';
";
				Db.Connection.ExecuteNonQuery(sql);
			}
		}

		string FindNewAppropriateAdditionalReferenceTypeCode(List<string> existingCodeList)
		{
			return Enumerable.Range(1, 999).Select(i => i.ToString("D3")).FirstOrDefault(code => !existingCodeList.Contains(code));
		}
	}
}
