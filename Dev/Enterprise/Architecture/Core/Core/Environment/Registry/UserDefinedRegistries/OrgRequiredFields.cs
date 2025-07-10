using System.Data;
using System.IO;

namespace Enterprise.ZArchitecture.Environment
{
	public struct OrgRequiredFields
	{
		public readonly bool RequireAddress2;
		public readonly bool RequireBranch;
		public readonly bool RequireCity;
		public readonly bool RequirePhoneNumber;
		public readonly bool RequireBusinessNumber;
		public readonly bool RequirePhoneOrBusinessNumber;
		public readonly bool RequireFaxNumber;
		public readonly bool RequireEmailAddress;
		public readonly bool RequireWebAddress;
		public readonly bool RequireFaxEmailOrWeb;
		public readonly bool RequireARContact;

		public OrgRequiredFields(bool requireAddress2, bool requireBranch, bool requireCity, bool requirePhoneNumber, bool requireBusinessNumber, bool requirePhoneOrBusinessNumber, bool requireFaxNumber, bool requireEmailAddress, bool requireWebAddress, bool requireFaxEmailOrWeb, bool requireArContact)
		{
			RequireAddress2 = requireAddress2;
			RequireBranch = requireBranch;
			RequireCity = requireCity;
			RequirePhoneNumber = requirePhoneNumber;
			RequireBusinessNumber = requireBusinessNumber;
			RequirePhoneOrBusinessNumber = requirePhoneOrBusinessNumber;
			RequireFaxNumber = requireFaxNumber;
			RequireEmailAddress = requireEmailAddress;
			RequireWebAddress = requireWebAddress;
			RequireFaxEmailOrWeb = requireFaxEmailOrWeb;
			RequireARContact = requireArContact;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		static bool GetValueFromRegistry(string regKey, DataSet data)
		{
			if (data.Tables[0].Columns.Contains(regKey))
			{
				return bool.Parse((string)data.Tables[0].Rows[0][regKey]);
			}
			else
			{
				return false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public OrgRequiredFields(byte[] registryValue)
		{
			if (registryValue != null)
			{
				MemoryStream xmlStream = new MemoryStream(registryValue);
				DataSet data = new DataSet();
				data.ReadXml(xmlStream, XmlReadMode.Auto);
				RequireAddress2 = GetValueFromRegistry("RequireAddress2", data);
				RequireBranch = GetValueFromRegistry("RequireBranch", data);
				RequireCity = GetValueFromRegistry("RequireCity", data);
				RequirePhoneNumber = GetValueFromRegistry("RequirePhoneNumber", data);
				RequireBusinessNumber = GetValueFromRegistry("RequireBusinessNumber", data);
				RequirePhoneOrBusinessNumber = GetValueFromRegistry("RequirePhoneOrBusinessNumber", data);
				RequireFaxNumber = GetValueFromRegistry("RequireFaxNumber", data);
				RequireEmailAddress = GetValueFromRegistry("RequireEmailAddress", data);
				RequireWebAddress = GetValueFromRegistry("RequireWebAddress", data);
				RequireFaxEmailOrWeb = GetValueFromRegistry("RequireFaxEmailOrWeb", data);
				RequireARContact = GetValueFromRegistry("RequireARContact", data);
			}
			else
			{
				RequireAddress2 = false;
				RequireBranch = false;
				RequireCity = true;
				RequirePhoneNumber = false;
				RequireBusinessNumber = false;
				RequirePhoneOrBusinessNumber = false;
				RequireFaxNumber = false;
				RequireEmailAddress = false;
				RequireWebAddress = false;
				RequireFaxEmailOrWeb = false;
				RequireARContact = false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public byte[] GetRegistryValue()
		{
			DataTable table = new DataTable();

			DataColumn requireAddress2Column = new DataColumn("RequireAddress2", typeof(bool));
			DataColumn requireBranchColumn = new DataColumn("RequireBranch", typeof(bool));
			DataColumn requireCityColumn = new DataColumn("RequireCity", typeof(bool));
			DataColumn requirePhoneNumberColumn = new DataColumn("RequirePhoneNumber", typeof(bool));
			DataColumn requireBusinessNumberColumn = new DataColumn("RequireBusinessNumber", typeof(bool));
			DataColumn requirePhoneOrBusinessNumberColumn = new DataColumn("RequirePhoneOrBusinessNumber", typeof(bool));
			DataColumn requireFaxNumberColumn = new DataColumn("RequireFaxNumber", typeof(bool));
			DataColumn requireEmailAddressColumn = new DataColumn("RequireEmailAddress", typeof(bool));
			DataColumn requireWebAddressColumn = new DataColumn("RequireWebAddress", typeof(bool));
			DataColumn requireFaxEmailOrWebColumn = new DataColumn("RequireFaxEmailOrWeb", typeof(bool));
			DataColumn requireARContactColumn = new DataColumn("RequireARContact", typeof(bool));

			table.Columns.Add(requireAddress2Column);
			table.Columns.Add(requireBranchColumn);
			table.Columns.Add(requireCityColumn);
			table.Columns.Add(requirePhoneNumberColumn);
			table.Columns.Add(requireBusinessNumberColumn);
			table.Columns.Add(requirePhoneOrBusinessNumberColumn);
			table.Columns.Add(requireFaxNumberColumn);
			table.Columns.Add(requireEmailAddressColumn);
			table.Columns.Add(requireWebAddressColumn);
			table.Columns.Add(requireFaxEmailOrWebColumn);
			table.Columns.Add(requireARContactColumn);

			DataRow row = table.NewRow();

			row[requireAddress2Column] = RequireAddress2;
			row[requireBranchColumn] = RequireBranch;
			row[requireCityColumn] = RequireCity;
			row[requirePhoneNumberColumn] = RequirePhoneNumber;
			row[requireBusinessNumberColumn] = RequireBusinessNumber;
			row[requirePhoneOrBusinessNumberColumn] = RequirePhoneOrBusinessNumber;
			row[requireFaxNumberColumn] = RequireFaxNumber;
			row[requireEmailAddressColumn] = RequireEmailAddress;
			row[requireWebAddressColumn] = RequireWebAddress;
			row[requireFaxEmailOrWebColumn] = RequireFaxEmailOrWeb;
			row[requireARContactColumn] = RequireARContact;

			table.Rows.Add(row);

			DataSet data = new DataSet();
			data.Tables.Add(table);

			MemoryStream xmlStream = new MemoryStream();
			data.WriteXml(xmlStream, XmlWriteMode.IgnoreSchema);

			return xmlStream.ToArray();
		}
	}
}
