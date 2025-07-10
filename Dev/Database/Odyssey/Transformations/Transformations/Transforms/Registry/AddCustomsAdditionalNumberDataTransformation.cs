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
	public abstract class AddCustomsAdditionalNumberDataTransformation : RegistryDataTransformation
	{
		protected abstract string Code {  get; }
		protected abstract string Description { get; }
		protected abstract bool IsUnique { get; }
		protected abstract bool IsAutomation {  get; }

		protected override void OfflinePostUpgradeTransform()
		{
			using (var dataTable = GetDataTable(RegistryItemName))
			{
				foreach (DataRow dataRow in dataTable.Rows)
				{
					var pk = dataRow.Field<Guid>(StmDataSchema.Constants.PK);
					var originBinaryValue = dataRow.Field<byte[]>(StmDataSchema.Constants.SD_BinaryValue);
					if (originBinaryValue == null)
					{
						continue;
					}
					var updateCustomsAdditionalNumberBinaryValue = GetUpdatedNumberRowIfNecessary(originBinaryValue);
					if (updateCustomsAdditionalNumberBinaryValue != null)
					{
						UpdateDatabaseValue(pk, updateCustomsAdditionalNumberBinaryValue);
					}
				}
			}
		}

		byte[] GetUpdatedNumberRowIfNecessary(byte[] binaryValue)
		{
			var xDocument = XDocument.Load(new MemoryStream(binaryValue));
			var rootElement = xDocument.Root;
			if (rootElement?.Name == "CustomsReferenceNumberTypes")
			{
				var additionalNumber = rootElement.Descendants("CustomsReferenceNumberType").FirstOrDefault(element => element.Element("Code")?.Value == Code);
				if (additionalNumber == null)
				{
					var addNumber = new XElement("CustomsReferenceNumberType");
					addNumber.Add(new XElement("Code", Code));
					addNumber.Add(new XElement("Description", Description));
					addNumber.Add(new XElement("IsUnique", IsUnique ? "Y" : "N"));
					addNumber.Add(new XElement("IsAutomation", IsAutomation ? "Y" : "N"));
					rootElement.Add(addNumber);
				}
				else
				{
					additionalNumber.Element("IsUnique")?.SetValue(IsUnique ? "Y" : "N");
					additionalNumber.Element("IsAutomation")?.SetValue(IsAutomation ? "Y" : "N");
					additionalNumber.Element("Description")?.SetValue(Description);
				}
				var xmlStr = xDocument.ToString();
				return Encoding.Unicode.GetBytes(xmlStr);
			}

			return null;
		}

		public const string RegistryItemName = "CustomsAdditionalReferenceNumbers";
	}
}
